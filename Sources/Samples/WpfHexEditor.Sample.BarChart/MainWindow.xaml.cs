using Microsoft.Win32;
using System.Windows;
using WpfHexaEditor.Core.MethodExtention;

namespace WpfHexEditor.Sample.BarChart
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow() => InitializeComponent();

        private void OpenButton_Click(object sender, RoutedEventArgs e) =>
            new OpenFileDialog().With(o =>
            {
                o.CheckFileExists = true;
                o.CheckPathExists = true;
                o.Multiselect = false;

                if (o.ShowDialog() ?? false)
                    HexEditor.FileName = o.FileName;
            });
    }
}
