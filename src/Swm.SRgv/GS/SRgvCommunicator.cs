using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Serilog;
using Swm.Device;
using Swm.Device.Rgv;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Swm.SRgv.GS
{
    public class SRgvCommunicator : ICommunicator<SRgvDirective, SRgvState>
    {
        readonly ILogger _logger;

        readonly IPAddress _address;
        readonly int _port;
        IChannel clientChannel;
        Bootstrap _bootstrap;
        MultithreadEventLoopGroup _group;

        public SRgvCommunicator(IPAddress address, int port, MultithreadEventLoopGroup group, ILogger logger)
        {
            _address = address;
            _port = port;
            _group = group;
            _logger = logger;
        }

        public async Task ConnectAsync()
        {
            // TODO 防止重复调用

            if (_bootstrap == null)
            {
                _bootstrap = new Bootstrap()
                    .Group(_group)
                    .Channel<TcpSocketChannel>()
                    .Option(ChannelOption.TcpNodelay, true)
                    .Handler(new ActionChannelInitializer<ISocketChannel>(c =>
                    {
                        IChannelPipeline pipeline = c.Pipeline;

                        //配置编码解码器
                        //pipeline.AddLast(new CommonClientEncoder());
                        pipeline.AddLast(new DelimiterBasedFrameDecoder(4096, Delimiters.TexlexDelimiter())); // TCP拆包
                        pipeline.AddLast(new StringDecoder());          // 字节流 --> 字符串
                        pipeline.AddLast(new SRgvStateDecoder());       // 字符串 --> 通用状态

                        pipeline.AddLast(new StringEncoder());          // 字节流 <-- 字符串
                        pipeline.AddLast(new SRgvDirectiveEncoder());    // 字符串 <-- 通用指令

                        pipeline.AddLast(new SRgvHandler(this));
                    }));
            }


            if (clientChannel != null && clientChannel.Active)
            {
                _logger.Warning("已连接，忽略");
                return;
            }
            try
            {
                _logger.Debug("正在连接");
                clientChannel = await _bootstrap.ConnectAsync(new IPEndPoint(_address, _port));
                _logger.Information("连接成功");
            }
            catch (Exception ex)
            {
                throw new Exception("连接失败", ex);
            }
        }

        public async Task DisconnectAsync()
        {
            if (clientChannel == null && clientChannel.Active == false)
            {
                _logger.Warning("未连接，忽略");
                return;
            }

            await clientChannel.CloseAsync();
        }

        public bool IsConnected
        {
            get
            {
                return clientChannel?.Active == true;
            }
        }

        public string ProtocolVersion => "合肥井松智能";

        public event EventHandler<SRgvState> StateReceived;

        internal void OnMessageReceived(SRgvState e)
        {
            StateReceived?.Invoke(this, e);
        }

        public async Task SendDirectiveAsync(SRgvDirective directive)
        {
            await clientChannel.WriteAndFlushAsync(directive);
        }

    }

}
