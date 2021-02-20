<img src="Images/Logo.png?raw=true" width="420" height="100" /> 
  
[![NuGet](https://img.shields.io/badge/Nuget-v2.1.5-red.svg)](https://www.nuget.org/packages/WPFHexaEditor/)
[![NetFramework](https://img.shields.io/badge/.Net%20Framework-4.7/4.8-green.svg)](https://www.microsoft.com/net/download/windows)
[![NetFramework](https://img.shields.io/badge/.Net%20-5.0-green.svg)](https://www.microsoft.com/net/download/windows)
[![NetFramework](https://img.shields.io/badge/Language-C%23%207.0+-orange.svg)](https://blogs.msdn.microsoft.com/dotnet/2016/08/24/whats-new-in-csharp-7-0/)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://github.com/abbaye/WpfHexEditorControl/blob/master/LICENSE)

Wpf Hexeditor is a powerful and fully customisable user control for editing file or stream as hexadecimal, decimal and binary. 

You can use it very easily in Wpf or WinForm application. Download the code and test the Wpf (C#, VB.NET) and WinForm (C#) samples.

The control are localized in English, French, Russian, Polish, Portuguese and Chinese

### ⭐ You want to say thank or just like project  ?

Hexeditor control is totaly free and can be used in all project you want like open source and commercial applications. I make it in my free time and a few colaborators help me when they can... Please hit the ⭐️ button or fork and I will be very happy ;) I accept help contribution...  

### 🖼 Screenshots

Sample with standard ASCII character table
![example](Images/Sample11-NOTBL.png?raw=true)

Sample with custom character table (TBL) on SNES Final Fantasy II US
![example](Images/Sample9-TBL.png?raw=true)

Sample use ByteShiftLeft and BytePerLine properties with custom TBL for edit fixed lenght table...
![example](Images/Sample12-FIXEDTBL-BYTESHIFT.png?raw=true)

Sample use of find and find/replace dialog...
![example](Images/Sample15-FindReplaceDialog.png?raw=true)


⭐ Sample use of BarChart representation of the data ...
![example](Images/Sample12-BarChart.png?raw=true)

⭐ Sample use of control in AvalonDock ...

![example](Images/Sample11-AvalonDock.png?raw=true)

⭐ Sample use of CustomBackgroundBlock in the "Find difference bytes sample" ...
![example](Images/Sample15-CustomBackgroundBlock.png?raw=true)

## 🧾 What is TBL (custom character table)
The TBL are small plaintext .tbl files that link every hexadecimal value with a character, which proves most useful when reading and changing text data. Wpf HexEditor support .tbl and you can define your custom character table as you want.

Unicode TBL are supported. For use put value at the right of equal (=) like this (0401=塞西尔) or (42=Д) in you plaintext .tbl file.

![example](Images/TBLExplain.png?raw=true)

### 🛒 Somes features

⭐ = New features

- ⭐ AvalonDock support
- ⭐ Edit in hexadecimal, decimal and binary 
- ⭐ Edit in 8bit, 16bit and 32bit
- ⭐ Edit in LoHi or HiLo
- ⭐ View as BarChart (see in screenshot. will evoluate in future)
- Find and Find/Replace dialog
- Append byte at end of file
- Include HexBox, an Hexadecimal TextBox with spinner
- Fill selection (or another array) with byte.
- Support of common key in window like CTRL+C, CTRL+V, CTRL+Z, CTRL+Y, CTRL+A, ESC...
- Copy to clipboard as code like C#, VB.Net, C, Java, F# ... 
- Support custom .TBL character table file insted of default ASCII.
- Unlimited Undo / Redo
- Finds methods (FindFirst, FindNext, FindAll, FindLast, FindSelection) and overload for (string, byte[])
- Replace methods (ReplaceFirst, ReplaceNext, ReplaceAll) and overload for (string, byte[])
- Highlight byte with somes find methods
- Bookmark
- Group byte in block of 2,4,6,8 bytes...
- Show data as hexadecimal or decimal
- Possibility to view only a part of file/stream in editor and dont loose anychange when used it (AllowVisualByteAdress...)
- Zoom / UnZoom hexeditor content (50% to 200%)
- Positility to show or not the bytes are deleted.
- Customize the color of bytes, TBL, background, header, and much more ...
- ...

### 👏 How to use
Add a reference to `WPFHexaEditor.dll` from your project, then add the following namespace to your XAML:

```xaml
xmlns:control="clr-namespace:WpfHexaEditor;assembly=WPFHexaEditor"
```

Insert the control like this in your XAML...:

```xaml
<control:HexEditor/>
<control:HexEditor Width="NaN" Height="NaN"/>
<control:HexEditor Width="Auto" Height="Auto"/>
<control:HexEditor FileName="{Binding FileNamePath}" Width="Auto" Height="Auto"/>
```

---
✨ Wpf HexEditor user control, by Derek Tremblay (derektremblay666@gmail.com) coded for your fun! 😊🤟
