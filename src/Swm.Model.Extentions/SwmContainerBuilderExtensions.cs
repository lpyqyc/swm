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

using Autofac;
using Serilog;

namespace Swm.Model.Extentions
{
    /// <summary>
    /// 用于向容器注册类型的扩展方法。在 Startup.ConfigureContainer 方法中调用。
    /// </summary>
    public static class ExContainerBuilderExtensions
    {
        static ILogger _logger = Log.ForContext(typeof(SwmContainerBuilderExtensions));

        public static void AddEx(this ContainerBuilder builder)
        {
            builder.RegisterType<TaskSender>().As<ITaskSender>();
        }
    }
}
