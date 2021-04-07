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

namespace Swm.Materials
{
    /// <summary>
    /// 库存状态信息。
    /// </summary>
    public class StockStatusInfo
    {
        /// <summary>
        /// 库存状态
        /// </summary>
        public virtual string StockStatus { get; set; } = default!;

        /// <summary>
        /// 展示名称
        /// </summary>
        public virtual string? DisplayName { get; set; }

        /// <summary>
        /// 展示次序
        /// </summary>
        public virtual int DisplayOrder { get; set; }


        /// <summary>
        /// 使用范围
        /// </summary>
        public virtual string? Scope { get; set; }

        /// <summary>
        /// 是否可见
        /// </summary>
        public bool Visible { get; set; } = true;

    }

}
