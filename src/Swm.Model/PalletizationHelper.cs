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

using NHibernate;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Swm.Model
{
    public class PalletizationHelper
    {
        // TODO 依赖太多
        readonly ILogger _logger;
        readonly IUnitloadFactory _unitloadFactory;
        readonly IUnitloadStorageInfoProvider _storageInfoProvider;
        readonly IOutOrderingProvider _outOrderingProvider;
        readonly IPalletCodeValidator _palletCodeValidator;
        readonly ISession _session;
        readonly FlowHelper _flowHelper;

        public PalletizationHelper(ISession session, 
            IUnitloadFactory unitloadFactory,            
            IUnitloadStorageInfoProvider storageInfoProvider, 
            IOutOrderingProvider outOrderingProvider, 
            IPalletCodeValidator palletCodeValidator,
            FlowHelper flowHelper,
            ILogger logger)
        {
            _logger = logger;
            _unitloadFactory = unitloadFactory;
            _storageInfoProvider = storageInfoProvider;
            _outOrderingProvider = outOrderingProvider;
            _palletCodeValidator = palletCodeValidator;
            _session = session;
            _flowHelper = flowHelper;
        }


        public async Task<Unitload> PalletizeAsync<TStockKey>(string palletCode,
                                                   IEnumerable<PalletizationItemInfo<TStockKey>> items,
                                                   string opType,
                                                   string bizType,
                                                   string orderCode = Cst.None,
                                                   string bizOrder = Cst.None,
                                                   bool updateStock = true)
            where TStockKey : StockKeyBase
        {
            if (palletCode == null)
            {
                throw new ArgumentNullException(nameof(palletCode));
            }

            if (bizType == null)
            {
                throw new ArgumentNullException(nameof(bizType));
            }
            if (opType == null)
            {
                throw new ArgumentNullException(nameof(opType));
            }

            _logger.Information($"正在组盘，托盘号：{palletCode}", palletCode);

            // 验证托盘编码是否已占用
            bool used = await _session.Query<Unitload>()
                .IsContainerCodeInUseAsync(palletCode)
                .ConfigureAwait(false);
            if (used)
            {
                throw new InvalidOperationException($"托盘号已被占用：【{palletCode}】");
            }
            // 验证托盘编码是否符合合法
            if (_palletCodeValidator.IsWellFormed(palletCode, out string err) == false)
            {
                throw new InvalidOperationException($"托盘号无效：{err}");
            }

            // 生成货载和流水
            Unitload unitload = _unitloadFactory.CreateUnitload();

            unitload.ContainerCode = palletCode;
            foreach (var item in items)
            {
                UnitloadItem unitloadItem = _unitloadFactory.CreateUnitloadItem();
                unitloadItem.SetStockKey(item.StockKey);
                unitloadItem.Quantity = item.Quantity;
                unitloadItem.OutOrdering = _outOrderingProvider.GetOutOrdering(item.StockKey);
                unitload.AddItem(unitloadItem);

                await _flowHelper
                    .CreateAndSaveAsync(item.StockKey, item.Quantity, FlowDirection.Inbound, bizType, opType, palletCode, orderCode, bizOrder, Cst.None, updateStock)
                    .ConfigureAwait(false);
            }

            var loc = await _session.Query<Location>().GetNAsync().ConfigureAwait(false);
            unitload.Enter(loc);

            unitload.StorageInfo = new UnitloadStorageInfo
            {
                StorageGroup = _storageInfoProvider.GetStorageGroup(unitload),
                OutFlag = _storageInfoProvider.GetOutFlag(unitload),
                ContainerSpecification = _storageInfoProvider.GetContainerSpecification(unitload)
            };

            // 将货载保存到数据库
            await _session.SaveAsync(unitload).ConfigureAwait(false);

            _logger.Information($"组盘成功");
            return unitload;
        }

        public async Task<Unitload> PalletizeAsync<TStockKey>(string palletCode,
                                                   TStockKey stockKey,
                                                   decimal quantity,
                                                   string opType,
                                                   string bizType,
                                                   string orderCode = Cst.None,
                                                   string bizOrder = Cst.None,
                                                   bool updateStock = true)
            where TStockKey : StockKeyBase
        {
            var item = new PalletizationItemInfo<TStockKey> { StockKey = stockKey, Quantity = quantity };
            var items = new[] { item };
            return await PalletizeAsync(palletCode, items, opType, bizType, orderCode, bizOrder, updateStock)
                .ConfigureAwait(false);
        }

    }

}
