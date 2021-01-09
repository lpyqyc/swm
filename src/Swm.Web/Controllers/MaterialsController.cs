// Copyright 2020 王建军
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
using NPOI.XSSF.UserModel;
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
    [Route("[controller]")]
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
        [HttpGet]
        [OperationType(OperationTypes.查看物料)]
        public async Task<ListResult<MaterialListItem>> List([FromQuery] MaterialListArgs args)
        {
            var pagedList = await _session.Query<Material>().SearchAsync(args, args.Sort, args.Current, args.PageSize);

            return new ListResult<MaterialListItem>
            {
                Success = true,
                Data = pagedList.List.Select(x => new MaterialListItem
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
                }),
                Total = pagedList.Total
            };
        }

        /// <summary>
        /// 物料选择列表
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet]
        [Route("select-list")]
        public async Task<List<MaterialSelectListItem>> SelectList(MaterialSelectListArgs args)
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

            return items;
        }

        /// <summary>
        /// 获取物料类型的选择列表
        /// </summary>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet]
        [Route("material-type-select-list")]
        public async Task<List<MaterialTypeSelectListItem>> MaterialTypesSelectList()
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

            return list;
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
        [HttpPost("actions/import")]
        public async Task<ActionResult> Import(IFormFile file)
        {
            string[] arr = new[] { ".xlsx", ".xls" };
            if (arr.Contains(Path.GetExtension(file.FileName)?.ToLower()) == false)
            {
                return BadRequest(new { message = "Invalid file extension" });
            }


            string filename = await WriteFileAsync(file);

            DataTable dt = ReadFile(filename);

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

            _ = await _opHelper.SaveOpAsync("导入 {0}，覆盖 {1}", imported, covered);

            return Ok();


            static async Task<string> WriteFileAsync(IFormFile file)
            {
                var dir = Path.Combine(Directory.GetCurrentDirectory(), "Upload\\files");
                Directory.CreateDirectory(dir);

                string fileName = DateTime.Now.ToString("up-m-yyyyMMdd HHmmss") + Path.GetExtension(file.FileName);
                var path = Path.Combine(dir, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return path;
            }

            static DataTable ReadFile(string filePath)
            {
                XSSFWorkbook hssfworkbook;

                using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    hssfworkbook = new XSSFWorkbook(file);
                }

                NPOI.SS.UserModel.ISheet sheet = hssfworkbook.GetSheetAt(0);
                DataTable table = new DataTable();
                IRow headerRow = sheet.GetRow(0);//第一行为标题行
                int cellCount = headerRow.LastCellNum;//LastCellNum = PhysicalNumberOfCells
                int rowCount = sheet.LastRowNum;//LastRowNum = PhysicalNumberOfRows - 1

                //handling header.
                for (int i = headerRow.FirstCellNum; i < cellCount; i++)
                {
                    DataColumn column = new DataColumn(headerRow.GetCell(i).StringCellValue);
                    table.Columns.Add(column);
                }
                for (int i = (sheet.FirstRowNum + 1); i <= rowCount; i++)
                {
                    IRow row = sheet.GetRow(i);
                    DataRow dataRow = table.NewRow();

                    if (row != null)
                    {
                        for (int j = row.FirstCellNum; j < cellCount; j++)
                        {
                            if (row.GetCell(j) != null)
                                dataRow[j] = row.GetCell(j);
                        }
                    }
                    table.Rows.Add(dataRow);
                }
                return table;
            }

        }


        

    }

}
