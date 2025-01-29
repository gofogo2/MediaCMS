using Launcher_SE.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfApp11.Helpers;
using WpfApp11.Helpers.Launcher_SE.Helpers;
using WpfApp11.UserControls;

namespace MediaCMS
{
    public partial class DraggableItemControl : UserControl
    {
        private DlpProjectorHelper3 dlpProjectorHelper;
        public WakeOnLan wol;
        private int PingInterval = 6000; // 5초마다 핑 체크
        private CancellationTokenSource pduStatusCancellationTokenSource;
        private CancellationTokenSource pingCancellationTokenSource;
        string vncViewerPath = @"C:\Program Files\TightVNC\tvnviewer.exe"; // TightVNC 뷰어 경로
        public ItemConfiguration Configuration { get; private set; }
      
        
        //SolidColorBrush onColor = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#ACD7FE"));
        //SolidColorBrush offColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));
           
        SolidColorBrush onColor = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#FF00F61C"));
        SolidColorBrush offColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF0000"));
        private int relay_onoff = 0;
        public bool ispow = false;
        private int commonDelay = 1000;


        public DraggableItemControl(ItemConfiguration config)
        {
            InitializeComponent();
            Configuration = config;
            UpdateUI();
            wol = new WakeOnLan();

            var main = Application.Current.MainWindow as MainWindow;

            //PingInterval = main.pingtime * 1000;

            //if (PingInterval < 10000)
            //{
            //    PingInterval = 10000;
            //}

            PingInterval = main.pingtime;

            if (PingInterval < 5000)
            {
                PingInterval = 5000;
            }



            // 전체 상태체크
            if (Configuration.DeviceType.ToLower() == "pc")
            {
                //UpdatePowerState(false);
                StartPingCheck();
            }
            else if (Configuration.DeviceType.ToLower() == "프로젝터(pjlink)")
            {
                //UpdatePowerState(false);
                //isControllingProjectors = false;
                _ = Status();

            }
           else if (Configuration.DeviceType.ToLower() == "프로젝터(appotronics)")
            {
                
                dlpProjectorHelper = new DlpProjectorHelper3(Configuration.IpAddress);
                _ = AppotronicsStatus();
            }
            else if (Configuration.DeviceType == "PDU")
            {
                StartPDUStatus();
                //UpdatePowerState(false);
            }
            else if (Configuration.DeviceType == "RELAY")
            {
                UpdatePowerState(false);
                // RELAY 관련 코드
            }

            // ContextMenu 생성
            CreateContextMenu();
        }

        public async void StartPDUStatus()
        {
            StopPDUStatus(); // 기존 체크가 실행 중이면 중지
            pduStatusCancellationTokenSource = new CancellationTokenSource();
            await PDUStatusCheckLoop(pduStatusCancellationTokenSource.Token);
        }

        public void StopPDUStatus()
        {
            pduStatusCancellationTokenSource?.Cancel();
            pduStatusCancellationTokenSource?.Dispose();
            pduStatusCancellationTokenSource = null;
        }

        public void StopStatusDevice()
        {
            StopPDUStatus();
            StopPJLinkStatusCheck();
            StartPingCheck();
        }

        bool isDoing = false;

        private async Task PDUStatusCheckLoop(CancellationToken cancellationToken)
        {
            if (!isDoing)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        Dictionary<string, string> p = await WebApiHelper.Instance.StatusPDU(Configuration.IpAddress, Configuration.Channel);
                        await Dispatcher.InvokeAsync(() =>
                        {
                            if (!cancellationToken.IsCancellationRequested)
                            {
                                if (p.ContainsKey("Error"))
                                {
                                    UpdatePowerState(false);
                                }
                                else
                                {
                                    int channelNumber = Convert.ToInt32(Configuration.Channel);
                                    string formattedChannel = channelNumber.ToString("00");
                                    bool isOn = p[formattedChannel] == "ON";
                                    UpdatePowerState(isOn);
                                }
                            }
                        });

                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"PDU 상태 체크 중 오류 발생: {ex.Message}");
                        Debug.WriteLine($"PDU 상태 체크 중 오류 발생: {ex.Message}");
                    }

