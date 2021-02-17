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
    /// 表示库龄报表的数据项。
    /// </summary>
    public class AgeReportItemInfo
    {
        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; } = default!;

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
        /// 计量单位
        /// </summary>
        public string Uom { get; set; } = default!;

        /// <summary>
        /// 7天以内的库存数量
        /// </summary>
        public decimal ZeroToSevenDays { get; set; }

        /// <summary>
        /// 7到30天的库存数量
        /// </summary>
        public decimal SevenToThirtyDays { get; set; }

        /// <summary>
        /// 30天到90天的库存数量
        /// </summary>
        public decimal ThirtyToNinetyDays { get; set; }

        /// <summary>
        /// 90天以上的库存数量
        /// </summary>
        public decimal MoreThanNinetyDays { get; set; }
    }

}
