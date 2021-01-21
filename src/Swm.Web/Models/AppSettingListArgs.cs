using Arctic.NHibernateExtensions;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 列表查询参数
    /// </summary>
    public class AppSettingListArgs
    {
        /// <summary>
        /// 设置名称
        /// </summary>
        [SearchArg(SearchMode.Equal)]
        public string? SettingName { get; set; }


        /// <summary>
        /// 排序字段
        /// </summary>
        public string? Sort { get; set; }

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
