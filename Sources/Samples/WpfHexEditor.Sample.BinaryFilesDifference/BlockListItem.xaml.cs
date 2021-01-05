using System.Windows.Controls;
using WpfHexaEditor.Core;

namespace WpfHexEditor.Sample.BinaryFilesDifference
{
    /// <summary>
    /// Logique d'interaction pour BlockListItem.xaml
    /// </summary>
    public partial class BlockListItem : UserControl
    {
        private CustomBackgroundBlock _customBackGroundBlock;

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
    }
}
