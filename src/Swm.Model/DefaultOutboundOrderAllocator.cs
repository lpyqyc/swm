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
using NHibernate;
using NHibernate.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Swm.Model
{
    /// <summary>
    /// 出库单库存分配程序。
    /// </summary>
    public class DefaultOutboundOrderAllocator : IOutboundOrderAllocator
    {
        protected readonly ILogger _logger;

        protected readonly ISession _session;

        /// <summary>
        /// 初始化类 <see cref="Wms.OutboundOrderAllocator"/> 的一个实例
        /// </summary>
        public DefaultOutboundOrderAllocator(ISession session, ILogger logger)
        {
            _session = session;
            _logger = logger;
        }

        /// <summary>
        /// 为出库单分配库存。
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="outboundOrder">要分配库存的出库单</param>
        /// <param name="options">分配选项</param>
        public async Task AllocateAsync(OutboundOrder outboundOrder, OutboundOrderAllocationOptions options)
        {
            if (outboundOrder == null)
            {
                throw new ArgumentNullException(nameof(outboundOrder));
            }
            if (outboundOrder.Closed)
            {
                throw new InvalidOperationException("出库单已关闭");
            }
            if (options == null)
            {
                options = new OutboundOrderAllocationOptions();
            }
            options.Normalize();            

            _logger.Information("正在为出库单 {outboundOrderCode} 分配库存", outboundOrder.OutboundOrderCode);
            _logger.Information("区域：{areas}", options.Areas == null ? "" : string.Join(", ", options.Areas));
            //_logger.Information("跳过脱机的巷道：{skipOfflineLaneways}", options.SkipOfflineLaneways);
            //_logger.Information("排除的巷道：{excludeLanewasys}", string.Join(", ", options.ExcludeLaneways.Select(x => x.LanewayCode)));
            _logger.Information("包含的托盘：{includePallets}", string.Join(", ", options.IncludePallets));
            _logger.Information("排除的托盘：{excludePallets}", string.Join(", ", options.ExcludePallets));
            //_logger.Information("跳过禁止出站的货位：{skipLocationsOutboundDisabled}", options.SkipLocationsOutboundDisabled);

            foreach (var line in outboundOrder.Lines)
            {
                await ProcessLineAsync(line, options).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 处理单个出库行
        /// </summary>
        /// <param name="line">出库行</param>
        /// <param name="laneways">用于分配的货载所在的巷道</param>
        /// <param name="includeUnitloads">要在分配中包含的货载，这些货载优先参与分配。</param>
        /// <param name="excludeUnitloads">要在分配中排除的货载，这些货载不会参与分配，即使出现在 includeUnitloads 中，也不参与分配。</param>
        async Task ProcessLineAsync(OutboundLine line, OutboundOrderAllocationOptions options)
        {
            _logger.Information("正在为出库单明细 {outboundLine} 分配库存", line);

            {
                var shortage = line.ComputeShortage();
                _logger.Debug("出库单明细 {outboundLine} 的分配欠数是 {shortage}", line, shortage);
                if (shortage <= 0)
                {
                    _logger.Debug("不需要分配");
                    return;
                }
            }

            // 显式包含的货载项
            List<UnitloadItem> included = new List<UnitloadItem>();
            if (options.IncludePallets.Length > 0)
            {
                included = await _session.Query<UnitloadItem>()
                    .Where(x => options.IncludePallets.Contains(x.Unitload.PalletCode)
                        && x.Material == line.Material)
                    .ToListAsync()
                    .ConfigureAwait(false);
            }
            // 库内候选项
            var candidateItems = _session
                .Query<UnitloadItem>()
                .Where(x => 
                    included.Contains(x) == false   // 显式包含的货载项已在上面处理过，这里需排除
                    && x.Material == line.Material)
                .OrderBy(x => x.OutOrdering)
                .ThenBy(x => x.Unitload.CurrentLocation.Cell.oByShape)
                .ThenBy(x => x.Unitload.CurrentLocation.Cell.o1)
                .ThenBy(x => x.Unitload.CurrentLocation.Rack.Deep)
                .LoadInChunksAsync(options.ChunkSize);
            await foreach (var item in Union(included, candidateItems))
            {
                if (item.Quantity == 0)
                {
                    _logger.Warning("货载项 {unitloadItem} 的数量为 0", item);
                    continue;
                }

                var taken = await AllocateItemAsync(line, item, options);
                var shortage = line.ComputeShortage();
                _logger.Debug("出库单明细 {outboundLine} 的分配欠数是 {shortage}", line, shortage);
                if (shortage <= 0)
                {
                    _logger.Information("出库单明细 {outboundLine} 分配库存完成", line);
                    return;
                }
            }
            
            _logger.Information("出库单明细 {outboundLine} 分配库存完成", line);
            
            static async IAsyncEnumerable<UnitloadItem> Union(List<UnitloadItem> include, IAsyncEnumerable<UnitloadItem> candidateItems)
            {
                foreach (var item in include)
                {
                    yield return item;
                }

                await foreach (var item in candidateItems)
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// 为出库行从指定的货载项进行分配。此方法具有副作用，会更改货载的分配信息
        /// </summary>
        /// <param name="line">出库行</param>
        /// <param name="item">要从中分配的货载项</param>
        /// <returns>从货载项中分配的数量</returns>
        async Task<decimal> AllocateItemAsync(OutboundLine line, UnitloadItem item, OutboundOrderAllocationOptions options)
        {
            if (!TestUnitloadItem(line, item, options))
            {
                return 0m;
            }

            if (line.ComputeShortage() <= 0m)
            {
                return 0m;
            }

            // 货载项中的可用数（未分配数）
            var allocated = line.OutboundOrder.ComputeAllocated(item);
            var available = item.Quantity - allocated;
            _logger.Debug("货载项 {unitloadItem} 的库存数量是 {quantity}", item, item.Quantity);
            _logger.Debug("货载项 {unitloadItem} 的已分配数量是 {allocated}", item, allocated);
            _logger.Debug("货载项 {unitloadItem} 的可用数量是 {available}", item, available);
            if (available < 0)
            {
                throw new Exception("程序错误");
            }

            if (available == 0)
            {
                return 0m;
            }
            var taken = Math.Min(available, line.ComputeShortage());

            // 更新货载项的分配信息
            OutboundLineAllocation allocation = new OutboundLineAllocation();
            allocation.UnitloadItem = item;
            allocation.Quantity = taken;
            line.Allocations.Add(allocation);

            item.Unitload.SetCurrentUat(line.OutboundOrder, OutboundOrder.UatTypeDescription);
            await _session.UpdateAsync(item.Unitload).ConfigureAwait(false);

            _logger.Information("为出库单明细 {outboundLine} 从货载项 {unitloadItem} 中分配了 {quantity} {uom}", line, item, taken, item.Uom);

            return taken;
        }

        /// <summary>
        /// 测试货载项是否满足出库单明细的需求
        /// </summary>
        /// <remarks>
        /// 会动态调用使用 <see cref="TestUnitloadItemAttribute"/> 标记的方法。
        /// </remarks>
        /// <param name="line"></param>
        /// <param name="unitloadItem"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public bool TestUnitloadItem(OutboundLine line, UnitloadItem item, OutboundOrderAllocationOptions options)
        {
            bool passed = true;
            _logger.Debug("检查出库单明细 {outboundLine} 与货载项 {unitloadItem} 是否匹配", line, item);

            if (options.ExcludePallets!= null && options.ExcludePallets.Contains(item.Unitload.PalletCode) == false)
            {
                _logger.Debug("（√）未显式排除");
            }
            else
            {
                _logger.Debug("（×）已显式排除");
                passed = false;
            }

            if (line.Material == item.Material)
            {
                _logger.Debug("（√）物料匹配");
            }
            else
            {
                _logger.Debug("（×）物料不匹配");
                passed = false;
            }

            if (line.Batch == null || line.Batch == item.Batch)
            {
                _logger.Debug("（√）批号匹配");
            }
            else
            {
                _logger.Debug("（×）批号不匹配");
                passed = false;
            }
            
            if (line.StockStatus == item.StockStatus)
            {
                _logger.Debug("（√）库存状态匹配");
            }
            else
            {
                _logger.Debug("（×）库存状态不匹配");
                passed = false;
            }

            if (line.Uom == item.Uom)
            {
                _logger.Debug("（√）计量单位匹配");
            }
            else
            {
                _logger.Debug("（×）计量单位不匹配");
                passed = false;
            }

            if (item.Unitload.CurrentUat == null || item.Unitload.CurrentUat == line.OutboundOrder)
            {
                _logger.Debug("（√）未分配到其他对象");
            }
            else
            {
                _logger.Debug("（×）已分配到其他对象");
                passed = false;
            }
            
            if (item.Unitload.HasCountingError == false)
            {
                _logger.Debug("（√）无盘点错误");
            }
            else
            {
                _logger.Debug("（×）有盘点错误");
                passed = false;
            }

            // TODO 提取常数
            string[] taskTypes = new[] { "让路", "整理" };
            if (item.Unitload.BeingMoved == false || taskTypes.Contains(item.Unitload.GetCurrentTask().TaskType))
            {
                _logger.Debug("（√）无任务");
            }
            else
            {
                _logger.Debug("（×）有任务");
                passed = false;
            }

            if (item.Unitload.OpHintType == Cst.None && item.Unitload.OpHintInfo == Cst.None)
            {
                _logger.Debug("（√）无操作提示");
            }
            else
            {
                _logger.Debug("（×）有操作提示");
                passed = false;
            }

            if (InAreas(item.Unitload.CurrentLocation?.Rack?.Laneway?.Area, options.Areas) || options.IncludePallets.Contains(item.Unitload.PalletCode, StringComparer.OrdinalIgnoreCase))
            {
                _logger.Debug("（√）在指定区域、或显式包含");
            }
            else
            {
                _logger.Debug("（×）不在指定区域、且未显式包含");
                passed = false;
            }

            if (item.Unitload.CurrentLocation?.Rack?.Laneway?.Offline == false || options.SkipOfflineLaneways == false || options.IncludePallets.Contains(item.Unitload.PalletCode, StringComparer.OrdinalIgnoreCase))
            {
                _logger.Debug("（√）巷道未脱机、或允许从脱机巷道分配、或显式包含");
            }
            else
            {
                _logger.Debug("（×）巷道脱机");
                passed = false;
            }

            if (passed)
            {
                _logger.Debug("出库单明细 {outboundLine} 与货载项 {unitloadItem} 匹配", line, item);
            }
            else
            {
                _logger.Debug("出库单明细 {outboundLine} 与货载项 {unitloadItem} 不匹配", line, item);
            }
            return passed;

            static bool InAreas(string? area, string[] areas)
            {
                return areas == null 
                    || areas.Length == 0 
                    || areas.Where(x => x != null).Any(x => string.Equals(x, area, StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// 解除出库单在货架上的分配，货架外的分配使用 <see cref="DeallocateAsync(OutboundOrder, Unitload)"/> 方法单独处理。
        /// </summary>
        /// <param name="outboundOrder">出库单</param>
        public async Task DeallocateInRackAsync(OutboundOrder outboundOrder)
        {
            if (outboundOrder == null)
            {
                throw new ArgumentNullException(nameof(outboundOrder));
            }

            if (outboundOrder.Closed)
            {
                throw new InvalidOperationException("出库单已关闭");
            }

            _logger.Debug("正在取消分配出库单 {outboundOrder} 在货架上的托盘", outboundOrder);
            var unitloadsInRack = outboundOrder.Unitloads
                .Where(x => x.InRack() && x.BeingMoved == false)
                .ToArray();
            foreach (var u in unitloadsInRack)
            {
                await DeallocateAsync(outboundOrder, u).ConfigureAwait(false);
            }
            await _session.FlushAsync().ConfigureAwait(false);
            _logger.Information("已取消分配出库单 {outboundOrder} 在货架上的 {palletCount} 个托盘", outboundOrder, unitloadsInRack.Length);
        }

        /// <summary>
        /// 解除指定出库单在指定货载上分配信息。
        /// </summary>
        /// <param name="outboundOrder">出库单</param>
        /// <param name="unitload">货载</param>
        public async Task DeallocateAsync(OutboundOrder outboundOrder, Unitload unitload)
        {
            if (outboundOrder == null)
            {
                throw new ArgumentNullException(nameof(outboundOrder));
            }

            if (outboundOrder.Closed)
            {
                throw new InvalidOperationException("出库单已关闭。");
            }

            if (unitload == null)
            {
                throw new ArgumentNullException(nameof(unitload));
            }

            _logger.Debug("正在从出库单 {outboundOrder} 解除分配托盘 {palletCode}", outboundOrder, unitload.PalletCode);

            if (unitload.CurrentUat != outboundOrder)
            {
                string msg = string.Format("货载未分配给出库单");
                throw new InvalidOperationException(msg);
            }

            if (unitload.BeingMoved)
            {
                _logger.Warning("托盘 {palletCode} 有任务，应避免取消分配", unitload.PalletCode);
            }

            foreach (var line in outboundOrder.Lines)
            {
                foreach (var alloc in line.Allocations.Where(x => x.UnitloadItem.Unitload == unitload).ToArray())
                {
                    line.Allocations.Remove(alloc);
                }
            }

            unitload.ResetCurrentUat();

            await _session.UpdateAsync(outboundOrder).ConfigureAwait(false);
            await _session.UpdateAsync(unitload).ConfigureAwait(false);

            _logger.Debug("成功从出库单 {outboundOrder} 解除分配托盘 {palletCode}", outboundOrder, unitload.PalletCode);
        }
    }

}

