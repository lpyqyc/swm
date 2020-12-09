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
using System.ComponentModel.DataAnnotations;

namespace Swm.Model
{
    public class Rack
    {
        /// <summary>
        /// 初始化 Rack 类的新实例。
        /// </summary>
        public Rack()
        {
            this.Locations = new HashSet<Location>();
        }


        /// <summary>
        /// Id
        /// </summary>
        public virtual Int32 RackId { get; internal protected set; }

        /// <summary>
        /// 货架的编码。自然键。
        /// </summary>
        [Required]
        [MaxLength(16)]
        public virtual String RackCode { get; set; }

        /// <summary>
        /// 货架所属的巷道。
        /// </summary>
        [Required]
        public virtual Laneway Laneway { get; set; }


        /// <summary>
        /// 指示货架在巷道哪一侧。
        /// </summary>
        public virtual RackSide Side { get; set; }

        /// <summary>
        /// 指示货架的深度。
        /// </summary>
        public virtual RackDeep Deep { get; set; }

        /// <summary>
        /// 列数
        /// </summary>
        public virtual Int32 Columns { get; set; }

        /// <summary>
        /// 层数
        /// </summary>
        public virtual Int32 Levels { get; set; }

        /// <summary>
        /// 备注。
        /// </summary>
        public virtual String Comment { get; set; }

        /// <summary>
        /// 此货架上的货位，Exists 为 False 的货位也在其中。
        /// </summary>
        public virtual ISet<Location> Locations { get; protected set; }
    }

}
