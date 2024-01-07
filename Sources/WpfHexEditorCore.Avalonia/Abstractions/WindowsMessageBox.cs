namespace WPFHexEditorControl.Avalonia.Abstractions;
using System.Windows;

public class WindowsMessageBox : IMessageBox
{
    public void Show(string message, string title)
    {
        MessageBox.Show(message, title);
    }
}