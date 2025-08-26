using System.Net.Http;
using System.Text;
using System.Text.Json;
using w02_mes.device;

namespace w02_mes.http;

public static class MesUploader
{
    private static readonly string _url = "http://101.226.8.125:2030/api/dataportal/invoke"; // MES接口URL

    private static readonly int _invOrg = 1;
    
    /// <summary>
    /// 上传条码和过站类型到MES
    /// </summary>
    /// <param name="device">设备实例</param>
    /// <param name="moveType">过站类型(0:入站,1:出站,99:过程)</param>
    /// <returns>MES接口响应字符串</returns>
    public static bool UploadByDevice(Device device, int moveType)
    {
        var uploadAsync = UploadAsync(device.Barcode, moveType, device.DeviceType, device.DeviceCode, moveType == 1 ? device.GetData() : new List<string>());
        return  uploadAsync.Result;
    }

    /// <summary>
    /// 上传条码和过站类型到MES
    /// </summary>
    /// <param name="barcode">条码</param>
    /// <param name="moveType">过站类型(0:入站,1:出站,99:过程)</param>
    /// <param name="deviceCode">设备编号</param>
    /// <returns>MES接口响应字符串</returns>
    public static async Task<bool> UploadAsync(string barcode, int moveType, string deviceType, string deviceCode, List<string> activeValues)
    {
        // 生成 actualValue JSON 字符串数组
        var items = activeValues.Select((val, index) =>
        {
            string jsonVal = string.IsNullOrEmpty(val) ? "\"\"" : (int.TryParse(val, out _) || double.TryParse(val, out _) || bool.TryParse(val, out _) ? val : $"\"{val}\"");
            return $"{{\"TargetName\":\"{index + 1}\",\"Val\":{jsonVal}}}";
        });
        string actualValueJson = "[" + string.Join(",", items) + "]";

        string targetName = "deviceValue";
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
                        deviceType,
                        deviceCode,
                        actualValue = actualValueJson,  // JSON字符串数组格式
                        sn = barcode,
                        moveType,
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

        MainWindow.ShowLog("Mes", $"MES接口请求内容: {json}");
        
        using var client = new HttpClient();
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var resp = await client.PostAsync(_url, content);

        string readAsStringAsync = await resp.Content.ReadAsStringAsync();
        MainWindow.ShowLog("Mes", $"MES接口响应内容:{resp.StatusCode} {readAsStringAsync}");
        try
        {
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
                    MainWindow.ShowLog("Mes", "MES接口返回：成功");
                    return true;
                }
                MainWindow.ShowLog("Mes", "MES接口返回：失败");
                return false;
            }
            MainWindow.ShowLog("Mes", "MES接口返回内容无法解析Success字段");
            return false;
        }
        catch(Exception ex)
        {
            MainWindow.ShowLog("Mes", $"MES接口返回内容无法解析: {ex.Message}");
            return false;
        }
        

    }
}