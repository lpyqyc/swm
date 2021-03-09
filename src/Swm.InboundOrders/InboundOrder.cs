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

using Arctic.Auditing;
using Swm.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Swm.InboundOrders
{
    /// <summary>
    /// 表示入库单。入库单既是指引入库操作的指令，也是入库操作之后的凭据。
    /// </summary>
    public class InboundOrder : IHasCtime, IHasCuser, IHasMtime, IHasMuser
    {
        private ISet<InboundLine> _lines;

        /// <summary>
        /// 初始化此类的新实例。
        /// </summary>
        public InboundOrder()
        {
            this.ctime = DateTime.Now;
            this.mtime = DateTime.Now;
            this.Lines = new HashSet<InboundLine>();
            this.BizOrder = Cst.None;
        }

        /// <summary>
        /// Id
        /// </summary>
        public virtual Int32 InboundOrderId { get; protected set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public virtual Int32 v { get; set; }

        /// <summary>
        /// 创建时间。
        /// </summary>
        public virtual DateTime ctime { get; set; }

        /// <summary>
        /// 创建人。
        /// </summary>
        [MaxLength(FIELD_LENGTH.USERNAME)]
        public virtual string cuser { get; set; }

        /// <summary>
        /// 更新时间。
        /// </summary>
        public virtual DateTime mtime { get; set; }

        /// <summary>
        /// 更改人
        /// </summary>
        [MaxLength(FIELD_LENGTH.USERNAME)]
        public virtual string muser { get; set; }

        /// <summary>
        /// Wms 内部单号，自然键。
        /// </summary>
        [Required]
        [MaxLength(20)]
        public virtual String InboundOrderCode { get; set; }

        /// <summary>
        /// 业务类型。
        /// </summary>
        [Required]
        [MaxLength(FIELD_LENGTH.APP_CODE)]
        public virtual String BizType { get; set; }


        /// <summary>
        /// 业务单号，例如采购单，退货单，生产计划编号。
        /// </summary>
        [MaxLength(20)]
        public virtual String BizOrder { get; set; }

        /// <summary>
        /// 是否已关闭。关闭的入库单不能再入库。
        /// </summary>
        public virtual Boolean Closed { get; set; }

        /// <summary>
        /// 关单人。可以为空。
        /// </summary>
        [MaxLength(FIELD_LENGTH.USERNAME)]
        public virtual String ClosedBy { get; set; }

        /// <summary>
        /// 关闭时间。
        /// </summary>
        public virtual DateTime? ClosedAt { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public virtual String Comment { get; set; }

        /// <summary>
        /// 此入库单中的行。
        /// </summary>
        public virtual IEnumerable<InboundLine> Lines
        {
            get
            {
                return _lines;
            }
            protected set
            {
                _lines = (ISet<InboundLine>)value;
            }
        }

        /// <summary>
        /// 向此入库单中添加行。
        /// </summary>
        /// <param name="line"></param>
        public virtual void AddLine(InboundLine line)
        {
            if (line.InboundOrder != null)
            {
                throw new InvalidOperationException("入库行已属于其他入库单。");
            }

            line.InboundOrder = this;
            this._lines.Add(line);
        }


        public virtual void RemoveLine(InboundLine line)
        {
            line.InboundOrder = null;
            this._lines.Remove(line);
        }

        public override string ToString()
        {
            return this.InboundOrderCode;
        }
    }
}