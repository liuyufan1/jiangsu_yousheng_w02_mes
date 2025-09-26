using System.Collections.Specialized;
using System.Configuration;
using w02_mes.device.rivetGun;
using w02_mes.http;
using w02_mes.listen;

namespace w02_mes.device.slipway;

public class SlipwayInstall : Device
{

    public override string Name { get; }
    public override string Barcode { get; set; } = "";
    public override string DeviceType { get; }
    public override string DeviceCode { get; }
    
    public List<string> ScannerOkMqtt { get; set; }

    public SlipwayInstall(string name, string deviceCode, List<string> scannerOkMqtt)
    {
        Name = name;
        DeviceType = "60";
        DeviceCode = deviceCode;
        ScannerOkMqtt = scannerOkMqtt;
    }

    // 滑台获取扫码器条码
    public string ReadBarcode()
    {
        NameValueCollection ipSection = (NameValueCollection)ConfigurationManager.GetSection("ipSettings");
        string? ip = ipSection[Name];
        if (ip == null)
        {
            MainWindow.ShowLog("滑台", "获取ip失败，name: " + Name);
            return "";
        }

        Task<string> readBarcodeAsync = BarcodeClient.ReadBarcodeAsync(ip);
        return readBarcodeAsync.Result;
        
    }

    public void InStation()
    {
        MainWindow.ShowLog(Name, "开始扫码");
        var readBarcode = this.ReadBarcode();
        MainWindow.ShowLog(Name, "扫码结果：" + readBarcode);
        Barcode = readBarcode;
        var uploadByDevice = MesUploader.UploadByDevice(this, 0, false);
        if (uploadByDevice.success)
        {
            MainWindow.ShowLog(Name, "上传mes成功：" + uploadByDevice.message);
            _ = SendOk();
        }
        else
        {
            MainWindow.ShowLog(Name, "上传mes失败：" + uploadByDevice.message);
            // 如果屏蔽mes则不判断返回结果
            if (MainWindow.BlockMesEnabled)
            {
                MainWindow.ShowLog(Name, "mes结果已屏蔽，发送启动信号：" + uploadByDevice.message);
                _ = SendOk();
            }
        }
    }

    public async Task SendOk()
    {
        foreach (var se in ScannerOkMqtt)
        {
            _ = Task.Run(() =>
            {
                HslManager.Mqtt.ReadRpc<bool>("Edge/WriteData", new { data = se, value = true });
                Thread.Sleep(1000);
                HslManager.Mqtt.ReadRpc<bool>("Edge/WriteData", new { data = se, value = false });
            });
        }
    }
    

