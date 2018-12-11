using Microsoft.Win32;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows;
using System.Windows.Input;
using WpfHexEditor.Sample.MVVM.Contracts.App;
using WpfHexEditor.Sample.MVVM.Helpers;
using WpfHexEditor.Sample.MVVM.Contracts.ToolTip;
using WpfHexEditor.Sample.MVVM.Shell;
using WpfHexEditor.Sample.MVVM.Contracts.Hex;

using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfHexaEditor.Core.Interfaces;
using WpfHexaEditor.Core;

namespace WpfHexEditor.Sample.MVVM.ViewModels {
    [Export]
    public partial class ShellViewModel : BindableBase {
        public ShellViewModel() {
            InitializeToolTips();
#if DEBUG
            //try {
            //    Stream = File.OpenRead("E://HBMS400M.img");
            //}
            //catch(Exception ex) {

            //}
            
#endif

        }


        private long _selectionStart;
        public long SelectionStart {
            get => _selectionStart;
            set => SetProperty(ref _selectionStart, value);
        }

        
        private long _focusPosition;
        public long FocusPosition {
            get => _focusPosition;
            set {
                SetProperty(ref _focusPosition, value);
                RaisePropertyChanged(nameof(SelectionLine));
            }
        }
        public long SelectionLine {
            get {
                if(BytePerLine > 0) {
                    return FocusPosition / BytePerLine;
                }

                return -1;
            }
        }



        private long _selectionLength;
        public long SelectionLength {
            get => _selectionLength;
            set => SetProperty(ref _selectionLength, value);
        }


        private long _position;
        public long Position {
            get => _position;
            set => SetProperty(ref _position, value);
        }


        public ObservableCollection<WpfHexaEditor.Core.BrushBlock> CustomBackgroundBlocks { get; set; } = new ObservableCollection<WpfHexaEditor.Core.BrushBlock>();
        

        private DelegateCommand _loadedCommand;
        public DelegateCommand LoadedCommand => _loadedCommand ??
            (_loadedCommand = new DelegateCommand(
                () => {
#if DEBUG
                    //Stream = File.OpenRead("E://backup.ab");
                    //CustomBackgroundBlocks.Add(new WpfHexaEditor.Core.BrushBlock { Brush = Brushes.AliceBlue, StartOffset = 1024, Length = 16 });
#endif
                }
            ));

        public InteractionRequest<Notification> UpdateBackgroundRequest { get; set; } = new InteractionRequest<Notification>();


        private DelegateCommand _testCommand;
        public DelegateCommand TestCommand => _testCommand ??
            (_testCommand = new DelegateCommand(
                () => {
                   if(CustomBackgroundBlocks.Count != 0) {
                        var rand = new Random();
                        var brush = new SolidColorBrush(Color.FromRgb((byte)rand.Next(byte.MaxValue), (byte)rand.Next(byte.MaxValue), (byte)rand.Next(byte.MaxValue)));
                        foreach (var block in CustomBackgroundBlocks) {
                            block.Brush = brush;
                        }
                        UpdateBackgroundRequest.Raise(new Notification());
                        return;
                    }
#if DEBUG
                   if(Stream == null) {
                        return;
                   }
                   for (int i = 0; i < 200; i++) {
                        var block = CustomBackgroundFactory.CreateNew();
                        block.StartOffset = 24 + i;
                        block.Length = 1;
                        block.Brush = Brushes.Chocolate;
                        CustomBackgroundBlocks.Add(block);
                        UpdateBackgroundRequest.Raise(new Notification());
                    }

                   
                    //CustomBackgroundBlocks = customBacks;
#endif
                }
            ));

        
        private Stream _stream;
        public Stream Stream {
            get => _stream;
            set => SetProperty(ref _stream, value);
        }


        #region File_Menu
        private DelegateCommand _openFileCommand;
        public DelegateCommand OpenFileCommand => _openFileCommand ??
            (_openFileCommand = new DelegateCommand(
                () => {
                    var fileName = DialogService.Current?.OpenFile();
                    if (string.IsNullOrEmpty(fileName)) {
                        return;
                    }

                    OpenFile(fileName);
                }
            ));

