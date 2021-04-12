using DotNetty.Transport.Channels;
using Swm.Device.Rgv;
using System;

namespace Swm.SRgv.GS
{
    class SRgvHandler : SimpleChannelInboundHandler<SRgvState>
    {
        readonly SRgvCommunicator _rgvCommunicator;
        public SRgvHandler(SRgvCommunicator rgvCommunicator)
        {
            _rgvCommunicator = rgvCommunicator;
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            Console.WriteLine("ChannelActive");
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            Console.WriteLine("ChannelInactive");
        }

        public override void ChannelRegistered(IChannelHandlerContext context)
        {
            Console.WriteLine("ChannelRegistered");
        }

        public override void ChannelUnregistered(IChannelHandlerContext context)
        {
            Console.WriteLine("ChannelUnregistered");
        }

        public override void HandlerAdded(IChannelHandlerContext context)
        {
            Console.WriteLine("HandlerAdded");
        }

        public override void HandlerRemoved(IChannelHandlerContext context)
        {
            Console.WriteLine("HandlerRemoved");
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, SRgvState msg)
        {
            if (msg == null)
            {
                return;
            }

            _rgvCommunicator.OnMessageReceived(msg);
        }
    }

}
