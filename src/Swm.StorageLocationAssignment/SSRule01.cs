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
using Serilog;
using Swm.Locations;
using Swm.Palletization;
using System;
using System.Threading.Tasks;

namespace Swm.StorageLocationAssignment
{
    /// <summary>
    /// 此规则适用于单叉双深巷道。
    /// </summary>
    public class SSRule01 : IRule
    {
        readonly ISession _session;
        readonly ILogger _logger;

        public SSRule01(ISession session, ILogger logger)
        {
            _session = session;
            _logger = logger;
        }


        /// <summary>
        /// 获取此规则是否适用于双深巷道。
        /// </summary>
        public bool DoubleDeep => false;

        public string Name => "SSRule01";

        public int Order => 100;


        /// <summary>
        /// 在指定的巷道和分组中分配一个货位以供入库。
        /// </summary>
        /// <param name="laneway">要在其中分配货位的巷道。</param>
        /// <param name="excludedIdList">要排除的货位。</param>
        /// <param name="excludedColumnList">要排除的列。</param>
        /// <param name="excludedLevelList">要排除的层。</param>
        /// <param name="storageInfo">入库货物信息。</param>
        /// <param name="orderBy">排序依据。这是 LocationUnit 类的属性名。</param>
        /// <returns></returns>
        public async Task<Location?> SelectAsync(Laneway laneway, StorageInfo storageInfo, int[]? excludedIdList, int[]? excludedColumnList, int[]? excludedLevelList, string orderBy)
        {
            if (laneway == null)
            {
                throw new ArgumentNullException("laneway");
            }

            if (storageInfo == null)
            {
                throw new ArgumentNullException("storageInfo");
            }

            if (string.IsNullOrWhiteSpace(orderBy))
            {
                throw new ArgumentException("参数 orderBy 不能为 null 或空字符串。");
            }

            if (laneway.DoubleDeep != this.DoubleDeep)
            {
                string msg = $"巷道类型不匹配，此规则适用于单深巷道，但传入的巷道是双深。【{laneway.LanewayCode}】。";
                throw new InvalidOperationException(msg);
            }

            if (laneway.Offline)
            {
                _logger.Warning("巷道 {lanewayCode} 已离线", laneway.LanewayCode);
                return null;
            }

            string queryString = $@"
SELECT loc.LocationId
FROM Location loc
JOIN loc.Cell c
WHERE loc.Laneway = :laneway

AND loc.Exists = true
AND loc.UnitloadCount = 0
AND loc.OutboundCount = 0
AND loc.InboundDisabled = false
AND loc.InboundCount = 0
AND loc.WeightLimit >= :weight
AND loc.HeightLimit >= :height
AND loc.StorageGroup = :storageGroup
AND loc.Specification = :locSpec
{"AND loc.LocationId NOT IN (:excludedIdList)".If(excludedIdList)}
{"AND loc.Column NOT IN (:excludedColumnList)".If(excludedColumnList)}
{"AND loc.Level NOT IN (:excludedLevelList)".If(excludedLevelList)}

ORDER BY loc.WeightLimit, loc.HeightLimit, c.$orderBy 
";

            queryString = queryString.Replace("$orderBy", orderBy);

            IQuery q = _session
                .CreateQuery(queryString)
                .SetParameter("laneway", laneway)
                .SetParameter("weight", storageInfo.Weight)
                .SetParameter("height", storageInfo.Height)
                .SetParameter("storageGroup", storageInfo.StorageGroup)
                .SetParameter("locSpec", storageInfo.ContainerSpecification)
                .EmptySafeSetParameterList("excludedIdList", excludedIdList)
                .EmptySafeSetParameterList("excludedColumnList", excludedColumnList)
                .EmptySafeSetParameterList("excludedLevelList", excludedLevelList)
                .SetMaxResults(1);

            int? id = await q.UniqueResultAsync<int?>().ConfigureAwait(false);

            if (id == null)
            {
                return null;
            }
            else
            {
                var loc = await _session.GetAsync<Location>(id.Value).ConfigureAwait(false);
                return loc;
            }
        }
    }

}
