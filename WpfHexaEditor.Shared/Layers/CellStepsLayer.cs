//////////////////////////////////////////////
// Apache 2.0  - 2018
// Author : Janus Tida
// Modified by : Derek Tremblay
//////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfHexaEditor.Core;
using WpfHexaEditor.Core.Bytes;
using WpfHexaEditor.Core.Interfaces;

namespace WpfHexaEditor.Layers {
    /// <summary>
    /// To show Stream Offsets(left of HexEditor) and Column Index(top of HexEditor);
    /// </summary>
    public class CellStepsLayer : FontControlBase, ICellsLayer, IOffsetsInfoLayer
    {

        public event EventHandler<(int cellIndex, MouseButtonEventArgs e)> MouseLeftDownOnCell;
        public event EventHandler<(int cellIndex, MouseButtonEventArgs e)> MouseLeftUpOnCell;
        public event EventHandler<(int cellIndex, MouseEventArgs e)> MouseMoveOnCell;
        public event EventHandler<(int cellIndex, MouseButtonEventArgs e)> MouseRightDownOnCell;

        public Thickness CellMargin { get; set; } = new Thickness(0);
        public Thickness CellPadding { get; set; } = new Thickness(0);

        //If datavisualtype is Hex,"ox" should be calculated.
        public virtual Size GetCellSize() => new Size(
            ((DataVisualType == DataVisualType.Hexadecimal ? 2 : 0) + SavedBits) * 
            CharSize.Width + CellPadding.Left + CellPadding.Right,
            CharSize.Height + CellPadding.Top + CellPadding.Bottom);
        
