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
using Arctic.NHibernateExtensions.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Swm.Model;
using System.Threading.Tasks;

namespace Swm.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LocController : ControllerBase
    {
        readonly ILogger _logger;
        readonly SimpleEventBus _eventBus;
        readonly global::NHibernate.ISession _session;
        readonly ILocationFactory _factory;

        public LocController(
            global::NHibernate.ISession session,
            ILocationFactory factory,
            SimpleEventBus eventBus,
            ILogger logger)
        {
            _session = session;
            _factory = factory;
            _logger = logger;
            _eventBus = eventBus;
        }





        [HttpPost]
        [Route("load-port-list")]
        [OperationType(OperationTypes.查看位置)]
        [AutoTx]
        public async Task<ActionResult> LoadPortListAsync(PortListArgs args)
        {
            var (list, totalItemCount) = await _session.Query<Port>().ListAsync(args);

            var items = list.Select(x => new
            {
                x.PortId,
                x.PortCode,
                KP1 = x.KP1.LocationCode,
                KP2 = x.KP2?.LocationCode,
                Laneways = x.Laneways.Select(x => x.LanewayCode),
                CurrentUat = "后端未赋值", // x.CurrentUat?.ToString(),
                x.CheckedAt,
                x.CheckMessage,
            });
            return this.Success(items, totalItemCount);
        }

        [HttpPost]
        [Route("get-select-list-of-ports")]
        [AutoTx]
        public async Task<ActionResult> GetSelectListOfPortsAsync()
        {
            var items = await _session
                .Query<Port>()
                .Select(x => new
                {
                    x.PortId,
                    x.PortCode,
                    KP1 = x.KP1.LocationCode,
                    KP2 = x.KP2 == null ? "" : x.KP2.LocationCode,
                    Laneways = x.Laneways.Select(x => x.LanewayCode),
                })
                .ToListXAsync();
            return Ok(items);
        }




        [HttpPost]
        [Route("load-as-list")]
        [OperationType(OperationTypes.查看位置)]
        [AutoTx]
        public async Task<ActionResult> ASListAsync(ASListArgs args)
        {
            var (list, totalItemCount) = await _session.Query<Location>().ListAsync(args);

            var items = list.Select(x => new
            {
                x.LocationId,
                x.LocationCode,
                x.Rack.Laneway.LanewayId,
                x.Rack.Laneway.LanewayCode,
                x.WeightLimit,
                x.HeightLimit,
                x.InboundCount,
                x.InboundDisabled,
                x.InboundDisabledComment,
                x.OutboundCount,
                x.OutboundDisabled,
                x.OutboundDisabledComment,
                x.StorageGroup,
                x.UnitloadCount,
            });
            return this.Success(items, totalItemCount);
        }

        [HttpPost]
        [Route("load-ak-list")]
        [OperationType(OperationTypes.查看位置)]
        [AutoTx]
        public async Task<ActionResult> AKListAsync(AKListArgs args)
        {
            var (list, totalItemCount) = await _session.Query<Location>().ListAsync(args);

            var items = list.Select(x => new
            {
                x.LocationId,
                x.LocationCode,
                x.InboundCount,
                x.InboundDisabled,
                x.InboundDisabledComment,
                x.InboundLimit,
                x.OutboundCount,
                x.OutboundDisabled,
                x.OutboundDisabledComment,
                x.OutboundLimit,
                x.Tag,
                x.RequestType,
                x.UnitloadCount,
            });

            return this.Success(items, totalItemCount);
        }


        [HttpPost]
        [Route("disable-inbound")]
        [OperationType(OperationTypes.禁止入站)]
        [AutoTx]
        public async Task<ActionResult> DisableInboundAsync(DisableInboundArgs args)
        {
            AutoTxAttribute.BuildResultOnError(HttpContext, ex =>
            {
                string errMsg = "禁止入站时出错。" + ex.Message;
                return this.Error(errMsg);
            });

            List<Location> locs = await _session.Query<Location>().Where(x => args.LocationIdList.Contains(x.LocationId)).ToListXAsync();

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
                await RebuildLanewayStatAsync(laneway);
            }

            async Task DisableOneAsync(Location loc)
            {
                if (loc.InboundDisabled == false)
                {
                    loc.InboundDisabled = true;
                    loc.InboundDisabledComment = args.Comment;
                    await _session.UpdateAsync(loc);

                    LocationOp op = new LocationOp();
                    op.OpType = this.HttpContext.GetOpType();
                    op.Comment = args.Comment;
                    op.ctime = DateTime.Now;
                    op.Location = loc;
                    await _session.SaveAsync(op);

                    await _eventBus.FireEventAsync(EventTypes.LocationInboundDisabled, loc);

                    affected++;
                }
            }

            _ = await HttpContext.SaveOpAsync("将 {0} 个位置设为禁止入站。", affected);

            return this.Success("操作成功");
        }

        [HttpPost]
        [Route("enable-inbound")]
        [OperationType(OperationTypes.允许入站)]
        [AutoTx]
        public async Task<ActionResult> EnableInboundAsync(EnableInboundArgs args)
        {
            AutoTxAttribute.BuildResultOnError(HttpContext, ex =>
            {
                string errMsg = "允许入站时出错。" + ex.Message;
                return this.Error(errMsg);
            });

            List<Location> locs = await _session.Query<Location>().Where(x => args.LocationIdList.Contains(x.LocationId)).ToListXAsync();

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
                await RebuildLanewayStatAsync(laneway);
            }

            async Task EnableOneAsync(Location loc)
            {
                if (loc.InboundDisabled)
                {
                    loc.InboundDisabled = false;
                    loc.InboundDisabledComment = null;
                    await _session.UpdateAsync(loc);

                    LocationOp op = new LocationOp();
                    op.OpType = this.HttpContext.GetOpType();
                    op.Comment = null;
                    op.ctime = DateTime.Now;
                    op.Location = loc;
                    await _session.SaveAsync(op);
                    await _eventBus.FireEventAsync(EventTypes.LocationInboundEnabled, loc);

                    affected++;
                }
            }

            _ = await HttpContext.SaveOpAsync("将 {0} 个位置设为允许入站。", affected);

            return this.Success("操作成功");
        }

        [HttpPost]
        [Route("disable-outbound")]
        [OperationType(OperationTypes.禁止出站)]
        [AutoTx]
        public async Task<ActionResult> DisableOutboundAsync(DisableOutboundArgs args)
        {
            AutoTxAttribute.BuildResultOnError(HttpContext, ex =>
            {
                string errMsg = "禁止出站时出错。" + ex.Message;
                return this.Error(errMsg);
            });

            List<Location> locs = await _session.Query<Location>().Where(x => args.LocationIdList.Contains(x.LocationId)).ToListXAsync();
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
                await RebuildLanewayStatAsync(laneway);
            }

            async Task DisableOneAsync(Location loc)
            {
                if (loc.OutboundDisabled == false)
                {
                    loc.OutboundDisabled = true;
                    loc.OutboundDisabledComment = args.Comment;
                    await _session.UpdateAsync(loc);

                    LocationOp op = new LocationOp();
                    op.OpType = this.HttpContext.GetOpType();
                    op.Comment = args.Comment;
                    op.ctime = DateTime.Now;
                    op.Location = loc;
                    await _session.SaveAsync(op);

                    await _eventBus.FireEventAsync(EventTypes.LocationInboundDisabled, loc);
                    affected++;
                }
            }

            _ = await HttpContext.SaveOpAsync("将 {0} 个位置设为禁止出站。", affected);

            return this.Success("操作成功");
        }

        [HttpPost]
        [Route("enable-outbound")]
        [OperationType(OperationTypes.允许入站)]
        [AutoTx]
        public async Task<ActionResult> EnableOutboundAsync(EnableOutboundArgs args)
        {
            AutoTxAttribute.BuildResultOnError(HttpContext, ex =>
            {
                string errMsg = "允许出站时出错。" + ex.Message;
                return this.Error(errMsg);
            });

            List<Location> locs = await _session.Query<Location>().Where(x => args.LocationIdList.Contains(x.LocationId)).ToListXAsync();
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
                await RebuildLanewayStatAsync(laneway);
            }

            async Task EnableOneAsync(Location loc)
            {
                if (loc.OutboundDisabled)
                {
                    loc.OutboundDisabled = false;
                    loc.OutboundDisabledComment = null;
                    await _session.UpdateAsync(loc);

                    LocationOp op = new LocationOp();
                    op.OpType = this.HttpContext.GetOpType();
                    op.Comment = null;
                    op.ctime = DateTime.Now;
                    op.Location = loc;
                    await _session.SaveAsync(op);

                    await _eventBus.FireEventAsync(EventTypes.LocationInboundDisabled, loc);
                    affected++;
                }
            }

            _ = await HttpContext.SaveOpAsync("将 {0} 个位置设为允许出站。", affected);

            return this.Success("操作成功");
        }


        [HttpPost]
        [Route("get-side-view-data")]
        [OperationType(OperationTypes.查看位置)]
        [AutoTx]
        public async Task<ActionResult> GetSideViewData(GetSideViewDataArgs args)
        {
            AutoTxAttribute.BuildResultOnError(HttpContext, ex =>
            {
                string errMsg = "获取侧视图数据出错。" + ex.Message;
                return this.Error(errMsg);
            });

            if (args.LanewayCode == null)
            {
                throw new InvalidOperationException("未指定参数 LanewayCode。");
            }

            Laneway laneway = await _session.Query<Laneway>().Where(x => x.LanewayCode == args.LanewayCode).SingleOrDefaultAsync();
            object obj = new
            {
                args.LanewayCode,
                laneway.Offline,
                laneway.OfflineComment,
                racks = laneway.Racks.Select(rack => new
                {
                    rack.RackCode,
                    Side = rack.Side.ToString(),
                    rack.Columns,
                    rack.Levels,
                    rack.Deep,
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
                    Locations = rack.Locations.Select(loc => new
                    {
                        loc.LocationId,
                        loc.LocationCode,
                        Loaded = loc.UnitloadCount > 0,
                        loc.Level,
                        loc.Column,
                        loc.InboundDisabled,
                        loc.InboundDisabledComment,
                        loc.InboundCount,
                        loc.InboundLimit,
                        loc.OutboundDisabled,
                        loc.OutboundDisabledComment,
                        loc.OutboundLimit,
                        loc.OutboundCount,
                        loc.Specification,
                        loc.StorageGroup,
                        loc.WeightLimit,
                        loc.HeightLimit,
                        loc.Exists,
                        loc.Cell.i1,
                        loc.Cell.o1,
                        loc.Cell.i2,
                        loc.Cell.o2,
                        loc.Cell.i3,
                        loc.Cell.o3,
                        // 物料、批号、
                    })
                }),

            };

            return this.Ok(new
            {
                success = true,
                sideViewData = obj
            });

        }


        [HttpPost]
        [Route("create-k")]
        [OperationType(OperationTypes.创建关键点)]
        [AutoTx]
        public async Task<ActionResult> CreateKAsync(CreateKArgs args)
        {
            AutoTxAttribute.BuildResultOnError(HttpContext, ex =>
            {
                string errMsg = "创建关键点时出错。" + ex.Message;
                return this.Error(errMsg);
            });

            Location loc = _factory.CreateLocation(args.LocationCode, LocationTypes.K, null, 0, 0);
            loc.RequestType = args.RequestType;
            loc.OutboundLimit = args.OutboundLimit;
            loc.InboundLimit = args.InboundLimit;
            loc.Tag = args.Tag;
            await _session.SaveAsync(loc);
            _ = await HttpContext.SaveOpAsync("创建关键点成功，【{0}#{1}】。", loc.LocationCode, loc.LocationId);
            await _eventBus.FireEventAsync("KeyPointChanged", null);

            return this.Success("创建关键点成功");
        }

        [HttpPost]
        [Route("edit-k")]
        [OperationType(OperationTypes.编辑关键点)]
        [AutoTx]
        public async Task<ActionResult> EditKAsync(EditKArgs args)
        {
            AutoTxAttribute.BuildResultOnError(HttpContext, ex =>
            {
                string errMsg = "编辑关键点时出错。" + ex.Message;
                return this.Error(errMsg);
            });

            Location loc = await _session.GetAsync<Location>(args.LocationId);
            if (loc == null)
            {
                throw new InvalidOperationException($"位置不存在，【#{args.LocationId}】。");
            }
            loc.LocationCode = args.LocationCode;
            loc.RequestType = args.RequestType;
            loc.OutboundLimit = args.OutboundLimit;
            loc.InboundLimit = args.InboundLimit;
            loc.Tag = args.Tag;
            await _session.UpdateAsync(loc);
            _ = await HttpContext.SaveOpAsync("编辑关键点成功，【{0}#{1}】。", loc.LocationCode, loc.LocationId);

            await _eventBus.FireEventAsync("KeyPointChanged", null);

            return this.Success("创建关键点成功");
        }

        /// <summary>
        /// 重建巷道的统计信息。原有统计信息将被清除。此操作占用资源较多，不应频繁调用。
        /// </summary>
        private async Task RebuildLanewayStatAsync(Laneway laneway)
        {
            if (laneway == null)
            {
                throw new ArgumentNullException(nameof(laneway));
            }

            laneway.Usage.Clear();

            var keys = _session.Query<Laneway>()
                .Where(x => x == laneway)
                .SelectMany(x => x.Racks)
                .SelectMany(x => x.Locations)
                .Where(x => x.Exists)
                .GroupBy(x => new
                {
                    x.StorageGroup,
                    x.Specification,
                    x.WeightLimit,
                    x.HeightLimit
                })
                .Select(x => new LanewayUsageKey
                {
                    StorageGroup = x.Key.StorageGroup,
                    Specification = x.Key.Specification,
                    WeightLimit = x.Key.WeightLimit,
                    HeightLimit = x.Key.HeightLimit
                });

            foreach (var key in keys)
            {
                await UpdateUsageAsync(laneway, key);
            }

            async Task UpdateUsageAsync(Laneway laneway, LanewayUsageKey key)
            {
                var q = _session.Query<Laneway>()
                    .Where(x => x == laneway)
                    .SelectMany(x => x.Racks)
                    .SelectMany(x => x.Locations)
                    .Where(x => x.Exists
                        && x.StorageGroup == key.StorageGroup
                        && x.Specification == key.Specification
                        && x.WeightLimit == key.WeightLimit
                        && x.HeightLimit == key.HeightLimit
                    );

                var total = q
                    .ToFutureValue(fq => fq.Count());

                var loaded = q
                    .Where(x => x.UnitloadCount > 0)
                    .ToFutureValue(fq => fq.Count());

                var available = q
                    .Where(x =>
                        x.UnitloadCount == 0
                        && x.InboundDisabled == false)
                    .ToFutureValue(fq => fq.Count());

                var inboundDisabled = q
                    .Where(x => x.InboundDisabled == true)
                    .ToFutureValue(fq => fq.Count());

                var outboundDisabled = q
                    .Where(x => x.OutboundDisabled == true)
                    .ToFutureValue(fq => fq.Count());

                laneway.Usage[key] = new LanewayUsageData
                {
                    mtime = DateTime.Now,
                    Total = await total.GetValueAsync(),
                    Available = await available.GetValueAsync(),
                    Loaded = await loaded.GetValueAsync(),
                    InboundDisabled = await inboundDisabled.GetValueAsync(),
                };
            }
        }

    }

}
