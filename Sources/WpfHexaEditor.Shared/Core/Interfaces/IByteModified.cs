//////////////////////////////////////////////
// Apache 2.0  - 2017
// Author : Derek Tremblay (derektremblay666@gmail.com)
//////////////////////////////////////////////

using WpfHexaEditor.Core.Bytes;

namespace WpfHexaEditor.Core.Interfaces
{
    public interface IByteModified
    {
        //Properties
        ByteAction Action { get; set; }

        byte? Byte { get; set; }
        long BytePositionInFile { get; set; }
        bool IsValid { get; }
        long UndoLength { get; set; }

        //Methods
        void Clear();

        ByteModified GetCopy();
        string ToString();
    }
}