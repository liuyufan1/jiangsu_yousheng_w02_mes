using Serilog;
using w02_mes.start;

namespace w02_mes.device.rivetGun;

public class RivetGunRecord
{
    public float 此次拉铆拉力 { get; set; }
    public float 此次拉铆位移 { get; set; }
    public float 配方拉力上限 { get; set; }
    public float 配方拉力下限 { get; set; }
    public float 配方位移上限 { get; set; }
    public float 配方位移下限 { get; set; }

    public string 判定结果
    {
        get
        {
            bool 拉力OK = 此次拉铆拉力 > 配方拉力下限 && 此次拉铆拉力 < 配方拉力上限;
            bool 位移OK = 此次拉铆位移 > 配方位移下限 && 此次拉铆位移 < 配方位移上限;
            return (拉力OK && 位移OK) ? "OK" : "NG";
        }
    }

    public DateTime 采集时间 { get; set; } = DateTime.Now;

    // 构造方法，直接用数组填充
    public RivetGunRecord(List<float> se)
    {
        LogService.Information("test", string.Join(",", se));
        此次拉铆拉力 = MathF.Round(se[0], 2);
        此次拉铆位移 = MathF.Round(se[1], 2);
        配方拉力上限 = MathF.Round(se[2], 2);
        配方拉力下限 = MathF.Round(se[3], 2);
        配方位移上限 = MathF.Round(se[4], 2);
        配方位移下限 = MathF.Round(se[5], 2);
    }
}