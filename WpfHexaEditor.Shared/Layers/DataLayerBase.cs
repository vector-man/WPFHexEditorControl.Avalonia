//////////////////////////////////////////////
// Apache 2.0  - 2018
// Author : Janus Tida
// Modified by : Derek Tremblay
//////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WpfHexaEditor.Core;
using WpfHexaEditor.Core.Interfaces;
using WpfHexaEditor.Events;

namespace WpfHexaEditor.Layers {
    public abstract class DataLayerBase : FontControlBase, IDataLayer, ICellsLayer
    {

        public event EventHandler<MouseButtonOnCellEventArgs> MouseDownOnCell;
        public event EventHandler<MouseButtonOnCellEventArgs> MouseUpOnCell;
        public event EventHandler<MouseOnCellEventArgs> MouseMoveOnCell;

        public byte[] Data
        {
            get => (byte[]) GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }
        
        // Using a DependencyProperty as the backing store for DataProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(
                nameof(Data),
                typeof(byte[]),
                typeof(DataLayerBase),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    DataProperty_Changed
                )
            );

        private static void DataProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is DataLayerBase ctrl))
                return;

            ctrl.InitializeMouseState();
        }
        

        public IEnumerable<IBrushBlock> ForegroundBlocks
        {
            get => (IEnumerable<IBrushBlock>) GetValue(ForegroundBlocksProperty);
            set => SetValue(ForegroundBlocksProperty, value);
        }

        // Using a DependencyProperty as the backing store for ForegroundBlocks.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ForegroundBlocksProperty =
            DependencyProperty.Register(nameof(ForegroundBlocks),
                typeof(IEnumerable<IBrushBlock>),
                typeof(DataLayerBase),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender
                ));

        public IEnumerable<IBrushBlock> BackgroundBlocks
        {
            get => (IEnumerable<IBrushBlock>) GetValue(BackgroundBlocksProperty);
            set => SetValue(BackgroundBlocksProperty, value);
        }

        // Using a DependencyProperty as the backing store for BackgroundBlocks.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundBlocksProperty =
            DependencyProperty.Register(nameof(BackgroundBlocks),
                typeof(IEnumerable<IBrushBlock>),
                typeof(DataLayerBase),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender
                ));


        public int BytePerLine
        {
            get => (int) GetValue(BytePerLineProperty);
            set => SetValue(BytePerLineProperty, value);
        }

        // Using a DependencyProperty as the backing store for BytePerLine.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BytePerLineProperty =
            DependencyProperty.Register(nameof(BytePerLine), typeof(int), typeof(DataLayerBase),
                new FrameworkPropertyMetadata(
                    16,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    BytePerLine_PropertyChanged
                ));

        private static void BytePerLine_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (!(d is DataLayerBase ctrl) || e.NewValue == e.OldValue) return;
            ctrl.InvalidateMeasure();
        }

        public Thickness CellPadding { get; set; } = new Thickness(0);
        public Thickness CellMargin { get; set; } = new Thickness(0);
#if DEBUG
        private readonly Stopwatch _watch = new Stopwatch();
