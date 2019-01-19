using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using WpfHexaEditor.Events;

namespace WpfHexaEditor.Core.Interfaces {
    public interface ICellsLayer {
        Thickness CellMargin { get; set; }
        Thickness CellPadding { get; set; }
        Size GetCellSize();

        event EventHandler<MouseButtonOnCellEventArgs> MouseDownOnCell;
        event EventHandler<MouseButtonOnCellEventArgs> MouseUpOnCell;
        event EventHandler<MouseOnCellEventArgs> MouseMoveOnCell;

        /// <summary>
        /// Get the cell location of view for a cell whose index of Data is <paramref name="cellIndex"/>;
        /// </summary>
        /// <param name="cellIndex"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        bool GetCellPosition(long cellIndex,ref Point position);
    }

    /// <summary>
    /// CellsLayerExtensions;
    /// </summary>
    public static class CellsLayerExtensions {
        public static Point? GetCellPosition(this ICellsLayer cellsLayer,int index) {
            if(cellsLayer == null) {
                throw new ArgumentNullException(nameof(cellsLayer));
            }

            var position = new Point();

            if(cellsLayer.GetCellPosition(index,ref position)) {
                return position;
            }

            return null;
        }

    }
}
