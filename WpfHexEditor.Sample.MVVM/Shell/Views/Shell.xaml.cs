using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Data;
using WpfHexaEditor;
using WpfHexEditor.Sample.MVVM.Contracts.Shell;

namespace WpfHexEditor.Sample.MVVM.Views {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [Export(typeof(IShell))]
    public partial class Shell : Window,IShell {
        public Shell() {
            InitializeComponent();
        }
        
    }
}
