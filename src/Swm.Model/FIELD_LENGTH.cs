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

namespace Swm.Model
{
    /// <summary>
    /// 提供字段长度常数。
    /// </summary>
    public static class FIELD_LENGTH
    {
        /// <summary>
        /// 用户名字段的长度。
        /// </summary>
        public const int USERNAME = 30;

        /// <summary>
        /// 角色名字段的长度。
        /// </summary>
        public const int ROLENAME = 30;

        /// <summary>
        /// 操作类型字段的长度。
        /// </summary>
        public const int OPERATION_TYPE = 20;


        public const int APP_CODE = 20;

        /// <summary>
        /// 计量单位字段的长度。
        /// </summary>
        public const int UOM = 8;

        /// <summary>
        /// 批号字段的长度。
        /// </summary>
        public const int BATCH = 20;

        /// <summary>
        /// 库存状态字段的长度
        /// </summary>
        public const int STOCK_STATUS = 10;
    }
}
