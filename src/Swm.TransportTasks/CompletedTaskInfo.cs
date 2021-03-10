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

using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Swm.TransportTasks
{
    /// <summary>
    /// 表示已完成的任务信息。
    /// </summary>
    public class CompletedTaskInfo
    {
        /// <summary>
        /// 任务编号。
        /// </summary>
        public string TaskCode { get; set; }

        /// <summary>
        /// 任务类型。
        /// </summary>
        public string TaskType { get; set; }


        /// <summary>
        /// 指示任务是否已被取消。
        /// </summary>
        public bool Cancelled { get; set; }


        /// <summary>
        /// 实际完成位置。
        /// </summary>
        public string ActualEnd { get; set; }


        public Dictionary<string, string> AdditionalInfo { get; set; }

        /// <summary>
        /// 返回表示此实例的字符串。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
