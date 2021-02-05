using Arctic.NHibernateExtensions;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 参数列表查询参数
    /// </summary>
    public class AppSettingListArgs
    {
        /// <summary>
        /// 参数名称
        /// </summary>
        [SearchArg(SearchMode.Equal)]
        public string? SettingName { get; set; }

    }


}
