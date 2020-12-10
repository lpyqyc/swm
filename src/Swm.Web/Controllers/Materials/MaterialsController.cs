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

using Arctic.AppCodes;
using Arctic.NHibernateExtensions;
using Arctic.NHibernateExtensions.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
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
    public class MaterialsController : ControllerBase
    {
        readonly ILogger _logger;
        readonly ISession _session;
        readonly PalletizationHelper _palletizationHelper;

        public MaterialsController(PalletizationHelper palletizationHelper, ISession session, ILogger logger)
        {
            _logger = logger;
            _session = session;
            _palletizationHelper = palletizationHelper;
        }

        /// <summary>
        /// 物料列表
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPost]
        [Route("list")]
        [OperationType(OperationTypes.物料列表)]
        public async Task<MaterialList> ListAsync(MaterialListArgs args)
        {
            var pagedList = await _session.Query<Material>().ToPagedListAsync(args);

            return new MaterialList
            {
                Success = true,
                Message = "OK",
                Data = pagedList.List.Select(x => new MaterialListItem
                {
                    MaterialId = x.MaterialId,
                    MaterialCode = x.MaterialCode,
                    MaterialType = x.MaterialType,
                    Description = x.Description,
                    Specification = x.Specification,
                    BatchEnabled = x.BatchEnabled,
                    MaterialGroup = x.MaterialGroup,
                    ValidDays = x.ValidDays,
                    StandingTime = x.StandingTime,
                    AbcClass = x.AbcClass,
                    Uom = x.Uom,
                    LowerBound = x.LowerBound,
                    UpperBound = x.UpperBound,
                    DefaultQuantity = x.DefaultQuantity,
                    DefaultStorageGroup = x.DefaultStorageGroup,
                    Comment = x.Comment
                }),
                Total = pagedList.Total
            };

        }

        /// <summary>
        /// 物料选择列表
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPost]
        [Route("select-list")]
        public async Task<List<MaterialSelectListItem>> SelectListAsync(MateriaSelectListArgs args)
        {
            var items = await _session.Query<Material>()
                .FilterByKeyword(args.Keyword, args.MaterialType)
                .Select(x => new MaterialSelectListItem
                {
                    MaterialId = x.MaterialId,
                    MaterialCode = x.MaterialCode,
                    Description = x.Description,
                    MaterialType = x.MaterialType,
                })
                .Take(args.Limit ?? 10)
                .WrappedToListAsync();

            return items;
        }

        /// <summary>
        /// 获取物料类型
        /// </summary>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet]
        [Route("material-types")]
        public async Task<List<MaterialTypeSelectListItem>> MaterialTypesAsync()
        {
            var appCodes = await _session
                .Query<AppCode>()
                .GetAppCodesAsync(AppCodeTypes.MaterialType);

            var list = appCodes
                .Select(x => new MaterialTypeSelectListItem
                {
                    MaterialType = x.AppCodeValue,
                    Description = x.Description,
                    Scope = x.Scope,
                    DisplayOrder = x.DisplayOrder,
                }).ToList();

            return list;
        }

        //[AutoTx]
        //[HttpPost]
        //[Route("get-select-list-of-stock-status")]
        //public async Task<ActionResult> GetSelectListOfStockStatusAsync()
        //{
        //    var list = await _session.Query<AppCode>().GetAppCodesAsync(AppCodeTypes.StockStatus);

        //    var items = list.Select(x => new
        //    {
        //        StockStatus = x.AppCodeValue,
        //        x.Description,
        //        x.Scope,
        //        x.DisplayOrder,
        //    });

        //    return Ok(items);
        //}

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

        //[AutoTx]
        //[HttpPost]
        //[Route("unitload-list")]
        //public async Task<ActionResult> UnitloadListAsync(UnitloadListArgs args)
        //{
        //    var (list, totalItemCount) = await _session.Query<Unitload>().ListAsync(args);

        //    var items = list.Select(x => new
        //    {
        //        x.UnitloadId,
        //        x.ContainerCode,
        //        x.ctime,
        //        x.CurrentLocation.LocationId,
        //        x.CurrentLocation.LocationCode,
        //        x.CurrentLocation?.Rack?.Laneway?.LanewayId,
        //        x.CurrentLocation?.Rack?.Laneway?.LanewayCode,
        //        x.StorageInfo,
        //        unitloadItems = x.Items.Select(i => new
        //        {
        //            i.UnitloadItemId,
        //            i.Material.MaterialId,
        //            i.Material.MaterialCode,
        //            i.Material.MaterialType,
        //            i.Material.Description,
        //            i.Material.Specification,
        //            i.Batch,
        //            i.StockStatus,
        //            i.Quantity,
        //            i.Uom,
        //        }),
        //        x.Comment
        //    });

        //    return this.Success(items, totalItemCount);
        //}

        //[AutoTx]
        //[HttpPost]
        //[Route("palletize-without-order")]
        //[OperationType(OperationTypes.无单据组盘)]
        //public async Task<ActionResult> PalletizeWithoutOrderAsync(PalletizeWithoutOrderArgs args)
        //{
        //    AutoTxAttribute.BuildResultOnError(HttpContext, ex =>
        //    {
        //        string errMsg = "组盘时出错。" + ex.Message;
        //        return this.Error(errMsg);
        //    });

        //    List<PalletizationItemInfo<StockKey>> items = new List<PalletizationItemInfo<StockKey>>();
        //    foreach (var item in args.Items)
        //    {
        //        Material material = await _session.Query<Material>().GetMaterialAsync(item.MaterialCode);
        //        if (material == null)
        //        {
        //            throw new InvalidOperationException($"物料主数据不存在：【{item.MaterialCode}】");
        //        }
        //        StockKey stockKey = new StockKey(material, item.Batch, item.StockStatus, item.Uom);

        //        items.Add(new PalletizationItemInfo<StockKey> { StockKey = stockKey, Quantity = item.Quantity });
        //    }

        //    var op = await HttpContext.SaveOpAsync($"托盘号：{args.ContainerCode}。");

        //    // TODO 处理硬编码：无单据组盘
        //    await _palletizationHelper.PalletizeAsync(args.ContainerCode,
        //                                              items,
        //                                              op.OpType,
        //                                              "无单据组盘");

        //    return this.Success("组盘成功。");
        //}


        //[AutoTx]
        //[HttpPost]
        //[Route("flow-list")]
        //public async Task<ActionResult> GetListOfFlowsAsync(GetListOfFlowsArgs args)
        //{
        //    var (list, totalItemCount) = await _session.Query<Flow>().ListAsync(args);

        //    var items = list.Select(x => new
        //    {
        //        x.FlowId,
        //        x.ctime,
        //        x.Material.MaterialCode,
        //        x.Material.Description,
        //        x.Batch,
        //        x.StockStatus,
        //        x.BizType,
        //        x.Direction,
        //        x.ContainerCode,
        //        x.OrderCode,
        //        x.BizOrder,
        //        x.OpType,
        //        x.Quantity,
        //        x.Uom,
        //        x.cuser,
        //        x.Comment
        //    });

        //    return this.Success(items, totalItemCount);
        //}



        //[AutoTx]
        //[HttpPost]
        //[Route("get-select-list-of-biz-types")]
        //public async Task<ActionResult> GetSelectListOfBizTypesAsync()
        //{
        //    var list = await _session.Query<AppCode>().GetAppCodesAsync(AppCodeTypes.BizType);

        //    var items = list.Select(x => new
        //    {
        //        BizType = x.AppCodeValue,
        //        x.Description,
        //        x.Scope,
        //        x.DisplayOrder,
        //    });

        //    return Ok(items);
        //}
    }




}
