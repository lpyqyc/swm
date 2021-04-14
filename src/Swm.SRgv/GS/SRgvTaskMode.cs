using System;
using System.ComponentModel;

namespace Swm.SRgv.GS
{
    /// <summary>
    /// 任务模式
    /// </summary>
    public enum SRgvTaskMode
    {
        /// <summary>无任务类型</summary>
        [Description("无任务类型")]
        Initialized = 0,

        /// <summary>表示HB任务
        /// 全自动任务
        /// </summary>
        [Description("全自动任务")]
        [Obsolete("穿梭车已改为上位机拆分任务，该任务类型已取消。")]
        AutomaticTask = 1,

        /// <summary>取货任务</summary>
        [Description("取货任务")]
        Picking = 2,

        /// <summary>放货任务</summary>
        [Description("放货任务")]
        Putting = 3,


        /// <summary>有货行走</summary>
        [Description("有货行走")]
        WalkWithGoods = 4,

        /// <summary>无货行走</summary>
        [Description("无货行走")]
        Walk = 5,

    }

}

