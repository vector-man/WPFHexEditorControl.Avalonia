//////////////////////////////////////////////
// Apache 2.0  - 2018
// Author : Janus Tida
// Modified by : Derek Tremblay
//////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WpfHexaEditor.Core;
using WpfHexaEditor.Core.Bytes;
using WpfHexaEditor.Core.Interfaces;

namespace WpfHexaEditor.Layers {
    public class HexDataLayer : DataLayerBase
    {
        private readonly char[] _cachedHexCharArr = new char[2];
        private Point _cachedCellPosition = new Point();

        public override Size GetCellSize() => 
            new Size(2 * CharSize.Width + CellPadding.Left + CellPadding.Right,
                CellPadding.Top + CellPadding.Bottom + CharSize.Height);
        
        
        protected override void DrawText(DrawingContext drawingContext) {
            base.DrawText(drawingContext);

            DrawRenderLines(drawingContext);
        }

        private void DrawRenderLines(DrawingContext drawingContext) {
            var renderLines = GetRenderLines();
            if (renderLines == null) {
                return;
            }

            foreach (var textline in renderLines) {
                DrawRenderLine(drawingContext, textline);
            }
        }


        private IEnumerable<HexTextRenderLine> GetRenderLines() {
            if (Data == null)
                yield break;

            var cellSize = GetCellSize();
            var fontSize = FontSize;
            var foreground = Foreground;


            var row = -1;
            var byteList = new List<byte>();
            HexTextRenderLine lastRenderLine = default;
            var lineReturned = false;

            for (var index = 0; index < Data.Length; index++) {
                var thisRow = index / BytePerLine;
                var thisForeground = foreground;

                if (ForegroundBlocks != null) {
                    foreach (var brushBlock in ForegroundBlocks) {
                        if (brushBlock.StartOffset <= index && brushBlock.StartOffset + brushBlock.Length - 1 >= index)
                            thisForeground = brushBlock.Brush;
                    }
                }

                ///We will add new <see cref="HexTextRenderLine"/> to the buffer<see cref="textLineList"/>;
                if (thisRow != row || !lineReturned || lastRenderLine.Foreground != thisForeground) {

                    this.GetCellPosition(index, ref _cachedCellPosition);

                    if (lineReturned) {
                        lastRenderLine.Data = byteList.ToArray();
                        yield return lastRenderLine;
                    }

                    byteList.Clear();

                    lastRenderLine.Foreground = thisForeground;
                    lastRenderLine.CellStartPosition = _cachedCellPosition;
                    
                    lineReturned = true;
                }

                byteList.Add(Data[index]);

                row = thisRow;
            }


            if (lineReturned) {
                lastRenderLine.Data = byteList.ToArray();
                yield return lastRenderLine;
            }
        }

        private void DrawHexForegroundPositions() {

        }

        protected override void DrawBackground(DrawingContext drawingContext) {
            base.DrawBackground(drawingContext);

            DrawHexBackgroundPositions(drawingContext);
        }

        private void DrawHexBackgroundPositions(DrawingContext drawingContext) {

            if (Data == null) {
                return;
            }

            /*HexBackgroundPositions Drawing*/

            var hexBackgroundPositions = HexBackgroundPositions;

#if DEBUG 

            //hexBackgroundPositions = new IHexBrushPosition[] {
            //     new HexBrushPosition{
            //        FirstCharBrush = Brushes.Red,
            //        SecondCharBrush = Brushes.Purple,
            //        Position = 0
            //    }
            //};
#endif


            if (hexBackgroundPositions == null) {
                return;
            }


            var drawRect = new Rect(new Size(CharSize.Width + CellPadding.Left, CharSize.Height + CellPadding.Top + CellPadding.Bottom));
            var cellSize = GetCellSize();
            var blockPosition = new Point();

            foreach (var hexBackgroundPosition in hexBackgroundPositions) {
                if (hexBackgroundPosition.Position < 0 || hexBackgroundPosition.Position >= Data.Length) {
                    continue;
                }

                
                GetCellPosition(hexBackgroundPosition.Position, ref blockPosition);

                drawRect.Y = blockPosition.Y;

                if (hexBackgroundPosition.FirstCharBrush != null) {
                    drawRect.X = blockPosition.X;
                    drawingContext.DrawRectangle(hexBackgroundPosition.FirstCharBrush, null, drawRect);
                }

                if (hexBackgroundPosition.SecondCharBrush != null) {
                    drawRect.X = blockPosition.X + drawRect.Width;
                    drawingContext.DrawRectangle(hexBackgroundPosition.SecondCharBrush, null, drawRect);
                }
            }

        }
        
        private void DrawRenderLine(DrawingContext drawingContext, HexTextRenderLine bufferRenderLine) {

            if (bufferRenderLine.Data == null) {
                return;
            }

            var fontSize = FontSize;

            var textStartCellPosition = bufferRenderLine.CellStartPosition;
            textStartCellPosition.Y += GlyphTypeface.AdvanceHeights[0] * fontSize + CellMargin.Top + CellPadding.Top;
            
            var glyphIndexes = new ushort[bufferRenderLine.Data.Length * 4];
            var advanceWidths = new double[bufferRenderLine.Data.Length * 4];

            for (int i = 0; i < bufferRenderLine.Data.Length; i++) {
                ByteConverters.ByteToHexCharArray(bufferRenderLine.Data[i], _cachedHexCharArr);

                GlyphTypeface.CharacterToGlyphMap.TryGetValue(' ',out glyphIndexes[4 * i]);
                GlyphTypeface.CharacterToGlyphMap.TryGetValue(_cachedHexCharArr[0], out glyphIndexes[4 * i + 1]);
                GlyphTypeface.CharacterToGlyphMap.TryGetValue(_cachedHexCharArr[1], out glyphIndexes[4 * i + 2]);
                GlyphTypeface.CharacterToGlyphMap.TryGetValue(' ',out glyphIndexes[4 * i + 3]);

                advanceWidths[4 * i] = CellMargin.Left + CellPadding.Left;
                advanceWidths[4 * i + 1] = CharSize.Width;
                advanceWidths[4 * i + 2] = CharSize.Width;
                advanceWidths[4 * i + 3] = CellPadding.Right + CellMargin.Right;
            }

#if NET47
            var glyph = new GlyphRun(GlyphTypeface, 0, false, fontSize, (float)PixelPerDip, glyphIndexes, textStartCellPosition, advanceWidths, null, null, null, null, null, null);
#endif

#if NET451
            var glyph = new GlyphRun(GlyphTypeface, 0, false, fontSize, glyphIndexes, textStartCellPosition, advanceWidths, null, null, null, null, null, null);
#endif
            drawingContext.DrawGlyphRun(bufferRenderLine.Foreground, glyph);
        }
        
        public IEnumerable<IHexBrushPosition> HexBackgroundPositions {
            get => (IEnumerable<IHexBrushPosition>)GetValue(HexBackgroundPositionsProperty); 
            set => SetValue(HexBackgroundPositionsProperty, value); 
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HexBackgroundPositionsProperty =
            DependencyProperty.Register(nameof(HexBackgroundPositions), typeof(IEnumerable<IHexBrushPosition>), typeof(HexDataLayer), new PropertyMetadata(null));

        /// <summary>
        /// HexTextRenderLine class;
        /// </summary>
        struct HexTextRenderLine {
            public byte[] Data { get; set; }
            public Brush Foreground { get; set; }
            public Point CellStartPosition { get; set; }
        }
    }


}
