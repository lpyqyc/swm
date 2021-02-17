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

using System.ComponentModel.DataAnnotations;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 物料主数据信息
    /// </summary>
    public class MaterialInfo
    {
        /// <summary>
        /// 物料Id
        /// </summary>
        public int MaterialId { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        [Required]
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
        /// 规格
        /// </summary>
        public string Specification { get; set; } = default!;

        /// <summary>
        /// 是否启用批次管理
        /// </summary>
        public bool BatchEnabled { get; set; }

        /// <summary>
        /// 物料分组
        /// </summary>
        public string? MaterialGroup { get; set; }

        /// <summary>
        /// 有效天数
        /// </summary>
        public decimal ValidDays { get; set; }

        /// <summary>
        /// 静置时间
        /// </summary>
        public decimal StandingTime { get; set; }

        /// <summary>
        /// ABC分类
        /// </summary>
        public string? AbcClass { get; set; }

        /// <summary>
        /// 计量单位
        /// </summary>
        public string Uom { get; set; } = default!;

        /// <summary>
        /// 库存下边界
        /// </summary>
        public decimal LowerBound { get; set; }

        /// <summary>
        /// 库存上边界
        /// </summary>
        public decimal UpperBound { get; set; }

        // TODO 重命名，分离到单独的结构中
        /// <summary>
        /// 默认数量
        /// </summary>
        public decimal DefaultQuantity { get; set; }

        /// <summary>
        /// 默认存储分组
        /// </summary>
        public string DefaultStorageGroup { get; set; } = default!;

        /// <summary>
        /// 备注
        /// </summary>
        public string? Comment { get; set; }

    }




}
