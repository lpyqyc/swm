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
