using System;
using System.ComponentModel.DataAnnotations;

namespace Swm.Model
{
    /// <summary>
    /// 表示单元货物明细的快照。
    /// </summary>
    public class UnitloadItemSnapshot
    {
        internal protected UnitloadItemSnapshot()
        {
        }

        public virtual int UnitloadItemSnapshotId { get; internal protected set; }

        /// <summary>
        /// 源库存项的Id
        /// </summary>
        public virtual int UnitloadItemId { get; internal protected set; }


        [Required]
        public virtual UnitloadSnapshot Unitload { get; internal protected set; }

        [Required]
        public virtual Material Material { get; set; }

        [Required]
        [MaxLength(FIELD_LENGTH.BATCH)]
        public virtual string Batch { get; set; }

        [Required]
        [MaxLength(FIELD_LENGTH.APP_CODE)]
        public virtual string StockStatus { get; set; }


        public virtual decimal Quantity { get; set; }

        [MaxLength(FIELD_LENGTH.UOM)]
        [Required]
        public virtual string Uom { get; set; }


        public virtual DateTime ProductionTime { get; set; }



    }


}
