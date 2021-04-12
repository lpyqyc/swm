using System;

namespace Swm.Device
{
    [Serializable]
    public class CannotSendDirectiveException : Exception
    {
        public CannotSendDirectiveException(CannotSendDirectiveReason reason)
        {
            this.Reason = reason;
        }
        public CannotSendDirectiveException(string message) : base(message) { }
        public CannotSendDirectiveException(string message, Exception inner) : base(message, inner) { }
        protected CannotSendDirectiveException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }


        public CannotSendDirectiveReason Reason { get; }
    }


}
