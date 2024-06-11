using System;
using System.IO;

namespace ArchiveFileManagementNs.Models
{
    public class DynamicTable
    {
        public string TableName { get; set; }
        public string FieldsStr { get; set; }
        public string ColFieldStr { get; set; }
    }
}