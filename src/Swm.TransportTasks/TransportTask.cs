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
using Swm.Locations;
using Swm.Palletization;
using System;
using System.ComponentModel.DataAnnotations;

namespace Swm.TransportTasks
{
    /// <summary>
    /// 表示任务。
    /// </summary>
    public class TransportTask : IHasCtime, IUnitloadTransportTask
    {
        /// <summary>
        /// 初始化新实例。
        /// </summary>
        public TransportTask()
        {
            this.ctime = DateTime.Now;
        }

        /// <summary>
        /// Id
        /// </summary>
        public virtual int TaskId { get; internal protected set; }

        /// <summary>
        /// 编码。
        /// </summary>
        [Required]
        [MaxLength(20)]
        public virtual string? TaskCode { get; internal protected set; }

        /// <summary>
        /// 获取或设置移动类型，当移动完成时，使用此属性来决定分配给哪个处理程序。
        /// </summary>
        [Required]
        [MaxLength(20)]
        public virtual string? TaskType { get; set; }

        /// <summary>
        /// 创建时间。
        /// </summary>
        public virtual DateTime ctime { get; set; }


        /// <summary>
        /// 获取或设置此任务搬运的货载。
        /// </summary>
        [Required]
        public virtual Unitload? Unitload { get; internal protected set; }


        /// <summary>
        /// 获取或设置此任务的起点位置。
        /// </summary>
        [Required]
        public virtual Location? Start { get; internal protected set; }


        /// <summary>
        /// 获取或设置此任务的终点位置。
        /// </summary>
        [Required]
        public virtual Location? End { get; internal protected set; }


        /// <summary>
        /// 指示此任务是否需要 Wcs 执行，若为 true，则会下发给 Wcs 执行，否则不会下发给 Wcs。
        /// 在平库场景或纠正错误库存时使用。
        /// </summary>
        public virtual bool ForWcs { get; set; }

        /// <summary>
        /// 指示此任务是否已下发给 Wcs。
        /// </summary>
        public virtual bool WasSentToWcs { get; set; }

        /// <summary>
        /// 获取货设置任务下发的时间。
        /// </summary>
        public virtual DateTime SendTime { get; set; }

        /// <summary>
        /// 获取或设置此任务是为哪个Wms单据产生的。
        /// </summary>
        [MaxLength(20)]
        public virtual string? OrderCode { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public virtual string? Comment { get; set; }

        /// <summary>
        /// 备用字段1
        /// </summary>
        [MaxLength(9999)]
        public virtual string? ex1 { get; set; }

        /// <summary>
        /// 备用字段2
        /// </summary>
        [MaxLength(9999)]
        public virtual string? ex2 { get; set; }

        public override string? ToString()
        {
            return $"{this.TaskType} {this.TaskCode}";
        }

    }
}
