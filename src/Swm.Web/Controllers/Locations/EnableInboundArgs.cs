namespace Swm.Web.Controllers
{
    /// <summary>
    /// 允许位置入站操作的参数
    /// </summary>
    public class EnableInboundArgs
    {
        /// <summary>
        /// 要允许入站的位置Id列表
        /// </summary>
        public int[] LocationIdList { get; set; } = default!;
    }

}
