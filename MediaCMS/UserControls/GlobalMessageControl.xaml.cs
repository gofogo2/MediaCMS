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

namespace WpfApp11.UserControls
{
    /// <summary>
    /// GlobalMessageControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GlobalMessageControl : UserControl
    {
        public GlobalMessageControl()
        {
            InitializeComponent();
        }

        public void SetMessage(string message)
        {
            MessageText.Text = message;
        }
    }
}
