//////////////////////////////////////////////
// Apache 2.0  - 2016-2020
// Author : Derek Tremblay (derektremblay666@gmail.com)
// Contributor: Janus Tida
//////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WpfHexaEditor.Core;
using WpfHexaEditor.Core.Bytes;
using WpfHexaEditor.Core.CharacterTable;

namespace WpfHexaEditor
{
    internal class StringByte : BaseByte
    {
        #region Global class variables

        private bool _tblShowMte = true;
        private bool _barchart = false;

        #endregion Global variable

        #region Contructor

        public StringByte(HexEditor parent, bool barChart) : base(parent) => _barchart = barChart;

        #endregion Contructor

        #region Properties

        /// <summary>
        /// Next Byte of this instance (used for TBL/MTE decoding)
        /// </summary>
        public byte? ByteNext { get; set; }

        #endregion Properties

        #region Characters tables

        /// <summary>
        /// Show or not Multi Title Enconding (MTE) are loaded in TBL file
        /// </summary>
        public bool TblShowMte
        {
            get => _tblShowMte;
            set
            {
                _tblShowMte = value;
                UpdateTextRenderFromByte();
            }
        }

        /// <summary>
        /// Type of caracter table are used un hexacontrol.
        /// For now, somes character table can be readonly but will change in future
        /// </summary>
        public CharacterTableType TypeOfCharacterTable { get; set; }

        /// <summary>
        /// Custom character table
        /// </summary>
        public TblStream TblCharacterTable { get; set; }

        #endregion Characters tables

        #region Methods

        /// <summary>
        /// Update the render of text derived bytecontrol from byte property
        /// </summary>
        public override void UpdateTextRenderFromByte()
        {
            if (Byte != null )
            {
                switch (TypeOfCharacterTable)
                {
                    case CharacterTableType.Ascii:
                        Text = ByteConverters.ByteToChar(Byte.Byte[0]).ToString();
                        break;
                    case CharacterTableType.TblFile:
                        if (TblCharacterTable != null)
                        {
                            ReadOnlyMode = !TblCharacterTable.AllowEdit;

                            var content = "#";

                            if (TblShowMte && ByteNext.HasValue)
                                content = TblCharacterTable.FindMatch(ByteConverters.ByteToHex(Byte.Byte[0]) +
                                                                      ByteConverters.ByteToHex(ByteNext.Value), true);

                            if (content == "#")
                                content = TblCharacterTable.FindMatch(ByteConverters.ByteToHex(Byte.Byte[0]), true);

                            Text = content;
                        }
                        else
                            goto case CharacterTableType.Ascii;
                        break;
                }
            }
            else
                Text = string.Empty;
        }

        /// <summary>
        /// Update Background,foreground and font property
        /// </summary>
        public override void UpdateVisual()
        {
            if (IsSelected)
            {
                FontWeight = _parent.FontWeight;
                Foreground = _parent.ForegroundContrast;

                Background = FirstSelected ? _parent.SelectionFirstColor : _parent.SelectionSecondColor;
            }
            else if (IsHighLight)
            {
                FontWeight = _parent.FontWeight;
                Foreground = _parent.Foreground;
                Background = _parent.HighLightColor;
            }
            else if (Action != ByteAction.Nothing)
            {
                switch (Action)
                {
                    case ByteAction.Modified:
                        FontWeight = FontWeights.Bold;
                        Background = _parent.ByteModifiedColor;
                        Foreground = _parent.Foreground;
                        break;

                    case ByteAction.Deleted:
                        FontWeight = FontWeights.Bold;
                        Background = _parent.ByteDeletedColor;
                        Foreground = _parent.Foreground;
                        break;
                }
            }
            else
            {
                #region TBL COLORING
                var cbb = _parent.GetCustomBackgroundBlock(BytePositionInStream);

                Description = cbb != null ? cbb.Description : "";

                Background = cbb != null ? cbb.Color : Brushes.Transparent;
                FontWeight = _parent.FontWeight;
                Foreground = _parent.Foreground;

                if (TypeOfCharacterTable == CharacterTableType.TblFile)
                    Foreground = (Dte.TypeDte(Text)) switch
                    {
                        DteType.DualTitleEncoding => _parent.TbldteColor,
                        DteType.MultipleTitleEncoding => _parent.TblmteColor,
                        DteType.EndLine => _parent.TblEndLineColor,
                        DteType.EndBlock => _parent.TblEndBlockColor,
                        _ => _parent.TblDefaultColor,
                    };

                #endregion
            }

            UpdateAutoHighLiteSelectionByteVisual();

            InvalidateVisual();
        }

