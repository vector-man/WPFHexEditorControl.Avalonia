using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using WpfHexaEditor.Core;
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

                d.Title = Path.GetFileName(filename);
                d.ToolTip = filename;
                d.IsSelected = true;

                new WpfHexaEditor.HexEditor().With(h =>
                {
                    h.PreloadByteInEditorMode = PreloadByteInEditor.MaxVisibleLineExtended;
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
