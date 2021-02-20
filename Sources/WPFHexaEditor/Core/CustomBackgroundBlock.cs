//////////////////////////////////////////////
// Apache 2.0  - 2018-2021
// Author : Derek Tremblay (derektremblay666@gmail.com)
//////////////////////////////////////////////

using System;
using System.Windows.Media;
using WpfHexaEditor.Core.Bytes;

namespace WpfHexaEditor.Core
{
    /// <summary>
    /// Used to create block of custom colors background 
    /// </summary>
    public class CustomBackgroundBlock : ICloneable
    {

        #region Constructors
        public CustomBackgroundBlock() { }

        public CustomBackgroundBlock(long start, long length, SolidColorBrush color, string description)
        {
            StartOffset = start;
            Length = length;
            Color = color;
            Description = description;
        }

        public CustomBackgroundBlock(long start, long length, SolidColorBrush color)
        {
            StartOffset = start;
            Length = length;
            Color = color;
        }

        public CustomBackgroundBlock(long start, long length, bool setRandomBrush = true)
        {
            StartOffset = start;
            Length = length;
            if (setRandomBrush) Color = RandomBrushes.PickBrush();
        }

        public CustomBackgroundBlock(string start, long length, SolidColorBrush color)
        {
            var (success, position) = ByteConverters.HexLiteralToLong(start);

            StartOffset = success ? position : throw new Exception(Properties.Resources.ConvertStringToLongErrorString);
            Length = length;
            Color = color;
        }

        public CustomBackgroundBlock(string start, long length, SolidColorBrush color, string description)
        {
            var (success, position) = ByteConverters.HexLiteralToLong(start);

            StartOffset = success ? position : throw new Exception(Properties.Resources.ConvertStringToLongErrorString);
            Length = length;
            Color = color;
            Description = description;
        }
        #endregion

        #region Property
        /// <summary>
        /// Get or set the start offset
        /// </summary>
        public long StartOffset { get; set; }

        /// <summary>
        /// Get the stop offset
        /// </summary>
        public long StopOffset => StartOffset + Length;

        /// <summary>
        /// Get or set the length of background block
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// Description of background block
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Get or set the color used in the visual
        /// </summary>
        public SolidColorBrush Color { get; set; } = Brushes.Transparent;

        ///// <summary>
        ///// Get or set the color used in the visual
        ///// </summary>
        //public SolidColorBrush ForeGroundColor { get; set; } = Brushes.Transparent;
        #endregion

        #region Methods
        /// <summary>
        /// Get clone of this CustomBackgroundBlock
        /// </summary>
        public object Clone() => MemberwiseClone();

        /// <summary>
        /// Set a random brush to this instance
        /// </summary>
        public void SetRandomColor() => Color = RandomBrushes.PickBrush();
        #endregion
    }
}
