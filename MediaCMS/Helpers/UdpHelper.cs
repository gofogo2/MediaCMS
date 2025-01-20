using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using WpfApp11.Helpers;

namespace Launcher_SE.Helpers
{
    public static class NetworkHelper
    {
        public static string GetMyIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public static bool IsLocalHost(IPEndPoint ep)
        {
            return ep.Address.ToString().Equals(GetMyIpAddress());
        }
    }

    public static class ByteConverter
    {
        public static object ByteToObject(byte[] buffer)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(buffer))
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    stream.Position = 0;
                    return binaryFormatter.Deserialize(stream);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
            return null;
        }

        public static byte[] ObjectToByte(object obj)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(stream, obj);
                    return stream.ToArray();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
            return null;
        }

        public static string ByteToString(byte[] buffer)
        {
            string str = Encoding.Default.GetString(buffer);
            return str;
        }

        public static byte[] StringToByte(string str)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(str);
            return buffer;
        }
    }

    public class UdpHelper
    {
        private const string ServerHost = "127.0.0.1";
        private const string ServerBroadcast = "255.255.255.255";
        private const int ReceiverPort = 8888;
        private const int SenderPort = 8889;

        private static UdpHelper _instance;
        public static UdpHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UdpHelper();
                }
                return _instance;
            }
        }

        public delegate void PacketReceiveEventHandler(string code);
        public event PacketReceiveEventHandler PacketReceived;

        private UdpHelper()
        {
            Task.Run(() => ReceiveAsync());
        }

        public Task SendAsync(string msg, bool isBroadcast = false, int port = SenderPort)
        {
            return SendByteAsync(ByteConverter.StringToByte(msg), isBroadcast, port);
        }

        public Task SendWithIpAsync(string msg, string ip, int port)
        {
            return SendByteAsync(ByteConverter.StringToByte(msg), false, port, ip);
        }

        //public Task SendWithIpDLPProjectorAsync(string msg, string ip, int port)
        //{
        //    return SendByteAsync(ByteConverter.StringToByte(msg), false, port, ip);
        //}

        //public async Task<bool> SendPowerOnCommandToDLPProjector(string projectorIp)
        //{
        //    string hexCommand = "41542B53797374656D3D4F6E41542B53797374656D3D4F6E0D";
        //    byte[] commandBytes = StringToByteArray(hexCommand);
        //    string commandString = Encoding.ASCII.GetString(commandBytes);

        //    await UdpHelper.Instance.SendWithIpDLPProjectorAsync(commandString, projectorIp, 4352);
        //    return true;
        //}

        //public async Task<bool> SendPowerOffCommandToDLPProjector(string projectorIp)
        //{
        //    string hexCommand = "41542B53797374656D3D4F66660D";
        //    byte[] commandBytes = StringToByteArray(hexCommand);
        //    string commandString = Encoding.ASCII.GetString(commandBytes);

        //    await UdpHelper.Instance.SendWithIpDLPProjectorAsync(commandString, projectorIp, 4352);
        //    return true;
        //}

        private byte[] StringToByteArray(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        public Task SendObjectAsync(object msg, int port = SenderPort)
        {
            return SendByteAsync(ByteConverter.ObjectToByte(msg), false, port);
        }

        public Task SendByteAsync(byte[] msg, bool isBroadcast = false, int port = SenderPort, string ip = null)
        {
            return Task.Run(async () =>
            {
                try
                {
                    using (var sender = new UdpClient())
                    {
                        var endpoint = new IPEndPoint(
                            IPAddress.Parse(ip ?? (isBroadcast ? ServerBroadcast : ServerHost)),
                            port
                        );
                        await sender.SendAsync(msg, msg.Length, endpoint);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"SendByteAsync error: {e.Message}");
                    Logger.LogError($"Error : {e.Message}");
                }
            });
        }

        public Task SendHexAsync(string hexString, bool isBroadcast = false, int port = SenderPort, string ip = null)
        {
            return Task.Run(async () =>
            {
                try
                {
                    // 16진수 문자열을 바이트 배열로 변환
                    byte[] msg = HexStringToByteArray(hexString);

                    using (var sender = new UdpClient())
                    {
                        var endpoint = new IPEndPoint(
                            IPAddress.Parse(ip ?? (isBroadcast ? ServerBroadcast : ServerHost)),
                            port
                        );
                        await sender.SendAsync(msg, msg.Length, endpoint);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"SendHexAsync error: {e.Message}");
                    Logger.LogError($"Error : {e.Message}");
                }
            });
        }

        private byte[] HexStringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }


        public Task SendAsciiAsyncASCII(string msg, bool isBroadcast = false, int port = SenderPort, string ip = null)
        {
            return Task.Run(async () =>
            {
                try
                {
                    using (var sender = new UdpClient())
                    {
                        var endpoint = new IPEndPoint(
                            IPAddress.Parse(ip ?? (isBroadcast ? ServerBroadcast : ServerHost)),
                            port
                        );
                        byte[] asciiBytes = Encoding.ASCII.GetBytes(msg);
                        await sender.SendAsync(asciiBytes, asciiBytes.Length, endpoint);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"SendAsciiAsync error: {e.Message}");
                    Logger.LogError($"Error : {e.Message}");
                }
            });
        }

        private async Task ReceiveAsync(int port = ReceiverPort)
        {
            try
            {
                using (var receiver = new UdpClient(port))
                {
                    while (true)
                    {
                        var result = await receiver.ReceiveAsync();
                        if (!NetworkHelper.IsLocalHost(result.RemoteEndPoint))
                        {
                            var msg = ByteConverter.ByteToString(result.Buffer);
                            if (PacketReceived != null)
                            {
                                PacketReceived(msg);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Error : {e.Message}");
                Debug.WriteLine($"ReceiveAsync error: {e.Message}");
                await Task.Delay(1000); // 오류 발생 시 1초 대기 후 재시도
                await ReceiveAsync(port);
            }
        }
    }
}