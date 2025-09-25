using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using w02_mes.device;
using w02_mes.sqlServer;
using w02_mes.start;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace w02_mes.http;

public static class MesUploader
{
    private static readonly string _url = "http://101.226.8.125:2030/api/dataportal/invoke"; // MES接口URL

    private static readonly int _invOrg = 1;
    private static HttpClient client = new ();
    
    static MesUploader()
    {
        client.Timeout = new TimeSpan(0, 0, 10);
    }
  
    /// <summary>
    /// 上传条码和过站类型到MES
    /// </summary>
    /// <param name="device">设备实例</param>
    /// <param name="moveType">过站类型(0:入站,1:出站,99:过程)</param>
    /// <param name="needUpData">是否调用toJson方法</param>
    /// <returns>MES接口响应字符串</returns>
    public static (bool success, string message) UploadByDevice(Device device, int moveType,  bool needUpData)
    {
        string rawValue = "";
        if (needUpData)
        {
            rawValue = device.ToJson();
            // 插入数据库
            var dataEntity = new DataEntity();
            dataEntity.Barcode = device.Barcode;
            dataEntity.Step = device.Name;
            string readableValue = UnicodeToString(rawValue);  // 转换 \uXXXX 为中文
            LogService.Information("转义后数据", readableValue);
            dataEntity.Data = readableValue;
            dataEntity.CreateTime = DateTime.Now;
            _ = DataRepository.InsertAsync(dataEntity);
            
        }
        
        var uploadAsync = UploadAsync(device.Barcode??"", moveType, device.DeviceType, device.DeviceCode,rawValue);
        return  uploadAsync.GetAwaiter().GetResult();
    }
    private static string UnicodeToString(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        return Regex.Replace(input, @"\\u([0-9a-fA-F]{4})", match =>
        {
            // 将 \uXXXX 转为对应字符
            return ((char)Convert.ToInt32(match.Groups[1].Value, 16)).ToString();
        });
    }

    /// <summary>
    /// 上传条码和过站类型到MES
    /// </summary>
    /// <param name="barcode">条码</param>
    /// <param name="moveType">过站类型(0:入站,1:出站,99:过程)</param>
    /// <param name="deviceCode">设备编号</param>
    /// <returns>MES接口响应字符串</returns>
    public static async Task<(bool success, string message)> UploadAsync(string barcode, int moveType, string deviceType, string deviceCode, string activeValues)
    {
        
        var reqObj = new
        {
            ApiType = "ScadaController",
            Method = "ScadaUploadData",
            Parameters = new[]
            {
                new
                {
                    Value = new
                    {
                        invOrg = _invOrg,
                        deviceType = deviceType,
                        deviceCode = deviceCode,
                        actualValue = activeValues,  // JSON字符串数组格式
                        sn = barcode,
                        moveType = moveType,
                        dataGenTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff+08:00"),
                        isFirstProcess = false
                    }
                }
            },
            Context = new
            {
                Ticket = "",
                InvOrgId = _invOrg
            }
        };

        string json = JsonSerializer.Serialize(reqObj);

        try
        {
            MainWindow.ShowLog("Mes", $"MES接口请求内容: {json}");
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = client.PostAsync(_url, content).Result;
            string readAsStringAsync = resp.Content.ReadAsStringAsync().Result;
            MainWindow.ShowLog("Mes", $"MES接口响应内容:{resp.StatusCode} {readAsStringAsync}");
            
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(readAsStringAsync);

            if (dict != null && dict.TryGetValue("Success", out var successElement))
            {
                bool isSuccess = false;
                if (successElement.ValueKind == JsonValueKind.True)
                {
                    isSuccess = true;
                }
                else if (successElement.ValueKind == JsonValueKind.False)
                {
                    isSuccess = false;
                }
                else if (successElement.ValueKind == JsonValueKind.String)
                {
                    string? successStr = successElement.GetString();
                    isSuccess = successStr != null && successStr.Equals("true", StringComparison.OrdinalIgnoreCase);
                }
                // 你还可以根据需要支持数字类型
                else if (successElement.ValueKind == JsonValueKind.Number)
                {
                    if (successElement.TryGetInt32(out int num))
                        isSuccess = num != 0;
                }

                if (isSuccess)
                {
                    return (true, "成功");
                }
                return (false,"MES接口返回：" + readAsStringAsync);
            }
            return (false, "MES接口返回内容无法解析Success字段");
        }
        catch(Exception ex)
        {
            return (false, $"MES接口返回内容无法解析: {ex.Message}");
        }
        

    }
}