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
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 任务列表的查询参数
    /// </summary>
    public class TaskListArgs
    {
        /// <summary>
        /// 任务号
        /// </summary>
        [SearchArg(SearchMode.Like)]
        public string? TaskCode { get; set; }

        /// <summary>
        /// 托盘号
        /// </summary>
        [SearchArg(SearchMode.Like)]
        [SourceProperty("Unitload.PalletCode")]
        public string? PalletCode { get; set; }

        /// <summary>
        /// 托盘号
        /// </summary>
        [SearchArg(SearchMode.Like)]
        public string? TaskType { get; set; }

        /// <summary>
        /// 起点编号
        /// </summary>
        [SearchArg(SearchMode.Like)]
        [SourceProperty("Start.LocationCode")]
        public string? StartLocationCode { get; set; }

        /// <summary>
        /// 终点编号
        /// </summary>
        [SearchArg(SearchMode.Like)]
        [SourceProperty("End.LocationCode")]
        public string? EndLocationCode { get; set; }

        /// <summary>
        /// 起点或者终点编号
        /// </summary>
        [SearchArg(SearchMode.Expression)]
        public string? AnyLocationCode { get; set; }

        /// <summary>
        /// AnyLocationCode 的查询条件
        /// </summary>
        internal Expression<Func<TransportTask, bool>>? AnyLocationCodeExpr
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(this.AnyLocationCode))
                {
                    return x => x.Start.LocationCode.Like(AnyLocationCode)
                        || x.End.LocationCode.Like(this.AnyLocationCode);
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 物料类型
        /// </summary>
        [SearchArg(SearchMode.Expression)]
        public string? MaterialType { get; set; }

        /// <summary>
        /// MaterialType 的查询条件
        /// </summary>
        internal Expression<Func<TransportTask, bool>>? MaterialTypeExpr
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(MaterialType))
                {
                    return (x => x.Unitload.Items
                        .Select(c => c.Material)
                        .Any(m => m.MaterialType.Like(this.MaterialType))
                        );
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 物料编号
        /// </summary>
        [SearchArg(SearchMode.Expression)]
        public string? MaterialCode { get; set; }

        /// <summary>
        /// MaterialCode 的查询条件
        /// </summary>
        internal Expression<Func<TransportTask, bool>>? MaterialCodeExpr
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(MaterialCode))
                {
                return (x => x.Unitload.Items
                    .Select(c => c.Material)
                    .Any(m => m.MaterialCode.Like(this.MaterialCode))
                    );
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 批号
        /// </summary>
        [SearchArg(SearchMode.Expression)]
        public string? Batch { get; set; }

        /// <summary>
        /// Batch 查询条件
        /// </summary>
        internal Expression<Func<TransportTask, bool>>? BatchExpr
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Batch))
                {
                    return (x => x.Unitload.Items
                        .Any(m => m.Batch.Like(this.Batch))
                        );
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 巷道Id列表
        /// </summary>
        [SearchArg(SearchMode.Expression)]
        public int[]? LanewayIdList { get; set; }

        /// <summary>
        /// LanewayIdList 的查询条件
        /// </summary>
        internal Expression<Func<TransportTask, bool>>? LanewayIdListExpr
        {
            get
            {
                if (LanewayIdList != null && LanewayIdList.Length > 0)
                {
                    return (x =>
                        this.LanewayIdList.Contains(x.Start.Rack.Laneway.LanewayId)
                        || this.LanewayIdList.Contains(x.End.Rack.Laneway.LanewayId)
                        );
                }
                else
                {
                    return null;
                }
            }
        }


    }

}