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

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 巷道使用数据
    /// </summary>
    public class LanewayUsageInfo
    {
        /// <summary>
        /// 货位存储分组
        /// </summary>
        public string StorageGroup { get; set; } = default!;

        /// <summary>
        /// 货位规格
        /// </summary>
        public string Specification { get; set; } = default!;

        /// <summary>
        /// 货位限重
        /// </summary>
        public decimal WeightLimit { get; set; }

        /// <summary>
        /// 货位限高
        /// </summary>
        public decimal HeightLimit { get; set; }

        /// <summary>
        /// 总货位数
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 当前可用的货位数
        /// </summary>
        public int Available { get; set; }

        /// <summary>
        /// 当前有货的货位数
        /// </summary>
        public int Loaded { get; set; }

        /// <summary>
        /// 当前已禁止入站的货位数
        /// </summary>
        public int InboundDisabled { get; set; }

    }

}