        public int SavedBits {
            get { return (int)GetValue(SavedBitsProperty); }
            set { SetValue(SavedBitsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SaveBits.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SavedBitsProperty =
            DependencyProperty.Register(nameof(SavedBits), typeof(int), typeof(CellStepsLayer),
                new FrameworkPropertyMetadata(2, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
        
        public Orientation Orientation
        {
            get => (Orientation) GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        // Using a DependencyProperty as the backing store for Orientation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(CellStepsLayer),
                new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsRender));

        public long StartStepIndex
        {
            get => (long) GetValue(StartStepIndexProperty);
            set => SetValue(StartStepIndexProperty, value);
        }

        // Using a DependencyProperty as the backing store for StartOffset.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartStepIndexProperty =
            DependencyProperty.Register(nameof(StartStepIndex), typeof(long), typeof(CellStepsLayer),
                new FrameworkPropertyMetadata(-1L, FrameworkPropertyMetadataOptions.AffectsRender));

        public int StepsCount
        {
            get => (int) GetValue(StepsProperty);
            set => SetValue(StepsProperty, value);
        }

        public DataVisualType DataVisualType { get; set; }

        // Using a DependencyProperty as the backing store for EndOffset.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StepsProperty =
            DependencyProperty.Register(nameof(StepsCount), typeof(int), typeof(CellStepsLayer),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));
        
        public int StepLength
        {
            get => (int) GetValue(StepLengthProperty);
            set => SetValue(StepLengthProperty, value);
        }

        // Using a DependencyProperty as the backing store for StepLength.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StepLengthProperty =
            DependencyProperty.Register(nameof(StepLength), typeof(int), typeof(CellStepsLayer),
                new PropertyMetadata(1));
#if DEBUG
        private readonly Stopwatch _watch = new Stopwatch();
#endif

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
#if DEBUG
            _watch.Restart();
#endif
            Func<int, Point> getOffsetLocation = null;

            var cellSize = GetCellSize();

            if (Orientation == Orientation.Horizontal)
            {
                getOffsetLocation = step => new Point(
                    (CellMargin.Left + CellMargin.Right + cellSize.Width) * step + CellMargin.Left + CellPadding.Left,
                    CellMargin.Top + CellPadding.Top
                );
            }
            else
            {
                getOffsetLocation = step => new Point(
                    CellMargin.Left + CellPadding.Left,
                    (CellMargin.Top + CellMargin.Bottom + cellSize.Height) * step + CellMargin.Top + CellPadding.Top
                );    
            }
            
            DrawSteps(drawingContext, getOffsetLocation);

#if DEBUG
            _watch.Stop();
            Debug.WriteLine($"Render Time for cellSteps Text:{_watch.ElapsedMilliseconds}");
#endif
        }

        private void DrawSteps(DrawingContext drawingContext, Func<int, Point> getOffsetLocation) {
            var fontSize = FontSize;
            var foreground = Foreground;

            for (var i = 0; i < StepsCount; i++) {
                DrawOneStep(
                    drawingContext,
                    i * StepLength + StartStepIndex,
                    getOffsetLocation(i),
                    fontSize,
                    foreground
                );
            }
        }

        private void DrawOneStep(DrawingContext drawingContext,long offSet, Point startPoint,double fontSize,Brush foreground) {
            string text = null;
            switch (DataVisualType) {
                case DataVisualType.Hexadecimal:
                    text = $"0x{ByteConverters.LongToHex(offSet, SavedBits)}";
                    break;
                case DataVisualType.Decimal:
                    text = ByteConverters.LongToString(offSet, SavedBits);
                    break;
            }

            DrawString(drawingContext, text, fontSize, foreground, ref startPoint);
        }

        private void DrawString(DrawingContext drawingContext, string text, double fontSize, Brush foreground, ref Point textPoint) {
            var glyphRun = CreateGlyphRun(text, fontSize, ref textPoint);

            if (glyphRun != null) {
                drawingContext.DrawGlyphRun(foreground, glyphRun);
            }
            else {
                var formattedText = GetFormattedText(text, fontSize, foreground);
                drawingContext.DrawText(formattedText, textPoint);
                return;
            }

            
        }

        private GlyphRun CreateGlyphRun(string text, double fontSize, ref Point position) {
            if (GlyphTypeface == null) {
                return null;
            }

            var glyphIndexes = new ushort[text.Length];
            var advancedWidths = new double[text.Length];

            var glyphWidth = 0D;

            var glyphHeight = GlyphTypeface.AdvanceHeights[0] * fontSize;

            for (int i = 0; i < text.Length; i++) {
                GlyphTypeface.CharacterToGlyphMap.TryGetValue(text[i], out var glyphIndex);

                glyphIndexes[i] = glyphIndex;

                //GlyphTypeface.AdvanceWidths.TryGetValue(glyphIndex, out glyphWidth);
                //glyphWidth *= fontSize;
                glyphWidth = CharSize.Width;

                advancedWidths[i] = glyphWidth;
            }

            var offsetPosition = new Point(position.X, position.Y + glyphHeight);
#if NET451
            var glyphRun = new GlyphRun(GlyphTypeface, 0, false , fontSize, glyphIndexes, offsetPosition, advancedWidths, null, null, null, null, null, null);
#endif
#if NET47
            var glyphRun = new GlyphRun(GlyphTypeface, 0, false, fontSize, (float)PixelPerDip, glyphIndexes, offsetPosition, advancedWidths, null, null, null, null, null, null);
#endif

            return glyphRun;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            availableSize = base.MeasureOverride(availableSize);
            
            if (Orientation == Orientation.Horizontal)
            {
                availableSize.Height = CellMargin.Top + CellMargin.Bottom + GetCellSize().Height;

                if (double.IsInfinity(availableSize.Width))
                    availableSize.Width = 0;
            }
            else
            {
                availableSize.Width = CellMargin.Left + CellMargin.Right + GetCellSize().Width;

                if (double.IsInfinity(availableSize.Height))
                    availableSize.Height = 0;
            }

            return availableSize;
        }

        private int? GetIndexFromLocation(Point location)
        {
            if (StartStepIndex == -1)
                return null;

            if (Orientation == Orientation.Horizontal)
            {
                if (!(location.Y > 0 && location.Y < CellMargin.Bottom + CellMargin.Top + GetCellSize().Height))
                    return null;

                var col = (int) (location.X / (GetCellSize().Width + CellMargin.Left + CellMargin.Right));
                if (col >= StepsCount)
                    return null;

                return col;
            }

            if (!(location.X > 0 && location.X < CellMargin.Left + CellMargin.Right + GetCellSize().Width))
                return null;

            var row = (int) (location.Y / (GetCellSize().Width + CellMargin.Top + CellMargin.Bottom));
            if (row >= StepsCount)
                return null;

            return row;
        }

        private int? GetIndexFromMouse(MouseEventArgs e) => 
            e == null ? null : GetIndexFromLocation(e.GetPosition(this));

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (e.Handled)
                return;

            var index = GetIndexFromMouse(e);
            if (index != null)
                MouseLeftDownOnCell?.Invoke(this, (index.Value, e));
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (e.Handled)
                return;

            var index = GetIndexFromMouse(e);
            if (index != null)
                MouseLeftUpOnCell?.Invoke(this, (index.Value, e));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.Handled)
                return;

            var index = GetIndexFromMouse(e);
            if (index != null)
                MouseMoveOnCell?.Invoke(this, (index.Value, e));
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            if (e.Handled)
                return;

            var index = GetIndexFromMouse(e);
            if (index != null)
                MouseRightDownOnCell?.Invoke(this, (index.Value, e));
        }

        public bool GetCellPosition(int index,ref Point position) => 
            throw new NotImplementedException();
    }

}
