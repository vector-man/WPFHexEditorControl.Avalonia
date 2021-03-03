//////////////////////////////////////////////
// Apache 2.0  - 2016-2021
// Author : Derek Tremblay (derektremblay666@gmail.com)
// Contributor: Janus Tida
// Contributor: ehsan69h
//////////////////////////////////////////////

using System;
using System.Windows;
using System.Windows.Input;
using WpfHexaEditor.Core;
using WpfHexaEditor.Core.EventArguments;
using WpfHexaEditor.Core.MethodExtention;

namespace WpfHexaEditor
{
    internal class HexByte : BaseByte
    {
        #region Global class variables

        private KeyDownLabel _keyDownLabel = KeyDownLabel.FirstChar;

        #endregion global class variables

        #region Constructor

        public HexByte(HexEditor parent) : base(parent)
        {
            //Update width
            UpdateDataVisualWidth();
        }

        #endregion Contructor

        #region Methods

        /// <summary>
        /// Update the render of text derived bytecontrol from byte property
        /// </summary>
        public override void UpdateTextRenderFromByte() =>
            Text = Byte is not null
                ? Byte.GetText(_parent.DataStringVisual, _parent.DataStringState, _parent.ByteOrder)
                : string.Empty;

        public override void Clear()
        {
            base.Clear();
            _keyDownLabel = KeyDownLabel.FirstChar;
        }

        public void UpdateDataVisualWidth() =>
            Width = CalculateCellWidth(_parent.ByteSize, _parent.DataStringVisual, _parent.DataStringState);

        public static int CalculateCellWidth(ByteSizeType byteSize, DataVisualType type, DataVisualState state) =>
            byteSize switch
            {
                ByteSizeType.Bit8 => type switch
                {
                    DataVisualType.Decimal =>
                        state == DataVisualState.Changes ? 30 :
                        state == DataVisualState.ChangesPercent ? 35 : 25,
                    DataVisualType.Hexadecimal =>
                        state == DataVisualState.Changes ? 25 :
                        state == DataVisualState.ChangesPercent ? 35 : 20,
                    DataVisualType.Binary =>
                        state == DataVisualState.Changes ? 70 :
                        state == DataVisualState.ChangesPercent ? 65 : 65,
                    _ => throw new NotImplementedException()
                },

                ByteSizeType.Bit16 => type switch
                {
                    DataVisualType.Decimal =>
                        state == DataVisualState.Changes
                            ? 40
                            : state == DataVisualState.ChangesPercent ? 35 : 40,
                    DataVisualType.Hexadecimal =>
                        state == DataVisualState.Changes
                            ? 40
                            : state == DataVisualState.ChangesPercent ? 35 : 40,
                    DataVisualType.Binary =>
                        state == DataVisualState.Changes
                            ? 120
                            : state == DataVisualState.ChangesPercent ? 65 : 120,
                    _ => throw new NotImplementedException()
                },

                ByteSizeType.Bit32 => type switch
                {
                    DataVisualType.Decimal =>
                        state == DataVisualState.Changes
                            ? 80
                            : state == DataVisualState.ChangesPercent ? 35 : 80,
                    DataVisualType.Hexadecimal =>
                        state == DataVisualState.Changes
                            ? 70
                            : state == DataVisualState.ChangesPercent ? 35 : 70,
                    DataVisualType.Binary =>
                        state == DataVisualState.Changes
                            ? 220
                            : state == DataVisualState.ChangesPercent ? 65 : 220,
                    _ => throw new NotImplementedException()
                },
                _ => throw new NotImplementedException(),
            };

        #endregion Methods

        #region Events delegate

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && IsFocused)
            {
                //Is focused set editing to second char.
                _keyDownLabel = KeyDownLabel.SecondChar;
                UpdateCaret();
            }

            base.OnMouseDown(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (Byte == null) return;

            if (KeyValidation(e)) return;

            //MODIFY BYTE
            if (!_parent.IsLockedFile && (!ReadOnlyMode || !_parent.ReadOnlyMode) && KeyValidator.IsHexKey(e.Key))
            {
                if (_keyDownLabel == KeyDownLabel.NextPosition)
                {
                    _parent.AppendByte(new byte[] { 0 });
                    OnMoveNext(new ByteEventArgs(BytePositionInStream));
                }
                else
                {
                    bool isEndChar;

                    (Action, isEndChar) = Byte.Update(_parent.DataStringVisual, e.Key, byteOrder: _parent.ByteOrder, ref _keyDownLabel);

                    if (isEndChar && _parent.Length != BytePositionInStream + 1)
                    {
                        _keyDownLabel = KeyDownLabel.NextPosition;
                        OnMoveNext(new ByteEventArgs(BytePositionInStream));
                    }
                }

                UpdateTextRenderFromByte();
            }

            UpdateCaret();

            base.OnKeyDown(e);
        }

