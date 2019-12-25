using System;
using System.Collections.Generic;
using System.Web.Mvc;
using WHC.CRM.BLL;
using WHC.CRM.Entity;
using WHC.Framework.ControlUtil;

namespace WHC.MVCWebMis.Controllers
{
    public class ContactGroupController : BusinessController<ContactGroup, ContactGroupInfo>
    {
        public ContactGroupController() : base()
        {
        }

        #region д������ǰ�޸Ĳ�������
        protected override void OnBeforeInsert(ContactGroupInfo info)
        {
            //��������Բ�����������޸�
            info.CreateTime = DateTime.Now;
            info.Creator = CurrentUser.ID.ToString();
            info.Company_ID = CurrentUser.Company_ID;
            info.Dept_ID = CurrentUser.Dept_ID;
        }

        protected override void OnBeforeUpdate(ContactGroupInfo info)
        {
            //��������Բ�����������޸�
            info.Editor = CurrentUser.ID.ToString();
            info.EditTime = DateTime.Now;
        }
        #endregion

        /// <summary>
        /// ��ȡ��ϵ�˷�����Json�ַ���
        /// </summary>
        /// <returns></returns>
        public ActionResult GetGroupTreeJson(string userId)
        {
            //���һ��δ�����ȫ���ͻ������
            List<EasyTreeData> treeList = new List<EasyTreeData>();
            EasyTreeData pNode = new EasyTreeData("", "������ϵ��");
            treeList.Insert(0, pNode);
            treeList.Add(new EasyTreeData("", "δ������ϵ��"));

            List<ContactGroupNodeInfo> groupList = BLLFactory<ContactGroup>.Instance.GetTree(userId);
            AddContactGroupTree(groupList, pNode);

            return ToJsonContent(treeList);
        }

        /// <summary>
        /// ��ʼ�����󶨿ͻ����˷�����Ϣ
        /// </summary>
        public ActionResult GetMyContactGroup(string contactId, string userId)
        {
            List<ContactGroupInfo> myGroupList = BLLFactory<ContactGroup>.Instance.GetByContact(contactId);
            List<string> groupIdList = new List<string>();
            foreach (ContactGroupInfo info in myGroupList)
            {
                groupIdList.Add(info.ID);
            }

            List<ContactGroupNodeInfo> groupList = BLLFactory<ContactGroup>.Instance.GetTree(userId);

            List<EasyTreeData> treeList = new List<EasyTreeData>();
            foreach (ContactGroupNodeInfo nodeInfo in groupList)
            {
                bool check = groupIdList.Contains(nodeInfo.ID);
                EasyTreeData treeData = new EasyTreeData(nodeInfo.ID, nodeInfo.Name);
                treeData.Checked = check;

                treeList.Add(treeData);
            }

            return ToJsonContent(treeList);
        }

        /// <summary>
        /// ��ȡ�ͻ����鲢��
        /// </summary>
        private void AddContactGroupTree(List<ContactGroupNodeInfo> nodeList, EasyTreeData treeNode)
        {
            foreach (ContactGroupNodeInfo nodeInfo in nodeList)
            {
                EasyTreeData subNode = new EasyTreeData(nodeInfo.ID, nodeInfo.Name, "icon-view");
                treeNode.children.Add(subNode);

                AddContactGroupTree(nodeInfo.Children, subNode);
            }
        }
    }
}
