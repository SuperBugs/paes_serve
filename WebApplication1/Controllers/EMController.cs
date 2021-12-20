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
    public class EMController : ControllerBase
    {
        private readonly ILogger<EMController> _logger;
        public EMController(ILogger<EMController> logger)
        {
            _logger = logger;
        }
        [HttpPost]
        [Route("api/em/chamber/add")]
        public EMChamberAddRes Post([FromBody] EMChamberAddReq req)
        {
            EMChamberAddRes res = new EMChamberAddRes();
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
                sql = "INSERT INTO Chamber " +
                      "(name,num,type,status,capacity,start_time,end_time,lab,return_staffs)" +
                      " VALUES (@name,@num,@type,@status,@capacity,@start_time,@end_time,@lab,@return_staffs)";
                param = new SqlParameter[] {
                     new SqlParameter("@name",req.name),
                     new SqlParameter("@num",req.num),
                     new SqlParameter("@type",req.type),
                     new SqlParameter("@status",req.status),
                     new SqlParameter("@capacity",req.capacity),
                     new SqlParameter("@start_time",req.start_time),
                     new SqlParameter("@end_time",req.end_time),
                     new SqlParameter("@lab",req.start_time),
                     new SqlParameter("@return_staffs",req.return_staff),
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
                res.errorMessage = "添加Chamber接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        [HttpPost]
        [Route("api/em/chamber/delete")]
        public EMChamberDeleteRes Post([FromBody] EMChamberDeleteReq req)
        {
            EMChamberDeleteRes res = new EMChamberDeleteRes();
            try
            {
                res.success = "false";
                TokenCheckResult tokenCheckResult = TokenHelper.CheckToken(req.token);
                if (!(tokenCheckResult.isValid && tokenCheckResult.userType.Equals("admin")))
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }

                string sql = "DELETE FROM Chamber WHERE id=@id ";
                SqlParameter[] param = new SqlParameter[] {
                    new SqlParameter("@id",req.id),
                };
                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);
                if (DataSource != 1)
                {
                    res.errorMessage = "Chamber设备删除失败";
                    return res;
                }
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "删除Chamber接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }


        [HttpPost]
        [Route("api/em/chamber/change")]
        public EMChamberChangeRes Post([FromBody] EMChamberChangeReq req)
        {
            EMChamberChangeRes res = new EMChamberChangeRes();
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

                sql = "UPDATE Chamber SET " +
                    "name=@name,num=@num,type=@type,status=@status,capacity=@capacity," +
                    "start_time=@start_time,end_time=@end_time,lab=@lab,return_staffs=@return_staff " +
                    "WHERE id=@id";
                param = new SqlParameter[] {
                    new SqlParameter("@id",req.id),
                    new SqlParameter("@name",req.name),
                    new SqlParameter("@num",req.num),
                    new SqlParameter("@type",req.type),
                    new SqlParameter("@status",req.status),
                    new SqlParameter("@capacity",req.capacity),
                    new SqlParameter("@start_time",CommonUtils.DateNull(req.start_time)),
                    new SqlParameter("@end_time",CommonUtils.DateNull(req.start_time)),
                    new SqlParameter("@lab",req.start_time),
                    new SqlParameter("@return_staff",req.return_staff),
                };

                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);
                if (DataSource != 1)
                {
                    res.errorMessage = "Chamber修改失败";
                }
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "修改Chamber数据接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        [HttpPost]
        [Route("api/em/chamber/search")]
        public EMChamberRetrieveRes Post([FromBody] EMChamberRetrieveReq req)
        {
            EMChamberRetrieveRes res = new EMChamberRetrieveRes();
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
                    new SqlParameter("@name", req.filters[1]),
                    new SqlParameter("@type", req.filters[2]),
                };
                if (req.filters[0].Length != 0)
                {
                    sqlFilter = sqlFilter + " AND id=@id ";
                }

                if (req.filters[1].Length != 0)
                {
                    sqlFilter = sqlFilter + " AND name=@name ";
                }

                if (req.filters[2].Length != 0)
                {
                    sqlFilter = sqlFilter + " AND type=@type ";
                }

                sql = "SELECT TOP(@pageSize) * FROM Chamber WHERE 1=1" + sqlFilter +
                    " AND id NOT IN" +
                    "(SELECT TOP(@beforeSize) id FROM Chamber WHERE 1=1" + sqlFilter +
                    " ORDER BY id) ORDER BY id";

                var DataSource = SqlHelper.GetTableText(sql, param);
                res.data = new EMChamberData();
                EMChamberResult[] results = new EMChamberResult[DataSource[0].Rows.Count];
                int i = 0;
                foreach (DataTable table in DataSource)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        EMChamberResult result = new EMChamberResult();
                        result.id = Convert.ToDecimal(row["id"]);
                        result.name = Convert.ToString(row["name"]);
                        result.num = Convert.ToString(row["num"]);
                        result.type = Convert.ToString(row["type"]);
                        result.status = Convert.ToString(row["status"]);
                        result.capacity = Convert.ToDecimal(row["capacity"]);
                        result.start_time = Convert.ToString(row["start_time"]);
                        result.end_time = Convert.ToString(row["end_time"]);
                        result.lab = Convert.ToString(row["lab"]);
                        result.return_staff = Convert.ToString(row["return_staffs"]);
                        results[i] = result;
                        i++;
                    }
                }
                sql = "SELECT COUNT(id) FROM Chamber WHERE 1=1" + sqlFilter;
                param = new SqlParameter[] {
                        new SqlParameter("@id", req.filters[0]),
                        new SqlParameter("@name", req.filters[1]),
                        new SqlParameter("@type", req.filters[2]),
                 };
                res.data.result = results;
                res.data.total = Convert.ToDecimal(SqlHelper.ExecuteScalar(CommandType.Text, sql, param));
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "Chamber数据接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }
    }
}
