using System;

namespace ArchiveFileManagementNs.Models
{
    public class OperationInfo
    {
        public string UserId { get; set; }//操作人员
        public string UserName { get; set; }//操作人员
        public string Department { get; set; }//所属部门
        public string ArchType { get; set; }//档案类型
        public string TableName { get; set; }//档案类型对应的表
        public string FuncName { get; set; }//功能名
        public string FuncModal { get; set; }//所述功能模块
        public string OperTime { get; set; }//操作时间
        public string OperTag { get; set; }//操作说明
        public string SourceIP { get; set; }//操作IP
    }
}