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
    public class TasksController : ControllerBase
    {
        readonly ILogger _logger;
        readonly ISession _session;

        public TasksController(ISession session, ILogger logger)
        {
            _logger = logger;
            _session = session;
        }


        [AutoTransaction]
        [HttpPost]
        [Route("list")]
        public async Task<TaskList> ListAsync(TaskListArgs args)
        {
            if (args.Sort == null || args.Sort.Count == 0)
            {
                args.Sort = new System.Collections.Specialized.OrderedDictionary
                {
                    { "TaskId", "DESC" }
                };
            }

            var pagedList = await _session.Query<TransportTask>().SearchAsync(args, args.Sort, args.Current, args.PageSize);

            return new TaskList
            {
                Success = true,
                Message = "OK",
                Data = pagedList.List.Select(x => new TaskListItem
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
                    Items = x.Unitload.Items.Select(i => new UnitloadItemInfo
                    {
                        UnitloadItemId = i.UnitloadItemId,
                        MaterialId = i.Material.MaterialId,
                        MaterialCode = i.Material.MaterialCode,
                        MaterialType = i.Material.MaterialType,
                        Description = i.Material.Description,
                        Specification = i.Material.Specification,
                        Batch = i.Batch,
                        StockStatus = i.StockStatus,
                        Quantity = i.Quantity,
                        Uom = i.Uom,
                    }).ToList(),
                }),
                Total = pagedList.Total,
            };
        }


        [AutoTransaction]
        [HttpPost]
        [Route("archived")]
        public async Task<ArchivedTaskList> ArchivedAsync(ArchivedTaskListArgs args)
        {
            if (args.Sort == null || args.Sort.Count == 0)
            {
                args.Sort = new System.Collections.Specialized.OrderedDictionary
                {
                    { "TaskId", "DESC" }
                };
            }

            var pagedList = await _session.Query<ArchivedTransportTask>().SearchAsync(args, args.Sort, args.Current, args.PageSize);

            return new ArchivedTaskList
            {
                Success = true,
                Message = "OK",
                Data = pagedList.List.Select(x => new ArchivedTaskListItem
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
                    Items = x.Unitload.Items.Select(i => new UnitloadItemInfo
                    {
                        UnitloadItemId = i.UnitloadItemId,
                        MaterialId = i.Material.MaterialId,
                        MaterialCode = i.Material.MaterialCode,
                        MaterialType = i.Material.MaterialType,
                        Description = i.Material.Description,
                        Specification = i.Material.Specification,
                        Batch = i.Batch,
                        StockStatus = i.StockStatus,
                        Quantity = i.Quantity,
                        Uom = i.Uom,
                    }).ToList(),
                }),
                Total = pagedList.Total,
            };
        }

    }
}
