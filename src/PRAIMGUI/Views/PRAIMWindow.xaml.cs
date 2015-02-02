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
using System.Globalization;
using PRAIM.Views;



namespace PRAIM
{
    /// <summary>
    /// Interaction logic for PRAIMWindow.xaml
    /// </summary>
    public partial class PRAIMWindow : Window
    {
        /// <summary>
        /// The view model for the view
        /// </summary>
        public PRAIMViewModel ViewModel { get { return this.DataContext as PRAIMViewModel; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public PRAIMWindow()
        {
            InitializeComponent();
        }

        #region Command handlers

        /// <summary>
        /// Take snapshot button handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTakeSnapshot(object sender, RoutedEventArgs e)
        {
            this.LayoutUpdated += RunSnapshotMgr;
            this.Hide();
        }

        /// <summary>
        /// Run snapshot manager (open the snapshot manager window)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RunSnapshotMgr(object sender, EventArgs e)
        {
            if (IsVisible == true) return;
            Thread.Sleep(200);

            SnapshotManagerWindow snapshotMgr = new SnapshotManagerWindow();
            snapshotMgr.Closed += SnapshotMgrClosed;
            snapshotMgr.Show();

            this.LayoutUpdated -= RunSnapshotMgr;
        }

        /// <summary>
        /// Handler for when the snapshot manager is closed (snapshot taken)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SnapshotMgrClosed(object sender, EventArgs e)
        {
            SnapshotManagerWindow snapshotMgr = sender as SnapshotManagerWindow;
            this.Show();

            byte[] image_bytes;
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            if (snapshotMgr.CroppedImage != null) {
                encoder.Frames.Add(BitmapFrame.Create(snapshotMgr.CroppedImage));
                using (MemoryStream ms = new MemoryStream()) {
                    encoder.Save(ms);
                    image_bytes = ms.ToArray();
                    ViewModel.CroppedImageBytes = image_bytes;
                    ViewModel.CroppedImage = snapshotMgr.CroppedImage;
                    ViewModel.DateTime = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// Handler for search DB button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchDB(object sender, RoutedEventArgs e)
        {
            ViewModel.SearchDB();
        }

        /// <summary>
        /// Handler for exit clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnExit(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handler for show full size action item snapshot.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActionItemDoubleClick(object sender, RoutedEventArgs e)
        {
            BitmapSource source = ViewModel.GetSnapshotSource(ViewModel.SelectedActionItem);

            ViewSnapshotDlg dlg = new ViewSnapshotDlg()
            {
                SnapshotSource = source,
                Owner = this,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner
            };
            dlg.Show();
        }

        /// <summary>
        /// Handler for generate report button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GenerateReport(object sender, RoutedEventArgs e)
        {
            ViewModel.GenerateReport();
        }

        /// <summary>
        /// Handler for open about dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenAbout(object sender, EventArgs e)
        {
            AboutDlg dlg = new AboutDlg() { Owner = this, WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner };
            dlg.Show();
        }

        #endregion Command handlers

        private void OpenUserManual(object sender, RoutedEventArgs e)
        {
            string pdf_loc = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            System.Diagnostics.Process.Start(pdf_loc + "user_manual.pdf");
        }

        private void PriorityComboboxLoaded(object sender, RoutedEventArgs e)
        {
            (sender as ComboBox).SelectedIndex = 0;
        }
    }
}
