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

using Arctic.AspNetCore;
using Arctic.NHibernateExtensions;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using NHibernate.Linq;
using Serilog;
using Swm.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Swm.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UnitloadsController : ControllerBase
    {
        readonly ILogger _logger;
        readonly ISession _session;
        readonly PalletizationHelper _palletizationHelper;
        readonly OpHelper _opHelper;
        readonly FlowHelper _flowHelper;

        public UnitloadsController(PalletizationHelper palletizationHelper, FlowHelper flowHelper, OpHelper opHelper, ISession session, ILogger logger)
        {
            _logger = logger;
            _session = session;
            _opHelper = opHelper;
            _flowHelper = flowHelper;
            _palletizationHelper = palletizationHelper;
        }


        //// TODO 移走
        //[AutoTx]
        //[HttpPost]
        //[Route("get-select-list-of-op-hint-types")]
        //public async Task<ActionResult> GetSelectListOfOpHintTypesAsync()
        //{
        //    var list = await _session.Query<AppCode>().GetAppCodesAsync(AppCodeTypes.OpHintType);

        //    var items = list.Select(x => new
        //    {
        //        OpHintType = x.AppCodeValue,
        //        x.Description,
        //        x.Scope,
        //        x.DisplayOrder,
        //    });

        //    return Ok(items);
        //}

        /// <summary>
        /// 货载列表
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet]
        [OperationType(OperationTypes.查看货载)]
        public async Task<ListResult<UnitloadListItem>> List([FromQuery] UnitloadListArgs args)
        {
            var pagedList = await _session.Query<Unitload>().SearchAsync(args, args.Sort, args.Current, args.PageSize);

            return new ListResult<UnitloadListItem>
            {
                Success = true,
                Data = pagedList.List.Select(x => new UnitloadListItem
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
                }),
                Total = pagedList.Total,
            };
        }

        /// <summary>
        /// 货载项列表，用于在变更状态页面展示货载项
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet("items")]
        [OperationType(OperationTypes.查看货载)]
        public async Task<ListResult<UnitloadItemListItem>> UnitloadItemList([FromQuery] UnitloadItemListArgs args)
        {
            var pagedList = await _session.Query<UnitloadItem>().SearchAsync(args, args.Sort, args.Current, args.PageSize);

            return new ListResult<UnitloadItemListItem>
            {
                Success = true,
                Data = pagedList.List.Select(x => new UnitloadItemListItem
                {
                    UnitloadItemId = x.UnitloadItemId,
                    PalletCode = x.Unitload.PalletCode,
                    LocationCode = x.Unitload.CurrentLocation.LocationCode,
                    LanewayCode = x.Unitload.CurrentLocation.Rack?.Laneway?.LanewayCode,
                    BeingMoved = x.Unitload.BeingMoved,
                    MaterialId = x.Material.MaterialId,
                    MaterialCode = x.Material.MaterialCode,
                    MaterialType = x.Material.MaterialType,
                    Description = x.Material.Description,
                    Specification = x.Material.Specification,
                    Batch = x.Batch,
                    StockStatus = x.StockStatus,
                    Quantity = x.Quantity,
                    Uom = x.Uom,
                    Allocated = (x.Unitload.CurrentUat != null),
                    CanChangeStockStatus = CanChangeStockStatus(x).ok,
                    ReasonWhyStockStatusCannotBeChanged = CanChangeStockStatus(x).reason,
                }),
                Total = pagedList.Total,
            };
        }

        /// <summary>
        /// 货载详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet("{id}")]
        [OperationType(OperationTypes.查看货载)]
        public async Task<ActionResult<UnitloadDetail>> Detail(int id)
        {
            var unitload = await _session.GetAsync<Unitload>(id);
            if (unitload == null)
            {
                return NotFound();
            }

            return ToUnitloadDetail(unitload);
        }

        /// <summary>
        /// 货载详情
        /// </summary>
        /// <param name="palletCode"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet("{palletCode}")]
        [OperationType(OperationTypes.查看货载)]
        public async Task<ActionResult<UnitloadDetail>> Detail(string palletCode)
        {
            var unitload = await _session.Query<Unitload>().Where(x => x.PalletCode == palletCode).SingleOrDefaultAsync();
            if (unitload == null)
            {
                return NotFound();
            }

            return ToUnitloadDetail(unitload);
        }

        private UnitloadDetail ToUnitloadDetail(Unitload unitload)
        {
            var task = unitload.GetCurrentTask();
            return new UnitloadDetail
            {
                UnitloadId = unitload.UnitloadId,
                PalletCode = unitload.PalletCode,
                ctime = unitload.ctime,
                LocationCode = unitload.CurrentLocation.LocationCode,
                LanewayCode = unitload.CurrentLocation?.Rack?.Laneway?.LanewayCode,
                BeingMoved = unitload.BeingMoved,
                Items = unitload.Items.Select(i => new UnitloadItemInfo
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
                Comment = unitload.Comment,
                CurrentTask = task == null ? null : new UnitloadDetail.CurrentTaskInfo
                {
                    TaskCode = task.TaskCode,
                    TaskType = task.TaskType,
                    StartLocationCode = task.Start.LocationCode,
                    EndLocationCode = task.End.LocationCode,
                },
                CurrentUat = unitload.CurrentUat?.ToString(),
                OpHintInfo = unitload.OpHintInfo,
                OpHintType = unitload.OpHintType,

            };
        }

        /// <summary>
        /// 无单据组盘
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPost]
        [Route("palletize-without-order")]
        [OperationType(OperationTypes.无单据组盘)]
        public async Task<IActionResult> PalletizeWithoutOrder(PalletizeWithoutOrderArgs args)
        {
            List<PalletizationItemInfo<DefaultStockKey>> items = new List<PalletizationItemInfo<DefaultStockKey>>();
            foreach (var item in args.Items)
            {
                Material material = await _session.Query<Material>().GetMaterialAsync(item.MaterialCode);
                if (material == null)
                {
                    throw new InvalidOperationException($"物料主数据不存在：【{item.MaterialCode}】");
                }
                DefaultStockKey stockKey = new DefaultStockKey(material, item.Batch, item.StockStatus, item.Uom);
                items.Add(new PalletizationItemInfo<DefaultStockKey> { StockKey = stockKey, Quantity = item.Quantity });
            }

            var op = await _opHelper.SaveOpAsync($"托盘号：{args.PalletCode}");

            // TODO 处理硬编码：无单据组盘
            await _palletizationHelper.PalletizeAsync(args.PalletCode,
                                                      items,
                                                      op.OperationType,
                                                      "无单据组盘" // TODO 这里有硬编码文本
                                                      );

            return this.Success();
        }

        /// <summary>
        /// 更改库存状态
        /// </summary>
        /// <param name="ids">半角逗号分隔的货载项Id，货载项列表使用 GET /unitloads/items 获取</param>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPost]
        [Route("items/{ids}/actions/change-stock-status")]
        [OperationType(OperationTypes.更改库存状态)]
        public async Task<IActionResult> ChangeStockStatus(string ids, ChangeStockStatusArgs args)
        {
            if (string.IsNullOrWhiteSpace(args.IssuingStockStatus))
            {
                throw new InvalidOperationException("未提供发出状态");
            }
            if (string.IsNullOrWhiteSpace(args.ReceivingStockStatus))
            {
                throw new InvalidOperationException("未提供接收状态");
            }

            if (args.IssuingStockStatus == args.ReceivingStockStatus)
            {
                throw new InvalidOperationException("发出状态和接收状态不能相同");
            }

            List<int> list = ids
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => int.Parse(x))
                .ToList();
            List<UnitloadItem> unitloadItems = await _session.Query<UnitloadItem>()
                .Where(x => list.Contains(x.UnitloadItemId))
                .ToListAsync();

            const string bizType = "库存状态变更";

            if (unitloadItems.Count == 0)
            {
                throw new InvalidOperationException("未选中任何货载项。");
            }

            foreach (UnitloadItem item in unitloadItems)
            {
                if (item.StockStatus != args.IssuingStockStatus)
                {
                    throw new InvalidOperationException("货载项的状态与发出状态不一致");
                }

                var (ok, reason) = CanChangeStockStatus(item);
                if (ok == false)
                {
                    throw new InvalidOperationException(reason);
                }
            }

            var op = await _opHelper.SaveOpAsync("{0}-->{1}", args.IssuingStockStatus, args.ReceivingStockStatus);

            foreach (var item in unitloadItems)
            {
                // TODO 扩展点：替换泛型参数 DefaultStockKey
                // 1 生成发货流水
                Flow flowOut = await _flowHelper.CreateAndSaveAsync(item.GetStockKey<DefaultStockKey>(),
                                                                    item.Quantity,
                                                                    FlowDirection.Outbound,
                                                                    bizType,
                                                                    op.OperationType,
                                                                    item.Unitload.PalletCode).ConfigureAwait(false);
                // 2 更改库存数据
                item.StockStatus = args.ReceivingStockStatus;

                // 3 生成收货流水
                Flow flowIn = await _flowHelper.CreateAndSaveAsync(item.GetStockKey<DefaultStockKey>(),
                                                                   item.Quantity,
                                                                   FlowDirection.Inbound,
                                                                   bizType,
                                                                   op.OperationType,
                                                                   item.Unitload.PalletCode).ConfigureAwait(false);
                
                await _session.UpdateAsync(item.Unitload).ConfigureAwait(false);
            }

            return this.Success();
        }

        internal static (bool ok, string reason) CanChangeStockStatus(UnitloadItem item)
        {
            List<string> list = new List<string>();

            if (item.Unitload.CurrentUat != null)
            {
                list.Add("已分配");
            }

            if (item.Unitload.BeingMoved)
            {
                list.Add("有任务");
            }

            if (item.Unitload.HasCountingError)
            {
                list.Add("有盘点错误");
            }

            if (item.Unitload.OpHintType.IsNotNone())
            {
                list.Add("有操作提示");
            }

            if (list.Count > 0)
            {
                return (false, string.Join(", ", list));
            }

            return (true, string.Empty);
        }
    }
}
