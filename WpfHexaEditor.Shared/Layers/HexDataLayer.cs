//////////////////////////////////////////////
// Apache 2.0  - 2018
// Author : Janus Tida
// Modified by : Derek Tremblay
//////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WpfHexaEditor.Core;
using WpfHexaEditor.Core.Bytes;
using WpfHexaEditor.Core.Interfaces;
using WpfHexaEditor.Events;

namespace WpfHexaEditor.Layers {
    public partial class HexDataLayer : DataLayerBase
    {
        private readonly char[] _cachedHexCharArr = new char[2];

        public override Size GetCellSize() => 
            new Size(2 * CharSize.Width + CellPadding.Left + CellPadding.Right,
                CellPadding.Top + CellPadding.Bottom + CharSize.Height);
        
        
        protected override void DrawText(DrawingContext drawingContext) {
            base.DrawText(drawingContext);

            DrawRenderLines(drawingContext);
            DrawHexForegroundPositions(drawingContext);
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
            var lastRenderLine = default(HexTextRenderLine);
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

                    var cellPosition = this.GetCellPosition(index);

                    if(cellPosition == null) {
                        continue;
                    }

                    if (lineReturned) {
                        lastRenderLine.Data = byteList.ToArray();
                        yield return lastRenderLine;
                    }

                    byteList.Clear();

                    lastRenderLine.Foreground = thisForeground;
                    lastRenderLine.CellStartPosition = cellPosition.Value;
                    
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
        
        private void DrawHexForegroundPositions(DrawingContext drawingContext) {
            if (GlyphTypeface == null) {
                return;
            }

            var hexForegroundPositions = HexForegroundPositions;

#if DEBUG
            //hexForegroundPositions = new IHexBrushPosition[] {new HexBrushPosition{
            //    Position = 0,
            //    FirstCharBrush = Brushes.Red,
            //    SecondCharBrush = Brushes.Blue
            //} };
#endif
            if (hexForegroundPositions == null) {
                return;
            }

            var yCharOffset = GlyphTypeface.AdvanceHeights[0] * FontSize + CellMargin.Top + CellPadding.Top;

            foreach (var hexForegroundPosition in hexForegroundPositions) {
                if (hexForegroundPosition.Position < 0 || hexForegroundPosition.Position >= Data.Length) {
                    continue;
                }
                
                if(hexForegroundPosition.FirstCharBrush == null && hexForegroundPosition.SecondCharBrush == null) {
                    continue;
                }

                var cellPosition = GetCellPosition(hexForegroundPosition.Position);
                if(cellPosition == null) {
                    continue;
                }
                
                
                var bt = Data[hexForegroundPosition.Position];
                ByteConverters.ByteToHexCharArray(bt, _cachedHexCharArr);

                if(hexForegroundPosition.FirstCharBrush != null) {
                    var run = CreateGlyphRunByChar(
                        _cachedHexCharArr[0],
                        new Point {
                            X = cellPosition.Value.X + CellMargin.Left + CellPadding.Left,
                            Y = cellPosition.Value.Y + yCharOffset
                        }
                    );
                    
                    drawingContext.DrawGlyphRun(hexForegroundPosition.FirstCharBrush, run);
                }

                if(hexForegroundPosition.SecondCharBrush != null) {
                    
                    var run = CreateGlyphRunByChar(
                        _cachedHexCharArr[1],
                        
                        new Point {
                            X = cellPosition.Value.X + CellMargin.Left + CellPadding.Left + CharSize.Width,
                            Y = cellPosition.Value.Y + yCharOffset
                        }
                    );
                    drawingContext.DrawGlyphRun(hexForegroundPosition.SecondCharBrush, run);
                }


            }
        }
        private GlyphRun CreateGlyphRunByChar(char ch,Point position) {
            if (!GlyphTypeface.CharacterToGlyphMap.TryGetValue(ch, out var glyphIndex)) {
                return null;
            }
            
            var glyphIndexes = new ushort[] { glyphIndex };
#if NET451
            var run = new GlyphRun(GlyphTypeface, 0, false, FontSize, glyphIndexes, position, new double[] { CharSize.Width }, null, null, null, null, null, null);
#endif


#if NET47
            var run = new GlyphRun(GlyphTypeface, 0, false, FontSize,(float)PixelPerDip, glyphIndexes, position, new double[] { CharSize.Width }, null, null, null, null, null, null);
#endif

            return run;
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
            

            foreach (var hexBackgroundPosition in hexBackgroundPositions) {
                if (hexBackgroundPosition.Position < 0 || hexBackgroundPosition.Position >= Data.Length) {
                    continue;
                }

                var blockPosition = GetCellPosition(hexBackgroundPosition.Position);

                if(blockPosition == null) {
                    continue;
                }
                
                drawRect.Y = blockPosition.Value.Y;

                if (hexBackgroundPosition.FirstCharBrush != null) {
                    drawRect.X = blockPosition.Value.X;
                    drawingContext.DrawRectangle(hexBackgroundPosition.FirstCharBrush, null, drawRect);
                }

                if (hexBackgroundPosition.SecondCharBrush != null) {
                    drawRect.X = blockPosition.Value.X + drawRect.Width;
                    drawingContext.DrawRectangle(hexBackgroundPosition.SecondCharBrush, null, drawRect);
                }
            }

        }
        
        private void DrawRenderLine(DrawingContext drawingContext, HexTextRenderLine bufferRenderLine) {
            if (bufferRenderLine.Data == null) {
                return;
            }
            
            if (GlyphTypeface == null) {
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
            DependencyProperty.Register(nameof(HexBackgroundPositions), typeof(IEnumerable<IHexBrushPosition>), typeof(HexDataLayer), new FrameworkPropertyMetadata(null,FrameworkPropertyMetadataOptions.AffectsRender));
        
        public IEnumerable<IHexBrushPosition> HexForegroundPositions {
            get { return (IEnumerable<IHexBrushPosition>)GetValue(HexForegroundPositionsProperty); }
            set { SetValue(HexForegroundPositionsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HexForegroundPositionsProperty =
            DependencyProperty.Register(nameof(HexForegroundPositions), typeof(IEnumerable<IHexBrushPosition>), typeof(HexDataLayer), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        
        /// <summary>
        /// HexTextRenderLine class;
        /// </summary>
        struct HexTextRenderLine {
            public byte[] Data { get; set; }
            public Brush Foreground { get; set; }
            public Point CellStartPosition { get; set; }
        }
    }

    /// <summary>
    /// Mouse Event Part;
    /// </summary>
    public partial class HexDataLayer {
        public event EventHandler<HexMouseButtonOnCellEventArgs> HexMouseDownOnCell;
        protected override void OnMouseDown(MouseButtonEventArgs e) {
            base.OnMouseDown(e);
            if (e.Handled) {
                return;
            }

            var position = e.GetPosition(this);

            var hexChar = GetHexCharByPosition(position);
            if (hexChar == null) {
                return;
            }

            var cellIndex = GetCellIndexByPosition(position);
            if (cellIndex == null) {
                return;
            }

            HexMouseDownOnCell?.Invoke(this, new HexMouseButtonOnCellEventArgs(hexChar.Value, cellIndex.Value, e));
        }

        public HexChar? GetHexCharByPosition(Point position) {
            if (Data == null)
                return null;

            var cellSize = GetCellSize();
            var unitWidth = CellMargin.Left + CellMargin.Right + cellSize.Width;
            var unitHeight = CellMargin.Top + CellMargin.Bottom + cellSize.Height;

            var xOffset = position.X % unitWidth;
            var yOffset = position.Y % unitHeight;

            if(yOffset < CellMargin.Top || yOffset > unitHeight - CellMargin.Bottom) {
                return null;
            }

            if(xOffset < CellMargin.Left || xOffset > unitWidth - CellMargin.Right) {
                return null;
            }

            return xOffset <= CellMargin.Left + CellPadding.Left + CharSize.Width ? HexChar.First : HexChar.Second;
        }
    }


}
