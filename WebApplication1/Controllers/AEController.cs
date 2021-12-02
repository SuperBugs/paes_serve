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

                /*if (req.filters != null)
                {
                    if (req.filters.Contains("忙碌") && !req.filters.Contains("空闲") && !req.filters.Contains("维修"))
                    {
                        sqlFilter = " AND status='running' ";
                    }

                    if (req.filters.Contains("忙碌") && req.filters.Contains("空闲") && !req.filters.Contains("维修"))
                    {
                        sqlFilter = " AND （status='running' OR status='free') ";
                    }

                    if (req.filters.Contains("忙碌") && !req.filters.Contains("空闲") && req.filters.Contains("维修"))
                    {
                        sqlFilter = " AND （status='running' OR status='error') ";
                    }

                    if (!req.filters.Contains("忙碌") && req.filters.Contains("空闲") && !req.filters.Contains("维修"))
                    {
                        sqlFilter = " AND status='free' ";
                    }

                    if (!req.filters.Contains("忙碌") && !req.filters.Contains("空闲") && req.filters.Contains("维修"))
                    {
                        sqlFilter = " AND status='error' ";
                    }
                }*/

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

        private string getScaleStatus(decimal machine_id, string start_time, string end_time)
        {
            string sql = "SELECT COUNT(id) FROM UnChamberOrder WHERE status!='over' AND " +
                "((start_time<@start_time AND end_time>@start_time) OR (start_time<@end_time AND end_time>@end_time))";
            SqlParameter[] param = new SqlParameter[] {
                    new SqlParameter("@id", machine_id),
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
                    " ON UnChamberOrder.staff_num=CompanyUser.num" +
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
    }
}
