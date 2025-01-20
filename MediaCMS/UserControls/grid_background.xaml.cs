using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfApp11.UserControls
{
    /// <summary>
    /// grid_background.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class grid_background : UserControl
    {
        private int clickCount = 0;
        private DispatcherTimer clickTimer;

        public grid_background()
        {
            InitializeComponent();
            clickTimer = new DispatcherTimer();
            clickTimer.Interval = TimeSpan.FromMilliseconds(500); // 500 ms interval for double-click detection
            clickTimer.Tick += ClickTimer_Tick;
        }

        private void MenuItem1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Menu Item 1 clicked");
        }

        private void MenuItem2_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Menu Item 2 clicked");
        }

        private void MenuItem3_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Menu Item 3 clicked");
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            clickCount++;
            if (clickCount == 2)
            {
                // Double-click detected
                MessageBox.Show("Grid inside UserControl was double-clicked!");
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

    }
}
