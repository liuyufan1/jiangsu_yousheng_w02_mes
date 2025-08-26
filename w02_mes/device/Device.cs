namespace w02_mes.device;

public abstract class Device
{
    // 子类必须实现这个 Name 属性
    public abstract string Name { get; }

    public abstract string Barcode { get; set; }
    
    public abstract string DeviceType { get; }
    
    public abstract string DeviceCode { get; }
    
    public List<string> Data = new();
    
    public List<string> GetData()
    {
        List<string> DataTemp = Data;
        Data = new List<string>();
        return DataTemp;
    }

    public abstract void Onfinish();
}