                    await Task.Delay(PingInterval, cancellationToken);
                }
            }
            }


        private async Task Status()
        {
            while (true)
            {
                if (!isControllingProjectors)
                {
                    //foreach (var item in dragItems)
                    //{
                        if (Configuration.DeviceType.Contains("PJLINK"))
                        {
                        try
                        {
                            using (var pjLink = new PJLinkHelper(Configuration.IpAddress))
                            {
                                await pjLink.ConnectAsync();
                                PowerStatus result = await pjLink.GetPowerStatusAsync();
                                Debug.WriteLine($"{Configuration.IpAddress} - {result}");
                                UpdatePowerState(result == PowerStatus.PoweredOn);
                            }
                        }
                        catch (Exception ex)
                        {
                            //Debug.WriteLine($"Error checking status for {Configuration.IpAddress}: {ex.Message}");
                        }
                        //}
                    }
                }
                await Task.Delay(10000); // 5초 대기
            }
        }
        bool isControllingProjectors = false;
        public void StartPJLinkStatusCheck()
        {
            isControllingProjectors = false;
        }

        public void StopPJLinkStatusCheck()
        {
            isControllingProjectors = true;
        }

        bool isControllingAPPOProjectors = false;






        private async Task AppotronicsStatus()
        {
            while (true)
            {
                Debug.WriteLine("aaaaaaaaaaaaa");
                if (!isControllingAPPOProjectors && dlpProjectorHelper != null)
                {
                    try
                    {
                        string status = await dlpProjectorHelper.GetPowerStatusAsync();
                        Debug.WriteLine($"APPO {Configuration.IpAddress} - {status}");
                        bool isPoweredOn = status == "Powered On";
                        UpdatePowerState(isPoweredOn);

                        ////// 여기에 새로운 로직 추가
                        //if (Configuration.IsCurrentState && !isPoweredOn)
                        //{
                        //    Debug.WriteLine($"APPO {Configuration.IpAddress} - 자동 전원 켜기 시도");
                        //    await AutoPowerOnAppotronics();
                        //}
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error checking status for {Configuration.IpAddress}: {ex.Message}");
                        // 연결 문제가 발생했을 때 dlpProjectorHelper를 재생성
                        dlpProjectorHelper.Dispose();
                        dlpProjectorHelper = new DlpProjectorHelper3(Configuration.IpAddress);
                    }
                }
                await Task.Delay(15000); // 10초 대기
            }
        }


        private async Task AutoPowerOnAppotronics()
        {
            //Logger.Log2($"######### APPO {Configuration.IpAddress} - 자동 전원 켜기 시도 ################");
            try
            {
                isControllingAPPOProjectors = true;
                await Task.Delay(commonDelay);
                bool success = await APPOControlProjector(true);
                if (success)
                {
                    //Debug.WriteLine($"APPO {Configuration.IpAddress} - 자동 전원 켜기 성공");
                    //Logger.Log2($"APPO {Configuration.IpAddress} - 자동 전원 켜기 성공");
                }
                else
                {
                    //Debug.WriteLine($"APPO {Configuration.IpAddress} - 자동 전원 켜기 실패");
                    //Logger.Log2($"APPO {Configuration.IpAddress} - 자동 전원 켜기 실패");
                }
            }
            catch (Exception ex)
            {
                //Debug.WriteLine($"APPO {Configuration.IpAddress} - 자동 전원 켜기 중 오류 발생: {ex.Message}");
                //Logger.Log2($"APPO {Configuration.IpAddress} - 자동 전원 켜기 중 오류 발생: {ex.Message}");
            }
            finally
            {
                isControllingAPPOProjectors = false;
            }
        }


        public async Task APPOControlProjector(string ipAddress, bool powerOn)
        {
            using (var projectorHelper = new DlpProjectorHelper(ipAddress))
            {
                try
                {
                    bool result = powerOn
                        ? await projectorHelper.SendPowerOnCommandAsync()
                        : await projectorHelper.SendPowerOffCommandAsync();

                    if (result)
                    {
                        Debug.WriteLine($"프로젝터 {ipAddress} 전원이 {(powerOn ? "켜졌" : "꺼졌")}습니다.");
                    }
                    else
                    {
                        Debug.WriteLine($"프로젝터 {ipAddress} 전원 {(powerOn ? "켜기" : "끄기")}에 실패했습니다.");
                    }

                    string status = await projectorHelper.GetPowerStatusAsync();
                    Debug.WriteLine($"프로젝터 {ipAddress} 상태: {status}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"프로젝터 {ipAddress} 제어 중 오류 발생: {ex.Message}");
                }
            }
        }
        public void StartAPPOStatusCheck()
        {
            isControllingAPPOProjectors = false;
        }

        public void StopAPPOStatusCheck()
        {
            isControllingAPPOProjectors = true;
        }

        private void CreateContextMenu()
        {
            ContextMenu menu = new ContextMenu();
            menu.Style = FindResource("CustomContextMenuStyle") as Style;
            var main = Application.Current.MainWindow as MainWindow;

            if (Configuration.DeviceType.ToLower() == "pc")
            {
                menu.Items.Add(CreateMenuItem("VNC", VNC_Button_Click));
                menu.Items.Add(CreateMenuItem("FTP", FTP_Button_Click));
            }
            menu.Items.Add(CreateMenuItem("EDIT", EditMenuItem_Click));
            menu.Items.Add(CreateMenuItem("DELETE", DeleteMenuItem_Click));

            this.ContextMenu = menu;
        }

        private MenuItem CreateMenuItem(string header, RoutedEventHandler clickHandler)
        {
            MenuItem menuItem = new MenuItem();

            TextBlock headerTextBlock = new TextBlock
            {
                Text = header,
                FontSize = 16,
                FontFamily = (FontFamily)FindResource("NotoSansFontBoldFamily"),
                FontWeight = FontWeights.Bold,
            };

            menuItem.Header = headerTextBlock;

            menuItem.Click += clickHandler;
            menuItem.MouseEnter += MenuItem_MouseEnter;
            menuItem.MouseLeave += MenuItem_MouseLeave;
            menuItem.Style = FindResource("CustomMenuItemStyle") as Style;
            return menuItem;
        }

        private void MenuItem_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is MenuItem button)
            {
                button.Cursor = Cursors.Arrow;
            }
        }

        private void MenuItem_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is MenuItem button)
            {
                button.Cursor = Cursors.Hand;
            }
        }

        public void UpdatePowerState(bool isOn)
        {
            ispow = isOn;
            Configuration.IsOn = isOn;
            PowerState.Fill = isOn ? onColor : offColor;
        }

        public void StartPingCheck()
        {
            try
            {
                if (Configuration.DeviceType.ToLower() == "pc")
                {
                    StopPingCheck();
                    pingCancellationTokenSource = new CancellationTokenSource();
                    _ = PingCheckLoop(pingCancellationTokenSource.Token);
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Ping 체크 시작 중 오류 발생: {e.Message}");
            }
        }

        public void StopPingCheck()
        {
            pingCancellationTokenSource?.Cancel();
            pingCancellationTokenSource?.Dispose();
            pingCancellationTokenSource = null;
        }

        private async Task PingCheckLoop(CancellationToken cancellationToken)
        {
            
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!isDoing)
                {
                    bool isReachable = await PingHostAsync(Configuration.IpAddress);
                    await Dispatcher.InvokeAsync(() =>
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            UpdatePowerState(isReachable);
                        }
                    });
                    await Task.Delay(PingInterval, cancellationToken);
                }
            }
        }

        private async Task<bool> PingHostAsync(string ipAddress)
        {

            try
            {
                using (var ping = new Ping())
                {
                    var reply = await ping.SendPingAsync(ipAddress, 2000);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }

        private void UpdateUI()
        {
            if (Configuration.Name.Length > 22)
            {
                TitleTextBlock.Text = Configuration.Name.Substring(0, 20) + "..";
            }
            else
            {
                TitleTextBlock.Text = Configuration.Name;
            }
            DeviceTextBlock.Text = Configuration.DeviceType.ToLower();
            ipTextBlock.Text = Configuration.IpAddress;
            chanelTextBlock.Text = Configuration.Channel;
            if (Configuration.DeviceType.ToLower() == "프로젝터(pjlink)")
            {
                
                IconImage.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/projector.png"));
            }
            else if (Configuration.DeviceType.ToLower() == "프로젝터(appotronics)")
            {
                IconImage.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/projector.png"));
            }
            else if (Configuration.DeviceType == "PDU")
            {
                IconImage.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/PDU.png"));
                IconImage.Width = 67;
                IconImage.Height = 46;
            }
            else if (Configuration.DeviceType == "RELAY")
            {
                IconImage.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/RELAY.png"));
                IconImage.Width = 95;
                IconImage.Height = 46;
            }
            else
            {
                IconImage.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/pc.png"));
            }
        }

        private void StatusIndicator_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Configuration.DeviceType.ToLower() == "pc")
            {
                OverlayGrid.Visibility = OverlayGrid.Visibility == Visibility.Visible ?
                                     Visibility.Collapsed : Visibility.Visible;
            }
        }

        private void FTP_Button_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                if (Configuration.IsOn == true)
                {
                    mainWindow.ShowFileExplorer(Configuration.IpAddress, Configuration.Name);
                }
                else
                {
                    MessageBox.Show("전원이 꺼져있습니다.");
                    //mainWindow.ShowFileExplorer(Configuration.IpAddress, Configuration.Name);
                }
            }
            OverlayGrid.Visibility = Visibility.Collapsed;
        }

        private void VNC_Button_Click(object sender, RoutedEventArgs e)
        {
            var main = Application.Current.MainWindow as MainWindow;

            if (main.isvnc)
            {
                if (Configuration.IsOn)
                {
                    try
                    {
                        Process.Start(vncViewerPath, $"{Configuration.IpAddress} -password={Configuration.VncPw}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"연결 오류: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    Debug.WriteLine(Configuration.IpAddress);
                }
                else
                {
                    MessageBox.Show("전원이 꺼져있습니다.");
                }
            }
            else
            {
                MessageBox.Show("VNC 기능이 비활성화되어 있습니다.");
            }

            OverlayGrid.Visibility = Visibility.Collapsed;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OverlayGrid.Visibility = Visibility.Collapsed;
        }

        private void EditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow.add_device_ppanel.Visibility = Visibility.Visible;
            mainWindow.addDeviceWindow.set_edit_value(Configuration);
        }


        private void d_select_unchecked(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;

            mainWindow.temp_total_stop = true;

            mainWindow.delete_total_toggle.IsChecked = false;

            mainWindow.temp_total_stop = false;
        }




        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            delete_dialog dialog = new delete_dialog
            {
                Owner = Application.Current.MainWindow,
            };

            dialog.popup_msg.Text = "해당 장비를 삭제하시겠습니까?\n삭제 후 되돌릴 수 없습니다.";

            bool? result = dialog.ShowDialog();

            if (dialog.DialogResult == true)
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                mainWindow?.RemoveDevice(this);
            }
        }

        private void SaveConfiguration()
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.SaveItemConfigurations();
        }



        private async Task device_on_progress(MainWindow main, string device_type)
        {
            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    main.PowerStatusText.Text = "전원 ON";
                    main.PowerOverlay.Visibility = Visibility.Visible;
                    main.PowerProgressBar.Value = 0;
                });

                double duration = 10000;

                if (device_type == "프로젝터(pjlink)")
                {
                    duration = main.Progress_duration_projector;
                }
                else if (device_type == "프로젝터(appotronics)")
                {
                    duration = main.Progress_duration_projector;
                }
                else
                {
                    duration = 1000;
                }
               


                // 프로그레스바 업데이트를 별도의 Task로 실행
                _ = Task.Run(async () =>
                {
                    await UpdateProgressBarAsyncs(main, 0, 100, duration);



                    // 프로그레스바 작업이 완료되면 오버레이 숨기기
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        main.PowerOverlay.Visibility = Visibility.Collapsed;
                    });
                });
            }
            catch (Exception ex)
            {
               
            }
        }


        private async Task device_off_progress(MainWindow main, string device_type)
        {
            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    main.PowerStatusText.Text = "전원 OFF";
                    main.PowerOverlay.Visibility = Visibility.Visible;
                    main.PowerProgressBar.Value = 0;
                });
                double duration = 10000;

                if (device_type == "프로젝터(pjlink)")
                {
                    duration = main.Progress_duration_projector;
                }
                else if (device_type == "프로젝터(appotronics)")
                {
                    duration = main.Progress_duration_projector;
                }
                else
                {
                    duration = 1000;
                }



                // 프로그레스바 업데이트를 별도의 Task로 실행
                _ = Task.Run(async () =>
                {
                    await UpdateProgressBarAsyncs(main, 0, 100, duration);

                    // 프로그레스바 작업이 완료되면 오버레이 숨기기
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        main.PowerOverlay.Visibility = Visibility.Collapsed;
                    });
                });
            }
            catch (Exception ex)
            {
                // 예외 처리
            }
        }



        private async Task UpdateProgressBarAsyncs(MainWindow main, double startValue, double endValue, double durationSeconds)
        {
           
            double currentValue = startValue;
            int totalSteps = 100;  // 총 스텝 수
            double increment = (endValue - startValue) / totalSteps;  // 각 스텝당 증가값
            double stepDelay = durationSeconds / totalSteps;  // 각 스텝 사이의 대기 시간

            while (currentValue <= endValue)
            {
                bool isCompleted = false;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    main.PowerProgressBar.Value = Math.Min(currentValue, 100);

                    if (currentValue >= 100)
                    {
                        main.PowerOverlay.Visibility = Visibility.Collapsed;
                        isCompleted = true;
                        if (main.PowerStatusText.Text == "전원 OFF")
                        {
                            MessageBox.Show("전원이 꺼졌습니다");
                        }
                        else
                        {
                            MessageBox.Show("전원이 켜졌습니다");
                        }
                        
                    }
                });

                if (isCompleted)
                    break;

                currentValue += increment;
                await Task.Delay((int)stepDelay);  // duration 시간에 맞춰 대기
            }
        }
    


        private async void pow_on(object sender, RoutedEventArgs e)
        {

            var main = Application.Current.MainWindow as MainWindow;


            var progressTask = Task.Run(async () =>
            {
                await device_on_progress(main, Configuration.DeviceType.ToLower());
            });



            isDoing = true;
            Configuration.IsCurrentState = true;
            SaveConfiguration();
            string result = "Success";
            try
            {
                if (Configuration.DeviceType.ToLower() == "pc")
                {
                    for(int i=0;i<5;i++)
                    wol.TurnOnPC(Configuration.MacAddress);
                }
                else if (Configuration.DeviceType.ToLower() == "프로젝터(pjlink)")
                {

                    isControllingProjectors = true;
                    await Task.Delay(commonDelay);
                    using (var pjLink = new PJLinkHelper(Configuration.IpAddress))
                    {
                        await pjLink.ConnectAsync();
                        bool r = await pjLink.PowerOnAsync();
                        Debug.WriteLine(r
                            ? $"{Configuration.IpAddress} 프로젝터 전원이 {( "켜졌" )}습니다."
                            : $"{Configuration.IpAddress} 프로젝터 전원을 {"켜는" }데 실패했습니다.");
                    }
                    //await APPOControlProjector(Configuration.IpAddress, true);
                    isControllingProjectors = false;
                    await Task.Delay(commonDelay);
                }
                else if (Configuration.DeviceType.ToLower() == "프로젝터(appotronics)")
                {

                    isControllingAPPOProjectors = true;
                    await Task.Delay(commonDelay);
                    bool success = await APPOControlProjector(true);
                    result = success ? "Success" : "Success";
                    isControllingAPPOProjectors = false;
                    await Task.Delay(commonDelay);
                    UpdatePowerState(true);
                }

                else if (Configuration.DeviceType == "PDU")
                {
                    await WebApiHelper.Instance.OnPDU(Configuration.IpAddress, Configuration.Channel);
                }
                else if (Configuration.DeviceType == "RELAY")
                {
                    string hexStr = Utils.Instance.IntToHex(Configuration.Channel);
                    Debug.WriteLine(hexStr);
                    string hex = $"525920{hexStr}20310D";
                    Logger.Log(Configuration.IpAddress, Configuration.port, "Power ON", hex);
                    await UdpHelper.Instance.SendHexAsync(hex, false, int.Parse(Configuration.port), Configuration.IpAddress);
                }
                isDoing = false;
             
            }
            catch (Exception ex)
            {
                result = $"Error: {ex.Message}";
                Logger.LogError($"Error: {ex.Message}");
            }

            Logger.Log(Configuration.Name, Configuration.DeviceType, "Power On", result);
            //MessageBox.Show("전원이 켜졌습니다");
        }

        private async Task ControlProjector(string ipAddress, bool powerOn)
        {
            isControllingProjectors = true;
            try
            {
                //Debug.WriteLine($"{ipAddress} {(powerOn ? "켜기" : "끄기")}시작");
                using (var pjLink = new PJLinkHelper(ipAddress))
                {
                    await pjLink.ConnectAsync();
                    bool result = powerOn ? await pjLink.PowerOnAsync() : await pjLink.PowerOffAsync();
                }
            }
            catch (Exception ex)
            {
                //Debug.WriteLine($"Error controlling projector {ipAddress}: {ex.Message}");
            }
            finally
            {
                isControllingProjectors = false;
            }
            await Task.Delay(200); // 각 프로젝터 제어 후 2초 대기
        }

        public async
        Task
