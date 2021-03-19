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

using Swm.Constants;
using System;
using System.Linq;

namespace Swm.Palletization
{
    public class UnitloadStorageInfoProvider : IUnitloadStorageInfoProvider
    {
        public virtual string GetOutFlag(Unitload unitload)
        {
            if (unitload.Items.Any() == false)
            {
                return "空托盘";
            }
            var list = unitload.Items
                .Select(x => new
                {
                    x.Batch,
                    x.Material?.MaterialCode
                })
                .Distinct()
                .ToList();
            if (list.Count > 1)
            {
                return "混合";
            }
            return list.Single().Batch?.ToUpper() + "@" + list.Single().MaterialCode;
        }


        public virtual string GetStorageGroup(Unitload unitload)
        {
            var list = unitload.Items.Select(x => x.Material?.DefaultStorageGroup).Distinct();
            if (list.Count() == 1)
            {
                return list.Single()!;
            }
            else
            {
                throw new ApplicationException("无法确定存储分组。");
            }
        }

        public virtual string GetContainerSpecification(Unitload unitload)
        {
            return "None";
        }
    }


}
