using Serilog;
using System.Threading.Tasks;

namespace Swm.Model
{
    public class TestCompletedTaskHandler : ICompletedTaskHandler
    {
        readonly ILogger _logger;
        public TestCompletedTaskHandler(ILogger logger)
        {
            _logger = logger;
        }

        public async Task ProcessCompletedTaskAsync(CompletedTaskInfo taskInfo, TransportTask task)
        {
            _logger.Debug(taskInfo.ToString());
            await Task.CompletedTask;
        }
    }
}
