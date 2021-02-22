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
    /// 批号选择列表的查询参数
    /// </summary>
    public class BatchOptionsArgs
    {
        /// <summary>
        /// 关键字
        /// </summary>
        public string? Keyword { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        public string? MaterialCode { get; set; }

        /// <summary>
        /// 库存状态
        /// </summary>
        public string? StockStatus { get; set; }

        /// <summary>
        /// 取多少条记录，默认为 10
        /// </summary>
        public int? Limit { get; set; } = 10;

    }

}
