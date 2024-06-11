using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveFileManagementNs.Models
{
    public class ColDictionary
    {
        public string ShowName { get; set; }
        public string ColName { get; set; }
        public bool ColNull { get; set; }
        public string MaxLength { get; set; }

        public string ColShowType { get; set; }//显示的那种控件：下拉框、文本框等
        public string ColShowValue { get; set; }//显示的控件种类对应的代码值

        public string BaseCode { get; set; } //显示的哪个辅助代码：保管期限、密级等

        public string DataType { get; set; }//数据类型，如，字符串或数字类型


        public ColDictionary(){}

        public ColDictionary(string showName, string colName, bool colNull, string maxLength)
        {
            this.ShowName = showName;
            this.ColName = colName;
            this.ColNull = colNull;
            this.MaxLength = maxLength;
        }

        public override string ToString()
        {
            return this.ShowName;
        }
    }
}
