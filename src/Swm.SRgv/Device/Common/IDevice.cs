using System;
using System.Threading.Tasks;

namespace Swm.Device
{
    public interface IDevice 
    {
        /// <summary>
        /// 连接到设备
        /// </summary>
        /// <returns></returns>
        Task ConnectAsync();

        /// <summary>
        /// 与设备断开连接
        /// </summary>
        /// <returns></returns>
        Task DisconnectAsync();

        /// <summary>
        /// 连接状态
        /// </summary>
        DeviceConnectionState ConnectionState { get; }


        /// <summary>
        /// 与断开连接，并释放资源。
        /// </summary>
        Task ShutdownAsync();

        /// <summary>
        /// 锁定设备
        /// </summary>
        /// <returns></returns>
        Task LockAsync(string password);

        /// <summary>
        /// 解锁设备
        /// </summary>
        /// <returns></returns>
        Task UnlockAsync(string password);

        /// <summary>
        /// 指示设备是否被锁定
        /// </summary>
        bool IsLocked { get; }

        /// <summary>
        /// 状态发生变化时引发
        /// </summary>
        event EventHandler StateChanged;


    }




}
