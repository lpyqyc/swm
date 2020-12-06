using System.Threading.Tasks;

namespace Swm.Model
{
    /// <summary>
    /// 此接口定义 wcs 请求的处理程序。
    /// </summary>
    public interface IRequestHandler
    {
        /// <summary>
        /// 对请求进行处理。此方法将在 TransactionScope 范围中被调用。
        /// </summary>
        /// <param name="requestInfo">请求信息</param>
        Task ProcessRequestAsync(RequestInfo requestInfo);
    }
}
