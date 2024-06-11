using System;
using System.IO;

namespace ArchiveFileManagementNs.Models
{
    public class MoSearch
    {
        public string Txt1 { get; set; }

        public string Sel1 { get; set; }
    }

    public class MoFile
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Url { get; set; }
        public string Content { get; set; }
        public FileAttributes Attributes { get; set; }
    }

    /// <summary>
    /// 接口统一类
    /// </summary>
    public class MoData
    {
        public string Msg { get; set; }

        public int Status { get; set; }
    }
}