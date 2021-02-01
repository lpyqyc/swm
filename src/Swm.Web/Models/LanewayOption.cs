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
    /// 表示巷道选项列表中的元素
    /// </summary>
    public class LanewayOption
    {
        /// <summary>
        /// 巷道Id
        /// </summary>
        public int LanewayId { get; init; }

        /// <summary>
        /// 巷道编码
        /// </summary>
        public string LanewayCode { get; init; } = default!;


        /// <summary>
        /// 是否离线
        /// </summary>
        public bool Offline { get; init; }

    }

    
}
