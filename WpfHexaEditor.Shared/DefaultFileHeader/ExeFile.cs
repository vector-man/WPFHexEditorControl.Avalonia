using System.Collections.Generic;
using System.Windows.Media;

namespace WpfHexaEditor.DefaultFileHeader
{
    /// <summary>
    /// IMPLEMENTATON NOT COMPLETED
    /// Window executable file custom background block.
    /// </summary>
    /// TODO : Localize string
    public class ExeFile
    {
        public List<CustomBackgroundBlock> GetCustomBackgroundBlock() =>
            new List<CustomBackgroundBlock>
            {
                new CustomBackgroundBlock(0, 2, Brushes.BlueViolet, @"This is the 'magic number' of an EXE file. "),
                new CustomBackgroundBlock(2, 2, Brushes.Brown,
                    @"The number of bytes in the last block of the program that are actually used. 
If this value is zero, that means the entire last block is used"),
                new CustomBackgroundBlock(4, 2, Brushes.SeaGreen,
                    @"Number of blocks in the file that are part of the EXE file. 
If [02-03] is non-zero, only that much of the last block is used."),
                new CustomBackgroundBlock(6, 2, Brushes.CadetBlue,
                    @"Number of relocation entries stored after the header. May be zero"),
                new CustomBackgroundBlock(8, 2, Brushes.DarkGoldenrod, @"Number of paragraphs in the header. 
The program's data begins just after the header, and this field can be used to calculate the appropriate file offset. 
The header includes the relocation entries. 
Note that some OSs and/or programs may fail if the header is not a multiple of 512 bytes."),
                new CustomBackgroundBlock("0x0A", 2, Brushes.Coral,
                    @"Number of paragraphs of additional memory that the program will need.
This is the equivalent of the BSS size in a Unix program. 
The program can't be loaded if there isn't at least this much memory available to it."),
                new CustomBackgroundBlock("0x0C", 2, Brushes.HotPink,
                    @"Maximum number of paragraphs of additional memory. 
Normally, the OS reserves all the remaining conventional memory for your program, but you can limit it with this field."),
                new CustomBackgroundBlock("0x0E", 2, Brushes.Cyan, @"Relative value of the stack segment. 
This value is added to the segment the program was loaded at, and the result is used to initialize the SS register."),
                new CustomBackgroundBlock("0x10", 2, Brushes.IndianRed, @"Initial value of the SP register."),
                new CustomBackgroundBlock("0x12", 2, Brushes.LimeGreen, @"Word checksum. 
If set properly, the 16-bit sum of all words in the file should be zero. 
Usually, this isn't filled in."),
                new CustomBackgroundBlock("0x14", 2, Brushes.PaleTurquoise, @"Initial value of the IP register."),
                new CustomBackgroundBlock("0x16", 2, Brushes.DarkOrange,
                    @"Initial value of the CS register, relative to the segment the program was loaded at."),
                new CustomBackgroundBlock("0x18", 2, Brushes.Chartreuse,
                    @"Offset of the first relocation item in the file."),
                new CustomBackgroundBlock("0x1A", 2, Brushes.DarkSeaGreen,
                    @"Overlay number. Normally zero, meaning that it's the main program.")
            };
    }
}