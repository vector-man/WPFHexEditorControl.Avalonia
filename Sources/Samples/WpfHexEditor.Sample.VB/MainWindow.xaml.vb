Imports Microsoft.Win32
Imports System.IO
Imports WpfHexaEditor.Core
Imports WpfHexaEditor.Core.CharacterTable
Imports WpfHexaEditor.Dialog
Imports WpfHexEditor.Sample.VB.MySettings

Namespace WPFHexaEditorExample
    Partial Public Class MainWindow
        Private Enum SettingEnum
            HeaderVisibility
            [ReadOnly]
            ScrollVisibility
            StatusBarVisibility
        End Enum

        Public Sub New()
            InitializeComponent()
            UpdateAllSettings()
        End Sub

        Private Sub OpenMenu_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            Dim fileDialog = New OpenFileDialog()

            If fileDialog.ShowDialog() IsNot Nothing AndAlso File.Exists(fileDialog.FileName) Then
                Windows.Application.Current.MainWindow.Cursor = Cursors.Wait
                HexEdit.FileName = fileDialog.FileName
                Windows.Application.Current.MainWindow.Cursor = Nothing
            End If
        End Sub

        Private Sub SaveMenu_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            Application.Current.MainWindow.Cursor = Cursors.Wait
            HexEdit.SubmitChanges()
            Application.Current.MainWindow.Cursor = Nothing
        End Sub

        Private Sub CloseFileMenu_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            HexEdit.CloseProvider()
        End Sub

        Private Sub SetReadOnlyMenu_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            UpdateSetting(SettingEnum.[ReadOnly])
        End Sub

        Private Sub ShowHeaderMenu_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            UpdateSetting(SettingEnum.HeaderVisibility)
        End Sub

        Private Sub StatusBarVisibility_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            UpdateSetting(SettingEnum.StatusBarVisibility)
        End Sub

        Private Sub UpdateSetting(ByVal setting As SettingEnum)
            Select Case setting
                Case SettingEnum.HeaderVisibility
                    HexEdit.HeaderVisibility = If(Not [Default].HeaderVisibility, Visibility.Collapsed, Visibility.Visible)
                    [Default].HeaderVisibility = HexEdit.HeaderVisibility = Visibility.Visible
                Case SettingEnum.[ReadOnly]
                    HexEdit.ReadOnlyMode = [Default].ReadOnlyS
                    HexEdit.ClearAllChange()
                    HexEdit.RefreshView()
                Case SettingEnum.StatusBarVisibility
                    HexEdit.StatusBarVisibility = If(Not [Default].StatusBarVisibility, Visibility.Collapsed, Visibility.Visible)
                    [Default].StatusBarVisibility = HexEdit.StatusBarVisibility = Visibility.Visible
            End Select
        End Sub

        Private Sub UpdateAllSettings()
            UpdateSetting(SettingEnum.HeaderVisibility)
            UpdateSetting(SettingEnum.[ReadOnly])
            UpdateSetting(SettingEnum.ScrollVisibility)
        End Sub

        Private Sub Window_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs)
            HexEdit.CloseProvider()
            [Default].Save()
        End Sub

        Private Sub ExitMenu_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            Close()
        End Sub

        Private Sub CopyHexaMenu_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            HexEdit.CopyToClipboard(CopyPasteMode.HexaString)
        End Sub

        Private Sub CopyStringMenu_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            HexEdit.CopyToClipboard()
        End Sub

        Private Sub DeleteSelectionMenu_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            HexEdit.DeleteSelection()
        End Sub

        Private Sub GOPosition_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            Dim position = Nothing

            If Long.TryParse(PositionText.Text, position) Then
                HexEdit.SetPosition(position, 1)
            Else
                MessageBox.Show("Enter long value.")
            End If

            ViewMenu.IsSubmenuOpen = False
        End Sub

        Private Sub PositionText_TextChanged(ByVal sender As Object, ByVal e As TextChangedEventArgs)
            Dim __ = Nothing
            GoPositionButton.IsEnabled = Long.TryParse(PositionText.Text, __)
        End Sub

        Private Sub UndoMenu_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            HexEdit.Undo()
        End Sub

        Private Sub RedoMenu_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            HexEdit.Redo()
        End Sub

        Private Sub SetBookMarkButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            HexEdit.SetBookMark()
        End Sub

        Private Sub DeleteBookmark_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            HexEdit.ClearScrollMarker(ScrollMarker.Bookmark)
        End Sub

        Private Sub FindAllSelection_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            HexEdit.FindAllSelection(True)
        End Sub

        Private Sub SelectAllButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            HexEdit.SelectAll()
        End Sub

        Private Sub CTableASCIIButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            HexEdit.TypeOfCharacterTable = CharacterTableType.Ascii
            CTableAsciiButton.IsChecked = True
            CTableTblButton.IsChecked = False
            CTableTblDefaultAsciiButton.IsChecked = False
        End Sub

        Private Sub CTableTBLButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            Dim fileDialog = New OpenFileDialog()

            If fileDialog.ShowDialog() IsNot Nothing Then

                If File.Exists(fileDialog.FileName) Then
                    Application.Current.MainWindow.Cursor = Cursors.Wait
                    HexEdit.LoadTblFile(fileDialog.FileName)
                    HexEdit.TypeOfCharacterTable = CharacterTableType.TblFile
                    CTableAsciiButton.IsChecked = False
                    CTableTblButton.IsChecked = True
                    CTableTblDefaultAsciiButton.IsChecked = False
                    Application.Current.MainWindow.Cursor = Nothing
                End If
            End If
        End Sub

        Private Sub CTableTBLDefaultASCIIButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            Application.Current.MainWindow.Cursor = Cursors.Wait
            HexEdit.TypeOfCharacterTable = CharacterTableType.TblFile
            HexEdit.LoadDefaultTbl()
            Application.Current.MainWindow.Cursor = Nothing
        End Sub

        Private Sub SaveAsMenu_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            Dim fileDialog = New SaveFileDialog()
            If fileDialog.ShowDialog() IsNot Nothing Then HexEdit.SubmitChanges(fileDialog.FileName, True)
        End Sub

        Private Sub CTableTblDefaultEBCDICButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            Application.Current.MainWindow.Cursor = Cursors.Wait
            HexEdit.TypeOfCharacterTable = CharacterTableType.TblFile
            HexEdit.LoadDefaultTbl(DefaultCharacterTableType.EbcdicWithSpecialChar)
            Application.Current.MainWindow.Cursor = Nothing
        End Sub

        Private Sub CTableTblDefaultEBCDICNoSPButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            Application.Current.MainWindow.Cursor = Cursors.Wait
            HexEdit.TypeOfCharacterTable = CharacterTableType.TblFile
            HexEdit.LoadDefaultTbl(DefaultCharacterTableType.EbcdicNoSpecialChar)
            Application.Current.MainWindow.Cursor = Nothing
        End Sub

        Private Sub FindMenu_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            Dim window = New FindWindow(HexEdit) With {
                .Owner = Me
            }
            window.Show()
        End Sub

        Private Sub ReplaceMenu_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            Dim window = New FindReplaceWindow(HexEdit, HexEdit.SelectionByteArray) With {
                .Owner = Me
            }
            window.Show()
        End Sub
    End Class
End Namespace
