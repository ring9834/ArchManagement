using System;

namespace ArchiveFileManagementNs.Models
{
    public class OperationInfo
    {
        public string UserId { get; set; }//������Ա
        public string UserName { get; set; }//������Ա
        public string Department { get; set; }//��������
        public string ArchType { get; set; }//��������
        public string TableName { get; set; }//�������Ͷ�Ӧ�ı�
        public string FuncName { get; set; }//������
        public string FuncModal { get; set; }//��������ģ��
        public string OperTime { get; set; }//����ʱ��
        public string OperTag { get; set; }//����˵��
        public string SourceIP { get; set; }//����IP
    }
}