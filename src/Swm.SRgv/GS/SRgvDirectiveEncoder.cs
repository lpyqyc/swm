using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Swm.Device.Rgv;
using System;
using System.Collections.Generic;

namespace Swm.SRgv.GS
{
    class SRgvDirectiveEncoder : MessageToMessageEncoder<SRgvDirective>
    {
        protected override void Encode(IChannelHandlerContext context, SRgvDirective message, List<object> output)
        {
            output.Add(ToTelex(message).GetText());
        }


        internal SRgvDirectiveTelex ToTelex(SRgvDirective directive)
        {
            return directive switch
            {
                SRgvDirective.Inquire => throw new NotSupportedException(),
                SRgvDirective.SendTask dir => dir.TaskInfo switch
                {
                    SRgvTaskInfo.WalkWithoutPallet task => new SRgvDirectiveTelex
                    {
                        TypeFlag = "HB",
                        TaskId = task.TaskNo,
                        PalletCode = 0,
                        StartingStation = 0,
                        StartingStationAction = ChainAction.None,
                        DestinationStation = Convert.ToUInt16(task.ToStation),
                        DestinationStationAction = ChainAction.None,
                        TaskMode = RailGuidedVehicleTaskMode.Walk
                    },
                    SRgvTaskInfo.WalkWithPallet task => new SRgvDirectiveTelex
                    {
                        TypeFlag = "HB",
                        TaskId = task.TaskNo,
                        PalletCode = Convert.ToUInt32(task.PalletCode),
                        StartingStation = 0,
                        StartingStationAction = ChainAction.None,
                        DestinationStation = Convert.ToUInt16(task.ToStation),
                        DestinationStationAction = ChainAction.None,
                        TaskMode = RailGuidedVehicleTaskMode.Walk
                    },
                    SRgvTaskInfo.LeftLoad task => new SRgvDirectiveTelex
                    {
                        TypeFlag = "HB",
                        TaskId = task.TaskNo,
                        PalletCode = Convert.ToUInt32(task.PalletCode),
                        StartingStation = Convert.ToUInt16(task.Station),
                        StartingStationAction = ChainAction.LeftPicking,
                        DestinationStation = 0,
                        DestinationStationAction = ChainAction.None,
                        TaskMode = RailGuidedVehicleTaskMode.Picking,
                    },
                    SRgvTaskInfo.RightLoad task => new SRgvDirectiveTelex
                    {
                        TypeFlag = "HB",
                        TaskId = task.TaskNo,
                        PalletCode = Convert.ToUInt32(task.PalletCode),
                        StartingStation = Convert.ToUInt16(task.Station),
                        StartingStationAction = ChainAction.RightPicking,
                        DestinationStation = 0,
                        DestinationStationAction = ChainAction.None,
                        TaskMode = RailGuidedVehicleTaskMode.Picking,
                    },
                    SRgvTaskInfo.LeftUnload task => new SRgvDirectiveTelex
                    {
                        TypeFlag = "HB",
                        TaskId = task.TaskNo,
                        PalletCode = Convert.ToUInt32(task.PalletCode),
                        StartingStation = 0,
                        StartingStationAction = ChainAction.None,
                        DestinationStation = Convert.ToUInt16(task.Station),
                        DestinationStationAction = ChainAction.LeftUnloading,
                        TaskMode = RailGuidedVehicleTaskMode.Putting,
                    },
                    SRgvTaskInfo.RightUnload task => new SRgvDirectiveTelex
                    {
                        TypeFlag = "HB",
                        TaskId = task.TaskNo,
                        PalletCode = Convert.ToUInt32(task.PalletCode),
                        StartingStation = 0,
                        StartingStationAction = ChainAction.None,
                        DestinationStation = Convert.ToUInt16(task.Station),
                        DestinationStationAction = ChainAction.RightUnloading,
                        TaskMode = RailGuidedVehicleTaskMode.Putting,
                    },
                    _ => throw new("无效的任务类型"),
                },
                SRgvDirective.ClearTask => new SRgvDirectiveTelex
                {
                    TypeFlag = "HB",
                    TaskId = 0,
                    PalletCode = 0,
                    StartingStation = 0,
                    StartingStationAction = ChainAction.None,
                    DestinationStation = 0,
                    DestinationStationAction = ChainAction.None,
                    TaskMode = RailGuidedVehicleTaskMode.Initialized,
                },
                SRgvDirective.Lock => throw new NotSupportedException(),
                SRgvDirective.Unlock => throw new NotSupportedException(),
                SRgvDirective.EStop => throw new NotSupportedException(),
                _ => throw new("无效的指令类型"),
            };
        }
    }



}
