using Arctic.AppSettings;
using Arctic.AspNetCore;
using Arctic.NHibernateExtensions;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using Serilog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Swm.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppSettingsController : ControllerBase
    {
        readonly ISession _session;
        readonly ILogger _logger;
        readonly OpHelper _opHelper;
        readonly IAppSettingService _appSettingService;

        public AppSettingsController(IAppSettingService appSettingService, ISession session, OpHelper opHelper, ILogger logger)
        {
            _appSettingService = appSettingService;
            _session = session;
            _opHelper = opHelper;
            _logger = logger;
        }

        /// <summary>
        /// 程序设置列表
        /// </summary>
        /// <param name="args">查询参数</param>
        /// <returns></returns>
        [HttpGet("list")]
        [DebugShowArgs]
        [AutoTransaction]
        public async Task<ListData<AppSetting>> List([FromQuery] AppSettingListArgs args)
        {
            var pagedList = await _session.Query<AppSetting>().SearchAsync(args, "settingName ASC", 1, 9999);
            return this.ListData(pagedList);
        }


        /// <summary>
        /// 更改设置
        /// </summary>
        /// <param name="settingName">设置名称</param>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPost("set/{settingName}")]
        [OperationType(OperationTypes.更改设置)]
        public async Task<ApiData> Set([Required] string? settingName, SetAppSettingArgs args)
        {
            settingName = settingName?.Trim();
            if (string.IsNullOrEmpty(settingName))
            {
                throw new InvalidOperationException("设置名不能为空。");
            }
            var setting = await _appSettingService.GetAsync(settingName);
            if (setting == null)
            {
                throw new InvalidOperationException("设置不存在");
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

            _logger.Information("将设置 {settingName} 的值由 {prevValue} 改为 {value}", settingName, prevValue, args.SettingValue);
            await _opHelper.SaveOpAsync($"设置名 {settingName}，前值 {prevValue}，新值 {args.SettingValue}", settingName, prevValue, args.SettingValue);

            return this.Success2();
        }

        /// <summary>
        /// 更改设置
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPost("create")]
        [OperationType(OperationTypes.更改设置)]
        public async Task<ApiData> Create(CreateAppSettingArgs args)
        {
            var settingName = args.SettingName?.Trim();
            if (string.IsNullOrEmpty(settingName))
            {
                throw new InvalidOperationException("设置名不能为空。");
            }

            var setting = await _appSettingService.GetAsync(settingName);
            if (setting != null)
            {
                throw new InvalidOperationException("设置已存在。");
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

            _logger.Information("创建设置 {settingName}，值为 {value}", settingName, args.SettingValue);
            await _opHelper.SaveOpAsync($"设置名 {settingName}，值 {args.SettingValue}", settingName, args.SettingValue);

            return this.Success2();
        }
    }
}
