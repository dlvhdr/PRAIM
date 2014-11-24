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

namespace PRAIM.SnapshotManager
{
    /// <summary>
    /// Interaction logic for SnapshotManager.xaml
    /// </summary>
    public partial class SnapshotManagerWindow : Window, INotifyPropertyChanged
    {
        #region Constants

        public static double MinimumRecLength = 2;
        public static double MinimumHookProximity = 3;

        #endregion Constants

        #region Public Properties

        public CroppedBitmap CroppedImage { get; set; }

        public Rect OuterRect
        {
            get { return new Rect(0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height); }
        }

        public Rect InnerRect
        {
            get { return _InnerRect; }
            set
            {
                _InnerRect = value;
                NotifyPropertyChanged("InnerRect");
            }
        }

        public double RectLeftPos
        {
            get { return _RectLeftPos; }
            set
            {
                if (_RectLeftPos != value) {
                    _RectLeftPos = value;
                    UpdateInnerRect();
                    NotifyPropertyChanged("RectLeftPos");
                }
            }
        }

        private void UpdateInnerRect()
        {
            InnerRect = new Rect(RectLeftPos, RectTopPos, RectWidth, RectHeight);
        }

        public double RectTopPos
        {
            get { return _RectTopPos;  }
            set
            {
                if (_RectTopPos != value) {
                    _RectTopPos = value;
                    UpdateInnerRect();
                    NotifyPropertyChanged("RectTopPos");
                }
            }
        }

        public double RectWidth
        {
            get { return _RectWidth; }
            set
            {
                if (_RectWidth != value && !Utilities.Equal(value, 0)) {
                    _RectWidth = value;
                    UpdateInnerRect();
                    NotifyPropertyChanged("RectWidth");
                }
            }
        }

        public double RectHeight
        {
            get { return _RectHeight; }
            set
            {
                if (_RectHeight != value && !Utilities.Equal(value, 0)) {
                    _RectHeight = value;
                    UpdateInnerRect();
                    NotifyPropertyChanged("RectHeight");
                }
            }
        }

        public bool IsResizing
        {
            get
            {
                return _IsResizing;
            }
            set
            {
                if (_IsResizing != value) {
                    _IsResizing = value;
                    NotifyPropertyChanged("IsResizing");
                }
            }
        }
        
        #endregion Public Properties

        /// <summary>
        /// Constructor
        /// </summary>
        public SnapshotManagerWindow()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
        }

