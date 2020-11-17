using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfHexaEditor.Core.MethodExtention;
using Xceed.Wpf.AvalonDock.Layout;

namespace WpfHexEditor.Sample.AvalonDock
{
    /// <summary>
    /// Early implementation of the sample for Avalondock
    /// 
    /// This project will be used to debug the Hexeditor when used in AvalonDock...
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow() => InitializeComponent();

        private void LoadNewHexEditor(string filename)
        {
            if (!(dockManager.Layout.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault() is LayoutDocumentPane firstDocumentPane)) return;

            new LayoutDocument().With(d =>
            {
                if (!File.Exists(filename)) return;
                
                d.Closed += Doc_Closed;
                d.IsSelectedChanged += Doc_IsSelectedChanged;
                                
                d.Title = System.IO.Path.GetFileName(filename);
                d.ToolTip = filename;
                d.IsSelected = true;

                new WpfHexaEditor.HexEditor().With(h => 
                {
                    h.FileName = filename;
                    d.Content = h;
                });
                
                firstDocumentPane.Children.Add(d);
            });              
        }

        private void Doc_IsSelectedChanged(object sender, EventArgs e)
        {
            //not implemented...
        }

        private void Doc_Closed(object sender, EventArgs e)
        {
            //not implemented...
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e) =>
            new OpenFileDialog().With(o =>
            {
                o.CheckFileExists = true;
                o.CheckPathExists = true;
                o.Multiselect = false;

                if (o.ShowDialog() ?? false)
                    LoadNewHexEditor(o.FileName);
            });
    }
}
