using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using WpfHexaEditor.Core.Interfaces;

namespace WpfHexaEditor.Core
{
    class HexBrushPosition : IHexBrushPosition {
        public int Position { get; set; }

        public Brush FirstCharBrush { get; set; }

        public Brush SecondCharBrush { get; set; }
    }
}
