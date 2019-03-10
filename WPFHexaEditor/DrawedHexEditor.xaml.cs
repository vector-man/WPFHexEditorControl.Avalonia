//////////////////////////////////////////////
// Apache 2.0  - 2018
// Author : Janus Tida
// Modified by : Derek Tremblay
//////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
using WpfHexaEditor.Events;
using WpfHexaEditor.Layers;

namespace WpfHexaEditor
{
    /// <summary>
    /// Interaction logic for DrawedHexEditor.xaml
    /// </summary>
    public partial class DrawedHexEditor {
        #region DevBranch

        public DrawedHexEditor() {
            InitializeComponent();

            InitilizeEvents();
            InitializeBindings();
            UpdateCellPaddings();
            UpdateCellMargins();

            FontSize = 16;

            FontFamily = new FontFamily("Arial");
            //FontFamily = new FontFamily("Microsoft YaHei");
            //FontFamily = new FontFamily("Courier New");
            //FontFamily = new FontFamily("Lucida Bright");
            DataVisualType = DataVisualType.Decimal;
        }

        //Cuz xaml designer's didn't support valuetuple,events subscribing will be executed in code-behind.
        private void InitilizeEvents() {
            this.SizeChanged += delegate { UpdateContent(); };
            void initialCellsLayer(ICellsLayer layer) {
                layer.MouseUpOnCell += DataLayer_MouseLeftUpOnCell;
                layer.MouseMoveOnCell += DataLayer_MouseMoveOnCell;
            }

            initialCellsLayer(HexDataLayer);
            initialCellsLayer(StringDataLayer);

            InitializeTooltipEvents();
            InitializeDataLayers();
        }

        private void InitializeDataLayers() {
            HexDataLayer.HexMouseDownOnCell += HexDataLayer_HexMouseDownOnCell;
            StringDataLayer.MouseDownOnCell += StringDataLayer_MouseDownOnCell;
        }

        private void StringDataLayer_MouseDownOnCell(object sender, MouseButtonOnCellEventArgs e) {
            ActivedPanel = LayerPanel.String;
            HandleMouseDownOnCellEventArgsCore(e);
        }

