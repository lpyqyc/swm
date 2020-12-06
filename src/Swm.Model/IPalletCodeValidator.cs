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

namespace Swm.Model
{
    // TODO 扩展点

    public interface IPalletCodeValidator
    {
        /// <summary>
        /// 判断托盘号格式是否正确，不检查托盘号是否占用。
        /// </summary>
        /// <param name="palletCode">要检查的托盘号</param>
        /// <param name="msg">描述性文本</param>
        /// <returns>true 表示托盘号格式正确，否则为不正确</returns>
        bool IsWellFormed(string palletCode, out string msg);
    }

}