namespace Swm.Web.Controllers
{
    /// <summary>
    /// 列表页的数据项
    /// </summary>
    public class LanewayListItem
    {
        /// <summary>
        /// 巷道Id
        /// </summary>
        public int LanewayId { get; init; }

        /// <summary>
        /// 巷道编码
        /// </summary>
        public string LanewayCode { get; init; } = default!;

        /// <summary>
        /// 是否自动化巷道
        /// </summary>
        public bool Automated { get; init; }

        /// <summary>
        /// 是否双深
        /// </summary>
        public bool DoubleDeep { get; init; }

        /// <summary>
        /// 是否离线
        /// </summary>
        public bool Offline { get; init; }

        /// <summary>
        /// 离线备注
        /// </summary>
        public string? OfflineComment { get; init; }

        /// <summary>
        /// 货位总数
        /// </summary>
        public int TotalLocationCount { get; init; }

        /// <summary>
        /// 可用货位数
        /// </summary>
        public int AvailableLocationCount { get; init; }

        /// <summary>
        /// 保留货位数
        /// </summary>
        public int ReservedLocationCount { get; init; }

        /// <summary>
        /// 可到达的出口
        /// </summary>
        public PortInfo[] Ports { get; init; } = default!;

        /// <summary>
        /// 总脱机时间
        /// </summary>
        public double TotalOfflineHours { get; init; }


        /// <summary>
        /// 出口信息
        /// </summary>
        public class PortInfo
        {
            /// <summary>
            /// 出口Id
            /// </summary>
            public int PortId { get; init; }

            /// <summary>
            /// 出口编码
            /// </summary>
            public string PortCode { get; init; }        
        }

    }

    
}
