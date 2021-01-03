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
    /// 选择列表的数据项
    /// </summary>
    public class PortSelectListItem
    {
        /// <summary>
        /// 出口Id
        /// </summary>
        public int PortId { get; init; }

        /// <summary>
        /// 出口编码
        /// </summary>
        public string PortCode { get; init; } = default!;
        
        // TODO 重命名
        /// <summary>
        /// 当前下架的单据
        /// </summary>
        public string? CurrentUat { get; init; }
    }

    
}
