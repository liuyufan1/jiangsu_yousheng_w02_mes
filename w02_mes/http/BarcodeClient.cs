using System.Net.Sockets;
using System.Text;
using Serilog;
using w02_mes.start;

namespace w02_mes.http;

// 通过 tcp 获取扫码器条码
public class BarcodeClient
{
    /// <summary>
    /// 向指定 IP 的 2001 端口发送 "start" 命令，并返回扫码器返回的条码
    /// 如果失败，会再重试一次
    /// </summary>
    public static async Task<string> ReadBarcodeAsync(string ip, int port = 2001, int timeout = 1000)
    {
        int scannerCount = 5;

        for (int attempt = 1; attempt <= scannerCount; attempt++)
        {
            try
            {
                using var client = new TcpClient();

                // ConnectAsync 超时控制
                var connectTask = client.ConnectAsync(ip, port);
                if (await Task.WhenAny(connectTask, Task.Delay(timeout)).ConfigureAwait(false) != connectTask)
                {
                    client.Close();
                    if (attempt == scannerCount)
                    {
                        LogService.Error("scanner", $"ReadBarcode 超时");
                        return "timeout";
                    }
                    continue;
                }

                await connectTask.ConfigureAwait(false);

                using var stream = client.GetStream();

                // 发送 start
                byte[] sendBytes = Encoding.UTF8.GetBytes("start");
                using var ctsWrite = new CancellationTokenSource(timeout);
                await stream.WriteAsync(sendBytes, 0, sendBytes.Length, ctsWrite.Token).ConfigureAwait(false);

                // 接收返回
                byte[] buffer = new byte[1024];
                using var ctsRead = new CancellationTokenSource(timeout);
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ctsRead.Token).ConfigureAwait(false);

                string result = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                LogService.Information("scanner", $"扫码结果: {result}");

                if (!string.IsNullOrEmpty(result) && !result.Equals("noRead", StringComparison.OrdinalIgnoreCase))
                {
                    return result;
                }

                if (attempt == scannerCount)
                {
                    LogService.Error("scanner", $"result is nullOrEmptyOrNoRead: {result}");
                    return "noRead";
                }
            }
            catch (Exception ex)
            {
                if (attempt == scannerCount)
                {
                    LogService.Error("scanner", $"ReadBarcode 异常: {ex.Message}");
                    return "error";
                }
            }

            await Task.Delay(50).ConfigureAwait(false); // 小间隔再重试
        }

        return "noRead";
    }


}
