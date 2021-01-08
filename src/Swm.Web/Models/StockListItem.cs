using System;
using System.ComponentModel.DataAnnotations;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 列表页的数据项。
    /// </summary>
    public class StockListItem
    {
        /// <summary>
        /// 库存Id
        /// </summary>
        public int StockId { get; set; }

        /// <summary>
        /// 库存最后一次变动时间
        /// </summary>
        public DateTime mtime { get; set; }

        /// <summary>
        /// 物料代码
        /// </summary>
        [Required]
        public string MaterialCode { get; set; }

        /// <summary>
        /// 物料描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 批号
        /// </summary>
        public string Batch { get; set; }

        /// <summary>
        /// 库存状态
        /// </summary>
        [MaxLength(10)]
        [Required]
        public string StockStatus { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// 计量单位
        /// </summary>
        [Required]
        public string Uom { get; set; }

        /// <summary>
        /// 出库次序
        /// </summary>
        [MaxLength(20)]
        [Required]
        public string OutOrdering { get; set; }

        /// <summary>
        /// 库龄基线
        /// </summary>
        public DateTime AgeBaseline { get; set; }

    }


}