        private void OpenFile(string fileName) {
            if (Stream != null) {
                Stream.Close();
                Stream = null;
            }

            if (File.Exists(fileName)) {
                Application.Current.MainWindow.Cursor = Cursors.Wait;

                Stream = File.Open(fileName, FileMode.Open, FileAccess.ReadWrite);

                Application.Current.MainWindow.Cursor = null;
            }
        }

        private DelegateCommand _submitChangesCommand;
        public DelegateCommand SubmitChangesCommand => _submitChangesCommand ??
            (_submitChangesCommand = new DelegateCommand(
                () => {
                    //FileEditor?.SubmitChanges();
                }
            ));

        private DelegateCommand _saveAsCommand;
        public DelegateCommand SaveAsCommand => _saveAsCommand ??
            (_saveAsCommand = new DelegateCommand(
                () => {
                    var fileDialog = new SaveFileDialog();

                    if (fileDialog.ShowDialog() != null) {
                        //FileEditor.SubmitChanges(fileDialog.FileName, true);
                    }
                }
            ));

        private DelegateCommand _closeCommand;
        public DelegateCommand CloseCommand => _closeCommand ??
            (_closeCommand = new DelegateCommand(
                () => {
                    //FileEditor?.CloseProvider();
                }
            ));

        private DelegateCommand _exitCommand;
        public DelegateCommand ExitCommand => _exitCommand ??
            (_exitCommand = new DelegateCommand(
                () => {
                    ExitRequest.Raise(new Notification());
                }
            ));
        public InteractionRequest<Notification> ExitRequest { get; } = new InteractionRequest<Notification>();

        #endregion

        #region Edit_Menu

        private DelegateCommand _setReadOnlyCommand;
        public DelegateCommand SetReadOnlyCommand =>
            _setReadOnlyCommand ?? (_setReadOnlyCommand = new DelegateCommand(
                () => {

                }
            ));

        private DelegateCommand _undoCommand;
        public DelegateCommand UndoCommand => _undoCommand ?? (_undoCommand = new DelegateCommand(
            () => {
                //FileEditor?.Undo();
            }
        ));


        #endregion


        //Set the focused position char as Selection Start
        private DelegateCommand _setAsStartCommand;
        public DelegateCommand SetAsStartCommand => _setAsStartCommand ??
            (_setAsStartCommand = new DelegateCommand(
                () => {
                    if(Stream == null) {
                        return;
                    }

                    //Check if FocusPosition is valid;
                    if(FocusPosition == -1) {
                        return;
                    }
                    if(FocusPosition >= (Stream?.Length ?? 0)) {
                        return;
                    }
                    
                    if (SelectionStart != -1 && SelectionLength != 0
                    && SelectionStart + SelectionLength - 1 > FocusPosition) {
                        SelectionLength = SelectionStart + SelectionLength - FocusPosition;
                    }
                    else {
                        SelectionLength = 1;
                    }

                    SelectionStart = FocusPosition;

                }
            ));
        
        //Set the focused position char as Selection End
        private DelegateCommand _setAsEndCommand;
        public DelegateCommand SetAsEndCommand => _setAsEndCommand ??
            (_setAsEndCommand = new DelegateCommand(
                () => {
                    if (Stream == null) {
                        return;
                    }

                    //Check if FocusPosition is valid;
                    if (FocusPosition == -1) {
                        return;
                    }
                    if (FocusPosition >= (Stream?.Length ?? 0)) {
                        return;
                    }

                    
                    if (SelectionStart != -1 && SelectionLength != 0
                    && SelectionStart < FocusPosition) {
                        SelectionLength = FocusPosition - SelectionStart + 1;
                    }
                    else {
                        SelectionStart = FocusPosition;
                        SelectionLength = 1;
                    }
                }
            ));


