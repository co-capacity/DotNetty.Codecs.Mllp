using System.Collections.Generic;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;

namespace DotNetty.Codecs.Mllp
{
    /// <summary>
    /// 
    /// </summary>
    public class FrameEncoder : MessageToMessageEncoder<IByteBuffer>
    {
        private readonly byte[] _append;
        private readonly byte[] _prepend;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prepend">Frame prepender</param>
        /// <param name="append">Frame appender</param>
        public FrameEncoder(byte[] prepend, byte[] append)
        {
            _prepend = prepend;
            _append = append;
        }

        protected override void Encode(IChannelHandlerContext context, IByteBuffer message, List<object> output)
        {
            output.Add(_prepend);
            output.Add(message.Retain());
            output.Add(_append);
        }
    }
}