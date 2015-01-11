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

namespace PRAIM.Views
{
    /// <summary>
    /// Interaction logic for AboutDlg.xaml
    /// </summary>
    public partial class AboutDlg : Window
    {
        public AboutDlg()
        {
            InitializeComponent();
        }

        private void OnOK(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
