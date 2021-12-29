using paems.Common;
using paems.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace paems.Controllers
{
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserController(IHttpContextAccessor httpContextAccessor, ILogger<UserController> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        [HttpPost]
        [Route("api/login/account")]
        public UserLoginRes Post([FromBody] UserLoginReq req)
        {
            UserLoginRes res = new UserLoginRes();
            //base64解密密码
            req.password = Encoding.Default.GetString(Convert.FromBase64String(req.password));

            res.success = "false";

            try
            {
                string sql;
                SqlParameter[] param;
                sql = "SELECT pass,role FROM CompanyUser WHERE num=@num";
                param = new SqlParameter[] {
                    new SqlParameter("@num",req.username),
                };
                var DataSource = SqlHelper.GetTableText(sql, param);

                foreach (DataTable table in DataSource)
                {
                    //用户不存在创建用户
                    if (table.Rows.Count == 0)
                    {
                        if (!req.username.Equals(req.password))
                        {
                            res.errorMessage = "默认密码错误，请重新输入!";
                            return res;
                        }
                        sql = "INSERT INTO CompanyUser " +
                            "(num,pass)" +
                            " VALUES (@num,@pass)";
                        param = new SqlParameter[] {
                             new SqlParameter("@num",req.username),
                             new SqlParameter("@pass",CommonUtils.MD5Encrypt32(req.password)),
                        };
                        var DataSource2 = SqlHelper.ExecteNonQueryText(sql, param);
                        if (DataSource2 != 1)
                        {
                            res.errorMessage = "新增用户失败";
                            return res;
                        }
                        res.success = "true";
                        res.data = TokenHelper.CreateToken(req.username, "User");
                        return res;
                    }
                    foreach (DataRow row in table.Rows)
                    {
                        if (Convert.ToString(row["pass"]).Equals(CommonUtils.MD5Encrypt32(req.password)))
                        {

                            res.data = TokenHelper.CreateToken(req.username, Convert.ToString(row["role"]));

                            res.success = "true";
                        }
                        else
                        {
                            res.errorMessage = "密码错误";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                res.errorMessage = "用户登录接口异常";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));

            }
            return res;
        }

        [HttpPost]
        [Route("api/currentUser")]
        public UserCurrentRes Post([FromBody] UserCurrentReq req)
        {
            UserCurrentRes res = new UserCurrentRes();
            res.success = "false";
            try
            {
                TokenCheckResult tokenCheckResult = TokenHelper.CheckToken(req.token);
                if (!tokenCheckResult.isValid || tokenCheckResult.userType.Equals("User"))
                {
                    res.errorMessage = "身份验证失败";
                    return res;
                }
                string sql;
                SqlParameter[] param;
                sql = "SELECT * FROM CompanyUser WHERE num=@num";
                param = new SqlParameter[] {
                    new SqlParameter("@num",tokenCheckResult.userNum),
                };
                var DataSource = SqlHelper.GetTableText(sql, param);
                foreach (DataTable table in DataSource)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        res.name = Convert.ToString(row["name"]);
                        res.email = Convert.ToString(row["email"]);
                        res.userid = Convert.ToString(row["num"]);
                        res.phone = Convert.ToString(row["phone"]);
                        if (Convert.ToString(row["role"]).Equals("user"))
                        {
                            res.access = "user";
                        }
                        else
                        {
                            res.access = "admin";
                        }
                        res.success = "true";
                    }
                }
            }
            catch (Exception e)
            {
                res.errorMessage = "请登录";
                _logger.LogError(e, res.errorMessage + "\r\n" + CommonUtils.JSON(req));
            }
            return res;
        }

    }
}
