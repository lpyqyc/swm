using Serilog;
using System;
using System.Linq;
using System.Reflection;

namespace Swm.Model
{
    internal static class CopyUtil
    {

        /// <summary>
        /// 遍历目标对象的属性，从源对象复制同名属性的值
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <param name="excluded">目标类型中要排除的属性名称</param>
        public static void CopyProperties(object src, object dest, string[] excluded = null)
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            if (dest == null)
            {
                throw new ArgumentNullException(nameof(dest));
            }


            excluded ??= new string[0];
            ILogger logger = Log.ForContext(typeof(CopyUtil));
            logger.Debug("正在复制属性，源类型 {srcType}，目标类型 {destType}，排除属性：{excluded}", src.GetType(), dest.GetType(), string.Join(",", excluded));

            var destProps = dest.GetType()
                .GetProperties()
                .Where(x => excluded.Contains(x.Name) == false)
                .ToArray();
            foreach (var destProp in destProps)
            {
                logger.Debug("正在复制属性 {propName}，目标属性类型是 {propType}", destProp.Name, destProp.PropertyType.Name);
                var srcProp = src.GetType()
                    .GetProperty(destProp.Name, BindingFlags.Public | BindingFlags.Instance);
                if (srcProp == null)
                {
                    logger.Warning("在源类型上找不到名为 {propName} 的属性", destProp.Name);
                }
                else if (destProp.PropertyType.IsAssignableFrom(srcProp.PropertyType) == false)
                {
                    logger.Warning("在源类型上找到名为 {propName} 的属性，但源属性 {srcPropType} 的类型与目标不兼容", destProp.Name, srcProp.PropertyType);
                }
                else
                {
                    object val = srcProp.GetValue(src);
                    destProp.SetValue(dest, val);
                    logger.Debug("已复制属性 {propName}", destProp.Name);
                }
            }

            logger.Debug("已复制所有属性");

        }
    }
}
