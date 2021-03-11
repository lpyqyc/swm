﻿// Copyright 2020-2021 王建军
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

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 储位详情
    /// </summary>
    public class StorageLocationDetail : StorageLocationInfo
    {
        /// <summary>
        /// 在此货位上的货载，包含出站任务未完成的货载
        /// </summary>
        public UnitloadDetail[]? Unitloads { get; set; }

    }
}
