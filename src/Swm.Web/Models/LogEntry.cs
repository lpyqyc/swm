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
using System.ComponentModel.DataAnnotations;

namespace Swm.Web
{
    // TODO 使用ELK处理日志，不使用 SQL Server
    /// <summary>
    /// 表示一条日志记录。
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// 日志记录Id
        /// </summary>
        [Key]
        public long LogId { get; set; }

        /// <summary>
        /// 日志消息
        /// </summary>
        public string Message { get; set; } = default!;

        /// <summary>
        /// 消息模板
        /// </summary>
        public string? MessageTemplate { get; set; }

        /// <summary>
        /// 日志级别
        /// </summary>
        public string? Level { get; set; }

        /// <summary>
        /// 日志时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 异常
        /// </summary>
        public string? Exception { get; set; }

        /// <summary>
        /// 附加属性
        /// </summary>
        public string? Properties { get; set; }

        /// <summary>
        /// aspnet请求Id
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// 源上下文
        /// </summary>
        public string? SourceContext { get; set; }

    }

}
