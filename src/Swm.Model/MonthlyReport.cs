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

using Arctic.Auditing;
using System;
using System.Collections.Generic;

namespace Swm.Model
{
    /// <summary>
    /// 月报
    /// </summary>
    public class MonthlyReport : IHasCtime
    {
        /// <summary>
        /// 月报的月份，取值为每个月的1号零点。
        /// </summary>
        public virtual DateTime Month { get; set; }

        /// <summary>
        /// 数据创建时间。
        /// </summary>
        public virtual DateTime ctime { get; set; }

        /// <summary>
        /// 用于标识 unsaved 状态，不用于版本控制
        /// </summary>
        protected virtual int v { get; set; }

        /// <summary>
        /// 报表条目。
        /// </summary>
        public virtual ISet<MonthlyReportItem> Items { get; protected set; } = new HashSet<MonthlyReportItem>();
    }

}
