﻿// Copyright 2020 王建军
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

using Arctic.AppCodes;
using Arctic.AspNetCore;
using Arctic.NHibernateExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using NHibernate.Linq;
using NPOI.SS.UserModel;
using Serilog;
using Swm.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Swm.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaterialsController : ControllerBase
    {
        readonly ILogger _logger;
        readonly NHibernate.ISession _session;
        readonly IMaterialFactory _materialFactory;
        readonly OpHelper _opHelper;

        public MaterialsController(NHibernate.ISession session, IMaterialFactory materialFactory, OpHelper opHelper, ILogger logger)
        {
            _logger = logger;
            _materialFactory = materialFactory;
            _opHelper = opHelper;
            _session = session;
        }

        /// <summary>
        /// 物料列表
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet("list")]
        [OperationType(OperationTypes.查看物料)]
        public async Task<ListData<MaterialListItem>> List([FromQuery] MaterialListArgs args)
        {
            var pagedList = await _session.Query<Material>().SearchAsync(args, args.Sort, args.Current, args.PageSize);

            return this.ListData(pagedList, x => new MaterialListItem
            {
                MaterialId = x.MaterialId,
                MaterialCode = x.MaterialCode,
                MaterialType = x.MaterialType,
                Description = x.Description,
                Specification = x.Specification,
                BatchEnabled = x.BatchEnabled,
                MaterialGroup = x.MaterialGroup,
                ValidDays = x.ValidDays,
                StandingTime = x.StandingTime,
                AbcClass = x.AbcClass,
                Uom = x.Uom,
                LowerBound = x.LowerBound,
                UpperBound = x.UpperBound,
                DefaultQuantity = x.DefaultQuantity,
                DefaultStorageGroup = x.DefaultStorageGroup,
                Comment = x.Comment
            });
        }

        /// <summary>
        /// 物料选择列表
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet("get-select-list")]
        public async Task<ApiData<List<MaterialSelectListItem>>> SelectList(MaterialSelectListArgs args)
        {
            var items = await _session.Query<Material>()
                .FilterByKeyword(args.Keyword, args.MaterialType)
                .Select(x => new MaterialSelectListItem
                {
                    MaterialId = x.MaterialId,
                    MaterialCode = x.MaterialCode,
                    Description = x.Description,
                    MaterialType = x.MaterialType,
                })
                .Take(args.Limit ?? 10)
                .ToListAsync();

            return this.Success2(items);
        }

        /// <summary>
        /// 获取物料类型的选择列表
        /// </summary>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet("get-material-type-select-list")]
        public async Task<ApiData<List<MaterialTypeSelectListItem>>> MaterialTypesSelectList()
        {
            var appCodes = await _session
                .Query<AppCode>()
                .GetAppCodesAsync(AppCodeTypes.MaterialType);

            var list = appCodes
                .Select(x => new MaterialTypeSelectListItem
                {
                    MaterialType = x.AppCodeValue,
                    Description = x.Description,
                    Scope = x.Scope,
                    DisplayOrder = x.DisplayOrder,
                }).ToList();

            return this.Success2(list);
        }

        /// <summary>
        /// 导入物料主数据
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [OperationType(OperationTypes.导入物料主数据)]
        [AutoTransaction]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [HttpPost("import")]
        public async Task<ApiData> Import(IFormFile file)
        {
            string[] arr = new[] { ".xlsx", ".xls" };
            if (arr.Contains(Path.GetExtension(file.FileName)?.ToLower()) == false)
            {
                throw new InvalidOperationException("无效的文件扩展名。");
            }


            string filename = await WriteFileAsync(file);

            DataTable dt = ExcelUtil.ReadDataSet(filename).Tables[0];

            int imported = 0;
            int covered = 0;
            int empty = 0;

            foreach (DataRow row in dt.Rows)
            {
                string? mcode = Convert.ToString(row["编码"]);
                if (string.IsNullOrWhiteSpace(mcode))
                {
                    // 忽略空行
                    empty++;
                    continue;
                }
                Material material = await _session.Query<Material>().Where(x => x.MaterialCode == mcode).SingleOrDefaultAsync();
                if (material != null)
                {
                    covered++;
                    _logger.Warning("将覆盖已存在的物料 {material}", material.MaterialCode);
                }
                else
                {
                    material = _materialFactory.CreateMaterial();
                    material.MaterialCode = Convert.ToString(row["编码"]);
                }

                material.Description = Convert.ToString(row["描述"]);
                material.BatchEnabled = Convert.ToBoolean(row["批次管理"]);
                material.StandingTime = 24;
                material.ValidDays = Convert.ToInt32(row["有效天数"]);
                material.MaterialType = Convert.ToString(row["物料类型"]);
                material.Uom = Convert.ToString(row["计量单位"])?.ToUpper();
                material.DefaultQuantity = Convert.ToDecimal(row["每托数量"]);
                material.Specification = Convert.ToString(row["规格型号"]);
                // TODO 取拼音首字母
                // material.MnemonicCode = PinyinUtil.ChineseCap(material.Description).NullSafeLeft(20);

                await _session.SaveOrUpdateAsync(material);
                _logger.Information("已导入物料 {material}", material.MaterialCode);
                imported++;
            }

            _ = await _opHelper.SaveOpAsync($"导入 {imported}，覆盖 {covered}");

            return this.Success2($"导入 {imported}，覆盖 {covered}");


            static async Task<string> WriteFileAsync(IFormFile file)
            {
                var dir = Path.Combine(Directory.GetCurrentDirectory(), "Upload\\files");
                Directory.CreateDirectory(dir);

                string fileName = $"up-m-{DateTime.Now:yyyyMMddHHmmss}" + Path.GetExtension(file.FileName);
                var path = Path.Combine(dir, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return path;
            }


        }
    }
}
