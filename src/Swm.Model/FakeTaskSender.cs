using NHibernate;
using System;

namespace Swm.Model
{
    public class FakeTaskSender : ITaskSender
    {
        ISession _session;
        public FakeTaskSender(ISession session)
        {
            _session = session;
        }
        public void SendTask(TransportTask task)
        {
            task.WasSentToWcs = true;
            task.SentToWcsAt = DateTime.Now;
            _session.Save(task);
        }
    }

}
