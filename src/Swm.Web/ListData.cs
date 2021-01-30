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

using System.Collections.Generic;

namespace Swm.Web
{
    /// <summary>
    /// 列表页数据
    /// </summary>
    /// <typeparam name="TElement"></typeparam>
    public record ListData<TElement> : ApiData
    {
        /// <summary>
        /// 数据
        /// </summary>
        public List<TElement>? Data { get; init; }

        /// <summary>
        /// 当前页
        /// </summary>
        public int CurrentPage { get; init; }

        /// <summary>
        /// 页大小
        /// </summary>
        public int PageSize { get; init; }

        /// <summary>
        /// 记录总数
        /// </summary>
        public int Total { get; init; }
    }

}
