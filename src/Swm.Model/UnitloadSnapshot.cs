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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Swm.Model
{
    // TODO 移到 Tasks 里，因为只有归档任务需要这个类，是归档任务的附属数据
    /// <summary>
    /// 表示单元货物的快照。
    /// </summary>
    /// <remarks>
    /// 货载快照用来代替 ArchivedUnitload 和 UnionUnitload，为历史任务保存库存数据。
    /// 每次任务完成时，都会创建一个货载快照供任务引用。
    /// 
    /// 具体影响：
    /// （1）在模型设计上可以删除 ArchivedUnitload 和 UnionUnitload 类，早期版本引用 UnionUnitload 的类有两个：ArchivedKeeping 和 ArchivedTask，
    /// 前者已从模型中删除，而 ArchivedTask 改为使用快照，UnionUnitload 不被任何类引用，不需要在模型中保留，ArchivedUnitload 仅作为
    /// UnionUnitload 的组成部分存在，当 UnionUnitload 删除后，ArchivedUnitload 与 UnionUnitload 关联的视图也不再需要。视图无法同 nh 自动导出，经常出现忘记维护，运行时报错的问题。
    /// （2）不需要将货载作为不可变对象处理。早期版本若修改了货载数据，查看历史任务时，
    /// 看到的货载数据不是生成任务时的版本，为了保持历史任务能够看到当时的货载数据，货载必须作为不可变对象对待，
    /// 每次货载上的变化要通过【归档→复制→修改】的复杂流程完成。受影响的功能有：
    ///    * 注册
    ///    * 拣货
    ///    * 转换
    ///    * 盘盈盘亏
    ///    * 托盘历史查询：暂时放弃，可以通过流水实现，但似乎没有历史货载方式有效，此功能使用价值不大，可暂时放弃。
    ///    * 每次在货载发生任何变动时，应重新计算 StorageInfo 
    /// （3）数据库空间：根据爱博斯和中山中荣的历史数据估算，货载快照的数据库空间消耗时归档货载方式的3倍，。
    /// </remarks>
    public class UnitloadSnapshot
    {
        internal protected UnitloadSnapshot()
        {
            Items = new HashSet<UnitloadItemSnapshot>();
        }

        
        public virtual int UnitloadSnapshotId { get; internal protected set; }

        /// <summary>
        /// 快照的创建时间。
        /// </summary>
        public virtual DateTime SnapshotTime { get; set; }

        /// <summary>
        /// 源货载的Id
        /// </summary>
        public virtual int UnitloadId { get; internal protected set; }

        [Required]
        [MaxLength(20)]
        public virtual string ContainerCode { get; set; }

        /// <summary>
        /// 源货载的创建时间。此属性不是 <see cref="IHasCtime.ctime"/> 的实现。
        /// </summary>
        public virtual DateTime ctime { get; set; }

        /// <summary>
        /// 源货载的修改时间。此属性不是 <see cref="IHasMtime.mtime"/> 的实现。
        /// </summary>
        public virtual DateTime mtime { get; set; }

        [MaxLength(FIELD_LENGTH.USERNAME)]
        public virtual string cuser { get; set; }


        public virtual UnitloadStorageInfo StorageInfo { get; set; }

        public virtual bool HasCountingError { get; set; }

        public virtual string Comment { get; set; }

        public virtual ISet<UnitloadItemSnapshot> Items { get; protected set; }
        
        #region 维护 Items 集合

        public virtual void AddItem(UnitloadItemSnapshot item)
        {
            item.Unitload = this;
            this.Items.Add(item);
        }


        public virtual void RemoveItem(UnitloadItemSnapshot item)
        {
            item.Unitload = null;
            this.Items.Remove(item);
        }

        #endregion
    }
}
