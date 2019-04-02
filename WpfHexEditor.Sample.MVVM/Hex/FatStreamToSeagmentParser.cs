using DiscUtils.Fat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfHexEditor.Sample.MVVM.Contracts.App;
using WpfHexEditor.Sample.MVVM.Contracts.Hex;

namespace WpfHexEditor.Sample.MVVM.Hex {
    [ExportStreamToSeagmentsParser(Order = 2)]
    class FatStreamToSeagmentParser : IStreamToSeagmentsParser {
        public ParsedInfo ParseStream(Stream stream) {
            if (!FatFileSystem.Detect(stream)) {
                return null;
            }

            try {
                
                using (var fileSystem = new FatFileSystem(stream)) {
                    var seagments = GetSeagmentsByFatFileSystem(fileSystem);
                    var parsedInfo = new ParsedInfo(seagments);
                    parsedInfo.ParsedType = fileSystem.FatVariant.ToString();
                    
                    return parsedInfo;
                }
            }
            catch(Exception ex) {
                LoggerService.WriteException(ex);
                return null;
            }
        }


        private static readonly (string propName, long startIndex, long length)[] fatProperties = new (string propName, long startIndex, long length)[] {
            (nameof(FatFileSystem.OemName),3,8),
            (nameof(FatFileSystem.SectorsPerCluster),13,1),
            (nameof(FatFileSystem.ReservedSectorCount),14,2),
            (nameof(FatFileSystem.FatCount),16,1),
            (nameof(FatFileSystem.MaxRootDirectoryEntries),17,2),
            (nameof(FatFileSystem.TotalSectors),19,2),
            (nameof(FatFileSystem.Media),21,1),
            (nameof(FatFileSystem.FatSize),22,2),
            (nameof(FatFileSystem.SectorsPerTrack),24,2),
            (nameof(FatFileSystem.Heads),26,2),
            (nameof(FatFileSystem.HiddenSectors),28,4),
            (nameof(FatFileSystem.TotalSectors),32,4),

            //(nameof(FatFileSystem.OemName),36,8),
            //(nameof(FatFileSystem.OemName),3,8),
            //(nameof(FatFileSystem.OemName),3,8),
            //(nameof(FatFileSystem.OemName),3,8),
            //(nameof(FatFileSystem.OemName),3,8),
            //(nameof(FatFileSystem.OemName),3,8),
            //(nameof(FatFileSystem.OemName),3,8),
            //(nameof(FatFileSystem.OemName),3,8),
            //(nameof(FatFileSystem.OemName),3,8),
            //(nameof(FatFileSystem.OemName),3,8),
            //(nameof(FatFileSystem.OemName),3,8),
            //(nameof(FatFileSystem.OemName),3,8),
            //(nameof(FatFileSystem.OemName),3,8),
            //(nameof(FatFileSystem.OemName),3,8),
            //(nameof(FatFileSystem.OemName),3,8),
            //(nameof(FatFileSystem.OemName),3,8),
            //(nameof(FatFileSystem.OemName),3,8),
            //(nameof(FatFileSystem.OemName),3,8),
            //(nameof(FatFileSystem.OemName),3,8),
            //(nameof(FatFileSystem.OemName),3,8),
            //(nameof(FatFileSystem.OemName),3,8),
            //(nameof(FatFileSystem.OemName),3,8),
            //(nameof(FatFileSystem.OemName),3,8),
            //(nameof(FatFileSystem.OemName),3,8),
            //(nameof(FatFileSystem.OemName),3,8),
            //(nameof(FatFileSystem.OemName),3,8),
            //(nameof(FatFileSystem.OemName),3,8),
            //(nameof(FatFileSystem.OemName),3,8),
        };
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static IReadOnlyList<Seagment> GetSeagmentsByFatFileSystem(FatFileSystem fs) {
            var seagments = new List<Seagment>();
            var fsType = fs.GetType();
            foreach (var item in fatProperties) {
                var propInfo = fsType.GetProperty(item.propName);
                if(propInfo == null) {
                    continue;
                }

                try {
                    var seagment = new Seagment(item.startIndex, item.length);
                    seagment.Description = item.propName;
                    seagment.Value = propInfo.GetValue(fs).ToString();
                    seagments.Add(seagment);
                }
                catch(Exception ex) {
                    LoggerService.WriteException(ex);
                }
            }

            return seagments;
        }
    }
}
