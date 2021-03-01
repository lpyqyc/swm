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

using NHibernate;
using Serilog;
using Swm.Model.StorageLocationAssignment;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Swm.Model.Extentions
{
    public class 上架请求处理程序 : IRequestHandler
    {

        readonly TaskHelper _taskHelper;

        readonly ITaskSender _taskSender;

        readonly ISession _session;

        readonly SAllocationHelper _sallocHelper;

        readonly ILogger _logger;

        public 上架请求处理程序(ISession session, TaskHelper taskHelper, ITaskSender taskSender, SAllocationHelper sallocHelper, ILogger logger)
        {
            _session = session;
            _taskHelper = taskHelper;
            _taskSender = taskSender;
            _sallocHelper = sallocHelper;
            _logger = logger;
        }


        public async Task ProcessRequestAsync(RequestInfo requestInfo)
        {
            _logger.Debug(requestInfo.ToString());
            CheckRequest(requestInfo);

            // 1 入口
            string entranceLocationCode = requestInfo.LocationCode;
            var entrance = await _session.Query<Location>().GetAsync(entranceLocationCode).ConfigureAwait(false);
            if (entrance == null)
            {
                string msg = string.Format("请求位置在 Wms 中不存在。【{0}】。", entranceLocationCode);
                throw new InvalidRequestException(msg);
            }

            // 2 托盘
            string containerCode = requestInfo.PalletCode;
            var unitload = await _session.Query<Unitload>().GetAsync(containerCode).ConfigureAwait(false);
            if (unitload == null)
            {
                string msg = string.Format("货载不存在。容器编码【{0}】。", containerCode);
                throw new InvalidRequestException(msg);
            }

            // 将请求中的高度和重量记录到货载
            unitload.StorageInfo.Height = requestInfo.Height;
            unitload.StorageInfo.Weight = requestInfo.Weight;

            // 3 分配货位
            SResult s = SResult.Failure;
            var laneways = _session.Query<Laneway>().Take(5).ToArray();
            foreach (var laneway in laneways)
            {
                _logger.Debug("正在检查巷道 {lanewayCode}", laneway.LanewayCode);
                if (laneway.Offline)
                {
                    _logger.Warning("跳过脱机的巷道 {lanewayCode}", laneway.LanewayCode);
                    continue;
                }

                s = await _sallocHelper.AllocateAsync(laneway, unitload.StorageInfo).ConfigureAwait(false);

                if (s.Success)
                {
                    _logger.Information("在 {lanewayCode} 分配到货位 {locationCode}", laneway.LanewayCode, s.Target.LocationCode);
                    break;
                }
                else
                {
                    _logger.Information("在 {lanewayCode} 未分配到货位。", laneway.LanewayCode);
                    continue;
                }
            }
            if (s.Success == false)
            {
                // 分配货位失败
                throw new Exception("未分配到货位。");
            }

            // 4 生成任务
            var task = new TransportTask();
            string taskType = "上架";
            await _taskHelper.BuildAsync(task, taskType, entrance, s.Target, unitload);

            // 5 下发任务
            _taskSender.SendTask(task);
        }

        public virtual void CheckRequest(RequestInfo requestInfo)
        {
            if (string.IsNullOrWhiteSpace(requestInfo.LocationCode))
            {
                throw new InvalidRequestException("请求信息中应提供请求位置。");
            }

            if (string.IsNullOrWhiteSpace(requestInfo.PalletCode))
            {
                throw new InvalidRequestException("请求中未提供容器编码。");
            }

            if (requestInfo.Height < 0m)
            {
                throw new InvalidRequestException("请求中提供的 Height 值无效，应大于等于 0。");
            }

            if (requestInfo.Weight < 0m)
            {
                throw new InvalidRequestException("请求中提供的 Weight 值无效，应大于等于 0。");
            }
        }

    }

}
