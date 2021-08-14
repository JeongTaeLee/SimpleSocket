using System;
using System.Text;
using System.Linq;
using NUnit.Framework;
using SimpleSocket.Common;

namespace SimpleSocket.Test
{
    [TestFixture]
    public class PrefixBodySizeMessageFilterTest
    {
        private class TestFilter : PrefixBodySizeMessageFilter
        {
            public TestFilter()
                : base(4)
            {
            }

            protected override int ParsingBodySize(byte[] buffer, int offset, int length)
            {
                return BitConverter.ToInt32(buffer, offset);
            }

            protected override object ParsingBody(byte[] buffer, int offset, int length)
            {
                return Encoding.UTF8.GetString(buffer, offset, length);
            }
        }

        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void FirstTestCase()
        {
            var filter = new TestFilter();

            var testMessage = CreateBuffer("Hello world");

            Assert.IsNull(filter.Filtering(testMessage, 0, 2, out var firstReadSize));
            Assert.IsFalse(filter.parsedPrefix);
            Assert.AreEqual(firstReadSize, 0);

            Assert.IsNull(filter.Filtering(testMessage, firstReadSize, 6, out var secondReadSize));
            Assert.IsTrue(filter.parsedPrefix);
            Assert.AreEqual(secondReadSize, 4);

            Assert.IsNotNull(filter.Filtering(testMessage, secondReadSize, testMessage.Length - secondReadSize,
                out var thirdReadSize));
            Assert.AreEqual(testMessage.Length, firstReadSize + secondReadSize + thirdReadSize);
        }

        [Test]
        public void SecondTestCase()
        {
            var filter = new TestFilter();

            var firstTestMessage = CreateBuffer("First Hello world");
            var secondTestMessage = CreateBuffer("Second Hello world");
            var thirdTestMessage = CreateBuffer("Third Hello world");

            var totalTestMessage = MergeBuffer(firstTestMessage, secondTestMessage, thirdTestMessage);

            Assert.IsNotNull(filter.Filtering(totalTestMessage, 0, firstTestMessage.Length, out var firstReadSize));

            Assert.IsFalse(filter.parsedPrefix);
            Assert.AreEqual(filter.bodySize, 0);

            Assert.IsNotNull(filter.Filtering(totalTestMessage, firstTestMessage.Length, secondTestMessage.Length,
                out var secondReadSize));

            Assert.IsFalse(filter.parsedPrefix);
            Assert.AreEqual(filter.bodySize, 0);

            Assert.IsNotNull(filter.Filtering(totalTestMessage, firstTestMessage.Length + secondTestMessage.Length,
                thirdTestMessage.Length, out var thirdReadSize));

            Assert.IsFalse(filter.parsedPrefix);
            Assert.AreEqual(filter.bodySize, 0);
        }

        private byte[] CreateBuffer(string data)
        {
            var testBody = Encoding.UTF8.GetBytes(data);
            var testHeader = BitConverter.GetBytes(testBody.Length);

            var testBuffer = new byte[4 + testBody.Length];
            Buffer.BlockCopy(testHeader, 0, testBuffer, 0, testHeader.Length);
            Buffer.BlockCopy(testBody, 0, testBuffer, testHeader.Length, testBody.Length);

            return testBuffer;
        }

        private byte[] MergeBuffer(params byte[][] buffers)
        {
            var returnBuffer = new byte[buffers.Sum(buffer => buffer.Length)];
            var offset = 0;
            foreach (var buffer in buffers)
            {
                Buffer.BlockCopy(buffer, 0, returnBuffer, offset, buffer.Length);
                offset += buffer.Length;
            }

            return returnBuffer;
        }
    }
}