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

using Swm.Model;
using System.Collections.Specialized;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 列表查询参数
    /// </summary>
    public class MaterialListArgs : IListArgs<Material>
    {
        /// <summary>
        /// 物料代码
        /// </summary>
        [ListFilter(ListFilterOperator.Like)]
        public string? MaterialCode { get; set; }

        /// <summary>
        /// 物料描述
        /// </summary>
        [ListFilter(ListFilterOperator.Like)]
        public string? Description { get; set; }

        /// <summary>
        /// 物料类型
        /// </summary>
        [ListFilter]
        public string? MaterialType { get; set; }

        /// <summary>
        /// 排序字段
        /// </summary>
        public OrderedDictionary? Sort { get; set; }

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
