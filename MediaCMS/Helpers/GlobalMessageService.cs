using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp11.Helpers
{
    public static class GlobalMessageService
    {
        public static event EventHandler<string> MessageReceived;

        public static void ShowMessage(string message)
        {
            MessageReceived?.Invoke(null, message);
        }
    }
}
