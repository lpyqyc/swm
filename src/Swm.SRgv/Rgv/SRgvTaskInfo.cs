namespace Swm.Device.Rgv
{
    /// <summary>
    /// 单工位穿梭车的任务信息
    /// </summary>
    public abstract record SRgvTaskInfo(int TaskNo)
    {
        /// <summary>
        /// 穿梭车接受的任务号
        /// </summary>
        public int TaskNo { get; init; } = TaskNo;

        /// <summary>
        /// 表示行走任务
        /// </summary>
        public record Walk(int TaskNo, int ToStation) : SRgvTaskInfo(TaskNo)
        {
            /// <summary>
            /// 行走任务的目的站
            /// </summary>
            public int ToStation { get; init; } = ToStation;

            /// <summary>
            /// 表示带货行走任务
            /// </summary>
            public record WithPallet(int TaskNo, string PalletCode, int ToStation) : Walk(TaskNo, ToStation)
            {
                /// <summary>
                /// 获取任务关联的托盘号
                /// </summary>
                public string PalletCode { get; init; } = PalletCode;
            }

            /// <summary>
            /// 表示无货行走任务
            /// </summary>
            public record WithoutPallet(int TaskNo, int ToStation) : Walk(TaskNo, ToStation);

        }

        /// <summary>
        /// 表示输送任务，
        /// </summary>
        public record Convey(int TaskNo, string PalletCode, int Station) : SRgvTaskInfo(TaskNo)
        {
            /// <summary>
            /// 获取任务关联的托盘号
            /// </summary>
            public string PalletCode { get; init; } = PalletCode;

            /// <summary>
            /// 表示输送站点
            /// </summary>
            public int Station { get; init; } = Station;

            /// <summary>
            /// 表示取货任务
            /// </summary>
            public record Load(int TaskNo, string PalletCode, int Station) : Convey(TaskNo, PalletCode, Station)
            {
                /// <summary>
                /// 表示左取货任务
                /// </summary>
                public record LeftLoad(int TaskNo, string PalletCode, int Station) : Load(TaskNo, PalletCode, Station);

                /// <summary>
                /// 表示右取货任务
                /// </summary>
                public record RightLoad(int TaskNo, string PalletCode, int Station) : Load(TaskNo, PalletCode, Station);
            }

            /// <summary>
            /// 表示放货任务
            /// </summary>
            public record Unload(int TaskNo, string PalletCode, int Station) : Convey(TaskNo, PalletCode, Station)
            {
                /// <summary>
                /// 表示左放货任务
                /// </summary>
                public record LeftUnload(int TaskNo, string PalletCode, int Station) : Unload(TaskNo, PalletCode, Station);

                /// <summary>
                /// 表示右放货任务
                /// </summary>
                public record RightUnload(int TaskNo, string PalletCode, int Station) : Unload(TaskNo, PalletCode, Station);

            }
        }
    }
}