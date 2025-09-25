using System.Configuration;
using Microsoft.IdentityModel.Protocols;
using SqlSugar;

namespace w02_mes.sqlServer;

public class DataRepository
{
    // private static readonly string ConnectionString = "Server=127.0.0.1;Database=w02;User Id=sa;Password=sa123456;";
    private static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["SqlServerConn"].ConnectionString;


    // 工厂方法：每次新建一个 SqlSugarClient
    private static SqlSugarClient CreateDb()
    {
        return new SqlSugarClient(new ConnectionConfig()
        {
            ConnectionString = ConnectionString,
            DbType = DbType.SqlServer,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute
        });
    }

    // 插入一条数据（异步）
    public static async Task<int> InsertAsync(DataEntity entity)
    {
        try
        {
            using var db = CreateDb();
            var insertResult = await db.Insertable(entity).ExecuteReturnIdentityAsync();
            MainWindow.ShowLog(entity.Step, $"插入数据成功，ID: {insertResult}");
            return insertResult;
        }
        catch (Exception ex)
        {
            MainWindow.ShowLog(entity.Step, $"插入数据失败: {ex.Message}");
            return -1; // 插入失败返回 -1
        }
    }

    // 批量插入（异步）
    public static async Task InsertRangeAsync(List<DataEntity> entities)
    {
        try
        {
            using var db = CreateDb();
            var insertResult = await db.Insertable(entities).ExecuteCommandAsync();
            MainWindow.ShowLog("sqlServer", $"批量插入数据成功，数量: {insertResult}");
        }
        catch (Exception ex)
        {
            MainWindow.ShowLog("sqlServer", $"批量插入数据失败: {ex.Message}");
        }
    }

    // 根据条码查询（异步）
    public static async Task<List<DataEntity>> GetByBarcodeAsync(string barcode)
    {
        try
        {
            using var db = CreateDb();
            return await db.Queryable<DataEntity>()
                .Where(d => d.Barcode == barcode)
                .OrderBy(d => d.CreateTime, OrderByType.Desc)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            MainWindow.ShowLog("sqlServer", $"查询数据失败: {ex.Message}");
            return new List<DataEntity>(); // 查询失败返回空列表
        }
    }

}