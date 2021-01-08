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
using Arctic.AspNetCore;
using Arctic.NHibernateExtensions;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using NHibernate.Linq;
using Serilog;
using Swm.Model;
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
        public MaterialsController(ISession session, ILogger logger)
        {
            _logger = logger;
            _session = session;
        }

        /// <summary>
        /// 物料列表
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet]
        [OperationType(OperationTypes.物料列表)]
        public async Task<ListResult<MaterialListItem>> Get([FromQuery]MaterialListArgs args)
        {
            var pagedList = await _session.Query<Material>().SearchAsync(args, args.Sort, args.Current, args.PageSize);

            return new ListResult<MaterialListItem>
            {
                Success = true,
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
        public async Task<List<MaterialSelectListItem>> GetSelectList(MaterialSelectListArgs args)
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
                .ToListAsync();

            return items;
        }

        /// <summary>
        /// 获取物料类型的选择列表
        /// </summary>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet]
        [Route("material-type-select-list")]
        public async Task<List<MaterialTypeSelectListItem>> GetMaterialTypesSelectList()
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

        /// <summary>
        /// 获取库存状态的选择列表
        /// </summary>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPost]
        [Route("stock-status-select-list")]
        public async Task<List<StockStatusSelectListItem>> GetStockStatusSelectList()
        {
            var appCodes = await _session.Query<AppCode>().GetAppCodesAsync(AppCodeTypes.StockStatus);

            var list = appCodes.Select(x => new StockStatusSelectListItem
            {
                StockStatus = x.AppCodeValue,
                Description = x.Description,
                Scope = x.Scope,
                DisplayOrder = x.DisplayOrder,
            }).ToList();

            return list;
        }

    }

}
