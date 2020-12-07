namespace Swm.Web.Controllers
{
    /// <summary>
    /// 选择列表的数据项
    /// </summary>
    public class LanewaySelectListItem
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
        /// 是否离线
        /// </summary>
        public bool Offline { get; init; }

    }

    
}
