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

        public UnitloadsController(PalletizationHelper palletizationHelper, OpHelper opHelper, ISession session, ILogger logger)
        {
            _logger = logger;
            _session = session;
            _opHelper = opHelper;
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
        public async Task<ListResult<UnitloadListItem>> Get([FromQuery]UnitloadListArgs args)
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
                    Comment = x.Comment
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
        public async Task<ActionResult<UnitloadDetail>> Get(int id)
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
        public async Task<ActionResult<UnitloadDetail>> Get(string palletCode)
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

        // TODO 

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

    }


}
