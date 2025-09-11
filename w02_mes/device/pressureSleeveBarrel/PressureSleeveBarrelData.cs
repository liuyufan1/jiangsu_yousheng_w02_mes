using System.Formats.Asn1;
using Newtonsoft.Json;

namespace w02_mes.device.pressureSleeveBarrel;

public class PressureSleeveBarrelData
{
    public float ProductOK = 0;
    public float ProductNG = 0;
    public bool status = false;
    
    public List<float> Pressures { get; set; } = new();      // 对应 data[]
    public List<float> Displacement { get; set; } = new();    // 对应 data2[]
    
    

    // 生成 JSON 方法 目前先不上传数据
    public string ToJson()
    {
        // var list = new List<object>();
        //
        // // 实际值
        // for (int i = 0; i < 10; i++)
        // {
        //     list.Add(new
        //     {
        //         TagName = $"压力{i + 1}实际值",
        //         Val = Pressures[i].ToString()
        //     });
        //     list.Add(new
        //     {
        //         TagName = $"位移{i + 1}实际值",
        //         Val = Displacement[i].ToString()
        //     });
        // }
        // // 测试值
        // for (int i = 10; i < 20; i++)
        // {
        //     list.Add(new
        //     {
        //         TagName = $"压力{i + 1}测试值",
        //         Val = Pressures[i].ToString()
        //     });
        //     list.Add(new
        //     {
        //         TagName = $"位移{i + 1}测试值",
        //         Val = Displacement[i].ToString()
        //     });
        // }
        // // 上限值
        // for (int i = 20; i < 30; i++)
        // {
        //     list.Add(new
        //     {
        //         TagName = $"压力{i + 1}上限值",
        //         Val = Pressures[i].ToString()
        //     });
        //     list.Add(new
        //     {
        //         TagName = $"位移{i + 1}上限值",
        //         Val = Displacement[i].ToString()
        //     });
        // }
        // // 下限值
        // for (int i = 30; i < 40; i++)
        // {
        //     list.Add(new
        //     {
        //         TagName = $"压力{i + 1}下限值",
        //         Val = Pressures[i].ToString()
        //     });
        //     list.Add(new
        //     {
        //         TagName = $"位移{i + 1}下限值",
        //         Val = Displacement[i].ToString()
        //     });
        // }
        //
        //
        // // 总体压力/检测字段
        // list.Add(new { TagName = "productOK", Val = ProductOK == 0 ? "true" : "false" });
        // list.Add(new { TagName = "productNG", Val = ProductNG == 0 ? "true" : "false" });
        // list.Add(new { TagName = "status", Val = status ? "0" : "1" });
        // list.Add(new { TagName = "压套筒数量", Val = "10" });
        //
        // return JsonConvert.SerializeObject(list, Formatting.None);
        return "";
    }
}
