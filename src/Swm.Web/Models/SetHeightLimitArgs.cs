using System.ComponentModel.DataAnnotations;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 设置限高的操作参数
    /// </summary>
    public class SetHeightLimitArgs
    {
        /// <summary>
        /// 限高。
        /// </summary>
        [Required]
        public decimal HeightLimit { get; set; }
    }
}
