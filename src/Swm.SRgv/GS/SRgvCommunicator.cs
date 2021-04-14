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

        readonly IPEndPoint _endPoint;
        Bootstrap? _bootstrap;
        MultithreadEventLoopGroup? _group;
        IChannel? clientChannel;

        private bool _connecting;

        public SRgvCommunicator(IPEndPoint endPoint, ILogger logger)
        {
            _endPoint = endPoint;
            _logger = logger;
        }

        /// <summary>
        /// 连接到设备
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            _logger.Debug("正在连接到设备");
            lock (this)
            {
                if (_connecting)
                {
                    _logger.Warning("不必要的调用：当前正在连接设备");
                    return;
                }

                if (clientChannel?.Active == true)
                {
                    _logger.Warning("不必要的调用：当前已连接到设备");
                    return;
                }

                _connecting = true;
            }

            if (_bootstrap == null)
            {
                _group = new MultithreadEventLoopGroup();
                _bootstrap = new Bootstrap()
                    .Group(_group)
                    .Channel<TcpSocketChannel>()
                    .Option(ChannelOption.TcpNodelay, true)
                    .Handler(new ActionChannelInitializer<ISocketChannel>(c =>
                    {
                        IChannelPipeline pipeline = c.Pipeline;

                        // 配置编码解码器
                        pipeline.AddLast(new DelimiterBasedFrameDecoder(4096, Delimiters.TexlexDelimiter())); // TCP拆包
                        pipeline.AddLast(new StringDecoder());          // 字节流 --> 字符串
                        pipeline.AddLast(new SRgvStateDecoder());       // 字符串 --> 状态

                        pipeline.AddLast(new StringEncoder());          // 字节流 <-- 字符串
                        pipeline.AddLast(new SRgvDirectiveEncoder());   // 字符串 <-- 指令

                        pipeline.AddLast(new SRgvHandler(this));
                    }));
            }

            try
            {
                clientChannel = await _bootstrap.ConnectAsync(_endPoint);
                _logger.Information("连接设备成功");
            }
            catch (Exception ex)
            {
                throw new Exception("连接设备失败", ex);
            }
            finally
            {
                _connecting = false;
            }
        }

        /// <summary>
        /// 从设备断开连接
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectAsync()
        {
            if (clientChannel == null || clientChannel.Active == false)
            {
                _logger.Warning("未连接，忽略");
                return;
            }

            await clientChannel.CloseAsync();
            clientChannel = null;
            _logger.Information("已与设备断开连接");
        }

        /// <summary>
        /// 指示当前是否连接到了设备
        /// </summary>
        public DeviceConnectionState ConnectionState
        {
            get
            {
                if (clientChannel?.Active == true)
                {
                    return DeviceConnectionState.Connected;
                }
                else if (_connecting)
                {
                    return DeviceConnectionState.Connecting;
                }
                else
                {
                    return DeviceConnectionState.Disconnected;
                }
            }
        }

        public string ProtocolVersion => "合肥井松";

        public CommunicatorStatistics Statistics { get; private set; } = CommunicatorStatistics.Empty;

        /// <summary>
        /// 收到设备上报的状态时引发，如果设备将相同状态连续发送两次，则此事件会引发两次。
        /// </summary>
        public event EventHandler<SRgvState>? StateMessageReceived;

        /// <summary>
        /// 引发 <see cref="StateMessageReceived"/> 事件。
        /// </summary>
        /// <param name="e"></param>
        internal void OnStateMessageReceived(SRgvState e)
        {
            Statistics = Statistics with
            {
                LastState = e,
                LastStateTime = DateTime.Now,
                StateMessageCount = Statistics.StateMessageCount + 1,
            };
            StateMessageReceived?.Invoke(this, e);
        }

        public async Task SendDirectiveAsync(SRgvDirective directive)
        {
            if (clientChannel != null)
            {
                await clientChannel.WriteAndFlushAsync(directive);
            }
        }

        public async Task ShutdownAsync()
        {
            if (this.ConnectionState == DeviceConnectionState.Connected)
            {
                await this.DisconnectAsync();
            }

            _bootstrap = null;
            if (_group != null)
            {
                await this._group.ShutdownGracefullyAsync();
                _group = null;
            }
        }
    }
}