        /// <summary>
        /// DragEventArg is not test-friendly,what we care is Data (IDataObject-typed) prop,
        /// That's why we set the command as IDataObject generic-typed;
        /// </summary>
        private DelegateCommand<IDataObject> _dropCommand;
        public DelegateCommand<IDataObject> DropCommand => _dropCommand ??
            (_dropCommand = new DelegateCommand<IDataObject>(
                dataObject => {
                    if(Stream != null) {
                        Stream.Dispose();
                        Stream = null;
                    }

                    if(dataObject == null) {
                        return;
                    }

                    try {
                        string[] files = (string[])dataObject.GetData(DataFormats.FileDrop);
                        if(files.Length == 0) {
                            return;
                        }
                        OpenFile(files[0]);
                    }
                    catch(Exception ex) {
                        LoggerService.Current.WriteCallerLine(ex.Message);
                    }
                    //arg.Data.GetData()
                }
            ));
        
    }

    /// <summary>
    /// ToolTip
    /// </summary>
    public partial class ShellViewModel{
        private void InitializeToolTips() {
            _positionToolTip = ToolTipItemFactory.CreateIToolTipDataItem();
            _valToolTip = ToolTipItemFactory.CreateIToolTipDataItem();
            
            _positionToolTip.KeyName = AppHelper.FindResourceString(Constants.ToolTipTag_Offset);
            _valToolTip.KeyName = AppHelper.FindResourceString(Constants.ToolTipTag_Value);

#if DEBUG
            
            //Test DataToolTips;
            CustomDataToolTipItems.Add((0, 8, "Test Key", "Test Value"));
            //CustomBackgroundBlocks.Add((0, 8, Brushes.Chocolate));

            //Test ObjectToolTips;
            var testObjectDataToolTip = ToolTipItemFactory.CreateToolTipObjectItem();
            testObjectDataToolTip.UIObject = new Image {
                Source = new BitmapImage(new Uri("pack://application:,,,/WpfHexEditor.Sample.MVVM;component/Resources/Icon/17101371.jpg"))
            };
            //testObjectDataToolTip.UIObject = new TextBlock {
            //    Text = "Test ToolTip"
            //};
            CustomObjectToolTipItems.Add((8, 8, testObjectDataToolTip));
            //CustomBackgroundBlocks.Add((8, 8, Brushes.Blue));
#endif
        }

        private IToolTipDataItem _valToolTip;
        private IToolTipDataItem _positionToolTip;

        private long _hoverPosition;
        public long HoverPosition {
            get => _hoverPosition;
            set {
                SetProperty(ref _hoverPosition, value);
                RaisePropertyChanged(nameof(HoverByte));
                UpdateToolTipItems();
            }
        }
        
        public byte HoverByte {
            get {
                if(Stream != null) {
                    Stream.Position = HoverPosition;
                    return (byte)Stream.ReadByte();
                }
                return 0;
            }
        }

        public ObservableCollection<IToolTipItem> DataToolTips { get; set; } = new ObservableCollection<IToolTipItem>();
        
        private IToolTipItem _selectedToolTipItem;
        public IToolTipItem SelectedToolTipItem {
            get => _selectedToolTipItem;
            set => SetProperty(ref _selectedToolTipItem, value);
        }
        
        private DelegateCommand _copyKeyCommand;
        public DelegateCommand CopyKeyCommand => _copyKeyCommand ??
            (_copyKeyCommand = new DelegateCommand(
                () => {
                    if(!(SelectedToolTipItem is IToolTipDataItem dataItem)) {
                        return;
                    }
                    Clipboard.SetText(dataItem.KeyName);
                },
                () => SelectedToolTipItem != null
            )).ObservesProperty(() => SelectedToolTipItem);


        private DelegateCommand _copyValueCommand;
        public DelegateCommand CopyValueCommand => _copyValueCommand ??
            (_copyValueCommand = new DelegateCommand(
                () => {
                    if (!(SelectedToolTipItem is IToolTipDataItem dataItem)) {
                        return;
                    }
                    Clipboard.SetText(dataItem.Value);
                },
                () => SelectedToolTipItem != null
            )).ObservesProperty(() => SelectedToolTipItem);
        
