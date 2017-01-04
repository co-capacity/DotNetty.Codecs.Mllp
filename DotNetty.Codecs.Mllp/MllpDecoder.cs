using System.Collections.Generic;
using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;

namespace DotNetty.Codecs.Mllp
{
    /// <summary>
    ///     A decoder that splits the received <see cref="IByteBuffer" /> by frame from appended bytes and prepended bytes.
    /// </summary>
    public class MllpDecoder : DelimiterBasedFrameDecoder
    {
        private readonly ByteProcessor _processor;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maximumFrameLength">
        ///     The maximum length of the decoded frame
        ///     NOTE: A see <see cref="TooLongFrameException" /> is thrown if the length of the frame exceeds this
        ///     value.
        /// </param>
        public MllpDecoder(int maximumFrameLength)
    : base(maximumFrameLength, Unpooled.CopiedBuffer(new byte[]{ 28, 13}))
        {
            _processor = new ByteProcessor.IndexOfProcessor(11);
        }
        /// <summary>
        /// </summary>
        /// <param name="maximumFrameLength">
        ///     The maximum length of the decoded frame
        ///     NOTE: A see <see cref="TooLongFrameException" /> is thrown if the length of the frame exceeds this
        ///     value.
        /// </param>
        /// <param name="prepend">Frame prepend bytes</param>
        /// <param name="append">Frame append bytes</param>
        public MllpDecoder(int maximumFrameLength, byte prepend, byte[] append)
            : base(maximumFrameLength, Unpooled.CopiedBuffer(append))
        {
            _processor = new ByteProcessor.IndexOfProcessor(prepend);
        }

        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            var buf = (IByteBuffer) Decode(context, input);
            if (buf != null)
            {
                var pos = buf.ForEachByte(_processor);
                if (pos >= 0)
                {
                    buf.SkipBytes(pos + 1);
                    output.Add(buf.Slice().Retain());
                }
                else
                {
                    throw new CorruptedFrameException("input didn't start with prepend bytes");
                }
                buf.Release();
            }
        }
    }
}