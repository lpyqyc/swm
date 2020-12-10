using System.ComponentModel.DataAnnotations;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 禁止位置入站操作的参数
    /// </summary>
    public class DisableInboundArgs
    {
        /// <summary>
        /// 要禁止入站的位置Id列表
        /// </summary>
        public int[] LocationIdList { get; set; } = default!;

        /// <summary>
        /// 禁止入站的操作备注
        /// </summary>
        [Required]
        public string Comment { get; set; } = default!;
    }

}
