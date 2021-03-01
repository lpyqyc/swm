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
    /// 使巷道脱机和联机的操作参数
    /// </summary>
    public class TakeOfflineArgs
    {
        /// <summary>
        /// 脱机或联机的操作备注。联机也需要填写操作备注，操作鼠标偶尔会出现手抖连续误点击的情况，要求填写操作备注，使强制用户使用键盘的安全措施。
        /// </summary>
        [Required]
        public string Comment { get; set; } = default!;
    }
}
