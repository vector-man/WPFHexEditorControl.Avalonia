using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using WpfHexaEditor.Core.Bytes;

namespace HexEditUnitTest.Core.Bytes {
    [TestClass]
    public class ByteConvertersTest {
        [TestMethod]
        public void LongToHexTest() {
            var watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < 1000000; i++) {
                //Assert.AreEqual(ByteConverters.LongToHex2(16), "10");
                Assert.AreEqual(ByteConverters.LongToHex(17, 3), "011");
                Assert.AreEqual(ByteConverters.LongToHex(1048576), "100000");
                Assert.AreEqual(ByteConverters.LongToHex(1024, 4), "0400");
            }
            watch.Stop();


            Trace.WriteLine(watch.ElapsedMilliseconds);
        }

        [TestMethod]
        public void LongToStringTest() {
            var watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < 1000000; i++) {
                //Assert.AreEqual(ByteConverters.LongToHex2(16), "10");
                Assert.AreEqual(ByteConverters.LongToString(17, 3), "017");
                Assert.AreEqual(ByteConverters.LongToString(1048576), "1048576");
                Assert.AreEqual(ByteConverters.LongToString(1024, 4), "1024");
            }
            watch.Stop();


            Trace.WriteLine(watch.ElapsedMilliseconds);
        }        
    }
}