        private void HexDataLayer_HexMouseDownOnCell(object sender, HexMouseButtonOnCellEventArgs e) {
            HexFocusedChar = e.HexChar;
            ActivedPanel = LayerPanel.Hex;
            HandleMouseDownOnCellEventArgsCore(e);
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

            void SetFontControlBindings(IEnumerable<FontControlBase> fontControls) {
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



        //To avoid wrong mousemove event;
        private bool _contextMenuShowing;

        private int MaxVisibleLength {
            get {
                if (Stream == null)
                    return 0;

                return (int)Math.Min(HexDataLayer.AvailableRowsCount * BytePerLine,
                    Stream.Length - Position / BytePerLine * BytePerLine);
            }
        }

        protected override void OnContextMenuOpening(ContextMenuEventArgs e) {
            base.OnContextMenuOpening(e);
            _contextMenuShowing = true;
        }

        protected override void OnContextMenuClosing(ContextMenuEventArgs e) {
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


        /// <summary>
        /// In order to avoid endless looping of ScrollBar_ValueChanged and Position_PropertyChanged.
        /// We created this field for signaling;
        /// </summary>
        private bool _scrollBarValueUpdating;

        /// <summary>
        /// Remember the position in which the mouse last clicked.
        /// </summary>
        private long? _lastMouseDownPosition;

        #region EventSubscriber handlers;

        private void Control_MouseWheel(object sender, MouseWheelEventArgs e) {
            if (Stream == null) return;

            if (e.Delta > 0) //UP
                VerticalScrollBar.Value -= e.Delta / 120 * (int)MouseWheelSpeed;

            if (e.Delta < 0) //Down
                VerticalScrollBar.Value += e.Delta / 120 * -(int)MouseWheelSpeed;
        }

        private void HandleMouseDownOnCellEventArgsCore(MouseButtonOnCellEventArgs arg) {
            if (arg.NativeEventArgs.ChangedButton != MouseButton.Left &&
                arg.NativeEventArgs.ChangedButton != MouseButton.Right) {
                return;
            }

            if (arg.CellIndex >= MaxVisibleLength)
                return;

            var clickPosition = Position / BytePerLine * BytePerLine + arg.CellIndex;
            if (Keyboard.Modifiers == ModifierKeys.Shift) {
                long oldStart = -1;

                if (SelectionStart != -1)
                    oldStart = SelectionStart;

                if (FocusPosition != -1)
                    oldStart = FocusPosition;

                if (oldStart != -1) {
                    SelectionStart = Math.Min(oldStart, clickPosition);
                    SelectionLength = Math.Abs(oldStart - clickPosition) + 1;
                }
            }

            _lastMouseDownPosition = clickPosition;

            ///Cuz <see cref="UpdateForegroundsAndBackgrounds"/> will be invoked after <see cref="FocusPosition"/> changed.
            ///Or we should invoke <see cref="UpdateForegroundsAndBackgrounds"/> manually.
            if (FocusPosition != clickPosition) {
                FocusPosition = clickPosition;
            }
            else {
                UpdateForegroundsAndBackgrounds();
            }

            this.Focus();
        }


        private void DataLayer_MouseMoveOnCell(object sender, MouseOnCellEventArgs arg) {
            if (arg.NativeEventArgs.LeftButton != MouseButtonState.Pressed)
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

            var cellPosition = Position / BytePerLine * BytePerLine + arg.CellIndex;
            if (_lastMouseDownPosition.Value == cellPosition)
                return;

            var length = Math.Abs(cellPosition - _lastMouseDownPosition.Value) + 1;
            SelectionStart = Math.Min(cellPosition, _lastMouseDownPosition.Value);
            SelectionLength = length;
        }

        private void DataLayer_MouseLeftUpOnCell(object sender, MouseButtonOnCellEventArgs arg) =>
            _lastMouseDownPosition = null;

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            foreach (var action in GetKeyDownEventHandlers()) {
                action(e);
            }   
        }

        private IEnumerable<Action<KeyEventArgs>> GetKeyDownEventHandlers() {
            yield return KeyDownOnSelection;
            yield return KeyDownOnFocus;
        }


        protected override void OnTextInput(TextCompositionEventArgs e) {
            base.OnTextInput(e);
            foreach (var action in GetTextInputEventHandlers()) {
                action(e);
            }
        }

        private IEnumerable<Action<TextCompositionEventArgs>> GetTextInputEventHandlers() {
            yield return TextInputOnHex;
            yield return TextInputOnString;
        }



       
        #endregion


        /// <summary>
        /// This method won't be while scrolling,but only when stream is opened or closed,byteperline changed(UpdateInfo);
        /// </summary>
        private void UpdateInfoes() {
            ClearStates();

            UpdateScrollBarInfo();
            UpdateColumnHeaderInfo();
            UpdateOffsetLinesInfo();


        }

        private void ClearStates() {
            ClearPositionState();

            ClearFocusState();

            ClearSelectionState();

            ClearCaretState();
        }

        #region These methods won't be invoked everytime scrolling.but only when stream is opened or closed,byteperline changed(UpdateInfo).

        /// <summary>
        /// Update vertical scrollbar with file info
        /// </summary>
        private void UpdateScrollBarInfo() {
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
        private void UpdateColumnHeaderInfo() {
            ColumnsOffsetInfoLayer.StartStepIndex = 0;
            ColumnsOffsetInfoLayer.StepsCount = BytePerLine;
        }

        /// <summary>
        /// Update the position info panel at left of the control,notice this won't change the content of the OffsetLines;
        /// </summary>
        private void UpdateOffsetLinesInfo() {
            if (Stream == null)
                return;

            LinesOffsetInfoLayer.DataVisualType = DataVisualType;
            LinesOffsetInfoLayer.StepLength = BytePerLine;

            LinesOffsetInfoLayer.SavedBits = DataVisualType == DataVisualType.Hexadecimal
                ? ByteConverters.GetHexBits(Stream.Length)
                : ByteConverters.GetDecimalBits(Stream.Length);
        }

        //This will affect how a linesinfo and columnsinfo index change.
        public DataVisualType DataVisualType {
            get => (DataVisualType)GetValue(DataVisualTypeProperty);
            set => SetValue(DataVisualTypeProperty, value);
        }

        // Using a DependencyProperty as the backing store for DataVisualType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataVisualTypeProperty =
            DependencyProperty.Register(nameof(DataVisualType),
                typeof(DataVisualType), typeof(DrawedHexEditor),
                new PropertyMetadata(DataVisualType.Hexadecimal, DataVisualTypeProperty_Changed));

        private static void DataVisualTypeProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (!(d is DrawedHexEditor ctrl)) {
                return;
            }

            ctrl.LinesOffsetInfoLayer.DataVisualType = (DataVisualType)e.NewValue;
            ctrl.ColumnsOffsetInfoLayer.DataVisualType = (DataVisualType)e.NewValue;
            ctrl.UpdateContent();
        }

        #endregion


        public long Position {
            get => (long)GetValue(PositionProperty);
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

        private void ClearPositionState() {
            //Position PropertyChangedCallBack will update the content;
            Position = 0;
        }
        /**/

        /// <summary>
        /// Refresh currentview of hexeditor
        /// </summary>
        public void UpdateContent() {
            UpdateOffsetLinesContent();
            UpdateScrollBarContent();
            UpdateForegroundsAndBackgrounds();
            UpdateDataContent();
            UpdateBlockLines();
        }



        #region  These methods will be invoked every time scrolling the content(scroll or position changed)(Refreshview calling);

        ///<see cref="UpdateContent"/>
        /// <summary>
        /// Update the hex and string layer you current view;
        /// </summary>
        private void UpdateDataContent() {
            if (!(Stream?.CanRead ?? false)) {
                HexDataLayer.Data = null;
                StringDataLayer.Data = null;
                return;
            }

            HexDataLayer.Data = null;
            StringDataLayer.Data = null;

            Stream.Position = Position / BytePerLine * BytePerLine;
            HexDataLayer.PositionStartToShow = (int)(Position / BytePerLine * BytePerLine);
            StringDataLayer.PositionStartToShow = (int)(Position / BytePerLine * BytePerLine);

            if (_viewBuffer == null || _viewBuffer.Length != MaxVisibleLength)
                _viewBuffer = new byte[MaxVisibleLength];

            foreach (var action in GetUpdateViewBufferActions()) {
                action();
            }
            
            HexDataLayer.Data = _viewBuffer;
            StringDataLayer.Data = _viewBuffer;
        }

        private IEnumerable<Action> GetUpdateViewBufferActions() {
            yield return UpdateViewBufferFromStream;
            yield return UpdateViewBufferFromCaret;
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

        
    }

    /// <summary>
    /// Stream and Data Part;
    /// </summary>
    public partial class DrawedHexEditor {
        /// <summary>
        /// Save the view byte buffer as a field. 
        /// To save the time when Scolling i do not building them every time when scolling.
        /// </summary>
        private byte[] _viewBuffer;

        #region DependencyPorperties

        #region BytePerLine property/methods

        /// <summary>
        /// Get or set the number of byte are show in control
        /// </summary>
        public int BytePerLine {
            get => (int)GetValue(BytePerLineProperty);
            set => SetValue(BytePerLineProperty, value);
        }

        public static readonly DependencyProperty BytePerLineProperty =
            DependencyProperty.Register(nameof(BytePerLine), typeof(int), typeof(DrawedHexEditor),
                new PropertyMetadata(16, BytePerLine_PropertyChanged));

        private static void BytePerLine_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (!(d is DrawedHexEditor ctrl) || e.NewValue == e.OldValue) return;
            ctrl.HexDataLayer.BytePerLine = (int)e.NewValue;
            ctrl.StringDataLayer.BytePerLine = (int)e.NewValue;

            ctrl.UpdateInfoes();
            ctrl.UpdateContent();
        }

        #endregion



        public MouseWheelSpeed MouseWheelSpeed {
            get => (MouseWheelSpeed)GetValue(MouseWheelSpeedProperty);
            set => SetValue(MouseWheelSpeedProperty, value);
        }

        // Using a DependencyProperty as the backing store for MouseWheelSpeed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseWheelSpeedProperty =
            DependencyProperty.Register(nameof(MouseWheelSpeed), typeof(MouseWheelSpeed), typeof(DrawedHexEditor),
                new PropertyMetadata(MouseWheelSpeed.Normal));


        /// <summary>
        /// Set the Stream are used by ByteProvider
        /// </summary>
        public Stream Stream {
            get => (Stream)GetValue(StreamProperty);
            set => SetValue(StreamProperty, value);
        }

        // Using a DependencyProperty as the backing store for Stream.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StreamProperty =
            DependencyProperty.Register(nameof(Stream), typeof(Stream), typeof(DrawedHexEditor),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits,
                    Stream_PropertyChanged));

        private static void Stream_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (!(d is DrawedHexEditor ctrl)) return;
            //These methods won't be invoked everytime scrolling.but only when stream is opened or closed.
            ctrl.UpdateInfoes();
            ctrl.UpdateContent();
        }

        #endregion

        /// <summary>
        /// Update <see cref="_viewBuffer"/>;
        /// </summary>
        private void UpdateViewBufferFromStream() {
           

            Stream.Read(_viewBuffer, 0, MaxVisibleLength);
        }
    }

