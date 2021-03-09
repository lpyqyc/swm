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

namespace Swm.TransportTasks
{
    public enum FailtoBuildTaskReason
    {
        /// <summary>
        /// 生成任务失败，因为货载正在移动。
        /// </summary>
        UnitloadBeingMoved,

        /// <summary>
        /// 生成任务失败，因为起点和终点相同。
        /// </summary>
        StartAndEndArdTheSame,

        /// <summary>
        /// 生成任务失败，因为巷道不是自动化的。
        /// </summary>
        LanewayNotAutomated,

        /// <summary>
        /// 生成任务失败，因为巷道已脱机。
        /// </summary>
        LanewayOffline,

        /// <summary>
        /// 生成任务失败，因为起点或终点被标记为不存在。
        /// </summary>
        LocationNotExists,

        /// <summary>
        /// 生成任务失败，因为起点已禁止出站。
        /// </summary>
        OutboundDisabled,

        /// <summary>
        /// 生成任务失败，因为起点已达到出站数限制。
        /// </summary>
        OutboundLimitReached,

        /// <summary>
        /// 生成任务失败，因为终点已禁止入站。
        /// </summary>
        InboundDisabled,

        /// 生成任务失败，因为终点已达到入站数限制。
        InboundLimitReached,

        /// <summary>
        /// 生成任务失败，因为双深位干涉，例如一深有货挡住二深货位的任务。
        /// </summary>
        DoubleDeepInterference,

        /// <summary>
        /// 生成任务失败，因为使用 N 位置作为WCS任务的起点或终点。
        /// </summary>
        NForWcsTask,

        /// <summary>
        /// 起点无效：
        /// 货载在N位置上，但起点是非K位置
        /// - 或 -
        /// 货载不在N位置上，但起点与货载当前位置不同
        /// </summary>
        InvalidStart,

    }

}
