using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using WpfHexaEditor.Core;

namespace WpfHexaEditor.Events {
    public class HexMouseButtonOnCellEventArgs:MouseButtonOnCellEventArgs {
        public HexMouseButtonOnCellEventArgs(HexChar hexChar,int cellIndex,MouseButtonEventArgs mouseButtonEventArgs):base(cellIndex,mouseButtonEventArgs) {
            this.HexChar = hexChar;
        }

        public HexChar HexChar { get; }
    }
}
