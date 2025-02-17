﻿//////////////////////////////////////////////
// Apache 2.0  - 2016-2018
// Author : Derek Tremblay (derektremblay666@gmail.com)
//////////////////////////////////////////////

using System;
using System.Globalization;
using Avalonia.Data.Converters;
using WPFHexEditorControl.Avalonia.Core.Bytes;

namespace WPFHexEditorControl.Avalonia.Core.Converters
{
    /// <summary>
    /// Used to convert hexadecimal to Long value.
    /// </summary>
    public sealed class HexToLongStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) return string.Empty;

            var (success, val) = ByteConverters.IsHexValue(value.ToString());

            return success
                ? (object)val
                : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    }
}