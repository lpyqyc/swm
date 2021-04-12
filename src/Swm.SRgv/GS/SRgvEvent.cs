using System.ComponentModel;

namespace Swm.SRgv.GS
{
    /// <summary>
    /// 穿梭车事件
    /// </summary>
    public enum SRgvEvent
    {
        /// <summary>无货无任务</summary>
        [Description("无货无任务")]
        Initialized = 0,

        /// <summary>接到任务未运行</summary>
        [Description("接到任务未运行")]
        TaskedAndNotRunning = 1,

        /// <summary>行走运行</summary>
        [Description("行走运行")]
        Running = 2,

        /// <summary>到达起始点</summary>
        [Description("到达起始点")]
        ArrivedStartStation = 3,


        /// <summary>到达目的地</summary>
        [Description("到达目的地")]
        ArrivedEndStation = 4,

        /// <summary>执行让道任务</summary>
        [Description("执行让道任务")]
        ClearedTheWay = 5,

        /// <summary>
        /// 自动任务完成
        /// </summary>
        [Description("自动任务完成")]
        AutomaticTaskCompletion = 6,

        /// <summary>
        /// 手动报完成
        /// </summary>
        [Description("手动报完成")]
        TaskCompletionByManual = 7,

        /// <summary>
        /// 交接货超时
        /// </summary>
        [Description("交接货超时")]
        交接货超时 = 8,
    }

}

