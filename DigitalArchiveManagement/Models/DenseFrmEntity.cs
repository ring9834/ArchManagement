using System;
using System.Collections.Generic;
using System.IO;

namespace ArchiveFileManagementNs.Models
{
    public class DenseFrmEntity
    {
        public string Unique_code { get; set; }
        public string code_name { get; set; }
        public string code_value { get; set; }
        public int tire_count { get; set; }
        public int sqare_count { get; set; }
        public string order_id { get; set; }
        public List<FrmData> FrameData { get; set; }
    }

    public class FrmData
    {
        public string DenseFrmInfo { get; set; }

        public string ArchInfo { get; set; }
    }
}