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

using NHibernate.Mapping.ByCode;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Arctic.NHibernateExtensions
{
    /// <summary>
    /// 实现以下约定：
    /// 如果属性或多对一关联上标记了 <see cref="RequiredAttribute"/>，则使数据库字段不可为 null；
    /// 如果属性是值类型且不是可空类型，则使数据库字段不可为 null；
    /// 如果属性上标记了 <see cref="MaxLengthAttribute"/>，则指定使数据库字段的长度；
    /// </summary>
    internal class ModelMapperConvention : IModelMapperConfigurer
    {
        public void ConfigureModelMapper(ModelMapper modelMapper)
        {
            ApplyConventions(modelMapper);
        }

        private ModelMapper ApplyConventions(ModelMapper modelMapper)
        {
            // 多对一
            modelMapper.BeforeMapManyToOne += (insp, prop, map) => {
                // 应用 RequiredAttribute 列不可空
                if (prop.LocalMember.IsDefined(typeof(RequiredAttribute), true))
                {
                    map.NotNullable(true);
                }
            };

            // 属性
            modelMapper.BeforeMapProperty += (insp, prop, map) =>
            {
                // 不可空值类型对应的列也不可空
                PropertyInfo? p = prop.LocalMember as PropertyInfo;
                if (p != null)
                {
                    if (IsValueTypeAndNotNullable(p.PropertyType))
                    {
                        map.NotNullable(true);
                    }
                }

                // 应用 RequiredAttribute 列不可空
                if (prop.LocalMember.IsDefined(typeof(RequiredAttribute), true))
                {
                    map.NotNullable(true);
                }

                // 长度
                var attr = prop.LocalMember.GetCustomAttributes(typeof(MaxLengthAttribute), true).OfType<MaxLengthAttribute>().FirstOrDefault();
                if (attr != null)
                {
                    map.Length(attr.Length);
                }
            };


            return modelMapper;
        }


        private static bool IsValueTypeAndNotNullable(Type type)
        {
            if (type.IsValueType == false)
            {
                return false;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return false;
            }
            return true;
        }
    }
}
