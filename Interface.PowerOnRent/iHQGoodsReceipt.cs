using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;


namespace Interface.PowerOnRent
{
    [ServiceContract]
    public partial interface iHQGoodsReceipt
    {
        #region [HQ] Part Receipt Head
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        PORtGRNHead GetReceiptHeadByReceiptID(long ReceiptID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long SetIntoGRNHead(PORtGRNHead GRNHead, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mStatu> GetStatusListForGRN(string Remark, string state, long UserID, string[] conn);
        #endregion

        #region [HQ] Part Details Of Receipt
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetails_OfGRN_HQ_Result> GetReceiptPartDetailByReceiptID(long ReceiptID, long SiteID, string sessionID, string userID, string CurrentObject, string IssuedQtySameAs, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetails_OfGRN_HQ_Result> GetReceiptPartDetailByIssueID(long IssueID, long SiteID, string sessionID, string userID, string CurrentObject, string IssuedQtySameAs, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void ClearTempDataFromDB(string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetails_OfGRN_HQ_Result> GetExistingTempDataBySessionIDObjectName(string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetails_OfGRN_HQ_Result> AddPartIntoReceipt_TempData(string PartIDs, long SiteID, string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetails_OfGRN_HQ_Result> RemovePartFromReceipt_TempData(string paraSessionID, string paraUserID, string paraCurrentObjectName, int paraSequence, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string[] UpdatePartReceipt_TempData(string SessionID, string CurrentObjectName, string UserID, POR_SP_GetPartDetails_OfGRN_HQ_Result Receipt, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetails_OfGRN_HQ_Result> RemovePartFromHQReceipt_TempData(string paraSessionID, string paraUserID, string paraCurrentObjectName, int paraSequence, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void FinalSaveReceiptPartDetail(string paraSessionID, string paraCurrentObjectName, long GRNH_ID, string paraUserID, string[] conn);

        #endregion

        #region [HQ] Receipt Summary
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetHQReceiptSummaryBySiteIDsOrUserIDOrReceiptIDs_Result> GetReceiptSummaryByUserID(long UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetHQReceiptSummaryBySiteIDsOrUserIDOrReceiptIDs_Result> GetReceiptSummaryBySiteIDs(string SiteIDs, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetHQReceiptSummaryBySiteIDsOrUserIDOrReceiptIDs_Result> GetReceiptSummaryByReceiptIDs(string ReceiptIDs, string[] conn);
        #endregion

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetails_OfMIN_Transfer_Result> GetIssuePartDetailByIssueID_Transfer(long IssueID, string sessionID, string userID, string CurrentObject, string IssuedQtySameAs, long SiteID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetails_OfMIN_Transfer_Result> GetExistingTempDataBySessionIDObjectName_Transfer(string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn);
    }
}
