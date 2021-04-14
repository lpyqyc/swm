namespace Swm.SRgv.GS
{
    /// <summary>
    /// 表示下发给 RGV 的指令报文
    /// </summary>
    internal record SRgvDirectiveTelex
    {
        /// <summary>
        /// 取货站链条动作
        /// </summary>
        public ChainAction StartingStationAction { get; init; }

        /// <summary>
        /// 放货站链条动作
        /// </summary>
        public ChainAction DestinationStationAction { get; init; }

        /// <summary>
        /// 托盘号（最长4位）
        /// </summary>
        public uint PalletCode { get; init; }

        /// <summary>
        /// 起始站
        /// </summary>
        public ushort StartingStation { get; init; }

        /// <summary>
        /// 目的站
        /// </summary>
        public ushort DestinationStation { get; init; }

        /// <summary>
        /// 任务模式
        /// </summary>
        public SRgvTaskMode TaskMode { get; init; }


        /// <summary>
        /// 任务号（最长8位数）
        /// </summary>
        public int TaskId { get; init; }

        /// <summary>
        /// 报文的功能字符
        /// </summary>
        public string? TypeFlag { get; init; }

        /// <summary>
        /// 将指令转换为添加了前缀和后缀的报文
        /// </summary>
        /// <returns></returns>
        public string GetText()
        {
            const char prefix = (char)2;
            const char suffix = (char)3;
            return string.Format("{0}{1}{2:00000000}{3:0000}{4:000}{5:000}{6}{7}{8}{9}0",
               prefix,
               TypeFlag,
               TaskId,
               PalletCode,
               StartingStation,
               DestinationStation,
               (int)StartingStationAction,
               (int)DestinationStationAction,
               (int)TaskMode,
               suffix);
        }
    }

}

