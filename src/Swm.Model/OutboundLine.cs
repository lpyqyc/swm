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

using NHibernate;
using Serilog;
using System;
using System.ComponentModel.DataAnnotations;

namespace Swm.Model
{
    /// <summary>
    /// 表示出库单明细。
    /// </summary>
    public class OutboundLine
    {
        public OutboundLine()
        {
            this.Uom = Cst.None;
        }

        /// <summary>
        /// 出库单明细Id。
        /// </summary>
        public virtual Int32 OutboundLineId { get; set; }

        /// <summary>
        /// 所属出库单。
        /// </summary>
        [Required]
        public virtual OutboundOrder OutboundOrder { get; internal protected set; }

        /// <summary>
        /// 要出库的物料。
        /// </summary>
        [Required]
        public virtual Material Material { get; set; }


        /// <summary>
        /// 要出库的批号，可以为空
        /// </summary>
        [MaxLength(FIELD_LENGTH.BATCH)]
        public virtual String? Batch { get; set; }


        /// <summary>
        /// 要出库的库存状态。
        /// </summary>
        [MaxLength(FIELD_LENGTH.APP_CODE)]
        public virtual string StockStatus { get; set; }

        /// <summary>
        /// 计量单位。
        /// </summary>
        [Required]
        [MaxLength(FIELD_LENGTH.UOM)]
        public virtual String Uom { get; set; } = Cst.None;

        /// <summary>
        /// 需求数量。
        /// </summary>
        public virtual decimal QuantityRequired { get; set; }

        /// <summary>
        /// 已出数量
        /// </summary>
        public virtual decimal QuantityDelivered { get; set; }


        /// <summary>
        /// 未出数量，MAX(应出-已出, 0)
        /// </summary>
        public virtual decimal QuantityUndelivered
        {
            get
            {
                return Math.Max(QuantityRequired - QuantityDelivered, 0);
            }
            protected set
            {
            }
        }

