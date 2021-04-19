using System.ComponentModel.DataAnnotations;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 库存状态变更的操作参数
    /// </summary>
    public class ChangeStockStatusArgs
    {
        /// <summary>
        /// 要变更状态的货载项
        /// </summary>
        public int[] UnitloadItemIds { get; set; } = new int[0];

        /// <summary>
        /// 业务类型
        /// </summary>
        [Required]
        public string? BizType { get; set; }


        // TODO 重复的逻辑
        /// <summary>
        /// 获取发货库存状态
        /// </summary>
        public string IssuingStockStatus => BizType switch
        {
            "待检转合格" => "待检",
            "待检转不合格" => "待检",
            "不合格转合格" => "不合格",
            "合格转不合格" => "合格",
            _ => throw new(),

        };

        /// <summary>
        /// 获取收货库存状态
        /// </summary>
        public string ReceivingStockStatus => BizType switch
        {
            "待检转合格" => "合格",
            "待检转不合格" => "不合格",
            "不合格转合格" => "合格",
            "合格转不合格" => "不合格",
            _ => throw new(),

        };

    }

}
