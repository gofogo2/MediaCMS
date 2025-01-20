using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class PJLinkHelper : IDisposable
{
    private const int DefaultTimeout = 5000; // 5초 타임아웃
    private const int MaxRetries = 3; // 최대 재시도 횟수
    private const int MaxPoolSize = 5; // 최대 연결 풀 크기
    private readonly string ipAddress;
    private readonly int port;
    private readonly ConcurrentBag<TcpClient> connectionPool;
    private readonly SemaphoreSlim poolLock;
    private bool isDisposed;

    public PJLinkHelper(string ipAddress, int port = 4352)
    {
        this.ipAddress = ipAddress;
        this.port = port;
        this.connectionPool = new ConcurrentBag<TcpClient>();
        this.poolLock = new SemaphoreSlim(1, 1);
    }

    public async Task ConnectAsync(int timeout = DefaultTimeout)
    {
        TcpClient client = await GetConnectionAsync(timeout);
        ReturnConnection(client);
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
                var connectTask = client.ConnectAsync(ipAddress, port);
                if (await Task.WhenAny(connectTask, Task.Delay(timeout, cts.Token)) != connectTask)
                {
                    throw new TimeoutException("Connection attempt timed out");
                }
                await connectTask; // 예외를 전파하기 위해
            }
            client.ReceiveTimeout = timeout;
            client.SendTimeout = timeout;

            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            using (var cts = new CancellationTokenSource(timeout))
            {
                var readTask = stream.ReadAsync(buffer, 0, buffer.Length, cts.Token);
                if (await Task.WhenAny(readTask, Task.Delay(timeout, cts.Token)) != readTask)
                {
                    throw new TimeoutException("Read operation timed out");
                }
                int bytesRead = await readTask;
                string initialResponse = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
                if (!initialResponse.StartsWith("PJLINK 0"))
                {
                    throw new Exception($"Unexpected initial PJLink response: {initialResponse}");
                }
            }

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

    public async Task<bool> PowerOnAsync()
    {
        return await ExecuteCommandAsync("%1POWR 1", InterpretPowerResponse);
    }

    public async Task<bool> PowerOffAsync()
    {
        return await ExecuteCommandAsync("%1POWR 0", InterpretPowerResponse);
    }

    public async Task<PowerStatus> GetPowerStatusAsync()
    {
        return await ExecuteCommandAsync("%1POWR ?", InterpretPowerStatusResponse);
    }

    private async Task<T> ExecuteCommandAsync<T>(string command, Func<string, T> interpreter)
    {
        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            TcpClient client = null;
            try
            {
                client = await GetConnectionAsync();
                string response = await SendCommandAsync(client, command);
                return interpreter(response);
            }
            catch (Exception ex) when (ex is SocketException || ex is IOException || ex is TimeoutException)
            {
                if (attempt == MaxRetries - 1)
                    throw;
                await Task.Delay(1000); // 1초 대기 후 재시도
            }
            finally
            {
                if (client != null)
                    ReturnConnection(client);
            }
        }
        throw new Exception("Failed to execute command after multiple attempts");
    }

    private async Task<string> SendCommandAsync(TcpClient client, string command)
    {
        NetworkStream stream = client.GetStream();
        byte[] data = Encoding.ASCII.GetBytes(command + "\r");
        await stream.WriteAsync(data, 0, data.Length);

        byte[] buffer = new byte[1024];
        using (var cts = new CancellationTokenSource(client.ReceiveTimeout))
        {
            var readTask = stream.ReadAsync(buffer, 0, buffer.Length, cts.Token);
            if (await Task.WhenAny(readTask, Task.Delay(client.ReceiveTimeout)) != readTask)
            {
                throw new TimeoutException("Read operation timed out");
            }
            int bytesRead = await readTask;
            return Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
        }
    }

    private bool InterpretPowerResponse(string response)
    {
        if (response.EndsWith("OK"))
            return true;
        else if (response.StartsWith("%1POWR=ERR"))
        {
            Console.WriteLine($"Power command failed with error: {response}");
            return false;
        }
        else
            throw new Exception($"Unexpected response to power command: {response}");
    }

    private PowerStatus InterpretPowerStatusResponse(string response)
    {
        if (response.StartsWith("%1POWR="))
        {
            string status = response.Substring(7).Trim();
            switch (status)
            {
                case "0": return PowerStatus.StandBy;
                case "1": return PowerStatus.PoweredOn;
                case "2": return PowerStatus.Cooling;
                case "3": return PowerStatus.WarmUp;
                default: return PowerStatus.Unknown;
            }
        }
        throw new Exception($"Unexpected response to power status query: {response}");
    }

    public void Dispose()
    {
        if (isDisposed)
            return;

        isDisposed = true;
        foreach (var client in connectionPool)
        {
            client.Dispose();
        }
        poolLock.Dispose();
    }
}

public enum PowerStatus
{
    Unknown,
    StandBy,
    PoweredOn,
    Cooling,
    WarmUp
}