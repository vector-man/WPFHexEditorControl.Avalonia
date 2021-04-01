//////////////////////////////////////////////
// Apache 2.0  - 2021
// Author : Derek Tremblay (derektremblay666@gmail.com)
//
//
// NOT A TRUE PROJECT! IT'S JUST A SAMPLE FOR TESTING THE HEXEDITOR IN VARIOUS SITUATIONS... 
//////////////////////////////////////////////

using System;
using System.Windows.Controls;
using System.Windows.Media;
using WpfHexaEditor.Core;

namespace WpfHexEditor.Sample.BinaryFilesDifference
{
    public partial class BlockListItem : UserControl
    {
        private CustomBackgroundBlock _customBackGroundBlock;
        public event EventHandler PatchButtonClick;
        public event EventHandler Click;

        public BlockListItem() => InitializeComponent();

        public BlockListItem(CustomBackgroundBlock cbb)
        {
            InitializeComponent();

            CustomBlock = cbb;
        }

        public CustomBackgroundBlock CustomBlock
        {
            get => _customBackGroundBlock;

            set
            {
                _customBackGroundBlock = value;
                DataContext = value;
            }
        }

        private void PatchBlockButton_Click(object sender, System.Windows.RoutedEventArgs e) =>
            PatchButtonClick?.Invoke(this, new EventArgs());

        private void UserControl_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) =>
            Background = Brushes.Orange;

        private void UserControl_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) =>
            Background = Brushes.Transparent;

        private void UserControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Click?.Invoke(this, new EventArgs());
        }
    }
}
