using System;

namespace Swm.Device
{
    // TODO 重命名
    [Flags]
    public enum CannotSendDirectiveReason
    {
        /// <summary>
        /// 可以下发指令
        /// </summary>
        可以下发指令,

        /// <summary>
        /// 未连接到设备
        /// </summary>
        未连接到设备 = 1 << 0,

        /// <summary>
        /// 有正在处理的指令
        /// </summary>
        有正在处理的指令 = 1 << 1,


        /// <summary>
        /// 设备状态未知
        /// </summary>
        设备状态未知 = 1 << 2,


        /// <summary>
        /// 设备处于手动模式
        /// </summary>
        设备处于手动模式 = 1 << 3,

        /// <summary>
        /// 设备报警停机
        /// </summary>
        设备报警停机 = 1 << 4,

        /// <summary>
        /// 任务已完成，防止二次下发任务
        /// </summary>
        任务已完成 = 1 << 5,

        /// <summary>
        /// 设备有任务
        /// </summary>
        设备有任务 = 1 << 6,

        /// <summary>
        /// 设备出错，错误号不为零
        /// </summary>
        设备出错 = 1 << 7,

    }


}
