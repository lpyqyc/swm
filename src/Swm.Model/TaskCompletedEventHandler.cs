using Arctic.EventBus;
using NHibernate;
using Serilog;
using Swm.Model.Cfg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Swm.Model
{
    public class TaskCompletedEventHandler : IEventHandler
    {
        readonly IEnumerable<Lazy<ICompletedTaskHandler, CompletedTaskHandlerMeta>> _completedTaskHandlers;
        readonly ILogger _logger;
        readonly ISession _session;
        public TaskCompletedEventHandler(IEnumerable<Lazy<ICompletedTaskHandler, CompletedTaskHandlerMeta>> completedTaskHandlers, ISession session, ILogger logger)
        {
            _completedTaskHandlers = completedTaskHandlers;
            _session = session;
            _logger = logger;
        }
        public async Task ProcessAsync(string eventType, object eventData)
        {
            await ProcessCompletedTaskAsync((CompletedTaskInfo)eventData);
        }

        private async Task ProcessCompletedTaskAsync(CompletedTaskInfo taskInfo)
        {
            string taskCode = taskInfo.TaskCode;

            _logger.Information("正在引发任务完成事件，任务类型 {taskType}，任务号 {taskCode}", taskInfo.TaskType, taskCode);

            if (taskInfo.TaskType == null)
            {
                string msg = string.Format("未提供任务类型。任务编号 {0}。", taskCode);
                throw new ApplicationException(msg);
            }

            ICompletedTaskHandler handler = GetCompletedTaskHandler(taskInfo.TaskType);
            if (handler == null)
            {
                throw new ApplicationException($"没有找到可用的完成处理程序。任务类型：{taskInfo.TaskType}。");
            }

            _logger.Debug("完成处理程序是 {handlerType}。", handler.GetType());

            // 判断任务是否存在
            TransportTask task = await _session.Query<TransportTask>().GetTaskAsync(taskInfo.TaskCode);
            if (task != null)
            {
                await handler.ProcessCompletedTaskAsync(taskInfo, task);
            }
            else
            {
                _logger.Warning("未找到任务 {taskCode}，已忽略", taskInfo.TaskCode);
            }
        }

        /// <summary>
        /// 为指定的任务类型创建处理程序。
        /// </summary>
        /// <param name="taskType"></param>
        /// <returns></returns>
        private ICompletedTaskHandler GetCompletedTaskHandler(string taskType)
        {
            var lazy = _completedTaskHandlers
                .Where(x => string.Equals(x.Metadata.TaskType, taskType, StringComparison.InvariantCultureIgnoreCase))
                .LastOrDefault();
            if (lazy == null)
            {
                return null;
            }
            return lazy.Value;
        }

    }

}
