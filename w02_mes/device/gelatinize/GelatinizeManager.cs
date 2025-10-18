using w02_mes.http;

namespace w02_mes.device.gelatinize;

/// <summary>
/// 涂胶
/// </summary>
public class GelatinizeManager
{
    public static GelatinizeInstall gelatinize1 = new ("W02涂胶", "JSYS10217018");

    public static void Mqttmanage(string topic, string message)
    {
        if("false".Equals(message, StringComparison.OrdinalIgnoreCase))
            return;
        switch (topic)
        {
            case "开始扫码":
                var readBarcode = gelatinize1.ReadBarcode();
                MainWindow.ShowLog("w02涂胶", $"扫码结果:{readBarcode}");
                gelatinize1.Barcode = readBarcode;
                var (uploadByDevice, resMessage) = MesUploader.UploadByDevice(gelatinize1, 0, true);
                if (MainWindow.BlockGlueEnabled)
                {
                    MainWindow.ShowLog("w02涂胶", "MES开启屏蔽");
                    uploadByDevice = true;
                }
                _ = gelatinize1.SendOkOrFail(uploadByDevice);
                if (uploadByDevice)
                {
                    MainWindow.ShowLog("w02涂胶", "上传mes成功：" + resMessage);
                    Task.Run(() =>
                    {
                        MainWindow.ShowLog(gelatinize1.Name, "上传过程数据");
                        _ = MesUploader.UploadByDevice(gelatinize1, 99, true);
                        Thread.Sleep(1000 * 60 * 3);
                        MainWindow.ShowLog(gelatinize1.Name, "3分钟上传过程数据");
                        _ = MesUploader.UploadByDevice(gelatinize1, 99, true);
                    });
                }
                else
                {
                    MainWindow.ShowLog("w02涂胶", "上传mes失败：" + resMessage);
                }
                
                break;
            case "完成信号":
                MainWindow.ShowLog("w02涂胶", "完成信号    " + gelatinize1.Barcode);
                if(gelatinize1.Barcode != "")
                    _ = MesUploader.UploadByDevice(gelatinize1, 1, true);
                break;
        }
    }

}