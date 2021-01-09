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
using Arctic.EventBus;
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


    [Route("[controller]")]
    [ApiController]
    public class LanewaysController : ControllerBase
    {
        readonly ISession _session;
        readonly OpHelper _opHelper;
        readonly LocationHelper _locHelper;
        readonly ILogger _logger;
        readonly SimpleEventBus _eventBus;

        public LanewaysController(ISession session, LocationHelper locHelper, OpHelper opHelper, SimpleEventBus eventBus, ILogger logger)
        {
            _session = session;
            _locHelper = locHelper;
            _opHelper = opHelper;
            _eventBus = eventBus;
            _logger = logger;
        }

        /// <summary>
        /// 获取所有巷道
        /// </summary>
        /// <param name="args">查询参数</param>
        /// <returns></returns>
        [HttpGet]
        [DebugShowArgs]
        [AutoTransaction]
        [OperationType(OperationTypes.查看巷道)]
        public async Task<ListResult<LanewayListItem>> Get([FromQuery]LanewayListArgs args)
        {
            var pagedList = await _session.Query<Laneway>().SearchAsync(args, args.Sort, args.Current, args.PageSize);
            return new ListResult<LanewayListItem>
            {
                Success = true,
                Data = pagedList.List.Select(x => new LanewayListItem
                {
                    LanewayId = x.LanewayId,
                    LanewayCode = x.LanewayCode,
                    Automated = x.Automated,
                    DoubleDeep = x.DoubleDeep,
                    Offline = x.Offline,
                    OfflineComment = x.OfflineComment,
                    TotalLocationCount = x.GetTotalLocationCount(),
                    AvailableLocationCount = x.GetAvailableLocationCount(),
                    ReservedLocationCount = x.ReservedLocationCount,
                    Ports = x.Ports
                        .Select(x => new PortSelectListItem
                        {
                            PortId = x.PortId,
                            PortCode = x.PortCode,
                            CurrentUat = x.CurrentUat?.ToString()
                        })
                        .ToArray(),
                    TotalOfflineHours = x.TotalOfflineHours,
                }),
                Total = pagedList.Total
            };
        }

        /// <summary>
        /// 巷道选择列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("select-list")]
        [AutoTransaction]
        public async Task<List<LanewaySelectListItem>> GetSelectList()
        {
            var items = await _session.Query<Laneway>()
                .Select(x => new LanewaySelectListItem
                {
                    LanewayId = x.LanewayId,
                    LanewayCode = x.LanewayCode,
                    Offline = x.Offline,
                })
                .ToListAsync();
            return items;
        }

        /// <summary>
        /// 使巷道脱机
        /// </summary>
        /// <param name="id"></param>
        /// <param name="args">参数</param>
        /// <returns></returns>
        [HttpPost]
        [Route("{id}/actions/take-offline")]
        [OperationType(OperationTypes.脱机巷道)]
        [AutoTransaction]
        public async Task<IActionResult> TakeOffline(int id, [FromBody]TakeOfflineArgs args)
        {
            Laneway laneway = await _session.GetAsync<Laneway>(id);
            if (laneway == null)
            {
                return NotFound();
            }

            if (laneway.Offline == true)
            {
                throw new InvalidOperationException($"巷道已处于脱机状态。【{laneway.LanewayCode}】");
            }

            laneway.Offline = true;
            laneway.TakeOfflineTime = DateTime.Now;
            laneway.OfflineComment = args.Comment;
            await _session.UpdateAsync(laneway);
            _ = await _opHelper.SaveOpAsync($"巷道【{laneway.LanewayCode}】，备注【{args.Comment}】");
            _logger.Information("已将巷道 {lanewayCode} 脱机", laneway.LanewayCode);

            return this.Success();
        }


        /// <summary>
        /// 使巷道联机
        /// </summary>
        /// <param name="id"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{id}/actions/take-online")]
        [OperationType(OperationTypes.联机巷道)]
        [AutoTransaction]
        public async Task<IActionResult> TakeOnline(int id, TakeOnlineArgs args)
        {
            Laneway laneway = await _session.GetAsync<Laneway>(id);
            if (laneway == null)
            {
                return NotFound();
            }

            if (laneway.Offline == false)
            {
                throw new InvalidOperationException($"巷道已处于联机状态。【{laneway.LanewayCode}】");
            }

            laneway.Offline = false;
            laneway.TotalOfflineHours += DateTime.Now.Subtract(laneway.TakeOfflineTime).TotalHours;
            laneway.OfflineComment = args.Comment;
            await _session.UpdateAsync(laneway);
            _ = await _opHelper.SaveOpAsync($"巷道【{laneway.LanewayCode}】");
            _logger.Information("已将巷道 {lanewayCode} 联机", laneway.LanewayCode);

            return this.Success();
        }

        /// <summary>
        /// 设置巷道可以到达的出口
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{id}/actions/set-ports")]
        [OperationType(OperationTypes.设置出口)]
        [AutoTransaction]
        public async Task<IActionResult> SetPorts(int id, SetPortsArgs args)
        {
            Laneway laneway = await _session.GetAsync<Laneway>(id);
            if (laneway == null)
            {
                return NotFound();
            }

            laneway.Ports.Clear();
            foreach (var portId in args.PortIdList)
            {
                Port port = await _session.GetAsync<Port>(portId);
                laneway.Ports.Add(port);
            }

            var op = await _opHelper.SaveOpAsync("巷道【{0}】，{1} 个出货口", laneway.LanewayCode, laneway.Ports.Count);
            _logger.Information("设置出货口成功，{lanewayCode} --> {ports}", laneway.LanewayCode, string.Join(",", laneway.Ports.Select(x => x.PortCode)));

            return this.Success();
        }

        /// <summary>
        /// 巷道侧视图
        /// </summary>
        /// <param name="id">巷道Id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/side-view")]
        [OperationType(OperationTypes.侧视图)]
        [AutoTransaction]
        public async Task<ActionResult<SideViewData>> GetSideViewData(int id)
        {
            Laneway? laneway = await _session.GetAsync<Laneway>(id);
            if (laneway == null)
            {
                return NotFound();
            }

            var sideViewData = new SideViewData
            {
                LanewayCode = laneway.LanewayCode,
                Offline = laneway.Offline,
                OfflineComment = laneway.OfflineComment,
                Racks = laneway.Racks.Select(rack => new SideViewRack
                {
                    RackCode = rack.RackCode,
                    Side = rack.Side,
                    Columns = rack.Columns,
                    Levels = rack.Levels,
                    Deep = rack.Deep,
                    LocationCount = rack.Locations
                        .Where(x => x.Exists)
                        .Count(),
                    AvailableCount = rack.Locations
                        .Where(x =>
                            x.Exists
                            && x.UnitloadCount == 0
                            && x.InboundCount == 0
                            && x.InboundDisabled == false)
                        .Count(),
                    Locations = rack.Locations.Select(loc => new SideViewLocation
                    {
                        LocationId = loc.LocationId,
                        LocationCode = loc.LocationCode,
                        Loaded = loc.UnitloadCount > 0,
                        Level = loc.Level,
                        Column = loc.Column,
                        InboundDisabled = loc.InboundDisabled,
                        InboundDisabledComment = loc.InboundDisabledComment,
                        InboundCount = loc.InboundCount,
                        InboundLimit = loc.InboundLimit,
                        OutboundDisabled = loc.OutboundDisabled,
                        OutboundDisabledComment = loc.OutboundDisabledComment,
                        OutboundLimit = loc.OutboundLimit,
                        OutboundCount = loc.OutboundCount,
                        Specification = loc.Specification,
                        StorageGroup = loc.StorageGroup,
                        WeightLimit = loc.WeightLimit,
                        HeightLimit = loc.HeightLimit,
                        Exists = loc.Exists,
                        i1 = loc.Cell.i1,
                        o1 = loc.Cell.o1,
                        i2 = loc.Cell.i2,
                        o2 = loc.Cell.o2,
                        i3 = loc.Cell.i3,
                        o3 = loc.Cell.o3,
                    }).ToList()
                }).ToList(),
            };

            return sideViewData;
        }

        /// <summary>
        /// 重建所有巷道的统计信息，这个操作消耗资源较多
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("stats")]
        [OperationType(OperationTypes.重建巷道统计信息)]
        [AutoTransaction]
        public async Task<IActionResult> RebuildLanewaysStat()
        {
            var laneways = await _session.Query<Laneway>().ToListAsync();
            foreach (var laneway in laneways)
            {
                await _locHelper.RebuildLanewayStatAsync(laneway);
            }
            return this.Success();
        }

    }

}
