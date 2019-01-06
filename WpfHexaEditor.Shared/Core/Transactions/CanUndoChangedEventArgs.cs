using System;
using System.Collections.Generic;
using System.Text;

namespace WpfHexaEditor.Core.Transactions {
    public class CanUndoChangedEventArgs:EventArgs {
        public CanUndoChangedEventArgs(bool newValue) {
            NewValue = newValue;
        }

        public bool NewValue { get; }
    }
}
