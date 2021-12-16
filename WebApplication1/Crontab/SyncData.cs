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
        // 非chamber设备更新
        [Invoke(Begin = "2021-12-2 00:00", Interval = 1000 * 60)]
        public void Hour(IServiceProvider service)
        {
            try
            {
                /*非chamber
                设备非维修状态下：
                忙碌->空闲，订单status为running且结束时间小于当前时间，则设置status为over，
                设备状态设置为空闲，设备开始时间和结束时间设置为空，设备status设置为free时间置空
                预约中->忙碌，订单status为waitting且开始时间-1分钟大于当前时间，则设置status为running
                设备状态设置为忙碌，设备开始/结束时间设置为订单开始/结束时间，设备status设置为running，
                更新前使用人员
                */
                string sql;
                SqlParameter[] param;
                DateTime dateTime = DateTime.Now;
                sql = "select UnChamberOrder.id,UnChamberOrder.machine_id,UnChamberOrder.start_time,UnChamberOrder.end_time,UnChamberOrder.staff_num,UnchamberOrder.status " +
                    "from UnChamberOrder INNER JOIN UnChamber ON " +
                    "UnChamberOrder.machine_id=UnChamber.id WHERE UnChamber.status!='error' AND " +
                    "(UnChamberOrder.status='running' or UnChamberOrder.status='waitting')";
                param = new SqlParameter[] {
                };
                List<decimal> orderToOver = new List<decimal>();
                List<decimal> orderToRunning = new List<decimal>();
                List<decimal> machineToFree = new List<decimal>();
                List<decimal> machineToRuning = new List<decimal>();
                var DataSource = SqlHelper.GetTableText(sql, param);
                List<KeyValuePair<string, SqlParameter[]>> sqlStrList = new List<KeyValuePair<string, SqlParameter[]>>();
                string updateSql;
                foreach (DataTable table in DataSource)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        if (Convert.ToString(row["status"]).Equals("running") &&
                            Convert.ToDateTime(row["end_time"]) < dateTime)
                        {

                            updateSql = "UPDATE UnChamberOrder SET status='over' WHERE id=@id;";
                            sqlStrList.Add(new KeyValuePair<string, SqlParameter[]>(updateSql, new SqlParameter[] {
                                new SqlParameter("@id", Convert.ToDecimal(row["id"]))
                            }));

                            updateSql = "UPDATE UnChamber SET status='free',lend_time=null,return_time=null,return_staff=@return_staff WHERE id=@id;";
                            sqlStrList.Add(new KeyValuePair<string, SqlParameter[]>(updateSql, new SqlParameter[] {
                                new SqlParameter("@return_staff", Convert.ToString(row["staff_num"])),
                                new SqlParameter("@id", Convert.ToDecimal(row["machine_id"]))
                            }));
                        }
                        if (Convert.ToString(row["status"]).Equals("waitting") &&
                            Convert.ToDateTime(row["start_time"]).AddMinutes(-1) < dateTime)
                        {

                            updateSql = "UPDATE UnChamberOrder SET status='running' WHERE id=@id;";
                            sqlStrList.Add(new KeyValuePair<string, SqlParameter[]>(updateSql, new SqlParameter[] {
                                new SqlParameter("@id", Convert.ToDecimal(row["id"])) }));

                            updateSql = "UPDATE UnChamber SET status='running',lend_time=@lend_time,return_time=@return_time WHERE id=@id;";
                            sqlStrList.Add(new KeyValuePair<string, SqlParameter[]>(updateSql, new SqlParameter[] {
                                new SqlParameter("@lend_time", Convert.ToDateTime(row["start_time"])),
                                new SqlParameter("@return_time", Convert.ToDateTime(row["end_time"])),
                                new SqlParameter("@id", Convert.ToDecimal(row["machine_id"]))
                            }));
                        }
                    }
                }

                SqlHelper.ExecuteSqlTran(sqlStrList);
            }
            catch (Exception e)
            {
                CommonUtils.Nlog().Error(e, "更新UnChamber设备状态失败");
            }
        }

        // Chamber设备更新10分钟轮
        [Invoke(Begin = "2021-12-2 00:05", Interval = 1000 * 60)]
        public void Chamber(IServiceProvider service)
        {
            try
            {
                /*chamber
                chamber设备非维修状态
                忙碌->空闲，主订单status为running且结束时间小于当前时间，则设置所有订单status为over，
                设备状态设置为空闲，设备开始时间和结束时间设置为空，设备status设置为free时间置空
                预约中->忙碌，主订单status=waitting且开始时间-1分钟大于当前时间，则设置status为running
                设备状态设置为忙碌，设备开始/结束时间设置为主订单开始/结束时间，设备status设置为running
                */
                string sql;
                SqlParameter[] param;
                DateTime dateTime = DateTime.Now;
                sql = "select ChamberOrder.id,ChamberOrder.machine_id,ChamberOrder.start_time,ChamberOrder.end_time,ChamberOrder.status " +
                    "from ChamberOrder INNER JOIN Chamber ON " +
                    "ChamberOrder.machine_id=Chamber.id WHERE Chamber.status!='error' AND " +
                    "(ChamberOrder.status='running' or ChamberOrder.status='waitting')";
                param = new SqlParameter[] {
                };
                List<decimal> orderToOver = new List<decimal>();
                List<decimal> orderToRunning = new List<decimal>();
                List<decimal> machineToFree = new List<decimal>();
                List<decimal> machineToRuning = new List<decimal>();
                var DataSource = SqlHelper.GetTableText(sql, param);
                List<KeyValuePair<string, SqlParameter[]>> sqlStrList = new List<KeyValuePair<string, SqlParameter[]>>();
                string updateSql;
                foreach (DataTable table in DataSource)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        if (Convert.ToString(row["status"]).Equals("running") &&
                            Convert.ToDateTime(row["end_time"]) < dateTime)
                        {

                            updateSql = "UPDATE ChamberTimeOrder SET status='over' WHERE id=@id;";
                            sqlStrList.Add(new KeyValuePair<string, SqlParameter[]>(updateSql, new SqlParameter[] {
                                new SqlParameter("@id", Convert.ToDecimal(row["id"]))
                            }));

                            updateSql = "UPDATE Chamber SET status='free',start_time=null,end_time=null WHERE id=@machine_id;";
                            sqlStrList.Add(new KeyValuePair<string, SqlParameter[]>(updateSql, new SqlParameter[] {
                                new SqlParameter("@machine_id", Convert.ToDecimal(row["machine_id"]))
                            }));

                            updateSql = "UPDATE ChamberOrder SET status='over' WHERE time_order_id=@id;";
                            sqlStrList.Add(new KeyValuePair<string, SqlParameter[]>(updateSql, new SqlParameter[] {
                                new SqlParameter("@id", Convert.ToDecimal(row["id"]))
                            }));
                        }
                        if (Convert.ToString(row["status"]).Equals("waitting") &&
                            Convert.ToDateTime(row["start_time"]).AddMinutes(-1) < dateTime)
                        {

                            updateSql = "UPDATE ChamberTimeOrder SET status='running' WHERE id=@id;";
                            sqlStrList.Add(new KeyValuePair<string, SqlParameter[]>(updateSql, new SqlParameter[] {
                                new SqlParameter("@id", Convert.ToDecimal(row["id"]))
                            }));

                            updateSql = "UPDATE Chamber SET status='running',start_time=@start_time,end_time=@end_time WHERE id=@machine_id;";
                            sqlStrList.Add(new KeyValuePair<string, SqlParameter[]>(updateSql, new SqlParameter[] {
                                new SqlParameter("@start_time", Convert.ToDateTime(row["start_time"])),
                                new SqlParameter("@end_time", Convert.ToDateTime(row["end_time"])),
                                new SqlParameter("@id", Convert.ToDecimal(row["machine_id"]))
                            }));

                            updateSql = "UPDATE ChamberOrder SET status='running' WHERE time_order_id=@id;";
                            sqlStrList.Add(new KeyValuePair<string, SqlParameter[]>(updateSql, new SqlParameter[] {
                                new SqlParameter("@id", Convert.ToDecimal(row["id"]))
                            }));
                        }
                    }
                }

                SqlHelper.ExecuteSqlTran(sqlStrList);
            }
            catch (Exception e)
            {
                CommonUtils.Nlog().Error(e, "更新Chamber设备状态失败");
            }
        }

    }

}
