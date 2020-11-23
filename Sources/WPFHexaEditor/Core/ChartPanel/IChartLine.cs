using System;
using System.Collections.Generic;
using System.Text;

namespace WpfHexaEditor.Core.ChartPanel
{
    public interface IChartLine
    {
        public List<IChartBarItem> Items { get; set; }
    }
}
