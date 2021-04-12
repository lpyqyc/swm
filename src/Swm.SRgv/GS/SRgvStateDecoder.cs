using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Swm.Device.Rgv;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Swm.SRgv.GS
{
    class SRgvStateDecoder : MessageToMessageDecoder<string>
    {
        internal static bool ValidateStateTelex(string telex)
        {
            if (string.IsNullOrWhiteSpace(telex))
            {
                return false;
            }
            return Regex.IsMatch(telex, @"^LA\d{34}");
        }


        /// <summary>
        /// 将报文解析成状态对象
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static SRgvStateTelex ParseTelex(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentException(nameof(message));
            }
            message = message.Replace('\0', '0');

            if (!ValidateStateTelex(message))
            {
                // TODO 打日志
                throw new InvalidOperationException("状态报文格式无效");
            }

            //Match match = Regex.Match(telex, @"^LA(<position>\d{7})(<station>\d{3})(\d)(\d{2})(\d)(\d)(\d{8})(\d{4})(\d{3})(\d{3})(\d{1})", RegexOptions.Compiled);
            // TODO 为什么转成字符串再转成整型？
            return new SRgvStateTelex
            {
                Position = Convert.ToUInt32(message.Substring(2, 7)),
                CurrentStation = Convert.ToUInt16(message.Substring(9, 3)),
                AtStation = !Convert.ToBoolean(Convert.ToInt32(message.Substring(12, 1))),
                ErrorCode = Convert.ToInt32(message.Substring(13, 2)),
                State = (RailGuidedVehicleStatus)Enum.Parse(typeof(RailGuidedVehicleStatus), message.Substring(15, 1)),
                Event = (SRgvEvent)Enum.Parse(typeof(SRgvEvent), message.Substring(16, 1)),
                TaskId = Convert.ToInt32(message.Substring(17, 8)),
                ContainerCode = Convert.ToUInt32(message.Substring(25, 4)),
                FromStation = Convert.ToUInt16(message.Substring(29, 3)),
                ToStation = Convert.ToUInt16(message.Substring(32, 3)),
                TaskMode = (RailGuidedVehicleTaskMode)Enum.Parse(typeof(RailGuidedVehicleTaskMode), message.Substring(35, 1)),
                RawMessage = message,
            };
        }


        protected override void Decode(IChannelHandlerContext context, string message, List<object> output)
        {
            SRgvStateTelex telex = ParseTelex(message);

            SRgvState state = new SRgvState
            {
                ErrorCode = telex.ErrorCode,
                Walking = telex.State == RailGuidedVehicleStatus.无货运行
                    || telex.State == RailGuidedVehicleStatus.有货运行,
                InManualMode = telex.State == RailGuidedVehicleStatus.手动模式,
                EStopped = null,
                Locked = null,
                Occupied = telex.State == RailGuidedVehicleStatus.有货运行
                    || telex.State == RailGuidedVehicleStatus.有货待命
                    || telex.State == RailGuidedVehicleStatus.有货,
                Loading = telex.State == RailGuidedVehicleStatus.输送线运行
                    && telex.TaskMode == RailGuidedVehicleTaskMode.Picking,
                Unloading = telex.State == RailGuidedVehicleStatus.输送线运行
                    && telex.TaskMode == RailGuidedVehicleTaskMode.Putting,
                Position = Convert.ToInt32(telex.Position),
                StationNo = telex.AtStation ? Convert.ToInt32(telex.CurrentStation) : null,
                TaskInfo = telex.TaskMode == RailGuidedVehicleTaskMode.Initialized
                    ? null
                    : telex.TaskMode switch
                    {
                        RailGuidedVehicleTaskMode.Initialized => null,
                        RailGuidedVehicleTaskMode.AutomaticTask => throw new global::System.NotImplementedException(),
                        RailGuidedVehicleTaskMode.Picking => new SRgvTaskInfo.LeftLoad(telex.TaskId, telex.ContainerCode.ToString(), telex.CurrentStation),
                        RailGuidedVehicleTaskMode.Putting => throw new global::System.NotImplementedException(),
                        RailGuidedVehicleTaskMode.WalkWithGoods => throw new global::System.NotImplementedException(),
                        RailGuidedVehicleTaskMode.Walk => throw new global::System.NotImplementedException(),
                    }
            };
            output.Add(state);
        }
    }
}

