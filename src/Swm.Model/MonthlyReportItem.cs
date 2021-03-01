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

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Swm.Model
{
    /// <summary>
    /// 月报数据项
    /// </summary>
    public class MonthlyReportItem : IEquatable<MonthlyReportItem>, IHasStockKey
    {
        /// <summary>
        /// 用于标识 unsaved 状态，不用于版本控制。
        /// </summary>
        protected virtual int v { get; set; }


        public virtual MonthlyReport MonthlyReport { get; set; }

        /// <summary>
        /// 物料
        /// </summary>
        [Required]
        [MaxLength(10)]
        public virtual Material Material { get; set; }

        /// <summary>
        /// 批号
        /// </summary>
        [Required]
        [MaxLength(FIELD_LENGTH.BATCH)]
        public virtual string Batch { get; set; }

        /// <summary>
        /// 库存状态
        /// </summary>
        [Required]
        [MaxLength(FIELD_LENGTH.STOCK_STATUS)]
        public virtual string StockStatus { get; set; }

        /// <summary>
        /// 计量单位
        /// </summary>
        [Required]
        [MaxLength(FIELD_LENGTH.UOM)]
        public virtual string Uom { get; set; }

        /// <summary>
        /// 期初数量。期初数量 = 上期期末数量。
        /// </summary>
        public virtual decimal Beginning { get; set; }

        /// <summary>
        /// 流入数量。
        /// </summary>
        public virtual decimal Incoming { get; set; }

        /// <summary>
        /// 流出数量。
        /// </summary>
        public virtual decimal Outgoing { get; set; }

        /// <summary>
        /// 期末数量。期末数量 = 期初数量 + 流入数量 - 流出数量。
        /// </summary>
        public virtual decimal Ending
        {
            get
            {
                return Beginning + Incoming - Outgoing;
            }
            protected set { }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MonthlyReportItem);
        }

        public virtual bool Equals(MonthlyReportItem other)
        {
            return other != null &&
                   EqualityComparer<MonthlyReport>.Default.Equals(MonthlyReport, other.MonthlyReport) &&
                   EqualityComparer<Material>.Default.Equals(Material, other.Material) &&
                   Batch == other.Batch &&
                   StockStatus == other.StockStatus &&
                   Uom == other.Uom;
        }

        public override int GetHashCode()
        {
            int hashCode = -1058726734;
            hashCode = hashCode * -1521134295 + EqualityComparer<MonthlyReport>.Default.GetHashCode(MonthlyReport);
            hashCode = hashCode * -1521134295 + EqualityComparer<Material>.Default.GetHashCode(Material);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Batch);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(StockStatus);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Uom);
            return hashCode;
        }

        public static bool operator ==(MonthlyReportItem left, MonthlyReportItem right)
        {
            return EqualityComparer<MonthlyReportItem>.Default.Equals(left, right);
        }

        public static bool operator !=(MonthlyReportItem left, MonthlyReportItem right)
        {
            return !(left == right);
        }
    }

}
