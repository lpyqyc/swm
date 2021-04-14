using System.ComponentModel;

namespace Swm.SRgv.GS
{
    /// <summary>
    /// 穿梭车状态
    /// </summary>
    public enum SRgvStatus
    {
        /// <summary>
        /// 初始化
        /// </summary>
        [Description("初始化")]
        初始化 = 0,

        /// <summary>
        /// 手动模式
        /// </summary>
        [Description("手动模式")]
        手动模式 = 1,

        /// <summary>
        /// 无货待命
        /// </summary>
        [Description("无货待命")]
        无货待命 = 2,

        /// <summary>
        /// 有货待命
        /// </summary>
        [Description("有货待命")]
        有货待命 = 3,

        /// <summary>
        /// 无货运行
        /// </summary>
        [Description("无货运行")]
        无货运行 = 4,

        /// <summary>
        /// 有货运行
        /// </summary>
        [Description("有货运行")]
        有货运行 = 5,

        /// <summary>
        /// 输送线运行
        /// </summary>
        [Description("输送线运行")]
        输送线运行 = 6,


        /// <summary>
        /// 停止
        /// </summary>
        [Description("停止")]
        停止 = 7,

        /// <summary>
        /// 报警停机
        /// </summary>
        [Description("报警停机")]
        报警停机 = 8,

        /// <summary>
        /// 有货
        /// </summary>
        [Description("有货")]
        有货 = 9,


    }

}

