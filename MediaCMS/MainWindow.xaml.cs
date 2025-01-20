using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Threading.Tasks;
using MediaCMS.UserControls;
using TcpHelperNamespace;
using System.Diagnostics;
using Launcher_SE.Helpers;
using MediaCMS;
using static System.Net.Mime.MediaTypeNames;
using OSC_Test.Helpers;
using System.Text.RegularExpressions;
using WpfApp11.Helpers;
using WpfApp11.UserControls;

namespace MediaCMS
{
    public partial class MainWindow : Window
    {
        public LogViewerWindow logViewer;
        public LogViewerWindow errorlogViewer;
        private List<DraggableItemControl> dragItems = new List<DraggableItemControl>();
        private const string SettingsFile = "settings.json";
        private Point offset;
        private DraggableItemControl draggedItem;
        private const string ConfigFile = "itemConfig.json";
        private const int GridCellHeight = 200;
        private const int GridCellWidth = 200;
        private int GridRows = 5;
        private int GridColumns = 5;
        //private const int GridRows = 4;
        //private const int GridColumns = 8;
        private bool[,] occupiedCells;
        private int highestZIndex = 1;
        private DateTime lastClickTime = DateTime.MinValue;
        private const double DoubleClickTime = 300; // 밀리초
        private double powerProgress = 0;

        private double Progress_duration = 5000;

        public double Progress_duration_projector = 5000;
        public bool isvnc = false;
        public bool isftp = false;

        private int clickCount = 0;
        private DispatcherTimer clickTimer;
        public WakeOnLan wol;


        DispatcherTimer pow_timer = new DispatcherTimer();

        public string local_pc_name = "pc";
        public string vnc_pw = "0909";
        public string local_path = @"C:\MEDIA";

        public int pingtime = 30000;
        public int powerInterva01;
        public int powerInterva02;

        public string web_name = "administrator";
        public string web_pw = "password";
        bool first_init = false;

        private string authPath = @"C:\Users\Default\AppData";
        private string authCode = @"b500b9a2bb02";

        //ProtocolHelper pl;
        KIA360ProtocolHelper pl;


        private bool checkAuth()
        {
            DirectoryInfo di = new DirectoryInfo(authPath);
            var item = di.GetDirectories();
            var tf = false;

            foreach (var i in item)
            {
                if (i.Name.Contains(authCode))
                {
                    tf = true;
                    break;
                }
            }
            return tf;
        }

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;

            //if (!checkAuth())
            //{
            //    MessageBox.Show("not allowed");
            //    this.Close();
            //    return;
            //}

            //늘 주석
            //pl = new ProtocolHelper();
            //pl = new KIA360ProtocolHelper();
            //pl.Start();

            clickTimer = new DispatcherTimer();
            clickTimer.Interval = TimeSpan.FromMilliseconds(200);
            clickTimer.Tick += ClickTimer_Tick;
            AutoPowerSettingsControl.init();
            wol = new WakeOnLan();
            if (!File.Exists(SettingsFile))
            {
                SaveSettings();
            }

            LoadSettings();
            occupiedCells = new bool[GridRows, GridColumns];
            CreateGrid();

            LoadItemConfigurations();
            InitializeAutoPowerSettings();
            FileExplorerControl.CloseRequested += FileExplorerControl_CloseRequested;

            pow_timer.Interval = TimeSpan.FromSeconds(10);
            pow_timer.Tick += Pow_timer_Tick;

            first_init = true;

            this.KeyDown += MainWindow_KeyDown;
        }

        private void tt(object sender, EventArgs e)
        {


            var canvas = sender as Canvas;
            if (canvas != null)
            {
                // Get the ContextMenu associated with the Canvas
                var contextMenu = canvas.ContextMenu;

                // Show the ContextMenu at the position of the mouse click
                if (contextMenu != null)
                {
                    contextMenu.IsOpen = true;
                }
            }
        }




        // 윈도우 중앙에 뜨게
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var workingArea = SystemParameters.WorkArea;
            this.Left = (workingArea.Width - this.Width) / 2 + workingArea.Left;
            this.Top = (workingArea.Height - this.Height) / 2 + workingArea.Top;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                addDeviceWindow.cancle_popup();
                AutoPowerSettingsControl.cancle_ev();

