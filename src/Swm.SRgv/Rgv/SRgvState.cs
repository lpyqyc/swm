using System;

namespace Swm.Device.Rgv
{
    /// <summary>
    /// 表示单工位穿梭车的状态
    /// </summary>
    public record SRgvState
    {
        /// <summary>
        /// 指示穿梭车是否处于手动状态
        /// </summary>
        public bool InManualMode { get; init; }

        /// <summary>
        /// 指示穿梭车是否处于急停状态
        /// </summary>
        public bool? EStopped { get; init; }

        /// <summary>
        /// 指示穿梭车是否处于锁定状态
        /// </summary>
        public bool? Locked { get; init; }

        /// <summary>
        /// 穿梭车的错误号，null 表示无错误
        /// </summary>
        public int? ErrorCode { get; init; }

        /// <summary>
        /// 穿梭车接受的任务信息，未接受任务时为 null
        /// </summary>
        public SRgvTaskInfo? TaskInfo { get; init; }

        /// <summary>
        /// 穿梭车所在的位置
        /// </summary>
        public int Position { get; init; }

        /// <summary>
        /// 穿梭车所在站点，不在站点时为 null
        /// </summary>
        public int? StationNo { get; init; }

        /// <summary>
        /// 指示穿梭车输送机上是否有货
        /// </summary>
        public bool Occupied { get; init; }

        /// <summary>
        /// 指示穿梭车当前是否正在行走
        /// </summary>
        public bool Walking { get; init; }

        /// <summary>
        /// 指示穿梭车输送机是否正在取货
        /// </summary>
        public bool Loading { get; init; }

        /// <summary>
        /// 指示穿梭车输送机是否正在放货
        /// </summary>
        public bool Unloading { get; init; }
    }

}