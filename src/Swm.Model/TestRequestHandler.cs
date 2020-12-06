using Serilog;
using System.Threading.Tasks;

namespace Swm.Model
{
    public class TestRequestHandler : IRequestHandler
    {
        readonly ILogger _logger;
        public TestRequestHandler(ILogger logger)
        {
            _logger = logger;
        }

        public async Task ProcessRequestAsync(RequestInfo requestInfo)
        {
            _logger.Debug(requestInfo.ToString());
            await Task.CompletedTask;
        }
    }
}
