namespace Swm.Device
{
    /// <summary>
    /// 表示输送线站点
    /// </summary>
    public record ConveyorStationInfo
    {
        private ConveyorStationInfo()
        {

        }

        public static readonly ConveyorStationInfo Instance = new ConveyorStationInfo();
    }


}
