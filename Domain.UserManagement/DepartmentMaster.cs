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

    public partial class DepartmentMaster : Interface.UserManagement.iDepartmentMaster
    {
        Domain.Server.Server svr = new Server.Server();

        #region GetDeparmentList
        /// <summary>
        /// GetDeparmentList is providing List of Deparment
        /// </summary>
        /// <returns></returns>
        /// 
        public List<mDepartment> GetDeparmentList(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mDepartment> Department = new List<mDepartment>();
            Department = (from p in ce.mDepartments
                          where p.Active == "Y"
                          orderby p.Sequence
                          select p).ToList();
            if (Department.Count == 0)
            {
                Department = null;
            }
            return Department;
        }
        #endregion

        #region InsertmDepartment
        public int InsertmDepartment(mDepartment Department, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)); ;
            ce.mDepartments.AddObject(Department);
            ce.SaveChanges();
            return 1;
        }
        #endregion

        #region updatemDepartment
        public int updatemDepartment(mDepartment updateDepartment, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)); ;
            //string result = "";

            ce.mDepartments.Attach(updateDepartment);
            ce.ObjectStateManager.ChangeObjectState(updateDepartment, EntityState.Modified);
            ce.SaveChanges();
            return 1;
            //if (ce.SaveChanges != null)
            //{
            //}
            //return result;

        }
        #endregion

        #region GetDepartmentListByID
        /// <summary>
        /// GetDepartmentListByID is providing List of DepartmentList ByID
        /// </summary>
        /// <returns></returns>
        /// 
        public mDepartment GetDepartmentListByID(int DepartmentId, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)); ;
            mDepartment DepartmentID = new mDepartment();
            DepartmentID = (from p in ce.mDepartments
                            where p.ID == DepartmentId
                            select p).FirstOrDefault();
            ce.Detach(DepartmentID);
            return DepartmentID;
        }
        #endregion

        #region checkDuplicateRecord
        /// <summary>
        /// checkDuplicateRecord is providing List of Department by DepartmentName 
        /// </summary>
        /// <returns></returns>
        /// 
        public string checkDuplicateRecord(string DepartmentName, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)); ;
            string result = "";
            var output = (from p in ce.mDepartments
                          where p.Name == DepartmentName
                          select new { p.Name }).FirstOrDefault();
            if (output != null)
            {
                result = "[ " + DepartmentName + " ] Department Name allready exist";
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
        public string checkDuplicateRecordEdit(int DepartmentID, string DepartmentName, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)); ;
            string result = "";
            var output = (from p in ce.mDepartments
                          where p.Name == DepartmentName && p.ID != DepartmentID
                          select new { p.Name }).FirstOrDefault();
            if (output != null)
            {
                result = "[ " + DepartmentName + " ] Department Name  allready exist";
            }
            return result;
        }
        #endregion

        #region GetDepartmentRecordToBind
        /// <summary>
        /// GetDepartmentRecordToBind is providing List of Department for bind grid with Actine Yes/No
        /// </summary>
        /// <returns></returns>
        /// 
        public DataSet GetDepartmentRecordToBind(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)); ;
            List<mDepartment> LeadSource = new List<mDepartment>();
            XElement xmlDepartmentMaster = new XElement("DepartmentList", from m in ce.mDepartments.AsEnumerable()
                                                                          orderby m.Sequence
                                                                          select new XElement("Department",
                                                                          new XElement("ID", m.ID),
                                                                          new XElement("Name", m.Name),
                                                                          new XElement("Sequence", m.Sequence),
                                                                          new XElement("Active", m.Active == "Y" ? "Yes" : "No")
                                                                          ));
            DataSet ds = new DataSet();
            ds.ReadXml(xmlDepartmentMaster.CreateReader());
            DataTable dt = new DataTable();
            if (ds.Tables.Count <= 0)
            {
                dt = ds.Tables.Add("Department1");
            }
            return ds;
        }
        #endregion
    }
}
