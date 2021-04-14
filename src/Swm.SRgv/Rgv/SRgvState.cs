using System;

namespace Swm.Device.Rgv
{
    /// <summary>
    /// 表示单工位穿梭车上报的状态
    /// </summary>
    public record SRgvState
    {
        /// <summary>
        /// 指示穿梭车是否处于手动状态
        /// </summary>
        public bool InManualMode { get; init; }

        /// <summary>
        /// 穿梭车的错误号，null 表示无错误
        /// </summary>
        public int? ErrorCode { get; init; }

        /// <summary>
        /// 穿梭车接受的任务信息，未接受任务时为 null
        /// </summary>
        /// <remarks>
        /// 如果协议不完善，穿梭车上报的任务信息与下发的任务信息相比可能会有信息丢失，例如，下发时任务是左取货任务，上报时变为取货任务
        /// </remarks>
        public SRgvTaskInfo? TaskInfo { get; init; }

        /// <summary>
        /// 指示穿梭车是否已完成当前任务。
        /// </summary>
        public bool TaskCompleted { get; init; }

        /// <summary>
        /// 穿梭车的动作状态
        /// </summary>
        public SRgvActionState ActionState { get; init; } = new SRgvActionState.None();

        /// <summary>
        /// 穿梭车所在的位置
        /// </summary>
        public int Position { get; init; }

        /// <summary>
        /// 穿梭车所在站点，不在站点时为 null
        /// </summary>
        public int? StationNo { get; init; }
    }
}