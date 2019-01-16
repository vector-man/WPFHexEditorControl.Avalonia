//////////////////////////////////////////////
// Apache 2.0  - 2018
// Author : Janus Tida
// Modified by : Derek Tremblay
//////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using WpfHexaEditor.Core;
using WpfHexaEditor.Core.Bytes;
using WpfHexaEditor.Core.Interfaces;
using WpfHexaEditor.Core.Transactions;
using WpfHexaEditor.Layers;

namespace WpfHexaEditor
{
    /// <summary>
    /// Interaction logic for DrawedHexEditor.xaml
    /// </summary>
    public partial class DrawedHexEditor
    {
        #region DevBranch

        public DrawedHexEditor()
        {
            InitializeComponent();
            
            InitilizeEvents();
            InitializeBindings();
            UpdateCellPaddings();
            UpdateCellMargins();

            FontSize = 16;

            //FontFamily = new FontFamily("Arial New");
            //FontFamily = new FontFamily("Microsoft YaHei");
            //FontFamily = new FontFamily("Courier New");
            FontFamily = new FontFamily("Lucida Bright");
            DataVisualType = DataVisualType.Decimal;
        }
        
        //Cuz xaml designer's didn't support valuetuple,events subscribing will be executed in code-behind.
        private void InitilizeEvents()
        {
            this.SizeChanged += delegate { UpdateContent(); };
            void initialCellsLayer(ICellsLayer layer)
            {
                layer.MouseLeftDownOnCell += DataLayer_MouseLeftDownOnCell;
                layer.MouseLeftUpOnCell += DataLayer_MouseLeftUpOnCell;
                layer.MouseMoveOnCell += DataLayer_MouseMoveOnCell;
                layer.MouseRightDownOnCell += DataLayer_MouseRightDownOnCell;
            }

            initialCellsLayer(HexDataLayer);
            initialCellsLayer(StringDataLayer);
            
            InitializeTooltipEvents();
        }
        
        /// <summary>
        /// To reduce the memory consuming,avoid recreating the same binding objects;
        /// </summary>
        private void InitializeBindings() {
            InitializeFontBindings();
            InitializeFixedSeperatorsBindings();
            InitializeEncodingBinding();
        }

        Binding GetBindingToSelf(string propName) {
            var binding = new Binding() {
                Path = new PropertyPath(propName),
                Source = this
            };
            return binding;
        }

        private void InitializeFontBindings() {
            var fontSizeBinding = GetBindingToSelf(nameof(FontSize));
            var fontFamilyBinding = GetBindingToSelf(nameof(FontFamily));
            var fontWeightBinding = GetBindingToSelf(nameof(FontWeight));

            void SetFontControlBindings(IEnumerable< FontControlBase> fontControls) {
                foreach (var fontCtrl in fontControls) {
                    fontCtrl.SetBinding(FontControlBase.FontSizeProperty, fontSizeBinding);
                    fontCtrl.SetBinding(FontControlBase.FontFamilyProperty, fontFamilyBinding);
                    fontCtrl.SetBinding(FontControlBase.FontWeightProperty, fontWeightBinding);
                }
            };

            IEnumerable<FontControlBase> GetFontControls() {
                yield return HexDataLayer;
                yield return StringDataLayer;
                yield return ColumnsOffsetInfoLayer;
                yield return LinesOffsetInfoLayer;
            };

            SetFontControlBindings(GetFontControls());
        }

        /// <summary>
        /// Save the view byte buffer as a field. 
        /// To save the time when Scolling i do not building them every time when scolling.
        /// </summary>
        private byte[] _viewBuffer;

        private byte[] _viewBuffer2;

        //To avoid resigning buffer everytime and notify the UI to rerender,
        //we're gonna switch from one to another while refreshing.
        private byte[] _realViewBuffer;

        //To avoid wrong mousemove event;
        private bool _contextMenuShowing;

        private int MaxVisibleLength
        {
            get
            {
                if (Stream == null)
                    return 0;

                return (int) Math.Min(HexDataLayer.AvailableRowsCount * BytePerLine,
                    Stream.Length - Position / BytePerLine * BytePerLine);
            }
        }

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            base.OnContextMenuOpening(e);
            _contextMenuShowing = true;
        }

        protected override void OnContextMenuClosing(ContextMenuEventArgs e)
        {
            base.OnContextMenuClosing(e);
            _contextMenuShowing = false;
#if DEBUG
            //ss++;
#endif
        }

#if DEBUG
        //private long ss = 0;
#endif

        /// <summary>
        /// Obtain the max line for verticalscrollbar
        /// </summary>
        private long MaxLine => Stream.Length / BytePerLine;

#if DEBUG
        private readonly Stopwatch _watch = new Stopwatch();
#endif
        

        //To avoid endless looping of ScrollBar_ValueChanged and Position_PropertyChanged.
        private bool _scrollBarValueUpdating;

        //Remember the position in which the mouse last clicked.
        private long? _lastMouseDownPosition;

        #region EventSubscriber handlers;

