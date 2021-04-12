using System;
using System.Threading.Tasks;

namespace Swm.Device
{
    /// <summary>
    /// 表示与设备通信的程序
    /// </summary>
    /// <typeparam name="TState">设备向上发送的消息类型</typeparam>
    /// <typeparam name="TDirective">向设备下发的消息类型</typeparam>
    public interface ICommunicator<TDirective, TState>
        where TDirective : notnull
        where TState : notnull
    {
        /// <summary>
        /// 协议版本，应体现公司、设备类型、时间等足够的信息。
        /// </summary>
        string ProtocolVersion { get; }

        /// <summary>
        /// 指示是否已连接到设备
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 在收到设备传来的状态消息时引发
        /// </summary>
        event EventHandler<TState> StateReceived;

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
        /// 向设备下发指令
        /// </summary>
        /// <param name="directive">要发送的指令</param>
        /// <returns></returns>
        Task SendDirectiveAsync(TDirective directive);


    }

}