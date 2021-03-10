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

namespace Swm.Palletization
{
    /// <summary>
    /// 表示一个出库需求
    /// </summary>
    public interface IOutboundDemand
    {
        /// <summary>
        /// 获取需求数量
        /// </summary>
        decimal QuantityDemanded { get; }

        /// <summary>
        /// 获取或设置已分配数量
        /// </summary>
        decimal GetQuantityAllocated();

        /// <summary>
        /// 获取或设置已完成数量（产生出库流水）
        /// </summary>
        decimal QuantityFulfilled { get; set; }

    }

}