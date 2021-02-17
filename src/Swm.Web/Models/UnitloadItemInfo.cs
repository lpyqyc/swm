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
    // TODO 多处引用，写转换函数
    /// <summary>
    /// 货载明细信息
    /// </summary>
    public class UnitloadItemInfo
    {
        /// <summary>
        /// 货载项Id
        /// </summary>
        public int UnitloadItemId { get; set; }

        /// <summary>
        /// 托盘号
        /// </summary>
        public string PalletCode { get; set; } = default!;

        /// <summary>
        /// 所在货位编码
        /// </summary>
        public string LocationCode { get; set; } = default!;


        /// <summary>
        /// 所在巷道编码
        /// </summary>
        public string? LanewayCode { get; set; }

        /// <summary>
        /// 托盘是否已分配
        /// </summary>
        public bool Allocated { get; set; }

        /// <summary>
        /// 托盘是否有任务
        /// </summary>
        public bool BeingMoved { get; set; }

        /// <summary>
        /// 托盘是否有盘点错误
        /// </summary>
        public bool HasCountingError { get; set; }

        /// <summary>
        /// 物料Id
        /// </summary>
        public int MaterialId { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; } = default!;

        /// <summary>
        /// 物料类型
        /// </summary>
        public string MaterialType { get; set; } = default!;

        /// <summary>
        /// 物料描述
        /// </summary>
        public string Description { get; set; } = default!;

        /// <summary>
        /// 物料规格
        /// </summary>
        public string Specification { get; set; } = default!;

        /// <summary>
        /// 批号
        /// </summary>
        public string Batch { get; set; } = default!;

        /// <summary>
        /// 库存状态
        /// </summary>
        public string StockStatus { get; set; } = default!;

        /// <summary>
        /// 是否可变更库存状态
        /// </summary>
        public bool CanChangeStockStatus { get; set; }

        /// <summary>
        /// 不可变更库存状态的原因
        /// </summary>
        public string ReasonWhyStockStatusCannotBeChanged { get; set; } = string.Empty;

        /// <summary>
        /// 数量
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// 计量单位
        /// </summary>
        public string Uom { get; set; } = default!;
    }

}
