using System;
using System.Collections.Generic;
using System.Text;

namespace WpfHexaEditor.Core.Transactions {
    public class CanRedoChangedEventArgs:EventArgs {
        public CanRedoChangedEventArgs(bool newValue) {
            NewValue = newValue;
        }

        public bool NewValue { get; }
    }
}
