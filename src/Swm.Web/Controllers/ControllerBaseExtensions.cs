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

using Microsoft.AspNetCore.Mvc;

namespace Swm.Web.Controllers
{
    public static class ControllerBaseExtensions
    {
        /// <summary>
        /// 表示操作成功。http 状态码 201。
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ObjectResult Success(this ControllerBase controller, object? value = null)
        {
            return controller.StatusCode(201, value);
        }
    }

}
