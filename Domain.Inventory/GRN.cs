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
     public partial class GRN : Interface.Inventory.iGRN
    {
         Domain.Server.Server svr = new Server.Server();

         #region GetGRNByID
         /// <summary>
         /// GetGRNByID is providing List of GRN ByID
         /// </summary>
         /// <returns></returns>
         /// 
         public tGRN GetGRNByID(long RecID, string[] conn)
         {
             BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
             tGRN GRN = new tGRN();
             GRN = ce.tGRNs.Where(p => p.GRN_ID == RecID).FirstOrDefault();
             ce.Detach(GRN);
             return GRN;
         }
         #endregion
         #region GetGRNDetailsByID
         /// <summary>
         /// GetGRNDetailsByID is providing List of GRN Details ByID
         /// </summary>
         /// <returns></returns>
         /// 
         public tGRNDetail GetGRNDetailsByID(long RecID, string[] conn)
         {
             BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
             tGRNDetail GRNDetails = new tGRNDetail();
             GRNDetails = ce.tGRNDetails.Where(p => p.GRN_ID == RecID).FirstOrDefault();
             ce.Detach(GRNDetails);
             return GRNDetails;
         }
         #endregion
         #region InsertGRN
         public long InsertGRN(tGRN  objGRN, string[] conn)
         {
             BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
             ce.tGRNs.AddObject(objGRN );
             ce.SaveChanges();
             return objGRN.GRN_ID;
         }
         #endregion

         #region updateGRN
         public long updateGRN(tGRN  objGRN, string[] conn)
         {
             BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
             ce.tGRNs.Attach(objGRN);
             ce.ObjectStateManager.ChangeObjectState(objGRN, EntityState.Modified);
             ce.SaveChanges();
             return 1;
         }
         #endregion
         #region InsertGRNDetails
         public long InsertGRNDetails(DataTable dt, long GRN_ID, string[] conn)
         {
             try
             {
                 DataTable dtPR = new DataTable();
                 SqlDataAdapter da = new SqlDataAdapter("Select * from tGRNDetail where GRN_ID = 0", svr.GetSqlConn(conn));
                 da.Fill(dtPR);

                 int SrNo = 1;
                 foreach (DataRow dr in dt.Rows)
                 {
                     dtPR.Rows.Add();
                     dtPR.Rows[dtPR.Rows.Count - 1]["GRN_ID"] = GRN_ID;
                     dtPR.Rows[dtPR.Rows.Count - 1]["Prod_ID"] = dr["ID"];
                     //dtPR.Rows[dtPR.Rows.Count - 1]["Prod_ID"] = dr["PartDesc"];
                     dtPR.Rows[dtPR.Rows.Count - 1]["ReceiptQty"] = dr["GRNQty"];
                     dtPR.Rows[dtPR.Rows.Count - 1]["Remark"] = "";
                     //dtPR.Rows[dtPR.Rows.Count - 1]["Sequence"] = SrNo;
                     // dtPR.Rows[dtPR.Rows.Count - 1]["ProbableShippingDt"] = null;
                     SrNo += 1;
                 }

                 SqlCommand cmd = new SqlCommand("Delete from dbo.tGRNDetail where GRN_ID = " + GRN_ID, svr.GetSqlConn(conn));
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
         //#region UpdateGRNDetails
         //public long UpdateMaterialIssueDetail(tMINDetail objMaterialIssueDetails, string[] conn)
         //{
         //    BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
         //    ce.tMINDetails.Attach(objMaterialIssueDetails);
         //    ce.ObjectStateManager.ChangeObjectState(objMaterialIssueDetails, EntityState.Modified);
         //    ce.SaveChanges();

         //    return 1;
         //}
         //#endregion
    }
}
