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

namespace Swm.Model
{
    /// <summary>
    /// 表示操作记录。
    /// </summary>
    public class Op : IHasCtime, IHasCuser
    {
        /// <summary>
        /// 初始化此类的新实例。
        /// </summary>
        public Op()
        {
        }

        /// <summary>
        /// Id
        /// </summary>
        public virtual int OpId { get; protected set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTime ctime { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        [Required]
        [MaxLength(FIELD_LENGTH.USERNAME)]
        public virtual string cuser { get; set; }


        /// <summary>
        /// 操作类型
        /// </summary>
        [Required]
        [MaxLength(FIELD_LENGTH.OPERATION_TYPE)]
        public virtual string OperationType { get; set; }

        /// <summary>
        /// 产生此记录的 Url
        /// </summary>
        public virtual string Url { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [MaxLength(2048)]
        public virtual string Comment { get; set; }

    }

}

