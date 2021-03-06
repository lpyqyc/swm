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
    /// 列表页的数据项
    /// </summary>
    public class StorageLocationListItem
    {
        /// <summary>
        /// 货位 Id
        /// </summary>
        public int LocationId { get; set; }

        /// <summary>
        /// 货位编码
        /// </summary>
        public string LocationCode { get; set; } = default!;

        /// <summary>
        /// 巷道 Id
        /// </summary>
        public int LanewayId { get; set; }

        /// <summary>
        /// 巷道编码
        /// </summary>
        public string LanewayCode { get; set; } = default!;

        /// <summary>
        /// 限重
        /// </summary>
        public decimal WeightLimit { get; set; }

        /// <summary>
        /// 限高
        /// </summary>
        public decimal HeightLimit { get; set; }

        /// <summary>
        /// 入站数
        /// </summary>
        public int InboundCount { get; set; }

        /// <summary>
        /// 禁止入站
        /// </summary>
        public bool InboundDisabled { get; set; }

        /// <summary>
        /// 禁止入站备注
        /// </summary>
        public string? InboundDisabledComment { get; set; }

        /// <summary>
        /// 出站数
        /// </summary>
        public int OutboundCount { get; set; }

        /// <summary>
        /// 禁止出站
        /// </summary>
        public bool OutboundDisabled { get; set; }

        /// <summary>
        /// 禁止出站备注
        /// </summary>
        public string? OutboundDisabledComment { get; set; }

        /// <summary>
        /// 存储分组
        /// </summary>
        public string StorageGroup { get; set; } = default!;

        /// <summary>
        /// 货载数
        /// </summary>
        public int UnitloadCount { get; set; }

    }


}
