using w02_mes.device.rivetGun;
using w02_mes.http;

namespace w02_mes.device.slipway;

public class SlipwayManager
{
    public static SlipwayInstall slipway1 = new ("滑台1", "JSYS10221038", new List<string>{"滑台/3-1扫码完成", "滑台/3-2扫码完成"});
    public static SlipwayInstall slipway2 = new ("滑台2", "JSYS10221039", new List<string>{"滑台/3-1扫码完成", "滑台/3-2扫码完成"});
    public static SlipwayInstall slipway3 = new ("滑台3", "JSYS10221060", new List<string>{"滑台/3-3扫码完成"});
    
    // 分配mqtt收到的信号
    public static void MqttManager(string topic, string message)
    {
        if ("false".Equals(message.ToLower()))
            return;
        switch (topic)
        {
            case "3-1允许扫码": //正面铸件M6通孔拉铆53颗
                slipway1.InStation();
                break;
            case "3-2允许扫码":
                slipway2.InStation();
                break;
            case "3-3允许扫码":
                slipway3.InStation();
                break;
            case "3-1完成信号":
                slipway1.OutStation();
                break;
            case "3-2完成信号":
                slipway2.OutStation();
                break;
            case "3-3完成信号":
                slipway3.OutStation();
                break;
            
            default:
                MainWindow.ShowLog("滑台", "未知topic:" + topic);
                break;
        }
     }
}