        #endregion Events delegate

        #region Caret events/methods

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            _keyDownLabel = KeyDownLabel.FirstChar;
            UpdateCaret();

            base.OnGotFocus(e);
        }

        private void UpdateCaret()
        {
            if (ReadOnlyMode || Byte == null)
                _parent.HideCaret();
            else
            {
                //TODO: clear size and use BaseByte.TextFormatted property...
                //TODO: Take the scale factor from parent
                var size = Text[1].ToString()
                    .GetScreenSize(_parent.FontFamily, _parent.FontSize, _parent.FontStyle, FontWeight,
                        _parent.FontStretch, _parent.Foreground, this);

                //update site with scale factor
                //size.Width *= _parent.ZoomScale;
                //size.Height *= _parent.ZoomScale;

                _parent.SetCaretSize(size.Width + 2, ActualHeight - 2);
                _parent.SetCaretMode(_parent.VisualCaretMode);

                //TODO: DEBUG POSITION WHEN THE SCALE FACTOR IS NOT 1

                switch (_keyDownLabel)
                {
                    case KeyDownLabel.FirstChar:
                        _parent.MoveCaret(TransformToAncestor(_parent).Transform(new Point(0, 0)));
                        break;
                    case KeyDownLabel.SecondChar:
                        _parent.MoveCaret(TransformToAncestor(_parent).Transform(new Point(size.Width + 1, 0)));
                        break;
                    case KeyDownLabel.NextPosition:
                        if (_parent.Length == BytePositionInStream + 1)
                            if (_parent.AllowExtend)
                            {
                                _parent.SetCaretMode(CaretMode.Insert);
                                _parent.MoveCaret(TransformToAncestor(_parent).Transform(new Point(size.Width * 2, 0)));
                            }
                            else
                                _parent.HideCaret();

                        break;

                    //TODO: Caret position for all this folliwing case !!!

                    case KeyDownLabel.ThirdChar:
                        break;
                    case KeyDownLabel.FourthChar:
                        break;
                    case KeyDownLabel.FifthChar:
                        break;
                    case KeyDownLabel.SixthChar:
                        break;
                    case KeyDownLabel.SeventhChar:
                        break;
                    case KeyDownLabel.EighthChar:
                        break;
                    case KeyDownLabel.Ninth:
                        break;
                    case KeyDownLabel.Tenth:
                        break;
                    case KeyDownLabel.Eleventh:
                        break;
                    case KeyDownLabel.Twelfth:
                        break;
                    case KeyDownLabel.Thirteenth:
                        break;
                    case KeyDownLabel.Fourteenth:
                        break;
                    case KeyDownLabel.Fifteenth:
                        break;
                    case KeyDownLabel.Sixteenth:
                        break;
                    case KeyDownLabel.Seventeenth:
                        break;
                    case KeyDownLabel.Eighteenth:
                        break;
                    case KeyDownLabel.Ninteenth:
                        break;
                    case KeyDownLabel.Twentieth:
                        break;
                    case KeyDownLabel.TwentyFirst:
                        break;
                    case KeyDownLabel.TwentySecond:
                        break;
                    case KeyDownLabel.TwentyThird:
                        break;
                    case KeyDownLabel.TwentyFourth:
                        break;
                    case KeyDownLabel.TwentyFifth:
                        break;
                    case KeyDownLabel.TwentySixth:
                        break;
                    case KeyDownLabel.TwentySeventh:
                        break;
                    case KeyDownLabel.TwentyEighth:
                        break;
                    case KeyDownLabel.TwentyNinth:
                        break;
                    case KeyDownLabel.Thirtieth:
                        break;
                    case KeyDownLabel.ThirtyFirst:
                        break;
                    case KeyDownLabel.ThirtySecond:
                        break;
                }
            }
        }

        #endregion
    }
}
