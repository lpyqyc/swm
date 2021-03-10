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

using Swm.Palletization;
using System.Threading.Tasks;

namespace Swm.OutboundOrders
{
    /// <summary>
    /// 出库单库存分配程序。
    /// </summary>
    public interface IOutboundOrderAllocator
    {
        /// <summary>
        /// 为出库单分配库存。
        /// </summary>
        /// <param name="outboundOrder">要分配库存的出库单</param>
        /// <param name="options">分配选项</param>
        Task AllocateAsync(OutboundOrder outboundOrder, AllocateStockOptions options);


        /// <summary>
        /// 解除出库单在货架上的分配，货架外的分配使用 <see cref="DeallocateAsync(OutboundOrder, Unitload)"/> 方法单独处理。
        /// </summary>
        /// <param name="outboundOrder"></param>
        Task DeallocateInRackAsync(OutboundOrder outboundOrder);


        /// <summary>
        /// 从出库单取消特定货载的分配。
        /// </summary>
        /// <param name="outboundOrder"></param>
        /// <param name="unitload"></param>
        Task DeallocateAsync(OutboundOrder outboundOrder, Unitload unitload);
    }


}