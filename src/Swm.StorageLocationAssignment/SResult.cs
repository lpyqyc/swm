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

using Swm.Locations;

namespace Swm.StorageLocationAssignment
{
    // TODO 改为记录
    /// <summary>
    /// 表示单叉巷道分配货位的结果，只包含一个货位。
    /// </summary>
    public sealed class SResult
    {
        public static readonly SResult Failure = new SResult(false, null);
        private SResult(bool success, Location target)
        {
            this.Success = success;
            this.Target = target;
        }
        /// <summary>
        /// 指示分配是否成功。
        /// </summary>
        public bool Success { get; private set; }

        /// <summary>
        /// 若分配成功，包含分配到的货位。
        /// </summary>
        public Location Target { get; private set; }

        public static SResult MakeSuccess(Location first)
        {
            return new SResult(true, first);
        }
    }

}
