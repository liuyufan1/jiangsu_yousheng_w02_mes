namespace w02_mes.device.rivetGun;

public class RivetGunManager
{
    public static RivetGunInstall rivetGun1 = new ("拉铆1", "JSYS10221084");
    public static RivetGunInstall rivetGun2 = new ("拉铆2", "JSYS10221039"); 
    public static RivetGunInstall rivetGun3 = new ("拉铆3", "JSYS10221084");
    public static RivetGunInstall rivetGun4 = new ("拉铆4", "JSYS10221060");
    
    public static void Mqttmanage(string device, string topic, string  message)
    {
        switch (device)
        {
            case "1号拉铆枪":
                rivetGun1.Event(device, topic, message);
                break;
            case "2号拉铆枪":
                rivetGun2.Event(device, topic, message);
                break;
            case "3号拉铆枪":
                rivetGun3.Event(device, topic, message);
                break;
            case "4号拉铆枪":
                rivetGun4.Event(device, topic, message);
                break;
        }
    }
}