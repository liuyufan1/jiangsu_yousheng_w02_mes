using w02_mes.device.rivetGun;
using w02_mes.device.slipway;
using w02_mes.http;

namespace w02_mes.listen;

public class Slipway
{
    // 分配mqtt收到的信号
    public static void MqttManager(string topic, string message)
    {
        if ("false".ToLower().Equals(message.ToLower()))
            return;
        string barcode = "";
        switch (topic)
        {
            case "3-1允许扫码": //正面铸件M6通孔拉铆53颗
                barcode = SlipwayManager.slipway1.ReadBarcode();
                MainWindow.ShowLog("正面铸件M6通孔拉铆53颗", "条码：" + barcode);
                RivetGunManager.rivetGun1.Barcode = barcode; // 给拉铆枪绑定条码
                var uploaded1 = MesUploader.UploadByDevice(RivetGunManager.rivetGun1, 0); // 入站
                if (uploaded1)
                {
                    MainWindow.ShowLog("正面铸件M6通孔拉铆53颗", "上传成功");
                }
                else
                {
                    MainWindow.ShowLog("正面铸件M6通孔拉铆53颗", "上传失败");
                }
                break;
            case "3-2允许扫码":
                barcode = SlipwayManager.slipway2.ReadBarcode();
                MainWindow.ShowLog("正面边梁M6盲孔拉铆44颗", "条码：" + barcode);
                MainWindow.ShowLog("密封拉铆4颗", "条码：" + barcode);
                RivetGunManager.rivetGun2.Barcode = barcode; // 给拉铆枪绑定条码
                RivetGunManager.rivetGun3.Barcode = barcode; // 给拉铆枪绑定条码
                var uploaded2 = MesUploader.UploadByDevice(RivetGunManager.rivetGun2, 0); // 入站
                var uploaded3 = MesUploader.UploadByDevice(RivetGunManager.rivetGun3, 0); // 入站
                if (uploaded2 && uploaded3)
                {
                    MainWindow.ShowLog("正面边梁M6盲孔拉铆44颗", "上传成功");
                    MainWindow.ShowLog("密封拉铆4颗", "上传成功");
                    
                }
                break;
            case "3-3允许扫码":
                barcode = SlipwayManager.slipway3.ReadBarcode();
                MainWindow.ShowLog("背面六角拉铆28颗", "条码：" + barcode);
                RivetGunManager.rivetGun4.Barcode = barcode; // 给拉铆枪绑定条码
                var uploaded4 = MesUploader.UploadByDevice(RivetGunManager.rivetGun4, 0); // 入站
                if (uploaded4)
                {
                    MainWindow.ShowLog("背面六角拉铆28颗", "上传成功");
                }
                break;
            default:
                MainWindow.ShowLog("滑台", "未知topic:" + topic);
                break;
        }
     }
}