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
using System.Linq;

namespace Swm.Model
{
    /// <summary>
    /// 表示一个位置，位置的本质是货物停留点和移动的起止点。
    /// </summary>
    public class Location : IHasCtime, IHasMtime
    {
        /// <summary>
        /// 初始化位置类的新实例。
        /// </summary>
        internal protected Location()
        {
            this.ctime = DateTime.Now;
            this.mtime = DateTime.Now;
            this.Exists = true;
            this.StorageGroup = Cst.NA;
            this.RequestType = Cst.NA;
            this.Specification = Cst.NA;
        }

        /// <summary>
        /// 主键
        /// </summary>
        public virtual Int32 LocationId { get; internal protected set; }

        /// <summary>
        /// 获取或设置此位置的编码。这是自然键。
        /// </summary>
        [Required]
        [MaxLength(16)]
        public virtual String LocationCode { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public virtual int v { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTime ctime { get; set; }

        /// <summary>
        /// 更新时间。
        /// </summary>
        public virtual DateTime mtime { get; set; }


        /// <summary>
        /// 指示此位置是否是货架货位。
        /// </summary>
        [MaxLength(4)]
        [Required]
        public virtual string LocationType { get; internal protected set; }


        /// <summary>
        /// 获取或设置此货位的入站数，已生成未下发的任务也计算在内。
        /// </summary>
        public virtual int InboundCount { get; internal protected set; }


        /// <summary>
        /// 获取或设置此货位的入站数限制。当入站数达到此值时，不允许生成新的入站。
        /// 此属性用来限制任务的生成，而不用来限制任务的下发。
        /// </summary>
        [Range(1, 999)]
        public virtual int InboundLimit { get; set; }

        /// <summary>
        /// 获取或设置此位置是否禁止入站。
        /// </summary>
        /// <remarks>
        /// 若为 true，则不允许生成新的以此位置为终点的任务。
        /// 已存在的任务不受影响，可以正常完成。
        /// </remarks>
        public virtual Boolean InboundDisabled { get; set; }

        /// <summary>
        /// 获取或设置此位置禁止入站的备注。
        /// </summary>
        public virtual String InboundDisabledComment { get; set; }

        /// <summary>
        /// 获取或设置此货位的出站数，已生成未下发的任务也计算在内。
        /// </summary>
        public virtual int OutboundCount { get; internal protected set; }


        /// <summary>
        /// 获取或设置此货位的最大出站数。当出站数达到此值时，不允许生成新的出站任务。
        /// 此属性用来限制任务的生成，而不用来限制任务的下发。
        /// </summary>
        [Range(1, 999)]
        public virtual int OutboundLimit { get; set; }

        /// <summary>
        /// 获取或设置此位置是否禁止出站。
        /// </summary>
        /// <remarks>
        /// 若为 true，则不允许生成新的以此位置为起点的任务。
        /// 已存在的任务不受影响，可以正常完成。
        /// </remarks>
        public virtual Boolean OutboundDisabled { get; set; }

        /// <summary>
        /// 获取或设置此位置禁止出站的备注。
        /// </summary>
        public virtual String OutboundDisabledComment { get; set; }

        /// <summary>
        /// 获取或设置此位置此货位是否存在。
        /// </summary>
        public virtual Boolean Exists { get; set; }


        /// <summary>
        /// 此货位的限重，以千克为单位。货物重量大于此值时不能分配此货位。<see cref="Unitload.Weight"/>。
        /// </summary>
        public virtual decimal WeightLimit { get; set; }

        /// <summary>
        /// 此货位的限高，以米为单位。货物高度大于此值时不能分配此货位。<see cref="Unitload.Height"/>。
        /// </summary>
        public virtual decimal HeightLimit { get; set; }

        /// <summary>
        /// 指示此货位的规格（不含高度），例如【九角1200x1100】,此属性与 <see cref="UnitloadStorageInfo.ContainerSpecification"/> 配合使用，影响入库分配货位。
        /// </summary>
        [MaxLength(16)]
        public virtual string Specification { get; set; }


        /// <summary>
        /// 指示货位属于哪个巷道
        /// </summary>
        public virtual Laneway Laneway { get; set; }

        /// <summary>
        /// 指示货位在巷道哪一侧。
        /// </summary>
        public virtual RackSide Side { get; set; }

        /// <summary>
        /// 指示货位属于第几深位。
        /// </summary>
        public virtual RackDeep Deep { get; set; }


        /// <summary>
        /// 此位置的列。
        /// </summary>
        public virtual int Column { get; internal protected set; }

        /// <summary>
        /// 此位置的层。
        /// </summary>
        public virtual int Level { get; internal protected set; }

        /// <summary>
        /// 获取或设置此货位的存储分组。
        /// </summary>
        [MaxLength(10)]
        [Required]
        public virtual string StorageGroup { get; set; }

        /// <summary>
        /// 指示此货位上的货载数，此属性不适用于 <see cref="LocationTypes.N"/> 类型的货位。
        /// 货载正在出站但任务未完成时，此计数保持不变，直到出站任务完成时才更新计数。
        /// </summary>
        public virtual int UnitloadCount { get; internal protected set; }


        /// <summary>
        /// 获取或设置此货位所在的单元格。仅适用于货架货位。
        /// </summary>
        public virtual Cell Cell { get; set; }

        /// <summary>
        /// 获取或设置此位置的标记。
        /// </summary>
        [MaxLength(30)]
        public virtual string Tag { get; set; }

        /// <summary>
        /// 获取或设置此位置上的请求类型，仅适用于关键点。
        /// </summary>
        [MaxLength(16)]
        public virtual string RequestType { get; set; }

        public virtual Location GetDeep1()
        {
            if (this.LocationType != LocationTypes.S)
            {
                throw new InvalidOperationException();
            }

            if (this.Laneway.DoubleDeep == false)
            {
                throw new InvalidOperationException();
            }

            if (this.Deep != RackDeep.Deep2)
            {
                string errMsg = string.Format("{0} 不是二深货位。", this.LocationCode);
                throw new InvalidOperationException(errMsg);
            }

            return this.Cell.Locations.Single(x => x.Deep == RackDeep.Deep1);
        }


        public virtual Location GetDeep2()
        {
            if (this.LocationType != LocationTypes.S)
            {
                throw new InvalidOperationException();
            }

            if (this.Laneway.DoubleDeep == false)
            {
                throw new InvalidOperationException();
            }

            if (this.Deep != RackDeep.Deep1)
            {
                string errMsg = string.Format("{0} 不是一深货位。", this.LocationCode);
                throw new InvalidOperationException(errMsg);
            }

            return this.Cell.Locations.Single(x => x.Deep == RackDeep.Deep2);
        }


        /// <summary>
        /// 获取一个值指示此货位是否有货。
        /// </summary>
        /// <returns></returns>
        public virtual bool Loaded()
        {
            return this.UnitloadCount > 0;
        }

        /// <summary>
        /// 获取一个值指示此货位是否可用。货位可用是指货位无货且未禁止入站。
        /// 货位的入站数和出站数不影响此方法的返回值。
        /// </summary>
        /// <returns></returns>
        public virtual bool Available()
        {
            return Loaded() == false
                && InboundDisabled == false;
        }


        public virtual void IncreaseUnitloadCount()
        {
            if (LocationType == LocationTypes.N)
            {
                throw new InvalidOperationException("不能为 N 位置调用此方法。");
            }

            var prevLoaded = Loaded();
            var prevAvail = Available();
            UnitloadCount++;

            if (LocationType == LocationTypes.S)
            {
                Cell.UpdateState();

                // 更新巷道使用数据
                var key = new LanewayUsageKey
                {
                    StorageGroup = StorageGroup,
                    Specification = Specification,
                    WeightLimit = WeightLimit,
                    HeightLimit = HeightLimit,
                };
                var usage = Laneway.Usage;
                if (usage.ContainsKey(key))
                {
                    var loaded = Loaded();
                    var avail = Available();

                    if (prevLoaded != loaded)
                    {
                        usage[key].Loaded++;
                        usage[key].mtime = DateTime.Now;
                    }
                    if (prevAvail != avail)
                    {
                        usage[key].Available--;
                        usage[key].mtime = DateTime.Now;
                    }
                }
            }
        }



        public virtual void DecreaseUnitloadCount()
        {
            if (LocationType == LocationTypes.N)
            {
                throw new InvalidOperationException("不能为 N 位置调用此方法。");
            }

            if (UnitloadCount == 0)
            {
                throw new InvalidOperationException($"{nameof(UnitloadCount)} 不能小于 0。");
            }

            var prevLoaded = Loaded();
            var prevAvail = Available();
            UnitloadCount--;

            if (LocationType == LocationTypes.S)
            {
                Cell.UpdateState();

                var key = new LanewayUsageKey
                {
                    StorageGroup = StorageGroup,
                    Specification = Specification,
                    WeightLimit = WeightLimit,
                    HeightLimit = HeightLimit,
                };

                var usage = Laneway.Usage;
                if (usage.ContainsKey(key))
                {
                    var loaded = Loaded();
                    var avail = Available();

                    if (prevLoaded != loaded)
                    {
                        usage[key].Loaded--;
                        usage[key].mtime = DateTime.Now;
                    }
                    if (prevAvail != avail)
                    {
                        usage[key].Available++;
                        usage[key].mtime = DateTime.Now;
                    }
                }
            }
        }



        public override string ToString()
        {
            return this.LocationCode;
        }
    }


}
