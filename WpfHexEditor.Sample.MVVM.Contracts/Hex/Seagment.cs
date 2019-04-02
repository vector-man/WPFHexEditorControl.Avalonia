using System;
using System.Collections.Generic;
using System.Text;

namespace WpfHexEditor.Sample.MVVM.Contracts.Hex {

    public class Seagment {
        public Seagment(long startIndex,long length) {
            this.StartIndex = startIndex;
            this.Length = length;
        }

        public string Description { get; set; }
        public string Value { get; set; }
        public long StartIndex { get; }
        public long Length { get;}
    }
}
