using System;

namespace Swm.Constants
{
    //  TODO 整理
    /// <summary>
    /// 封装常量。
    /// </summary>
    public static class Cst
    {
        /// <summary>
        /// 表示空值。
        /// </summary>
        public const string None = "None";

        /// <summary>
        /// 若字符串与 <see cref="Cst.None"/> 相等，返回 true，忽略大小写。
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNone(this string str)
        {
            return string.Equals(str, Cst.None, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// 若字符串与 <see cref="Cst.None"/> 不相等，返回 true，忽略大小写。
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNotNone(this string str)
        {
            return str.IsNone() == false;
        }
    }

}
