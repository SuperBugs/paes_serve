using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceProcess;
using System.Text;
using System.Text.Json;
using System.Text.Unicode;

namespace paems.Common
{
    public class CommonUtils
    {
        public static void printDataTableCollection(DataTableCollection dataTableCollection)
        {
            // Get Each DataTable in the DataTableCollection and
            // print each row value.
            foreach (DataTable table in dataTableCollection)
                foreach (DataRow row in table.Rows)
                    foreach (DataColumn column in table.Columns)
                    {
                        if (row[column] != null)
                        {
                            Console.WriteLine(column);
                            Console.WriteLine(row[column]);
                        }

                    }
        }

        /// <summary>
        /// 64位MD5加密
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string MD5Encrypt64(string password)
        {
            MD5 md5 = MD5.Create(); //实例化一个md5对像
            // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(s);
        }

        public static bool IsDate(string strDate)
        {
            try
            {
                DateTime.Parse(strDate);  //不是字符串时会出现异常
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static object DateNull(string obj)
        {
            if (!IsDate(obj))
            {
                return DBNull.Value;
            }
            else
            {
                return obj;
            }
        }

        public static object StringNull(string obj)
        {
            if (obj == null)
            {
                return DBNull.Value;
            }
            else
            {
                return obj;
            }
        }

        /// <summary>
        /// 16位MD5加密
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string MD5Encrypt16(string password)
        {
            var md5 = new MD5CryptoServiceProvider();
            string t2 = BitConverter.ToString(md5.ComputeHash(Encoding.Default.GetBytes(password)), 4, 8);
            t2 = t2.Replace("-", "");
            return t2;
        }

        /// <summary>
        /// 32位MD5加密（小写）
        /// </summary>
        /// <param name="input">输入字段</param>
        /// <returns></returns>
        public static string MD5Encrypt32(string str)
        {
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(str));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));//转化为小写的16进制
            }
            return sBuilder.ToString();
        }


        public static bool CompareArr(string[] arr1, string[] arr2)
        {
            var q = from a in arr1 join b in arr2 on a equals b select a;
            bool flag = arr1.Length == arr2.Length && q.Count() == arr1.Length;
            return flag;//内容相同返回true,反之返回false。
        }

        public static double TimeToSeconds(DateTime dateTime)
        {
            return Math.Round((dateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
        }

        public static string JSON(object ob)
        {
            var options = new JsonSerializerOptions();
            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(UnicodeRanges.All);
            return JsonSerializer.Serialize(ob, options);
        }

        public static void RestartRedis()
        {
            // 重启Redis
            try
            {
                ServiceController service = new ServiceController("Redis");
                if (service.Status == ServiceControllerStatus.Running)
                {
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped);
                }
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running);
            }
            catch (Exception ex)
            {
                var log = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
                log.Error(ex, "Redis服务重启异常");
            }
        }

        public static NLog.Logger Nlog()
        {
            var pathToContentRoot = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string nlogConfigPath = $"{Path.Combine(pathToContentRoot, "nlog.config")}";
            if (File.Exists(nlogConfigPath))
            {
                // 独立发布exe的情况下
                NLog.LogManager.LogFactory.SetCandidateConfigFilePaths(new List<string> { nlogConfigPath });
                return NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            }
            else
            {
                // debug环境nlog.config路径
                return NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            }

        }

    }


}
