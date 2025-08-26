
using System.Windows;
using Serilog;

namespace w02_mes;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const int MaxLogCount = 3000;

    // 保存所有日志（不做过滤）
    private static readonly List<string> _logs = new();

    // 当前窗口实例
    private static MainWindow _instance;

    // 当前过滤关键字
    private string _filter = string.Empty;

    public MainWindow()
    {
        InitializeComponent();
        _instance = this;

        RefreshLogList();
    }

    /// <summary>
    /// 向日志添加记录（静态方法，外部调用）
    /// </summary>
    public static void ShowLog(string tag, string log)
    {
        Log.Information($"[{tag}] {log}");
        if (string.IsNullOrWhiteSpace(log)) return;

        if (_logs.Count >= MaxLogCount)
            _logs.RemoveAt(0);

        string line = $"{DateTime.Now:HH:mm:ss} [{tag}] {log}";
        _logs.Add(line);

        _instance?.Dispatcher.Invoke(() =>
        {
            _instance.RefreshLogList(scrollToEnd: true);
        });
    }

    /// <summary>
    /// 刷新日志列表，根据过滤条件显示
    /// </summary>
    private void RefreshLogList(bool scrollToEnd = false)
    {
        IEnumerable<string> filtered = string.IsNullOrWhiteSpace(_filter)
            ? _logs
            : _logs.Where(l => l.IndexOf(_filter, StringComparison.OrdinalIgnoreCase) >= 0);

        LogList.ItemsSource = null;
        LogList.ItemsSource = filtered.ToList();

        if (scrollToEnd && LogList.Items.Count > 0)
            LogList.ScrollIntoView(LogList.Items[^1]);
    }

    /// <summary>
    /// 过滤框输入变化
    /// </summary>
    private void FilterBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        _filter = FilterBox.Text?.Trim() ?? string.Empty;
        RefreshLogList();
    }
}