                editpanel.Visibility = Visibility.Collapsed;
                for (int i = 0; i < dragItems.Count; i++)
                {
                    dragItems[i].delete_select.Visibility = Visibility.Collapsed;
                }
            }

            else if (e.Key == Key.F1)
            {
                if (logViewer == null || !logViewer.IsVisible)
                {
                    logViewer = new LogViewerWindow(Logger.LogFilePath);
                    logViewer.Title = "Logviewer";
                    logViewer.Show();
                }
                else
                {
                    logViewer.Close();
                    logViewer = new LogViewerWindow(Logger.LogFilePath);
                    logViewer.Title = "Logviewer";
                    logViewer.Show();
                }
            }
            else if (e.Key == Key.F2)
            {
                if (logViewer == null || !logViewer.IsVisible)
                {
                    logViewer = new LogViewerWindow(Logger.LogPowerFilePath);
                    logViewer.Title = "PowerLogviewer";
                    logViewer.Show();
                }
                else
                {
                    logViewer.Close();

                    logViewer = new LogViewerWindow(Logger.LogPowerFilePath);
                    logViewer.Title = "PowerLogviewer";
                    logViewer.Show();
                }
            }
            else if (e.Key == Key.F3)
            {
                if (logViewer == null || !logViewer.IsVisible)
                {
                    logViewer = new LogViewerWindow(Logger.LogErrorFilePath);
                    logViewer.Title = "ErrorLogviewer";
                    logViewer.Show();
                }
                else
                {
                    logViewer.Close();

                    logViewer = new LogViewerWindow(Logger.LogErrorFilePath);
                    logViewer.Title = "ErrorLogviewer";
                    logViewer.Show();
                }
            }
        }

        private void LoadSettings()
        {
            if (File.Exists(SettingsFile))
            {
                string json = File.ReadAllText(SettingsFile);
                var settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (settings.TryGetValue("AutoPowerEnabled", out var autoPowerValue) && autoPowerValue is bool isEnabled)
                {
                    AutoPowerToggle.IsChecked = isEnabled;
                }
                if (settings.TryGetValue("CMSTitle", out var nameValue) && nameValue is string name)
                {
                    local_pc_name = name;
                    p_title.Text = local_pc_name;
                    Title = local_pc_name;
                }
                if (settings.TryGetValue("ContentsPath", out var local_pathValue) && local_pathValue is string local_path_value)
                {
                    local_path = local_path_value;
                }
                if (settings.TryGetValue("ProgressDuration", out var progressDurationValue) && progressDurationValue is double progressDuration)
                {
                    Progress_duration = progressDuration;
                }
                if (settings.TryGetValue("UseVNC", out var vnc_value) && vnc_value is bool vncvalue)
                {
                    isvnc = vncvalue;
                }
                if (settings.TryGetValue("UseFTP", out var ftp_value) && ftp_value is bool ftpvalue)
                {
                    isftp = ftpvalue;
                }
                if (settings.TryGetValue("VNCPassword", out var vncpw) && vncpw is string vncpw2)
                {
                    vnc_pw = vncpw2;
                }
                if (settings.TryGetValue("StatusCheckInterval", out var ping_timer))
                {
                    pingtime = int.Parse(ping_timer.ToString());
                }
                if (settings.TryGetValue("PowerInterval01", out var _powerInterval01))
                {
                    powerInterva01 = int.Parse(_powerInterval01.ToString());
                }
                if (settings.TryGetValue("PowerInterval02", out var _powerInterval02))
                {
                    powerInterva02 = int.Parse(_powerInterval02.ToString());
                }

                if (settings.TryGetValue("WebName", out var webname) && webname is string webnames)
                {
                    web_name = webnames;
                }

                if (settings.TryGetValue("WebPassword", out var webpw) && webpw is string webpws)
                {
                    web_pw = webpws;
                }

                if (settings.TryGetValue("GridRows", out var GridRows1) && GridRows1 is string GridRows12)
                {
                    GridRows = int.Parse(GridRows12);
                }


                if (settings.TryGetValue("GridColumns", out var GridColumns1) && GridColumns1 is string GridColumns12)
                {
                    GridColumns = int.Parse(GridColumns12);
                }

                if (settings.TryGetValue("ProjectorProgressDuration", out var progressDurationValue_p) && progressDurationValue_p is double progressDuration_p)
                {
                    Progress_duration_projector = progressDuration_p;
                }
            }
        }

        private void SaveSettings()
        {
            var settings = new Dictionary<string, object>
            {
                { "CMSTitle", local_pc_name },
                { "ContentsPath", local_path },
                { "AutoPowerEnabled", AutoPowerToggle.IsChecked ?? false },
                { "ProgressDuration", Progress_duration },
                { "UseVNC", isvnc },
                { "UseFTP", isftp},
                { "VNCPassword", vnc_pw},
                { "StatusCheckInterval", pingtime},
                { "PowerInterval01", powerInterva01},
                { "PowerInterval02", powerInterva02},
                {"WebName", web_name },
                {"WebPassword", web_pw },
                {"GridRows", GridRows.ToString() },
                {"GridColumns", GridColumns.ToString() },
                 {"ProjectorProgressDuration", Progress_duration_projector },
            };

            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(SettingsFile, json);
        }

        private void AutoPowerToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (first_init)
            {
                SaveSettings();
            }
            pow_timer.Start();
        }

        private void AutoPowerToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (first_init)
            {
                SaveSettings();
            }
            pow_timer.Stop();
        }

        bool go_pow = false;

        int pow_cnt = 0;
        private async void Pow_timer_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            string[] days = { "월요일", "화요일", "수요일", "목요일", "금요일", "토요일", "일요일" };
            string currentDay;

            if ((int)now.DayOfWeek == 0)
            {
                currentDay = days[6];
            }
            else
            {
                currentDay = days[(int)now.DayOfWeek - 1];
            }


            if (AutoPowerSettingsControl.pow_schedule[currentDay].IsEnabled)
            {
                if (go_pow == false)
                {

                    string currentTime = now.ToString("HH:mm");
                    if (currentTime == AutoPowerSettingsControl.pow_schedule[currentDay].StartTime.ToString().Substring(0, 5))
                    {
                        go_pow = true;
                        Logger.LogPower($"자동 전원 관리 전원 ON - {currentDay}");
                        await OnDevice();
                    }
                    else if (currentTime == AutoPowerSettingsControl.pow_schedule[currentDay].EndTime.ToString().Substring(0, 5))
                    {
                        go_pow = true;
                        Logger.LogPower($"자동 전원 관리 전원 OFF - {currentDay}");
                        await OffDevice();
                    }



                    if (currentTime == AutoPowerSettingsControl.pow_schedule[currentDay].StartTime.Add(TimeSpan.FromMinutes(10)).ToString().Substring(0, 5))
                    {

                        //if 꺼져있다면
                        int half_count = 0;

                        foreach (var item in dragItems)
                        {
                            if (item.Configuration.IsOn == false)
                            {
                                half_count++;
                            }
                        }

                        int majority = dragItems.Count / 2;

                        if (half_count > majority)
                        {

                            go_pow = true;
                            Logger.LogPower($"자동 전원 관리 전원 ON 다시 실행- {currentDay}");
                            await OnDevice();
                        }


                        //if (dragItems.Count != 0)
                        //{
                        //    if (dragItems[0].Configuration.IsOn == false)
                        //    {

                        //        go_pow = true;
                        //        Logger.LogPower($"자동 전원 관리 전원 ON 다시 실행- {currentDay}");
                        //        await OnDevice();
                        //    }
                        //}



                    }
                    else if (currentTime == AutoPowerSettingsControl.pow_schedule[currentDay].EndTime.Add(TimeSpan.FromMinutes(10)).ToString().Substring(0, 5))
                    {

                        int half_count = 0;

                        foreach (var item in dragItems)
                        {
                            if (item.Configuration.IsOn == true)
                            {
                                half_count++;
                            }
                        }

                        int majority = dragItems.Count / 2;

                        if (half_count > majority)
                        {
                            go_pow = true;
                            Logger.LogPower($"자동 전원 관리 전원 OFF 다시 실행- {currentDay}");
                            await OffDevice();
                        }


                        //if 켜져있다면

                        //if (dragItems.Count != 0)
                        //{
                        //    if (dragItems[0].Configuration.IsOn == true)
                        //    {

                        //        go_pow = true;
                        //        Logger.LogPower($"자동 전원 관리 전원 OFF 다시 실행- {currentDay}");
                        //        await OffDevice();
                        //    }
                        //}



                    }

                }
                else
                {
                    pow_cnt++;
                    Console.WriteLine("pow_cnt : " + pow_cnt);
                    if (pow_cnt > 6)
                    {
                        pow_cnt = 0;
                        go_pow = false;
                    }

                }
            }
        }

        private void UpdateAllDevicesCurrentState(bool state)
        {
            //============== 추가==============
            if (state == false)
            {
                foreach (var item in dragItems)
                {
                    item.Configuration.IsCurrentState = state;
                }
            }
            else
            {
                foreach (var item in dragItems)
                {
                    if (item.Configuration.IsPower == true)
                        item.Configuration.IsCurrentState = state;
                }
            }

            SaveItemConfigurations();
        }

        public async Task OnDevice()
        {
            await ControlAllDevices(true);
        }

        public async Task OffDevice()
        {
            await ControlAllDevices(false);
        }

        public async Task SortAndProcessDragItems(List<ItemConfiguration> drags, bool onOff, double startProgress, double endProgress)
        {
            Debug.WriteLine($"Starting SortAndProcessDragItems. OnOff: {onOff}, StartProgress: {startProgress}, EndProgress: {endProgress}");

            var deviceTypes = new[] { "프로젝터(pjlink)", "프로젝터(appotronics)", "relay", "pdu", "pc" };
            var dummyItems = deviceTypes.Select(type => new ItemConfiguration { DeviceType = type, IsDummy = true, IsPower = true }).ToList();

            Debug.WriteLine($"Created {dummyItems.Count} dummy items");

            var allItems = drags.Concat(dummyItems).ToList();
            Debug.WriteLine($"Total items (including dummies): {allItems.Count}");

            var sortedDragItems = onOff
                ? allItems.OrderBy(a => a.DeviceType.ToLower() == "프로젝터(pjlink)" ? 1 :
                                        a.DeviceType.ToLower() == "프로젝터(appotronics)" ? 2 :
                                        a.DeviceType.ToLower() == "relay" ? 3 :
                                        a.DeviceType.ToLower() == "pdu" ? 4 :
                                        a.DeviceType.ToLower() == "pc" ? 5 : 6)
                       .ToList()
                : allItems.OrderBy(a => a.DeviceType.ToLower() == "pc" ? 1 :
                                        a.DeviceType.ToLower() == "pdu" ? 2 :
                                        a.DeviceType.ToLower() == "relay" ? 3 :
                                        a.DeviceType.ToLower() == "프로젝터(appotronics)" ? 4 :
                                        a.DeviceType.ToLower() == "프로젝터(pjlink)" ? 5 : 6)
                       .ToList();

            Debug.WriteLine($"Items sorted. Order: {string.Join(", ", sortedDragItems.Select(i => i.DeviceType))}");



            //sortedDragItems = sortedDragItems.FindAll(a => a.IsPower == true).ToList();


            if (onOff == false)
            {
                sortedDragItems = sortedDragItems.FindAll(a => a.IsPower == true || a.IsPower == false).ToList();
            }
            else
            {
                sortedDragItems = sortedDragItems.FindAll(a => a.IsPower == true).ToList();
            }


            int totalItems = sortedDragItems.Count;
            Debug.WriteLine($"Filtered power-controllable items. Total: {totalItems}");

            string previousDeviceType = "";

            for (int i = 0; i < totalItems; i++)
            {
                var item = sortedDragItems[i];
                Debug.WriteLine($"Processing item {i + 1}/{totalItems}: {item.DeviceType} (IsDummy: {item.IsDummy})");

                if (onOff)
                {
                    if (previousDeviceType.ToLower() == "프로젝터(appotronics)" && item.DeviceType.ToLower() == "relay")
                    {
                        Debug.WriteLine($"Adding delay of {powerInterva01}ms between 프로젝터(appotronics) and relay");
                        await Task.Delay(powerInterva01);
                    }
                    else if (previousDeviceType.ToLower() == "pdu" && item.DeviceType.ToLower() == "pc")
                    {
                        Debug.WriteLine($"Adding delay of {powerInterva02}ms between pdu and pc");
                        await Task.Delay(powerInterva02);
                    }
                }
                else
                {
                    if (previousDeviceType.ToLower() == "pc" && item.DeviceType.ToLower() == "pdu")
                    {
                        Debug.WriteLine($"Adding delay of {powerInterva01}ms between pc and pdu");
                        await Task.Delay(powerInterva01);
                    }
                    else if (previousDeviceType.ToLower() == "relay" && item.DeviceType.ToLower() == "프로젝터(appotronics)")
                    {
                        Debug.WriteLine($"Adding delay of {powerInterva02}ms between relay and 프로젝터(appotronics)");
                        await Task.Delay(powerInterva02);
                    }
                }

                if (!item.IsDummy)
                {
                    Debug.WriteLine($"Processing real item: {item.DeviceType}");
                    switch (item.DeviceType.ToLower())
                    {
                        case "프로젝터(pjlink)":
                            await ProcessProjector(item, onOff);
                            await Task.Delay(500);
                            break;
                        case "프로젝터(appotronics)":
                            await ProcessDLPProjector(item, onOff);
                            await Task.Delay(500);
                            break;
                        case "pc":
                            ProcessPC(item, onOff);
                            break;
                        case "relay":
                            ProcessRelay1(item, onOff);
                            break;
                        case "pdu":
                            ProcessPDU(item, onOff);
                            break;
                    }
                }
                else
                {
                    Debug.WriteLine($"Skipping dummy item: {item.DeviceType}");
                }

                await Task.Delay(200);
                previousDeviceType = item.DeviceType;
            }

            foreach (var i in dragItems)
            {
                i.StartPingCheck();
            }
            Debug.WriteLine("Started ping checks for all items");

            Debug.WriteLine("SortAndProcessDragItems completed");
        }


        private async Task AddDelay(double startProgress, double endProgress)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            double delayDuration = 10000; // 10초

            while (watch.ElapsedMilliseconds < delayDuration)
            {
                double progress = startProgress + (endProgress - startProgress) * (watch.ElapsedMilliseconds / delayDuration);
                Dispatcher.Invoke(() => PowerProgressBar.Value = progress);
                await Task.Delay(100);
            }

            watch.Stop();
        }

        private Task FinalizeDeviceOperation(bool isOn)
        {
            PowerOverlay.Visibility = Visibility.Collapsed;

            //foreach (var item in dragItems)
            //{
            //    item.Configuration.IsOn = isOn;
            //    UpdateItemPowerState(item, isOn);
            //}
            //SaveItemConfigurations();
            return Task.CompletedTask;
        }



        private async void TotalPowerBtnOn_Click(object sender, RoutedEventArgs e)
        {
            Logger.LogPower("전체 전원 ON");
            PowerStatusText.Text = "전원 ON";
            await ControlAllDevices(true);
        }

        private async void TotalPowerBtnOff_Click(object sender, RoutedEventArgs e)
        {
            Logger.LogPower("전체 전원 OFF");
            PowerStatusText.Text = "전원 OFF";
            await ControlAllDevices(false);
        }

        private async Task ControlAllDevices(bool turnOn)
        {
            UpdateAllDevicesCurrentState(turnOn);
            PowerOverlay.Visibility = Visibility.Visible;
            PowerProgressBar.Value = 0;

            var progressTask = UpdateProgressBarAsync(0, 100, Progress_duration);
            var deviceControlTask = ProcessDevicesAsync(turnOn);

            await Task.WhenAll(progressTask, deviceControlTask);



            PowerOverlay.Visibility = Visibility.Collapsed;
        }

        private async Task UpdateProgressBarAsync(double startValue, double endValue, double durationSeconds)
        {
            DateTime startTime = DateTime.Now;
            while ((DateTime.Now - startTime).TotalMilliseconds < durationSeconds)
            {
                double progress = (DateTime.Now - startTime).TotalMilliseconds / durationSeconds;
                double currentValue = startValue + (endValue - startValue) * progress;
                PowerProgressBar.Value = Math.Min(currentValue, 100);
                await Task.Delay(50); // 50ms마다 업데이트
            }
            PowerProgressBar.Value = endValue;
        }

        private async Task ProcessDevicesAsync(bool turnOn)
        {
            var items = dragItems.Select(a => a.Configuration).ToList();
            await SortAndProcessDragItems(items, turnOn, 0, 100);
        }

        private async Task ProcessProjector(ItemConfiguration item, bool onOff)
        {
            try
            {
                foreach (var ditem in dragItems)
                {
                    if (ditem.Configuration.IpAddress == item.IpAddress)
                    {
                        if (onOff)
                        {
                            await ditem.pow_on_pjlink();
                        }
                        else
                        {
                            await ditem.pow_off_pjlink();
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing projector: {ex.Message}");
                Logger.LogError($"Error processing projector: {ex.Message}");
            }
        }

        private async Task ProcessDLPProjector(ItemConfiguration item, bool onOff)
        {
            try
            {
                foreach (var ditem in dragItems)
                {
                    if (ditem.Configuration.IpAddress == item.IpAddress)
                    {
                        if (onOff)
                        {
                            await ditem.pow_on_appo();
                        }
                        else
                        {
                            await ditem.pow_off_appo();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing projector: {ex.Message}");
                Logger.LogError($"Error processing projector: {ex.Message}");
            }
        }

        private void ProcessPC(ItemConfiguration item, bool onOff)
        {
            try
            {
                if (onOff)
                {
                    for (int i = 0; i < 5; i++)
                        wol.TurnOnPC(item.MacAddress);
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                        UdpHelper.Instance.SendWithIpAsync("power|0", item.IpAddress, 8889);
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Error processing PC: {e.Message}");
            }
            Task.Delay(200);
        }

        private void ProcessRelay1(ItemConfiguration item, bool onOff)
        {
            if (onOff)
            {
                OnRelay(item);
            }
            else
            {
                OffRelay(item);
            }
        }


        private async void OnRelay(ItemConfiguration item)
        {
            try
            {
                string hexStr = Utils.Instance.IntToHex(item.Channel);
                Debug.WriteLine(hexStr);
                string hex = $"525920{hexStr}20310D";
                Logger.Log(item.IpAddress, item.port, "Power ON", hex);
                await UdpHelper.Instance.SendHexAsync(hex, false, int.Parse(item.port), item.IpAddress);
            }
            catch (Exception e)
            {
                Logger.LogError($"Error : {e.Message}");
            }
        }

        private async void OffRelay(ItemConfiguration item)
        {
            try
            {
                string hexStr = Utils.Instance.IntToHex(item.Channel);
                Debug.WriteLine(hexStr);


                string hex = $"525920{hexStr}20300D";
                Logger.Log(item.IpAddress, item.port, "Power OFF", hex);
                await UdpHelper.Instance.SendHexAsync(hex, false, int.Parse(item.port), item.IpAddress);
            }
            catch (Exception e)
            {
                Logger.LogError($"Error : {e.Message}");
            }
        }


        private void ProcessPDU(ItemConfiguration item, bool onOff)
        {
            try
            {
                if (onOff)
                {
                    _ = WebApiHelper.Instance.OnPDU(item.IpAddress, item.Channel);
                }
                else
                {
                    _ = WebApiHelper.Instance.OffPDU(item.IpAddress, item.Channel);
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Error : {e.Message}");
            }
        }

       
        private void AnimateMessage(bool show)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                To = show ? 30 : 0,
                Duration = TimeSpan.FromSeconds(0.3)
            };

            animation.Completed += (s, e) =>
            {
                if (show)
                {
                    Task.Delay(1000).ContinueWith(_ => Dispatcher.Invoke(() => AnimateMessage(false)));
                }
            };

        }

        private void CreateGrid()
        {
            GridContainer.Width = GridColumns * GridCellWidth;
            GridContainer.Height = GridRows * GridCellHeight;

            for (int i = 0; i < GridRows; i++)
            {
                for (int j = 0; j < GridColumns; j++)
                {
                    Rectangle cell = new Rectangle
                    {
                        //Width = GridCellWidth,
                        //Height = GridCellHeight,
                        Width = 200,
                        Height = 200,
                        Stroke = Brushes.White,
                        StrokeThickness = 1

                    };
                    Canvas.SetLeft(cell, j * GridCellWidth);
                    Canvas.SetTop(cell, i * GridCellHeight);

                    var gs = new grid_background();
                    var vb = new VisualBrush();
                    vb.Visual = gs;
                    cell.Fill = vb;

                    GridCanvas.Children.Add(cell);
                }
            }
        }

        private const int ItemMargin = 10;

        private void CreateDraggableItem(ItemConfiguration config)
        {
            var itemControl = new DraggableItemControl(config)
            {
                Width = 180,
                Height = 180
                //Width = GridCellWidth - (ItemMargin * 2),
                //Height = GridCellHeight - (ItemMargin * 2)
            };

            itemControl.MouseLeftButtonDown += Item_MouseLeftButtonDown;
            itemControl.MouseLeftButtonUp += Item_MouseLeftButtonUp;
            itemControl.MouseMove += Item_MouseMove;


            double left = (config.Column * GridCellWidth) + ItemMargin;
            double top = (config.Row * GridCellHeight) + ItemMargin;

            Canvas.SetLeft(itemControl, left);
            Canvas.SetTop(itemControl, top);
            Canvas.SetZIndex(itemControl, config.ZIndex);

            highestZIndex = Math.Max(highestZIndex, config.ZIndex);

            ItemCanvas.Children.Add(itemControl);
            dragItems.Add(itemControl);

            SnapToGrid(itemControl);
        }

        bool test = true;

        private bool isControllingProjectors = false;

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                var result = MessageBox.Show(
                    "종료하시겠습니까?",  // 메시지
                    "종료 확인",          // 제목
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    this.Close();
                }
                return;
            }
        }

        private async Task ControlProjector(string ipAddress, bool powerOn)
        {
            isControllingProjectors = true;
            try
            {
                Debug.WriteLine($"{ipAddress} {(powerOn ? "켜기" : "끄기")}시작");
                using (var pjLink = new PJLinkHelper(ipAddress))
                {
                    await pjLink.ConnectAsync();
                    bool result = powerOn ? await pjLink.PowerOnAsync() : await pjLink.PowerOffAsync();
                    Debug.WriteLine(result
                        ? $"{ipAddress} 프로젝터 전원이 {(powerOn ? "켜졌" : "꺼졌")}습니다."
                        : $"{ipAddress} 프로젝터 전원을 {(powerOn ? "켜는" : "끄는")}데 실패했습니다.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error controlling projector {ipAddress}: {ex.Message}");
            }
            finally
            {
                isControllingProjectors = false;
            }
            await Task.Delay(2000); // 각 프로젝터 제어 후 2초 대기
        }

        private async Task ControlAllProjectors(bool powerOn)
        {
            isControllingProjectors = true;
            await Task.Delay(10000);
            try
            {
                string[] ipAddresses = { "192.168.0.11", "192.168.0.12", "192.168.0.13" };
                foreach (string ip in ipAddresses)
                {
                    await ControlProjector(ip, powerOn);
                    await Task.Delay(1000);
                }
            }
            finally
            {
                isControllingProjectors = false;
                await Task.Delay(10000);
            }
        }



        public void ShowFileExplorer(string ftpAddress, string name)
        {
            if (isftp)
            {
                OverlayGrid.Visibility = Visibility.Visible;
                FileExplorerControl.Initialize(ftpAddress);
                FileExplorerControl.targetname.Text = name;
            }
        }

        private void Item_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var now = DateTime.Now;
            if ((now - lastClickTime).TotalMilliseconds <= DoubleClickTime)
            {
                var item = sender as DraggableItemControl;
                if (item != null && item.Configuration.DeviceType.ToLower() == "pc" && item.ispow)
                {
                    ShowFileExplorer(item.Configuration.IpAddress, item.Configuration.Name);
                }
                e.Handled = true;
            }
            else
            {
                draggedItem = sender as DraggableItemControl;
                offset = e.GetPosition(draggedItem);
                draggedItem.CaptureMouse();

                int rowIndex = (int)Math.Round(Canvas.GetTop(draggedItem) / GridCellHeight);
                int columnIndex = (int)Math.Round(Canvas.GetLeft(draggedItem) / GridCellWidth);
                if (rowIndex >= 0 && rowIndex < GridRows && columnIndex >= 0 && columnIndex < GridColumns)
                {
                    occupiedCells[rowIndex, columnIndex] = false;
                }

                Canvas.SetZIndex(draggedItem, ++highestZIndex);
            }

            lastClickTime = now;
        }

        private void Item_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (draggedItem != null)
            {
                draggedItem.ReleaseMouseCapture();
                SnapToGrid(draggedItem);
                draggedItem = null;
                SaveItemConfigurations();
            }
        }

        private void Item_MouseMove(object sender, MouseEventArgs e)
        {
            if (draggedItem != null)
            {
                Point currentPos = e.GetPosition(ItemCanvas);
                Canvas.SetLeft(draggedItem, currentPos.X - offset.X);
                Canvas.SetTop(draggedItem, currentPos.Y - offset.Y);
            }
        }

        private void SnapToGrid(DraggableItemControl element)
        {
            double left = Canvas.GetLeft(element);
            double top = Canvas.GetTop(element);

            int columnIndex = (int)Math.Round((left - ItemMargin) / GridCellWidth);
            int rowIndex = (int)Math.Round((top - ItemMargin) / GridCellHeight);

            int[] nearestEmptyCell = FindNearestEmptyCell(rowIndex, columnIndex);
            rowIndex = nearestEmptyCell[0];
            columnIndex = nearestEmptyCell[1];

            Canvas.SetLeft(element, (columnIndex * GridCellWidth) + ItemMargin);
            Canvas.SetTop(element, (rowIndex * GridCellHeight) + ItemMargin);

            occupiedCells[rowIndex, columnIndex] = true;

            element.Configuration.Row = rowIndex;
            element.Configuration.Column = columnIndex;
            element.Configuration.ZIndex = Canvas.GetZIndex(element);
        }

        private int[] FindNearestEmptyCell(int startRow, int startCol)
        {
            int maxDistance = Math.Max(GridRows, GridColumns);

            for (int distance = 0; distance < maxDistance; distance++)
            {
                for (int i = -distance; i <= distance; i++)
                {
                    for (int j = -distance; j <= distance; j++)
                    {
                        if (Math.Abs(i) + Math.Abs(j) == distance)
                        {
                            int newRow = startRow + i;
                            int newCol = startCol + j;

                            if (newRow >= 0 && newRow < GridRows && newCol >= 0 && newCol < GridColumns)
                            {
                                if (!occupiedCells[newRow, newCol])
                                {
                                    return new int[] { newRow, newCol };
                                }
                            }
                        }
                    }
                }
            }

            return new int[] { startRow, startCol };
        }

        public void SaveItemConfigurations()
        {
            List<ItemConfiguration> configurations = dragItems.Select(item => item.Configuration).ToList();
            string jsonString = JsonConvert.SerializeObject(configurations, Formatting.Indented);
            File.WriteAllText(ConfigFile, jsonString);
        }

        public void EditItemConfiguration(ItemConfiguration updatedConfiguration)
        {
            List<ItemConfiguration> currentConfigurations = new List<ItemConfiguration>();

            if (File.Exists(ConfigFile))
            {
                string jsonString = File.ReadAllText(ConfigFile);
                currentConfigurations = JsonConvert.DeserializeObject<List<ItemConfiguration>>(jsonString) ?? new List<ItemConfiguration>();
            }

            var existingConfig = currentConfigurations.FirstOrDefault(c => c.id == updatedConfiguration.id);
            if (existingConfig != null)
            {
                existingConfig.Name = updatedConfiguration.Name;
                existingConfig.port = updatedConfiguration.port;
                existingConfig.IpAddress = updatedConfiguration.IpAddress;
                existingConfig.DeviceType = updatedConfiguration.DeviceType;
                existingConfig.MacAddress = updatedConfiguration.MacAddress;
                existingConfig.Channel = updatedConfiguration.Channel;
                existingConfig.IsPower = updatedConfiguration.IsPower;
            }

            string updatedJsonString = JsonConvert.SerializeObject(currentConfigurations, Formatting.Indented);
            File.WriteAllText(ConfigFile, updatedJsonString);

            add_device_ppanel.Visibility = Visibility.Collapsed;

            LoadItemConfigurations();
        }

        private void LoadItemConfigurations()
        {
            if (File.Exists(ConfigFile))
            {
                ItemCanvas.Children.Clear();
                dragItems.Clear();
                string jsonString = File.ReadAllText(ConfigFile);
                List<ItemConfiguration> configurations = JsonConvert.DeserializeObject<List<ItemConfiguration>>(jsonString);

                occupiedCells = new bool[GridRows, GridColumns];

                foreach (var config in configurations)
                {
                    CreateDraggableItem(config);

                    if (config.Row >= 0 && config.Row < GridRows && config.Column >= 0 && config.Column < GridColumns)
                    {
                        occupiedCells[config.Row, config.Column] = true;
                    }

                    highestZIndex = Math.Max(highestZIndex, config.ZIndex);
                }
            }
        }



        private async Task AnimateProgressBar(bool isOn)
        {
            double animationDuration = Progress_duration * 1;
            var watch = System.Diagnostics.Stopwatch.StartNew();

            while (watch.Elapsed.TotalSeconds < animationDuration)
            {
                await Task.Delay(16); // 약 60fps
                double progress = Math.Min(watch.Elapsed.TotalSeconds / animationDuration, 1);
                PowerProgressBar.Value = progress * 100;
            }

            watch.Stop();
            PowerProgressBar.Value = 100;

            PowerOverlay.Visibility = Visibility.Collapsed;

            bool newState = isOn;
            foreach (var item in dragItems)
            {
                item.Configuration.IsOn = newState;
                UpdateItemPowerState(item, newState);
            }
            SaveItemConfigurations();
        }

        private void UpdateItemPowerState(DraggableItemControl item, bool isOn)
        {
            item.UpdatePowerState(isOn);
        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T)
                    return (T)child;
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        private void FileExplorerControl_CloseRequested(object sender, EventArgs e)
        {
            OverlayGrid.Visibility = Visibility.Collapsed;
        }



        private void add_devi(object sender, RoutedEventArgs e)
        {
            add_Device_init();
            add_device_ppanel.Visibility = Visibility.Visible;
            addDeviceWindow.addbtn.Visibility = Visibility.Visible;
            addDeviceWindow.editbtn.Visibility = Visibility.Collapsed;
            addDeviceWindow.title.Text = "장비 등록";
            addDeviceWindow.InitialStateCheckBox.IsChecked = true;
            addDeviceWindow.DeviceTypeComboBox.IsEnabled = true;
        }

        private void del_devi(object sender, RoutedEventArgs e)
        {
            editpanel.Visibility = Visibility.Visible;
            for (int i = 0; i < dragItems.Count; i++)
            {
                dragItems[i].delete_select.Visibility = Visibility.Visible;
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            clickCount++;
            if (clickCount == 2)
            {
                // Double-click detected
                add_Device_init();

                add_device_ppanel.Visibility = Visibility.Visible;
                addDeviceWindow.addbtn.Visibility = Visibility.Visible;
                addDeviceWindow.editbtn.Visibility = Visibility.Collapsed;
                addDeviceWindow.title.Text = "장비 등록";
                addDeviceWindow.InitialStateCheckBox.IsChecked = true;
                addDeviceWindow.DeviceTypeComboBox.IsEnabled = true;


                clickCount = 0; // Reset click count
                clickTimer.Stop(); // Stop the timer
            }
            else
            {
                clickTimer.Start(); // Start or restart the timer
            }
        }

        private void ClickTimer_Tick(object sender, EventArgs e)
        {
            clickTimer.Stop(); // Stop the timer when interval expires
            clickCount = 0; // Reset click count
        }



        private void AddDevice_Click(object sender, RoutedEventArgs e)
        {
            add_Device_init();
            add_device_ppanel.Visibility = Visibility.Visible;
            addDeviceWindow.addbtn.Visibility = Visibility.Visible;
            addDeviceWindow.editbtn.Visibility = Visibility.Collapsed;
            addDeviceWindow.title.Text = "장비 등록";
            addDeviceWindow.DeviceTypeComboBox.IsEnabled = true;
        }


        void add_Device_init()
        {
            addDeviceWindow.NameTextBox.Text = "";
            addDeviceWindow.DeviceTypeComboBox.SelectedIndex = -1;
            addDeviceWindow.IpAddressTextBox.Text = "";
            addDeviceWindow.DescriptionTextBox.Text = "";
            addDeviceWindow.MacAddressTextBox.Text = "";
            addDeviceWindow.ChannelTextBox.Text = "";

            addDeviceWindow.InitialStateCheckBox.IsChecked = false;
        }

        public void createitem(ItemConfiguration newconfig)
        {
            CreateDraggableItem(newconfig);
            SaveItemConfigurations();
        }

        private void RemoveDevice_Click(object sender, RoutedEventArgs e)
        {
            editpanel.Visibility = Visibility.Visible;
            for (int i = 0; i < dragItems.Count; i++)
            {
                dragItems[i].delete_select.Visibility = Visibility.Visible;
            }
        }

        private void item_delete(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
               "현재 목록을 저장 하시겠습니까?",
               "확인",
               MessageBoxButton.YesNo,
               MessageBoxImage.Question
           );

            if (result == MessageBoxResult.Yes)
            {
                for (int i = dragItems.Count - 1; i >= 0; i--)
                {
                    if (dragItems[i].d_select.IsChecked == true)
                    {
                        ItemCanvas.Children.Remove(dragItems[i]);
                        dragItems.RemoveAt(i);
                    }
                }

                SaveItemConfigurations();

                editpanel.Visibility = Visibility.Collapsed;
                for (int i = 0; i < dragItems.Count; i++)
                {
                    dragItems[i].delete_select.Visibility = Visibility.Collapsed;
                    dragItems[i].d_select.IsChecked = false;
                }
            }
        }

        private void editpanel_close(object sender, RoutedEventArgs e)
        {
            editpanel.Visibility = Visibility.Collapsed;
            for (int i = 0; i < dragItems.Count; i++)
            {
                dragItems[i].delete_select.Visibility = Visibility.Collapsed;
            }
        }

        private void AutoPowerSettings_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ShowAutoPowerSettingsOverlay();
            }
        }

        private void ShowAutoPowerSettingsOverlay()
        {
            AutoPowerSettingsControl.LoadSchedule();
            AutoPowerSettingsControl.Visibility = Visibility.Visible;
        }

        private void InitializeAutoPowerSettings()
        {
            AutoPowerSettingsControl.CloseRequested += (sender, e) =>
            {
                AutoPowerSettingsControl.Visibility = Visibility.Collapsed;
            };
        }

        private void Auto_wol_btn_Click(object sender, RoutedEventArgs e)
        {
            ShowAutoPowerSettingsOverlay();
        }

        private UIElement CreateDaySettingControl(string day)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };

            panel.Children.Add(new CheckBox { Content = day, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) });
            panel.Children.Add(new TextBlock { Text = "시작 시간", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0) });
            panel.Children.Add(new TextBox { Width = 50, Margin = new Thickness(0, 0, 5, 0) });
            panel.Children.Add(new TextBlock { Text = "시", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) });
            panel.Children.Add(new TextBox { Width = 50, Margin = new Thickness(0, 0, 5, 0) });
            panel.Children.Add(new TextBlock { Text = "분", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) });
            panel.Children.Add(new TextBlock { Text = "/ 종료 시간", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0) });
            panel.Children.Add(new TextBox { Width = 50, Margin = new Thickness(0, 0, 5, 0) });
            panel.Children.Add(new TextBlock { Text = "시", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) });
            panel.Children.Add(new TextBox { Width = 50, Margin = new Thickness(0, 0, 5, 0) });
            panel.Children.Add(new TextBlock { Text = "분", VerticalAlignment = VerticalAlignment.Center });

            return panel;
        }

        public void RemoveDevice(DraggableItemControl deviceControl)
        {
            ItemCanvas.Children.Remove(deviceControl);
            dragItems.Remove(deviceControl);
            SaveItemConfigurations();

            //여기
            LoadItemConfigurations();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is ToggleButton button)
            {
                button.Cursor = Cursors.Hand;
            }
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is ToggleButton button)
            {
                button.Cursor = Cursors.Arrow; // 또는 Cursors.Default
            }
        }

        private void MenuButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is MenuItem button)
            {
                button.Cursor = Cursors.Hand;
            }
        }

        private void MenuButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is MenuItem button)
            {
                button.Cursor = Cursors.Arrow; // 또는 Cursors.Default
            }
        }
    }


    public class DraggableItem
    {
        public UIElement UIElement { get; set; }
        public ItemConfiguration Configuration { get; set; }
    }

    public class ItemConfiguration
    {
        public string id { get; set; }
        public string Name { get; set; }
        public string DeviceType { get; set; }
        public bool IsOn { get; set; }
        public bool IsPower { get; set; }
        public string FtpAddress { get; set; }
        public string MacAddress { get; set; }
        public string IpAddress { get; set; }
        public string port { get; set; }
        public string Channel { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public int ZIndex { get; set; }
        public string VncPw { get; set; }
        public bool IsDummy { get; set; }
        public bool IsCurrentState { get; set; }
    }
}