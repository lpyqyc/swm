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

namespace Swm.Locations
{
    public static class LocationTypes
    {
        /// <summary>
        /// 可以储存货物的位置。
        /// </summary>
        public const string S = "S";

        /// <summary>
        /// 关键点，通常是自动化的。
        /// </summary>
        public const string K = "K";

        /// <summary>
        /// N 位置，表示一个不存在的特殊位置。货载刚刚注册时，在 N 位置上，N 位置在整个系统中只有一个实例。
        /// </summary>
        public const string N = "N";
    }


    public static class LocationCodes
    {
        /// <summary>
        ///  N 位置的编码
        /// </summary>
        public const string N = "N";
    }
}
