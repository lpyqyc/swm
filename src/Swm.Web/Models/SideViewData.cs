﻿// Copyright 2020-2021 王建军
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

using Swm.Model;
using System.Collections.Generic;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 侧视图数据。
    /// </summary>
    public class SideViewData
    {
        /// <summary>
        /// 巷道编码
        /// </summary>
        public string LanewayCode { get; set; } = default!;

        /// <summary>
        /// 巷道是否离线
        /// </summary>
        public bool Offline { get; set; }


        /// <summary>
        /// 巷道的货位数，不包含 <see cref="Location.Exists"/> 为 false 的货位。
        /// </summary>
        public int LocationCount { get; set; }

        /// <summary>
        /// 巷道的可用货位数，即存在、无货、无入站任务、未禁止入站的货位
        /// </summary>
        public int AvailableCount { get; set; }


        /// <summary>
        /// 巷道的货位，包含 <see cref="Location.Exists"/> 为 false 的货位。
        /// </summary>
        public List<SideViewLocation> Locations { get; set; } = default!;

        /// <summary>
        /// 巷道离线的备注
        /// </summary>
        public string? OfflineComment { get; set; }
    }

}
