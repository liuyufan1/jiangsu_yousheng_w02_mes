using w02_mes.device.rivetGun;

namespace w02_mes.listen;

public class RivetGunMqttListen
{
    public static void Mqttmanage(string device, string topic, string  message)
    {
        switch (device)
        {
            case "1号拉铆枪":
                RivetGunManager.rivetGun1.Event(device, topic, message);
                break;
            case "2号拉铆枪":
                RivetGunManager.rivetGun2.Event(device, topic, message);
                break;
            case "3号拉铆枪":
                RivetGunManager.rivetGun3.Event(device, topic, message);
                break;
            case "4号拉铆枪":
                RivetGunManager.rivetGun4.Event(device, topic, message);
                break;
        }
    }
    // 产品完成
    public static void OnFinish()
    {
        
    }
    // 单次拉铆完成 读取拉铆数据并保存
    public static void OnSingleFinish()
    {
        
    }
}