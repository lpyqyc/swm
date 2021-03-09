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

using Arctic.AppCodes;
using NHibernate;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Swm.Materials
{
    public class MaterialService
    {
        readonly ISession _session;

        public MaterialService(ISession session)
        {
            _session = session;
        }

        public async Task<List<MaterialTypeInfo>> GetMaterialTypesAsync()
        {
            var appCodes = await _session
                .Query<AppCode>()
                .GetAppCodesAsync(AppCodeTypes.MaterialType)
                .ConfigureAwait(false);

            var list = appCodes
                .Select(x => new MaterialTypeInfo
                {
                    MaterialType = x.AppCodeValue,
                    Description = x.Description,
                    Scope = x.Scope,
                    DisplayOrder = x.DisplayOrder,
                }).ToList();

            return list;
        }

        public async Task<List<BizTypeInfo>> GetBizTypesAsync()
        {
            var appCodes = await _session
                .Query<AppCode>()
                .GetAppCodesAsync(AppCodeTypes.BizType)
                .ConfigureAwait(false);

            var list = appCodes.Select(x => new BizTypeInfo
            {
                BizType = x.AppCodeValue,
                Description = x.Description,
                Scope = x.Scope,
                DisplayOrder = x.DisplayOrder
            }).ToList();

            return list;
        }

        public async Task<List<StockStatusInfo>> GetStockStatusAsync()
        {
            var appCodes = await _session
                .Query<AppCode>()
                .GetAppCodesAsync(AppCodeTypes.StockStatus)
                .ConfigureAwait(false);

            var list = appCodes.Select(x => new StockStatusInfo
            {
                StockStatus = x.AppCodeValue,
                Description = x.Description,
                Scope = x.Scope,
                DisplayOrder = x.DisplayOrder,
            }).ToList();

            return list;
        }

    }

}
