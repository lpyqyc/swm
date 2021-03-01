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

using System;

namespace Swm.Model
{
    //  TODO 整理
    /// <summary>
    /// 封装常量。
    /// </summary>
    [Obsolete]
    public static class Cst
    {
        /// <summary>
        /// 表示空值。
        /// </summary>
        public const string None = "None";

        /// <summary>
        /// 表示不适用。
        /// </summary>
        public const string NA = "NA";


        public const string 更改位置 = "更改位置";

        /// <summary>
        /// 表示默认的存储分组。
        /// </summary>
        public const string DefaultStorageGroup = "普通";

        /// <summary>
        /// 若字符串与 <see cref="Cst.None"/> 相等，返回 true，忽略大小写。
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNone(this string str)
        {
            return string.Equals(str, Cst.None, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// 若字符串与 <see cref="Cst.None"/> 不相等，返回 true，忽略大小写。
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNotNone(this string str)
        {
            return str.IsNone() == false;
        }
    }

}
