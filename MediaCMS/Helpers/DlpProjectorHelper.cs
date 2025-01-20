using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
using System.Diagnostics;
using WpfApp11.Helpers;

public class DlpProjectorHelper : IDisposable
{
    private const int ProjectorPort = 4352;
    private const int TimeoutMilliseconds = 11000;
    private const int MaxRetries = 3;
    private readonly string projectorIp;
    private TcpClient client;
    private bool isDisposed = false;

    private static class Commands
    {
        public const string PowerOn = "41542B53797374656D3D4F6E41542B53797374656D3D4F6E0D";
        public const string PowerOff = "41542B53797374656D3D4F66660D";
        public const string PowerStatus = "41542B53797374656D3F41542B53797374656D3F0D";
    }

    public DlpProjectorHelper(string projectorIp)
    {
        this.projectorIp = projectorIp;
    }

    private async Task EnsureConnectedAsync()
    {
        const int MaxRetries = 1; // 명시적으로 최대 재시도 횟수 설정
        for (int retry = 0; retry < MaxRetries; retry++)
        {
            try
            {
                if (client != null)
                {
                    client.Close();
                    client.Dispose();
                }
                client = new TcpClient();

                // 연결 시도 시 타임아웃 설정
                var connectTask = client.ConnectAsync(projectorIp, ProjectorPort);
                if (await Task.WhenAny(connectTask, Task.Delay(5000)) != connectTask)
                {
                    throw new TimeoutException($"Connection attempt timed out for {projectorIp}:{ProjectorPort}");
                }

                await connectTask; // 실제 예외를 발생시키기 위해

                if (client.Connected)
                {
                    Debug.WriteLine($"Successfully connected to {projectorIp}:{ProjectorPort} on attempt {retry + 1}");
                    return; // 연결 성공 시 메서드 종료
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Connection attempt {retry + 1} failed for {projectorIp}:{ProjectorPort}: {ex.Message}");
                if (retry == MaxRetries - 1)
                {
                    throw new Exception($"Failed to connect to {projectorIp}:{ProjectorPort} after {MaxRetries} attempts", ex);
                }
                await Task.Delay(1000 * (retry + 1)); // Exponential backoff
            }
        }
        throw new Exception($"Failed to connect to {projectorIp}:{ProjectorPort} after {MaxRetries} attempts");
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
            throw new ObjectDisposedException(nameof(DlpProjectorHelper));
        }

        for (int retry = 0; retry < MaxRetries; retry++)
        {
            try
            {
                await EnsureConnectedAsync();
                string response = await SendCommandAsync(command);
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
        return false;
    }

    private async Task<string> SendStatusCommandAsync(string command, string operationType)
    {
        if (isDisposed)
        {
            throw new ObjectDisposedException(nameof(DlpProjectorHelper));
        }

        //for (int retry = 0; retry < MaxRetries; retry++)
        //{
        try
        {
            await EnsureConnectedAsync();
            string response = await SendCommandAsync(command);
            Debug.WriteLine($"{operationType} Response: {response}");
            //Logger.Log2($"{operationType} Response: {response}");
            return ParsePowerStatus(response);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error in {operationType} Response: {ex.Message}");
            //Debug.WriteLine($"Error in {operationType} (Attempt {retry + 1}): {ex.Message}");
            //if (retry == MaxRetries - 1)
            //{
            //    return "Error";
            //}
            //await Task.Delay(1000 * (retry + 1)); // Exponential backoff
            return "Error";
        }
        //}
        return "Error";
    }

    private async Task<string> SendCommandAsync(string hexCommand)
    {
        using (var stream = client.GetStream())
        {
            stream.WriteTimeout = TimeoutMilliseconds;
            byte[] commandBytes = StringToByteArray(hexCommand);
            await stream.WriteAsync(commandBytes, 0, commandBytes.Length);

            byte[] buffer = new byte[1024];
            using (var cts = new System.Threading.CancellationTokenSource(TimeoutMilliseconds))
            {
                Task<int> readTask = stream.ReadAsync(buffer, 0, buffer.Length, cts.Token);
                if (await Task.WhenAny(readTask, Task.Delay(TimeoutMilliseconds, cts.Token)) != readTask)
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
            client?.Close();
            client?.Dispose();
            isDisposed = true;
        }
    }
}