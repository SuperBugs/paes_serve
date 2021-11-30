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
    public class ConfigMangmentController : ControllerBase
    {
        private readonly ILogger<ConfigMangmentController> _logger;
        public ConfigMangmentController(ILogger<ConfigMangmentController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Route("api/config/add")]
        public ConfigAddRes Post([FromBody] ConfigAddReq req)
        {
            ConfigAddRes res = new ConfigAddRes();

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

                sql = "SELECT * FROM init_config WHERE url=@url";
                param = new SqlParameter[] {
                      new SqlParameter("@url",req.url),
                };
                count = Convert.ToDecimal(SqlHelper.ExecuteScalar(CommandType.Text, sql, param));
                if (count >= 1)
                {
                    res.errorMessage = "链接已存在";
                    return res;
                }

                sql = "INSERT INTO init_config " +
                        "(name,url,start_time,end_time,is_run)" +
                        " VALUES (@name,@url,@start_time,@end_time,@is_run)";
                param = new SqlParameter[] {
                     new SqlParameter("@name",req.name),
                     new SqlParameter("@url",req.url),
                     new SqlParameter("@start_time",req.start_time),
                     new SqlParameter("@end_time",req.end_time),
                     new SqlParameter("@is_run",req.is_run),
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
        [Route("api/config/delete")]
        public ConfigDeleteRes Post([FromBody] ConfigDeleteReq req)
        {
            ConfigDeleteRes res = new ConfigDeleteRes();
            try
            {
                res.success = "false";
                TokenCheckResult tokenCheckResult = TokenHelper.CheckToken(req.token);
                if (!(tokenCheckResult.isValid))
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }

                string sql = "DELETE FROM init_config WHERE id=@id";
                SqlParameter[] param = new SqlParameter[] {
                    new SqlParameter("@id",req.id),
                };
                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);
                if (DataSource != 1)
                {
                    res.errorMessage = "配置删除失败";
                    return res;
                }
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "删除配置接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        [HttpPost]
        [Route("api/config/record")]
        public ConfigRetrieveRes Post([FromBody] ConfigRetrieveReq req)
        {
            ConfigRetrieveRes res = new ConfigRetrieveRes();
            try
            {
                res.success = "false";
                TokenCheckResult tokenCheckResult = TokenHelper.CheckToken(req.token);
                if (!tokenCheckResult.isValid || tokenCheckResult.userType.Equals("User"))
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }
                string sql;
                string totalSql;
                SqlParameter[] param;
                res.data = new ConfigData();
                sql = "SELECT TOP(@pageSize) * FROM init_config WHERE id NOT IN" +
                        "(SELECT TOP(@beforeSize) id FROM init_config ORDER BY id DESC) ORDER BY id DESC";
                totalSql = "SELECT COUNT(id) FROM init_config";
                param = new SqlParameter[] {
                        new SqlParameter("@pageSize",Convert.ToInt16(req.pageSize)),
                        new SqlParameter("@beforeSize",Convert.ToInt16(req.pageSize*(req.currentPage-1))),
                    };
                res.data.total = Convert.ToDecimal(SqlHelper.ExecuteScalar(CommandType.Text, totalSql));

                var DataSource = SqlHelper.GetTableText(sql, param);

                ConfigResult[] results = new ConfigResult[DataSource[0].Rows.Count];
                int i = 0;
                foreach (DataTable table in DataSource)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        ConfigResult result = new ConfigResult();
                        result.id = Convert.ToDecimal(row["id"]);
                        result.name = Convert.ToString(row["name"]);
                        result.url = Convert.ToString(row["url"]);
                        result.start_time = Convert.ToString(row["start_time"]);
                        result.end_time = Convert.ToString(row["end_time"]);
                        result.is_run = Convert.ToString(row["is_run"]);
                        results[i] = result;
                        i++;
                    }
                }
                res.data.result = results;
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "配置记录接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        [HttpPost]
        [Route("/api/config/change")]
        public ConfigChangeRes Post([FromBody] ConfigChangeReq req)
        {
            ConfigChangeRes res = new ConfigChangeRes();
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

                sql = "UPDATE init_config SET " +
                    "name=@name," +
                    "url=@url,start_time=@start_time,end_time=@end_time,is_run=@is_run " +
                    "WHERE id=@id";
                param = new SqlParameter[] {
                    new SqlParameter("@id",req.id),
                    new SqlParameter("@name",req.name),
                     new SqlParameter("@url",req.url),
                     new SqlParameter("@start_time",req.start_time),
                     new SqlParameter("@end_time",req.end_time),
                     new SqlParameter("@is_run",req.is_run),
                };

                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);
                if (DataSource != 1)
                {
                    res.errorMessage = "配置信息修改失败";
                    return res;
                }
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "修改配置信息接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n");
            }
            return res;
        }

    }

}

