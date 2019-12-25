using Aspose.Cells;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Web.Mvc;
using WHC.ContactBook.BLL;
using WHC.ContactBook.Entity;
using WHC.Framework.Commons;
using WHC.Framework.ControlUtil;
using WHC.Pager.Entity;

namespace WHC.MVCWebMis.Controllers
{
    public class AddressController : BusinessController<Address, AddressInfo>
    {
        public AddressController()
            : base()
        {
        }

        #region 导入Excel数据操作

        //导入或导出的字段列表   
        string columnString = "通讯录类型,姓名,性别,出生日期,手机,电子邮箱,QQ,家庭电话,办公电话,家庭住址,办公地址,传真号码,公司单位,部门,其他,备注,创建人,创建时间";

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

            List<AddressInfo> list = new List<AddressInfo>();

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
                    AddressInfo info = new AddressInfo();

                    info.AddressType = EnumHelper.GetInstance<AddressType>(dr["通讯录类型"].ToString());
                    info.Name = dr["姓名"].ToString();
                    info.Sex = dr["性别"].ToString();
                    converted = DateTime.TryParse(dr["出生日期"].ToString(), out dt);
                    if (converted && dt > dtDefault)
                    {
                        info.Birthdate = dt;
                    }
                    info.Mobile = dr["手机"].ToString();
                    info.Email = dr["电子邮箱"].ToString();
                    info.QQ = dr["QQ"].ToString();
                    info.HomeTelephone = dr["家庭电话"].ToString();
                    info.OfficeTelephone = dr["办公电话"].ToString();
                    info.HomeAddress = dr["家庭住址"].ToString();
                    info.OfficeAddress = dr["办公地址"].ToString();
                    info.Fax = dr["传真号码"].ToString();
                    info.Company = dr["公司单位"].ToString();
                    info.Dept = dr["部门"].ToString();
                    info.Other = dr["其他"].ToString();
                    info.Note = dr["备注"].ToString();
                    info.Creator = dr["创建人"].ToString();
                    converted = DateTime.TryParse(dr["创建时间"].ToString(), out dt);
                    if (converted && dt > dtDefault)
                    {
                        info.CreateTime = dt;
                    }
                    info.Dept_ID = dr["所属部门"].ToString();
                    info.Company_ID = dr["所属公司"].ToString();

