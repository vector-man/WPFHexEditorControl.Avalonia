using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using WpfHexaEditor;

namespace WPFHexaEditorExample
{
    /// <summary>
    /// Logique d'interaction pour ColorPickerWindow.xaml
    /// </summary>
    public partial class ColorPickerWindow : Window
    {
        public ColorPickerWindow(HexEditor hexedit)
        {
            InitializeComponent();

            LoadColorProperties(hexedit);
        }

        private void LoadColorProperties(HexEditor hexedit)
        {
            foreach (var prop in hexedit.GetType().GetProperties().Where(c => c.PropertyType == typeof(Brush)))
            {
                ColorPropertyPanel.Children.Add(new StackPanel 
                { 
                    Orientation = Orientation.Horizontal,
                    //new Label { Content = prop.Name}
                });
            }
        }
    }
}
