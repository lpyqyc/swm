using Arctic.NHibernateExtensions;
using System;
using System.Collections.Specialized;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 列表查询参数
    /// </summary>
    public class OpListArgs
    {
        /// <summary>
        /// 支持模糊查找，使用 ? 表示单个字符，使用 * 表示任意个字符
        /// </summary>
        [SearchArg(SearchMode.Like)]
        public string? Prop1 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [SearchArg(SearchMode.GreaterThan)]
        public int? Prop2From { get; set; }


        /// <summary>
        /// 
        /// </summary>
        [SearchArg(SearchMode.Less)]
        public DateTime? Prop2To { get; set; }

        /// <summary>
        /// 排序字段
        /// </summary>
        public OrderedDictionary? Sort { get; set; }

        /// <summary>
        /// 基于 1 的当前页面。
        /// </summary>
        public int? Current { get; set; }

        /// <summary>
        /// 每页大小
        /// </summary>
        public int? PageSize { get; set; }

    }
}