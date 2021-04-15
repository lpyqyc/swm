using System;
using System.Collections.Generic;

namespace Swm.Device
{
    [Serializable]
    public class CannotSendDirectiveException : Exception
    {
        public CannotSendDirectiveException(List<string> errors)
        {
            this.Errors = errors;
        }
        public CannotSendDirectiveException(string message) : base(message) { }
        public CannotSendDirectiveException(string message, Exception inner) : base(message, inner) { }
        protected CannotSendDirectiveException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }


        public List<string> Errors { get; } = new List<string>();

        public override string Message => string.Join(", ", this.Errors);
    }


}
