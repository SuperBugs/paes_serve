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
    public class CommentController : ControllerBase
    {
        private readonly ILogger<CommentController> _logger;
        public CommentController(ILogger<CommentController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Route("api/comment/add")]
        public CommentAddRes Post([FromBody] CommentAddReq req)
        {
            CommentAddRes res = new CommentAddRes();

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

                sql = "INSERT INTO comment " +
                        "(date,category,classify,issue_description,comment_type,content," +
                        "content_id,feedback_comments,model,producturl,imageurls,comment_address)" +
                        " VALUES (@date,@category,@classify,@issue_description,@comment_type," +
                        "@content,@content_id,@feedback_comments,@model,@producturl,@imageurls,@comment_address)";
                param = new SqlParameter[] {
                     new SqlParameter("@date",req.date),
                     new SqlParameter("@category",req.category),
                     new SqlParameter("@classify",req.classify),
                     new SqlParameter("@issue_description",req.issue_description),
                     new SqlParameter("@comment_type",req.comment_type),
                     new SqlParameter("@content",req.content),
                     new SqlParameter("@content_id",req.content_id),
                     new SqlParameter("@feedback_comments",req.feedback_comments),
                     new SqlParameter("@model",req.model),
                     new SqlParameter("@producturl",req.producturl),
                     new SqlParameter("@imageurls",req.imageurls),
                     new SqlParameter("@comment_address",req.comment_address),
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
                res.errorMessage = "添加数据异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        [HttpPost]
        [Route("api/comment/delete")]
        public CommentDeleteRes Post([FromBody] CommentDeleteReq req)
        {
            CommentDeleteRes res = new CommentDeleteRes();
            try
            {
                res.success = "false";
                TokenCheckResult tokenCheckResult = TokenHelper.CheckToken(req.token);
                if (!(tokenCheckResult.isValid))
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }

                string sql = "DELETE FROM comment WHERE id=@id";
                SqlParameter[] param = new SqlParameter[] {
                    new SqlParameter("@id",req.id),
                };
                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);
                if (DataSource != 1)
                {
                    res.errorMessage = "数据删除失败";
                    return res;
                }
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "删除数据接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        [HttpPost]
        [Route("api/comment/record")]
        public CommentRetrieveRes Post([FromBody] CommentRetrieveReq req)
        {
            CommentRetrieveRes res = new CommentRetrieveRes();
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
                res.data = new CommentData();
                sql = "SELECT TOP(@pageSize) * FROM comment WHERE id NOT IN" +
                        "(SELECT TOP(@beforeSize) id FROM Comment ORDER BY id DESC) ORDER BY content_id DESC";
                totalSql = "SELECT COUNT(id) FROM comment";
                param = new SqlParameter[] {
                        new SqlParameter("@pageSize",Convert.ToInt16(req.pageSize)),
                        new SqlParameter("@beforeSize",Convert.ToInt16(req.pageSize*(req.currentPage-1))),
                    };
                res.data.total = Convert.ToDecimal(SqlHelper.ExecuteScalar(CommandType.Text, totalSql));

                var DataSource = SqlHelper.GetTableText(sql, param);

                CommentResult[] results = new CommentResult[DataSource[0].Rows.Count];
                int i = 0;
                foreach (DataTable table in DataSource)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        CommentResult result = new CommentResult();
                        result.id = Convert.ToDecimal(row["id"]);
                        result.category = Convert.ToString(row["category"]);
                        result.classify = Convert.ToString(row["classify"]);
                        result.date = Convert.ToString(row["date"]);
                        result.issue_description = Convert.ToString(row["issue_description"]);
                        result.comment_type = Convert.ToString(row["comment_type"]);
                        result.content = Convert.ToString(row["content"]);
                        result.content_id = Convert.ToString(row["content_id"]);
                        result.feedback_comments = Convert.ToString(row["feedback_comments"]);
                        result.model = Convert.ToString(row["model"]);
                        result.producturl = Convert.ToString(row["producturl"]);
                        result.imageurls = Convert.ToString(row["imageurls"]);
                        result.comment_address = Convert.ToString(row["comment_address"]);
                        results[i] = result;
                        i++;
                    }
                }
                res.data.result = results;
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "数据记录接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        [HttpPost]
        [Route("/api/comment/change")]
        public CommentChangeRes Post([FromBody] CommentChangeReq req)
        {
            CommentChangeRes res = new CommentChangeRes();
            res.success = "false";
            try
            {
                TokenCheckResult tokenCheckResult = TokenHelper.CheckToken(req.token);
                if (!tokenCheckResult.isValid || tokenCheckResult.userType.Equals("admin"))
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }
                string sql;
                SqlParameter[] param;

                sql = "UPDATE comment SET " +
                    "date=@date,category=@category,classify=@classify," +
                    "issue_description=@issue_description,comment_type=@comment_type," +
                    "content=@content,content_id=@content_id,feedback_comments=@feedback_comments," +
                    "model=@model,producturl=@producturl,imageurls=@imageurls," +
                    "comment_address=@comment_address WHERE id=@id";
                param = new SqlParameter[] {
                    new SqlParameter("@id",req.id),
                    new SqlParameter("@date",req.date),
                    new SqlParameter("@category",req.category),
                    new SqlParameter("@classify",req.classify),
                    new SqlParameter("@issue_description",req.issue_description),
                    new SqlParameter("@comment_type",req.comment_type),
                    new SqlParameter("@content",req.content),
                    new SqlParameter("@content_id",req.content_id),
                    new SqlParameter("@feedback_comments",req.feedback_comments),
                    new SqlParameter("@model",req.model),
                    new SqlParameter("@producturl",req.producturl),
                    new SqlParameter("@imageurls",req.imageurls),
                    new SqlParameter("@comment_address",req.comment_address),
                };

                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);
                if (DataSource != 1)
                {
                    res.errorMessage = "评论数据修改失败";
                    return res;
                }
                res.success = "true";
            }
            catch (Exception e) 
            {
                res.errorMessage = "修改评论数据接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n");
            }
            return res;

        }

    }

}

