using System.Collections.Specialized;
using System.Configuration;
using w02_mes.http;
using w02_mes.listen;

namespace w02_mes.device.slipway;

public class SlipwayInstall : Device
{

    public override string Name { get; }
    public override string Barcode { get; set; }
    public override string DeviceType { get; }
    public override string DeviceCode { get; }
    
    public string ScannerOkMqtt { get; set; }

    public SlipwayInstall(string name, string deviceType, string deviceCode, string scannerOkMqtt)
    {
        Name = name;
        DeviceType = deviceType;
        DeviceCode = deviceCode;
        ScannerOkMqtt = scannerOkMqtt;
    }

    // 滑台获取扫码器条码
    public string ReadBarcode()
    {
        NameValueCollection ipSection = (NameValueCollection)ConfigurationManager.GetSection("ipSettings");
        string? ip = ipSection[Name];
        if (ip == null)
        {
            MainWindow.ShowLog("滑台", "获取ip失败，name: " + Name);
            return "";
        }

        Task<string> readBarcodeAsync = BarcodeClient.ReadBarcodeAsync(ip);
        return readBarcodeAsync.Result;
    }

    public void SendOk()
    {
        HslManager.Mqtt.ReadRpc<bool>("Edge/WriteData", new { data = ScannerOkMqtt, value = true });
    }
    
    public override void Onfinish()
    {
        
        
    }
}