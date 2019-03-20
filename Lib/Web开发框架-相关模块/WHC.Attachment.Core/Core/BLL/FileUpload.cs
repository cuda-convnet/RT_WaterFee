using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;

using WHC.Attachment.Entity;
using WHC.Attachment.IDAL;
using WHC.Pager.Entity;
using WHC.Framework.Commons;
using WHC.Framework.ControlUtil;

namespace WHC.Attachment.BLL
{
    /// <summary>
    /// �ϴ��ļ���Ϣ
    /// </summary>
	public class FileUpload : BaseBLL<FileUploadInfo>
    {
        public FileUpload() : base()
        {
            base.Init(this.GetType().FullName, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
        }

        /// <summary>
        /// �ϴ��ļ�
        /// </summary>
        /// <param name="info">�ļ���Ϣ�����������ݣ�</param>
        /// <returns></returns>
        public CommonResult Upload(FileUploadInfo info)
        {
            CommonResult result = new CommonResult();

            try
            {
                #region ȷ�����Ŀ¼��Ȼ���ϴ��ļ�

                string relativeSavePath = "";

                //����ϴ���ʱ�� ��ָ���˻���·������ô�Ͳ����޸�
                if (string.IsNullOrEmpty(info.BasePath))
                {
                    //���ûָ������·������������Ϊ�������û��������AttachmentBasePath��Ĭ��һ�����Ŀ¼
                    AppConfig config = new AppConfig();
                    string AttachmentBasePath = config.AppConfigGet("AttachmentBasePath");//���õĻ���·��
                    if (string.IsNullOrEmpty(AttachmentBasePath))
                    {
                        //Ĭ���Ը�Ŀ¼�µ�UploadFilesĿ¼Ϊ�ϴ�Ŀ¼�� ����"C:\SPDTPatientMisService\UploadFiles";
                        info.BasePath = "UploadFiles";//Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "UploadFiles");
                    }
                    else
                    {
                        info.BasePath = AttachmentBasePath;
                    }

                     //���ûָ������·��,�ͱ����ļ����ϴ�
                    relativeSavePath = UploadFile(info);
                }
                else
                {
                    //���ָ���˻���·������ô����Winform���س��������ӣ�����Ҫ�ļ��ϴ�,���·�������ļ���
                    relativeSavePath = info.FileName;
                }                

                #endregion

                if (!string.IsNullOrEmpty(relativeSavePath))
                {
                    info.SavePath = relativeSavePath.Trim('\\');
                    info.AddTime = DateTime.Now;

                    try
                    {
                        bool success = base.Insert(info);
                        if (success)
                        {
                            result.Success = success;
                        }
                        else
                        {
                            result.ErrorMessage = "����д�����ݿ����";
                        }
                    }
                    catch (Exception ex)
                    {
                        //FileUtil.DeleteFile(filePath);
                        result.ErrorMessage = ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// ��ȡ��һ���ļ����ݣ������ļ��ֽ����ݣ�
        /// </summary>
        /// <param name="id">������¼��ID</param>
        /// <returns></returns>
        public FileUploadInfo Download(string id)
        {
            FileUploadInfo info = FindByID(id);
            if (info != null && !string.IsNullOrEmpty(info.SavePath))
            {
                string serverRealPath = Path.Combine(info.BasePath, info.SavePath.Trim('\\'));
                if (!Path.IsPathRooted(serverRealPath))
                {
                    //��������Ŀ¼�����ϵ�ǰ�����Ŀ¼���ܶ�λ�ļ���ַ
                    serverRealPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, serverRealPath);
                }

                if (File.Exists(serverRealPath))
                {
                    info.FileData = FileUtil.FileToBytes(serverRealPath);
                }
            }
            return info;
        }

        /// <summary>
        /// ��ȡ��һ���ļ����ݣ������ļ��ֽ����ݣ�
        /// </summary>
        /// <param name="id">������¼��ID</param>
        /// <returns></returns>
        public FileUploadInfo Download(string id, int width, int height)
        {
            //����ͼƬ�����ߴ�
            width = width > 1024 ?  1024 : width;
            height = height > 768 ? 768 : height;

            FileUploadInfo info = FindByID(id);
            if (info != null && !string.IsNullOrEmpty(info.SavePath))
            {
                string serverRealPath = Path.Combine(info.BasePath, info.SavePath.Trim('\\'));
                if (!Path.IsPathRooted(serverRealPath))
                {
                    //��������Ŀ¼�����ϵ�ǰ�����Ŀ¼���ܶ�λ�ļ���ַ
                    serverRealPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, serverRealPath);
                }

                if (File.Exists(serverRealPath))
                {
                    byte[] bytes = FileUtil.FileToBytes(serverRealPath);
                    Image image = ImageHelper.BitmapFromBytes(bytes);
                    Image smallImage = ImageHelper.ChangeImageSize(image, width, height);
                    info.FileData = ImageHelper.ImageToBytes(smallImage);
                }
            }
            return info;
        }

        /// <summary>
        /// ��ȡָ���û����ϴ���Ϣ
        /// </summary>
        /// <param name="userId">�û�ID</param>
        /// <returns></returns>
        public List<FileUploadInfo> GetAllByUser(string userId)
        {
            IFileUpload dal = baseDal as IFileUpload;
            return dal.GetAllByUser(userId);
        }
               
        /// <summary>
        /// ��ȡָ���û����ϴ���Ϣ
        /// </summary>
        /// <param name="userId">�û�ID</param>
        /// <param name="category">�������ࣺ���˸�����ҵ�񸽼�</param>
        /// <param name="pagerInfo">��ҳ��Ϣ</param>
        /// <returns></returns>
        public List<FileUploadInfo> GetAllByUser(string userId, string category, PagerInfo pagerInfo)
        {
            IFileUpload dal = baseDal as IFileUpload;
            return dal.GetAllByUser(userId, category, pagerInfo);
        }

        /// <summary>
        /// ��ȡָ��������GUID�ĸ�����Ϣ
        /// </summary>
        /// <param name="attachmentGUID">������GUID</param>
        /// <param name="pagerInfo">��ҳ��Ϣ</param>
        /// <returns></returns>
        public List<FileUploadInfo> GetByAttachGUID(string attachmentGUID, PagerInfo pagerInfo)
        {
            IFileUpload dal = baseDal as IFileUpload;
            return dal.GetByAttachGUID(attachmentGUID, pagerInfo);
        }
                        
        /// <summary>
        /// ��ȡָ��������GUID�ĸ�����Ϣ
        /// </summary>
        /// <param name="attachmentGUID">������GUID</param>
        /// <returns></returns>
        public List<FileUploadInfo> GetByAttachGUID(string attachmentGUID)
        {
            IFileUpload dal = baseDal as IFileUpload;
            return dal.GetByAttachGUID(attachmentGUID);
        }

        /// <summary>
        /// �����ļ������·����ɾ���ļ�
        /// </summary>
        /// <param name="relativeFilePath"></param>
        /// <returns></returns>
        public bool DeleteByFilePath(string relativeFilePath, string userId)
        {
            IFileUpload dal = baseDal as IFileUpload;
            return dal.DeleteByFilePath(relativeFilePath, userId);
        }

        /// <summary>
        /// ����Owner��ȡ��Ӧ�ĸ����б�
        /// </summary>
        /// <param name="ownerID">ӵ����ID</param>
        /// <returns></returns>
        public List<FileUploadInfo> GetByOwner(string ownerID)
        {
            string condition = string.Format("Owner_ID ='{0}' ", ownerID);
            return base.Find(condition);
        }

        /// <summary>
        /// ����Owner��ȡ��Ӧ�ĸ����б�
        /// </summary>
        /// <param name="ownerID">ӵ����ID</param>
        /// <returns></returns>
        public List<FileUploadInfo> GetByOwner(string ownerID, PagerInfo pagerInfo)
        {
            string condition = string.Format("Owner_ID ='{0}' ", ownerID);
            return base.FindWithPager(condition, pagerInfo);
        }

        /// <summary>
        /// ����Owner��ȡ��Ӧ�ĸ����б�
        /// </summary>
        /// <param name="ownerID">ӵ����ID</param>
        /// <param name="attachmentGUID">������GUID</param>
        /// <returns></returns>
        public List<FileUploadInfo> GetByOwnerAndAttachGUID(string ownerID, string attachmentGUID)
        {
            string condition = string.Format("Owner_ID ='{0}' AND AttachmentGUID='{1}' ", ownerID, attachmentGUID);
            return base.Find(condition);
        }

        /// <summary>
        /// ����Owner��ȡ��Ӧ�ĸ����б�
        /// </summary>
        /// <param name="ownerID">ӵ����ID</param>
        /// <param name="attachmentGUID">������GUID</param>
        /// <returns></returns>
        public List<FileUploadInfo> GetByOwnerAndAttachGUID(string ownerID, string attachmentGUID, PagerInfo pagerInfo)
        {
            string condition = string.Format("Owner_ID ='{0}' AND AttachmentGUID='{1}' ", ownerID, attachmentGUID);
            return base.FindWithPager(condition, pagerInfo);
        }

        /// <summary>
        /// ���ݸ�����GUID��ȡ��Ӧ���ļ����б������г��ļ���
        /// </summary>
        /// <param name="attachmentGUID">������GUID</param>
        /// <returns>����ID���ļ������б�</returns>
        public Dictionary<string, string> GetFileNames(string attachmentGUID)
        {
            IFileUpload dal = baseDal as IFileUpload;
            return dal.GetFileNames(attachmentGUID);
        }

        /// <summary>
        /// ���Ϊɾ������ֱ��ɾ��)
        /// </summary>
        /// <param name="id">�ļ���ID</param>
        /// <returns></returns>
        public bool SetDeleteFlag(string id)
        {
            IFileUpload dal = baseDal as IFileUpload;
            return dal.SetDeleteFlag(id);
        }

        /// <summary>
        /// ���ļ����浽ָ��Ŀ¼,��������Ի���Ŀ¼��·��
        /// </summary>
        /// <param name="info">�ļ��ϴ���Ϣ</param>
        /// <returns>�ɹ�������Ի���Ŀ¼��·�������򷵻ؿ��ַ�</returns>
        private string UploadFile(FileUploadInfo info)
        {
            //������뼰���·��
            string filePath = GetFilePath(info);
            string relativeSavePath = filePath.Replace(info.BasePath, "").Trim('\\');//�滻����ʼĿ¼��Ϊ���·��

            string serverRealPath = filePath;
            if (!Path.IsPathRooted(filePath))
            {
                serverRealPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, filePath);
            }

            //ͨ��ʵ���ļ���ȥ���Ҷ�Ӧ���ļ�����
            serverRealPath = GetRightFileName(serverRealPath, 1);

            //���ļ��Ѵ��ڣ�����������ʱ���޸�Filename��relativeSavePath
            relativeSavePath = relativeSavePath.Substring(0, relativeSavePath.LastIndexOf(info.FileName)) +   FileUtil.GetFileName(serverRealPath);
            info.FileName = FileUtil.GetFileName(serverRealPath);
            
            //����ʵ���ļ��������ļ�
            FileUtil.CreateFile(serverRealPath, info.FileData);

            bool success = FileUtil.IsExistFile(serverRealPath);
            if (success)
            {
                return relativeSavePath;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// ������뼰���·��
        /// </summary>
        /// <param name="info">�ϴ��ļ���Ϣ</param>
        /// <returns></returns>
        public string GetFilePath(FileUploadInfo info)
        {
            string fileName = info.FileName;
            string category = info.Category;

            if (string.IsNullOrEmpty(category))
            {
                category = "Photo";
            }

            //��������Ŀ¼����
            string uploadFolder = Path.Combine(info.BasePath, category);
            string realFolderPath = uploadFolder;

            //���Ŀ¼Ϊ���Ŀ¼����ôת��Ϊʵ��Ŀ¼��������
            if (!Path.IsPathRooted(uploadFolder))
            {
                realFolderPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, uploadFolder);
            }
            if (!Directory.Exists(realFolderPath))
            {
                Directory.CreateDirectory(realFolderPath);
            }

            //�������Ŀ¼
            string filePath = Path.Combine(uploadFolder, fileName);
            return filePath;
        }

        /// <summary>
        /// ����attachmentGUID�Ĳ�����ȡ��Ӧ�ĵ�һ���ļ�·��
        /// </summary>
        /// <param name="attachmentGUID">������attachmentGUID</param>
        /// <returns></returns>
        public string GetFirstFilePath(string attachmentGUID)
        {
            string serverRealPath = "";
            if (!string.IsNullOrEmpty(attachmentGUID))
            {
                List<FileUploadInfo> fileList = BLLFactory<FileUpload>.Instance.GetByAttachGUID(attachmentGUID);
                if (fileList != null && fileList.Count > 0)
                {
                    FileUploadInfo fileInfo = fileList[0];
                    if (fileInfo != null)
                    {
                        serverRealPath = Path.Combine(fileInfo.BasePath, fileInfo.SavePath.Trim('\\'));
                        if (!Path.IsPathRooted(serverRealPath))
                        {
                            //��������Ŀ¼�����ϵ�ǰ�����Ŀ¼���ܶ�λ�ļ���ַ
                            serverRealPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, serverRealPath);
                        }
                    }
                }
            }
            return serverRealPath;
        }

        /// <summary>
        /// �����ļ�����������������ļ��������(i)��i��1��ʼ����
        /// </summary>
        /// <param name="originalFileName">ԭ�ļ���</param>
        /// <param name="i">����ֵ</param>
        /// <returns></returns>
        private string GetRightFileName(string originalFilePath, int i)
        {
            bool fileExist = FileUtil.IsExistFile(originalFilePath);
            if (fileExist)
            {
                string onlyFileName = FileUtil.GetFileName(originalFilePath, true);
                int idx = originalFilePath.LastIndexOf(onlyFileName);
                string firstPath = originalFilePath.Substring(0, idx);
                string onlyExt = FileUtil.GetExtension(originalFilePath);
                string newFileName = string.Format("{0}{1}({2}){3}", firstPath, onlyFileName, i, onlyExt);
                if (FileUtil.IsExistFile(newFileName))
                {
                    i++;
                    return GetRightFileName(originalFilePath, i);
                }
                else
                {
                    return newFileName;
                }
            }
            else
            {
                return originalFilePath;
            }
        }

        /// <summary>
        /// ɾ��ָ����ID��¼����������Ŀ¼���ļ����Ƴ��ļ���DeletedFiles�ļ�������
        /// </summary>
        /// <param name="key">��¼ID</param>
        /// <returns></returns>
        public override bool Delete(object key, DbTransaction trans = null)
        {
            //ɾ����¼ǰ����Ҫ���ļ��ƶ���ɾ��Ŀ¼����
            FileUploadInfo info = FindByID(key, trans);
            if (info != null && !string.IsNullOrEmpty(info.SavePath))
            {
                string serverRealPath = Path.Combine(info.BasePath, info.SavePath.Trim('\\'));
                if (!Path.IsPathRooted(serverRealPath))
                {
                    //��������Ŀ¼�����ϵ�ǰ�����Ŀ¼���ܶ�λ�ļ���ַ
                    serverRealPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, serverRealPath);

                    //��������Ŀ¼�ģ��ƶ���ɾ��Ŀ¼����
                    if (File.Exists(serverRealPath))
                    {
                        try
                        {
                            string deletedPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, Path.Combine(info.BasePath, "DeletedFiles"));
                            DirectoryUtil.AssertDirExist(deletedPath);

                            string newFilePath = Path.Combine(deletedPath, info.FileName);
                            newFilePath = GetRightFileName(newFilePath, 1);
                            File.Move(serverRealPath, newFilePath);
                        }
                        catch (Exception ex)
                        {
                            LogTextHelper.Error(ex);
                        }
                    }
                }
            }

            return base.Delete(key, trans);
        }

        /// <summary>
        /// ɾ��ָ��OwnerID�����ݼ�¼
        /// </summary>
        /// <param name="owerID">�����ߵ�ID</param>
        /// <returns></returns>
        public bool DeleteByOwerID(string owerID)
        {
            string condition = string.Format("Owner_ID ='{0}' ", owerID);
            List<FileUploadInfo> list = base.Find(condition);
            foreach (FileUploadInfo info in list)
            {
                Delete(info.ID);
            }
            return true;
        }

        /// <summary>
        /// ɾ��ָ��Attachment_GUID�����ݼ�¼
        /// </summary>
        /// <param name="attachment_GUID">�����ߵ�ID</param>
        /// <returns></returns>
        public bool DeleteByAttachGUID(string attachment_GUID)
        {
            string condition = string.Format("AttachmentGUID ='{0}' ", attachment_GUID);
            List<FileUploadInfo> list = base.Find(condition);
            foreach (FileUploadInfo info in list)
            {
                Delete(info.ID);
            }
            return true;
        }
    }
}
