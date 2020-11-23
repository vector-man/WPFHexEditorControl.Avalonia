using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace WpfHexaEditor.Core.ChartPanel
{
    public class BarChartLine : StackPanel, IChartLine
    {
        public List<IChartBarItem> Items { get; set; }
    }
}
