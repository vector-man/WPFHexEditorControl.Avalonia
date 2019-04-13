using System;
using System.Collections.Generic;
using System.Text;

namespace WpfHexEditor.Sample.MVVM.Contracts.Hex
{
    /// <summary>
    /// The encoding algrithm that convert byte[] to char;
    /// </summary>
    public interface IBytesToCharEncoding {
        /// <summary>
        /// The length of each char in byte;
        /// </summary>
        int BytePerChar { get; }

        char Convert(byte[] bytesToConvert);

        /// <summary>
        /// The unique ID;
        /// </summary>
        string GUID { get; }

        /// <summary>
        /// Encoding name
        /// </summary>
        string EncodingName { get; }

        /// <summary>
        /// Sort;
        /// </summary>
        int Sort { get; }
    }
}
