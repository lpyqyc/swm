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

using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Data;
using System.IO;

namespace Swm.Web
{
    /// <summary>
    /// 使用 NPOI 读取 Excel 数据
    /// </summary>
    public static class ExcelUtil
    {
        /// <summary>
        /// 读取 xlsx 文件，转换为 DataSet
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static DataSet ReadDataSet(string filePath)
        {
            DataSet dataSet = new DataSet(Path.GetFileNameWithoutExtension(filePath));

            XSSFWorkbook xssfWorkbook;

            using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                xssfWorkbook = new XSSFWorkbook(file);
            }

            for (int n = 0; n < xssfWorkbook.NumberOfSheets; n++)
            {
                NPOI.SS.UserModel.ISheet sheet = xssfWorkbook.GetSheetAt(n);
                var table = ToTable(sheet);
                dataSet.Tables.Add(table);
            }

            return dataSet;
        }

        /// <summary>
        /// 读取 xlsx 文件，转换为 DataSet
        /// </summary>
        /// <param name="filePath">工作簿文件路径</param>
        /// <param name="sheetName">工作表名</param>
        /// <returns></returns>
        public static DataTable? ReadDataTable(string filePath, string sheetName)
        {
            XSSFWorkbook xssfWorkbook;

            using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                xssfWorkbook = new XSSFWorkbook(file);
            }

            for (int n = 0; n < xssfWorkbook.NumberOfSheets; n++)
            {
                if (string.Equals(xssfWorkbook.GetSheetName(n), sheetName, StringComparison.OrdinalIgnoreCase) == false)
                {
                    continue;
                }

                NPOI.SS.UserModel.ISheet sheet = xssfWorkbook.GetSheetAt(n);
                return ToTable(sheet);
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sheet"></param>
        /// <returns></returns>
        private static DataTable ToTable(ISheet sheet)
        {
            DataTable table = new DataTable(sheet.SheetName);

            IRow headerRow = sheet.GetRow(0);       //第一行为标题行
            int cellCount = headerRow.LastCellNum;  //LastCellNum = PhysicalNumberOfCells
            int rowCount = sheet.LastRowNum;        //LastRowNum = PhysicalNumberOfRows - 1

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
