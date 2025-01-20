using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using System.IO;
using System.Windows.Media;
using System.Windows.Input;

namespace MediaCMS
{
    public partial class AutoPowerSettingsControl : UserControl
    {
        private const string ScheduleFile = "schedule.json";
        private List<DaySettingControl> daySettings = new List<DaySettingControl>();

        public event EventHandler CloseRequested;


        public Dictionary<string, DaySchedule> pow_schedule = new Dictionary<string, DaySchedule>();

        bool all_check = false;

        public AutoPowerSettingsControl()
        {
            InitializeComponent();
            PopulateComboBoxes();
            PopulateDaySettings();
            LoadSchedule();

            DayCheckBox.Checked += DayCheckBox_Checked;
            DayCheckBox.Unchecked += DayCheckBox_Unchecked;


            w1.DayCheckBox.Unchecked += DayCheckBox_Unchecked1;
            w2.DayCheckBox.Unchecked += DayCheckBox_Unchecked1;
            w3.DayCheckBox.Unchecked += DayCheckBox_Unchecked1;
            w4.DayCheckBox.Unchecked += DayCheckBox_Unchecked1;
            w5.DayCheckBox.Unchecked += DayCheckBox_Unchecked1;
            w6.DayCheckBox.Unchecked += DayCheckBox_Unchecked1;
            w7.DayCheckBox.Unchecked += DayCheckBox_Unchecked1;



            w1.StartHourComboBox.SelectionChanged += StartHourComboBox_SelectionChanged;
            w2.StartHourComboBox.SelectionChanged += StartHourComboBox_SelectionChanged;
            w3.StartHourComboBox.SelectionChanged += StartHourComboBox_SelectionChanged;
            w4.StartHourComboBox.SelectionChanged += StartHourComboBox_SelectionChanged;
            w5.StartHourComboBox.SelectionChanged += StartHourComboBox_SelectionChanged;
            w6.StartHourComboBox.SelectionChanged += StartHourComboBox_SelectionChanged;
            w7.StartHourComboBox.SelectionChanged += StartHourComboBox_SelectionChanged;





            w1.StartMinuteComboBox.SelectionChanged += StartMinuteComboBox_SelectionChanged;
            w2.StartMinuteComboBox.SelectionChanged += StartMinuteComboBox_SelectionChanged;
            w3.StartMinuteComboBox.SelectionChanged += StartMinuteComboBox_SelectionChanged;
            w4.StartMinuteComboBox.SelectionChanged += StartMinuteComboBox_SelectionChanged;
            w5.StartMinuteComboBox.SelectionChanged += StartMinuteComboBox_SelectionChanged;
            w6.StartMinuteComboBox.SelectionChanged += StartMinuteComboBox_SelectionChanged;
            w7.StartMinuteComboBox.SelectionChanged += StartMinuteComboBox_SelectionChanged;


            w1.EndHourComboBox.SelectionChanged += EndHourComboBox_SelectionChanged;
            w2.EndHourComboBox.SelectionChanged += EndHourComboBox_SelectionChanged;
            w3.EndHourComboBox.SelectionChanged += EndHourComboBox_SelectionChanged;
            w4.EndHourComboBox.SelectionChanged += EndHourComboBox_SelectionChanged;
            w5.EndHourComboBox.SelectionChanged += EndHourComboBox_SelectionChanged;
            w6.EndHourComboBox.SelectionChanged += EndHourComboBox_SelectionChanged;
            w7.EndHourComboBox.SelectionChanged += EndHourComboBox_SelectionChanged;


            w1.EndMinuteComboBox.SelectionChanged += EndMinuteComboBox_SelectionChanged;
            w2.EndMinuteComboBox.SelectionChanged += EndMinuteComboBox_SelectionChanged;
            w3.EndMinuteComboBox.SelectionChanged += EndMinuteComboBox_SelectionChanged;
            w4.EndMinuteComboBox.SelectionChanged += EndMinuteComboBox_SelectionChanged;
            w5.EndMinuteComboBox.SelectionChanged += EndMinuteComboBox_SelectionChanged;
            w6.EndMinuteComboBox.SelectionChanged += EndMinuteComboBox_SelectionChanged;
            w7.EndMinuteComboBox.SelectionChanged += EndMinuteComboBox_SelectionChanged;




        }

        private void EndMinuteComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (all_check == false)
            {
                if (DayCheckBox.IsChecked == true)
                {
                    if (e.AddedItems[0] != all_EndMinuteComboBox.SelectedValue)
                    {

                        DayCheckBox.IsChecked = false;
                        allDay_bg.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#8D8D8D"));

                    }
                }
            }
        }

