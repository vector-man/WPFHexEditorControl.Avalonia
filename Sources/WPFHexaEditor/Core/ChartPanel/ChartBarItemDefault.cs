using System.Windows;
using System.Windows.Media;

namespace WpfHexaEditor.Core.ChartPanel
{
    public class ChartBarItemDefault: FrameworkElement
    {
        public ChartBarItemDefault(double height)
        {
            Background = new SolidColorBrush(Color.FromArgb(200, 0, 20, 0));
            Value = 50;
            Margin = new Thickness(2);
            Width = 10;
            Height = height;
        }


        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(Brush), typeof(ChartBarItemDefault),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        
        /// <summary>
        /// Defines the background
        /// </summary>
        protected Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Value", typeof(int), typeof(ChartBarItemDefault),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));
        
        /// <summary>
        /// Defines the Value
        /// </summary>
        public int Value
        {
            get => (int)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        /// <summary>
        /// Render the control
        /// </summary>
        protected override void OnRender(DrawingContext dc)
        {
            //Draw background
            if (Background != null)
            {
                int fillHeight = Value * (int)Height / 100;
                dc.DrawRectangle(Background, null, new Rect(0, Height - fillHeight, Width, fillHeight));

            }
        }

        public void Clear()
        {
            Value = 0;
        }
    }
}
