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

using System;
using System.Collections.Generic;

namespace Swm.Web
{
    /// <summary>
    /// 提供为操作类型获取授权的方法。
    /// </summary>
    public interface IOperaionTypeAuthoriztion
    {
        /// <summary>
        /// 指定操作类型，获取允许执行此操作的角色。
        /// </summary>
        /// <param name="operationType"></param>
        /// <returns></returns>
        List<string> GetAllowedRoles(string operationType);

    }
}
