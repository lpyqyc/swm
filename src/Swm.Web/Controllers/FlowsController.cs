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
using Serilog;
using Swm.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Swm.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlowsController : ControllerBase
    {
        readonly ILogger _logger;
        readonly ISession _session;

        public FlowsController(ISession session, ILogger logger)
        {
            _logger = logger;
            _session = session;
        }


        /// <summary>
        /// 获取流水列表
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet]
        public async Task<ListData<FlowListItem>> List([FromQuery] FlowListArgs args)
        {
            var pagedList = await _session.Query<Flow>().SearchAsync(args, args.Sort, args.Current, args.PageSize);

            return this.ListData(pagedList, x => new FlowListItem
            {
                FlowId = x.FlowId,
                ctime = x.ctime,
                MaterialCode = x.Material.MaterialCode,
                Description = x.Material.Description,
                Batch = x.Batch,
                StockStatus = x.StockStatus,
                BizType = x.BizType,
                Direction = x.Direction,
                PalletCode = x.PalletCode,
                OrderCode = x.OrderCode,
                BizOrder = x.BizOrder,
                OperationType = x.OpType,
                Quantity = x.Quantity,
                Uom = x.Uom,
                cuser = x.cuser,
                Comment = x.Comment,
            });            
        }


        /// <summary>
        /// 获取业务类型列表数据
        /// </summary>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet("get-biz-type-select-list")]
        public async Task<ApiData<List<BizTypeSelectListItem>>> BizTypeSelectList()
        {
            var list = await _session.Query<AppCode>().GetAppCodesAsync(AppCodeTypes.BizType);

            var items = list.Select(x => new BizTypeSelectListItem
            {
                BizType = x.AppCodeValue,
                Description = x.Description,
                Scope = x.Scope,
                DisplayOrder = x.DisplayOrder
            }).ToList();

            return this.Success2(items);
        }

    }
}
