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


    [Route("api/[controller]")]
    [ApiController]
    public class LocController : ControllerBase
    {
        readonly ISession _session;
        readonly OpHelper _opHelper;
        readonly LocationHelper _locHelper;
        readonly ILocationFactory _locFactory;
        readonly ILogger _logger;
        readonly SimpleEventBus _eventBus;

        public LocController(ISession session, LocationHelper locHelper, ILocationFactory locFactory, OpHelper opHelper, SimpleEventBus eventBus, ILogger logger)
        {
            _session = session;
            _locHelper = locHelper;
            _locFactory = locFactory;
            _opHelper = opHelper;
            _eventBus = eventBus;
            _logger = logger;
        }

        /// <summary>
        /// 获取所有巷道
        /// </summary>
        /// <param name="args">查询参数</param>
        /// <returns></returns>
        [HttpGet("get-laneway-list")]
        [DebugShowArgs]
        [AutoTransaction]
        [OperationType(OperationTypes.查看巷道)]
        public async Task<ListData<LanewayListItem>> GetLanewayList([FromQuery] LanewayListArgs args)
        {
            var pagedList = await _session.Query<Laneway>().SearchAsync(args, args.Sort, args.Current, args.PageSize);
            return this.ListData(pagedList, x => new LanewayListItem
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
                UsageRate = (x.GetTotalLocationCount() - x.GetAvailableLocationCount()) / (double)x.GetTotalLocationCount(),
                UsageInfos = x.Usage.Select(x => new LanewayUsageInfo
                {
                    StorageGroup = x.Key.StorageGroup,
                    WeightLimit = x.Key.WeightLimit,
                    HeightLimit = x.Key.HeightLimit,
                    Specification = x.Key.Specification,
                    Total = x.Value.Total,
                    Loaded = x.Value.Loaded,
                    InboundDisabled = x.Value.InboundDisabled,
                    Available = x.Value.Available,
                }).ToArray(),
                Ports = x.Ports
                        .Select(x => new PortOption
                        {
                            PortId = x.PortId,
                            PortCode = x.PortCode,
                            CurrentUat = x.CurrentUat?.ToString()
                        })
                        .ToArray(),
                TotalOfflineHours = x.TotalOfflineHours,
            });
        }

        /// <summary>
        /// 获取巷道的选项列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("get-laneway-options")]
        [AutoTransaction]
        public async Task<OptionsData<LanewayOption>> GetLanewayOptions()
        {
            var items = await _session.Query<Laneway>()
                .Select(x => new LanewayOption
                {
                    LanewayId = x.LanewayId,
                    LanewayCode = x.LanewayCode,
                    Offline = x.Offline,
                })
                .ToListAsync();
            return this.OptionsData(items);
        }

        /// <summary>
        /// 使巷道脱机
        /// </summary>
        /// <param name="id"></param>
        /// <param name="args">参数</param>
        /// <returns></returns>
        [HttpPost("take-offline/{id}")]
        [OperationType(OperationTypes.脱机巷道)]
        [AutoTransaction]
        public async Task<ApiData> TakeOffline(int id, [FromBody]TakeOfflineArgs args)
        {
            Laneway laneway = await _session.GetAsync<Laneway>(id);
            if (laneway == null)
            {
                throw new InvalidOperationException("巷道不存在。");
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
        [HttpPost("take-online/{id}")]
        [OperationType(OperationTypes.联机巷道)]
        [AutoTransaction]
        public async Task<ApiData> TakeOnline(int id, TakeOnlineArgs args)
        {
            Laneway laneway = await _session.GetAsync<Laneway>(id);
            if (laneway == null)
            {
                throw new InvalidOperationException("巷道不存在");
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
        /// <param name="id">巷道Id</param>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost("set-ports/{id}")]
        [OperationType(OperationTypes.设置出口)]
        [AutoTransaction]
        public async Task<ApiData> SetPorts(int id, SetPortsArgs args)
        {
            Laneway laneway = await _session.GetAsync<Laneway>(id);
            if (laneway == null)
            {
                throw new InvalidOperationException("巷道不存在");
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
        [HttpGet("get-side-view/{id}")]
        [OperationType(OperationTypes.侧视图)]
        [AutoTransaction]
        public async Task<ApiData<SideViewData>> GetSideViewData(int id)
        {
            Laneway? laneway = await _session.GetAsync<Laneway>(id);
            if (laneway == null)
            {
                throw new InvalidOperationException("巷道不存在");
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

            return this.Success(sideViewData);
        }

        /// <summary>
        /// 重建所有巷道的统计信息，这个操作消耗资源较多
        /// </summary>
        /// <returns></returns>
        [HttpPost("rebuild-stats")]
        [OperationType(OperationTypes.重建巷道统计信息)]
        [AutoTransaction]
        public async Task<ApiData> RebuildLanewaysStat()
        {
            var laneways = await _session.Query<Laneway>().ToListAsync();
            foreach (var laneway in laneways)
            {
                await _locHelper.RebuildLanewayStatAsync(laneway);
            }
            return this.Success();
        }

        /// <summary>
        /// 出口列表
        /// </summary>
        /// <param name="args">查询参数</param>
        /// <returns></returns>
        [HttpGet("get-port-list")]
        [DebugShowArgs]
        [AutoTransaction]
        [OperationType(OperationTypes.查看出口)]
        public async Task<ListData<PortListItem>> GetPortList([FromQuery] PortListArgs args)
        {
            var pagedList = await _session.Query<Port>().SearchAsync(args, args.Sort, args.Current, args.PageSize);
            return this.ListData(pagedList, x => new PortListItem
            {
                PortId = x.PortId,
                PortCode = x.PortCode,
                CurrentUat = x.CurrentUat?.ToString(),
                KP1 = x.KP1.LocationCode,
                KP2 = x.KP2?.LocationCode,
                Laneways = x.Laneways.Select(x => x.LanewayCode).ToArray(),
                CheckedAt = x.CheckedAt,
                CheckMessage = x.CheckMessage,
            });
        }

        /// <summary>
        /// 获取出口的选项列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("get-port-options")]
        [AutoTransaction]
        public async Task<OptionsData<PortOption>> GetPortOptions()
        {
            var list = await _session.Query<Port>().ToListAsync();
            var items = list
                .Select(x => new PortOption
                {
                    PortId = x.PortId,
                    PortCode = x.PortCode,
                    CurrentUat = x.CurrentUat?.ToString(),
                })
                .ToList();
            return this.OptionsData(items);
        }

        /// <summary>
        /// 货位列表
        /// </summary>
        /// <param name="args">查询参数</param>
        /// <returns></returns>
        [HttpGet("get-storage-location-list")]
        [DebugShowArgs]
        [AutoTransaction]
        [OperationType(OperationTypes.查看位置)]
        public async Task<ListData<StorageLocationListItem>> GetStorageLocationList([FromQuery] StorageLocationListArgs args)
        {
            var pagedList = await _session.Query<Location>().SearchAsync(args, args.Sort, args.Current, args.PageSize);
            return this.ListData(pagedList, x => new StorageLocationListItem
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
            });
        }

        /// <summary>
        /// 关键点列表
        /// </summary>
        /// <param name="args">查询参数</param>
        /// <returns></returns>
        [HttpGet("get-key-point-list")]
        [DebugShowArgs]
        [AutoTransaction]
        [OperationType(OperationTypes.查看位置)]
        public async Task<ListData<KeyPointListItem>> GetKeyPointList([FromQuery] KeyPointListArgs args)
        {
            var pagedList = await _session.Query<Location>().SearchAsync(args, args.Sort, args.Current, args.PageSize);
            return this.ListData(pagedList, x => new KeyPointListItem
            {
                LocationId = x.LocationId,
                LocationCode = x.LocationCode,
                InboundCount = x.InboundCount,
                InboundDisabled = x.InboundDisabled,
                InboundDisabledComment = x.InboundDisabledComment,
                InboundLimit = x.InboundLimit,
                OutboundCount = x.OutboundCount,
                OutboundDisabled = x.OutboundDisabled,
                OutboundDisabledComment = x.OutboundDisabledComment,
                OutboundLimit = x.OutboundLimit,
                Tag = x.Tag,
                RequestType = x.RequestType,
                UnitloadCount = x.UnitloadCount,
            });
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
        //    var list = await _session.Query<Port>().ToListAsync();
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
        /// <param name="ids">逗号分隔的位置Id</param>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost("disable-inbound/[[{ids}]]")]
        [OperationType(OperationTypes.禁止入站)]
        [AutoTransaction]
        public async Task<ApiData> DisableInbound(string ids, DisableInboundArgs args)
        {
            List<int> list = ids
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => int.Parse(x))
                .ToList();
            List<Location> locs = await _session.Query<Location>()
                .Where(x => list.Contains(x.LocationId))
                .ToListAsync();

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
            _ = await _opHelper.SaveOpAsync("将 {0} 个位置设为禁止入站", affected);

            return this.Success();

            async Task DisableOneAsync(Location loc)
            {
                if (loc.InboundDisabled == false)
                {
                    loc.InboundDisabled = true;
                    loc.InboundDisabledComment = args.Comment;
                    await _session.UpdateAsync(loc);

                    LocationOp op = new LocationOp
                    {
                        OpType = _opHelper.GetOperationType(),
                        Comment = args.Comment,
                        ctime = DateTime.Now,
                        Location = loc
                    };
                    await _session.SaveAsync(op);

                    await _eventBus.FireEventAsync(EventTypes.LocationInboundDisabled, loc);

                    affected++;
                }
            }
        }

        /// <summary>
        /// 允许入站
        /// </summary>
        /// <param name="ids">逗号分隔的位置Id</param>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost("enable-inbound/[[{ids}]]")]
        [OperationType(OperationTypes.允许入站)]
        [AutoTransaction]
        public async Task<ApiData> EnableInbound(string ids, EnableInboundArgs args)
        {
            List<int> list = ids
               .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
               .Select(x => int.Parse(x))
               .ToList();

            List<Location> locs = await _session.Query<Location>()
                .Where(x => list.Contains(x.LocationId))
                .ToListAsync();

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

            _ = await _opHelper.SaveOpAsync("将 {0} 个位置设为允许入站。", affected);

            return this.Success();

            async Task EnableOneAsync(Location loc)
            {
                if (loc.InboundDisabled)
                {
                    loc.InboundDisabled = false;
                    loc.InboundDisabledComment = null;
                    await _session.UpdateAsync(loc);

                    LocationOp op = new LocationOp
                    {
                        OpType = _opHelper.GetOperationType(),
                        Comment = null,
                        ctime = DateTime.Now,
                        Location = loc
                    };
                    await _session.SaveAsync(op);
                    await _eventBus.FireEventAsync(EventTypes.LocationInboundEnabled, loc);

                    affected++;
                }
            }
        }

        /// <summary>
        /// 禁止出站
        /// </summary>
        /// <param name="ids">逗号分隔的位置Id</param>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost("disable-outbound/[[{ids}]]")]
        [OperationType(OperationTypes.禁止出站)]
        [AutoTransaction]
        public async Task<ApiData> DisableOutbound(string ids, DisableOutboundArgs args)
        {
            List<int> list = ids
               .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
               .Select(x => int.Parse(x))
               .ToList();

            List<Location> locs = await _session.Query<Location>()
                .Where(x => list.Contains(x.LocationId))
                .ToListAsync();
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

            _ = await _opHelper.SaveOpAsync("将 {0} 个位置设为禁止出站。", affected);

            return this.Success();

            async Task DisableOneAsync(Location loc)
            {
                if (loc.OutboundDisabled == false)
                {
                    loc.OutboundDisabled = true;
                    loc.OutboundDisabledComment = args.Comment;
                    await _session.UpdateAsync(loc);

                    LocationOp op = new LocationOp
                    {
                        OpType = _opHelper.GetOperationType(),
                        Comment = args.Comment,
                        ctime = DateTime.Now,
                        Location = loc
                    };
                    await _session.SaveAsync(op);

                    await _eventBus.FireEventAsync(EventTypes.LocationInboundDisabled, loc);
                    affected++;
                }
            }
        }

        /// <summary>
        /// 允许入站
        /// </summary>
        /// <param name="ids">逗号分隔的位置Id</param>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost("enable-outbound/[[{ids}]]")]
        [OperationType(OperationTypes.允许入站)]
        [AutoTransaction]
        public async Task<ApiData> EnableOutbound(string ids, EnableOutboundArgs args)
        {
            List<int> list = ids
               .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
               .Select(x => int.Parse(x))
               .ToList();

            List<Location> locs = await _session.Query<Location>()
                .Where(x => list.Contains(x.LocationId))
                .ToListAsync();
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

            _ = await _opHelper.SaveOpAsync("将 {0} 个位置设为允许出站。", affected);
            return this.Success();

            async Task EnableOneAsync(Location loc)
            {
                if (loc.OutboundDisabled)
                {
                    loc.OutboundDisabled = false;
                    loc.OutboundDisabledComment = null;
                    await _session.UpdateAsync(loc);

                    LocationOp op = new LocationOp
                    {
                        OpType = _opHelper.GetOperationType(),
                        Comment = null,
                        ctime = DateTime.Now,
                        Location = loc
                    };
                    await _session.SaveAsync(op);

                    await _eventBus.FireEventAsync(EventTypes.LocationInboundDisabled, loc);
                    affected++;
                }
            }
        }


        /// <summary>
        /// 创建关键点
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost("create-key-point")]
        [OperationType(OperationTypes.创建关键点)]
        [AutoTransaction]
        public async Task<ApiData> CreateKeyPoint(CreateKeyPointArgs args)
        {
            Location loc = _locFactory.CreateLocation(args.LocationCode, LocationTypes.K, null, 0, 0);
            loc.RequestType = args.RequestType;
            loc.OutboundLimit = args.OutboundLimit;
            loc.InboundLimit = args.InboundLimit;
            loc.Tag = args.Tag;
            await _session.SaveAsync(loc);
            _ = await _opHelper.SaveOpAsync("{0}#{1}", loc.LocationCode, loc.LocationId);
            await _eventBus.FireEventAsync("KeyPointChanged", null);

            return this.Success();
        }

        /// <summary>
        /// 编辑关键点
        /// </summary>
        /// <param name="id">关键点Id</param>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost("update-key-point/{id}")]
        [OperationType(OperationTypes.编辑关键点)]
        [AutoTransaction]
        public async Task<ApiData> UpdateKeyPoint(int id, UpdateKeyPointArgs args)
        {
            Location loc = await _session.GetAsync<Location>(id);
            if (loc == null || loc.LocationType != LocationTypes.K)
            {
                throw new InvalidOperationException("关键点不存在。");
            }
            loc.LocationCode = args.LocationCode;
            loc.RequestType = args.RequestType;
            loc.OutboundLimit = args.OutboundLimit;
            loc.InboundLimit = args.InboundLimit;
            loc.Tag = args.Tag;
            await _session.UpdateAsync(loc);
            _ = await _opHelper.SaveOpAsync("{0}#{1}", loc.LocationCode, loc.LocationId);

            await _eventBus.FireEventAsync("KeyPointChanged", null);

            return this.Success();
        }

    }

}
