namespace Swm.Model
{
    public static class LocationTypes
    {
        /// <summary>
        /// 可以储存货物的位置。
        /// </summary>
        public const string S = "S";

        /// <summary>
        /// 关键点，通常是自动化的。
        /// </summary>
        public const string K = "K";

        /// <summary>
        /// N 位置，表示一个不存在的特殊位置。货载刚刚注册时，在 N 位置上，N 位置在整个系统中只有一个实例。
        /// </summary>
        public const string N = "N";
    }

}
