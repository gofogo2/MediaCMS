using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp11.Helpers
{
    class DlpProjectorHelper3 : IDisposable
    {
        private const int ProjectorPort = 4352;
        private const int DefaultTimeout = 11000; // 5초 타임아웃
        private const int MaxRetries = 1; // 최대 재시도 횟수

        private readonly string projectorIp;
        private bool isDisposed = false;

        private static class Commands
        {
            public const string PowerOn = "41542B53797374656D3D4F6E41542B53797374656D3D4F6E0D";
            public const string PowerOff = "41542B53797374656D3D4F66660D";
            public const string PowerStatus = "41542B53797374656D3F41542B53797374656D3F0D";
        }

        public DlpProjectorHelper3(string projectorIp)
        {
            this.projectorIp = projectorIp;
        }

        private async Task<TcpClient> CreateConnectionAsync(int timeout = DefaultTimeout)
        {
            var client = new TcpClient();
            try
            {
                var connectTask = client.ConnectAsync(projectorIp, ProjectorPort);
                if (await Task.WhenAny(connectTask, Task.Delay(timeout)) != connectTask)
                {
                    throw new TimeoutException($"Connection attempt timed out for {projectorIp}:{ProjectorPort}");
                }

                // 연결 작업이 완료될 때까지 대기
                await connectTask;

                client.ReceiveTimeout = timeout;
                client.SendTimeout = timeout;
                Debug.WriteLine($"Successfully connected to {projectorIp}:{ProjectorPort}");
                return client;
            }
            catch
            {
                client.Dispose();
                throw;
            }
        }

        public async Task<bool> SendPowerOnCommandAsync()
        {
            return await SendPowerCommandAsync(Commands.PowerOn, "PowerOn");
        }

        public async Task<bool> SendPowerOffCommandAsync()
        {
            return await SendPowerCommandAsync(Commands.PowerOff, "PowerOff");
        }

        public async Task<string> GetPowerStatusAsync()
        {
            return await SendStatusCommandAsync(Commands.PowerStatus, "PowerStatus");
        }

        private async Task<bool> SendPowerCommandAsync(string command, string operationType)
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(nameof(DlpProjectorHelper2));
            }

            for (int retry = 0; retry < MaxRetries; retry++)
            {
                using (TcpClient client = await CreateConnectionAsync())
                {
                    try
                    {
                        string response = await SendCommandAsync(client, command);
                        Debug.WriteLine($"{operationType} Response: {response}");
                        return ParsePowerCommandResponse(response);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error in {operationType} (Attempt {retry + 1}): {ex.Message}");
                        if (retry == MaxRetries - 1)
                        {
                            return false;
                        }
                        await Task.Delay(1000 * (retry + 1)); // Exponential backoff
                    }
                }
            }
            return false;
        }

        private async Task<string> SendStatusCommandAsync(string command, string operationType)
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(nameof(DlpProjectorHelper2));
            }

            using (TcpClient client = await CreateConnectionAsync())
            {
                try
                {
                    string response = await SendCommandAsync(client, command);
                    return ParsePowerStatus(response);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in {operationType}: {ex.Message}");
                    return "Error";
                }
            }
        }

        private async Task<string> SendCommandAsync(TcpClient client, string hexCommand)
        {
            using (var stream = client.GetStream())
            {
                byte[] commandBytes = StringToByteArray(hexCommand);
                await stream.WriteAsync(commandBytes, 0, commandBytes.Length);

                byte[] buffer = new byte[1024];
                var readTask = stream.ReadAsync(buffer, 0, buffer.Length);
                if (await Task.WhenAny(readTask, Task.Delay(client.ReceiveTimeout)) != readTask)
                {
                    throw new TimeoutException("Read operation timed out.");
                }

                int bytesRead = await readTask;
                return Encoding.ASCII.GetString(buffer, 0, bytesRead);
            }
        }

        private bool ParsePowerCommandResponse(string response)
        {
            return response.ToUpper().Contains("OK");
        }

        private string ParsePowerStatus(string response)
        {
            if (response.ToUpper().Contains("ON"))
            {
                return "Powered On";
            }
            else if (response.ToUpper().Contains("OFF"))
            {
                return "Powered Off";
            }
            else
            {
                return "Unknown Status";
            }
        }

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

        public void Dispose()
        {
            isDisposed = true;
        }
    }
}