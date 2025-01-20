using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WpfApp11.Helpers;

namespace TcpHelperNamespace
{
    public class TcpHelper
    {
        private static TcpHelper _instance;
        private static readonly object _lock = new object();

        public static TcpHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new TcpHelper();
                        }
                    }
                }
                return _instance;
            }
        }

        private TcpHelper() { }

        public async Task<string> Send(string serverIp, int port, string message)
        {
            using (TcpClient client = new TcpClient())
            {
                try
                {
                    await client.ConnectAsync(serverIp, port);
                    using (NetworkStream stream = client.GetStream())
                    {
                        byte[] data = Encoding.UTF8.GetBytes(message);
                        await stream.WriteAsync(data, 0, data.Length);

                        data = new byte[1024];
                        int bytesRead = await stream.ReadAsync(data, 0, data.Length);
                        return Encoding.UTF8.GetString(data, 0, bytesRead);
                    }
                }
                catch (SocketException ex)
                {
                    Logger.LogError($"Error : {ex.Message}");
                    throw new Exception($"서버 연결 또는 통신 오류: {ex.Message}", ex);
                    
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error : {ex.Message}");
                    throw new Exception($"예기치 않은 오류 발생: {ex.Message}", ex);
                }
            }
        }
    }
}