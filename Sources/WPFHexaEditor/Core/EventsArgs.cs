using System;

namespace WpfHexaEditor.Core
{
    /// <summary>
    /// Custom event arguments used for pass somes informations to delegate
    /// </summary>
    public class ByteEventArgs : EventArgs
    {
        public ByteEventArgs(long position) => BytePositionInStream = position;

        public ByteEventArgs() { }

        public long BytePositionInStream { get; set; }

        public int Index { get; set; }
    }
}