        private void EndHourComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (all_check == false)
            {
                if (DayCheckBox.IsChecked == true)
                {
                    if (e.AddedItems[0] != all_EndHourComboBox.SelectedValue)
                    {

                        DayCheckBox.IsChecked = false;
                        allDay_bg.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#8D8D8D"));

                    }
                }
            }
        }

        private void StartMinuteComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (all_check == false)
            {
                if (DayCheckBox.IsChecked == true)
                {
                    if (e.AddedItems[0] != all_StartMinuteComboBox.SelectedValue)
                    {

                        DayCheckBox.IsChecked = false;
                        allDay_bg.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#8D8D8D"));

                    }
                }
            }
        }

        private void StartHourComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (all_check == false)
            {
                if (DayCheckBox.IsChecked == true)
                {
                    if (e.AddedItems[0] != all_StartHourComboBox.SelectedValue)
                    {

                        DayCheckBox.IsChecked = false;
                        allDay_bg.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#8D8D8D"));

                    }
                }
            }
        }

        private void DayCheckBox_Unchecked1(object sender, RoutedEventArgs e)
        {
            DayCheckBox.IsChecked = false;
        }

        private void DayCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            allDay_bg.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#8D8D8D"));
        }

        private void DayCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            all_check = true;
            if (all_StartHourComboBox.SelectedIndex != -1 && all_StartMinuteComboBox.SelectedIndex != -1 && all_EndHourComboBox.SelectedIndex != -1 && all_EndMinuteComboBox.SelectedIndex != -1)
            {
                for (int i = 0; i < daySettings.Count; i++)
                {
                    daySettings[i].StartHourComboBox.SelectedIndex = all_StartHourComboBox.SelectedIndex;
                    daySettings[i].StartMinuteComboBox.SelectedIndex = all_StartMinuteComboBox.SelectedIndex;
                    daySettings[i].EndHourComboBox.SelectedIndex = all_EndHourComboBox.SelectedIndex;
                    daySettings[i].EndMinuteComboBox.SelectedIndex = all_EndMinuteComboBox.SelectedIndex;

                    daySettings[i].DayCheckBox.IsChecked = true;


                    allDay_bg.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#5BB0FE"));
                }
            }
            else
            {
                MessageBox.Show("모든 시간을 입력하세요.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                DayCheckBox.IsChecked = false;
            }
            all_check = false;
        }

        private void PopulateComboBoxes()
        {
            for (int i = 0; i < 24; i++)
            {
                TextBlock tb = new TextBlock();
                tb.Text = i.ToString("00");
                tb.FontFamily = (FontFamily)FindResource("NotoSansFontBoldFamily");
                tb.FontWeight = FontWeights.Bold;
                all_StartHourComboBox.Items.Add(tb);

                //all_StartHourComboBox.Items.Add(i.ToString("D2"));
                //all_EndHourComboBox.Items.Add(i.ToString("D2"));
            }
            for (int i = 0; i < 24; i++)
            {
                TextBlock tb = new TextBlock();
                tb.Text = i.ToString("00");
                tb.FontFamily = (FontFamily)FindResource("NotoSansFontBoldFamily");
                tb.FontWeight = FontWeights.Bold;
                all_EndHourComboBox.Items.Add(tb);

                //all_StartHourComboBox.Items.Add(i.ToString("D2"));
                //all_EndHourComboBox.Items.Add(i.ToString("D2"));
            }

            for (int i = 0; i < 60; i += 1)
            {
                TextBlock tb = new TextBlock();
                tb.Text = i.ToString("00");
                tb.FontFamily = (FontFamily)FindResource("NotoSansFontBoldFamily");
                tb.FontWeight = FontWeights.Bold;
                all_StartMinuteComboBox.Items.Add(tb);
                

               // all_StartMinuteComboBox.Items.Add(i.ToString("D2"));
               //all_EndMinuteComboBox.Items.Add(i.ToString("D2"));
            }
            for (int i = 0; i < 60; i += 1)
            {
                TextBlock tb = new TextBlock();
                tb.Text = i.ToString("00");
                tb.FontFamily = (FontFamily)FindResource("NotoSansFontBoldFamily");
                tb.FontWeight = FontWeights.Bold;
                all_EndMinuteComboBox.Items.Add(tb);


                // all_StartMinuteComboBox.Items.Add(i.ToString("D2"));
                //all_EndMinuteComboBox.Items.Add(i.ToString("D2"));
            }
        }

        private void allapply_click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (all_StartHourComboBox.SelectedIndex != -1 && all_StartMinuteComboBox.SelectedIndex != -1 && all_EndHourComboBox.SelectedIndex != -1 && all_EndMinuteComboBox.SelectedIndex != -1)
            {
                for (int i = 0; i < daySettings.Count; i++)
                {
                    daySettings[i].StartHourComboBox.SelectedIndex = all_StartHourComboBox.SelectedIndex;
                    daySettings[i].StartMinuteComboBox.SelectedIndex = all_StartMinuteComboBox.SelectedIndex;
                    daySettings[i].EndHourComboBox.SelectedIndex = all_EndHourComboBox.SelectedIndex;
                    daySettings[i].EndMinuteComboBox.SelectedIndex = all_EndMinuteComboBox.SelectedIndex;

                    daySettings[i].DayCheckBox.IsChecked = true;
                }
            }
            else
            {
                MessageBox.Show("모든 시간을 입력하세요.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }


            

        }

        private void PopulateDaySettings()
        {
            //string[] days = { "월요일", "화요일", "수요일", "목요일", "금요일", "토요일", "일요일" };
            //foreach (var day in days)
            //{
            //    var daySettingControl = new DaySettingControl(day);
            //    DaySettingsPanel.Children.Add(daySettingControl);
            //    daySettings.Add(daySettingControl);
            //}

            w1.settingControl("월요일");
            daySettings.Add(w1);

            w2.settingControl("화요일");
            daySettings.Add(w2);

            w3.settingControl("수요일");
            daySettings.Add(w3);

            w4.settingControl("목요일");
            daySettings.Add(w4);

            w5.settingControl("금요일");
            daySettings.Add(w5);

            w6.settingControl("토요일");
            daySettings.Add(w6);

            w7.settingControl("일요일");
            daySettings.Add(w7);

        }

        public void LoadSchedule()
        {
            DayCheckBox.IsChecked = false;
            all_StartHourComboBox.SelectedIndex = -1;
            all_StartMinuteComboBox.SelectedIndex = -1;

            all_EndHourComboBox.SelectedIndex = -1;
            all_EndMinuteComboBox.SelectedIndex = -1;

            if (File.Exists(ScheduleFile))
            {
                string json = File.ReadAllText(ScheduleFile);
                var schedule = JsonConvert.DeserializeObject<Dictionary<string, DaySchedule>>(json);
                pow_schedule = schedule;
                foreach (var daySetting in daySettings)
                {
                    if (schedule.TryGetValue(daySetting.Day, out var daySchedule))
                    {
                        daySetting.SetSchedule(daySchedule);
                    }
                }
            }
            else
            {

                var schedule = new Dictionary<string, DaySchedule>();


                DaySchedule d = new DaySchedule();
                d.StartTime = TimeSpan.Parse("01:00");
                d.EndTime = TimeSpan.Parse("03:00");

                schedule.Add("월요일", d);
                schedule.Add("화요일", d);
                schedule.Add("수요일", d);
                schedule.Add("목요일", d);
                schedule.Add("금요일", d);
                schedule.Add("토요일", d);
                schedule.Add("일요일", d);

                pow_schedule = schedule;
                string json = JsonConvert.SerializeObject(schedule, Formatting.Indented);
                File.WriteAllText(ScheduleFile, json);



                 string json2 = File.ReadAllText(ScheduleFile);
                var schedule2 = JsonConvert.DeserializeObject<Dictionary<string, DaySchedule>>(json2);
                pow_schedule = schedule2;
                foreach (var daySetting in daySettings)
                {
                    if (schedule2.TryGetValue(daySetting.Day, out var daySchedule))
                    {
                        daySetting.SetSchedule(daySchedule);
                    }
                }






            }
        }

        public void init()
        {
            //if (!File.Exists(ScheduleFile))
            //{
            //    var schedule = new Dictionary<string, DaySchedule>();


            //    DaySchedule d = new DaySchedule();
            //    d.StartTime = TimeSpan.Parse("01:00");
            //    d.EndTime = TimeSpan.Parse("03:00");

            //    schedule.Add("월요일", d);
            //    schedule.Add("화요일", d);
            //    schedule.Add("수요일", d);
            //    schedule.Add("목요일", d);
            //    schedule.Add("금요일", d);
            //    schedule.Add("토요일", d);
            //    schedule.Add("일요일", d);

            //    pow_schedule = schedule;
            //    string json = JsonConvert.SerializeObject(schedule, Formatting.Indented);
            //    File.WriteAllText(ScheduleFile, json);
            //}
        }


        private void SaveSchedule()
        {
            var schedule = new Dictionary<string, DaySchedule>();
            
            foreach (var daySetting in daySettings)
            {
                schedule[daySetting.Day] = daySetting.GetSchedule();
            }
          

            //daySettings[0].Day = "월요일";
            //daySettings[0].
            //schedule.Add

            pow_schedule = schedule;
            string json = JsonConvert.SerializeObject(schedule, Formatting.Indented);
            File.WriteAllText(ScheduleFile, json);
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSchedule();
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        public void cancle_ev()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void StartHourComboBox_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var button = sender as FrameworkElement;
            button.Cursor = Cursors.Hand;
        }

        private void StartHourComboBox_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var button = sender as FrameworkElement;
            button.Cursor = Cursors.Arrow;
        }

    }



    public class DaySchedule
    {
        public bool IsEnabled { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}