using System;
using System.Collections.Generic;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;

namespace DotNetty.Codecs.Mllp
{
    /// <summary>
    ///     A decoder that splits the received <see cref="IByteBuffer" /> by frame from appended bytes and prepended bytes.
    /// </summary>
    public class FrameDecoder : DelimiterBasedFrameDecoder
    {
        private readonly byte[] _prepend;

        /// <summary>
        /// </summary>
        /// <param name="maximumFrameLength">
        ///     The maximum length of the decoded frame
        ///     NOTE: A see <see cref="TooLongFrameException" /> is thrown if the length of the frame exceeds this
        ///     value.
        /// </param>
        /// <param name="prepend">Frame prepend bytes</param>
        /// <param name="append">Frame append bytes</param>
        public FrameDecoder(int maximumFrameLength, byte[] prepend, byte[] append)
            : base(maximumFrameLength, Unpooled.CopiedBuffer(append))
        {
            if (prepend == null) throw new ArgumentNullException(nameof(prepend));
            _prepend = prepend;
        }

        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            var buf = (IByteBuffer) Decode(context, input);
            if (buf != null)
            {
                var pos = IndexOf(buf, _prepend);
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

        private static int IndexOf(IByteBuffer haystack, IReadOnlyList<byte> needle)
        {
            for (var i = haystack.ReaderIndex; i < haystack.WriterIndex; i++)
            {
                var haystackIndex = i;
                int needleIndex;
                for (needleIndex = 0; needleIndex < needle.Count; needleIndex++)
                {
                    if (haystack.GetByte(haystackIndex) != needle[needleIndex])
                        break;
                    haystackIndex++;
                    if (haystackIndex == haystack.WriterIndex && needleIndex != needle.Count - 1)
                        return -1;
                }

                if (needleIndex == needle.Count)
                    return i - haystack.ReaderIndex;
            }
            return -1;
        }
    }
}