namespace w02_mes.device;

public abstract class Device
{
    // 子类必须实现这个 Name 属性
    public abstract string Name { get; }

    public abstract string Barcode { get; set; }
    
    public abstract string DeviceType { get; }
    
    public abstract string DeviceCode { get; }

    public abstract string ToJson();
    public abstract void OutStation();
}