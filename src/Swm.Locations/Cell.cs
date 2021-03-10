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
using System.Linq;

namespace Swm.Locations
{


    /// <summary>
    /// 货位单元。
    /// </summary>
    public class Cell
    {
        protected Cell()
        {
            this.Locations = new HashSet<Location>();
            this.Laneway = default!;
        }
        /// <summary>
        /// 初始化新实例
        /// </summary>
        public Cell(Laneway laneway)
        {
            this.Laneway = laneway;
            this.Locations = new HashSet<Location>();
        }

        /// <summary>
        /// Id
        /// </summary>
        public virtual int CellId { get; protected set; }

        /// <summary>
        /// 此货位单元所属的巷道。
        /// </summary>
        [Required]
        public virtual Laneway Laneway { get; protected set; }

        /// <summary>
        /// 此货位单元在巷道的哪一侧。
        /// </summary>
        public virtual RackSide Side { get; set; }


        /// <summary>
        /// 此货位单元所在的列，定义为 n1 所在的列。
        /// </summary>
        public virtual int Column { get; set; }

        /// <summary>
        /// 此货位单元所在的层，定义为 n1 所在的层。
        /// </summary>
        public virtual int Level { get; set; }

        /// <summary>
        /// 获取或设置此货位单元的形态。
        /// 形态由若干位表示，数字 0 表示无货，数字 1 表示有货。
        /// 单叉单深单元的形态在 SSUnitShape 枚举中定义，
        /// 单叉双深单元的形态在 SDUnitShape 枚举中定义，
        /// </summary>
        [Required]
        [MaxLength(10)]
        public virtual string? Shape { get; internal protected set; }

        /// <summary>
        /// 获取或设置此货位单元按形态决定的入次序。
        /// </summary>
        public virtual int iByShape { get; set; }


        /// <summary>
        /// 获取或设置此货位单元按形态决定的出次序。
        /// </summary>
        public virtual int oByShape { get; set; }

        /// <summary>
        /// 获取此单元中的货位。
        /// </summary>
        public virtual ISet<Location> Locations { get; protected set; }


#pragma warning disable IDE1006 // 命名样式

        /// <summary>
        /// 入次序 1。在分配货位时使用。
        /// </summary>
        public virtual int i1 { get; set; }

        /// <summary>
        /// 出次序 1。在分配库存和下架时使用。
        /// </summary>
        public virtual int o1 { get; set; }

        /// <summary>
        /// 入次序 2。在分配货位时使用。
        /// </summary>
        public virtual int i2 { get; set; }

        /// <summary>
        /// 出次序 2。在分配库存和下架时使用。
        /// </summary>
        public virtual int o2 { get; set; }

        /// <summary>
        /// 入次序 3。在分配货位时使用。
        /// </summary>
        public virtual int i3 { get; set; }

        /// <summary>
        /// 出次序 3。在分配库存和下架时使用。
        /// </summary>
        public virtual int o3 { get; set; }

#pragma warning restore IDE1006 // 命名样式


        /// <summary>
        /// 更新此单元格的状态。
        /// </summary>
        public virtual void UpdateState()
        {
            if (this.Laneway.DoubleDeep)
            {
                this.UpdateSD();
            }
            else
            {
                this.UpdateSS();
            }
        }


        void UpdateSS()
        {
            this.Shape = "SS_" + (this.Locations.Single().Loaded() ? "1" : "0");
            SSUnitShape shape = (SSUnitShape)Enum.Parse(typeof(SSUnitShape), this.Shape);
            switch (shape)
            {
                case SSUnitShape.SS_0:
                    this.iByShape = 200;
                    this.oByShape = 10000;
                    break;
                case SSUnitShape.SS_1:
                    this.iByShape = 10000;
                    this.oByShape = 300;
                    break;
                default:
                    throw new ApplicationException("枚举值无效。");
            }
        }

        void UpdateSD()
        {
            this.Shape = "SD_" + (this.Locations.Single(x => x.Deep == 1).Loaded() ? "1" : "0")
                + (this.Locations.Single(x => x.Deep == 2).Loaded() ? "1" : "0");
            SDUnitShape shape = (SDUnitShape)Enum.Parse(typeof(SDUnitShape), this.Shape);
            switch (shape)
            {
                case SDUnitShape.SD_00:
                    this.iByShape = 200;
                    this.oByShape = 10000;
                    break;
                case SDUnitShape.SD_01:
                    this.iByShape = 100;
                    this.oByShape = 200;
                    break;
                case SDUnitShape.SD_10:
                    this.iByShape = 10000;
                    this.oByShape = 100;
                    break;
                case SDUnitShape.SD_11:
                    this.iByShape = 10000;
                    this.oByShape = 300;
                    break;
                default:
                    throw new ApplicationException("枚举值无效。");
            }
        }
    }

}
