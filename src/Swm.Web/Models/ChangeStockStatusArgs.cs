using System.ComponentModel.DataAnnotations;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 库存状态变更的操作参数
    /// </summary>
    public class ChangeStockStatusArgs
    {
        /// <summary>
        /// 发货库存状态
        /// </summary>
        [Required]
        public string IssuingStockStatus { get; set; } = default!;

        /// <summary>
        /// 收货库存状态
        /// </summary>
        [Required]
        public string ReceivingStockStatus { get; set; } = default!;

    }

}
