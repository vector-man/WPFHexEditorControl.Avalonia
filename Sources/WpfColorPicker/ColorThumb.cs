//////////////////////////////////////////////
// 2006 - Microsoft 
//        https://blogs.msdn.microsoft.com/wpfsdk/2006/10/26/uncommon-dialogs-font-chooser-color-picker-dialogs/ 
//
// 2019 - Modified and adapted by Derek Tremblay (derektremblay666@gmail.com)
//////////////////////////////////////////////

using System.Windows;
using System.Windows.Media;

namespace Microsoft.Samples.CustomControls
{
    public class ColorThumb : System.Windows.Controls.Primitives.Thumb
    {
        static ColorThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorThumb), 
                new FrameworkPropertyMetadata(typeof(ColorThumb)));
        }

        public static readonly DependencyProperty ThumbColorProperty =
            DependencyProperty.Register(nameof(ThumbColor), typeof(Color), typeof(ColorThumb),
                new FrameworkPropertyMetadata(Colors.Transparent));

        public static readonly DependencyProperty PointerOutlineThicknessProperty =
            DependencyProperty.Register(nameof(PointerOutlineThickness), typeof(double), typeof(ColorThumb),
                new FrameworkPropertyMetadata(1.0));

        public static readonly DependencyProperty PointerOutlineBrushProperty =
            DependencyProperty.Register("PointerOutlineBrush", typeof(Brush), typeof(ColorThumb),
                new FrameworkPropertyMetadata(null));

        public Color ThumbColor
        {
            get => (Color)GetValue(ThumbColorProperty);
            set => SetValue(ThumbColorProperty, value);
        }

        public double PointerOutlineThickness
        {
            get => (double)GetValue(PointerOutlineThicknessProperty);
            set => SetValue(PointerOutlineThicknessProperty, value);
        }

        public Brush PointerOutlineBrush
        {
            get => (Brush)GetValue(PointerOutlineBrushProperty);
            set => SetValue(PointerOutlineBrushProperty, value);
        }
    }
}