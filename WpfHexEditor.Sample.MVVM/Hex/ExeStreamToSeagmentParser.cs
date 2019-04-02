using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfHexEditor.Sample.MVVM.Contracts.Hex;
using static WpfHexEditor.Sample.MVVM.Constants;

namespace WpfHexEditor.Sample.MVVM.Hex {
    [ExportStreamToSeagmentsParser]
    class ExeStreamToSeagmentParser : IStreamToSeagmentsParser {
        public ParsedInfo ParseStream(Stream stream) {
            if (stream == null) {
                throw new ArgumentNullException(nameof(stream));
            }
            if (stream.Length < 512) {
                return null;
            }

            stream.Position = 0;
            if (stream.ReadByte() != MagicNumber[0] || stream.ReadByte() != MagicNumber[1]) {
                return null;
            }

            var seagments = new List<Seagment>();
            var buffer = new byte[2];
            foreach (var param in ExeParams) {
                var seagment = new Seagment(param.startIndex, param.length);
                stream.Position = param.startIndex;
                seagment.Description = param.languageKey;

                stream.Read(buffer, 0, 2);
                seagment.Value = $"0x{(buffer[0] + (buffer[1] << 8)).ToString("X")}";

                seagments.Add(seagment);
            }

            return new ParsedInfo(seagments) {
                ParsedType = "Exe"
            };
        }

        //"MZ"
        private static readonly byte[] MagicNumber = new byte[] { 0x4D, 0x5A };
        private static readonly (long startIndex, long length, string languageKey)[] ExeParams = new (long startIndex, long length, string languageKey)[]{
            (0,    2, CBB_EXEFile_MagicNumberString),
            (2,    2, CBB_EXEFile_BytesInLastBlockString),
            (4,    2, CBB_EXEFile_NumberOfBlockInFileBlockString),
            (6,    2, CBB_EXEFile_NumberOfRelocationEntriesString),
            (8,    2, CBB_EXEFile_NumberOfRelocationEntriesString),
            (0x0A, 2, CBB_EXEFile_NumberOfHeaderParagraphAdditionalMemoryString),
            (0x0C, 2, CBB_EXEFile_MaxNumberOfHeaderParagraphAdditionalMemoryString),
            (0x0E, 2, CBB_EXEFile_RelativeValueOfStackSegmentString),
            (0x10, 2, CBB_EXEFile_InitialValueOfSPRegisterString),
            (0x12, 2, CBB_EXEFile_WordChecksumString),
            (0x14, 2, CBB_EXEFile_InitialValueOfIPRegisterString),
            (0x16, 2, CBB_EXEFile_InitialValueOfCSRegisterString),
            (0x18, 2, CBB_EXEFile_OffsetOfTheFirstRelocationItemString),
            (0x1A, 2, CBB_EXEFile_OverlayNumberString),
        };

                
        
    }
}
