using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfHexEditor.Sample.MVVM.Contracts.Hex;

namespace WpfHexEditor.Sample.MVVM.Hex {
    //[ExportStreamToSeagmentsParser(Order = 9)]
    class StreamToSeagmentsParserDemo : IStreamToSeagmentsParser {
        public ParsedInfo ParseStream(Stream stream) {
            var seagments = new Seagment[] {
                new Seagment(0, 64) {
                    Description = "DemoProperty",
                    Value = "DemoValue"
                },
                new Seagment(64, 64) {
                    Description = "DemoProperty2",
                    Value = "DemoValue2"
                }
            };

            return new ParsedInfo(seagments);
        }
    }
}