#endif

        public int AvailableRowsCount =>
            (int) (ActualHeight / (GetCellSize().Height + CellMargin.Top + CellMargin.Bottom));

        public abstract Size GetCellSize();
        

        public Brush Background
        {
            get => (Brush) GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        public int PositionStartToShow { get; set; }

        protected IEnumerable<byte> GetBytesFromData(int offset,int length) {
            if (Data == null)
                yield break;

            for (int index = 0; index < length; index++) {
                if(index + offset > Data.Length || index + offset < 0) {
                    yield break;
                }
                else {
                    yield return Data[index + offset];
                }
            }
        }

        // Using a DependencyProperty as the backing store for Background.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register(nameof(Background), typeof(Brush), typeof(DataLayerBase),
                new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));


        protected virtual void DrawBackground(DrawingContext drawingContext)
        {
            if (BackgroundBlocks == null)
                return;

            
            if (Data == null)
                return;

            var drawRect = new Rect {
                Width = ActualWidth,
                Height = ActualHeight
            };

            drawingContext.DrawRectangle(Background, null, drawRect);

            
            Brush backgroundBrush = null;
            var cellSize = GetCellSize();

            for (var i = 0; i < Data.Length; i++)
            {
                backgroundBrush = null;

                foreach (var block in BackgroundBlocks) {
                    if(block.StartOffset <= i && block.StartOffset + block.Length > i) {
                        backgroundBrush = block.Brush;
                    }
                }

                if(backgroundBrush == null) {
                    continue;
                }
                
                var col = i % BytePerLine;
                var row = i / BytePerLine;
                
                drawRect.X = col * (CellMargin.Right + CellMargin.Left + cellSize.Width) + CellMargin.Left;
                drawRect.Y = row * (CellMargin.Top + CellMargin.Bottom + cellSize.Height) + CellMargin.Top;
                drawRect.Height = cellSize.Height;
                drawRect.Width = cellSize.Width;

                drawingContext.DrawRectangle(
                    backgroundBrush,
                    null,
                    drawRect
                );
            }
            
        }
        

        protected virtual void DrawText(DrawingContext drawingContext) {
            
        }
  
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
#if DEBUG
            _watch.Restart();
#endif

            DrawBackground(drawingContext);

#if DEBUG
            _watch.Stop();
            Debug.WriteLine($"Render Time for layer Background:{_watch.ElapsedMilliseconds}");
#endif
            
#if DEBUG
            _watch.Restart();
#endif
            DrawText(drawingContext);
#if DEBUG
            _watch.Stop();
            Debug.WriteLine($"Render Time for layer Text:{_watch.ElapsedMilliseconds}");
#endif
            
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            availableSize.Width = (GetCellSize().Width + CellMargin.Left + CellMargin.Right) * BytePerLine;
            
            if (double.IsInfinity(availableSize.Height))
                availableSize.Height = 0;

            return availableSize;
        }

        protected int? GetCellIndexByPosition(Point location)
        {
            if (Data == null)
                return null;

            var col = (int) (location.X / (CellMargin.Left + CellMargin.Right + GetCellSize().Width));
            var row = (int) (location.Y / (CellMargin.Top + CellMargin.Bottom + GetCellSize().Height));

            if (row * BytePerLine + col < Data.Length)
                return row * BytePerLine + col;

            return null;
        }

        private int? GetIndexFromMouse(MouseEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            return GetCellIndexByPosition(e.GetPosition(this));
        }

        protected override void OnMouseDown(MouseButtonEventArgs e) {
            base.OnMouseDown(e);

            if (e.Handled)
                return;

            if (Data == null)
                return;

            var index = GetIndexFromMouse(e);
            if (index != null)
                MouseDownOnCell?.Invoke(this, new MouseButtonOnCellEventArgs(index.Value, e));

        }

        protected override void OnMouseUp(MouseButtonEventArgs e) {
            base.OnMouseUp(e);
            if (e.Handled)
                return;

            var index = GetIndexFromMouse(e);
            if (index != null)
                MouseUpOnCell?.Invoke(this, new MouseButtonOnCellEventArgs(index.Value, e));
        }
        
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.Handled)
                return;

            var index = GetIndexFromMouse(e);
            if (index != null)
            {
                if (index == lastMouseMoveIndex)
                    return;

                lastMouseMoveIndex = index;
                MouseMoveOnCell?.Invoke(this,new MouseOnCellEventArgs(index.Value, e));
            }
        }
        
        private int? lastMouseMoveIndex;


        private void InitializeMouseState() => 
            lastMouseMoveIndex = null;

        public Point? GetCellPosition(long index)
        {
            if (Data == null)
                return null;

            if (index > Data.Length)
                throw new IndexOutOfRangeException($"{nameof(index)} is larger than elements.");

            var position = new Point();
            var col = index % BytePerLine;
            var row = index / BytePerLine;

            position.X = (GetCellSize().Width + CellMargin.Left + CellMargin.Right) * col;
            position.Y = (GetCellSize().Height + CellMargin.Top + CellMargin.Bottom) * row;

            return position;
        }

        
    }

}
