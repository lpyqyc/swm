using Arctic.AspNetCore;
using Arctic.NHibernateExtensions;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using Serilog;
using Swm.Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Swm.Web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class OpsController : ControllerBase
    {
        readonly ISession _session;
        readonly ILogger _logger;

        public OpsController(ISession session, ILogger logger)
        {
            _session = session;
            _logger = logger;
        }

        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="args">查询参数</param>
        /// <returns></returns>
        [HttpPost]
        [DebugShowArgs]
        [AutoTransaction]
        [Route("list")]
        public async Task<OpList> List(OpListArgs args)
        {
            var pagedList = await _session.Query<Op>().SearchAsync(args, args.Sort, args.Current, args.PageSize);
            return new OpList
            {
                Success = true,
                Message = "OK",
                Data = pagedList.List.Select(x => new OpListItem
                {
                    OpId = x.OpId,
                    ctime = x.ctime,
                    cuser = x.cuser,
                    OperationType = x.OperationType,
                    Url = x.Url,
                    Comment = x.Comment
                }),
                Total = pagedList.Total
            };
        }

    }
}
