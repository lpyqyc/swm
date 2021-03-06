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

using NHibernate;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Swm.Model.StorageLocationAssignment
{
    /// <summary>
    /// 此规则适用于单叉双深巷道。
    /// </summary>
    public class SDRule02 : IRule
    {
        readonly ISession _session;
        readonly ILogger _logger;

        public SDRule02(ISession session, ILogger logger)
        {
            _session = session;
            _logger = logger;
        }

        /// <summary>
        /// 获取此规则是否适用于双深巷道。
        /// </summary>
        public bool DoubleDeep => true;


        public string Name => "SDRule02";

        public int Order => 200;

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
        public async Task<Location> SelectAsync(Laneway laneway, UnitloadStorageInfo storageInfo, int[] excludedIdList, int[] excludedColumnList, int[] excludedLevelList, string orderBy)
        {
            if (laneway == null)
            {
                throw new ArgumentNullException("laneway");
            }

            if (storageInfo == null)
            {
                throw new ArgumentNullException("cargoInfo");
            }

            if (string.IsNullOrWhiteSpace(orderBy))
            {
                throw new ArgumentException("参数 orderBy 不能为 null 或空字符串。");
            }

            if (laneway.DoubleDeep != this.DoubleDeep)
            {
                string msg = $"巷道类型不匹配，此规则适用于双深巷道，但传入的巷道是单深。【{laneway.LanewayCode}】。";
                throw new InvalidOperationException(msg);
            }

            if (laneway.Offline)
            {
                _logger.Warning("巷道 {lanewayCode} 已离线", laneway.LanewayCode);
                return null;
            }

            // 此规则向双深单元的二深分配
            // 使双深单元的形态 00| 变为 01|
            // 此规则不需要考虑出库标记和分配标记

            string queryString = $@"
SELECT loc2.LocationId

FROM Location loc1
     JOIN loc1.Rack rack1
     JOIN loc1.Cell c,
   
     Location loc2
     JOIN loc2.Rack rack2

WHERE rack1.Deep = 1
AND rack2.Deep = 2
AND loc1.Cell = loc2.Cell
AND rack1.Laneway = :laneway

AND loc1.UnitloadCount = 0
AND loc1.OutboundCount = 0
AND loc1.InboundCount = 0

AND loc2.Exists = true
AND loc2.UnitloadCount = 0
AND loc2.OutboundCount = 0
AND loc2.InboundDisabled = false
AND loc2.InboundCount = 0
AND loc2.WeightLimit >= :weight
AND loc2.HeightLimit >= :height
AND loc2.StorageGroup = :storageGroup
AND loc2.Specification = :locSpec
{"AND loc2.LocationId NOT IN (:excludedIdList)".If(excludedIdList)}
{"AND loc2.Column NOT IN (:excludedColumnList)".If(excludedColumnList)}
{"AND loc2.Level NOT IN (:excludedLevelList)".If(excludedLevelList)}

ORDER BY loc2.WeightLimit, loc2.HeightLimit, c.$orderBy
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
