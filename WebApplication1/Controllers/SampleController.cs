using System;
using System.Data;
using System.Data.SqlClient;
using paems.Common;
using paems.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Net;
using Nancy.Json;

namespace paems.Controllers
{
    [ApiController]
    public class SampleController : ControllerBase
    {
        private readonly ILogger<SampleController> _logger;
        public SampleController(ILogger<SampleController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("api/test")]
        public string Post()
        {
            string res =  "";
            try
            {
                res = "false";
                
            }
            catch (Exception e)
            {
               
            }
            return res;
        }

        [HttpPost]
        [Route("api/sample/add")]
        public SampleAddRes Post([FromBody] SampleAddReq req)
        {
            SampleAddRes res = new SampleAddRes();

            try
            {
                res.success = "false";
                string sql;
                SqlParameter[] param;
                decimal count;
                TokenCheckResult tokenCheckResult = TokenHelper.CheckToken(req.token);
                if (!tokenCheckResult.isValid)
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }

                sql = "SELECT * FROM sample WHERE key_first=@key_first and key_second=@key_second";
                param = new SqlParameter[] {
                      new SqlParameter("@key_first",req.key_first),
                      new SqlParameter("@key_second",req.key_second),
                };
                count = Convert.ToDecimal(SqlHelper.ExecuteScalar(CommandType.Text, sql, param));
                if (count >= 1)
                {
                    res.errorMessage = "样本已存在";
                    return res;
                }

                sql = "INSERT INTO sample " +
                        "(category,classify,mode,key_first,key_second)" +
                        " VALUES (@category,@classify,@mode,@key_first,@key_second)";
                param = new SqlParameter[] {
                     new SqlParameter("@category",req.category),
                     new SqlParameter("@classify",req.classify),
                     new SqlParameter("@mode",req.mode),
                     new SqlParameter("@key_first",req.key_first),
                     new SqlParameter("@key_second",req.key_second),
                };
                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);
                if (DataSource != 1)
                {
                    res.errorMessage = "数据插入失败";
                    return res;
                }
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "添加配置异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        [HttpPost]
        [Route("api/sample/delete")]
        public SampleDeleteRes Post([FromBody] SampleDeleteReq req)
        {
            SampleDeleteRes res = new SampleDeleteRes();
            try
            {
                res.success = "false";
                TokenCheckResult tokenCheckResult = TokenHelper.CheckToken(req.token);
                if (!(tokenCheckResult.isValid))
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }

                string sql = "DELETE FROM sample WHERE id=@id";
                SqlParameter[] param = new SqlParameter[] {
                    new SqlParameter("@id",req.id),
                };
                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);
                if (DataSource != 1)
                {
                    res.errorMessage = "样本删除失败";
                    return res;
                }
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "删除样本接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        [HttpPost]
        [Route("api/sample/record")]
        public SampleRetrieveRes Post([FromBody] SampleRetrieveReq req)
        {
            SampleRetrieveRes res = new SampleRetrieveRes();
            try
            {
                res.success = "false";
                TokenCheckResult tokenCheckResult = TokenHelper.CheckToken(req.token);
                if (!tokenCheckResult.isValid)
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }
                string sql;
                string totalSql;
                SqlParameter[] param;
                res.data = new SampleData();
                sql = "SELECT TOP(@pageSize) * FROM sample WHERE id NOT IN" +
                        "(SELECT TOP(@beforeSize) id FROM sample ORDER BY id DESC) ORDER BY id DESC";
                totalSql = "SELECT COUNT(id) FROM sample";
                param = new SqlParameter[] {
                        new SqlParameter("@pageSize",Convert.ToInt16(req.pageSize)),
                        new SqlParameter("@beforeSize",Convert.ToInt16(req.pageSize*(req.currentPage-1))),
                    };
                res.data.total = Convert.ToDecimal(SqlHelper.ExecuteScalar(CommandType.Text, totalSql));

                var DataSource = SqlHelper.GetTableText(sql, param);

                SampleResult[] results = new SampleResult[DataSource[0].Rows.Count];
                int i = 0;
                foreach (DataTable table in DataSource)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        SampleResult result = new SampleResult();
                        result.id = Convert.ToDecimal(row["id"]);
                        result.category = Convert.ToString(row["category"]);
                        result.classify = Convert.ToString(row["classify"]);
                        result.key_first = Convert.ToString(row["key_first"]);
                        result.key_second = Convert.ToString(row["key_second"]);
                        result.mode = Convert.ToString(row["mode"]);
                        results[i] = result;
                        i++;
                    }
                }
                res.data.result = results;
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "样本记录接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        [HttpPost]
        [Route("/api/sample/change")]
        public SampleChangeRes Post([FromBody] SampleChangeReq req)
        {
            SampleChangeRes res = new SampleChangeRes();
            res.success = "false";
            try
            {
                TokenCheckResult tokenCheckResult = TokenHelper.CheckToken(req.token);
                if (!tokenCheckResult.isValid)
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }
                string sql;
                SqlParameter[] param;

                sql = "UPDATE sample SET " +
                    "category=@category,classify=@classify,key_second=@key_second,key_first=@key_first,mode=@mode " +
                    "WHERE id=@id";
                param = new SqlParameter[] {
                    new SqlParameter("@id",req.id),
                    new SqlParameter("@category",req.category),
                    new SqlParameter("@classify",req.classify),
                    new SqlParameter("@key_second",req.key_second),
                    new SqlParameter("@mode",req.mode),
                    new SqlParameter("@key_first",req.key_first),
                };

                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);
                if (DataSource != 1)
                {
                    res.errorMessage = "样本信息修改失败";
                    return res;
                }
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "修改样本信息接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n");
            }
            return res;
        }

        [HttpGet]
        [Route("api/test")]
        public string test([FromForm] IFormCollection formCollection)
        {
            SampleAddRes res = new SampleAddRes();
            res.success = "false";
            res.errorMessage = "";
            //Console.WriteLine(formCollection["data"]);
            try
            {
                string r = GetHtml("https://club.jd.com/comment/skuProductPageComments.action?productId=100022195792&score=0&sortType=6&page=0&pageSize=10&isShadowSku=0&fold=1");

                JavaScriptSerializer js = new JavaScriptSerializer();//实例化一个能够序列化数据的类

                Response list = js.Deserialize<Response>(r);
                return list.comments[0].content;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "新建提案异常:" + formCollection["data"]);

                res.errorMessage = e.Message;
                return "false";
            }
        }

        private string GetHtml(string url, Encoding ed)
        {
            string Html = string.Empty;//初始化新的webRequst
            HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(url);

            Request.KeepAlive = true;
            Request.ProtocolVersion = HttpVersion.Version11;
            Request.Method = "GET";
            Request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
            Request.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/536.5 (KHTML, like Gecko) Chrome/19.0.1084.56 Safari/536.5";
            Request.Referer = url;

            HttpWebResponse htmlResponse = (HttpWebResponse)Request.GetResponse();
            //从Internet资源返回数据流
            Stream htmlStream = htmlResponse.GetResponseStream();
            //读取数据流
            StreamReader weatherStreamReader = new StreamReader(htmlStream, ed);
            //读取数据
            Html = weatherStreamReader.ReadToEnd();
            weatherStreamReader.Close();
            htmlStream.Close();
            htmlResponse.Close();
            //针对不同的网站查看html源文件
            return Html;
        }

        private string GetHtml(string url)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return GetHtml(url, Encoding.GetEncoding("GB2312"));
        }

    }

}

