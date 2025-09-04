using System.Text;
using System.Text.RegularExpressions;
using HslCommunication;
using HslCommunication.MQTT;
using w02_mes.device.gelatinize;
using w02_mes.device.pressureSleeveBarrel;
using w02_mes.device.rivetGun;
using w02_mes.device.rollingRubber;
using w02_mes.device.slipway;

namespace w02_mes.listen;

public class HslManager
{
    private static  MqttClient  MqttClient;
    
    public static MqttSyncClient Mqtt;
    
    public static void Start()
    {
        MqttClient = new MqttClient(new MqttConnectionOptions()
        {


            IpAddress = "127.0.0.1",
            Port = 521,
            Credentials = new MqttCredential("admin", "123456"),   // 设置了用户名和密码

        });
        OperateResult connect = MqttClient.ConnectServer();

        MqttClient.OnMqttMessageReceived += MqttClient_OnMqttMessageReceived; // 调用一次即可


        MqttClient.SubscribeMessage("拉铆枪/#");
        MqttClient.SubscribeMessage("滑台/#");
        MqttClient.SubscribeMessage("W02滚胶/#");
        MqttClient.SubscribeMessage("W02涂胶/#");
        MqttClient.SubscribeMessage("压套筒/#");



        Mqtt = new MqttSyncClient(new MqttConnectionOptions()
        {

            IpAddress = "127.0.0.1",
            Port = 521,
            Credentials = new MqttCredential("admin", "123456"),   // 设置了用户名和密码
            ConnectTimeout = 2000
        });
        Mqtt.SetPersistentConnection();
    }
    
    private static void MqttClient_OnMqttMessageReceived(MqttClient client, string topic, byte[] payload)
    {
        string mqttValue = Encoding.UTF8.GetString(payload).Replace("\r", "").Replace("\n", "");;

        // 取 topic 第一个 '/' 之前的部分作为类别
        string category = topic.Contains("/") ? topic.Split('/')[0] : topic;

        // 去掉第一个前缀
        string subTopic = topic.Contains("/") ? topic.Substring(topic.IndexOf('/') + 1) : topic;

        switch (category)
        {
            case "拉铆枪":
                HandleRivetingGun(subTopic, mqttValue);
                break;
            case "滑台":
                HandleSlider(subTopic, mqttValue);
                break;
            case "W02滚胶":
                HandleW02GlueRoll(subTopic, mqttValue);
                break;
            case "W02涂胶":
                HandleW02GluePaint(subTopic, mqttValue);
                break;
            case "压套筒":
                HandlePressureSleeveBarrel(subTopic, mqttValue);
                break;
            default:
                MainWindow.ShowLog("未知Mqtt topic", $"{topic}: {mqttValue}");
                break;
        }
    }

    // 具体处理方法
    private static void HandleRivetingGun(string topic, string value)
    {
        string prefix = topic.Split('/')[0]; // 第一个/前的
        string suffix = topic.Contains("/") ? topic.Substring(topic.IndexOf('/') + 1) : "";
        RivetGunManager.Mqttmanage(prefix, suffix, value);
        
    }

    private static void HandleSlider(string topic, string value)
    {
        try
        {
            SlipwayManager.MqttManager(topic, value);
        }
        catch (Exception e)
        {
            MainWindow.ShowLog("滑台", $"{topic}: {value}异常： {e.Message}");
        }
    }

    private static void HandleW02GlueRoll(string topic, string value)
    {
        if (topic.Contains("心跳"))
            return;
        if ("false".Equals(value, StringComparison.OrdinalIgnoreCase))
            return;
        RollingRubberManager.Mqttmanage(topic, value);
    }

    private static void HandleW02GluePaint(string topic, string value)
    {
        if ("false".Equals(value, StringComparison.OrdinalIgnoreCase))
            return;
        GelatinizeManager.Mqttmanage(topic, value);
    }
    
    private static void HandlePressureSleeveBarrel(string topic, string value)
    {
        PressureSleeveBarrelManager.Mqttmanage(topic, value);
    }

}