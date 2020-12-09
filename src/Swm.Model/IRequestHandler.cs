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

using System.Threading.Tasks;

namespace Swm.Model
{
    /// <summary>
    /// 此接口定义 wcs 请求的处理程序。
    /// </summary>
    public interface IRequestHandler
    {
        /// <summary>
        /// 对请求进行处理。此方法将在 TransactionScope 范围中被调用。
        /// </summary>
        /// <param name="requestInfo">请求信息</param>
        Task ProcessRequestAsync(RequestInfo requestInfo);
    }
}
