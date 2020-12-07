using Swm.Model;
using System;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 列表查询参数
    /// </summary>
    public class LanewayListArgs : IListArgs<Laneway>
    {
        /// <summary>
        /// 支持模糊查找，使用 ? 表示单个字符，使用 * 表示任意个字符
        /// </summary>
        [ListFilter(ListFilterOperator.Like)]
        public string? LanewayCode { get; set; }

        /// <summary>
        /// 排序字段
        /// </summary>
        public OrderedDictionary? Sort { get; set; }

        /// <summary>
        /// 基于 1 的当前页面，默认值为 1。
        /// </summary>
        public int? Current { get; set; } = 1;

        /// <summary>
        /// 每页大小，默认值为 10。
        /// </summary>
        public int? PageSize { get; set; }

    }

    /// <summary>
    /// 脱机巷道的操作参数
    /// </summary>
    public class TakeOfflineArgs
    {
        /// <summary>
        /// 脱机操作备注
        /// </summary>
        [Required]
        public string Comment { get; set; } = default!;
    }

    /// <summary>
    /// 联机巷道的操作参数
    /// </summary>
    public class TakeOnlineArgs
    {
        /// <summary>
        /// 联机操作备注，非必填
        /// </summary>
        public string Comment { get; set; } = default!;
    }

    /// <summary>
    /// 设置巷道出口的操作参数。
    /// </summary>
    public class SetPortsArgs
    {
        /// <summary>
        /// 出口Id列表
        /// </summary>
        [Required]
        public int[] PortIdList { get; set; } = default!;
    }
}
