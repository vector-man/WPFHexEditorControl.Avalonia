using System;

namespace WpfHexaEditor.Core
{
    /// <summary>
    /// Custom event arguments used for ByteModified
    /// </summary>
    public class ByteEventArgs : EventArgs
    {
        public int Index { get; set; }
    }
}
