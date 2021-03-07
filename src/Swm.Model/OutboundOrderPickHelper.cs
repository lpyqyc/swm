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

using NHibernate;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Swm.Model
{
    public class OutboundOrderPickHelper
    { 
        IOutboundOrderAllocator _outboundOrderAllocator;
        ILogger _logger;
        FlowHelper _flowHelper;
        ISession _session;

        public OutboundOrderPickHelper(IOutboundOrderAllocator outboundOrderAllocator, FlowHelper flowHelper, ISession session, ILogger logger)
        {
            _outboundOrderAllocator = outboundOrderAllocator;
            _flowHelper = flowHelper;
            _session = session;
            _logger = logger;
        }

        /// <summary>
        /// 从货载中拣货。
        /// </summary>
        /// <param name="unitload"></param>
        /// <param name="pickInfos">拣货信息。</param>
        /// <returns></returns>
        public async Task<List<Flow>> PickAsync<TStockKey>(Unitload unitload, OutboundOrderPickInfo[] pickInfos, Op op) where TStockKey : StockKeyBase
        {
            if (unitload == null)
            {
                throw new ArgumentNullException(nameof(unitload));
            }
            if (pickInfos == null)
            {
                throw new ArgumentNullException(nameof(pickInfos));
            }

            _logger.Debug("{palletCode}", unitload);
            _logger.Debug("{pickInfo}", pickInfos);

            if (pickInfos.Any(x => x.QuantityPicked < 0))
            {
                throw new ArgumentOutOfRangeException(nameof(pickInfos), "拣货数量不能小于 0。");
            }

            if (unitload.CurrentLocation.LocationType == LocationTypes.S)
            {
                string msg = string.Format("拣货失败。当货载在储位上时，不允许拣货。容器编码：{0}。", unitload.PalletCode);
                throw new InvalidOperationException(msg);
            }

            if ((unitload.CurrentUat as OutboundOrder) == null)
            {
                string msg = string.Format("拣货失败。货载未分配给出库单。容器编码：{0}。", unitload.PalletCode);
                throw new InvalidOperationException(msg);
            }

            OutboundOrder outboundOrder = (OutboundOrder)unitload.CurrentUat;
            if (outboundOrder.Closed)
            {
                string msg = string.Format("拣货失败。出库单已关闭。单号：{0}。", outboundOrder.OutboundOrderCode);
                throw new InvalidOperationException(msg);
            }

            UnitloadItemAllocation[] unitloadItemAllocations = unitload.Items
                .SelectMany(x => x.Allocations)
                .ToArray();

            // 检查拣货信息合法性：
            // 1，范围，每个拣货信息都在当前货载中有对应的分配信息，不能拣货涉及两个货载；
            // 2，数量，每个拣货信息的实际拣货数量都不能超过分配数量；
            foreach (var pickInfo in pickInfos)
            {
                var allocInfo = unitloadItemAllocations.SingleOrDefault(x => x.UnitloadItemAllocationId == pickInfo.AllocId);

                if (allocInfo == null)
                {
                    string msg = string.Format("未在货载中找到分配信息#{0}。", pickInfo.AllocId);
                    throw new InvalidOperationException(msg);
                }

                // 检查分配的一致性
                OutboundLine outboundLine = (OutboundLine)allocInfo.OutboundDemand;
                if (outboundOrder.Lines.Contains(outboundLine) == false)
                {
                    string msg = string.Format("库存项分配到的出库明细不在货载分配到的出库单中。分配信息#{0}。", pickInfo.AllocId);
                    throw new InvalidOperationException(msg);
                }

                // 实际拣货数量不允许超出分配数量
                if (allocInfo.QuantityAllocated < pickInfo.QuantityPicked)
                {
                    string msg = string.Format("实际拣货数量超出分配数量，分配信息#{0}。", pickInfo.AllocId);
                    throw new InvalidOperationException(msg);
                }
            }

            // 到这里检查结束

            // 清除货载上的分配信息
            await _outboundOrderAllocator.DeallocateAsync(outboundOrder, unitload).ConfigureAwait(false);

            // 若拣货为 0，则将货载清除分配信息后立即返回
            List<Flow> flows = new List<Flow>();
            if (pickInfos.All(x => x.QuantityPicked == 0))
            {
                return flows;
            }

            // 以下是拣货不为 0 的情况

            // 在这个循环中：
            // 1，更新出库行的已出数量
            // 2，更新库存项的数量
            // 3，生成流水
            foreach (var pickInfo in pickInfos)
            {
                _logger.Debug("{pickInfo}", pickInfo);
                var allocInfo = unitloadItemAllocations.Single(x => x.UnitloadItemAllocationId == pickInfo.AllocId);
                _logger.Debug("{allocInfo}", allocInfo);

                UnitloadItem item = allocInfo.UnitloadItem;
                OutboundLine outboundLine = (OutboundLine)allocInfo.OutboundDemand;

                if (pickInfo.QuantityPicked == 0)
                {
                    continue;
                }
                // 扣除拣货数量
                item.Quantity -= pickInfo.QuantityPicked;
                if (item.Quantity < 0)
                {
                    throw new Exception("程序错误。");
                }
                if (item.Quantity == 0)
                {
                    _logger.Information("拣货后 {item} 数量为 0，从 {unitload} 中移除", item, unitload);
                    unitload.RemoveItem(item);

                    if (unitload.Items.Count() == 0)
                    {
                        _logger.Information("拣货后 {unitload} 无库存，删除", unitload);
                        await _session.DeleteAsync(unitload).ConfigureAwait(false);
                    }
                }

                // 增加出库行的已出数量。
                if (outboundLine.QuantityFulfilled + pickInfo.QuantityPicked > outboundLine.QuantityDemanded)
                {
                    _logger.Warning("超过出库行的需求数量");
                    // TODO 根据实际情况检查是否超出分配数量
                    bool allowOverDelivery = false;
                    if (!allowOverDelivery)
                    {
                        throw new ApplicationException("不允许超发。");
                    }
                }

                outboundLine.QuantityFulfilled += pickInfo.QuantityPicked;
                _logger.Debug("{outboundLine} 已出数量增加到 {quantityFulfilled}。", outboundLine.OutboundLineId, outboundLine.QuantityFulfilled);
                // 生成流水记录
                Flow flow = await _flowHelper.CreateAndSaveAsync(
                    item.GetStockKey<TStockKey>(), 
                    pickInfo.QuantityPicked, 
                    FlowDirection.Outbound, 
                    outboundOrder.BizType, 
                    op.OperationType, 
                    unitload.PalletCode, 
                    outboundOrder.OutboundOrderCode, 
                    outboundOrder.BizOrder, 
                    updateStock: true)
                    .ConfigureAwait(false);

                flows.Add(flow);
            }

            // 并发处理：增加出库单版本号
            _session.Lock(outboundOrder, LockMode.Upgrade);

            return flows;

        }
    }
}