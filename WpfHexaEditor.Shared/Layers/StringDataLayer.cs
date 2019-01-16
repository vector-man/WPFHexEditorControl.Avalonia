//////////////////////////////////////////////
// Apache 2.0  - 2018
// Author : Janus Tida
// Modified by : Derek Tremblay
//////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using WpfHexaEditor.Core;
using WpfHexaEditor.Core.Interfaces;

namespace WpfHexaEditor.Layers {
    public class StringDataLayer : DataLayerBase
    {
        public override Size GetCellSize() =>
            new Size(CellPadding.Right + CellPadding.Left + CharSize.Width,
                CharSize.Height + CellPadding.Top + CellPadding.Bottom);

        private byte[] _drawCharBuffer = null;

        protected override void DrawText(DrawingContext drawingContext) {
            if (Data == null) {
                return;
            }
                
            if(BytesToCharEncoding == null) {
                return;
            }

            var renderLines = GetRenderLines();

            if(renderLines == null) {
                return;
            }
            
            foreach (var renderLine in renderLines) {
                DrawRenderLine(drawingContext, renderLine);
            }

        }


        private IEnumerable<StringTextRenderLine> GetRenderLines() {
            if (BytesToCharEncoding == null)
                yield break;

            if (Data == null)
                yield break;


            var data = Data;
            var bytesToCharEncoding = BytesToCharEncoding;
            var bytePerLine = BytePerLine;
            var foreground = Foreground;
            var foregroundBlocks = ForegroundBlocks;
            var fontSize = FontSize;

            var textPosition = new Point();
            var cellSize = GetCellSize();
            var firstVisibleBtIndex = (int)(bytesToCharEncoding.BytePerChar - DataOffsetInOriginalStream % bytesToCharEncoding.BytePerChar) % bytesToCharEncoding.BytePerChar;
            var charCount = (data.Length - firstVisibleBtIndex) / bytesToCharEncoding.BytePerChar;
            var row = -1;
            StringTextRenderLine lastRenderLine = null;
            var charList = new List<char>();

            if (_drawCharBuffer == null || _drawCharBuffer.Length != bytesToCharEncoding.BytePerChar) {
                _drawCharBuffer = new byte[bytesToCharEncoding.BytePerChar];
            }

            for (int chIndex = 0; chIndex < charCount; chIndex++) {
                var btIndex = bytesToCharEncoding.BytePerChar * chIndex;
                var thisCol = btIndex % bytePerLine;
                var thisRow = btIndex / bytePerLine;
                var thisForeground = foreground;

                //Refresh foreground;
                if (foregroundBlocks != null) {
                    foreach (var brushBlock in foregroundBlocks) {
                        if (brushBlock.StartOffset <= btIndex && brushBlock.StartOffset + brushBlock.Length - 1 >= btIndex)
                            thisForeground = brushBlock.Brush;
                    }
                }

                Buffer.BlockCopy(data, btIndex + firstVisibleBtIndex, _drawCharBuffer, 0, bytesToCharEncoding.BytePerChar);
                
                var ch = bytesToCharEncoding.ConvertToChar(_drawCharBuffer);

                if (thisRow != row || lastRenderLine == null || lastRenderLine.Foreground != thisForeground) {

                    this.GetCellPosition(btIndex, ref textPosition);
                    if (lastRenderLine != null) {
                        lastRenderLine.Data = charList.ToArray();
                        yield return lastRenderLine;
                    }

                    charList.Clear();
                    lastRenderLine = new StringTextRenderLine {
                        Foreground = thisForeground,
                        StartPosition = textPosition
                    };

                    row = thisRow;
                }

                charList.Add(ch);
            }

            if (lastRenderLine != null) {
                lastRenderLine.Data = charList.ToArray();
                yield return lastRenderLine;
            }

        }

        private void DrawRenderLine(DrawingContext drawingContext, StringTextRenderLine bufferRenderLine) {
            if (bufferRenderLine == null) {
                throw new ArgumentNullException(nameof(bufferRenderLine));
            }

            if (bufferRenderLine.Data == null) {
                return;
            }

            var fontSize = FontSize;

            var startPosition = bufferRenderLine.StartPosition;
            startPosition.Y += GlyphTypeface.AdvanceHeights[0] * fontSize + CellMargin.Top + CellPadding.Top;
            
            var glyphIndexes = new ushort[bufferRenderLine.Data.Length * 3];
            var advanceWidths = new double[bufferRenderLine.Data.Length * 3];
            
            for (int i = 0; i < bufferRenderLine.Data.Length; i++) {
                GlyphTypeface.CharacterToGlyphMap.TryGetValue(' ', out glyphIndexes[3 * i]);
                GlyphTypeface.CharacterToGlyphMap.TryGetValue(bufferRenderLine.Data[i], out glyphIndexes[3 * i + 1]);
                GlyphTypeface.CharacterToGlyphMap.TryGetValue(' ', out glyphIndexes[3 * i + 2]);

                advanceWidths[3 * i] = CellMargin.Left + CellPadding.Right;
                advanceWidths[3 * i + 1] = CharSize.Width;
                advanceWidths[3 * i + 2] = CellPadding.Right + CellMargin.Right;
            }

#if NET47
            var glyph = new GlyphRun(GlyphTypeface, 0, false, fontSize, (float)PixelPerDip, glyphIndexes, startPosition, advanceWidths, null, null, null, null, null, null);
#endif

#if NET451
            var glyph = new GlyphRun(GlyphTypeface, 0, false, fontSize, glyphIndexes, startPosition, advanceWidths, null, null, null, null, null, null);
#endif
            drawingContext.DrawGlyphRun(bufferRenderLine.Foreground, glyph);
        }


        public IBytesToCharEncoding BytesToCharEncoding {
            get { return (IBytesToCharEncoding)GetValue(BytesToCharEncodingProperty); }
            set { SetValue(BytesToCharEncodingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BytesToCharConverterProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BytesToCharEncodingProperty =
            DependencyProperty.Register(nameof(BytesToCharEncoding), typeof(IBytesToCharEncoding), typeof(StringDataLayer), new FrameworkPropertyMetadata(BytesToCharEncodings.ASCII, FrameworkPropertyMetadataOptions.AffectsRender));

        class StringTextRenderLine {
            public char[] Data { get; set; }
            public Brush Foreground { get; set; }
            public Point StartPosition { get; set; }
        }
    }
}
