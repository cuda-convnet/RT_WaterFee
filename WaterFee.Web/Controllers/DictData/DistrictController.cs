﻿using Aspose.Cells;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Web.Mvc;
using WHC.Dictionary.BLL;
using WHC.Dictionary.Entity;
using WHC.Framework.Commons;
using WHC.Framework.ControlUtil;

namespace WHC.MVCWebMis.Controllers
{
    public class DistrictController : BusinessController<District, DistrictInfo>
    {
        public DistrictController() : base()
        {
        }

        #region 导入Excel数据操作

        //导入或导出的字段列表   
        string columnString = "行政区名称,城市ID";

        /// <summary>
        /// 检查Excel文件的字段是否包含了必须的字段
        /// </summary>
        /// <param name="guid">附件的GUID</param>
        /// <returns></returns>
        public ActionResult CheckExcelColumns(string guid)
        {
            CommonResult result = new CommonResult();

            try
            {
                DataTable dt = ConvertExcelFileToTable(guid);
                if (dt != null)
                {
                    //检查列表是否包含必须的字段
                    result.Success = DataTableHelper.ContainAllColumns(dt, columnString);
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex);
                result.ErrorMessage = ex.Message;
            }

            return ToJsonContent(result);
        }

        /// <summary>
        /// 获取服务器上的Excel文件，并把它转换为实体列表返回给客户端
        /// </summary>
        /// <param name="guid">附件的GUID</param>
        /// <returns></returns>
        public ActionResult GetExcelData(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            List<DistrictInfo> list = new List<DistrictInfo>();

            DataTable table = ConvertExcelFileToTable(guid);
            if (table != null)
            {
                #region 数据转换
                int i = 1;
                foreach (DataRow dr in table.Rows)
                {
                    bool converted = false;
                    DateTime dtDefault = Convert.ToDateTime("1900-01-01");
                    DateTime dt;
                    DistrictInfo info = new DistrictInfo();

                    info.DistrictName = dr["行政区名称"].ToString();
                    info.CityID = dr["城市ID"].ToString().ToInt32();

                    //info.Creator = CurrentUser.ID.ToString();
                    //info.CreateTime = DateTime.Now;
                    //info.Editor = CurrentUser.ID.ToString();
                    //info.EditTime = DateTime.Now;

                    list.Add(info);
                }
                #endregion
            }

            var result = new { total = list.Count, rows = list };
            return ToJsonContentDate(result);
        }

        /// <summary>
        /// 保存客户端上传的相关数据列表
        /// </summary>
        /// <param name="list">数据列表</param>
        /// <returns></returns>
        public ActionResult SaveExcelData(List<DistrictInfo> list)
        {
            CommonResult result = new CommonResult();
            if (list != null && list.Count > 0)
            {
                #region 采用事务进行数据提交

                DbTransaction trans = BLLFactory<District>.Instance.CreateTransaction();
                if (trans != null)
                {
                    try
                    {
                        //int seq = 1;
                        foreach (DistrictInfo detail in list)
                        {
                            //detail.Seq = seq++;//增加1
                            //detail.CreateTime = DateTime.Now;
                            //detail.Creator = CurrentUser.ID.ToString();
                            //detail.Editor = CurrentUser.ID.ToString();
                            //detail.EditTime = DateTime.Now;

                            BLLFactory<District>.Instance.Insert(detail, trans);
                        }
                        trans.Commit();
                        result.Success = true;
                    }
                    catch (Exception ex)
                    {
                        LogTextHelper.Error(ex);
                        result.ErrorMessage = ex.Message;
                        trans.Rollback();
                    }
                }
                #endregion
            }
            else
            {
                result.ErrorMessage = "导入信息不能为空";
            }

            return ToJsonContent(result);
        }

