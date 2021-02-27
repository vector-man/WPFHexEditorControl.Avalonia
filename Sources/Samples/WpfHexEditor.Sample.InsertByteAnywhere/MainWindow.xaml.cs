//////////////////////////////////////////////
// Apache 2.0  - 2021
// Author : Derek Tremblay (derektremblay666@gmail.com)
//
// INSERT BYTE ANYWHERE SAMPLE / DEVELOPMENT TEST
//
// THIS SAMPLE IS FOR DEVELOP THE POSSIBILITY TO INSERT BYTE ANYWHERE.
// IT'S WILL BE THE NEXT MAJOR UPDATE OF "WPF HEX EDITOR CONTROL"
//
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

        /// <summary>
        /// Test of adding somes byte
        /// 
        /// IN DEVELOPMENT, NOT WORKING PROPRELY :)
        /// </summary>
        private void AddByteButton_Click(object sender, RoutedEventArgs e) =>
            HexEditor.With(c =>
            {
                c.InsertByte(224, 15);
                c.InsertByte(245, 16);
                c.InsertByte(226, 17);

                //Actually the visual not show the result. I'm working on this :)
                c.RefreshView();
            });
    }
}
