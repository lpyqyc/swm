using DotNetty.Buffers;

namespace Swm.SRgv.GS
{
    public static class Delimiters
    {
        /// <summary>
        ///     Returns {@code CR ('\r')} and {@code LF ('\n')} delimiters, which could
        ///     be used for text-based line protocols.
        /// </summary>
        public static IByteBuffer[] TexlexDelimiter()
        {
            return new[]
            {
                Unpooled.WrappedBuffer(new[] { (byte)2, (byte)3 }),
                Unpooled.WrappedBuffer(new[] { (byte)3 }),
                Unpooled.WrappedBuffer(new[] { (byte)2 }),
            };
        }
    }

}

