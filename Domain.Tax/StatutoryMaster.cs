using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Activation;
using System.Configuration;
using Interface.Tax;
using System.ServiceModel;
using System.Xml.Linq;
using Domain.Server;

namespace Domain.Tax
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]

    public partial class StatutoryMaster : Interface.Tax.iStatutoryMaster
    {
        Domain.Server.Server svr = new Server.Server();
        #region GetStatutoryList
        /// <summary>
        /// GetDesignationList is providing List of Designation
        /// </summary>
        /// <returns></returns>
        /// 
        public List<mStatutory> GetStatutoryList(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mStatutory> Statutory = new List<mStatutory>();      
            Statutory = (from p in ce.mStatutories
                           orderby p.Sequence
                           select p).ToList();
            return Statutory;
        }
        #endregion

        #region InsertmStatutory
        public int InsertmStatutory(mStatutory statutory, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.mStatutories.AddObject(statutory);
            ce.SaveChanges();
            return 1;
        }
        #endregion

        #region updatemStatutory
        public int updatemStatutory(mStatutory updateStatutory, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.mStatutories.Attach(updateStatutory);
            ce.ObjectStateManager.ChangeObjectState(updateStatutory, EntityState.Modified);
            ce.SaveChanges();
            return 1;
        }
        #endregion

        #region GetGetStatutoryListByID
        /// <summary>
        /// GetDesignationListByID is providing List of DesignationList By ID
        /// </summary>
        /// <returns></returns>
        /// 
        public mStatutory GetGetStatutoryListByID(int statutoryId, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
           mStatutory StatutoryID = new mStatutory();
            StatutoryID = (from p in ce.mStatutories
                             where p.ID == statutoryId
                             select p).FirstOrDefault();
            ce.Detach(StatutoryID);
            return StatutoryID;
        }
        #endregion

        #region checkDuplicateRecord
        /// <summary>
        /// checkDuplicateRecord is providing List of Statutory by StatutoryName and ObjectName
        /// </summary>
        /// <returns></returns>
        /// 
        public string checkDuplicateRecord(string statutoryName, string objName, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            var output = (from p in ce.mStatutories
                          where p.Name == statutoryName && p.ObjectName == objName
                          select new { p.Name }).FirstOrDefault();
            if (output != null)
            {     
                result = "[ " + statutoryName + " ]  Statutory Name for the Object [ " + objName + " ] already exist";
            }
            return result;
        }
        #endregion

        #region checkDuplicateRecordEdit
        /// <summary>
        /// checkDuplicateRecord for Edit is providing List of Statutory by StatutoryName And ObjectName And StatutoryID
        /// </summary>
        /// <returns></returns>
        /// 
        public string checkDuplicateRecordEdit(int statutoryID, string statutoryName, string objName, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            var output = (from p in ce.mStatutories
                          where p.Name == statutoryName && p.ID != statutoryID && p.ObjectName == objName
                          select new { p.Name }).FirstOrDefault();

            if (output != null)
            {      
                result = "[ " + statutoryName + " ]  Statutory Name for the Object [ " + objName + " ] already exist";
            }
            return result;

        }
        #endregion

        #region GetStatutoryRecordToBindGrid
        /// <summary>
        /// GetStatutoryRecordToBindGrid is providing List of Statutory for bind grid with Actine Yes/No
        /// </summary>
        /// <returns></returns>
        /// 
        public DataSet GetStatutoryRecordToBindGrid(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mStatutory> LeadSource = new List<mStatutory>();
            XElement xmlStatutoryMaster = new XElement("StatutoryList", from m in ce.mStatutories.AsEnumerable()
                                                                            orderby m.Sequence
                                                                            select new XElement("Statutory",
                                                                            new XElement("ID", m.ID),                                                                           
                                                                            new XElement("ObjectName", m.ObjectName),
                                                                            new XElement("Name", m.Name),
                                                                            new XElement("Sequence", m.Sequence),
                                                                            new XElement("Remark", m.Remark),
                                                                            new XElement("Active", m.Active == "Y" ? "Yes" : "No")
                                                                            ));
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            ds.ReadXml(xmlStatutoryMaster.CreateReader());
            if (ds.Tables.Count <= 0)
            {
                dt = ds.Tables.Add("Statutory1");
            }
            return ds;
        }
        #endregion
    }
}
