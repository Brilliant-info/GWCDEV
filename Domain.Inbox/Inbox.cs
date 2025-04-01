using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Activation;
using System.Configuration;
using Interface.Inbox;
using System.ServiceModel;
using System.Xml.Linq;
using System.Data.EntityClient;
using Domain.Server;

namespace Domain.Inbox
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]

    public partial class Inbox : Interface.Inbox.iInbox
    {
        Domain.Server.Server svr = new Server.Server();
        Server.Server server = new Server.Server();

        public List<SP_GetInboxData_Result> GetInboxDetailByUserID(long UserID, string isAll, string WhereValue, string[] condetails)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(server.GetEntityConnection(condetails));
            List<SP_GetInboxData_Result> lst = new List<SP_GetInboxData_Result>();
            //lst = (from inbox in db.SP_GetInboxData(UserID, isAll)
            //       select inbox).ToList();
            //return lst;
            switch (WhereValue.ToLower())
            {
                case "today":
                    lst = (from inbox in db.SP_GetInboxData(UserID, "false")
                           where inbox.ECD == DateTime.Now
                           select inbox).ToList();
                    break;

                case "tomorrow":
                    lst = (from inbox in db.SP_GetInboxData(UserID, "false")
                           where inbox.ECD == DateTime.Today.AddDays(1)
                           select inbox).ToList();

                    break;

                case "overdue":

                    lst = (from inbox in db.SP_GetInboxData(UserID, "false")
                           where inbox.ECD < DateTime.Now.Date && inbox.TaskStatus == "New Task"
                           select inbox).ToList();
                    break;

                case "today+overdue":
                    lst = (from inbox in db.SP_GetInboxData(UserID, "false")
                           where inbox.ECD <= DateTime.Now.Date && inbox.TaskStatus == "New Task"
                           select inbox).ToList();
                    break;

                case "7days+overdue":
                    lst = (from inbox in db.SP_GetInboxData(UserID, "false")
                           where inbox.ECD < DateTime.Now.AddDays(7) && inbox.ECD > DateTime.Now && inbox.TaskStatus == "New Task"
                           select inbox).ToList();
                    break;
                case "alltoday":
                    lst = (from inbox in db.SP_GetInboxData(UserID, "true")
                           where inbox.ECD == DateTime.Now.Date
                           select inbox).ToList();
                    break;
                case "alltomorrow":
                    lst = (from inbox in db.SP_GetInboxData(UserID, "true")
                           where inbox.ECD == DateTime.Today.AddDays(1)
                           select inbox).ToList();
                    break;

                case "alloverdue":
                    lst = (from inbox in db.SP_GetInboxData(UserID, "true")
                           where inbox.ECD < DateTime.Now.Date && inbox.TaskStatus == "New Task"
                           select inbox).ToList();
                    break;

                case "alltoday+overdue":
                    lst = (from inbox in db.SP_GetInboxData(UserID, "true")
                           where inbox.ECD <= DateTime.Now.Date && inbox.TaskStatus == "New Task"
                           select inbox).ToList();
                    break;

                case "all7days+overdue":
                    lst = (from inbox in db.SP_GetInboxData(UserID, "true")
                           where inbox.ECD < DateTime.Now.AddDays(7) && inbox.ECD > DateTime.Now && inbox.TaskStatus == "New Task"
                           select inbox).ToList();
                    break;

                default:
                    lst = (from inbox in db.SP_GetInboxData(UserID, isAll)
                           orderby inbox.ECD ascending
                           select inbox).ToList();
                    break;
            }
            return lst;
        }

        public DataSet bindInboxDetailData(string ObjectName, string ReferenceID, string[] conn)
        {
            DataSet ds = new DataSet();
            ds.Reset();
            string str;
            switch (ObjectName.ToLower())
            {
                case "lead":
                    str = "select * from vGetDefaultView_Lead where id in(" + ReferenceID + ")";
                    ds = fillds(str, conn);
                    break;

                case "opportunity":
                    str = "select * from vGetDefaultView_Opportunity where id in(" + ReferenceID + ")";
                    ds = fillds(str, conn);
                    break;


                case "quotation":
                    str = "select * from vGetDefaultView_Quotation where id in(" + ReferenceID + ")";
                    ds = fillds(str, conn);
                    break;

                case "salesorder":
                    str = "select * from vGetDefaultView_SalesOrder where id in(" + ReferenceID + ")";
                    ds = fillds(str, conn);
                    break;

                case "invoice":
                    str = "select * from vGetDefaultView_Invoice where id in(" + ReferenceID + ")";
                    ds = fillds(str, conn);
                    break;
            }

            return ds;

        }

        protected DataSet fillds(String strquery, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(server.GetEntityConnection(conn));
            DataSet ds = new DataSet();
            using (System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection("Data Source=" + conn[0] + ";Initial Catalog=" + conn[1] + "; User ID=" + conn[3] + "; Password=" + conn[2] + ";"))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(strquery, sqlConn))
                {
                    try
                    {
                        ds.Reset();
                        sqlConn.Open();
                        da.Fill(ds);
                        sqlConn.Close();
                    }
                    catch
                    {
                        sqlConn.Close();
                    }
                }
            }
            return ds;
        }

        public List<SP_GetInboxDataOfApproval_Result> GetInboxDetailBySiteUserID(long UserID, string Site, string[] condetails)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(server.GetEntityConnection(condetails));
            List<SP_GetInboxDataOfApproval_Result> lst = new List<SP_GetInboxDataOfApproval_Result>();

            if (Site == "All")
            {
                lst = (from inbox in db.SP_GetInboxDataOfApproval(UserID)
                       select inbox).OrderByDescending(i => i.RequestedDate).ToList();
            }
            else
            {
                lst = (from inbox in db.SP_GetInboxDataOfApproval(UserID)
                       where inbox.Site == Site
                       select inbox).OrderByDescending(i => i.RequestedDate).ToList();
            }
            return lst;
        }

        public List<POR_vGetInboxData> GetInboxDataByUserID(long UserID, string[] conn)
        {
            List<POR_vGetInboxData> InboxData = new List<POR_vGetInboxData>();
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(server.GetEntityConnection(conn));
                InboxData = db.POR_vGetInboxData.Where(d => d.ToUserID == UserID).OrderByDescending(d => d.Date).ToList();
            }
            catch { }
            return InboxData;
        }

        #region GWC_Inbox
        public DataSet GetUserInbox(long UserID, string[] conn)
        {
            DataSet ds = new DataSet();
            try
            {
                ds = fillds("select * from VW_CorrespondanceDetail where OrderHeadID in (select ID from tOrderhead where requestBy=" + UserID + ") and Archive!=1  and MailStatus!=0 and  MessageFrom is not null order by Id desc ", conn);
            }
            catch
            {
                ds.Dispose();
            }
            return ds;

        }


        public DataSet GetUserInboxWhere(long UserID, string WhereCondition, string[] conn)
        {
            DataSet ds = new DataSet();
            try
            {
                if (WhereCondition == "Archive Messages")
                {
                    ds = fillds("select * from VW_CorrespondanceDetail where OrderHeadID in (select ID from tOrderhead where requestBy=" + UserID + ") and Archive=1  and MailStatus!=0 order by Id desc", conn);
                }
                else if (WhereCondition == "All Messages")
                {
                    ds = fillds("select * from VW_CorrespondanceDetail where OrderHeadID in (select ID from tOrderhead where requestBy=" + UserID + ") and Archive!=1 and MailStatus!=0 order by Id desc", conn);
                }
                else if (WhereCondition == "Yesterdays Message")
                {
                    string ydt = DateTime.Now.AddDays(-1).ToShortDateString();
                    ds = fillds("  select * from VW_CorrespondanceDetail where OrderHeadID in (select ID from tOrderhead where requestBy=" + UserID + ") and Archive!=1  and MessageFrom  is not Null and MailStatus!=0 order by Id desc", conn);
                }
                else if (WhereCondition == "Todays Message")
                {
                    //ds = fillds(" DECLARE @d1 DATETIME set @d1=GetDate()  select * from VW_CorrespondanceDetail where OrderHeadID in (select ID from tOrderhead where requestBy=" + UserID + ") and Archive!=1 and [date]>= convert(varchar(12) ,@d1 , 101)  and MailStatus!=0 order by Id desc", conn);
                    ds = fillds("  select * from VW_CorrespondanceDetail where OrderHeadID in (select ID from tOrderhead where requestBy=" + UserID + ") and Archive!=1 and MessageFrom  is Null  and MailStatus!=0 order by Id desc", conn);
                }
            }
            catch
            {
                ds.Dispose();
            }
            return ds;
        }

        public DataSet GetInbox(long UserID, string[] conn)
        {
           
            DataSet ds = new DataSet();
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "getinboxrecord";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@userID", UserID);
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    return ds;
                }
                
            }
            catch
            {
                ds.Dispose();
            }
            return ds;
        }

        public DataSet GetInboxWhere(long UserID, string WhereCondition, string[] conn)
        {
            //DataSet dsUserDept = new DataSet();
            //dsUserDept = fillds("select TerritoryID from mUserTerritoryDetail where Userid=" + UserID + " ", conn);
            DataSet ds = new DataSet();
            try
            {
                if (WhereCondition == "Archive Messages")
                {
                    ds = fillds("select * from VW_CorrespondanceDetail where storeId in (select TerritoryID from mUserTerritoryDetail where Userid=" + UserID + ") and Archive=1  and MailStatus!=0 order by Id desc", conn);
                }
                else if (WhereCondition == "All Messages")
                {
                    ds = fillds("select * from VW_CorrespondanceDetail where storeId in (select TerritoryID from mUserTerritoryDetail where Userid=" + UserID + ") and Archive!=1  and MailStatus!=0 order by Id desc", conn);
                }
                else if (WhereCondition == "Yesterdays Message")
                {
                    // ds = fillds("select * from VW_CorrespondanceDetail where storeId=" + dsUserDept.Tables[0].Rows[0]["TerritoryID"].ToString() + " and Archive!=1 and cast(date as date)=cast(" + DateTime.Now.AddDays(-1) + " as date)   and MailStatus!=0 order by Id desc", conn);
                    string ydt = DateTime.Now.AddDays(-1).ToShortDateString();
                    ds = fillds("select * from VW_CorrespondanceDetail  where storeId in(select TerritoryID from mUserTerritoryDetail where Userid=" + UserID + ")  and Archive!=1  and   MessageFrom  is not Null  and MailStatus!=0 order by Id desc", conn);
                }
                else if (WhereCondition == "Todays Message")
                {
                    ds = fillds("select * from VW_CorrespondanceDetail where storeId in(select TerritoryID from mUserTerritoryDetail where Userid=" + UserID + ") and Archive!=1 and MessageFrom  is Null  and MailStatus!=0 order by Id desc", conn);
                }
            }
            catch
            {
                ds.Dispose();
            }
            return ds;
        }

        public void SetArchive(string SelectedRec, string[] conn)
        {
            DataSet ds = new DataSet();
            try
            {
                ds = fillds("update tCorrespond set Archive =1 where Id in(" + SelectedRec + ")", conn);
                ds.Dispose();
            }
            catch (Exception)
            {
                ds.Dispose();
            }
        }

        public DateTime GetLastPasswordChangeDate(long UserID, string[] conn)
        {
            long ChngDays = 0;
            DataSet dsLastCreateddate = new DataSet();
            string str = "SELECT top (1)  CreatedDate FROM  mPasswordDetails where UserProfileID=" + UserID + " order by CreatedDate desc";
            dsLastCreateddate = fillds(str, conn);
            DateTime LstChngDt = Convert.ToDateTime(dsLastCreateddate.Tables[0].Rows[0]["CreatedDate"].ToString());
            return LstChngDt;
            //DateTime CrntDt = DateTime.Now;

            //System.TimeSpan datediff = CrntDt - LstChngDt;

            //long dtdiff = long.Parse(datediff.ToString());
            //ChngDays = dtdiff;
            //return ChngDays;

        }
# endregion


        public string ChkFillSecurityQue(long userid, string[] conn)
        {
            string result = "";
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            using (SqlDataAdapter da = new SqlDataAdapter())
            {
                try
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_ChkFillSecurityQue";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("userid", userid);
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    cmd.Connection.Close();
                    if(ds.Tables[0].Rows.Count >0)
                    {
                        result = "yes";
                    }
                    else
                    {
                        result = "no";
                    }
                }
                catch
                {
                    cmd.Connection.Close();
                }
                finally
                {

                }

            }

            return result;

        }
    }
}
