//////////////////////////////////////////////
// Apache 2.0  - 2021
// Author : Derek Tremblay (derektremblay666@gmail.com)
//
//
// NOT A TRUE PROJECT! IT'S JUST A SAMPLE FOR TESTING THE HEXEDITOR IN VARIOUS SITUATIONS... 
//////////////////////////////////////////////

using System.Windows.Controls;
using WpfHexaEditor.Core.Bytes;

namespace WpfHexEditor.Sample.BinaryFilesDifference
{
    public partial class ByteDifferenceListItem : UserControl
    {
        private ByteDifference _byteDiff;

        public ByteDifference ByteDiff
        {
            get => _byteDiff;
            set
            {
                _byteDiff = value;
                DataContext = value;
            }
        }

        public ByteDifferenceListItem() => InitializeComponent();

        public ByteDifferenceListItem(ByteDifference byteDiff)
        {
            InitializeComponent();

            ByteDiff = byteDiff;
        }

        private void PatchSecondFile_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
