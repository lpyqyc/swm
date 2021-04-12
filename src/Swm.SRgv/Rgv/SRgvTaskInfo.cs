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
        /// 无货行走任务
        /// </summary>
        public record WalkWithoutPallet(int TaskNo, int? FromStation, int ToStation) : SRgvTaskInfo(TaskNo)
        {
            /// <summary>
            /// 表示行走任务的起始站
            /// </summary>
            public int? FromStation { get; init; } = FromStation;

            /// <summary>
            /// 表示行走任务的目的站
            /// </summary>
            public int ToStation { get; init; } = ToStation;
        }
        
        /// <summary>
        /// 带货行走任务
        /// </summary>
        public record WalkWithPallet(int TaskNo, string PalletCode, int? FromStation, int ToStation) : SRgvTaskInfo(TaskNo)
        {
            /// <summary>
            /// 获取任务关联的托盘号
            /// </summary>
            public string PalletCode { get; init; } = PalletCode;

            /// <summary>
            /// 表示行走任务的起始站
            /// </summary>
            public int? FromStation { get; init; } = FromStation;

            /// <summary>
            /// 表示行走任务的目的站
            /// </summary>
            public int ToStation { get; init; } = ToStation;
        }

        /// <summary>
        /// 左取货任务
        /// </summary>
        public record LeftLoad(int TaskNo, string PalletCode, int Station) : SRgvTaskInfo(TaskNo)
        {
            /// <summary>
            /// 获取任务关联的托盘号
            /// </summary>
            public string PalletCode { get; init; } = PalletCode;

            /// <summary>
            /// 取货站点
            /// </summary>
            public int Station { get; init; } = Station;
        }

        /// <summary>
        /// 左放货任务
        /// </summary>
        public record LeftUnload(int TaskNo, string PalletCode, int Station) : SRgvTaskInfo(TaskNo)
        {
            /// <summary>
            /// 获取任务关联的托盘号
            /// </summary>
            public string PalletCode { get; init; } = PalletCode;

            /// <summary>
            /// 放货站点
            /// </summary>
            public int Station { get; init; } = Station;
        }

        /// <summary>
        /// 右取货任务
        /// </summary>
        public record RightLoad(int TaskNo, string PalletCode, int Station) : SRgvTaskInfo(TaskNo)
        {
            /// <summary>
            /// 获取任务关联的托盘号
            /// </summary>
            public string PalletCode { get; init; } = PalletCode;

            /// <summary>
            /// 取货站点
            /// </summary>
            public int Station { get; init; } = Station;
        }

        /// <summary>
        /// 右放货任务
        /// </summary>
        public record RightUnload(int TaskNo, string PalletCode, int Station) : SRgvTaskInfo(TaskNo)
        {
            /// <summary>
            /// 获取任务关联的托盘号
            /// </summary>
            public string PalletCode { get; init; } = PalletCode;

            /// <summary>
            /// 放货站点
            /// </summary>
            public int Station { get; init; } = Station;
        }


    }



}