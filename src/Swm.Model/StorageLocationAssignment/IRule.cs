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

using System.Threading.Tasks;

namespace Swm.Model.StorageLocationAssignment
{
    public interface IRule
    {
        /// <summary>
        /// 获取此规则的名称。
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 获取此规则是否适用于双深巷道。
        /// </summary>
        bool DoubleDeep { get; }

        /// <summary>
        /// 获取此规则的排序。
        /// </summary>
        int Order { get; }

        /// <summary>
        /// 在巷道内选择货位。
        /// </summary>
        /// <param name="laneway"></param>
        /// <param name="cargoInfo"></param>
        /// <param name="excludedIdList"></param>
        /// <param name="excludedColumnList"></param>
        /// <param name="excludedLevelList"></param>
        /// <param name="orderBy"></param>
        /// <returns>返回 Location，表示分配到的目标货位。若分配失败，返回 null。</returns>
        Task<Location> SelectAsync(
            Laneway laneway,
            UnitloadStorageInfo cargoInfo,
            int[] excludedIdList,
            int[] excludedColumnList,
            int[] excludedLevelList,
            string orderBy
            );
    }

}
