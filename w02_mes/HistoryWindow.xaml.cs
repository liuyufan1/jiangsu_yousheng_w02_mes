using System;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ClosedXML.Excel;
using w02_mes.sqlServer;

namespace w02_mes;

public partial class HistoryWindow : Window
{
    private readonly ObservableCollection<DataEntityView> _items = new();
    private int _pageIndex = 1;
    private int _pageSize = 20;
    private int _total = 0;
    private int _pages = 0;
    private bool _loading = false;
    private string _lastKeyword = "";

    public HistoryWindow()
    {
        InitializeComponent();
        ResultGrid.ItemsSource = _items;

        // 默认每页 20
        PageSizeBox.SelectedIndex = 1; // 10, 20, 50, 100 => index 1 是 20
        UpdatePagerInfo();
    }

    // 触发：回车即搜索（从第 1 页开始）
    private void BarcodeBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            _pageIndex = 1;
            _ = LoadPageAsync();
        }
    }

    // 点击查询（从第 1 页开始）
    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        _pageIndex = 1;
        _ = LoadPageAsync();
    }

    private void PageSizeBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (int.TryParse((PageSizeBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString(), out var size))
        {
            _pageSize = size;
            _pageIndex = 1;
            _ = LoadPageAsync();
        }
    }

    private void PrevButton_Click(object sender, RoutedEventArgs e)
    {
        if (_pageIndex > 1)
        {
            _pageIndex--;
            _ = LoadPageAsync();
        }
    }

    private void NextButton_Click(object sender, RoutedEventArgs e)
    {
        if (_pageIndex < _pages)
        {
            _pageIndex++;
            _ = LoadPageAsync();
        }
    }

    private void PageIndexBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        // 仅在点击“跳转”时真正触发；这里不做逻辑
    }

    private void GotoButton_Click(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(PageIndexBox.Text, out var idx))
        {
            if (idx < 1) idx = 1;
            if (_pages > 0 && idx > _pages) idx = _pages;
            if (idx != _pageIndex)
            {
                _pageIndex = idx;
                _ = LoadPageAsync();
            }
        }
    }

    private async Task LoadPageAsync()
    {
        if (_loading) return;
        try
        {
            _loading = true;
            BusyBar.Visibility = Visibility.Visible;
            StatusText.Text = "搜索中…";

            var keyword = BarcodeBox.Text?.Trim() ?? "";
            _lastKeyword = keyword;

            // 分页查询（模糊搜索）
            var result = await DataRepository.GetByBarcodePagedAsync(
                barcodeKeyword: keyword,
                pageIndex: _pageIndex,
                pageSize: _pageSize);

            _items.Clear();
            foreach (var x in result.Records)
                _items.Add(new DataEntityView(x));

            _pageIndex = result.PageIndex;
            _pageSize  = result.PageSize;
            _total     = result.Total;
            _pages     = result.Pages;

            UpdatePagerInfo();

            StatusText.Text = $"已加载 {_items.Count} 条";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"查询失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusText.Text = "查询失败";
        }
        finally
        {
            BusyBar.Visibility = Visibility.Collapsed;
            _loading = false;
        }
    }

    private void UpdatePagerInfo()
    {
        PageIndexBox.Text = _pageIndex.ToString();
        PagerInfoText.Text = $"共 {_total} 条 · 第 {_pageIndex}/{Math.Max(_pages, 1)} 页";
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (ResultGrid.SelectedItem is not DataEntityView row)
        {
            row = (sender as FrameworkElement)?.DataContext as DataEntityView;
        }
        if (row == null) return;

        if (!row.CanExport)
        {
            MessageBox.Show("该条记录的 Step 并非以“滑台”开头，无法导出。", "提示",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            // 解析 JSON 并转换为 DataTable
            var dt = ConvertRivetJsonToDataTable(row.Data);

            if (dt.Rows.Count == 0)
            {
                MessageBox.Show("数据为空或格式不符合预期，未生成内容。", "提示",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var safeStep = MakeSafeFilePart(row.Step);
            var safeBarcode = MakeSafeFilePart(row.Barcode);
            var file = Path.Combine(desktop, $"{safeBarcode}_{safeStep}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("数据");
                ws.Cell(1, 1).InsertTable(dt, "RivetData", true);
                ws.Columns().AdjustToContents();
                wb.SaveAs(file);
            }

            MessageBox.Show($"已生成：{file}", "完成", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"导出失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static string MakeSafeFilePart(string s)
    {
        var invalid = Path.GetInvalidFileNameChars();
        foreach (var ch in invalid) s = s.Replace(ch, '_');
        return s;
    }

    /// <summary>
    /// 把“拉铆枪/配方限值/此次拉铆拉力/位移/判定结果”的 JSON 数组转换为 DataTable。
    /// 支持多把拉铆枪的连续段落。
    /// </summary>
    private static DataTable ConvertRivetJsonToDataTable(string json)
    {
        var dt = new DataTable();
        dt.Columns.Add("拉铆枪号", typeof(string));
        dt.Columns.Add("配方拉力上限", typeof(double));
        dt.Columns.Add("配方拉力下限", typeof(double));
        dt.Columns.Add("配方位移上限", typeof(double));
        dt.Columns.Add("配方位移下限", typeof(double));
        dt.Columns.Add("此次拉铆拉力", typeof(double));
        dt.Columns.Add("此次拉铆位移", typeof(double));
        dt.Columns.Add("判定结果", typeof(string));

        if (string.IsNullOrWhiteSpace(json)) return dt;

        TagItem[] items;
        try
        {
            var options = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };
            items = JsonSerializer.Deserialize<TagItem[]>(json, options) ?? Array.Empty<TagItem>();
        }
        catch
        {
            json = json.Trim('\uFEFF', '\u200B', '\u0000', ' ', '\r', '\n', '\t');
            items = JsonSerializer.Deserialize<TagItem[]>(json) ?? Array.Empty<TagItem>();
        }

        string gunName = "";
        double upForce = 0, lowForce = 0, upDisp = 0, lowDisp = 0;

        int i = 0;
        while (i < items.Length)
        {
            var tag = items[i].TagName?.Trim();

            if (tag == "拉铆枪：")
            {
                gunName = items[i].AsString();
                if (i + 4 < items.Length)
                {
                    upForce = items[i + 1].AsDouble();
                    lowForce = items[i + 2].AsDouble();
                    upDisp  = items[i + 3].AsDouble();
                    lowDisp = items[i + 4].AsDouble();
                }
                i += 5;
                continue;
            }

            if (tag == "此次拉铆拉力" && i + 2 < items.Length)
            {
                var force = items[i].AsDouble();
                var disp  = items[i + 1].AsDouble();
                var judge = items[i + 2].AsString();

                var row = dt.NewRow();
                row["拉铆枪号"] = gunName;
                row["配方拉力上限"] = upForce;
                row["配方拉力下限"] = lowForce;
                row["配方位移上限"] = upDisp;
                row["配方位移下限"] = lowDisp;
                row["此次拉铆拉力"] = force;
                row["此次拉铆位移"] = disp;
                row["判定结果"] = judge;
                dt.Rows.Add(row);

                i += 3;
                continue;
            }

            i++;
        }

        return dt;
    }

    private class TagItem
    {
        [JsonPropertyName("TagName")]
        public string TagName { get; set; } = "";

        [JsonPropertyName("Val")]
        public JsonElement Val { get; set; }

        public string AsString()
        {
            return Val.ValueKind switch
            {
                JsonValueKind.String => Val.GetString() ?? "",
                JsonValueKind.Number => Val.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => Val.GetRawText()
            };
        }

        public double AsDouble()
        {
            try
            {
                if (Val.ValueKind == JsonValueKind.Number && Val.TryGetDouble(out var d)) return d;
                if (Val.ValueKind == JsonValueKind.String && double.TryParse(Val.GetString(), out var s)) return s;
            }
            catch { }
            return 0d;
        }
    }

    /// <summary>
    /// DataGrid 的行模型：从 DataEntity 扩展，提供 CanExport
    /// </summary>
    public class DataEntityView : DataEntity
    {
        public bool CanExport => !string.IsNullOrWhiteSpace(Step) && Step.StartsWith("滑台");
        public DataEntityView() { }
        public DataEntityView(DataEntity e)
        {
            Id = e.Id;
            Barcode = e.Barcode;
            Step = e.Step;
            Data = e.Data;
            CreateTime = e.CreateTime;
        }
    }
}
