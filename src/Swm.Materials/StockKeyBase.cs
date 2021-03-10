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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Swm.Materials
{
    // TODO 文档：扩展点

    /// <summary>
    /// 库存键确定库存“是什么”。
    /// 两条库存记录只有在库存键相等的情况下才可以进行比较大小和加减操作。
    /// </summary>
    /// <remarks><see cref="Arctic.Models.Materials.StockKeyExtensions"/> 使用反射操作 StockKey，
    /// 子类应使用位置记录的语法形式声明，参考 <see cref="Arctic.Models.Materials.DefaultStockKey"/> 的写法。</remarks>
    public abstract record StockKeyBase();

}

