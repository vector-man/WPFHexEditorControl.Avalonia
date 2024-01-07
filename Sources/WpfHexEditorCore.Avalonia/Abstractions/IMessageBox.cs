using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFHexEditorControl.Avalonia.Abstractions
{
    public interface IMessageBox
    {
        void Show(string message, string title);
    }

}
