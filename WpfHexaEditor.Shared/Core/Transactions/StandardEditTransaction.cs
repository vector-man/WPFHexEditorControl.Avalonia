using System;
using System.Collections.Generic;
using System.Text;
using WpfHexaEditor.Core.Interfaces;

namespace WpfHexaEditor.Core.Transactions {

    /// <summary>
    /// StandardEditTransaction class;
    /// </summary>
    public class StandardEditTransaction : IEditTransaction {
        public StandardEditTransaction(Action undoAct,Action redoAct) {
            _undoAct = undoAct ?? throw new ArgumentNullException(nameof(undoAct));
            _redoAct = redoAct ?? throw new ArgumentNullException(nameof(redoAct));
        }

        private readonly Action _undoAct;
        private readonly Action _redoAct;

        public void Redo() => _redoAct();

        public void Undo() => _undoAct();
    }
}
