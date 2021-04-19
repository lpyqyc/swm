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

using Arctic.AspNetCore;
using Arctic.NHibernateExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.International.Converters.PinYinConverter;
using NHibernate.Linq;
using Serilog;
using Swm.Materials;
using Swm.Ops;
using Swm.Palletization;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 提供物料 api
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MatlController : ControllerBase
    {
        readonly ILogger _logger;
        readonly NHibernate.ISession _session;
        readonly Func<Material> _materialFactory;
        readonly OpHelper _opHelper;
        readonly FlowHelper _flowHelper;
        readonly PalletizationHelper _palletizationHelper;
        readonly IPalletCodeValidator _palletCodeValidator;
        public MatlController(
            NHibernate.ISession session,
            Func<Material> materialFactory, 
            FlowHelper flowHelper, 
            PalletizationHelper palletizationHelper, 
            OpHelper opHelper,
            IPalletCodeValidator palletCodeValidator,
            ILogger logger)
        {
            _logger = logger;
            _materialFactory = materialFactory;
            _opHelper = opHelper;
            _flowHelper = flowHelper;
            _palletizationHelper = palletizationHelper;
            _session = session;
            _palletCodeValidator = palletCodeValidator;
        }

        /// <summary>
        /// 物料列表
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet("get-material-list")]
        [OperationType(OperationTypes.查看物料)]
        public async Task<ListData<MaterialInfo>> GetMaterialList([FromQuery] MaterialListArgs args)
        {
            var pagedList = await _session.Query<Material>().SearchAsync(args, args.Sort, args.Current, args.PageSize);

            return this.ListData(pagedList, x => new MaterialInfo
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
        /// 获取物料的选项列表
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet("get-material-options")]
        public async Task<OptionsData<MaterialInfo>> GetMaterialOptions([FromQuery] MaterialOptionsArgs args)
        {
            var items = await _session.Query<Material>()
                .FilterByKeyword(args.Keyword, args.MaterialType)
                .Select(x => new MaterialInfo
                {
                    MaterialId = x.MaterialId,
                    MaterialCode = x.MaterialCode,
                    Description = x.Description,
                    Specification = x.Specification,
                    MaterialType = x.MaterialType,
                    Uom = x.Uom,
                })
                .Take(args.Limit ?? 10)
                .ToListAsync();

            return this.OptionsData(items);
        }

        /// <summary>
        /// 查找有库存的批号
        /// </summary>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet("get-batch-options")]
        public async Task<OptionsData<string>> GetBatchOptions([FromQuery] BatchOptionsArgs args)
        {
            var keyword = args?.Keyword?.Trim();
            var materialCode = args?.MaterialCode?.Trim();
            var stockStatus = args?.StockStatus?.Trim();

            var q = _session.Query<UnitloadItem>();
            if (keyword != null)
            {
                q = q.Where(x => x.Batch.Contains(keyword));
            }

            if (materialCode != null)
            {
                q = q.Where(x => x.Material.MaterialCode == materialCode);
            }

            if (stockStatus != null)
            {
                q = q.Where(x => x.StockStatus == stockStatus);
            }

            List<string> arr = await q
                .Select(x => x.Batch)
                .OrderBy(x => x)
                .Distinct()
                .Take(args?.Limit ?? 10)
                .ToListAsync();

            return this.OptionsData(arr);
        }

        /// <summary>
        /// 获取物料类型的选项列表
        /// </summary>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet("get-material-type-options")]
        public async Task<OptionsData<MaterialTypeInfo>> GetMaterialTypeOptions()
        {
            var list = await _session.Query<MaterialTypeInfo>().ToListAsync();
            var result = this.OptionsData(list);
            return result;
        }

        /// <summary>
        /// 导入物料主数据
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [OperationType(OperationTypes.导入物料主数据)]
        [AutoTransaction]
        [HttpPost("import-materials")]
        public async Task<ApiData> ImportMaterials(IFormFile file)
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
                    material = _materialFactory.Invoke();
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
                string pinyin = GetPinyin(material.Description);
                if (pinyin.Length > 20)
                {
                    pinyin = pinyin.Substring(0, 20);
                }
                material.MnemonicCode = pinyin;

                await _session.SaveOrUpdateAsync(material);
                _logger.Information("已导入物料 {material}", material.MaterialCode);
                imported++;
            }

            _ = await _opHelper.SaveOpAsync($"导入 {imported}，覆盖 {covered}");

            return this.Success($"导入 {imported}，覆盖 {covered}");


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

            static string GetPinyin(string? text)
            {
                if (text == null)
                {
                    return string.Empty;
                }
                var charArray = text
                    .Select(ch =>
                    {
                        if (ChineseChar.IsValidChar(ch))
                        {
                            ChineseChar cc = new ChineseChar(ch);
                            if (cc.Pinyins.Count > 0 && cc.Pinyins[0].Length > 0)
                            {
                                return cc.Pinyins[0][0];
                            }
                        }

                        return ch;
                    })
                    .ToArray();
                return new string(charArray);
            }
        }



        /// <summary>
        /// 获取流水列表
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet("get-flow-list")]
        public async Task<ListData<FlowInfo>> GetFlowList([FromQuery] FlowListArgs args)
        {
            var pagedList = await _session.Query<Flow>().SearchAsync(args, args.Sort, args.Current, args.PageSize);

            return this.ListData(pagedList, x => new FlowInfo
            {
                FlowId = x.FlowId,
                ctime = x.ctime,
                MaterialCode = x.Material?.MaterialCode,
                MaterialType = x.Material?.MaterialType,
                Description = x.Material.Description,
                Batch = x.Batch,
                StockStatus = x.StockStatus,
                BizType = x.BizType,
                Direction = x.Direction,
                PalletCode = x.PalletCode,
                OrderCode = x.OrderCode,
                BizOrder = x.BizOrder,
                OperationType = x.OpType,
                Quantity = x.Quantity,
                Uom = x.Uom,
                cuser = x.cuser,
                Comment = x.Comment,
            });
        }


        /// <summary>
        /// 获取业务类型选择列表
        /// </summary>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet("get-biz-type-options")]
        public async Task<OptionsData<BizTypeInfo>> GetBizTypeOptions()
        {
            var list = await _session.Query<BizTypeInfo>().ToListAsync();
            var result = this.OptionsData(list);
            return result;
        }

        /// <summary>
        /// 获取操作提示类型的选项列表
        /// </summary>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPost("get-op-hint-type-options")]
        public async Task<OptionsData<OpHintTypeOption>> GetOpHintTypeOptions()
        {
            throw new NotImplementedException();

            // return this.OptionsData(items);
        }

        /// <summary>
        /// 获取库存状态的选项列表
        /// </summary>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet("get-stock-status-options")]
        public async Task<OptionsData<StockStatusInfo>> GetStockStatusOptions()
        {
            var list = await _session.Query<StockStatusInfo>().ToListAsync();
            var result = this.OptionsData(list);
            return result;
        }

        /// <summary>
        /// 验证托盘号
        /// </summary>
        /// <param name="palletCode"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet("validate-pallet-code")]
        public async Task<ApiData> ValidatePalletCode(string palletCode)
        {
            var b = _palletCodeValidator.IsWellFormed(palletCode, out string msg);
            if (b == false)
            {
                return this.Failure(msg);
            }

            bool e = await _session.Query<Unitload>().AnyAsync(x => x.PalletCode == palletCode);
            if (e)
            {
                return this.Failure("托盘号已占用");
            }

            return this.Success();
        }

        /// <summary>
        /// 货载列表
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet("get-unitload-list")]
        [OperationType(OperationTypes.查看货载)]
        public async Task<ListData<UnitloadInfo>> GetUnitloadList([FromQuery] UnitloadListArgs args)
        {
            var pagedList = await _session.Query<Unitload>().SearchAsync(args, args.Sort, args.Current, args.PageSize);
            return this.ListData(pagedList, x => DtoConvert.ToUnitloadInfo(x));
        }

        /// <summary>
        /// 货载项列表，用于在变更状态页面展示货载项
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet("get-unitload-item-list")]
        [OperationType(OperationTypes.查看货载)]
        public async Task<ListData<ChangeStockStatusUnitloadItemInfo>> GetChangeStockStatusUnitloadItemList([FromQuery] ChangeStockStatusUnitloadItemListArgs args)
        {
            var pagedList = await _session.Query<UnitloadItem>().SearchAsync(args, args.Sort, args.Current, args.PageSize);

            return this.ListData(pagedList, x => new ChangeStockStatusUnitloadItemInfo
            {
                UnitloadItemId = x.UnitloadItemId,
                PalletCode = x.Unitload?.PalletCode,
                LocationCode = x.Unitload?.CurrentLocation?.LocationCode,
                StreetletCode = x.Unitload?.CurrentLocation?.Streetlet?.StreetletCode,
                BeingMoved = x.Unitload?.BeingMoved ?? default,
                MaterialId = x.Material?.MaterialId ?? default,
                MaterialCode = x.Material?.MaterialCode,
                MaterialType = x.Material?.MaterialType,
                Description = x.Material?.Description,
                Specification = x.Material?.Specification,
                Batch = x.Batch,
                StockStatus = x.StockStatus,
                Quantity = x.Quantity,
                Uom = x.Uom,
                Allocated = (x.Unitload?.CurrentUat != null),
                CanChangeStockStatus = CanChangeStockStatus(x).ok,
                ReasonWhyStockStatusCannotBeChanged = CanChangeStockStatus(x).reason,
            });
        }

        /// <summary>
        /// 货载详情
        /// </summary>
        /// <param name="palletCode"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpGet("get-unitload-detail/{palletCode}")]
        [OperationType(OperationTypes.查看货载)]
        public async Task<ApiData<UnitloadDetail>> GetUnitloadDetail(string palletCode)
        {
            var unitload = await _session.Query<Unitload>()
                .Where(x => x.PalletCode == palletCode)
                .SingleOrDefaultAsync();
            if (unitload == null)
            {
                throw new InvalidOperationException("货载不存在。");
            }

            return this.Success(DtoConvert.ToUnitloadDetail(unitload));
        }

        /// <summary>
        /// 独立组盘
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPost("palletize-standalonely")]
        [OperationType(OperationTypes.独立组盘)]
        public async Task<ApiData> PalletizeStandalonely(PalletizeStandalonelyArgs args)
        {
            List<PalletizationItemInfo<DefaultStockKey>> items = new List<PalletizationItemInfo<DefaultStockKey>>();

            Material material = await _session.Query<Material>().GetMaterialAsync(args.MaterialCode);
            if (material == null)
            {
                throw new InvalidOperationException($"物料主数据不存在：【{args.MaterialCode}】");
            }
            DefaultStockKey stockKey = new DefaultStockKey(material, args.Batch, args.StockStatus, args.Uom);
            items.Add(new PalletizationItemInfo<DefaultStockKey> { StockKey = stockKey, Quantity = args.Quantity });


            var op = await _opHelper.SaveOpAsync($"托盘号：{args.PalletCode}");

            await _palletizationHelper.PalletizeAsync(args.PalletCode,
                                                      items,
                                                      op.OperationType,
                                                      "独立入库" // TODO 这里有硬编码文本
                                                      );

            return this.Success();
        }

        /// <summary>
        /// 更改库存状态
        /// </summary>
        /// <param name="itemIds">半角逗号分隔的货载项Id</param>
        /// <param name="args"></param>
        /// <returns></returns>
        [AutoTransaction]
        [HttpPost("change-stock-status/{itemIds}")]
        [OperationType(OperationTypes.更改库存状态)]
        public async Task<ApiData> ChangeStockStatus(string itemIds, ChangeStockStatusArgs args)
        {
            if (string.IsNullOrWhiteSpace(args.IssuingStockStatus))
            {
                throw new InvalidOperationException("未提供发出状态。");
            }
            if (string.IsNullOrWhiteSpace(args.ReceivingStockStatus))
            {
                throw new InvalidOperationException("未提供接收状态。");
            }

            if (args.IssuingStockStatus == args.ReceivingStockStatus)
            {
                throw new InvalidOperationException("发出状态和接收状态不能相同。");
            }

            List<int> list = itemIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => int.Parse(x))
                .ToList();
            List<UnitloadItem> unitloadItems = await _session.Query<UnitloadItem>()
                .Where(x => list.Contains(x.UnitloadItemId))
                .ToListAsync();

            const string bizType = "库存状态变更";

            if (unitloadItems.Count == 0)
            {
                throw new InvalidOperationException("未选中任何货载项。");
            }

            foreach (UnitloadItem item in unitloadItems)
            {
                if (item.StockStatus != args.IssuingStockStatus)
                {
                    throw new InvalidOperationException("货载项的状态与发出状态不一致");
                }

                var (ok, reason) = CanChangeStockStatus(item);
                if (ok == false)
                {
                    throw new InvalidOperationException(reason);
                }
            }

            var op = await _opHelper.SaveOpAsync("{0}-->{1}", args.IssuingStockStatus, args.ReceivingStockStatus);

            foreach (var item in unitloadItems)
            {
                // TODO 扩展点：替换泛型参数 DefaultStockKey
                // 1 生成发货流水
                Flow flowOut = await _flowHelper.CreateAndSaveAsync(item.GetStockKey<DefaultStockKey>(),
                                                                    item.Quantity,
                                                                    FlowDirection.Outbound,
                                                                    bizType,
                                                                    op.OperationType,
                                                                    item.Unitload.PalletCode).ConfigureAwait(false);
                // 2 更改库存数据
                item.StockStatus = args.ReceivingStockStatus;

                // 3 生成收货流水
                Flow flowIn = await _flowHelper.CreateAndSaveAsync(item.GetStockKey<DefaultStockKey>(),
                                                                   item.Quantity,
                                                                   FlowDirection.Inbound,
                                                                   bizType,
                                                                   op.OperationType,
                                                                   item.Unitload.PalletCode).ConfigureAwait(false);

                await _session.UpdateAsync(item.Unitload).ConfigureAwait(false);
            }

            return this.Success();
        }

        internal static (bool ok, string reason) CanChangeStockStatus(UnitloadItem item)
        {
            if (item.Unitload == null)
            {
                throw new Exception("货载明细不属于任何货载");
            }

            List<string> list = new List<string>();

            if (item.Unitload.CurrentUat != null)
            {
                list.Add("已分配");
            }

            if (item.Unitload.BeingMoved)
            {
                list.Add("有任务");
            }

            if (item.Unitload.HasCountingError)
            {
                list.Add("有盘点错误");
            }

            if (!string.IsNullOrWhiteSpace(item.Unitload.OpHintType))
            {
                list.Add("有操作提示");
            }

            if (list.Count > 0)
            {
                return (false, string.Join(", ", list));
            }

            return (true, string.Empty);
        }
    }


}
