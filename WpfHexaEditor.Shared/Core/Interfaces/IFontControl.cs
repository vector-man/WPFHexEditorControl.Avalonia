using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace WpfHexaEditor.Core.Interfaces {

    public interface IFontControl {
        double FontSize { get; set; }
        FontFamily FontFamily { get; set; }
        FontWeight FontWeight { get; set; }

        Brush Foreground { get; set; }

        //How big a char text will be,this value will be caculated internally.
        Size CharSize { get; }
    }
}
