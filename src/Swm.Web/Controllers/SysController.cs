using Arctic.AppSettings;
using Arctic.AspNetCore;
using Arctic.NHibernateExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NHibernate;
using Serilog;
using Swm.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 提供系统 api
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SysController : ControllerBase
    {
        readonly ISession _session;
        readonly ILogger _logger;
        readonly OpHelper _opHelper;
        readonly IAppSettingService _appSettingService;

        /// <summary>
        /// 初始化新实例。
        /// </summary>
        /// <param name="appSettingService"></param>
        /// <param name="session"></param>
        /// <param name="opHelper"></param>
        /// <param name="logger"></param>
        public SysController(
            IAppSettingService appSettingService, 
            ISession session, 
            OpHelper opHelper, 
            ILogger logger
            )
        {
            _appSettingService = appSettingService;
            _session = session;
            _opHelper = opHelper;
            _logger = logger;
        }

        /// <summary>
        /// 系统参数列表
        /// </summary>
        /// <param name="args">查询参数</param>
        /// <returns></returns>
        [HttpGet("get-app-setting-list")]
        [DebugShowArgs]
        [AutoTransaction]
        public async Task<ListData<AppSetting>> GetAppSettingList([FromQuery] AppSettingListArgs args)
        {
            var pagedList = await _session.Query<AppSetting>().SearchAsync(args, "settingName ASC", 1, 9999);
            return this.ListData(pagedList);
        }


        /// <summary>
        /// 更改参数
        /// </summary>
        /// <param name="settingName">参数名称</param>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPost("update-app-setting/{settingName}")]
        [OperationType(OperationTypes.更改系统参数)]
        public async Task<ApiData> UpdateAppSetting([Required] string? settingName, UpdateAppSettingArgs args)
        {
            settingName = settingName?.Trim();
            if (string.IsNullOrEmpty(settingName))
            {
                throw new InvalidOperationException("参数名不能为空。");
            }
            var setting = await _appSettingService.GetAsync(settingName);
            if (setting == null)
            {
                throw new InvalidOperationException("参数不存在");
            }

            var prevValue = setting.SettingValue;

            switch (setting.SettingType)
            {
                case AppSettingTypes.字符串:
                    await _appSettingService.SetStringAsync(settingName, args.SettingValue);
                    break;
                case AppSettingTypes.布尔:
                    await _appSettingService.SetBooleanAsync(settingName, Convert.ToBoolean(args.SettingValue));
                    break;
                case AppSettingTypes.数字:
                    await _appSettingService.SetNumberAsync(settingName, Convert.ToDecimal(args.SettingValue));
                    break;
                default:
                    break;
            }

            _logger.Information("将参数 {settingName} 的值由 {prevValue} 改为 {value}", settingName, prevValue, args.SettingValue);
            await _opHelper.SaveOpAsync($"参数名 {settingName}，前值 {prevValue}，新值 {args.SettingValue}", settingName, prevValue, args.SettingValue);

            return this.Success();
        }

        /// <summary>
        /// 更改参数
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPost("create-app-setting")]
        [OperationType(OperationTypes.更改系统参数)]
        public async Task<ApiData> Create(CreateAppSettingArgs args)
        {
            var settingName = args.SettingName?.Trim();
            if (string.IsNullOrEmpty(settingName))
            {
                throw new InvalidOperationException("参数名不能为空。");
            }

            var setting = await _appSettingService.GetAsync(settingName);
            if (setting != null)
            {
                throw new InvalidOperationException("餐宿已存在。");
            }

            switch (args.SettingType)
            {
                case AppSettingTypes.字符串:
                    await _appSettingService.SetStringAsync(settingName, args.SettingValue);
                    break;
                case AppSettingTypes.布尔:
                    await _appSettingService.SetBooleanAsync(settingName, Convert.ToBoolean(args.SettingValue));
                    break;
                case AppSettingTypes.数字:
                    await _appSettingService.SetNumberAsync(settingName, Convert.ToDecimal(args.SettingValue));
                    break;
                default:
                    break;
            }

            _logger.Information("创建参数 {settingName}，值为 {value}", settingName, args.SettingValue);
            await _opHelper.SaveOpAsync($"参数名 {settingName}，值 {args.SettingValue}", settingName, args.SettingValue);

            return this.Success();
        }


        /// <summary>
        /// 操作记录列表
        /// </summary>
        /// <param name="args">查询参数</param>
        /// <returns></returns>
        [HttpGet("get-op-list")]
        [DebugShowArgs]
        [AutoTransaction]
        public async Task<ListData<OpListInfo>> GetOpList([FromQuery] OpListArgs args)
        {
            if (string.IsNullOrWhiteSpace(args.Sort))
            {
                args.Sort = "opId desc";
            }
            var pagedList = await _session.Query<Op>().SearchAsync(args, args.Sort, args.Current, args.PageSize);
            return this.ListData(pagedList, x => new OpListInfo
            {
                OpId = x.OpId,
                ctime = x.ctime,
                cuser = x.cuser,
                OperationType = x.OperationType,
                Url = x.Url,
                Comment = x.Comment
            });
        }

    }
}
