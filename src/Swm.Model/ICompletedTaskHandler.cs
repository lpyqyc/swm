// Copyright 2020-2021 王建军
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

using System.Threading.Tasks;

namespace Swm.Model
{
    /// <summary>
    /// 定义任务完成处理程序。
    /// </summary>
    public interface ICompletedTaskHandler
    {
        /// <summary>
        /// 处理已完成的任务。
        /// </summary>
        /// <param name="taskInfo">已完成的任务信息。</param>
        /// <param name="task">任务对象，此对象中的数据处于完成前的状态。</param>
        Task ProcessCompletedTaskAsync(CompletedTaskInfo taskInfo, TransportTask task);
    }
}
