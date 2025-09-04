using System.Collections.Specialized;
using System.Configuration;
using HslCommunication;
using w02_mes.http;
using w02_mes.listen;

namespace w02_mes.device.pressureSleeveBarrel;

public class PressureSleeveBarrelInstall : Device
{
    public override string Name { get; }
    public override string Barcode { get; set; } = "";
    public override string DeviceType { get; }
    public override string DeviceCode { get; }
    
    public PressureSleeveBarrelData Data = new();
    
    public PressureSleeveBarrelInstall(string name, string deviceCode)
    { 
        Name = name;
        DeviceType = "84";
        DeviceCode = deviceCode;
    }
    
    // 压套筒获取扫码器条码
    public string ReadBarcode()
    {
        //  ip
        NameValueCollection ipSection = (NameValueCollection)ConfigurationManager.GetSection("ipSettings");
        string? ip = ipSection[Name];
        if (ip == null)
        {
            MainWindow.ShowLog("压套筒", "获取ip失败，name: " + Name);
            return "";
        }

        Task<string> readBarcodeAsync = BarcodeClient.ReadBarcodeAsync(ip);
        return readBarcodeAsync.Result;
        // return "0050010400401G41131046190017";
    }


    public override string ToJson()
    {
        
        return Data.ToJson();
    }

    public override void OutStation()
    {
        // 获取数据
        // 出站
    }

    public async Task SendOk()
    {
        HslManager.Mqtt.ReadRpc<bool>("Edge/WriteData", new { data = $"{Name}/扫码完成", value = true });
        Thread.Sleep(1000);
        HslManager.Mqtt.ReadRpc<bool>("Edge/WriteData", new { data = $"{Name}/扫码完成", value = false });
    }

    public void SaveData()
    {
        
        OperateResult<float[]> displacement = HslManager.Mqtt.ReadRpc<float[]>("Edge/DeviceData", new { data = "压套筒/位移" });
        OperateResult<float[]> pressureList = HslManager.Mqtt.ReadRpc<float[]>("Edge/DeviceData", new { data = "压套筒/压力" });
        OperateResult<float> productOK = HslManager.Mqtt.ReadRpc<float>("Edge/DeviceData", new { data = "压套筒/产品OK" });
        OperateResult<float> productNG = HslManager.Mqtt.ReadRpc<float>("Edge/DeviceData", new { data = "压套筒/产品ng" });
        OperateResult<bool> status = HslManager.Mqtt.ReadRpc<bool>("Edge/DeviceData", new { data = "压套筒/状态" });
        Data.Pressures = pressureList.Content.ToList();
        Data.Displacement = displacement.Content.ToList();
        Data.ProductOK = productOK.Content;
        Data.ProductNG = productNG.Content;
        Data.status = status.Content;

    }
}