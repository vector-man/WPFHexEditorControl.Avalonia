//////////////////////////////////////////////
// Apache 2.0  - 2021
// Author : Derek Tremblay (derektremblay666@gmail.com)
//
//
// NOT A TRUE PROJECT! IT'S JUST A SAMPLE FOR TESTING THE HEXEDITOR IN VARIOUS SITUATIONS... 
//////////////////////////////////////////////

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WpfHexaEditor;
using WpfHexaEditor.Core;
using WpfHexaEditor.Core.Bytes;
using WpfHexaEditor.Core.EventArguments;
using WpfHexaEditor.Core.MethodExtention;

namespace WpfHexEditor.Sample.BinaryFilesDifference
{
    public partial class MainWindow : Window
    {        
        /// <summary>
        /// Used to catch internal change for cath potential infinite loop
        /// </summary>
        private bool _internalChange = false;
        List<ByteDifference> _differences = null;

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
            if (_internalChange) return;
            if (FileDiffBytesList.SelectedItem is not ByteDifferenceListItem byteDifferenceItem) return;

            _internalChange = true;
            FirstFile.SetPosition(byteDifferenceItem.ByteDiff.BytePositionInStream, 1);
            SecondFile.SetPosition(byteDifferenceItem.ByteDiff.BytePositionInStream, 1);
            _internalChange = false;
        }

        private void PatchButton_Click(object sender, RoutedEventArgs e)
        {
            if (_differences is null) return;

            SecondFile.With(c =>
            {
                c.ReadOnlyMode = false;

                foreach (BlockListItem itm in FileDiffBlockList.Items)
                {
                    var diffList = _differences.Where(d => d.BytePositionInStream >= itm.CustomBlock.StartOffset &&
                                                           d.BytePositionInStream <= itm.CustomBlock.StopOffset);

                    foreach (ByteDifference byteDiff in diffList)
                        c.AddByteModified(byteDiff.Destination, byteDiff.BytePositionInStream);

                    itm.PatchBlockButton.IsEnabled = false;
                }

                c.ReadOnlyMode = true;
            });
        }

        private void SaveChangeButton_Click(object sender, RoutedEventArgs e)
        {
            SecondFile.With(c =>
            {
                c.ReadOnlyMode = false;
                c.SubmitChanges();
                c.ReadOnlyMode = true;
            });

            ClearUI();
        }

        #endregion

        #region Various methods

        private void LoadByteDifferenceList()
        {
            //Clear UI
            FileDiffBytesList.Items.Clear();

            //Validation
            if (_differences is null) return;
            if (FileDiffBlockList.SelectedItem is not BlockListItem blockitm) return;

            foreach (ByteDifference byteDifference in _differences
                .Where(c => c.BytePositionInStream >= blockitm.CustomBlock.StartOffset &&
                            c.BytePositionInStream <= blockitm.CustomBlock.StopOffset))
            {
                byteDifference.Color = blockitm.CustomBlock.Color;
                FileDiffBytesList.Items.Add(new ByteDifferenceListItem(byteDifference));
            }
        }

        private void OpenFile(HexEditor hexEditor)
        {
            ClearUI();

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
        private void ClearUI()
        {
            FileDiffBytesList.Items.Clear();
            FileDiffBlockList.Items.Clear();
            FirstFile.ClearCustomBackgroundBlock();
            SecondFile.ClearCustomBackgroundBlock();
            SecondFile.ClearAllChange();
            PatchButton.IsEnabled = false;
            _differences = null;
        }

        /// <summary>
        /// Find the difference of the two loaded file and add to lists the results
        /// </summary>
        private void FindDifference()
        {
            ClearUI();

            if (FirstFile.FileName == string.Empty || SecondFile.FileName == string.Empty) return;

            var cbb = new CustomBackgroundBlock();
            int j = 0;

            //load the difference
            _differences = FirstFile.Compare(SecondFile).ToList();

            //Load list of difference
            foreach (ByteDifference byteDifference in _differences)
            {
                //create or update custom background block
                if (j == 0)
                    cbb = new CustomBackgroundBlock(byteDifference.BytePositionInStream, ++j, RandomBrushes.PickBrush());
                else
                    cbb.Length = ++j;
                                
                if (!_differences.Any(c => c.BytePositionInStream == byteDifference.BytePositionInStream + 1))
                {
                    j = 0;

                    new BlockListItem(cbb).With(c =>
                    {
                        c.PatchButtonClick += BlockItem_PatchButtonClick;
                        FileDiffBlockList.Items.Add(c);
                    });

                    //add to hexeditor
                    FirstFile.CustomBackgroundBlockItems.Add(cbb);
                    SecondFile.CustomBackgroundBlockItems.Add(cbb);
                }
            }

            //refresh editor
            FirstFile.RefreshView();
            SecondFile.RefreshView();

            //Enable patch button
            PatchButton.IsEnabled = true;
        }

        /// <summary>
        /// Patch the selected block in the second file
        /// </summary>
        private void BlockItem_PatchButtonClick(object sender, EventArgs e)
        {
            if (sender is not BlockListItem itm) return;
            if (_differences is null) return;

            SecondFile.With(c =>
            {
                c.ReadOnlyMode = false;

                var diffList = _differences.Where(d => d.BytePositionInStream >= itm.CustomBlock.StartOffset &&
                                                       d.BytePositionInStream <= itm.CustomBlock.StopOffset);

                foreach (ByteDifference byteDiff in diffList)
                    c.AddByteModified(byteDiff.Origine, byteDiff.BytePositionInStream);

                c.ReadOnlyMode = true;
            });

            itm.PatchBlockButton.IsEnabled = false;
        }
        #endregion

        #region Synchronise the two hexeditor
        private void FirstFile_VerticalScrollBarChanged(object sender, ByteEventArgs e)
        {
            if (_internalChange) return;

            _internalChange = true;
            SecondFile.SetPosition(e.BytePositionInStream);
            _internalChange = false;
        }

        private void SecondFile_VerticalScrollBarChanged(object sender, ByteEventArgs e)
        {
            if (_internalChange) return;

            _internalChange = true;
            FirstFile.SetPosition(e.BytePositionInStream);
            _internalChange = false;
        }
        #endregion
                
    }
}
