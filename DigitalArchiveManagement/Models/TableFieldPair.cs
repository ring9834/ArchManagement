using System;
using System.Collections.Generic;

namespace ArchiveFileManagementNs.Models
{
    public class TableFieldPair
    {
        public string Id { get; set; }
        public string Table { get; set; }
        public string Field { get; set; }
        public string CountType { get; set; }
    }

    public class Statistics
    {
        public string statistic_type { get; set; }

        public List<TableFieldPair> Tfs { get; set; }
    }
}