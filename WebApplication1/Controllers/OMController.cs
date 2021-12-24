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
    public class OMController : ControllerBase
    {
        private readonly ILogger<OMController> _logger;
        public OMController(ILogger<OMController> logger)
        {
            _logger = logger;
        }
        // 暂时直接删除不处理ChamberTimeOrder
        [HttpPost]
        [Route("api/om/chamber/delete")]
        public OMChamberDeleteRes Post([FromBody] OMChamberDeleteReq req)
        {
            OMChamberDeleteRes res = new OMChamberDeleteRes();
            try
            {
                res.success = "false";
                TokenCheckResult tokenCheckResult = TokenHelper.CheckToken(req.token);
                if (!(tokenCheckResult.isValid && tokenCheckResult.userType.Equals("admin")))
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }

                string sql = "DELETE FROM ChamberOrder WHERE id=@id ";
                SqlParameter[] param = new SqlParameter[] {
                    new SqlParameter("@id",req.id),
                };
                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);
                if (DataSource != 1)
                {
                    res.errorMessage = "Chamber订单删除失败";
                    return res;
                }
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "Chamber订单删除异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        [HttpPost]
        [Route("api/om/unchamber/delete")]
        public OMUnChamberDeleteRes Post([FromBody] OMUnChamberDeleteReq req)
        {
            OMUnChamberDeleteRes res = new OMUnChamberDeleteRes();
            try
            {
                res.success = "false";
                TokenCheckResult tokenCheckResult = TokenHelper.CheckToken(req.token);
                if (!(tokenCheckResult.isValid && tokenCheckResult.userType.Equals("admin")))
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }

                string sql = "DELETE FROM UnChamberOrder WHERE id=@id ";
                SqlParameter[] param = new SqlParameter[] {
                    new SqlParameter("@id",req.id),
                };
                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);
                if (DataSource != 1)
                {
                    res.errorMessage = "UnChamber订单删除失败";
                    return res;
                }
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "删除UnChamber订单接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        [HttpPost]
        [Route("api/om/chamber/search")]
        public OMChamberRetrieveRes Post([FromBody] OMChamberRetrieveReq req)
        {
            OMChamberRetrieveRes res = new OMChamberRetrieveRes();
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
                    new SqlParameter("@machine_id", req.filters[1]),
                    new SqlParameter("@staff_num", req.filters[2]),
                    new SqlParameter("@time_order_id", req.filters[3]),
                    new SqlParameter("@status", req.filters[4]),
                };
                if (req.filters[0].Length != 0)
                {
                    sqlFilter = sqlFilter + " AND id=@id ";
                }

                if (req.filters[1].Length != 0)
                {
                    sqlFilter = sqlFilter + " AND machine_id=@machine_id ";
                }

                if (req.filters[2].Length != 0)
                {
                    sqlFilter = sqlFilter + " AND staff_num=@staff_num ";
                }

                if (req.filters[3].Length != 0)
                {
                    sqlFilter = sqlFilter + " AND time_order_id=@time_order_id ";
                }

                if (req.filters[4].Length != 0)
                {
                    sqlFilter = sqlFilter + " AND status=@status ";
                }

                sql = "SELECT TOP(@pageSize) * FROM ChamberOrder WHERE 1=1" + sqlFilter +
                    " AND id NOT IN" +
                    "(SELECT TOP(@beforeSize) id FROM ChamberOrder WHERE 1=1" + sqlFilter +
                    " ORDER BY id DESC) ORDER BY id DESC";

                var DataSource = SqlHelper.GetTableText(sql, param);
                res.data = new OMChamberData();
                OMChamberResult[] results = new OMChamberResult[DataSource[0].Rows.Count];
                int i = 0;
                foreach (DataTable table in DataSource)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        OMChamberResult result = new OMChamberResult();
                        result.id = Convert.ToDecimal(row["id"]);
                        result.machine_id = Convert.ToString(row["machine_id"]);
                        result.staff_num = Convert.ToString(row["staff_num"]);
                        result.start_time = Convert.ToString(row["start_time"]);
                        result.end_time = Convert.ToString(row["end_time"]);
                        result.order_time = Convert.ToString(row["order_time"]);
                        result.customer_type = Convert.ToString(row["customer_type"]);
                        result.test_machine_type = Convert.ToString(row["test_machine_type"]);
                        result.test_stage = Convert.ToString(row["test_stage"]);
                        result.test_item = Convert.ToString(row["test_item"]);
                        result.test_count = Convert.ToString(row["test_count"]);
                        result.test_target = Convert.ToString(row["test_target"]);
                        result.status = Convert.ToString(row["status"]);
                        result.time_order_id = Convert.ToString(row["time_order_id"]);
                        results[i] = result;
                        i++;
                    }
                }
                sql = "SELECT COUNT(id) FROM ChamberOrder WHERE 1=1" + sqlFilter;
                param = new SqlParameter[] {
                    new SqlParameter("@id", req.filters[0]),
                    new SqlParameter("@machine_id", req.filters[1]),
                    new SqlParameter("@staff_num", req.filters[2]),
                    new SqlParameter("@time_order_id", req.filters[3]),
                    new SqlParameter("@status", req.filters[4]),
                 };
                res.data.result = results;
                res.data.total = Convert.ToDecimal(SqlHelper.ExecuteScalar(CommandType.Text, sql, param));
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "Chamber订单记录接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        [HttpPost]
        [Route("api/om/unchamber/search")]
        public OMUnChamberRetrieveRes Post([FromBody] OMUnChamberRetrieveReq req)
        {
            OMUnChamberRetrieveRes res = new OMUnChamberRetrieveRes();
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
                    new SqlParameter("@machine_id", req.filters[1]),
                    new SqlParameter("@staff_num", req.filters[2]),
                    new SqlParameter("@status", req.filters[3]),
                };
                if (req.filters[0].Length != 0)
                {
                    sqlFilter = sqlFilter + " AND id=@id ";
                }

                if (req.filters[1].Length != 0)
                {
                    sqlFilter = sqlFilter + " AND machine_id=@machine_id ";
                }

                if (req.filters[2].Length != 0)
                {
                    sqlFilter = sqlFilter + " AND staff_num=@staff_num ";
                }

                if (req.filters[3].Length != 0)
                {
                    sqlFilter = sqlFilter + " AND status=@status ";
                }

                sql = "SELECT TOP(@pageSize) * FROM UnChamberOrder WHERE 1=1" + sqlFilter +
                    " AND id NOT IN" +
                    "(SELECT TOP(@beforeSize) id FROM UnChamberOrder WHERE 1=1" + sqlFilter +
                    " ORDER BY id DESC) ORDER BY id DESC";

                var DataSource = SqlHelper.GetTableText(sql, param);
                res.data = new OMUnChamberData();
                OMUnChamberResult[] results = new OMUnChamberResult[DataSource[0].Rows.Count];
                int i = 0;
                foreach (DataTable table in DataSource)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        OMUnChamberResult result = new OMUnChamberResult();
                        result.id = Convert.ToDecimal(row["id"]);
                        result.machine_id = Convert.ToString(row["machine_id"]);
                        result.staff_num = Convert.ToString(row["staff_num"]);
                        result.start_time = Convert.ToString(row["start_time"]);
                        result.end_time = Convert.ToString(row["end_time"]);
                        result.order_time = Convert.ToString(row["order_time"]);
                        result.customer_type = Convert.ToString(row["customer_type"]);
                        result.test_machine_type = Convert.ToString(row["test_machine_type"]);
                        result.test_stage = Convert.ToString(row["test_stage"]);
                        result.test_item = Convert.ToString(row["test_item"]);
                        result.test_count = Convert.ToString(row["test_count"]);
                        result.test_target = Convert.ToString(row["test_target"]);
                        result.status = Convert.ToString(row["status"]);
                        results[i] = result;
                        i++;
                    }
                }
                sql = "SELECT COUNT(id) FROM UnChamberOrder WHERE 1=1" + sqlFilter;
                param = new SqlParameter[] {
                    new SqlParameter("@id", req.filters[0]),
                    new SqlParameter("@machine_id", req.filters[1]),
                    new SqlParameter("@staff_num", req.filters[2]),
                    new SqlParameter("@status", req.filters[3]),
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
    }
}
