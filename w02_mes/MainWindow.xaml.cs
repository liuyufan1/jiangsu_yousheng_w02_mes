
using System.Windows;
using System.Windows.Controls;
using Serilog;
using w02_mes.device.gelatinize;
using w02_mes.device.rollingRubber;
using w02_mes.device.slipway;
using w02_mes.helpers;

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
    
    // 滑台屏蔽 MES
    public static bool BlockMesEnabled { get; private set; } = false;
    
    // 贴PC片（滚胶） 屏蔽 MES
    public static bool BlockMesByPcEnabled { get; private set; } = false;

    // 屏蔽滚胶
    public static bool BlockGlueEnabled { get; private set; } = false;

    // 屏蔽压套筒
    public static bool BlockSleeveEnabled { get; private set; } = false;

    public MainWindow()
    {
        InitializeComponent();
        _instance = this;
        var appConfig = ConfigHelper.Load();
        BlockMesEnabled = appConfig.BlockMesEnabled;
        BlockMesCheckBox.IsChecked = appConfig.BlockMesEnabled;
        BlockMesByPcEnabled = appConfig.BlockMesByPcEnabled;
        BlockMesByPcCheckBox.IsChecked = appConfig.BlockMesByPcEnabled;
        BlockGlueEnabled = appConfig.BlockGlueEnabled;
        BlockGlueCheckBox.IsChecked = appConfig.BlockGlueEnabled;
        BlockSleeveEnabled = appConfig.BlockSleeveEnabled;
        BlockSleeveCheckBox.IsChecked = appConfig.BlockSleeveEnabled;
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
    
    
    private void EnterButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string selectedPlatform = (PlatformSelect.SelectedItem as ComboBoxItem)?.Content?.ToString();

            switch (selectedPlatform)
            {
                case "滑台1":
                    ShowLog("UI", "入站：执行滑台1的逻辑");
                    // 调用滑台1的出站方法
                    SlipwayManager.slipway1.InStation();
                    break;

                case "滑台2":
                    ShowLog("UI", "入站：执行滑台2的逻辑");
                    // 调用滑台2的出站方法
                    SlipwayManager.slipway2.InStation();
                    break;

                case "滑台3":
                    ShowLog("UI", "入站：执行滑台3的逻辑");
                    //  调用滑台3的出站方法
                    SlipwayManager.slipway3.InStation();
                    break;

                default:
                    ShowLog("UI", "未选择滑台！");
                    break;
            }
            
        }catch(Exception ex)
        {
            ShowLog("UI", "入站异常：" + ex.Message);
        }

    }

    private void ExitButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string selectedPlatform = (PlatformSelect.SelectedItem as ComboBoxItem)?.Content?.ToString();

            switch (selectedPlatform)
            {
                case "滑台1":
                    ShowLog("UI", "出站：执行滑台1的逻辑");
                    // 调用滑台1的出站方法
                    SlipwayManager.slipway1.OutStation();
                    break;

                case "滑台2":
                    ShowLog("UI", "出站：执行滑台2的逻辑");
                    // 调用滑台2的出站方法
                    SlipwayManager.slipway2.OutStation();
                    break;

                case "滑台3":
                    ShowLog("UI", "出站：执行滑台3的逻辑");
                    //  调用滑台3的出站方法
                    SlipwayManager.slipway3.OutStation();
                    break;

                default:
                    ShowLog("UI", "未选择滑台！");
                    break;
            }
            
        }catch(Exception ex)
        {
            ShowLog("UI", "出站异常：" + ex.Message);
        }
        
    }

    private void RollGlueScanButton_Click(object sender, RoutedEventArgs e)
    {
        RollingRubberManager.Mqttmanage("开始扫码", "true");
    }

    private void GelatinizeScanButton_Click(object sender, RoutedEventArgs e)
    {
        GelatinizeManager.Mqttmanage("开始扫码", "true");
    }

    private void GelatinizeScanButton_finish_Click(object sender, RoutedEventArgs e)
    {
        GelatinizeManager.Mqttmanage("完成信号", "true");
    }

  
    private void BlockMesCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        BlockMesEnabled = true;
        var appConfig = ConfigHelper.Load();
        appConfig.BlockMesEnabled = true;
        ConfigHelper.Save(appConfig);
        ShowLog("UI", "滑台屏蔽MES 已启用");
    }

    private void BlockMesCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        BlockMesEnabled = false;
        var appConfig = ConfigHelper.Load();
        appConfig.BlockMesEnabled = false;
        ConfigHelper.Save(appConfig);
        ShowLog("UI", "滑台屏蔽MES 已关闭");
    }

    private void PCMesCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        BlockMesByPcEnabled = true;
        var appConfig = ConfigHelper.Load();
        appConfig.BlockMesByPcEnabled = true;  // ⚡ 你需要在 AppConfig 类里增加这个字段
        ConfigHelper.Save(appConfig);
        ShowLog("UI", "PC片屏蔽MES 已启用");
    }

    private void PCMesCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        BlockMesByPcEnabled = false;
        var appConfig = ConfigHelper.Load();
        appConfig.BlockMesByPcEnabled = false;
        ConfigHelper.Save(appConfig);
        ShowLog("UI", "PC片屏蔽MES 已关闭");
    }
    
    // ✅ 屏蔽滚胶
    private void BlockGlueCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        BlockGlueEnabled = true;
        var appConfig = ConfigHelper.Load();
        appConfig.BlockGlueEnabled = true;   // ⚡ 你需要在 AppConfig 类里加上这个字段
        ConfigHelper.Save(appConfig);
        ShowLog("UI", "屏蔽滚胶 已启用");
    }
    private void BlockGlueCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        BlockGlueEnabled = false;
        var appConfig = ConfigHelper.Load();
        appConfig.BlockGlueEnabled = false;
        ConfigHelper.Save(appConfig);
        ShowLog("UI", "屏蔽滚胶 已关闭");
    }

    // ✅ 屏蔽压套筒
    private void BlockSleeveCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        BlockSleeveEnabled = true;
        var appConfig = ConfigHelper.Load();
        appConfig.BlockSleeveEnabled = true;  // ⚡ 你需要在 AppConfig 类里加上这个字段
        ConfigHelper.Save(appConfig);
        ShowLog("UI", "屏蔽压套筒 已启用");
    }
    private void BlockSleeveCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        BlockSleeveEnabled = false;
        var appConfig = ConfigHelper.Load();
        appConfig.BlockSleeveEnabled = false;
        ConfigHelper.Save(appConfig);
        ShowLog("UI", "屏蔽压套筒 已关闭");
    }

    private void HistoryButton_Click(object sender, RoutedEventArgs e)
    {
        
        var win = new HistoryWindow();
        win.Owner = this;
        win.Show();
    }
}