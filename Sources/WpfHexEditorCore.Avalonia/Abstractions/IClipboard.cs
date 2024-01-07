namespace WPFHexEditorControl.Avalonia.Abstractions;

public interface IClipboard
{
    public void SetText(string text);
    public string GetText();
    public void SetData(object data);
    public object GetData();
}