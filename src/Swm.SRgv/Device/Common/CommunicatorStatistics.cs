using Swm.Device.Rgv;
using System;

namespace Swm.Device
{
    public record CommunicatorStatistics
    {
        public static readonly CommunicatorStatistics Empty = new CommunicatorStatistics();

        /// <summary>
        /// 获取此实例的创建时间
        /// </summary>
        public DateTime StartTime { get; init; } = new DateTime();

        /// <summary>
        /// 获取此实例的持续时间
        /// </summary>
        public TimeSpan Duration
        {
            get
            {
                return DateTime.Now - StartTime;
            }
        }

        /// <summary>
        /// 获取此实例发送的指令消息数量
        /// </summary>
        public int DirectiveMessageCount { get; init; }

        /// <summary>
        /// 获取此实例最近一次发送的指令
        /// </summary>
        public SRgvDirective? LastDirective { get; init; }

        /// <summary>
        /// 获取此实例最近一次发送指令的时间
        /// </summary>
        public DateTime? LastDirectiveTime { get; init; }

        ///// <summary>
        ///// 获取此实例最近一次发送的指令的原始消息
        ///// </summary>
        //public string? LastDirectiveMessage { get; init; }

        /// <summary>
        /// 获取此实例接受的状态消息数量
        /// </summary>
        public int StateMessageCount { get; init; }

        /// <summary>
        /// 获取此实例最近一次发送的指令
        /// </summary>
        public SRgvState? LastState { get; init; }

        /// <summary>
        /// 获取此实例最近一次发送指令的时间
        /// </summary>
        public DateTime? LastStateTime { get; init; }
        
        ///// <summary>
        ///// 获取此实例最近一次接收的状态的原始消息
        ///// </summary>
        //public string? LastStateMessage { get; init; }



    }
}