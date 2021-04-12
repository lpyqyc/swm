using System;

namespace Swm.Device
{
    [Serializable]
    public class NotRespondedException : Exception
    {
        public NotRespondedException() { }
        public NotRespondedException(string message) : base(message) { }
        public NotRespondedException(string message, Exception inner) : base(message, inner) { }
        protected NotRespondedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

}
