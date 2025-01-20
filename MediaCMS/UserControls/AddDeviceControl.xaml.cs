using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MediaCMS;

namespace MediaCMS.UserControls
{
   


    public partial class AddDeviceControl : UserControl
    {

        public ItemConfiguration NewDeviceConfig { get; private set; }

        public AddDeviceControl()
        {
            InitializeComponent();
        }

        static bool IsValidIPv4(string ipAddress)
        {
            string pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
            return Regex.IsMatch(ipAddress, pattern);
        }


        string ConfigFile = "itemConfig.json";
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {



            if (string.IsNullOrWhiteSpace(NameTextBox.Text) ||
                DeviceTypeComboBox.SelectedItem == null ||
                
                string.IsNullOrWhiteSpace(MacAddressTextBox.Text) ||
                string.IsNullOrWhiteSpace(IpAddressTextBox.Text))
            {
                MessageBox.Show("필수 필드를 모두 입력해주세요.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (IsValidIPv4(IpAddressTextBox.Text))
            {
            }
            else
            {
                MessageBox.Show("유효하지 않은 IP 주소입니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string portText = DescriptionTextBox.Text;

            // 포트 번호 유효성 검사
            if (int.TryParse(portText, out int portNumber))
            {
                if (portNumber >= 1 && portNumber <= 65535)
                {
                  
                }
                else
                {
                    MessageBox.Show("유효하지 않은 포트입니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                MessageBox.Show("유효하지 않은 포트입니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            Guid uniqueId = Guid.NewGuid();

            // 문자열로 변환
            string uniqueIdString = uniqueId.ToString();

            var main = Application.Current.MainWindow as MainWindow;



            NewDeviceConfig = new ItemConfiguration
            {
                id = uniqueIdString,
                Name = NameTextBox.Text,
                DeviceType = ((TextBlock)((ComboBoxItem)DeviceTypeComboBox.SelectedItem).Content).Text.ToString(),
                
                MacAddress = MacAddressTextBox.Text,
                IpAddress = IpAddressTextBox.Text,
                port = DescriptionTextBox.Text,
                Channel = ChannelTextBox.Text,
                IsPower = InitialStateCheckBox.IsChecked ?? false,
                Row = 0,
                Column = 0,
                ZIndex = 1,
                VncPw = main.vnc_pw
            };


            //ItemConfiguration newConfig = addDeviceWindow.NewDeviceConfig;
            //CreateDraggableItem(newConfig);
            //SaveItemConfigurations();
            //var main = Application.Current.MainWindow as MainWindow;
            main.createitem(NewDeviceConfig);

            main.add_device_ppanel.Visibility = Visibility.Collapsed;

            data_clear();
            //DialogResult = true;
        }

        ItemConfiguration tempconfig;
        public void set_edit_value(ItemConfiguration config)
        {
            tempconfig = config;
            title.Text = "기기 수정";
            addbtn.Visibility = Visibility.Collapsed;
            editbtn.Visibility = Visibility.Visible;


            NameTextBox.Text = config.Name;


            if (config.DeviceType.ToLower() == "pc")
            {
                DeviceTypeComboBox.SelectedIndex = 0;
            }
            else if (config.DeviceType.ToLower() == "프로젝터(pjlink)")
            {
                DeviceTypeComboBox.SelectedIndex = 1;
            }
            else if (config.DeviceType.ToLower() == "프로젝터(appotronics)")
            {
                DeviceTypeComboBox.SelectedIndex = 2;
            }
            else if (config.DeviceType == "RELAY")
            {
                DeviceTypeComboBox.SelectedIndex = 3;
            }
            else if (config.DeviceType == "PDU")
            {
                DeviceTypeComboBox.SelectedIndex = 4;
            }

            DeviceTypeComboBox.IsEnabled = false;
            MacAddressTextBox.Text = config.MacAddress;
            IpAddressTextBox.Text = config.IpAddress;
            DescriptionTextBox.Text = config.port;
            ChannelTextBox.Text = config.Channel;
            InitialStateCheckBox.IsChecked = config.IsPower;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            cancle_popup();
        }

        public void cancle_popup()
        {
            var main = Application.Current.MainWindow as MainWindow;
            main.add_device_ppanel.Visibility = Visibility.Collapsed;
            data_clear();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {

            var main = Application.Current.MainWindow as MainWindow;



            tempconfig.Name = NameTextBox.Text;
            tempconfig.DeviceType = ((TextBlock)((ComboBoxItem)DeviceTypeComboBox.SelectedItem).Content).Text.ToString();
            tempconfig.MacAddress = MacAddressTextBox.Text;
            tempconfig.IpAddress = IpAddressTextBox.Text;
            tempconfig.port = DescriptionTextBox.Text;
            tempconfig.Channel = ChannelTextBox.Text;
            tempconfig.IsPower = InitialStateCheckBox.IsChecked ?? false;
            //tempconfig.IsOn = InitialStateCheckBox.IsChecked ?? false;


            main.EditItemConfiguration(tempconfig);

            //var main = Application.Current.MainWindow as MainWindow;
            //main.add_device_ppanel.Visibility = Visibility.Collapsed;


            //data_clear();



        }



        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Regular expression to match only alphanumeric characters
            var regex = new Regex("^[a-zA-Z0-9]*$");

            // Check if the input text matches the regular expression
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Remove any non-alphanumeric characters from the text
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                // Use regular expression to remove invalid characters
                var cleanedText = Regex.Replace(textBox.Text, "[^a-zA-Z0-9]", "");

                // Check if the length exceeds the maximum allowed length
                if (cleanedText.Length > 12)
                {
                    cleanedText = cleanedText.Substring(0, 12);
                }

                // Update the textBox text and set the cursor position
                textBox.Text = cleanedText;
                textBox.SelectionStart = textBox.Text.Length; // Move cursor to the end
            }
        }




        void data_clear()
        {
            NameTextBox.Text = "";
            DeviceTypeComboBox.SelectedItem = null;


            MacAddressTextBox.Text = "";
            IpAddressTextBox.Text = "";
            DescriptionTextBox.Text = "";
            ChannelTextBox.Text = string.Empty;
        }
    }
}
