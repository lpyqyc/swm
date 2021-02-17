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

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 巷道信息
    /// </summary>
    public class LanewayInfo
    {
        /// <summary>
        /// 巷道Id
        /// </summary>
        public int LanewayId { get; init; }

        /// <summary>
        /// 巷道编码
        /// </summary>
        public string LanewayCode { get; init; } = default!;

        /// <summary>
        /// 是否自动化巷道
        /// </summary>
        public bool Automated { get; init; }

        /// <summary>
        /// 是否双深
        /// </summary>
        public bool DoubleDeep { get; init; }

        /// <summary>
        /// 是否离线
        /// </summary>
        public bool Offline { get; init; }

        /// <summary>
        /// 离线备注
        /// </summary>
        public string? OfflineComment { get; init; }

        /// <summary>
        /// 货位总数
        /// </summary>
        public int TotalLocationCount { get; init; }

        /// <summary>
        /// 可用货位数
        /// </summary>
        public int AvailableLocationCount { get; init; }

        /// <summary>
        /// 保留货位数
        /// </summary>
        public int ReservedLocationCount { get; init; }

        /// <summary>
        /// 货位使用率
        /// </summary>
        public double UsageRate { get; init; }

        /// <summary>
        /// 货位使用数据
        /// </summary>
        public LanewayUsageInfo[]? UsageInfos { get; init; }

        /// <summary>
        /// 可到达的出口
        /// </summary>
        public PortInfo[] Ports { get; init; } = default!;

        /// <summary>
        /// 总脱机时间
        /// </summary>
        public double TotalOfflineHours { get; init; }

    }

}
