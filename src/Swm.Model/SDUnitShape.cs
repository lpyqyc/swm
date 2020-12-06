namespace Swm.Model
{
    /// <summary>
    /// 单叉双深单元的形态。两位数字依次表示 N1, F1。
    /// </summary>
    public enum SDUnitShape
    {
        /// <summary>
        /// 一深无货，二深无货。
        /// </summary>
        SD_00,

        /// <summary>
        /// 一深无货，二深有货。
        /// </summary>
        SD_01,

        /// <summary>
        /// 一深有货，二深无货。
        /// </summary>
        SD_10,

        /// <summary>
        /// 一深有货，二深有货。
        /// </summary>
        SD_11
    }

}
