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

        [TestMethod]
        public void GetDecimalBitsTest() {
            Assert.AreEqual(ByteConverters.GetDecimalBits(1), 1);
            Assert.AreEqual(ByteConverters.GetDecimalBits(10),2);
            Assert.AreEqual(ByteConverters.GetDecimalBits(100), 3);
            Assert.AreEqual(ByteConverters.GetDecimalBits(1010), 4);
            Assert.AreEqual(ByteConverters.GetDecimalBits(9999), 4);
            Assert.AreEqual(ByteConverters.GetDecimalBits(9999 + 1), 5);
        }

        [TestMethod]
        public void GetHexBitsTest() {
            
            Assert.AreEqual(ByteConverters.GetHexBits(0x1), 1);
            Assert.AreEqual(ByteConverters.GetHexBits(0xf), 1);
            Assert.AreEqual(ByteConverters.GetHexBits(0x10), 2);
        }
    }
}
