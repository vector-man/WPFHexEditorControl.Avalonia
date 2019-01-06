using System;
using System.Collections.Generic;
using System.Text;

namespace WpfHexaEditor.Core.Interfaces {
    /// <summary>
    /// IEditTransaction interface;
    /// </summary>
    public interface IEditTransaction {
        /// <summary>
        /// Undo;
        /// </summary>
        void Undo();

        /// <summary>
        /// Redo;
        /// </summary>
        void Redo();
        
    }
}
