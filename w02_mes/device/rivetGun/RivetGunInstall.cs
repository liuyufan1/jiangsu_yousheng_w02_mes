using w02_mes.http;
using w02_mes.listen;

namespace w02_mes.device.rivetGun;

public class RivetGunInstall : Device
{
    public override string Name { get; }
    public override string Barcode { get; set; }
    public override string DeviceType { get; }
    public override string DeviceCode { get; }
    
    public RivetGunInstall(string name, string deviceType, string deviceCode)
    {
        Name = name;
        DeviceType = deviceType;
        DeviceCode = deviceCode;
    }

    public void Event(string device, string topic, string value)
    {
        if ("false".Equals(value, StringComparison.OrdinalIgnoreCase))
            return;
        
        switch (topic)
        {
            case "单次拉铆完成":
                var contentList = HslManager.Mqtt.ReadRpc<List< string>>("Edge/DeviceData", new { data = $"拉铆枪/{device}/拉铆数据" }).Content;
                string content = string.Join(",", contentList);
                MainWindow.ShowLog(device, "单次拉铆数据：" + content);
                Data.Add(content);
                break;
            case "产品完成":
                MainWindow.ShowLog(device, "产品完成");
                Onfinish();
                break;
        }
    }
    
    public override void Onfinish()
    {
        // moveType为1 自动清除Data
        MesUploader.UploadByDevice(this, 1);
    }
}