        private void Control_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Stream == null) return;

            if (e.Delta > 0) //UP
                VerticalScrollBar.Value -= e.Delta / 120 * (int) MouseWheelSpeed;

            if (e.Delta < 0) //Down
                VerticalScrollBar.Value += e.Delta / 120 * -(int) MouseWheelSpeed;
        }

        private void DataLayer_MouseLeftDownOnCell(object sender, (int cellIndex, MouseButtonEventArgs e) arg)
        {
            if (arg.cellIndex >= MaxVisibleLength)
                return;

            var clickPosition = Position / BytePerLine * BytePerLine + arg.cellIndex;
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                long oldStart = -1;

                if (SelectionStart != -1)
                    oldStart = SelectionStart;

                if (FocusPosition != -1)
                    oldStart = FocusPosition;

                if (oldStart != -1)
                {
                    SelectionStart = Math.Min(oldStart, clickPosition);
                    SelectionLength = Math.Abs(oldStart - clickPosition) + 1;
                }
            }

            _lastMouseDownPosition = clickPosition;

            FocusPosition = _lastMouseDownPosition.Value;
            this.Focus();
        }

        private void DataLayer_MouseRightDownOnCell(object sender, (int cellIndex, MouseButtonEventArgs e) arg) => 
            DataLayer_MouseLeftDownOnCell(sender, arg);

        private void DataLayer_MouseMoveOnCell(object sender, (int cellIndex, MouseEventArgs e) arg)
        {
            if (arg.e.LeftButton != MouseButtonState.Pressed)
                return;

            if (_contextMenuShowing)
                return;

#if DEBUG
            //arg.cellIndex = 15;
            //_lastMouseDownPosition = 0;
#endif
            //Operate Selection;
            if (_lastMouseDownPosition == null)
                return;

            var cellPosition = Position / BytePerLine * BytePerLine + arg.cellIndex;
            if (_lastMouseDownPosition.Value == cellPosition)
                return;

            var length = Math.Abs(cellPosition - _lastMouseDownPosition.Value) + 1;
            SelectionStart = Math.Min(cellPosition, _lastMouseDownPosition.Value);
            SelectionLength = length;
        }

        private void DataLayer_MouseLeftUpOnCell(object sender, (int cellIndex, MouseButtonEventArgs e) arg) => 
            _lastMouseDownPosition = null;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (Stream == null)
                return;

            if (FocusPosition == -1)
                return;

            if (KeyValidator.IsArrowKey(e.Key))
            {
                OnArrowKeyDown(e);
                e.Handled = true;
            }

        }

        //Deal with operation while arrow key is pressed.
        private void OnArrowKeyDown(KeyEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (!KeyValidator.IsArrowKey(e.Key))
                throw new ArgumentException($"The key '{e.Key}' is not a arrow key.");

            if (Stream == null)
                return;

            if (FocusPosition == -1)
                return;

            //Update Selection if shift key is pressed;
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                long vectorEnd = -1;
                switch (e.Key)
                {
                    case Key.Left:
                        if (FocusPosition > 0)
                        {
                            vectorEnd = FocusPosition - 1;
                        }

                        break;
                    case Key.Up:
                        if (FocusPosition >= BytePerLine)
                        {
                            vectorEnd = FocusPosition - BytePerLine;
                        }

                        break;
                    case Key.Right:
                        if (FocusPosition + 1 < Stream.Length)
                        {
                            vectorEnd = FocusPosition + 1;
                        }

                        break;
                    case Key.Down:
                        if (FocusPosition + BytePerLine < Stream.Length)
                        {
                            vectorEnd = FocusPosition + BytePerLine;
                        }

                        break;
                }

                if (vectorEnd != -1)
                {
                    //BackWard;
                    if (vectorEnd < FocusPosition)
                    {
                        if (FocusPosition == SelectionStart)
                        {
                            SelectionLength += SelectionStart - vectorEnd;
                            SelectionStart = vectorEnd;
                        }
                        else if (FocusPosition == SelectionStart + SelectionLength - 1 && 
                                 SelectionLength >= FocusPosition - vectorEnd + 1)
                        {
                            SelectionLength -= FocusPosition - vectorEnd;
                        }
                        else
                        {
                            SelectionStart = vectorEnd;
                            SelectionLength = FocusPosition - vectorEnd + 1;
                        }
                    }
                    //Forward;
                    else if (vectorEnd > FocusPosition)
                    {
                        if (FocusPosition == SelectionStart + SelectionLength - 1)
                        {
                            SelectionLength += vectorEnd - FocusPosition;
                        }
                        else if (FocusPosition == SelectionStart && 
                                 SelectionLength >= vectorEnd - FocusPosition + 1)
                        {
                            SelectionLength -= vectorEnd - SelectionStart;
                            SelectionStart = vectorEnd;
                        }
                        else
                        {
                            SelectionStart = FocusPosition;
                            SelectionLength = vectorEnd - FocusPosition + 1;
                        }
                    }
                }

            }

            //Updte FocusSelection;
            switch (e.Key)
            {
                case Key.Left:
                    if (FocusPosition > 0)
                        FocusPosition--;

                    break;
                case Key.Up:
                    if (FocusPosition >= BytePerLine)
                        FocusPosition -= BytePerLine;

                    break;
                case Key.Right:
                    if (FocusPosition + 1 < Stream.Length)
                        FocusPosition++;

                    break;
                case Key.Down:
                    if (FocusPosition + BytePerLine < Stream.Length)
                        FocusPosition += BytePerLine;

                    break;
                default:
                    return;
            }

            //Update scrolling(if needed);
            var firstVisiblePosition = Position / BytePerLine * BytePerLine;
            var lastVisiblePosition = firstVisiblePosition + MaxVisibleLength - 1;
            if (FocusPosition < firstVisiblePosition)
            {
                Position -= BytePerLine;
            }
            else if (FocusPosition > lastVisiblePosition)
            {
                Position += BytePerLine;
            }

        }

        #endregion


        /// <summary>
        /// This method won't be while scrolling,but only when stream is opened or closed,byteperline changed(UpdateInfo);
        /// </summary>
        private void UpdateInfoes()
        {
            UpdateScrollBarInfo();
            UpdateColumnHeaderInfo();
            UpdateOffsetLinesInfo();

            //Position PropertyChangedCallBack will update the content;
            Position = 0;

            //Restore/Update Focus Position;
            if (FocusPosition >= (Stream?.Length ?? 0))
                FocusPosition = -1;

            //RestoreSelection;
            SelectionStart = -1;
            SelectionLength = 0;
            
        }

        #region These methods won't be invoked everytime scrolling.but only when stream is opened or closed,byteperline changed(UpdateInfo).

        /// <summary>
        /// Update vertical scrollbar with file info
        /// </summary>
        private void UpdateScrollBarInfo()
        {
            VerticalScrollBar.Visibility = Visibility.Collapsed;

            if (Stream == null) return;

            VerticalScrollBar.Visibility = Visibility.Visible;
            VerticalScrollBar.SmallChange = 1;
            //VerticalScrollBar.LargeChange = ScrollLargeChange;
            VerticalScrollBar.Maximum = MaxLine - 1;
        }

        /// <summary>
        /// Update the position info panel at top of the control
        /// </summary>
        private void UpdateColumnHeaderInfo()
        {
            ColumnsOffsetInfoLayer.StartStepIndex = 0;
            ColumnsOffsetInfoLayer.StepsCount = BytePerLine;
        }

        /// <summary>
        /// Update the position info panel at left of the control,see this won't change the content of the OffsetLines;
        /// </summary>
        private void UpdateOffsetLinesInfo()
        {
            if (Stream == null)
                return;

            LinesOffsetInfoLayer.DataVisualType = DataVisualType;
            LinesOffsetInfoLayer.StepLength = BytePerLine;
            
            LinesOffsetInfoLayer.SavedBits = DataVisualType == DataVisualType.Hexadecimal
                ? ByteConverters.GetHexBits(Stream.Length)
                : ByteConverters.GetDecimalBits(Stream.Length);
        }

        //This will affect how a linesinfo and columnsinfo index change.
        public DataVisualType DataVisualType
        {
            get => (DataVisualType) GetValue(DataVisualTypeProperty);
            set => SetValue(DataVisualTypeProperty, value);
        }

        // Using a DependencyProperty as the backing store for DataVisualType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataVisualTypeProperty =
            DependencyProperty.Register(nameof(DataVisualType),
                typeof(DataVisualType), typeof(DrawedHexEditor),
                new PropertyMetadata(DataVisualType.Hexadecimal, DataVisualTypeProperty_Changed));

        private static void DataVisualTypeProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is DrawedHexEditor ctrl))
            {
                return;
            }

            ctrl.LinesOffsetInfoLayer.DataVisualType = (DataVisualType) e.NewValue;
            ctrl.ColumnsOffsetInfoLayer.DataVisualType = (DataVisualType) e.NewValue;
            ctrl.UpdateContent();
        }

        #endregion


        public long Position
        {
            get => (long) GetValue(PositionProperty); 
            set => SetValue(PositionProperty, value);
        }

        // Using a DependencyProperty as the backing store for Position.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register(nameof(Position), typeof(long), typeof(DrawedHexEditor),
                new FrameworkPropertyMetadata(-1L, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PositionProperty_Changed));

        private static void PositionProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (!(d is DrawedHexEditor ctrl)) return;