        /// <summary>
        /// 指示此明细行是否发生过出库操作。发生过出库操作的出库明细不能被删除。
        /// 在第一次收货时，此属性变为 true，此后不会变回 false。
        /// </summary>
        public virtual bool Dirty { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public virtual String Comment { get; set; }
    }


//    /// <summary>
//    /// 出库单分配程序
//    /// </summary>
//    public class DefaultDeliveryOrderAllocator// : IDeliveryOrderAllocator
//    {
//        protected ILogger _logger;
//        protected ISession _session;

//        /// <summary>
//        /// </summary>
//        /// <param name="session"></param>
//        /// <param name="logger"></param>
//        public DefaultDeliveryOrderAllocator(ISession session, ILogger logger)
//        {
//            _session = session;
//            _logger = logger;
//        }

//        /// <summary>
//        /// 创建 CandidateItems 临时表。
//        /// 表定义如下：
//        /// create table #CandidateItems(
//        ///     DeliveryLineId int not null,            -- 出库行 id。
//        ///     ItemId int not null,                    -- 库存项 id。
//        ///     AllocOrder int not null,                -- 库存项的分配次序，从 1 开始，在出库行内唯一，框架将按照这个次序进行分配。
//        ///     NumberAvailable decimal(19,5),          -- 库存项中可用的数量，这是库存量减去已分配量的值。
//        ///     NumberAccumulate decimal(19,5),         -- 按 AllocOrder 排序时， NumberAvailable 列的累加数量。
//        ///     UNIQUE(DeliveryLineId, ItemId),
//        ///     UNIQUE CLUSTERED(DeliveryLineId, AllocOrder)
//        /// )        
//        /// </summary>
//        private void CreateTempTable()
//        {
//            const string sql = @"
//CREATE TABLE #CandidateItems(
//    DeliveryLineId INT NOT NULL,
//    ItemId int NOT NULL,
//    AllocOrder INT NOT NULL,
//    NumberAvailable DECIMAL(19,5),
//    NumberAccumulate DECIMAL(19,5),            
//    UNIQUE (DeliveryLineId, ItemId),
//    UNIQUE CLUSTERED(DeliveryLineId, AllocOrder)
//    )
//";
//            var q = _session.CreateSQLQuery(sql);
//            q.ExecuteUpdate();
//        }

//        /// <summary>
//        /// 删除 CandidateItems 临时表。
//        /// </summary>
//        private void DropTempTable()
//        {
//            const string sql = @"DROP TABLE #CandidateItems";
//            var q = _session.CreateSQLQuery(sql);
//            q.ExecuteUpdate();
//        }

//        /// <summary>
//        /// 为出库单分配库存。分配的范围取决于子类实现的 FillCandidateItemsTable 方法。
//        /// </summary>
//        /// <remarks>
//        /// 分配的步骤：
//        /// 1，框架：创建临时表；
//        /// 2，项目：重写 FillCandidateItemsTable 方法，填充临时表的 DeliveryLineId, ItemId, AllocOrder 三列，
//        ///    允许一个 ItemId 在临时表中出现多次，但在特定的 DeliveryLineId 下只能出现一次。
//        /// 3，框架：按照 Id 次序逐个处理每个出库行：
//        ///     a，更新这个出库行在临时表中的 NumberAvailable 列；
//        ///     b，按照 AllocOrder 次序将 NumberAvailable 的值累计到 NumberAccumulate 列；
//        ///     c，从临时表读取累计数量刚好满足出库行分配欠数的记录；
//        ///     d，对每个临时表记录，取出其对应的 Item 对象进行分配；
//        /// 4，框架：删除临时表；
//        /// 5，项目：检查是否所有出库行均无分配欠数，如果有，根据配置决定是否抛出异常；
//        /// </remarks>
//        /// <param name="deliveryOrder">需要进行分配的出库单</param>
//        /// <param name="laneways">用于分配的货载所在的巷道</param>
//        /// <param name="includeUnitLoads">要在分配中包含的货载，这些货载优先参与分配。</param>
//        /// <param name="excludeUnitLoads">要在分配中排除的货载，这些货载不会参与分配，即使出现在 includeUnitLoads 中。</param>
//        public virtual void Allocate(DeliveryOrder deliveryOrder, Laneway[] laneways, UnitLoad[] includeUnitLoads, UnitLoad[] excludeUnitLoads)
//        {
//            if (deliveryOrder == null)
//            {
//                throw new ArgumentNullException(nameof(deliveryOrder));
//            }

//            this.CreateTempTable();
//            bool ovr = false;
//            FillCandidateItemsTableOvr(deliveryOrder, laneways, includeUnitLoads, excludeUnitLoads, ref ovr);
//            if (!ovr)
//            {
//                this.FillCandidateItemsTable(deliveryOrder, laneways, includeUnitLoads, excludeUnitLoads);
//            }

//            foreach (var line in deliveryOrder.Lines)
//            {
//                ProcessLine(line);
//            }

//            this.DropTempTable();

//        }

//        /// <summary>
//        /// 处理单个出库行。
//        /// </summary>
//        /// <param name="line">出库行</param>
//        private void ProcessLine(DeliveryLine line)
//        {
//            _session.Flush();
//            this.RefreshNumberColumns(line);

//            var list = ReadRecords(line.Id, line.ComputeNumberShort());

//            if (ConfigurationManager.AppSettings["debug.log-candidate-items"] == "on")
//            {
//                StringBuilder sb = new StringBuilder();
//                sb.AppendFormat("出库行#{0}，欠数：{1:0.#}，候选库存项：", line.Id, line.ComputeNumberShort());
//                sb.AppendLine();
//                sb.AppendLine("库存项\t可用\t累加");
//                foreach (var item in list)
//                {
//                    sb.AppendFormat("{0}\t{1:0.#}\t{2:0.#}", item.ItemId, item.NumberAvailable, item.NumberAccumulate);
//                    sb.AppendLine();
//                }
//                _logger.Debug(sb.ToString());
//            }

//            // 预先加载，使 nhibernate 可以批量读取，提升性能
//            foreach (var c in list)
//            {
//                _unitLoadRepository.Load(c.ItemId);
//            }

//            foreach (var c in list)
//            {
//                if (c.NumberAvailable == 0)
//                {
//                    continue;
//                }

//                Item item = _unitLoadRepository.GetItem(c.ItemId);
//                decimal take;
//                var testResult = this.AllocateItem(line, item, out take);
//                switch (testResult)
//                {
//                    case CheckItemResult.库存项和出库行的计量单位不一致:
//                    case CheckItemResult.货载已分配给其他对象:
//                        {
//                            string msg = string.Format("错误，{0}。", testResult.ToString());
//                            throw new ApplicationException(msg);
//                        }
//                    default:
//                        break;
//                }
//            }

//        }

//        /// <summary>
//        /// 为出库行从 #CandidateItems 临时表读取满足 numberRequired 的记录。
//        /// 当累积数量大于等于 numberRequired 时停止读取，
//        /// 若累积数量不能满足 numberRequired，则读取所有记录。
//        /// </summary>
//        /// <param name="deliveryLineId">出库行的 Id。</param>
//        /// <param name="numberRequired">需要的数量。</param>
//        /// <returns>返回满足分配数量的记录</returns>
//        private List<CandidateItem> ReadRecords(int deliveryLineId, decimal numberRequired)
//        {
//            ISQLQuery q = _session.CreateSQLQuery(@"
//select top 1 AllocOrder
//from #CandidateItems 
//where DeliveryLineId = :deliveryLineId 
//and NumberAccumulate >= :required 
//order by AllocOrder");
//            q.SetParameter<Int32>("deliveryLineId", deliveryLineId);
//            q.SetParameter<decimal>("required", numberRequired);

//            int? allocOrder = q.UniqueResult<Int32?>();

//            if (allocOrder == null)
//            {
//                q = _session.CreateSQLQuery(@"
//select *
//from #CandidateItems 
//where DeliveryLineId = :deliveryLineId 
//order by AllocOrder");
//                q.SetParameter<Int32>("deliveryLineId", deliveryLineId);
//            }
//            else
//            {
//                q = _session.CreateSQLQuery(@"
//select *
//from #CandidateItems 
//where DeliveryLineId = :deliveryLineId 
//and AllocOrder <= :allocOrder 
//order by AllocOrder");
//                q.SetParameter<Int32>("deliveryLineId", deliveryLineId);
//                q.SetParameter<Int32>("allocOrder", allocOrder.Value);
//            }

//            q.SetResultTransformer(NHibernate.Transform.Transformers.AliasToEntityMap);
//            List<CandidateItem> list = new List<CandidateItem>();
//            var rows = q.List<Hashtable>();
//            foreach (var row in rows)
//            {
//                CandidateItem c = new CandidateItem();

//                c.DeliveryLineId = Convert.ToInt32(row["DeliveryLineId"]);
//                c.ItemId = Convert.ToInt32(row["ItemId"]);
//                c.AllocOrder = Convert.ToInt32(row["AllocOrder"]);
//                c.NumberAvailable = Convert.ToDecimal(row["NumberAvailable"]);
//                c.NumberAccumulate = Convert.ToDecimal(row["NumberAccumulate"]);

//                list.Add(c);
//            }

//            return list;
//        }

//        /// <summary>
//        /// 向 #CandidateItems 临时表填充数据，
//        /// 项目要填充的列有：DeliveryLineId, ItemId, UnitLoadId, LocationId, AllocOrder。 
//        /// 允许一个库存项出现多次，但必须分别属于不同的出库行。
//        /// </summary>
//        /// <remarks>
//        /// #CandidateItems 用于支持库存分配操作。表中的数据表示可以分配到出库行的库存项。
//        /// 表定义如下：
//        /// create table #CandidateItems(
//        ///     DeliveryLineId int not null,            -- 出库行 id。
//        ///     ItemId int not null,                    -- 库存项 id。
//        ///     AllocOrder int not null,                -- 库存项的分配次序，从 1 开始，在出库行内唯一，框架将按照这个次序进行分配。
//        ///     NumberAvailable decimal(19,5),          -- 库存项中可用的数量，这是库存量减去已分配量的值。
//        ///     NumberAccumulate decimal(19,5),         -- 按 AllocOrder 排序时， NumberAvailable 列的累加数量。
//        ///     UNIQUE(DeliveryLineId, ItemId),
//        ///     UNIQUE CLUSTERED(DeliveryLineId, AllocOrder)
//        /// )
//        /// 此临时表在分配操作中由框架创建和删除。由于框架不清楚具体的业务需求，向表中填充数据的操作必须由项目完成。
//        /// 按照库存项的位置，可以分为三类：货架外的，正在移动的，货架内的。填充临时表要考虑的因素
//        /// 项目应在指定的巷道内筛选，将可分配的库存项填充到表中。一般来说，具有以下情形的库存项不应填充到临时表中：
//        /// 1，货位已禁止出站；
//        /// 2，货位虽未禁止出站，但它是远端货位，且其近端有货并已禁止出站；
//        /// 3，货载正在移动（尾货载除外）；
//        /// 4，货载有盘点错误；
//        /// 5，货载已分配给其他对象；
//        /// 6，库存项与出库行的计量单位不同；
//        /// 
//        /// 项目应为 AllocOrder 设置合适的值，框架将按照 AllocOrder 指定的次序处理库存项。排序要考虑的因素有：
//        /// 1，先入先出，先入库的库存项排在前边；
//        /// 2，尾分配优先，尾分配是指已分配但库存量大于分配量的库存项；
//        /// 3，尾货载优先，尾货载是指含有尾分配的货载，尾货载在拣货后将有剩余，需要返库；
//        /// 4，货位次序，若为双深位巷道，应考虑双深单元的状态，每个单元内部又按照先近端后远端的次序进行；
//        /// 5，货载次序，在以上条件的前提下，按照货载id 排序可以使框架尽量从一个货载中分配，减少分配涉及的托盘数；
//        /// </remarks>
//        /// <param name="deliveryOrder"></param>
//        /// <param name="laneways"></param>
//        /// <param name="includeUnitLoads">要在分配中包含的货载，这些货载优先参与分配。</param>
//        /// <param name="excludeUnitLoads">要在分配中排除的货载，这些货载不会参与分配，即使出现在 includeUnitLoads 中。</param>
//        protected virtual void FillCandidateItemsTable(DeliveryOrder deliveryOrder, Laneway[] laneways, UnitLoad[] includeUnitLoads, UnitLoad[] excludeUnitLoads)
//        {
//            _logger.Debug("正在处理出库单 {0}，共 {1} 个出库行。", deliveryOrder.Code, deliveryOrder.Lines.Count);
//            _session.Flush();

//            var lanewayIdList = laneways?.Select(x => x.Id)?.ToArray();
//            var includeUnitLoadIdList = includeUnitLoads?.Select(x => x.Id)?.ToArray();
//            var excludeUnitLoadIdList = excludeUnitLoads?.Select(x => x.Id)?.ToArray();
//            foreach (DeliveryLine line in deliveryOrder.Lines)
//            {
//                _logger.Debug("正在处理出库行#{0}，物料是【{1}】。", line.Id, line.Material.Name);

//                if (line.NumberDelivered >= line.NumberRequired)
//                {
//                    _logger.Debug("此行已完成，跳过。");
//                    continue;
//                }

//                string[] batchList = line.BatchList.NullSafeSplit();
//                string[] qcStatusList = line.QCStatusList.NullSafeSplit();
//                int i = 0;  // 显式包含的货载在临时表中的记录数
//                int j = 0;  // 从货架上分配的货载在临时表中的记录数


//                // 1，处理显式包含的货载，这些货载具有较高的优先级
//                if (includeUnitLoadIdList != null && includeUnitLoadIdList.Length > 0)
//                {
//                    string sql = $@"
//INSERT INTO #CandidateItems(DeliveryLineId, ItemId, AllocOrder)
//SELECT :deliveryLineId AS DeliveryLineId,
//    i.Id AS ItemId,
//    ROW_NUMBER() OVER(ORDER BY i.OutOrdering) AS AllocOrder
//FROM Items i
//INNER JOIN UnitLoads u ON i.UnitLoadId = u.Id

//WHERE i.MaterialId = :materialId
//AND i.SO = :so
//AND i.SOLine = :soLine
//AND i.xNumber > 0
//AND i.Unit = :unit
//{"AND i.Batch IN (:batchList)".RetainedBy(batchList)}
//{"AND i.QCStatus IN (:qcStatusList)".RetainedBy(qcStatusList)}

//AND u.Id IN (:includeUnitLoadIdList)
//{"AND u.Id NOT IN (:excludeUnitLoadIdList)".RetainedBy(excludeUnitLoadIdList)}

//AND u.IsBeingMoved = 0
//AND u.HasCountingError = 0
//AND (u.IsAllocated = 0
//    OR
//    u.IsAllocated = 1
//    AND u.CurrentAllocationTableType = :deliveryOrderType
//    AND u.CurrentAllocationTableId = :deliveryOrderId)
//AND u.OpHint = 'None'
//";

//                    IQuery q = _session.CreateSQLQuery(sql);
//                    q.SetParameter<int>("deliveryLineId", line.Id)
//                        .SetParameter<string>("unit", line.Unit)
//                        .SetParameter<Int32>("materialId", line.Material.Id)
//                        .SetParameter<string>("so", line.SO)
//                        .SetParameter<string>("soLine", line.SOLine)
//                        .SafeSetParameterList("batchList", batchList)
//                        .SafeSetParameterList("qcStatusList", qcStatusList)
//                        .SafeSetParameterList("includeUnitLoadIdList", includeUnitLoadIdList)
//                        .SafeSetParameterList("excludeUnitLoadIdList", excludeUnitLoadIdList)
//                        .SetParameter<String>("deliveryOrderType", "Wms.DeliveryOrder")
//                        .SetParameter<Int32>("deliveryOrderId", deliveryOrder.Id);

//                    i = q.ExecuteUpdate();
//                    _logger.Debug("显式包含在临时表中产生 {0} 个记录。", i);
//                }

//                // 2，从货架上分配
//                //  includeUnitLoadIdList 已在上面处理，这里需使用 NOT IN 而非 IN
//                {
//                    string sql = $@"
//INSERT INTO #CandidateItems(DeliveryLineId, ItemId, AllocOrder)
//SELECT :deliveryLineId AS DeliveryLineId,
//    i.Id AS ItemId,
//    ROW_NUMBER() OVER(ORDER BY i.OutOrdering, locUnit.oByShape, locUnit.o1, rack.Deep) + :i AS AllocOrder
//FROM Items i
//INNER JOIN UnitLoads u ON i.UnitLoadId = u.Id
//INNER JOIN Keepings k ON u.Id = k.UnitLoadId
//INNER JOIN Locations loc ON k.LocationId = loc.Id
//INNER JOIN Racks rack ON loc.RackId = rack.Id
//INNER JOIN LocationUnits locUnit ON loc.UnitId = locUnit.Id

//WHERE i.MaterialId = :materialId
//AND i.SO = :so
//AND i.SOLine = :soLine
//AND i.xNumber > 0
//AND i.Unit = :unit
//{"AND i.Batch IN (:batchList)".RetainedBy(batchList)}
//{"AND i.QCStatus IN (:qcStatusList)".RetainedBy(qcStatusList)}
//{"AND u.Id NOT IN (:includeUnitLoadIdList)".RetainedBy(includeUnitLoadIdList)}
//{"AND u.Id NOT IN (:excludeUnitLoadIdList)".RetainedBy(excludeUnitLoadIdList)}

//AND u.IsBeingMoved = 0
//AND u.HasCountingError = 0
//AND (u.IsAllocated = 0
//    OR
//    u.IsAllocated = 1
//    AND u.CurrentAllocationTableType = :deliveryOrderType
//    AND u.CurrentAllocationTableId = :deliveryOrderId)
//AND u.OpHint = 'None'
//AND loc.xExists = 1
//{"AND rack.LanewayId IN (:lanewayIdList)".RetainedBy(lanewayIdList)}
//";

//                    IQuery q = _session.CreateSQLQuery(sql);
//                    q.SetParameter<int>("deliveryLineId", line.Id)
//                        .SetParameter<Int32>("i", i)
//                        .SetParameter<string>("unit", line.Unit)
//                        .SetParameter<Int32>("materialId", line.Material.Id)
//                        .SetParameter<string>("so", line.SO)
//                        .SetParameter<string>("soLine", line.SOLine)
//                        .SafeSetParameterList("batchList", batchList)
//                        .SafeSetParameterList("qcStatusList", qcStatusList)
//                        .SafeSetParameterList("includeUnitLoadIdList", includeUnitLoadIdList)
//                        .SafeSetParameterList("excludeUnitLoadIdList", excludeUnitLoadIdList)
//                        .SetParameter<String>("deliveryOrderType", "Wms.DeliveryOrder")
//                        .SetParameter<Int32>("deliveryOrderId", deliveryOrder.Id)
//                        .SafeSetParameterList("lanewayIdList", lanewayIdList);

//                    j = q.ExecuteUpdate();
//                    _logger.Debug("普通库内分配在临时表中产生 {0} 个记录。", j);
//                }
//            }
//        }

//        /// <summary>
//        /// 覆盖 FillCandidateItemsTable 方法。
//        /// </summary>
//        /// <param name="deliveryOrder">出库单</param>
//        /// <param name="laneways">进行分配的巷道</param>
//        /// <param name="includeUnitLoads">指定要排除的货载</param>
//        /// <param name="excludeUnitLoads">指定要包含的货载</param>
//        /// <param name="ovr">是否已重写</param>
//        partial void FillCandidateItemsTableOvr(DeliveryOrder deliveryOrder, Laneway[] laneways, UnitLoad[] includeUnitLoads, UnitLoad[] excludeUnitLoads, ref bool ovr);

//        /// <summary>
//        /// 刷新临时表中的数量列，即 NumberAvailable 和 NumberAccumulate 两个列。
//        /// 框架在为每个出库行分配库存之前调用此方法。
//        /// </summary>
//        /// <param name="deliveryLine">出库单</param>
//        protected virtual void RefreshNumberColumns(DeliveryLine deliveryLine)
//        {
//            const string sql = @"
//MERGE #CandidateItems AS c
//USING Items AS i
//ON (c.ItemId = i.Id)
//WHEN MATCHED AND c.DeliveryLineId = :deliveryLineId
//  THEN UPDATE SET c.NumberAvailable = i.xNumber - i.NumberAllocated;
      
//-- 为 accumulate 列赋值
//DECLARE @accumulate DECIMAL(20,8);
//SET @accumulate = 0;

//DECLARE @numberAvailable DECIMAL(20,8);

//DECLARE cur CURSOR FOR
//SELECT NumberAvailable 
//FROM #CandidateItems 
//WHERE DeliveryLineId = :deliveryLineId
//ORDER BY AllocOrder
//FOR UPDATE OF NumberAccumulate;

//OPEN cur;
//FETCH NEXT FROM cur INTO @numberAvailable;
//WHILE @@FETCH_STATUS = 0
//BEGIN
//   set @accumulate = @accumulate + @numberAvailable;
//   update #CandidateItems set NumberAccumulate = @accumulate where CURRENT of cur;
//   FETCH NEXT FROM cur INTO @numberAvailable;
//END

//CLOSE cur;
//DEALLOCATE cur;
//";
//            var q = _session.CreateSQLQuery(sql)
//                .SetParameter<Int32>("deliveryLineId", deliveryLine.Id);
//            q.ExecuteUpdate();
//        }

//        /// <summary>
//        /// 测试出库行是否可从库存项中进行分配，如果可以分配，输出应取的数量。此方法没有副作用，不会更改出库行和库存项的状态。
//        /// </summary>
//        /// <param name="line">出库单</param>
//        /// <param name="item">库存项</param>
//        /// <param name="shouldTake">还应从库存项中分配的数</param>
//        /// <returns></returns>
//        internal protected CheckItemResult CheckItem(DeliveryLine line, Item item, out decimal shouldTake)
//        {
//            if (line.ComputeNumberShort() <= 0m)
//            {
//                shouldTake = 0;
//                return CheckItemResult.出库行没有欠数;
//            }

//            if (item.UnitLoad.IsAllocated && item.UnitLoad.CurrentAllocationTable != line.DeliveryOrder)
//            {
//                shouldTake = 0;
//                return CheckItemResult.货载已分配给其他对象;
//            }

//            if (item.UnitLoad.HasCountingError)
//            {
//                shouldTake = 0;
//                return CheckItemResult.货载有盘点错误;
//            }

//            if (string.Equals(item.Unit, line.Unit, StringComparison.InvariantCultureIgnoreCase) == false)
//            {
//                shouldTake = 0;
//                return CheckItemResult.库存项和出库行的计量单位不一致;
//            }

//            var available = item.Number - item.NumberAllocated;
//            shouldTake = Math.Min(available, line.ComputeNumberShort());
//            return CheckItemResult.成功;

//        }

//        /// <summary>
//        /// 为出库行从指定的库存项进行分配。此方法具有副作用，会更改库存项以及货载的分配信息。
//        /// </summary>
//        /// <param name="line">出库行</param>
//        /// <param name="item">要从中分配的库存项。</param>
//        /// <param name="take">输出从库存项中分配的数量。</param>
//        /// <returns></returns>
//        protected CheckItemResult AllocateItem(DeliveryLine line, Item item, out decimal take)
//        {
//            var testResult = CheckItem(line, item, out take);
//            if (testResult == CheckItemResult.成功)
//            {
//                // 更新库存项的分配信息
//                // 注意，如果库存项是尾分配，则库存项的原分配数量不为 0，因此，分配数量要增加
//                ItemAllocation allocInfo = item.AllocateTo(line, take);

//                UnitLoad unitLoad = item.UnitLoad;
//                unitLoad.SetAllocationTable(line.DeliveryOrder);
//                _unitLoadRepository.Update(unitLoad);

//                // 更新 wrapper 中的 shortage
//                if (line.ComputeNumberShort() < 0)
//                {
//                    string msg = string.Format("DeliveryLine.NumberShort 小于 0 说明存在 bug。");
//                    throw new ApplicationException(msg);
//                }

//                _logger.Info($"出库行#{line.Id}，从库存项#{item.Id} 中分配或追加了 {take.ToString("0.#")}，它属于货载#{item.UnitLoad.Id}，容器编码是 {item.UnitLoad.ContainerCode}。");
//            }

//            return testResult;
//        }
//        /// <summary>
//        /// 解除指定出库单在货载上分配信息，仅限货架上的货载；货架外的货载使用DeallocateUnitLoad函数
//        /// </summary>
//        /// <param name="deliveryOrder">出库单</param>
//        public void CancelUnitLoadsOnRack(DeliveryOrder deliveryOrder)
//        {
//            if (deliveryOrder == null)
//            {
//                throw new ArgumentNullException(nameof(deliveryOrder));
//            }

//            _logger.Debug("正在取消出库单 {0} 在货架上的分配。", deliveryOrder.Code);
//            var unitLoadsOnRack = _unitLoadRepository.GetUnitLoadsOnRackAllocatedTo(deliveryOrder).ToArray();
//            foreach (var u in unitLoadsOnRack)
//            {
//                DeallocateUnitLoad(deliveryOrder, u);
//            }
//            _session.Flush();
//            _logger.Debug("已取消出库单 {0} 在货架上的分配，共 {1} 个货载。", deliveryOrder.Code, unitLoadsOnRack.Length);
//        }

//        /// <summary>
//        /// 解除指定出库单在库存项上分配的信息
//        /// </summary>
//        /// <param name="deliveryOrder">出库单</param>
//        /// <param name="item">库存项</param>
//        protected void DeallocateItem(DeliveryOrder deliveryOrder, Item item)
//        {
//            if (deliveryOrder == null)
//            {
//                throw new ArgumentNullException(nameof(deliveryOrder));
//            }
//            if (item == null)
//            {
//                throw new ArgumentNullException(nameof(item));
//            }

//            _logger.Debug("正在从出库单中解除分配库存项。");

//            var unitLoad = item.UnitLoad;
//            if (unitLoad != null && deliveryOrder.Contains(item.UnitLoad) == false)
//            {
//                string msg = string.Format("解除分配库存项失败。库存项所在的货载未分配给此出库单。");
//                throw new InvalidOperationException(msg);
//            }

//            item.ResetAllocations();
//            _logger.Debug("已重置库存项的分配信息。");

//            if (unitLoad != null)
//            {
//                if (unitLoad.Items.All(x => x.IsAllocated == false))
//                {
//                    unitLoad.ResetAllocationTable();
//                    _logger.Debug("库存项所在的容器编码是 {0}，货载中的全部库存项均为未分配状态，已重置货载的分配表。", unitLoad.ContainerCode);
//                }
//            }
//            _logger.Debug("从出库单解除分配库存项成功。");
//        }

//        /// <summary>
//        /// 解除指定出库单在指定货载上分配信息
//        /// </summary>
//        /// <param name="deliveryOrder">出库单</param>
//        /// <param name="unitLoad">货载</param>
//        public void DeallocateUnitLoad(DeliveryOrder deliveryOrder, UnitLoad unitLoad)
//        {
//            if (deliveryOrder == null)
//            {
//                throw new ArgumentNullException(nameof(deliveryOrder));
//            }
//            if (unitLoad == null)
//            {
//                throw new ArgumentNullException(nameof(unitLoad));
//            }

//            _logger.Debug("正在从出库单中解除分配货载。");

//            if (deliveryOrder.Contains(unitLoad) == false)
//            {
//                string msg = string.Format("解除分配货载失败。货载未分配给此出库单。");
//                throw new InvalidOperationException(msg);
//            }

//            foreach (var i in unitLoad.Items)
//            {
//                if (i.IsAllocated)
//                {
//                    DeallocateItem(deliveryOrder, i);
//                }
//            }

//            _logger.Debug("从出库单解除分配货载成功。");
//        }


//        /// <summary>
//        /// 用于支持库存分配的数据结构。
//        /// </summary>
//        public class CandidateItem
//        {
//            /// <summary>
//            /// 出库行
//            /// </summary>
//            public int DeliveryLineId { get; set; }
//            /// <summary>
//            /// 库存项
//            /// </summary>
//            public int ItemId { get; set; }
//            /// <summary>
//            /// 分配次序
//            /// </summary>
//            public int AllocOrder { get; set; }
//            /// <summary>
//            /// 库存项中可用的数量
//            /// </summary>
//            public decimal NumberAvailable { get; set; }
//            /// <summary>
//            /// 按 AllocOrder 排序时， NumberAvailable 列的累加数量。
//            /// </summary>
//            public decimal NumberAccumulate { get; set; }
//        }


//        /// <summary>
//        /// 表示分配测试结果。
//        /// </summary>
//        public enum CheckItemResult
//        {
//            /// <summary>
//            /// 成功
//            /// </summary>
//            成功,

//            /// <summary>
//            /// 出库行没有欠数
//            /// </summary>
//            出库行没有欠数,

//            /// <summary>
//            /// 库存项和出库行的计量单位不一致
//            /// </summary>
//            库存项和出库行的计量单位不一致,

//            /// <summary>
//            /// 货载已分配给其他对象
//            /// </summary>
//            货载已分配给其他对象,

//            /// <summary>
//            /// 货载有盘点错误
//            /// </summary>
//            货载有盘点错误,
//        }
//    }


}