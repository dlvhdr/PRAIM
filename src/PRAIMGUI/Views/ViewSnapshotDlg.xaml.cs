using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace PRAIM
{
    /// <summary>
    /// Interaction logic for ViewSnapshotDlg.xaml
    /// </summary>
    public partial class ViewSnapshotDlg : Window, INotifyPropertyChanged
    {
        public BitmapSource SnapshotSource
        {
            get { return _SnapshotSource; }
            set
            {
                _SnapshotSource = value;
                NotifyPropertyChanged("SnapshotSource");
            }
        }

        public ViewSnapshotDlg()
        {
            InitializeComponent();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion INotifyPropertyChanged

        private  BitmapSource _SnapshotSource;
    }
}
