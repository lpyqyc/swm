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
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Event;
using NHibernate.Logging.Serilog;
using NHibernate.Mapping.ByCode;
using Serilog;
using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace Arctic.NHibernateExtensions
{
    public static class NHibernateContainerBuilderExtensions
    {
        static readonly ILogger _logger = Log.ForContext(typeof(NHibernateContainerBuilderExtensions));

        /// <summary>
        /// 设置 NHiberernate 使用 <see cref="SerilogLoggerFactory"/>；
        /// 使用 hibernate.cfg.xml 配置 NHibernate；
        /// 从容器解析 <see cref="ModelMapperExtensions"/> 添加到 <see cref="Configuration"/>；
        /// 根据选项添加 <see cref="CheckTransactionListener"/>；
        /// 向容器注册 <see cref="Configuration"/>；
        /// 向容器注册添加了 <see cref="AuditInterceptor"/> 的 <see cref="ISessionFactory"/>；
        /// 向容器注册 <see cref="ISession"/>；
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options">配置选项。</param>
        public static void AddNHibernate(this ContainerBuilder builder)
        {
            _logger.Information("正在配置 NHibernate");

            NHibernateLogger.SetLoggersFactory(new SerilogLoggerFactory());
            _logger.Information("使用 LoggerFactory: {loggerFactory}", typeof(SerilogLoggerFactory));

            builder.AddModelMapperConfigurer(new ModelMapperConvention());

            builder.Register(c =>
            {
                Configuration configuration = new Configuration();
                configuration.Configure();
                configuration.SetNamingStrategy(ImprovedNamingStrategy.Instance);

                var mapper = new ModelMapper();

                // 添加映射
                {
                    var modelMapperConfigurers = c.Resolve<IEnumerable<IModelMapperConfigurer>>();
                    foreach (var configurer in modelMapperConfigurers)
                    {
                        configurer.ConfigureModelMapper(mapper);
                    }
                    var mappings = mapper.CompileMappingForEachExplicitlyAddedEntity();
                    foreach (var mapping in mappings)
                    {
                        configuration.AddMapping(mapping);
                    }
                }
                // 开始：nh 事件，检查事务，要求必须打开事务
                CheckTransactionListener checkTransactionListener = new CheckTransactionListener();
                configuration.AppendListeners(ListenerType.PreInsert, new IPreInsertEventListener[] { checkTransactionListener });
                configuration.AppendListeners(ListenerType.PreUpdate, new IPreUpdateEventListener[] { checkTransactionListener });
                configuration.AppendListeners(ListenerType.PreDelete, new IPreDeleteEventListener[] { checkTransactionListener });
                configuration.AppendListeners(ListenerType.PreLoad, new IPreLoadEventListener[] { checkTransactionListener });
                // 结束：nh 事件

                _logger.Information("向 NHibernate.Cfg.Configuration 添加了事件侦听程序 CheckTransactionListener");

                return configuration;

            }).SingleInstance();

            builder.Register(c =>
            {
                // 生成 SessionFactory
                var configuration = c.Resolve<Configuration>();
                ISessionFactory sessionFactory = configuration.BuildSessionFactory();
                return sessionFactory;
            }).SingleInstance().ExternallyOwned();

            builder.Register(c => 
            {
                var principal = c.Resolve<IPrincipal>();
                AuditInterceptor interceptor = new AuditInterceptor(principal);
                return c.Resolve<ISessionFactory>()
                    .WithOptions()
                    // 添加审计拦截器
                    .Interceptor(interceptor)
                    .OpenSession();
            }).InstancePerLifetimeScope();


            _logger.Information("已配置 NHibernate");
        }
    
    
        /// <summary>
        /// 向容器添加 <see cref="ModelMapper"/> 的配置程序。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        public static void AddModelMapperConfigurer(this ContainerBuilder builder, IModelMapperConfigurer modelMapperConfigurer)
        {
            builder.RegisterInstance(modelMapperConfigurer).As<IModelMapperConfigurer>().SingleInstance();
        }

        public static void RegisterEntityFactory<TEntity>(this ContainerBuilder builder, Func<TEntity> factory) where TEntity : notnull, new()
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            builder.Register(c => factory.Invoke()).As<TEntity>().InstancePerDependency();
        }
    }
}