﻿using ArchiveFileManagementNs.Models;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ArchiveFileManagementNs
{
    /// <summary>  
    /// 盘古分词在lucene.net中的使用帮助类  
    /// 调用PanGuLuceneHelper.instance  
    /// </summary>  
    public class PanGuLuceneHelper
    {
        private PanGuLuceneHelper() { }

        #region 单一实例  
        private static PanGuLuceneHelper _instance = null;
        /// <summary>  
        /// 单一实例  
        /// </summary>  
        public static PanGuLuceneHelper instance
        {
            get
            {
                if (_instance == null) _instance = new PanGuLuceneHelper();
                return _instance;
            }
        }
        #endregion

        #region 分词测试  
        /// <summary>  
        /// 分词测试  
        /// </summary>  
        /// <param name="keyword"></param>  
        /// <returns></returns>  
        //public string Token(string keyword)
        //{
        //    string ret = "";
        //    System.IO.StringReader reader = new System.IO.StringReader(keyword);
        //    Lucene.Net.Analysis.TokenStream ts = analyzer.GetTokenStream(keyword, reader);
        //    bool hasNext = ts.IncrementToken();
        //    Lucene.Net.Analysis.TokenAttributes.ITermAttribute ita;
        //    while (hasNext)
        //    {
        //        ita = ts.GetAttribute<Lucene.Net.Analysis.TokenAttributes.ITermAttribute>();
        //        ret += ita.Term + "|";
        //        hasNext = ts.IncrementToken();
        //    }
        //    ts.CloneAttributes();
        //    reader.Close();
        //    //analyzer.Close();
        //    return ret;
        //}
        #endregion

        #region 创建索引  
        /// <summary>  
        /// 创建索引  
        /// </summary>  
        /// <param name="datalist"></param>  
        /// <returns></returns>  
        public bool CreateIndex(List<SearchUnit> datalist)
        {
            IndexWriterConfig writerConf = new IndexWriterConfig(Lucene.Net.Util.LuceneVersion.LUCENE_48, analyzer);
            IndexWriter writer = new IndexWriter(directory_luce, writerConf);
            //try
            //{
            //    writer = new IndexWriterConfig(Lucene.Net.Util.LuceneVersion.LUCENE_48, analyzer);//false表示追加（true表示删除之前的重新写入）  
            //}新写入）  
            //}
            foreach (SearchUnit data in datalist)
            //catch
            //{
            //    writer = new IndexWriter(directory_luce, analyzer, true, IndexWriter.MaxFieldLength.LIMITED);//false表示追加（true表示删除之前的重
            {
                CreateIndex(writer, data);
            }

            //writer.Optimize();
            //writer.ForceMerge();//这个是新的Optimize()
            writer.Dispose();
            return true;
        }

        public IndexWriter SpecificWriter { get; set; }
        public bool CreateIndex(SearchUnit data)
        {
            //IndexWriter writer = new IndexWriter(directory_luce, writerConf);
            if (SpecificWriter == null)
            {
                IndexWriterConfig writerConf = new IndexWriterConfig(Lucene.Net.Util.LuceneVersion.LUCENE_48, analyzer);
                writerConf.OpenMode = OpenMode.CREATE_OR_APPEND;//新建或重新覆盖
                SpecificWriter = new IndexWriter(directory_luce, writerConf);
            }
            try
            {
                if (data == null) return false;
                Document doc = new Document();
                Type type = data.GetType();//assembly.GetType("Reflect_test.PurchaseOrderHeadManageModel", true, true); //命名空间名称 + 类名      

                //创建类的实例      
                //object obj = Activator.CreateInstance(type, true);    
                //获取公共属性      
                PropertyInfo[] Propertys = type.GetProperties();
                for (int i = 0; i < Propertys.Length; i++)
                {
                    //Propertys[i].SetValue(Propertys[i], i, null); //设置值  
                    PropertyInfo pi = Propertys[i];
                    string name = pi.Name;
                    object objval = pi.GetValue(data, null);
                    string value = objval == null ? "" : objval.ToString(); //值  
                    if (name == "id" || name == "flag")//id在写入索引时必是不分词，否则是模糊搜索和删除，会出现混乱  
                    {
                        //doc.Add(new Field(name, value, Field.Store.YES, Field.Index.NOT_ANALYZED));//id不分词  
                        doc.Add(new StringField(name, value, Field.Store.YES));
                    }
                    else
                    {
                        //doc.Add(new Field(name, value, Field.Store.YES, Field.Index.ANALYZED));
                        doc.Add(new TextField(name, value, Field.Store.YES));
                    }
                }
                SpecificWriter.AddDocument(doc);

            }
            catch (System.IO.FileNotFoundException fnfe)
            {
                throw fnfe;
            }
            return true;
        }

        public bool CreateIndex(IndexWriter writer, SearchUnit data)
        {
            try
            {
                if (data == null) return false;
                Document doc = new Document();
                Type type = data.GetType();//assembly.GetType("Reflect_test.PurchaseOrderHeadManageModel", true, true); //命名空间名称 + 类名      

                //创建类的实例      
                //object obj = Activator.CreateInstance(type, true);    
                //获取公共属性      
                PropertyInfo[] Propertys = type.GetProperties();
                for (int i = 0; i < Propertys.Length; i++)
                {
                    //Propertys[i].SetValue(Propertys[i], i, null); //设置值  
                    PropertyInfo pi = Propertys[i];
                    string name = pi.Name;
                    object objval = pi.GetValue(data, null);
                    string value = objval == null ? "" : objval.ToString(); //值  
                    if (name == "id" || name == "flag")//id在写入索引时必是不分词，否则是模糊搜索和删除，会出现混乱  
                    {
                        //doc.Add(new Field(name, value, Field.Store.YES, Field.Index.NOT_ANALYZED));//id不分词  
                        doc.Add(new StringField(name, value, Field.Store.YES));
                    }
                    else
                    {
                        //doc.Add(new Field(name, value, Field.Store.YES, Field.Index.ANALYZED));
                        doc.Add(new TextField(name, value, Field.Store.YES));
                    }
                }
                writer.AddDocument(doc);

            }
            catch (System.IO.FileNotFoundException fnfe)
            {
                throw fnfe;
            }
            return true;
        }
        #endregion

        #region 在title和content字段中查询数据  
        /// <summary>  
        /// 在title和content字段中查询数据  
        /// </summary>  
        /// <param name="keyword"></param>  
        /// <returns></returns>  
        public List<SearchUnit> Search(string keyword)
        {

            //string[] fileds = { "title", "content" };//查询字段  
            string[] fileds = { "content" };//查询字段  
            //Stopwatch st = new Stopwatch();  
            //st.Start();  
            QueryParser parser = null;// new QueryParser(Lucene.Net.Util.Version.LUCENE_30, field, analyzer);//一个字段查询  
            parser = new MultiFieldQueryParser(Lucene.Net.Util.LuceneVersion.LUCENE_48, fileds, analyzer);//多个字段查询  
            Query query = parser.Parse(keyword);
            int n = 1000;
            var indexer = DirectoryReader.Open(directory_luce);
            var searcher = new IndexSearcher(indexer);
            //IndexSearcher searcher = new IndexSearcher(directory_luce, true);//true-表示只读  
            TopDocs docs = searcher.Search(query, (Filter)null, n);
            if (docs == null || docs.TotalHits == 0)
            {
                return null;
            }
            else
            {
                List<SearchUnit> list = new List<SearchUnit>();
                int counter = 1;
                foreach (ScoreDoc sd in docs.ScoreDocs)//遍历搜索到的结果  
                {
                    try
                    {
                        Document doc = searcher.Doc(sd.Doc);
                        string id = doc.Get("id");
                        string title = doc.Get("title");
                        string content = doc.Get("content");
                        string flag = doc.Get("flag");
                        string imageurl = doc.Get("imageurl");
                        string updatetime = doc.Get("updatetime");

                        string createdate = doc.Get("createdate");
                        PanGu.HighLight.SimpleHTMLFormatter simpleHTMLFormatter = new PanGu.HighLight.SimpleHTMLFormatter("<font color=\"red\">", "</font>");
                        PanGu.HighLight.Highlighter highlighter = new PanGu.HighLight.Highlighter(simpleHTMLFormatter, new PanGu.Segment());
                        highlighter.FragmentSize = 50;
                        content = highlighter.GetBestFragment(keyword, content);
                        string titlehighlight = highlighter.GetBestFragment(keyword, title);
                        if (titlehighlight != "") title = titlehighlight;
                        list.Add(new SearchUnit(id, title, content, flag, imageurl, updatetime));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    counter++;
                }
                return list;
            }
            //st.Stop();  
            //Response.Write("查询时间：" + st.ElapsedMilliseconds + " 毫秒<br/>");  

        }
        #endregion

        #region 在不同的分类下再根据title和content字段中查询数据(分页)  
        /// <summary>  
        /// 在不同的类型下再根据title和content字段中查询数据(分页)  
        /// </summary>  
        /// <param name="_flag">分类,传空值查询全部</param>  
        /// <param name="keyword"></param>  
        /// <param name="PageIndex"></param>  
        /// <param name="PageSize"></param>  
        /// <param name="TotalCount"></param>  
        /// <returns></returns>  
        public List<SearchUnit> Search(string _flag, string keyword, int PageIndex, int PageSize, out int TotalCount)
        {
            if (PageIndex < 1) PageIndex = 1;
            //Stopwatch st = new Stopwatch();  
            //st.Start();  
            BooleanQuery bq = new BooleanQuery();
            if (_flag != "")
            {
                QueryParser qpflag = new QueryParser(Lucene.Net.Util.LuceneVersion.LUCENE_48, "flag", analyzer);
                Query qflag = qpflag.Parse(_flag);
                bq.Add(qflag, Occur.MUST);//与运算  
            }
            if (keyword != "")
            {
                //string[] fileds = { "title", "content" };//查询字段  
                string[] fileds = { "content" };//查询字段  
                QueryParser parser = null;// new QueryParser(version, field, analyzer);//一个字段查询  
                parser = new MultiFieldQueryParser(Lucene.Net.Util.LuceneVersion.LUCENE_48, fileds, analyzer);//多个字段查询  
                Query queryKeyword = parser.Parse(keyword);
                bq.Add(queryKeyword, Occur.MUST);//与运算  
            }

            TopScoreDocCollector collector = TopScoreDocCollector.Create(PageIndex * PageSize, false);
            var indexer = DirectoryReader.Open(directory_luce);
            var searcher = new IndexSearcher(indexer);
            //IndexSearcher searcher = new IndexSearcher(directory_luce, true);//true-表示只读  
            searcher.Search(bq, collector);
            if (collector == null || collector.TotalHits == 0)
            {
                TotalCount = 0;
                return null;
            }
            else
            {
                int start = PageSize * (PageIndex - 1);
                //结束数  
                int limit = PageSize;
                ScoreDoc[] hits = collector.GetTopDocs(start, limit).ScoreDocs;
                List<SearchUnit> list = new List<SearchUnit>();
                int counter = 1;
                TotalCount = collector.TotalHits;
                foreach (ScoreDoc sd in hits)//遍历搜索到的结果  
                {
                    try
                    {
                        Document doc = searcher.Doc(sd.Doc);
                        string id = doc.Get("id");
                        string title = doc.Get("title");
                        string content = doc.Get("content");
                        string flag = doc.Get("flag");
                        string imageurl = doc.Get("imageurl");
                        string updatetime = doc.Get("updatetime");

                        PanGu.HighLight.SimpleHTMLFormatter simpleHTMLFormatter = new PanGu.HighLight.SimpleHTMLFormatter("<font color=\"red\">", "</font>");
                        PanGu.HighLight.Highlighter highlighter = new PanGu.HighLight.Highlighter(simpleHTMLFormatter, new PanGu.Segment());
                        highlighter.FragmentSize = 50;
                        content = highlighter.GetBestFragment(keyword, content);
                        string titlehighlight = highlighter.GetBestFragment(keyword, title);
                        if (titlehighlight != "") title = titlehighlight;
                        list.Add(new SearchUnit(id, title, content, flag, imageurl, updatetime));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    counter++;
                }
                return list;
            }
            //st.Stop();  
            //Response.Write("查询时间：" + st.ElapsedMilliseconds + " 毫秒<br/>");  

        }
        #endregion

        #region 删除索引数据（根据id）  
        /// <summary>  
        /// 删除索引数据（根据id）  
        /// </summary>  
        /// <param name="id"></param>  
        /// <returns></returns>  
        public bool Delete(string id)
        {
            bool IsSuccess = false;
            Term term = new Term("id", id);
            //Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);  
            //Version version = new Version();  
            //MultiFieldQueryParser parser = new MultiFieldQueryParser(version, new string[] { "name", "job" }, analyzer);//多个字段查询  
            //Query query = parser.Parse("小王");  

            //IndexReader reader = IndexReader.Open(directory_luce, false);  
            //reader.DeleteDocuments(term);  
            //Response.Write("删除记录结果： " + reader.HasDeletions + "<br/>");  
            //reader.Dispose();  

            IndexWriterConfig writerConf = new IndexWriterConfig(Lucene.Net.Util.LuceneVersion.LUCENE_48, analyzer);
            IndexWriter writer = new IndexWriter(directory_luce, writerConf);

            //IndexWriter writer = new IndexWriter(directory_luce, analyzer, false, IndexWriter.MaxFieldLength.LIMITED);
            writer.DeleteDocuments(term); // writer.DeleteDocuments(term)或者writer.DeleteDocuments(query);  
            ////writer.DeleteAll();  
            writer.Commit();
            //writer.Optimize();//  
            IsSuccess = writer.HasDeletions();
            writer.Dispose();
            return IsSuccess;
        }
        #endregion

        #region 删除全部索引数据  
        /// <summary>  
        /// 删除全部索引数据  
        /// </summary>  
        /// <returns></returns>  
        public bool DeleteAll()
        {
            bool IsSuccess = true;
            try
            {
                IndexWriterConfig writerConf = new IndexWriterConfig(Lucene.Net.Util.LuceneVersion.LUCENE_48, analyzer);
                IndexWriter writer = new IndexWriter(directory_luce, writerConf);
                //IndexWriter writer = new IndexWriter(directory_luce, analyzer, false, IndexWriter.MaxFieldLength.LIMITED);
                writer.DeleteAll();
                writer.Commit();
                //writer.Optimize();//  
                IsSuccess = writer.HasDeletions();
                writer.Dispose();
            }
            catch
            {
                IsSuccess = false;
            }
            return IsSuccess;
        }
        #endregion

        #region directory_luce  
        private Lucene.Net.Store.Directory _directory_luce = null;
        /// <summary>  
        /// Lucene.Net的目录-参数  
        /// </summary>  
        public Lucene.Net.Store.Directory directory_luce
        {
            get
            {
                if (_directory_luce == null) _directory_luce = Lucene.Net.Store.FSDirectory.Open(directory);
                return _directory_luce;
            }
        }
        #endregion

        #region directory  
        private System.IO.DirectoryInfo _directory = null;
        /// <summary>  
        /// 索引在硬盘上的目录  
        /// </summary>  
        public System.IO.DirectoryInfo directory
        {
            get
            {
                if (_directory == null)
                {
                    string dirPath = AppDomain.CurrentDomain.BaseDirectory + "SearchIndex";
                    if (System.IO.Directory.Exists(dirPath) == false) _directory = System.IO.Directory.CreateDirectory(dirPath);
                    else _directory = new System.IO.DirectoryInfo(dirPath);
                }
                return _directory;
            }
        }
        #endregion

        #region analyzer  
        private Analyzer _analyzer = null;
        /// <summary>  
        /// 分析器  
        /// </summary>  
        public Analyzer analyzer
        {
            get
            {
                //if (_analyzer == null)  
                {
                    //_analyzer = new Lucene.Net.Analysis.PanGu.PanGuAnalyzer();//盘古分词分析器  
                    _analyzer = new JieBaAnalyzer(JiebaNet.Segmenter.TokenizerMode.Default);//Jieba分词分析器  
                    //_analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);//标准分析器  
                }
                return _analyzer;
            }
        }
        #endregion
    }
}
