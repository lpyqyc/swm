using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Swm.Model
{
    /// <summary>
    /// 表示已完成的任务信息。
    /// </summary>
    public class CompletedTaskInfo
    {
        /// <summary>
        /// 任务编号。
        /// </summary>
        public string TaskCode { get; set; }

        /// <summary>
        /// 任务类型。
        /// </summary>
        public string TaskType { get; set; }


        /// <summary>
        /// 指示任务是否已被取消。
        /// </summary>
        public bool Cancelled { get; set; }


        /// <summary>
        /// 实际完成位置。
        /// </summary>
        public string ActualEnd { get; set; }


        public Dictionary<string, string> AdditionalInfo { get; set; }

        /// <summary>
        /// 返回表示此实例的字符串。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
