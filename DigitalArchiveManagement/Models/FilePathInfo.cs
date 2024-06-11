using System;
using System.IO;

namespace ArchiveFileManagementNs.Models
{
    public class FilePathInfo
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public FileAttributes Attributes { get; set; }
        public string Extension { get; set; }
        public int ID { get; set; }
    }
}