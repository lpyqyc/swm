// Copyright 2020-2021 王建军
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Arctic.EventBus;
using Autofac.Features.Indexed;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Swm.TransportTasks
{
    public class RequestEventHandler : IEventHandler
    {
        readonly IIndex<string, IRequestHandler> _requestHandlers;
        readonly ILogger _logger;
        public RequestEventHandler(IIndex<string, IRequestHandler> requestHandlers, ILogger logger)
        {
            _requestHandlers = requestHandlers;
            _logger = logger;
        }

        public async Task ProcessAsync(string eventType, object? eventData)
        {
            await ProcessRequest((RequestInfo)(eventData ?? throw new Exception("未提供请求信息")));
        }

        private async Task ProcessRequest(RequestInfo requestInfo)
        {
            _logger.Information("正在引发 Wcs 请求事件。{requestInfo}", requestInfo);

            if (requestInfo.RequestType == null)
            {
                throw new InvalidRequestException("请求类型不能是 null。");
            }

            var handler = GetRequestHandler(requestInfo.RequestType);
            if (handler == null)
            {
                throw new ApplicationException($"没有找到可用的请求处理程序。请求类型：{requestInfo.RequestType}。");
            }

            _logger.Debug("请求处理程序是：{handlerType}", handler.GetType());
            await handler.ProcessRequestAsync(requestInfo);
        }

        /// <summary>
        /// 为指定的请求类型创建处理程序。
        /// </summary>
        /// <param name="requestType"></param>
        /// <returns></returns>
        private IRequestHandler? GetRequestHandler(string requestType)
        {
            bool b = _requestHandlers.TryGetValue(requestType, out var handler);
            if (b)
            {
                return handler;
            }
            return null;
        }
    }

}
