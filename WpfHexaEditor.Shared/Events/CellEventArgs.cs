using System;
using System.Collections.Generic;
using System.Text;

namespace WpfHexaEditor.Events {
    public abstract class CellEventArgs<TEventArgs> : EventArgs where TEventArgs : EventArgs {
        public CellEventArgs(int cellIndex,TEventArgs nativeEventArgs) {
            this.CellIndex = cellIndex;
            NativeEventArgs = nativeEventArgs;
        }

        public int CellIndex { get; }

        public TEventArgs NativeEventArgs { get; }
    }
}
