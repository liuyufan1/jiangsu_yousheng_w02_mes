using System.Collections.Specialized;
using System.Configuration;
using HslCommunication;
using Newtonsoft.Json;
using w02_mes.http;
using w02_mes.listen;
using Formatting = System.Xml.Formatting;

namespace w02_mes.device.rollingRubber;

public class RollingRubberInstall : Device
{
    public override string Name { get; }
    public override string Barcode { get; set; } = "";
    public override string DeviceType { get; }
    public override string DeviceCode { get; }
    
    public RollingRubberInstall(string name, string deviceCode)
    { 
        Name = name;
        DeviceCode = deviceCode;
        DeviceType = "30";
    }
    
    // 获取扫码器条码
    public string ReadBarcode()
    {
        NameValueCollection ipSection = (NameValueCollection)ConfigurationManager.GetSection("ipSettings");
        string? ip = ipSection[Name];
        if (ip == null)
        {
            MainWindow.ShowLog(Name, "获取ip失败，name: " + Name);
            return "";
        }

        Task<string> readBarcodeAsync = BarcodeClient.ReadBarcodeAsync(ip);
        return readBarcodeAsync.Result;
    }


    public (bool success, string message) InStation(string barcode)
    {
        MainWindow.ShowLog(Name, "滚胶扫码: " + barcode);
        return MesUploader.UploadByDevice(this, 0, false);
        
    }
    
    public async Task SendOk()
    {
        HslManager.Mqtt.ReadRpc<bool>("Edge/WriteData", new { data = $"{Name}/扫码完成", value = true });
        Thread.Sleep(1000); 
        HslManager.Mqtt.ReadRpc<bool>("Edge/WriteData", new { data = $"{Name}/扫码完成", value = false });
    }

    public override void OutStation()
    {
        var (success, message) = MesUploader.UploadByDevice(this, 1, true);
        if (!success)
        {
            MainWindow.ShowLog(Name, "出站上传mes失败：" + message);
            return;
        }
        MainWindow.ShowLog(Name, "出站上传mes成功");
    }
    
    public override string ToJson()
    {
        //  读取数据
        OperateResult<int> status = HslManager.Mqtt.ReadRpc<int>("Edge/DeviceData", new { data = "W02滚胶/设备状态" });
        OperateResult<int> code = HslManager.Mqtt.ReadRpc<int>("Edge/DeviceData", new { data = "W02滚胶/报警代码" });
        OperateResult<int> result = HslManager.Mqtt.ReadRpc<int>("Edge/DeviceData", new { data = "W02滚胶/检测结果" });
        OperateResult<int> lowStart = HslManager.Mqtt.ReadRpc<int>("Edge/DeviceData", new { data = "W02滚胶/下压初始值" });
        OperateResult<int> lowEnd = HslManager.Mqtt.ReadRpc<int>("Edge/DeviceData", new { data = "W02滚胶/下压结束值" });

        // 构建列表
        var list = new List<object>
        {
            new { TagName = "设备状态", Val = status.Content.ToString() },
            new { TagName = "报警代码", Val = code.Content.ToString() },
            new { TagName = "检测结果", Val = result.Content.ToString() },
            new { TagName = "下压初始值", Val = lowStart.Content.ToString() },
            new { TagName = "下压结束值", Val = lowEnd.Content.ToString() }
        };

        // 转为 JSON
        return JsonConvert.SerializeObject(list);
    }
}