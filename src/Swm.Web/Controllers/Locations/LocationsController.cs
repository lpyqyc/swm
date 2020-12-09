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

using Arctic.NHibernateExtensions;
using Arctic.NHibernateExtensions.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using Serilog;
using Swm.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Swm.Web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        readonly ISession _session;
        readonly OpHelper _opHelper;
        readonly ILogger _logger;

        public LocationsController(ISession session, OpHelper opHelper, ILogger logger)
        {
            _session = session;
            _opHelper = opHelper;
            _logger = logger;
        }

        /// <summary>
        /// 货位列表
        /// </summary>
        /// <param name="args">查询参数</param>
        /// <returns></returns>
        [HttpPost]
        [DebugShowArgs]
        [AutoTransaction]
        [OperationType(OperationTypes.货位列表)]
        [Route("s")]
        public async Task<LocationListOfS> ListOfSAsync(LocationListOfSArgs args)
        {
            var pagedList = await _session.Query<Location>().ToPagedListAsync(args);
            return new LocationListOfS
            {
                Success = true,
                Message = "OK",
                Data = pagedList.List.Select(x => new LocationListItemOfS
                {
                    LocationId = x.LocationId,
                    LocationCode = x.LocationCode,
                    LanewayId = x.Rack.Laneway.LanewayId,
                    LanewayCode = x.Rack.Laneway.LanewayCode,
                    WeightLimit = x.WeightLimit,
                    HeightLimit = x.HeightLimit,
                    InboundCount = x.InboundCount,
                    InboundDisabled = x.InboundDisabled,
                    InboundDisabledComment = x.InboundDisabledComment,
                    OutboundCount = x.OutboundCount,
                    OutboundDisabled = x.OutboundDisabled,
                    OutboundDisabledComment = x.OutboundDisabledComment,
                    StorageGroup = x.StorageGroup,
                    UnitloadCount = x.UnitloadCount,
                }),
                Total = pagedList.Total
            };
        }

        /// <summary>
        /// 关键点列表
        /// </summary>
        /// <param name="args">查询参数</param>
        /// <returns></returns>
        [HttpPost]
        [DebugShowArgs]
        [AutoTransaction]
        [OperationType(OperationTypes.关键点列表)]
        [Route("k")]
        public async Task<LocationListOfK> ListOfKAsync(LocationListOfKArgs args)
        {
            var pagedList = await _session.Query<Location>().ToPagedListAsync(args);
            return new LocationListOfK
            {
                Success = true,
                Message = "OK",
                Data = pagedList.List.Select(x => new LocationListItemOfK
                {
                    LocationId = x.LocationId,
                    LocationCode = x.LocationCode,
                    InboundCount = x.InboundCount,
                    InboundDisabled = x.InboundDisabled,
                    InboundDisabledComment = x.InboundDisabledComment,
                    OutboundCount = x.OutboundCount,
                    OutboundDisabled = x.OutboundDisabled,
                    OutboundDisabledComment = x.OutboundDisabledComment,
                    Tag = x.Tag,
                    RequestType = x.RequestType,
                    UnitloadCount = x.UnitloadCount,
                }),
                Total = pagedList.Total
            };
        }

        ///// <summary>
        ///// 关键点选择列表
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet]
        //[Route("k/select-list")]
        //[AutoTransaction]
        //public async Task<List<PortSelectListItem>> SelectListOfKAsync()
        //{
        //    var list = await _session.Query<Port>().WrappedToListAsync();
        //    var items = list
        //        .Select(x => new PortSelectListItem
        //        {
        //            PortId = x.PortId,
        //            PortCode = x.PortCode,
        //            CurrentUat = x.CurrentUat?.ToString(),
        //        })
        //        .ToList();
        //    return items;
        //}
    }
}
