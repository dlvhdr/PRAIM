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
    /// Interaction logic for NewVersionDlg.xaml
    /// </summary>
    public partial class NewVersionDlg : Window
    {
        public List<string> TakenVersions { get; set; }
        public bool Result { get; set; }
        public string Version { get; set; }

        public NewVersionDlg()
        {
            InitializeComponent();
        }

        private void OnNameChanged(object sender, TextChangedEventArgs e)
        {
            string version = (sender as TextBox).Text;

            if (String.IsNullOrEmpty(version) || TakenVersions.Contains(version)) {
                OkButton.IsEnabled = false;
                Result = false;
            } else {
                OkButton.IsEnabled = true;
                Result = true;
            }
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            Result = false;
            this.Close();
        }

        private void OnOK(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