    /// <summary>
    /// Margin and Padding Part;
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

        private const double LinesOffsetInfoLayerHMargin = 8;
        private const double ColumnsOffsetInfoLayerVMarging = 4;

        private void UpdateCellMargins() {
            var cellMargin = CellMargin;
            HexDataLayer.CellMargin = cellMargin;
            StringDataLayer.CellMargin = cellMargin;
            LinesOffsetInfoLayer.CellMargin = new Thickness(LinesOffsetInfoLayerHMargin, cellMargin.Top, LinesOffsetInfoLayerHMargin, cellMargin.Bottom);
            ColumnsOffsetInfoLayer.CellMargin = new Thickness(cellMargin.Left, ColumnsOffsetInfoLayerVMarging, cellMargin.Right, ColumnsOffsetInfoLayerVMarging);
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
    /// <see cref="IBrushBlock"/> and <see cref="IHexBrushPosition"/> Part;
    /// </summary>
    public partial class DrawedHexEditor {
        /// <summary>
        /// We create this dictionary to store the relationship between two brushBlocks,
        /// key of which stores the position and length that relative to Stream,
        /// while the value of which stores the position and length that relative to <see cref="Position"/> in visible range;
        /// To-DO:avoid reproducing the brushblock every time when refreshing the view.
        /// </summary>
        private readonly Dictionary<IBrushBlock, IBrushBlock> _cachedBrushBlockDict = new Dictionary<IBrushBlock, IBrushBlock>();

        /// <summary>
        /// We create this dictionary to store the relationship between two HexBrushPosition,
        /// key of which stores the position and length that relative to Stream,
        /// while the value of which stores the position and length that relative to <see cref="Position"/> in visible range;
        /// To-DO:avoid reproducing the <see cref="IHexBrushPosition"/> every time when refreshing the view.
        /// </summary>
        private readonly Dictionary<IHexBrushPosition, IHexBrushPosition> _cachedHexBrushPositionDict = new Dictionary<IHexBrushPosition, IHexBrushPosition>();

        /// <summary>
        /// Create or Get the <see cref="IBrushBlock"/>(value) that related to <paramref name="brushBlockKey"/>(key);
        /// </summary>
        /// <param name="brushBlockKey"></param>
        /// <returns></returns>
        private IBrushBlock CreateOrGetBrushBlockValue(IBrushBlock brushBlockKey) {
            IBrushBlock brushBlockValue = null;
            if (_cachedBrushBlockDict.TryGetValue(brushBlockKey, out var exsitingBrushBlockValue)) {
                brushBlockValue = exsitingBrushBlockValue;
            }
            else {
                brushBlockValue = new BrushBlock();
                _cachedBrushBlockDict.Add(brushBlockKey, brushBlockValue);
            }

            if (brushBlockValue.Brush != brushBlockKey.Brush) {
                brushBlockValue.Brush = brushBlockKey.Brush;
            }

            return brushBlockValue;
        }

        /// <summary>
        /// Create or Get the <see cref="IHexBrushPosition"/> that related to <paramref name="hexBrushPositionKey"/>;
        /// </summary>
        /// <param name="hexBrushPositionKey"></param>
        /// <returns></returns>
        private IHexBrushPosition CreateOrGetHexBrushPosition(IHexBrushPosition hexBrushPositionKey) {
            IHexBrushPosition hexBrushPositionValue = null;
            if(_cachedHexBrushPositionDict.TryGetValue(hexBrushPositionKey,out var exsitingHexBrushPositionValue)) {
                hexBrushPositionValue = exsitingHexBrushPositionValue;
            }
            else {
                hexBrushPositionValue = new HexBrushPosition();
                _cachedHexBrushPositionDict.Add(hexBrushPositionKey, hexBrushPositionValue);
            }

            if(hexBrushPositionValue.FirstCharBrush != hexBrushPositionKey.FirstCharBrush) {
                hexBrushPositionValue.FirstCharBrush = hexBrushPositionKey.FirstCharBrush;
            }

            if(hexBrushPositionValue.SecondCharBrush != hexBrushPositionKey.SecondCharBrush) {
                hexBrushPositionValue.SecondCharBrush = hexBrushPositionKey.SecondCharBrush;
            }

            return hexBrushPositionValue;
        }

        private void AddBrushBlockCore(IBrushBlock brushBlock, IList<IBrushBlock> brushBlocks) {
            if (Stream == null)
                return;

            //Check whether the backgroundblock is visible;
            if (!(brushBlock.StartOffset + brushBlock.Length >= Position && brushBlock.StartOffset < Position + MaxVisibleLength))
                return;


            var maxIndex = Math.Max(brushBlock.StartOffset, Position);
            var minEnd = Math.Min(brushBlock.StartOffset + brushBlock.Length, Position + MaxVisibleLength);

            var brushBlockValue = CreateOrGetBrushBlockValue(brushBlock);
            
            brushBlockValue.StartOffset = maxIndex - Position;
            brushBlockValue.Length = minEnd - maxIndex;

            brushBlocks.Add(brushBlockValue);
        }
        
        private void AddHexBrushPositionCore(IHexBrushPosition hexBrushPosition,IList<IHexBrushPosition> hexBrushPositions) {
            if (Stream == null)
                return;

            ///Check whether the <param name="hexBrushPosition"/> is visible;
            if (hexBrushPosition.Position < Position || hexBrushPosition.Position >= Position + MaxVisibleLength)
                return;

            var hexBrushPositionValue = CreateOrGetHexBrushPosition(hexBrushPosition);

            hexBrushPositionValue.Position = hexBrushPosition.Position - Position;

            hexBrushPositions.Add(hexBrushPositionValue);
        }

        private void UpdateForegroundsAndBackgrounds() {
            UpdateBackgroundBlocks();
            UpdateForegroundBlocks();
            UpdateHexBackgroundPositions();
            UpdateHexForegroundPositions();
        }
    }

    /// <summary>
    /// BackgroundBlocks Part;
    /// </summary>
    public partial class DrawedHexEditor {
        private readonly List<IBrushBlock> _stringBackgroundBlocks = new List<IBrushBlock>();

        private readonly List<IBrushBlock> _hexBackgroundBlocks = new List<IBrushBlock>();

        private readonly List<IHexBrushPosition> _hexBackgroundPositions = new List<IHexBrushPosition>();
        
        /// <summary>
        /// We create this dictionary to store the relationship between two backgroundBlocks,
        /// key of which stores the position and length that relative to Stream,
        /// while the value of which stores the position and length that relative to <see cref="Position"/> in visible range;
        /// To-DO:avoid reproducing the brushblock every time when refreshing the view.
        /// </summary>
        private readonly Dictionary<IBrushBlock, IBrushBlock> _cachedBackgroundBlockDict = new Dictionary<IBrushBlock, IBrushBlock>();

        
        public IEnumerable<IBrushBlock> CustomBackgroundBlocks {
            get => (IEnumerable<IBrushBlock>)GetValue(
                CustomBackgroundBlocksProperty);
            set => SetValue(CustomBackgroundBlocksProperty, value);
        }

        // Using a DependencyProperty as the backing store for CustomBackgroundBlocks.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CustomBackgroundBlocksProperty =
            DependencyProperty.Register(nameof(CustomBackgroundBlocks),
                typeof(IEnumerable<IBrushBlock>),
                typeof(DrawedHexEditor),
                new PropertyMetadata(null, CustomBackgroundBlocksProperty_Changed));

        private static void CustomBackgroundBlocksProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            
        }

        private void SetupCustomBackgroundBlocks(IEnumerable<IBrushBlock> backgroundBlocks) {

        }


        public void UpdateBackgroundBlocks() {
            //ClearBackgroundBlocks;
            HexDataLayer.BackgroundBlocks = null;
            StringDataLayer.BackgroundBlocks = null;

            _stringBackgroundBlocks.Clear();
            _hexBackgroundBlocks.Clear();

            foreach (var action in GetUpdateBackgroundActions()) {
                action();
            }

            StringDataLayer.BackgroundBlocks = _stringBackgroundBlocks;
            HexDataLayer.BackgroundBlocks = _hexBackgroundBlocks;
        }


        private void UpdateHexBackgroundPositions() {
            HexDataLayer.HexBackgroundPositions = null;

            _hexBackgroundPositions.Clear();

            foreach (var action in GetUpdateHexBackgroundPositionActions()) {
                action();
            }

            HexDataLayer.HexBackgroundPositions = _hexBackgroundPositions;
        }

        private void UpdateHexForegroundPositions() {
            HexDataLayer.HexForegroundPositions = null;

            _hexForegroundPositions.Clear();

            foreach (var action in GetUpdateHexForegroundPositionActions()) {
                action();
            }

            HexDataLayer.HexForegroundPositions = _hexForegroundPositions;
        }

        private IEnumerable<Action> GetUpdateBackgroundActions() {
            yield return UpdateCustomBackgroundBlocks;
            yield return UpdateSelectionBackgroundBlocks;
            yield return UpdateFocusBackgroundBlock;
        }

        private IEnumerable<Action> GetUpdateHexBackgroundPositionActions() {
            yield return UpdateFocusHexBackgroundPosition;
        }

        private IEnumerable<Action> GetUpdateHexForegroundPositionActions() {
            yield return UpdateFocusHexForegroundPosition;
        }

        private void UpdateCustomBackgroundBlocks() {
            if (CustomBackgroundBlocks == null) return;

            foreach (var block in CustomBackgroundBlocks)
                AddBackgroundBlock(block);
        }

        /// <summary>
        /// Add <paramref name="brushBlock"/> to <see cref="_stringBackgroundBlocks"/> and <see cref="_hexBackgroundBlocks"/>;
        /// </summary>
        /// <param name="brushBlock"></param>
        private void AddBackgroundBlock(IBrushBlock brushBlock) {
            AddHexBackgroundBlock(brushBlock);
            AddStringBackgroundBlock(brushBlock);
        }
        
        private void AddHexBackgroundBlock(IBrushBlock brushBlock) {
            AddBrushBlockCore(brushBlock, _hexBackgroundBlocks);
        }

        private void AddStringBackgroundBlock(IBrushBlock brushBlock) {
            AddBrushBlockCore(brushBlock, _stringBackgroundBlocks);
        }
        
        private void AddHexBackgroundPosition(IHexBrushPosition hexBackgroundPosition) {
            AddHexBrushPositionCore(hexBackgroundPosition, _hexBackgroundPositions);
        }

        private void AddHexForegroundPosition(IHexBrushPosition hexForegroundPosition) {
            AddHexBrushPositionCore(hexForegroundPosition, _hexForegroundPositions);
        }
    }
    
