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
        /**
         * 
        // 预约
        [HttpPost]
        [Route("api/ae/unchamber")]
        public UnChamberSearchRes Post([FromBody] UnChamberSearchReq req)
        {
            UnChamberSearchRes res = new UnChamberSearchRes();
            try
            {
                res.success = "false";
                string sql;
                SqlParameter[] param;
                decimal count;
                TokenCheckResult tokenCheckResult = TokenHelper.CheckToken(req.token);
                if (!tokenCheckResult.isValid || tokenCheckResult.userType.Equals("User"))
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }

                sql = "SELECT * FROM COMPANY_USER WHERE USER_NUMBER=@num";
                param = new SqlParameter[] {
                      new SqlParameter("@num",req.num),
                };
                count = Convert.ToDecimal(SqlHelper.ExecuteScalar(CommandType.Text, sql, param));
                if (count >= 1)
                {
                    res.errorMessage = "用户已存在";
                    return res;
                }

                sql = "INSERT INTO COMPANY_USER " +
                        "(USER_NUMBER,USER_NAME,USER_PASSWORD,USER_PHONE,USER_EMAIL,DEPARTMENT_NAME,USER_TYPE)" +
                        " VALUES (@num,@name,@password,@phone,@email,@department,@type)";
                param = new SqlParameter[] {
                     new SqlParameter("@name",req.name),
                     new SqlParameter("@num",req.num),
                     new SqlParameter("@password",CommonUtils.MD5Encrypt32(req.password)),
                     new SqlParameter("@phone",req.phone),
                     new SqlParameter("@email",req.email),
                     new SqlParameter("@department",req.department),
                     new SqlParameter("@type",Convert.ToInt16(0))
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
                res.errorMessage = "添加用户接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        [HttpPost]
        [Route("api/admin/user/delete")]
        public UserAdminDeleteRes Post([FromBody] UserAdminDeleteReq req)
        {
            UserAdminDeleteRes res = new UserAdminDeleteRes();
            try
            {
                res.success = "false";
                TokenCheckResult tokenCheckResult = TokenHelper.CheckToken(req.token);
                if (!tokenCheckResult.isValid || tokenCheckResult.userType.Equals("User"))
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }

                string sql = "DELETE FROM COMPANY_USER WHERE num=@num ";
                SqlParameter[] param = new SqlParameter[] {
                    new SqlParameter("@num",req.num),
                };
                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);
                if (DataSource != 1)
                {
                    res.errorMessage = "用户从数据库删除失败";
                    return res;
                }
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "删除用户接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }


        [HttpPost]
        [Route("api/admin/user/change")]
        public UserAdminChangeRes Post([FromBody] UserAdminChangeReq req)
        {
            UserAdminChangeRes res = new UserAdminChangeRes();
            try
            {
                res.success = "false";
                TokenCheckResult tokenCheckResult = TokenHelper.CheckToken(req.token);
                if (!tokenCheckResult.isValid || tokenCheckResult.userType.Equals("User"))
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }
                string sql;
                SqlParameter[] param;
                // 检查密码是否被修改，32位默认没修改
                if (req.password.Length == 32)
                {
                    sql = "UPDATE COMPANY_USER SET " +
                        "name=@name," +
                        "phone=@phone,email=@email,department=@department" +
                        " WHERE id=@id AND num=@num";
                    param = new SqlParameter[] {
                        new SqlParameter("@num",req.num),
                        new SqlParameter("@name",req.name),
                        new SqlParameter("@phone",req.phone),
                        new SqlParameter("@email",req.email),
                        new SqlParameter("@department",req.department),
                        new SqlParameter("@id",req.id),
                    };
                }
                else
                {
                    sql = "UPDATE COMPANY_USER SET " +
                        "name=@name,pass=@password," +
                        "phone=@phone,email=@email,department=@department" +
                        " WHERE id=@id AND num=@num";
                    param = new SqlParameter[] {
                    new SqlParameter("@num",req.num),
                    new SqlParameter("@name",req.name),
                    new SqlParameter("@password",CommonUtils.MD5Encrypt32(req.password)),
                    new SqlParameter("@phone",req.phone),
                    new SqlParameter("@email",req.email),
                    new SqlParameter("@department",req.department),
                    new SqlParameter("@id",req.id),
                };
                }

                var DataSource = SqlHelper.ExecteNonQueryText(sql, param);
                if (DataSource != 1)
                {
                    res.errorMessage = "用户修改失败";
                }
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "修改用户信息接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
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
                if (!tokenCheckResult.isValid || tokenCheckResult.userType.Equals("User"))
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
                    new SqlParameter("@num", req.filters[0]),
                    new SqlParameter("@name", req.filters[1]),
                    new SqlParameter("@department", req.filters[2]),
                };
                if (req.filters[0].Length != 0)
                {
                    sqlFilter = sqlFilter + "AND num=@num ";
                }

                if (req.filters[1].Length != 0)
                {
                    sqlFilter = sqlFilter + "AND name=@name ";
                }

                if (req.filters[2].Length != 0)
                {
                    sqlFilter = sqlFilter + "AND department=@department ";
                }

                sql = "SELECT TOP(@pageSize) * FROM COMPANY_USER WHERE type='User' " + sqlFilter +
                    " AND ID NOT IN" +
                    "(SELECT TOP(@beforeSize) ID FROM COMPANY_USER WHERE type='User'" + sqlFilter +
                    " ORDER BY ID DESC) ORDER BY ID DESC";

                var DataSource = SqlHelper.GetTableText(sql, param);
                // CommonUtils.printDataTableCollection(DataSource);
                res.data = new UserAdminData();
                UserAdminResult[] results = new UserAdminResult[DataSource[0].Rows.Count];
                int i = 0;
                foreach (DataTable table in DataSource)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        UserAdminResult result = new UserAdminResult();
                        result.id = Convert.ToDecimal(row["id"]);
                        result.name = Convert.ToString(row["name"]);
                        result.email = Convert.ToString(row["email"]);
                        result.password = Convert.ToString(row["pass"]);
                        result.phone = Convert.ToString(row["phone"]);
                        result.department = Convert.ToString(row["department"]);
                        result.num = Convert.ToString(row["num"]);
                        result.key = i;
                        results[i] = result;
                        i++;
                    }
                }
                sql = "SELECT COUNT(ID) FROM COMPANY_USER WHERE type='User'" + sqlFilter;
                param = new SqlParameter[] {
                    new SqlParameter("@num", req.filters[0]),
                    new SqlParameter("@name", req.filters[1]),
                    new SqlParameter("@department", req.filters[2]),
                };
                res.data.result = results;
                res.data.total = Convert.ToDecimal(SqlHelper.ExecuteScalar(CommandType.Text, sql, param));
                res.success = "true";
            }
            catch (Exception e)
            {
                res.errorMessage = "用户记录接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

        [HttpGet]
        [Route("api/admin/rank")]
        public void rank()
        {
            // 更新积分排行榜
            string sql;
            SqlParameter[] param;
            try
            {
                // 月初
                DateTime dateTime = DateTime.Now.Date.AddDays(1 - DateTime.Now.Day);
                sql = "select * from POINTS_RECORD where time >= @dt and value > 0";
                param = new SqlParameter[] {
                    new SqlParameter("@dt",dateTime),
                };
                var DataSource = SqlHelper.GetTableText(sql, param);
                POINTS_RECORD[] results = new POINTS_RECORD[DataSource[0].Rows.Count];
                int i = 0;
                foreach (DataTable table in DataSource)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        POINTS_RECORD result = new POINTS_RECORD();
                        result.id = Convert.ToDecimal(row["id"]);
                        result.value = Convert.ToInt32(row["value"]);
                        result.num = Convert.ToString(row["user_num"]);
                        result.time = Convert.ToDateTime(row["time"]);
                        result.activity = Convert.ToString(row["activity"]);
                        results[i] = result;
                        i++;
                    }
                }
                // 计算排序
                var num = results.Select(x => x.num).ToList().Distinct().ToList();
                DateTime time = DateTime.Now.Date.AddDays(1 - DateTime.Now.Day);
                sql = "DELETE FROM MONTH_RANK WHERE time >= @time";
                param = new SqlParameter[] {
                    new SqlParameter("@time",time),
                };
                if (SqlHelper.ExecteNonQueryText(sql, param) < 0)
                {
                    CommonUtils.Nlog().Error("定时同步计算排行榜失败,删除历史数据失败");
                }

                Dictionary<string, int> moBom = new Dictionary<string, int>();
                for (int h = 0; h < num.Count; h++)
                {
                    moBom.Add(num[h], 0);
                }
                foreach (POINTS_RECORD p in results)
                {
                    moBom[p.num] = moBom[p.num] + p.value;
                }
                var data = moBom.Values.ToList();
                data.Sort();
                DataTable dt = new DataTable();
                dt.Columns.Add("id", typeof(int));
                dt.Columns.Add("num", typeof(string));
                dt.Columns.Add("count", typeof(int));
                dt.Columns.Add("time", typeof(DateTime));

                for (int x = 0; x < moBom.Count; x++)
                {
                    DataRow dr = dt.NewRow();
                    dr["num"] = num[x];
                    dr["count"] = moBom[num[x]];
                    dr["time"] = DateTime.Now;
                    dt.Rows.Add(dr);
                }

                if (!SqlHelper.BulkExcute(dt, "MONTH_RANK"))
                {
                    CommonUtils.Nlog().Error("定时同步计算排行榜失败");
                }
            }
            catch (Exception e)
            {
                CommonUtils.Nlog().Error(e, "定时同步计算排行榜失败");
            }
            return;
        }

        [HttpGet]
        [Route("api/admin/analysis")]
        public void analysis()
        {
            // 数据分析
            string sql;
            SqlParameter[] param;
            try
            {
                // oldTime昨天零点 nowTime今天零点
                DateTime oldTime = DateTime.Now.Date.AddDays(-1);
                DateTime nowTime = DateTime.Now.Date;

                sql = "select top (7) * from WEEK_DATA order by id";
                param = new SqlParameter[] {
                };
                var DataSource = SqlHelper.GetTableText(sql, param);
                WEEK_DATA[] results = new WEEK_DATA[7];
                for (int i = 0; i < 7; i++)
                {
                    WEEK_DATA result = new WEEK_DATA();
                    result.id = i;
                    result.receive = 0;
                    result.handle = 0;
                    result.finish = 0;

                    if (DataSource[0].Rows.Count >= i + 2)
                    {
                        result.receive = Convert.ToInt32(DataSource[0].Rows[i + 1]["receive"]);
                        result.handle = Convert.ToInt32(DataSource[0].Rows[i + 1]["handle"]);
                        result.finish = Convert.ToInt32(DataSource[0].Rows[i + 1]["finish"]);
                    }
                    results[i] = result;
                }

                // 昨天处理完毕
                sql = "select count(id) from QUESTION where end_time >= @oldTime and end_time<@nowTime";
                param = new SqlParameter[] {
                    new SqlParameter("@oldTime",oldTime),
                    new SqlParameter("@nowTime",nowTime),
                };
                results[6].finish = Convert.ToInt32(SqlHelper.ExecuteScalar(CommandType.Text, sql, param));
                // 处理过程中的
                sql = "select count(id) from QUESTION where process_now!='End'";
                param = new SqlParameter[] {
                };
                results[6].handle = Convert.ToInt32(SqlHelper.ExecuteScalar(CommandType.Text, sql, param));
                // 昨天接收
                sql = "select count(id) from QUESTION where sub_time >= @oldTime and sub_time<@nowTime";
                param = new SqlParameter[] {
                    new SqlParameter("@oldTime",oldTime),
                    new SqlParameter("@nowTime",nowTime),
                };
                results[6].receive=Convert.ToInt32(SqlHelper.ExecuteScalar(CommandType.Text, sql, param));

                //删除历史数据
                sql = "DELETE FROM WEEK_DATA WHERE 1=1";
                param = new SqlParameter[] {
                };
                if (SqlHelper.ExecteNonQueryText(sql, param) < 0)
                {
                    CommonUtils.Nlog().Error("删除历史数据失败");
                }
                // 插入数据
                DataTable dt = new DataTable();
                dt.Columns.Add("id", typeof(int));
                dt.Columns.Add("receive", typeof(int));
                dt.Columns.Add("handle", typeof(int));
                dt.Columns.Add("finish", typeof(int));

                for (int x = 0; x < 7; x++)
                {
                    DataRow dr = dt.NewRow();
                    dr["id"] = results[x].id;
                    dr["finish"] = results[x].finish;
                    dr["handle"] = results[x].handle;
                    dr["receive"] = results[x].receive;
                    dt.Rows.Add(dr);
                }

                if (!SqlHelper.BulkExcute(dt, "WEEK_DATA"))
                {
                    CommonUtils.Nlog().Error("定时分析数据失败");
                }
            }
            catch (Exception e)
            {
                CommonUtils.Nlog().Error(e, "定时分析数据失败");
            }
            return;
        }
        **
        */
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
                if (req.lab.Length != 0)
                {
                    sqlFilter = sqlFilter + " AND lab=@lab ";
                }

                if (req.deviceName.Length != 0)
                {
                    sqlFilter = sqlFilter + " AND name=@deviceName ";
                }

                if (req.filters != null)
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
                }

                sql = "SELECT TOP(@pageSize) * FROM UnChamber WHERE 1=1 "+sqlFilter +
                    " AND id NOT IN" +
                    "(SELECT TOP(@beforeSize) id FROM UnChamber WHERE 1=1 " + sqlFilter +
                    " ORDER BY id DESC) ORDER BY id DESC";

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
                        result.test_type = Convert.ToString(row["test_type"]);
                        result.lend_time = Convert.ToString(row["lend_time"]);
                        result.return_time = Convert.ToString(row["return_time"]);
                        result.lab = Convert.ToString(row["lab"]);
                        result.return_staff = Convert.ToString(row["return_staff"]);
                        results[i] = result;
                        i++;
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
    }
}
