using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WpfHexaEditor.Core.Bytes;

namespace HexEditUnitTest
{
    [TestClass]
    public class ByteProviderTest
    {
        [TestMethod]
        public void GetByteCountTest()
        {
            var bp = new ByteProvider(@"C:\TEST\TEST.xls");

            var length = bp.Length;

            var watch = new Stopwatch();

            watch.Start();
            var byteCount = bp.GetByteCount().Sum();
            watch.Stop();

            bp.Dispose();

            Console.WriteLine($@"duration {watch.Elapsed}");
            Console.WriteLine($@"File length {length}");

            Assert.AreEqual(length, byteCount, "The number of byte need to be equal!");
        }
    }
}
