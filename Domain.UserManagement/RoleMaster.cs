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
using System.Data.Objects;
using Domain.Server;

namespace Domain.UserManagement
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]

    public partial class RoleMaster : Interface.UserManagement.iRoleMaster
    {
        Domain.Server.Server svr = new Server.Server();

        /// <summary>
        /// GetDataToBindRoleMasterDetailsByRoleID
        /// </summary>
        /// <param name="RoleID"></param>
        /// <param name="CompanyID"></param>
        /// <returns></returns>
        public List<SP_GetDataToBindRoleMaster_Result> GetDataToBindRoleMasterDetailsByRoleID(long RoleID, long CompanyID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetDataToBindRoleMaster_Result> lst = new List<SP_GetDataToBindRoleMaster_Result>();
            lst = (from dbtable in db.SP_GetDataToBindRoleMaster(RoleID, CompanyID)
                   orderby (long)(dbtable.mSequence), (long)(dbtable.pSequence), (long)(dbtable.oSequence)
                   select dbtable).ToList();
            return lst;
        }

        public List<SP_GetDataToBindRoleMaster_Result> UpdateRoleIntoSessionList(List<SP_GetDataToBindRoleMaster_Result> sessionList, SP_GetDataToBindRoleMaster_Result updateRole, int rowindex, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            SP_GetDataToBindRoleMaster_Result findRow = new SP_GetDataToBindRoleMaster_Result();
            findRow = sessionList.Where(s => s.mSequence == updateRole.mSequence && s.pSequence == updateRole.pSequence && s.oSequence == updateRole.oSequence).FirstOrDefault();
            if (findRow != null)
            {
                sessionList = sessionList.Where(s => s != findRow).ToList();
                findRow.Add = updateRole.Add;
                findRow.Edit = updateRole.Edit;
                findRow.View = updateRole.View;
                findRow.Delete = updateRole.Delete;
                findRow.Approval = updateRole.Approval;
                findRow.AssignTask = updateRole.AssignTask;
            }
            sessionList.Add(findRow);
            return sessionList;
        }

        public void FinalSave(List<SP_GetDataToBindRoleMaster_Result> sessionList, mRole objmRole, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            if (objmRole.ID == 0) 
            { 
                db.AddTomRoles(objmRole); db.SaveChanges(); 
            }
            else 
            { 
                db.mRoles.Attach(objmRole); 
                db.ObjectStateManager.ChangeObjectState(objmRole, EntityState.Modified); db.SaveChanges(); 
            }

            if (sessionList.Count > 0)
            {
                XElement xmlEle = new XElement("RoleMasterList", from rec in sessionList.AsEnumerable()
                                                                 select new XElement("RoleMaster",
                                                                new XElement("ObjectName", rec.ObjectName),
                                                                new XElement("Add", rec.Add),
                                                                new XElement("Edit", rec.Edit),
                                                                new XElement("View", rec.View),
                                                                new XElement("Delete", rec.Delete),
                                                                new XElement("Approval", rec.Approval),
                                                                new XElement("AssignTask", rec.AssignTask)
                                                                ));

                ObjectParameter _paraXmlData = new ObjectParameter("xmlData", typeof(string));
                _paraXmlData.Value = xmlEle.ToString();

                ObjectParameter _paraRoleID = new ObjectParameter("paraRoleID", typeof(long));
                _paraRoleID.Value = objmRole.ID;

                ObjectParameter _paraCompanyID = new ObjectParameter("paraCompanyID", typeof(long));
                _paraCompanyID.Value = objmRole.CompanyID;

                ObjectParameter[] obj = new ObjectParameter[] { _paraXmlData, _paraRoleID, _paraCompanyID };
                db.ExecuteFunction("SP_InsertIntoRoleMasterDetail", obj);
                db.SaveChanges();
            }
        }

        public mRole GetmRoleByID(long roleId, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            mRole obj = new mRole();
            obj = db.mRoles.Where(r => r.ID == roleId).FirstOrDefault();
            if (obj != null) db.mRoles.Detach(obj);
            return obj;
        }

        public List<vGetRoleMasterData> BindRoleMasterSummary(string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<vGetRoleMasterData> lst = new List<vGetRoleMasterData>();
            lst = (from data in db.vGetRoleMasterDatas
                   orderby data.DeptSequence, data.DesigSequence, data.mrSequence
                   select data).ToList();
            return lst;
        }

        public List<vGetGWCRoleMasterData> BindGWCRoleMasterSummary(string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<vGetGWCRoleMasterData> lst = new List<vGetGWCRoleMasterData>();
            lst = (from data in db.vGetGWCRoleMasterDatas
                   select data).ToList();
            return lst;
        }


        public List<vGetRoleMasterData> GetRoleMasterListByDepartmentIDDesignationID(long DepartmentID, long DesignationID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<vGetRoleMasterData> lst = new List<vGetRoleMasterData>();
            lst = (from data in db.vGetRoleMasterDatas
                   where data.DepartmentID == DepartmentID && data.DesignationID == DesignationID
                   orderby data.DeptSequence, data.DesigSequence, data.mrSequence
                   select data).ToList();
            return lst;
           

        }

        public List<VGetRollNameByDeptID> GetRoleMasterListByDepartmentID(long DepartmentID, long DesignationID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<VGetRollNameByDeptID> lst = new List<VGetRollNameByDeptID>();
            lst = (from data in db.VGetRollNameByDeptIDs
                   where data.TerritoryID == DepartmentID
                   select data).ToList();
            return lst;

        }



        #region checkDuplicateRecordForRoleName
        /// <summary>
        /// checkDuplicateRecord is providing List of vendor by VendorName 
        /// </summary>
        /// <returns></returns>
        /// 
        public string checkDuplicateRecord(string RoleName, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            var output = (from p in ce.mRoles
                          where p.RoleName == RoleName
                          select new { p.RoleName }).FirstOrDefault();

            if (output != null)
            {
                result = "[ " + RoleName + " ] Role Name already exist";
            }
            return result;

        }
        #endregion

        #region checkDuplicateRecordForRoleNameWithId
        /// <summary>
        /// checkDuplicateRecord for Edit is providing List of Department by Vendor Name and ID
        /// </summary>
        /// <returns></returns>
        /// 
        public string checkDuplicateRecordEdit(int RoleId, string RoleName, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            var output = (from p in ce.mRoles
                          where p.RoleName == RoleName && p.ID != RoleId
                          select new { p.RoleName }).FirstOrDefault();


            if (output != null)
            {
                result = "[ " + RoleName + " ] Role Name  already exist";
            }
            return result;

        }
        #endregion

        public List<SP_GWCGetDataToBindRoleMaster_Result> GetGWCDataToBindRoleMasterDetailsByRoleID(long RoleID, long CompanyID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GWCGetDataToBindRoleMaster_Result> lst = new List<SP_GWCGetDataToBindRoleMaster_Result>();
            lst = (from dbtable in db.SP_GWCGetDataToBindRoleMaster(RoleID, CompanyID)
                   orderby (long)(dbtable.mSequence), (long)(dbtable.pSequence), (long)(dbtable.oSequence)
                   select dbtable).ToList();
            return lst;
        }

        public void FinalSaveRole(List<SP_GWCGetDataToBindRoleMaster_Result> sessionList, mRole objmRole, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            if (objmRole.ID == 0)
            {
                db.AddTomRoles(objmRole); db.SaveChanges();
            }
            else
            {
                db.mRoles.Attach(objmRole);
                db.ObjectStateManager.ChangeObjectState(objmRole, EntityState.Modified); db.SaveChanges();
            }

            if (sessionList.Count > 0)
            {
                XElement xmlEle = new XElement("RoleMasterList", from rec in sessionList.AsEnumerable()
                                                                 select new XElement("RoleMaster",
                                                                new XElement("ObjectName", rec.ObjectName),
                                                                new XElement("Add", rec.Add),
                                                                new XElement("Edit", rec.Edit),
                                                                new XElement("View", rec.View),
                                                                new XElement("Delete", rec.Delete),
                                                                new XElement("Approval", rec.Approval),
                                                                new XElement("AssignTask", rec.AssignTask)
                                                                ));

                ObjectParameter _paraXmlData = new ObjectParameter("xmlData", typeof(string));
                _paraXmlData.Value = xmlEle.ToString();

                ObjectParameter _paraRoleID = new ObjectParameter("paraRoleID", typeof(long));
                _paraRoleID.Value = objmRole.ID;

                ObjectParameter _paraCompanyID = new ObjectParameter("paraCompanyID", typeof(long));
                _paraCompanyID.Value = objmRole.CompanyID;

                ObjectParameter[] obj = new ObjectParameter[] { _paraXmlData, _paraRoleID, _paraCompanyID };
                db.ExecuteFunction("SP_InsertIntoRoleMasterDetail", obj);
                db.SaveChanges();
            }
        }


        public List<mRole> GetRoleList(string[] conn)
        {

            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mRole> Rolelist = new List<mRole>();
            Rolelist = (from l in db.mRoles
                   select l).ToList();
            return Rolelist;
        }

        public List<SP_GWCGetDataToBindRoleMaster_Result> GWCUpdateRoleIntoSessionList(List<SP_GWCGetDataToBindRoleMaster_Result> sessionList, SP_GWCGetDataToBindRoleMaster_Result updateRole, int rowindex, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            SP_GWCGetDataToBindRoleMaster_Result findRow = new SP_GWCGetDataToBindRoleMaster_Result();
            findRow = sessionList.Where(s => s.mSequence == updateRole.mSequence && s.pSequence == updateRole.pSequence && s.oSequence == updateRole.oSequence).FirstOrDefault();
            if (findRow != null)
            {
                sessionList = sessionList.Where(s => s != findRow).ToList();
                findRow.Add = updateRole.Add;
                findRow.Edit = updateRole.Edit;
                findRow.View = updateRole.View;
                findRow.Delete = updateRole.Delete;
                findRow.Approval = updateRole.Approval;
                findRow.AssignTask = updateRole.AssignTask;
            }
            sessionList.Add(findRow);
            return sessionList;
        }

       

    }
}
