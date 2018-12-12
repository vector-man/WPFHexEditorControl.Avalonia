//////////////////////////////////////////////
// Apache 2.0  - 2018
// Author : Janus Tida
// Modified by : Derek Tremblay
//////////////////////////////////////////////

using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using WpfHexaEditor.Core.Interfaces;

namespace WpfHexaEditor
{
    public abstract class FontControlBase : FrameworkElement, IFontControl
    {
        public double FontSize
        {
            get => (double) GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        // Using a DependencyProperty as the backing store for FontSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register(nameof(FontSize), typeof(double), typeof(FontControlBase),
                new FrameworkPropertyMetadata(
                    12.0D,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,
                    FontSize_PropertyChanged
                ));

        //Cuz font size may affectrender,the CellSize and squareSize should be updated.
        private static void FontSize_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is FontControlBase dataLB))
                return;

            dataLB.OnFontSizeChanged((double) e.OldValue, (double) e.NewValue);
        }

        protected virtual void OnFontSizeChanged(double oldFontSize, double newFontSize) {
            UpdateCharSize();
            UpdateFont();
        }
          

        /// <summary>
        /// Update CharSize...
        /// </summary>
        private void UpdateCharSize() => _charSize = null;

        private void UpdateFont() {
            _typeface = null;
        }


        public FontFamily FontFamily
        {
            get => (FontFamily) GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        // Using a DependencyProperty as the backing store for FontFamily.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontFamilyProperty =
            DependencyProperty.Register(nameof(FontFamily), typeof(FontFamily), typeof(FontControlBase),
                new FrameworkPropertyMetadata(
                    new FontFamily("Microsoft YaHei"),
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,
                    FontFamily_PropertyChanged
                ));

        private static void FontFamily_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is FontControlBase dataLB))
                return;
             
            dataLB.OnFontFamilyChanged((FontFamily) e.OldValue, (FontFamily) e.NewValue);
        }

        protected virtual void OnFontFamilyChanged(FontFamily oldFontSize, FontFamily newFontSize) {
            UpdateCharSize();
            UpdateFont();
        }

        public FontWeight FontWeight
        {
            get => (FontWeight) GetValue(FontWeightProperty);
            set => SetValue(FontWeightProperty, value);
        }

        public Brush Foreground
        {
            get => (Brush) GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        // Using a DependencyProperty as the backing store for DefaultForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register(nameof(Foreground), typeof(Brush),
                typeof(DataLayerBase),
                new FrameworkPropertyMetadata(
                    Brushes.Black,
                    FrameworkPropertyMetadataOptions.AffectsRender
                ));


        // Using a DependencyProperty as the backing store for FontWeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontWeightProperty =
            DependencyProperty.Register(nameof(FontWeight), typeof(FontWeight), typeof(FontControlBase),
                new FrameworkPropertyMetadata(
                    new FontWeight(),
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,
                    FontWeight_PropertyChanged
                ));

        private static void FontWeight_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is FontControlBase fontLB))
                return;

            fontLB.OnFontWeightChanged((FontWeight) e.OldValue, (FontWeight) e.NewValue);
        }

        protected virtual void OnFontWeightChanged(FontWeight oldFontWeight, FontWeight newFontWeight) {
            UpdateCharSize();
            UpdateFont();
        }
            


#if NET47
        protected double PixelPerDip =>
            (_pixelPerDip ?? (_pixelPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip)).Value;

        private double? _pixelPerDip;
#endif

        protected Typeface TypeFace => _typeface ??
                                       (_typeface = new Typeface(FontFamily, new FontStyle(), FontWeight,
                                           new FontStretch()));

        private Typeface _typeface;

        //"D" may have the "widest" size
        public const char WidestChar = 'D';

        private Size? _charSize;

        //Get the size of every char text;
        public Size CharSize
        {
            get {
                if (_charSize == null) {
                    //Cuz "D" may have the "widest" size,we got the char width when the char is 'D';
                    //var width = GlyphTypeface.AdvanceWidths[WidestChar - 29];
                    //var height = GlyphTypeface.AdvanceHeights[WidestChar - 29];
                    //_charSize = new Size(width, height);
                    //return _charSize.Value;

                    var typeface = new Typeface(FontFamily, new FontStyle(), new FontWeight(), new FontStretch());
#if NET451
                var measureText = new FormattedText(
                    WidestChar.ToString(), CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight, typeface, FontSize, Brushes.Black
                );
#endif
#if NET47
                var measureText = new FormattedText(
                    WidestChar.ToString(), CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight, typeface, FontSize, Brushes.Black,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip
                );
#endif
                    _charSize = new Size(measureText.Width, measureText.Height);
                }

                return _charSize.Value;
            }
        }

        private GlyphTypeface _glyphTypeface;
        protected GlyphTypeface GlyphTypeface {
            get {
                if(_glyphTypeface == null && TypeFace != null) {
                    TypeFace.TryGetGlyphTypeface(out _glyphTypeface);
                }

                return _glyphTypeface;
            }
        }

        protected GlyphRun CreateGlyphRun(string text,double fontSize,ref Point position) {
            if (GlyphTypeface == null) {
                return null;
            }

            var glyphIndexes = new ushort[text.Length];
            var advancedWidths = new double[text.Length];
            
            var glyphWidth = 0D;

            var glyphHeight = GlyphTypeface.AdvanceHeights[0] * fontSize;

            for (int i = 0; i < text.Length; i++) {
                var glyphIndex = (ushort)(text[i] - 29);
                glyphIndexes[i] = glyphIndex;

                if (GlyphTypeface.AdvanceWidths.Count > glyphIndex) {
                    glyphWidth = GlyphTypeface.AdvanceWidths[glyphIndex] * fontSize;
                }
                else {
                    glyphWidth = GlyphTypeface.AdvanceWidths[0] * fontSize;
                }
                
                advancedWidths[i] = glyphWidth;
            }

            var offsetPosition = new Point(position.X , position.Y + glyphHeight);
#if NET451
            var glyphRun = new GlyphRun(GlyphTypeface, 0, false , fontSize, glyphIndexes, offsetPosition, advancedWidths, null, null, null, null, null, null);
#endif
#if NET47
            var glyphRun = new GlyphRun(GlyphTypeface, 0, false, fontSize,(float) PixelPerDip, glyphIndexes, offsetPosition, advancedWidths, null, null, null, null, null, null);
#endif
            
            return glyphRun;
        }

        protected void DrawString(DrawingContext drawingContext, string text, double fontSize, Brush foreground, ref Point textPoint) {
            var glyphRun = CreateGlyphRun(text, fontSize, ref textPoint);

            if (glyphRun != null)  {
                drawingContext.DrawGlyphRun(foreground, glyphRun);
            }
            else {
                var formattedText = GetFormattedText(text, fontSize, foreground);
                drawingContext.DrawText(formattedText, textPoint);
            }
        }

        protected FormattedText GetFormattedText(string text, double fontSize, Brush foreground) {
#if NET451
            var formattedText = new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, TypeFace,
                fontSize,
                foreground
            );
#endif

#if NET47
            var formattedText = new FormattedText
            (
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, TypeFace, FontSize,
                foreground, PixelPerDip
            );
#endif

            return formattedText;
        }
    }
}
