namespace Swm.SRgv.GS
{
    /// <summary>
    /// 穿梭车 到站后 动作
    /// </summary>
    public enum ChainAction
    {
        /// <summary>
        /// 不动作
        /// </summary>
        None = 0,

        /// <summary>
        /// 左取货
        /// </summary>
        LeftPicking = 1,

        /// <summary>
        /// 左卸货
        /// </summary>
        LeftUnloading = 2,

        /// <summary>
        /// 右取货
        /// </summary>
        RightPicking = 3,

        /// <summary>
        /// 右卸货
        /// </summary>
        RightUnloading = 4
    }

}

