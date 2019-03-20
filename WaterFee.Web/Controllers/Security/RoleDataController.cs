using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using WHC.Pager.Entity;
using WHC.Framework.Commons;
using WHC.Security.BLL;
using WHC.Security.Entity;
using WHC.Framework.ControlUtil;

namespace WHC.MVCWebMis.Controllers
{
    /// <summary>
    /// ��ɫ�ɷ������ݣ���֯�������Ŀ�������
    /// </summary>
    public class RoleDataController : BusinessController<RoleData, RoleDataInfo>
    {
        public RoleDataController() : base()
        {
        }

        /// <summary>
        /// �����ɫ������Ȩ��
        /// </summary>
        /// <param name="roleId">��ɫID</param>
        /// <param name="belongCompanys">������˾</param>
        /// <param name="belongDepts">��������</param>
        /// <returns></returns>
        public ActionResult UpdateRoleData(int roleId, string belongCompanys, string belongDepts)
        {
            CommonResult result = new CommonResult();
            try
            {
                result.Success = BLLFactory<RoleData>.Instance.UpdateRoleData(roleId, belongCompanys, belongDepts);
            }
            catch(Exception ex)
            {
                LogTextHelper.Error(ex);
                result.ErrorMessage = ex.Message;
            }

            return ToJsonContent(result);
        }

        /// <summary>
        /// ��ȡ��ɫ����������Ȩ�ޣ���֯����ID�б�
        /// </summary>
        /// <param name="roleId">��ɫID</param>
        /// <returns></returns>
        public ActionResult GetRoleDataList(int roleId)
        {
            Dictionary<int,int> dict = BLLFactory<RoleData>.Instance.GetRoleDataDict(roleId);

            List<int> list = new List<int>(); 
            list.AddRange(dict.Keys);

            return Json(list, JsonRequestBehavior.AllowGet);
        }
    }
}
