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
            // 완전한 헤더 + 완전한 바디
            var filter = new TestFilter();

            var testMsg = "Hello world";
            var testMessage = CreateMessage(testMsg);

            // 한번에 파싱.
            var parsedMsg = filter.Filtering(testMessage, 0, testMessage.Length, out var firstReadSize);

            // 파싱 결과 테스트.
            Assert.AreEqual(testMsg, parsedMsg);

            // read size 테스트.
            Assert.AreEqual(testMessage.Length, firstReadSize);

            // 초기화 테스트.
            Assert.IsFalse(filter.parsedPrefix);
            Assert.AreEqual(filter.bodySize, 0);
        }

        [Test]
        public void SecondTestCase()
        {
            // 완전한 헤더, 완전한 바디
            var filter = new TestFilter();

            var testMsg = "Hello world";
            var testMessage = CreateMessage(testMsg);

            // Prefix만 파싱.
            Assert.IsNull(filter.Filtering(testMessage, 0, 4, out var firstReadSize));
            Assert.IsTrue(filter.parsedPrefix);
            Assert.AreEqual(filter.bodySize, testMessage.Length - filter.prefixSize);

            // 바디 파싱.
            var parsedMsg = filter.Filtering(testMessage, firstReadSize, testMessage.Length - firstReadSize,
                out var secondReadSize);

            // 파싱 결과 테스트.
            Assert.AreEqual(testMsg, parsedMsg);

            // read size 테스트.
            Assert.AreEqual(testMessage.Length, firstReadSize + secondReadSize);

            // 초기화 테스트.
            Assert.IsFalse(filter.parsedPrefix);
            Assert.AreEqual(filter.bodySize, 0);
        }

        [Test]
        public void ThirdTestCase()
        {
            // 분리된 헤더, 나머지 헤더 + 완전환 바디
            var filter = new TestFilter();

            var testStr = "Hello world";
            var testMessage = CreateMessage(testStr);

            // 분리된 헤더 파싱.
            Assert.IsNull(filter.Filtering(testMessage, 0, 2, out var firstReadSize));
            Assert.IsFalse(filter.parsedPrefix);
            Assert.AreEqual(firstReadSize, 0);

            // 나머지 헤더 파싱.
            Assert.IsNull(filter.Filtering(testMessage, firstReadSize, 6, out var secondReadSize));
            Assert.IsTrue(filter.parsedPrefix);
            Assert.AreEqual(secondReadSize, 4);

            // 바디 파싱
            var parsedBody = filter.Filtering(testMessage, secondReadSize, testMessage.Length - secondReadSize,
                out var thirdReadSize);

            // 파싱 결과 테스트.
            Assert.AreEqual(testStr, parsedBody);

            // read size 테스트.
            Assert.AreEqual(testMessage.Length, firstReadSize + secondReadSize + thirdReadSize);

            // 초기화 테스트.
            Assert.IsFalse(filter.parsedPrefix);
            Assert.AreEqual(filter.bodySize, 0);
        }

        [Test]
        public void FourthTestCase()
        {
            // 완전한 헤더 + 분리된 바디, 나머지 바디.
            var filter = new TestFilter();

            var testStr = "Hello world";
            var testMessage = CreateMessage(testStr);

            // 완전한 헤더 + 분리된 바디.
            Assert.IsNull(filter.Filtering(testMessage, 0, filter.prefixSize + testMessage.Length / 2,
                out var firstReadSize));
            Assert.IsTrue(filter.parsedPrefix);
            Assert.AreEqual(filter.prefixSize, firstReadSize);
            Assert.AreEqual(filter.bodySize, testMessage.Length - firstReadSize);

            // 나머지 바디.
            var parsedMessage = filter.Filtering(testMessage, firstReadSize, testMessage.Length - firstReadSize,
                out var secondReadSize);

            // 파싱 결과 테스트.
            Assert.AreEqual(testStr, parsedMessage);

            // read size 테스트.
            Assert.AreEqual(testMessage.Length, firstReadSize + secondReadSize);

            // 초기화 테스트.
            Assert.IsFalse(filter.parsedPrefix);
            Assert.AreEqual(filter.bodySize, 0);
        }

        [Test]
        public void FifthTestCase()
        {
            // 여러 메시지 뭉치 테스트.
            var filter = new TestFilter();

            var firstTestStr = "First Hello world";
            var firstTestMessage = CreateMessage(firstTestStr);
            var secondTestStr = "Second Hello world";
            var secondTestMessage = CreateMessage(secondTestStr);
            var thirdTestStr = "Third Hello world";
            var thirdTestMessage = CreateMessage(thirdTestStr);

            var totalTestMessage = MergeMessage(firstTestMessage, secondTestMessage, thirdTestMessage);

            // 첫번째 메시지 파싱 (완전한 헤더 + 완전한 바디)
            var parsedMessage =
                filter.Filtering(totalTestMessage, 0, firstTestMessage.Length, out var firstMsgReadSize);
            Assert.AreEqual(parsedMessage, firstTestStr);
            Assert.AreEqual(firstTestMessage.Length, firstMsgReadSize);

            // 초기화 테스트.
            Assert.IsFalse(filter.parsedPrefix);
            Assert.AreEqual(filter.bodySize, 0);

            // 두번째 메시지 파싱 (완전한 헤더, 완전한 바디)
            Assert.IsNull(filter.Filtering(totalTestMessage, firstMsgReadSize, filter.prefixSize,
                out var secondMsgReadSize01));
            Assert.IsTrue(filter.parsedPrefix);
            Assert.AreEqual(filter.prefixSize, secondMsgReadSize01);
            Assert.AreEqual(filter.bodySize, secondTestMessage.Length - secondMsgReadSize01);

            parsedMessage = filter.Filtering(totalTestMessage, firstMsgReadSize + secondMsgReadSize01,
                secondTestMessage.Length - secondMsgReadSize01, out var secondMsgReadSize02);
            Assert.AreEqual(parsedMessage, secondTestStr);
            Assert.AreEqual(secondTestMessage.Length, secondMsgReadSize01 + secondMsgReadSize02);

            // 초기화 테스트.
            Assert.IsFalse(filter.parsedPrefix);
            Assert.AreEqual(filter.bodySize, 0);

            // 세번째 메시지 파싱(완전한 헤더 + 분리된 바디, 나머지 바디).
            Assert.IsNull(filter.Filtering(thirdTestMessage, 0, filter.prefixSize + thirdTestMessage.Length / 2,
                out var thirdMsgReadSize01));
            Assert.IsTrue(filter.parsedPrefix);
            Assert.AreEqual(filter.prefixSize, thirdMsgReadSize01);
            Assert.AreEqual(filter.bodySize, thirdTestMessage.Length - thirdMsgReadSize01);

            parsedMessage = filter.Filtering(thirdTestMessage, thirdMsgReadSize01,
                thirdTestMessage.Length - thirdMsgReadSize01,
                out var thirdMsgReadSize02);
            Assert.AreEqual(thirdTestStr, parsedMessage);
            Assert.AreEqual(thirdTestMessage.Length, thirdMsgReadSize01 + thirdMsgReadSize02);

            // read size 테스트.
            Assert.AreEqual(totalTestMessage.Length,
                firstMsgReadSize + secondMsgReadSize01 + secondMsgReadSize02 + thirdMsgReadSize01 + thirdMsgReadSize02);
        }

        private byte[] CreateMessage(string data)
        {
            var testBody = Encoding.UTF8.GetBytes(data);
            var testHeader = BitConverter.GetBytes(testBody.Length);

            var testBuffer = new byte[4 + testBody.Length];
            Buffer.BlockCopy(testHeader, 0, testBuffer, 0, testHeader.Length);
            Buffer.BlockCopy(testBody, 0, testBuffer, testHeader.Length, testBody.Length);

            return testBuffer;
        }

        private byte[] MergeMessage(params byte[][] buffers)
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