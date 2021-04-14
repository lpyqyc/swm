namespace Swm.Device
{
    /// <summary>
    /// 表示设备连接状态
    /// </summary>
    public enum DeviceConnectionState
    {        
        /// <summary>
        /// 未连接或已断开
        /// </summary>
        Disconnected,

        /// <summary>
        /// 正在连接
        /// </summary>
        Connecting,

        /// <summary>
        /// 已连接
        /// </summary>
        Connected,
    }

}