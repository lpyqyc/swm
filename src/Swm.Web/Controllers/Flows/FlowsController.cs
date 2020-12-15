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
using Serilog;
using Swm.Model;
using System.Linq;
using System.Threading.Tasks;

namespace Swm.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FlowsController : ControllerBase
    {
        readonly ILogger _logger;
        readonly ISession _session;

        public FlowsController(ISession session, ILogger logger)
        {
            _logger = logger;
            _session = session;
        }


        [AutoTransaction]
        [HttpPost]
        [Route("list")]
        public async Task<FlowList> ListAsync(FlowListArgs args)
        {
            var pagedList = await _session.Query<Flow>().SearchAsync(args, args.Sort, args.Current, args.PageSize);

            return new FlowList
            {
                Success = true,
                Message = "OK",
                Data = pagedList.List.Select(x => new FlowListItem
                {
                    FlowId = x.FlowId,
                    ctime = x.ctime,
                    MaterialCode = x.Material.MaterialCode,
                    Description = x.Material.Description,
                    Batch = x.Batch,
                    StockStatus = x.StockStatus,
                    BizType = x.BizType,
                    Direction = x.Direction,
                    PalletCode = x.ContainerCode,
                    OrderCode = x.OrderCode,
                    BizOrder = x.BizOrder,
                    OperationType = x.OpType,
                    Quantity = x.Quantity,
                    Uom = x.Uom,
                    cuser = x.cuser,
                    Comment = x.Comment,
                }),
                Total = pagedList.Total,
            };
        }



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
