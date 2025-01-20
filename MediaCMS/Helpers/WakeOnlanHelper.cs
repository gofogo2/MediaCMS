using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WpfApp11.Helpers;

namespace Launcher_SE.Helpers
{
    public class WakeOnLan : UdpClient
    {
        public WakeOnLan()
            : base()
        {

        }

        /// <summary>
        /// 컴퓨터 부팅 하기
        /// </summary>
        /// <param name="macAddress">부팅 할 컴퓨터의 맥어드레스</param>
        public void TurnOnPC(string macAddress)
        {
            try { 
            this.Connect(new System.Net.IPAddress(0xffffffff), 0x2fff); //255.255.255.255 : 12287

            if (this.Active)
            {
                this.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 0);
            }

            byte[] bytes = GetMagicPacketToByteArray(macAddress);

            // 컴퓨터를 부팅 할 매직패킷을 보낸다.
            int reterned_value = this.Send(bytes, 1024);
        }catch(Exception e)
            {
                Logger.LogError($"TurnOnPC Error :mac-{macAddress} , {e.Message}");
            }
}

        public void TurnOnPC(string ip, string macAddress)
        {
            try
            {
                this.Connect(ip, 12287); //255.255.255.255 : 12287

                if (this.Active)
                {
                    this.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 0);
                }

                byte[] bytes = GetMagicPacketToByteArray(macAddress);

                // 컴퓨터를 부팅 할 매직패킷을 보낸다.
                int reterned_value = this.Send(bytes, 1024);
            }catch(Exception e)
            {
                Logger.LogError($"Error : {e.Message}");
            }
        }

        private byte[] GetMagicPacketToByteArray(string macAddress)
        {
            // 보낼 바이트 초기화
            int counter = 0;

            // 보낼 버퍼 초기화
            byte[] bytes = new byte[1024];

            // 처음 6개 바이트는 "0xFF"  
            for (int y = 0; y < 6; y++)
                bytes[counter++] = 0xFF;

            // 부팅 할 컴퓨터의 맥어드레스를 16번 반복한다.
            for (int y = 0; y < 16; y++)
            {
                int i = 0;
                for (int z = 0; z < 6; z++)
                {
                    bytes[counter++] =
                        byte.Parse(macAddress.Substring(i, 2),
                        System.Globalization.NumberStyles.HexNumber);
                    i += 2;
                }
            }
            return bytes;
        }
    }
}
