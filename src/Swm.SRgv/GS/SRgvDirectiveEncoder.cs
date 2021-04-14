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
                    SRgvTaskInfo.Walk.WithoutPallet task => new SRgvDirectiveTelex
                    {
                        TypeFlag = "HS",
                        TaskId = task.TaskNo,
                        PalletCode = 0,
                        StartingStation = 0,
                        StartingStationAction = ChainAction.None,
                        DestinationStation = Convert.ToUInt16(task.ToStation),
                        DestinationStationAction = ChainAction.None,
                        TaskMode = SRgvTaskMode.Walk
                    },
                    SRgvTaskInfo.Walk.WithPallet task => new SRgvDirectiveTelex
                    {
                        TypeFlag = "HS",
                        TaskId = task.TaskNo,
                        PalletCode = Convert.ToUInt32(task.PalletCode),
                        StartingStation = 0,
                        StartingStationAction = ChainAction.None,
                        DestinationStation = Convert.ToUInt16(task.ToStation),
                        DestinationStationAction = ChainAction.None,
                        TaskMode = SRgvTaskMode.WalkWithGoods
                    },
                    SRgvTaskInfo.Convey.Load.LeftLoad task => new SRgvDirectiveTelex
                    {
                        TypeFlag = "HB",
                        TaskId = task.TaskNo,
                        PalletCode = Convert.ToUInt32(task.PalletCode),
                        StartingStation = Convert.ToUInt16(task.Station),
                        StartingStationAction = ChainAction.LeftPicking,
                        DestinationStation = 0,
                        DestinationStationAction = ChainAction.None,
                        TaskMode = SRgvTaskMode.Picking,
                    },
                    SRgvTaskInfo.Convey.Load.RightLoad task => new SRgvDirectiveTelex
                    {
                        TypeFlag = "HB",
                        TaskId = task.TaskNo,
                        PalletCode = Convert.ToUInt32(task.PalletCode),
                        StartingStation = Convert.ToUInt16(task.Station),
                        StartingStationAction = ChainAction.RightPicking,
                        DestinationStation = 0,
                        DestinationStationAction = ChainAction.None,
                        TaskMode = SRgvTaskMode.Picking,
                    },
                    SRgvTaskInfo.Convey.Unload.LeftUnload task => new SRgvDirectiveTelex
                    {
                        TypeFlag = "HB",
                        TaskId = task.TaskNo,
                        PalletCode = Convert.ToUInt32(task.PalletCode),
                        StartingStation = 0,
                        StartingStationAction = ChainAction.None,
                        DestinationStation = Convert.ToUInt16(task.Station),
                        DestinationStationAction = ChainAction.LeftUnloading,
                        TaskMode = SRgvTaskMode.Putting,
                    },
                    SRgvTaskInfo.Convey.Unload.RightUnload task => new SRgvDirectiveTelex
                    {
                        TypeFlag = "HB",
                        TaskId = task.TaskNo,
                        PalletCode = Convert.ToUInt32(task.PalletCode),
                        StartingStation = 0,
                        StartingStationAction = ChainAction.None,
                        DestinationStation = Convert.ToUInt16(task.Station),
                        DestinationStationAction = ChainAction.RightUnloading,
                        TaskMode = SRgvTaskMode.Putting,
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
                    TaskMode = SRgvTaskMode.Initialized,
                },
                _ => throw new("无效的指令类型"),
            };
        }
    }



}