        private DelegateCommand _copyExpressionCommand;
        public DelegateCommand CopyExpressionCommand => _copyExpressionCommand ??
            (_copyExpressionCommand = new DelegateCommand(
                () => {
                    if (!(SelectedToolTipItem is IToolTipDataItem dataItem)) {
                        return;
                    }
                    Clipboard.SetText($"{dataItem.KeyName}:{dataItem.Value}");
                },
                () => SelectedToolTipItem != null
            )).ObservesProperty(() => SelectedToolTipItem);

        /// <summary>
        /// These properties make the tool tip more extensible;
        /// </summary>
        public ICollection<(long position, long size, string key, string value)> CustomDataToolTipItems = new List<(long position, long size, string key, string value)>();
        public ICollection<(long position, long size, IToolTipObjectItem toolTipObjectItem)> CustomObjectToolTipItems = new List<(long position, long size, IToolTipObjectItem toolTipObjectItem)>();

        /// <summary>
        /// This is for better performance,reducing frequency of the building IToolTipDataItem;
        /// </summary>
        private List<IToolTipDataItem> _cachedToolTipDataItems = new List<IToolTipDataItem>();
        private void UpdateToolTipItems() {
            if (!(Stream?.CanRead ?? false)) {
                return;
            }

            if (HoverPosition >= Stream.Length) {
                return;
            }

            DataToolTips.Clear();

            Stream.Position = HoverPosition;
            _positionToolTip.Value = HoverPosition.ToString();
            _valToolTip.Value = Stream.ReadByte().ToString();

            DataToolTips.Add(_positionToolTip);
            DataToolTips.Add(_valToolTip);

            if (_cachedToolTipDataItems.Count < CustomDataToolTipItems.Count) {
                var sub = CustomDataToolTipItems.Count - _cachedToolTipDataItems.Count;
                for (int i = 0; i < sub; i++) {
                    _cachedToolTipDataItems.Add(ToolTipItemFactory.CreateIToolTipDataItem());
                }
            }


            //Update  Custom ToolDataTips;
            var dataToolTipIndex = 0;
            foreach ((long position, long size, string key, string value) in CustomDataToolTipItems) {
                if(!(HoverPosition >= position && HoverPosition < size + position)) {
                    continue;
                }

                var tooltipDataItem = _cachedToolTipDataItems[dataToolTipIndex];
                tooltipDataItem.KeyName = key;
                tooltipDataItem.Value = value;
                DataToolTips.Add(tooltipDataItem);

                dataToolTipIndex++;
            }
            
            foreach ((long position, long size, IToolTipObjectItem toolTipObjectItem) in CustomObjectToolTipItems) {
                if (!(HoverPosition >= position && HoverPosition < size + position)) {
                    continue;
                }

                DataToolTips.Add(toolTipObjectItem);
            }
        }


        private int _bytePerLine = 16;
        public int BytePerLine {
            get => _bytePerLine;
            set => SetProperty(ref _bytePerLine, value);
        }
    }

    /// <summary>
    /// Encodings
    /// </summary>
    public partial class ShellViewModel {
        private WpfHexaEditor.Core.Interfaces.IBytesToCharEncoding _bytesToCharEncoding =  BytesToCharEncodings.ASCII;
        public WpfHexaEditor.Core.Interfaces.IBytesToCharEncoding BytesToCharEncoding {
            get => _bytesToCharEncoding;
            set => SetProperty(ref _bytesToCharEncoding, value);
        }



        private DelegateCommand _ASCIICommand;
        public DelegateCommand ASCIICommand => _ASCIICommand ??
            (_ASCIICommand = new DelegateCommand(
                () => {
                    BytesToCharEncoding = BytesToCharEncodings.ASCII;
                }
            ));


        private DelegateCommand _UTF8Command;
        public DelegateCommand UTF8Command => _UTF8Command ??
            (_UTF8Command = new DelegateCommand(
                () => {
                    BytesToCharEncoding = BytesToCharEncodings.UTF8;
                }
            ));
    }
}
