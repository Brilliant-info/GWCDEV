using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Data.Entity;
using System.Data;


namespace Interface.AccountSearchDetails
{
    [ServiceContract]
    public interface iCustomer
    {
        #region AllMethodsForAccountMaster
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<GetCustomerDetail> GetGetCustomerDetail(long UserID, string LoginUserType, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<GetCustomerDetail> GetGetCustomerDetailForApplication(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<GetCustomerDetail> GetGetCustomerDetailForAdmission(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mSector> GetLeadSector(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mCustomerType> GetCompanyType(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int SaveCustomerDetails(tCustomerHead cust, string State, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        tCustomerHead GetCustomerHeadDetailByCustomerID(long customerID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string checkDuplicateRecord(string AccountName, string userID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string checkDuplicateRecordEdit(int CustomerID, string AccountName, string UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_AcountHistory_Result> GetAccountHistoryDtls(string paraInvoiceId, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int SaveOpeningBalance(tOpeningBalance ObjOpeningBal, string state, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        tOpeningBalance GetTOpeningBalanceDtls(long CustomerId, string[] conn);

        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //int SaveRegistrationDetails(tbl_Registration Regis, string state, string[] conn);

        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //int SaveLoginDetails(tbl_Login Login, string state, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        GetCustomerDetail GetAllCustomerDetailsByID(long CustomerID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetOptionalSubject(string[] conn);
        #endregion

        #region AccountCourse

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        SP_CourseStudentWise_Result GetCourseTempData(string SessionID, string TargetObjectName, string UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void SetValuesToTempData_onChangeCourse(string SessionID, string UserID, string TargetObjectName, int Sequence, SP_CourseStudentWise_Result paraInput, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void InsertIntoTempCourse(SP_CourseStudentWise_Result Course, string SessionID, string UserID, string TargetObjectName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_CourseStudentWise_Result> GetCourseTempDataToBindGrid(string TargetObjectName, string SessionID, string UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_CourseStudentWise_Result> GetCourseDataByStudentID(long StudID, string sessionID, string userID, string CurrentObject, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetProductNameFromID(long PrdCode, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void FinalSaveStudentCourse(string paraSessionID, string paraCurrentObjectName, long StudID, string paraUserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void ClearTempDataFromDB(string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_CourseStudentWise_Result> RemoveCourseFromTempDataList(string paraSessionID, string paraUserID, string paraCurrentObjectName, int paraSequence, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string ChangeStatusOfSelectedRecord(string selectedRecd, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string ChangeStatusOfSelectedRecordToStud(string selectedRecd, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void AllocateCenterToSelStudent(string selectedStuds, long AllocatedCenter, string[] conn);
        #endregion

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int SaveApplicationFeeDetails(tInvoiceHead invc, string State, tAddToCartProductDetail PrdDetail, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long GetNewInvoiceNo(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetPrdDetail(long PrdID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long GetInvoiceIdOfCustomer(long CustID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void SavePymentDetails(long RegID, long CourseID, string TransactionAmt, DateTime TransactionDate, string PayMode, DateTime DateCreated, long Product_Id, DateTime StartDate, DateTime EndDate, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet getProductIDByCourseID(long CourseID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet CheckCoupen(long UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet UpdateCoupen(string CoupenNo, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void SaveContactDetails(long RegID, string email, string Mobile, DateTime DateCreated, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void SaveContactDetails1(long RegID, string email, string Mobile, DateTime datemodified, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void DeletetImportStudentData(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetimportStudentdata(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GettempImportStudentData(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void InsertStudInRegistration(string Title, string Fname, string Mname, string Lname, string Gender, string Usertype, string Active, DateTime Datecreated, string PartnerType, long PartnerID, long CourseID, string Medium, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void InsertStudInCustomerHead(long SectorID, string Name, string DisplayName, string Active, long CreatedBy, DateTime CreationDate, long CompanyID, string ConperID, long BillingAddressID, long ShippingAddressID, long BranchID, string Fname, string Mname, string Lname, string Gender, DateTime Date_of_Registration, string EmailID, string MobileNo, long CourseID, string Medium, string payment, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetRegID(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void InsrtIntoMediumTable(long RegID, string Medium, DateTime StartDate, DateTime EndDate, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void InsertStudDataInLoginTable(long RegID, string UserName, string Password, string Active, DateTime Datecreated, int AccessPeriod, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void InsertStudDataInContactTable(long RegID, string EmailID, string MobileNo, string Active, DateTime Datecreated, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void InsertStudDataInPaymentTable(long RegID, long CourseID, string TransactionAmt, DateTime TransactionDate, string PayMode, DateTime Datecreated, long Product_Id, DateTime StartDate, DateTime EndDate, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet UpdateMediumInRegister(long RegID, string Medium, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetCustID(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet UpdateStudIDinRegister(long CustID, long RegID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetRegisterIDByAccountID(long AccountID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetloginDetailsByRegisterId(long RegisterID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetMediumDetailsByRegisterId(long RegisterID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void InsertMediumInMeduimTable(long RegID, string medium, DateTime StartDate, DateTime EndDate, string[] conn);


        #region CalculateTotalLicence
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int GetTotalNoofLicences(string SelStud, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int InsertPaymentBooking(decimal Amount, decimal TotalLicense, string StudID, string ddlPaymentMode, DateTime hdnDT, string txtBankName, string txtTransactionNo, string txtChequeNo, string txtRemark, long CustomerID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateMediumTable(long RegID, string Medium, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetOTP(string[] conn);
        #endregion

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void AddIntomSLA(mSLA ObjSLA, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        mSLA GetSLADetailsDeptWise(long DptID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void DeleteFanancialApprover(long DptID, string[] conn);
    }
}
