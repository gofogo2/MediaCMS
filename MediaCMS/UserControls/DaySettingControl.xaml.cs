using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MediaCMS
{
    public partial class DaySettingControl : UserControl
    {
        public string Day { get; private set; }

        string starttime_hour = string.Empty;
        string endtime_hour = string.Empty;
        string starttime_min = string.Empty;
        string endtime_min = string.Empty;
        public DaySettingControl()
        {
            InitializeComponent();

            DayCheckBox.Checked += DayCheckBox_Checked;
            DayCheckBox.Unchecked += DayCheckBox_Unchecked;

            StartHourComboBox.SelectionChanged += StartHourComboBox_SelectionChanged;
            StartMinuteComboBox.SelectionChanged += StartMinuteComboBox_SelectionChanged;

            EndHourComboBox.SelectionChanged += EndHourComboBox_SelectionChanged;
            EndMinuteComboBox.SelectionChanged += EndMinuteComboBox_SelectionChanged;
        }

        private void EndMinuteComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            endtime_min = e.AddedItems[0].ToString();
        }

        private void EndHourComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0)
            {
                endtime_hour = e.AddedItems[0].ToString();
                //checkTimeSet_enable("endhour");
            }
            
        }

        private void StartMinuteComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            starttime_min = e.AddedItems[0].ToString();
        }

        private void StartHourComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0)
            {
                starttime_hour = e.AddedItems[0].ToString();
                //checkTimeSet_enable("starthour");
            }  
        }



        void checkTimeSet_enable(string select_state)
        {
            if (select_state == "endhour")
            {
                if (starttime_hour != string.Empty && endtime_hour != string.Empty)
                {
                    int sh = Convert.ToInt32(starttime_hour);
                    int eh = Convert.ToInt32(endtime_hour);
                    if (sh > eh)
                    {
                        MessageBox.Show("종료시간이 시작시간보다 빠를 수 없습니다.");
                        EndHourComboBox.SelectedIndex = -1;
                    }
                }
            }
            else if (select_state == "starthour")
            {
                if (starttime_hour != string.Empty && endtime_hour != string.Empty)
                {
                    int sh = Convert.ToInt32(starttime_hour);
                    int eh = Convert.ToInt32(endtime_hour);
                    if (sh > eh)
                    {
                        MessageBox.Show("시작시간이 종료시간보다 늦을 수 없습니다.");
                        StartHourComboBox.SelectedIndex = -1;
                    }
                }
            }
        }

        private void DayCheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                if (checkBox.IsChecked == true)
                {
                        
                    Day_bg.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#5BB0FE"));
                }
                else
                {
                    Day_bg.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#8D8D8D"));
                }
            }

        }
        private void DayCheckBox_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                if (checkBox.IsChecked == false)
                {
                    Day_bg.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#8D8D8D"));
                }
            }
        }


        //public DaySettingControl(string day)
        //{
        //    InitializeComponent();
        //    Day = day;
        //    DayCheckBox.Content = day;
        //    PopulateComboBoxes();
        //}

        public void settingControl(string day)
        {
            Day = day;
            DayName.Text = day;
            PopulateComboBoxes();
        }

        private void PopulateComboBoxes()
        {
            //for (int i = 1; i < 25; i++)
            //{
            //    StartHourComboBox.Items.Add(i.ToString("D2"));
            //    EndHourComboBox.Items.Add(i.ToString("D2"));
            //}

            //for (int i = 0; i < 60; i += 1)
            //{
            //    StartMinuteComboBox.Items.Add(i.ToString("D2"));
            //    EndMinuteComboBox.Items.Add(i.ToString("D2"));
            //}




            for (int i = 0; i < 24; i++)
            {
                TextBlock tb = new TextBlock();
                tb.Text = i.ToString("00");
                tb.FontFamily = (FontFamily)FindResource("NotoSansFontBoldFamily");
                tb.FontWeight = FontWeights.Bold;
                StartHourComboBox.Items.Add(tb);
            }
            for (int i = 0; i < 24; i++)
            {
                TextBlock tb = new TextBlock();
                tb.Text = i.ToString("00");
                tb.FontFamily = (FontFamily)FindResource("NotoSansFontBoldFamily");
                tb.FontWeight = FontWeights.Bold;
                EndHourComboBox.Items.Add(tb);
            }

            for (int i = 0; i < 60; i += 1)
            {
                TextBlock tb = new TextBlock();
                tb.Text = i.ToString("00");
                tb.FontFamily = (FontFamily)FindResource("NotoSansFontBoldFamily");
                tb.FontWeight = FontWeights.Bold;
                StartMinuteComboBox.Items.Add(tb);
            }
            for (int i = 0; i < 60; i += 1)
            {
                TextBlock tb = new TextBlock();
                tb.Text = i.ToString("00");
                tb.FontFamily = (FontFamily)FindResource("NotoSansFontBoldFamily");
                tb.FontWeight = FontWeights.Bold;
                EndMinuteComboBox.Items.Add(tb);
            }








        }

        public void SetSchedule(DaySchedule schedule)
        {
            DayCheckBox.IsChecked = schedule.IsEnabled;

            foreach (var item in StartHourComboBox.Items)
            {
                if (item is TextBlock textBlock && textBlock.Text == schedule.StartTime.Hours.ToString("D2"))
                {
                    StartHourComboBox.SelectedItem = textBlock;
                    break;
                }
            }


            foreach (var item in StartMinuteComboBox.Items)
            {
                if (item is TextBlock textBlock && textBlock.Text == schedule.StartTime.Minutes.ToString("D2"))
                {
                    StartMinuteComboBox.SelectedItem = textBlock;
                    break;
                }
            }

            foreach (var item in EndHourComboBox.Items)
            {
                if (item is TextBlock textBlock && textBlock.Text == schedule.EndTime.Hours.ToString("D2"))
                {
                    EndHourComboBox.SelectedItem = textBlock;
                    break;
                }
            }

            foreach (var item in EndMinuteComboBox.Items)
            {
                if (item is TextBlock textBlock && textBlock.Text == schedule.EndTime.Minutes.ToString("D2"))
                {
                    EndMinuteComboBox.SelectedItem = textBlock;
                    break;
                }
            }


            //StartHourComboBox.SelectedItem = schedule.StartTime.Hours.ToString("D2");
            //StartMinuteComboBox.SelectedItem = schedule.StartTime.Minutes.ToString("D2");
            //EndHourComboBox.SelectedItem = schedule.EndTime.Hours.ToString("D2");
            //EndMinuteComboBox.SelectedItem = schedule.EndTime.Minutes.ToString("D2");





        }

        public DaySchedule GetSchedule()
        {
            var ds = new DaySchedule();
            ds.IsEnabled = DayCheckBox.IsChecked ?? false;

            if (StartHourComboBox.SelectedItem != null && StartMinuteComboBox.SelectedItem != null && EndHourComboBox.SelectedItem != null && EndMinuteComboBox.SelectedItem != null)
            {
                TextBlock selectedTextBlock = StartHourComboBox.SelectedItem as TextBlock;
                TextBlock selectedTextBlock2 = StartMinuteComboBox.SelectedItem as TextBlock;

                TextBlock selectedTextBlock3 = EndHourComboBox.SelectedItem as TextBlock;
                TextBlock selectedTextBlock4 = EndMinuteComboBox.SelectedItem as TextBlock;

                ds.StartTime = new TimeSpan(int.Parse(selectedTextBlock.Text), int.Parse(selectedTextBlock2.Text), 0);
                ds.EndTime = new TimeSpan(int.Parse(selectedTextBlock3.Text), int.Parse(selectedTextBlock4.Text), 0);
            }
            else
            {
            }
            return ds;


            //    return new DaySchedule
            //{
            //    IsEnabled = DayCheckBox.IsChecked ?? false,
            //    StartTime = new TimeSpan(int.Parse(StartHourComboBox.SelectedItem as string), int.Parse(StartMinuteComboBox.SelectedItem as string), 0),
            //    EndTime = new TimeSpan(int.Parse(EndHourComboBox.SelectedItem as string), int.Parse(EndMinuteComboBox.SelectedItem as string), 0)
            //};
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
}