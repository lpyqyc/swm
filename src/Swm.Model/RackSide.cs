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
    /// 指示货架在巷道的左侧还是右侧。左右并没有绝对意义，仅用于将两侧的货架区分开。
    /// </summary>
    public enum RackSide
    {
        /// <summary>
        /// 左侧货架。
        /// </summary>
        Left = -1,

        /// <summary>
        /// 未指定货架是左侧还是右侧。
        /// </summary>
        NA,

        /// <summary>
        /// 右侧货架。
        /// </summary>
        Right = 1
    }

}
