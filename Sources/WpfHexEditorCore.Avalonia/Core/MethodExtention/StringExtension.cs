//////////////////////////////////////////////
// 2012 - Code from :
// https://stackoverflow.com/questions/11447019/is-there-any-way-to-find-the-width-of-a-character-in-a-fixed-width-font-given-t
// 
// 2018-2020 - Modified/adapted by Derek Tremblay (derektremblay666@gmail.com)
//////////////////////////////////////////////

using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace WPFHexEditorControl.Avalonia.Core.MethodExtention
{
    public static class StringExtension
    {
        /// <summary>
        /// Get the screen size of a string
        /// TODO: Ensure that method changes conform to Avalonia standards.
        /// </summary>
        public static Size GetScreenSize(this string text, FontFamily fontFamily, double fontSize, FontStyle fontStyle,
            FontWeight fontWeight, FontStretch fontStretch, Brush foreGround, Visual visual)
        {
            fontFamily ??= new TextBlock().FontFamily;
            fontSize = fontSize > 0 ? fontSize : new TextBlock().FontSize;


            var ft = new FormattedText(text ?? string.Empty,
                                       CultureInfo.InvariantCulture,
                                       FlowDirection.LeftToRight,
                                       new Typeface(fontFamily, fontStyle, fontWeight, fontStretch),
                                       fontSize,
                                       foreGround);

            return new Size(ft.Width, ft.Height);
        }
    }
}