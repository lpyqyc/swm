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

using Arctic.Auditing;
using System;
using System.ComponentModel.DataAnnotations;

namespace Swm.Model
{
    /// <summary>
    /// 表示库存流水。
    /// 使用 NHibernate.ISession 保存流水不会自动维护 <see cref="Stock"/>，
    /// 如果需要在保存流水时自动维护 <see cref="Stock"/>，
    /// 可使用 <see cref="FlowHelper.SaveAsync(Flow, bool)"/> 方法。
    /// </summary>
    public class Flow : IHasCtime, IHasCuser, IHasStockKey
    {
        /// <summary>
        /// 初始化 Flow 类的新实例。
        /// </summary>
        internal protected Flow()
        {
            this.ctime = DateTime.Now;
        }

        public virtual int FlowId { get; internal protected set; }

        public virtual DateTime ctime { get; set; }

        [MaxLength(FIELD_LENGTH.USERNAME)]
        public virtual string cuser { get; set; }

        [Required]
        public virtual Material Material { get; set; }

        [Required]
        [MaxLength(FIELD_LENGTH.BATCH)]
        public virtual string Batch { get; set; }

        [MaxLength(FIELD_LENGTH.STOCK_STATUS)]
        [Required]
        public virtual string StockStatus { get; set; }

        public virtual decimal Quantity { get; set; }

        [Required]
        [MaxLength(FIELD_LENGTH.UOM)]
        public virtual string Uom { get; set; }


        [Required]
        [MaxLength(FIELD_LENGTH.APP_CODE)]
        public virtual string BizType { get; set; }

        public virtual FlowDirection Direction { get; set; }

        // TODO 重命名
        [Required]
        [MaxLength(FIELD_LENGTH.OPERATION_TYPE)]
        public virtual string OpType { get; set; }


        [Required]
        [MaxLength(20)]
        public virtual string TxNo { get; set; }


        [MaxLength(20)]
        [Required]
        public virtual string OrderCode { get; set; }

        [Required]
        [MaxLength(20)]
        public virtual string BizOrder { get; set; }

        [MaxLength(20)]
        public virtual string PalletCode { get; set; }

        public virtual decimal Balance { get; set; }

        public virtual string Comment { get; set; }

    }
}
