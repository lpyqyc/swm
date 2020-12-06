using System;

namespace Swm.Model
{
    [Serializable]
    public class FailToBuildTaskException : Exception
    {
        public FailtoBuildTaskReason Reason { get; private set; }

        public FailToBuildTaskException(FailtoBuildTaskReason reason)
        {
            Reason = reason;
        }

        public FailToBuildTaskException(string message) : base(message) { }
        public FailToBuildTaskException(string message, Exception inner) : base(message, inner) { }
        protected FailToBuildTaskException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public override string Message
        {
            get
            {
                switch (Reason)
                {
                    case FailtoBuildTaskReason.UnitloadBeingMoved:
                        return "托盘有任务";
                    case FailtoBuildTaskReason.StartAndEndArdTheSame:
                        return "起点和终点相同";
                    case FailtoBuildTaskReason.LanewayNotAutomated:
                        return "不是自动化巷道";
                    case FailtoBuildTaskReason.LanewayOffline:
                        return "巷道脱机";
                    case FailtoBuildTaskReason.LocationNotExists:
                        return "货位不存在";
                    case FailtoBuildTaskReason.OutboundDisabled:
                        return "禁止出站";
                    case FailtoBuildTaskReason.OutboundLimitReached:
                        return "达到出站限制";
                    case FailtoBuildTaskReason.InboundDisabled:
                        return "禁止入站";
                    case FailtoBuildTaskReason.InboundLimitReached:
                        return "达到入站限制";
                    case FailtoBuildTaskReason.DoubleDeepInterference:
                        return "双深干涉";
                    case FailtoBuildTaskReason.NForWcsTask:
                        return "不是自动化任务";
                    case FailtoBuildTaskReason.InvalidStart:
                        return "无效的起点";
                    default:
                        return "未知错误";
                }
            }
        }
    }

}
