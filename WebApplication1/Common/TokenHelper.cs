
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace paems.Common
{
    /// 此类为抽象类，
    /// 不允许实例化，在应用时直接调用即可
    /// 
    public abstract class TokenHelper
    {
        public static TokenCheckResult CheckToken(string token)
        {
            //token 用户类型、工号            
            TokenCheckResult tokenCheckResult = new TokenCheckResult();
            var iat = 0.00;
            try
            {
                //当前的时间戳用于判断token是否过期
                var now = Math.Round((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);

                var encodedJwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
                //开始时间
                iat = Convert.ToDouble(encodedJwt.Claims.FirstOrDefault(u => u.Type == "iat").Value);
                //结束时间
                var exp = Convert.ToDouble(encodedJwt.Claims.FirstOrDefault(u => u.Type == "exp").Value);

                //如果当前时间戳不再Token声明周期范围内，则返回Token过期
                if (!(iat < now && now < exp))
                {
                    // Console.WriteLine("时间过期");
                    tokenCheckResult.isValid = false;
                }

                tokenCheckResult.userNum = encodedJwt.Claims.FirstOrDefault(u => u.Type == "UserNumber").Value;
                tokenCheckResult.userType = encodedJwt.Claims.FirstOrDefault(u => u.Type == "UserType").Value;
            }
            catch (Exception e)
            {
                CommonUtils.Nlog().Error(e, $"token解析异常token:{token}");
                tokenCheckResult.isValid = false;
            }
           
            tokenCheckResult.isValid = true;

            return tokenCheckResult;
        }

        public static string CreateToken(string userNumber, string userType)
        {
            var claims = new[]{
                new Claim(JwtRegisteredClaimNames.Iat,$"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}") ,
                new Claim (JwtRegisteredClaimNames.Exp,$"{new DateTimeOffset(DateTime.Now.AddDays(Config.TokenExpireTime)).ToUnixTimeSeconds()}"),
                new Claim("UserNumber", userNumber),
                new Claim("UserType", userType)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Config.SecurityKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
              issuer: Config.Company,
              audience: Config.Audience,
              claims: claims,
              expires: DateTime.Now.AddDays(Config.TokenExpireTime),
              signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }

    public class TokenCheckResult
    {
        public string userNum { get; set; }
        public string userType { get; set; }
        public bool isValid { get; set; }
    }
}
