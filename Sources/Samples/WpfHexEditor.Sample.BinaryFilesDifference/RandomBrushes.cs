//////////////////////////////////////////////
// Apache 2.0 - 2021
// Author : Derek Tremblay (derektremblay666@gmail.com)
//
// BINARY FILE DIFFERENCE SAMPLE USING WPF HEXEDITOR
//////////////////////////////////////////////

using System;
using System.Reflection;
using System.Windows.Media;

namespace WpfHexEditor.Sample.BinaryFilesDifference
{
    /// <summary>
    /// Pick a random bruch
    /// </summary>
    public static class RandomBrushes
    {
        public static SolidColorBrush PickBrush()
        {
            PropertyInfo[] properties = typeof(Brushes).GetProperties();

            return (SolidColorBrush)properties
                [
                    new Random().Next(properties.Length)
                ].GetValue(null, null);
        }
    }
}
