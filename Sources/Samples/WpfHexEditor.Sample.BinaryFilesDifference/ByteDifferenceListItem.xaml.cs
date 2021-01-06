using System.Windows.Controls;

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
    }
}
