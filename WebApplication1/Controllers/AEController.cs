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
using paems.Crontab;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace paems.Controllers
{
    [ApiController]
    public class AEController : ControllerBase
    {
        private readonly ILogger<AEController> _logger;
        public AEController(ILogger<AEController> logger)
        {
            _logger = logger;
        }

        // 搜索符合条件的设备 实验室、设备名称为筛选条件,时间为预约时间
        [HttpPost]
        [Route("api/ae/unchamber/search")]
        public UnChamberSearchRes Post([FromBody] UnChamberSearchReq req)
        {
            UnChamberSearchRes res = new UnChamberSearchRes();
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
                    new SqlParameter("@lab", req.lab),
                    new SqlParameter("@deviceName", req.deviceName),
                };
                if (!req.lab.Equals(""))
                {
                    sqlFilter = sqlFilter + " AND lab=@lab ";
                }

                if (!req.deviceName.Equals(""))
                {
                    sqlFilter = sqlFilter + " AND name=@deviceName ";
                }

                sql = "SELECT TOP(@pageSize) * FROM UnChamber WHERE 1=1 " + sqlFilter +
                    " AND id NOT IN" +
                    "(SELECT TOP(@beforeSize) id FROM UnChamber WHERE 1=1 " + sqlFilter +
                    " ORDER BY id) ORDER BY id";

                var DataSource = SqlHelper.GetTableText(sql, param);
                // CommonUtils.printDataTableCollection(DataSource);
                res.data = new UnChamberSearchData();
                UnChamberSearchResult[] results = new UnChamberSearchResult[DataSource[0].Rows.Count];
                int i = 0;
                foreach (DataTable table in DataSource)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        UnChamberSearchResult result = new UnChamberSearchResult();
                        result.id = Convert.ToDecimal(row["id"]);
                        result.name = Convert.ToString(row["name"]);
                        result.type = Convert.ToString(row["type"]);
                        result.num = Convert.ToString(row["num"]);
                        result.test_type = Convert.ToString(row["test_type"]);
                        result.lend_time = Convert.ToString(row["lend_time"]);
                        result.return_time = Convert.ToString(row["return_time"]);
                        result.lab = Convert.ToString(row["lab"]);
                        result.return_staff = Convert.ToString(row["return_staff"]);
                        // 确定所选时间内的设备状态 free或者running去订单表查询状态
                        result.status = "error";
                        if (!Convert.ToString(row["status"]).Equals("error"))
                        {
                            result.status = getScaleStatus(Convert.ToDecimal(row["id"]), req.date[0], req.date[1]);
                        }

                        if (req.filters.Contains("忙碌") && result.status.Equals("running"))
                        {
                            results[i] = result;
                            i++;
                            continue;
                        }
                        if (req.filters.Contains("空闲") && result.status.Equals("free"))
                        {
                            results[i] = result;
                            i++;
                            continue;
                        }
                        if (req.filters.Contains("维修") && result.status.Equals("error"))
                        {
                            results[i] = result;
                            i++;
                            continue;
                        }
                        results[i] = result;
                        i++;
                        continue;
                    }
                }
                sql = "SELECT COUNT(id) FROM UnChamber WHERE 1=1 " + sqlFilter;
                param = new SqlParameter[] {
                    new SqlParameter("@lab", req.lab),
                    new SqlParameter("@deviceName", req.deviceName),
                };
                res.data.result = results;
                res.data.total = Convert.ToDecimal(SqlHelper.ExecuteScalar(CommandType.Text, sql, param));
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "UnChamber查询接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }
        [HttpPost]
        [Route("api/ae/chamber/search")]
        public ChamberSearchRes Post([FromBody] ChamberSearchReq req)
        {
            ChamberSearchRes res = new ChamberSearchRes();
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
                sql = "SELECT machine_type FROM ChamberTestItem WHERE test_item=@test_item";
                param = new SqlParameter[] {
                    new SqlParameter("@test_item",req.test_project),
                };
                var machine_type = "";
                foreach (DataTable table in SqlHelper.GetTableText(sql, param))
                {
                    foreach (DataRow row in table.Rows)
                    {
                        machine_type = Convert.ToString(row["machine_type"]);
                    }
                }
                sql = "SELECT TOP(@pageSize) * FROM Chamber WHERE type=@machine_type " +
                    " AND id NOT IN" +
                    "(SELECT TOP(@beforeSize) id FROM Chamber WHERE type=@machine_type " +
                    " ORDER BY id) ORDER BY id";
                param = new SqlParameter[] {
                    new SqlParameter("@pageSize",Convert.ToInt16(req.pageSize)),
                    new SqlParameter("@beforeSize",Convert.ToInt16(req.pageSize*(req.currentPage-1))),
                    new SqlParameter("@machine_type", machine_type),
                };
                var DataSource = SqlHelper.GetTableText(sql, param);
                // CommonUtils.printDataTableCollection(DataSource);
                res.data = new ChamberSearchData();
                ChamberSearchResult[] results = new ChamberSearchResult[DataSource[0].Rows.Count];
                int i = 0;
                foreach (DataTable table in DataSource)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        ChamberSearchResult result = new ChamberSearchResult();
                        result.id = Convert.ToDecimal(row["id"]);
                        result.name = Convert.ToString(row["name"]);
                        result.num = Convert.ToString(row["num"]);
                        result.lend_time = Convert.ToString(row["start_time"]);
                        result.return_time = Convert.ToString(row["end_time"]);
                        result.lab = Convert.ToString(row["lab"]);
                        result.return_staffs = Convert.ToString(row["return_staffs"]);
                        // 模拟4组测试数据
                        if (i == 0)
                        {
                            result.status = "free";
                            result.use_count = "0";
                            result.remain_count = Convert.ToString(Convert.ToDecimal(row["capacity"]) -
                            Convert.ToDecimal(result.use_count));
                            results[i] = result;
                            i++;
                            continue;
                        }
                        if (i == 1)
                        {
                            result.status = "waitting";
                            result.use_count = "4";
                            result.remain_count = Convert.ToString(Convert.ToDecimal(row["capacity"]) -
                            Convert.ToDecimal(result.use_count));
                            results[i] = result;
                            i++;
                            continue;
                        }
                        if (i == 2)
                        {
                            result.status = "error";
                            result.use_count = "0";
                            result.remain_count = Convert.ToString(Convert.ToDecimal(row["capacity"]) -
                            Convert.ToDecimal(result.use_count));
                            results[i] = result;
                            i++;
                            continue;
                        }
                        if (i == 3)
                        {
                            result.status = "running";
                            result.use_count = "10";
                            result.remain_count = Convert.ToString(Convert.ToDecimal(row["capacity"]) -
                            Convert.ToDecimal(result.use_count));
                            results[i] = result;
                            i++;
                            continue;
                        }


                        // 如果开始时间和结束时间为空则为空闲状态，否则为忙碌（已经开始或者剩余为0）或者待测、空闲

                        result.remain_count = Convert.ToString(Convert.ToDecimal(row["capacity"]) -
                        Convert.ToDecimal(result.use_count));


                        // 确定所选时间内的设备状态 free或者running去订单表查询状态
                        result.status = "error";
                        if (!Convert.ToString(row["status"]).Equals("error"))
                        {
                            result.status = getScaleStatus(Convert.ToDecimal(row["id"]), req.date[0], req.date[1]);
                        }

                        if (req.filters.Contains("忙碌") && result.status.Equals("running"))
                        {
                            results[i] = result;
                            i++;
                            continue;
                        }
                        if (req.filters.Contains("忙碌") && result.status.Equals("full"))
                        {
                            results[i] = result;
                            i++;
                            continue;
                        }
                        if (req.filters.Contains("待测") && result.status.Equals("waitting"))
                        {
                            results[i] = result;
                            i++;
                            continue;
                        }
                        if (req.filters.Contains("空闲") && result.status.Equals("free"))
                        {
                            results[i] = result;
                            i++;
                            continue;
                        }
                        if (req.filters.Contains("维修") && result.status.Equals("error"))
                        {
                            results[i] = result;
                            i++;
                            continue;
                        }
                        results[i] = result;
                        i++;
                        continue;
                    }
                }
                sql = "SELECT COUNT(id) FROM Chamber WHERE type=@machine_type";
                param = new SqlParameter[] {
                    new SqlParameter("@machine_type", machine_type),
                };
                res.data.result = results;
                res.data.total = Convert.ToDecimal(SqlHelper.ExecuteScalar(CommandType.Text, sql, param));
                res.data.total = 4;
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "UnChamber查询接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }
        private string getScaleStatus(decimal machine_id, string start_time, string end_time)
        {
            string sql = "SELECT COUNT(id) FROM UnChamberOrder WHERE status!='over' AND machine_id=@machine_id AND " +
                "((start_time<@start_time AND end_time>@start_time) OR (start_time<@end_time AND end_time>@end_time))";
            SqlParameter[] param = new SqlParameter[] {
                    new SqlParameter("@machine_id", machine_id),
                    new SqlParameter("@start_time", start_time),
                    new SqlParameter("@end_time", end_time),
            };
            if (Convert.ToDecimal(SqlHelper.ExecuteScalar(CommandType.Text, sql, param)) > 0)
            {
                return "running";
            }
            else
            {
                return "free";
            }

        }

        [HttpPost]
        [Route("api/ae/unchamber")]
        public UnChamberAERes Post([FromBody] UnChamberAEReq req)
        {
            UnChamberAERes res = new UnChamberAERes();
            try
            {
                res.success = "false";
                TokenCheckResult tokenCheckResult = TokenHelper.CheckToken(req.token);
                if (!tokenCheckResult.isValid)
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }
                string sql = "if not " +
                    "exists(SELECT * FROM UnChamberOrder WHERE status!='over' AND id=@machine_id AND " +
                    "((start_time<@start_time AND end_time>@start_time) " +
                    "OR (start_time<@end_time AND end_time>@end_time)))" +
                    " INSERT INTO UnChamberOrder " +
                    "(machine_id,staff_num,start_time,end_time,order_time,customer_type," +
                    "test_machine_type,test_stage,test_item,test_count,test_target,status)" +
                    " VALUES " +
                    "(@machine_id,@staff_num,@start_time,@end_time,@order_time,@customer_type," +
                    "@test_machine_type,@test_stage,@test_item,@test_count,@test_target,'waitting')";

                SqlParameter[] param = new SqlParameter[] {
                    new SqlParameter("@machine_id",req.id),
                    new SqlParameter("@staff_num",tokenCheckResult.userNum),
                    new SqlParameter("@start_time",req.lend_time),
                    new SqlParameter("@end_time",req.return_time),
                    new SqlParameter("@order_time",DateTime.Now),
                    new SqlParameter("@customer_type",req.customer),
                    new SqlParameter("@test_machine_type",req.test_type),
                    new SqlParameter("@test_stage",req.test_stage),
                    new SqlParameter("@test_item",req.test_program),
                    new SqlParameter("@test_count",req.test_count),
                    new SqlParameter("@test_target",req.test_target),
                };

                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);
                if (DataSource != 1)
                {
                    res.errorMessage = "预约失败";
                }
                res.success = "true";

            }
            catch (Exception e)
            {
                res.errorMessage = "Unchamber设备预约接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        // 指定设备预约数据查询
        [HttpPost]
        [Route("api/ae/unchamber/schedule")]
        public UnChamberScheduleRes Post([FromBody] UnChamberScheduleReq req)
        {
            UnChamberScheduleRes res = new UnChamberScheduleRes();
            try
            {
                res.success = "false";
                TokenCheckResult tokenCheckResult = TokenHelper.CheckToken(req.token);
                if (!tokenCheckResult.isValid)
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }

                SqlParameter[] param;
                param = new SqlParameter[] {
                    new SqlParameter("@id", req.id),
                };
                string sql = "SELECT UnChamberOrder.staff_num,UnChamberOrder.start_time,UnChamberOrder.end_time," +
                    "CompanyUser.name,CompanyUser.num,CompanyUser.phone FROM UnChamberOrder INNER JOIN CompanyUser" +
                    " ON UnChamberOrder.staff_num=CompanyUser.num AND UnChamberOrder.status='waitting'" +
                    " WHERE UnChamberOrder.machine_id=@id AND UnChamberOrder.status='waitting';";
                var DataSource = SqlHelper.GetTableText(sql, param);
                res.data = new UnChamberScheduleData();
                UnChamberScheduleResult[] results = new UnChamberScheduleResult[DataSource[0].Rows.Count];
                int i = 0;
                foreach (DataTable table in DataSource)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        UnChamberScheduleResult result = new UnChamberScheduleResult();
                        result.name = Convert.ToString(row["name"]);
                        result.num = Convert.ToString(row["staff_num"]);
                        result.phone = Convert.ToString(row["phone"]);
                        result.start_time = Convert.ToString(row["start_time"]);
                        result.end_time = Convert.ToString(row["end_time"]);
                        results[i] = result;
                        i++;
                    }
                }
                res.data.result = results;
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "指定设备预约数据查询";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        [HttpPost]
        [Route("api/ae/unchamber/query_device_name")]
        public UnChamberQueryDeviceNameRes Post([FromBody] UnChamberQueryDeviceNameReq req)
        {
            UnChamberQueryDeviceNameRes res = new UnChamberQueryDeviceNameRes();
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

                sql = "SELECT DISTINCT TOP(10) name FROM UnChamber WHERE name LIKE @name;";
                param = new SqlParameter[] {
                    new SqlParameter("@name","%"+req.query+"%"),
                };
                var DataSource = SqlHelper.GetTableText(sql, param);
                res.data = new UnChamberQueryDeviceNameData();
                UnChamberQueryDeviceNameResult[] results = new UnChamberQueryDeviceNameResult[DataSource[0].Rows.Count];
                int i = 0;
                foreach (DataTable table in DataSource)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        UnChamberQueryDeviceNameResult result = new UnChamberQueryDeviceNameResult();
                        result.name = Convert.ToString(row["name"]);
                        results[i] = result;
                        i++;
                    }
                }
                res.data.result = results;
                if (i <= 9)
                {
                    res.data.total = i;
                }
                else
                {
                    res.data.total = 10;
                }
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "查询设备名称接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        [HttpPost]
        [Route("api/ae/unchamber/query_lab")]
        public UnChamberQueryLabNameRes Post([FromBody] UnChamberQueryLabNameReq req)
        {
            UnChamberQueryLabNameRes res = new UnChamberQueryLabNameRes();
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

                sql = "SELECT DISTINCT TOP(10) lab FROM UnChamber WHERE lab LIKE @lab;";
                param = new SqlParameter[] {
                    new SqlParameter("@lab","%"+req.query+"%"),
                };
                var DataSource = SqlHelper.GetTableText(sql, param);
                res.data = new UnChamberQueryLabNameData();
                UnChamberQueryLabNameResult[] results = new UnChamberQueryLabNameResult[DataSource[0].Rows.Count];
                int i = 0;
                foreach (DataTable table in DataSource)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        UnChamberQueryLabNameResult result = new UnChamberQueryLabNameResult();
                        result.name = Convert.ToString(row["lab"]);
                        results[i] = result;
                        i++;
                    }
                }
                res.data.result = results;
                if (i <= 9)
                {
                    res.data.total = i;
                }
                else
                {
                    res.data.total = 10;
                }
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "查询实验室名称接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        // 查询设备类型测试的项目名称
        [HttpPost]
        [Route("api/ae/unchamber/query_project")]
        public UnChamberQueryProjectNameRes Post([FromBody] UnChamberQueryProjectNameReq req)
        {
            UnChamberQueryProjectNameRes res = new UnChamberQueryProjectNameRes();
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

                sql = "SELECT DISTINCT test_item FROM UnChamberTestItem WHERE machine_type=@machine_name;";
                param = new SqlParameter[] {
                    new SqlParameter("@machine_name",req.query),
                };
                var testData = SqlHelper.GetTableText(sql, param);
                res.data = new UnChamberQueryProjectNameData();
                UnChamberQueryProjectNameResult[] results = new UnChamberQueryProjectNameResult[testData[0].Rows.Count];
                int i = 0;
                foreach (DataTable table in testData)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        UnChamberQueryProjectNameResult result = new UnChamberQueryProjectNameResult();
                        result.name = Convert.ToString(row["test_item"]);
                        results[i] = result;
                        i++;
                    }
                }
                if (i == 0)
                {
                    res.success = "false";
                    res.errorMessage = "设备类型-测试项目表待维护";
                    return res;
                }
                else
                {
                    res.data.result = results;
                }
                // 获取客户类型
                sql = "SELECT DISTINCT type FROM CustomerType";
                var customerData = SqlHelper.GetTableText(sql, new SqlParameter[] { });
                UnChamberQueryCustomerResult[] customer = new UnChamberQueryCustomerResult[customerData[0].Rows.Count];
                int a = 0;
                foreach (DataTable table in customerData)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        UnChamberQueryCustomerResult result = new UnChamberQueryCustomerResult();
                        result.label = Convert.ToString(row["type"]);
                        result.value = Convert.ToString(row["type"]);
                        customer[a] = result;
                        a++;
                    }
                }
                res.data.customer = customer;
                // 获取用户负责的机型
                sql = "SELECT machine FROM UserMachine WHERE user_num=@id";
                param = new SqlParameter[] {
                    new SqlParameter("@id",tokenCheckResult.userNum),
                };
                var machineData = SqlHelper.GetTableText(sql, param);
                UnChamberQueryMachineTypeResult[] machine = new UnChamberQueryMachineTypeResult[machineData[0].Rows.Count];
                int b = 0;
                foreach (DataTable table in machineData)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        UnChamberQueryMachineTypeResult result = new UnChamberQueryMachineTypeResult();
                        result.label = Convert.ToString(row["machine"]);
                        result.value = Convert.ToString(row["machine"]);
                        machine[b] = result;
                        b++;
                    }
                }
                res.data.machine = machine;
                res.success = "true";

            }
            catch (Exception e)
            {
                res.errorMessage = "查询预约信息接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }
        // 查询chamber类型设备的素有测试项目名称
        [HttpPost]
        [Route("api/ae/chamber/query_project")]
        public ChamberQueryProjectNameRes Post([FromBody] ChamberQueryProjectNameReq req)
        {
            ChamberQueryProjectNameRes res = new ChamberQueryProjectNameRes();
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

                sql = "SELECT test_item FROM ChamberTestItem WHERE test_item LIKE @test_item;";
                param = new SqlParameter[] {
                    new SqlParameter("@test_item","%"+req.query+"%"),
                };
                var testData = SqlHelper.GetTableText(sql, param);
                res.data = new ChamberQueryProjectNameData();
                ChamberQueryProjectNameResult[] results = new ChamberQueryProjectNameResult[testData[0].Rows.Count];
                int i = 0;
                foreach (DataTable table in testData)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        ChamberQueryProjectNameResult result = new ChamberQueryProjectNameResult();
                        result.name = Convert.ToString(row["test_item"]);
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
                res.errorMessage = "Chamber设备类型-测试项目表待维护";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }
    }
}
