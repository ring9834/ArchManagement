using System;

namespace ArchiveFileManagementNs.Models
{
    public class SuperSchCondtion
    {
        /// <summary>
        /// 键值
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// AND 或 OR
        /// </summary>
        public string AndOr { get; set; }

        /// <summary>
        /// 条件符号 如，等于，大于...
        /// </summary>
        public string Condition { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 字段名
        /// </summary>
        public string Field { get; set; }
    }
}