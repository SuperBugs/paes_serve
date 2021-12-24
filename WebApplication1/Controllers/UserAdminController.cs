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
    public class UserAdminController : ControllerBase
    {
        private readonly ILogger<UserAdminController> _logger;
        public UserAdminController(ILogger<UserAdminController> logger)
        {
            _logger = logger;
        }
        [HttpPost]
        [Route("api/admin/user/add")]
        public UserAdminAddRes Post([FromBody] UserAdminAddReq req)
        {
            UserAdminAddRes res = new UserAdminAddRes();
            //base64解密密码
            req.pass = Encoding.Default.GetString(Convert.FromBase64String(req.pass));
            try
            {
                res.success = "false";
                string sql;
                SqlParameter[] param;
                decimal count;
                TokenCheckResult tokenCheckResult = TokenHelper.CheckToken(req.token);
                if (!(tokenCheckResult.isValid && tokenCheckResult.userType.Equals("admin")))
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }

                sql = "SELECT * FROM CompanyUser WHERE num=@num";
                param = new SqlParameter[] {
                      new SqlParameter("@num",req.num),
                };
                count = Convert.ToDecimal(SqlHelper.ExecuteScalar(CommandType.Text, sql, param));
                if (count >= 1)
                {
                    res.errorMessage = "用户已存在";
                    return res;
                }

                sql = "INSERT INTO CompanyUser " +
                        "(num,name,pass,phone,email,role)" +
                        " VALUES (@num,@name,@pass,@phone,@email,@role)";
                param = new SqlParameter[] {
                     new SqlParameter("@name",req.name),
                     new SqlParameter("@num",req.num),
                     new SqlParameter("@pass",CommonUtils.MD5Encrypt32(req.pass)),
                     new SqlParameter("@phone",CommonUtils.StringNull(req.phone)),
                     new SqlParameter("@email",CommonUtils.StringNull(req.email)),
                     new SqlParameter("@role",req.role),
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
                if (!(tokenCheckResult.isValid && tokenCheckResult.userType.Equals("admin")))
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }

                string sql = "DELETE FROM CompanyUser WHERE id=@id ";
                SqlParameter[] param = new SqlParameter[] {
                    new SqlParameter("@id",req.id),
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
                if (!(tokenCheckResult.isValid && tokenCheckResult.userType.Equals("admin")))
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }
                string sql;
                SqlParameter[] param;

                sql = "SELECT * FROM CompanyUser WHERE id=@id AND pass=@pass";
                param = new SqlParameter[] {
                      new SqlParameter("@id",req.id),
                      new SqlParameter("@pass",req.pass),
                };
                var count = Convert.ToDecimal(SqlHelper.ExecuteScalar(CommandType.Text, sql, param));
                if (count >= 1)
                {
                    sql = "UPDATE CompanyUser SET " +
                        "name=@name,phone=@phone,email=@email,role=@role " +
                        "WHERE id=@id AND num=@num";
                    param = new SqlParameter[] {
                        new SqlParameter("@num",req.num),
                        new SqlParameter("@name",req.name),
                        new SqlParameter("@phone",CommonUtils.StringNull(req.phone)),
                        new SqlParameter("@email",CommonUtils.StringNull(req.email)),
                        new SqlParameter("@id",req.id),
                        new SqlParameter("@role",req.role),
                    };
                }
                else
                {
                    sql = "UPDATE CompanyUser SET " +
                        "name=@name,phone=@phone,email=@email,role=@role,pass=@pass " +
                        "WHERE id=@id AND num=@num";
                    param = new SqlParameter[] {
                        new SqlParameter("@pass",CommonUtils.MD5Encrypt32(req.pass)),
                        new SqlParameter("@num",req.num),
                        new SqlParameter("@name",req.name),
                        new SqlParameter("@phone",CommonUtils.StringNull(req.phone)),
                        new SqlParameter("@email",CommonUtils.StringNull(req.email)),
                        new SqlParameter("@id",req.id),
                        new SqlParameter("@role",req.role),
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

        [HttpPost]
        [Route("api/admin/user")]
        public UserAdminRetrieveRes Post([FromBody] UserAdminRetrieveReq req)
        {
            UserAdminRetrieveRes res = new UserAdminRetrieveRes();
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
                    new SqlParameter("@num", req.filters[1]),
                    new SqlParameter("@name", req.filters[2]),
                };
                if (req.filters[0].Length != 0)
                {
                    sqlFilter = sqlFilter + "AND id=@id ";
                }

                if (req.filters[1].Length != 0)
                {
                    sqlFilter = sqlFilter + "AND num=@num ";
                }

                if (req.filters[2].Length != 0)
                {
                    sqlFilter = sqlFilter + "AND name=@name ";
                }

                sql = "SELECT TOP(@pageSize) * FROM CompanyUser WHERE 1=1 " + sqlFilter +
                    " AND id NOT IN" +
                    "(SELECT TOP(@beforeSize) id FROM CompanyUser WHERE 1=1 " + sqlFilter +
                    " ORDER BY id) ORDER BY id";

                var DataSource = SqlHelper.GetTableText(sql, param);
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
                        result.pass = Convert.ToString(row["pass"]);
                        result.phone = Convert.ToString(row["phone"]);
                        result.role = Convert.ToString(row["role"]);
                        result.num = Convert.ToString(row["num"]);
                        results[i] = result;
                        i++;
                    }
                }
                sql = "SELECT COUNT(ID) FROM CompanyUser WHERE 1=1" + sqlFilter;
                param = new SqlParameter[] {
                    new SqlParameter("@id", req.filters[0]),
                    new SqlParameter("@num", req.filters[1]),
                    new SqlParameter("@name", req.filters[2]),
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
    }
}
