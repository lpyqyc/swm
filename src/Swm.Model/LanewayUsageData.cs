using System;

namespace Swm.Model
{
    /// <summary>
    /// 巷道使用数据。
    /// </summary>
    public class LanewayUsageData : ICloneable
    {
        public LanewayUsageData()
        {
        }

        /// <summary>
        /// 数据更新时间。
        /// </summary>
        public virtual DateTime mtime { get; set; }


        /// <summary>
        /// 获取或设置总货位数。
        /// <see cref="Location.Exists"/> 为 false 的货位不参与统计。
        /// </summary>
        public virtual int Total { get; set; }

        /// <summary>
        /// 获取或设置当前可用的货位数。
        /// 可用货位是指 <see cref="Location.Available"/> 方法返回 true 的货位。
        /// <see cref="Location.Exists"/> 为 false 的货位不参与统计。
        /// </summary>
        public virtual int Available { get; set; }

        /// <summary>
        /// 获取或设置当前有货的货位数。
        /// 有货的货位是指 <see cref="Location.Loaded"/> 方法返回 true 的货位。
        /// <see cref="Location.Exists"/> 为 false 的货位不参与统计。
        /// </summary>
        public virtual int Loaded { get; set; }

        /// <summary>
        /// 获取或设置当前已禁止入站的货位数，
        /// <see cref="Location.Exists"/> 为 false 的货位不参与统计。
        /// </summary>
        public virtual int InboundDisabled { get; set; }


        public LanewayUsageData Clone()
        {
            return (LanewayUsageData)this.MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }
    }

}
