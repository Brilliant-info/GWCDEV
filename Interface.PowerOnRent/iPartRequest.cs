using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Data;


namespace Interface.PowerOnRent
{
    [ServiceContract]
    public partial interface iPartRequest
    {
        #region Part Request Head
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        PORtPartRequestHead GetRequestHeadByRequestID(long RequestID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long SetIntoPartRequestHead(PORtPartRequestHead PartRequest, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mStatu> GetStatusListForRequest(string Remark, string state, long UserID, string[] conn);
        #endregion

        #region Request Part Detail
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetail_ForRequest_Result> GetRequestPartDetailByRequestID(long RequestID, long siteID, string sessionID, string userID, string CurrentObject, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void ClearTempDataFromDB(string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void ClearTempDataFromDBNEW(string paraSessionID, string paraUserID, string CurrentObjectName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetail_ForRequest_Result> GetExistingTempDataBySessionIDObjectName(string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetail_ForRequest_Result> AddPartIntoRequest_TempData(string paraProductIDs, string paraSessionID, string paraUserID, string paraCurrentObjectName, long SiteID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetail_ForRequest_Result> RemovePartFromRequest_TempData(string paraSessionID, string paraUserID, string paraCurrentObjectName, int paraSequence, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdatePartRequest_TempData(string SessionID, string CurrentObjectName, string UserID, POR_SP_GetPartDetail_ForRequest_Result Request, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdatePartRequest_TempData1(string SessionID, string CurrentObjectName, string UserID, POR_SP_GetPartDetail_ForRequest_Result Request, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int FinalSaveRequestPartDetail(string paraSessionID, string paraCurrentObjectName, long paraReferenceID, string paraUserID, int StatusID, long DepartmentID, long PreviousStatusID, string[] conn);
        #endregion

        #region GridRequestList Summary
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetRequestBySiteIDsOrUserID_Result> GetRequestSummayByUserID(long UserID, string[] conn, string OrdType);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetRequestSummayByUserIDNew(long UserID, string[] conn, string Ordtype);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetRequestBySiteIDsOrUserID_Result> GetRequestSummayByUserIDIssue(long UserID, string[] conn, string OrdType);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetRequestBySiteIDsOrUserID_Result> GetRequestSummayBySiteIDs(string SiteIDs, string[] conn, string OrdType);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetRequestByRequestIDs_Result> GetRequestSummayByRequestIDs(string RequestIDs, string[] conn);
        #endregion

        #region Approval Code
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string SaveApprovalStatus(long RequestID, string ApprovalStatus, string ApprovalRemark, long UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        tApprovalDetail GetApprovalDetailsByReqestID(long RequestID, string[] conn);
        #endregion

        #region GWCGetRequestSummayByUserID
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetTemplateDetails(long UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetTemplateDetailsSuperAdmin(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetTemplateDetailsAdmin(long UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetTemplateDetailsBind(long UserID, long DeptID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetGetInterfaceDetails(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetGetMessageDetails(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetApprovalDetailsNew(long OrderID, long ApproverID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetApprovalDetailsAllApproved(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetUOMofSelectedProduct(int ProdID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetRequestBySiteIDsOrUserID_Result> GetRequestSummayOfUser(long UserID, string[] conn, string Ordtype);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetRequestBySiteIDsOrUserID_Result> GetRequestSummayOfUserIssue(long UserID, string[] conn, string Ordtype);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long SetIntotOrderHead(tOrderHead PartRequest, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        tOrderHead GetOrderHeadByOrderID(long OrderID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long InsertIntomRequestTemplateHead(mRequestTemplateHead ReqTemplHead, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void FinalSavemRequestTemplateDetail(string paraSessionID, string paraCurrentObjectName, long paraReferenceID, string paraUserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void FinalSavemRequestTemplateDetailTemplate(string paraSessionID, string paraCurrentObjectName, long paraReferenceID, string paraUserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        mRequestTemplateHead GetTemplateOrderHead(long TemplateID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetTemplatePartLstByTemplateID(long TemplateID, string[] conn);

        #endregion

        #region GWC_Approval
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdatetApprovalTransAfterApproval(long ApprovalID, long RequestID, long statusID, string Remark, long ApproverID, string InvoiceNo, string tag, string ismoney, string ERPfinacial, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdatetApprovalTransAfterReject(long ApprovalID, long RequestID, long statusID, string Remark, long ApproverID, string Reasoncode, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int SetApproverDataafterSave(string paraCurrentObjectName, long paraReferenceID, string paraUserID, int StatusID, long DepartmentID, long PreviousStatusID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long GetPreviousStatusID(long RequestId, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetApprovalDetailsNewAdmin(long OrderID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void InsertIntotCorrespond(tCorrespond Msg, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetCorrespondance(long RequestID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        tCorrespond GetCorrespondanceDetail(long CORID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetBomDetails(string PrdID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetSelectedUom(long OrderId, long ProdID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetSelectedUomTemplate(long OrderId, long ProdID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int GridRowCount(string paraSessionID, string paraCurrentObjectName, string paraUserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetail_ForRequest_Result> GridRowsTemplate(string paraSessionID, string paraCurrentObjectName, string paraUserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long ChkTemplateTitle(string TemplateTitle, string[] conn);

        #endregion

        #region GWC_Deliveries

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        VW_OrderDeliveryDetails GetOrderDeliveryDetails(long OrderID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int GetDispatchedOrders(string SelectedOrder, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetDriverDetails(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int AssignSelectedDriver(long orderNo, long DriverID, string TruckDetails, long AssignBy, string[] conn);
        #endregion

        #region GWCVer2
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetDeptWisePaymentMethod(long selectedDept, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetPaymentMethodFields(long SelpaymentMethod, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mPaymentMethodDetail> GetPMFields(long SelpaymentMethod, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<VW_DeptWisePaymentMethod> GEtDeptPaymentmethod(long selectedDept, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mCostCenterMain> GetCostCenter(long DeptId, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdatePartRequest_TempData12(string SessionID, string CurrentObjectName, string UserID, POR_SP_GetPartDetail_ForRequest_Result Request, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        decimal GetTotalFromTempData(string SessionID, string CurrentObjectName, string UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        decimal GetTotalQTYFromTempData(string SessionID, string CurrentObjectName, string UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long GetMaxDeliveryDaysofDept(long Dept, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetMandatoryFields(long pm, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long GetStatutoryID(string PMLabel, long pmID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void AddIntotStatutory(tStatutoryDetail pmd, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void AddIntotStatutoryNEW(string ObjectName, long ReferenceID, long StatutoryID, string StatutoryValue, long CreatedBy, long Sequence, long ApproverID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int GetDeptPriceChange(long deptID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetNewOrderNo(long StoreId, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateStatutoryDetails(long RequestID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int IsPriceChanged(int ProdID, decimal price, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetSelectedCostCenter(long RequestId, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetAddedAdditionalFields(long RequestID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int GetPartAccessofUser(long requestID, long UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetProductOfOrder(long OrderID, int Sequence, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetOrderProductAccess(long requestID, long UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int IsPriceEditYN(long OrderID, long UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int IsSkuChangeYN(long OrderID, long UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetQtyofSelectedUOM(long SelectedProduct, long SelectedUOM, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int UpdateOrderQtyTotal(decimal OrderQty, decimal Price, decimal Total, long OrderID, int Sequence, long UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<tOrderHead> GetOrderHeadByOrderIDQTYTotal(long OrderID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int CancelSelectedOrder(long SelectedOrder, int reasoncode, long UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetUOMName(long UOMID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long GetCostCenterApproverID(long StatutoryValue, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetFilteredDriverList(string SearchValue, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetInvoiceNoofOrder(long OrderID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void RemoveFromTStatutory(long OrderID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int EmailSendWhenRequestSubmit(long RequestID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void EmailSendofApproved(long ApproverID, long RequestID, int id, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetApproverDepartmentWise(long Deptid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void addUpdateGrandTotal(long OrderHeadId, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void updateApprovedflag(long OrderID, int ApprovalLevel, long UserApproverID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long CheckForAllreadyApproved(long OrderID, int ApprovalLevel, long UserApproverID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet checkLocationForDepartment(long deptid, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetOrderType(long Id, string skucode, int packageid, string[] conn);
        #endregion

        #region New Change Requirment

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetDeptReqUser(long ID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        decimal GetCurrentStockofProduct(long pid, string[] conn);

        #endregion


        #region Driver assign 25 order

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet CheckPreviousOrderCount(long driverid, string[] conn);

        #endregion

        #region Ver 3by suraj

        #region Show Dispatch Grid information

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet ShowDispatchGrid(long OrderId, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetOrderdate(long OrderID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string UpdatePendingforActivation(long RequestID, string SkuCode, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetReportData(Int64 OrderID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string CheckAllProductISActiveOrNot(Int64 OrderID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string UpdateVirtualProduct(long RequestID, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string ISCheckPendingforAvtivation(string RequestID, string Skucode, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetOrderNo(long OrderID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string UpdateDocumentation(long Orderno, string skucode, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string ISCompleteOrderOrNot(string OrderID, string Skucode, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string UpdateCompletedOrder(long Orderno, string skucode, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string UpdateMSISDN(long Id, string MSSIDN, string skucode, string[] conn);




        #endregion




        #region Cust analytics

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetCustomerAnalyticsData(string orderno, string[] conn);

        #endregion

        #region Version3 by suraj show orderparameter GV

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet BindGVOrderParameter(long OrderId, string[] conn);
        #endregion



        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //List<POR_SP_GetRequestDetails_Result> GetRequestSuperAdmin(long userid, string[] conn, string ordtype);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetOrderDetails(long Id, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string ChkBarcode(long Id, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void InsertintomDeliverybarCode(long Id, byte[] ono, byte[] amtpaid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet BindReasonCodeDDL(string ordno, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet BindReasonCodeDDLForCancel(string ordno, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetRequestSearch(string fdate, string todate, string ordcat, string ordno, string lcode, string passport, string ordertype, string misidn, string email, long UserID, string semserial, string paymenttype, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string ISeCommerceOrNot(string userid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetRequestBySiteIDsOrUserID_Result> GetRequestSummayOfRetailUser(long UserID, string[] conn, string Ordtype);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void ActivtaedSendEmail(long RequestID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string CheckDeptIsEcomOrNot(long orderno, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string ISEcommOrNot(long orderno, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetBoardbandReportData(Int64 OrderID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string ChkOrdtypeIsOnlyDevice(long orderno, string[] conn);

        #endregion


        #region Advance Search
        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //DataSet GetRequestSummayByUserIDAdvanceSearch(string fdate, string todate, string ordcat, string ordno, string lcode, string passport, string ordtype, string misidn, long UserID,string invoker, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetRequestByForAdvanceSearch_Result> GetRequestSummayByUserIDIssueAdvanceSearch(string fdate, string todate, string ordcat, string ordno, string lcode, string passport, string ordtype, string misidn, long UserID, string Invoker, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetRequestByForAdvanceSearch_Result> GetRequestSummayByUserIDAdvanceSearch(string fdate, string todate, string ordcat, string ordno, string lcode, string passport, string ordtype, string misidn, long UserID, string Invoker, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetRequestSummayOfRetailUserSearch(string fdate, string todate, string ordcat, string ordno, string lcode, string passport, string ordertype, string misidn, string email, long UserID, string semserial, string paymenttype, string[] conn);
        #endregion

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string ChangeStatus(string OrderID, string cngvalue, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long GetStatus(string orderno, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int GetOrdersCnt(string orderno, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string CheckStatusForDriverAllocation(long orderno, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetDeliveryType(string OrderNo, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetMobilenumberforpostpaidreport(string OrderNo, string Skucode, string PackageID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetExtrasvalues(string OrderNo, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetMobilePackName(string OrderNo, string Skucode, string PackageID, string[] conn);


        #region QNBN Report

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string CkhUserType(long userid, string[] conn);


        #endregion

        #region Noor Project

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string CheckOrderIsFMSOrNot(string OrderID, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetFMSReportData(Int64 OrderID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int GetDispatchedstatusOrders(string SelectedOrder, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int GetDispstatusOrdforfullfillment(string SelectedOrder, string[] conn);
        #endregion


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string ChkOrdIsApproved(long OrdId, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string FunChkmsgispresentOrNotforapproval(long RequestID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string FunChkmsgispresentOrNotforapprovalNew(long RequestID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string InterfacestaggingfailOrder(long RequestID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void SendMessageInterfacenotification(long RequestID, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string FunChkOrderIsApprovedorNotapproval(long RequestID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateTProductStockReserveQtyTotalDispatchQtyapproval(long RequestID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string CheckStockadjustmentisdoneapproval(long RequestID, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetProjectSiteDetails(long ID, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string SaveEditmodedata(string ID, string pid, string sid, long uid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string ChkISFlextProduct(long Oid, string[] conn);

        #region SerialNumber  
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet CheckISvft(long Oid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateOrderEditFlag(long Id, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string ChkISOrdCreatorOrNot(long Oid, long Uid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string ChkSerialedCompany(long Companyid, string[] conn);

        #endregion

        #region Request Serial Number
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetPartSerialDetail_Result> GetPartSerialDetailByRequestID(long RequestID, long siteID, string sessionID, string userID, string CurrentObject, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetPartSerialDetail_Result> GetExistingTempDataBySessionIDSerialNumber(string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetPartSerialDetail_Result> AddPartIntoRqstSeialNumber_TempData(string paraProductIDs, string paraSessionID, string paraUserID, string paraCurrentObjectName, long SiteID, long Skuid, long Orderid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int GetSelectedSKUQty(string paraSessionID, string paraUserID, string paraCurrentObjectName, string skuid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetail_ForRequest_Result> GetSelectedSKUQty1(string paraSessionID, string paraUserID, string paraCurrentObjectName, string skuid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string ChkSerialedCompanyDeptID(long Deptid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int FinalSaveRequestPartDetailSerial(string paraSessionID, string paraCurrentObjectName, long paraReferenceID, string paraUserID, long deptid, long PreviousStatusID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetail_ForRequest_Result> AddPartIntoRequest_TempDataNew(string paraProductIDs, string paraSessionID, string paraUserID, string paraCurrentObjectName, long SiteID, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetPartSerialDetail_Result> GetRequestPartserialDetailByRequestID(long RequestID, long siteID, string sessionID, string userID, string CurrentObject, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void DeleteSerialTemptable(long oid, string[] conn);
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetOnlySerialsku(long Oid, string[] conn);
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetPartSerialDetail_Result> RemoveSkuserialFromRequest_TempData(string paraSessionID, string paraUserID, string paraCurrentObjectName, long paraSequence, string[] conn);
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void DeleteSkuwiseSerialTemptable(long UserID, long skuid, long Oid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string ChkOrderSerialedCompany(long OId, string[] conn);
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void SaveTempDataToDBNew(List<POR_SP_GetPartDetail_ForRequest_Result> paraobjList, string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn);
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdatePartRequest_TempDataNew(string SessionID, string UserID, string CurrentObjectName, long sid, decimal qty, string[] conn);
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void DeleteSkuwiseSerialFromqtychange(long UserID, long skuid, long Oid, string eventnm, string[] conn);
        #endregion

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetUserCustomer(long UserID, long CompanyID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void DeleteResurveSerialnumber(string paraSessionID, string paraUserID, string paraCurrentObjectName, long orderid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void EmailSendToRequestorchnage(long RequestID, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void Updateallowflag(long orderid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void EmailSendWhenRequestreSubmit(long RequestID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void AddIntomMessageTransNew(long RequestID, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetReportDataNew(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetReportDataNew1(string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetReportDataNew3(string[] conn);

        #region Bulk Order Import
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int SetApproverDataafterSaveBulk(string paraCurrentObjectName, long paraReferenceID, string paraUserID, int StatusID, long DepartmentID, long PreviousStatusID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void SaveCostcenterApprover(string paymethodvalue, long paymenyid, long oid, long storeid, long uid, string[] conn);
        #endregion

        #region Emoney

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet getismoneydept(long SearchValue, string[] conn);
        #endregion

        #region 2022 combine CR document pending Approval list

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetPendingApprovalList(long UseriD, string[] conn, string Ordertype);

        #endregion

        #region ERP

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string CheckStoreERPAutoApproved(long StoreID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet CheckStoreERPAutoByReqID(long ReqID, long userID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet getErpOrderhead(long orderID, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet getErpOrderdetail(long orderID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void Savefailedaprorder(long OrderID, string servicedesc,string status, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet chkisERPAutoApproval(string deptID, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void InsetERPOrderNotificationLog(long OrderID, long Orderby, long storeID, string[] conn);
        #endregion

        #region 2022 combine CR document project 3 return, cancel & expire Order

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int checkstatusforRefundupdate(long SelectedOrder, string UserType, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetRefundstatus(string UserType, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string UpdatefinRefundstatus(long OrderID, long statusID, string status, long userid, string Codevalue, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet emailGetOrderHead(long OrderID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet emailGetOrderDetail(long OrderID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetAccountEmail(long OrderID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetExpiredCancelEmail(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void InsertOrderStatuslog(long OrderID, long statusID, string status, long userid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void MailCompose(string UserEmail, long RequestID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetAccountPendingfinActivity(long UserID, string[] conn, string Ordtype);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int CheckRetailCancelMailsend(long OrderID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string getrefunddate(long Orderid, string[] conn);

        #endregion

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long UpdateIseApprflag(long requestID, string[] conn);

        #region Normal Order show

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetNormalOrderDetails(long Id, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string ChkNormalOrderBarcode(long Id, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void InsertintoNormalOrdermDeliverybarCode(long Id, byte[] ono, byte[] amtpaid, string[] conn);

        #endregion


        #region Project 2 change status CR 03-10-2023

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet Getcngstatus(long orderno, string UserType, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int GestatusOrdforSuperAdmin1(long SelectedOrder, string userType, string[] conn);
        #endregion




        #region Remove entity code

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void EmailSendWhenRequestreSubmit_New(long RequestID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int FinalSaveRequestPartDetailSerial_New(string paraSessionID, string paraCurrentObjectName, long paraReferenceID, string paraUserID, long deptid, long PreviousStatusID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        decimal GetCurrentStockofProduct_New(long pid, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long GetPreviousStatusID_New(long RequestId, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long SetIntotOrderHead_New(long CreatedBy, string Creationdate, long Id, long LastModifiedBy, string LastModifiedDt, long StoreId, string OrderNumber,
                                        string Priority, long Status, string Title, string Orderdate, long RequestBy, string Remark, string Deliverydate, long ContactId1,
                                        long AddressId, string Con2, long PaymentID, decimal TotalQty, decimal GrandTotal, long LocationID, long ProjTypeID,
                                        long SiteID, string ASNNo, string RefundCode, string OrderNo, string orderType, long segmentID, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int FinalSaveRequestPartDetail_New(string paraSessionID, string paraCurrentObjectName, long paraReferenceID, string paraUserID, int StatusID, long DepartmentID, long PreviousStatusID, string[] conn);

        #endregion
    }
}

