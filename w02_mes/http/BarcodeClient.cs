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
        for (int attempt = 1; attempt <= 2; attempt++) // 最多尝试两次
        {
            try
            {
                using TcpClient client = new TcpClient();
                
                // 设置连接超时
                var connectTask = client.ConnectAsync(ip, port);
                if (await Task.WhenAny(connectTask, Task.Delay(timeout)) != connectTask)
                {
                    if (attempt == 2) return ""; // 第二次仍超时直接返回
                    continue; // 第一次超时重试
                }

                using NetworkStream stream = client.GetStream();
                stream.ReadTimeout = timeout;
                stream.WriteTimeout = timeout;

                // 发送 start
                byte[] sendBytes = Encoding.UTF8.GetBytes("start");
                await stream.WriteAsync(sendBytes, 0, sendBytes.Length);

                // 接收返回数据
                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string result = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                 
                    return string.IsNullOrEmpty(result) ? "" : result;
                }

                if (attempt == 2) return ""; // 第二次没有数据直接返回
            }
            catch (Exception ex)
            {
                if (attempt == 2) // 第二次异常返回 null
                {
                    LogService.Error("scanner", $"ReadBarcode 异常: {ex.Message}");
                    return "";
                }
            }
        }

        return ""; // 不会走到这里，保留以防万一
    }
}