    public override string ToJson()
    {
        List<object> list = new();

        switch (Name)
        {
            case "滑台1":
            case "滑台2":
                List<RivetGunRecord> rivetGun1Data = RivetGunManager.rivetGun1.InstallData.Data;
                List<RivetGunRecord> rivetGun2Data = RivetGunManager.rivetGun2.InstallData.Data;
                List<RivetGunRecord> rivetGun3Data = RivetGunManager.rivetGun3.InstallData.Data;

                if (rivetGun1Data.Any())
                {
                    list.Add(new { TagName = "拉铆枪：", Val = "1号拉铆枪" });

                    // 只取一次配方数据（第一条即可）
                    var first = rivetGun1Data.First();
                    list.Add(new { TagName = "配方拉力上限", Val = first.配方拉力上限 });
                    list.Add(new { TagName = "配方拉力下限", Val = first.配方拉力下限 });
                    list.Add(new { TagName = "配方位移上限", Val = first.配方位移上限 });
                    list.Add(new { TagName = "配方位移下限", Val = first.配方位移下限 });

                    // 每条记录都传动态数据
                    foreach (var se in rivetGun1Data)
                    {
                        list.Add(new { TagName = "此次拉铆拉力", Val = se.此次拉铆拉力 });
                        list.Add(new { TagName = "此次拉铆位移", Val = se.此次拉铆位移 });
                        list.Add(new { TagName = "判定结果", Val = se.判定结果 });
                        // list.Add(new { TagName = "采集时间", Val = se.采集时间.ToString("yyyy-MM-dd HH:mm:ss") });
                    }
                }

                if (rivetGun2Data.Any())
                {
                    list.Add(new { TagName = "拉铆枪：", Val = "2号拉铆枪" });

                    // 只取一次配方数据（第一条即可）
                    var first = rivetGun2Data.First();
                    list.Add(new { TagName = "配方拉力上限", Val = first.配方拉力上限 });
                    list.Add(new { TagName = "配方拉力下限", Val = first.配方拉力下限 });
                    list.Add(new { TagName = "配方位移上限", Val = first.配方位移上限 });
                    list.Add(new { TagName = "配方位移下限", Val = first.配方位移下限 });

                    // 每条记录都传动态数据
                    foreach (var se in rivetGun2Data)
                    {
                        list.Add(new { TagName = "此次拉铆拉力", Val = se.此次拉铆拉力 });
                        list.Add(new { TagName = "此次拉铆位移", Val = se.此次拉铆位移 });
                        list.Add(new { TagName = "判定结果", Val = se.判定结果 });
                        // list.Add(new { TagName = "采集时间", Val = se.采集时间.ToString("yyyy-MM-dd HH:mm:ss") });
                    }
                }
                
                if (rivetGun3Data.Any())
                {
                    list.Add(new { TagName = "拉铆枪：", Val = "3号拉铆枪" });

                    // 只取一次配方数据（第一条即可）
                    var first = rivetGun3Data.First();
                    list.Add(new { TagName = "配方拉力上限", Val = first.配方拉力上限 });
                    list.Add(new { TagName = "配方拉力下限", Val = first.配方拉力下限 });
                    list.Add(new { TagName = "配方位移上限", Val = first.配方位移上限 });
                    list.Add(new { TagName = "配方位移下限", Val = first.配方位移下限 });

                    // 每条记录都传动态数据
                    foreach (var se in rivetGun3Data)
                    {
                        list.Add(new { TagName = "此次拉铆拉力", Val = se.此次拉铆拉力 });
                        list.Add(new { TagName = "此次拉铆位移", Val = se.此次拉铆位移 });
                        list.Add(new { TagName = "判定结果", Val = se.判定结果 });
                        // list.Add(new { TagName = "采集时间", Val = se.采集时间.ToString("yyyy-MM-dd HH:mm:ss") });
                    }
                }
                RivetGunManager.rivetGun1.InstallData.Data = new();
                RivetGunManager.rivetGun2.InstallData.Data = new();
                RivetGunManager.rivetGun3.InstallData.Data = new();
                break;

            case "滑台3":
                List<RivetGunRecord> rivetGun4Data = RivetGunManager.rivetGun4.InstallData.Data;
                
                if (rivetGun4Data.Any())
                {
                    list.Add(new { TagName = "拉铆枪：", Val = "4号拉铆枪" });

                    // 只取一次配方数据（第一条即可）
                    var first = rivetGun4Data.First();
                    list.Add(new { TagName = "配方拉力上限", Val = first.配方拉力上限 });
                    list.Add(new { TagName = "配方拉力下限", Val = first.配方拉力下限 });
                    list.Add(new { TagName = "配方位移上限", Val = first.配方位移上限 });
                    list.Add(new { TagName = "配方位移下限", Val = first.配方位移下限 });

                    // 每条记录都传动态数据
                    foreach (var se in rivetGun4Data)
                    {
                        list.Add(new { TagName = "此次拉铆拉力", Val = se.此次拉铆拉力 });
                        list.Add(new { TagName = "此次拉铆位移", Val = se.此次拉铆位移 });
                        list.Add(new { TagName = "判定结果", Val = se.判定结果 });
                        // list.Add(new { TagName = "采集时间", Val = se.采集时间.ToString("yyyy-MM-dd HH:mm:ss") });
                    }
                }
                RivetGunManager.rivetGun4.InstallData.Data = new();
                break;

        }

        // 序列化为 JSON 数组字符串
        return System.Text.Json.JsonSerializer.Serialize(list);
    }

    public override void OutStation()
    {
        MainWindow.ShowLog(Name, "出站信号 " + Barcode);
        Task.Run(() =>
        {
            if (Barcode == "" || Barcode == null)
            {
                Barcode = "";
            }

            var uploadByDevice = MesUploader.UploadByDevice(this, 1, true); 
            Barcode = "";
            if (uploadByDevice.success)
            {
                MainWindow.ShowLog(Name, "上传mes成功：" + uploadByDevice.success);
            }
            else
            {
                MainWindow.ShowLog(Name, "上传mes失败：" + uploadByDevice.message);
            }
        });
        
    }
}