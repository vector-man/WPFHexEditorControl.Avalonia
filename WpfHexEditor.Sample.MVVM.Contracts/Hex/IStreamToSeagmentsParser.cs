using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;

namespace WpfHexEditor.Sample.MVVM.Contracts.Hex {
    /// <summary>
    /// This interface parse the stream to custom Seagments;
    /// </summary>
    public interface IStreamToSeagmentsParser {
        /// <summary>
        /// The parser will parse the stream to seagments if succeed,otherwise,null will be returned.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        ParsedInfo ParseStream(Stream stream);
    }

    /// <summary>
    /// The description info of <see cref="IStreamToSeagmentsParser"/>
    /// </summary>
    public interface IStreamToSeagmentsParserMetaData {
        int Order { get; }
    }

    [MetadataAttribute,AttributeUsage(AttributeTargets.Class,AllowMultiple = false)]
    public sealed class ExportStreamToSeagmentsParserAttribute : ExportAttribute,IStreamToSeagmentsParserMetaData {
        public ExportStreamToSeagmentsParserAttribute():base(typeof(IStreamToSeagmentsParser)) {

        }

        public int Order { get; set; }
        
    }
}