    /// <summary>
    /// ForegroundBlock Part;
    /// </summary>
    public partial class DrawedHexEditor {
        private readonly List<IBrushBlock> _stringForegroundBlocks = new List<IBrushBlock>();

        private readonly List<IBrushBlock> _hexForegroundBlocks = new List<IBrushBlock>();

        private readonly List<IHexBrushPosition> _hexForegroundPositions = new List<IHexBrushPosition>();

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

            _hexForegroundBlocks.Clear();
            _stringForegroundBlocks.Clear();
            
            foreach (var action in GetUpdateForegroundBlockActions()) {
                action();
            }
            
            HexDataLayer.ForegroundBlocks = _hexForegroundBlocks;
            StringDataLayer.ForegroundBlocks = _stringForegroundBlocks;
        }
        
        private IEnumerable<Action> GetUpdateForegroundBlockActions() {
            yield return UpdateCustomForegroundBlocks;
            yield return UpdateSelectionForegroundBlocks;
            yield return UpdateCaretForegroundBlocks;
            yield return UpdateFocusForegroundBlock;
        }

        private void UpdateCustomForegroundBlocks() {
            if (CustomForegroundBlocks == null) return;

            foreach (var block in CustomForegroundBlocks)
                AddForegroundBlock(block);
        }


        /// <summary>
        /// Add <paramref name="brushBlock"/> to <see cref="_stringBackgroundBlocks"/> and <see cref="_hexBackgroundBlocks"/>;
        /// </summary>
        /// <param name="brushBlock"></param>
        private void AddForegroundBlock(IBrushBlock brushBlock) {
            AddHexForegroundBlock(brushBlock);
            AddStringForegroundBlock(brushBlock);
        }

        private void AddHexForegroundBlock(IBrushBlock brushBlock) {
            AddBrushBlockCore(brushBlock, _hexForegroundBlocks);
        }

