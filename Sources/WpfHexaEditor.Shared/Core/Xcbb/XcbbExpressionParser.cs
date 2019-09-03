using System;
using System.Collections.Generic;
using System.Text;
using WpfHexaEditor.Core.Bytes;

namespace WpfHexaEditor.Core.Xcbb
{
    /// <summary>
    /// Used to parser expression used in Xccb file for validate byte data. 
    /// </summary>
    /// <remarks>
    /// Expression parser need to be linked to a ByteProvided for validatig data.
    /// </remarks>
    public class XcbbExpressionParser
    {
        public ByteProvider Provider { get; set; }

        XcbbExpressionParser(ByteProvider provider)
        {
            Provider = provider;
        }

        XcbbExpressionParser() { }

        /// <summary>
        /// Use for valid expresion "valid if data are equal to..."
        /// </summary>
        /// <param name="expression">expression like: 0x00-0x01=$'4D 5A'</param>
        /// <returns>
        /// True = expression is valid
        /// False = expression not valid
        /// Null = unable to valid expression</returns>
        public bool? ValidIf(string expression)
        {
            return null;
        }

    }
}
