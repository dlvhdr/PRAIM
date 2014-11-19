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

        public static double MinimumRecLength = 2;
        public static double MinimumHookProximity = 3;

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
                NotifyPropertyChanged("SelectionImageSource");
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
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

            //---------------------------------
            // Give mouse capture to 
            //  canvas for initial selection
            //---------------------------------
            Mouse.Capture(this);
        }

        #region Window Mouse Handlers

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) {
                if (!_IsResizing) {
                    SelectionRect.Visibility = Visibility.Collapsed;
                }

                Mouse.Capture(this);
                _IsResizing = true;
                _PastResizeThreshold = false;
                _StartX = e.GetPosition(MainCanvas).X;
                _StartY = e.GetPosition(MainCanvas).Y;
            }
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_IsResizing || !this.IsMouseCaptured) return;

            _EndX = e.GetPosition(MainCanvas).X;
            _EndY = e.GetPosition(MainCanvas).Y;

            if (IsPastResizeThreshold()) {
                Canvas.SetLeft(SelectionRect, _StartX);
                Canvas.SetTop(SelectionRect, _StartY);
                SelectionRect.Visibility = Visibility.Visible;
                _PastResizeThreshold = true;
            }

            if (_IsResizing && _PastResizeThreshold) {
                this.MouseMove -= OnMouseMove;
                Mouse.Capture(BottomRightHook);
                return;
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            _IsResizing = false;
            _PastResizeThreshold = false;
            this.MouseMove += OnMouseMove;
        }

        #endregion Window Mouse Handlers

        #region Move Rectangle Handlers

        private void OnMoveHelperDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            double new_left = _StartX + e.HorizontalChange;
            double new_top = _StartY + e.VerticalChange;

            //Make sure control is not exceeding the canvas' limits
            if (new_left < 0) new_left = 0;
            if (new_top < 0) new_top = 0;

            if (new_left + SelectionRect.Width > MainCanvas.Width) {
                new_left = MainCanvas.Width - SelectionRect.Width;
            }

            if (MainCanvas != null && new_top + SelectionRect.Height > MainCanvas.Height) {
                new_top = MainCanvas.Height - SelectionRect.Height;
            }

            _StartX = new_left;
            _StartY = new_top;
            Canvas.SetLeft(SelectionRect, _StartX);
            Canvas.SetTop(SelectionRect, _StartY);
        }

        #endregion Move Rectangle Handlers

        #region Bottom Right Hook Handlers

        private void BottomRightMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_IsResizing) return;

            _EndX = e.GetPosition(MainCanvas).X;
            _EndY = e.GetPosition(MainCanvas).Y;

            if (IsPastResizeThreshold()) {
                _PastResizeThreshold = true;
            }

            //-------------------------------------
            // Give mouse control to UpperLeftHook
            //-------------------------------------
            if (_PastResizeThreshold) {
                if (SelectionRect.Width < MinimumHookProximity && SelectionRect.Height < MinimumHookProximity &&
                    _EndX < _StartX && _EndY < _StartY) {
                    Mouse.Capture(UpperLeftHook);
                    return;
                }
                if (SelectionRect.Width > MinimumHookProximity && SelectionRect.Height < MinimumHookProximity
                    && _EndY < _StartY) {
                    Mouse.Capture(UpperRightHook);
                    return;
                }
                if (SelectionRect.Width < MinimumHookProximity && SelectionRect.Height > MinimumHookProximity
                    && _EndX < _StartX) {
                    Mouse.Capture(BottomLeftHook);
                    return;
                }
            }

            if (_IsResizing && _PastResizeThreshold && BottomRightHook.IsMouseCaptured) {
                SelectionRect.Width = Math.Max(MinimumRecLength, _EndX - _StartX);
                SelectionRect.Height = Math.Max(MinimumRecLength, _EndY - _StartY);
            }

            Mouse.Capture(BottomRightHook);
            e.Handled = true;
        }

        #endregion Bottom Right Hook Handlers

        #region Upper Left Hook Handlers

        private void UpperLeftMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_IsResizing) return;

            _EndX = e.GetPosition(MainCanvas).X;
            _EndY = e.GetPosition(MainCanvas).Y;

            if (IsPastResizeThreshold()) {
                _PastResizeThreshold = true;
            }

            //----------------------------------------
            // Give mouse control to BottomRightHook
            //----------------------------------------
            if (_PastResizeThreshold) {
                if (SelectionRect.Width < MinimumHookProximity && SelectionRect.Height < MinimumHookProximity &&
                    _EndX > _StartX && _EndY > _StartY) {
                    Mouse.Capture(BottomRightHook);
                    return;
                }
                if (SelectionRect.Width > MinimumHookProximity && SelectionRect.Height < MinimumHookProximity
                    && _EndY > _StartY) {
                    Mouse.Capture(BottomLeftHook);
                    return;
                }
                if (SelectionRect.Width < MinimumHookProximity && SelectionRect.Height > MinimumHookProximity
                    && _EndX > _StartX) {
                    Mouse.Capture(UpperRightHook);
                    return;
                }
            }

            if (_IsResizing && _PastResizeThreshold && UpperLeftHook.IsMouseCaptured) {
                //Set width and height
                double old_width = SelectionRect.Width;
                double old_height = SelectionRect.Height;

                SelectionRect.Width = Math.Max(MinimumRecLength, SelectionRect.Width - (_EndX - _StartX));
                SelectionRect.Height = Math.Max(MinimumRecLength, SelectionRect.Height - (_EndY - _StartY));

                //Set Canvas.Left and Canvas.Top
                if (_EndX < _StartX || !Utilities.Equal(SelectionRect.Width, MinimumRecLength)) {
                    _StartX = _EndX;
                    Canvas.SetLeft(SelectionRect, _StartX);
                } else {
                    _StartX = _StartX + old_width - SelectionRect.Width;
                    Canvas.SetLeft(SelectionRect, _StartX);
                }

                if (_EndY < _StartY || !Utilities.Equal(SelectionRect.Height, MinimumRecLength)) {
                    _StartY = _EndY;
                    Canvas.SetTop(SelectionRect, _StartY);
                } else {
                    _StartY = _StartY + old_height - SelectionRect.Height;
                    Canvas.SetTop(SelectionRect, _StartY);
                }
            }

            Mouse.Capture(UpperLeftHook);
            e.Handled = true;
        }

        #endregion Upper Left Hook Handlers

        #region Upper Right Hook Handlers

        private void UpperRightMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_IsResizing) return;

            _EndX = e.GetPosition(MainCanvas).X;
            _EndY = e.GetPosition(MainCanvas).Y;

            if (IsPastResizeThreshold()) {
                _PastResizeThreshold = true;
            }

            //-----------------------------------------------
            // Check if need to give control to another hook
            //-----------------------------------------------
            if (_PastResizeThreshold) {
                if (SelectionRect.Width < MinimumHookProximity && SelectionRect.Height < MinimumHookProximity
                    && _EndX < _StartX && _EndY > _StartY) {
                    Mouse.Capture(BottomLeftHook);
                    return;
                }
                if (SelectionRect.Width > MinimumHookProximity && SelectionRect.Height < MinimumHookProximity
                    && _EndY > _StartY) {
                    Mouse.Capture(BottomRightHook);
                    return;
                }
                if (SelectionRect.Width < MinimumHookProximity && SelectionRect.Height > MinimumHookProximity &&
                    _EndX < _StartX) {
                    Mouse.Capture(UpperLeftHook);
                    return;
                }
            }

            if (_IsResizing && _PastResizeThreshold && UpperRightHook.IsMouseCaptured) {
                double old_height = SelectionRect.Height;
                
                SelectionRect.Width = Math.Max(MinimumRecLength, _EndX - _StartX);
                SelectionRect.Height = Math.Max(MinimumRecLength, SelectionRect.Height - (_EndY - _StartY));

                if (_EndY < _StartY || !Utilities.Equal(SelectionRect.Height, MinimumRecLength)) {
                    _StartY = _EndY;
                    Canvas.SetTop(SelectionRect, _StartY);
                } else {
                    _StartY = _StartY + old_height - SelectionRect.Height;
                    Canvas.SetTop(SelectionRect, _StartY);
                }
            }

            Mouse.Capture(UpperRightHook);
            e.Handled = true;
        }

        #endregion Upper Right Hook Handlers

        #region Bottom Left Hook Handlers

        private void BottomLeftMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_IsResizing) return;

            _EndX = e.GetPosition(MainCanvas).X;
            _EndY = e.GetPosition(MainCanvas).Y;

            if (IsPastResizeThreshold()) {
                _PastResizeThreshold = true;
            }

            //-----------------------------------------------
            // Check if need to give control to another hook
            //-----------------------------------------------
            if (_PastResizeThreshold) {
                if (SelectionRect.Width < MinimumHookProximity && SelectionRect.Height < MinimumHookProximity
                    && _EndX > _StartX && _EndY < _StartY) {
                    Mouse.Capture(UpperRightHook);
                    return;
                }
                if (SelectionRect.Width > MinimumHookProximity && SelectionRect.Height < MinimumHookProximity
                    && _EndY < _StartY) {
                    Mouse.Capture(UpperLeftHook);
                    return;
                }
                if (SelectionRect.Width < MinimumHookProximity && SelectionRect.Height > MinimumHookProximity &&
                    _EndX > _StartX) {
                    Mouse.Capture(BottomRightHook);
                    return;
                }
            }

            if (_IsResizing && _PastResizeThreshold && BottomLeftHook.IsMouseCaptured) {
                double old_width = SelectionRect.Width;

                SelectionRect.Width = Math.Max(MinimumRecLength, SelectionRect.Width - (_EndX - _StartX));
                SelectionRect.Height = Math.Max(MinimumRecLength, _EndY - _StartY);

                //Set Canvas.Left and Canvas.Top
                if (_EndX < _StartX || !Utilities.Equal(SelectionRect.Width, MinimumRecLength)) {
                    _StartX = _EndX;
                    Canvas.SetLeft(SelectionRect, _StartX);
                } else {
                    _StartX = _StartX + old_width - SelectionRect.Width;
                    Canvas.SetLeft(SelectionRect, _StartX);
                }
            }

            Mouse.Capture(BottomLeftHook);
            e.Handled = true;
        }

        #endregion Bottom Left Hook Handlers

        #region Upper Center Hook Handlers

        private void UpperCenterMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_IsResizing) return;

            _EndX = e.GetPosition(MainCanvas).X;
            _EndY = e.GetPosition(MainCanvas).Y;

            if (IsPastResizeThreshold()) {
                _PastResizeThreshold = true;
            }

            //-----------------------------------------------
            // Check if need to give control to another hook
            //-----------------------------------------------
            if (_PastResizeThreshold) {
                if (SelectionRect.Height < MinimumHookProximity && _EndY > _StartY) {
                    Mouse.Capture(BottomCenterHook);
                    return;
                }
            }

            if (_IsResizing && _PastResizeThreshold && UpperCenterHook.IsMouseCaptured) {
                double old_height = SelectionRect.Height;

                SelectionRect.Height = Math.Max(MinimumRecLength, SelectionRect.Height - (_EndY - _StartY));

                if (_EndY < _StartY || !Utilities.Equal(SelectionRect.Height, MinimumRecLength)) {
                    _StartY = _EndY;
                    Canvas.SetTop(SelectionRect, _StartY);
                } else {
                    _StartY = _StartY + old_height - SelectionRect.Height;
                    Canvas.SetTop(SelectionRect, _StartY);
                }
            }

            Mouse.Capture(UpperCenterHook);
            e.Handled = true;
        }

        #endregion Upper Center Hook Handlers

        #region Bottom Center Hook Handlers

        private void BottomCenterMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_IsResizing) return;

            _EndX = e.GetPosition(MainCanvas).X;
            _EndY = e.GetPosition(MainCanvas).Y;

            if (IsPastResizeThreshold()) {
                _PastResizeThreshold = true;
            }

            //-----------------------------------------------
            // Check if need to give control to another hook
            //-----------------------------------------------
            if (_PastResizeThreshold) {
                if (SelectionRect.Height < MinimumHookProximity && _EndY < _StartY) {
                    Mouse.Capture(UpperCenterHook);
                    return;
                }
            }

            if (_IsResizing && _PastResizeThreshold && BottomCenterHook.IsMouseCaptured) {
                SelectionRect.Height = Math.Max(MinimumRecLength, _EndY - _StartY);
            }

            Mouse.Capture(BottomCenterHook);
            e.Handled = true;
        }

        #endregion Bottom Center Hook Handlers
        
        #region Private Methods

        /// <summary>
        /// Exit the screen shot manager when ESC is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnKeyPressed(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape) {
                this.Close();
            }
        }

        private void OnHookMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) {
                _IsResizing = true;
                _PastResizeThreshold = false;
                Mouse.Capture(sender as IInputElement);
            }
            e.Handled = true;
        }

        private bool IsPastResizeThreshold()
        {
            if (Math.Abs(_EndX - _StartX) > SystemParameters.MinimumHorizontalDragDistance
                || Math.Abs(_EndY - _StartY) > SystemParameters.MinimumVerticalDragDistance) {
                return true;
            }

            return false;
        }

        #endregion Private Methods
        
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion INotifyPropertyChanged
       
        #region Private Fields

        private double _StartX;
        private double _StartY;
        private double _EndX;
        private double _EndY;
        private bool _IsResizing = false;
        private bool _PastResizeThreshold = false;
        private ImageSource _SelectionImageSource;

        #endregion Private Fields
    }
}
