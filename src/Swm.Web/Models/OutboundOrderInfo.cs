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

using System;
using System.Collections.Generic;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 出库单列表页的数据项
    /// </summary>
    public class OutboundOrderInfo
    {
        /// <summary>
        /// 出库单Id
        /// </summary>
        public int OutboundOrderId { get; set; }

        /// <summary>
        /// 出库单编号。
        /// </summary>
        public string OutboundOrderCode { get; set; }


        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime ctime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string cuser { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime mtime { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public string muser { get; set; }


        /// <summary>
        /// 业务类型
        /// </summary>
        public string BizType { get; set; }

        /// <summary>
        /// 业务单据号
        /// </summary>
        public string BizOrder { get; set; }

        /// <summary>
        /// 是否已关闭
        /// </summary>
        public bool Closed { get; set; }

        /// <summary>
        /// 关闭时间
        /// </summary>
        public DateTime? ClosedAt { get; set; }

        /// <summary>
        /// 由谁关闭
        /// </summary>
        public string ClosedBy { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// 出库单明细集合。
        /// </summary>
        public List<OutboundLineInfo> Lines { get; set; }

        /// <summary>
        /// 已分配的货载数
        /// </summary>
        public int UnitloadCount { get; set; }


    }



}

