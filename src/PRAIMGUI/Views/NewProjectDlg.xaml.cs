using Common;
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
    /// Interaction logic for NewProjectDlg.xaml
    /// </summary>
    public partial class NewProjectDlg : Window
    {
        public Command OnOKCommand { get; set; }
        public bool Result { get; set; }
        public bool LegalName { get; set; }

        public List<string> TakenNames { get; set; }
        
        public NewProjectDlg()
        {
            OnOKCommand = new Command(CanPressOK, OnOK);

            InitializeComponent();
        }

        private bool CanPressOK(object parameter)
        {
            return LegalName;
        }

        private void OnOK(object parameter)
        {
            Result = true;
            this.Close();
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            Result = false;
            this.Close();
        }

        private void OnNameChanged(object sender, TextChangedEventArgs e)
        {
            string new_name = (sender as TextBox).Text;
            LegalName = true;
            if (String.IsNullOrEmpty(new_name)) LegalName = false;
            if (TakenNames.Contains(new_name)) LegalName = false;
            OnOKCommand.UpdateCanExecuteState();
        }
    }
}
