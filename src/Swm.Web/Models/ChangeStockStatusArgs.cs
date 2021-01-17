namespace Swm.Web.Controllers
{
    /// <summary>
    /// 库存状态变更操作的参数
    /// </summary>
    public class ChangeStockStatusArgs
    {
        /// <summary>
        /// 发出的库存状态
        /// </summary>
        public string IssuingStockStatus { get; set; } = default!;

        /// <summary>
        /// 接收的库存状态
        /// </summary>
        public string ReceivingStockStatus { get; set; } = default!;

    }

}
