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

using Arctic.NHibernateExtensions;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Swm.TransportTasks
{
    /// <summary>
    /// 
    /// </summary>
    public static class TransportTasksQueries
    {
        /// <summary>
        /// 获取具有指定任务号的任务。
        /// </summary>
        /// <param name="taskCode">任务号。</param>
        /// <returns></returns>
        public static async Task<TransportTask> GetTaskAsync(this IQueryable<TransportTask> q, string taskCode)
        {
            return await q.SingleOrDefaultAsync(x => x.TaskCode == taskCode).ConfigureAwait(false);
        }
    }


}
