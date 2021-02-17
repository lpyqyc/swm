using System.ComponentModel.DataAnnotations;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 禁用或启用位置的操作参数
    /// </summary>
    public class DisableLocationArgs
    {
        /// <summary>
        /// 禁用或启用位置的操作备注。
        /// </summary>
        [Required]
        public string Comment { get; set; } = default!;
    }

}
