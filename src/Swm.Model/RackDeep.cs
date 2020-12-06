namespace Swm.Model
{
    /// <summary>
    /// 货架深度。
    /// </summary>
    public enum RackDeep
    {
        /// <summary>
        /// 未指定货架的深度，或不适用。
        /// </summary>
        NA = 0,

        /// <summary>
        /// 一深，也就是离堆垛机较近的货架。
        /// </summary>
        Deep1 = 1,

        /// <summary>
        /// 二深，也就是离堆垛机较远的货架。。
        /// </summary>
        Deep2 = 2,
    }

}
