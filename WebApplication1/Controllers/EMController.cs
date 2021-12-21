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
                      "(name,num,type,status,capacity,lab)" +
                      " VALUES (@name,@num,@type,@status,@capacity,@lab)";
                param = new SqlParameter[] {
                     new SqlParameter("@name",req.name),
                     new SqlParameter("@num",req.num),
                     new SqlParameter("@type",req.type),
                     new SqlParameter("@status","free"),
                     new SqlParameter("@capacity",req.capacity),
                     new SqlParameter("@lab",req.lab),
                };
                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);
                if (DataSource != 1)
                {
                    res.errorMessage = "Chamber数据新增失败";
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
        [Route("api/em/unchamber/add")]
        public EMUnChamberAddRes Post([FromBody] EMUnChamberAddReq req)
        {
            EMUnChamberAddRes res = new EMUnChamberAddRes();
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
                sql = "INSERT INTO UnChamber " +
                      "(name,num,type,status,lab)" +
                      " VALUES (@name,@num,@type,@status,@lab)";
                param = new SqlParameter[] {
                     new SqlParameter("@name",req.name),
                     new SqlParameter("@num",req.num),
                     new SqlParameter("@type",req.type),
                     new SqlParameter("@status","free"),
                     new SqlParameter("@lab",req.lab),
                };
                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);
                if (DataSource != 1)
                {
                    res.errorMessage = "UnChamber数据新增失败";
                    return res;
                }
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "添加UnChamber接口异常";
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
        [Route("api/em/unchamber/delete")]
        public EMUnChamberDeleteRes Post([FromBody] EMUnChamberDeleteReq req)
        {
            EMUnChamberDeleteRes res = new EMUnChamberDeleteRes();
            try
            {
                res.success = "false";
                TokenCheckResult tokenCheckResult = TokenHelper.CheckToken(req.token);
                if (!(tokenCheckResult.isValid && tokenCheckResult.userType.Equals("admin")))
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }

                string sql = "DELETE FROM UnChamber WHERE id=@id ";
                SqlParameter[] param = new SqlParameter[] {
                    new SqlParameter("@id",req.id),
                };
                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);
                if (DataSource != 1)
                {
                    res.errorMessage = "UnChamber设备删除失败";
                    return res;
                }
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "删除UnChamber接口异常";
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
                    new SqlParameter("@lab",req.lab),
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
        [Route("api/em/unchamber/change")]
        public EMUnChamberChangeRes Post([FromBody] EMUnChamberChangeReq req)
        {
            EMUnChamberChangeRes res = new EMUnChamberChangeRes();
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

                sql = "UPDATE UnChamber SET " +
                    "name=@name,num=@num,type=@type,status=@status,test_type=@test_type," +
                    "lend_time=@start_time,return_time=@end_time,lab=@lab,return_staff=@return_staff " +
                    "WHERE id=@id";
                param = new SqlParameter[] {
                    new SqlParameter("@id",req.id),
                    new SqlParameter("@name",req.name),
                    new SqlParameter("@num",req.num),
                    new SqlParameter("@type",req.type),
                    new SqlParameter("@status",req.status),
                    new SqlParameter("@test_type",CommonUtils.StringNull(req.type)),
                    new SqlParameter("@start_time",CommonUtils.DateNull(req.start_time)),
                    new SqlParameter("@end_time",CommonUtils.DateNull(req.start_time)),
                    new SqlParameter("@lab",req.lab),
                    new SqlParameter("@return_staff",req.return_staff),
                };

                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);

                // 将所有相关非结束订单都设置为Cancel

                if (DataSource != 1)
                {
                    res.errorMessage = "UnChamber修改失败";
                }
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "修改UnChamber数据接口异常";
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
                    " ORDER BY id DESC) ORDER BY id DESC";

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

        [HttpPost]
        [Route("api/em/unchamber/search")]
        public EMUnChamberRetrieveRes Post([FromBody] EMUnChamberRetrieveReq req)
        {
            EMUnChamberRetrieveRes res = new EMUnChamberRetrieveRes();
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

                sql = "SELECT TOP(@pageSize) * FROM UnChamber WHERE 1=1" + sqlFilter +
                    " AND id NOT IN" +
                    "(SELECT TOP(@beforeSize) id FROM UnChamber WHERE 1=1" + sqlFilter +
                    " ORDER BY id DESC) ORDER BY id DESC";

                var DataSource = SqlHelper.GetTableText(sql, param);
                res.data = new EMUnChamberData();
                EMUnChamberResult[] results = new EMUnChamberResult[DataSource[0].Rows.Count];
                int i = 0;
                foreach (DataTable table in DataSource)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        EMUnChamberResult result = new EMUnChamberResult();
                        result.id = Convert.ToDecimal(row["id"]);
                        result.name = Convert.ToString(row["name"]);
                        result.num = Convert.ToString(row["num"]);
                        result.type = Convert.ToString(row["type"]);
                        result.status = Convert.ToString(row["status"]);
                        result.start_time = Convert.ToString(row["lend_time"]);
                        result.end_time = Convert.ToString(row["return_time"]);
                        result.lab = Convert.ToString(row["lab"]);
                        result.return_staff = Convert.ToString(row["return_staff"]);
                        results[i] = result;
                        i++;
                    }
                }
                sql = "SELECT COUNT(id) FROM UnChamber WHERE 1=1" + sqlFilter;
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
                res.errorMessage = "UnChamber数据接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        // 查询chamber类型设备的有测试项目名称
        [HttpPost]
        [Route("api/em/chamber/machine_type")]
        public QueryChamberMachineTypeRes Post([FromBody] QueryChambeMachineTypeReq req)
        {
            QueryChamberMachineTypeRes res = new QueryChamberMachineTypeRes();
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
                SqlParameter[] param;

                sql = "SELECT DISTINCT TOP(10) machine_type FROM ChamberTestItem WHERE machine_type LIKE @machine_type;";
                param = new SqlParameter[] {
                    new SqlParameter("@machine_type","%"+req.query+"%"),
                };
                var testData = SqlHelper.GetTableText(sql, param);
                res.data = new QueryChamberMachineTypeData();
                QueryChamberMachineTypeResult[] results = new QueryChamberMachineTypeResult[testData[0].Rows.Count];
                int i = 0;
                foreach (DataTable table in testData)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        QueryChamberMachineTypeResult result = new QueryChamberMachineTypeResult();
                        result.name = Convert.ToString(row["machine_type"]);
                        results[i] = result;
                        i++;
                    }
                }
                res.data.total = i;
                res.data.result = results;
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "Chamber类型查询异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        // 查询unchamber类型设备的有测试项目名称
        [HttpPost]
        [Route("api/em/unchamber/machine_type")]
        public QueryUnChamberMachineTypeRes Post([FromBody] QueryUnChamberMachineTypeReq req)
        {
            QueryUnChamberMachineTypeRes res = new QueryUnChamberMachineTypeRes();
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
                SqlParameter[] param;

                sql = "SELECT DISTINCT TOP(10) machine_type FROM UnChamberTestItem WHERE machine_type LIKE @machine_type;";
                param = new SqlParameter[] {
                    new SqlParameter("@machine_type","%"+req.query+"%"),
                };
                var testData = SqlHelper.GetTableText(sql, param);
                res.data = new QueryUnChamberMachineTypeData();
                QueryUnChamberMachineTypeResult[] results = new QueryUnChamberMachineTypeResult[testData[0].Rows.Count];
                int i = 0;
                foreach (DataTable table in testData)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        QueryUnChamberMachineTypeResult result = new QueryUnChamberMachineTypeResult();
                        result.name = Convert.ToString(row["machine_type"]);
                        results[i] = result;
                        i++;
                    }
                }
                res.data.total = i;
                res.data.result = results;
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "UnChamber类型查询异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }
    }
}
