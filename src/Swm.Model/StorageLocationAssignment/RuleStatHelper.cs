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

namespace Swm.Model.StorageLocationAssignment
{


    public sealed partial class SAllocationHelper
    {
        //private sealed class RuleStatHelper
        //{
        //    private IEnumerable<string> _allRuleNames;
        //    private static Dictionary<string, LocationAllocRuleStat> _cache;
        //    private static object _cacheLock = new object();
        //    private static DateTime LastFlush = DateTime.MinValue;
        //    ISession _session;
        //    public RuleStatHelper(IEnumerable<string> allRuleNames, ISession session)
        //    {
        //        _allRuleNames = allRuleNames;
        //        _session = session;
        //    }

        //    public void Update(string ruleName, Location target, DateTime time, double duration)
        //    {
        //        lock (_cacheLock)
        //        {
        //            try
        //            {
        //                CreateCache();

        //                var stat = _cache[ruleName];
        //                var succ = (target != null);
        //                stat.TotalTimes++;
        //                stat.TotalDuration += duration;
        //                if (succ)
        //                {
        //                    stat.SuccessTimes++;
        //                }
        //                stat.LastRunTime = time;
        //                stat.LastRunSuccess = succ;
        //                stat.LastRunTarget = target?.LocationCode;
        //                stat.LastRunDuration = duration;

        //                FlushCacheIfNeeded();

        //            }
        //            catch (Exception ex)
        //            {
        //                Log.Error(ex, "更新规则统计数据失败。" + ex.Message);
        //            }
        //        }
        //    }

        //    private void CreateCache()
        //    {
        //        if (_cache == null)
        //        {
        //            using (ITransaction tx = _session.BeginTransaction())
        //            {
        //                _cache = _session
        //                    .Query<LocationAllocRuleStat>()
        //                    .ToArray()
        //                    .ToDictionary(x => x.RuleName, x => x);

        //                tx.Commit();
        //            }

        //            foreach (var ruleName in _allRuleNames)
        //            {
        //                if (_cache.ContainsKey(ruleName) == false)
        //                {
        //                    LocationAllocRuleStat stat = new LocationAllocRuleStat();
        //                    stat.RuleName = ruleName;
        //                    _cache.Add(stat.RuleName, stat);
        //                }
        //            }
        //        }
        //    }

        //    private void FlushCacheIfNeeded()
        //    {
        //        const int minutes = 5;
        //        if (DateTime.Now.Subtract(LastFlush).TotalMinutes >= minutes)
        //        {
        //            using (ITransaction tx = _session.BeginTransaction())
        //            {
        //                foreach (var entry in _cache)
        //                {
        //                    _session.SaveOrUpdate(entry.Value);
        //                }
        //                tx.Commit();
        //            }

        //            LastFlush = DateTime.Now;
        //        }
        //    }
        //}
    }

}
