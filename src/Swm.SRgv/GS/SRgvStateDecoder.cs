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
        internal static SRgvStateTelex ParseTelex(string? message)
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
                AtStation = Convert.ToBoolean(Convert.ToInt32(message.Substring(12, 1))),
                ErrorCode = Convert.ToInt32(message.Substring(13, 2)),
                State = (SRgvStatus)Enum.Parse(typeof(SRgvStatus), message.Substring(15, 1)),
                Event = (SRgvEvent)Enum.Parse(typeof(SRgvEvent), message.Substring(16, 1)),
                TaskId = Convert.ToUInt32(message.Substring(17, 8)),
                ContainerCode = Convert.ToUInt32(message.Substring(25, 4)),
                FromStation = Convert.ToUInt16(message.Substring(29, 3)),
                ToStation = Convert.ToUInt16(message.Substring(32, 3)),
                TaskMode = (SRgvTaskMode)Enum.Parse(typeof(SRgvTaskMode), message.Substring(35, 1)),
                RawMessage = message,
            };
        }

        protected override void Decode(IChannelHandlerContext context, string message, List<object> output)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            SRgvStateTelex telex = ParseTelex(message);

            SRgvState state = new SRgvState
            {
                ErrorCode = telex.ErrorCode == 0 ? null : telex.ErrorCode,
                InManualMode = telex.State == SRgvStatus.手动模式,
                Position = Convert.ToInt32(telex.Position),
                StationNo = telex.AtStation ? Convert.ToInt32(telex.CurrentStation) : null,
                TaskNo = telex.TaskId,
                TaskCompleted = telex.Event == SRgvEvent.AutomaticTaskCompletion || telex.Event == SRgvEvent.TaskCompletionByManual,
                ActionState = telex.State switch
                {
                    SRgvStatus.初始化 or
                    SRgvStatus.手动模式 or
                    SRgvStatus.有货待命 or
                    SRgvStatus.停止 or
                    SRgvStatus.报警停机 or
                    SRgvStatus.有货 or
                    SRgvStatus.无货待命 => new SRgvActionState.None(),
                    SRgvStatus.无货运行 => new SRgvActionState.Walking.WithoutPallet(),
                    SRgvStatus.有货运行 => new SRgvActionState.Walking.WithPallet(),
                    SRgvStatus.输送线运行 => new SRgvActionState.Conveying(),
                    _ => throw new("不支持的状态值"),
                }
            };

            output.Add(state);
        }
    }
}

