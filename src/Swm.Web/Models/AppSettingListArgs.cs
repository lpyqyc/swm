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

    }


}
