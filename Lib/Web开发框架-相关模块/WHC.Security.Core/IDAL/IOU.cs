﻿using System.Collections;
using WHC.Security.Entity;
using System.Collections.Generic;
using WHC.Framework.ControlUtil;
using System.Data.Common;

namespace WHC.Security.IDAL
{
    public interface IOU : IBaseDAL<OUInfo>
	{
		void AddUser(int userID, int ouID);
        List<OUInfo> GetOUsByRole(int roleID);
        List<OUInfo> GetOUsByUser(int userID);
		void RemoveUser(int userID, int ouID);
                        
        /// <summary>
        /// 根据指定机构节点ID，获取其下面所有机构列表
        /// </summary>
        /// <param name="parentId">指定机构节点ID</param>
        /// <returns></returns>
        List<OUInfo> GetAllOUsByParent(int parentId);

        /// <summary>
        /// 获取树形结构的机构列表
        /// </summary>
        List<OUNodeInfo> GetTree();

        /// <summary>
        /// 获取指定机构下面的树形列表
        /// </summary>
        /// <param name="mainOUID">指定机构ID</param>
        List<OUNodeInfo> GetTreeByID(int mainOUID);

        /// <summary>
        /// 获取机构的名称
        /// </summary>
        /// <param name="id">机构ID</param>
        /// <returns></returns>
        string GetName(int id, DbTransaction trans = null);
                      
        /// <summary>
        /// 设置删除标志
        /// </summary>
        /// <param name="id">记录ID</param>
        /// <param name="deleted">是否删除</param>
        /// <param name="trans">事务对象</param>
        /// <returns></returns>
        bool SetDeletedFlag(object id, bool deleted = true, DbTransaction trans = null);
                        
        /// <summary>
        /// 为机构制定新的人员列表
        /// </summary>
        /// <param name="ouID">机构ID</param>
        /// <param name="newUserList">人员列表</param>
        /// <returns></returns>
        bool EditOuUsers(int ouID, List<int> newUserList);
    }
}