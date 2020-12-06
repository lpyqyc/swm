namespace Swm.Model
{
    /// <summary>
    /// 指示货架在巷道的左侧还是右侧。左右并没有绝对意义，仅用于将两侧的货架区分开。
    /// </summary>
    public enum RackSide
    {
        /// <summary>
        /// 左侧货架。
        /// </summary>
        Left = -1,

        /// <summary>
        /// 未指定货架是左侧还是右侧。
        /// </summary>
        NA,

        /// <summary>
        /// 右侧货架。
        /// </summary>
        Right = 1
    }

}
