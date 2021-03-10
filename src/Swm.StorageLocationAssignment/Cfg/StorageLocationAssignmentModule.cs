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
using System;
using System.Reflection;

namespace Swm.StorageLocationAssignment
{
    /// <summary>
    /// 用于向容器注册类型的扩展方法。在 Startup.ConfigureContainer 方法中调用。
    /// </summary>
    public class StorageLocationAssignmentModule : Autofac.Module
    {
        static ILogger _logger = Log.ForContext<StorageLocationAssignmentModule>();

        protected override void Load(ContainerBuilder builder)
        {
            RegisterBySuffix("Helper");
            RegisterByPrefix("SSRule");

            void RegisterBySuffix(string suffix)
            {
                var asm = Assembly.GetExecutingAssembly();
                builder.RegisterAssemblyTypes(asm)
                    .Where(t => t.IsAbstract == false && t.Name.EndsWith(suffix, StringComparison.Ordinal))
                    .AsImplementedInterfaces()
                    .AsSelf();
                _logger.Information("已注册后缀 {suffix}", suffix);
            }


            void RegisterByPrefix(string prefix)
            {
                var asm = Assembly.GetExecutingAssembly();
                builder.RegisterAssemblyTypes(asm)
                    .Where(t => t.IsAbstract == false && t.Name.StartsWith(prefix, StringComparison.Ordinal))
                    .AsImplementedInterfaces()
                    .AsSelf();
                _logger.Information("已注册前缀 {suffix}", prefix);
            }
        }

    }

}