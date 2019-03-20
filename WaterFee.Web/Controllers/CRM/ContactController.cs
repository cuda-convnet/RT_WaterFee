using System;
using System.IO;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using Aspose.Cells;
using WHC.Pager.Entity;
using WHC.Framework.Commons;
using WHC.Framework.ControlUtil;
using WHC.MVCWebMis.BLL;
using WHC.MVCWebMis.Entity;
using WHC.CRM.BLL;
using WHC.CRM.Entity;

namespace WHC.MVCWebMis.Controllers
{
    public class ContactController : BusinessController<Contact, ContactInfo>
    {
        public ContactController() : base()
        {
        }

        #region д������ǰ�޸Ĳ�������
        protected override void OnBeforeInsert(ContactInfo info)
        {
            //��������Բ�����������޸�
            info.CreateTime = DateTime.Now;
            info.Creator = CurrentUser.ID.ToString();
            info.Company_ID = CurrentUser.Company_ID;
            info.Dept_ID = CurrentUser.Dept_ID;
        }

        protected override void OnBeforeUpdate(ContactInfo info)
        {
            //��������Բ�����������޸�
            info.Editor = CurrentUser.ID.ToString();
            info.EditTime = DateTime.Now;
        }
        #endregion

        public override ActionResult FindWithPager()
        {
            //����û��Ƿ���Ȩ�ޣ������׳�MyDenyAccessException�쳣
            base.CheckAuthorized(AuthorizeKey.ListKey);

            string where = GetPagerCondition();
            PagerInfo pagerInfo = GetPagerInfo();
            List<ContactInfo> list = new List<ContactInfo>();

            //����Զ����ѯ�����ǿգ���ô�����Զ����ѯ����
            string CustomedCondition = Request["CustomedCondition"] ?? "";
            if (!string.IsNullOrWhiteSpace(CustomedCondition))
            {
                Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(CustomedCondition);
                if (dict != null)
                {
                    string id = dict["id"];
                    string groupname = dict["groupname"];
                    string userid = dict["userid"];

                    if (string.IsNullOrEmpty(id))
                    {
                        if (groupname == "������ϵ��")
                        {
                            where = "";//ֱ��ʹ�ÿ�����
                            list = baseBLL.FindWithPager(where, pagerInfo);
                        }
                        else if (groupname == "δ������ϵ��")
                        {
                            list = BLLFactory<Contact>.Instance.FindByGroupName(userid, null, pagerInfo);
                        }
                    }
                    else
                    {
                        list = BLLFactory<Contact>.Instance.FindByGroupName(userid, groupname, pagerInfo);
                    }
                }
            }
            else
            {
                list = baseBLL.FindWithPager(where, pagerInfo);
            }

            foreach (ContactInfo info in list)
            {
                //����һ�������ֶε�ת��
                info.Data1 = BLLFactory<Customer>.Instance.GetCustomerName(info.Customer_ID);
            }

            //Json��ʽ��Ҫ��{total:22,rows:{}}
            //�����Json�ĸ�ʽ����
            var result = new { total = pagerInfo.RecordCount, rows = list };
            return ToJsonContentDate(result);
        }

        /// <summary>
        /// ������ϵ�˵����
        /// </summary>
        /// <param name="contactId">��ϵ��ID</param>
        /// <param name="groupIdList">����Id����</param>
        /// <returns></returns>
        public ActionResult ModifyContactGroup(string contactId, string groupIdList)
        {
            List<string> idList = new List<string>();
            if (!string.IsNullOrEmpty(groupIdList))
            {
                idList = groupIdList.ToDelimitedList<string>(",");
            }

            CommonResult result = new CommonResult();
            try
            {
                result.Success = BLLFactory<Contact>.Instance.ModifyContactGroup(contactId, idList);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error(ex);
                result.ErrorMessage = ex.Message;
            }
            return ToJsonContent(result);
        }

        #region ����Excel���ݲ���
        