pow_on_pjlink()
        {
            isControllingProjectors = true;
            await Task.Delay(commonDelay);
            await ControlProjector(Configuration.IpAddress, true);
            isControllingProjectors = false;
            await Task.Delay(commonDelay);
        }

        public async 
        Task
pow_off_pjlink()
        {
            isControllingProjectors = true;
            await Task.Delay(commonDelay);
            await ControlProjector(Configuration.IpAddress, false);
            isControllingProjectors = false;
            await Task.Delay(commonDelay);
        }



        public async
        Task
pow_on_appo()
        {
            isControllingAPPOProjectors = true;
            await Task.Delay(commonDelay);
            bool success = await APPOControlProjector(true);
            isControllingAPPOProjectors = false;
            await Task.Delay(commonDelay);
        }

        public async
        Task
pow_off_appo()
        {
            isControllingAPPOProjectors = true;
            await Task.Delay(commonDelay);
            bool success = await APPOControlProjector(false);
            isControllingAPPOProjectors = false;
            await Task.Delay(commonDelay);
        }


      

        private async void pow_off(object sender, RoutedEventArgs e)
        {
            Configuration.IsCurrentState = false;
            SaveConfiguration();

            var main = Application.Current.MainWindow as MainWindow;


            var progressTask = Task.Run(async () =>
            {
                await device_off_progress(main, Configuration.DeviceType.ToLower());
            });

            string result = "Success";
            try
            {
                if (Configuration.DeviceType.ToLower() == "pc")
                {
                    for (int i = 0; i < 5; i++)
                        await UdpHelper.Instance.SendWithIpAsync("power|0", Configuration.IpAddress, 8889);
                }
                else if (Configuration.DeviceType.ToLower() == "프로젝터(pjlink)")
                {

                    isControllingProjectors = true;
                    await Task.Delay(commonDelay);
                    await ControlProjector(Configuration.IpAddress, false);
                    isControllingProjectors = false;
                    await Task.Delay(commonDelay);
                }
                else if (Configuration.DeviceType.ToLower() == "프로젝터(appotronics)")
                {
                    isControllingAPPOProjectors = true;
                    await Task.Delay(commonDelay);
                    bool success = await APPOControlProjector(false);
                    result = success ? "Success" : "Failed";
                    isControllingAPPOProjectors = false;
                    await Task.Delay(commonDelay);
                    UpdatePowerState(false);
                }
                else if (Configuration.DeviceType == "PDU")
                {
                    await WebApiHelper.Instance.OffPDU(Configuration.IpAddress, Configuration.Channel);
                }
                else if (Configuration.DeviceType == "RELAY")
                {
                    string hexStr = Utils.Instance.IntToHex(Configuration.Channel);
                    Debug.WriteLine(hexStr);
                    string hex = $"525920{hexStr}20300D";
                    Logger.Log(Configuration.IpAddress, Configuration.port, "Power OFF", hex);
                    await UdpHelper.Instance.SendHexAsync(hex, false, int.Parse(Configuration.port), Configuration.IpAddress);
                }
            }
            catch (Exception ex)
            {
                result = $"Error: {ex.Message}";
                Logger.LogError($"Error: {ex.Message}");
            }

            Logger.Log(Configuration.Name, Configuration.DeviceType, "Power Off", result);
            //MessageBox.Show("전원이 꺼졌습니다.");
        }


        public async Task<bool> APPOControlProjector(bool powerOn)
        {
            if (dlpProjectorHelper == null)
            {
                Debug.WriteLine($"DlpProjectorHelper is not initialized for {Configuration.IpAddress}");
                return false;
            }

            try
            {
                bool result = powerOn
                    ? await dlpProjectorHelper.SendPowerOnCommandAsync()
                    : await dlpProjectorHelper.SendPowerOffCommandAsync();

                Debug.WriteLine($"프로젝터 {Configuration.IpAddress} 전원 {(powerOn ? "켜기" : "끄기")} 결과: {result}");

                //string status = await dlpProjectorHelper.GetPowerStatusAsync();
                //Debug.WriteLine($"프로젝터 {Configuration.IpAddress} 상태: {status}");

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"프로젝터 {Configuration.IpAddress} 제어 중 오류 발생: {ex.Message}");
                return false;
            }
        }

        private void OnMessageReceived(string message)
        {
            Dispatcher.Invoke(() =>
            {
                if (message == "00")
                {
                    if (relay_onoff < 5)
                    {
                        relay_onoff++;
                    }
                    else
                    {
                        UpdatePowerState(false);
                    }
                }
                else
                {
                    UpdatePowerState(true);
                    relay_onoff = 0;
                }
            });
        }
    }
}