using System;
using System.Collections.Generic;
using System.Text;

namespace WpfHexEditor.Sample.MVVM.Contracts.Hex {
    public class ParsedInfo {
        public ParsedInfo(IReadOnlyList<Seagment> seagments) {
            Seagments = seagments ?? throw new ArgumentNullException(nameof(seagments));
        }

        public IReadOnlyList<Seagment> Seagments { get; }

        public string ParsedType { get; set; }
    }
}
