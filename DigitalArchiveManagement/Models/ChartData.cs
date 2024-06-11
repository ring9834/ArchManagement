using System;
using System.Collections.Generic;
using System.IO;

namespace ArchiveFileManagementNs.Models
{
    public class ChartData
    {
        public List<string> Legends { get; set; }

        public List<string> Years { get; set; }

        public List<List<int>> Series { get; set; }
    }

    public class YearCountPair
    { 
        public string Year { get; set; }

        public string Count { get; set; }
    }

    public class ChartData2
    {
        public List<string> Legends { get; set; }

        public List<int> Amounts { get; set; }
    }
}