using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using WpfHexaEditor.Core.Bytes;

namespace WpfHexaEditor.Core.Interfaces
{
    public delegate void D_ByteListProp(List<byte> newValue, int index);

    interface IByte
    {
        public List<byte> Byte { get; set; }
        public List<byte> OriginByte { get; set; }

        public String GetText(DataVisualType type, DataVisualState state, ByteOrderType order);

        public D_ByteListProp del_ByteOnChange { get; set; }

        public bool IsEqual(byte[] bytes);

        public (ByteAction, bool) Update(DataVisualType type, Key _key, ref KeyDownLabel _keyDownLabel);

        public void ChangeByteValue(byte newValue, long position);

    }
}
