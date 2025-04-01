using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Activation;
using System.Configuration;
using Interface.Approval;
using System.ServiceModel;
using System.Data.EntityClient;
using System.Data.Objects;
using System.Globalization;
using System.Reflection;
using System.Xml.Linq;
using Domain.Server;

namespace Domain.Approval
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]

    public partial class ApprovalLevelMaster : Interface.Approval.iApprovalLevelMaster
    {
        //BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities();
        Domain.Server.Server svr = new Server.Server();
        #region GetApprovalLevelList
        /// <summary>
        /// GetApprovalLevelList is providing List of Approval
        /// </summary>
        /// <returns></returns>
        /// 
        public List<mApprovalLevel> GetActivityList(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mApprovalLevel> Approval = new List<mApprovalLevel>();
            Approval = (from p in ce.mApprovalLevels
                        select p).ToList();
            return Approval;
        }
        #endregion

        #region InsertmApprovalLevel
        public int InsertmApprovalLevel(mApprovalLevel approval, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.mApprovalLevels.AddObject(approval);
            ce.SaveChanges();
            return Convert.ToInt32(approval.ID);
            // return 1;
        }
        #endregion

        #region updatemApprovalLevel
        public int updatemApprovalLevel(mApprovalLevel updateApproval, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.mApprovalLevels.Attach(updateApproval);
            ce.ObjectStateManager.ChangeObjectState(updateApproval, EntityState.Modified);
            ce.SaveChanges();

            return 1;
        }
        #endregion

        #region GetApprovalRecordByID
        /// <summary>
        /// GetApprovalRecordByID is providing List of Approval By ID for Edit mode
        /// </summary>
        /// <returns></returns>
        /// 
        public mApprovalLevel GetApprovalRecordByID(int approvalId, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            mApprovalLevel ApprovalID = new mApprovalLevel();
            ApprovalID = (from p in ce.mApprovalLevels
                          where p.ID == approvalId
                          select p).FirstOrDefault();
            ce.Detach(ApprovalID);
            return ApprovalID;
        }
        #endregion

        #region GetApprovalDetailRecordByID
        /// <summary>
        /// GetApprovalDetailRecordByID is providing List of Approval By ID for Edit mode
        /// </summary>
        /// <returns></returns>
        /// 
        public List<mApprovalLevelDetail> GetApprovalDetailRecordByID(int approvalDetailId, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mApprovalLevelDetail> ApprovalID = new List<mApprovalLevelDetail>();
            ApprovalID = (from p in ce.mApprovalLevelDetails
                          where p.ApprovalLevelID == approvalDetailId
                          select p).ToList();

            return ApprovalID;
        }
        #endregion

        #region GetObjectList
        /// <summary>
        /// GetObjectList is providing List of Object for bind ObjectdropDown
        /// </summary>
        /// <returns></returns>
        /// 
        public List<sysmObject> GetObjectList(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<sysmObject> ObjectList = new List<sysmObject>();

            ObjectList = (from p in ce.sysmObjects
                          orderby p.Sequence
                          where p.ApprovalProcess == true
                          select p).ToList();
            return ObjectList;
        }
        #endregion

        #region GetApprovalLevelMax
        /// <summary>
        /// GetApprovalLevel is providing max value of ApprovalLevel for bind ApprovalLevelLable
        /// </summary>
        /// <returns></returns>
        /// 
        public Int32 GetApprovalLevelMax(string objName, long TerritoryID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            //int approval = (from p in ce.mApprovalLevels
            //                where p.ObjectName == objName
            //                select p.ApprovalLevel).Max();
            var approval = (from db in ce.mApprovalLevels
                            where db.ObjectName == objName && db.TerritoryID == TerritoryID
                            select (int?)db.ApprovalLevel).Max();

            return approval == null ? 1 : Convert.ToInt32(approval) + 1;
        }
        #endregion

        #region GetApprovalRecordToBindGrid
        /// <summary>
        /// GetApprovalRecordToBindGrid is providing List of Approval for bind grid with Actine Yes/No
        /// </summary>
        /// <returns></returns>
        /// 
        public DataSet GetApprovalRecordToBindGrid(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mApprovalLevel> LeadSource = new List<mApprovalLevel>();
            XElement xmlApprovalMaster = new XElement("ApprovalList", from m in ce.mApprovalLevels.AsEnumerable()

                                                                      select new XElement("Approval",
                                                                      new XElement("ID", m.ID),
                                                                      new XElement("ObjectName", m.ObjectName),
                                                                      new XElement("ApprovalLevel", m.ApprovalLevel),
                                                                      new XElement("NoOfApprovers", m.NoOfApprovers),
                                                                      new XElement("MinApprovalReq", m.MinApprovalReq),
                                                                      new XElement("IsLowerLevelApprovalReq", m.IsLowerLevelApprovalReq == true ? "Yes" : "No"),
                                                                      new XElement("MaxAmount", m.MaxAmount),
                                                                      new XElement("Active", m.Active == "Y" ? "Yes" : "No")
                                                                      ));
            DataSet ds = new DataSet();
            ds.ReadXml(xmlApprovalMaster.CreateReader());

            if (ds.Tables.Count <= 0)
            {
                ds.Tables.Add("Activity1");
            }
            return ds;
        }
        #endregion

        #region GetUserListForEdit
        /// <summary>
        /// GetUserListForEdit is providing List of Object for bind ObjectdropDown
        /// </summary>
        /// <returns></returns>
        /// 
        public List<mApprovalLevelDetail> GetUserListForEdit(int approvalid, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mApprovalLevelDetail> UserList = new List<mApprovalLevelDetail>();
            UserList = (from p in ce.mApprovalLevelDetails
                        where p.ApprovalLevelID == approvalid
                        select p).ToList();
            return UserList;
        }
        #endregion

        #region GetUserListForEditbySP
        /// <summary>
        /// GetUserListForEditbySP is providing List of Object for bind Grid
        /// </summary>
        /// <returns></returns>
        /// 
        public List<SP_GetUserForApprovalMaster_Result> GetUserListForEditbySP(int approvalid, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetUserForApprovalMaster_Result> UserList = new List<SP_GetUserForApprovalMaster_Result>();
            UserList = (from p in ce.SP_GetUserForApprovalMaster(approvalid)
                        select p).ToList();
            return UserList;

        }
        #endregion

        #region GetUserListForEditbySPResult
        /// <summary>
        /// GetApprovalRecordToBindGrid is providing List of Approval for bind grid with Actine Yes/No
        /// </summary>
        /// <returns></returns>
        /// 
        public List<SP_GetUserForApprovalMaster_Result> GetUserListForEditbySPResult(int approvalid, string[] conn)
        {

            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetUserForApprovalMaster_Result> userList = new List<SP_GetUserForApprovalMaster_Result>();
            userList = (from p in ce.SP_GetUserForApprovalMaster(approvalid)
                        select p).ToList();
            return userList;
            //XElement xmlApprovalMaster = new XElement("ApprovalUserList", from m in ce.SP_GetUserForApprovalMaster(approvalid).AsEnumerable()

            //                                                          select new XElement("ApprovalUser",
            //                                                          new XElement("ID", m.ID),
            //                                                          new XElement("checked", m.@checked),
            //                                                          new XElement("Name", m.Name),
            //                                                          new XElement("DeptName", m.DeptName),
            //                                                          new XElement("DesiName", m.DesiName),
            //                                                          new XElement("DateOfBirth",string.Format("{0:dd-MMM-yyyy}", m.DateOfBirth)),
            //                                                          new XElement("DateOfJoining",string.Format("{0:dd-MMM-yyyy}", m.DateOfJoining)),
            //                                                          new XElement("EmailID", m.EmailID),
            //                                                          new XElement("MobileNo", m.MobileNo),
            //                                                          new XElement("PhoneNo",m.PhoneNo),
            //                                                          new XElement("UserRole",m.RoleName),
            //                                                          new XElement("Active",m.Active)
            //                                                          ));
            //DataSet ds = new DataSet();
            //ds.ReadXml(xmlApprovalMaster.CreateReader());

            //if (ds.Tables.Count <= 0)
            //{
            //    ds.Tables.Add("ApprovalUser1");
            //}
            //return ds;
        }
        #endregion

        /// <summary>
        /// Save SaveApprovalLevelDetail To DataBase
        /// </summary>
        /// <param name="paraObjectName"></param>
        /// <param name="paraReferenceIDs"></param>
        /// <param name="paraInput"></param>
        /// <param name="paraUserID"></param>
        /// <returns></returns>
        public bool SaveApprovalLevelDetail(string paraUserIDs, long paraApprovalLevelID, mApprovalLevelDetail paraInput, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            bool result = false;
            try
            {
                string[] strings = new string[] { };
                strings = paraUserIDs.Split(',');
                long[] arrayIDs = strings.Select(x => long.Parse(x)).ToArray();

                List<mApprovalLevelDetail> objtApprovalDetail = new List<mApprovalLevelDetail>();

                var ListofID = from a in arrayIDs.AsEnumerable()
                               select new { NewID = a };

                XElement xmlApprovalDetail = new XElement("ApprovalDetailList", from a in ListofID
                                                                                join m in objtApprovalDetail on a.NewID equals m.UserID into newTaskList
                                                                                from newList in newTaskList.DefaultIfEmpty()
                                                                                select new XElement("ApprovalList",
                                                                                  new XElement("ApprovalLevelID", paraInput.ApprovalLevelID),
                                                                                  new XElement("UserID", a.NewID),
                                                                                  new XElement("Active", paraInput.Active == null ? "Y" : paraInput.Active),
                                                                                  new XElement("CreatedBy", paraInput.CreatedBy == null ? "Admin" : paraInput.CreatedBy),
                                                                                  new XElement("CreationDate", paraInput.CreationDate == null ? DateTime.Now : paraInput.CreationDate),
                                                                                  new XElement("LastModifiedBy", paraInput.LastModifiedBy == null ? null : paraInput.LastModifiedBy),
                                                                                  new XElement("LastModifiedDate", paraInput.LastModifiedDate == null ? null : paraInput.LastModifiedDate),
                                                                                  new XElement("CompanyID", paraInput.CompanyID)

                                                                          ));
                ObjectParameter _paraXML = new ObjectParameter("xmlData", typeof(string));
                _paraXML.Value = xmlApprovalDetail.ToString();

                ObjectParameter _paraApprovalLevelID = new ObjectParameter("paraApprovalLevelID", typeof(long));
                _paraApprovalLevelID.Value = paraApprovalLevelID;

                //ObjectParameter _paraUserIDs = new ObjectParameter("paraUserIDs", typeof(string));
                //_paraUserIDs.Value = paraUserIDs;

                ObjectParameter[] obj = new ObjectParameter[] { _paraXML, _paraApprovalLevelID };
                ce.ExecuteFunction("SP_InsetIntoApprovalLevelDetail", obj);
                ce.SaveChanges();

                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                // uc.ErrorTracking(ex, UserID, ConnectionStr);
            }
            return result;
        }


        public Int32 GetApprovalLevel(long CompanyID, long DeptID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            //int approval = (from p in ce.mApprovalLevels
            //                where p.ObjectName == objName
            //                select p.ApprovalLevel).Max();
            var approval = (from db in ce.mTerritories
                            where db.ParentID == CompanyID && db.ID == DeptID
                            select (int?)db.ApprovalLevel).Max();

            return approval == null ? 0 : Convert.ToInt32(approval) + 0;
        }


        public List<SP_GWCGetUserForApprovalMaster_Result> GWCGetUserForApprovalMaster(long CurrentApprovalLevelID, string UserIDS, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GWCGetUserForApprovalMaster_Result> UserList = new List<SP_GWCGetUserForApprovalMaster_Result>();
            UserList = (from p in ce.SP_GWCGetUserForApprovalMaster(CurrentApprovalLevelID, UserIDS)
                        select p).ToList();
            return UserList;

        }

        public List<SP_GWCGetApproverListBySp_Result> GetApproverListBySp(long CurrentApprovalLevelID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GWCGetApproverListBySp_Result> UserList = new List<SP_GWCGetApproverListBySp_Result>();
            UserList = (from p in ce.SP_GWCGetApproverListBySp(CurrentApprovalLevelID)
                        select p).ToList();
            return UserList;

        }

        public List<vGWCGetApprovelevelDetail> GetGWCApprovalRecordToBindGrid(string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<vGWCGetApprovelevelDetail> lst = new List<vGWCGetApprovelevelDetail>();
            lst = (from data in db.vGWCGetApprovelevelDetails
                   select data).ToList();
            return lst;
        }

        public long GetCancelDays(long DeptID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));

            var CancelDays = (from db in ce.mTerritories
                              where db.ID == DeptID
                              select (long?)db.cancelDays).Max();

            return CancelDays == null ? 1 : Convert.ToInt64(CancelDays) + 0;
            //return Convert.ToInt64(CancelDays);

            //return approval == null ? 1 : Convert.ToInt32(approval) + 0;


            //ce.mApprovalLevels.AddObject(approval);
            //ce.SaveChanges();
            //return Convert.ToInt32(approval.ID);
            //// return 1;
        }

        public void InsertGWCApprovalDetails(mApprovalLevelDetail ApprovalDetails, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            try
            {
                ce.mApprovalLevelDetails.AddObject(ApprovalDetails);
                ce.SaveChanges();

            }
            catch { }
        }


        //public List<vGWCGetApprovalDetailsForEdit> GWCGetApprovalDetailsForEdit(long ApprovalLevelID, string[] conn)
        public DataSet GWCGetApprovalDetailsForEdit(long ApprovalLevelID, string[] conn)
        {
            //BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            //List<vGWCGetApprovalDetailsForEdit> UserList = new List<vGWCGetApprovalDetailsForEdit>();
            //UserList = (from p in ce.vGWCGetApprovalDetailsForEdits
            //            where p.ApprovalLevelID == ApprovalLevelID
            //            select p).ToList();
            //return UserList;
            DataSet ds = new DataSet();
            ds = fillds("select * from vGWCGetApprovalDetailsForEdit where ApprovalLevelID = '" + ApprovalLevelID + "'", conn);
            return ds;

        }

        public DataSet GWCGetApprovalDetailForEditWithZero(long ApprovalLevelID, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from vGWCGetApprovalDetailsForEdit where ApprovalLevelID = '" + ApprovalLevelID + "' or ApprovalLevelID = 0 ", conn);
            return ds;
        }

        public DataSet GetApproverList(string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from vGWCGetApproverList where ApprovalLevelID = 0", conn);
            return ds;
        }

        protected DataSet fillds(String strquery, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            DataSet ds = new DataSet();

            System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection("Data Source=" + conn[0] + ";Initial Catalog=" + conn[1] + "; User ID=" + conn[3] + "; Password=" + conn[2] + ";");
            SqlDataAdapter da = new SqlDataAdapter(strquery, sqlConn);
            ds.Reset();
            da.Fill(ds);
            return ds;
        }

        public void DeleteApproverFromGrid(long ID,long UserId, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_DeleteApproverFromGrid";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("Id", ID);
            cmd.Parameters.AddWithValue("UserID", UserId);
            cmd.ExecuteNonQuery();
        }

        public void UpdateApprovallevelID(long ApprovalLevelID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_UpdateApprovalLevelID";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("ApprovalLevelID", ApprovalLevelID);
            cmd.ExecuteNonQuery();
        }

        public void UpdateMApproveHeader(long ID, int ApprovalLevel, int NoOfApprovers, string LastModifiedBy, DateTime LastModifiedDate, long TerritoryID, long OrderCancelInDays, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "UpdateApprovalLevelHeader";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("ID", ID);
            cmd.Parameters.AddWithValue("ApprovalLevel", ApprovalLevel);
            cmd.Parameters.AddWithValue("NoOfApprovers", NoOfApprovers);
            cmd.Parameters.AddWithValue("LastModifiedBy", LastModifiedBy);
            cmd.Parameters.AddWithValue("LastModifiedDate", LastModifiedDate);
            cmd.Parameters.AddWithValue("TerritoryID", TerritoryID);
            cmd.Parameters.AddWithValue("OrderCancelInDays", OrderCancelInDays);
            cmd.ExecuteNonQuery();
        }

        public void DeleteZeroDetailaproval(string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_DeleteZeroDetailaproval";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.ExecuteNonQuery();
        }

        public DataSet GetApprovalgridList(string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_GetApprovalList";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;
        }

        public int GetApprovalRecordByApvrlLevelID(int ApprovalID, long TerritoryID ,string[] conn)
        {
            int cnt=0;
            DataSet ds = new DataSet();
            ds = fillds("select * from mApprovalLevel where ApprovalLevel="+ ApprovalID +" and TerritoryID=" + TerritoryID + "", conn);
            cnt = ds.Tables[0].Rows.Count;
            if (cnt == null) cnt = 0;
            return cnt;
        }


        public string GetApprovalLevelApproverLogicvalue(long ApprovalID,  string[] conn)
        {
            string  result = "";
            DataSet ds = new DataSet();
            ds = fillds("select approverlogic from mApprovalLevelDetail where ApprovalLevelID=" + ApprovalID + "", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                result = ds.Tables[0].Rows[0]["approverlogic"].ToString();
            }         
            return result;
        }
        #region ERP


        public DataSet GetTriggerList(string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_GetReTriggerList";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            da.SelectCommand = cmd;
            da.Fill(ds);
            cmd.Connection.Close();
            return ds;
        }
        public DataSet GetReTriggerOrderHead(long Id, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_GetReTriggerOrderHead";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("Id", Id);
            da.SelectCommand = cmd;
            da.Fill(ds);
            cmd.Connection.Close();
            return ds;
        }
        public DataSet GetReTriggerOrderDetails(long Id, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_GetReTriggerOrderDetails";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("Id", Id);
            da.SelectCommand = cmd;
            da.Fill(ds);
            cmd.Connection.Close();
            return ds;
        }

        public void UpdateReTriggerDetails(long Id, string RequestStatus, string ServiceDescription,string reqeustresponse, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_UpdateReTriggerDetails";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("Id", Id);
            cmd.Parameters.AddWithValue("RequestStatus", RequestStatus);
            cmd.Parameters.AddWithValue("ServiceDescription", ServiceDescription);
            cmd.Parameters.AddWithValue("Reqeustresponse", reqeustresponse);
            cmd.ExecuteNonQuery();
        }
        public void UpdateStatus(long OrderID, string status, string[] conn)
        {
            using (SqlCommand cmd1 = new SqlCommand())
            {
                cmd1.CommandType = CommandType.StoredProcedure;
                cmd1.CommandText = "sp_UpdateTriggerStatus";
                cmd1.Connection = svr.GetSqlConn(conn); ;
                cmd1.Parameters.Clear();
                cmd1.Parameters.AddWithValue("@RequestID", OrderID);
                cmd1.Parameters.AddWithValue("@statuscode", status);
                cmd1.ExecuteNonQuery();

            }
        }
        public DataSet getErpAPROrderhead(long orderID, string[] conn)
        {

            DataSet ds = new DataSet();

            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_getERPAPROrderhead";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@OrderID", orderID);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;

            }

        }
        public DataSet getErpAPROrderdetail(long orderID, string[] conn)
        {

            DataSet ds = new DataSet();

            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_getERPAPROrderdetail";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@OrderID", orderID);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;

            }

        }
        public void InsetERPOrderNotificationLog(long OrderID, long Orderby, long storeID, string[] conn)
        {
            using (SqlCommand cmd1 = new SqlCommand())
            {
                cmd1.CommandType = CommandType.StoredProcedure;
                cmd1.CommandText = "insetErpOrderNotification";
                cmd1.Connection = svr.GetSqlConn(conn);
                cmd1.Parameters.Clear();
                cmd1.Parameters.AddWithValue("@RequestID", OrderID);
                cmd1.Parameters.AddWithValue("@OrderBy", Orderby);
                cmd1.Parameters.AddWithValue("@StoreID", storeID);
                cmd1.ExecuteNonQuery();
            }
        }
        #endregion

    }
}
