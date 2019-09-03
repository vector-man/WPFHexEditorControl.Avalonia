Imports System.IO
Imports Microsoft.Win32
Imports WpfHexaEditor.Core

Class MainWindow
    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Dim fileDialog = New OpenFileDialog()

        If (fileDialog.ShowDialog() IsNot Nothing) AndAlso File.Exists(fileDialog.FileName) Then
            Application.Current.MainWindow.Cursor = Cursors.Wait

            HexEditor.FileName = fileDialog.FileName

            Application.Current.MainWindow.Cursor = Nothing
        End If
    End Sub

    Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
        Dim fileDialog = New OpenFileDialog()

        If fileDialog.ShowDialog() IsNot Nothing Then
            If File.Exists(fileDialog.FileName) Then
                Application.Current.MainWindow.Cursor = Cursors.Wait

                HexEditor.LoadTblFile(fileDialog.FileName)
                HexEditor.TypeOfCharacterTable = CharacterTableType.TblFile

                Application.Current.MainWindow.Cursor = Nothing
            End If
        End If
    End Sub
End Class
