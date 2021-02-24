﻿// Copyright 2020 王建军
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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 创建出库单操作的参数
    /// </summary>
    public class CreateOutboundOrderArgs
    {
        /// <summary>
        /// 业务类型
        /// </summary>
        [Required]
        public string BizType { get; set; } = default!;

        /// <summary>
        /// 业务单据号
        /// </summary>
        public string? BizOrder { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// 出库行信息
        /// </summary>
        public List<EditOutboundLineInfo> Lines { get; set; } = default!;
    }

}

