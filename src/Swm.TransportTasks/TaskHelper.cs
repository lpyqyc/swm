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

using Arctic.AppSeqs;
using NHibernate;
using Serilog;
using Swm.Locations;
using Swm.Palletization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Swm.TransportTasks
{
    /// <summary>
    ///  任务生成、完成、取消时，货位状态维护。
    /// </summary>
    public sealed class TaskHelper
    {
        readonly ISession _session;

        readonly ILogger _logger;

        readonly IAppSeqService _appSeqService;
        readonly UnitloadSnapshopHelper _unitloadSnapshopHelper;

        public TaskHelper(ISession session, IAppSeqService appSeqService, UnitloadSnapshopHelper unitloadSnapshopHelper, ILogger logger)
        {
            _session = session;
            _appSeqService = appSeqService;
            _unitloadSnapshopHelper = unitloadSnapshopHelper;
            _logger = logger;
        }


        /// <summary>
        /// 生成任务，并保存到数据库。
        /// </summary>
        /// <param name="transTask">空白的、尚未保存到数据库的任务对象。</param>
        /// <param name="start">起点。</param>
        /// <param name="end">终点。</param>
        /// <param name="unitload">要移动的货载。</param>
        /// <param name="taskType">任务类型</param>
        /// <param name="forWcs">是否是为 wcs 准备的。若为 true，则会在生成任务指令之前执行一些检查，例如，货位是否存在，详细的检查规则见 <see cref="CheckForWcs(Location, Location, IEnumerable{Unitload})"/> 方法。</param>
        public async Task BuildAsync(TransportTask transTask, string taskType, Location start, Location end, Unitload unitload, bool forWcs = true)
        {
            if (transTask == null)
            {
                throw new ArgumentNullException(nameof(transTask));
            }

            if (IsBlank(transTask) == false)
            {
                throw new ArgumentException($"transTask 实参不是空白对象，其 Id 值为 {transTask.TaskId}。");
            }

            if (string.IsNullOrWhiteSpace(taskType))
            {
                throw new ArgumentNullException(nameof(taskType));
            }

            if (start == null)
            {
                throw new ArgumentNullException(nameof(start));
            }

            if (end == null)
            {
                throw new ArgumentNullException(nameof(end));
            }

            if (unitload == null)
            {
                throw new ArgumentNullException(nameof(unitload));
            }

            if (start == end)
            {
                throw new FailToBuildTaskException(FailtoBuildTaskReason.StartAndEndArdTheSame);
            }


            // 检查货载
            if (unitload.BeingMoved)
            {
                throw new FailToBuildTaskException(FailtoBuildTaskReason.UnitloadBeingMoved);
            }

            // 检查
            if (forWcs)
            {
                _logger.Debug("这是 WCS 任务，进行生成前检查");
                this.CheckForWcs(start, end, unitload);
                _logger.Debug("检查 WCS 任务通过");
            }

            _logger.Information("正在生成任务");

            // 生成
            transTask.TaskCode = await GetNextTaskCode().ConfigureAwait(false);
            transTask.TaskType = taskType;
            transTask.Start = start;
            transTask.End = end;
            transTask.ForWcs = forWcs;
            transTask.Unitload = unitload;
            await _session.SaveAsync(transTask);

            unitload.BeingMoved = true;
            unitload.CurrentTask = transTask;

            // 更新货位
            start.OutboundCount++;
            end.InboundCount++;
            _logger.Information("已生成任务，任务号 {taskCode}, 任务类型 {taskType}", transTask.TaskCode, transTask.TaskType);

            bool IsBlank(TransportTask task)
            {
                return task.TaskId == 0
                    && task.TaskCode == null
                    && task.TaskType == null
                    && task.Unitload == null
                    && task.Start == null
                    && task.End == null;
            }
        }


        /// <summary>
        /// 下发前检查，确保指令可以下发。
        /// 检查的内容有：
        /// 1，指令已保存到数据库；
        /// 2，指令是为 Wcs 生成的；
        /// 3，指令尚未下发；
        /// </summary>
        void CheckBeforeSend(TransportTask transTask)
        {
            if (transTask.TaskId == 0)
            {
                throw new InvalidOperationException("未能下发任务，因为它尚未保存。");
            }

            if (transTask.ForWcs == false)
            {
                throw new InvalidOperationException("未能下发任务，因为它不是为 Wcs 生成的。");
            }

            if (transTask.WasSentToWcs)
            {
                throw new InvalidOperationException("未能下发任务，因为它已下发过。");
            }
        }

        /// <summary>
        /// 在生成任务对象前进行检查。
        /// </summary>
        /// <param name="start">任务起点。</param>
        /// <param name="end">任务终点。</param>
        /// <param name="unitload">要移动的货载。</param>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        void CheckForWcs(Location start, Location end, Unitload unitload)
        {
            // 检查起点
            if (start.Exists == false)
            {
                throw new FailToBuildTaskException(FailtoBuildTaskReason.LocationNotExists);
            }
            if (start.LocationType == LocationTypes.N)
            {
                throw new FailToBuildTaskException(FailtoBuildTaskReason.NForWcsTask);
            }

            if (start.OutboundDisabled)
            {
                throw new FailToBuildTaskException(FailtoBuildTaskReason.OutboundDisabled);
            }

            if (start.OutboundCount >= start.OutboundLimit)
            {
                throw new FailToBuildTaskException(FailtoBuildTaskReason.OutboundLimitReached);
            }

            if (start?.Laneway?.Automated == false)
            {
                throw new FailToBuildTaskException(FailtoBuildTaskReason.LanewayNotAutomated);
            }

            if (start.LocationType == LocationTypes.S)
            {

                if (start.Laneway.Offline)
                {
                    throw new FailToBuildTaskException(FailtoBuildTaskReason.LanewayOffline);
                }


                if (start.Deep == 2)
                {
                    Location deep1 = start.GetDeep1();

                    if (deep1.InboundCount > 0)
                    {
                        throw new FailToBuildTaskException(FailtoBuildTaskReason.DoubleDeepInterference);
                    }
                    if (deep1.Loaded())
                    {
                        throw new FailToBuildTaskException(FailtoBuildTaskReason.DoubleDeepInterference);
                    }
                }
            }

            // 检查货载当前位置和起点的关系
            if (unitload.CurrentLocation == null)
            {
                // noop
            }
            else if (unitload.CurrentLocation.LocationType == LocationTypes.N)
            {
                if (start.LocationType != LocationTypes.K)
                {
                    throw new FailToBuildTaskException(FailtoBuildTaskReason.InvalidStart);
                }
            }
            else
            {
                if (unitload.CurrentLocation != start)
                {
                    throw new FailToBuildTaskException(FailtoBuildTaskReason.InvalidStart);
                }
            }

            // 检查终点
            if (end.Exists == false)
            {
                throw new FailToBuildTaskException(FailtoBuildTaskReason.LocationNotExists);
            }

            if (end.LocationType == LocationTypes.N)
            {
                throw new FailToBuildTaskException(FailtoBuildTaskReason.NForWcsTask);
            }

            if (end.InboundDisabled)
            {
                throw new FailToBuildTaskException(FailtoBuildTaskReason.InboundDisabled);
            }

            if (end.InboundCount >= end.InboundLimit)
            {
                throw new FailToBuildTaskException(FailtoBuildTaskReason.InboundLimitReached);
            }

            if (end?.Laneway?.Automated == false)
            {
                throw new FailToBuildTaskException(FailtoBuildTaskReason.LanewayNotAutomated);
            }

            if (end.LocationType == LocationTypes.S)
            {
                if (end.Laneway.Offline)
                {
                    throw new FailToBuildTaskException(FailtoBuildTaskReason.LanewayOffline);
                }

                if (end.Deep == 2)
                {
                    Location deep1 = end.GetDeep1();

                    if (deep1.Loaded())
                    {
                        throw new FailToBuildTaskException(FailtoBuildTaskReason.DoubleDeepInterference);
                    }
                }
            }
        }


        /// <summary>
        /// 完成任务。
        /// </summary>
        /// <param name="transTask">要完成的任务。</param>
        /// <param name="actualEnd">货载实际到达的位置。</param>
        /// <param name="checkEnd">指示是否对终点进行检查。
        /// 设为 true 时，将检查终点的 <see cref="Location.UnitloadCount"/>，若大于 0，则抛出 InvalidOperationException 异常。
        /// 
        /// 出现此问题的典型场景是巷道口，底层可能会出现错误，
        /// 在前一个货载尚未发走时，将下一个移动提前报完成。
        /// 这种情况下，如果允许完成，将导致巷道口上存在两个先后到达的货载。
        /// 
        /// 到货架货位的任务不会出现此问题，因为在任务生成之前会执行分配操作，排除了有货的货位。
        /// </param>
        public async Task<ArchivedTransportTask> CompleteAsync(TransportTask transTask, Location actualEnd, bool checkEnd = true)
        {
            if (transTask == null)
            {
                throw new ArgumentNullException(nameof(transTask));
            }

            if (actualEnd == null)
            {
                throw new ArgumentNullException(nameof(actualEnd));
            }

            _logger.Debug("正在完成任务 {taskCode}，任务类型 {taskType}，完成位置 {actualEnd}", transTask.TaskCode, transTask.TaskType, actualEnd.LocationCode);

            if (checkEnd)
            {
                if (actualEnd.UnitloadCount - actualEnd.OutboundCount > 0)
                {
                    string errMsg = string.Format("不能完成任务，终点上有货。任务号【{0}】，实际完成位置【{1}】。", transTask, actualEnd);
                    throw new InvalidOperationException(errMsg);
                }
            }

            transTask.Unitload.LeaveCurrentLocation();
            transTask.Unitload.Enter(actualEnd);

            transTask.Unitload.BeingMoved = false;
            transTask.Unitload.CurrentTask = null;
            var archived = await ArchiveAsync(transTask, false, actualEnd);
            transTask.Start.OutboundCount--;
            transTask.End.InboundCount--;

            _logger.Information("任务已完成 {taskCode}", transTask.TaskCode);

            return archived;
        }


        /// <summary>
        /// 取消任务。
        /// </summary>
        /// <param name="transTask"></param>
        /// <param name="checkStart">
        /// 指示取消前是否对起点进行检查。
        /// 设为 true 时，将检查起点的 Keepings 集合中是否存在未移动的货载。
        /// 若存在，则抛出 InvalidOperationException 异常。
        /// 
        /// 出现此问题的典型场景是巷道口，堆垛机将巷道口上的货载取走后，
        /// 后面的货载会抵达巷道口，和巷道口形成 Keeping。
        /// 如果允许取消，将导致巷道口上存在两波货载。
        /// </param>
        public async Task<ArchivedTransportTask> CancelAsync(TransportTask transTask, bool checkStart = true)
        {
            if (transTask == null)
            {
                throw new ArgumentNullException(nameof(transTask));
            }

            _logger.Debug("正在取消任务 {taskCode}。", transTask.TaskCode);

            if (checkStart)
            {
                // 当托盘在 N 位置上时，当前位置和起点不同
                if (transTask.Unitload.CurrentLocation == transTask.Start)
                {
                    if (transTask.Start.UnitloadCount - transTask.Start.OutboundCount > 0)
                    {
                        string errMsg = string.Format("不能取消任务，起点上有其他货载。");
                        throw new InvalidOperationException(errMsg);
                    }
                }
            }

            transTask.Unitload.BeingMoved = false;
            transTask.Unitload.CurrentTask = null;
            var archived = await ArchiveAsync(transTask, true, transTask.Start);
            transTask.Start.OutboundCount--;
            transTask.End.InboundCount--;

            _logger.Information("已取消任务 {taskCode}", transTask.TaskCode);

            return archived;
        }


        /// <summary>
        /// 更改货载位置。会生成一条归档任务以方便在历史任务列表中查看货载位置的变动情况。
        /// </summary>
        /// <param name="unitload">要更改位置的货载。</param>
        /// <param name="dest">更改到的目标位置。</param>
        /// <param name="comment">更改的备注，将记在 ArchivedMove.Comments 属性中。</param>
        /// <param name="checkDest">指定是否检查目标位置。若为 true，
        /// 将检查目标位置的 <see cref="Location.UnitloadCount"/> 属性，若大于 0，则抛出 InvalidOperationException 异常。
        /// </param>
        public async Task<ArchivedTransportTask> ChangeUnitloadsLocationAsync(Unitload unitload, Location dest, string comment, bool checkDest = true)
        {
            if (unitload == null)
            {
                throw new ArgumentNullException(nameof(unitload));
            }

            if (dest == null)
            {
                throw new ArgumentNullException(nameof(dest));
            }

            _logger.Information("正在更改托盘 {palletCode} 的位置，目标是 {dest}", unitload.PalletCode, dest.LocationCode);

            if (unitload.BeingMoved)
            {
                throw new InvalidOperationException("无法更改货载位置，货载正在移动。");
            }

            if (checkDest)
            {
                if (dest.Loaded())
                {
                    throw new InvalidOperationException("无法更改货载位置，目标位置上已有货载。");
                }
            }

            Location orig = unitload.CurrentLocation;
            if (orig == null)
            {
                orig = await _session.Query<Location>().GetNAsync().ConfigureAwait(false);
            }

            // 创建任务并立即完成
            TransportTask transTask = new TransportTask();
            transTask.Comment = comment;
            await BuildAsync(transTask, Cst.更改位置, orig, dest, unitload, false);
            var archived = await CompleteAsync(transTask, dest, false);

            _logger.Information("已将托盘 {palletCode} 的位置改为 {dest}", unitload.PalletCode, dest.LocationCode);

            return archived;
        }


        private async Task<string> GetNextTaskCode()
        {
            DateTime today = DateTime.Now.Date;
            string seqName = string.Format("TaskCode-{0:yyMMdd}", today);
            int nextSn = await _appSeqService.GetNextAsync(seqName).ConfigureAwait(false);
            return $"T{today:yyMMdd}{nextSn:000000}";
        }

        public async Task<ArchivedTransportTask> ArchiveAsync(TransportTask transTask, bool cancelled, Location actualEnd)
        {
            _logger.Information("正在归档任务 {taskCode}", transTask.TaskCode);
            ArchivedTransportTask archived = new ArchivedTransportTask();
            archived.Unitload = _unitloadSnapshopHelper.GetSnapshot(transTask.Unitload);
            await _session.SaveAsync(archived.Unitload).ConfigureAwait(false);
            await _session.FlushAsync().ConfigureAwait(false);

            CopyUtil.CopyProperties(transTask, archived, new[]
            {
                nameof(ArchivedTransportTask.ArchivedAt),
                nameof(ArchivedTransportTask.Cancelled),
                nameof(ArchivedTransportTask.ActualEnd),
            });
            archived.ArchivedAt = DateTime.Now;
            archived.Cancelled = cancelled;
            archived.ActualEnd = actualEnd;

            await _session.DeleteAsync(transTask).ConfigureAwait(false);
            await _session.SaveAsync(archived).ConfigureAwait(false);

            _logger.Information("已归档任务 {taskCode}", transTask.TaskCode);
            return archived;
        }
    }
}
