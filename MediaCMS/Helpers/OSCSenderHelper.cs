using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace OSC_Test.Helpers
{
    public class OSCSenderHelper
    {
        private static OSCSenderHelper _instance { get; set; }
        public static OSCSenderHelper Instance
        {
            get
            {
                return _instance ?? (_instance = new OSCSenderHelper());
            }
        }

        public void Send(string ip, string address)
        {
            var sender = new SharpOSC.UDPSender(ip, 7000);
            var msg = new SharpOSC.OscMessage($"/composition/columns/{address}/connect", 1);
            sender.Send(msg);
        }

    }
}
