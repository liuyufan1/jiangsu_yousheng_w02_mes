using w02_mes.http;

namespace w02_mes.device.pressureSleeveBarrel;

public class PressureSleeveBarrelManager
{
    public static PressureSleeveBarrelInstall pressureSleeveBarrel1 = new("压套筒", "JSYS10302022");
    
    public static void Mqttmanage(string topic, string message)
    {
        if("false".Equals(message, StringComparison.OrdinalIgnoreCase))
            return;
        switch (topic)
        {
            case "开始扫码":
                MainWindow.ShowLog("压套筒", "开始扫码");
                var readBarcode = pressureSleeveBarrel1.ReadBarcode();
                MainWindow.ShowLog("压套筒", "扫码成功：" + readBarcode);
                pressureSleeveBarrel1.Barcode = readBarcode;
                var uploadByDevice = MesUploader.UploadByDevice(pressureSleeveBarrel1, 0, false);
                if (!uploadByDevice.success)
                {
                    MainWindow.ShowLog("压套筒", "上传mes失败：" + uploadByDevice.message);
                    
                    if (MainWindow.BlockSleeveEnabled)
                    {
                        MainWindow.ShowLog("压套筒", "MES开启屏蔽");
                        pressureSleeveBarrel1.Barcode = pressureSleeveBarrel1.Barcode;
                        _ = pressureSleeveBarrel1.SendOk();
                    }
                    else
                    {
                        pressureSleeveBarrel1.Barcode = "";
                    }
                    return;
                }
                MainWindow.ShowLog("压套筒", "上传mes成功");
                _ = pressureSleeveBarrel1.SendOk();
                pressureSleeveBarrel1.Data = new (); // 清空旧数据
                break;
            // case "扫码完成":
            //     MainWindow.ShowLog("压套筒", "扫码完成");
            //     _ = pressureSleeveBarrel1.SendOk();
            //     break;
            case "完成":
                MainWindow.ShowLog("压套筒", "完成 " + pressureSleeveBarrel1.Barcode);
                // 读取压套桶数据
                pressureSleeveBarrel1.SaveData();
                _ = MesUploader.UploadByDevice(pressureSleeveBarrel1, 1, true);
                pressureSleeveBarrel1.Barcode = "";
                break;
            default:
                // MainWindow.ShowLog("压套筒", "未知的topic:" + topic);
                break;
        }
    }
}