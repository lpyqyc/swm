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
    public class StocksController : ControllerBase
    {
        readonly ISession _session;
        readonly ILogger _logger;

        public StocksController(ISession session, ILogger logger)
        {
            _session = session;
            _logger = logger;
        }

        /// <summary>
        /// 实时库存
        /// </summary>
        /// <param name="args">查询参数</param>
        /// <returns></returns>
        [HttpGet]
        [DebugShowArgs]
        [AutoTransaction]
        [OperationType(OperationTypes.实时库存)]
        public async Task<ListResult<StockListItem>> Get([FromQuery] StockListArgs args)
        {
            var pagedList = await _session.Query<Stock>().SearchAsync(args, args.Sort, args.Current, args.PageSize);
            return new ListResult<StockListItem>
            {
                Success = true,
                Data = pagedList.List.Select(x => new StockListItem
                {
                    StockId = x.StockId,
                    mtime = x.mtime,
                    MaterialCode = x.Material.MaterialCode,
                    Description = x.Material.Description,
                    Batch = x.Batch,
                    StockStatus = x.StockStatus,
                    Quantity = x.Quantity,
                    Uom = x.Uom,
                    OutOrdering = x.OutOrdering,
                    AgeBaseline = x.AgeBaseline,
                }),
                Total = pagedList.Total
            };
        }
    }
}
