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

using NHibernate.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace Swm.Materials
{
    public static class MaterialsQueries
    {
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
    }

}
