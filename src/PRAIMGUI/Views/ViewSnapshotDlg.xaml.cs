using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
    /// View action item's snapshot window
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

            /// <summary>
        /// Save action item snapshot to a file
        /// </summary>
        private void OnExportActionItem(object sender, RoutedEventArgs e)
        {
            BitmapSource source = SnapshotSource;

            System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
            dlg.Filter = "JPEG (*.jpeg)|*.jpeg|PNG (*.png)|*.png|BMP (*.bmp)|*.bmp";

            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK) {
                BitmapEncoder encoder;

                if (dlg.FileName.EndsWith("jpeg")) {
                    encoder = new JpegBitmapEncoder();
                } else if (dlg.FileName.EndsWith("bmp")) {
                    encoder = new BmpBitmapEncoder();
                } else {
                    encoder = new PngBitmapEncoder();
                }

                encoder.Frames.Add(BitmapFrame.Create(source));
                using (FileStream fs = new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write)) {
                    encoder.Save(fs);
                }
            }
        }
    }
}
