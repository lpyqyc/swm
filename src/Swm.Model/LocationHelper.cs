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

using NHibernate;
using NHibernate.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Swm.Model
{
    public class LocationHelper
    {
        ISession _session;
        public LocationHelper(ISession session)
        {
            _session = session;
        }

        /// <summary>
        /// 重建巷道的统计信息。原有统计信息将被清除。此操作占用资源较多。
        /// </summary>
        public async Task RebuildLanewayStatAsync(Laneway laneway)
        {
            if (laneway == null)
            {
                throw new ArgumentNullException(nameof(laneway));
            }

            laneway.Usage.Clear();

            var keys = _session.Query<Laneway>()
                .Where(x => x == laneway)
                .SelectMany(x => x.Locations)
                .Where(x => x.Exists)
                .GroupBy(x => new
                {
                    x.StorageGroup,
                    x.Specification,
                    x.WeightLimit,
                    x.HeightLimit
                })
                .Select(x => new LanewayUsageKey
                {
                    StorageGroup = x.Key.StorageGroup,
                    Specification = x.Key.Specification,
                    WeightLimit = x.Key.WeightLimit,
                    HeightLimit = x.Key.HeightLimit
                });

            foreach (var key in keys)
            {
                await UpdateUsageAsync(laneway, key);
            }

            async Task UpdateUsageAsync(Laneway laneway, LanewayUsageKey key)
            {
                var q = _session.Query<Laneway>()
                    .Where(x => x == laneway)
                    .SelectMany(x => x.Locations)
                    .Where(x => x.Exists
                        && x.StorageGroup == key.StorageGroup
                        && x.Specification == key.Specification
                        && x.WeightLimit == key.WeightLimit
                        && x.HeightLimit == key.HeightLimit
                    );

                var total = q
                    .ToFutureValue(fq => fq.Count());

                var loaded = q
                    .Where(x => x.UnitloadCount > 0)
                    .ToFutureValue(fq => fq.Count());

                var available = q
                    .Where(x =>
                        x.UnitloadCount == 0
                        && x.InboundDisabled == false)
                    .ToFutureValue(fq => fq.Count());

                var inboundDisabled = q
                    .Where(x => x.InboundDisabled == true)
                    .ToFutureValue(fq => fq.Count());

                var outboundDisabled = q
                    .Where(x => x.OutboundDisabled == true)
                    .ToFutureValue(fq => fq.Count());

                laneway.Usage[key] = new LanewayUsageData
                {
                    mtime = DateTime.Now,
                    Total = await total.GetValueAsync(),
                    Available = await available.GetValueAsync(),
                    Loaded = await loaded.GetValueAsync(),
                    InboundDisabled = await inboundDisabled.GetValueAsync(),
                };
            }
        }
    }


}
