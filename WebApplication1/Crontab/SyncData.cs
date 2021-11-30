using paems.Common;
using Pomelo.AspNetCore.TimedJob;
using System;
using System.IO;
using System.Net;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using paems.Common;

namespace paems.Crontab
{
    public class SyncData : Job
    {
        // 10分钟轮
/*        [Invoke(Begin = "2020-10-28 00:05", Interval = 1000 * 60 * 10)]
        public void Hour(IServiceProvider service)
        {
            try
            {
                // 同步Redis用户缓存数据
                RedisClient.redisClient.SyncUserTable();
            }
            catch (Exception e)
            {
                CommonUtils.Nlog().Error(e, "定时同步用户信息Redis缓存失败");
            }
        }*/

        // 天轮
        [Invoke(Begin = "2020-10-28 00:30", Interval = 1000 * 60 * 60 * 24)]
        public void Day(IServiceProvider service)
        {
            string sql;
            SqlParameter[] param;
            try
            {
                DateTime dateTime = DateTime.Now.Date.AddDays(1 - DateTime.Now.Day);
                sql = "select * from init_config";
                param = new SqlParameter[] {
                };
                var DataSource = SqlHelper.GetTableText(sql, param);
                Config[] results = new Config[DataSource[0].Rows.Count];
                int i = 0;
                foreach (DataTable table in DataSource)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        Config result = new Config();
                        result.id = Convert.ToDecimal(row["id"]);
                        result.name = Convert.ToString(row["name"]);
                        result.link = Convert.ToString(row["link"]);
                        result.start_time = Convert.ToDateTime(row["start_time"]);
                        result.cycle_time = Convert.ToDecimal(row["cycle_time"]);
                        results[i] = result;
                        i++;
                    }
                }
                /*// 计算排序
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

                Stopwatch sw = new Stopwatch();
                sw.Start();

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
                */
            }
            catch (Exception e)
            {
                CommonUtils.Nlog().Error(e, "定时同步计算排行榜失败");
            }
            return;
        }

    }
    public class Config
    {
        public decimal id { get; set; }
        public decimal cycle_time { get; set; }
        public string name { get; set; }
        public string link { get; set; }
        public string is_start { get; set; }
        public DateTime start_time { get; set; }

    }

}
