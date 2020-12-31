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
using Swm.Model;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 列表查询参数
    /// </summary>
    public class OutboundOrderListArgs
    {
        /// <summary>
        /// 支持模糊查找，使用 ? 表示单个字符，使用 * 表示任意个字符
        /// </summary>
        [SearchArg(SearchMode.Like)]
        public string? OutboundOrderCode { get; set; }

        /// <summary>
        /// 业务类型
        /// </summary>
        [SearchArg(SearchMode.Like)]
        public string? BizType { get; set; }

        // TODO 改为 Expression 方式
        /// <summary>
        /// 是否显示已关闭的出库单
        /// </summary>
        public bool? ShowClosed { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public IQueryable<OutboundOrder> Filter(IQueryable<OutboundOrder> q)
        {
            if (ShowClosed != true)
            {
                q = q.Where(x => x.Closed == false);
            }
            return q;
        }

        /// <summary>
        /// 排序字段，例如 F1 DESC, F2 ASC, F3
        /// </summary>
        public string? Sort { get; set; }

        /// <summary>
        /// 基于 1 的当前页面，默认值为 1。
        /// </summary>
        public int? Current { get; set; } = 1;

        /// <summary>
        /// 每页大小，默认值为 10。
        /// </summary>
        public int? PageSize { get; set; }

    }

    /// <summary>
    /// 创建出库单操作的参数
    /// </summary>
    public class CreateOutboundOrderArgs
    {
        /// <summary>
        /// 业务类型
        /// </summary>
        [Required]
        public string BizType { get; set; }

        /// <summary>
        /// 业务单据号
        /// </summary>
        public string? BizOrder { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [Required]
        public string Comment { get; set; }


        public List<EditOutboundLineInfo> Lines { get; set; }
    }

    /// <summary>
    /// 创建出库单或编辑出库单操作中的出库行信息。
    /// </summary>
    public class EditOutboundLineInfo
    {
        /// <summary>
        /// 本行的操作：
        /// add 表示新增，
        /// edit 表示编辑，
        /// delete 表示删除。
        /// </summary>
        [Required]
        public string Op { get; set; } = default!;


        /// <summary>
        /// 出库单明细Id，用户在界面上新增的明细Id为0。
        /// </summary>
        public int OutboundLineId { get; set; }


        /// <summary>
        /// 物料代码
        /// </summary>
        [Required]
        public string MaterialCode { get; set; }


        /// <summary>
        /// 要出库的批号，可以为空
        /// </summary>
        public string? Batch { get; set; }


        /// <summary>
        /// 要出库的库存状态。
        /// </summary>
        [Required]
        public string StockStatus { get; set; }

        /// <summary>
        /// 计量单位。
        /// </summary>
        [Required]
        public string Uom { get; set; } = Cst.None;

        /// <summary>
        /// 需求数量。
        /// </summary>
        [Range(0, int.MaxValue)]
        public decimal QuantityRequired { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Comment { get; set; }
    }


    /// <summary>
    /// 编辑出库单操作的参数
    /// </summary>
    public class EditOutboundOrderArgs
    {
        /// <summary>
        /// 备注
        /// </summary>
        [Required]
        public string Comment { get; set; }


        public List<EditOutboundLineInfo> Lines { get; set; }
    }


}

