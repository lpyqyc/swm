namespace Swm.Device.Rgv
{
    /// <summary>
    /// 单工位 Rgv 的动作状态
    /// </summary>
    public abstract record SRgvActionState 
    {
        /// <summary>
        /// 表示当前无动作
        /// </summary>
        public record None : SRgvActionState;

        /// <summary>
        /// 表示正在输送
        /// </summary>
        public record Conveying : SRgvActionState
        {
            /// <summary>
            /// 表示正在取货
            /// </summary>
            public record Loading : Conveying
            {
                /// <summary>
                /// 表示正在左取货
                /// </summary>
                public record LeftLoading : Loading;

                /// <summary>
                /// 表示正在右取货
                /// </summary>
                public record RightLoading : Loading;
            }

            /// <summary>
            /// 表示正在放货
            /// </summary>
            public record Unloading : Conveying
            {
                /// <summary>
                /// 表示正在左放货
                /// </summary>
                public record LeftUnloading : Unloading;

                /// <summary>
                /// 表示正在右放货
                /// </summary>
                public record RightUnloading : Unloading;
            }
        }

        /// <summary>
        /// 表示正在行走
        /// </summary>
        public record Walking : SRgvActionState
        {
            /// <summary>
            /// 表示有货行走
            /// </summary>
            public record WithPallet : Walking;

            /// <summary>
            /// 表示无货行走
            /// </summary>
            public record WithoutPallet : Walking;
        }


    }

}