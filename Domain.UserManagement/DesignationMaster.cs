using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Activation;
using System.Configuration;
using Interface.UserManagement;
using System.ServiceModel;
using System.Xml.Linq;
using Domain.Server;

namespace Domain.UserManagement
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]

    public partial class DesignationMaster : Interface.UserManagement.iDesignationMaster
    {
        Domain.Server.Server svr = new Server.Server();

        #region GetDesignationList
        /// <summary>
        /// GetDesignationList is providing List of Designation
        /// </summary>
        /// <returns></returns>
        /// 
        public List<mDesignation> GetDesignationList(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mDesignation> Designation = new List<mDesignation>();
            Designation = (from p in ce.mDesignations
                           where p.Active == "Y"
                           orderby p.Sequence
                           select p).ToList();
            return Designation;
        }
        #endregion

        #region InsertmDesignation
        public int InsertmDesignation(mDesignation designation, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.mDesignations.AddObject(designation);
            ce.SaveChanges();
            return 1;
        }
        #endregion

        #region updatemDesignation
        public int updatemDesignation(mDesignation updateDesignation, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.mDesignations.Attach(updateDesignation);
            ce.ObjectStateManager.ChangeObjectState(updateDesignation, EntityState.Modified);
            ce.SaveChanges();
            return 1;
        }
        #endregion

        #region GetDesignationListByID
        /// <summary>
        /// GetDesignationListByID is providing List of DesignationList By ID
        /// </summary>
        /// <returns></returns>
        /// 
        public mDesignation GetDesignationListByID(int designationId, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            mDesignation DesignationID = new mDesignation();
            DesignationID = (from p in ce.mDesignations
                             where p.ID == designationId
                             select p).FirstOrDefault();
            ce.Detach(DesignationID);
            return DesignationID;

        }
        #endregion

        #region checkDuplicateRecord
        /// <summary>
        /// checkDuplicateRecord is providing List of Designation by DesignationName Department
        /// </summary>
        /// <returns></returns>
        /// 
        public string checkDuplicateRecord(string designationName, int deptID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            var output = (from p in ce.mDesignations
                          where p.Name == designationName && p.DepartmentID == deptID
                          select new { p.Name }).FirstOrDefault();

            if (output != null)
            {
                result = "[ " + designationName + " ]  Designation for the Department allready exist";
            }
            return result;
        }
        #endregion

        #region checkDuplicateRecordEdit
        /// <summary>
        /// checkDuplicateRecord for Edit is providing List of Department by DepartmentName and ID
        /// </summary>
        /// <returns></returns>
        /// 
        public string checkDuplicateRecordEdit(int designationID, string designationName, int deptID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            var output = (from p in ce.mDesignations
                          where p.Name == designationName && p.ID != designationID && p.DepartmentID == deptID
                          select new { p.Name }).FirstOrDefault();


            if (output != null)
            {
                result = "[ " + designationName + " ] Designation for the Department allready exist";
            }
            return result;
        }
        #endregion

        #region GetDesignationRecordToBind
        /// <summary>
        /// GetDesignationRecordToBind is providing List of Department for bind grid with Actine Yes/No
        /// </summary>
        /// <returns></returns>
        /// 
        public DataSet GetDesignationRecordToBind(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<vDesignation> LeadSource = new List<vDesignation>();
            XElement xmlDesignationMaster = new XElement("DesignationList", from m in ce.vDesignations.AsEnumerable()
                                                                            orderby m.Sequence
                                                                            select new XElement("Designation",
                                                                          new XElement("ID", m.ID),
                                                                          new XElement("Name", m.Name),
                                                                          new XElement("Department", m.Department),
                                                                          new XElement("Sequence", m.Sequence),
                                                                          new XElement("Active", m.Active == "Y" ? "Yes" : "No")
                                                                          ));
            DataSet ds = new DataSet();
            ds.ReadXml(xmlDesignationMaster.CreateReader());
            DataTable dt = new DataTable();
            if (ds.Tables.Count <= 0)
            {
                dt = ds.Tables.Add("Designation1");
            }
            return ds;
        }
        #endregion

        #region GetDesignationListByDepartmentID
        /// <summary>
        /// GetDesignationList is providing List of Designation
        /// </summary>
        /// <returns></returns>
        /// 
        public List<mDesignation> GetDesignationListByDepartmentID(int dept, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mDesignation> Designation = new List<mDesignation>();
            Designation = (from p in ce.mDesignations
                           where (p.DepartmentID == dept) && (p.Active == "Y")
                           orderby p.Sequence
                           select p).ToList();

            return Designation;
        }
        #endregion


        #region GetDesignationListByID
        /// <summary>
        /// GetDesignationListByID is providing List of DesignationList By ID
        /// </summary>
        /// <returns></returns>
        /// 
        public long GetDesignationIDByName(string designation, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            mDesignation DesignationID = new mDesignation();
            DesignationID = (from p in ce.mDesignations
                             where p.Name == designation
                             select p).FirstOrDefault();
            ce.Detach(DesignationID);
            return DesignationID.ID;

        }
        #endregion


        #region GetDesignationListByCompanyanddepartment 

        public List<mDesignation> GetDesignationListBycompanddept(long CompanyID, long dept, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mDesignation> Designation = new List<mDesignation>();
            Designation = (from p in ce.mDesignations
                           where (p.TerritoryID == dept) && (p.CompanyID == CompanyID)
                           orderby p.Sequence
                           select p).ToList();

            return Designation;
        }
        #endregion

    }
}
