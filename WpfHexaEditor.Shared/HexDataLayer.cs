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

namespace WpfHexaEditor {
    public class HexDataLayer : DataLayerBase
    {
        private readonly char[] _cachedHexCharArr = new char[2];

        public override Size GetCellSize() => 
            new Size(2 * CharSize.Width + CellPadding.Left + CellPadding.Right,
                CellPadding.Top + CellPadding.Bottom + CharSize.Height);
        
        
        protected override void DrawText(DrawingContext drawingContext) {
            base.DrawText(drawingContext);

            var renderLines = GetRenderLines();
            if(renderLines == null) {
                return;
            }

            foreach (var textline in renderLines) {
                DrawRenderLine(drawingContext,textline);
            }

            var hexForegroundPositions = HexForegroundPositions;

            if(hexForegroundPositions == null) {
                return;
            }

            var textPosition = new Point();
            foreach (var hexForegroundPosition in hexForegroundPositions) {
                if (hexForegroundPosition.Position < 0 || hexForegroundPosition.Position >= Data.Length) {
                    continue;
                }

                this.GetCellContentPosition(hexForegroundPosition.Position,ref textPosition);


            }
        }

        

        protected override void DrawBackground(DrawingContext drawingContext) {
            base.DrawBackground(drawingContext);

            if (Data == null) {
                return;
            }

            /*HexBackgroundPositions Drawing*/

            var hexBackgroundPositions = HexBackgroundPositions;

#if DEBUG && FALSE
            var slBrush = new SolidColorBrush(Colors.Red);
            var animation = new ColorAnimationUsingKeyFrames {
                Duration = TimeSpan.FromSeconds(0.5),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            animation.KeyFrames.Add(new DiscreteColorKeyFrame(Colors.Red));
            animation.KeyFrames.Add(
                new DiscreteColorKeyFrame(
                    Colors.Transparent,
                    KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.25)
                )
            ));

            slBrush.BeginAnimation(
                SolidColorBrush.ColorProperty,
                animation
            );

            hexBackgroundPositions = new IHexBrushPosition[] {
                new HexBrushPosition{
                    FirstCharBrush = slBrush,
                    Position = 12
                }
            };
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
                
                GetCellPosition(hexBackgroundPosition.Position,ref blockPosition);
                
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
        
        private IEnumerable<HexTextRenderLine> GetRenderLines() {
            if (Data == null)
                yield break;
            
            var textPosition = new Point();
            var cellSize = GetCellSize();
            var fontSize = FontSize;
            var foreground = Foreground;


            var row = -1;
            var byteList = new List<byte>();
            HexTextRenderLine lastRenderLine = null;

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
                if (thisRow != row || byteList.Count == 0 ||
                    lastRenderLine == null || lastRenderLine.Foreground != thisForeground) {

                    this.GetCellContentPosition(index, ref textPosition);

                    if (lastRenderLine != null) {
                        lastRenderLine.Data = byteList.ToArray();
                        yield return lastRenderLine;
                    }

                    byteList.Clear();
                    lastRenderLine = new HexTextRenderLine {
                        Foreground = thisForeground,
                        StartPosition = textPosition
                    };
                }

                byteList.Add(Data[index]);
                
                row = thisRow;
            }
         

            if (lastRenderLine != null) {
                lastRenderLine.Data = byteList.ToArray();
                yield return lastRenderLine;
            }
        }
        
        private void DrawRenderLine(DrawingContext drawingContext, HexTextRenderLine bufferRenderLine) {
            if (bufferRenderLine == null) {
                throw new ArgumentNullException(nameof(bufferRenderLine));
            }

            if (bufferRenderLine.Data == null) {
                return;
            }

            var fontSize = FontSize;

            var startPosition = bufferRenderLine.StartPosition;
            startPosition.Y += GlyphTypeface.AdvanceHeights[0] * fontSize;
            bufferRenderLine.StartPosition = startPosition;

            var chArr = _cachedHexCharArr;
            var glyphIndexes = new ushort[bufferRenderLine.Data.Length * 2];
            var advanceWidths = new double[bufferRenderLine.Data.Length * 2];
            
            for (int i = 0; i < bufferRenderLine.Data.Length; i++) {
                ByteConverters.ByteToHexCharArray(bufferRenderLine.Data[i], chArr);

                glyphIndexes[2 * i] = GlyphTypeface.CharacterToGlyphMap[chArr[0]];
                glyphIndexes[2 * i + 1] = GlyphTypeface.CharacterToGlyphMap[chArr[1]];

                advanceWidths[2 * i] = GlyphTypeface.AdvanceWidths[glyphIndexes[2 * i]] * fontSize;
                advanceWidths[2 * i + 1] = GlyphTypeface.AdvanceWidths[glyphIndexes[2 * i + 1]] * fontSize + CellPadding.Left + CellPadding.Right + CellMargin.Right + CellMargin.Left;
            }

#if NET47
            var glyph = new GlyphRun(GlyphTypeface, 0, false, fontSize, (float)PixelPerDip, glyphIndexes, bufferRenderLine.StartPosition, advanceWidths, null, null, null, null, null, null);
#endif

#if NET451
            var glyph = new GlyphRun(GlyphTypeface, 0, false, fontSize, glyphIndexes, bufferRenderLine.StartPosition, advanceWidths, null, null, null, null, null, null);
#endif
            drawingContext.DrawGlyphRun(bufferRenderLine.Foreground, glyph);
        }
        
        public IEnumerable<IHexBrushPosition> HexForegroundPositions {
            get => (IEnumerable < IHexBrushPosition > )GetValue(HexForegroundPositionsProperty);
            set => SetValue(HexForegroundPositionsProperty, value); 
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HexForegroundPositionsProperty =
            DependencyProperty.Register(nameof(HexForegroundPositions), typeof(IEnumerable<IHexBrushPosition>), typeof(HexDataLayer), new PropertyMetadata(null));
        
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
        class HexTextRenderLine {
            public byte[] Data { get; set; }
            public Brush Foreground { get; set; }
            public Point StartPosition { get; set; }
        }
    }


}
