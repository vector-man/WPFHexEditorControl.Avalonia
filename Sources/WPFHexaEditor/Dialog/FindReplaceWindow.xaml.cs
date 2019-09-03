using System.IO;
using System.Windows;

namespace WpfHexaEditor.Dialog
{
    /// <summary>
    /// Logique d'interaction pour FindReplaceWindow.xaml
    /// </summary>
    public partial class FindReplaceWindow
    {
        private MemoryStream _findMs;
        private MemoryStream _replaceMs;
        private readonly HexEditor _parent;

        public FindReplaceWindow(HexEditor parent, byte[] findData = null)
        {
            InitializeComponent();

            //Parent hexeditor for "binding" search
            _parent = parent;

            InitializeMStreamFind(findData);
            InitializeMStreamReplace();
        }

        #region Events
        private void ClearButton_Click(object sender, RoutedEventArgs e) => InitializeMStreamFind();
        private void ClearReplaceButton_Click(object sender, RoutedEventArgs e) => InitializeMStreamReplace();
        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void FindAllButton_Click(object sender, RoutedEventArgs e) =>
            _parent?.FindAll(FindHexEdit.GetAllBytes(), HighlightMenuItem.IsChecked);

        private void FindFirstButton_Click(object sender, RoutedEventArgs e) =>
            _parent?.FindFirst(FindHexEdit.GetAllBytes(), 0, HighlightMenuItem.IsChecked);

        private void FindNextButton_Click(object sender, RoutedEventArgs e) =>
            _parent?.FindNext(FindHexEdit.GetAllBytes(), HighlightMenuItem.IsChecked);

        private void FindLastButton_Click(object sender, RoutedEventArgs e) =>
            _parent?.FindLast(FindHexEdit.GetAllBytes(), HighlightMenuItem.IsChecked);

        private void ReplaceButton_Click(object sender, RoutedEventArgs e) =>
            _parent?.ReplaceFirst(FindHexEdit.GetAllBytes(), ReplaceHexEdit.GetAllBytes(),
                TrimMenuItem.IsChecked, HighlightMenuItem.IsChecked);

        private void ReplaceNextButton_Click(object sender, RoutedEventArgs e) =>
            _parent?.ReplaceNext(FindHexEdit.GetAllBytes(), ReplaceHexEdit.GetAllBytes(),
               TrimMenuItem.IsChecked, HighlightMenuItem.IsChecked);

        private void ReplaceAllButton_Click(object sender, RoutedEventArgs e) => 
            _parent?.ReplaceAll(FindHexEdit.GetAllBytes(), ReplaceHexEdit.GetAllBytes(), 
                TrimMenuItem.IsChecked, HighlightMenuItem.IsChecked);

        private void ReplaceHexEdit_BytesDeleted(object sender, System.EventArgs e) => 
            InitializeMStreamReplace(ReplaceHexEdit.GetAllBytes());

        private void FindHexEdit_BytesDeleted(object sender, System.EventArgs e) =>
            InitializeMStreamFind(FindHexEdit.GetAllBytes());

        private void SettingButton_Click(object sender, RoutedEventArgs e) =>
            SettingPopup.IsOpen = true;

        private void SettingMenuItem_Click(object sender, RoutedEventArgs e) =>
            SettingPopup.IsOpen = false;
        #endregion

        #region Methods
        /// <summary>
        /// Initialize stream and hexeditor
        /// </summary>
        private void InitializeMStreamFind(byte[] findData = null)
        {
            FindHexEdit.CloseProvider();

            _findMs = new MemoryStream(1);

            if (findData != null && findData.Length > 0)
                foreach (byte b in findData)
                    _findMs.WriteByte(b);            
            else            
                _findMs.WriteByte(0);
                            
            FindHexEdit.Stream = _findMs;
        }

        /// <summary>
        /// Initialize stream and hexeditor
        /// </summary>
        private void InitializeMStreamReplace(byte[] replaceData = null)
        {
            ReplaceHexEdit.CloseProvider();
                        
            _replaceMs = new MemoryStream(1);

            if (replaceData != null && replaceData.Length > 0)
                foreach (byte b in replaceData)
                    _replaceMs.WriteByte(b);
            else
                _replaceMs.WriteByte(0);

            ReplaceHexEdit.Stream = _replaceMs;
        }
        #endregion
    }
}
