using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Data.Entity;
using System.Data;

using Domain.Server;

namespace Interface.Company
{

    [ServiceContract]
    public partial interface iCompanySetup
    {
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mCompany> GetGroupCompany(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long InsertmCompany(mCompany Company, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int UpdateCompany(mCompany UpdateCompany, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string checkDuplicateRecord(string CompanyName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string checkDuplicateRecordEdit(string CompanyName, int CompanyID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        mCompany GetCompanyById(Int64 CompanyID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int InsertmCompanyRegistration(tCompanyRegistrationDetail Company, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int UpdateCompanyRegistration(tCompanyRegistrationDetail UpdateCompany, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<vGetCompanyDetail> GetCompanyList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        tCompanyRegistrationDetail GetCompanyRegisById(Int64 CompanyID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string ChechkCompanyServerNameDuplicate(string ServerName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string ChechkCompanyDatabaseNameDuplicate(string DataBaseName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string ChechkCompanyNameDuplicate(string CompanyName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string ChechkCompanyServer_DataBaseNameDuplicate(string ServerName, string Database, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void RestoreDatabase(string DataBaseName, string[] conn);

        # region New Customer or company code for GWC project

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetCompanyName(long ID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetDepartmentListforgrid(long ParentID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void Savecustdeptinfo(long ParentID, string Territory, long Sequence, string StoreCode, long ApprovalLevel, string AutoCancel, long cancelDays, string CreatedBy, DateTime CreationDate, string Active, string ApprovalRem, long ApproRemSchedul, string AutoCancRen, long AutoRemSchedule, bool GwcDeliveries, bool ECommerce, string OrderFormat, long MaxDeliveryDays, long AddressType, bool PriceChange, string location, int MerchantCode, decimal TokenExptime, long UserID, decimal PreOrderExpTime, string CustAnalytics,bool emoney, bool AutoApproval, int ExpiryDays, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetDepartmentToEdit(long ID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateDeptInfo(long ID, string Territory, string StoreCode, long ApprovalLevel, string AutoCancel, long cancelDays, string CreatedBy, DateTime CreationDate, string Active, string ApprovalRem, long ApproRemSchedul, string AutoCancRen, long AutoRemSchedule, bool GwcDeliveries, bool ECommerce, string OrderFormat, long MaxDeliveryDays, long FinApproverID, long AddressType, bool PriceChange, string location, int MerchantCode, decimal TokenExptime, long UserID, decimal PreOrderExpTime, string CustAnalytics,bool emoney, bool AutoApproval, int ExpiryDays, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long chkDeptDuplicate(string Territory, string StoreCode, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetDeptListWithSLA(long ParentID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetLocationList(long ID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long SaveLocContactInContact(string ObjectName, long ReferenceID, long CustomerHeadID, long Sequence, string Name, string EmailID, string MobileNo, long ContactTypeID, string Active, string CreatedBy, DateTime CreationDate, long CompanyID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void SaveEditLocation(long ID, long CompanyID, long ReferenceID, string LocationCode, string AddressLine1, string AddressLine2, string County, string State, string City, string zipcode, string landmark, string FaxNo, string Active, string ContactName, string ContactEmail, string LocationName, long MobileNo, string CreatedBy, DateTime CreationDate, string hdnstate, long ShippingID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateLocationDetails(long ID, long CompanyID, long ReferenceID, string LocationCode, string AddressLine1, string AddressLine2, string County, string State, string City, string zipcode, string landmark, string FaxNo, string Active, string ContactName, string ContactEmail, long MobileNo, string CreatedBy, DateTime CreationDate, string hdnstate, string LocationName, long ShippingID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string getFinApprovername(long ID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetAddressType(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long chkecommerceduplicate(long ParentID, long ID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void InsertIntoDeptPayment(long DeptID, long PMethodID, int Sequence, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void DeleteRecordWithZeroQty(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void RemoveDeptPMethod(long ID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateDeptPaymentMethod(long DeptID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetCostCenterList(long CompanyID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void DeleteZeroCompanyIDCostCenter(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void SaveCostCenter(string CenterName, string Code, long ApproverID, long CompanyID, string Remark, DateTime CreationDate, long CreatedBy, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void RemoveCostCenter(long ID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateCostCenterCmpanyID(long CompanyID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long Duplicatecostcenter(string CenterName, string Code, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long CheckDuplicatePMethod(long PMethodID, long DeptID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int CheckLocationIDForAssignedUser(long Location, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long GetContactIdasShippingId(long ID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateContacttableDetail(string Name, string EmailID, string MobileNo, long ID, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<tContactPersonDetail> GetContactPersonListDeptWise(long CompanyID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetContactPersonLocList(long CompanyID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet CheckLocationCode(string LocationCode, string[] conn);
        #endregion


        #region Department version3

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void AddData(long CompanyId, string CompanyNm, long DeptId, string DeptNm, string SchemaNm, string DatabaseName, string ConnectionString, string Deptcode, string Active, string wmsstorecode, long CreatedBy, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet BindGrid(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetDataByID(long ID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet BindCompany(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long FindMaxNumber(long CompanyId, long DeptId, string SchemaNm, string DatabaseName, string ConnectionString, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateData(long Id, long CompanyId, string CompanyNm, long DeptId, string DeptNm, string SchemaNm, string DatabaseName, string ConnectionString, string Deptcode, string Active, string wmsstorecode, long ModifiedBy, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetDepartmentCode(long CompanyId, long DeptId, string[] conn);

        #endregion


        #region Reason code


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetReasonCodeData(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void InsertReasonCodeData(string comid, string deptid, string typeid, string rcode, string rcodedes, string defaultvalue, string active, string reasonarb, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateReasonCodeData(long rid, string comid, string deptid, string typeid, string rcode, string rcodedes, string defaultvalue, string active, string reasonarb, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet BindSearchDataIntoGrid(string deptid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string CheckDefaultValueIsPresentOrNot(string comid, string deptid, string typeid, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetReasoncodeId(string comid, string deptid, string rcode, string typeid, string rcodedes, string active, string defaultvalue, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateReasonCodeDataUsingId(long id, string defaultvalue, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string ResoncodeCheckDuplicate(string comid, string deptid, string typeid, string rcode, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mTerritory> GetDepartmentList1(int ComapnyID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mCompany> GetUserCompanyNameNEW(long UID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateRecord(string comid, string deptid, string typeid, string[] conn);

        #endregion


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetDBConfigureDataByID(long DeptId, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet BindCompanyUserWise(long companyid, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet BindSFTPGrid(string[] conn);

        #region Minal

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long checkmerchantcode(long Deptid, string[] conn);


        #endregion


        #region Dedicated driver

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetEcommerceCompany(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mTerritory> GetDepartmentList2(int ComapnyID, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GrtDriverList(long companyd, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string CHKDuplicateDedicateddr(string State, string Driverid, string Customer, string Dept, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void ADDDedicatedDR(long id, string State, string Driverid, string Customer, string Dept, long userid, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetDRList(string filter, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetDriverData(string ID, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string ChkDrAssignActiveOrd(string drid, string[] conn);

        #endregion

        
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetPrdLst(long StoreId, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetPaymentLst(long StoreId, string[] conn);

        #region VFC Bulk import
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void SaveUserDetails(string compid, string deptid, string usersid, long uid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetUserDetails(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void DeleteUserConfig(string id, long uid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetCompanyListNEw(string[] conn);

        #endregion

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mCompany> GetCompanyDropDown(long companyid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetCustomerID(long UserID, string[] conn);
    }
}
