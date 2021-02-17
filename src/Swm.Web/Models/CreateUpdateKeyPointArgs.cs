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

using System.ComponentModel.DataAnnotations;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 创建和编辑关键点的操作参数
    /// </summary>
    public class CreateUpdateKeyPointArgs
    {
        /// <summary>
        /// 货位编号
        /// </summary>
        [Required]
        public string LocationCode { get; set; } = default!;

        /// <summary>
        /// 请求类型
        /// </summary>
        public string? RequestType { get; set; }

        /// <summary>
        /// 标记
        /// </summary>
        public string? Tag { get; set; }

        /// <summary>
        /// 入站数限制
        /// </summary>
        [Range(1, 999)]
        public int InboundLimit { get; set; }

        /// <summary>
        /// 出站数限制
        /// </summary>
        [Range(1, 999)]
        public int OutboundLimit { get; set; }
    }

}
