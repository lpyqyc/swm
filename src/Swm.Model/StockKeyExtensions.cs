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
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;

namespace Swm.Model
{
    public static class StockKeyExtensions
    {
        public static TStockKey GetStockKey<TStockKey>(this IHasStockKey hasStockKey) where TStockKey : StockKeyBase
        {
            return (TStockKey)GetStockKey(hasStockKey, typeof(TStockKey));
        }


        private static StockKeyBase GetStockKey(this IHasStockKey hasStockKey, Type stockKeyType)
        {
            if (hasStockKey == null)
            {
                throw new ArgumentNullException(nameof(hasStockKey));
            }

            if (stockKeyType == null)
            {
                throw new ArgumentNullException(nameof(stockKeyType));
            }

            if (typeof(StockKeyBase).IsAssignableFrom(stockKeyType) == false)
            {
                throw new ArgumentException("应为 StockKey 的子类", nameof(stockKeyType));
            }

            var entityProps = hasStockKey.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToArray();

            List<object> values = new List<object>();
            foreach (var keyParam in stockKeyType.GetConstructors()[0].GetParameters())
            {
                var entityProp = entityProps.SingleOrDefault(x => x.Name == keyParam.Name);
                if (entityProp == null)
                {
                    throw new InvalidOperationException($"未找到属性，类型【{hasStockKey.GetType()}】，名称【{keyParam.Name}】");
                }
                object value = entityProp.GetValue(hasStockKey);
                values.Add(value);
            }

            StockKeyBase stockKey = (StockKeyBase)Activator.CreateInstance(stockKeyType, values.ToArray());
            return stockKey;

        }


        public static void SetStockKey<TStockKey>(this IHasStockKey hasStockKey, TStockKey stockKey) where TStockKey : StockKeyBase
        {
            if (hasStockKey == null)
            {
                throw new ArgumentNullException(nameof(hasStockKey));
            }

            if (stockKey == null)
            {
                throw new ArgumentNullException(nameof(stockKey));
            }
            
            var entityProps = hasStockKey.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToArray();
            foreach (var keyProp in stockKey.GetType().GetProperties())
            {
                var entityProp = entityProps.SingleOrDefault(x => x.Name == keyProp.Name);
                if (entityProp == null)
                {
                    throw new InvalidOperationException($"未找到属性，类型【{hasStockKey.GetType()}】，名称【{keyProp.Name}】");
                }
                object value = keyProp.GetValue(stockKey);
                entityProp.SetValue(hasStockKey, value);
            }
        }


    }
}
