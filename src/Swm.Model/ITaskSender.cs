namespace Swm.Model
{
    /// <summary>
    /// 定义 WCS 任务下发程序。
    /// </summary>
    public interface ITaskSender
    {
        /// <summary>
        /// 将任务下发给WCS。
        /// </summary>
        /// <param name="task"></param>
        void SendTask(TransportTask task);
    }

}
