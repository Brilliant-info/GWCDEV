using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;


namespace Interface.PowerOnRent
{
    [ServiceContract]
    public partial interface iPartReceipt
    {
        #region Part Receipt Head
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        PORtGRNHead GetReceiptHeadByReceiptID(long ReceiptID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        PORtGRNHead GetReceiptHeadByIssueID(long IssueID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long SetIntoGRNHead(PORtGRNHead GRNHead, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mStatu> GetStatusListForGRN(string Remark, string state, long UserID, string[] conn);
        #endregion

        #region Part Details OfReceipt
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetails_OfGRN_Result> GetReceiptPartDetailByIssueID(long IssueID, long SiteID, string sessionID, string userID, string CurrentObject, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetails_OfGRN_Result> GetReceiptPartDetailByReceiptID(long ReceiptID, long SiteID, string sessionID, string userID, string CurrentObject, string IssuedQtySameAs, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void ClearTempDataFromDB(string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetails_OfGRN_Result> GetExistingTempDataBySessionIDObjectName(string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetails_OfGRN_Result> AddPartIntoReceipt_TempData(string MIND_IDs, long IssueID, long SiteID, string IssuedQtySameAs, string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetails_OfGRN_Result> RemovePartFromReceipt_TempData(string paraSessionID, string paraUserID, string paraCurrentObjectName, int paraSequence, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string[] UpdatePartReceipt_TempData(string SessionID, string CurrentObjectName, string UserID, POR_SP_GetPartDetails_OfGRN_Result Receipt, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void FinalSaveReceiptPartDetail(string paraSessionID, string paraCurrentObjectName, long GRNH_ID, string paraUserID, string ReceiptStatus, string[] conn);
        #endregion

        #region Receipt Summary
       /* [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetReceiptSummaryBySiteIDsOrUserIDOrRequestID_Result> GetReceiptSummaryByUserID(long UserID, string[] conn);
        //List<POR_SP_GetReceiptSummaryBySiteIDsOrUserIDOrRequestID_Result> GetReceiptSummaryBySiteIDs(string SiteIDs, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetReceiptSummaryBySiteIDsOrUserIDOrRequestID_Result> GetReceiptSummaryBySiteIDs(string SiteIDs, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetReceiptSummaryBySiteIDsOrUserIDOrRequestID_Result> GetReceiptSummaryByRequestID(long RequestID, string[] conn);*/
        #endregion
    }
}
