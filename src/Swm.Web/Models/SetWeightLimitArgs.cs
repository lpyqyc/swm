using System.ComponentModel.DataAnnotations;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 设置限重的操作参数
    /// </summary>
    public class SetWeightLimitArgs
    {
        /// <summary>
        /// 限重。
        /// </summary>
        [Required]
        public decimal WeightLimit { get; set; }
    }
}
