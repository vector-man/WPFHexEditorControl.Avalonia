//////////////////////////////////////////////
// Apache 2.0  - 2018-2019
// Author : Janus Tida
// Contributor: Derek Tremblay
//////////////////////////////////////////////

using System.Globalization;
using System.Windows;
using System.Windows.Media;
using WpfHexaEditor.Core.Interfaces;
using WpfHexaEditor.Core;

namespace WpfHexaEditor
{
    public abstract class FontControlBase : FrameworkElement, IFontControl
    {
        #region Global class variable
        private GlyphTypeface _glyphTypeface;
        private Typeface _typeface;
        private Size? _charSize;

#if NET47
        private double? _pixelPerDip;
#endif
        #endregion

        #region Base properties
        /// <summary>
        /// The size of font 
        /// </summary>
        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
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

        /// <summary>
        /// Cuz font size may affectrender,the CellSize and squareSize should be updated.
        /// </summary>        
        private static void FontSize_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is FontControlBase dataLB))
                return;

            dataLB.OnFontSizeChanged((double)e.OldValue, (double)e.NewValue);
        }

        protected virtual void OnFontSizeChanged(double oldFontSize, double newFontSize)
        {
            UpdateCharSize();
            UpdateFont();
        }

        /// <summary>
        /// Chose the font family
        /// </summary>
        public FontFamily FontFamily
        {
            get => (FontFamily)GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        // Using a DependencyProperty as the backing store for FontFamily.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontFamilyProperty =
            DependencyProperty.Register(nameof(FontFamily), typeof(FontFamily), typeof(FontControlBase),
                new FrameworkPropertyMetadata(
                    new FontFamily("Arial"),
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,
                    FontFamily_PropertyChanged
                ));

        private static void FontFamily_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is FontControlBase dataLB))
                return;

            dataLB.OnFontFamilyChanged((FontFamily)e.OldValue, (FontFamily)e.NewValue);
        }

        protected virtual void OnFontFamilyChanged(FontFamily oldFontSize, FontFamily newFontSize)
        {
            UpdateCharSize();
            UpdateFont();
        }

        public FontWeight FontWeight
        {
            get => (FontWeight)GetValue(FontWeightProperty);
            set => SetValue(FontWeightProperty, value);
        }

        public Brush Foreground
        {
            get => (Brush)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        // Using a DependencyProperty as the backing store for DefaultForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register(nameof(Foreground), typeof(Brush),
                typeof(FontControlBase),
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

            fontLB.OnFontWeightChanged((FontWeight)e.OldValue, (FontWeight)e.NewValue);
        }

        protected virtual void OnFontWeightChanged(FontWeight oldFontWeight, FontWeight newFontWeight)
        {
            UpdateCharSize();
            UpdateFont();
        }
        #endregion

        #region Methods

        /// <summary>
        /// Update CharSize...
        /// </summary>
        private void UpdateCharSize() => _charSize = null;

        /// <summary>
        /// Update the font
        /// </summary>
        private void UpdateFont() => _typeface = null;

        /// <summary>
        /// Get the formatted text as needed
        /// </summary>
        protected FormattedText GetFormattedText(string text, double fontSize, Brush foreground)
        {
#if NET451
            var formattedText = new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                TypeFace,
                fontSize,
                foreground
            );
#endif

#if NET47
            var formattedText = new FormattedText
            (
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                TypeFace,
                FontSize,
                foreground,
                PixelPerDip
            );
#endif

            return formattedText;
        }

        #endregion

        #region Other custom properties
#if NET47
        protected double PixelPerDip =>
            (_pixelPerDip ?? (_pixelPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip)).Value;

#endif

        protected Typeface TypeFace => _typeface ??
            (_typeface = new Typeface(FontFamily, new FontStyle(), FontWeight, new FontStretch()));


        //Get the size of every char text;
        public Size CharSize
        {
            get
            {
                if (_charSize == null)
                {
                    var glyphIndex = GlyphTypeface.CharacterToGlyphMap[ConstantReadOnly.WidestChar];

                    _charSize = new Size(
                        GlyphTypeface.AdvanceWidths[glyphIndex] * FontSize,
                        GlyphTypeface.AdvanceHeights[glyphIndex] * FontSize
                        );
                }

                return _charSize.Value;
            }
        }

        protected GlyphTypeface GlyphTypeface
        {
            get
            {
                if (_glyphTypeface == null && TypeFace != null)
                {
                    TypeFace.TryGetGlyphTypeface(out _glyphTypeface);
                }

                return _glyphTypeface;
            }
        }

        #endregion
    }

}
