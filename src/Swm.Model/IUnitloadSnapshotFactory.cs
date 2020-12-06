namespace Swm.Model
{
    public interface IUnitloadSnapshotFactory
    {
        /// <summary>
        /// 创建货载快照。
        /// </summary>
        /// <returns></returns>
        UnitloadSnapshot CreateUnitloadSnapshot();

        /// <summary>
        /// 创建货载项快照。
        /// </summary>
        /// <returns></returns>
        UnitloadItemSnapshot CreateUnitloadItemSnapshot();
    }
}
