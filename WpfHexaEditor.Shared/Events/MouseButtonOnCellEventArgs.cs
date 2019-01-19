using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace WpfHexaEditor.Events {

    public class MouseButtonOnCellEventArgs: CellEventArgs<MouseButtonEventArgs> {
        public MouseButtonOnCellEventArgs(int cellIndex,MouseButtonEventArgs e):base(cellIndex,e) {

        }
        
        
    }
}
