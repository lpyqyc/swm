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

using Arctic.NHibernateExtensions;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Swm.Model
{
    // TODO 整理
    /// <summary>
    /// 
    /// </summary>
    public static class Queries
    {
        /// <summary>
        /// 获取具有指定任务号的任务。
        /// </summary>
        /// <param name="taskCode">任务号。</param>
        /// <returns></returns>
        public static async Task<TransportTask> GetTaskAsync(this IQueryable<TransportTask> q, string taskCode)
        {
            return await q.SingleOrDefaultAsync(x => x.TaskCode == taskCode).ConfigureAwait(false);
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


        public static Dictionary<string, string> GetRequestTypes(this IQueryable<Location> q)
        {
            var list = q
                .Where(x => x.LocationType == LocationTypes.K)
                .Select(x => new { x.LocationCode, x.RequestType })
                .ToList();

            return list.ToDictionary(x => x.LocationCode, x => x.RequestType);
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
                .Where(x => x.LocationCode == Cst.None)
                .WithOptions(x => x.SetCacheable(true))
                .SingleOrDefaultAsync()
                .ConfigureAwait(false);
        }

        public static async Task<Material> GetMaterialAsync(this IQueryable<Material> q, string materialCode)
        {
            return await q
                .Where(x => x.MaterialCode == materialCode)
                .WithOptions(x => x.SetCacheable(true))
                .SingleOrDefaultAsync()
                .ConfigureAwait(false);
        }

        public static IQueryable<Material> FilterByKeyword(this IQueryable<Material> q, string? keyword = null, string type = null)
        {
            keyword = keyword?.Trim();
            type = type?.Trim();

            if (string.IsNullOrEmpty(keyword) == false)
            {
                q = q.Where(x =>
                    x.MaterialCode.Contains(keyword)
                    || x.Description.Contains(keyword)
                    || x.MnemonicCode.Contains(keyword)
                );
            }

            if (type != null)
            {
                q = q.Where(x => x.MaterialType == type);
            }
            return q;
        }

        public static IQueryable<T> OfStockKey<T>(this IQueryable<T> q, StockKeyBase stockKey) where T : class, IHasStockKey
        {
            if (q == null)
            {
                throw new ArgumentNullException(nameof(q));
            }

            if (stockKey == null)
            {
                throw new ArgumentNullException(nameof(stockKey));
            }

            var components = GetComponents();
            string where = BuildWhereClause(components);
            q = q.Where(where, components.Select(x => x.ComponentValue).ToArray());

            return q;

            List<StockKeyComponent> GetComponents()
            {
                return stockKey.GetType()
                    .GetProperties()
                    .Select(x => new StockKeyComponent(x.Name, x.GetValue(stockKey)))
                    .ToList();
            }
        }

        internal static string BuildWhereClause(IList<StockKeyComponent> components)
        {
            return string.Join(" AND ", components.Select((x, i) => $"{x.ComponentName} = @{i}"));
        }
    }

}
