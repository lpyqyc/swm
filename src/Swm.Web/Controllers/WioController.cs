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

using Arctic.AppSeqs;
using Arctic.AspNetCore;
using Arctic.EventBus;
using Arctic.NHibernateExtensions;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using NHibernate.Linq;
using Serilog;
using Swm.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 出入库。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WioController : ControllerBase
    {
        readonly ISession _session;
        readonly ILogger _logger;
        readonly OpHelper _opHelper;
        readonly IAppSeqService _appSeqService;
        readonly SimpleEventBus _simpleEventBus;
        readonly IOutboundOrderAllocator _outboundOrderAllocator;

        /// <summary>
        /// 初始化新实例。
        /// </summary>
        /// <param name="session"></param>
        /// <param name="outboundOrderAllocator">出库单库存分配程序</param>
        /// <param name="appSeqService"></param>
        /// <param name="opHelper"></param>
        /// <param name="simpleEventBus"></param>
        /// <param name="logger"></param>
        public WioController(ISession session, IOutboundOrderAllocator outboundOrderAllocator, IAppSeqService appSeqService, OpHelper opHelper, SimpleEventBus simpleEventBus, ILogger logger)
        {
            _session = session;
            _outboundOrderAllocator = outboundOrderAllocator;
            _appSeqService = appSeqService;
            _opHelper = opHelper;
            _simpleEventBus = simpleEventBus;
            _logger = logger;
        }

        /// <summary>
        /// 出库单列表
        /// </summary>
        /// <param name="args">查询参数</param>
        /// <returns></returns>
        [HttpGet("get-outbound-order-list")]
        [DebugShowArgs]
        [AutoTransaction]
        [OperationType(OperationTypes.查看出库单)]
        public async Task<ListData<OutboundOrderListItem>> GetOutboundOrderList([FromQuery]OutboundOrderListArgs args)
        {
            var pagedList = await _session.Query<OutboundOrder>().SearchAsync(args, args.Sort, args.Current, args.PageSize);
            return this.ListData(pagedList, x => new OutboundOrderListItem
            {
                OutboundOrderId = x.OutboundOrderId,
                OutboundOrderCode = x.OutboundOrderCode,
                ctime = x.ctime,
                cuser = x.cuser,
                mtime = x.mtime,
                muser = x.muser,
                BizType = x.BizType,
                BizOrder = x.BizOrder,
                Closed = x.Closed,
                ClosedAt = x.ClosedAt,
                ClosedBy = x.ClosedBy,
                Comment = x.Comment,
                Lines = x.Lines.Select(i => new OutboundLineInfo
                {
                    OutboundLineId = i.OutboundLineId,
                    MaterialId = i.Material.MaterialId,
                    MaterialCode = i.Material.MaterialCode,
                    MaterialType = i.Material.MaterialType,
                    Description = i.Material.Description,
                    Specification = i.Material.Specification,
                    Batch = i.Batch,
                    StockStatus = i.StockStatus,
                    Uom = i.Uom,
                    QuantityRequired = i.QuantityRequired,
                    QuantityDelivered = i.QuantityDelivered,
                    QuantityUndelivered = i.QuantityUndelivered,
                    Comment = i.Comment,
                }).ToList(),
                UnitloadCount = x.Unitloads.Count
            });
        }

        /// <summary>
        /// 出库单详细
        /// </summary>
        /// <param name="id">出库单Id</param>
        /// <returns></returns>
        [HttpGet("get-outbound-order-details/{id}")]
        [DebugShowArgs]
        [AutoTransaction]
        [OperationType(OperationTypes.查看出库单)]
        public async Task<ApiData<OutboundOrderListItem>> GetOutboundOrderDetails(int id)
        {
            var outboundOrder = await _session.GetAsync<OutboundOrder>(id);
            return this.Success(new OutboundOrderListItem
            {
                OutboundOrderId = outboundOrder.OutboundOrderId,
                OutboundOrderCode = outboundOrder.OutboundOrderCode,
                ctime = outboundOrder.ctime,
                cuser = outboundOrder.cuser,
                mtime = outboundOrder.mtime,
                muser = outboundOrder.muser,
                BizType = outboundOrder.BizType,
                BizOrder = outboundOrder.BizOrder,
                Closed = outboundOrder.Closed,
                ClosedAt = outboundOrder.ClosedAt,
                ClosedBy = outboundOrder.ClosedBy,
                Comment = outboundOrder.Comment,
                Lines = outboundOrder.Lines.Select(i => new OutboundLineInfo
                {
                    OutboundLineId = i.OutboundLineId,
                    MaterialId = i.Material.MaterialId,
                    MaterialCode = i.Material.MaterialCode,
                    MaterialType = i.Material.MaterialType,
                    Description = i.Material.Description,
                    Specification = i.Material.Specification,
                    Batch = i.Batch,
                    StockStatus = i.StockStatus,
                    Uom = i.Uom,
                    QuantityRequired = i.QuantityRequired,
                    QuantityDelivered = i.QuantityDelivered,
                    QuantityUndelivered = i.QuantityUndelivered,
                    Comment = i.Comment,
                }).ToList(),
                UnitloadCount = outboundOrder.Unitloads.Count,
            });
        }

        /// <summary>
        /// 获取分配给出库单的货载
        /// </summary>
        /// <param name="outboundOrderId"></param>
        /// <returns></returns>
        [HttpGet("get-allocated-unitloads/{outboundOrderId}")]
        [DebugShowArgs]
        [AutoTransaction]
        [OperationType(OperationTypes.查看出库单)]
        public async Task<ApiData<UnitloadListItem[]>> GetAllocatedUnitloads(int outboundOrderId)
        {
            var outboundOrder = await _session.GetAsync<OutboundOrder>(outboundOrderId);
            return this.Success(outboundOrder.Unitloads.Select(x => new UnitloadListItem
            {
                UnitloadId = x.UnitloadId,
                PalletCode = x.PalletCode,
                ctime = x.ctime,
                mtime = x.mtime,
                LocationCode = x.CurrentLocation.LocationCode,
                LanewayCode = x.CurrentLocation?.Rack?.Laneway?.LanewayCode,
                BeingMoved = x.BeingMoved,
                Items = x.Items.Select(i => new UnitloadItemInfo
                {
                    UnitloadItemId = i.UnitloadItemId,
                    MaterialId = i.Material.MaterialId,
                    MaterialCode = i.Material.MaterialCode,
                    MaterialType = i.Material.MaterialType,
                    Description = i.Material.Description,
                    Specification = i.Material.Specification,
                    Batch = i.Batch,
                    StockStatus = i.StockStatus,
                    Quantity = i.Quantity,
                    Uom = i.Uom,
                }).ToList(),
                Allocated = (x.CurrentUat != null),

                Comment = x.Comment
            }).ToArray());
        }


        /// <summary>
        /// 创建出库单
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost("create-outbound-order")]
        [OperationType(OperationTypes.创建出库单)]
        [AutoTransaction]
        public async Task<ApiData> CreateOutboundOrder(CreateOutboundOrderArgs args)
        {
            OutboundOrder outboundOrder = new OutboundOrder();

            string prefix = $"OBO{DateTime.Now:yyMMdd}";
            int next = await _appSeqService.GetNextAsync(prefix);
            outboundOrder.OutboundOrderCode = $"{prefix}{next:00000}";
            outboundOrder.BizType = args.BizType;
            outboundOrder.BizOrder = args.BizOrder;
            outboundOrder.Comment = args.Comment;

            if (args.Lines == null || args.Lines.Count == 0)
            {
                throw new InvalidOperationException("出库单应至少有一个出库行。");
            }

            foreach (var lineInfo in args.Lines)
            {
                OutboundLine line = new OutboundLine();
                var material = await _session.Query<Material>()
                    .Where(x => x.MaterialCode == lineInfo.MaterialCode)
                    .SingleOrDefaultAsync();
                if (material == null)
                {
                    throw new InvalidOperationException($"未找到编码为 {lineInfo.MaterialCode} 的物料。");
                }
                line.Material = material;
                line.QuantityRequired = lineInfo.QuantityRequired;
                line.QuantityDelivered = 0;
                line.Batch = lineInfo.Batch;
                line.StockStatus = lineInfo.StockStatus;
                line.Uom = lineInfo.Uom;

                outboundOrder.AddLine(line);
                _logger.Information("已添加出库单明细，物料 {materialCode}，批号 {batch}，需求数量 {quantity}", line.Material.MaterialCode, line.Batch, line.QuantityRequired);
            }

            await _session.SaveAsync(outboundOrder);
            _logger.Information("已创建出库单 {outboundOrder}", outboundOrder);
            _ = await _opHelper.SaveOpAsync(outboundOrder.OutboundOrderCode);

            return this.Success();
        }


        /// <summary>
        /// 编辑出库单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("update-outbound-order/{id}")]
        [OperationType(OperationTypes.编辑出库单)]
        [AutoTransaction]
        public async Task<ApiData> Edit(int id, EditOutboundOrderArgs args)
        {
            OutboundOrder outboundOrder = _session.Get<OutboundOrder>(id);
            if (outboundOrder == null)
            {
                String errMsg = String.Format("出库单不存在。", id);
                throw new InvalidOperationException(errMsg);
            }

            if (outboundOrder.Closed)
            {
                String errMsg = String.Format("出库单已关闭，不能编辑。单号：{0}。", outboundOrder.OutboundOrderCode);
                throw new InvalidOperationException(errMsg);
            }

            var movingDown = await _session.Query<Port>().AnyAsync(x => x.CurrentUat == outboundOrder);
            if (movingDown)
            {
                String errMsg = String.Format("出库单正在下架，不能编辑。单号：{0}。", outboundOrder.OutboundOrderCode);
                throw new InvalidOperationException(errMsg);
            }

            outboundOrder.Comment = args.Comment;

            if (args.Lines == null || args.Lines.Count == 0)
            {
                throw new InvalidOperationException("出库单应至少有一个出库行。");
            }

            foreach (var lineInfo in args.Lines)
            {
                switch (lineInfo.Op)
                {
                    case "delete":
                        {
                            var line = outboundOrder.Lines.Single(x => x.OutboundLineId == lineInfo.OutboundLineId);
                            if (line.Dirty)
                            {
                                string errMsg = String.Format("已发生过出库操作的明细不能删除。出库行#{0}。", line.OutboundLineId);
                                throw new InvalidOperationException(errMsg);
                            }
                            outboundOrder.RemoveLine(line);
                            _logger.Information("已删除出库单明细 {outboundLineId}", line.OutboundLineId);
                        }
                        break;
                    case "edit":
                        {
                            var line = outboundOrder.Lines.Single(x => x.OutboundLineId == lineInfo.OutboundLineId);
                            line.QuantityRequired = lineInfo.QuantityRequired;
                            if (line.QuantityRequired < line.QuantityDelivered)
                            {
                                _logger.Warning("出库单明细 {outboundLineId} 的需求数量修改后小于已出数量", line.OutboundLineId);
                            }
                        }
                        break;
                    case "add":
                        {
                            OutboundLine line = new OutboundLine();
                            var material = await _session.Query<Material>()
                                .Where(x => x.MaterialCode == lineInfo.MaterialCode)
                                .SingleOrDefaultAsync();
                            if (material == null)
                            {
                                throw new InvalidOperationException($"未找到物料。编码 {lineInfo.MaterialCode}。");
                            }
                            line.Material = material;
                            line.QuantityRequired = lineInfo.QuantityRequired;
                            line.QuantityDelivered = 0;
                            line.Batch = lineInfo.Batch;
                            line.StockStatus = lineInfo.StockStatus;
                            line.Uom = lineInfo.Uom;

                            outboundOrder.AddLine(line);
                            _logger.Information("已添加出库单明细，物料 {materialCode}，批号 {batch}，需求数量 {quantity}", line.Material.MaterialCode, line.Batch, line.QuantityRequired);
                        }
                        break;
                    default:
                        break;
                }
            }

            await _session.UpdateAsync(outboundOrder);
            _logger.Information("已更新出库单 {outboundOrder}", outboundOrder);
            _ = await _opHelper.SaveOpAsync("{0}", outboundOrder);

            // TODO 
            //// 取消库内分配
            //_deliveryOrderAllocator.Value.CancelUnitLoadsOnRack(m);
            //foreach (var line in m.Lines)
            //{
            //    if (line.ComputeNumberAllocated() > line.NumberRequired)
            //    {
            //        String errMsg = String.Format("取消库内分配后，分配数量仍然大于应出数量，请处理完此出库单下所有路上和库外的货载再编辑。出库行#{0}。", line.Id);
            //        throw new InvalidOperationException(errMsg);
            //    }
            //}

            return this.Success();
        }


        /// <summary>
        /// 删除出库单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPost("delete-outbound-order/{id}")]
        [OperationType(OperationTypes.删除出库单)]
        public async Task<ApiData> DeleteOutboundOrder(int id)
        {
            OutboundOrder  outboundOrder = await _session.GetAsync<OutboundOrder>(id);
            if (outboundOrder.Lines.Any(x => x.Dirty))
            {
                throw new InvalidOperationException("出库单已发生过操作。");
            }

            await _session.DeleteAsync(outboundOrder);
            _logger.Information("已删除出库单 {outboundOrder}", outboundOrder);
            await _opHelper.SaveOpAsync(outboundOrder.OutboundOrderCode);

            return this.Success();
        }

        /// <summary>
        /// 关闭出库单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPost("close-outbound-order/{id}")]
        [OperationType(OperationTypes.关闭出库单)]
        public async Task<ApiData> Close(int id)
        {
            OutboundOrder outboundOrder = await _session.GetAsync<OutboundOrder>(id);

            if (outboundOrder == null)
            {
                throw new InvalidOperationException("出库单不存在。");
            }

            if (outboundOrder.Closed)
            {
                throw new InvalidOperationException($"出库单已关闭。");
            }

            // 关闭前检查
            await CheckOnCloseAsync(outboundOrder);

            outboundOrder.Closed = true;
            outboundOrder.ClosedAt = DateTime.Now;
            _session.Update(outboundOrder);

            await _opHelper.SaveOpAsync(outboundOrder.OutboundOrderCode);

            //  取消分配，以免关单后有未释放的货载
            foreach (var u in outboundOrder.Unitloads.ToList())
            {
                await _outboundOrderAllocator.DeallocateAsync(outboundOrder, u);
            }
            await _session.UpdateAsync(outboundOrder);

            _logger.Information("已关闭出库单 {outboundOrder}", outboundOrder);

            await _simpleEventBus.FireEventAsync(EventTypes.OutboundOrderClosed, outboundOrder);

            return this.Success();
        }

        /// <summary>
        /// 为出库单分配库存
        /// </summary>
        /// <param name="id"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPost("allocate-stock/{id}")]
        [OperationType(OperationTypes.分配库存)]
        public async Task<ApiData> Allocate(int id, [FromBody] AllocatStockOptions options)
        {
            OutboundOrder? outboundOrder = await _session.GetAsync<OutboundOrder>(id);

            if (outboundOrder == null || outboundOrder.Closed)
            {
                throw new InvalidOperationException("出库单不存在或已关闭。");
            }
            if (options.ChunkSize > 50)
            {
                options.ChunkSize = 50;
            }
            await _outboundOrderAllocator.AllocateAsync(outboundOrder, options);

            return this.Success();
        }


        /// <summary>
        /// 为出库单取消库内分配
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPost("deallocate-stock-in-rack/{id}")]
        [OperationType(OperationTypes.分配库存)]
        public async Task<ApiData> DeallocateInRack(int id)
        {
            OutboundOrder outboundOrder = await _session.GetAsync<OutboundOrder>(id);

            if (outboundOrder == null || outboundOrder.Closed)
            {
                throw new InvalidOperationException("出库单不存在或已关闭。");
            }

            await _outboundOrderAllocator.DeallocateInRackAsync(outboundOrder);

            return this.Success();
        }

        /// <summary>
        /// 将托盘从出库单中取消分配
        /// </summary>
        /// <param name="id"></param>
        /// <param name="args">取消参数</param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPost("deallocate-stock/{id}")]
        [OperationType(OperationTypes.分配库存)]
        public async Task<ApiData> Deallocate(int id, OutboundOrderDeallocateArgs args)
        {
            if (args == null || args.PalletCodes == null || args.PalletCodes.Length == 0)
            {
                throw new InvalidOperationException("未指定要取消分配的托盘。");
            }
            OutboundOrder outboundOrder = await _session.GetAsync<OutboundOrder>(id);

            if (outboundOrder == null || outboundOrder.Closed)
            {
                throw new InvalidOperationException("出库单不存在或已关闭。");
            }
            List<Unitload> unitloads = await _session
                .Query<Unitload>()
                .Where(x => args.PalletCodes.Contains(x.PalletCode))
                .ToListAsync();
            foreach (var u in unitloads)
            {
                await _outboundOrderAllocator.DeallocateAsync(outboundOrder, u);
            }

            return this.Success();
        }



        /// <summary>
        /// 对出库单进行关闭前检查。
        /// </summary>
        /// <param name="outboundOrder"></param>
        private async Task CheckOnCloseAsync(OutboundOrder outboundOrder)
        {
            // 应始终检查出库单是否挂在出货口上
            var ports = await _session.Query<Port>().Where(x => x.CurrentUat == outboundOrder).Select(x => x.PortCode).ToListAsync();
            if (ports.Count() > 0)
            {
                string str = string.Join(", ", ports.Select(x => x));
                string msg = string.Format("出库单正在下架。在出口 {1}。", outboundOrder.OutboundOrderCode, str);
                throw new InvalidOperationException(msg);
            }

            // 出库单下有分配的货载时不允许关闭，否则，货载将无法释放。
            if (outboundOrder.Unitloads.Count > 0)
            {
                string msg = string.Format("出库单下有分配的库存。", outboundOrder.OutboundOrderCode);
                throw new InvalidOperationException(msg);
            }

            // 应始终检查是否有移动的货载
            if (outboundOrder.Unitloads.Any(x => x.BeingMoved))
            {
                string msg = string.Format("出库单下有任务。", outboundOrder.OutboundOrderCode);
                throw new InvalidOperationException(msg);
            }


            //// TODO 考虑是否在库外有托盘时禁止取消
            //if (outboundOrder.UnitloadsAllocated.Any(x => x.InRack() == false))
            //{
            //    string msg = string.Format("出库单下有任务。", outboundOrder.OutboundOrderCode);
            //    throw new InvalidOperationException(msg);
            //}
        }


    }

}

