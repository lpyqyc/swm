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

using Swm.Locations;
using Swm.Model;
using Swm.OutboundOrders;
using Swm.Palletization;
using Swm.TransportTasks;
using System;
using System.Data;
using System.Linq;

namespace Swm.Web.Controllers
{
    /// <summary>
    /// 将业务模型对象转换为 DTO。
    /// </summary>
    public static class DtoConvert
    {
        /// <summary>
        /// 将 <see cref="UnitloadItem"/> 转换为 <see cref="UnitloadItemInfo"/>
        /// </summary>
        /// <param name="unitloadItem"></param>
        /// <returns></returns>
        public static UnitloadItemInfo ToUnitloadItemInfo(UnitloadItem unitloadItem)
        {
            OutboundOrder? obo = unitloadItem.Unitload?.CurrentUat as OutboundOrder;

            return new UnitloadItemInfo
            {
                UnitloadItemId = unitloadItem.UnitloadItemId,
                MaterialId = unitloadItem.Material?.MaterialId ?? 0,
                MaterialCode = unitloadItem.Material?.MaterialCode,
                MaterialType = unitloadItem.Material?.MaterialType,
                Description = unitloadItem.Material?.Description,
                Specification = unitloadItem.Material?.Specification,
                Batch = unitloadItem.Batch,
                StockStatus = unitloadItem.StockStatus,
                Quantity = unitloadItem.Quantity,
                AllocationsToOutboundOrder = unitloadItem.Allocations
                    .Where(x => x.OutboundDemand != null && x.OutboundDemand is OutboundLine)
                    .Select(x => new UnitloadItemInfo.AllocationInfoToOutboundOrder
                    {
                        UnitloadItemAllocationId = x.UnitloadItemAllocationId,
                        OutboundLineId = ((OutboundLine)x.OutboundDemand!).OutboundLineId,
                        QuantityAllocated = x.QuantityAllocated,
                    })
                    .ToArray(),
                Uom = unitloadItem.Uom,
            };
        }


        /// <summary>
        /// 将 <see cref="UnitloadItemSnapshot"/> 转换为 <see cref="UnitloadItemInfo"/>
        /// </summary>
        /// <param name="unitloadItem"></param>
        /// <returns></returns>
        public static UnitloadItemInfo ToUnitloadItemInfo(UnitloadItemSnapshot unitloadItem)
        {

            return new UnitloadItemInfo
            {
                UnitloadItemId = unitloadItem.UnitloadItemId,
                MaterialId = unitloadItem.Material?.MaterialId ?? 0,
                MaterialCode = unitloadItem.Material?.MaterialCode,
                MaterialType = unitloadItem.Material?.MaterialType,
                Description = unitloadItem.Material?.Description,
                Specification = unitloadItem.Material?.Specification,
                Batch = unitloadItem.Batch,
                StockStatus = unitloadItem.StockStatus,
                Quantity = unitloadItem.Quantity,
                Uom = unitloadItem.Uom,
            };
        }

        /// <summary>
        /// 将 <see cref="Unitload"/> 转换为 <see cref="UnitloadDetail"/>
        /// </summary>
        /// <param name="unitload"></param>
        /// <returns></returns>
        public static UnitloadDetail ToUnitloadDetail(Unitload unitload)
        {
            var task = unitload.CurrentTask as TransportTask;
            OutboundOrder? obo = unitload.CurrentUat as OutboundOrder;

            return new UnitloadDetail
            {
                UnitloadId = unitload.UnitloadId,
                PalletCode = unitload.PalletCode,
                ctime = unitload.ctime,
                LocationCode = unitload.CurrentLocation?.LocationCode,
                LocationType = unitload.CurrentLocation?.LocationType,
                LocationTime = unitload.CurrentLocationTime,
                LanewayCode = unitload.CurrentLocation?.Laneway?.LanewayCode,
                BeingMoved = unitload.BeingMoved,
                Items = unitload.Items.Select(i => ToUnitloadItemInfo(i)).ToList(),
                Comment = unitload.Comment,

                CurrentTaskCode = task?.TaskCode,
                CurrentTaskType = task?.TaskType,
                CurrentTaskStartLocationCode = task?.Start?.LocationCode,
                CurrentTaskEndLocationCode = task?.End?.LocationCode,

                CurrentUat = unitload.CurrentUat?.ToString(),
                OpHintInfo = unitload.OpHintInfo,
                OpHintType = unitload.OpHintType,

                Weight = unitload.StorageInfo.Weight,
                Height = unitload.StorageInfo.Height,
                StorageGroup = unitload.StorageInfo.StorageGroup,
                OutFlag = unitload.StorageInfo.OutFlag,
                ContainerSpecification = unitload.StorageInfo.ContainerSpecification,

            };
        }

        /// <summary>
        /// 将 <see cref="Unitload"/> 转换为 <see cref="UnitloadInfo"/>
        /// </summary>
        /// <param name="unitload"></param>
        /// <returns></returns>
        public static UnitloadInfo ToUnitloadInfo(Unitload unitload)
        {
            return  new UnitloadInfo
            {
                UnitloadId = unitload.UnitloadId,
                PalletCode = unitload.PalletCode,
                ctime = unitload.ctime,
                mtime = unitload.mtime,
                LocationCode = unitload.CurrentLocation?.LocationCode,
                LocationType = unitload.CurrentLocation?.LocationType,
                LanewayCode = unitload.CurrentLocation?.Laneway?.LanewayCode,
                BeingMoved = unitload.BeingMoved,
                Items = unitload.Items.Select(i => ToUnitloadItemInfo(i)).ToList(),
                Allocated = (unitload.CurrentUat != null),
                Comment = unitload.Comment
            };

        }

    }


}
