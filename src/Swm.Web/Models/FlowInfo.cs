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

using Swm.Materials;
using Swm.Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 流水信息
    /// </summary>
    public class FlowInfo
    {
        /// <summary>
        /// 流水Id
        /// </summary>
        public int FlowId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime ctime { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        [Required]
        public string? MaterialCode { get; set; }

        /// <summary>
        /// 物料描述
        /// </summary>
        [Required]
        public string? Description { get; set; }

        /// <summary>
        /// 批号
        /// </summary>
        [Required]
        public string? Batch { get; set; }

        /// <summary>
        /// 库存状态
        /// </summary>
        [Required]
        public string? StockStatus { get; set; }

        /// <summary>
        /// 业务类型
        /// </summary>
        [Required]
        public string? BizType { get; set; }

        /// <summary>
        /// 流动方向
        /// </summary>
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FlowDirection Direction { get; set; }

        /// <summary>
        /// 托盘号
        /// </summary>
        public string? PalletCode { get; set; }

        /// <summary>
        /// WMS 单号
        /// </summary>
        public string? OrderCode { get; set; }

        /// <summary>
        /// 业务单号
        /// </summary>
        public string? BizOrder { get; set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        public string? OperationType { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// 计量单位
        /// </summary>
        [Required]
        public string? Uom { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        public string? cuser { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Comment { get; set; }

    }


}