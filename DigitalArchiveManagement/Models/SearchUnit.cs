using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveFileManagementNs.Models
{
    #region 索引的一个行单元，相当于数据库中的一行数据  
    /// <summary>  
    /// 索引的一个行单元，相当于数据库中的一行数据  
    /// </summary>  
    public class SearchUnit
    {
        public SearchUnit()
        { }
        public SearchUnit(string _id, string _title, string _content, string _flag, string _imageurl, string _updatetime)
        {
            this.id = _id;
            this.title = _title;
            this.content = _content;
            this.flag = _flag;
            this.imageurl = _imageurl;
            this.updatetime = _updatetime;
        }
        /// <summary>  
        /// 唯一的id号  
        /// </summary>  
        public string id { get; set; }
        /// <summary>  
        /// 标题  
        /// </summary>  
        public string title { get; set; }
        /// <summary>  
        /// 内容  
        /// </summary>  
        public string content { get; set; }
        /// <summary>  
        /// 其他信息  
        /// </summary>  
        public string flag { get; set; }
        /// <summary>  
        /// 图片路径  
        /// </summary>  
        public string imageurl { get; set; }
        /// <summary>  
        /// 时间  
        /// </summary>  
        public string updatetime { get; set; }
    }
    #endregion
}
