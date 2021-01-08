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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Swm.Web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PortsController : ControllerBase
    {
        readonly ISession _session;
        readonly OpHelper _opHelper;
        readonly ILogger _logger;

        public PortsController(ISession session, OpHelper opHelper, ILogger logger)
        {
            _session = session;
            _opHelper = opHelper;
            _logger = logger;
        }

        /// <summary>
        /// 出口列表
        /// </summary>
        /// <param name="args">查询参数</param>
        /// <returns></returns>
        [HttpGet]
        [DebugShowArgs]
        [AutoTransaction]
        [OperationType(OperationTypes.出口列表)]
        public async Task<ListResult<PortListItem>> Get([FromQuery]PortListArgs args)
        {
            var pagedList = await _session.Query<Port>().SearchAsync(args, args.Sort, args.Current, args.PageSize);
            return new ListResult<PortListItem>
            {
                Success = true,
                Data = pagedList.List.Select(x => new PortListItem
                {
                    PortId = x.PortId,
                    PortCode = x.PortCode,
                    CurrentUat = x.CurrentUat?.ToString(),
                    KP1 = x.KP1.LocationCode,
                    KP2 = x.KP2?.LocationCode,
                    Laneways = x.Laneways.Select(x => x.LanewayCode).ToArray(),
                    CheckedAt = x.CheckedAt,
                    CheckMessage = x.CheckMessage,
                }),
                Total = pagedList.Total
            };
        }

        /// <summary>
        /// 出口选择列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("select-list")]
        [AutoTransaction]
        public async Task<List<PortSelectListItem>> GetSelectList()
        {
            var list = await _session.Query<Port>().ToListAsync();
            var items = list
                .Select(x => new PortSelectListItem
                {
                    PortId = x.PortId,
                    PortCode = x.PortCode,
                    CurrentUat = x.CurrentUat?.ToString(),
                })
                .ToList();
            return items;
        }
    }
}
