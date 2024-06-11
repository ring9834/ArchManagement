using System;
using System.Collections.Generic;

namespace ArchiveFileManagementNs.Models
{
    public class ArchNumMakeUp
    {
        public int Amount { get; set; }

        public string ArchFieldName { get; set; }

        public string ConnectChar { get; set; }

        public List<ArchNumItem> ArchItems { get; set; }
    }

    public class ArchNumItem
    {
        public int ID { get; set; }
        public string ShowName { get; set; }
        public string ColName { get; set; }
        public string FieldPrefix { get; set; }

    }
}