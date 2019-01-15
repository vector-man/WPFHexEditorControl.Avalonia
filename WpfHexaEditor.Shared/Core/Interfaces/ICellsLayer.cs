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

        /// <summary>
        /// Get the cell location of view for a cell whose index of Data is <paramref name="index"/>;
        /// </summary>
        /// <param name="index"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        bool GetCellPosition(int index,ref Point position);
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

        /// <summary>
        /// Get cell position with data index;
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="cellSize"></param>
        /// <param name="contentPosition"></param>
        public static bool GetCellContentPosition(this ICellsLayer cellsLayer,int index, ref Point contentPosition) {
            if (cellsLayer == null) {
                throw new ArgumentNullException(nameof(cellsLayer));
            }
            
            if(cellsLayer.GetCellPosition(index, ref contentPosition)) {
                contentPosition.X += cellsLayer.CellPadding.Left + cellsLayer.CellMargin.Left;
                contentPosition.Y += cellsLayer.CellPadding.Top + cellsLayer.CellMargin.Top;
                return true;
            }

            return false;
        }
    }
}
