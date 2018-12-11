//////////////////////////////////////////////
// Apache 2.0  - 2018
// Author : Janus Tida
// Modified by : Derek Tremblay
//////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using WpfHexaEditor.Core;
using WpfHexaEditor.Core.Bytes;
using WpfHexaEditor.Core.Interfaces;

namespace WpfHexaEditor
{
    public class StringDataLayer : DataLayerBase
    {
        public override Size GetCellSize() =>
            new Size(CellPadding.Right + CellPadding.Left + CharSize.Width,
                CharSize.Height + CellPadding.Top + CellPadding.Bottom);

        private byte[] _drawCharBuffer = null;

        protected override void DrawTextOverride(DrawingContext drawingContext) {
            if (Data == null)
                return;

            if (BytesToCharEncoding == null)
                return;
            
            if(_drawCharBuffer == null || _drawCharBuffer.Length != BytesToCharEncoding.BytePerChar) {
                _drawCharBuffer = new byte[BytesToCharEncoding.BytePerChar];
            }
            
           
            var data = Data;
            var bytesToCharEncoding = BytesToCharEncoding;
            var bytePerLine = BytePerLine;
            var foreground = Foreground;
            var foregroundBlocks = ForegroundBlocks;
            var fontSize = FontSize;

            var textPoint = new Point();
            var cellSize = GetCellSize();
            var firstVisibleBtIndex = (int)(bytesToCharEncoding.BytePerChar - DataOffsetInOriginalStream % bytesToCharEncoding.BytePerChar) % bytesToCharEncoding.BytePerChar;
            var charCount = (data.Length - firstVisibleBtIndex) / bytesToCharEncoding.BytePerChar;
            
            for (int chIndex = 0; chIndex < charCount; chIndex++) {    
                var btIndex = bytesToCharEncoding.BytePerChar * chIndex;
                var col = btIndex % bytePerLine;
                var row = btIndex / bytePerLine;
                var thisForeground = foreground;

                if (foregroundBlocks != null) {
                    foreach (var brushBlock in foregroundBlocks) {
                        if (brushBlock.StartOffset <= btIndex && brushBlock.StartOffset + brushBlock.Length - 1 >= btIndex)
                            thisForeground = brushBlock.Brush;
                    }
                }
                
                Buffer.BlockCopy(data, btIndex + firstVisibleBtIndex, _drawCharBuffer, 0, bytesToCharEncoding.BytePerChar);

                textPoint.X = (CellMargin.Right + CellMargin.Left + cellSize.Width) * col + CellPadding.Left + CellMargin.Left;
                textPoint.Y = (CellMargin.Top + CellMargin.Bottom + cellSize.Height) * row + CellPadding.Top + CellMargin.Top;

                //var formattedText = GetFormattedText(bytesToCharEncoding, fontSize, thisForeground,_drawCharBuffer);

                DrawString(drawingContext, bytesToCharEncoding.Convert(_drawCharBuffer).ToString(),fontSize,thisForeground, ref textPoint);

                //DrawByteWithGlyph(drawingContext, bytesToCharEncoding.Convert(_drawCharBuffer), thisForeground, ref textPoint);
            }
        }
        

      
        
        public IBytesToCharEncoding BytesToCharEncoding {
            get { return (IBytesToCharEncoding)GetValue(BytesToCharEncodingProperty); }
            set { SetValue(BytesToCharEncodingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BytesToCharConverterProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BytesToCharEncodingProperty =
            DependencyProperty.Register(nameof(BytesToCharEncoding), typeof(IBytesToCharEncoding), typeof(StringDataLayer), new FrameworkPropertyMetadata(BytesToCharEncodings.ASCII, FrameworkPropertyMetadataOptions.AffectsRender));
        
    }
}
