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

        // 搜索符合条件的设备 实验室、设备名称为筛选条件,时间为预约时间
        [HttpPost]
        [Route("api/ar/unchamber/search")]
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
            string sql = "SELECT COUNT(id) FROM UnChamberOrder WHERE status!='over' AND machine_id=@machine_id AND " +
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
        [Route("api/ar/chamber/search")]
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
                            "AND remain_count>=@test_count " +
                            "AND ((@end_time > start_time AND @end_time < end_time)" +
                            "OR (@start_time > start_time AND @start_time < end_time)" +
                            "OR (@start_time < start_time AND @end_time > end_time))";
                        param = new SqlParameter[] {
                            new SqlParameter("@machine_id", Convert.ToString(row["id"])),
                            new SqlParameter("@test_count", req.test_count),
                            new SqlParameter("@start_time", req.date[0]),
                            new SqlParameter("@end_time", req.date[1]),
                        };
                        var order = SqlHelper.GetTableText(sql, param);

                        foreach (DataTable t in order)
                        {
                            foreach (DataRow r in t.Rows)
                            {
                                result.status = "waitting";
                                result.lend_time = Convert.ToString(r["start_time"]);
                                result.return_time = Convert.ToString(r["end_time"]);
                                result.use_count = Convert.ToString(Convert.ToDecimal(row["capacity"]) - Convert.ToDecimal(r["remain_count"]));
                                result.remain_count = Convert.ToDecimal(r["remain_count"]) + "";
                                results[i] = result;
                                i++;
                                break;
                            }
                        }
                        if (result.status.Equals("waitting"))
                        {
                            continue;
                        }
                        // 存在则为忙碌，不存在则为空闲
                        sql = "SELECT * FROM ChamberTimeOrder WHERE status!='over' " +
                            "AND machine_id=@machine_id " +
                            "AND ((@end_time > start_time AND @end_time < end_time)" +
                            "OR (@start_time > start_time AND @start_time < end_time)" +
                            "OR (@start_time < start_time AND @end_time > end_time))";
                        param = new SqlParameter[] {
                            new SqlParameter("@machine_id", Convert.ToString(row["id"])),
                            new SqlParameter("@end_time", Convert.ToDateTime(req.date[0]).AddHours(0-run_time)),
                            new SqlParameter("@start_time", Convert.ToDateTime(req.date[1]).AddHours(run_time)),
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
                            result.remain_count = "剩余容量不够";
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

    }
}
