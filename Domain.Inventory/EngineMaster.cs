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
using Domain.Server;
using System.Data.Objects;
using Interface.Inventory;
namespace Domain.Inventory
{

    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]

    public partial class EngineMaster : Interface.Inventory.iEngineMaster
    {
        Domain.Server.Server svr = new Server.Server();
        #region GetEngineMasterList
        /// <summary>
        /// GetDeparmentList is providing List of SiteMaste
        /// </summary>
        /// <returns></returns>
        /// 
        public List<v_GetEngineDetails> GetEngineMasterList(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<v_GetEngineDetails> v_GetEngineDetailsList = new List<v_GetEngineDetails>();
            v_GetEngineDetailsList = (from p in ce.v_GetEngineDetails
                                      select p).ToList();
            if (v_GetEngineDetailsList.Count == 0)
            {
                v_GetEngineDetailsList = null;
            }
            return v_GetEngineDetailsList;
        }
        #endregion

        #region InsertmEngine
        public int InsertmEngine(mEngine SiteMaster, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)); ;
            ce.mEngines.AddObject(SiteMaster);
            ce.SaveChanges();
            return 1;
        }
        #endregion

        #region updatemEngine
        public int updatemEngine(mEngine ObjmEngine, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)); ;
            ce.mEngines.Attach(ObjmEngine);
            ce.ObjectStateManager.ChangeObjectState(ObjmEngine, EntityState.Modified);
            ce.SaveChanges();
            return 1;
        }
        #endregion

        #region GetmEngineListByID
        /// <summary>
        /// GetDepartmentListByID is providing List of DepartmentList ByID
        /// </summary>
        /// <returns></returns>
        /// 
        public v_GetEngineDetails GetmEngineListByID(int EngineId, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            v_GetEngineDetails objv_GetEngineDetails = new v_GetEngineDetails();
            objv_GetEngineDetails = ce.v_GetEngineDetails.Where(p => p.ID == EngineId).FirstOrDefault();
            return objv_GetEngineDetails;
        }
        #endregion

        #region GetEngineOfSite
        /// <summary>
        /// GetEngineOfSite is providing List of Engine Numbers of Selected Site
        /// </summary>
        /// <returns></returns>
        /// 
        public DataSet GetEngineOfSite(string SId, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            DataSet ds = new DataSet();
            ds.Reset();
            string str = "declare @t varchar(5000);set @t='" + SId + "' select Engineserial from v_GetEngineDetails where Territory in (select part from SplitString(@t,',') )";
            ds = fillds(str, conn);
            return ds;
        }
        #endregion

        protected DataSet fillds(string strquery, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            DataSet ds = new DataSet();
            System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection("Data Source=" + conn[0] + ";Initial Catalog=" + conn[1] + "; User ID=" + conn[3] + "; Password=" + conn[2] + ";");
            SqlDataAdapter da = new SqlDataAdapter(strquery, sqlConn);
            ds.Reset();
            da.Fill(ds);
            return ds;
        }

        

    }
}
