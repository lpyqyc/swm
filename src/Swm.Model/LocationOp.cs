using Arctic.Auditing;
using System;
using System.ComponentModel.DataAnnotations;

namespace Swm.Model
{
    /// <summary>
    /// 表示货位上的操作记录。
    /// </summary>
    public class LocationOp : IHasCtime, IHasCuser
    {
        public virtual int LocationOpId { get; internal protected set; }

        [Required]
        public virtual Location Location { get; set; }

        [Required]
        [MaxLength(FIELD_LENGTH.OP_TYPE)]
        public virtual string OpType { get; set; }

        public virtual DateTime ctime { get; set; }

        [MaxLength(FIELD_LENGTH.USERNAME)]
        public virtual string cuser { get; set; }

        [MaxLength(9999)]
        public virtual string Comment { get; set; }

    }

}