#if DEBUG
            ctrl._watch.Restart();
#endif
            ctrl.UpdateContent();
#if DEBUG
            ctrl._watch.Stop();
            Debug.Print($"REFRESH TIME: {ctrl._watch.ElapsedMilliseconds} ms");
#endif
            
        }
        

        /**/
        
        /// <summary>
        /// Refresh currentview of hexeditor
        /// </summary>
        public void UpdateContent()
        {
            UpdateOffsetLinesContent();
            UpdateScrollBarContent();
            UpdateBackgroundBlocks();
            UpdateForegroundBlocks();
            UpdateDataContent();

            UpdateBlockLines();
        }



        #region  These methods will be invoked every time scrolling the content(scroll or position changed)(Refreshview calling);

        ///<see cref="UpdateContent"/>
        /// <summary>
        /// Update the hex and string layer you current view;
        /// </summary>
        private void UpdateDataContent()
        {
            if (!(Stream?.CanRead ?? false))
            {
                HexDataLayer.Data = null;
                StringDataLayer.Data = null;
                return;
            }

            Stream.Position = Position / BytePerLine * BytePerLine;
            HexDataLayer.DataOffsetInOriginalStream = Position / BytePerLine * BytePerLine;
            StringDataLayer.DataOffsetInOriginalStream = Position / BytePerLine * BytePerLine;

            if (_viewBuffer == null || _viewBuffer.Length != MaxVisibleLength)
                _viewBuffer = new byte[MaxVisibleLength];

            if (_viewBuffer2 == null || _viewBuffer2.Length != MaxVisibleLength)
                _viewBuffer2 = new byte[MaxVisibleLength];

            _realViewBuffer = _realViewBuffer == _viewBuffer ? _viewBuffer2 : _viewBuffer;

            Stream.Read(_realViewBuffer, 0, MaxVisibleLength);

            HexDataLayer.Data = _realViewBuffer;
            StringDataLayer.Data = _realViewBuffer;
        }

        private void UpdateOffsetLinesContent()
        {
            if (Stream == null)
            {
                LinesOffsetInfoLayer.StartStepIndex = 0;
                LinesOffsetInfoLayer.StepsCount = 0;
                return;
            }

            LinesOffsetInfoLayer.StartStepIndex = Position / BytePerLine * BytePerLine;
            LinesOffsetInfoLayer.StepsCount =
                Math.Min(HexDataLayer.AvailableRowsCount,
                    MaxVisibleLength / BytePerLine + (MaxVisibleLength % BytePerLine != 0 ? 1 : 0));
        }

        private void UpdateScrollBarContent()
        {
            if (_scrollBarValueUpdating) return;

            _scrollBarValueUpdating = true;
            VerticalScrollBar.Value = Position / BytePerLine;
            _scrollBarValueUpdating = false;
        }
        
        #endregion
        
        private void VerticalScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_scrollBarValueUpdating)
                return;

            _scrollBarValueUpdating = true;
            Position = (long) e.NewValue * BytePerLine;
            _scrollBarValueUpdating = false;
        }

        private void BottomRectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void BottomRectangle_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void BottomRectangle_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        private void TopRectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void TopRectangle_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void TopRectangle_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        #endregion

        #region DependencyPorperties

        #region BytePerLine property/methods

        /// <summary>
        /// Get or set the number of byte are show in control
        /// </summary>
        public int BytePerLine
        {
            get => (int) GetValue(BytePerLineProperty);
            set => SetValue(BytePerLineProperty, value);
        }

        public static readonly DependencyProperty BytePerLineProperty =
            DependencyProperty.Register("BytePerLine", typeof(int), typeof(DrawedHexEditor),
                new PropertyMetadata(16, BytePerLine_PropertyChanged));

        private static void BytePerLine_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is DrawedHexEditor ctrl) || e.NewValue == e.OldValue) return;
            ctrl.HexDataLayer.BytePerLine = (int) e.NewValue;
            ctrl.StringDataLayer.BytePerLine = (int) e.NewValue;

            ctrl.UpdateInfoes();
            ctrl.UpdateContent();
        }

        #endregion

        

        public MouseWheelSpeed MouseWheelSpeed
        {
            get => (MouseWheelSpeed) GetValue(MouseWheelSpeedProperty);
            set => SetValue(MouseWheelSpeedProperty, value);
        }

        // Using a DependencyProperty as the backing store for MouseWheelSpeed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseWheelSpeedProperty =
            DependencyProperty.Register(nameof(MouseWheelSpeed), typeof(MouseWheelSpeed), typeof(DrawedHexEditor),
                new PropertyMetadata(MouseWheelSpeed.Normal));


        /// <summary>
        /// Set the Stream are used by ByteProvider
        /// </summary>
        public Stream Stream
        {
            get => (Stream) GetValue(StreamProperty);
            set => SetValue(StreamProperty, value);
        }

        // Using a DependencyProperty as the backing store for Stream.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StreamProperty =
            DependencyProperty.Register(nameof(Stream), typeof(Stream), typeof(DrawedHexEditor),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits,
                    Stream_PropertyChanged));

        private static void Stream_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is DrawedHexEditor ctrl)) return;
            //These methods won't be invoked everytime scrolling.but only when stream is opened or closed.
            ctrl.UpdateInfoes();
            ctrl.UpdateContent();
        }

        #endregion
    }

    /// <summary>
    /// Part of Margin and Padding;
    /// </summary>
    public partial class DrawedHexEditor {

        public Thickness CellMargin {
            get => (Thickness)GetValue(CellMarginProperty);
            set => SetValue(CellMarginProperty, value);
        }

        // Using a DependencyProperty as the backing store for CellMargion.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CellMarginProperty =
            DependencyProperty.Register(nameof(CellMargin), typeof(Thickness), typeof(DrawedHexEditor),
                new PropertyMetadata(new Thickness(0,1,0,1), CellMargionProperty_Changed));

        private static void CellMargionProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (!(d is DrawedHexEditor ctrl)) return;

            ctrl.UpdateCellMargins();
        }

        private void UpdateCellMargins() {
            var cellMargin = CellMargin;
            HexDataLayer.CellMargin = cellMargin;
            StringDataLayer.CellMargin = cellMargin;
            LinesOffsetInfoLayer.CellMargin = new Thickness(8, cellMargin.Top, 8, cellMargin.Bottom);
            ColumnsOffsetInfoLayer.CellMargin = new Thickness(cellMargin.Left, 0, cellMargin.Right, 0);
        }

        public Thickness CellPadding {
            get => (Thickness)GetValue(CellPaddingProperty);
            set => SetValue(CellPaddingProperty, value);
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CellPaddingProperty =
            DependencyProperty.Register(nameof(CellPadding), typeof(Thickness), typeof(DrawedHexEditor),
                new PropertyMetadata(new Thickness(4), CellPaddingProperty_Changed));

        private static void CellPaddingProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (!(d is DrawedHexEditor ctrl)) return;

            ctrl.UpdateCellPaddings();
        }

        private void UpdateCellPaddings() {
            var cellPadding = CellPadding;
            HexDataLayer.CellPadding = cellPadding;
            StringDataLayer.CellPadding = cellPadding;
            LinesOffsetInfoLayer.CellPadding = new Thickness(0, cellPadding.Top, 0, cellPadding.Bottom);
            ColumnsOffsetInfoLayer.CellPadding = new Thickness(cellPadding.Left, 0, cellPadding.Right, 0);
        }
    }

    /// <summary>
        /// BackgroundBlocks Part;
    /// </summary>
    public partial class DrawedHexEditor {
        private readonly List<IBrushBlock> _dataBackgroundBlocks =
            new List<IBrushBlock>();

        /// <summary>
        /// Brush block for Selection;
        /// </summary>
        private readonly BrushBlock _selectionBrushBlock = new BrushBlock();

        /// <summary>
        /// Brush block for focus state;
        /// </summary>
        private readonly BrushBlock _focusBrushBlock = new BrushBlock();

        public IEnumerable<IBrushBlock> CustomBackgroundBlocks {
            get => (IEnumerable<IBrushBlock>)GetValue(
                CustomBackgroundBlocksProperty);
            set => SetValue(CustomBackgroundBlocksProperty, value);
        }

        // Using a DependencyProperty as the backing store for CustomBackgroundBlocks.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CustomBackgroundBlocksProperty =
            DependencyProperty.Register(nameof(CustomBackgroundBlocks),
                typeof(IEnumerable<IBrushBlock>),
                typeof(DrawedHexEditor));
        
        public void UpdateBackgroundBlocks() {
            //ClearBackgroundBlocks;
            HexDataLayer.BackgroundBlocks = null;
            StringDataLayer.BackgroundBlocks = null;
            _dataBackgroundBlocks.Clear();

            AddCustomBackgroundBlocks();
            AddSelectionBackgroundBlocks();
            AddFocusPositionBackgroundBlock();

            HexDataLayer.BackgroundBlocks = _dataBackgroundBlocks;
            StringDataLayer.BackgroundBlocks = _dataBackgroundBlocks;
        }

        private void AddCustomBackgroundBlocks() {
            if (CustomBackgroundBlocks == null) return;

            foreach (var block in CustomBackgroundBlocks)
                AddBackgroundBlock(block);
        }

        private void AddBackgroundBlock(IBrushBlock brushBlock) {
            if (Stream == null)
                return;

            //Check whether the backgroundblock is in the sight;
            if (!(brushBlock.StartOffset + brushBlock.Length >= Position && brushBlock.StartOffset < Position + MaxVisibleLength))
                return;
            
            var maxIndex = Math.Max(brushBlock.StartOffset, Position);
            var minEnd = Math.Min(brushBlock.StartOffset + brushBlock.Length, Position + MaxVisibleLength);

            _dataBackgroundBlocks.Add(new BrushBlock { StartOffset = maxIndex - Position, Length = minEnd - maxIndex, Brush = brushBlock.Brush });
        }

        private void AddSelectionBackgroundBlocks() {
            _selectionBrushBlock.StartOffset = SelectionStart;
            _selectionBrushBlock.Length = SelectionLength;
            _selectionBrushBlock.Brush = SelectionBrush;

            AddBackgroundBlock(_selectionBrushBlock);
        }
            

        private void AddFocusPositionBackgroundBlock() {
            if (FocusPosition >= 0) {
                _focusBrushBlock.StartOffset = FocusPosition;
                _focusBrushBlock.Length = 1;
                _focusBrushBlock.Brush = FocusBrush;
                AddBackgroundBlock(_focusBrushBlock);
            }
        }
        
    }

    /// <summary>
    /// ForegroundBlockParts;
    /// </summary>
    public partial class DrawedHexEditor {
        private readonly List<IBrushBlock> _dataForegroundBlocks =
            new List<IBrushBlock>();
        public IEnumerable<IBrushBlock> CustomForegroundBlocks {
            get { return (IEnumerable<IBrushBlock>)GetValue(CustomForegroundBlocksProperty); }
            set { SetValue(CustomForegroundBlocksProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CustomForegroundBlocks.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CustomForegroundBlocksProperty =
            DependencyProperty.Register(nameof(CustomForegroundBlocks), typeof(IEnumerable<IBrushBlock>), typeof(DrawedHexEditor));
        
        private void UpdateForegroundBlocks() {
            HexDataLayer.ForegroundBlocks = null;
            StringDataLayer.ForegroundBlocks = null;
            
            _dataForegroundBlocks.Clear();

            AddCustomForegroundBlocks();
            AddSelectionForegroundBlocks();
            AddFocusForegroundBlock();

            HexDataLayer.ForegroundBlocks = _dataForegroundBlocks;
            StringDataLayer.ForegroundBlocks = _dataForegroundBlocks;
        }

        private void AddCustomForegroundBlocks() {
            if (CustomForegroundBlocks == null) return;

            foreach (var block in CustomForegroundBlocks)
                AddForegroundBlock(block);
        }

        private void AddFocusForegroundBlock() {
            if(FocusBrush == null) {
                return;
            }

            if (FocusPosition >= 0)
                AddForegroundBlock(new BrushBlock { StartOffset = FocusPosition, Length = 1, Brush = FocusForeground });
        }


        private void AddSelectionForegroundBlocks() {
            if(SelectionForeground == null) {
                return;
            }

            AddForegroundBlock(new BrushBlock { StartOffset = SelectionStart, Length = SelectionLength, Brush = SelectionForeground });
        }
            

        private void AddForegroundBlock(IBrushBlock brushBlock) {
            if (Stream == null)
                return;

            //Check whether the backgroundblock is in visible;
            if (!(brushBlock.StartOffset + brushBlock.Length >= Position && brushBlock.StartOffset < Position + MaxVisibleLength))
                return;

            var maxIndex = Math.Max(brushBlock.StartOffset, Position);
            var minEnd = Math.Min(brushBlock.StartOffset + brushBlock.Length, Position + MaxVisibleLength);

            _dataForegroundBlocks.Add(new BrushBlock { StartOffset = maxIndex - Position, Length = minEnd - maxIndex, Brush = brushBlock.Brush });
        }
    }

    /// <summary>
    /// Selection Part;
    /// </summary>
    public partial class DrawedHexEditor {

        public long SelectionStart {
            get => (long)GetValue(SelectionStartProperty);
            set => SetValue(SelectionStartProperty, value);
        }

        // Using a DependencyProperty as the backing store for SelectionStart.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectionStartProperty =
            DependencyProperty.Register(nameof(SelectionStart), typeof(long), typeof(DrawedHexEditor),
                new FrameworkPropertyMetadata(-1L, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    SelectionStart_PropertyChanged));

        private static void SelectionStart_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (!(d is DrawedHexEditor ctrl))
                return;

            ctrl.UpdateBackgroundBlocks();
            ctrl.UpdateForegroundBlocks();
        }

        public long SelectionLength {
            get => (long)GetValue(SelectionLengthProperty);
            set => SetValue(SelectionLengthProperty, value);
        }


        // Using a DependencyProperty as the backing store for SelectionLength.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectionLengthProperty =
            DependencyProperty.Register(nameof(SelectionLength), typeof(long), typeof(DrawedHexEditor),
                new FrameworkPropertyMetadata(0L, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    SelectionLengthProperty_Changed));

        private static void SelectionLengthProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (!(d is DrawedHexEditor ctrl))
                return;

            ctrl.UpdateBackgroundBlocks();
            ctrl.UpdateForegroundBlocks();
        }

        public Brush SelectionBrush {
            get => (Brush)GetValue(SelectionBrushProperty);
            set => SetValue(SelectionBrushProperty, value);
        }

        // Using a DependencyProperty as the backing store for SelectionBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectionBrushProperty =
            DependencyProperty.Register(nameof(SelectionBrush), typeof(Brush), typeof(DrawedHexEditor),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xff))));



        public Brush SelectionForeground {
            get { return (Brush)GetValue(SelectionForegroundProperty); }
            set { SetValue(SelectionForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectionForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectionForegroundProperty =
            DependencyProperty.Register(nameof(SelectionForeground), typeof(Brush), typeof(DrawedHexEditor),
                new PropertyMetadata(Brushes.Azure));


    }

    /// <summary>
    /// Focus Part;
    /// </summary>
    public partial class DrawedHexEditor {
        public long FocusPosition {
            get => (long)GetValue(FocusPositionProperty);
            set => SetValue(FocusPositionProperty, value);
        }

        // Using a DependencyProperty as the backing store for FocusPosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FocusPositionProperty =
            DependencyProperty.Register(nameof(FocusPosition), typeof(long), typeof(DrawedHexEditor),
                new FrameworkPropertyMetadata(-1L, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    FocusPositionProperty_Changed));

        private static void FocusPositionProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (!(d is DrawedHexEditor ctrl))
                return;

            ctrl.UpdateBackgroundBlocks();
            ctrl.UpdateForegroundBlocks();

            if ((long)e.NewValue == -1) return;

            ctrl.Focusable = true;
        }

        public Brush FocusBrush {
            get => (Brush)GetValue(FocusBrushProperty);
            set => SetValue(FocusBrushProperty, value);
        }

        // Using a DependencyProperty as the backing store for FocusBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FocusBrushProperty =
            DependencyProperty.Register(nameof(FocusBrush), typeof(Brush), typeof(DrawedHexEditor),
                new PropertyMetadata(Brushes.Blue));

        public Brush FocusForeground {
            get { return (Brush)GetValue(FocusForegroundProperty); }
            set { SetValue(FocusForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FocusForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FocusForegroundProperty =
            DependencyProperty.Register(nameof(FocusForeground), typeof(Brush), typeof(DrawedHexEditor),
                new PropertyMetadata(Brushes.White));



        public FocusedPanel FocusedPanel {
            get { return (FocusedPanel)GetValue(FocusedPanelProperty); }
            set { SetValue(FocusedPanelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FocusedPanel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FocusedPanelProperty =
            DependencyProperty.Register(nameof(FocusedPanel), typeof(FocusedPanel), typeof(DrawedHexEditor), new PropertyMetadata(FocusedPanel.Hex));



        public HexFocusedChar HexFocusedChar {
            get { return (HexFocusedChar)GetValue(HexFocusedCharProperty); }
            set { SetValue(HexFocusedCharProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HexFocusedChar.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HexFocusedCharProperty =
            DependencyProperty.Register(nameof(HexFocusedChar), typeof(HexFocusedChar), typeof(DrawedHexEditor), new PropertyMetadata(HexFocusedChar.First));


    }

    /// <summary>
    /// Seperator Lines Part;
    /// </summary>
    public partial class DrawedHexEditor {
        public double SeperatorLineWidth {
            get { return (double)GetValue(SperatorLineWidthProperty); }
            set { SetValue(SperatorLineWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SperatorLineWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SperatorLineWidthProperty =
            DependencyProperty.Register(nameof(SeperatorLineWidth), typeof(double), typeof(DrawedHexEditor), new PropertyMetadata(0.5d));
        
        public Visibility SeperatorLineVisibility {
            get { return (Visibility)GetValue(SeperatorLineVisibilityProperty); }
            set { SetValue(SeperatorLineVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SeperatorLineVisibilityProperty =
            DependencyProperty.Register(nameof(SeperatorLineVisibility), typeof(Visibility), typeof(DrawedHexEditor), new PropertyMetadata(Visibility.Visible));
        
        public Brush SeperatorLineBrush {
            get { return (Brush)GetValue(SeperatorLineBrushProperty); }
            set { SetValue(SeperatorLineBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SeperatorLineBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SeperatorLineBrushProperty =
            DependencyProperty.Register(nameof(SeperatorLineBrush), typeof(Brush), typeof(DrawedHexEditor), 
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0xC8,0xC8,0xC8))));
        
        private void UpdateBlockLines() {
            if(SeperatorLineVisibility != Visibility.Visible) {
                return;
            }
            
            if(BlockSize <= 0) {
                return;
            }

            if(BytePerLine <= 0) {
                return;
            }

            //Local variables is faster than Dependency Property,we storage the size below;
            long firstRowIndex = this.Position / BytePerLine;
            long maxRowCount = MaxVisibleLength / BytePerLine;

            var rowPerblock = BlockSize / BytePerLine;
            long lastVisbleRowIndexWithLine = (firstRowIndex + maxRowCount) / rowPerblock * rowPerblock;
            long firstVisibleRowIndexWithLine = firstRowIndex % rowPerblock == 0 ? (firstRowIndex / rowPerblock * rowPerblock) : (firstRowIndex / rowPerblock * rowPerblock) + 1;
            long rowIndexWithLine = lastVisbleRowIndexWithLine;

            var lineCount = (lastVisbleRowIndexWithLine - firstVisibleRowIndexWithLine + 1) / rowPerblock;
            var lineHeight = HexDataLayer.GetCellSize().Height + HexDataLayer.CellMargin.Top + HexDataLayer.CellMargin.Bottom;
          
            //If line count is larger than the count of cached seperators,fill the rest;
            while (BlockLinesContainer.Children.Count < lineCount) {
                var seperator = new Rectangle {
                    VerticalAlignment = VerticalAlignment.Top
                };
                SetSeperatorBinding(seperator, Orientation.Horizontal);
                BlockLinesContainer.Children.Add(seperator);
            }
            var lineIndex = 0;
            while (rowIndexWithLine > firstRowIndex) {
                var seperator = (Rectangle)BlockLinesContainer.Children[lineIndex];
                seperator.Opacity = 1;

                //(visibleRowIndexWithLine - firstRowIndex) * lineHeight
                seperator.Margin = new Thickness(0, (rowIndexWithLine - firstRowIndex) * lineHeight, 0, 0);
                rowIndexWithLine -= rowPerblock;
                lineIndex++;
            }
            for (int i = 0; i < BlockLinesContainer.Children.Count - lineCount; i++) {
                BlockLinesContainer.Children[BlockLinesContainer.Children.Count - i - 1].Opacity = 0;
            }
        }
        
        /// <summary>
        /// This property indicates how big a block area is,which may effect the vertical offset position of blocklines;
        /// </summary>
        /// <remarks>The value should be divisible to BytePerLine</remarks>
        public int BlockSize {
            get { return (int)GetValue(BlockSizeProperty); }
            set { SetValue(BlockSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BlockSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BlockSizeProperty =
            DependencyProperty.Register(nameof(BlockSize), typeof(int), typeof(DrawedHexEditor), new PropertyMetadata(512, BlockSize_PropertyChanged));

        private static void BlockSize_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if(!(d is DrawedHexEditor ctrl)) {
                return;
            }
            var newBlSize = (int)e.NewValue;
            if (newBlSize <= 0) {
                throw new ArgumentOutOfRangeException(nameof(BlockSize));
            }
            
            if(ctrl.BytePerLine <= 0) {
                return;
            }

            if(newBlSize % ctrl.BytePerLine != 0) {
                throw new ArgumentException($"{nameof(BlockSize)} is not a available argument due to the unmatched {nameof(BytePerLine)}:{ctrl.BytePerLine}");
            }
        }

        private void InitializeFixedSeperatorsBindings() {
            IEnumerable<(Rectangle seperator, Orientation orientation)> GetFixedSeperatorTuples() {
                yield return (seperatorLineLeft, Orientation.Vertical);
                yield return (seperatorLineTop, Orientation.Horizontal);
                yield return (seperatorLineRight, Orientation.Vertical);
            }

            SetSeperatorBindings(GetFixedSeperatorTuples());
        }

        private Binding _spVisibilityBinding;
        private Binding _spLineBrushBinding;
        private Binding _spWidthBinding;
        private Binding SpVisibilityBinding => _spVisibilityBinding ?? (_spVisibilityBinding = GetBindingToSelf(nameof(Visibility)));
        private Binding SpLineBrushBinding => _spLineBrushBinding ?? (_spLineBrushBinding = GetBindingToSelf(nameof(SeperatorLineBrush)));
        private Binding SpWidthBinding => _spWidthBinding ?? (_spWidthBinding = GetBindingToSelf(nameof(SeperatorLineWidth)));

        private void SetSeperatorBinding(Rectangle seperator, Orientation orientation) {
            seperator.SetBinding(VisibilityProperty, SpVisibilityBinding);
            seperator.SetBinding(Rectangle.FillProperty, SpLineBrushBinding);
            if (orientation == Orientation.Horizontal) {
                seperator.SetBinding(HeightProperty, SpWidthBinding);
            }
            else {
                seperator.SetBinding(WidthProperty, SpWidthBinding);
            }
        }
        private void SetSeperatorBindings(IEnumerable<(Rectangle seperator, Orientation orientation)> seperatorTuples) {
            foreach (var (seperator, orientation) in seperatorTuples) {
                SetSeperatorBinding(seperator, orientation);
            }
        }
    }

    /// <summary>
    /// String encoding parts.
    /// </summary>
    public partial class DrawedHexEditor {
        public IBytesToCharEncoding BytesToCharEncoding {
            get { return (IBytesToCharEncoding)GetValue(BytesToCharEncodingProperty); }
            set { SetValue(BytesToCharEncodingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BytesToCharEncoding.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BytesToCharEncodingProperty =
            DependencyProperty.Register(nameof(BytesToCharEncoding), typeof(IBytesToCharEncoding), typeof(DrawedHexEditor), new PropertyMetadata(BytesToCharEncodings.ASCII));

        private void InitializeEncodingBinding() {
#if DEBUG
            //BytesToCharEncoding = BytesToCharEncodings.UTF8;
#endif
            var encodingBinding = new Binding {
                Path = new PropertyPath(nameof(BytesToCharEncoding)) ,
                Source = this
            };

            StringDataLayer.SetBinding(StringDataLayer.BytesToCharEncodingProperty, encodingBinding);
        }
    }

    /// <summary>
    /// Hex/String ToolTip parts.
    /// </summary>
    public partial class DrawedHexEditor {
        private void InitializeTooltipEvents() {
            HexDataLayer.MouseMoveOnCell += Datalayer_MouseMoveOnCell;
            StringDataLayer.MouseMoveOnCell += Datalayer_MouseMoveOnCell;
        }

        private long _mouseOverLevel;

        private void Datalayer_MouseMoveOnCell(object sender, (int cellIndex, MouseEventArgs e) arg) {
            var index = arg.cellIndex;
            if (!(sender is DataLayerBase dataLayer))
                return;

            if (_contextMenuShowing)
                return;

            var popPoint = dataLayer.GetCellPosition(index);
            if (popPoint == null)
                return;

            var pointValue = popPoint.Value;
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                return;

            HoverPosition = Position / BytePerLine * BytePerLine + arg.cellIndex;

            if (ToolTipExtension.GetOperatableToolTip(dataLayer) == null)
                return;

            dataLayer.SetToolTipOpen(false);
            var thisLevel = _mouseOverLevel++;

            //Delay is designed to improve the experience;
            ThreadPool.QueueUserWorkItem(cb => {
                Thread.Sleep(200);
                if (_mouseOverLevel > thisLevel + 1)
                    return;

                Dispatcher.Invoke(() => {
                    if (Mouse.LeftButton == MouseButtonState.Pressed)
                        return;
                    if (Mouse.DirectlyOver != dataLayer) {
                        return;
                    }

                    dataLayer.SetToolTipOpen(true, new Point {
                        X = pointValue.X + dataLayer.CellMargin.Left + dataLayer.CharSize.Width +
                            dataLayer.CellPadding.Left,
                        Y = pointValue.Y + dataLayer.CharSize.Height + dataLayer.CellPadding.Top +
                            dataLayer.CellMargin.Top
                    });
                });
            });
        }

        public FrameworkElement HexDataToolTip {
            get => (FrameworkElement)GetValue(HexDataToolTipProperty);
            set => SetValue(HexDataToolTipProperty, value);
        }

        // Using a DependencyProperty as the backing store for HexDataToolTip.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HexDataToolTipProperty =
            DependencyProperty.Register(nameof(HexDataToolTip), typeof(FrameworkElement), typeof(DrawedHexEditor),
                new PropertyMetadata(null,
                    HexDataToolTip_PropertyChanged));

        private static void HexDataToolTip_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (!(d is DrawedHexEditor ctrl))
                return;

            if (e.NewValue is FrameworkElement newElem)
                ToolTipExtension.SetOperatableToolTip(ctrl.HexDataLayer, newElem);
        }

        public FrameworkElement StringDataToolTip {
            get => (FrameworkElement)GetValue(HexDataToolTipProperty);
            set => SetValue(HexDataToolTipProperty, value);
        }

        // Using a DependencyProperty as the backing store for HexDataToolTip.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StringDataToolTipProperty =
            DependencyProperty.Register(nameof(StringDataToolTip), typeof(FrameworkElement), typeof(DrawedHexEditor),
                new PropertyMetadata(null,
                    StringDataToolTip_PropertyChanged));

        private static void StringDataToolTip_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (!(d is DrawedHexEditor ctrl))
                return;

            if (e.NewValue is FrameworkElement newElem)
                ToolTipExtension.SetOperatableToolTip(ctrl.StringDataLayer, newElem);
        }

        public long HoverPosition {
            get => (long)GetValue(HoverPositionProperty);
            set => SetValue(HoverPositionProperty, value);
        }

        // Using a DependencyProperty as the backing store for HoverPosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HoverPositionProperty =
            DependencyProperty.Register(nameof(HoverPosition), typeof(long), typeof(DrawedHexEditor),
                new FrameworkPropertyMetadata(-1L, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
    }

    /// <summary>
    /// Undo/Redo parts.
    /// </summary>
    public partial class DrawedHexEditor {
        private readonly Stack<IEditTransaction> _undoTransactions = new Stack<IEditTransaction>();
        private readonly Stack<IEditTransaction> _redoTransactions = new Stack<IEditTransaction>();

        public bool CanUndo => _undoTransactions.Count != 0;
        public bool CanRedo => _redoTransactions.Count != 0;

        public event EventHandler<CanRedoChangedEventArgs> CanUndoChanged;
        public event EventHandler<CanUndoChangedEventArgs> CanRedoChanged;
            
        public void Undo() {
            if (!CanUndo) {
                return;
            }

            CanUndoChanged?.Invoke(this, new CanRedoChangedEventArgs(CanUndo));
        }

        public void Redo() {
            if (!CanRedo) {
                return;
            }

            CanRedoChanged?.Invoke(this, new CanUndoChangedEventArgs(CanRedo));
        }

        private void CommitTransaction(IEditTransaction editTransaction) {
            if(editTransaction == null) {
                throw new ArgumentNullException(nameof(editTransaction));
            }


        }
    }

#if DEBUG
    public partial class DrawedHexEditor {
        ~DrawedHexEditor() {

        }
    }
#endif
}
