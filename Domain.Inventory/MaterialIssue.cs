using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Activation;
using System.Configuration;
using Interface.Inventory;
using System.ServiceModel;

using System.Xml.Linq;
using System.Data.Objects;
using Domain.Server;

namespace Domain.Inventory
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public partial class MaterialIssue : Interface.Inventory.iMaterialIssue
    {
        Domain.Server.Server svr = new Server.Server();

        #region GetRequisitionByID
        /// <summary>
        /// GetRequisitionByID is providing List of Material Issue ByID
        /// </summary>
        /// <returns></returns>
        /// 
        public tMIN GetMaterialIssueByID(long RecID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            tMIN Issue = new tMIN();
            Issue = ce.tMINs.Where(p => p.MIN_ID == RecID).FirstOrDefault();
            ce.Detach(Issue);
            return Issue;
        }
        #endregion
        #region GetMaterialIssueDetailsByID
        /// <summary>
        /// GetMaterialIssueDetailsByID is providing List of Material Issue Details ByID
        /// </summary>
        /// <returns></returns>
        /// 
        public tMINDetail GetMaterialIssueDetailsByID(long RecID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            tMINDetail IssueDetails = new tMINDetail();
            IssueDetails = ce.tMINDetails.Where(p => p.MIN_ID == RecID).FirstOrDefault();
            ce.Detach(IssueDetails);
            return IssueDetails;
        }
        #endregion
        #region InsertMaterialIssue
        public long InsertMaterialIssue(tMIN objMaterialIssue, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.tMINs.AddObject(objMaterialIssue);
            ce.SaveChanges();
            return objMaterialIssue.MIN_ID;
        }
        #endregion

        #region updateMaterialIssue
        public long updateMaterialIssue(tMIN objMaterialIssue, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.tMINs.Attach(objMaterialIssue);
            ce.ObjectStateManager.ChangeObjectState(objMaterialIssue, EntityState.Modified);
            ce.SaveChanges();
            return 1;
        }
        #endregion
        #region InsertMaterialIssueDetails
        public long InsertMaterialIssueDetails(DataTable dt, long PRM_ID, string[] conn)
        {
            try
            {
                DataTable dtPR = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter("Select * from tMINDetail where MIN_ID = 0", svr.GetSqlConn(conn));
                da.Fill(dtPR);

                int SrNo = 1;
                foreach (DataRow dr in dt.Rows)
                {
                    dtPR.Rows.Add();
                    dtPR.Rows[dtPR.Rows.Count - 1]["MIN_ID"] = PRM_ID;
                    dtPR.Rows[dtPR.Rows.Count - 1]["PRD_ID"] = dr["ID"];
                    if (dr["Prod_ID"].ToString() != "")
                    {
                        dtPR.Rows[dtPR.Rows.Count - 1]["Prod_ID"] = dr["Prod_ID"];
                    }
                    dtPR.Rows[dtPR.Rows.Count - 1]["IssueQty"] = dr["IssueQty"];
                    dtPR.Rows[dtPR.Rows.Count - 1]["Remark"] = "";
                    dtPR.Rows[dtPR.Rows.Count - 1]["Sequence"] = SrNo;
                    dtPR.Rows[dtPR.Rows.Count - 1]["Prod_Rate"] = 0;
                    // dtPR.Rows[dtPR.Rows.Count - 1]["ProbableShippingDt"] = null;
                    SrNo += 1;
                }

                SqlCommand cmd = new SqlCommand("Delete from dbo.tMINDetail where MIN_ID = " + PRM_ID, svr.GetSqlConn(conn));
                cmd.Connection = svr.GetSqlConn(conn);
                if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                if (cmd.Connection.State == ConnectionState.Open) cmd.Connection.Close();
                SqlCommandBuilder cb = new SqlCommandBuilder(da);
                da.Update(dtPR);
                return 1;
            }
            catch { }
            finally { }
            return 0;
        }
        #endregion
        #region UpdateMaterialIssueDetail
        public long UpdateMaterialIssueDetail(tMINDetail objMaterialIssueDetails, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.tMINDetails.Attach(objMaterialIssueDetails);
            ce.ObjectStateManager.ChangeObjectState(objMaterialIssueDetails, EntityState.Modified);
            ce.SaveChanges();

            return 1;
        }
        #endregion


     
         

    }
}
