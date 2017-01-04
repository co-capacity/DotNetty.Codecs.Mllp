using System.Collections.Generic;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;

namespace DotNetty.Codecs.Mllp
{
    /// <summary>
    /// 
    /// </summary>
    public class MllpEncoder : MessageToMessageEncoder<IByteBuffer>
    {
        private readonly byte[] _append;
        private readonly byte _prepend;

        /// <summary>
        /// 
        /// </summary>
        public MllpEncoder() : this(11, new byte []{28,13})
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prepend">Frame prepender</param>
        /// <param name="append">Frame appender</param>
        public MllpEncoder(byte prepend, byte[] append)
        {
            _prepend = prepend;
            _append = append;
        }

        protected override void Encode(IChannelHandlerContext context, IByteBuffer message, List<object> output)
        {
            output.Add(Unpooled.WrappedBuffer(new []{_prepend}));
            output.Add(message.Retain());
            output.Add(Unpooled.WrappedBuffer(_append));
        }
    }
}