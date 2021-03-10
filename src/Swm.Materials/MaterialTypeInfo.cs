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

namespace Swm.Materials
{
    /// <summary>
    /// 物料类型信息
    /// </summary>
    public class MaterialTypeInfo
    {
        /// <summary>
        /// 物料类型
        /// </summary>
        [Required]
        public string MaterialType { get; set; } = default!;

        /// <summary>
        /// 物料类型说明
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 适用范围
        /// </summary>
        public string? Scope { get; set; }

        /// <summary>
        /// 展示顺序
        /// </summary>
        public int DisplayOrder { get; set; }

    }

}
