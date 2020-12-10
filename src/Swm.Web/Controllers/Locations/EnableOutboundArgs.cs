namespace Swm.Web.Controllers
{
    /// <summary>
    /// 允许位置入站操作的参数
    /// </summary>
    public class EnableOutboundArgs
    {
        /// <summary>
        /// 要允许出站的位置Id列表
        /// </summary>
        public int[] LocationIdList { get; set; } = default!;
    }
}
