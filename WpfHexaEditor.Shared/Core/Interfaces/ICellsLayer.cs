using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace WpfHexaEditor.Core.Interfaces {
    public interface ICellsLayer {
        Thickness CellMargin { get; set; }
        Thickness CellPadding { get; set; }
        Size GetCellSize();

        event EventHandler<(int cellIndex, MouseButtonEventArgs e)> MouseLeftDownOnCell;
        event EventHandler<(int cellIndex, MouseButtonEventArgs e)> MouseLeftUpOnCell;
        event EventHandler<(int cellIndex, MouseEventArgs e)> MouseMoveOnCell;
        event EventHandler<(int cellIndex, MouseButtonEventArgs e)> MouseRightDownOnCell;

        Point? GetCellLocation(int index);
    }
}
