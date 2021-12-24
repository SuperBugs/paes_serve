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
    public class TMController : ControllerBase
    {
        private readonly ILogger<TMController> _logger;
        public TMController(ILogger<TMController> logger)
        {
            _logger = logger;
        }
        [HttpPost]
        [Route("api/tm/chamber/add")]
        public TMChamberAddRes Post([FromBody] TMChamberAddReq req)
        {
            TMChamberAddRes res = new TMChamberAddRes();
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
                sql = "INSERT INTO ChamberTestItem " +
                      "(machine_type,test_item,test_time)" +
                      " VALUES (@machine_type,@test_item,@test_time)";
                param = new SqlParameter[] {
                     new SqlParameter("@machine_type",req.machine_type),
                     new SqlParameter("@test_item",req.test_item),
                     new SqlParameter("@test_time",Convert.ToDouble(req.test_time)),
                };
                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);
                if (DataSource != 1)
                {
                    res.errorMessage = "ChamberTestItem数据新增失败";
                    return res;
                }
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "添加ChamberTestIte接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        [HttpPost]
        [Route("api/tm/unchamber/add")]
        public TMUnChamberAddRes Post([FromBody] TMUnChamberAddReq req)
        {
            TMUnChamberAddRes res = new TMUnChamberAddRes();
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
                sql = "INSERT INTO UnChamberTestItem " +
                      "(machine_type,test_item)" +
                      " VALUES (@machine_type,@test_item)";
                param = new SqlParameter[] {
                     new SqlParameter("@machine_type",req.machine_type),
                     new SqlParameter("@test_item",req.test_item),
                };
                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);
                if (DataSource != 1)
                {
                    res.errorMessage = "UnChamberTestItem数据新增失败";
                    return res;
                }
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "UnChamberTestItem数据新增接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        [HttpPost]
        [Route("api/tm/chamber/delete")]
        public TMChamberDeleteRes Post([FromBody] TMChamberDeleteReq req)
        {
            TMChamberDeleteRes res = new TMChamberDeleteRes();
            try
            {
                res.success = "false";
                TokenCheckResult tokenCheckResult = TokenHelper.CheckToken(req.token);
                if (!(tokenCheckResult.isValid && tokenCheckResult.userType.Equals("admin")))
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }

                string sql = "DELETE FROM ChamberTestItem WHERE id=@id ";
                SqlParameter[] param = new SqlParameter[] {
                    new SqlParameter("@id",req.id),
                };
                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);
                if (DataSource != 1)
                {
                    res.errorMessage = "ChamberTestItem记录删除失败";
                    return res;
                }
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "删除ChamberTestItem记录接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        [HttpPost]
        [Route("api/tm/unchamber/delete")]
        public TMUnChamberDeleteRes Post([FromBody] TMUnChamberDeleteReq req)
        {
            TMUnChamberDeleteRes res = new TMUnChamberDeleteRes();
            try
            {
                res.success = "false";
                TokenCheckResult tokenCheckResult = TokenHelper.CheckToken(req.token);
                if (!(tokenCheckResult.isValid && tokenCheckResult.userType.Equals("admin")))
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }

                string sql = "DELETE FROM UnChamberTestItem WHERE id=@id ";
                SqlParameter[] param = new SqlParameter[] {
                    new SqlParameter("@id",req.id),
                };
                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);
                if (DataSource != 1)
                {
                    res.errorMessage = "UnChamberTestItem记录删除失败";
                    return res;
                }
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "删除UnChamberTestItem记录接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        [HttpPost]
        [Route("api/tm/chamber/change")]
        public TMChamberChangeRes Post([FromBody] TMChamberChangeReq req)
        {
            TMChamberChangeRes res = new TMChamberChangeRes();
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

                sql = "UPDATE ChamberTestItem SET " +
                    "machine_type=@machine_item,test_time=@test_time,test_item=@test_item " +
                    "WHERE id=@id";
                param = new SqlParameter[] {
                    new SqlParameter("@machine_type",req.machine_type),
                    new SqlParameter("@test_item",req.test_item),
                    new SqlParameter("@test_time",Convert.ToDouble(req.test_time)),
                    new SqlParameter("@id",req.id),
                };

                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);
                if (DataSource != 1)
                {
                    res.errorMessage = "ChamberTestItem修改失败";
                }
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "修改ChamberTestItem数据接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        [HttpPost]
        [Route("api/tm/unchamber/change")]
        public TMUnChamberChangeRes Post([FromBody] TMUnChamberChangeReq req)
        {
            TMUnChamberChangeRes res = new TMUnChamberChangeRes();
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

                sql = "UPDATE UnChamberTestItem SET " +
                    "machine_type=@machine_item,test_time=@test_time,test_item=@test_item " +
                    "WHERE id=@id";
                param = new SqlParameter[] {
                    new SqlParameter("@id",req.id),
                    new SqlParameter("@machine_type",req.machine_type),
                    new SqlParameter("@test_item",req.test_item),
                };

                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);

                // 将所有相关非结束订单都设置为Cancel

                if (DataSource != 1)
                {
                    res.errorMessage = "UnChamberTestItem修改失败";
                }
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "修改UnChamberTestItem数据接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        [HttpPost]
        [Route("api/tm/chamber/search")]
        public TMChamberRetrieveRes Post([FromBody] TMChamberRetrieveReq req)
        {
            TMChamberRetrieveRes res = new TMChamberRetrieveRes();
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
                    new SqlParameter("@test_item", req.filters[2]),
                };
                if (req.filters[0].Length != 0)
                {
                    sqlFilter = sqlFilter + " AND id=@id ";
                }

                if (req.filters[1].Length != 0)
                {
                    sqlFilter = sqlFilter + " AND machine_type=@machine_type ";
                }

                if (req.filters[2].Length != 0)
                {
                    sqlFilter = sqlFilter + " AND test_item=@test_item ";
                }

                sql = "SELECT TOP(@pageSize) * FROM ChamberTestItem WHERE 1=1" + sqlFilter +
                    " AND id NOT IN" +
                    "(SELECT TOP(@beforeSize) id FROM ChamberTestItem WHERE 1=1" + sqlFilter +
                    " ORDER BY id DESC) ORDER BY id DESC";

                var DataSource = SqlHelper.GetTableText(sql, param);
                res.data = new TMChamberData();
                TMChamberResult[] results = new TMChamberResult[DataSource[0].Rows.Count];
                int i = 0;
                foreach (DataTable table in DataSource)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        TMChamberResult result = new TMChamberResult();
                        result.id = Convert.ToDecimal(row["id"]);
                        result.machine_type = Convert.ToString(row["machine_type"]);
                        result.test_time = Convert.ToString(row["test_time"]);
                        result.test_item = Convert.ToString(row["test_item"]);
                        results[i] = result;
                        i++;
                    }
                }
                sql = "SELECT COUNT(id) FROM ChamberTestItem WHERE 1=1" + sqlFilter;
                param = new SqlParameter[] {
                    new SqlParameter("@id", req.filters[0]),
                    new SqlParameter("@machine_type", req.filters[1]),
                    new SqlParameter("@test_item", req.filters[2]),
                 };
                res.data.result = results;
                res.data.total = Convert.ToDecimal(SqlHelper.ExecuteScalar(CommandType.Text, sql, param));
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "ChamberTestItem数据接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        [HttpPost]
        [Route("api/tm/unchamber/search")]
        public TMUnChamberRetrieveRes Post([FromBody] TMUnChamberRetrieveReq req)
        {
            TMUnChamberRetrieveRes res = new TMUnChamberRetrieveRes();
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
                    new SqlParameter("@test_item", req.filters[2]),
                };
                if (req.filters[0].Length != 0)
                {
                    sqlFilter = sqlFilter + " AND id=@id ";
                }

                if (req.filters[1].Length != 0)
                {
                    sqlFilter = sqlFilter + " AND machine_type=@machine_type ";
                }

                if (req.filters[2].Length != 0)
                {
                    sqlFilter = sqlFilter + " AND test_item=@test_item ";
                }

                sql = "SELECT TOP(@pageSize) * FROM UnChamberTestItem WHERE 1=1" + sqlFilter +
                    " AND id NOT IN" +
                    "(SELECT TOP(@beforeSize) id FROM UnChamberTestItem WHERE 1=1" + sqlFilter +
                    " ORDER BY id DESC) ORDER BY id DESC";

                var DataSource = SqlHelper.GetTableText(sql, param);
                res.data = new TMUnChamberData();
                TMUnChamberResult[] results = new TMUnChamberResult[DataSource[0].Rows.Count];
                int i = 0;
                foreach (DataTable table in DataSource)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        TMUnChamberResult result = new TMUnChamberResult();
                        result.id = Convert.ToDecimal(row["id"]);
                        result.machine_type = Convert.ToString(row["machine_type"]);
                        result.test_item = Convert.ToString(row["test_item"]);
                        results[i] = result;
                        i++;
                    }
                }
                sql = "SELECT COUNT(id) FROM UnChamberTestItem WHERE 1=1" + sqlFilter;
                param = new SqlParameter[] {
                    new SqlParameter("@id", req.filters[0]),
                    new SqlParameter("@machine_type", req.filters[1]),
                    new SqlParameter("@test_item", req.filters[2]),
                 };
                res.data.result = results;
                res.data.total = Convert.ToDecimal(SqlHelper.ExecuteScalar(CommandType.Text, sql, param));
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "UnChamberTestItem数据接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }


    }
}
