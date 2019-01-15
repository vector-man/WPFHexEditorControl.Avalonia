using System;
using System.Collections.Generic;
using System.Text;
using WpfHexaEditor.Core.Bytes;
using WpfHexaEditor.Core.Interfaces;

namespace WpfHexaEditor.Core
{
    class ASCIIBytesToCharEncoding : GenericStaticInstance<ASCIIBytesToCharEncoding>,IBytesToCharEncoding {
        public int BytePerChar => 1;
        
        public char ConvertToChar(byte[] bytesToConvert) {
            if(bytesToConvert == null)
                throw new ArgumentNullException(nameof(bytesToConvert));

            if (bytesToConvert.Length != BytePerChar)
                throw new InvalidOperationException($"The length {nameof(bytesToConvert)} doesn't match {nameof(BytePerChar)}.");

            return ByteConverters.ByteToChar(bytesToConvert[0]);
        }
    }

    class UTF8BytesToCharEncoding : GenericStaticInstance<UTF8BytesToCharEncoding>, IBytesToCharEncoding {
        public int BytePerChar => 3;
        
        public char ConvertToChar(byte[] bytesToConvert) {
            if(bytesToConvert == null) {
                throw new ArgumentNullException(nameof(bytesToConvert));
            }

            if (bytesToConvert.Length != BytePerChar)
                throw new InvalidOperationException($"The length {nameof(bytesToConvert)} doesn't match {nameof(BytePerChar)}.");

            return Encoding.UTF8.GetChars(bytesToConvert)[0];
        }
    }
    
    public static class BytesToCharEncodings {
        public static IBytesToCharEncoding ASCII => ASCIIBytesToCharEncoding.StaticInstance;
        public static IBytesToCharEncoding UTF8 => UTF8BytesToCharEncoding.StaticInstance;
    }

}
