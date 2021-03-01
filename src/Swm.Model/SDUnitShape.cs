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

namespace Swm.Model
{
    /// <summary>
    /// 单叉双深单元的形态。两位数字依次表示 N1, F1。
    /// </summary>
    public enum SDUnitShape
    {
        /// <summary>
        /// 一深无货，二深无货。
        /// </summary>
        SD_00,

        /// <summary>
        /// 一深无货，二深有货。
        /// </summary>
        SD_01,

        /// <summary>
        /// 一深有货，二深无货。
        /// </summary>
        SD_10,

        /// <summary>
        /// 一深有货，二深有货。
        /// </summary>
        SD_11
    }

}