        /// <summary>
        /// Render the control
        /// </summary>
        protected override void OnRender(DrawingContext dc)
        {
            if (_barchart)
            {
                Width = 12;
                Margin = new Thickness(2);

                #region Draw control
                //Draw background
                if (Background != null)
                    dc.DrawRectangle(Background, null, new Rect(0, 0, RenderSize.Width, RenderSize.Height));

                //Prevent drawing wrong graph
                if (Byte is null) return;

                //Draw chart
                int fillHeight = PercentValue * (int)_parent.LineHeight / 100;

                dc.DrawRectangle(_parent.BarChartColor, 
                    null, 
                    new Rect(0, _parent.LineHeight - fillHeight, Width, fillHeight));
                #endregion
            }
            else
            {
                Margin = new Thickness(0);

                base.OnRender(dc);

                #region Update width of control 
                //It's 8-10 time more fastest to update width on render for TBL string

                Width = TypeOfCharacterTable switch
                {
                    CharacterTableType.Ascii => 12,
                    CharacterTableType.TblFile => TextFormatted?.Width > 12 ? TextFormatted.Width : 12
                };
                #endregion
            }
        }

        /// <summary>
        /// Clear control
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            ByteNext = null;
        }

        #endregion Methods

        #region Events delegate

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (_barchart) return;
            if (Byte == null) return;
            if (KeyValidation(e)) return;
            if (e.Key == Key.CapsLock) return;

            //MODIFY ASCII...
            if (!ReadOnlyMode)
            {
                var isok = false;

                if (Keyboard.GetKeyStates(Key.CapsLock) == KeyStates.Toggled)
                {
                    if (Keyboard.Modifiers != ModifierKeys.Shift && Keyboard.Modifiers != ModifierKeys.Control &&
                        e.Key != Key.RightShift && e.Key != Key.LeftShift)
                    {
                        Text = KeyValidator.GetCharFromKey(e.Key).ToString();
                        isok = true;
                    }
                    else if (Keyboard.Modifiers == ModifierKeys.Shift && Keyboard.Modifiers != ModifierKeys.Control &&
                             e.Key != Key.RightShift && e.Key != Key.LeftShift)
                    {
                        isok = true;
                        Text = KeyValidator.GetCharFromKey(e.Key).ToString().ToLower();
                    }
                }
                else
                {
                    if (Keyboard.Modifiers != ModifierKeys.Shift && Keyboard.Modifiers != ModifierKeys.Control &&
                        e.Key != Key.RightShift && e.Key != Key.LeftShift)
                    {
                        Text = KeyValidator.GetCharFromKey(e.Key).ToString().ToLower();
                        isok = true;
                    }
                    else if (Keyboard.Modifiers == ModifierKeys.Shift && Keyboard.Modifiers != ModifierKeys.Control &&
                             e.Key != Key.RightShift && e.Key != Key.LeftShift)
                    {
                        isok = true;
                        Text = KeyValidator.GetCharFromKey(e.Key).ToString();
                    }
                }

                //Move focus event
                if (isok)
                {
                    Action = ByteAction.Modified;

                    Byte.ChangeByteValue(ByteConverters.CharToByte(Text[0]), BytePositionInStream);

                    //Insert byte at end of file
                    if (_parent.Length == BytePositionInStream + 1)
                    {
                        byte[] byteToAppend = { 0 };
                        _parent.AppendByte(byteToAppend);
                    }

                    OnMoveNext(new EventArgs());
                }
            }

            base.OnKeyDown(e);
        }

        #endregion Events delegate

        #region Caret events

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            //TODO: complete caret implemention ....

            _parent.SetCaretMode(CaretMode.Overwrite);

            if (ReadOnlyMode || Byte == null)
                _parent.HideCaret();
            else
                _parent.MoveCaret(TransformToAncestor(_parent).Transform(new Point(0, 0)));

            base.OnGotFocus(e);
        }
        #endregion


        #region BarChart Support

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(PercentValue), typeof(int), typeof(StringByte),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Defines the Value
        /// </summary>
        public int PercentValue
        {
            get => (int)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        #endregion
    }
}
