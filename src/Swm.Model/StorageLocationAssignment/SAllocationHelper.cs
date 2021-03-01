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

using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Swm.Model.StorageLocationAssignment
{
    /// <summary>
    /// 单叉巷道分配货位帮助类。
    /// </summary>
    public sealed partial class SAllocationHelper
    {
        readonly IEnumerable<IRule> _rules;
        readonly ILogger _logger;
        //ILifetimeScope _lifetimeScope;

        public SAllocationHelper(IEnumerable<IRule> rules, ILogger logger
            //, ILifetimeScope lifetimeScope
            )
        {
            _rules = rules;
            _logger = logger;
            //_lifetimeScope = lifetimeScope;
        }

        /// <summary>
        /// 在指定的巷道和分组中分配一个货位以供入库。
        /// </summary>
        /// <param name="laneway">要在其中分配货位的巷道。</param>
        /// <param name="excludedIdList">要排除的货位。</param>
        /// <param name="excludedColumnList">要排除的列。</param>
        /// <param name="excludedLevelList">要排除的层。</param>
        /// <param name="cargoInfo">入库的货物信息。</param>
        /// <param name="orderBy">排序依据。这是 LocationUnit 类的属性名。</param>
        /// <returns>
        /// 从不返回 null。
        /// </returns>
        public async Task<SResult> AllocateAsync(
            Laneway laneway,
            UnitloadStorageInfo cargoInfo,
            int[] excludedIdList = null,
            int[] excludedColumnList = null,
            int[] excludedLevelList = null,
            string orderBy = "i1")
        {
            if (laneway == null)
            {
                throw new ArgumentNullException(nameof(laneway));
            }

            if (string.IsNullOrWhiteSpace(orderBy))
            {
                throw new ArgumentException("参数 orderBy 不能为 null 或空字符串。");
            }

            _logger.Debug("巷道 {lanewayCode}", laneway.LanewayCode);

            var rules = _rules.Where(x => x.DoubleDeep == laneway.DoubleDeep).OrderBy(x => x.Order);
            Stopwatch sw = new Stopwatch();
            foreach (var rule in rules)
            {
                _logger.Debug("正在测试 {ruleName}", rule.Name);
                sw.Restart();
                var loc = await rule.SelectAsync(laneway, cargoInfo, excludedIdList, excludedColumnList, excludedLevelList, orderBy).ConfigureAwait(false);
                sw.Stop();

                //// TODO 涉及巷道过多时，会打开很多session，需优化
                //ThreadPool.QueueUserWorkItem(state => 
                //{
                //    using (ILifetimeScope scope = _lifetimeScope.BeginLifetimeScope())
                //    {
                //        var ruleNames = _rules.Select(x => x.Name);
                //        RuleStatHelper ruleStatHelper = scope.Resolve<RuleStatHelper>(TypedParameter.From(ruleNames));
                //        ruleStatHelper.Update(rule.Name, loc, DateTime.Now, sw.ElapsedMilliseconds);
                //    }
                //});
                if (loc != null)
                {
                    _logger.Debug("{ruleName} 成功分配到货位 {locationCode}", rule.Name, loc.LocationCode);
                    return SResult.MakeSuccess(loc);
                }
                else
                {
                    _logger.Debug("{ruleName} 失败", rule.Name);
                }
            }

            return SResult.Failure;
        }
    }

}
