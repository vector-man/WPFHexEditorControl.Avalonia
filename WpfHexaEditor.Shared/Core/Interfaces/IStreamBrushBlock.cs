using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace WpfHexaEditor.Core.Interfaces
{
    /// <summary>
    /// This interface indicateds the brush(not only solidcolor);
    /// </summary>
    public interface IBrushBlock
    {
        /// <summary>
        /// This indicates where the brushblock start;
        /// </summary>
        long StartOffset { get; set; }
        /// <summary>
        /// This indicates how "long" the block lasts;
        /// </summary>
        long Length { get; set; }
        /// <summary>
        /// This tells what the brush looks like;
        /// </summary>
        Brush Brush { get; set; }
    }
}
