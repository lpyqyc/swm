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
using Arctic.NHibernateExtensions;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using NHibernate.Linq;
using Serilog;
using Swm.Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 提供任务 api
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TskController : ControllerBase
    {
        readonly ILogger _logger;
        readonly ISession _session;
        readonly TaskHelper _taskHelper;
        readonly OpHelper _opHelper;

        public TskController(ISession session, TaskHelper taskHelper, OpHelper opHelper, ILogger logger)
        {
            _logger = logger;
            _taskHelper = taskHelper;
            _opHelper = opHelper;
            _session = session;
        }

        /// <summary>
        /// 任务列表
        /// </summary>
        /// <param name="args">查询参数</param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet("get-task-list")]
        [OperationType(OperationTypes.查看任务)]
        public async Task<ListData<TaskInfo>> GetTaskList([FromQuery]TaskListArgs args)
        {
            var pagedList = await _session.Query<TransportTask>().SearchAsync(args, "TaskId DESC", 1, 999);

            return this.ListData(pagedList, x => new TaskInfo
            {
                TaskId = x.TaskId,
                TaskCode = x.TaskCode,
                TaskType = x.TaskType,
                PalletCode = x.Unitload.PalletCode,
                StartLocationCode = x.Start.LocationCode,
                EndLocationCode = x.End.LocationCode,
                SendTime = x.SendTime,
                OrderCode = x.OrderCode,
                Comment = x.Comment,
                Items = x.Unitload.Items.Select(i => DtoConvert.ToUnitloadItemInfo(i)).ToList(),
            });
        }

        /// <summary>
        /// 历史任务列表
        /// </summary>
        /// <param name="args">查询参数</param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet("get-archived-task-list")]
        [OperationType(OperationTypes.查看任务)]
        public async Task<ListData<ArchivedTaskInfo>> GetArchivedTaskList([FromQuery]ArchivedTaskListArgs args)
        {
            if (args.Sort == null)
            {
                args.Sort = "TaskId DESC";
            }

            var pagedList = await _session.Query<ArchivedTransportTask>().SearchAsync(args, args.Sort, args.Current, args.PageSize);

            return this.ListData(pagedList, x => new ArchivedTaskInfo
            {
                TaskId = x.TaskId,
                TaskCode = x.TaskCode,
                TaskType = x.TaskType,
                PalletCode = x.Unitload.PalletCode,
                StartLocationCode = x.Start.LocationCode,
                EndLocationCode = x.End.LocationCode,
                SendTime = x.SendTime,
                OrderCode = x.OrderCode,
                Comment = x.Comment,
                ArchivedAt = x.ArchivedAt,
                Cancelled = x.Cancelled,
                Items = x.Unitload.Items.Select(i => DtoConvert.ToUnitloadItemInfo(i)).ToList(),
            });
        }

        /// <summary>
        /// 更改货载位置
        /// </summary>
        /// <param name="palletCode">要更改位置的托盘号</param>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [OperationType(OperationTypes.更改货载位置)]
        [HttpPost("change-unitload-location/{palletCode}")]
        public async Task<ApiData> ChangeUnitloadLocation(string palletCode, ChangeLocationArgs args)
        {
            Unitload unitload = await _session.Query<Unitload>().Where(x => x.PalletCode == palletCode).SingleOrDefaultAsync();

            if (unitload == null)
            {
                throw new InvalidOperationException("托盘号不存在。");
            }

            Location dest = await _session.Query<Location>().Where(x => x.LocationCode == args.DestinationLocationCode).SingleOrDefaultAsync();
            if (dest == null)
            {
                throw new Exception("货位号不存在。");
            }

            var originalLocationCode = unitload.CurrentLocation?.LocationCode;
            if (originalLocationCode == null)
            {
                originalLocationCode = Cst.None;
            }

            var archived = await _taskHelper.ChangeUnitloadsLocationAsync(unitload, dest, args.Comment + string.Format("user: {0}", this.User?.Identity?.Name ?? "-"));

            _ = await _opHelper.SaveOpAsync("任务号 {0}", archived.TaskCode);

            _logger.Information("已将托盘 {palletCode} 的位置从 {originalLocationCode} 改为 {destinationLocationCode}", palletCode, originalLocationCode, args.DestinationLocationCode);

            return this.Success();
        }

    }
}
