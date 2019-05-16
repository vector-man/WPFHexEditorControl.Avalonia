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

        #region Buttons events
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
        #endregion

        #region Methods
        /// <summary>
        /// Initialize stream and hexeditor
        /// </summary>
        private void InitializeMStreamFind(byte[] findData = null)
        {
            FindHexEdit.CloseProvider();

            if (findData != null && findData.Length > 0)
                _findMs = new MemoryStream(findData);
            else
            {
                _findMs = new MemoryStream(1);
                _findMs.WriteByte(0);
            }                

            FindHexEdit.Stream = _findMs;
        }

        /// <summary>
        /// Initialize stream and hexeditor
        /// </summary>
        private void InitializeMStreamReplace()
        {
            ReplaceHexEdit.CloseProvider();
            _replaceMs = new MemoryStream(1);
            _replaceMs.WriteByte(0);
            ReplaceHexEdit.Stream = _replaceMs;
        }
        #endregion

        #region Settings events
        private void SettingButton_Click(object sender, RoutedEventArgs e) => 
            SettingPopup.IsOpen = true;

        private void SettingMenuItem_Click(object sender, RoutedEventArgs e) => 
            SettingPopup.IsOpen = false;
        #endregion

    }
}
