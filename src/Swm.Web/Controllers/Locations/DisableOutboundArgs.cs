namespace Swm.Web.Controllers
{
    /// <summary>
    /// 禁止位置出站操作的参数
    /// </summary>
    public class DisableOutboundArgs
    {
        /// <summary>
        /// 要禁止出站的位置Id列表
        /// </summary>
        public int[] LocationIdList { get; set; } = default!;

        /// <summary>
        /// 禁止出站的操作备注
        /// </summary>
        public string Comment { get; set; } = default!;
    }
}
