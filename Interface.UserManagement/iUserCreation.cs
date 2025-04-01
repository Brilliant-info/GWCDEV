using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Data.Entity;

using System.Data;

namespace Interface.UserManagement
{
    [ServiceContract]
    public partial interface iUserCreation
    {

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetUserCreationList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long InsertUserCreation(mUserProfileHead user, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int UpdateUserProfile(mUserProfileHead User, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        mUserProfileHead GetUserByID(long UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mUserProfileHead> SelectEmployeeDepartmentwise(mDesignation objmDesignation, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string checkDuplicateRecord(string EmpCode, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string checkDuplicateRecordEdit(string EmpCode, int UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        bool FinalSaveUserRoles(List<SP_GetUserRoleDetail_Result> sessionList, string userID, long userIDForRole, long CompanyID, long RoleID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetUserRoleDetail_Result> UpdateRoleIntoSessionList(List<SP_GetUserRoleDetail_Result> sessionList, SP_GetUserRoleDetail_Result updateRole, int rowindex);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetUserRoleDetail_Result> GetDataToBindRoleMasterDetailsByRoleID(long RoleID, long UserIdForRole, long CompanyID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<vGetUserProfileList> GetUserList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        vGetUserProfileByUserID GetUserProfileByUserID(long UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void SaveUsersLocationDetails(long ToUserID, long Level, long LocationIDs, string CreatedBy, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetLocationListByUserID(long UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetHTMLMenuByUserID(long UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetHTMLMenuArabicByUserID(long UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetTerritoryID_FromUserId(long userId, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetSiteNameFromId(string sid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mCompany> GetCompanyName(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<vGWCGetUserProfileList> GetGWCUserList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mDropdownValue> GetUserType(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        bool FinalSaveGWCUserRoles(List<SP_GWCGetUserRoleDetail_Result> sessionList, string userID, long userIDForRole, long CompanyID, long RoleID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GWCGetUserRoleDetail_Result> GetRoleDetails(long RoleID, long UserIdForRole, long CompanyID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<vGWCGetUserList> GWCSearchUserList(long ComanyID, long DeptID, string[] conn);

        # region
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetUserDelegationDetail(string state, long DeligateFrom, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void SaveEditUserDelegation(long ID, long DeligateFrom, DateTime FromDate, DateTime ToDate, long DeligateTo, string Remark, string state, long CreatedBy, DateTime CreatedDate, long DeptID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet getUserDelegateDetail(long ID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet getDelegateToList(long DepartmentID, long UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateDelegateFrom(long DeligateFrom, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetDepartmentforUsersave(long ParentID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetRollNameById(long TerritoryID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetUserTypeByRoll(long ID, string[] conn);

        #endregion

        #region Passwordpolicy

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet CheckPasswordHistory(long UserProfileID, string Password, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void SavePasswordDetails(long UserProfileID, string Email, string UserName, string Password, string transactedby, string ip, string type, long userid, string[] conn);

        #endregion Passwordpolicy

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetUserDepartment(long userID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet getDelegateToListMultipleDept(string SelectedLocation, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void updatelockunlock(string UserName, byte IsLockedOut, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetLogoPath(long ID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int GetApprovalDetailsOfUser(long userID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int GetAdditionalDistributationOfUser(long userID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetDepartmentDelegate(long UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetDepartmentSSuperAdmin(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void Deletedelegate(long ID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long GetDuplicateDelegate(DateTime FromDate, DateTime ToDate, long DeligateTo, long DeptID, long DeligateFrom, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string CheckOneDayValidation(long UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetUserNameByID(long UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetComppanyLogo(long CmpnyID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetUserLoginDetails(long UserProfileID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void InsertIntoUserLocation(long UserID, long LocationID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long GetDuplicatlocationUser(long UserID, long LocationID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void RemoveUserLoc(long ID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void RemoveDepartment(long ID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetUserRoleID(string userid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetApprovalLeveldetailsofUser(string userid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateUserTerritoryDetails(long userid, string createdby, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void DeleteUserTeritoryifUserIdZero(string createdby, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long CheckUserterritoryDuplicate(long TerritoryId, long UserID, long CreatedBy, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void insertUserterritory(long TerritoryId, long UserID, long CreatedBy, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetDuplicateAssignDeptForUser(long deptid, string createdby, string[] conn);

        #region Version 3.0 code

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetAllLocation(long ID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void InsertAllLocation(DataTable dt, long UserId, string[] conn);


        #endregion


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetSkuListDeptWiseNewe1(int pageIndex, string filter, long UserID, long DeptID, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetSkuListDeptWiseToUpdate1(int pageIndex, string filter, long UserID, long DeptID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetDatabaseSchemaforDept1(long DeptId, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetProdAvailableBal1(long Pid, string[] conn);


        #region Security question and get IP detail Suraj Jagtap 28/12/2019

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet BindDDLQuestion(string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long SaveSecurityQuestion(string q1, string q2, string q3, string a1, string a2, string a3, long Userid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet BindDDLQuestionforuser(long userid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string WMChkSecurityQuestion(string q1, string q2, string a1, string a2, long userid, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string ClearSecurityQuestions(string uid, string ipadd, string ttype, string UserName, long userid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet BindQuestionandAnswer(long UserID, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetDataSetTable(string userid, string q1, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetStringReturn(string userid, string q1, string[] conn);



        #endregion

        #region Serial Number

        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //DataSet GetProductSerialDetails(long Oid, long Sid, string[] conn);
        #endregion

        #region VF Import
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GWCSearchUserListVF(long Cid, long Did, string[] conn);



        #endregion

        #region check return order access
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int CheckRMSUser(long userid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int GetMarkAsReturnOrder(string SelectedOrder, string userType, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void insertReturnOrder(string OrderId, long userid, string[] conn);

        #endregion

        #region  return order Collection Report
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetAllReturnOrdercollectionreport(string FrmDt, string Todt, string SelectedCompany, string SelectedDepartment, string SelectedUser, string SelectedStatus, string SelectedDriver, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetAllReturnOrderSummaryreport(string FrmDt, string Todt, string SelectedCompany, string SelectedDepartment, string SelectedUser, string SelectedStatus, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetReturnCustomerName(long UID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetReturnDepartmentList(long CID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetReturnStatus(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetReturnDriver(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetReturnOrderCollectionReportData(string FrmDt, string Todt, string SelectedRec, string SelectedCompany, string SelectedDepartment, string SelectedDriver, string SelectedStatus, string[] conn);
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetReturnOrderReceiptSummaryReport(string frmdtlcl, string todtlcl, string SelectedRec, string SelectedCompany, string SelectedDepartment, string SelectedStatus, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void RemoveReturnMenuNewUser( long UserID, string state, string[] conn);
        #endregion


    }
}
