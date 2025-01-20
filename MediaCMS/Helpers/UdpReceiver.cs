using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp11.Helpers
{
    public class UdpReceiver
    {
        private UdpClient _udpClient;
        private IPEndPoint _remoteEndPoint;
        private IPAddress _allowedIp;

        public UdpReceiver(int port, IPAddress allowedIp)
        {
            _udpClient = new UdpClient(port);
            _remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
            _allowedIp = allowedIp;
        }

        public async Task StartReceivingAsync(Action<string> onMessageReceived, Action<string> onInvalidIpReceived)
        {
            while (true)
            {
                var result = await _udpClient.ReceiveAsync();
                var message = Encoding.UTF8.GetString(result.Buffer);

                // 수신된 패킷의 IP 주소가 허용된 IP와 일치하는지 확인합니다.
                //if (result.RemoteEndPoint.Address.Equals(""))
                //{
                    
                //}

                onMessageReceived?.Invoke(message);

                await Task.Delay(2000);


                //else
                //{
                //    onInvalidIpReceived?.Invoke(message);
                //}
            }
        }

        public void Stop()
        {
            _udpClient?.Close();
        }
    }
}