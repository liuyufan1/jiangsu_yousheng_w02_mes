using SqlSugar;

namespace w02_mes.sqlServer;


[SugarTable("data")]
public class DataEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    public string Barcode { get; set; }

    public string Step { get; set; }

    public string Data { get; set; }

    [SugarColumn(ColumnDataType = "datetime", IsNullable = true, ColumnName = "create_time")]
    public DateTime CreateTime { get; set; }
}