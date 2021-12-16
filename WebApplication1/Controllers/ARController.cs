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
    public class ARController : ControllerBase
    {
        private readonly ILogger<ARController> _logger;
        public ARController(ILogger<ARController> logger)
        {
            _logger = logger;
        }

        // 搜索用户unchamber设备预约记录
        [HttpPost]
        [Route("api/ar/unchamber/search")]
        public ARUnChamberSearchRes Post([FromBody] ARUnChamberSearchReq req)
        {
            ARUnChamberSearchRes res = new ARUnChamberSearchRes();
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
                string sqlFilter = "AND UnChamberOrder.staff_num=@staff_num ";
                SqlParameter[] param;
                param = new SqlParameter[] {
                    new SqlParameter("@pageSize",Convert.ToInt16(req.pageSize)),
                    new SqlParameter("@beforeSize",Convert.ToInt16(req.pageSize*(req.currentPage-1))),
                    new SqlParameter("@test_item", req.testProject),
                    new SqlParameter("@name", req.deviceName),
                    new SqlParameter("@staff_num", tokenCheckResult.userNum),
                    new SqlParameter("@test_stage", req.testStage),
                    new SqlParameter("@status", req.status),
                };
                if (!req.testProject.Equals(""))
                {
                    sqlFilter = sqlFilter + " AND UnChamberOrder.test_item=@test_item ";
                }

                if (!req.deviceName.Equals(""))
                {
                    sqlFilter = sqlFilter + " AND UnChamber.name=@name ";
                }
                if (!req.testStage.Equals(""))
                {
                    sqlFilter = sqlFilter + " AND UnChamberOrder.test_stage=@test_stage ";
                }
                if (!req.status.Equals(""))
                {
                    sqlFilter = sqlFilter + " AND UnChamberOrder.status=@status ";
                }
                sql = "SELECT TOP(@pageSize) UnChamber.name,UnChamber.num,UnChamber.lab,UnChamber.return_staff,UnChamberOrder.id," +
                    "UnChamberOrder.start_time,UnChamberOrder.end_time,UnChamberOrder.test_stage,UnChamberOrder.test_item,UnChamberOrder.status" +
                    " FROM UnChamberOrder INNER JOIN UnChamber ON UnChamberOrder.machine_id=UnChamber.id AND UnChamberOrder.staff_num=@staff_num WHERE 1=1" + sqlFilter +
                    " AND UnChamberOrder.id NOT IN" +
                    "(SELECT TOP(@beforeSize) UnChamberOrder.id" +
                    " FROM UnChamberOrder INNER JOIN UnChamber ON UnChamberOrder.machine_id=UnChamber.id AND UnChamberOrder.staff_num=@staff_num WHERE 1=1" + sqlFilter +
                    " ORDER BY UnChamberOrder.id DESC)" +
                    " ORDER BY UnChamberOrder.id DESC";
                var DataSource = SqlHelper.GetTableText(sql, param);
                res.data = new ARUnChamberSearchData();
                ARUnChamberSearchResult[] data = new ARUnChamberSearchResult[DataSource[0].Rows.Count];
                int i = 0;
                foreach (DataTable table in DataSource)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        ARUnChamberSearchResult result = new ARUnChamberSearchResult();
                        result.id = Convert.ToDecimal(row["id"]);
                        result.name = Convert.ToString(row["name"]);
                        result.num = Convert.ToString(row["num"]);
                        result.test_stage = Convert.ToString(row["test_stage"]);
                        result.test_item = Convert.ToString(row["test_item"]);
                        result.start_time = Convert.ToString(row["start_time"]);
                        result.end_time = Convert.ToString(row["end_time"]);
                        result.lab = Convert.ToString(row["lab"]);
                        result.status = Convert.ToString(row["status"]);
                        result.return_staff = Convert.ToString(row["return_staff"]);
                        data[i] = result;
                        i++;
                    }
                }
                res.data.result = data;
                sql = "SELECT COUNT(UnChamberOrder.id) FROM UnChamberOrder INNER JOIN UnChamber ON UnChamberOrder.machine_id=UnChamber.id AND UnChamberOrder.staff_num=@staff_num WHERE 1=1" + sqlFilter;
                param = new SqlParameter[] {
                    new SqlParameter("@test_item", req.testProject),
                    new SqlParameter("@name", req.deviceName),
                    new SqlParameter("@staff_num", tokenCheckResult.userNum),
                    new SqlParameter("@test_stage", req.testStage),
                    new SqlParameter("@status", req.status),
                };
                res.data.total = Convert.ToDecimal(SqlHelper.ExecuteScalar(CommandType.Text, sql, param));
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "UnChamber预约记录查询接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        // 搜索用户chamber设备预约记录
        [HttpPost]
        [Route("api/ar/chamber/search")]
        public ARChamberSearchRes Post([FromBody] ARChamberSearchReq req)
        {
            ARChamberSearchRes res = new ARChamberSearchRes();
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
                string sqlFilter = "AND ChamberOrder.staff_num=@staff_num ";
                SqlParameter[] param;
                param = new SqlParameter[] {
                    new SqlParameter("@pageSize",Convert.ToInt16(req.pageSize)),
                    new SqlParameter("@beforeSize",Convert.ToInt16(req.pageSize*(req.currentPage-1))),
                    new SqlParameter("@test_item", req.testProject),
                    new SqlParameter("@name", req.deviceName),
                    new SqlParameter("@staff_num", tokenCheckResult.userNum),
                    new SqlParameter("@test_stage", req.testStage),
                    new SqlParameter("@status", req.status),
                };
                if (!req.testProject.Equals(""))
                {
                    sqlFilter = sqlFilter + " AND ChamberOrder.test_item=@test_item ";
                }

                if (!req.deviceName.Equals(""))
                {
                    sqlFilter = sqlFilter + " AND Chamber.name=@name ";
                }
                if (!req.testStage.Equals(""))
                {
                    sqlFilter = sqlFilter + " AND ChamberOrder.test_stage=@test_stage ";
                }
                if (!req.status.Equals(""))
                {
                    sqlFilter = sqlFilter + " AND ChamberOrder.status=@status ";
                }
                sql = "SELECT TOP(@pageSize) Chamber.name,Chamber.num,Chamber.lab,Chamber.return_staffs,ChamberOrder.id," +
                    "ChamberOrder.start_time,ChamberOrder.end_time,ChamberOrder.test_stage,ChamberOrder.test_item,ChamberOrder.status" +
                    " FROM ChamberOrder INNER JOIN Chamber ON ChamberOrder.machine_id=Chamber.id AND ChamberOrder.staff_num=@staff_num WHERE 1=1" + sqlFilter +
                    " AND ChamberOrder.id NOT IN" +
                    "(SELECT TOP(@beforeSize) ChamberOrder.id" +
                    " FROM ChamberOrder INNER JOIN Chamber ON ChamberOrder.machine_id=Chamber.id AND ChamberOrder.staff_num=@staff_num WHERE 1=1" + sqlFilter +
                    " ORDER BY ChamberOrder.id DESC)" +
                    " ORDER BY ChamberOrder.id DESC";
                var DataSource = SqlHelper.GetTableText(sql, param);
                res.data = new ARChamberSearchData();
                ARChamberSearchResult[] data = new ARChamberSearchResult[DataSource[0].Rows.Count];
                int i = 0;
                foreach (DataTable table in DataSource)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        ARChamberSearchResult result = new ARChamberSearchResult();
                        result.id = Convert.ToDecimal(row["id"]);
                        result.name = Convert.ToString(row["name"]);
                        result.num = Convert.ToString(row["num"]);
                        result.test_stage = Convert.ToString(row["test_stage"]);
                        result.test_item = Convert.ToString(row["test_item"]);
                        result.start_time = Convert.ToString(row["start_time"]);
                        result.end_time = Convert.ToString(row["end_time"]);
                        result.lab = Convert.ToString(row["lab"]);
                        result.status = Convert.ToString(row["status"]);
                        result.return_staff = Convert.ToString(row["return_staffs"]);
                        data[i] = result;
                        i++;
                    }
                }
                res.data.result = data;
                sql = "SELECT COUNT(ChamberOrder.id) FROM ChamberOrder INNER JOIN Chamber ON ChamberOrder.machine_id=Chamber.id AND ChamberOrder.staff_num=@staff_num WHERE 1=1" + sqlFilter;
                param = new SqlParameter[] {
                    new SqlParameter("@test_item", req.testProject),
                    new SqlParameter("@name", req.deviceName),
                    new SqlParameter("@staff_num", tokenCheckResult.userNum),
                    new SqlParameter("@test_stage", req.testStage),
                    new SqlParameter("@status", req.status),
                };
                res.data.total = Convert.ToDecimal(SqlHelper.ExecuteScalar(CommandType.Text, sql, param));
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "Chamber预约记录查询接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        // 非chamber设备取消使用，使用完毕
        [HttpPost]
        [Route("api/ar/unchamber/cancel")]
        public UnChamberARCancelRes Post([FromBody] UnChamberARCancelReq req)
        {
            UnChamberARCancelRes res = new UnChamberARCancelRes();
            try
            {
                res.success = "false";
                TokenCheckResult tokenCheckResult = TokenHelper.CheckToken(req.token);
                if (!tokenCheckResult.isValid)
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }
                SqlParameter[] param = new SqlParameter[] {
                    new SqlParameter("@order_id",req.id),
                };
                var rows = SqlHelper.ExecteNonQueryProducts("CancelUnChamberOrder", param);

                if (rows == -1)
                {
                    res.errorMessage = "取消失败";
                }
                res.success = "true";

            }
            catch (Exception e)
            {
                res.errorMessage = "Unchamber设备取消接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        // chamber设备取消使用，使用完毕
        [HttpPost]
        [Route("api/ar/chamber/cancel")]
        public ChamberARCancelRes Post([FromBody] ChamberARCancelReq req)
        {
            ChamberARCancelRes res = new ChamberARCancelRes();
            try
            {
                res.success = "false";
                TokenCheckResult tokenCheckResult = TokenHelper.CheckToken(req.token);
                if (!tokenCheckResult.isValid)
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }
                SqlParameter[] param = new SqlParameter[] {
                    new SqlParameter("@order_id",req.id),
                };
                var rows = SqlHelper.ExecteNonQueryProducts("CancelChamberOrder", param);

                if (rows == -1)
                {
                    res.errorMessage = "取消失败";
                }
                res.success = "true";

            }
            catch (Exception e)
            {
                res.errorMessage = "chamber设备取消接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

    }
}