        private void AddStringForegroundBlock(IBrushBlock brushBlock) {
            AddBrushBlockCore(brushBlock, _stringForegroundBlocks);
        }

        
    }


    /// <summary>
    /// Selection Part;
    /// </summary>
    public partial class DrawedHexEditor {

        /// <summary>
        /// Background block for Selection;
        /// </summary>
        private readonly IBrushBlock _selectionBackgroundBlock = new BrushBlock();

        /// <summary>
        /// Foreground block for Selection;
        /// </summary>
        private readonly IBrushBlock _selectionForegroundBlock = new BrushBlock();

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


            ctrl.UpdateForegroundsAndBackgrounds();
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

            ctrl.UpdateForegroundsAndBackgrounds();
        }

        public Brush SelectionBackground {
            get => (Brush)GetValue(SelectionBackgroundProperty);
            set => SetValue(SelectionBackgroundProperty, value);
        }

        // Using a DependencyProperty as the backing store for SelectionBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectionBackgroundProperty =
            DependencyProperty.Register(nameof(SelectionBackground), typeof(Brush), typeof(DrawedHexEditor),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0xe0, 0xe0, 0xff))));



        public Brush SelectionForeground {
            get { return (Brush)GetValue(SelectionForegroundProperty); }
            set { SetValue(SelectionForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectionForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectionForegroundProperty =
            DependencyProperty.Register(nameof(SelectionForeground), typeof(Brush), typeof(DrawedHexEditor),
                new PropertyMetadata(Brushes.Azure));


        private void UpdateSelectionBackgroundBlocks() {
            if(SelectionBackground == null) {
                return;
            }

            _selectionBackgroundBlock.StartOffset = SelectionStart;
            _selectionBackgroundBlock.Length = SelectionLength;
            _selectionBackgroundBlock.Brush = SelectionBackground;

            AddBackgroundBlock(_selectionBackgroundBlock);
        }
        
        private void UpdateSelectionForegroundBlocks() {
            if (SelectionForeground == null) {
                return;
            }

            _selectionForegroundBlock.StartOffset = SelectionStart;
            _selectionForegroundBlock.Length = SelectionLength;
            _selectionForegroundBlock.Brush = SelectionForeground;
            
            AddForegroundBlock(_selectionForegroundBlock);
        }

        private void ClearSelectionState() {

            //RestoreSelection;
            SelectionStart = -1;
            SelectionLength = 0;
        }

        private void KeyDownOnSelection(KeyEventArgs args) {
            //Update Selection if shift key is pressed;
            if (Keyboard.Modifiers == ModifierKeys.Shift) {
                long vectorEnd = -1;
                switch (args.Key) {
                    case Key.Left:
                        if (FocusPosition > 0) {
                            vectorEnd = FocusPosition - 1;
                        }

                        break;
                    case Key.Up:
                        if (FocusPosition >= BytePerLine) {
                            vectorEnd = FocusPosition - BytePerLine;
                        }

                        break;
                    case Key.Right:
                        if (FocusPosition + 1 < Stream.Length) {
                            vectorEnd = FocusPosition + 1;
                        }

                        break;
                    case Key.Down:
                        if (FocusPosition + BytePerLine < Stream.Length) {
                            vectorEnd = FocusPosition + BytePerLine;
                        }

                        break;
                }

                if (vectorEnd != -1) {
                    //BackWard;
                    if (vectorEnd < FocusPosition) {
                        if (FocusPosition == SelectionStart) {
                            SelectionLength += SelectionStart - vectorEnd;
                            SelectionStart = vectorEnd;
                        }
                        else if (FocusPosition == SelectionStart + SelectionLength - 1 &&
                                 SelectionLength >= FocusPosition - vectorEnd + 1) {
                            SelectionLength -= FocusPosition - vectorEnd;
                        }
                        else {
                            SelectionStart = vectorEnd;
                            SelectionLength = FocusPosition - vectorEnd + 1;
                        }
                    }
                    //Forward;
                    else if (vectorEnd > FocusPosition) {
                        if (FocusPosition == SelectionStart + SelectionLength - 1) {
                            SelectionLength += vectorEnd - FocusPosition;
                        }
                        else if (FocusPosition == SelectionStart &&
                                 SelectionLength >= vectorEnd - FocusPosition + 1) {
                            SelectionLength -= vectorEnd - SelectionStart;
                            SelectionStart = vectorEnd;
                        }
                        else {
                            SelectionStart = FocusPosition;
                            SelectionLength = vectorEnd - FocusPosition + 1;
                        }
                    }
                }

            }
        }
    }

    /// <summary>
    /// Focus Part;
    /// </summary>
    public partial class DrawedHexEditor {

        /// <summary>
        /// Brush block for focus state;
        /// </summary>
        private readonly IBrushBlock _stringFocusBackgroundBlock = new BrushBlock { Length = 1 };
        private readonly IBrushBlock _stringFocusForegroundBlock = new BrushBlock { Length = 1 };
        
        private readonly IHexBrushPosition _hexFocusBackgroundPosition = new HexBrushPosition();
        private readonly IHexBrushPosition _hexFocusForegroundPosition = new HexBrushPosition();
        
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

            ctrl.UpdateForegroundsAndBackgrounds();

            if ((long)e.NewValue == -1) return;

            ctrl.Focusable = true;
        }

        public Brush FocusBackground {
            get => (Brush)GetValue(FocusBackgroundProperty);
            set => SetValue(FocusBackgroundProperty, value);
        }

        // Using a DependencyProperty as the backing store for FocusBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FocusBackgroundProperty =
            DependencyProperty.Register(nameof(FocusBackground), typeof(Brush), typeof(DrawedHexEditor),
                new PropertyMetadata(Brushes.Blue));

        public Brush FocusForeground {
            get { return (Brush)GetValue(FocusForegroundProperty); }
            set { SetValue(FocusForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FocusForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FocusForegroundProperty =
            DependencyProperty.Register(nameof(FocusForeground), typeof(Brush), typeof(DrawedHexEditor),
                new PropertyMetadata(Brushes.White));
        
        public Brush FocusBackgroundNonActive {
            get { return (Brush)GetValue(FocusBackgroundNonActiveProperty); }
            set { SetValue(FocusBackgroundNonActiveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FocusBackgroundNonActive.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FocusBackgroundNonActiveProperty =
            DependencyProperty.Register(nameof(FocusBackgroundNonActive), typeof(Brush), typeof(DrawedHexEditor), new PropertyMetadata(Brushes.Gray));
        
        public Brush FocusForegroundNonActive {
            get { return (Brush)GetValue(FocusForegroundNonActiveProperty); }
            set { SetValue(FocusForegroundNonActiveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FocusForegroundNonActive.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FocusForegroundNonActiveProperty =
            DependencyProperty.Register(nameof(FocusForegroundNonActive), typeof(Brush), typeof(DrawedHexEditor), new PropertyMetadata(Brushes.White));



        public LayerPanel ActivedPanel {
            get { return (LayerPanel)GetValue(ActivedPanelProperty); }
            set { SetValue(ActivedPanelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FocusedPanel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActivedPanelProperty =
            DependencyProperty.Register(nameof(ActivedPanel), typeof(LayerPanel), typeof(DrawedHexEditor), new PropertyMetadata(LayerPanel.Hex));



        public HexChar HexFocusedChar {
            get { return (HexChar)GetValue(HexFocusedCharProperty); }
            set { SetValue(HexFocusedCharProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HexFocusedChar.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HexFocusedCharProperty =
            DependencyProperty.Register(nameof(HexFocusedChar), typeof(HexChar), typeof(DrawedHexEditor), new PropertyMetadata(HexChar.First,HexCharProperty_Changed));

        private static void HexCharProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if(!(d is DrawedHexEditor ctrl)) {
                return;
            }

            ctrl.UpdateForegroundsAndBackgrounds();
        }

        private void UpdateFocusBackgroundBlock() {
            if(FocusPosition < 0 ) {
                return;
            }

            var stringFocusBackground = ActivedPanel == LayerPanel.String ? FocusBackground : FocusBackgroundNonActive;
            if(stringFocusBackground == null) {
                return;
            }

            _stringFocusBackgroundBlock.StartOffset = FocusPosition;
            _stringFocusBackgroundBlock.Brush = stringFocusBackground;
            AddStringBackgroundBlock(_stringFocusBackgroundBlock);
        }
        

        private void UpdateFocusForegroundBlock() {
            if (FocusBackground == null) {
                return;
            }
            
            var stringFocusForeground = ActivedPanel == LayerPanel.String ? FocusForeground : FocusForegroundNonActive;
            if(stringFocusForeground == null) {
                return;
            }


            if (FocusPosition >= 0) {
                _stringFocusForegroundBlock.StartOffset = FocusPosition;
                _stringFocusForegroundBlock.Brush = stringFocusForeground;

                AddStringForegroundBlock(_stringFocusForegroundBlock);
            }
        }


        private void UpdateFocusHexBackgroundPosition() {
            if (FocusPosition < 0) {
                return;
            }

            var hexFocusBackground = ActivedPanel == LayerPanel.Hex ? FocusBackground : FocusBackgroundNonActive;
            if(hexFocusBackground == null) {
                return;
            }

            _hexFocusBackgroundPosition.Position = FocusPosition;

            if (HexFocusedChar == HexChar.First) {
                _hexFocusBackgroundPosition.FirstCharBrush = hexFocusBackground;
                _hexFocusBackgroundPosition.SecondCharBrush = null;
            }
            else {
                _hexFocusBackgroundPosition.FirstCharBrush = null;
                _hexFocusBackgroundPosition.SecondCharBrush = hexFocusBackground;
            }

            AddHexBackgroundPosition(_hexFocusBackgroundPosition);
        }

        private void UpdateFocusHexForegroundPosition() {
            if (FocusPosition < 0) {
                return;
            }
            
            var hexFocusForeground = ActivedPanel == LayerPanel.Hex ? FocusForeground : FocusForegroundNonActive;
            if(hexFocusForeground == null) {
                return;
            }

            _hexFocusForegroundPosition.Position = FocusPosition;
            if(HexFocusedChar == HexChar.First) {
                _hexFocusForegroundPosition.FirstCharBrush = hexFocusForeground;
                _hexFocusForegroundPosition.SecondCharBrush = null;
            }
            else {
                _hexFocusForegroundPosition.FirstCharBrush = null;
                _hexFocusForegroundPosition.SecondCharBrush = hexFocusForeground;
            }

            AddHexForegroundPosition(_hexFocusForegroundPosition);
        }

        private void ClearFocusState() {
            //Restore/Update Focus Position;
            if (FocusPosition >= (Stream?.Length ?? 0))
                FocusPosition = -1;
        }
        
        private void KeyDownOnFocus(KeyEventArgs e) {
            if (!KeyValidator.IsArrowKey(e.Key)) {
                return;
            }

            if(Stream == null) {
                return;
            }

            e.Handled = true;
            if (FocusPosition == -1)
                return;

            if (ActivedPanel == LayerPanel.Hex) {
                if(e.Key == Key.Left && FocusPosition == 0 && HexFocusedChar == HexChar.First) {
                    return;
                }

                if(e.Key == Key.Right && FocusPosition == Stream.Length - 1 && HexFocusedChar == HexChar.Second) {
                    return;
                }

                if (e.Key == Key.Right || e.Key == Key.Left) {
                    HexFocusedChar = HexFocusedChar == HexChar.First ? HexChar.Second : HexChar.First;
                }

                if (HexFocusedChar == HexChar.Second && e.Key == Key.Right) {
                    return;
                }
                else if (HexFocusedChar == HexChar.First && e.Key == Key.Left) {
                    return;
                }
            }

            var previewFocusPosition = GetPreviewFocusPosition(e.Key);
            if (previewFocusPosition == null) {
                return;
            }

            
            SetFocusPositionAndScroll(previewFocusPosition.Value);
        }

        /// <summary>
        /// Set <see cref="FocusPosition"/> to <paramref name="focusPosition"/>,and change <see cref="Position"/> if neccessary.
        /// </summary>
        /// <param name="focusPosition"></param>
        private void SetFocusPositionAndScroll(long focusPosition) {
            if(focusPosition > Stream.Length - 1) {
                return;
            }

            FocusPosition = focusPosition;
            //Update scrolling Position(if neccessary.);
            var firstVisiblePosition = Position / BytePerLine * BytePerLine;
            var lastVisiblePosition = firstVisiblePosition + MaxVisibleLength - 1;
            if (FocusPosition < firstVisiblePosition) {
                Position -= BytePerLine;
            }
            else if (FocusPosition > lastVisiblePosition) {
                Position += BytePerLine;
            }
        }



        private long? GetPreviewFocusPosition(Key arrowKey) {
            switch (arrowKey) {
                case Key.Left:
                    if (FocusPosition > 0)
                        return FocusPosition - 1;

                    break;
                case Key.Up:
                    if (FocusPosition >= BytePerLine)
                        return FocusPosition - BytePerLine;

                    break;
                case Key.Right:
                    if (FocusPosition + 1 < Stream.Length)
                        return FocusPosition + 1;

                    break;
                case Key.Down:
                    if (FocusPosition + BytePerLine < Stream.Length)
                        return FocusPosition + BytePerLine;

                    break;
                default:
                    return null;
            }

            return null;
        }
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
    /// String encoding part.
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
    /// Hex/String ToolTip part.
    /// </summary>
    public partial class DrawedHexEditor {
        private void InitializeTooltipEvents() {
            HexDataLayer.MouseMoveOnCell += Datalayer_MouseMoveOnCell;
            StringDataLayer.MouseMoveOnCell += Datalayer_MouseMoveOnCell;
        }

        private long _mouseOverLevel;

        private void Datalayer_MouseMoveOnCell(object sender, MouseOnCellEventArgs arg) {
            var index = arg.CellIndex;
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

            HoverPosition = Position / BytePerLine * BytePerLine + arg.CellIndex;

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
    /// Caret Part;
    /// </summary>
    public partial class DrawedHexEditor {
        private readonly Stack<ByteCaretCell> _undoByteCarets = new Stack<ByteCaretCell>();
        private readonly Stack<ByteCaretCell> _redoByteCarets = new Stack<ByteCaretCell>();
        
        private readonly Dictionary<ByteCaret, IBrushBlock> _caretForegroundDict = new Dictionary<ByteCaret, IBrushBlock>();
        class ByteCaretCell {
            /// <summary>
            /// Create a <see cref="ByteCaretCell"/>;
            /// </summary>
            /// <param name="byteCarets">The bytecaret array,which should own more than one element.</param>
            public ByteCaretCell(ByteCaret[] byteCarets) {
                if(byteCarets == null || byteCarets.Length == 0) {
                    throw new ArgumentException($"{nameof(byteCarets)} should not be null or empty.");
                }

                ByteCarets = byteCarets;
            }

            public ByteCaret[] ByteCarets { get; }

            public bool IsCommited { get; set; }
        }


        public bool CanUndo => _undoByteCarets.Count != 0;
        public bool CanRedo => _redoByteCarets.Count != 0;

        public event EventHandler<CanRedoChangedEventArgs> CanUndoChanged;
        public event EventHandler<CanUndoChangedEventArgs> CanRedoChanged;

        public Brush CaretForeground {
            get { return (Brush)GetValue(CaretForegroundProperty); }
            set { SetValue(CaretForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CaretForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CaretForegroundProperty =
            DependencyProperty.Register(nameof(CaretForeground), typeof(Brush), typeof(DrawedHexEditor), new PropertyMetadata(Brushes.Blue));



        public bool IsReadOnly {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(DrawedHexEditor), new PropertyMetadata(false));



        private void TextInputOnHex(TextCompositionEventArgs e) {
            if (!CheckCanWrite()) {
                return;
            }

            if (ActivedPanel != LayerPanel.Hex) {
                return;
            }

            if(_viewBuffer == null || _viewBuffer.Length == 0) {
                return;
            }

            e.Handled = true;
            if(e.Text == null || e.Text.Length != 1) {
                return;
            }

            var (success, hexByte) = ByteConverters.HexToUniqueByte(e.Text);
            if (!success) {
                return;
            }

            //Get the byte that is presented in the FocusedPosition;
            var focusedBytePresented = GetPresentedByte(FocusPosition);
            if(focusedBytePresented == null) {
                focusedBytePresented = _viewBuffer[0];
                FocusPosition = Position;
            }

            var originByte = focusedBytePresented.Value;
            var modifiedByte = (byte)0;

            if (HexFocusedChar == HexChar.First) {
                modifiedByte = (byte)((hexByte << 4 & 0x0000_00F0) | (originByte & 0x0000_000F));
            }
            else {
                modifiedByte = (byte)((originByte & 0x0000_00F0) | (hexByte & 0x0000_000F));
            }

            if(modifiedByte != originByte) {
                var newCaret = CreateByteCaret(originByte, modifiedByte, FocusPosition);
                AddByteCaretToUndoBuffer(newCaret);
            }
            
            if(HexFocusedChar == HexChar.First) {
                HexFocusedChar = HexChar.Second;
            }
            else {
                if (FocusPosition < Stream.Length - 1) {
                    SetFocusPositionAndScroll(FocusPosition + 1);
                    HexFocusedChar = HexChar.First;
                }
            }

            UpdateContent();
        }

        private void TextInputOnString(TextCompositionEventArgs e) {
            if (!CheckCanWrite()) {
                return;
            }
            
            if (ActivedPanel != LayerPanel.String) {
                return;
            }

            if (_viewBuffer == null || _viewBuffer.Length == 0) {
                return;
            }

            e.Handled = true;

            if (string.IsNullOrEmpty(e.Text)) {
                return;
            }

            var offset = 0;
            foreach (var ch in e.Text) {
                if (FocusPosition + offset >= Stream.Length - 1) {
                    break;
                }
                
                //If the char is ascii byte,"replace" the byte in focusedposition;
                if(ch < byte.MaxValue / 2) {
                    //Get the byte that is presented in the FocusedPosition + index;
                    var originByte = GetPresentedByte(FocusPosition + offset);
                    var modifiedByte = (byte)ch;
                    if(originByte == null) {
                        break;
                    }

                    if(originByte != modifiedByte) {
                        var newCaret = CreateByteCaret(originByte.Value, modifiedByte, FocusPosition + offset);
                        AddByteCaretToUndoBuffer(newCaret);
                    }
                    
                    offset++;
                }
                else {
                    if(FocusPosition + offset + 2 >= Stream.Length - 1) {
                        break;
                    }

                    //Get the byte that is presented in the FocusedPosition + index;
                    var originByte0 = GetPresentedByte(FocusPosition + offset);
                    var originByte1 = GetPresentedByte(FocusPosition + offset + 1);
                    if(originByte0 == null || originByte1 == null) {
                        break;
                    }

                    var modifiedByte0 = (byte)(ch & 0x0000_00FF);
                    var modifiedByte1 = (byte)((ch & 0x0000_FF00) >> 8);

                    var newCaret0 = CreateByteCaret(originByte0.Value, modifiedByte0, FocusPosition + offset);
                    var newCaret1 = CreateByteCaret(originByte1.Value, modifiedByte1, FocusPosition + offset + 1);

                    
                    AddByteCaretToUndoBuffer(newCaret0, newCaret1);
                    offset += 2;
                }
            }

            UpdateContent();
            SetFocusPositionAndScroll(FocusPosition + offset);
        }

        private bool CheckCanWrite() {
            if(IsReadOnly || Stream == null || !Stream.CanWrite) {
                return false;
            }

            return true;
        }

        

        private void AddByteCaretToUndoBuffer(params (byte originByte,byte modifiedByte,long bytePosition)[] caretParams) {
            if(caretParams == null || caretParams.Length == 0) {
                throw new ArgumentException($"{nameof(caretParams)} can't be null or empty.");
            }

            var count = caretParams.Length;
            var carets = new ByteCaret[count];
            
            foreach (var (originByte, modifiedByte, bytePosition) in caretParams) {
                var caret = CreateByteCaret(originByte, modifiedByte, bytePosition);
            }
        }

        private ByteCaret CreateByteCaret(byte originByte, byte modifiedByte, long bytePosition) {
            var newCaret = new ByteCaret(originByte, modifiedByte, bytePosition) {
                ActivedPanel = ActivedPanel,
                Position = Position,
                FocusedChar = HexFocusedChar
            };

            return newCaret;
        }

        /// <summary>
        /// Add the <paramref name="byteCaret"/>To <see cref="_undoByteCarets"/>,and clear <see cref="_redoByteCarets"/>;
        /// </summary>
        /// <param name="byteCaret"></param>
        private void AddByteCaretToUndoBuffer(params ByteCaret[] byteCarets) {
            if(byteCarets == null) {
                throw new ArgumentException($"The {nameof(byteCarets)} can't be null or empty.");
            }

            _undoByteCarets.Push(new ByteCaretCell(byteCarets));
            _redoByteCarets.Clear();
        }

        private byte? GetPresentedByte(long bytePosition) {
            if(bytePosition < Position) {
                return null;
            }

            if (_viewBuffer == null || _viewBuffer.Length < bytePosition - Position) {
                return null;
            }

            return _viewBuffer[bytePosition - Position];
        }
        
        private void ClearCaretState() {
            _undoByteCarets.Clear();
            _redoByteCarets.Clear();
            _caretForegroundDict.Clear();
            RaiseCanUndoRedoChanged();
        }

        private IBrushBlock CreateOrGetForegroundBlockByCaret(ByteCaret byteCaret) {
            IBrushBlock brushBlockValue = null;
            if(_caretForegroundDict.TryGetValue(byteCaret,out var exisitingBrushBlock)) {
                brushBlockValue = exisitingBrushBlock;
            }
            else {
                brushBlockValue = new BrushBlock();
            }

            brushBlockValue.Brush = CaretForeground;
            brushBlockValue.Length = 1;
            brushBlockValue.StartOffset = byteCaret.BytePosition;

            return brushBlockValue;
        }


        private void UpdateCaretForegroundBlocks() {
            foreach (var caretCells in _undoByteCarets.Where(p => !p.IsCommited).Reverse()) {
                foreach (var caret in caretCells.ByteCarets) {
                    AddForegroundBlock(CreateOrGetForegroundBlockByCaret(caret));
                }
            }

            foreach (var caretCells in _redoByteCarets.Where(p => p.IsCommited).Reverse()) {
                foreach (var caret in caretCells.ByteCarets) {
                    AddForegroundBlock(CreateOrGetForegroundBlockByCaret(caret));
                }
            }
        }


        private void UpdateViewBufferFromCaret() {
            foreach (var caretCell in _undoByteCarets.Where(p => !p.IsCommited).Reverse()) {
                foreach (var caret in caretCell.ByteCarets) {
                    var caretOffset = caret.BytePosition - Position;
                    if (caretOffset < 0 || caretOffset >= _viewBuffer.Length) {
                        return;
                    }

                    _viewBuffer[caretOffset] = caret.ModifiedByte;
                }
            }

            foreach (var caretCell in _redoByteCarets.Where(p => p.IsCommited).Reverse()) {
                foreach (var caret in caretCell.ByteCarets) {
                    var caretOffset = caret.BytePosition - Position;
                    if (caretOffset < 0 || caretOffset >= _viewBuffer.Length) {
                        return;
                    }

                    _viewBuffer[caretOffset] = caret.OriginByte;
                }
            }
        }
        
        /// <summary>
        /// Save changes,this 
        /// </summary>
        public void SaveChanges() {
            foreach (var caretCell in _undoByteCarets.Where(p => !p.IsCommited)) {
                foreach (var caret in caretCell.ByteCarets) {
                    Stream.Position = caret.BytePosition;
                    Stream.WriteByte(caret.ModifiedByte);
                }

                caretCell.IsCommited = true;
            }

            foreach (var caretCell in _redoByteCarets.Where(p => p.IsCommited)) {
                foreach (var caret in caretCell.ByteCarets) {
                    Stream.Position = caret.BytePosition;
                    Stream.WriteByte(caret.OriginByte);
                }

                caretCell.IsCommited = false;
            }

            UpdateContent();
        }

        #region Undo/Redo Part;
        public void Undo() {
            if (!CanUndo) {
                return;
            }

            var caretCell = _undoByteCarets.Pop();
            _redoByteCarets.Push(caretCell);
            
            var lastByteCaret = caretCell.ByteCarets.First();
            SetFocusPositionAndScroll(lastByteCaret.BytePosition);
            ActivedPanel = lastByteCaret.ActivedPanel;

            if(lastByteCaret.ActivedPanel == LayerPanel.Hex) {
                HexFocusedChar = lastByteCaret.FocusedChar;
            }

            UpdateContent();

            RaiseCanUndoRedoChanged();
        }

        public void Redo() {
            if (!CanRedo) {
                return;
            }

            var caretCell = _redoByteCarets.Pop();
            _undoByteCarets.Push(caretCell);

            var lastByteCaret = caretCell.ByteCarets.First();
            SetFocusPositionAndScroll(lastByteCaret.BytePosition);
            ActivedPanel = lastByteCaret.ActivedPanel;

            if (lastByteCaret.ActivedPanel == LayerPanel.Hex) {
                HexFocusedChar = lastByteCaret.FocusedChar;
            }

            UpdateContent();

            RaiseCanUndoRedoChanged();
        }

        private void RaiseCanUndoRedoChanged() {
            CanUndoChanged?.Invoke(this, new CanRedoChangedEventArgs(CanUndo));
            CanRedoChanged?.Invoke(this, new CanUndoChangedEventArgs(CanRedo));
        }
        #endregion
    }
    
    
#if DEBUG
    public partial class DrawedHexEditor {
        ~DrawedHexEditor() {

        }
    }
#endif
}
