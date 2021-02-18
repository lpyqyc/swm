using System;
using System.ComponentModel.DataAnnotations;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 操作记录信息
    /// </summary>
    public class OpListInfo
    {
        /// <summary>
        /// 操作记录 Id
        /// </summary>
        public int OpId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime ctime { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        [Required]
        public string? cuser { get; set; }


        /// <summary>
        /// 操作类型
        /// </summary>
        [Required]
        public string OperationType { get; set; } = default!;

        /// <summary>
        /// 产生此记录的 Url
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [MaxLength(2048)]
        public string? Comment { get; set; }

    }


}
