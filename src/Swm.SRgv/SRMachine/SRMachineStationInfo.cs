namespace Swm.Device
{
    /// <summary>
    /// 表示堆垛机站点
    /// </summary>
    public record SRMachineStationInfo(int Ray, int Level)
    {
        /// <summary>
        /// 列号
        /// </summary>
        public int Ray { get; init; } = Ray;

        /// <summary>
        /// 层号
        /// </summary>
        public int Level { get; init; } = Level;
    }
}
