//// Copyright 2020-2021 王建军
////
//// Licensed under the Apache License, Version 2.0 (the "License");
//// you may not use this file except in compliance with the License.
//// You may obtain a copy of the License at
////
////     http://www.apache.org/licenses/LICENSE-2.0
////
//// Unless required by applicable law or agreed to in writing, software
//// distributed under the License is distributed on an "AS IS" BASIS,
//// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//// See the License for the specific language governing permissions and
//// limitations under the License.

//using Arctic.AspNetCore;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using NHibernate;
//using Serilog;
//using Swm.Model;
//using System;
//using System.Threading.Tasks;

//namespace Swm.Web.Controllers
//{
//    public class AccountController : Controller
//    {
//        readonly ILogger _logger;

//        readonly ISession _session;

//        readonly OpHelper _opHelper;

//        readonly PasswordHelper _passwordHelper;

//        public AccountController(OpHelper opHelper)
//        {
//            _opHelper = opHelper;
//        }

//        [HttpPost("login")]
//        [AutoTransaction]
//        [AllowAnonymous]
//        public async Task<ApiData> Login(LoginViewModel model)
//        {
//            if (!ModelState.IsValid)
//            {
//                throw new InvalidOperationException("ModelState 无效。");
//            }

//            bool succ = false;
//            string[] roles;
//            string reason;

//            succ = _passwordHelper.ValidateUser(model.UserName, model.Password, out roles, out reason);


//            // 操作记录
//            if (succ)
//            {
//                var op = await _opHelper.SaveOpAsync("登录成功");
//                op.cuser = model.UserName;
//                return this.Success();
//            }
//            else
//            {
//                var op = await _opHelper.SaveOpAsync("登录失败。用户名：{ model.UserName}，消息：{ reason}", model.UserName, reason);
//                op.cuser = "匿名";
//                this.Failure(reason);
//            }
//        }


//        [HttpPost]
//        [AutoTransaction]
//        [OperationType(OperationTypes.修改密码)]
//        public async Task<ApiData> ChangePassword(ChangePasswordViewModel model)
//        {
//            // 更改密码
//            _passwordHelper.ChangePassword(this.HttpContext.User.Identity.Name, model.Password);

//            // 操作记录
//            Op op = new Op();
//            op.cuser = HttpContext.User.Identity.Name;
//            op.OpType = "修改个人密码";
//            op.Url = HttpContext.Request.RawUrl;

//            _session.Save(op);

//            _logger.Information("修改密码成功");
//            return Json(new { success = true, msg = "密码修改成功。" });
//        }

//        [HttpGet]
//        [AllowAnonymous]
//        [AutoTransaction]
//        public ActionResult Logout()
//        {
//            throw new NotImplementedException();
//        }



//    }

//}
