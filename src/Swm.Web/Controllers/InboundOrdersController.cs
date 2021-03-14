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
using Swm.InboundOrders;
using Swm.Locations;
using Swm.Materials;
using Swm.Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 操作入库单。
    /// </summary>
    [ApiController]
    [Route("api/inbound-orders")]
    public class InboundOrdersController : ControllerBase
    {
        readonly ISession _session;
        readonly ILogger _logger;
        readonly OpHelper _opHelper;
        readonly IAppSeqService _appSeqService;
        readonly SimpleEventBus _simpleEventBus;

        /// <summary>
        /// 初始化新实例。
        /// </summary>
        /// <param name="session"></param>
        /// <param name="appSeqService"></param>
        /// <param name="opHelper"></param>
        /// <param name="simpleEventBus"></param>
        /// <param name="logger"></param>
        public InboundOrdersController(ISession session, IAppSeqService appSeqService, OpHelper opHelper, SimpleEventBus simpleEventBus, ILogger logger)
        {
            _session = session;
            _appSeqService = appSeqService;
            _opHelper = opHelper;
            _simpleEventBus = simpleEventBus;
            _logger = logger;
        }

        /// <summary>
        /// 入库单列表
        /// </summary>
        /// <param name="args">查询参数</param>
        /// <returns></returns>
        [HttpGet]
        [DebugShowArgs]
        [AutoTransaction]
        [OperationType(OperationTypes.查看入库单)]
        public async Task<ListData<InboundOrderListItem>> List([FromQuery] InboundOrderListArgs args)
        {
            var pagedList = await _session.Query<InboundOrder>().SearchAsync(args, args.Sort, args.Current, args.PageSize);
            return this.ListData(pagedList, x => new InboundOrderListItem
            {
                InboundOrderId = x.InboundOrderId,
                InboundOrderCode = x.InboundOrderCode,
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
                Lines = x.Lines.Select(i => new InboundLineInfo
                {
                    InboundLineId = i.InboundLineId,
                    MaterialId = i.Material.MaterialId,
                    MaterialCode = i.Material.MaterialCode,
                    MaterialType = i.Material.MaterialType,
                    Description = i.Material.Description,
                    Specification = i.Material.Specification,
                    Batch = i.Batch,
                    StockStatus = i.StockStatus,
                    Uom = i.Uom,
                    QuantityExpected = i.QuantityExpected,
                    QuantityReceived = i.QuantityReceived,
                    Comment = i.Comment,
                }).ToList(),
            });
        }


        /// <summary>
        /// 入库单详细
        /// </summary>
        /// <param name="id">入库单Id</param>
        /// <returns></returns>
        [HttpGet("get-detail/{id}")]
        [DebugShowArgs]
        [AutoTransaction]
        [OperationType(OperationTypes.查看入库单)]
        public async Task<InboundOrderListItem> Detail(int id)
        {
            var inboundOrder = await _session.GetAsync<InboundOrder>(id);
            return new InboundOrderListItem
            {
                InboundOrderId = inboundOrder.InboundOrderId,
                InboundOrderCode = inboundOrder.InboundOrderCode,
                ctime = inboundOrder.ctime,
                cuser = inboundOrder.cuser,
                mtime = inboundOrder.mtime,
                muser = inboundOrder.muser,
                BizType = inboundOrder.BizType,
                BizOrder = inboundOrder.BizOrder,
                Closed = inboundOrder.Closed,
                ClosedAt = inboundOrder.ClosedAt,
                ClosedBy = inboundOrder.ClosedBy,
                Comment = inboundOrder.Comment,
                Lines = inboundOrder.Lines.Select(i => new InboundLineInfo
                {
                    InboundLineId = i.InboundLineId,
                    MaterialId = i.Material.MaterialId,
                    MaterialCode = i.Material.MaterialCode,
                    MaterialType = i.Material.MaterialType,
                    Description = i.Material.Description,
                    Specification = i.Material.Specification,
                    Batch = i.Batch,
                    StockStatus = i.StockStatus,
                    Uom = i.Uom,
                    QuantityExpected = i.QuantityExpected,
                    QuantityReceived = i.QuantityReceived,
                    Comment = i.Comment,
                }).ToList(),          
            };
        }


        /// <summary>
        /// 创建入库单
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost]
        [OperationType(OperationTypes.创建入库单)]
        [AutoTransaction]
        public async Task<ApiData> Create(CreateInboundOrderArgs args)
        {
            string prefix = $"IBO{DateTime.Now:yyMMdd}";
            int next = await _appSeqService.GetNextAsync(prefix);
            var inboundOrderCode = $"{prefix}{next:00000}";
            InboundOrder inboundOrder = new InboundOrder(inboundOrderCode);
            inboundOrder.BizType = args.BizType;
            inboundOrder.BizOrder = args.BizOrder;
            inboundOrder.Comment = args.Comment;

            if (args.Lines == null || args.Lines.Count == 0)
            {
                throw new InvalidOperationException("入库单应至少有一个入库行。");
            }

            foreach (var lineInfo in args.Lines)
            {
                InboundLine line = new InboundLine();
                var material = await _session.Query<Material>()
                    .Where(x => x.MaterialCode == lineInfo.MaterialCode)
                    .SingleOrDefaultAsync();
                if (material == null)
                {
                    throw new InvalidOperationException($"未找到编码为 {lineInfo.MaterialCode} 的物料。");
                }
                line.Material = material;
                line.QuantityExpected = lineInfo.QuantityExpected;
                line.QuantityReceived = 0;
                line.Batch = lineInfo.Batch;
                line.StockStatus = lineInfo.StockStatus;
                line.Uom = lineInfo.Uom;

                inboundOrder.AddLine(line);
                _logger.Information("已添加入库单明细，物料 {materialCode}，批号 {batch}，应入数量 {quantity}", line.Material.MaterialCode, line.Batch, line.QuantityReceived);
            }

            await _session.SaveAsync(inboundOrder);
            _logger.Information("已创建入库单 {inboundOrder}", inboundOrder);
            _ = await _opHelper.SaveOpAsync(inboundOrder.InboundOrderCode);

            return this.Success();
        }


        /// <summary>
        /// 编辑入库单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("edit/{id}")]
        [OperationType(OperationTypes.编辑入库单)]
        [AutoTransaction]
        public async Task<ApiData> Edit(int id, EditInboundOrderArgs args)
        {
            InboundOrder inboundOrder = _session.Get<InboundOrder>(id);
            if (inboundOrder == null)
            {
                String errMsg = String.Format("入库单不存在。Id 是 {0}。", id);
                throw new InvalidOperationException(errMsg);
            }

            if (inboundOrder.Closed)
            {
                String errMsg = String.Format("入库单已关闭，不能编辑。单号：{0}。", inboundOrder.InboundOrderCode);
                throw new InvalidOperationException(errMsg);
            }

            var movingDown = await _session.Query<Port>().AnyAsync(x => x.CurrentUat == inboundOrder);
            if (movingDown)
            {
                String errMsg = String.Format("入库单正在下架，不能编辑。单号：{0}。", inboundOrder.InboundOrderCode);
                throw new InvalidOperationException(errMsg);
            }

            inboundOrder.Comment = args.Comment;

            if (args.Lines == null || args.Lines.Count == 0)
            {
                throw new InvalidOperationException("入库单应至少有一个入库行。");
            }

            foreach (var lineInfo in args.Lines)
            {
                switch (lineInfo.Op)
                {
                    case "delete":
                        {
                            var line = inboundOrder.Lines.Single(x => x.InboundLineId == lineInfo.InboundLineId);
                            if (line.Dirty)
                            {
                                string errMsg = String.Format("已发生过入库操作的明细不能删除。入库行#{0}。", line.InboundLineId);
                                throw new InvalidOperationException(errMsg);
                            }
                            inboundOrder.RemoveLine(line);
                            _logger.Information("已删除入库单明细 {inboundLineId}", line.InboundLineId);
                        }
                        break;
                    case "edit":
                        {
                            var line = inboundOrder.Lines.Single(x => x.InboundLineId == lineInfo.InboundLineId);
                            line.QuantityExpected = lineInfo.QuantityExpected;
                            if (line.QuantityReceived < line.QuantityReceived)
                            {
                                _logger.Warning("入库单明细 {inboundLineId} 的应入数量修改后小于已入数量", line.InboundLineId);
                            }
                        }
                        break;
                    case "added":
                        {
                            InboundLine line = new InboundLine();
                            var material = await _session.Query<Material>()
                                .Where(x => x.MaterialCode == lineInfo.MaterialCode)
                                .SingleOrDefaultAsync();
                            if (material == null)
                            {
                                throw new InvalidOperationException($"未找到物料。编码 {lineInfo.MaterialCode}。");
                            }
                            line.Material = material;
                            line.QuantityExpected = lineInfo.QuantityExpected;
                            line.QuantityReceived = 0;
                            line.Batch = lineInfo.Batch;
                            line.StockStatus = lineInfo.StockStatus;
                            line.Uom = lineInfo.Uom;

                            inboundOrder.AddLine(line);
                            _logger.Information("已添加入库单明细，物料 {materialCode}，批号 {batch}，应入数量 {quantity}", line.Material.MaterialCode, line.Batch, line.QuantityReceived);
                        }
                        break;
                    default:
                        break;
                }
            }

            await _session.UpdateAsync(inboundOrder);
            _logger.Information("已更新入库单 {inboundOrder}", inboundOrder);
            _ = await _opHelper.SaveOpAsync("{0}", inboundOrder);

            return this.Success();
        }


        /// <summary>
        /// 删除入库单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPost("delete/{id}")]
        [OperationType(OperationTypes.删除入库单)]
        public async Task<ApiData> Delete(int id)
        {
            InboundOrder  inboundOrder = await _session.GetAsync<InboundOrder>(id);
            if (inboundOrder.Lines.Any(x => x.Dirty))
            {
                throw new InvalidOperationException("入库单已发生过操作。");
            }

            await _session.DeleteAsync(inboundOrder);
            _logger.Information("已删除入库单 {inboundOrder}", inboundOrder);
            await _opHelper.SaveOpAsync(inboundOrder.InboundOrderCode);

            return this.Success();
        }

        /// <summary>
        /// 关闭入库单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPost("close/{id}")]
        [OperationType(OperationTypes.关闭入库单)]
        public async Task<ApiData> Close(int id)
        {
            InboundOrder inboundOrder = await _session.GetAsync<InboundOrder>(id);

            if (inboundOrder == null)
            {
                throw new Exception("入库单不存在。");
            }

            if (inboundOrder.Closed)
            {
                throw new InvalidOperationException($"入库单已关闭。{inboundOrder.InboundOrderCode}");
            }

            inboundOrder.Closed = true;
            inboundOrder.ClosedAt = DateTime.Now;
            _session.Update(inboundOrder);

            await _opHelper.SaveOpAsync(inboundOrder.InboundOrderCode);

            await _session.UpdateAsync(inboundOrder);

            _logger.Information("已关闭入库单 {inboundOrder}", inboundOrder);

            await _simpleEventBus.FireEventAsync(InboundOrdersEventTypes.InboundOrderClosed, inboundOrder);

            return this.Success();
        }


    }

}

