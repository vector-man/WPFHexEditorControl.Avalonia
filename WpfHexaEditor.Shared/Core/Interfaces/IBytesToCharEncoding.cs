//////////////////////////////////////////////
// Apache 2.0  - 2017
// Author       : Janus Tida
// Contributor  : Derek Tremblay
//////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

namespace WpfHexaEditor.Core.Interfaces
{
    /// <summary>
    /// The instances that implement this interface maybe use in StringDataLayer and StringByteControl,aiming at varies of charsets.
    /// Note:Some implements from <see cref="System.Text.Encoding"/> doesn't work as we expect,that is why we create this interface standing-alone.
    /// </summary>
    public interface IBytesToCharEncoding
    {
        /// <summary>
        /// Convert the <paramref name="bytesToConvert"/> to a <see cref="System.Char"/>.
        /// </summary>
        /// <param name="bytesToConvert">The byte buffer to be converted,the length of which should be equal to <see cref="BytePerChar"/></param>
        /// <returns></returns>
        char ConvertToChar(byte[] bytesToConvert);
        
        /// <summary>
        /// The count of bytes per <see cref="Char"/>
        /// </summary>
        int BytePerChar { get; }
    }
}
