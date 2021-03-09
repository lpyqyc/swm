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

using NHibernate;
using System;

namespace Swm.StorageLocationAssignment
{
    /// <summary>
    /// 提供协助查询的扩展方法。
    /// </summary>
    internal static class NhQueryExtensions
    {
        /// <summary>
        /// 若 values 数组不为 null 且长度大于 0，则使用 paramName 和 values 为 q 设置参数列表。否则，不做任何操作。
        /// </summary>
        /// <param name="q">被扩展的查询对象。</param>
        /// <param name="paramName">参数名。</param>
        /// <param name="values">用作实参的数组。</param>
        public static IQuery EmptySafeSetParameterList(this IQuery q, string paramName, Array values)
        {
            if (q == null)
            {
                throw new ArgumentNullException(nameof(q));
            }

            if (string.IsNullOrWhiteSpace(paramName))
            {
                throw new ArgumentException(nameof(paramName));
            }

            if (values == null || values.Length == 0)
            {
                return q;
            }
            return q.SetParameterList(paramName, values);
        }


        /// <summary>
        /// 根据 values 是否有值决定是否保留子句。
        /// </summary>
        /// <param name="clause">子句字符串。</param>
        /// <param name="values">将用作 in 子句的实参数组。</param>
        /// <returns>返回一个字符串。若 values 数组为 null 或长度为 0，则返回 string.Empty，否则，将原字符串原样返回。</returns>
        public static string If(this string clause, Array values)
        {
            if (values == null || values.Length == 0)
            {
                return string.Empty;
            }
            return clause;
        }
    }

}
