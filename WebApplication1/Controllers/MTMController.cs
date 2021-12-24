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
    public class MTMController : ControllerBase
    {
        private readonly ILogger<MTMController> _logger;
        public MTMController(ILogger<MTMController> logger)
        {
            _logger = logger;
        }


        [HttpPost]
        [Route("api/mtm/add")]
        public MTMAddRes Post([FromBody] MTMAddReq req)
        {
            MTMAddRes res = new MTMAddRes();
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
                sql = "INSERT INTO MachineType " +
                      "(machine_type)" +
                      " VALUES (@machine_type)";
                param = new SqlParameter[] {
                     new SqlParameter("@machine_type",req.machine_type),
                };
                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);
                if (DataSource != 1)
                {
                    res.errorMessage = "MachineType数据新增失败";
                    return res;
                }
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "添加MachineType接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        [HttpPost]
        [Route("api/mtm/delete")]
        public MTMDeleteRes Post([FromBody] MTMDeleteReq req)
        {
            MTMDeleteRes res = new MTMDeleteRes();
            try
            {
                res.success = "false";
                TokenCheckResult tokenCheckResult = TokenHelper.CheckToken(req.token);
                if (!(tokenCheckResult.isValid && tokenCheckResult.userType.Equals("admin")))
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }

                string sql = "DELETE FROM MachineType WHERE id=@id ";
                SqlParameter[] param = new SqlParameter[] {
                    new SqlParameter("@id",req.id),
                };
                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);
                if (DataSource != 1)
                {
                    res.errorMessage = "MachineType删除失败";
                    return res;
                }
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "MachineType删除接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }


        [HttpPost]
        [Route("api/mtm/change")]
        public MTMChangeRes Post([FromBody] MTMChangeReq req)
        {
            MTMChangeRes res = new MTMChangeRes();
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

                sql = "UPDATE MachineType SET " +
                    "machine_type=@machine_type" +
                    "WHERE id=@id";
                param = new SqlParameter[] {
                    new SqlParameter("@id",req.id),
                    new SqlParameter("@machine_type",req.machine_type),
                };

                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);

                if (DataSource != 1)
                {
                    res.errorMessage = "MachineType修改失败";
                }
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "修改MachineType数据接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        [HttpPost]
        [Route("api/mtm/search")]
        public MTMRetrieveRes Post([FromBody] MTMRetrieveReq req)
        {
            MTMRetrieveRes res = new MTMRetrieveRes();
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
                    new SqlParameter("@machine_type", req.filters[1]),
                };
                if (req.filters[0].Length != 0)
                {
                    sqlFilter = sqlFilter + " AND id=@id ";
                }

                if (req.filters[1].Length != 0)
                {
                    sqlFilter = sqlFilter + " AND machine_type=@machine_type ";
                }

                sql = "SELECT TOP(@pageSize) * FROM MachineType WHERE 1=1" + sqlFilter +
                    " AND id NOT IN" +
                    "(SELECT TOP(@beforeSize) id FROM MachineType WHERE 1=1" + sqlFilter +
                    " ORDER BY id DESC) ORDER BY id DESC";

                var DataSource = SqlHelper.GetTableText(sql, param);
                res.data = new MTMData();
                MTMResult[] results = new MTMResult[DataSource[0].Rows.Count];
                int i = 0;
                foreach (DataTable table in DataSource)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        MTMResult result = new MTMResult();
                        result.id = Convert.ToDecimal(row["id"]);
                        result.machine_type = Convert.ToString(row["machine_type"]);
                        results[i] = result;
                        i++;
                    }
                }
                sql = "SELECT COUNT(id) FROM MachineType WHERE 1=1" + sqlFilter;
                param = new SqlParameter[] {
                        new SqlParameter("@id", req.filters[0]),
                        new SqlParameter("@machine_type", req.filters[1]),
                 };
                res.data.result = results;
                res.data.total = Convert.ToDecimal(SqlHelper.ExecuteScalar(CommandType.Text, sql, param));
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "MachineType数据接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

    }
}
