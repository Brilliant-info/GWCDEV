using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Activation;
using System.Configuration;
//using ElegantCRM.Model;
using System.ServiceModel;
using Domain.Server;
using System.Xml.Linq;
using System.Data.Objects;
using Interface.PowerOnRent;

namespace Domain.PowerOnRent
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]

    public partial class SiteMaster : iSiteMaster
    {
        Domain.Server.Server svr = new Server.Server();

        #region GetSiteMasterList
        /// <summary>
        /// GetDeparmentList is providing List of SiteMaste
        /// </summary>
        /// <returns></returns>
        /// 
        public List<v_GetSiteDetails> GetmTerritoryList(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<v_GetSiteDetails> SiteMaster = new List<v_GetSiteDetails>();
            SiteMaster = (from p in ce.v_GetSiteDetails
                          select p).ToList();
            if (SiteMaster.Count == 0)
            {
                SiteMaster = null;
            }
            return SiteMaster;
        }
        #endregion

        #region InsertmTerritory
        public long InsertSiteMaster(mTerritory SiteMaster, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.mTerritories.AddObject(SiteMaster);
            ce.SaveChanges();
            return SiteMaster.ID;

        }
        #endregion

        #region updatemTerritory
        public long updatemTerritory(mTerritory ObjmTerritory, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.mTerritories.Attach(ObjmTerritory);
            ce.ObjectStateManager.ChangeObjectState(ObjmTerritory, EntityState.Modified);
            ce.SaveChanges();
            return 1;
        }
        #endregion

        #region GetTerritoryListByID
        /// <summary>
        /// GetDepartmentListByID is providing List of DepartmentList ByID
        /// </summary>
        /// <returns></returns>
        /// 
        public v_GetSiteDetails GetTerritoryListByID(long TerritoryId, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)); ;
            v_GetSiteDetails Territory = new v_GetSiteDetails();
            Territory = ce.v_GetSiteDetails.Where(p => p.ID == TerritoryId).FirstOrDefault();
            ce.Detach(Territory);
            return Territory;
        }
        #endregion

        #region checkDuplicateRecord
        /// <summary>
        /// checkDuplicateRecord is providing List of Territory by TerritoryName 
        /// </summary>
        /// <returns></returns>
        /// 
        public string checkDuplicateRecord(string TerritoryName, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)); ;
            string result = "";
            var output = (from p in ce.mTerritories
                          where p.Territory == TerritoryName
                          select new { p.Territory }).FirstOrDefault();
            if (output != null)
            {
                result = "[ " + TerritoryName + " ] Site Name already exist";
            }
            return result;
        }
        #endregion

        #region checkDuplicateRecordEdit
        /// <summary>
        /// checkDuplicateRecord for Edit is providing List of Territory by TerritoryName and ID
        /// </summary>
        /// <returns></returns>
        /// 
        public string checkDuplicateRecordEdit(int TerritorytID, string TerritoryName, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            var output = (from p in ce.mTerritories
                          where p.Territory == TerritoryName && p.ID != TerritorytID
                          select new { p.Territory }).FirstOrDefault();
            if (output != null)
            {
                result = "[ " + TerritoryName + " ] Site Name already exist";
            }
            return result;
        }
        #endregion

        public List<mTerritory> GetSiteDtls(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mTerritory> SiteList = new List<mTerritory>();
            SiteList = ce.mTerritories.Where(p => p.GroupTitle == "Site").ToList();
            return SiteList;
        }

        #region InsertSiteAddress
        public long InsertSiteAddress(tAddress SiteAddress, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.tAddresses.AddObject(SiteAddress);
            ce.SaveChanges();
            return 1;
        }
        #endregion

        #region updateSiteAddress
        public long updateSiteAddress(tAddress ObjAddress, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.tAddresses.Attach(ObjAddress);
            ce.ObjectStateManager.ChangeObjectState(ObjAddress, EntityState.Modified);
            ce.SaveChanges();
            return 1;
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

        public DataSet GetTableList(string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from mDropdownValues where Parameter = 'Table'", conn);
            return ds;
        }

        public DataSet GetDataTypeList(string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from mDropdownValues where Parameter = 'DataType'", conn);
            return ds;
        }

        public void InsertInterface(string Tablename, string DataType, string FieldName, string IsNull, long CreatedBy, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_Instert_InterfaceDetails";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("TableName", Tablename);
            cmd.Parameters.AddWithValue("FieldDataType", DataType);
            cmd.Parameters.AddWithValue("Fieldname", FieldName);
            cmd.Parameters.AddWithValue("IsNull", IsNull);
            cmd.Parameters.AddWithValue("CreatedBy", CreatedBy);
            cmd.ExecuteNonQuery();
        }

        public void UpdateInterface(long ID, string Tablename, string DataType, string FieldName, string IsNull, long ModifyBy, string[] conn)
        {

            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_Update_InterfaceDetails";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("ID", ID);
            cmd.Parameters.AddWithValue("Fieldname", FieldName);
            cmd.Parameters.AddWithValue("FieldDataType", DataType);
            cmd.Parameters.AddWithValue("IsNull", IsNull);
            cmd.Parameters.AddWithValue("TableName", Tablename);
            cmd.Parameters.AddWithValue("ModifyBy", ModifyBy);
            cmd.ExecuteNonQuery();
        }
    }
   
}
