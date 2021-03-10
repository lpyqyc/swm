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

using NHibernate;
using Swm.TransportTasks;
using System;

namespace Swm.Model.Extentions
{
    public class TaskSender : ITaskSender
    {
        ISession _session;
        public TaskSender(ISession session)
        {
            _session = session;
        }
        public void SendTask(TransportTask task)
        {
            task.WasSentToWcs = true;
            task.SendTime = DateTime.Now;
            _session.Save(task);
        }
    }

}
