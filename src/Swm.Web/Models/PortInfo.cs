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

using System;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 列表页的数据项
    /// </summary>
    public class PortInfo
    {
        /// <summary>
        /// 出口Id
        /// </summary>
        public int PortId { get; init; }

        /// <summary>
        /// 出口编码
        /// </summary>
        public string PortCode { get; init; } = default!;


        /// <summary>
        /// 出口的关键点1，不为 null
        /// </summary>
        public string KP1 { get; init; } = default!;

        /// <summary>
        /// 出口的关键点2，可能为 null
        /// </summary>
        public string? KP2 { get; init; }

        /// <summary>
        /// 可到达此出口的巷道
        /// </summary>
        public string[]? Laneways { get; init; }

        // TODO 重命名
        /// <summary>
        /// 当前下架的单据
        /// </summary>
        public string? CurrentUat { get; init; }

        // TODO 重命名
        /// <summary>
        /// 最近一次为此出口调度下架的时间
        /// </summary>
        public DateTime? CheckedAt { get; init; }

        /// <summary>
        /// 最近一次为此出口调度下架的消息
        /// </summary>
        public string? CheckMessage { get; init; }


    }


}
