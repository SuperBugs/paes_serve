using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using paems.Common;
using paems.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Exchange.WebServices.Data;

namespace paems.Controllers
{
    [ApiController]
    public class CMController : ControllerBase
    {
        private readonly ILogger<CMController> _logger;
        public CMController(ILogger<CMController> logger)
        {
            _logger = logger;
        }


        [HttpPost]
        [Route("api/cm/add")]
        public CMAddRes Post([FromBody] CMAddReq req)
        {
            CMAddRes res = new CMAddRes();
            try
            {
                res.success = "false";
                string sql;
                SqlParameter[] param;
                TokenCheckResult tokenCheckResult = TokenHelper.CheckToken(req.token);
                if (!(tokenCheckResult.isValid && tokenCheckResult.userType.Equals("admin")))
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }
                sql = "INSERT INTO CustomerType " +
                      "(type)" +
                      " VALUES (@type)";
                param = new SqlParameter[] {
                     new SqlParameter("@type",req.type),
                };
                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);
                if (DataSource != 1)
                {
                    res.errorMessage = "CustomerType数据新增失败";
                    return res;
                }
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "添加CustomerType接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        [HttpPost]
        [Route("api/cm/delete")]
        public CMDeleteRes Post([FromBody] CMDeleteReq req)
        {
            CMDeleteRes res = new CMDeleteRes();
            try
            {
                res.success = "false";
                TokenCheckResult tokenCheckResult = TokenHelper.CheckToken(req.token);
                if (!(tokenCheckResult.isValid && tokenCheckResult.userType.Equals("admin")))
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }

                string sql = "DELETE FROM CustomerType WHERE id=@id ";
                SqlParameter[] param = new SqlParameter[] {
                    new SqlParameter("@id",req.id),
                };
                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);
                if (DataSource != 1)
                {
                    res.errorMessage = "CustomerType删除失败";
                    return res;
                }
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "CustomerType删除接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }


        [HttpPost]
        [Route("api/cm/change")]
        public CMChangeRes Post([FromBody] CMChangeReq req)
        {
            CMChangeRes res = new CMChangeRes();
            try
            {
                res.success = "false";
                TokenCheckResult tokenCheckResult = TokenHelper.CheckToken(req.token);
                if (!(tokenCheckResult.isValid && tokenCheckResult.userType.Equals("admin")))
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }
                string sql;
                SqlParameter[] param;

                sql = "UPDATE CustomerType SET " +
                    "type=@type" +
                    "WHERE id=@id";
                param = new SqlParameter[] {
                    new SqlParameter("@id",req.id),
                    new SqlParameter("@type",req.type),
                };

                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);

                if (DataSource != 1)
                {
                    res.errorMessage = "CustomerType修改失败";
                }
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "修改CustomerType数据接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        [HttpPost]
        [Route("api/cm/search")]
        public CMRetrieveRes Post([FromBody] CMRetrieveReq req)
        {
            CMRetrieveRes res = new CMRetrieveRes();
            try
            {
                res.success = "false";
                TokenCheckResult tokenCheckResult = TokenHelper.CheckToken(req.token);
                if (!(tokenCheckResult.isValid && tokenCheckResult.userType.Equals("admin")))
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }
                string sql;
                string sqlFilter = "";
                SqlParameter[] param;
                param = new SqlParameter[] {
                    new SqlParameter("@pageSize",Convert.ToInt16(req.pageSize)),
                    new SqlParameter("@beforeSize",Convert.ToInt16(req.pageSize*(req.currentPage-1))),
                    new SqlParameter("@id", req.filters[0]),
                    new SqlParameter("@type", req.filters[1]),
                };
                if (req.filters[0].Length != 0)
                {
                    sqlFilter = sqlFilter + " AND id=@id ";
                }

                if (req.filters[1].Length != 0)
                {
                    sqlFilter = sqlFilter + " AND type=@type ";
                }

                sql = "SELECT TOP(@pageSize) * FROM CustomerType WHERE 1=1" + sqlFilter +
                    " AND id NOT IN" +
                    "(SELECT TOP(@beforeSize) id FROM CustomerType WHERE 1=1" + sqlFilter +
                    " ORDER BY id DESC) ORDER BY id DESC";

                var DataSource = SqlHelper.GetTableText(sql, param);
                res.data = new CMData();
                CMResult[] results = new CMResult[DataSource[0].Rows.Count];
                int i = 0;
                foreach (DataTable table in DataSource)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        CMResult result = new CMResult();
                        result.id = Convert.ToDecimal(row["id"]);
                        result.type = Convert.ToString(row["type"]);
                        results[i] = result;
                        i++;
                    }
                }
                sql = "SELECT COUNT(id) FROM CustomerType WHERE 1=1" + sqlFilter;
                param = new SqlParameter[] {
                        new SqlParameter("@id", req.filters[0]),
                        new SqlParameter("@type", req.filters[1]),
                 };
                res.data.result = results;
                res.data.total = Convert.ToDecimal(SqlHelper.ExecuteScalar(CommandType.Text, sql, param));
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "CustomerType数据接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

    }
}
