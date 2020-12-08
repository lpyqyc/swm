using Arctic.NHibernateExtensions;
using Arctic.NHibernateExtensions.AspNetCore;
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
        readonly ILogger _logger;

        public LanewaysController(ISession session, OpHelper opHelper, ILogger logger)
        {
            _session = session;
            _opHelper = opHelper;
            _logger = logger;
        }

        /// <summary>
        /// 巷道列表
        /// </summary>
        /// <param name="args">查询参数</param>
        /// <returns></returns>
        [HttpPost]
        [DebugShowArgs]
        [AutoTransaction]
        [OperationType(OperationTypes.巷道列表)]
        [Route("list")]
        public async Task<LanewayList> List(LanewayListArgs args)
        {
            var pagedList = await _session.Query<Laneway>().ToPagedListAsync(args);
            return new LanewayList
            {
                Success = true,
                Message = "OK",                
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
                        .Select(x => new LanewayListItem.PortInfo { 
                            PortId = x.PortId, 
                            PortCode = x.PortCode })
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
        public async Task<List<LanewaySelectListItem>> SelectListAsync()
        {
            var items = await _session.Query<Laneway>()
                .Select(x => new LanewaySelectListItem
                {
                    LanewayId = x.LanewayId,
                    LanewayCode = x.LanewayCode,
                    Offline = x.Offline,
                })
                .WrappedToListAsync();
            return items;
        }

        /// <summary>
        /// 脱机巷道
        /// </summary>
        /// <param name="id">巷道id</param>
        /// <param name="args">参数</param>
        /// <returns></returns>
        [HttpPost]
        [Route("take-offline/{id}")]
        [OperationType(OperationTypes.脱机巷道)]
        [AutoTransaction]
        public async Task<ActionResult<OperationResult>> TakeOfflineAsync(int id, [FromBody]TakeOfflineArgs args)
        {
            HttpContext.SetResultFactoryOnError(ex =>
            {
                string errMsg = "使巷道脱机时出错。" + ex.Message;
                return Ok(new OperationResult
                {
                    Success = false,
                    Message = errMsg,
                });
            });

            Laneway laneway = await _session.GetAsync<Laneway>(id);
            if (laneway == null)
            {
                throw new InvalidOperationException($"巷道不存在#{id}。");
            }
            if (laneway.Offline == true)
            {
                throw new InvalidOperationException($"巷道已处于脱机状态。【{laneway.LanewayCode}】");
            }

            laneway.Offline = true;
            laneway.TakeOfflineTime = DateTime.Now;
            laneway.OfflineComment = args.Comment;
            await _session.UpdateAsync(laneway);
            _ = await _opHelper.SaveOpAsync($"巷道【{laneway.LanewayCode}】，备注【{args.Comment}】。");
            _logger.Information("已将巷道 {lanewayCode} 脱机", laneway.LanewayCode);

            return Ok(new OperationResult {
                Success = true,
                Message = "操作成功",
            });
        }


        /// <summary>
        /// 联机巷道
        /// </summary>
        /// <param name="id"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("take-online/{id}")]
        [OperationType(OperationTypes.联机巷道)]
        [AutoTransaction]
        public async Task<ActionResult<OperationResult>> TakeOnlineAsync(int id, [FromBody]TakeOnlineArgs args)
        {
            HttpContext.SetResultFactoryOnError(ex =>
            {
                string errMsg = "使巷道联机时出错。" + ex.Message;
                return Ok(new OperationResult
                {
                    Success = false,
                    Message = errMsg,
                });
            });

            Laneway laneway = await _session.GetAsync<Laneway>(id);
            if (laneway == null)
            {
                throw new InvalidOperationException($"巷道不存在#{id}。");
            }

            if (laneway.Offline == false)
            {
                throw new InvalidOperationException($"巷道已处于联机状态。【{laneway.LanewayCode}】");
            }

            laneway.Offline = false;
            laneway.TotalOfflineHours += DateTime.Now.Subtract(laneway.TakeOfflineTime).TotalHours;
            laneway.OfflineComment = args.Comment;
            await _session.UpdateAsync(laneway);
            _ = await _opHelper.SaveOpAsync($"巷道【{laneway.LanewayCode}】，备注【{args.Comment}】。");
            _logger.Information("已将巷道 {lanewayCode} 联机", laneway.LanewayCode);

            return Ok(new OperationResult
            {
                Success = true,
                Message = "操作成功",
            });
        }


        /// <summary>
        /// 重建所有巷道的统计信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("rebuild-stat")]
        [OperationType(OperationTypes.重建巷道统计信息)]
        [AutoTransaction]
        public async Task<ActionResult> RebuildLanewaysStatAsync()
        {
            var laneways = await _session.Query<Laneway>().WrappedToListAsync();
            foreach (var laneway in laneways)
            {
                await RebuildLanewayStatAsync(laneway);
            }
            return Ok(new OperationResult
            {
                Success = true,
                Message = "操作成功",
            });
        }

        /// <summary>
        /// 设置巷道可以到达的出口
        /// </summary>
        /// <param name="id"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("set-ports/{id}")]
        [OperationType(OperationTypes.设置出口)]
        [AutoTransaction]
        public async Task<ActionResult> SetLanewaysPortsAsync(int id, [FromBody]SetPortsArgs args)
        {
            HttpContext.SetResultFactoryOnError(ex =>
            {
                string errMsg = "设置出货口时出错。" + ex.Message;
                return Ok(new OperationResult
                {
                    Success = false,
                    Message = errMsg,
                });
            });

            Laneway laneway = await _session.GetAsync<Laneway>(id);

            laneway.Ports.Clear();
            foreach (var portId in args.PortIdList)
            {
                Port port = await _session.GetAsync<Port>(portId);
                laneway.Ports.Add(port);
            }

            var op = await _opHelper.SaveOpAsync("巷道【{0}】，{1} 个出货口。", laneway.LanewayCode, laneway.Ports.Count);
            _logger.Information("设置出货口成功，巷道：{lanewayCode}。", laneway.LanewayCode);

            return Ok(new OperationResult
            {
                Success = true,
                Message = "操作成功",
            });
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
