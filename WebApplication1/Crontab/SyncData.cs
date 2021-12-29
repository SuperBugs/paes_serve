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
using System.Threading.Tasks;
using paems.Interfaces;
using Newtonsoft.Json;

namespace paems.Crontab
{
    public class SyncData : Job
    {
        // 非chamber设备更新 1分钟刷新一次
        [Invoke(Begin = "2021-12-2 00:00", Interval = 1000 * 60)]
        public async Task Hour(IServiceProvider service)
        {
            try
            {
                /*非chamber
                设备非维修状态下：
                忙碌->空闲，订单status为running且结束时间小于当前时间，则设置status为over，
                设备状态设置为空闲，设备开始时间和结束时间设置为空，设备status设置为free时间置空
                预约中->忙碌，订单status为waitting且开始时间-1分钟小于当前时间，则设置status为running
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

                            updateSql = "UPDATE UnChamber SET status='free',lend_time=null,return_time=null WHERE id=@id;";
                            sqlStrList.Add(new KeyValuePair<string, SqlParameter[]>(updateSql, new SqlParameter[] {
                                new SqlParameter("@id", Convert.ToDecimal(row["machine_id"]))
                            }));
                        }
                        if (Convert.ToString(row["status"]).Equals("waitting") &&
                            Convert.ToDateTime(row["start_time"]).AddMinutes(-1) < dateTime)
                        {

                            updateSql = "UPDATE UnChamberOrder SET status='running' WHERE id=@id;";
                            sqlStrList.Add(new KeyValuePair<string, SqlParameter[]>(updateSql, new SqlParameter[] {
                                new SqlParameter("@id", Convert.ToDecimal(row["id"])) }));

                            updateSql = "UPDATE UnChamber SET status='running',lend_time=@lend_time,return_time=@return_time,return_staff=@return_staff WHERE id=@id;";
                            sqlStrList.Add(new KeyValuePair<string, SqlParameter[]>(updateSql, new SqlParameter[] {
                                new SqlParameter("@return_staff", Convert.ToString(row["staff_num"])),
                                new SqlParameter("@lend_time", Convert.ToDateTime(row["start_time"])),
                                new SqlParameter("@return_time", Convert.ToDateTime(row["end_time"])),
                                new SqlParameter("@id", Convert.ToDecimal(row["machine_id"]))
                            }));
                        }
                    }
                }

                await SqlHelper.ExecuteSqlTran(sqlStrList);
            }
            catch (Exception e)
            {
                CommonUtils.Nlog().Error(e, "更新UnChamber设备状态失败");
            }
        }

        // Chamber定时异步处理
        [Invoke(Begin = "2021-12-2 00:05", Interval = 1000 * 60 * 1)]
        public async Task Chamber(IServiceProvider service)
        {
            try
            {
                string sql;
                SqlParameter[] param;
                DateTime dateTime = DateTime.Now;
                DateTime tempTime = dateTime.AddHours(1);
                sql = "select ChamberOrder.id,ChamberOrder.machine_id,ChamberOrder.start_time,ChamberOrder.end_time,ChamberOrder.status,ChamberTimeOrder.id," +
                "ChamberTimeOrder.remain_count,ChamberTimeOrder.test_item,ChamberTestItem.test_time,CompanyUser.email,Chamber.name,CompanyUser.num " +
                "from (((ChamberOrder INNER JOIN ChamberTimeOrder ON ChamberOrder.time_order_id = ChamberTimeOrder.id) " +
                "INNER JOIN ChamberTestItem ON ChamberTestItem.test_item=ChamberOrder.test_item) " +
                "INNER JOIN CompanyUser ON ChamberOrder.staff_num=CompanyUser.num) " +
                "INNER JOIN Chamber ON Chamber.id=ChamberOrder.machine_id " +
                "WHERE ChamberOrder.status = 'running' or ChamberOrder.status = 'waitting';";
                param = new SqlParameter[] {
                };
                List<DataRow> toRun = new List<DataRow>();
                List<DataRow> toFree = new List<DataRow>();
                var DataSource = SqlHelper.GetTableText(sql, param);
                List<KeyValuePair<string, SqlParameter[]>> sqlStrList = new List<KeyValuePair<string, SqlParameter[]>>();
                string updateSql;
                foreach (DataTable table in DataSource)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        /*
                        1、拼单满->忙碌，当主订单status = waitting，remain_count为0，且开始时间小于(当前时间 +1h)。
                        设置主订单表，status为running。
                        设置关联客户订单表，开始时间为主订单开始时间，结束时间为主订单开始时间加测试所需时间(通过machine_id去ChamberTestItem中取值)，status为running，发送提醒邮件。
                        设置设备表，开始时间/结束时间同客户订单表，status为running。*/
                        if (Convert.ToString(row["status"]).Equals("waitting") && Convert.ToDecimal(row["remain_count"]) == 0 &&
                            Convert.ToDateTime(row["start_time"]) < tempTime)
                        {
                            toRun.Add(row);
                            updateSql = "UPDATE ChamberTimeOrder SET status='running',start_time=@start_time,end_time=@end_time WHERE id=@id;";
                            sqlStrList.Add(new KeyValuePair<string, SqlParameter[]>(updateSql, new SqlParameter[] {
                                new SqlParameter("@id", Convert.ToDecimal(row["id1"])),
                                new SqlParameter("@start_time", Convert.ToDateTime(row["start_time"])),
                                new SqlParameter("@end_time", Convert.ToDateTime(row["start_time"]).AddHours(Convert.ToDouble(row["test_time"]))),
                            }));
                            updateSql = "UPDATE ChamberOrder SET status='running',start_time=@start_time,end_time=@end_time WHERE time_order_id=@id;";
                            sqlStrList.Add(new KeyValuePair<string, SqlParameter[]>(updateSql, new SqlParameter[] {
                                new SqlParameter("@id", Convert.ToDecimal(row["id1"])),
                                new SqlParameter("@start_time", Convert.ToDateTime(row["start_time"])),
                                new SqlParameter("@end_time", Convert.ToDateTime(row["start_time"]).AddHours(Convert.ToDouble(row["test_time"]))),
                            }));
                            updateSql = "UPDATE Chamber SET status='running',start_time=@start_time,end_time=@end_time WHERE id=@machine_id;";
                            sqlStrList.Add(new KeyValuePair<string, SqlParameter[]>(updateSql, new SqlParameter[] {
                                new SqlParameter("@start_time", Convert.ToDateTime(row["start_time"])),
                                new SqlParameter("@end_time", Convert.ToDateTime(row["start_time"]).AddHours(Convert.ToDouble(row["test_time"]))),
                                new SqlParameter("@id", Convert.ToDecimal(row["machine_id"]))
                            }));
                        }
                        /*2、拼单不满->忙碌，当主订单status = waitting，remain_count不为0，结束时间小于(当前时间 +1h)。
                        设置主订单表，status为running。
                        设置关联客户订单表，开始时间为主订单结束时间，结束时间为主订单结束时间加测试所需时间(通过machine_id去ChamberTestItem中取值)，status为running，发送提醒邮件。
                        设置设备表，开始/结束时间同客户订单表，status为running。*/
                        if (Convert.ToString(row["status"]).Equals("waitting") && Convert.ToDecimal(row["remain_count"]) > 0 &&
                            Convert.ToDateTime(row["end_time"]) < tempTime)
                        {
                            toRun.Add(row);
                            updateSql = "UPDATE ChamberTimeOrder SET status='running',start_time=@start_time,end_time=@end_time WHERE id=@id;";
                            sqlStrList.Add(new KeyValuePair<string, SqlParameter[]>(updateSql, new SqlParameter[] {
                                new SqlParameter("@id", Convert.ToDecimal(row["id1"])),
                                new SqlParameter("@start_time", Convert.ToDateTime(row["end_time"])),
                                new SqlParameter("@end_time", Convert.ToDateTime(row["end_time"]).AddHours(Convert.ToDouble(row["test_time"]))),
                            }));
                            updateSql = "UPDATE ChamberOrder SET status='running',start_time=@start_time,end_time=@end_time WHERE time_order_id=@id;";
                            sqlStrList.Add(new KeyValuePair<string, SqlParameter[]>(updateSql, new SqlParameter[] {
                                new SqlParameter("@id", Convert.ToDecimal(row["id1"])),
                                new SqlParameter("@start_time", Convert.ToDateTime(row["end_time"])),
                                new SqlParameter("@end_time", Convert.ToDateTime(row["end_time"]).AddHours(Convert.ToDouble(row["test_time"]))),
                            }));
                            updateSql = "UPDATE Chamber SET status='running',start_time=@start_time,end_time=@end_time WHERE id=@machine_id;";
                            sqlStrList.Add(new KeyValuePair<string, SqlParameter[]>(updateSql, new SqlParameter[] {
                                new SqlParameter("@start_time", Convert.ToDateTime(row["end_time"])),
                                new SqlParameter("@end_time", Convert.ToDateTime(row["end_time"]).AddHours(Convert.ToDouble(row["test_time"]))),
                                new SqlParameter("@machine_id", Convert.ToDecimal(row["machine_id"]))
                            }));
                        }
                        /*3.1当remain_count为0，当前时间大于订单结束时间。
                          设置主订单表，status为over
                          设置关联客户订单表，status为over
                          设置设备表，status为free
                         */
                        if (Convert.ToString(row["status"]).Equals("running") && Convert.ToDateTime(row["end_time"]) < dateTime)
                        {
                            toFree.Add(row);
                            updateSql = "UPDATE ChamberTimeOrder SET status='over' WHERE id=@id;";
                            sqlStrList.Add(new KeyValuePair<string, SqlParameter[]>(updateSql, new SqlParameter[] {
                                new SqlParameter("@id", Convert.ToDecimal(row["id1"]))
                            }));

                            updateSql = "UPDATE Chamber SET status='free',start_time=null,end_time=null WHERE id=@machine_id;";
                            sqlStrList.Add(new KeyValuePair<string, SqlParameter[]>(updateSql, new SqlParameter[] {
                                new SqlParameter("@machine_id", Convert.ToDecimal(row["machine_id"]))
                            }));

                            updateSql = "UPDATE ChamberOrder SET status='over' WHERE time_order_id=@id;";
                            sqlStrList.Add(new KeyValuePair<string, SqlParameter[]>(updateSql, new SqlParameter[] {
                                new SqlParameter("@id", Convert.ToDecimal(row["id1"]))
                            }));
                        }

                    }
                }
                // 发送邮件
                var res = await SqlHelper.ExecuteSqlTran(sqlStrList);
                if (toFree.Count == 0 && toRun.Count == 0)
                {
                    return;
                }
                if (res != (toFree.Count + toRun.Count) * 3)
                {
                    CommonUtils.Nlog().Error(DataSource.ToString(), "Chamber订单处理异常");
                }
                EmailBean email = new EmailBean();
                email.fromName = "PA I-Lab设备管理系统";
                email.subject = "预约设备放置提醒";
                email.systemName = "StudyMapConfig";
                email.emailCenterUrl = "http://pa.itest.com:8129/EmailNew/sendMailTest";
                List<string> empty = new List<string>();
                foreach (DataRow row in toRun)
                {
                    List<string> address = new List<string>();
                    address.Add(Convert.ToString(row["email"]));
                    email.ToList = address;
                    email.ccList = empty;
                    email.attachPathList = empty;
                    email.body = $@"Hi {Convert.ToString(row["num"])}: <br>&nbsp;&nbsp;PA I-Lab设备管理系统提醒您，" +
                    $@"您预约的设备{Convert.ToString(row["name"])}即将启动，请在一小时之内将设备放入指定位置。<br>&nbsp;&nbsp;开始时间：{Convert.ToString(row["start_time"])}," +
                    $@"&nbsp;&nbsp;结束时间{Convert.ToString(row["end_time"])}" +
                    $@"&nbsp;&nbsp;" + "<br>&nbsp;&nbsp;系统发送，请勿回复，谢谢！";
                    var result = await CommonUtils.HttpPostSend(email.emailCenterUrl, email);
                    EmailResult sendResult = JsonConvert.DeserializeObject<EmailResult>(result);
                    if (sendResult.data != true)
                    {
                        CommonUtils.Nlog().Error(email.body, "提醒邮件发送失败");
                    }

                }
                foreach (DataRow row in toFree)
                {
                    List<string> address = new List<string>();
                    address.Add(Convert.ToString(row["email"]));
                    email.ToList = address;
                    email.ccList = empty;
                    email.attachPathList = empty;
                    email.body = $@"Hi {Convert.ToString(row["num"])}: <br>&nbsp;&nbsp;PA I-Lab设备管理系统提醒您，" +
                    $@"您预约的设备{Convert.ToString(row["name"])}运行结束，请尽快取走设备。<br>&nbsp;&nbsp;开始时间：{Convert.ToString(row["start_time"])}," +
                    $@"<br>&nbsp;&nbsp;结束时间{Convert.ToString(row["end_time"])}" +
                    $@"<br>&nbsp;&nbsp;系统发送，请勿回复，谢谢！";
                    var result = await CommonUtils.HttpPostSend(email.emailCenterUrl, email);
                    EmailResult sendResult = JsonConvert.DeserializeObject<EmailResult>(result);
                    if (sendResult.data != true)
                    {
                        CommonUtils.Nlog().Error(email.body, "提醒邮件发送失败");
                    }
                }
            }
            catch (Exception e)
            {
                CommonUtils.Nlog().Error(e, "定时处理Chamber订单失败");
            }
        }

    }

}
