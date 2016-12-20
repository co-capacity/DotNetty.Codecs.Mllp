using System.Text;
using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels.Embedded;
using Xunit;

namespace DotNetty.Codecs.Mllp.Tests
{
    public class MllpDecoderTest
    {
        public MllpDecoderTest()
        {
            _iso = Encoding.GetEncoding("ISO-8859-1");
        }

        private readonly byte[] _prepend = {11};
        private readonly byte[] _append = {28, 13};
        private readonly Encoding _iso;

        [Fact]
        public void TestFailMissingPrepend()
        {
            var ch = new EmbeddedChannel(new FrameDecoder(1024, _prepend, _append));
            for (var i = 0; i < 2; i++)
                Assert.Throws<CorruptedFrameException>(
                    () => ch.WriteInbound(Unpooled.Buffer(3).WriteBytes(_iso.GetBytes("A")).WriteBytes(_append)));
        }

        [Fact]
        public void TestFailToLargeMessage()
        {
            var ch = new EmbeddedChannel(new FrameDecoder(2, _prepend, _append));
            for (var i = 0; i < 2; i++)
            {
                Assert.Throws<TooLongFrameException>(
                    () =>
                        ch.WriteInbound(
                            Unpooled.Buffer(5)
                                .WriteBytes(_prepend)
                                .WriteBytes(_iso.GetBytes("AAAA"))
                                .WriteBytes(_append)));

                ch.WriteInbound(Unpooled.Buffer(4)
                    .WriteBytes(_prepend)
                    .WriteBytes(_iso.GetBytes("A"))
                    .WriteBytes(_append));
                var buf = ch.ReadInbound<IByteBuffer>();
                ReferenceCountUtil.ReleaseLater(buf);
                Assert.Equal(1, buf.ReadableBytes);
                Assert.Equal("A", buf.ToString(_iso));
                buf.Release();
            }
        }

        [Fact]
        public void TestFullMessage()
        {
            var ch = new EmbeddedChannel(new FrameDecoder(1024, _prepend, _append));
            for (var i = 0; i < 2; i++)
            {
                ch.WriteInbound(Unpooled.Buffer(4)
                    .WriteBytes(_prepend)
                    .WriteBytes(_iso.GetBytes("A"))
                    .WriteBytes(_append));
                var buf = ch.ReadInbound<IByteBuffer>();
                ReferenceCountUtil.ReleaseLater(buf);
                Assert.Equal(1, buf.ReadableBytes);
                Assert.Equal("A", buf.ToString(_iso));
                buf.Release();
            }
        }

        [Fact]
        public void TestPartialMessage()
        {
            var ch = new EmbeddedChannel(new FrameDecoder(1024, _prepend, _append));
            for (var i = 0; i < 2; i++)
            {
                ch.WriteInbound(Unpooled.Buffer(2)
                    .WriteBytes(_prepend)
                    .WriteBytes(_iso.GetBytes("A")));
                var buf = ch.ReadInbound<IByteBuffer>();
                Assert.Null(buf);

                ch.WriteInbound(Unpooled.Buffer(1).WriteByte(_append[0]));
                buf = ch.ReadInbound<IByteBuffer>();
                Assert.Null(buf);

                ch.WriteInbound(Unpooled.Buffer(1).WriteByte(_append[1]));
                buf = ch.ReadInbound<IByteBuffer>();
                ReferenceCountUtil.ReleaseLater(buf);
                Assert.Equal("A", buf.ToString(_iso));
                buf.Release();
            }
        }
    }
}