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

using Arctic.AppSeqs;
using Arctic.AspNetCore;
using Arctic.EventBus;
using Arctic.NHibernateExtensions;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using NHibernate.Linq;
using Serilog;
using Swm.Constants;
using Swm.Locations;
using Swm.Materials;
using Swm.OutboundOrders;
using Swm.Palletization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 提供出入库 api。
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
        readonly OutboundOrderPickHelper _outboundOrderPickHelper;

        /// <summary>
        /// 初始化新实例。
        /// </summary>
        /// <param name="session"></param>
        /// <param name="outboundOrderAllocator">出库单库存分配程序</param>
        /// <param name="appSeqService"></param>
        /// <param name="opHelper"></param>
        /// <param name="simpleEventBus"></param>
        /// <param name="logger"></param>
        public WioController(ISession session, IOutboundOrderAllocator outboundOrderAllocator,
            OutboundOrderPickHelper outboundOrderPickHelper,
            IAppSeqService appSeqService, OpHelper opHelper, SimpleEventBus simpleEventBus, ILogger logger)
        {
            _session = session;
            _outboundOrderAllocator = outboundOrderAllocator;
            _appSeqService = appSeqService;
            _opHelper = opHelper;
            _simpleEventBus = simpleEventBus;
            _outboundOrderPickHelper = outboundOrderPickHelper;
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
        public async Task<ListData<OutboundOrderInfo>> GetOutboundOrderList([FromQuery]OutboundOrderListArgs args)
        {
            var pagedList = await _session.Query<OutboundOrder>().SearchAsync(args, args.Sort, args.Current, args.PageSize);
            return this.ListData(pagedList, x => new OutboundOrderInfo
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
                    QuantityDemanded = i.QuantityDemanded,
                    QuantityFulfilled = i.QuantityFulfilled,
                    QuantityUnfulfilled = i.GetQuantityUnfulfilled(),
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
        [HttpGet("get-outbound-order-detail/{id}")]
        [DebugShowArgs]
        [AutoTransaction]
        [OperationType(OperationTypes.查看出库单)]
        public async Task<ApiData<OutboundOrderInfo>> GetOutboundOrderDetail(int id)
        {
            var outboundOrder = await _session.GetAsync<OutboundOrder>(id);
            return this.Success(new OutboundOrderInfo
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
                    QuantityDemanded = i.QuantityDemanded,
                    QuantityFulfilled = i.QuantityFulfilled,
                    QuantityUnfulfilled = i.GetQuantityUnfulfilled(),
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
        public async Task<ApiData<UnitloadInfo[]>> GetAllocatedUnitloads(int outboundOrderId)
        {
            var outboundOrder = await _session.GetAsync<OutboundOrder>(outboundOrderId);
            return this.Success(outboundOrder.Unitloads.Select(x => DtoConvert.ToUnitloadInfo(x)).ToArray());
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
                line.QuantityDemanded = lineInfo.QuantityDemanded;
                line.QuantityFulfilled = 0;
                line.Batch = lineInfo.Batch;
                line.StockStatus = lineInfo.StockStatus;
                line.Uom = lineInfo.Uom;

                outboundOrder.AddLine(line);
                _logger.Information("已添加出库单明细，物料 {materialCode}，批号 {batch}，需求数量 {quantity}", line.Material.MaterialCode, line.Batch, line.QuantityDemanded);
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
        public async Task<ApiData> UpdateOutboundOrder(int id, UpdateOutboundOrderArgs args)
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

            outboundOrder.BizOrder = args.BizOrder;
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
                            line.QuantityDemanded = lineInfo.QuantityDemanded;
                            if (line.QuantityDemanded < line.QuantityFulfilled)
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
                            line.QuantityDemanded = lineInfo.QuantityDemanded;
                            line.QuantityFulfilled = 0;
                            line.Batch = lineInfo.Batch;
                            line.StockStatus = lineInfo.StockStatus;
                            line.Uom = lineInfo.Uom;

                            outboundOrder.AddLine(line);
                            _logger.Information("已添加出库单明细，物料 {materialCode}，批号 {batch}，需求数量 {quantity}", line.Material.MaterialCode, line.Batch, line.QuantityDemanded);
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

            await _simpleEventBus.FireEventAsync(OutboundOrdersEventTypes.OutboundOrderClosed, outboundOrder);

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
        public async Task<ApiData> Allocate(int id, [FromBody] AllocateStockOptions options)
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
            await _opHelper.SaveOpAsync("{0}: {1}", outboundOrder.OutboundOrderCode, options);

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
        /// 出库单下架
        /// </summary>
        /// <param name="id">出库单 Id</param>
        /// <param name="ports">出口编码</param>
        /// <returns></returns>
        [AutoTransaction]
        [OperationType(OperationTypes.出库单下架)]
        [HttpPost("attach-to-ports/{id}")]
        public async Task<ApiData> AttachToPorts(int id, string[] ports)
        {
            OutboundOrder obo = _session.Get<OutboundOrder>(id);
            _logger.Information("正在将出库单附加到出口");
            _logger.Debug("出库单 Id 是 {outboundOrderId}", id);
            _logger.Debug("出口是 {ports}", ports);

            if (obo == null || obo.Closed)
            {
                string errmsg = string.Format("出库单不存在，或已关闭，#{0}。", id);
                throw new InvalidOperationException(errmsg);
            }

            _logger.Information("出库单单号 {outboundOrderCode}", obo.OutboundOrderCode);
            if (obo.Unitloads.Where(x => x.InRack()).Count() == 0)
            {
                _logger.Warning("出库单 {outboundOrderCode} 在货架上没有货载", obo.OutboundOrderCode);
                return this.Failure($"出库单 {obo.OutboundOrderCode} 在货架上没有货载");
            }

            if (ports == null)
            {
                ports = new string[0];
            }

            var arr = _session.Query<Port>().Where(x => ports.Contains(x.PortCode)).ToArray();
            var prev = _session.Query<Port>().Where(x => x.CurrentUat == obo).ToArray();

            // 移除页面上没有指定的
            var deleted = prev.Where(x => arr.Contains(x) == false);
            foreach (var port in deleted)
            {
                _logger.Debug("将 {outboundOrderCode} 从 {port} 移除", obo.OutboundOrderCode, port.PortCode);
                port.ResetCurrentUat();
            }

            // 添加页面上新增的
            var added = arr.Except(prev);
            foreach (var port in added)
            {
                port.SetCurrentUat(obo);
                _logger.Debug("将 {outboundOrderCode} 附加到 {port}", obo.OutboundOrderCode, port.PortCode);
            }

            string str = string.Join(", ", _session.Query<Port>().Where(x => x.CurrentUat == obo).Select(x => x.PortCode));
            await _opHelper.SaveOpAsync("{0}@{1}", obo.OutboundOrderCode, str);

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

        /// <summary>
        /// 获取可用库存数量
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet("get-available-quantity")]
        public async Task<ActionResult<ApiData<decimal>>> GetAvailableQuantity([FromQuery] GetAvailableQuantityArgs args)
        {
            string? material = args?.MaterialCode?.Trim();
            string? stockStatus = args?.StockStatus?.Trim();
            string? batch = args?.Batch?.Trim();
            string? outboundOrderCode = args?.OutboundOrderCode?.Trim();

            var q = _session.Query<UnitloadItem>()
                .Where(x => x.Unitload!.OpHintType == null
                    && x.Unitload.HasCountingError == false
                    && x.Unitload.BeingMoved == false
                );

            if (material != null)
            {
                q = q.Where(x => x.Material!.MaterialCode == material);
            }

            if (stockStatus != null)
            {
                q = q.Where(x => x.StockStatus == stockStatus);
            }

            if (batch != null)
            {
                q = q.Where(x => x.Batch == batch);
            }

            if (outboundOrderCode == null)
            {
                q = q.Where(x => x.Unitload!.CurrentUat == null);
            }
            else
            {
                OutboundOrder o = await _session.Query<OutboundOrder>().Where(x => x.OutboundOrderCode == outboundOrderCode).SingleOrDefaultAsync();
                if (o == null)
                {
                    q = q.Where(x => x.Unitload!.CurrentUat == null);
                }
                else
                {
                    q = q.Where(x => x.Unitload!.CurrentUat == null || x.Unitload.CurrentUat == o);
                }
            }

            var sum = q.Sum(x => (decimal?)x.Quantity) ?? 0m;

            string str = sum.ToString("0.###");
            return Content(str);
        }


        /// <summary>
        /// 从托盘中为出库单拣货
        /// </summary>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPost("pick/{palletCode}")]
        [OperationType(OperationTypes.拣货)]
        public async Task<ApiData> Pick(string palletCode, [FromBody] OutboundOrderPickInfo[] pickInfos)
        {
            if (palletCode == null)
            {
                throw new ArgumentNullException(nameof(palletCode));
            }
            Unitload? unitload = await _session.Query<Unitload>().SingleOrDefaultAsync(x => x.PalletCode == palletCode);
            var op = await _opHelper.SaveOpAsync("{0}: {1}", palletCode, pickInfos);
            foreach (var pickInfo in pickInfos)
            {
                var flow = await _outboundOrderPickHelper.PickAsync<DefaultStockKey>(unitload, pickInfo, op.OperationType);
            }
            return this.Success();
        }


    }

}

