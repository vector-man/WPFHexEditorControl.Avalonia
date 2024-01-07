//////////////////////////////////////////////
// Apache 2.0  - 2019
// Author : Derek Tremblay (derektremblay666@gmail.com)
//////////////////////////////////////////////

using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Controls;
namespace WPFHexEditorControl.Avalonia.Core.Converters
{
    /// <summary>
    /// This VisibilityToBoolean converter convert Visibility <-> Boolean\
    /// TODO: Verify this converter conforms to Avalonia standards.
    /// </summary>
    public sealed class VisibilityToBooleanConverter :  IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            ((bool)value);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            (bool)value == true
                ? true
                : false;
    }
}