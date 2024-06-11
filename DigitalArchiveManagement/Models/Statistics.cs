using System;

namespace ArchiveFileManagementNs.Models
{
    public class SearchCondition
    {
        public string unique_code { get; set; }
        public string show_name { get; set; }
        public string col_name { get; set; }
        public string col_condition { get; set; }
        public string col_value { get; set; }
        public string col_andor { get; set; }
    }

    public class StatisticFields
    {
        public string unique_code { get; set; }
        public string show_name { get; set; }
        public string col_name { get; set; }
    }

    public class GroupFields
    {
        public string unique_code { get; set; }
        public string show_name { get; set; }
        public string col_name { get; set; }
    }
}