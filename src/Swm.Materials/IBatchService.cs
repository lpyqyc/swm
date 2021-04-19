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

namespace Swm.Materials
{
    public interface IBatchService
    {
        /// <summary>
        /// 获取表示没有批号的特殊值
        /// </summary>
        /// <returns></returns>
        string GetValueForNoBatch();
        
        /// <summary>
        /// 对用户录入的批号值进行规范化
        /// </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        string Normalize(string? batch);
    }
}
