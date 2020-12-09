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

using Serilog;
using System;

namespace Swm.Model
{
    public class UnitloadSnapshopHelper
    {
        readonly IUnitloadSnapshotFactory _unitloadSnapshotFactory;
        readonly ILogger _logger;

        public UnitloadSnapshopHelper(IUnitloadSnapshotFactory unitloadSnapshotFactory, ILogger logger)
        {
            _unitloadSnapshotFactory = unitloadSnapshotFactory;
            _logger = logger;
        }

        public UnitloadSnapshot GetSnapshot(Unitload unitload)
        {
            UnitloadSnapshot snapshot = _unitloadSnapshotFactory.CreateUnitloadSnapshot();
            CopyUtil.CopyProperties(unitload, snapshot, new[]
            {
                nameof(UnitloadSnapshot.UnitloadSnapshotId),
                nameof(UnitloadSnapshot.SnapshotTime),
                nameof(UnitloadSnapshot.Items)
            });
            snapshot.SnapshotTime = DateTime.Now;

            foreach (var item in unitload.Items)
            {
                UnitloadItemSnapshot itemSnapshot = _unitloadSnapshotFactory.CreateUnitloadItemSnapshot();
                CopyUtil.CopyProperties(item, itemSnapshot, new[]
                {
                    nameof(UnitloadItemSnapshot.UnitloadItemSnapshotId),
                    nameof(UnitloadItemSnapshot.Unitload),
                });
                snapshot.AddItem(itemSnapshot);
            }
            _logger.Information("已获取 {unitload} 的快照", unitload);

            return snapshot;
        }

    }
}