        #region Window Mouse Handlers

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) {
                Mouse.Capture(this);
                IsResizing = true;
                _PastResizeThreshold = false;
                _StartX = e.GetPosition(MainCanvas).X;
                _StartY = e.GetPosition(MainCanvas).Y;
            }
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!IsResizing || !this.IsMouseCaptured) return;

            UpdateEndPos(e);

            UpdatePastResizeThreshold();
            if (_PastResizeThreshold) {
                RectLeftPos = _StartX;
                RectTopPos = _StartY;
                SelectionRect.Visibility = Visibility.Visible;
            }

            if (IsResizing && _PastResizeThreshold) {
                this.MouseMove -= OnMouseMove;
                Mouse.Capture(BottomRightHook);
                return;
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            IsResizing = false;
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

            if (new_left + RectWidth > MainCanvas.Width) {
                new_left = MainCanvas.Width - RectWidth;
            }

            if (MainCanvas != null && new_top + RectHeight > MainCanvas.Height) {
                new_top = MainCanvas.Height - RectHeight;
            }

            _StartX = new_left;
            _StartY = new_top;
            RectLeftPos = _StartX;
            RectTopPos = _StartY;
        }

        #endregion Move Rectangle Handlers

        #region Bottom Right Hook Handlers

        private void BottomRightMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!IsResizing) return;

            UpdateEndPos(e);
            UpdatePastResizeThreshold();

            //-------------------------------------
            // Give mouse control to UpperLeftHook
            //-------------------------------------
            if (_PastResizeThreshold) {
                if (RectWidth < MinimumHookProximity && RectHeight < MinimumHookProximity &&
                    _EndX < _StartX && _EndY < _StartY) {
                    Mouse.Capture(UpperLeftHook);
                    return;
                }
                if (RectWidth > MinimumHookProximity && RectHeight < MinimumHookProximity
                    && _EndY < _StartY) {
                    Mouse.Capture(UpperRightHook);
                    return;
                }
                if (RectWidth < MinimumHookProximity && RectHeight > MinimumHookProximity
                    && _EndX < _StartX) {
                    Mouse.Capture(BottomLeftHook);
                    return;
                }
            }

            if (IsResizing && _PastResizeThreshold && BottomRightHook.IsMouseCaptured) {
                RectWidth = Math.Max(MinimumRecLength, _EndX - _StartX);
                RectHeight = Math.Max(MinimumRecLength, _EndY - _StartY);
            }

            Mouse.Capture(BottomRightHook);
            e.Handled = true;
        }

        #endregion Bottom Right Hook Handlers

        #region Upper Left Hook Handlers

        private void UpperLeftMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!IsResizing) return;

            UpdateEndPos(e);
            UpdatePastResizeThreshold();

            //----------------------------------------
            // Give mouse control to BottomRightHook
            //----------------------------------------
            if (_PastResizeThreshold) {
                if (RectWidth < MinimumHookProximity && RectHeight < MinimumHookProximity &&
                    _EndX > _StartX && _EndY > _StartY) {
                    Mouse.Capture(BottomRightHook);
                    return;
                }
                if (RectWidth > MinimumHookProximity && RectHeight < MinimumHookProximity
                    && _EndY > _StartY) {
                    Mouse.Capture(BottomLeftHook);
                    return;
                }
                if (RectWidth < MinimumHookProximity && RectHeight > MinimumHookProximity
                    && _EndX > _StartX) {
                    Mouse.Capture(UpperRightHook);
                    return;
                }
            }

            if (IsResizing && _PastResizeThreshold && UpperLeftHook.IsMouseCaptured) {
                //Set width and height
                double old_width = RectWidth;
                double old_height = RectHeight;

                RectWidth = Math.Max(MinimumRecLength, RectWidth - (_EndX - _StartX));
                RectHeight = Math.Max(MinimumRecLength, RectHeight - (_EndY - _StartY));

                //Set Canvas.Left and Canvas.Top
                if (_EndX < _StartX || !Utilities.Equal(RectWidth, MinimumRecLength)) {
                    _StartX = _EndX;
                } else {
                    _StartX = _StartX + old_width - RectWidth;
                }

                if (_EndY < _StartY || !Utilities.Equal(RectHeight, MinimumRecLength)) {
                    _StartY = _EndY;
                } else {
                    _StartY = _StartY + old_height - RectHeight;
                }

                RectLeftPos = _StartX;
                RectTopPos = _StartY;
            }

            Mouse.Capture(UpperLeftHook);
            e.Handled = true;
        }

        #endregion Upper Left Hook Handlers

        #region Upper Right Hook Handlers

        private void UpperRightMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_IsResizing) return;

            UpdateEndPos(e);
            UpdatePastResizeThreshold();

            //-----------------------------------------------
            // Check if need to give control to another hook
            //-----------------------------------------------
            if (_PastResizeThreshold) {
                if (RectWidth < MinimumHookProximity && RectHeight < MinimumHookProximity
                    && _EndX < _StartX && _EndY > _StartY) {
                    Mouse.Capture(BottomLeftHook);
                    return;
                }
                if (RectWidth > MinimumHookProximity && RectHeight < MinimumHookProximity
                    && _EndY > _StartY) {
                    Mouse.Capture(BottomRightHook);
                    return;
                }
                if (RectWidth < MinimumHookProximity && RectHeight > MinimumHookProximity &&
                    _EndX < _StartX) {
                    Mouse.Capture(UpperLeftHook);
                    return;
                }
            }

            if (IsResizing && _PastResizeThreshold && UpperRightHook.IsMouseCaptured) {
                double old_height = RectHeight;
                
                RectWidth = Math.Max(MinimumRecLength, _EndX - _StartX);
                RectHeight = Math.Max(MinimumRecLength, RectHeight - (_EndY - _StartY));

                if (_EndY < _StartY || !Utilities.Equal(RectHeight, MinimumRecLength)) {
                    _StartY = _EndY;
                } else {
                    _StartY = _StartY + old_height - RectHeight;
                }

                RectTopPos = _StartY;
            }

            Mouse.Capture(UpperRightHook);
            e.Handled = true;
        }

        #endregion Upper Right Hook Handlers

        #region Bottom Left Hook Handlers

        private void BottomLeftMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!IsResizing) return;

            UpdateEndPos(e);
            UpdatePastResizeThreshold();

            //-----------------------------------------------
            // Check if need to give control to another hook
            //-----------------------------------------------
            if (_PastResizeThreshold) {
                if (RectWidth < MinimumHookProximity && RectHeight < MinimumHookProximity
                    && _EndX > _StartX && _EndY < _StartY) {
                    Mouse.Capture(UpperRightHook);
                    return;
                }
                if (RectWidth > MinimumHookProximity && RectHeight < MinimumHookProximity
                    && _EndY < _StartY) {
                    Mouse.Capture(UpperLeftHook);
                    return;
                }
                if (RectWidth < MinimumHookProximity && RectHeight > MinimumHookProximity &&
                    _EndX > _StartX) {
                    Mouse.Capture(BottomRightHook);
                    return;
                }
            }

            if (IsResizing && _PastResizeThreshold && BottomLeftHook.IsMouseCaptured) {
                double old_width = RectWidth;

                RectWidth = Math.Max(MinimumRecLength, RectWidth - (_EndX - _StartX));
                RectHeight = Math.Max(MinimumRecLength, _EndY - _StartY);

                //Set Canvas.Left and Canvas.Top
                if (_EndX < _StartX || !Utilities.Equal(RectWidth, MinimumRecLength)) {
                    _StartX = _EndX;
                } else {
                    _StartX = _StartX + old_width - RectWidth;
                }

                RectLeftPos = _StartX;
            }

            Mouse.Capture(BottomLeftHook);
            e.Handled = true;
        }

        #endregion Bottom Left Hook Handlers

        #region Upper Center Hook Handlers

        private void UpperCenterMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!IsResizing) return;

            UpdateEndPos(e);
            UpdatePastResizeThreshold();

            //-----------------------------------------------
            // Check if need to give control to another hook
            //-----------------------------------------------
            if (_PastResizeThreshold) {
                if (RectHeight < MinimumHookProximity && _EndY > _StartY) {
                    Mouse.Capture(BottomCenterHook);
                    return;
                }
            }

            if (IsResizing && _PastResizeThreshold && UpperCenterHook.IsMouseCaptured) {
                double old_height = RectHeight;

                RectHeight = Math.Max(MinimumRecLength, RectHeight - (_EndY - _StartY));

                if (_EndY < _StartY || !Utilities.Equal(RectHeight, MinimumRecLength)) {
                    _StartY = _EndY;
                } else {
                    _StartY = _StartY + old_height - RectHeight;
                }

                RectTopPos = _StartY;
            }

            Mouse.Capture(UpperCenterHook);
            e.Handled = true;
        }

        #endregion Upper Center Hook Handlers

        #region Bottom Center Hook Handlers

        private void BottomCenterMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!IsResizing) return;

            UpdateEndPos(e);
            UpdatePastResizeThreshold();

            //-----------------------------------------------
            // Check if need to give control to another hook
            //-----------------------------------------------
            if (_PastResizeThreshold) {
                if (RectHeight < MinimumHookProximity && _EndY < _StartY) {
                    Mouse.Capture(UpperCenterHook);
                    return;
                }
            }

            if (IsResizing && _PastResizeThreshold && BottomCenterHook.IsMouseCaptured) {
                RectHeight = Math.Max(MinimumRecLength, _EndY - _StartY);
            }

            Mouse.Capture(BottomCenterHook);
            e.Handled = true;
        }

        #endregion Bottom Center Hook Handlers

        #region Left Center Hook Handlers

        private void LeftCenterMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!IsResizing) return;

            UpdateEndPos(e);
            UpdatePastResizeThreshold();

            //-----------------------------------------------
            // Check if need to give control to another hook
            //-----------------------------------------------
            if (_PastResizeThreshold) {
                if (RectWidth < MinimumHookProximity && _EndX > _StartX) {
                    Mouse.Capture(RightCenterHook);
                    return;
                }
            }

            if (IsResizing && _PastResizeThreshold && LeftCenterHook.IsMouseCaptured) {
                //Set width and height
                double old_width = RectWidth;
                RectWidth = Math.Max(MinimumRecLength, RectWidth - (_EndX - _StartX));

                //Set Canvas.Left and Canvas.Top
                if (_EndX < _StartX || !Utilities.Equal(RectWidth, MinimumRecLength)) {
                    _StartX = _EndX;
                } else {
                    _StartX = _StartX + old_width - RectWidth;
                }

                RectLeftPos = _StartX;
            }

            Mouse.Capture(LeftCenterHook);
            e.Handled = true;
        }

        #endregion Left Center Hook Handlers

        #region Right Center Hook Handlers

        private void RightCenterMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!IsResizing) return;

            UpdateEndPos(e);
            UpdatePastResizeThreshold();

            //-----------------------------------------------
            // Check if need to give control to another hook
            //-----------------------------------------------
            if (_PastResizeThreshold) {
                if (RectWidth < MinimumHookProximity && _EndX < _StartX) {
                    Mouse.Capture(LeftCenterHook);
                    return;
                }
            }

            if (IsResizing && _PastResizeThreshold && RightCenterHook.IsMouseCaptured) {
                RectWidth = Math.Max(MinimumRecLength, _EndX - _StartX);
            }

            Mouse.Capture(RightCenterHook);
            e.Handled = true;
        }

        #endregion Right Center Hook Handlers
        
        #region Private Methods

        private void UpdateEndPos(System.Windows.Input.MouseEventArgs e)
        {
            _EndX = e.GetPosition(MainCanvas).X;
            _EndY = e.GetPosition(MainCanvas).Y;
        }

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
                IsResizing = true;
                _PastResizeThreshold = false;
                Mouse.Capture(sender as IInputElement);
            }
            e.Handled = true;
        }

        private void UpdatePastResizeThreshold()
        {
            if (Math.Abs(_EndX - _StartX) > SystemParameters.MinimumHorizontalDragDistance
                || Math.Abs(_EndY - _StartY) > SystemParameters.MinimumVerticalDragDistance) {
                    _PastResizeThreshold = true;
            }
        }
        
        private void OnLoaded(object sender, RoutedEventArgs e)
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

        private void Capture(object sender, RoutedEventArgs e)
        {
            if (SelectionRect.Width < MinimumRecLength || SelectionRect.Height < MinimumRecLength) return;

            var source = (BitmapSource)MainImage.Source;

            var selectionRect = new Rect(SelectionRect.RenderSize);

            var sourceRect = SelectionRect.TransformToVisual(MainImage)
                                          .TransformBounds(selectionRect);

            var xMultiplier = source.PixelWidth / MainImage.ActualWidth;
            var yMultiplier = source.PixelHeight / MainImage.ActualHeight;

            sourceRect.Scale(xMultiplier, yMultiplier);

            if (sourceRect.Height < MinimumRecLength || sourceRect.Width < MinimumRecLength) return;

            CroppedImage = new CroppedBitmap(
                source,
                new Int32Rect(
                    (int)sourceRect.X,
                    (int)sourceRect.Y,
                    (int)sourceRect.Width,
                    (int)sourceRect.Height));
            
            this.Close();
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
        private double _RectLeftPos;
        private double _RectTopPos;
        private double _RectWidth;
        private double _RectHeight;
        private Rect _InnerRect;

        #endregion Private Fields
    }
}