        //����򵼳����ֶ��б�    
        string columnString = "�ͻ�����,���,����,���֤����,��������,�Ա�,�칫�绰,��ͥ�绰,����,��ϵ���ֻ�,��ϵ�˵�ַ,��������,�����ʼ�,QQ����,��ע,�������,����ʡ��,����,����������,����,��ͥסַ,����,�����̶�,��ҵѧУ,������ò,ְҵ����,ְ��,ְ��,���ڲ���,����,����,����,����״̬,����״��,��Ҫ����,�Ͽɳ̶�,��ϵ,��������,�����ص�,��������,����,����,�Ⱦ�,���,����,����,���˼���";

        /// <summary>
        /// ���Excel�ļ����ֶ��Ƿ�����˱�����ֶ�
        /// </summary>
        /// <param name="guid">������GUID</param>
        /// <returns></returns>
        public ActionResult CheckExcelColumns(string guid)
        {
            CommonResult result = new CommonResult();

            try
            {
                DataTable dt = ConvertExcelFileToTable(guid);
                if (dt != null)
                {
                    //����б��Ƿ����������ֶ�
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
        /// ��ȡ�������ϵ�Excel�ļ���������ת��Ϊʵ���б��ظ��ͻ���
        /// </summary>
        /// <param name="guid">������GUID</param>
        /// <returns></returns>
        public ActionResult GetExcelData(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            List<ContactInfo> list = new List<ContactInfo>();

            DataTable table = ConvertExcelFileToTable(guid);
            if (table != null)
            {
                #region ����ת��
                int i = 1;
                foreach (DataRow dr in table.Rows)
                {
                    string customerName = dr["�ͻ�����"].ToString();
                    if (string.IsNullOrEmpty(customerName))
                    {
                        continue;//�ͻ�����Ϊ�գ���¼����
                    }

                    CustomerInfo customerInfo = BLLFactory<Customer>.Instance.FindByName(customerName);
                    if (customerInfo == null)
                    {
                        continue;//�ͻ����Ʋ����ڣ���¼����
                    }

                    bool converted = false;
                    DateTime dtDefault = Convert.ToDateTime("1900-01-01");
                    DateTime dt;
                    ContactInfo info = new ContactInfo();
                    info.Customer_ID = customerInfo.ID;//�ͻ�ID
                    info.HandNo = dr["���"].ToString();
                    info.Name = dr["����"].ToString();
                    info.IDCarNo = dr["���֤����"].ToString();
                    converted = DateTime.TryParse(dr["��������"].ToString(), out dt);
                    if (converted && dt > dtDefault)
                    {
                        info.Birthday = dt;
                    }
                    info.Sex = dr["�Ա�"].ToString();
                    info.OfficePhone = dr["�칫�绰"].ToString();
                    info.HomePhone = dr["��ͥ�绰"].ToString();
                    info.Fax = dr["����"].ToString();
                    info.Mobile = dr["��ϵ���ֻ�"].ToString();
                    info.Address = dr["��ϵ�˵�ַ"].ToString();
                    info.ZipCode = dr["��������"].ToString();
                    info.Email = dr["�����ʼ�"].ToString();
                    info.Qq = dr["QQ����"].ToString();
                    info.Note = dr["��ע"].ToString();
                    info.Seq = dr["�������"].ToString();
                    info.Province = dr["����ʡ��"].ToString();
                    info.City = dr["����"].ToString();
                    info.District = dr["����������"].ToString();
                    info.Hometown = dr["����"].ToString();
                    info.HomeAddress = dr["��ͥסַ"].ToString();
                    info.Nationality = dr["����"].ToString();
                    info.Eduction = dr["�����̶�"].ToString();
                    info.GraduateSchool = dr["��ҵѧУ"].ToString();
                    info.Political = dr["������ò"].ToString();
                    info.JobType = dr["ְҵ����"].ToString();
                    info.Titles = dr["ְ��"].ToString();
                    info.Rank = dr["ְ��"].ToString();
                    info.Department = dr["���ڲ���"].ToString();
                    info.Hobby = dr["����"].ToString();
                    info.Animal = dr["����"].ToString();
                    info.Constellation = dr["����"].ToString();
                    info.MarriageStatus = dr["����״̬"].ToString();
                    info.HealthCondition = dr["����״��"].ToString();
                    info.Importance = dr["��Ҫ����"].ToString();
                    info.Recognition = dr["�Ͽɳ̶�"].ToString();
                    info.RelationShip = dr["��ϵ"].ToString();
                    info.ResponseDemand = dr["��������"].ToString();
                    info.CareFocus = dr["�����ص�"].ToString();
                    info.InterestDemand = dr["��������"].ToString();
                    info.BodyType = dr["����"].ToString();
                    info.Smoking = dr["����"].ToString();
                    info.Drink = dr["�Ⱦ�"].ToString();
                    info.Height = dr["���"].ToString();
                    info.Weight = dr["����"].ToString();
                    info.Vision = dr["����"].ToString();
                    info.Introduce = dr["���˼���"].ToString();

                    info.Creator = CurrentUser.ID.ToString();
                    info.CreateTime = DateTime.Now;
                    info.Editor = CurrentUser.ID.ToString();
                    info.EditTime = DateTime.Now;

                    //����һ�������ֶε�ת��
                    info.Data1 = BLLFactory<Customer>.Instance.GetCustomerName(info.Customer_ID);

                    list.Add(info);
                }
                #endregion
            }

            #region ����ת�����
            ////����һ���ͻ������ֶΣ�Ȼ����н���������һ��DataTable����
            //DataTable dtReturn = DataTableHelper.ListToDataTable<ContactInfo>(list);
            //dtReturn.Columns.Add("CustomerName");

            //foreach (DataRow row in dtReturn.Rows)
            //{
            //    row["CustomerName"] = BLLFactory<Customer>.Instance.GetCustomerName(row["Customer_ID"].ToString());
            //}
            //var result = new { total = dtReturn.Rows.Count, rows = dtReturn }; 
            #endregion

            var result = new { total = list.Count, rows = list };
            return ToJsonContentDate(result);
        }

        /// <summary>
        /// ����ͻ����ϴ�����������б�
        /// </summary>
        /// <param name="list">�����б�</param>
        /// <returns></returns>
        public ActionResult SaveExcelData(List<ContactInfo> list)
        {
            CommonResult result = new CommonResult();
            if (list != null && list.Count > 0)
            {
                #region ����������������ύ

                DbTransaction trans = BLLFactory<Contact>.Instance.CreateTransaction();
                if (trans != null)
                {
                    try
                    {
                        //int seq = 1;
                        foreach (ContactInfo detail in list)
                        {
                            //detail.Seq = seq++;//����1
                            detail.CreateTime = DateTime.Now;
                            detail.Creator = CurrentUser.ID.ToString();
                            detail.Editor = CurrentUser.ID.ToString();
                            detail.EditTime = DateTime.Now;

                            BLLFactory<Contact>.Instance.Insert(detail, trans);
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
                result.ErrorMessage = "������Ϣ����Ϊ��";
            }

            return ToJsonContent(result);
        }

        /// <summary>
        /// ���ݲ�ѯ���������б�����
        /// </summary>
        /// <returns></returns>
        public ActionResult Export()
        {
            #region ���ݲ�����ȡList�б�
            string where = GetPagerCondition();
            string CustomedCondition = Request["CustomedCondition"] ?? "";
            List<ContactInfo> list = new List<ContactInfo>();

            if (!string.IsNullOrWhiteSpace(CustomedCondition))
            {
                Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(CustomedCondition);
                if (dict != null)
                {
                    string id = dict["id"];
                    string groupname = dict["groupname"];
                    string userid = dict["userid"];

                    if (string.IsNullOrEmpty(id))
                    {
                        if (groupname == "������ϵ��")
                        {
                            where = "";//ֱ��ʹ�ÿ�����
                            list = baseBLL.Find(where);
                        }
                        else if (groupname == "δ������ϵ��")
                        {
                            list = BLLFactory<Contact>.Instance.FindByGroupName(userid, null);
                        }
                    }
                    else
                    {
                        list = BLLFactory<Contact>.Instance.FindByGroupName(userid, groupname);
                    }
                }
            }
            else
            {
                list = baseBLL.Find(where);
            }
            #endregion

            #region ���б�ת��ΪDataTable
            DataTable datatable = DataTableHelper.CreateTable("���|int," + columnString);
            DataRow dr;
            int j = 1;
            for (int i = 0; i < list.Count; i++)
            {
                dr = datatable.NewRow();
                dr["���"] = j++;
                dr["�ͻ�����"] = BLLFactory<Customer>.Instance.GetCustomerName(list[i].Customer_ID);//ת��Ϊ�ͻ�����
                dr["���"] = list[i].HandNo;
                dr["����"] = list[i].Name;
                dr["���֤����"] = list[i].IDCarNo;
                dr["��������"] = list[i].Birthday;
                dr["�Ա�"] = list[i].Sex;
                dr["�칫�绰"] = list[i].OfficePhone;
                dr["��ͥ�绰"] = list[i].HomePhone;
                dr["����"] = list[i].Fax;
                dr["��ϵ���ֻ�"] = list[i].Mobile;
                dr["��ϵ�˵�ַ"] = list[i].Address;
                dr["��������"] = list[i].ZipCode;
                dr["�����ʼ�"] = list[i].Email;
                dr["QQ����"] = list[i].Qq;
                dr["��ע"] = list[i].Note;
                dr["�������"] = list[i].Seq;
                dr["����ʡ��"] = list[i].Province;
                dr["����"] = list[i].City;
                dr["����������"] = list[i].District;
                dr["����"] = list[i].Hometown;
                dr["��ͥסַ"] = list[i].HomeAddress;
                dr["����"] = list[i].Nationality;
                dr["�����̶�"] = list[i].Eduction;
                dr["��ҵѧУ"] = list[i].GraduateSchool;
                dr["������ò"] = list[i].Political;
                dr["ְҵ����"] = list[i].JobType;
                dr["ְ��"] = list[i].Titles;
                dr["ְ��"] = list[i].Rank;
                dr["���ڲ���"] = list[i].Department;
                dr["����"] = list[i].Hobby;
                dr["����"] = list[i].Animal;
                dr["����"] = list[i].Constellation;
                dr["����״̬"] = list[i].MarriageStatus;
                dr["����״��"] = list[i].HealthCondition;
                dr["��Ҫ����"] = list[i].Importance;
                dr["�Ͽɳ̶�"] = list[i].Recognition;
                dr["��ϵ"] = list[i].RelationShip;
                dr["��������"] = list[i].ResponseDemand;
                dr["�����ص�"] = list[i].CareFocus;
                dr["��������"] = list[i].InterestDemand;
                dr["����"] = list[i].BodyType;
                dr["����"] = list[i].Smoking;
                dr["�Ⱦ�"] = list[i].Drink;
                dr["���"] = list[i].Height;
                dr["����"] = list[i].Weight;
                dr["����"] = list[i].Vision;
                dr["���˼���"] = list[i].Introduce;

                datatable.Rows.Add(dr);
            } 
            #endregion

            #region ��DataTableת��ΪExcel�����
            Aspose.Cells.Workbook workbook = new Aspose.Cells.Workbook();
            //Ϊ��Ԫ�������ʽ    
            Aspose.Cells.Style style = workbook.Styles[workbook.Styles.Add()];
            //���þ���
            style.HorizontalAlignment = Aspose.Cells.TextAlignmentType.Center;
            //���ñ�����ɫ
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

            //�����û�����Ŀ¼��ȷ�����ɵ��ļ����������ͻ
            string filePath = string.Format("/GenerateFiles/{0}/Contact.xls", CurrentUser.Name);
            string realPath = Server.MapPath(filePath);
            string parentPath = Directory.GetParent(realPath).FullName;
            DirectoryUtil.AssertDirExist(parentPath);

            workbook.Save(realPath, Aspose.Cells.SaveFormat.Excel97To2003); 

            #endregion

            //�������ɺ���ļ�·�����ÿͻ��˸��ݵ�ַ����
            return Content(filePath);
        }

        #endregion

    }
}
