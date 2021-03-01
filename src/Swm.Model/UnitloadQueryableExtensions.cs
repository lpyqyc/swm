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

using Arctic.NHibernateExtensions;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Swm.Model
{
    public static class UnitloadQueryableExtensions
    {
        public static async Task<Unitload> GetAsync(this IQueryable<Unitload> q, string palletCode)
        {
            return await q
                .Where(x => x.PalletCode == palletCode)
                .SingleOrDefaultAsync()
                .ConfigureAwait(false);
        }


        public static async Task<HashSet<Unitload>> GetAsync(this IQueryable<Unitload> q, params string[] palletCodes)
        {
            if (palletCodes == null || palletCodes.Length == 0)
            {
                throw new ArgumentException("未指定托盘号。");
            }

            List<Unitload> list = await q
                .Where(x => palletCodes.Contains(x.PalletCode))
                .ToListAsync()
                .ConfigureAwait(false);
            return list.ToHashSet();
        }

        public static async Task<HashSet<Unitload>> GetAsync(this IQueryable<Unitload> q, Location loc, bool? beingMoved)
        {
            q = q.Where(x => x.CurrentLocation == loc);
            if (beingMoved != null)
            {
                q = q.Where(x => x.BeingMoved == beingMoved);
            }

            List<Unitload> list = await q.ToListAsync().ConfigureAwait(false);
            return list.ToHashSet();
        }


        public static async Task<HashSet<Unitload>> GetAsync(this IQueryable<Unitload> q, string opHintType, string opHintInfo)
        {
            var list = await q
                .Where(x => x.OpHintType == opHintType && x.OpHintInfo == opHintInfo)
                .ToListAsync()
                .ConfigureAwait(false);
            return list.ToHashSet();
        }

        public static async Task<bool> IsPalletCodeInUseAsync(this IQueryable<Unitload> q, string palletCode)
        {
            return await q
                .Where(x => x.PalletCode == palletCode)
                .AnyAsync()
                .ConfigureAwait(false);
        }
    }
}