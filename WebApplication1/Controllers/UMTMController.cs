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
    public class UMTMController : ControllerBase
    {
        private readonly ILogger<UMTMController> _logger;
        public UMTMController(ILogger<UMTMController> logger)
        {
            _logger = logger;
        }


        [HttpPost]
        [Route("api/umtm/add")]
        public UMTMAddRes Post([FromBody] UMTMAddReq req)
        {
            UMTMAddRes res = new UMTMAddRes();
            try
            {
                res.success = "false";
                string sql;
                SqlParameter[] param;
                TokenCheckResult tokenCheckResult = TokenHelper.CheckToken(req.token);
                if (!tokenCheckResult.isValid)
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }

                sql = "SELECT * FROM UserMachine WHERE user_num=@user_num AND machine=@machine_type";
                param = new SqlParameter[] {
                      new SqlParameter("@user_num",tokenCheckResult.userNum),
                      new SqlParameter("@machine_type",req.machine_type),
                };
                var count = Convert.ToDecimal(SqlHelper.ExecuteScalar(CommandType.Text, sql, param));
                if (count >= 1)
                {
                    res.errorMessage = "测试机型已存在";
                    return res;
                }

                sql = "INSERT INTO UserMachine " +
                      "(machine,user_num)" +
                      " VALUES (@machine_type,@user_num)";
                param = new SqlParameter[] {
                     new SqlParameter("@machine_type",req.machine_type),
                     new SqlParameter("@user_num",tokenCheckResult.userNum),
                };
                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);
                if (DataSource != 1)
                {
                    res.errorMessage = "UserMachine数据新增失败";
                    return res;
                }
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "添加UserMachine接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        [HttpPost]
        [Route("api/umtm/delete")]
        public UMTMDeleteRes Post([FromBody] UMTMDeleteReq req)
        {
            UMTMDeleteRes res = new UMTMDeleteRes();
            try
            {
                res.success = "false";
                TokenCheckResult tokenCheckResult = TokenHelper.CheckToken(req.token);
                if (!tokenCheckResult.isValid)
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }

                string sql = "DELETE FROM UserMachine WHERE id=@id ";
                SqlParameter[] param = new SqlParameter[] {
                    new SqlParameter("@id",req.id),
                };
                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);
                if (DataSource != 1)
                {
                    res.errorMessage = "UserMachine删除失败";
                    return res;
                }
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "UserMachine删除接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        [HttpPost]
        [Route("api/umtm/search")]
        public UMTMRetrieveRes Post([FromBody] UMTMRetrieveReq req)
        {
            UMTMRetrieveRes res = new UMTMRetrieveRes();
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
                    sqlFilter = sqlFilter + " AND machine=@machine_type ";
                }

                sql = "SELECT TOP(@pageSize) * FROM UserMachine WHERE 1=1" + sqlFilter +
                    " AND id NOT IN" +
                    "(SELECT TOP(@beforeSize) id FROM UserMachine WHERE 1=1" + sqlFilter +
                    " ORDER BY id DESC) ORDER BY id DESC";

                var DataSource = SqlHelper.GetTableText(sql, param);
                res.data = new UMTMData();
                UMTMResult[] results = new UMTMResult[DataSource[0].Rows.Count];
                int i = 0;
                foreach (DataTable table in DataSource)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        UMTMResult result = new UMTMResult();
                        result.id = Convert.ToDecimal(row["id"]);
                        result.machine_type = Convert.ToString(row["machine"]);
                        results[i] = result;
                        i++;
                    }
                }
                sql = "SELECT COUNT(id) FROM UserMachine WHERE 1=1" + sqlFilter;
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
                res.errorMessage = "UserMachine数据接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        // 测试机型
        [HttpPost]
        [Route("api/umtm/test_machine_type")]
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

                sql = "SELECT DISTINCT TOP(10) machine_type FROM MachineType WHERE machine_type LIKE @machine_type;";
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
                res.errorMessage = "测试机型查询异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

    }
}
