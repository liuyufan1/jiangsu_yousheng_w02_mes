using System.Text.Json;
using Serilog;
using w02_mes.start;

namespace w02_mes.device.gelatinize;

public class GelatinizeData
{
    public float A计量出口压力上限 { get; set; }
    public float B计量出口压力上限 { get; set; }
    public float A计量出口实际压力 { get; set; }
    public float B计量出口实际压力 { get; set; }
    public float A计量出口速度 { get; set; }
    public float B计量出口速度 { get; set; }
    public float AB总速度 { get; set; }
    public float 压盘A_1 { get; set; }
    public float 压盘A_2 { get; set; }
    public float 压盘B_1 { get; set; }
    public float 压盘B_2 { get; set; }
    public float 胶管A_1 { get; set; }
    public float 胶管A_2 { get; set; }
    public float 胶管B_1 { get; set; }
    public float 胶管B_2 { get; set; }
    
    public string ToJson()
    {
        var list = new List<object>();
        var props = GetType().GetProperties();

        foreach (var prop in props)
        {
            var value = prop.GetValue(this);

            string strValue;
            if (value is float f)
            {
                strValue = f.ToString("F2"); // 保留两位小数
            }
            else if (value is double d)
            {
                strValue = d.ToString("F2"); // 保留两位小数
            }
            else if (value is decimal m)
            {
                strValue = m.ToString("F2"); // decimal 也可以这样
            }
            else
            {
                strValue = value?.ToString() ?? "";
            }

            list.Add(new
            {
                TagName = prop.Name,
                Val = strValue
            });
        }

        var serialize = JsonSerializer.Serialize(list, new JsonSerializerOptions
        {
            WriteIndented = false // 先不美化格式
        });
        LogService.Information("test", serialize);
        return serialize;
    }

}