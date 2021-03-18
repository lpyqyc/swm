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

using Arctic.EventBus;
using NHibernate;
using NHibernate.Linq;
using Swm.Constants;
using System;
using System.Threading.Tasks;

namespace Swm.Materials
{
    public class FlowHelper
    {
        readonly IStockFactory _stockFactory;
        readonly IFifoProvider _fifoProvider;
        readonly ISession _session;
        readonly IFlowFactory _flowFactory;
        readonly SimpleEventBus _eventBus;

        public FlowHelper(ISession session,
            IFlowFactory flowFactory,
            IStockFactory stockFactory, 
            IFifoProvider fifoProvider,
            SimpleEventBus eventBus
            )
        {
            _stockFactory = stockFactory;
            _fifoProvider = fifoProvider;
            _session = session;
            _flowFactory = flowFactory;
            _eventBus = eventBus;
        }

        /// <summary>
        /// 将流水保存到数据库。
        /// </summary>
        /// <param name="flow">要保存的流水。</param>
        /// <param name="updateStock">指示是否更新库存数据。</param>
        /// <returns></returns>
        async Task SaveAsync<TStockKey>(Flow flow) where TStockKey : StockKeyBase
        {
            if (flow.FlowId != 0)
            {
                throw new InvalidOperationException("不能保存重复的流水。");
            }
            await _session.SaveAsync(flow);
            await _eventBus.FireEventAsync(MaterialsEventTypes.FlowSaved, flow);
        }

        /// <summary>
        /// 填充流水的字段。
        /// </summary>
        /// <param name="flow">要填充的流水。</param>
        /// <param name="quantity">数量</param>
        /// <param name="bizType">业务类型</param>
        /// <param name="dir">流水方向</param>
        /// <param name="txNo">事务号，一个事务有多条流水时，用事务号将这些流水串起来。目前仅用于库存转换类业务，非转换类业务使用 <see cref="Cst.None"/>，参考 <see cref="Flow.TxNo"/></param>
        /// <param name="opType">操作类型</param>
        /// <param name="orderCode">导致此流水产生的WMS单据号。</param>
        /// <param name="bizOrder">WMS单据号对应的业务单号，不需要时使用<see cref="Cst.None"/></param>
        /// <param name="palletCode">库存发生变动的托盘号</param>
        void Populate<TStockKey>(Flow flow,
                                 TStockKey stockKey,
                                 decimal quantity,
                                 FlowDirection dir,
                                 string bizType,
                                 string opType,
                                 string palletCode,
                                 string? orderCode = null,
                                 string? bizOrder = null,
                                 string? txNo = null)
            where TStockKey : StockKeyBase
        {
            if (flow == null)
            {
                throw new ArgumentNullException(nameof(flow));
            }

            if (stockKey == null)
            {
                throw new ArgumentNullException(nameof(stockKey));
            }

            if (bizType == null)
            {
                throw new ArgumentNullException(nameof(bizType));
            }

            flow.SetStockKey(stockKey);
            flow.Quantity = quantity;
            flow.Direction = dir;
            flow.BizType = bizType;
            flow.OpType = opType;
            flow.PalletCode = palletCode;
            flow.OrderCode = orderCode;
            flow.BizOrder = bizOrder;
            flow.TxNo = txNo;
        }

        /// <summary>
        /// 创建流水并保存到数据库。
        /// </summary>
        /// <param name="stockKey"></param>
        /// <param name="quantity">数量</param>
        /// <param name="dir">流水方向</param>
        /// <param name="bizType">业务类型</param>
        /// <param name="opType">操作类型</param>
        /// <param name="palletCode">库存发生变动的托盘号</param>
        /// <param name="orderCode">导致此流水产生的WMS单据号</param>
        /// <param name="bizOrder">WMS单据号对应的业务单号，不需要时使用<see cref="Cst.None"/></param>
        /// <param name="txNo">事务号，一个事务有多条流水时，用事务号将这些流水串起来。目前仅用于库存转换类业务，非转换类业务使用 <see cref="Cst.None"/>，参考 <see cref="Flow.TxNo"/></param>
        /// <param name="updateStock">是否更新库存表数据</param>
        /// <returns></returns>
        public async Task<Flow> CreateAndSaveAsync<TStockKey>(TStockKey stockKey,
                                                              decimal quantity,
                                                              FlowDirection dir,
                                                              string bizType,
                                                              string opType,
                                                              string palletCode,
                                                              string? orderCode = null,
                                                              string? bizOrder = null,
                                                              string? txNo = null
                                                              )
            where TStockKey : StockKeyBase
        {
            var flow = _flowFactory.CreateFlow();
            Populate(flow, stockKey, quantity, dir, bizType, opType, palletCode, orderCode, bizOrder, txNo);
            await SaveAsync<TStockKey>(flow).ConfigureAwait(false);
            return flow;
        }
    }

}