        /// <summary>
        /// 根据查询条件导出列表数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Export()
        {
            #region 根据参数获取List列表
            string where = GetPagerCondition();
            string CustomedCondition = Request["CustomedCondition"] ?? "";
            List<DistrictInfo> list = new List<DistrictInfo>();

            if (!string.IsNullOrWhiteSpace(CustomedCondition))
            {
                //如果为自定义的json参数列表，那么可以使用字典反序列化获取参数，然后处理
                //Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(CustomedCondition);

                //如果是条件的自定义，可以使用Find查找
                list = baseBLL.Find(CustomedCondition);
            }
            else
            {
                list = baseBLL.Find(where);
            }

            #endregion

            #region 把列表转换为DataTable
            DataTable datatable = DataTableHelper.CreateTable("序号|int," + columnString);
            DataRow dr;
            int j = 1;
            for (int i = 0; i < list.Count; i++)
            {
                dr = datatable.NewRow();
                dr["序号"] = j++;
                dr["行政区名称"] = list[i].DistrictName;
                dr["城市ID"] = list[i].CityID;
                //如果为外键，可以在这里进行转义，如下例子
                //dr["客户名称"] = BLLFactory<Customer>.Instance.GetCustomerName(list[i].Customer_ID);//转义为客户名称

                datatable.Rows.Add(dr);
            }
            #endregion

            #region 把DataTable转换为Excel并输出
            Aspose.Cells.Workbook workbook = new Aspose.Cells.Workbook();
            //为单元格添加样式    
            Aspose.Cells.Style style = workbook.Styles[workbook.Styles.Add()];
            //设置居中
            style.HorizontalAlignment = Aspose.Cells.TextAlignmentType.Center;
            //设置背景颜色
            style.ForegroundColor = System.Drawing.Color.FromArgb(153, 204, 0);
            style.Pattern = BackgroundType.Solid;
            style.Font.IsBold = true;

            int rowIndex = 0;
            for (int i = 0; i < datatable.Columns.Count; i++)
            {
                DataColumn col = datatable.Columns[i];
                string columnName = col.Caption ?? col.ColumnName;
                workbook.Worksheets[0].Cells[rowIndex, i].PutValue(columnName);
                workbook.Worksheets[0].Cells[rowIndex, i].SetStyle(style);
            }
            rowIndex++;

            foreach (DataRow row in datatable.Rows)
            {
                for (int i = 0; i < datatable.Columns.Count; i++)
                {
                    workbook.Worksheets[0].Cells[rowIndex, i].PutValue(row[i].ToString());
                }
                rowIndex++;
            }

            for (int k = 0; k < datatable.Columns.Count; k++)
            {
                workbook.Worksheets[0].AutoFitColumn(k, 0, 150);
            }
            workbook.Worksheets[0].FreezePanes(1, 0, 1, datatable.Columns.Count);

            //根据用户创建目录，确保生成的文件不会产生冲突
            string filePath = string.Format("/GenerateFiles/{0}/District.xls", CurrentUser.Name);
            string realPath = Server.MapPath(filePath);
            string parentPath = Directory.GetParent(realPath).FullName;
            DirectoryUtil.AssertDirExist(parentPath);

            workbook.Save(realPath, Aspose.Cells.SaveFormat.Excel97To2003);

            #endregion

            //返回生成后的文件路径，让客户端根据地址下载
            return Content(filePath);
        }

        #endregion


        /// <summary>
        /// 根据城市名称获取对应的行政区划类别
        /// </summary>
        /// <param name="cityName">城市名称</param>
        /// <returns></returns>
        public ActionResult GetDistrictByCityName(string cityName)
        {
            List<EasyTreeData> treeList = new List<EasyTreeData>();
            EasyTreeData pNode = new EasyTreeData("", "选择记录", "icon-computer");
            treeList.Add(pNode);

            string condition = string.Format("");
            List<DistrictInfo> districtList = BLLFactory<District>.Instance.GetDistrictByCityName(cityName);
            foreach (DistrictInfo info in districtList)
            {
                EasyTreeData item = new EasyTreeData(info.ID, info.DistrictName, "icon-view");
                pNode.children.Add(item);
            }

            return ToJsonContent(treeList);
        }

        /// <summary>
        /// 根据城市ID获取对应的行政区划类别
        /// </summary>
        /// <param name="cityName">城市名称</param>
        /// <returns></returns>
        public ActionResult GetDistrictByCity(string cityId)
        {
            List<EasyTreeData> treeList = new List<EasyTreeData>();
            EasyTreeData pNode = new EasyTreeData("", "选择记录", "icon-computer");
            treeList.Add(pNode);

            string condition = string.Format("");
            List<DistrictInfo> districtList = BLLFactory<District>.Instance.GetDistrictByCity(cityId);
            foreach (DistrictInfo info in districtList)
            {
                EasyTreeData item = new EasyTreeData(info.ID, info.DistrictName, "icon-view");
                pNode.children.Add(item);
            }

            return ToJsonContent(treeList);
        }
    }
}
