using System;
using System.Threading.Tasks;

namespace Swm.Device
{
    /// <summary>
    /// 表示与设备通信的程序
    /// </summary>
    /// <typeparam name="TDirective">下发给设备的指令</typeparam>
    /// <typeparam name="TState">设备上报的状态</typeparam>
    public interface ICommunicator<TDirective, TState>
        where TDirective : notnull
        where TState : notnull
    {
        /// <summary>
        /// 协议版本，应体现公司、设备类型、时间等足够的信息。
        /// </summary>
        string ProtocolVersion { get; }

        /// <summary>
        /// 指示是否设备连接状态
        /// </summary>
        DeviceConnectionState ConnectionState { get; }

        /// <summary>
        /// 在收到设备传来的状态消息时引发，若设备断开，则留在缓冲区尚未消耗的消息不会引发此事件
        /// </summary>
        event EventHandler<TState> StateMessageReceived;

        /// <summary>
        /// 连接到设备
        /// </summary>
        /// <returns></returns>
        Task ConnectAsync();

        /// <summary>
        /// 与设备断开连接
        /// </summary>
        /// <returns></returns>
        Task DisconnectAsync();

        /// <summary>
        /// 在与设备连接成功后引发
        /// </summary>
        event EventHandler? Connected;

        /// <summary>
        /// 在与设备断开连接后引发
        /// </summary>
        event EventHandler? Disconnected;

        /// <summary>
        /// 与设备断开连接，并释放资源。
        /// </summary>
        /// <returns></returns>
        Task ShutdownAsync();

        /// <summary>
        /// 向设备下发指令
        /// </summary>
        /// <param name="directive">要发送的指令</param>
        /// <returns></returns>
        Task SendDirectiveAsync(TDirective directive);

        /// <summary>
        /// 获取统计数据
        /// </summary>
        CommunicatorStatistics Statistics { get; }
    }
}