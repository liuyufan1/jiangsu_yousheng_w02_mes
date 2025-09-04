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
    public static async Task<string> ReadBarcodeAsync(string ip, int port = 2001, int timeout = 2000)
    {
        for (int attempt = 1; attempt <= 2; attempt++)
        {
            try
            {
                using var client = new TcpClient();

                // ConnectAsync 超时控制
                var connectTask = client.ConnectAsync(ip, port);
                if (await Task.WhenAny(connectTask, Task.Delay(timeout)) != connectTask)
                {
                    client.Close();
                    if (attempt == 2) return "";
                    continue;
                }

                using var stream = client.GetStream();

                // 发送 start
                byte[] sendBytes = Encoding.UTF8.GetBytes("start");
                using var ctsWrite = new CancellationTokenSource(timeout);
                await stream.WriteAsync(sendBytes, 0, sendBytes.Length, ctsWrite.Token);

                // 接收返回
                byte[] buffer = new byte[1024];
                using var ctsRead = new CancellationTokenSource(timeout);
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ctsRead.Token);

                if (bytesRead > 0)
                {
                    string result = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    return string.IsNullOrEmpty(result) ? "" : result;
                }

                if (attempt == 2) return "";
            }
            catch (Exception ex)
            {
                if (attempt == 2)
                {
                    LogService.Error("scanner", $"ReadBarcode 异常: {ex.Message}");
                    return "";
                }
            }
        }

        return "";
    }

}
