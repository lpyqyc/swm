// Copyright 2020 王建军
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using NHibernate;
using Serilog;
using System.Threading.Tasks;

namespace Swm.Model.Extentions
{
    public class 上架完成处理程序 : ICompletedTaskHandler
    {
        readonly ISession _session;
        readonly ILogger _logger;
        public TaskHelper _taskHelper { get; set; }

        public 上架完成处理程序(ISession session, ILogger logger)
        {
            _session = session;
            _logger = logger;
        }

        public async Task ProcessCompletedTaskAsync(CompletedTaskInfo taskInfo, TransportTask task)
        {
            Location actualEnd = task.End;
            if (string.IsNullOrEmpty(taskInfo.ActualEnd) == false)
            {
                actualEnd = await _session.Query<Location>().GetAsync(taskInfo.ActualEnd).ConfigureAwait(false);
            }

            switch (taskInfo.Cancelled)
            {
                case false:
                    await _taskHelper.CompleteAsync(task, actualEnd, false).ConfigureAwait(false);
                    break;
                case true:
                default:
                    await _taskHelper.CancelAsync(task).ConfigureAwait(false);
                    break;
            }
        }
    }

}
