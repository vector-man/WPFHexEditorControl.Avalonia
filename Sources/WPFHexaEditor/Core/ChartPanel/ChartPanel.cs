using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace WpfHexaEditor.Core.ChartPanel
{
    public class ChartPanel: StackPanel
    {
        public ChartPanel(int lineCount, ByteOrderType byteOrder, ByteSizeType byteSize, DataVisualType byteVisual)
        {
            ChartLine = new List<IChartLine>();
            for (int i = 0; i < lineCount; i++)
            {
                ChartLine.Add(new BarChartLine());
            }
        }
        public List<IChartLine> ChartLine { get; set; }
    }
}
