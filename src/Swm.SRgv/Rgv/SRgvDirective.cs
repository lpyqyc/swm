using System;

namespace Swm.Device.Rgv
{
    /// <summary>
    /// 表示下发给单工位穿梭车的指令
    /// </summary>
    public abstract record SRgvDirective
    {
        /// <summary>
        /// 判断状态消息是否是对此指令的成功响应。
        /// </summary>
        public bool IsSuccessfulResponse(SRgvState? state) =>
             this switch
             {
                 Inquire _ => state != null,
                 SendTask sendTask => state?.TaskInfo?.TaskNo == sendTask.TaskInfo.TaskNo,
                 ClearTask _ => state != null && state.TaskInfo == null,
                 _ => throw new Exception("不认识的分支")
             };

        /// <summary>
        /// 查询指令
        /// </summary>
        public record Inquire : SRgvDirective;

        /// <summary>
        /// 发送任务指令
        /// </summary>
        public record SendTask(SRgvTaskInfo TaskInfo) : SRgvDirective
        {
            /// <summary>
            /// 任务信息
            /// </summary>
            public SRgvTaskInfo TaskInfo { get; init; } = TaskInfo;
        }

        /// <summary>
        /// 清除任务指令
        /// </summary>
        public record ClearTask() : SRgvDirective;

        /// <summary>
        /// 清除错误指令
        /// </summary>
        public record ClearError() : SRgvDirective;
    }

}