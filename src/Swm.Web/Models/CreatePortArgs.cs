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

using System.ComponentModel.DataAnnotations;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 创建出口的操作参数
    /// </summary>
    public class CreatePortArgs
    {
        /// <summary>
        /// 出口编码
        /// </summary>
        [Required]
        public string PortCode { get; set; } = default!;

        /// <summary>
        /// 关键点一的编码
        /// </summary>
        [Required]
        public string KP1 { get; set; } = default!;

        /// <summary>
        /// 关键点二的编码，可以为 null
        /// </summary>
        public string? KP2 { get; set; }
    }

}
