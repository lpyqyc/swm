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

using Swm.Constants;
using Swm.Model;
using System.ComponentModel.DataAnnotations;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 创建入库单或编辑入库单操作中的入库行信息。
    /// </summary>
    public class EditInboundLineInfo
    {
        /// <summary>
        /// 本行的操作：
        /// add 表示新增，
        /// edit 表示编辑，
        /// delete 表示删除。
        /// </summary>
        [Required]
        public string Op { get; set; } = default!;


        /// <summary>
        /// 入库单明细Id，用户在界面上新增的明细Id为0。
        /// </summary>
        public int InboundLineId { get; set; }


        /// <summary>
        /// 物料代码
        /// </summary>
        [Required]
        public string MaterialCode { get; set; }


        /// <summary>
        /// 要入库的批号，不可以为空
        /// </summary>
        [Required]
        public string Batch { get; set; }


        /// <summary>
        /// 要入库的库存状态。
        /// </summary>
        [Required]
        public string StockStatus { get; set; }

        /// <summary>
        /// 计量单位。
        /// </summary>
        [Required]
        public string Uom { get; set; }

        /// <summary>
        /// 应入数量
        /// </summary>
        [Range(0, int.MaxValue)]
        public decimal QuantityExpected { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Comment { get; set; }
    }

}

