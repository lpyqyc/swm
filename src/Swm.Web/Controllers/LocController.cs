// Copyright 2020-2021 王建军
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using NHibernate.Linq;
using Serilog;
using Swm.Locations;
using Swm.Ops;
using Swm.Palletization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 提供位置 api
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LocController : ControllerBase
    {
        readonly ISession _session;
        readonly OpHelper _opHelper;
        readonly LocationHelper _locHelper;
        readonly Func<Location> _locFactory;
        readonly ILogger _logger;
        readonly SimpleEventBus _eventBus;

        /// <summary>
        /// 初始化新实例。
        /// </summary>
        /// <param name="session"></param>
        /// <param name="locHelper"></param>
        /// <param name="locFactory"></param>
        /// <param name="opHelper"></param>
        /// <param name="eventBus"></param>
        /// <param name="logger"></param>
        public LocController(ISession session, LocationHelper locHelper, Func<Location> locFactory, OpHelper opHelper, SimpleEventBus eventBus, ILogger logger)
        {
            _session = session;
            _locHelper = locHelper;
            _locFactory = locFactory;
            _opHelper = opHelper;
            _eventBus = eventBus;
            _logger = logger;
        }

        /// <summary>
        /// 巷道列表
        /// </summary>
        /// <param name="args">查询参数</param>
        /// <returns></returns>
        [HttpGet("get-streetlet-list")]
        [DebugShowArgs]
        [AutoTransaction]
        [OperationType(OperationTypes.查看巷道)]
        public async Task<ListData<StreetletInfo>> GetStreetletList([FromQuery] StreetletListArgs args)
        {
            var pagedList = await _session.Query<Streetlet>().SearchAsync(args, args.Sort, args.Current, args.PageSize);
            return this.ListData(pagedList, x => new StreetletInfo
            {
                StreetletId = x.StreetletId,
                StreetletCode = x.StreetletCode,
                Automated = x.Automated,
                DoubleDeep = x.DoubleDeep,
                Offline = x.Offline,
                OfflineComment = x.OfflineComment,
                TotalLocationCount = x.GetTotalLocationCount(),
                AvailableLocationCount = x.GetAvailableLocationCount(),
                ReservedLocationCount = x.ReservedLocationCount,
                UsageRate = (x.GetTotalLocationCount() - x.GetAvailableLocationCount()) / (double)x.GetTotalLocationCount(),
                UsageInfos = x.Usage.Select(x => new StreetletUsageInfo
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
                        .Select(x => new PortInfo
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
        [HttpGet("get-streetlet-options")]
        [AutoTransaction]
        public async Task<OptionsData<StreetletInfo>> GetStreetletOptions()
        {
            var items = await _session.Query<Streetlet>()
                .Select(x => new StreetletInfo
                {
                    StreetletId = x.StreetletId,
                    StreetletCode = x.StreetletCode,
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
        public async Task<ApiData> TakeOffline(int id, [FromBody] TakeOfflineArgs args)
        {
            Streetlet streetlet = await _session.GetAsync<Streetlet>(id);
            if (streetlet == null)
            {
                throw new InvalidOperationException("巷道不存在。");
            }

            if (streetlet.Offline == true)
            {
                throw new InvalidOperationException($"巷道已处于脱机状态。【{streetlet.StreetletCode}】");
            }

            streetlet.Offline = true;
            streetlet.TakeOfflineTime = DateTime.Now;
            streetlet.OfflineComment = args.Comment;
            await _session.UpdateAsync(streetlet);
            _ = await _opHelper.SaveOpAsync($"巷道【{streetlet.StreetletCode}】，备注【{args.Comment}】");
            _logger.Information("已将巷道 {streetletCode} 脱机", streetlet.StreetletCode);

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
        public async Task<ApiData> TakeOnline(int id, TakeOfflineArgs args)
        {
            Streetlet streetlet = await _session.GetAsync<Streetlet>(id);
            if (streetlet == null)
            {
                throw new InvalidOperationException("巷道不存在");
            }

            if (streetlet.Offline == false)
            {
                throw new InvalidOperationException($"巷道已处于联机状态。【{streetlet.StreetletCode}】");
            }

            streetlet.Offline = false;
            streetlet.TotalOfflineHours += DateTime.Now.Subtract(streetlet.TakeOfflineTime).TotalHours;
            streetlet.OfflineComment = args.Comment;
            await _session.UpdateAsync(streetlet);
            _ = await _opHelper.SaveOpAsync($"巷道【{streetlet.StreetletCode}】");
            _logger.Information("已将巷道 {streetletCode} 联机", streetlet.StreetletCode);

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
            Streetlet streetlet = await _session.GetAsync<Streetlet>(id);
            if (streetlet == null)
            {
                throw new InvalidOperationException("巷道不存在");
            }

            streetlet.Ports.Clear();
            foreach (var portId in args.PortIdList)
            {
                Port port = await _session.GetAsync<Port>(portId);
                streetlet.Ports.Add(port);
            }

            var op = await _opHelper.SaveOpAsync("巷道【{0}】，{1} 个出货口", streetlet.StreetletCode, streetlet.Ports.Count);
            _logger.Information("设置出货口成功，{streetletCode} --> {ports}", streetlet.StreetletCode, string.Join(",", streetlet.Ports.Select(x => x.PortCode)));

            return this.Success();
        }

        /// <summary>
        /// 巷道侧视图
        /// </summary>
        /// <param name="streetletCode">巷道编号</param>
        /// <returns></returns>
        [HttpGet("get-side-view/{streetletCode}")]
        [OperationType(OperationTypes.侧视图)]
        [AutoTransaction]
        public async Task<ApiData<SideViewData>> GetSideViewData(string streetletCode)
        {
            Streetlet? streetlet = await _session.Query<Streetlet>().SingleOrDefaultAsync(x => x.StreetletCode == streetletCode);
            if (streetlet == null)
            {
                throw new InvalidOperationException("巷道不存在");
            }

            var sideViewData = new SideViewData
            {
                StreetletCode = streetlet.StreetletCode,
                Offline = streetlet.Offline,
                OfflineComment = streetlet.OfflineComment,
                AvailableCount = streetlet.Locations
                        .Where(x =>
                            x.Exists
                            && x.UnitloadCount == 0
                            && x.InboundCount == 0
                            && x.InboundDisabled == false)
                        .Count(),
                LocationCount = streetlet.Locations
                        .Where(x => x.Exists)
                        .Count(),
                Locations = streetlet.Locations.Select(loc => new SideViewLocation
                {
                    LocationId = loc.LocationId,
                    LocationCode = loc.LocationCode,
                    Loaded = loc.UnitloadCount > 0,
                    Side = loc.Side,
                    Deep = loc.Deep,
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
                    i1 = loc.Cell?.i1 ?? default,
                    o1 = loc.Cell?.o1 ?? default,
                    i2 = loc.Cell?.i2 ?? default,
                    o2 = loc.Cell?.o2 ?? default,
                    i3 = loc.Cell?.i3 ?? default,
                    o3 = loc.Cell?.o3 ?? default,
                }).ToList()
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
        public async Task<ApiData> RebuildStreetletsStat()
        {
            var streetlets = await _session.Query<Streetlet>().ToListAsync();
            foreach (var streetlet in streetlets)
            {
                await _locHelper.RebuildStreetletStatAsync(streetlet);
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
        public async Task<ListData<PortInfo>> GetPortList([FromQuery] PortListArgs args)
        {
            var pagedList = await _session.Query<Port>().SearchAsync(args, args.Sort, args.Current, args.PageSize);
            return this.ListData(pagedList, x => new PortInfo
            {
                PortId = x.PortId,
                PortCode = x.PortCode,
                CurrentUat = x.CurrentUat?.ToString(),
                KP1 = x.KP1?.LocationCode,
                KP2 = x.KP2?.LocationCode,
                Streetlets = x.Streetlets.Select(x => x.StreetletCode).ToArray(),
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
        public async Task<OptionsData<PortInfo>> GetPortOptions()
        {
            var list = await _session.Query<Port>().ToListAsync();
            var items = list
                .Select(x => new PortInfo
                {
                    PortId = x.PortId,
                    PortCode = x.PortCode,
                    CurrentUat = x.CurrentUat?.ToString(),
                    KP1 = x.KP1.LocationCode,
                    KP2 = x.KP2?.LocationCode,
                    Streetlets = x.Streetlets.Select(x => x.StreetletCode).ToArray(),
                    CheckedAt = x.CheckedAt,
                    CheckMessage = x.CheckMessage,
                })
                .ToList();
            return this.OptionsData(items);
        }

        /// <summary>
        /// 储位列表
        /// </summary>
        /// <param name="args">查询参数</param>
        /// <returns></returns>
        [HttpGet("get-storage-location-list")]
        [DebugShowArgs]
        [AutoTransaction]
        [OperationType(OperationTypes.查看位置)]
        public async Task<ListData<StorageLocationInfo>> GetStorageLocationList([FromQuery] StorageLocationListArgs args)
        {
            var pagedList = await _session.Query<Location>().SearchAsync(args, args.Sort, args.Current, args.PageSize);
            return this.ListData(pagedList, x => new StorageLocationInfo
            {
                LocationId = x.LocationId,
                LocationCode = x.LocationCode,
                StreetletId = x.Streetlet!.StreetletId,
                StreetletCode = x.Streetlet.StreetletCode,
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
        public async Task<ListData<KeyPointInfo>> GetKeyPointList([FromQuery] KeyPointListArgs args)
        {
            var pagedList = await _session.Query<Location>().SearchAsync(args, args.Sort, args.Current, args.PageSize);
            return this.ListData(pagedList, x => new KeyPointInfo
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


        /// <summary>
        /// 禁止入站
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost("disable-inbound")]
        [OperationType(OperationTypes.禁止入站)]
        [AutoTransaction]
        public async Task<ApiData> DisableInbound(DisableLocationArgs args)
        {
            List<Location> locs = await _session.Query<Location>()
                .Where(x => args.LocationIds.Contains(x.LocationId))
                .ToListAsync();

            int affected = 0;
            foreach (var loc in locs)
            {
                if (loc.LocationType == LocationTypes.N)
                {
                    // N 位置无法禁入禁出
                    _logger.Warning("不能禁用或启用 N 位置");
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

            var streetlets = locs
                .Where(x => x.Streetlet != null)
                .Select(x => x.Streetlet)
                .Distinct();
            foreach (var streetlet in streetlets)
            {
                if (streetlet != null)
                {
                    await _locHelper.RebuildStreetletStatAsync(streetlet);
                }
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

                    await _eventBus.FireEventAsync(LocationsEventTypes.LocationInboundDisabled, loc);

                    affected++;
                }
            }
        }

        /// <summary>
        /// 允许入站
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost("enable-inbound")]
        [OperationType(OperationTypes.允许入站)]
        [AutoTransaction]
        public async Task<ApiData> EnableInbound(DisableLocationArgs args)
        {
            List<Location> locs = await _session.Query<Location>()
                .Where(x => args.LocationIds.Contains(x.LocationId))
                .ToListAsync();

            int affected = 0;
            foreach (var loc in locs)
            {
                if (loc.LocationType == LocationTypes.N)
                {
                    // N 位置无法禁入禁出
                    _logger.Warning("不能禁用或启用 N 位置");
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

            var streetlets = locs.Where(x => x.Streetlet != null).Select(x => x.Streetlet).Distinct();
            foreach (var streetlet in streetlets)
            {
                await _locHelper.RebuildStreetletStatAsync(streetlet!);
            }

            _ = await _opHelper.SaveOpAsync("将 {0} 个位置设为允许入站。", affected);

            return this.Success();

            async Task EnableOneAsync(Location loc)
            {
                if (loc.InboundDisabled)
                {
                    loc.InboundDisabled = false;
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
                    await _eventBus.FireEventAsync(LocationsEventTypes.LocationInboundEnabled, loc);

                    affected++;
                }
            }
        }

        /// <summary>
        /// 禁止出站
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost("disable-outbound")]
        [OperationType(OperationTypes.禁止出站)]
        [AutoTransaction]
        public async Task<ApiData> DisableOutbound(DisableLocationArgs args)
        {
            List<Location> locs = await _session.Query<Location>()
                .Where(x => args.LocationIds.Contains(x.LocationId))
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
                    _logger.Warning("不能禁用或启用 N 位置");
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

            var streetlets = locs.Where(x => x.Streetlet != null).Select(x => x.Streetlet).Distinct();
            foreach (var streetlet in streetlets)
            {
                if (streetlet != null)
                {
                    await _locHelper.RebuildStreetletStatAsync(streetlet);
                }
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

                    await _eventBus.FireEventAsync(LocationsEventTypes.LocationInboundDisabled, loc);
                    affected++;
                }
            }
        }

        /// <summary>
        /// 允许入站
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost("enable-outbound")]
        [OperationType(OperationTypes.允许入站)]
        [AutoTransaction]
        public async Task<ApiData> EnableOutbound(DisableLocationArgs args)
        {
            List<Location> locs = await _session.Query<Location>()
                .Where(x => args.LocationIds.Contains(x.LocationId))
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
                    _logger.Warning("不能禁用或启用 N 位置");
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

            var streetlets = locs.Where(x => x.Streetlet != null).Select(x => x.Streetlet).Distinct();
            foreach (var streetlet in streetlets)
            {
                if (streetlet != null)
                {
                    await _locHelper.RebuildStreetletStatAsync(streetlet);
                }
            }

            _ = await _opHelper.SaveOpAsync("将 {0} 个位置设为允许出站", affected);
            return this.Success();

            async Task EnableOneAsync(Location loc)
            {
                if (loc.OutboundDisabled)
                {
                    loc.OutboundDisabled = false;
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

                    await _eventBus.FireEventAsync(LocationsEventTypes.LocationInboundDisabled, loc);
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
        public async Task<ApiData> CreateKeyPoint(CreateUpdateKeyPointArgs args)
        {
            Location loc = _locFactory.Invoke();
            loc.LocationCode = args.LocationCode;
            loc.LocationType = LocationTypes.K;
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
        public async Task<ApiData> UpdateKeyPoint(int id, CreateUpdateKeyPointArgs args)
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

        /// <summary>
        /// 创建出口
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [OperationType(OperationTypes.创建出口)]
        [AutoTransaction]
        [HttpPost("create-port")]
        public async Task<ApiData> CreatePort(CreatePortArgs args)
        {
            Port port = new Port(args.PortCode);
            if (string.IsNullOrWhiteSpace(args.KP1) == false)
            {
                port.KP1 = await CreateKeyPointAsync(_locFactory, _session, args.KP1);
            }
            if (string.IsNullOrWhiteSpace(args.KP2) == false)
            {
                port.KP2 = await CreateKeyPointAsync(_locFactory, _session, args.KP2);
            }

            await _session.SaveAsync(port).ConfigureAwait(false);

            return this.Success();

            static async Task<Location> CreateKeyPointAsync(Func<Location> locationFactory, ISession session, string locationCode)
            {
                Location loc = locationFactory.Invoke();
                loc.LocationCode = locationCode;
                loc.LocationType = LocationTypes.K;
                loc.RequestType = null;
                loc.Tag = "港口";
                loc.InboundLimit = 999;
                loc.OutboundLimit = 999;
                await session.SaveAsync(loc).ConfigureAwait(false);
                return loc;
            }

        }


        /// <summary>
        /// 设置分组
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost("set-storage-group")]
        [OperationType(OperationTypes.设置分组)]
        [AutoTransaction]
        public async Task<ApiData> SetStorageGroup(SetStorageGroupArgs args)
        {
            List<Location> locs = await _session.Query<Location>()
                .Where(x => args.LocationIds.Contains(x.LocationId))
                .ToListAsync();

            int affected = 0;
            foreach (var loc in locs)
            {
                if (loc.LocationType != LocationTypes.S)
                {
                    // N 位置无法禁入禁出
                    _logger.Warning("只能为类型为 S 的位置设置存储分组");
                    continue;
                }
                LocationOp op = new LocationOp
                {
                    OpType = _opHelper.GetOperationType() ?? throw new InvalidOperationException(),
                    Comment = $"{loc.StorageGroup} --> {args.StorageGroup}",
                    ctime = DateTime.Now,
                    Location = loc
                };
                await _session.SaveAsync(op);

                loc.StorageGroup = args.StorageGroup;
                await _session.UpdateAsync(loc);
            }

            var streetlets = locs.Where(x => x.Streetlet != null).Select(x => x.Streetlet).Distinct();
            foreach (var streetlet in streetlets)
            {
                if (streetlet != null)
                {
                    await _locHelper.RebuildStreetletStatAsync(streetlet);
                }
            }
            _ = await _opHelper.SaveOpAsync($"将 {affected} 个位置的存储分组设为 {args.StorageGroup}");

            return this.Success();

        }


        /// <summary>
        /// 设置限高
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost("set-height-limit")]
        [OperationType(OperationTypes.设置限高)]
        [AutoTransaction]
        public async Task<ApiData> SetHeightLimit(SetHeightLimitArgs args)
        {
            List<Location> locs = await _session.Query<Location>()
                .Where(x => args.LocationIds.Contains(x.LocationId))
                .ToListAsync();

            int affected = 0;
            foreach (var loc in locs)
            {
                if (loc.LocationType != LocationTypes.S)
                {
                    // N 位置无法禁入禁出
                    _logger.Warning("只能为类型为 S 的位置设置限高");
                    continue;
                }
                LocationOp op = new LocationOp
                {
                    OpType = _opHelper.GetOperationType() ?? throw new InvalidOperationException(),
                    Comment = $"{loc.HeightLimit} --> {args.HeightLimit}",
                    ctime = DateTime.Now,
                    Location = loc
                };
                await _session.SaveAsync(op);

                loc.HeightLimit = args.HeightLimit;
                await _session.UpdateAsync(loc);
            }

            var streetlets = locs.Where(x => x.Streetlet != null).Select(x => x.Streetlet).Distinct();
            foreach (var streetlet in streetlets)
            {
                if (streetlet != null)
                {
                    await _locHelper.RebuildStreetletStatAsync(streetlet);
                }
            }
            _ = await _opHelper.SaveOpAsync($"将 {affected} 个位置的存储分组设为 {args.HeightLimit}");

            return this.Success();

        }


        /// <summary>
        /// 设置限重
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost("set-weight-limit")]
        [OperationType(OperationTypes.设置限重)]
        [AutoTransaction]
        public async Task<ApiData> SetWeightLimit(SetWeightLimitArgs args)
        {
            List<Location> locs = await _session.Query<Location>()
                .Where(x => args.LocationIds.Contains(x.LocationId))
                .ToListAsync();

            int affected = 0;
            foreach (var loc in locs)
            {
                if (loc.LocationType != LocationTypes.S)
                {
                    // N 位置无法禁入禁出
                    _logger.Warning("只能为类型为 S 的位置设置限重");
                    continue;
                }
                LocationOp op = new LocationOp
                {
                    OpType = _opHelper.GetOperationType() ?? throw new InvalidOperationException(),
                    Comment = $"{loc.WeightLimit} --> {args.WeightLimit}",
                    ctime = DateTime.Now,
                    Location = loc
                };
                await _session.SaveAsync(op);

                loc.WeightLimit = args.WeightLimit;
                await _session.UpdateAsync(loc);
            }

            var streetlets = locs.Where(x => x.Streetlet != null).Select(x => x.Streetlet).Distinct();
            foreach (var streetlet in streetlets)
            {
                if (streetlet != null)
                {
                    await _locHelper.RebuildStreetletStatAsync(streetlet);
                }
            }
            _ = await _opHelper.SaveOpAsync($"将 {affected} 个位置的存储分组设为 {args.WeightLimit}");

            return this.Success();

        }




        /// <summary>
        /// 获取储位详情
        /// </summary>
        /// <param name="locationCode">位置编码</param>
        /// <returns></returns>
        [HttpGet("get-storage-location-detail/{locationCode}")]
        [OperationType(OperationTypes.查看位置)]
        [AutoTransaction]
        public async Task<ApiData<StorageLocationDetail>> GetStorageLocationDetail(string locationCode)
        {
            var loc = await _session.Query<Location>()
                .Where(x => x.LocationType == LocationTypes.S && x.LocationCode == locationCode)
                .SingleOrDefaultAsync();
            if (loc == null)
            {
                throw new InvalidOperationException("位置不存在");
            }

            var unitloads = await _session.Query<Unitload>()
                .Where(x => x.CurrentLocation == loc)
                .ToListAsync();

            var detail = new StorageLocationDetail
            {
                LocationId = loc.LocationId,
                LocationCode = loc.LocationCode,
                Exists = loc.Exists,
                StreetletId = loc.Streetlet!.StreetletId,
                StreetletCode = loc.Streetlet.StreetletCode,
                WeightLimit = loc.WeightLimit,
                HeightLimit = loc.HeightLimit,
                InboundCount = loc.InboundCount,
                InboundDisabled = loc.InboundDisabled,
                InboundDisabledComment = loc.InboundDisabledComment,
                OutboundCount = loc.OutboundCount,
                OutboundDisabled = loc.OutboundDisabled,
                OutboundDisabledComment = loc.OutboundDisabledComment,
                StorageGroup = loc.StorageGroup,
                UnitloadCount = loc.UnitloadCount,
                Unitloads = unitloads.Select(u => DtoConvert.ToUnitloadDetail(u)).ToArray(),
            };

            return this.Success(detail);

        }

    }

}
