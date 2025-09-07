namespace w02_mes.device.rollingRubber;

public class RollingRubberManager
{
    public static RollingRubberInstall rollingRubber1 = new("W02滚胶", "JSYS10217010");
    public static void Mqttmanage(string topic, string message)
    {
        if("false".Equals(message, StringComparison.OrdinalIgnoreCase))
            return;
        switch (topic)
        {
            case "开始扫码":
                MainWindow.ShowLog(rollingRubber1.Name, "开始扫码");
                string barcode = rollingRubber1.ReadBarcode();
                rollingRubber1.Barcode = barcode;
                var valueTuple = rollingRubber1.InStation(barcode);
                if (!valueTuple.success)
                {
                    MainWindow.ShowLog(rollingRubber1.Name, "上传mes失败：" + valueTuple.message);
                    return;
                }
                MainWindow.ShowLog(rollingRubber1.Name, "上传mes成功：" + barcode);
                _ = rollingRubber1.SendOkOrFail();
                rollingRubber1.Barcode = barcode;
                break;
            case "完成信号":
                MainWindow.ShowLog(rollingRubber1.Name, "完成信号");
                rollingRubber1.OutStation();
                break;
        }
    }
}