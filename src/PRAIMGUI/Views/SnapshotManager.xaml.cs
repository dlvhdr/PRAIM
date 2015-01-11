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

        public static double MinimumRecLength = 0.1;
        public static double MinimumHookProximity = 0.2;
        public static readonly double ScreenWidth = Screen.PrimaryScreen.Bounds.Width;
        public static readonly double ScreenHeight = Screen.PrimaryScreen.Bounds.Height;
        public double NegetiveButtonsMargin;

        #endregion Constants

        #region Public Properties

        public bool ButtonsStackVisibility
        {
            get
            {
                return _IsInitialized & !IsResizing;
            }
        }

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
                    UpdateButtonsStackPanel();
                    NotifyPropertyChanged("RectLeftPos");
                }
            }
        }

        public double RectTopPos
        {
            get { return _RectTopPos; }
            set
            {
                if (_RectTopPos != value) {
                    _RectTopPos = value;
                    UpdateInnerRect();
                    UpdateButtonsStackPanel();
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
                    UpdateButtonsStackPanel();
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
                    UpdateButtonsStackPanel();
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
                    NotifyPropertyChanged("ButtonsStackVisibility");
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

        #region General Mouse Handlers

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
                _IsInitialized = true;
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

        /// <summary>
        /// Handler for mouse hook down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHookMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) {
                IsResizing = true;
                _PastResizeThreshold = false;
                Mouse.Capture(sender as IInputElement);
            }
            e.Handled = true;
        }

        #endregion General Mouse Handlers

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

        /// <summary>
        /// Window on loaded handler. Initializes the snapshot manager.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            NegetiveButtonsMargin = -ButtonsStackPanel.Width;

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

        /// <summary>
        /// Update private fields of where the mouse is now
        /// </summary>
        /// <param name="e"></param>
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
                OnCancel(sender, e);
            }
        }

        /// <summary>
        /// Update whether a drag was started
        /// </summary>
        private void UpdatePastResizeThreshold()
        {
            if (Math.Abs(_EndX - _StartX) > SystemParameters.MinimumHorizontalDragDistance
                || Math.Abs(_EndY - _StartY) > SystemParameters.MinimumVerticalDragDistance) {
                _PastResizeThreshold = true;
            }
        }

        /// <summary>
        /// Capture handler - save the selection image to the public property.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Capture(object sender, RoutedEventArgs e)
        {
            this.CroppedImage = GetSelectionImage();
            this.Close();
        }

        private CroppedBitmap GetSelectionImage()
        {
            if (SelectionRect.Width < MinimumRecLength || SelectionRect.Height < MinimumRecLength) return null;

            var source = (BitmapSource)MainImage.Source;

            var selectionRect = new Rect(SelectionRect.RenderSize);

            var sourceRect = SelectionRect.TransformToVisual(MainImage)
                                          .TransformBounds(selectionRect);

            var xMultiplier = source.PixelWidth / MainImage.ActualWidth;
            var yMultiplier = source.PixelHeight / MainImage.ActualHeight;

            sourceRect.Scale(xMultiplier, yMultiplier);

            if (sourceRect.Height < MinimumRecLength || sourceRect.Width < MinimumRecLength) return null;

            return new CroppedBitmap(
                source,
                new Int32Rect(
                    (int)sourceRect.X,
                    (int)sourceRect.Y,
                    (int)sourceRect.Width,
                    (int)sourceRect.Height));
        }

        /// <summary>
        /// Update the buttons position on the canvas (called after resize/move of the selection)
        /// </summary>
        private void UpdateButtonsStackPanel()
        {
            bool exceed_right = RectLeftPos + RectWidth + ButtonsStackPanel.Width > MainCanvas.Width;
            bool exceed_left = RectLeftPos - ButtonsStackPanel.Width < 0;

            if (exceed_left && exceed_right) {
                Canvas.SetLeft(ButtonsStackPanel, RectLeftPos + RectWidth + NegetiveButtonsMargin);
                Canvas.SetTop(ButtonsStackPanel, Math.Max(0, RectTopPos + RectHeight - ButtonsStackPanel.Height));
            } else if (exceed_right) {
                Canvas.SetLeft(ButtonsStackPanel, RectLeftPos + NegetiveButtonsMargin);
                Canvas.SetTop(ButtonsStackPanel, Math.Max(0, RectTopPos + RectHeight - ButtonsStackPanel.Height));
            } else {
                Canvas.SetLeft(ButtonsStackPanel, RectLeftPos + RectWidth);
                Canvas.SetTop(ButtonsStackPanel, Math.Max(0, RectTopPos + RectHeight - ButtonsStackPanel.Height));
            }
        }

        /// <summary>
        /// Update the masking shape (called after resize/move of the selection)
        /// </summary>
        private void UpdateInnerRect()
        {
            InnerRect = new Rect(RectLeftPos, RectTopPos, RectWidth, RectHeight);
        }

        /// <summary>
        /// Cancel the snapshot taking (close the window)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Save selection to image file handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSaveToFile(object sender, RoutedEventArgs e)
        {
            CroppedBitmap image = GetSelectionImage();
            if (image == null) {
                System.Windows.MessageBox.Show("Selection rectangle is too small", "Error", MessageBoxButton.OK);
                return;
            }

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "JPEG (*.jpeg)|*.jpeg|PNG (*.png)|*.png|BMP (*.bmp)|*.bmp";

            DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK) {
                BitmapEncoder encoder;

                if (dlg.FileName.EndsWith("jpeg")) {
                    encoder = new JpegBitmapEncoder();
                } else if (dlg.FileName.EndsWith("bmp")) {
                    encoder = new BmpBitmapEncoder();
                } else {
                    encoder = new PngBitmapEncoder();
                }

                encoder.Frames.Add(BitmapFrame.Create(image));
                using (FileStream fs = new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write)) {
                    encoder.Save(fs);
                }

                this.Close();
            }
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
        private bool _IsResizing;
        private bool _IsInitialized;
        private bool _PastResizeThreshold;
        private double _RectLeftPos;
        private double _RectTopPos;
        private double _RectWidth;
        private double _RectHeight;
        private Rect _InnerRect;

        #endregion Private Fields
    }
}
