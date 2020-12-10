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

using Arctic.EventBus;
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
    [Route("[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        readonly ISession _session;
        readonly LocationHelper _locHelper;
        readonly ILocationFactory _locFactory;
        readonly OpHelper _opHelper;
        readonly ILogger _logger;
        readonly SimpleEventBus _eventBus;

        public LocationsController(ISession session, LocationHelper locHelper, ILocationFactory locFactory, OpHelper opHelper, SimpleEventBus eventBus, ILogger logger)
        {
            _session = session;
            _locHelper = locHelper;
            _locFactory = locFactory;
            _opHelper = opHelper;
            _eventBus = eventBus;
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
        [Route("storage-locations/list")]
        public async Task<StorageLocationList> StorageLocationListAsync(LocationListOfSArgs args)
        {
            var pagedList = await _session.Query<Location>().ToPagedListAsync(args);
            return new StorageLocationList
            {
                Success = true,
                Message = "OK",
                Data = pagedList.List.Select(x => new StorageLocationListItem
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
        [Route("key-points/list")]
        public async Task<KeyPointList> KeyPointListAsync(KeyPointListArgs args)
        {
            var pagedList = await _session.Query<Location>().ToPagedListAsync(args);
            return new KeyPointList
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

        /// <summary>
        /// 禁止入站
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("disable-inbound")]
        [OperationType(OperationTypes.禁止入站)]
        [AutoTransaction]
        public async Task<ActionResult> DisableInboundAsync(DisableInboundArgs args)
        {
            List<Location> locs = await _session.Query<Location>()
                .Where(x => args.LocationIdList.Contains(x.LocationId))
                .WrappedToListAsync();

            if (locs.Count == 0)
            {
                throw new InvalidOperationException("未指定货位。");
            }

            int affected = 0;
            foreach (var loc in locs)
            {
                if (loc.LocationType == LocationTypes.N)
                {
                    // N 位置无法禁入禁出
                    _logger.Warning("不能禁用或启用 N 位置。");
                    continue;
                }

                if (loc.Cell != null)
                {
                    foreach (var item in loc.Cell.Locations)
                    {
                        await DisableOneAsync(item);
                    }
                }
                else
                {
                    await DisableOneAsync(loc);
                }
            }

            var laneways = locs.Where(x => x.Rack != null).Select(x => x.Rack.Laneway).Distinct();
            foreach (var laneway in laneways)
            {
                await _locHelper.RebuildLanewayStatAsync(laneway);
            }

            async Task DisableOneAsync(Location loc)
            {
                if (loc.InboundDisabled == false)
                {
                    loc.InboundDisabled = true;
                    loc.InboundDisabledComment = args.Comment;
                    await _session.UpdateAsync(loc);

                    LocationOp op = new LocationOp();
                    op.OpType = _opHelper.GetOperationType();
                    op.Comment = args.Comment;
                    op.ctime = DateTime.Now;
                    op.Location = loc;
                    await _session.SaveAsync(op);

                    await _eventBus.FireEventAsync(EventTypes.LocationInboundDisabled, loc);

                    affected++;
                }
            }

            _ = await _opHelper.SaveOpAsync("将 {0} 个位置设为禁止入站。", affected);

            return Ok(new OperationResult
            {
                Success = true,
                Message = "操作成功",
            });
        }

        /// <summary>
        /// 允许入站
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("enable-inbound")]
        [OperationType(OperationTypes.允许入站)]
        [AutoTransaction]
        public async Task<ActionResult> EnableInboundAsync(EnableInboundArgs args)
        {
            List<Location> locs = await _session.Query<Location>()
                .Where(x => args.LocationIdList.Contains(x.LocationId))
                .WrappedToListAsync();

            if (locs.Count == 0)
            {
                throw new InvalidOperationException("未指定货位。");
            }

            int affected = 0;
            foreach (var loc in locs)
            {
                if (loc.LocationType == LocationTypes.N)
                {
                    // N 位置无法禁入禁出
                    _logger.Warning("不能禁用或启用 N 位置。");
                    continue;
                }

                if (loc.Cell != null)
                {
                    foreach (var item in loc.Cell.Locations)
                    {
                        await EnableOneAsync(item);
                    }
                }
                else
                {
                    await EnableOneAsync(loc);
                }
            }

            var laneways = locs.Where(x => x.Rack != null).Select(x => x.Rack.Laneway).Distinct();
            foreach (var laneway in laneways)
            {
                await _locHelper.RebuildLanewayStatAsync(laneway);
            }

            async Task EnableOneAsync(Location loc)
            {
                if (loc.InboundDisabled)
                {
                    loc.InboundDisabled = false;
                    loc.InboundDisabledComment = null;
                    await _session.UpdateAsync(loc);

                    LocationOp op = new LocationOp();
                    op.OpType = _opHelper.GetOperationType();
                    op.Comment = null;
                    op.ctime = DateTime.Now;
                    op.Location = loc;
                    await _session.SaveAsync(op);
                    await _eventBus.FireEventAsync(EventTypes.LocationInboundEnabled, loc);

                    affected++;
                }
            }

            _ = await _opHelper.SaveOpAsync("将 {0} 个位置设为允许入站。", affected);

            return Ok(new OperationResult
            {
                Success = true,
                Message = "操作成功",
            });
        }

        /// <summary>
        /// 禁止出站
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("disable-outbound")]
        [OperationType(OperationTypes.禁止出站)]
        [AutoTransaction]
        public async Task<ActionResult> DisableOutboundAsync(DisableOutboundArgs args)
        {
            List<Location> locs = await _session.Query<Location>()
                .Where(x => args.LocationIdList.Contains(x.LocationId))
                .WrappedToListAsync();
            if (locs.Count == 0)
            {
                throw new InvalidOperationException("未指定货位。");
            }
            int affected = 0;
            foreach (var loc in locs)
            {
                if (loc.LocationType == LocationTypes.N)
                {
                    // N 位置无法禁入禁出
                    _logger.Warning("不能禁用或启用 N 位置。");
                    continue;
                }

                if (loc.Cell != null)
                {
                    foreach (var item in loc.Cell.Locations)
                    {
                        await DisableOneAsync(item);
                    }
                }
                else
                {
                    await DisableOneAsync(loc);
                }
            }

            var laneways = locs.Where(x => x.Rack != null).Select(x => x.Rack.Laneway).Distinct();
            foreach (var laneway in laneways)
            {
                await _locHelper.RebuildLanewayStatAsync(laneway);
            }

            async Task DisableOneAsync(Location loc)
            {
                if (loc.OutboundDisabled == false)
                {
                    loc.OutboundDisabled = true;
                    loc.OutboundDisabledComment = args.Comment;
                    await _session.UpdateAsync(loc);

                    LocationOp op = new LocationOp();
                    op.OpType = _opHelper.GetOperationType();
                    op.Comment = args.Comment;
                    op.ctime = DateTime.Now;
                    op.Location = loc;
                    await _session.SaveAsync(op);

                    await _eventBus.FireEventAsync(EventTypes.LocationInboundDisabled, loc);
                    affected++;
                }
            }

            _ = await _opHelper.SaveOpAsync("将 {0} 个位置设为禁止出站。", affected);

            return Ok(new OperationResult
            {
                Success = true,
                Message = "操作成功",
            });
        }

        /// <summary>
        /// 允许入站
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("enable-outbound")]
        [OperationType(OperationTypes.允许入站)]
        [AutoTransaction]
        public async Task<ActionResult> EnableOutboundAsync(EnableOutboundArgs args)
        {
            List<Location> locs = await _session.Query<Location>()
                .Where(x => args.LocationIdList.Contains(x.LocationId))
                .WrappedToListAsync();
            if (locs.Count == 0)
            {
                throw new InvalidOperationException("未指定货位。");
            }
            int affected = 0;

            foreach (var loc in locs)
            {
                if (loc.LocationType == LocationTypes.N)
                {
                    // N 位置无法禁入禁出
                    _logger.Warning("不能禁用或启用 N 位置。");
                    continue;
                }

                if (loc.Cell != null)
                {
                    foreach (var item in loc.Cell.Locations)
                    {
                        await EnableOneAsync(item);
                    }
                }
                else
                {
                    await EnableOneAsync(loc);
                }
            }

            var laneways = locs.Where(x => x.Rack != null).Select(x => x.Rack.Laneway).Distinct();
            foreach (var laneway in laneways)
            {
                await _locHelper.RebuildLanewayStatAsync(laneway);
            }

            async Task EnableOneAsync(Location loc)
            {
                if (loc.OutboundDisabled)
                {
                    loc.OutboundDisabled = false;
                    loc.OutboundDisabledComment = null;
                    await _session.UpdateAsync(loc);

                    LocationOp op = new LocationOp();
                    op.OpType = _opHelper.GetOperationType();
                    op.Comment = null;
                    op.ctime = DateTime.Now;
                    op.Location = loc;
                    await _session.SaveAsync(op);

                    await _eventBus.FireEventAsync(EventTypes.LocationInboundDisabled, loc);
                    affected++;
                }
            }

            _ = await _opHelper.SaveOpAsync("将 {0} 个位置设为允许出站。", affected);

            return Ok(new OperationResult
            {
                Success = true,
                Message = "操作成功",
            });
        }


        /// <summary>
        /// 创建关键点
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("key-points/create")]
        [OperationType(OperationTypes.创建关键点)]
        [AutoTransaction]
        public async Task<OperationResult> CreateKeyPointAsync(CreateKeyPointArgs args)
        {
            Location loc = _locFactory.CreateLocation(args.LocationCode, LocationTypes.K, null, 0, 0);
            loc.RequestType = args.RequestType;
            loc.OutboundLimit = args.OutboundLimit;
            loc.InboundLimit = args.InboundLimit;
            loc.Tag = args.Tag;
            await _session.SaveAsync(loc);
            _ = await _opHelper.SaveOpAsync("{0}#{1}", loc.LocationCode, loc.LocationId);
            await _eventBus.FireEventAsync("KeyPointChanged", null);

            return new OperationResult
            {
                Success = true,
                Message = "操作成功",
            };
        }

        /// <summary>
        /// 编辑关键点
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("key-points/edit")]
        [OperationType(OperationTypes.编辑关键点)]
        [AutoTransaction]
        public async Task<OperationResult> EditKeyPointAsync(EditKeyPointArgs args)
        {
            Location loc = await _session.GetAsync<Location>(args.LocationId);
            if (loc == null || loc.LocationType != LocationTypes.K)
            {
                throw new InvalidOperationException($"关键点不存在，【#{args.LocationId}】。");
            }
            loc.LocationCode = args.LocationCode;
            loc.RequestType = args.RequestType;
            loc.OutboundLimit = args.OutboundLimit;
            loc.InboundLimit = args.InboundLimit;
            loc.Tag = args.Tag;
            await _session.UpdateAsync(loc);
            _ = await _opHelper.SaveOpAsync("{0}#{1}", loc.LocationCode, loc.LocationId);

            await _eventBus.FireEventAsync("KeyPointChanged", null);

            return new OperationResult
            {
                Success = true,
                Message = "操作成功",
            };
        }
    }
}
