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

using Arctic.NHibernateExtensions;
using Swm.Model;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 流水列表的查询参数
    /// </summary>
    public class FlowListArgs
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        [SourceProperty(nameof(Flow.ctime))]
        [SearchArg(SearchMode.GreaterThanOrEqual)]
        public DateTime? TimeFrom { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        [SourceProperty(nameof(Flow.ctime))]
        [SearchArg(SearchMode.LessThan)]
        public DateTime? TimeTo { get; set; }

        /// <summary>
        /// 物料类型
        /// </summary>
        [SourceProperty("Material.MaterialType")]
        [SearchArg(SearchMode.In)]
        public string[]? MaterialTypes { get; set; }

        /// <summary>
        /// 物料代码，不支持模糊查询
        /// </summary>
        [SourceProperty("Material.MaterialCode")]
        [SearchArg]
        public string? MaterialCode { get; set; }

        /// <summary>
        /// 托盘号，不支持模糊查询
        /// </summary>
        [SearchArg]
        public string? PalletCode { get; set; }

        /// <summary>
        /// 批号，不支持模糊查询
        /// </summary>
        [SearchArg]
        public string? Batch { get; set; }

        /// <summary>
        /// 库存状态
        /// </summary>
        [SearchArg(SearchMode.In)]
        public string[]? StockStatus { get; set; }

        /// <summary>
        /// 单号，不支持模糊查询
        /// </summary>
        [SearchArg(SearchMode.Equal)]
        public string? OrderCode { get; set; }

        /// <summary>
        /// 业务类型
        /// </summary>
        [SourceProperty(nameof(Flow.BizType))]
        [SearchArg(SearchMode.In)]
        public string[]? BizTypes { get; set; }

        /// <summary>
        /// 排序字段
        /// </summary>
        public string? Sort { get; set; }

        /// <summary>
        /// 基于 1 的当前页面，默认值为 1。
        /// </summary>
        public int? Current { get; set; } = 1;

        /// <summary>
        /// 每页大小，默认值为 10。
        /// </summary>
        public int? PageSize { get; set; }
    }
}