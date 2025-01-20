using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp11.Helpers
{

    class Utils
    {
        private static Utils _instance = null;
        private static readonly object _padlock = new object();

        private Utils() { }

        public static Utils Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_padlock)
                    {
                        if (_instance == null)
                        {
                            _instance = new Utils();
                        }
                    }
                }
                return _instance;
            }
        }


        public string IntToHex(string it)
        {
            string hex;
            switch (it)
            {
                case "1":
                    hex = "31";
                    break;
                case "2":
                    hex = "32";
                    break;
                case "3":
                    hex = "33";
                    break;
                case "4":
                    hex = "34";
                    break;
                case "5":
                    hex = "35";
                    break;
                case "6":
                    hex = "36";
                    break;
                case "7":
                    hex = "37";
                    break;
                case "8":
                    hex = "38";
                    break;
                case "9":
                    hex = "39";
                    break;
                case "10":
                    hex = "3a";
                    break;
                case "11":
                    hex = "3b";
                    break;
                case "12":
                    hex = "3c";
                    break;
                case "13":
                    hex = "3d";
                    break;
                case "14":
                    hex = "3e";
                    break;
                case "15":
                    hex = "3f";
                    break;
                case "16":
                    hex = "40";
                    break;
                default:
                    hex = "31";
                    break;
            }
            return hex;
        }
    }
}
