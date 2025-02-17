﻿//////////////////////////////////////////////
// Apache 2.0  - 2016-2018
// Author : Derek Tremblay (derektremblay666@gmail.com)
//////////////////////////////////////////////

using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace WPFHexEditorControl.Avalonia.Core.Converters
{
    /// <summary>
    /// Used to convert long value to hexadecimal string like this 0xFFFFFFFF.
    /// </summary>
    public sealed class LongToHexStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is not null
                ? (long.TryParse(value.ToString(), out var longValue)
                    ? (longValue > -1
                        ? "0x" + longValue
                              .ToString(ConstantReadOnly.HexLineInfoStringFormat, CultureInfo.InvariantCulture)
                              .ToUpperInvariant()
                        : ConstantReadOnly.DefaultHex8String)
                    : ConstantReadOnly.DefaultHex8String)
                : ConstantReadOnly.DefaultHex8String;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    }
}