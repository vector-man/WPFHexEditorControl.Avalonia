//////////////////////////////////////////////
// Apache 2.0  - 2021
// Author : Derek Tremblay (derektremblay666@gmail.com)
//
//
// NOT A TRUE PROJECT! IT'S JUST A SAMPLE FOR TESTING THE HEXEDITOR IN VARIOUS SITUATIONS... 
//////////////////////////////////////////////

using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Input;
using WpfHexaEditor;
using WpfHexaEditor.Core;

namespace WpfHexEditor.Sample.BinaryFilesDifference
{
    public partial class MainWindow : Window
    {        
        /// <summary>
        /// Used to catch internal change for cath potential infinite loop
        /// </summary>
        private bool _internalChange = false;

        public MainWindow() => InitializeComponent();

        #region Various controls events
        private void FirstHexEditor_Click(object sender, RoutedEventArgs e) => OpenFile(FirstFile);

        private void SecondHexEditor_Click(object sender, RoutedEventArgs e) => OpenFile(SecondFile);

        private void FindDifferenceButton_Click(object sender, RoutedEventArgs e)
        {
            if (FirstFile.FileName == string.Empty || SecondFile.FileName == string.Empty)
            {
                MessageBox.Show("LOAD TWO FILE!!", "HexEditor sample", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            Application.Current.MainWindow.Cursor = Cursors.Wait;
            FindDifference();
            Application.Current.MainWindow.Cursor = null;
        }

        private void FileDiffBlockList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_internalChange) return;
            if (FileDiffBlockList.SelectedItem is not BlockListItem blockitm) return;
            
            _internalChange = true;
            FirstFile.SetPosition(blockitm.CustomBlock.StartOffset, 1);
            SecondFile.SetPosition(blockitm.CustomBlock.StartOffset, 1);
            _internalChange = false;

            LoadByteDifferenceList();
        }

        private void FileDiffBytesList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void LoadByteDifferenceList()
        {
            //TODO NEED OPTIMISATION

            FileDiffBytesList.Items.Clear();

            if (FileDiffBlockList.SelectedItem is not BlockListItem blockitm) return;

            var cbb = blockitm.CustomBlock;

            for (long position = cbb.StartOffset; position <= cbb.StopOffset; position++)
            {
                var origine = FirstFile.GetByte(position, false);
                var destination = SecondFile.GetByte(position, false);

                var bytediff = new ByteDifference(origine.singleByte.Value, destination.singleByte.Value, position, cbb.Color);

                FileDiffBytesList.Items.Add(new ByteDifferenceListItem(bytediff));
            }
        }
        #endregion

        #region Various methods
        private void OpenFile(HexEditor hexEditor)
        {
            ClearDifference();

            #region Create file dialog
            var fileDialog = new OpenFileDialog
            {
                Multiselect = true,
                CheckFileExists = true
            };

            if (fileDialog.ShowDialog() == null || !File.Exists(fileDialog.FileName)) return;
            #endregion

            hexEditor.FileName = fileDialog.FileName;
        }

        /// <summary>
        /// Clear the difference in various control
        /// </summary>
        private void ClearDifference()
        {
            FileDiffBytesList.Items.Clear();
            FileDiffBlockList.Items.Clear();
            FirstFile.CustomBackgroundBlockItems.Clear();
            SecondFile.CustomBackgroundBlockItems.Clear();
        }

        private void FindDifference()
        {
            ClearDifference();

            if (FirstFile.FileName == string.Empty || SecondFile.FileName == string.Empty) return;

            //variables
            var maxLenght = FirstFile.Length > SecondFile.Length 
                ? FirstFile.Length 
                : SecondFile.Length;

            var cbb = new CustomBackgroundBlock();
            int j = 0;
            var ok = false;
            
            for (int i = 0; i < maxLenght; i++)
            {
                var equal = FirstFile.GetByte(i, false).singleByte == SecondFile.GetByte(i, false).singleByte;
                
                if (!equal)
                {
                    ok = true;

                    //Build CustomBackgroundBlock
                    if (j == 0)
                        cbb = new CustomBackgroundBlock(i, ++j, RandomBrushes.PickBrush());                        
                    else
                        cbb.Length = ++j;
                }
                else
                {
                    if (!ok) continue;
                    
                    //add to list
                    FileDiffBlockList.Items.Add(new BlockListItem(cbb));

                    //add to hexeditor
                    FirstFile.CustomBackgroundBlockItems.Add(cbb);
                    SecondFile.CustomBackgroundBlockItems.Add(cbb);

                    //reset variable
                    j = 0;
                    ok = false;

                    continue;
                }
            }

            //refresh editor
            FirstFile.RefreshView();
            SecondFile.RefreshView();
        }
        #endregion

        #region Synchronise the two hexeditor
        private void FirstFile_VerticalScrollBarChanged(object sender, WpfHexaEditor.Core.EventArguments.ByteEventArgs e)
        {
            if (_internalChange) return;

            _internalChange = true;
            SecondFile.SetPosition(e.BytePositionInStream);
            _internalChange = false;
        }

        private void SecondFile_VerticalScrollBarChanged(object sender, WpfHexaEditor.Core.EventArguments.ByteEventArgs e)
        {
            if (_internalChange) return;

            _internalChange = true;
            FirstFile.SetPosition(e.BytePositionInStream);
            _internalChange = false;
        }
        #endregion

    }
}
