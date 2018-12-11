//////////////////////////////////////////////
// Apache 2.0  - 2018
// Author : Janus Tida
// Modified by : Derek Tremblay
//////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WpfHexaEditor.Core;
using WpfHexaEditor.Core.Bytes;
using WpfHexaEditor.Core.Interfaces;

namespace WpfHexaEditor
{
    public class HexDataLayer : DataLayerBase
    {
        public override Size GetCellSize() => new Size
        (
            2 * CharSize.Width + CellPadding.Left + CellPadding.Right,
            CellPadding.Top + CellPadding.Bottom + CharSize.Height
        );
        
        
        protected override void DrawTextOverride(DrawingContext drawingContext) {
            if (Data == null)
                return;

            var index = 0;

            var textPoint = new Point();
            var cellSize = GetCellSize();
            var fontSize = FontSize;
            var foreground = Foreground;

            foreach (var bt in Data) {
                var col = index % BytePerLine;
                var row = index / BytePerLine;
                var thisForeground = foreground;

                if (ForegroundBlocks != null)
                    foreach (var brushBlock in ForegroundBlocks) {
                        if (brushBlock.StartOffset <= index && brushBlock.StartOffset + brushBlock.Length - 1 >= index)
                            thisForeground = brushBlock.Brush;
                    }

                textPoint.X = (CellMargin.Right + CellMargin.Left + cellSize.Width) * col + CellPadding.Left + CellMargin.Left;
                textPoint.Y = (CellMargin.Top + CellMargin.Bottom + cellSize.Height) * row + CellPadding.Top + CellMargin.Top;

                var chs = ByteConverters.ByteToHexCharArray(bt);

                DrawCharArray(drawingContext, chs, fontSize, thisForeground,ref textPoint);
                

                index++;
            }
        }

        private void DrawCharArray(DrawingContext drawingContext, char[] charArray, double fontSize, Brush foreground, ref Point textPoint) {
            for (var chIndex = 0; chIndex < 2; chIndex++) {
                textPoint.X += CharSize.Width * chIndex;
                DrawString(drawingContext, charArray[chIndex].ToString(), fontSize, foreground,ref textPoint);
            }
                
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

        protected override void DrawBackgroundOverride(DrawingContext drawingContext) {
            base.DrawBackgroundOverride(drawingContext);

            return;

#if DEBUG
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

            HexBackgroundPositions = new IHexBrushPosition[] {
                new HexBrushPosition{
                    FirstCharBrush = slBrush,
                    Position = 12
                }
            };
#endif

            if(HexBackgroundPositions == null) {
                return;
            }

            if(Data == null) {
                return;
            }
            
            
            var cellSize = GetCellSize();
            var drawRect = new Rect(new Size(CharSize.Width,cellSize.Height));

            foreach (var hexBackgroundPosition in HexBackgroundPositions) {
                if(hexBackgroundPosition.Position < 0 || hexBackgroundPosition.Position > Data.Length - 1) {
                    continue;
                }

                
                var col = hexBackgroundPosition.Position % BytePerLine;
                var row = hexBackgroundPosition.Position / BytePerLine;

                var x = col * (CellMargin.Right + CellMargin.Left + cellSize.Width) + CellPadding.Left + CellMargin.Left;
                var y = row * (CellMargin.Top + CellMargin.Bottom + cellSize.Height)  + CellMargin.Top;

                drawRect.Y = y;
                
                if(hexBackgroundPosition.FirstCharBrush != null) {
                    drawRect.X = x;
                    drawingContext.DrawRectangle(hexBackgroundPosition.FirstCharBrush, null, drawRect);
                }
                
                if(hexBackgroundPosition.SecondCharBrush != null) {
                    drawRect.X = x + drawRect.Width;
                    drawingContext.DrawRectangle(hexBackgroundPosition.SecondCharBrush, null, drawRect);
                }
            }

            
        }
    }


}
