using System.ComponentModel.DataAnnotations;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 设置存储分组的操作参数
    /// </summary>
    public class SetStorageGroupArgs
    {
        /// <summary>
        /// 禁用或启用位置的操作备注。
        /// </summary>
        [Required]
        public string StorageGroup { get; set; } = default!;
    }

}
