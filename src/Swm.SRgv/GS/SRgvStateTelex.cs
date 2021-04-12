namespace Swm.SRgv.GS
{
    /// <summary>
    /// 表示 Rgv 上报的报文结构
    /// </summary>
    internal record SRgvStateTelex
    {
        /// <summary>
        /// 位置值 
        /// </summary>
        public uint Position { get; init; }

        /// <summary>
        /// 当前站点
        /// </summary>
        public ushort CurrentStation { get; init; }

        /// <summary>
        /// 是否在站点
        /// </summary>
        public bool AtStation { get; init; }

        /// <summary>
        /// 错误号，0 表示无错误
        /// </summary>
        public int ErrorCode { get; init; }

        /// <summary>
        /// 状态
        /// </summary>
        public RailGuidedVehicleStatus State { get; init; }

        /// <summary>
        /// 事件
        /// </summary>
        public SRgvEvent Event { get; init; }

        /// <summary>
        /// 任务号
        /// </summary>
        public int TaskId { get; init; }

        /// <summary>
        /// 托盘号
        /// </summary>
        public uint ContainerCode { get; init; }

        /// <summary>
        /// 起始站
        /// </summary>
        public ushort FromStation { get; init; }

        /// <summary>
        /// 目的站
        /// </summary>
        public ushort ToStation { get; init; }

        /// <summary>
        /// 任务模式
        /// </summary>
        public RailGuidedVehicleTaskMode TaskMode { get; init; }

        /// <summary>
        /// 不带前缀和后缀的原始报文
        /// </summary>
        public string? RawMessage { get; init; }


    }

}

