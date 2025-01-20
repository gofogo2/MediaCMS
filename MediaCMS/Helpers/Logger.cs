using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp11.Helpers
{
 
    public static class Logger
    {
        public static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory+"Log\\", $"{DateTime.Now:yyyy-MM-dd}"+"_device_logs.txt");
        public static readonly string LogPowerFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Log\\", $"{DateTime.Now:yyyy-MM-dd}" + "_power_logs.txt");
        public static readonly string LogErrorFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Log\\", $"{DateTime.Now:yyyy-MM-dd}" + "_error_logs.txt");


        public static void CreateD()
        {
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Log"))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Log");
            }
        }

        public static void Log(string deviceName, string deviceType, string action, string result)
        {
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - 아이피: {deviceName} ({deviceType}) - 포트: {action} - 내용: {result}";
            
            try
            {
                CreateD();
                File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }

        public static void Log2(string result)
        {
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - 내용: {result}";

            try
            {
                CreateD();
                File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }

        public static void LogPower(string result)
        {
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - 내용: {result}";

            try
            {
                CreateD();
                File.AppendAllText(LogPowerFilePath, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }

        public static void LogError(string result)
        {
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - 내용: {result}";

            try
            {
                CreateD();
                File.AppendAllText(LogErrorFilePath, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }
    }
}
