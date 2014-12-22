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

namespace Common.Controls
{
    /// <summary>
    /// Combobox with option to clear selection
    /// </summary>
    public partial class NullableCombobox : ComboBox
    {
        public ICommand ClearSelectionCommand { get; set; }
        
        public NullableCombobox()
        {
            ClearSelectionCommand = new Command(ClearSelectionCanExecute, ClearSelectionExecute);
            InitializeComponent();
        }

        private void ClearSelectionExecute(object parameter)
        {
            this.SelectedItem = null;
        }

        private bool ClearSelectionCanExecute(object parameter)
        {
            return true;
        }

        private void OnKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Key.Delete) {
                this.SelectedItem = null;
            }
        }
    }
}
