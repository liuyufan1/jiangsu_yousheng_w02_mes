using System.Configuration;
using System.Data;
using System.Windows;
using Serilog;
using w02_mes.start;

namespace w02_mes;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 初始化日志
        LogService.Init();
        w02_mes.MainWindow.ShowLog("start", "程序启动");

        // 注册全局异常处理
        RegisterGlobalExceptionHandlers();

        // 启动业务
        ListenStart.StartHsl();
    }

    /// <summary>
    /// 注册全局异常处理
    /// </summary>
    private void RegisterGlobalExceptionHandlers()
    {
        // 捕获UI线程未处理的异常
        this.DispatcherUnhandledException += (sender, args) =>
        {
            Log.Fatal(args.Exception, "UI线程未捕获异常");
            MessageBox.Show($"程序出错：{args.Exception.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true; // 防止程序崩溃
        };

        // 捕获Task线程未观察的异常（忘记await时可能触发）
        TaskScheduler.UnobservedTaskException += (sender, args) =>
        {
            Log.Fatal(args.Exception, "任务未观察异常");
            args.SetObserved(); // 标记已处理，避免崩溃
        };

        // 捕获非UI线程抛出的异常
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            if (args.ExceptionObject is Exception ex)
            {
                Log.Fatal(ex, "非UI线程未捕获异常");
            }
            else
            {
                Log.Fatal($"非UI线程未捕获异常：{args.ExceptionObject}");
            }
        };
    }
}