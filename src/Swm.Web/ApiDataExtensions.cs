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

using Arctic.NHibernateExtensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Swm.Web
{
    public static class ApiDataExtensions
    {
        public static ApiData Success(this ControllerBase controller)
        {
            return new ApiData
            {
                Success = true,
                ErrorMessage = string.Empty,
                Host = controller.Request.Host.ToString(),
                TraceId = controller.HttpContext.TraceIdentifier,
            };
        }

        public static ApiData<TData> Success<TData>(this ControllerBase controller, TData? data)
        {
            return new ApiData<TData>
            {
                Success = true,
                Data = data,
                ErrorMessage = string.Empty,
                Host = controller.Request.Host.ToString(),
                TraceId = controller.HttpContext.TraceIdentifier,
            };
        }

        public static ApiData Failure(this ControllerBase controller, string? errorMessage)
        {
            return new ApiData
            {
                Success = false,
                ErrorMessage = errorMessage,
                Host = controller.Request.Host.ToString(),
                TraceId = controller.HttpContext.TraceIdentifier,
            };
        }

        public static ListData<T> ListData<T>(this ControllerBase controller, PagedList<T> pagedList)
        {
            return controller.ListData(pagedList, x => x);
        }

        public static ListData<U> ListData<T, U>(this ControllerBase controller, PagedList<T> pagedList, Func<T, U> selector)
        {
            return new ListData<U>
            {
                Success = true,
                Data = pagedList.List.Select(selector).ToList(),
                CurrentPage = pagedList.CurrentPage,
                PageSize = pagedList.PageSize,
                Total = pagedList.Total,
                ErrorMessage = string.Empty,
                Host = controller.Request.Host.ToString(),
                TraceId = controller.HttpContext.TraceIdentifier,
            };
        }

        public static OptionsData<T> OptionsData<T>(this ControllerBase controller, List<T> items)
        {
            return new Web.OptionsData<T>
            {
                Success = true,
                Data = items,
                ErrorMessage = string.Empty,
                Host = controller.Request.Host.ToString(),
                TraceId = controller.HttpContext.TraceIdentifier,
            };
        }

        public static ApiData Error(this ControllerBase controller, string? errorMessage)
        {
            return new ApiData
            {
                Success = false,
                ErrorMessage = errorMessage,
                Host = controller.Request.Host.ToString(),
                TraceId = controller.HttpContext.TraceIdentifier,
            };
        }
    }

}
