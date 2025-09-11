namespace w02_mes.device.rivetGun;

public class RivetGunInstallData
{
    public List<RivetGunRecord> Data { get; set; } = new();
    public void AddData(List<float> newData)
    {
        var roundedData = newData.Select(v => (float)Math.Round(v, 2)).ToList();

        Data.Add(new RivetGunRecord(roundedData));
        // 超过 300 条，删除最前面 2 条
        if (Data.Count > 300)
        {
            Data.RemoveRange(0, Math.Min(2, Data.Count));
        }
    }
}