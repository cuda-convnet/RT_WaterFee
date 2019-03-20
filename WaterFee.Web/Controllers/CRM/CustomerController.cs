using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using WHC.Pager.Entity;
using WHC.Framework.Commons;
using WHC.MVCWebMis.BLL;
using WHC.MVCWebMis.Entity;
using WHC.CRM.BLL;
using WHC.CRM.Entity;
using WHC.Framework.ControlUtil;

namespace WHC.MVCWebMis.Controllers
{
    public class CustomerController : BusinessController<Customer, CustomerInfo>
    {
        public CustomerController() : base()
        {
        }

        #region д������ǰ�޸Ĳ�������
        protected override void OnBeforeInsert(CustomerInfo info)
        {
            //��������Բ�����������޸�
            info.CreateTime = DateTime.Now;
            info.Creator = CurrentUser.ID.ToString();
            info.Company_ID = CurrentUser.Company_ID;
            info.Dept_ID = CurrentUser.Dept_ID;
        }

        protected override void OnBeforeUpdate(CustomerInfo info)
        {
            //��������Բ�����������޸�
            info.Editor = CurrentUser.ID.ToString();
            info.EditTime = DateTime.Now;
        }
        #endregion

        public ActionResult SelectCustomer()
        {
            return View("SelectCustomer");
        }


        public ActionResult GetCustomerName(string id)
        {
            string name = BLLFactory<Customer>.Instance.GetCustomerName(id);
            return ToJsonContent(name);
        }
    }
}
