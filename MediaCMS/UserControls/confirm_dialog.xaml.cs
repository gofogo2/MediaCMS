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
using System.Windows.Shapes;

namespace WpfApp11.UserControls
{
    /// <summary>
    /// confirm_dialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class confirm_dialog : Window
    {
        public bool? DialogResult { get; private set; }

        public confirm_dialog()
        {
            InitializeComponent();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
