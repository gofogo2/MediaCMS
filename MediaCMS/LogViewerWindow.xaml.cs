using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfApp11.Helpers;

namespace MediaCMS
{
    public partial class LogViewerWindow : Window
    {
        private string logFilePath;
        private FileSystemWatcher fileWatcher;
        private DispatcherTimer updateTimer;

        public LogViewerWindow(string logPath)
        {
            InitializeComponent();
            logFilePath = logPath;
            Logger.CreateD();
            fileWatcher = new FileSystemWatcher(System.IO.Path.GetDirectoryName(logFilePath));
            fileWatcher.Filter = System.IO.Path.GetFileName(logFilePath);
            fileWatcher.Changed += OnLogFileChanged;
            fileWatcher.EnableRaisingEvents = true;

            updateTimer = new DispatcherTimer();
            updateTimer.Interval = TimeSpan.FromSeconds(1);
            updateTimer.Tick += UpdateLogContent;
            updateTimer.Start();

            Closed += (s, e) =>
            {
                fileWatcher.Dispose();
                updateTimer.Stop();
            };

            UpdateLogContent(null, null);
        }

        private void OnLogFileChanged(object sender, FileSystemEventArgs e)
        {
            Dispatcher.Invoke(() => UpdateLogContent(null, null));
        }

        private void UpdateLogContent(object sender, EventArgs e)
        {
            try
            {
                string content = File.ReadAllText(logFilePath);
                LogTextBox.Text = content;
                LogTextBox.ScrollToEnd();
            }
            catch (IOException)
            {
                // File might be locked, we'll try again on the next timer tick
            }
        }
    }
}