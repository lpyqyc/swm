using System.Collections.Generic;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 列表页结果
    /// </summary>
    public class LocationList : OperationResult
    {
        /// <summary>
        /// 当前分页的数据
        /// </summary>
        public IEnumerable<LocationListItem>? Data { get; init; }

        /// <summary>
        /// 总共有多少个数据
        /// </summary>
        public int Total { get; init; }
    }


}
