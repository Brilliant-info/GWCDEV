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
using System.Data.Linq;

namespace Domain.UserManagement
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]

    public partial class UserCreation : Interface.UserManagement.iUserCreation
    {
        Domain.Server.Server svr = new Server.Server();
        #region GetUserCreationList
        /// <summary>
        /// GetUserCreationList is providing List of User
        /// </summary>
        /// <returns></returns>
        /// 
        public DataSet GetUserCreationList(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mUserProfileHead> Status = new List<mUserProfileHead>();
            XElement xmlUserCreation = new XElement("UserCreation", from m in ce.mUserProfileHeads.ToList()

                                                                    select new XElement("User",
                                                                    new XElement("ID", m.ID),
                                                                    new XElement("Name", m.FirstName + " " + m.LastName),
                                                                    new XElement("DateOfJoining", string.Format("{0:dd-MMM-yyyy}", m.DateOfJoining)),
                                                                    new XElement("DateOfBirth", string.Format("{0:dd-MMM-yyyy}", m.DateOfBirth)),
                                                                    new XElement("EmailID", m.EmailID),
                                                                    new XElement("MobileNo", m.MobileNo)
                                                                    ));


            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            ds.ReadXml(xmlUserCreation.CreateReader());
            dt = ds.Tables.Add("Dt");
            return ds;
        }


        #endregion

        #region InsertUserCreation

        /// <summary>
        ///  mUserProfileHead is the Method To Insert Record In Database
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public long InsertUserCreation(mUserProfileHead user, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            try
            {
                ce.mUserProfileHeads.AddObject(user);
                ce.SaveChanges();
                return user.ID;
            }
            catch { return 0; }
        }
        #endregion

        public List<SP_GetUserRoleDetail_Result> UpdateRoleIntoSessionList(List<SP_GetUserRoleDetail_Result> sessionList, SP_GetUserRoleDetail_Result updateRole, int rowindex)
        {
            SP_GetUserRoleDetail_Result findRow = new SP_GetUserRoleDetail_Result();
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

        public List<SP_GetUserRoleDetail_Result> GetDataToBindRoleMasterDetailsByRoleID(long RoleID, long UserIdForRole, long CompanyID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetUserRoleDetail_Result> lst = new List<SP_GetUserRoleDetail_Result>();
            lst = (from dbtable in ce.SP_GetUserRoleDetail(RoleID, UserIdForRole, CompanyID)
                   orderby (long)(dbtable.mSequence), (long)(dbtable.pSequence), (long)(dbtable.oSequence)
                   select dbtable).ToList();
            return lst;
        }

        public List<SP_GWCGetUserRoleDetail_Result> GetRoleDetails(long RoleID, long UserIdForRole, long CompanyID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GWCGetUserRoleDetail_Result> lst = new List<SP_GWCGetUserRoleDetail_Result>();
            lst = (from dbtable in ce.SP_GWCGetUserRoleDetail(RoleID, UserIdForRole, CompanyID)
                   orderby (long)(dbtable.mSequence), (long)(dbtable.pSequence), (long)(dbtable.oSequence)
                   select dbtable).ToList();
            return lst;
        }

        #region UpdateUserProfile

        /// <summary>
        /// Updatestatus Is Method To Update mUserProfileHead Table
        /// </summary>
        /// <param name="Updatestatus"></param>
        /// <returns></returns>
        /// 
        public int UpdateUserProfile(mUserProfileHead User, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.mUserProfileHeads.Attach(User);
            ce.ObjectStateManager.ChangeObjectState(User, EntityState.Modified);
            ce.SaveChanges();
            return Convert.ToInt32(User.ID);
        }
        #endregion

        #region GetUserByID
        /// <summary>
        /// GetUserCreationList is providing List of User
        /// </summary>
        /// <returns></returns>
        /// 
        public mUserProfileHead GetUserByID(long UserID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            mUserProfileHead User = new mUserProfileHead();
            User = (from p in ce.mUserProfileHeads
                    where p.ID == UserID
                    select p).FirstOrDefault();
            ce.Detach(User);
            return User;
        }
        #endregion

        #region Dupliacte

        public string checkDuplicateRecord(string EmpCode, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            var output = (from p in ce.mUserProfileHeads
                          where p.EmployeeID == EmpCode
                          select new { p.EmployeeID }).FirstOrDefault();

            if (output != null)
            {
                result = "Same Employee Code Already Exist";
            }
            return result;

        }
        #endregion

        #region chechDuplicateEdit
        public string checkDuplicateRecordEdit(string EmpCode, int UserID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            var output = (from p in ce.mUserProfileHeads
                          where p.EmployeeID == EmpCode && p.ID != UserID
                          select new { p.EmployeeID }).FirstOrDefault();

            if (output != null)
            {
                result = "Same Employee Code Already Exist";

            }
            return result;
        }
        #endregion

        public List<mUserProfileHead> SelectEmployeeDepartmentwise(mDesignation objmDesignation, string[] conn)
        {


            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mUserProfileHead> lst = new List<mUserProfileHead>();

            if (objmDesignation.Name == "0")
            {
                var result = from dbtable in ce.mUserProfileHeads
                             where ((dbtable.DesignationID == objmDesignation.ID) && (dbtable.DepartmentID == objmDesignation.DepartmentID))
                             select new { dbtable.FirstName, dbtable.ID, dbtable.MiddelName, dbtable.LastName, dbtable.EmployeeID };
                lst = result.AsEnumerable().Select(o => new mUserProfileHead { FirstName = o.FirstName, ID = o.ID, MiddelName = o.MiddelName, LastName = o.LastName, EmployeeID = o.EmployeeID }).ToList();
            }
            else
            {
                var result = from dbtable in ce.mUserProfileHeads
                             where ((dbtable.DesignationID == objmDesignation.ID) && (dbtable.DepartmentID == objmDesignation.DepartmentID) && (dbtable.ReportingTo == objmDesignation.Name))
                             select new { dbtable.FirstName, dbtable.ID, dbtable.MiddelName, dbtable.LastName, dbtable.EmployeeID };
                lst = result.AsEnumerable().Select(o => new mUserProfileHead { FirstName = o.FirstName, ID = o.ID, MiddelName = o.MiddelName, LastName = o.LastName, EmployeeID = o.EmployeeID }).ToList();
            }



            return lst;
        }

        public bool FinalSaveUserRoles(List<SP_GetUserRoleDetail_Result> sessionList, string userID, long userIDForRole, long CompanyID, long RoleID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            try
            {
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

                    ObjectParameter _paraxmlData = new ObjectParameter("xmlData", typeof(string));
                    _paraxmlData.Value = xmlEle.ToString();

                    ObjectParameter _paraCompanyID = new ObjectParameter("paraCompanyID", typeof(long));
                    _paraCompanyID.Value = CompanyID;

                    ObjectParameter _paraUserIDForRole = new ObjectParameter("paraUserIDForRole", typeof(long));
                    _paraUserIDForRole.Value = userIDForRole;

                    ObjectParameter _paraUserID = new ObjectParameter("paraUserID", typeof(long));
                    _paraUserID.Value = userID;

                    ObjectParameter _paraRoleID = new ObjectParameter("paraRoleID", typeof(long));
                    _paraRoleID.Value = RoleID;

                    ObjectParameter[] obj = new ObjectParameter[] { _paraxmlData, _paraCompanyID, _paraUserIDForRole, _paraUserID, _paraRoleID };
                    ce.ExecuteFunction("SP_InsertIntoUserRoleDetail", obj);
                    ce.SaveChanges();
                }

                return true;
            }
            catch { return false; }
        }

        public List<vGetUserProfileList> GetUserList(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<vGetUserProfileList> userProfileList = new List<vGetUserProfileList>();
            userProfileList = (from c in ce.vGetUserProfileLists
                               orderby c.deptname, c.desiName, c.userName
                               select c).ToList();
            return userProfileList;
        }

        public List<vGWCGetUserProfileList> GetGWCUserList(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<vGWCGetUserProfileList> userProfileList = new List<vGWCGetUserProfileList>();
            userProfileList = (from c in ce.vGWCGetUserProfileLists
                               orderby c.userID descending
                               select c).ToList();
            return userProfileList;
        }

        public vGetUserProfileByUserID GetUserProfileByUserID(long UserID, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {

                // BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                vGetUserProfileByUserID UserProfile = new vGetUserProfileByUserID();
                UserProfile = db.vGetUserProfileByUserIDs.Where(u => u.userID == UserID).FirstOrDefault();

                return UserProfile;
            }

        }

        public void SaveUsersLocationDetails(long ToUserID, long Level, long LocationIDs, string CreatedBy, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            try
            {
                ObjectParameter SPUserID = new ObjectParameter("UserID", typeof(long));
                SPUserID.Value = ToUserID;

                ObjectParameter SPLevel = new ObjectParameter("Level", typeof(long));
                SPLevel.Value = Level;

                ObjectParameter SPTerritoryIDs = new ObjectParameter("TerritoryIDs", typeof(long));
                SPTerritoryIDs.Value = LocationIDs;

                ObjectParameter SPCreatedBy = new ObjectParameter("CreatedBy", typeof(string));
                SPCreatedBy.Value = CreatedBy;

                ObjectParameter[] obj = new ObjectParameter[] { SPUserID, SPLevel, SPTerritoryIDs, SPCreatedBy };
                db.ExecuteFunction("SP_InsertIntomUserProfieTerritoryDetail", obj);
                db.SaveChanges();
            }
            catch { }
            finally { }

        }

        public string GetLocationListByUserID(long UserID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mTerritory> lst = new List<mTerritory>();
            string result = "";
            try
            {
                lst = (from mt in ce.mTerritories
                       join mut in ce.mUserTerritoryDetails on mt.ID equals mut.TerritoryID
                       where mut.UserID == UserID
                       select mt).ToList();

                foreach (mTerritory mt in lst)
                {
                    if (result != "")
                    {
                        result += " | " + mt.Territory;
                    }
                    else
                    {
                        result = mt.Territory;
                    }
                }

                if (result == "") result = "N/A";
            }
            catch { }
            return result;
        }

        public string GetHTMLMenuByUserID(long UserID, string[] conn)
        {
            string result = "";
            try
            {
                //DataSet ds = new DataSet();
                //ds = fillds("Declare @rs varchar(max); set @rs=''; declare @cnt bigint; set @cnt=0; select @cnt=count(mo.FormUrl) from  sysmObjects mo Left outer join mUserRolesDetail rd on Cast(mo.ObjectName as varchar(100)) = Cast(rd.ObjectName as varchar(100))  Where mo.HeaderMenu = 'YS' and rd.UserID = " + UserID + " And (rd.[Add] = 'true' OR rd.Edit = 'true' OR rd.[View] = 'true' or rd.[Delete]='True' or rd.Approval = 'true') Declare @counter bigint; set @counter =1;  while(@counter<=@cnt) begin    select @rs=@rs+FormUrl from ( select ROW_NUMBER() OVER(ORDER BY mo.Sequence ) AS Row,mo.FormUrl from  sysmObjects mo Left outer join mUserRolesDetail rd on Cast(mo.ObjectName as varchar(100)) = Cast(rd.ObjectName as varchar(100))  Where mo.HeaderMenu = 'YS' and rd.UserID = " + UserID + " And (rd.[Add] = 'true' OR rd.Edit = 'true' OR rd.[View] = 'true' or rd.[Delete]='True' or rd.Approval = 'true'))aaa  where Row=@counter  set @counter=@counter+1; end; set @rs='<ul id=''css3menu1'' class=''topmunu1''>' + @rs + '</ul>';   select @rs as 'HTMLMenu'", conn);
                //if (ds.Tables[0].Rows.Count > 0)
                //{
                //    result = ds.Tables[0].Rows[0]["HTMLMenu"].ToString();
                //}

                DataSet ds = new DataSet();
                using (SqlCommand cmd = new SqlCommand())
                {
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_GetHTMLMenuByUserID";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@UserID", UserID);
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        result = ds.Tables[0].Rows[0]["HTMLMenu"].ToString();
                    }
                }
            }
            catch (Exception ex) { result = ex.InnerException.Message.ToString(); }
            finally { }
            return result;
        }

        /*Add By Suresh For Arabic Menu*/
        public string GetHTMLMenuArabicByUserID(long UserID, string[] conn)
        {
            string result = "";
            try
            {
                DataSet ds = new DataSet();
                ds = fillds("Declare @result nvarchar(max); Set @result =''; Select @result = @result+ ' '+ Cast(mo.FormUrlArabic as nvarchar(max)) from sysmObjects mo Left outer join mUserRolesDetail rd on Cast(mo.ObjectName as varchar(100)) = Cast(rd.ObjectName as varchar(100))   Where mo.HeaderMenu = 'YS' and rd.UserID = " + UserID + "  And (rd.[Add] = 'true' OR rd.Edit = 'true' OR rd.[View] = 'true'  or rd.[Delete]='True' or rd.Approval = 'true') order by mo.Sequence Set @result =  '<ul id=''css3menu1'' class=''topmenu1''>' + @result + '</ul>';  Select @result as 'HTMLMenu' ", conn);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    result = ds.Tables[0].Rows[0]["HTMLMenu"].ToString();
                }
            }
            catch (Exception ex) { result = ex.InnerException.Message.ToString(); }
            finally { }
            return result;
        }
        /*Add By Suresh For Arabic Menu*/


        public string GetTerritoryID_FromUserId(long userId, string[] conn)
        {

            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mUserTerritoryDetail> SiteList = new List<mUserTerritoryDetail>();
            string TerritoryID = "";
            SiteList = (from p in ce.mUserTerritoryDetails
                        where p.UserID == userId
                        select p).ToList();

            foreach (var v in SiteList)
            {
                if (TerritoryID.ToString() != "")
                {

                    TerritoryID = TerritoryID + "," + v.TerritoryID;
                }
                else
                {
                    TerritoryID = v.TerritoryID.ToString();
                }
            }

            return TerritoryID;
        }


        public DataSet GetSiteNameFromId(string sid, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            DataSet ds = new DataSet();
            ds.Reset();
            string str = "select id,Territory from mTerritory where id in(" + sid + ")";
            ds = fillds(str, conn);
            return ds;
        }
        protected DataSet fillds(string strquery, string[] conn)
        {
            using (DataSet ds = new DataSet())
            {
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
        }

        public List<mCompany> GetCompanyName(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mCompany> companylist = new List<mCompany>();
            companylist = (from cl in ce.mCompanies
                           orderby cl.Name
                           select cl).ToList();
            return companylist;
        }

        public List<mDropdownValue> GetUserType(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mDropdownValue> UserTypeList = new List<mDropdownValue>();
            UserTypeList = (from U in ce.mDropdownValues
                            where U.Parameter == "User"
                            select U).ToList();
            return UserTypeList;
        }


        public bool FinalSaveGWCUserRoles(List<SP_GWCGetUserRoleDetail_Result> sessionList, string userID, long userIDForRole, long CompanyID, long RoleID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            try
            {
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

                    ObjectParameter _paraxmlData = new ObjectParameter("xmlData", typeof(string));
                    _paraxmlData.Value = xmlEle.ToString();

                    ObjectParameter _paraCompanyID = new ObjectParameter("paraCompanyID", typeof(long));
                    _paraCompanyID.Value = CompanyID;

                    ObjectParameter _paraUserIDForRole = new ObjectParameter("paraUserIDForRole", typeof(long));
                    _paraUserIDForRole.Value = userIDForRole;

                    ObjectParameter _paraUserID = new ObjectParameter("paraUserID", typeof(long));
                    _paraUserID.Value = userID;

                    ObjectParameter _paraRoleID = new ObjectParameter("paraRoleID", typeof(long));
                    _paraRoleID.Value = RoleID;

                    ObjectParameter[] obj = new ObjectParameter[] { _paraxmlData, _paraCompanyID, _paraUserIDForRole, _paraUserID, _paraRoleID };
                    ce.ExecuteFunction("SP_InsertIntoUserRoleDetail", obj);
                    ce.SaveChanges();
                }

                return true;
            }
            catch { return false; }
        }


        public List<vGWCGetUserList> GWCSearchUserList(long ComanyID, long DeptID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<vGWCGetUserList> userProfileList = new List<vGWCGetUserList>();
            userProfileList = (from c in ce.vGWCGetUserLists
                               where c.CompanyID == ComanyID && c.DepartmentID == DeptID && c.Active == "Yes"
                               select c).ToList();
            return userProfileList;
        }

        public DataSet GetUserDelegationDetail(string state, long DeligateFrom, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_BindAcccessDelegation";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("state", state);
            cmd.Parameters.AddWithValue("DeligateFrom", DeligateFrom);
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;
        }

        public void SaveEditUserDelegation(long ID, long DeligateFrom, DateTime FromDate, DateTime ToDate, long DeligateTo, string Remark, string state, long CreatedBy, DateTime CreatedDate, long DeptID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_SaveEditUserDelegate";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("ID", ID);
            cmd.Parameters.AddWithValue("DeligateFrom", DeligateFrom);
            cmd.Parameters.AddWithValue("FromDate", FromDate);
            cmd.Parameters.AddWithValue("ToDate", ToDate);
            cmd.Parameters.AddWithValue("DeligateTo", DeligateTo);
            cmd.Parameters.AddWithValue("Remark", Remark);
            cmd.Parameters.AddWithValue("state", state);
            cmd.Parameters.AddWithValue("CreatedBy", CreatedBy);
            cmd.Parameters.AddWithValue("CreatedDate", CreatedDate);
            cmd.Parameters.AddWithValue("DeptID", DeptID);
            cmd.ExecuteNonQuery();
        }

        public DataSet getUserDelegateDetail(long ID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_GetUserDelegateDetail";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("ID", ID);
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;
        }

        public DataSet getDelegateToList(long DepartmentID, long UserID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_GetDelegatetoUserList";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("DepartmentID", DepartmentID);
            cmd.Parameters.AddWithValue("UserID", UserID);
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;
        }

        public void UpdateDelegateFrom(long DeligateFrom, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_UpdateDelegateFrom";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("DeligateFrom", DeligateFrom);
            cmd.ExecuteNonQuery();
        }

        public DataSet GetDepartmentforUsersave(long ParentID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_GetDepartmentContact";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("ParentID", ParentID);
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;
        }

        public DataSet GetRollNameById(long TerritoryID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_GetRollNameByDeptId";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("TerritoryID", TerritoryID);
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;
        }

        public string GetUserTypeByRoll(long ID, string[] conn)
        {
            SqlDataReader dr;
            string UserType = "";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_GetUserTypeByRoll";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("ID", ID);
            dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                UserType = dr[0].ToString();
            }
            dr.Close();
            return UserType;
        }

        public DataSet CheckPasswordHistory(long UserProfileID, string Password, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_GetPasswordHistory";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("UserProfileID", UserProfileID);
            cmd.Parameters.AddWithValue("Password", Password);
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;
        }

        public void SavePasswordDetails(long UserProfileID, string Email, string UserName, string Password, string transactedby, string ip, string transactiontype, long userid, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "sp_SavePasswordDetails";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("UserProfileID", UserProfileID);
            cmd.Parameters.AddWithValue("Email", Email);
            cmd.Parameters.AddWithValue("UserName", UserName);
            cmd.Parameters.AddWithValue("Password", Password);
            cmd.Parameters.AddWithValue("CreatedDate", DateTime.Now);
            cmd.Parameters.AddWithValue("transactedby", transactedby);
            cmd.Parameters.AddWithValue("IPAddress", ip);
            cmd.Parameters.AddWithValue("transactiontype", transactiontype);
            cmd.Parameters.AddWithValue("userid", userid);
            cmd.ExecuteNonQuery();
        }

        public DataSet GetUserDepartment(long userID, string[] conn)
        {
            DataSet ds = new DataSet();
            ds.Reset();
            string str = "select UTD.TerritoryID ,MT.Territory,MT.StoreCode,MT.Active,MT.OrderFormat,MT.PriceChange,MT.AutoCancel,MC.Name as Company from  mUserTerritoryDetail UTD left outer join mTerritory MT on  UTD.TerritoryID=MT.ID left outer join mCompany MC on MT.ParentID=MC.ID where UTD.UserID=" + userID + "";
            ds = fillds(str, conn);
            return ds;
        }

        public DataSet getDelegateToListMultipleDept(string SelectedLocation, string[] conn)
        {
            DataSet ds = new DataSet();
            ds.Reset();
            string str = "select ID,FirstName +' ' + MiddelName + ' '+ LastName as Name from mUserProfileHead where DepartmentID in(" + SelectedLocation + ") and UserType in ('Super Admin','Admin','Approver','Requester And Approver')";
            ds = fillds(str, conn);
            return ds;
        }

        public void updatelockunlock(string UserName, byte IsLockedOut, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_UpdateLocking";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("UserName", UserName);
            cmd.Parameters.AddWithValue("IsLockedOut", IsLockedOut);
            cmd.ExecuteNonQuery();
        }

        public string GetLogoPath(long ID, string[] conn)
        {
            SqlDataReader dr;
            string UserLogo = "";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_GetLogoPath";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("ID", ID);
            dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                UserLogo = dr[0].ToString();
            }
            dr.Close();
            return UserLogo;
        }


        public int GetApprovalDetailsOfUser(long userID, string[] conn)
        {
            int result = 0;
            DataSet ds = new DataSet();
            ds.Reset();
            string str = "select * from  mApprovalLevelDetail where Userid=" + userID + "";
            ds = fillds(str, conn);
            result = ds.Tables[0].Rows.Count;
            return result;
        }

        public int GetAdditionalDistributationOfUser(long userID, string[] conn)
        {
            int result = 0;
            DataSet ds = new DataSet();
            ds.Reset();
            string str = "select * from mAddDistribution where ContactID=" + userID + "";
            ds = fillds(str, conn);
            result = ds.Tables[0].Rows.Count;
            return result;
        }

        public DataSet GetDepartmentDelegate(long UserID, string[] conn)
        {
            DataSet ds = new DataSet();
            ds.Reset();
            string str = "select distinct ID, Territory from mTerritory where ID in (Select TerritoryID from mUserTerritoryDetail where UserID = '" + UserID + "') order by Territory asc";
            ds = fillds(str, conn);
            return ds;
        }

        public DataSet GetDepartmentSSuperAdmin(string[] conn)
        {
            DataSet ds = new DataSet();
            ds.Reset();
            string str = "select ID, Territory from  mTerritory order by Territory asc";
            ds = fillds(str, conn);
            return ds;
        }

        public void Deletedelegate(long ID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_DeleteDelegation";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("ID", ID);
            cmd.ExecuteNonQuery();
        }

        public long GetDuplicateDelegate(DateTime FromDate, DateTime ToDate, long DeligateTo, long DeptID, long DeligateFrom, string[] conn)
        {

            SqlDataReader dr;
            long Count = 0;
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_CheckDuplicateDelegate";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("FromDate", FromDate);
            cmd.Parameters.AddWithValue("ToDate", ToDate);
            cmd.Parameters.AddWithValue("DeligateTo", DeligateTo);
            cmd.Parameters.AddWithValue("DeptID", DeptID);
            cmd.Parameters.AddWithValue("DeligateFrom", DeligateFrom);
            dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                Count = long.Parse(dr[0].ToString());
            }
            dr.Close();
            cmd.Connection.Close();
            return Count;
        }



        public string GetUserNameByID(long UserID, string[] conn)
        {
            DataSet ds = new DataSet();
            string str = "SELECT DISTINCT UserName  FROM   mPasswordDetails  WHERE  (UserProfileID IN (" + UserID + ")) ";
            ds = fillds(str, conn);
            string UserName = ds.Tables[0].Rows[0]["UserName"].ToString();
            return UserName;
        }


        public string GetComppanyLogo(long CmpnyID, string[] conn)
        {
            DataSet ds = new DataSet();
            string LogoPath = "";
            string str = "select LogoPath from mCompany where id=" + CmpnyID + " ";
            ds = fillds(str, conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                LogoPath = ds.Tables[0].Rows[0]["LogoPath"].ToString();
            }
            return LogoPath;
        }

        public DataSet GetUserLoginDetails(long UserProfileID, string[] conn)
        {
            DataSet ds = new DataSet();
            ds.Reset();
            string str = "select top(1) * from mPasswordDetails where UserProfileID = '" + UserProfileID + "' order by ID desc ";
            ds = fillds(str, conn);
            return ds;
        }

        public void InsertIntoUserLocation(long UserID, long LocationID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_InsertIntoUserLocation";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("UserID", UserID);
            cmd.Parameters.AddWithValue("LocationID", LocationID);
            cmd.ExecuteNonQuery();
        }

        public long GetDuplicatlocationUser(long UserID, long LocationID, string[] conn)
        {
            SqlDataReader dr;
            long Count = 0;
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_GetDuplicateLocUser";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("UserID", UserID);
            cmd.Parameters.AddWithValue("LocationID", LocationID);
            dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                Count = long.Parse(dr[0].ToString());
            }
            dr.Close();
            cmd.Connection.Close();
            return Count;
        }

        public void RemoveUserLoc(long ID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_RemoveUserLoc";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("ID", ID);
            cmd.ExecuteNonQuery();
        }

        public void RemoveDepartment(long ID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_RemoveUserDepartment";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("ID", ID);
            cmd.ExecuteNonQuery();
        }


        public DataSet GetUserRoleID(string userid, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from mUserProfileHead where ID=" + userid + "", conn);
            return ds;
        }

        public DataSet GetApprovalLeveldetailsofUser(string userid, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from mApprovalLevelDetail where UserID=" + userid + "", conn);
            return ds;
        }

        public void UpdateUserTerritoryDetails(long userid, string createdby, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = svr.GetSqlConn(conn); ;
            cmd.CommandText = "Update mUserTerritoryDetail set UserId=" + userid + " where CreatedBy='" + createdby + "' and UserID=0 ";
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            da.SelectCommand = cmd;
            da.Fill(ds, "dtl");
        }

        public void RemoveReturnMenuNewUser(long UserID, string state, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "sp_RemoveReturnMenuNewUser";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("UserID", UserID);
            cmd.Parameters.AddWithValue("state", state);
            cmd.ExecuteNonQuery();
        }

        public void DeleteUserTeritoryifUserIdZero(string createdby, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = svr.GetSqlConn(conn); ;
            cmd.CommandText = "Delete From mUserTerritoryDetail where  CreatedBy='" + createdby + "' and UserID=0 ";
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            da.SelectCommand = cmd;
            da.Fill(ds, "dtl");
        }


        public long CheckUserterritoryDuplicate(long TerritoryId, long UserID, long CreatedBy, string[] conn)
        {
            long result = 0;
            SqlDataReader dr;
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Sp_ChkDuplicateUserDept";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("TerritoryId", TerritoryId);
            cmd.Parameters.AddWithValue("UserID", UserID);
            cmd.Parameters.AddWithValue("CreatedBy", CreatedBy);
            dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                result = long.Parse(dr[0].ToString());
            }
            dr.Close();
            return result;
        }

        public void insertUserterritory(long TerritoryId, long UserID, long CreatedBy, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_InsertIntoMUserTerritory";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("TerritoryId", TerritoryId);
            cmd.Parameters.AddWithValue("UserID", UserID);
            cmd.Parameters.AddWithValue("CreatedBy", CreatedBy);
            cmd.ExecuteNonQuery();
        }


        public DataSet GetDuplicateAssignDeptForUser(long deptid, string createdby, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = svr.GetSqlConn(conn); ;
            cmd.CommandText = "Select * from mUserTerritoryDetail where  TerritoryId=" + deptid + " and CreatedBy='" + createdby + "' and UserID=0 ";
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            da.SelectCommand = cmd;
            da.Fill(ds, "dtl");
            return ds;
        }

        #region Version 3.0 code
        public DataSet GetAllLocation(long ID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = svr.GetSqlConn(conn); ;
            cmd.CommandText = "select  UP.ID as UserID, A.ID as LocationID from tAddress A inner join mUserProfileHead UP on A.CompanyID = UP.CompanyID where A.AddressType = 'Location' and A.Active = 'Y' and UP.ID =" + ID + "";
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            da.SelectCommand = cmd;
            da.Fill(ds, "dtl");
            return ds;
        }

        public void InsertAllLocation(DataTable dt, long Userid, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_InsertAllLocation";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("InsertLocation", dt);
            cmd.Parameters.AddWithValue("UserId", Userid);
            cmd.ExecuteNonQuery();
        }




        #endregion

        public DataSet GetSkuListDeptWiseNewe1(int pageIndex, string filter, long UserID, long DeptID, string[] conn)
        {
            DataSet ds = new DataSet();
            pageIndex = pageIndex + 1;
            if (filter == "")
            {
                filter = "0";
            }
            using (SqlCommand cmd = new SqlCommand())
            {
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_GetSkuListDeptWiseNewe";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("DeptID", DeptID);
                    cmd.Parameters.AddWithValue("filter", filter);
                    cmd.Connection = svr.GetSqlConn(conn);
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    cmd.Connection.Close();
                }
            }
            return ds;
        }

        public DataSet GetSkuListDeptWiseToUpdate1(int pageIndex, string filter, long UserID, long DeptID, string[] conn)
        {
            DataSet ds = new DataSet();
            pageIndex = pageIndex + 1;
            if (filter == "")
            {
                filter = "0";
            }
            using (SqlCommand cmd = new SqlCommand())
            {
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_GetSkuListDeptWiseToUpdate";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@DeptID", DeptID);
                    cmd.Parameters.AddWithValue("@filter", filter);
                    cmd.Connection = svr.GetSqlConn(conn);
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    cmd.Connection.Close();
                }
            }
            return ds;
        }

        public DataSet GetProdAvailableBal1(long Pid, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            using (SqlDataAdapter da = new SqlDataAdapter())
            {
                try
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_GetProdAvailableBal";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@SKUId", Pid);
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    cmd.Connection.Close();
                }
                catch
                {
                    cmd.Connection.Close();
                }
                finally
                {

                }

            }
            return ds;
        }

        public DataSet GetDatabaseSchemaforDept1(long DeptId, string[] conn)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    using (DataSet ds = new DataSet())
                    {
                        try
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "SP_GetConfigurationSchema";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("DeptId", DeptId);
                            cmd.Connection = svr.GetSqlConn(conn);
                            da.SelectCommand = cmd;
                            da.Fill(ds);
                            cmd.Connection.Close();
                        }
                        catch
                        {
                            cmd.Connection.Close();
                        }

                        return ds;

                    }
                }
            }
        }




        #region Security question and get IP detail Suraj Jagtap 28/12/2019


        public DataSet BindDDLQuestion(string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            using (SqlDataAdapter da = new SqlDataAdapter())
            {
                try
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_BindDDLQuestion";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    cmd.Connection.Close();
                }
                catch
                {
                    cmd.Connection.Close();
                }
                finally
                {

                }

            }
            return ds;
        }

        public long SaveSecurityQuestion(string q1, string q2, string q3, string a1, string a2, string a3, long Userid, string[] conn)
        {
            long i = 0;
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_SaveSecurityQuestion";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("q1", q1);
                cmd.Parameters.AddWithValue("q2", q2);
                cmd.Parameters.AddWithValue("q3", q3);
                cmd.Parameters.AddWithValue("a1", a1);
                cmd.Parameters.AddWithValue("a2", a2);
                cmd.Parameters.AddWithValue("a3", a3);
                cmd.Parameters.AddWithValue("Userid", Userid);
                //cmd.Parameters.Add("@id", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                i = 1;
            }
            return i;
        }

        public string WMChkSecurityQuestion(string q1, string q2, string a1, string a2, long userid, string[] conn)
        {
            string result = "";
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            using (SqlDataAdapter da = new SqlDataAdapter())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_WMChkSecurityQuestion";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("q1", q1);
                cmd.Parameters.AddWithValue("q2", q2);
                cmd.Parameters.AddWithValue("a1", a1);
                cmd.Parameters.AddWithValue("a2", a2);
                cmd.Parameters.AddWithValue("Userid", userid);
                da.SelectCommand = cmd;
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    result = ds.Tables[0].Rows[0]["Result"].ToString();
                }
            }
            return result;
        }

        public DataSet BindDDLQuestionforuser(long userid, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            using (SqlDataAdapter da = new SqlDataAdapter())
            {
                try
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_BindDDLQuestionforuser";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("userid", userid);
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    cmd.Connection.Close();
                }
                catch
                {
                    cmd.Connection.Close();
                }
                finally
                {

                }

            }
            return ds;
        }

        public string ClearSecurityQuestions(string uid, string ipadd, string ttype, string UserName, long userid, string[] conn)
        {
            string result = "";
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_ClearSecurityQuestions";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("Userid", uid);
                    cmd.Parameters.AddWithValue("Ipaddress", ipadd);
                    cmd.Parameters.AddWithValue("Trantype", ttype);
                    cmd.Parameters.AddWithValue("Username", UserName);
                    cmd.Parameters.AddWithValue("Createdby", userid);
                    cmd.ExecuteNonQuery();
                    result = "pass";
                }

            }
            catch
            {
                result = "failed";
            }
            return result;
        }

        //update function
        public string CheckOneDayValidation(long UserID, string[] conn)
        {
            string result = "";
            //DataSet dsLastCreateddate = new DataSet();
            //string str = "SELECT top (1)  Convert(varchar(10), CreatedDate,101) CreatedDate FROM  mPasswordDetails where UserProfileID=" + UserID + " order by CreatedDate desc";
            //dsLastCreateddate = fillds(str, conn);
            //DateTime LstChngDt = Convert.ToDateTime(dsLastCreateddate.Tables[0].Rows[0]["CreatedDate"].ToString());

            //DataSet dsCurrentdate = new DataSet();
            //string str1 = "select convert(varchar(10),getdate(),101) CurrentDate";
            //dsCurrentdate = fillds(str1, conn);
            //DateTime CrntDt = Convert.ToDateTime(dsCurrentdate.Tables[0].Rows[0]["CurrentDate"].ToString());

            //if (LstChngDt == CrntDt) result = "SameDay";

            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            using (SqlDataAdapter da = new SqlDataAdapter())
            {
                try
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_CheckOneDayValidation";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("UserID", UserID);
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    cmd.Connection.Close();
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        result = ds.Tables[0].Rows[0]["result"].ToString();
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

        public DataSet BindQuestionandAnswer(long UserID, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            using (SqlDataAdapter da = new SqlDataAdapter())
            {
                try
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_BindQuestionandAnswer";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("UserID", UserID);
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    cmd.Connection.Close();
                }
                catch
                {
                    cmd.Connection.Close();
                }
                finally
                {

                }
            }
            return ds;
        }

        public DataSet GetDataSetTable(string userid, string q1, string[] conn)
        {
            DataSet ds = new DataSet();
            try
            {
                SqlCommand cmd = new SqlCommand();
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_Securitycount";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("Userid", userid);
                cmd.Parameters.AddWithValue("Que", q1);
                da.SelectCommand = cmd;
                da.Fill(ds);
            }
            catch
            {

            }
            return ds;
        }

        public string GetStringReturn(string userid, string q1, string[] conn)
        {
            string result = "";
            try
            {
                DataSet ds = new DataSet();
                SqlCommand cmd = new SqlCommand();
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_SecurityQueAns";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("Userid", userid);
                cmd.Parameters.AddWithValue("Que", q1);
                da.SelectCommand = cmd;
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    result = ds.Tables[0].Rows[0]["answer"].ToString();
                }
            }
            catch
            {

            }
            return result;
        }

        #endregion

        #region VF Import        

        public DataSet GWCSearchUserListVF(long Cid, long Did, string[] conn)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    using (DataSet ds = new DataSet())
                    {
                        try
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "SP_GWCSearchUserListVF";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("Cid", Cid);
                            cmd.Parameters.AddWithValue("Did", Did);
                            cmd.Connection = svr.GetSqlConn(conn);
                            da.SelectCommand = cmd;
                            da.Fill(ds);
                            cmd.Connection.Close();
                        }
                        catch
                        {
                            cmd.Connection.Close();
                        }

                        return ds;

                    }
                }
            }

        }

        #endregion
        #region check return order access
        public int CheckRMSUser(long userid, string[] conn)
        {
            int result = 0;
            using (SqlCommand cmd = new SqlCommand())
            {
                //using (SqlDataAdapter da = new SqlDataAdapter())
                //{
                //    using (DataSet ds = new DataSet())
                //    {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_ChkReturnUser";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@userid", userid);
                //   da.SelectCommand = cmd;

                result = int.Parse(cmd.ExecuteScalar().ToString());
                if (result == 0)
                {
                    result = 0;
                }
                else
                {
                    result = 1;
                }

                //da.Fill(ds);
                //if (ds.Tables[0].Rows.Count > 0)
                //{
                //    result = Convert.ToInt32(ds.Tables[0].Rows[0]["id"].ToString());
                //    //    orderheadid = long.Parse(cmd.ExecuteScalar().ToString());
                //    if (result == 0)
                //    {
                //        result = 0;
                //    }
                //    else
                //    {
                //        result = 1;
                //    }

                //}
                //else
                //{
                //    result = 0;
                //}
                //}
                //}
            }
            return result;

        }

        public int GetMarkAsReturnOrder(string OId, string userType, string[] conn)
        {

            int cnt = 0;
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_ChkExpOrderForReturn";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@userType", userType);
                cmd.Parameters.AddWithValue("@OrderId", OId);
                da.SelectCommand = cmd;
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0)
                { 
                    cnt = Convert.ToInt32(ds.Tables[0].Rows[0]["id"].ToString());
                    if (cnt == 0)
                    {
                        cnt = 0;
                    }
                    else if(cnt == 1001)
                    {
                        cnt = 1001;
                    }
                }
                else
                {
                    cnt = 0;
                }
            }
            return cnt;
        }

        public void insertReturnOrder(string OrderId,long userid, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_InsertReturnOrder";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@OrderId", OrderId);
            cmd.Parameters.AddWithValue("@userid", userid);
            cmd.ExecuteNonQuery();
        }
        #endregion

        #region  return order Collection Report
        //public DataSet GetAllReturnOrdercollectionreport(string FrmDt, string Todt, string SelectedCompany, string SelectedDepartment, string SelectedUser, string SelectedStatus, string[] conn)
        //{
        //    DataSet ds = new DataSet();

        //    using (SqlCommand cmd = new SqlCommand())
        //    {
        //        SqlDataAdapter da = new SqlDataAdapter();
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        cmd.CommandText = "GetAllReturnOrdercollectionreport";
        //        cmd.Connection = svr.GetSqlConn(conn);
        //        cmd.Parameters.Clear();
        //        cmd.Parameters.AddWithValue("@FrmDt", FrmDt);
        //        cmd.Parameters.AddWithValue("@Todt", Todt);
        //        cmd.Parameters.AddWithValue("@Companyid", SelectedCompany);
        //        cmd.Parameters.AddWithValue("@Departmentid", SelectedDepartment);
        //        cmd.Parameters.AddWithValue("@Status", SelectedStatus);
        //        cmd.Parameters.AddWithValue("@Userid", SelectedUser);
        //        da.SelectCommand = cmd;
        //        da.Fill(ds);
        //        return ds;

        //    }
        //}
        public DataSet GetAllReturnOrdercollectionreport(string FrmDt, string Todt, string SelectedCompany, string SelectedDepartment, string SelectedUser, string SelectedStatus, string SelectedDriver, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {

                SqlDataAdapter da = new SqlDataAdapter();
                //using (SqlDataAdapter da = new SqlDataAdapter())
                //{
                //    using (DataSet ds = new DataSet())
                //    {
                try
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "GetAllReturnOrdercollectionreport";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@FrmDt", FrmDt);
                            cmd.Parameters.AddWithValue("@Todt", Todt);
                            cmd.Parameters.AddWithValue("@Companyid", SelectedCompany);
                            cmd.Parameters.AddWithValue("@Departmentid", SelectedDepartment);
                            cmd.Parameters.AddWithValue("@Status", SelectedStatus);
                            cmd.Parameters.AddWithValue("@Userid", SelectedUser);
                            cmd.Parameters.AddWithValue("@driverid", SelectedDriver);
                            cmd.Connection = svr.GetSqlConn(conn);
                            da.SelectCommand = cmd;
                            da.Fill(ds);
                            cmd.Connection.Close();
                        }
                        catch
                        {
                            cmd.Connection.Close();
                        }

                        return ds;

                    //}
                //}
            }
        }

        public DataSet GetAllReturnOrderSummaryreport(string FrmDt, string Todt, string SelectedCompany, string SelectedDepartment, string SelectedUser, string SelectedStatus,  string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {

                SqlDataAdapter da = new SqlDataAdapter();
                //using (SqlDataAdapter da = new SqlDataAdapter())
                //{
                //    using (DataSet ds = new DataSet())
                //    {
                try
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "GetAllReturnOrderSummaryreport";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@FrmDt", FrmDt);
                    cmd.Parameters.AddWithValue("@Todt", Todt);
                    cmd.Parameters.AddWithValue("@Companyid", SelectedCompany);
                    cmd.Parameters.AddWithValue("@Departmentid", SelectedDepartment);
                    cmd.Parameters.AddWithValue("@Status", SelectedStatus);
                    cmd.Parameters.AddWithValue("@Userid", SelectedUser);
                    cmd.Connection = svr.GetSqlConn(conn);
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    cmd.Connection.Close();
                }
                catch
                {
                    cmd.Connection.Close();
                }

                return ds;

                //}
                //}
            }
        }


        public DataSet GetReturnCustomerName(long UID, string[] conn)
        {

            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();

                //using (SqlDataAdapter da = new SqlDataAdapter())
                //{
                //    using (DataSet ds = new DataSet())
                //    {
                try
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_ROGetCustName";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@UID", UID);
                    cmd.Connection = svr.GetSqlConn(conn);
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    cmd.Connection.Close();
                }
                catch
                {
                    cmd.Connection.Close();
                }

                return ds;

                //}
                //}
            }
        }

        public DataSet GetReturnDepartmentList(long CID, string[] conn)
        {

            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();

                //using (SqlDataAdapter da = new SqlDataAdapter())
                //{
                //    using (DataSet ds = new DataSet())
                //    {
                try
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_ROGetDeptName";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@CID", CID);
                    cmd.Connection = svr.GetSqlConn(conn);
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    cmd.Connection.Close();
                }
                catch
                {
                    cmd.Connection.Close();
                }

                return ds;

                //}
                //}
            }
        }

        public DataSet GetReturnStatus(string[] conn)
        {

            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                //using (SqlDataAdapter da = new SqlDataAdapter())
                //{
                //    using (DataSet ds = new DataSet())
                //    {
                SqlDataAdapter da = new SqlDataAdapter();

                try
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_ROGetStatus";
                    cmd.Parameters.Clear();
                    //cmd.Parameters.AddWithValue("@CID", CID);
                    cmd.Connection = svr.GetSqlConn(conn);
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    // cmd.Connection.Close();
                }
                catch
                {
                    cmd.Connection.Close();
                }
                return ds;
                //}
                //}
            }
        }

        public DataSet GetReturnDriver(string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();

                //using (SqlDataAdapter da = new SqlDataAdapter())
                //{
                //    using (DataSet ds = new DataSet())
                //    {
                try
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_ROGetDriver";
                    cmd.Parameters.Clear();
                    //cmd.Parameters.AddWithValue("@CID", CID);
                    cmd.Connection = svr.GetSqlConn(conn);
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    //  cmd.Connection.Close();
                }
                catch
                {
                    cmd.Connection.Close();
                }
                return ds;
                //}
                //}
            }
        }

        public DataSet GetReturnOrderCollectionReportData(string FrmDt, string Todt, string SelectedRec, string SelectedCompany, string SelectedDepartment, string SelectedDriver, string SelectedStatus, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_GetROCollReportData";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@FrmDt", FrmDt);
                cmd.Parameters.AddWithValue("@Todt", Todt);
                cmd.Parameters.AddWithValue("@oid", SelectedRec);
                cmd.Parameters.AddWithValue("@companyid", SelectedCompany);
                cmd.Parameters.AddWithValue("@Departmentid", SelectedDepartment);
                cmd.Parameters.AddWithValue("@Driverid", SelectedDriver);
                cmd.Parameters.AddWithValue("@Status", SelectedStatus);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
            //ds = fillds("SELECT PMM.Methodname , TCPD.Mobileno, Tod.Prod_Name, Tod.Prod_code, Tod.Price, Tod.Orderqty, oh.OrderNumber, Mt.Territory, ta.AddressLine1, ta.AddressLine2,ta.City,Ta.state,Ta.County,Ta.Zipcode, Mdb.OrderNoBarCode,Mdb.AmtPaidBarCode, oh.OrderNo,oh.OrderNumber,convert(varchar,oh.Orderdate,103)Orderdate, oh.GrandTotal, convert(varchar,oh.Orderdate,103)OrderCreationDate, convert(varchar,OH.Deliverydate,103)IDExpiryDate FROM  Torderhead oh LEFT OUTER JOIN mPaymentMethodMain as PMM on  Oh.PaymentID = Pmm.Id Left Outer Join tContactPersonDetail TCPD on oh.ContactID1 = TCPD.ID LEFT OUTER JOIN Torderdetail Tod on oh.id = Tod.Orderheadid LEFT OUTER JOIN mDeliveryBarcode Mdb ON oh.Id = Mdb.OrderID LEFT OUTER JOIN   mTerritory Mt on oh.Storeid = mt.id LEFT OUTER JOIN  tAddress Ta on oh.locationid = ta.Id where  oh.id = " + OrderID + "", conn);

            //return ds;


        }

        public DataSet GetReturnOrderReceiptSummaryReport(string frmdtlcl, string todtlcl, string SelectedRec, string SelectedCompany, string SelectedDepartment, string SelectedStatus, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_GetROReceiptSummaryReport";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@FrmDt", frmdtlcl);
                cmd.Parameters.AddWithValue("@Todt", todtlcl);
                cmd.Parameters.AddWithValue("@oid", SelectedRec);
                cmd.Parameters.AddWithValue("@companyid", SelectedCompany);
                cmd.Parameters.AddWithValue("@Departmentid", SelectedDepartment);
                cmd.Parameters.AddWithValue("@Status", SelectedStatus);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
            //ds = fillds("SELECT PMM.Methodname , TCPD.Mobileno, Tod.Prod_Name, Tod.Prod_code, Tod.Price, Tod.Orderqty, oh.OrderNumber, Mt.Territory, ta.AddressLine1, ta.AddressLine2,ta.City,Ta.state,Ta.County,Ta.Zipcode, Mdb.OrderNoBarCode,Mdb.AmtPaidBarCode, oh.OrderNo,oh.OrderNumber,convert(varchar,oh.Orderdate,103)Orderdate, oh.GrandTotal, convert(varchar,oh.Orderdate,103)OrderCreationDate, convert(varchar,OH.Deliverydate,103)IDExpiryDate FROM  Torderhead oh LEFT OUTER JOIN mPaymentMethodMain as PMM on  Oh.PaymentID = Pmm.Id Left Outer Join tContactPersonDetail TCPD on oh.ContactID1 = TCPD.ID LEFT OUTER JOIN Torderdetail Tod on oh.id = Tod.Orderheadid LEFT OUTER JOIN mDeliveryBarcode Mdb ON oh.Id = Mdb.OrderID LEFT OUTER JOIN   mTerritory Mt on oh.Storeid = mt.id LEFT OUTER JOIN  tAddress Ta on oh.locationid = ta.Id where  oh.id = " + OrderID + "", conn);

            //return ds;


        }
        #endregion
    }
}
