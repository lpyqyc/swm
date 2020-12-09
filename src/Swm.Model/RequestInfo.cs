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
using System.Text.Json;

namespace Swm.Model
{
    /// <summary>
    /// Wcs 给 Wms 的请求信息。
    /// </summary>
    public class RequestInfo
    {
        /// <summary>
        /// 初始化 WcsRequestInfo 类的新实例。
        /// </summary>
        public RequestInfo()
        {
        }

        /// <summary>
        /// 请求类型，依项目而变化。
        /// </summary>
        public string RequestType { get; set; }

        /// <summary>
        /// 请求发出的位置。
        /// </summary>
        public string LocationCode { get; set; }

        /// <summary>
        /// 容器编码。
        /// </summary>
        public string ContainerCode { get; set; }

        /// <summary>
        /// 重量。
        /// </summary>
        public decimal Weight { get; set; }

        /// <summary>
        /// 高度。
        /// </summary>
        public decimal Height { get; set; }

        /// <summary>
        /// 附加信息。
        /// </summary>
        public Dictionary<string, string> AdditionalInfo { get; set; }

        /// <summary>
        /// 返回表示此实例的字符串。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true } );
        }
    }

}
