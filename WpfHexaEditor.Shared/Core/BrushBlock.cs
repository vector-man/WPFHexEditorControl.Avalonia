using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using WpfHexaEditor.Core.Interfaces;

namespace WpfHexaEditor.Core
{
    
    public class BrushBlock:IBrushBlock
    {
        public BrushBlock() {

        }
        public long StartOffset { get; set; }
        public long Length { get; set; }
        public Brush Brush { get; set; }
    }
}
