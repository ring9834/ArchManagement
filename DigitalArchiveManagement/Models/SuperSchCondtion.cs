using System;

namespace ArchiveFileManagementNs.Models
{
    public class SuperSchCondtion
    {
        /// <summary>
        /// ��ֵ
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// AND �� OR
        /// </summary>
        public string AndOr { get; set; }

        /// <summary>
        /// �������� �磬���ڣ�����...
        /// </summary>
        public string Condition { get; set; }

        /// <summary>
        /// ֵ
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// �ֶ���
        /// </summary>
        public string Field { get; set; }
    }
}