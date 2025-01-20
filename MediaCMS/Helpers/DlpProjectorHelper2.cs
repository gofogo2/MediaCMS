using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WpfApp11.Helpers
{
    class DlpProjectorHelper2 : IDisposable
    {
        private const int ProjectorPort = 4352;
        private const int DefaultTimeout = 11000; // 5초 타임아웃
        private const int MaxRetries = 1; // 최대 재시도 횟수
        private const int MaxPoolSize = 5; // 최대 연결 풀 크기

        private readonly string projectorIp;
        private readonly ConcurrentBag<TcpClient> connectionPool;
        private readonly SemaphoreSlim poolLock;
        private bool isDisposed = false;

        private static class Commands
        {
            public const string PowerOn = "41542B53797374656D3D4F6E41542B53797374656D3D4F6E0D";
            public const string PowerOff = "41542B53797374656D3D4F66660D";
            public const string PowerStatus = "41542B53797374656D3F41542B53797374656D3F0D";
        }

        public DlpProjectorHelper2(string projectorIp)
        {
            this.projectorIp = projectorIp;
            this.connectionPool = new ConcurrentBag<TcpClient>();
            this.poolLock = new SemaphoreSlim(1, 1);
        }

        private async Task<TcpClient> GetConnectionAsync(int timeout = DefaultTimeout)
        {
            await poolLock.WaitAsync();
            try
            {
                if (connectionPool.TryTake(out TcpClient client))
                {
                    if (client.Connected)
                    {
                        return client;
                    }
                    client.Dispose();
                }

                client = new TcpClient();
                using (var cts = new CancellationTokenSource(timeout))
                {
                    var connectTask = client.ConnectAsync(projectorIp, ProjectorPort);
                    if (await Task.WhenAny(connectTask, Task.Delay(timeout, cts.Token)) != connectTask)
                    {
                        throw new TimeoutException($"Connection attempt timed out for {projectorIp}:{ProjectorPort}");
                    }
                    await connectTask; // 예외를 전파하기 위해
                }
                client.ReceiveTimeout = timeout;
                client.SendTimeout = timeout;

                Debug.WriteLine($"Successfully connected to {projectorIp}:{ProjectorPort}");
                return client;
            }
            finally
            {
                poolLock.Release();
            }
        }

        private void ReturnConnection(TcpClient client)
        {
            if (connectionPool.Count < MaxPoolSize && client.Connected)
            {
                connectionPool.Add(client);
            }
            else
            {
                client.Dispose();
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
                TcpClient client = null;
                try
                {
                    client = await GetConnectionAsync();
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
                finally
                {
                    if (client != null)
                        ReturnConnection(client);
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

            TcpClient client = null;
            try
            {
                client = await GetConnectionAsync();
                string response = await SendCommandAsync(client, command);
                return ParsePowerStatus(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in {operationType}: {ex.Message}");
                return "Error";
            }
            finally
            {
                if (client != null)
                    ReturnConnection(client);
            }
        }

        private async Task<string> SendCommandAsync(TcpClient client, string hexCommand)
        {
            using (var stream = client.GetStream())
            {
                byte[] commandBytes = StringToByteArray(hexCommand);
                await stream.WriteAsync(commandBytes, 0, commandBytes.Length);

                byte[] buffer = new byte[1024];
                using (var cts = new CancellationTokenSource(client.ReceiveTimeout))
                {
                    Task<int> readTask = stream.ReadAsync(buffer, 0, buffer.Length, cts.Token);
                    if (await Task.WhenAny(readTask, Task.Delay(client.ReceiveTimeout)) != readTask)
                    {
                        throw new TimeoutException("Read operation timed out.");
                    }

                    int bytesRead = await readTask;
                    return Encoding.ASCII.GetString(buffer, 0, bytesRead);
                }
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
            if (!isDisposed)
            {
                foreach (var client in connectionPool)
                {
                    client.Dispose();
                }
                poolLock.Dispose();
                isDisposed = true;
            }
        }
    }
}