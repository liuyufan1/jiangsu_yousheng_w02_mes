using System.Collections.Specialized;
using System.Configuration;
using HslCommunication;
using w02_mes.http;
using w02_mes.listen;

namespace w02_mes.device.gelatinize;

public class GelatinizeInstall : Device
{
    public override string Name { get; }
    public override string Barcode { get; set; } = "";
    public override string DeviceType { get; }
    public override string DeviceCode { get; }
    
    // public GelatinizeData GelatinizeData = new();
    
    public GelatinizeInstall(string name, string deviceCode)
    {
        Name = name;
        DeviceType = "30";
        DeviceCode = deviceCode;
        
    }
    // 获取扫码器条码
    public string ReadBarcode()
    {
        NameValueCollection ipSection = (NameValueCollection)ConfigurationManager.GetSection("ipSettings");
        string? ip = ipSection[Name];
        if (ip == null)
        {
            MainWindow.ShowLog("涂胶", "获取ip失败，name: " + Name);
            return "";
        }

        Task<string> readBarcodeAsync = BarcodeClient.ReadBarcodeAsync(ip);
        return readBarcodeAsync.Result;
        // return "0050010400401G41131046190019";
    }

    public async Task SendOkOrFail(bool flag)
    {
        string suffix = flag ? "扫码完成" : "扫码报警";
        HslManager.Mqtt.ReadRpc<bool>("Edge/WriteData", new { data = $"{Name}/{suffix}", value = true });
        Thread.Sleep(1000); 
        HslManager.Mqtt.ReadRpc<bool>("Edge/WriteData", new { data = $"{Name}/{suffix}", value = false });
    }

    public override string ToJson()
    {
        var data = new GelatinizeData();

        // 按照字段逐个读取
        data.A计量出口压力上限   = HslManager.Mqtt.ReadRpc<float>("Edge/DeviceData", new { data = "涂胶机/A计量出口压力上限" }).Content;
        data.B计量出口压力上限   = HslManager.Mqtt.ReadRpc<float>("Edge/DeviceData", new { data = "涂胶机/B计量出口压力上限" }).Content;
        data.A计量出口实际压力   = HslManager.Mqtt.ReadRpc<float>("Edge/DeviceData", new { data = "涂胶机/A计量出口实际压力" }).Content;
        data.B计量出口实际压力   = HslManager.Mqtt.ReadRpc<float>("Edge/DeviceData", new { data = "涂胶机/B计量出口实际压力" }).Content;
        data.A计量出口速度       = HslManager.Mqtt.ReadRpc<float>("Edge/DeviceData", new { data = "涂胶机/A计量出口速度" }).Content;
        data.B计量出口速度       = HslManager.Mqtt.ReadRpc<float>("Edge/DeviceData", new { data = "涂胶机/B计量出口速度" }).Content;
        data.AB总速度           = HslManager.Mqtt.ReadRpc<float>("Edge/DeviceData", new { data = "涂胶机/AB总速度" }).Content;
        data.压盘A_1            = HslManager.Mqtt.ReadRpc<float>("Edge/DeviceData", new { data = "涂胶机/压盘A-1" }).Content;
        data.压盘A_2            = HslManager.Mqtt.ReadRpc<float>("Edge/DeviceData", new { data = "涂胶机/压盘A-2" }).Content;
        data.压盘B_1            = HslManager.Mqtt.ReadRpc<float>("Edge/DeviceData", new { data = "涂胶机/压盘B-1" }).Content;
        data.压盘B_2            = HslManager.Mqtt.ReadRpc<float>("Edge/DeviceData", new { data = "涂胶机/压盘B-2" }).Content;
        data.胶管A_1            = HslManager.Mqtt.ReadRpc<float>("Edge/DeviceData", new { data = "涂胶机/胶管A-1" }).Content;
        data.胶管A_2            = HslManager.Mqtt.ReadRpc<float>("Edge/DeviceData", new { data = "涂胶机/胶管A-2" }).Content;
        data.胶管B_1            = HslManager.Mqtt.ReadRpc<float>("Edge/DeviceData", new { data = "涂胶机/胶管B-1" }).Content;
        data.胶管B_2            = HslManager.Mqtt.ReadRpc<float>("Edge/DeviceData", new { data = "涂胶机/胶管B-2" }).Content;

        return data.ToJson();
    }


    public override void OutStation()
    {
        throw new NotImplementedException();
    }
}