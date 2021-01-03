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

using System.ComponentModel.DataAnnotations;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 无单据组盘。
    /// </summary>
    public class PalletizeWithoutOrderArgs
    {
        [Required]
        public string PalletCode { get; set; }

        public Item[] Items { get; set; }

        public class Item
        {
            [Required]
            public string MaterialCode { get; set; }

            [Required]
            public string Batch { get; set; }

            [Required]
            public string StockStatus { get; set; }

            public decimal Quantity { get; set; }

            [Required]
            public string Uom { get; set; }
        }

    }


}
