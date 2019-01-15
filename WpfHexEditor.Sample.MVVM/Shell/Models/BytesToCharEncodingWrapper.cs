using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfHexEditor.Sample.MVVM.Contracts.Hex;

namespace WpfHexEditor.Sample.MVVM.Shell.Models
{
    class BytesToCharEncodingWrapper:WpfHexaEditor.Core.Interfaces.IBytesToCharEncoding
    {
        public BytesToCharEncodingWrapper(IBytesToCharEncoding bytesToCharEncoding) {
            this._bytesToCharEncoding = bytesToCharEncoding;
        }

        private IBytesToCharEncoding _bytesToCharEncoding;
        public int BytePerChar => _bytesToCharEncoding.BytePerChar;

        public char ConvertToChar(byte[] bytesToConvert) => _bytesToCharEncoding.Convert(bytesToConvert);
    }
}
