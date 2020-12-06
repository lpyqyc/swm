namespace Swm.Model
{
    public class DefaultUnitloadSnapshotFactory : IUnitloadSnapshotFactory
    {
        public UnitloadSnapshot CreateUnitloadSnapshot()
        {
            return new UnitloadSnapshot();
        }

        public UnitloadItemSnapshot CreateUnitloadItemSnapshot()
        {
            return new UnitloadItemSnapshot();
        }
    }
}
