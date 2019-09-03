using System;
using System.Collections.Generic;
using System.Text;

namespace WpfHexaEditor.Core.Bytes {
    class ByteCaret {
        public ByteCaret(byte originByte,byte modifiedByte,long bytePosition) {
            if(bytePosition < 0) {
                throw new ArgumentOutOfRangeException(nameof(bytePosition));
            }

            OriginByte = originByte;
            ModifiedByte = modifiedByte;
            BytePosition = bytePosition;
        }

        /// <summary>
        /// The byte from origin stream or last <see cref="ByteCaret"/>;
        /// </summary>
        public byte OriginByte { get; }

        /// <summary>
        /// The byte that has been modified;
        /// </summary>
        public byte ModifiedByte { get; }

        /// <summary>
        /// The position of the byte;
        /// </summary>
        public long BytePosition { get; }

        /// <summary>
        /// View Position;
        /// </summary>
        public long Position { get; set; }

        /// <summary>
        /// The panel that was focused;
        /// </summary>
        public LayerPanel ActivedPanel { get; set; }

        /// <summary>
        /// The Focused Hex Char (Only available while <see cref="ActivedPanel"/> is <see cref="LayerPanel.Hex"/>;
        /// </summary>
        public HexChar FocusedChar { get; set; }
    }
}
