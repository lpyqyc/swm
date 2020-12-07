using Arctic.NHibernateExtensions.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Swm.Web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        readonly ISession _session;
        readonly ILogger _logger;

        public LocationsController(ISession session, ILogger logger)
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
        public async Task<LocationList> List(LocationListArgs args)
        {
            var pagedList = await _session.Query<Location>().ToPagedListAsync(args);
            return new LocationList
            {
                Success = true,
                Message = "OK",                
                Data = pagedList.List.Select(x => new LocationListItem
                {
                    LocationId = x.LocationId,
                    Prop1 = x.Prop1,
                    Prop2 = x.Prop2,
                }),
                Total = pagedList.Total
            };
        }

        /// <summary>
        /// 详细信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [AutoTransaction]
        public async Task<LocationDetail> Detail(int id)
        {
            var location = await _session.GetAsync<Location>(id);
            return new LocationDetail 
            {
                LocationId = Location.LocationId,
                Prop1 = Location.Prop1,
                Prop2 = Location.Prop2,
            };
        }

        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost]
        [AutoTransaction]
        [Route("create")]
        public async Task<OperationResult> Create(CreateLocationArgs args)
        {
            Location location = new Location
            {
                Prop1 = args.Prop1,
                Prop2 = args.Prop2,
            };
            await _session.SaveAsync(location);
            return new OperationResult { Success = true, Message = "OK" };
        }


        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="id"></param>
        /// <param name="args"></param>
        [HttpPut("update/{id}")]
        [AutoTransaction]
        public async Task<OperationResult> Update(int id, [FromBody] UpdateLocationArgs args)
        {
            Location? location = await _session.GetAsync<Location>(id);
            if (location == null)
            {
                throw new InvalidOperationException();
            }
            location.Prop1 = args.Prop1;
            location.Prop2 = args.Prop2;
            await _session.UpdateAsync(location);
            return new OperationResult { Success = true, Message = "OK" };
        }


        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpDelete("{id}")]
        public async Task<OperationResult> Delete(int id)
        {
            Location? location = await _session.GetAsync<Location>(id);
            if (location == null)
            {
                throw new InvalidOperationException();
            }
            await _session.DeleteAsync(location);
            return new OperationResult { Success = true, Message = "OK" };

        }
    }
}