                    info.Creator = Session["UserID"].ToString();
                    info.CreateTime = DateTime.Now;
                    info.Editor = Session["UserID"].ToString();
                    info.EditTime = DateTime.Now;

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
        public ActionResult SaveExcelData(List<AddressInfo> list)
        {
            CommonResult result = new CommonResult();
            if (list != null && list.Count > 0)
            {
                #region 采用事务进行数据提交

                DbTransaction trans = BLLFactory<Address>.Instance.CreateTransaction();
                if (trans != null)
                {
                    try
                    {
                        //int seq = 1;
                        foreach (AddressInfo detail in list)
                        {
                            //detail.Seq = seq++;//增加1
                            detail.CreateTime = DateTime.Now;
                            detail.Creator = CurrentUser.ID.ToString();
                            detail.Editor = CurrentUser.ID.ToString();
                            detail.EditTime = DateTime.Now;

                            BLLFactory<Address>.Instance.Insert(detail, trans);
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
            List<AddressInfo> list = new List<AddressInfo>();

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
                dr["通讯录类型[个人,公司]"] = list[i].AddressType;
                dr["姓名"] = list[i].Name;
                dr["性别"] = list[i].Sex;
                dr["出生日期"] = list[i].Birthdate;
                dr["手机"] = list[i].Mobile;
                dr["电子邮箱"] = list[i].Email;
                dr["QQ"] = list[i].QQ;
                dr["家庭电话"] = list[i].HomeTelephone;
                dr["办公电话"] = list[i].OfficeTelephone;
                dr["家庭住址"] = list[i].HomeAddress;
                dr["办公地址"] = list[i].OfficeAddress;
                dr["传真号码"] = list[i].Fax;
                dr["公司单位"] = list[i].Company;
                dr["部门"] = list[i].Dept;
                dr["其他"] = list[i].Other;
                dr["备注"] = list[i].Note;
                dr["创建人"] = list[i].Creator;
                dr["创建时间"] = list[i].CreateTime;
                dr["所属部门"] = list[i].Dept_ID;
                dr["所属公司"] = list[i].Company_ID;
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
            string filePath = string.Format("/GenerateFiles/{0}/Address.xls", CurrentUser.Name);
            string realPath = Server.MapPath(filePath);
            string parentPath = Directory.GetParent(realPath).FullName;
            DirectoryUtil.AssertDirExist(parentPath);

            workbook.Save(realPath, Aspose.Cells.SaveFormat.Excel97To2003);

            #endregion

            //返回生成后的文件路径，让客户端根据地址下载
            return Content(filePath);
        }

        #endregion

        #region 写入数据前修改部分属性
        protected override void OnBeforeInsert(AddressInfo info)
        {
            //留给子类对参数对象进行修改
            info.CreateTime = DateTime.Now;
            info.Creator = CurrentUser.ID.ToString();
            info.Company_ID = CurrentUser.Company_ID;
            info.Dept_ID = CurrentUser.Dept_ID;
        }

        protected override void OnBeforeUpdate(AddressInfo info)
        {
            //留给子类对参数对象进行修改
            info.Editor = CurrentUser.ID.ToString();
            info.EditTime = DateTime.Now;
        }
        #endregion

        /// <summary>
        /// 根据分组获取通讯录人员信息
        /// </summary>
        /// <param name="addressType"></param>
        /// <returns></returns>
        public ActionResult FindByAddressGroup(string addressType)
        {
            //检查用户是否有权限，否则抛出MyDenyAccessException异常
            base.CheckAuthorized(AuthorizeKey.ListKey);

            List<AddressInfo> list = new List<AddressInfo>();
            AddressType type = (addressType == "public") ? AddressType.公共 : AddressType.个人;

            PagerInfo pagerInfo = GetPagerInfo();
            //增加一个CustomedCondition条件，根据客户这个条件进行查询
            string CustomedCondition = Request["CustomedCondition"] ?? "";
            if (!string.IsNullOrWhiteSpace(CustomedCondition))
            {
                #region 自定义条件查询
                if (type == AddressType.公共)
                {
                    if (CustomedCondition == "all")
                    {
                        list = BLLFactory<Address>.Instance.GetAllByAddressType(AddressType.公共);
                    }
                    else if (CustomedCondition == "ungroup")
                    {
                        list = BLLFactory<Address>.Instance.FindByGroupNamePublic(null, pagerInfo);
                    }
                    else
                    {
                        string groupName = CustomedCondition;
                        list = BLLFactory<Address>.Instance.FindByGroupNamePublic(groupName, pagerInfo);
                    }
                }
                else
                {
                    if (CustomedCondition == "all")
                    {
                        list = BLLFactory<Address>.Instance.GetAllByAddressType(AddressType.个人);
                    }
                    else if (CustomedCondition == "ungroup")
                    {
                        list = BLLFactory<Address>.Instance.FindByGroupName(CurrentUser.ID.ToString(), null, pagerInfo);
                    }
                    else
                    {
                        string groupName = CustomedCondition;
                        list = BLLFactory<Address>.Instance.FindByGroupName(CurrentUser.ID.ToString(), groupName, pagerInfo);
                    }
                }
                #endregion
            }
            else
            {
                string where = GetPagerCondition();
                list = BLLFactory<Address>.Instance.FindWithPager(where, pagerInfo);
            }

            //Json格式的要求{total:22,rows:{}}
            //构造成Json的格式传递
            var result = new { total = pagerInfo.RecordCount, rows = list };
            return ToJsonContentDate(result);
        }

        public ActionResult ModifyAddressGroup(string id, string groupIdList)
        {
            List<string> idList = new List<string>();
            if (!string.IsNullOrEmpty(groupIdList))
            {
                idList = groupIdList.ToDelimitedList<string>(",");
            }

            CommonResult result = new CommonResult();
            try
            {
                result.Success = BLLFactory<Address>.Instance.ModifyAddressGroup(id, idList);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex);
                result.ErrorMessage = ex.Message;
            }
            return ToJsonContent(result);
        }
    }
}
