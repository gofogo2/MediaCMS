using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MediaCMS
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {

        Mutex mutex = null;

        public App()
        {
            // 어플리케이션 이름 확인
            string applicationName = Process.GetCurrentProcess().ProcessName;
            Duplicate_execution(applicationName);

        }

        private void Duplicate_execution(string mutexName)
        {
            try
            {
                mutex = new Mutex(false, mutexName);
            }
            catch (Exception ex)
            {
                Application.Current.Shutdown();
            }
            if (mutex.WaitOne(0, false))
            {
                InitializeComponent();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }
    }
}
