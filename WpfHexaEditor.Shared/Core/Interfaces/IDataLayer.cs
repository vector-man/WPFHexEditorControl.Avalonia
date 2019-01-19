using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfHexaEditor.Core.Interfaces
{
   
    public interface IDataLayer
    {
        byte[] Data { get; set; }
        IEnumerable<IBrushBlock> BackgroundBlocks { get; set; }
        IEnumerable<IBrushBlock> ForegroundBlocks { get; set; }

        //int ColumnGroupSize { get; set; }
        //double GroupMargin { get; set; }

        Brush Foreground { get; }

        int BytePerLine { get; set; }
        int AvailableRowsCount { get; }

        int PositionStartToShow { get; }
    }
}
