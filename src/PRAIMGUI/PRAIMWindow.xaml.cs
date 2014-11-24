using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using PRAIM.SnapshotManager;
using Common;
using System.IO;



namespace PRAIM
{
    /// <summary>
    /// Interaction logic for PRAIMWindow.xaml
    /// </summary>
    public partial class PRAIMWindow : Window
    {
        public PRAIMViewModel ViewModel { get { return this.DataContext as PRAIMViewModel; } }

        public ICollectionView DummyDBItems { get; private set; }


        public PRAIMWindow()
        {
            InitializeComponent();

            this.DataContext = new PRAIMViewModel(1, "1.0", Priority.Low);
        }

        private void OnTakeSnapshot(object sender, RoutedEventArgs e)
        {
            this.LayoutUpdated += RunSnapshotMgr;
            this.Hide();
        }

        private void RunSnapshotMgr(object sender, EventArgs e)
        {
            if (IsVisible == true) return;
            Thread.Sleep(200);

            SnapshotManagerWindow snapshotMgr = new SnapshotManagerWindow();
            snapshotMgr.Closed += SnapshotMgrClosed;
            snapshotMgr.Show();

            this.LayoutUpdated -= RunSnapshotMgr;
        }


        private void SnapshotMgrClosed(object sender, EventArgs e)
        {
            SnapshotManagerWindow snapshotMgr = sender as SnapshotManagerWindow;
            this.Show();

            byte[] image_bytes;
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(snapshotMgr.CroppedImage));
            using(MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                image_bytes = ms.ToArray();
                ViewModel.CroppedImageBytes = image_bytes;
            }
        }


        private void OnSave(object sender, RoutedEventArgs e)
        {
            ViewModel.SaveActionItem();
        }
    }
}
