using System.ComponentModel.DataAnnotations;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 禁止位置出站操作的参数
    /// </summary>
    public class DisableOutboundArgs
    {
        /// <summary>
        /// 禁止出站的操作备注
        /// </summary>
        [Required]
        public string Comment { get; set; } = default!;
    }
}
