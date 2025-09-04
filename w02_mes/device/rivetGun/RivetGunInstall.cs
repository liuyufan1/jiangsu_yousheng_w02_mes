using Newtonsoft.Json;
using w02_mes.device.rollingRubber;
using w02_mes.device.slipway;
using w02_mes.http;
using w02_mes.listen;
using w02_mes.start;

namespace w02_mes.device.rivetGun;

public class RivetGunInstall : Device
{
    public override string Name { get; }
    public override string Barcode { get; set; } = "";
    public override string DeviceType { get; }
    public override string DeviceCode { get; }
    
    
    public RivetGunInstallData InstallData = new();
    
    public RivetGunInstall(string name, string deviceCode)
    {
        Name = name;
        DeviceType = "60";
        DeviceCode = deviceCode;
    }

    public void Event(string device, string topic, string value)
    {
        if ("false".Equals(value, StringComparison.OrdinalIgnoreCase))
            return;
        
        switch (topic)
        {
            case "单次拉铆完成":
                var contentList = HslManager.Mqtt.ReadRpc<List<float>>("Edge/DeviceData", new { data = $"拉铆枪/{device}/拉铆数据" }).Content;
                InstallData.AddData(contentList);
                break;
            case "产品完成":
                // MainWindow.ShowLog(device, "产品完成");
                // OnFinish();
                break;
        }
    }

    public override string ToJson()
    {
        return null;
    }

    public override void OutStation()
    {
    }

    public void OnFinish()
    {
        // moveType为1 自动清除Data
        // MesUploader.UploadByDevice(this, 1);
        // switch (Name)
        // {
        //     case "拉铆1":
        //     case "拉铆2":
        //     case "拉铆3":
        //         if (SlipwayManager.slipway1.Barcode != "")
        //         {
        //             SlipwayManager.slipway1.RivetGunOkNumber += 1;
        //             LogService.Information(Name, "收到完成信号。滑台1计数：" + SlipwayManager.slipway1.RivetGunOkNumber);
        //             if (SlipwayManager.slipway1.RivetGunOkNumber >= 3)
        //             {
        //                 MainWindow.ShowLog("滑台1", "滑台1完成");
        //                 SlipwayManager.slipway1.OutStation();
        //             }
        //         }
        //
        //         if (SlipwayManager.slipway2.Barcode != "")
        //         {
        //             SlipwayManager.slipway2.RivetGunOkNumber += 1;
        //             LogService.Information(Name, "收到完成信号。滑台2计数：" + SlipwayManager.slipway1.RivetGunOkNumber);
        //             if (SlipwayManager.slipway1.RivetGunOkNumber >= 3)
        //             {
        //                 MainWindow.ShowLog("滑台2", "滑台2完成");
        //                 SlipwayManager.slipway1.OutStation();
        //             }
        //         }
        //         break;
        //     case "拉铆4":
        //         SlipwayManager.slipway3.OutStation();
        //         break;
        // }
        
    }
}