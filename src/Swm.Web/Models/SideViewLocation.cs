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

using Swm.Locations;
using Swm.Model;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 侧视图货位数据
    /// </summary>
    public class SideViewLocation
    {
        /// <summary>
        /// 货位Id
        /// </summary>
        public int LocationId { get; set; }

        /// <summary>
        /// 货位编码
        /// </summary>
        [Required]
        public string LocationCode { get; set; } = default!;

        /// <summary>
        /// 是否有货
        /// </summary>
        public bool Loaded { get; set; }

        /// <summary>
        /// 货架在巷道的哪一侧
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RackSide Side { get; set; }

        /// <summary>
        /// 货架是第几深位
        /// </summary>
        public int Deep { get; set; }

        /// <summary>
        /// 所在层
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 所在列
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// 是否禁止入站
        /// </summary>
        public bool InboundDisabled { get; set; }

        /// <summary>
        /// 禁止入站备注
        /// </summary>
        public string? InboundDisabledComment { get; set; }

        /// <summary>
        /// 入站数
        /// </summary>
        public int InboundCount { get; set; }

        /// <summary>
        /// 入站限制
        /// </summary>
        public int InboundLimit { get; set; }

        /// <summary>
        /// 是否禁止出站
        /// </summary>
        public bool OutboundDisabled { get; set; }

        /// <summary>
        /// 禁止出站备注
        /// </summary>
        public string? OutboundDisabledComment { get; set; }

        /// <summary>
        /// 出站限制
        /// </summary>
        public int OutboundLimit { get; set; }

        /// <summary>
        /// 出站数
        /// </summary>
        public int OutboundCount { get; set; }

        /// <summary>
        /// 货位规格
        /// </summary>
        public string Specification { get; set; } = default!;

        /// <summary>
        /// 存储分组
        /// </summary>
        [Required]
        public string StorageGroup { get; set; } = default!;

        /// <summary>
        /// 限重
        /// </summary>
        public decimal WeightLimit { get; set; }

        /// <summary>
        /// 限高
        /// </summary>
        public decimal HeightLimit { get; set; }

        /// <summary>
        /// 货位是否存在
        /// </summary>
        public bool Exists { get; set; }

        /// <summary>
        /// 入次序1
        /// </summary>
        public int i1 { get; set; }

        /// <summary>
        /// 出次序1
        /// </summary>
        public int o1 { get; set; }

        /// <summary>
        /// 入次序2
        /// </summary>
        public int i2 { get; set; }

        /// <summary>
        /// 出次序2
        /// </summary>
        public int o2 { get; set; }

        /// <summary>
        /// 入次序3
        /// </summary>
        public int i3 { get; set; }

        /// <summary>
        /// 出次序3
        /// </summary>
        public int o3 { get; set; }

        // TODO 多个如何处理
        ///// <summary>
        ///// 物料编码
        ///// </summary>
        //public string? MaterialCode { get; set; }

        ///// <summary>
        ///// 批号
        ///// </summary>
        //public string? Batch { get; set; }

    }

}
