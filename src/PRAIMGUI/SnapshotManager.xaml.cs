using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PRAIM
{
    /// <summary>
    /// Interaction logic for SnapshotManager.xaml
    /// </summary>
    public partial class SnapshotManager : Window, INotifyPropertyChanged
    {
        #region Constants

        public static double MinimumRecSize = 5;

        #endregion Constants

        public ImageSource SelectionImageSource
        {
            get
            {
                return _SelectionImageSource;
            }
            set
            {
                _SelectionImageSource = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SelectionImageSource"));
                }
            }
        }

        public SnapshotManager()
        {
            InitializeComponent();
            this.Loaded += SnapshotManager_Loaded;
        }

        void SnapshotManager_Loaded(object sender, RoutedEventArgs e)
        {
            Bitmap snapshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics graphics = Graphics.FromImage(snapshot);
            graphics.CopyFromScreen(new System.Drawing.Point(0, 0), new System.Drawing.Point(0, 0), Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);

            //-----------------------------------
            // Save the screen to MainImage
            //-----------------------------------
            MemoryStream ms = new MemoryStream();
            snapshot.Save(ms, ImageFormat.Bmp);
            ms.Position = 0;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();

            MainImage.Source = bi;
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _EndX = e.GetPosition(this).X;
            _EndY = e.GetPosition(this).Y;

            if (_IsResizing)
            {
                if (Math.Abs(_EndX - _StartX) > SystemParameters.MinimumHorizontalDragDistance
                       && Math.Abs(_EndY - _StartY) > SystemParameters.MinimumVerticalDragDistance)
                {
                    Canvas.SetTop(SelectionRect, _StartY);
                    Canvas.SetLeft(SelectionRect, _StartX);
                    _PastResizeThreshold = true;
                }
            }

            if (_IsResizing && _PastResizeThreshold)
            {
                SelectionRect.Width = Math.Abs(_EndX - _StartX);
                SelectionRect.Height = Math.Abs(_EndY - _StartY);

                if (_EndX < _StartX)
                {
                    Canvas.SetLeft(SelectionRect, _EndX);
                }

                if (_EndY < _StartY)
                {
                    Canvas.SetTop(SelectionRect, _EndY);
                }

                if (SelectionRect.Visibility != Visibility.Visible)
                {
                    SelectionRect.Visibility = Visibility.Visible;
                }
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!_IsResizing)
                {
                    SelectionRect.Visibility = Visibility.Hidden;
                }
                _IsResizing = true;
                _PastResizeThreshold = false;
                _StartX = e.GetPosition(this).X;
                _StartY = e.GetPosition(this).Y;
                _EndX = e.GetPosition(this).X;
                _EndY = e.GetPosition(this).Y;
            }
        }
        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _IsResizing = false;
            if (Math.Abs(_EndX - _StartX) < MinimumRecSize || Math.Abs(_EndY - _StartY) < MinimumRecSize)
            {
                SelectionRect.Visibility = Visibility.Hidden;
            }
        }

        #region Private Fields

        private double _StartX;
        private double _StartY;
        private double _EndX;
        private double _EndY;
        private bool _IsResizing = false;
        private bool _PastResizeThreshold = false;
        private ImageSource _SelectionImageSource;

        #endregion Private Fields

        private void OnKeyPressed(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
