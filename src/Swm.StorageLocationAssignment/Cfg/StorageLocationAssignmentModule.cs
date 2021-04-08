﻿// Copyright 2020-2021 王建军
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
    /// 向容器注册货位分配的服务。
    /// </summary>
    internal class StorageLocationAssignmentModule : Autofac.Module
    {
        static ILogger _logger = Log.ForContext<StorageLocationAssignmentModule>();

        StorageLocationAssignmentModuleBuilder _moduleBuilder;

        internal StorageLocationAssignmentModule(StorageLocationAssignmentModuleBuilder moduleBuilder)
        {
            _moduleBuilder = moduleBuilder;
        }


        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SAllocationHelper>().AsSelf();
            foreach (var ruleType in _moduleBuilder._ruleTypes)
            {
                builder.RegisterType(ruleType);
                _logger.Information("已注册分配货位规则：{ruleType}", ruleType);
            }
        }
    }
}