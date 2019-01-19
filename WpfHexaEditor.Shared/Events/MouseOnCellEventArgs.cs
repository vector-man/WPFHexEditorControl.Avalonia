using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace WpfHexaEditor.Events {
    public class MouseOnCellEventArgs:CellEventArgs<MouseEventArgs> {
        public MouseOnCellEventArgs(int cellIndex,MouseEventArgs e):base(cellIndex,e) {

        }
    }
}
