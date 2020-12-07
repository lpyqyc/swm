using System;
using System.ComponentModel.DataAnnotations;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 创建操作的参数
    /// </summary>
    public class CreateLocationArgs
    {
        /// <summary>
        /// Prop1
        /// </summary>
        [Required]
        public string? Prop1 { get; set; }

        /// <summary>
        /// Prop2
        /// </summary>
        public int Prop2 { get; set; }

    }


}
