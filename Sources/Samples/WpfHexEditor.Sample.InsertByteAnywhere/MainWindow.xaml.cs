//////////////////////////////////////////////
// Apache 2.0  - 2021
// Author : Derek Tremblay (derektremblay666@gmail.com)
//
// INSERT BYTE ANYWHERE SAMPLE / DEVELOPMENT TEST
//////////////////////////////////////////////

using Microsoft.Win32;
using System.Windows;
using WpfHexaEditor.Core.MethodExtention;

namespace WpfHexEditor.Sample.InsertByteAnywhere
{
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

        private void AddByteButton_Click(object sender, RoutedEventArgs e)
        {
            HexEditor.InsertByte(224, 15);
        }
    }
}
