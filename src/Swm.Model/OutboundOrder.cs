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
using Arctic.NHibernateExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Swm.Model
{
    /// <summary>
    /// 表示出库单。
    /// </summary>
    public class OutboundOrder : IHasCtime, IHasCuser, IHasMtime, IHasMuser, IUnitloadAllocationTable
    {
        /// <summary>
        /// 分配表类型描述
        /// </summary>
        public const string UatTypeDescription = "出库单";

        public OutboundOrder()
        {
            this.ctime = DateTime.Now;
            this.mtime = DateTime.Now;
            this.Lines = new HashSet<OutboundLine>();
            this.BizOrder = Cst.None;
        }

        /// <summary>
        /// 出库单Id
        /// </summary>
        public virtual Int32 OutboundOrderId { get; protected set; }

        /// <summary>
        /// 出库单编号。
        /// </summary>
        [Required]
        [MaxLength(20)]
        public virtual String OutboundOrderCode { get; set; }

        public virtual Int32 v { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTime ctime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [MaxLength(FIELD_LENGTH.USERNAME)]
        public virtual string cuser { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public virtual DateTime mtime { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        [MaxLength(FIELD_LENGTH.USERNAME)]
        public virtual string muser { get; set; }

        /// <summary>
        /// 业务类型
        /// </summary>
        [Required]
        [MaxLength(FIELD_LENGTH.APP_CODE)]
        public virtual String BizType { get; set; }

        /// <summary>
        /// 业务单据号
        /// </summary>
        [MaxLength(20)]
        public virtual String BizOrder { get; set; }

        /// <summary>
        /// 是否已关闭
        /// </summary>
        public virtual bool Closed { get; set; }

        /// <summary>
        /// 关闭时间
        /// </summary>
        public virtual DateTime? ClosedAt { get; set; }

        /// <summary>
        /// 由谁关闭
        /// </summary>
        [MaxLength(FIELD_LENGTH.USERNAME)]
        public virtual string ClosedBy { get; set; }


        /// <summary>
        /// 备注
        /// </summary>
        public virtual String Comment { get; set; }

        /// <summary>
        /// 出库单明细集合。
        /// </summary>
        public virtual ISet<OutboundLine> Lines { get; protected set; }

        /// <summary>
        /// 获取为此出库单分配的货载
        /// </summary>
        public virtual ISet<Unitload> Unitloads { get; protected set; }

        /// <summary>
        /// 向此出库单添加明细。
        /// </summary>
        /// <param name="line"></param>
        public virtual void AddLine(OutboundLine line)
        {
            if (line.OutboundOrder != null)
            {
                throw new InvalidOperationException("出库单明细已属于其他出库单。");
            }

            line.OutboundOrder = this;
            this.Lines.Add(line);
        }

        /// <summary>
        /// 从此出库单移除明细
        /// </summary>
        /// <param name="line"></param>
        public virtual void RemoveLine(OutboundLine line)
        {
            line.OutboundOrder = null;
            this.Lines.Remove(line);
        }

        /// <summary>
        /// 计算货载项在此出库单中的分配数量。
        /// </summary>
        /// <param name="unitloadItem"></param>
        /// <returns></returns>
        public virtual decimal ComputeAllocated(UnitloadItem unitloadItem)
        {
            return Lines.SelectMany(x => x.Allocations)
                .Where(x => x.UnitloadItem == unitloadItem)
                .Sum(x => x.Quantity);
        }


        public override string ToString()
        {
            return this.OutboundOrderCode;
        }

    }

}