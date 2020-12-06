using Arctic.EventBus;
using NHibernate;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Swm.Model
{
    /// <summary>
    /// 处理有且只有一个容器编码的入库请求。
    /// </summary>
    public class PreRequestEventHandler : IEventHandler
    {
        readonly ISession _session;
        readonly ILogger _logger;
        public PreRequestEventHandler(ISession session, ILogger logger)
        {
            _session = session;
            _logger = logger;
        }

        static Dictionary<string, string> _cachedRequestTypes;


        private Dictionary<string, string> GetCachedRequestTypes()
        {
            lock (typeof(PreRequestEventHandler))
            {
                if (_cachedRequestTypes == null)
                {
                    _logger.Information("正在为 PreRequestEventHandler 读取关键点请求类型");
                    _cachedRequestTypes = _session.Query<Location>().GetRequestTypes();
                }

                return _cachedRequestTypes;
            }
        }


        public async Task ProcessAsync(string eventType, object eventData)
        {
            switch (eventType)
            {
                case EventTypes.PreRequest:
                    OnPreRequest((RequestInfo)eventData);
                    break;
                case EventTypes.KeyPointChanged:
                    OnKeyPointChanged();
                    break;
                default:
                    break;
            }
            await Task.CompletedTask;
        }

        private void OnPreRequest(RequestInfo requestInfo)
        {
            _logger.Debug("正在对请求进行预处理。{requestInfo}", requestInfo);

            ResolveRequestType(requestInfo);

            _logger.Debug("已完成请求预处理");

        }


        public virtual void ResolveRequestType(RequestInfo requestInfo)
        {
            _logger.Debug("正在解析请求类型");
            requestInfo.RequestType = requestInfo.RequestType?.Trim();

            // 1，检查请求本身
            if (string.IsNullOrEmpty(requestInfo.RequestType) == false)
            {
                _logger.Debug("请求中已提供请求类型，解析成功");
                return;
            }

            _logger.Debug("请求中未提供请求类型");

            // 2，从请求位置进行解析
            ResolveRequestTypeByLocation(requestInfo);
            if (requestInfo.RequestType != null)
            {
                return;
            }

            throw new ApplicationException("未能解析请求类型。");

        }

        /// <summary>
        /// 通过位置解析请求类型
        /// </summary>
        /// <param name="requestInfo">请求信息引用传递</param>
        void ResolveRequestTypeByLocation(RequestInfo requestInfo)
        {
            _logger.Debug("正在根据请求位置解析请求类型");

            if (requestInfo.LocationCode == null)
            {
                _logger.Debug("请求中没有提供位置，按位置解析失败");
                return;
            }

            Dictionary<String, string> cachedRequestTypes = GetCachedRequestTypes();
            _logger.Information("请求位置是 {locationCode}", requestInfo.LocationCode);
            if (cachedRequestTypes.ContainsKey(requestInfo.LocationCode) == false)
            {
                _logger.Debug("请求位置在 Wms 中不存在，按位置解析失败");
                return;
            }

            string requestType = cachedRequestTypes[requestInfo.LocationCode];
            if (string.IsNullOrWhiteSpace(requestType))
            {
                _logger.Information("请求位置上未设置请求类型，按位置解析失败");
                return;
            }

            requestInfo.RequestType = requestType;
            _logger.Information("请求位置上设置的请求类型是 {requestType}，按位置解析成功", requestInfo.RequestType);

        }



        private void OnKeyPointChanged()
        {
            lock (typeof(PreRequestEventHandler))
            {
                _cachedRequestTypes = null;
                _logger.Information("已清除关键点请求类型缓存");
            }
        }
    }
}
