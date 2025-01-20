using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WpfApp11.Helpers
{
    public enum PowerOption
    {
        off,
        reboot,
        on
    }
    public class UdpManager
    {
        private static UdpManager _instance;
        private static readonly object _lock = new object();

        private UdpClient listener;
        private Thread udpThread;
        private int udpPort;

        public event Action<bool> ConnectionStatusChanged;

        private UdpManager()
        {
            udpPort = 11116;
        }

        public static UdpManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new UdpManager();
                        }
                    }
                }
                return _instance;
            }
        }

        public void StartListening()
        {
            udpThread = new Thread(new ThreadStart(ListenForData));
            udpThread.IsBackground = true;
            udpThread.Start();
        }

        private void ListenForData()
        {
            listener = new UdpClient(udpPort);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, udpPort);

            try
            {
                while (true)
                {
                    byte[] bytes = listener.Receive(ref groupEP);
                    string message = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                    message = message.Replace("\r", "").Replace("\n", "");

                    if (message.Contains("power"))
                    {
                        var msgs = message.Split('|');
                        if (msgs.Length == 2)
                        {
                            if (msgs[1] == PowerOption.off.ToString())
                            {
                                Process.Start("Shutdown.exe", "-s -f -t 00");
                            }
                            else if (msgs[1] == PowerOption.reboot.ToString())
                            {
                                Process.Start("Shutdown.exe", "-r -f -t 00");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                ConnectionStatusChanged?.Invoke(false);
            }
        }

        public void Stop()
        {
            listener?.Close();
            udpThread?.Abort();
        }
    }
}