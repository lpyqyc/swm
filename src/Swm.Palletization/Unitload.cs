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
using Swm.Locations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Swm.Palletization
{
    /// <summary>
    /// 表示单元货物，又称单元货载，简称货载。货载分为三种类型：
    /// * 空货载：Items 集合元素个数为 0；
    /// * 标准货载：Items 集合元素个数为 1；
    /// * 混合货载：Items 集合元素个数大于 1；
    /// </summary>
    public class Unitload : IHasCtime, IHasCuser, IHasMtime
    {
        private ISet<UnitloadItem> _items;

        internal protected Unitload()
        {
            this.ctime = DateTime.Now;
            this.mtime = DateTime.Now;
            this.StorageInfo = new StorageInfo();
            _items = new HashSet<UnitloadItem>();
        }

        public virtual int UnitloadId { get; internal protected set; }

        [Required]
        [MaxLength(20)]
        public virtual string? PalletCode { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:命名样式", Justification = "特殊属性")]
        public virtual int v { get; set; }

        public virtual DateTime ctime { get; set; }

        public virtual DateTime mtime { get; set; }

        [MaxLength(FIELD_LENGTH.USERNAME)]
        public virtual string? cuser { get; set; }

        public virtual StorageInfo StorageInfo { get; set; }

        public virtual bool HasCountingError { get; set; }

        public virtual bool Odd { get; set; }

        public virtual IEnumerable<UnitloadItem> Items
        {
            get
            {
                return _items;
            }
            protected set
            {
                _items = (ISet<UnitloadItem>)value;
            }
        }

        public virtual bool BeingMoved { get; set; }


        [Required]
        public virtual Location? CurrentLocation { get; internal protected set; }


        public virtual DateTime CurrentLocationTime { get; internal protected set; }


        public virtual IUnitloadAllocationTable? CurrentUat { get; protected set; }

        /// <summary>
        /// CurrentUat 的类型，用于帮助映射 <see cref="IUnitloadAllocationTable.Unitloads"/> 属性。
        /// </summary>
        /// <remarks>
        /// 映射 <see cref="CurrentUat"/> 属性的 any 元素的 CurrentUatType 列，取值可能是基类名称，也有可能是子类名称，CurrentUatRootType 是为了提供固定不变的值。
        /// </remarks>
        [MaxLength(50)]
        public virtual string? CurrentUatRootType { get; protected set; }


        /// <summary>
        /// 设置 <see cref="CurrentUat"/> 和 <see cref="CurrentUatRootType"/> 属性。
        /// </summary>
        /// <param name="uat"></param>
        /// <param name="uatRootType"></param>
        public virtual void SetCurrentUat(IUnitloadAllocationTable uat, string uatRootType)
        {
            if (uat == null)
            {
                throw new ArgumentNullException(nameof(uat));
            }

            if (this.CurrentUat != null && this.CurrentUat != uat)
            {
                throw new InvalidOperationException("已分配给其他单据");
            }

            this.CurrentUat = uat;
            this.CurrentUatRootType = uatRootType ?? throw new ArgumentNullException(nameof(uatRootType));
        }

        /// <summary>
        /// 清除 <see cref="CurrentUat"/> 和 <see cref="CurrentUatRootType"/> 属性。
        /// </summary>
        public virtual void ResetCurrentUat()
        {
            this.CurrentUat = null;
            this.CurrentUatRootType = null;
        }

        /// <summary>
        /// 获取托盘的当前任务。
        /// </summary>
        /// <returns></returns>
        public virtual IUnitloadTransportTask? CurrentTask { get; set; }


        // TODO 重命名
        [MaxLength(20)]
        public virtual string? OpHintType { get; protected set; }

        [MaxLength(20)]
        public virtual string? OpHintInfo { get; protected set; }

        public virtual string? Comment { get; set; }

        public virtual void SetOpHint(string opHintType, string opHintInfo)
        {
            if (string.IsNullOrWhiteSpace(this.OpHintType))
            {
                throw new InvalidOperationException("货载上有未清除的操作提示。");
            }

            this.OpHintType = opHintType;
            this.OpHintInfo = opHintInfo;
        }


        public virtual void ResetOpHint()
        {
            this.OpHintType = null;
            this.OpHintInfo = null;
        }

        public virtual void AddItem(UnitloadItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (item.Unitload != null)
            {
                throw new InvalidOperationException("向货载添加货载项失败，货载项已属于其他货载。");
            }
            _items.Add(item);
            item.Unitload = this;

        }


        public virtual void RemoveItem(UnitloadItem item)
        {
            if (this.Items.Contains(item) == false)
            {
                throw new InvalidOperationException("项不在这个货载里。");
            }

            item.Unitload = null!;
            _items.Remove(item);
        }

        /// <summary>
        /// 获取一个值，指示此货载是否在货架上。有下架任务的货载，在任务完成前，也算作在货架上。
        /// </summary>
        /// <returns></returns>
        public virtual bool InRack()
        {
            return this.CurrentLocation?.LocationType == LocationTypes.S;
        }

        public override string? ToString()
        {
            return this.PalletCode;
        }


        public virtual void Enter(Location target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (this.CurrentLocation == target)
            {
                throw new InvalidOperationException("货载已在目标位置上。");
            }

            if (this.CurrentLocation != null)
            {
                throw new InvalidOperationException("应先将货载的位置设为 null。");
            }

            this.SetCurrentLocation(target);

            if (target.LocationType != LocationTypes.N)
            {
                target.IncreaseUnitloadCount();
            }
        }



        public virtual void LeaveCurrentLocation()
        {
            if (this.CurrentLocation == null)
            {
                throw new InvalidOperationException("货载不在任何位置上。");
            }
            var loc = this.CurrentLocation;
            this.SetCurrentLocation(null);

            if (loc.LocationType != LocationTypes.N)
            {
                loc.DecreaseUnitloadCount();
            }
        }


        private void SetCurrentLocation(Location? location)
        {
            if (this.CurrentLocation != location)
            {
                this.CurrentLocation = location;
                this.CurrentLocationTime = DateTime.Now;
            }
        }

    }

}
