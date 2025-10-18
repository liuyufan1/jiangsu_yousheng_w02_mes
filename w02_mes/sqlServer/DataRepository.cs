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
                .Where(d => d.Barcode.Contains(barcode))
                .OrderBy(d => d.CreateTime, OrderByType.Desc)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            MainWindow.ShowLog("sqlServer", $"查询数据失败: {ex.Message}");
            return new List<DataEntity>(); // 查询失败返回空列表
        }
    }
    /// <summary>
    /// 条码模糊查询 + 分页
    /// </summary>
    /// <param name="barcodeKeyword">条码关键字（允许为空，为空时查询全部）</param>
    /// <param name="pageIndex">页码（从 1 开始）</param>
    /// <param name="pageSize">每页条数（建议 10~200）</param>
    public static async Task<PagedResult<DataEntity>> GetByBarcodePagedAsync(
        string? barcodeKeyword,
        int pageIndex = 1,
        int pageSize  = 20)
    {
        // 合法化参数
        if (pageIndex < 1) pageIndex = 1;
        if (pageSize  < 1) pageSize  = 20;
        if (pageSize  > 200) pageSize = 200;

        try
        {
            using var db = CreateDb();

            var query = db.Queryable<DataEntity>();

            if (!string.IsNullOrWhiteSpace(barcodeKeyword))
            {
                query = query.Where(d => d.Barcode.Contains(barcodeKeyword));
            }

            query = query.OrderBy(d => d.CreateTime, OrderByType.Desc);

            RefAsync<int> total = 0;
            var list = await query.ToPageListAsync(pageIndex, pageSize, total);

            return new PagedResult<DataEntity>
            {
                PageIndex = pageIndex,
                PageSize  = pageSize,
                Total     = total,
                Pages     = (int)Math.Ceiling(total * 1.0 / pageSize),
                Records   = list
            };
        }
        catch (Exception ex)
        {
            MainWindow.ShowLog("sqlServer", $"分页查询数据失败: {ex.Message}");
            return new PagedResult<DataEntity>
            {
                PageIndex = pageIndex,
                PageSize  = pageSize,
                Total     = 0,
                Pages     = 0,
                Records   = new List<DataEntity>()
            };
        }
    }

}