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
using Swm.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Swm.Locations
{
    public static class LocationsQueries
    {
        // 不要改为异步方法

        /// <summary>
        /// 获取所有关键点上配置的请求类型
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public static Dictionary<string, string?> GetRequestTypes(this IQueryable<Location> q)
        {
            var list = q
                .Where(x => x.LocationType == LocationTypes.K)
                .Select(x => new { x.LocationCode, x.RequestType });

            return list.ToDictionary(x => x.LocationCode, x => x.RequestType);
        }

        public static async Task<List<Location>> GetAsync(this IQueryable<Location> q, IEnumerable<int> locationIdList)
        {
            return await q
                .Where(x => locationIdList.Contains(x.LocationId))
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public static async Task<Location> GetAsync(this IQueryable<Location> q, string locationCode)
        {
            return await q
                .Where(x => x.LocationCode == locationCode)
                .SingleOrDefaultAsync()
                .ConfigureAwait(false);
        }

        public static async Task<Port> GetAsync(this IQueryable<Port> q, string portCode)
        {
            return await q
                .Where(x => x.PortCode == portCode)
                .WithOptions(x => x.SetCacheable(true))
                .SingleOrDefaultAsync()
                .ConfigureAwait(false);
        }

        // TODO 不是查询
        public static async Task SaveLocationOpAsync(this ISession session, Location loc, string opType, string comment)
        {
            LocationOp op = new LocationOp
            {
                OpType = opType,
                Comment = comment,
                ctime = DateTime.Now,
                Location = loc
            };
            await session.SaveAsync(op).ConfigureAwait(false);
        }

        public static async Task<Location> GetNAsync(this IQueryable<Location> q)
        {
            return await q
                .Where(x => x.LocationCode == "None")
                .WithOptions(x => x.SetCacheable(true))
                .SingleOrDefaultAsync()
                .ConfigureAwait(false);
        }
    }

}
