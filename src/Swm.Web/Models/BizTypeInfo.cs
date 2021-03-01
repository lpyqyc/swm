﻿// Copyright 2020-2021 王建军
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

namespace Swm.Web
{
    /// <summary>
    /// 表示业务类型选项
    /// </summary>
    public class BizTypeInfo
    {
        /// <summary>
        /// 业务类型
        /// </summary>
        public string BizType { get; set; } = default!;

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 适用范围
        /// </summary>
        public string? Scope { get; set; }

        /// <summary>
        /// 展示次序
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}
