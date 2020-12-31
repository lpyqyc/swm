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
using Serilog;
using Swm.Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 操作出库单。
    /// </summary>
    [ApiController]
    [Route("outbound-orders")]
    public class OutboundOrdersController : ControllerBase
    {
        readonly ISession _session;
        readonly ILogger _logger;
        readonly SimpleEventBus _eventBus;
        readonly OpHelper _opHelper;
        readonly IAppSeqService _appSeqService;

        /// <summary>
        /// 初始化新实例。
        /// </summary>
        /// <param name="session"></param>
        /// <param name="appSeqService"></param>
        /// <param name="eventBus"></param>
        /// <param name="opHelper"></param>
        /// <param name="logger"></param>
        public OutboundOrdersController(ISession session, IAppSeqService appSeqService, SimpleEventBus eventBus, OpHelper opHelper, ILogger logger)
        {
            _session = session;
            _appSeqService = appSeqService;
            _opHelper = opHelper;
            _logger = logger;
            _eventBus = eventBus;
        }

        /// <summary>
        /// 出库单列表
        /// </summary>
        /// <param name="args">查询参数</param>
        /// <returns></returns>
        [HttpGet]
        [DebugShowArgs]
        [AutoTransaction]
        [OperationType(OperationTypes.查看出库单)]
        public async Task<OutboundOrderList> ListAsync([FromQuery]OutboundOrderListArgs args)
        {
            var pagedList = await _session.Query<OutboundOrder>().SearchAsync(args, args.Sort, args.Current, args.PageSize);
            return new OutboundOrderList
            {
                Success = true,
                Message = "OK",
                Data = pagedList.List.Select(x => new OutboundOrderListItem
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
                }),
                Total = pagedList.Total
            };
        }


        /// <summary>
        /// 出库单详细
        /// </summary>
        /// <param name="id">出库单Id</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [DebugShowArgs]
        [AutoTransaction]
        [OperationType(OperationTypes.查看出库单)]
        public async Task<OutboundOrderListItem> DetailAsync(int id)
        {
            var outboundOrder = await _session.GetAsync<OutboundOrder>(id);
            return new OutboundOrderListItem
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
            };
        }


        /// <summary>
        /// 创建出库单
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost]
        [OperationType(OperationTypes.创建出库单)]
        [AutoTransaction]
        public async Task<ActionResult> CreateAsync(CreateOutboundOrderArgs args)
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
                    .WrappedSingleOrDefaultAsync();
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
            _logger.Information("已创建出库单 {outboundOrderCode}", outboundOrder.OutboundOrderCode);
            _ = await _opHelper.SaveOpAsync("{0}", outboundOrder.OutboundOrderCode);

            return CreatedAtAction(nameof(DetailAsync).Replace("Async", ""), new { id = outboundOrder.OutboundOrderId });
        }


        /// <summary>
        /// 编辑出库单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [OperationType(OperationTypes.编辑出库单)]
        [AutoTransaction]
        public async Task<ActionResult> EditAsync(int id, EditOutboundOrderArgs args)
        {
            OutboundOrder outboundOrder = _session.Get<OutboundOrder>(id);
            if (outboundOrder == null)
            {
                String errMsg = String.Format("出库单不存在。Id 是 {0}。", id);
                throw new InvalidOperationException(errMsg);
            }

            if (outboundOrder.Closed)
            {
                String errMsg = String.Format("出库单已关闭，不能编辑。单号：{0}。", outboundOrder.OutboundOrderCode);
                throw new InvalidOperationException(errMsg);
            }

            var movingDown = await _session.Query<Port>().WrappedAnyAsync(x => x.CurrentUat == outboundOrder);
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
                    case "added":
                        {
                            OutboundLine line = new OutboundLine();
                            var material = await _session.Query<Material>()
                                .Where(x => x.MaterialCode == lineInfo.MaterialCode)
                                .WrappedSingleOrDefaultAsync();
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
            _logger.Information("已更新出库单 {outboundOrderCode}", outboundOrder.OutboundOrderCode);
            _ = await _opHelper.SaveOpAsync("{0}", outboundOrder.OutboundOrderCode);

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

            return NoContent();
        }


        /// <summary>
        /// 删除出库单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpDelete]
        [Route("{id}")]
        [OperationType(OperationTypes.删除出库单)]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            OutboundOrder  outboundOrder = await _session.GetAsync<OutboundOrder>(id);
            if (outboundOrder.Lines.Any(x => x.Dirty))
            {
                throw new InvalidOperationException("出库单已发生过操作。");
            }

            await _session.DeleteAsync(outboundOrder);
            _logger.Information("已删除出库单 {outboundOrderCode}", outboundOrder.OutboundOrderCode);
            await _opHelper.SaveOpAsync(outboundOrder.OutboundOrderCode);

            return NoContent();
        }

    }

}

