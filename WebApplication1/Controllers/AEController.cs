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
                if (Convert.ToDateTime(req.date[0]).AddMinutes(10) < DateTime.Now)
                {
                    res.errorMessage = "开始时间小于当前时间";
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

                sql = "SELECT * FROM UnChamber WHERE 1=1 " + sqlFilter;
                var DataSource = SqlHelper.GetTableText(sql, param);
                res.data = new UnChamberSearchData();
                List<UnChamberSearchResult> data = new List<UnChamberSearchResult>();

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
                        result.status = "error"; ;
                        if (!Convert.ToString(row["status"]).Equals("error"))
                        {
                            result.status = getScaleStatus(Convert.ToDecimal(row["id"]), req.date[0], req.date[1]);
                        }
                        if (result.status.Equals("error") || result.status.Equals("free"))
                        {
                            result.lend_time = null;
                            result.return_time = null;
                        }
                        if (req.filters.Length == 3 && req.filters[0].Equals("") &&
                            req.filters[1].Equals("") && req.filters[2].Equals("") || req.filters[0].Equals("all"))
                        {
                            data.Add(result);
                            i++;
                            continue;
                        }
                        if (req.filters.Contains("running") && result.status.Equals("running"))
                        {
                            data.Add(result);
                            i++;
                            continue;
                        }
                        if (req.filters.Contains("free") && result.status.Equals("free"))
                        {
                            data.Add(result);
                            i++;
                            continue;
                        }
                        if (req.filters.Contains("error") && result.status.Equals("error"))
                        {
                            data.Add(result);
                            i++;
                            continue;
                        }
                    }
                }
                int start = 0;
                req.currentPage--;

                decimal count = 0;
                if (data.Count - req.pageSize * req.currentPage >= req.pageSize)
                {
                    count = req.pageSize;
                }
                else
                {
                    count = data.Count - req.pageSize * req.currentPage;
                }
                UnChamberSearchResult[] results = new UnChamberSearchResult[Convert.ToInt32(count)];
                for (decimal size = req.pageSize * req.currentPage; start < count; start++)
                {
                    results[start] = data[Convert.ToInt32(size + start)];
                }
                res.data.result = results;
                res.data.total = data.Count();
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
            string sql = "SELECT COUNT(id) FROM UnChamberOrder WHERE status!='over' AND status!='cancel' AND machine_id=@machine_id AND " +
                "((start_time<@start_time AND end_time>@start_time) " +
                "OR (start_time<@end_time AND end_time>@end_time) " +
                "OR (start_time>@start_time AND end_time<@end_time))";
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
        // chamber设备暂时不提供过滤功能，指定类型的Chamber类型较少
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
                if (Convert.ToDateTime(req.date[0]).AddMinutes(10) < DateTime.Now)
                {
                    res.errorMessage = "开始时间小于当前时间";
                    return res;
                }
                string sql;
                SqlParameter[] param;
                sql = "SELECT * FROM ChamberTestItem WHERE test_item=@test_item";
                param = new SqlParameter[] {
                    new SqlParameter("@test_item",req.test_project),
                };
                var machine_type = "";
                double run_time = 0.00;
                foreach (DataTable table in SqlHelper.GetTableText(sql, param))
                {
                    foreach (DataRow row in table.Rows)
                    {
                        machine_type = Convert.ToString(row["machine_type"]);
                        run_time = Convert.ToDouble(row["test_time"]);
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
                        result.lab = Convert.ToString(row["lab"]);
                        result.return_staffs = Convert.ToString(row["return_staffs"]);
                        result.status = "free";
                        if (Convert.ToString(row["status"]).Equals("error"))
                        {
                            result.use_count = "0";
                            result.status = "error";
                            result.remain_count = Convert.ToString(Convert.ToDecimal(row["capacity"]));
                            results[i] = result;
                            i++;
                            continue;
                        }
                        // 查询符合拼测的订单
                        sql = "SELECT * FROM ChamberTimeOrder WHERE status='waitting' " +
                            "AND machine_id=@machine_id " +
                            "AND test_item=@test_item " +
                            "AND ((@end_time > start_time AND @end_time < end_time)" +
                            "OR (@start_time > start_time AND @start_time < end_time)" +
                            "OR (@start_time < start_time AND @end_time > end_time))";
                        param = new SqlParameter[] {
                            new SqlParameter("@machine_id", Convert.ToString(row["id"])),
                            new SqlParameter("@start_time", req.date[0]),
                            new SqlParameter("@end_time", req.date[1]),
                            new SqlParameter("@test_item",req.test_project),
                        };
                        var order = SqlHelper.GetTableText(sql, param);

                        foreach (DataTable t in order)
                        {
                            foreach (DataRow r in t.Rows)
                            {
                                if (Convert.ToDecimal(r["remain_count"]) >= req.test_count)
                                {
                                    result.status = "waitting";
                                }
                                else
                                {
                                    result.status = "running";
                                }
                                result.lend_time = Convert.ToString(r["start_time"]);
                                result.return_time = Convert.ToString(r["end_time"]);
                                result.use_count = Convert.ToString(Convert.ToDecimal(row["capacity"]) - Convert.ToDecimal(r["remain_count"]));
                                result.remain_count = Convert.ToDecimal(r["remain_count"]) + "";
                                results[i] = result;
                                i++;
                                break;
                            }
                        }
                        if (result.status.Equals("waitting") || result.status.Equals("running"))
                        {
                            continue;
                        }
                        // 存在则为忙碌，不存在则为空闲
                        sql = "SELECT * FROM ChamberTimeOrder WHERE status!='over' AND status!='cancel'" +
                            "AND machine_id=@machine_id " +
                            "AND ((@end_time > start_time AND @end_time < end_time)" +
                            "OR (@start_time > start_time AND @start_time < end_time)" +
                            "OR (@start_time < start_time AND @end_time > end_time))";
                        param = new SqlParameter[] {
                            new SqlParameter("@machine_id", Convert.ToString(row["id"])),
                            new SqlParameter("@start_time", Convert.ToDateTime(req.date[0])),
                            new SqlParameter("@end_time", Convert.ToDateTime(req.date[1])),
                        };

                        var orders = SqlHelper.GetTableText(sql, param);

                        foreach (DataTable t in orders)
                        {
                            foreach (DataRow r in t.Rows)
                            {
                                result.status = "running";
                                result.lend_time = Convert.ToString(r["start_time"]);
                                result.return_time = Convert.ToString(r["end_time"]);
                                result.use_count = Convert.ToDecimal(row["capacity"]) - Convert.ToDecimal(r["remain_count"]) + "";
                                result.remain_count = Convert.ToDecimal(r["remain_count"]) + "";
                                results[i] = result;
                                i++;
                                break;
                            }
                        }
                        if (result.status.Equals("running"))
                        {
                            continue;
                        }
                        if (result.status.Equals("free") && Convert.ToDecimal(row["capacity"]) >= req.test_count)
                        {
                            result.lend_time = "";
                            result.return_time = "";
                            result.status = "free";
                            result.use_count = "0";
                            result.remain_count = Convert.ToString(Convert.ToDecimal(row["capacity"]));
                            results[i] = result;
                            i++;
                        }
                        else
                        {
                            result.status = "running";
                            result.lend_time = "";
                            result.return_time = "";
                            result.use_count = "0";
                            result.remain_count = "容量不够";
                            results[i] = result;
                            i++;
                        }
                    }
                }
                sql = "SELECT COUNT(id) FROM Chamber WHERE type=@machine_type";
                param = new SqlParameter[] {
                    new SqlParameter("@machine_type", machine_type),
                };
                res.data.result = results;
                res.data.total = Convert.ToDecimal(SqlHelper.ExecuteScalar(CommandType.Text, sql, param));
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "Chamber查询接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        //非chamber设备预约
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
                if (Convert.ToDateTime(req.lend_time).AddMinutes(10) < DateTime.Now)
                {
                    res.errorMessage = "开始时间小于当前时间";
                    return res;
                }
                string sql = "if not " +
                    "exists(SELECT * FROM UnChamberOrder WHERE status!='over' AND status!='cancel' AND id=@machine_id AND " +
                    "((start_time<@start_time AND end_time>@start_time) " +
                    "OR (start_time<@end_time AND end_time<@end_time) " +
                    "OR (start_time>@start_time AND end_time<@end_time)))" +
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
                    new SqlParameter("@test_target",CommonUtils.StringNull(req.test_target)),
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

        //chamber设备预约
        [HttpPost]
        [Route("api/ae/chamber")]
        public ChamberAERes Post([FromBody] ChamberAEReq req)
        {
            ChamberAERes res = new ChamberAERes();
            try
            {
                res.success = "false";
                TokenCheckResult tokenCheckResult = TokenHelper.CheckToken(req.token);
                if (!tokenCheckResult.isValid)
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }
                if (Convert.ToDateTime(req.start_time).AddMinutes(10) < DateTime.Now)
                {
                    res.errorMessage = "开始时间小于当前时间";
                    return res;
                }
                string sql;
                SqlParameter[] param;
                sql = "SELECT * FROM ChamberTestItem WHERE test_item=@test_item";
                param = new SqlParameter[] {
                    new SqlParameter("@test_item",req.test_program),
                };
                double run_time = 0.00;
                foreach (DataTable table in SqlHelper.GetTableText(sql, param))
                {
                    foreach (DataRow row in table.Rows)
                    {
                        run_time = Convert.ToDouble(row["test_time"]);
                    }
                }
                //拼测
                if (req.order_type.Equals("group"))
                {
                    param = new SqlParameter[] {
                        new SqlParameter("@machine_id",req.id),
                        new SqlParameter("@start_time",Convert.ToDateTime(req.start_time)),
                        new SqlParameter("@end_time",Convert.ToDateTime(req.end_time)),
                        new SqlParameter("@staff_num",tokenCheckResult.userNum),
                        new SqlParameter("@order_time",DateTime.Now),
                        new SqlParameter("@customer_type",req.customer),
                        new SqlParameter("@test_machine_type",req.test_type),
                        new SqlParameter("@test_stage",req.test_stage),
                        new SqlParameter("@test_item",req.test_program),
                        new SqlParameter("@test_count",req.test_count),
                        new SqlParameter("@test_target",CommonUtils.StringNull(req.test_target)),
                    };
                    var rows = SqlHelper.ExecteNonQueryProducts("GroupOrderChamber", param);
                    if (rows == -1)
                    {
                        res.errorMessage = "预约失败";
                    }
                    else
                    {
                        res.success = "true";
                    }

                }
                // 预定
                if (req.order_type.Equals("new"))
                {
                    param = new SqlParameter[] {
                        new SqlParameter("@machine_id",req.id),
                        new SqlParameter("@start_time",Convert.ToDateTime(req.start_time)),
                        new SqlParameter("@end_time",Convert.ToDateTime(req.end_time)),
                        new SqlParameter("@staff_num",tokenCheckResult.userNum),
                        new SqlParameter("@order_time",DateTime.Now),
                        new SqlParameter("@customer_type",req.customer),
                        new SqlParameter("@test_machine_type",req.test_type),
                        new SqlParameter("@test_stage",req.test_stage),
                        new SqlParameter("@test_item",req.test_program),
                        new SqlParameter("@test_count",req.test_count),
                        new SqlParameter("@test_target",CommonUtils.StringNull(req.test_target)),
                    };
                    var rows = SqlHelper.ExecteNonQueryProducts("NewOrderChamber", param);
                    if (rows == -1)
                    {
                        res.errorMessage = "预约失败";
                    }
                    else
                    {
                        res.success = "true";
                    }
                }
            }
            catch (Exception e)
            {
                res.errorMessage = "Chamber设备预约接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        // 指定UnChamber设备预约数据查询
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
                    " ON UnChamberOrder.staff_num=CompanyUser.num AND (UnChamberOrder.status='waitting' OR UnChamberOrder.status='running')" +
                    " WHERE UnChamberOrder.machine_id=@id;";
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

        // 指定UnChamber设备预约数据查询
        [HttpPost]
        [Route("api/ae/chamber/schedule")]
        public ChamberScheduleRes Post([FromBody] ChamberScheduleReq req)
        {
            ChamberScheduleRes res = new ChamberScheduleRes();
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
                string sql = "SELECT * FROM ChamberTimeOrder WHERE status='waitting' AND machine_id=@id;";
                var DataSource = SqlHelper.GetTableText(sql, param);
                res.data = new ChamberScheduleData();
                ChamberScheduleResult[] results = new ChamberScheduleResult[DataSource[0].Rows.Count];
                int i = 0;
                foreach (DataTable table in DataSource)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        ChamberScheduleResult result = new ChamberScheduleResult();
                        result.remain_count = Convert.ToString(row["remain_count"]);
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
                res.errorMessage = "指定Chamber设备预约数据查询";
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
        [Route("api/ae/chamber/query_device_name")]
        public ChamberQueryDeviceNameRes Post([FromBody] ChamberQueryDeviceNameReq req)
        {
            ChamberQueryDeviceNameRes res = new ChamberQueryDeviceNameRes();
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

                sql = "SELECT DISTINCT TOP(10) name FROM Chamber WHERE name LIKE @name;";
                param = new SqlParameter[] {
                    new SqlParameter("@name","%"+req.query+"%"),
                };
                var DataSource = SqlHelper.GetTableText(sql, param);
                res.data = new ChamberQueryDeviceNameData();
                ChamberQueryDeviceNameResult[] results = new ChamberQueryDeviceNameResult[DataSource[0].Rows.Count];
                int i = 0;
                foreach (DataTable table in DataSource)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        ChamberQueryDeviceNameResult result = new ChamberQueryDeviceNameResult();
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

                sql = "SELECT DISTINCT test_item FROM UnChamberTestItem WHERE machine_type=@machine_type;";
                param = new SqlParameter[] {
                    new SqlParameter("@machine_type",req.query),
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
        // 查询chamber类型设备的有测试项目名称
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

        // 查询chamber类型设备的有测试项目名称
        [HttpPost]
        [Route("api/ae/unchamber/query_test_item")]
        public QueryUnChamberTestItemRes Post([FromBody] QueryUnChamberTestItemReq req)
        {
            QueryUnChamberTestItemRes res = new QueryUnChamberTestItemRes();
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

                sql = "SELECT test_item FROM UnChamberTestItem WHERE test_item LIKE @test_item;";
                param = new SqlParameter[] {
                    new SqlParameter("@test_item","%"+req.query+"%"),
                };
                var testData = SqlHelper.GetTableText(sql, param);
                res.data = new QueryUnChamberTestItemData();
                QueryUnChamberTestItemResult[] results = new QueryUnChamberTestItemResult[testData[0].Rows.Count];
                int i = 0;
                foreach (DataTable table in testData)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        QueryUnChamberTestItemResult result = new QueryUnChamberTestItemResult();
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
                res.errorMessage = "UnChamber设备类型-测试项目表待维护";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        [HttpPost]
        [Route("api/ae/chamber/query_ae")]
        public ChamberQueryAERes Post([FromBody] ChamberQueryAEReq req)
        {
            ChamberQueryAERes res = new ChamberQueryAERes();
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
                res.data = new ChamberQueryAEData();
                // 获取客户类型
                sql = "SELECT DISTINCT type FROM CustomerType";
                var customerData = SqlHelper.GetTableText(sql, new SqlParameter[] { });
                ChamberQueryCustomerResult[] customer = new ChamberQueryCustomerResult[customerData[0].Rows.Count];
                int a = 0;
                foreach (DataTable table in customerData)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        ChamberQueryCustomerResult result = new ChamberQueryCustomerResult();
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
                ChamberQueryMachineTypeResult[] machine = new ChamberQueryMachineTypeResult[machineData[0].Rows.Count];
                int b = 0;
                foreach (DataTable table in machineData)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        ChamberQueryMachineTypeResult result = new ChamberQueryMachineTypeResult();
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
                res.errorMessage = "Chamber设备类型-测试项目表待维护";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }
    }
}
