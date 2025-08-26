using Serilog;

namespace w02_mes.start;

public class LogService
{
    public static void Init()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()  // 设置最低日志级别
            .WriteTo.Console()     // 输出到控制台
            .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day) // 按天分文件
            .CreateLogger();

        Log.Information("Serilog 初始化完成");
    }
    public static void Information(string tag, string message)
    {
        Log.Information("[{Tag}] {Message}", tag, message);
    }

    public static void Warning(string tag, string message)
    {
        Log.Warning("[{Tag}] {Message}", tag, message);
    }

    public static void Error(string tag, string message)
    {
        Log.Error("[{Tag}] {Message}", tag, message);
    }

    public static void Debug(string tag, string message)
    {
        Log.Debug("[{Tag}] {Message}", tag, message);
    }
}