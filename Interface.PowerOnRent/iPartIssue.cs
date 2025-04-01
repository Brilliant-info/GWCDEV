using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;


namespace Interface.PowerOnRent
{
    [ServiceContract]
    public partial interface iPartIssue
    {
        #region Part Issue Head
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        PORtMINHead GetIssueHeadByIssueID(long IssueID, string[] conn);

        /// <summary>
        /// Get status of Material Issue when record save
        /// -When All part issue against Request then Status is Fully Issued [7]
        /// -When Partial part issue against Request then Status is Partial Issued [6]
        /// </summary>
        /// <param name="SessionID"></param>
        /// <param name="UserID"></param>
        /// <param name="ObjectName"></param>
        /// <param name="RequestID"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long GetStatusOfIssueHead(string SessionID, string UserID, string ObjectName, long RequestID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long SetIntoMINHead(PORtMINHead MINHead, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mStatu> GetStatusListForIssue(string Remark, string state, long UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        bool CheckPendingIssueListToDecideAddNewAccess(long RequestID, string[] conn);
        #endregion

        #region Part Issue Detail
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetails_OfMIN_Result> GetIssuePartDetailByRequestID(long RequestID, string sessionID, string userID, string CurrentObject, string IssuedQtySameAs, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetails_OfMIN_Result> GetIssuePartDetailByIssueID(long IssueID, string sessionID, string userID, string CurrentObject, string IssuedQtySameAs, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void ClearTempDataFromDB(string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetails_OfMIN_Result> GetExistingTempDataBySessionIDObjectName(string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetails_OfMIN_Result> AddPartIntoIssue_TempData(string PRD_IDs, long IssueID, long RequestID, string IssuedQtySameAs, string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetails_OfMIN_Result> RemovePartFromIssue_TempData(string paraSessionID, string paraUserID, string paraCurrentObjectName, int paraSequence, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string UpdatePartIssue_TempData(string SessionID, string CurrentObjectName, string UserID, POR_SP_GetPartDetails_OfMIN_Result Issue, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void FinalSaveIssuePartDetail(string paraSessionID, string paraCurrentObjectName, long MINH_ID, long PRH_ID, string paraUserID, string IssueStatus, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string UpdateHQStock_TempData(string SessionID, string CurrentObjectName, string UserID, POR_SP_GetPartDetails_OfMIN_Result Issue, string[] conn);
        #endregion

        #region Display Pending Issue Part List
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetails_OfMIN_Result> GetPendingIssuePartList(string SessionID, string UserID, string ObjectName, long RequestID, string[] conn);
        #endregion

        #region Issue Summary
       /* [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetIssueSummaryBySiteIDsOrUserIDOrRequestIDOrIssueIDs_Result> GetIssueSummayByUserID(long UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetIssueSummaryBySiteIDsOrUserIDOrRequestIDOrIssueIDs_Result> GetIssueSummayBySiteIDs(string SiteIDs, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetIssueSummaryBySiteIDsOrUserIDOrRequestIDOrIssueIDs_Result> GetIssueSummayByRequestID(long RequestID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetIssueSummaryBySiteIDsOrUserIDOrRequestIDOrIssueIDs_Result> GetIssueSummayByIssueIDs(string IssueIDs, string[] conn);*/
        #endregion


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long SetIntoTransHead(PORtTransHead TransHead, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void FinalSaveTransferPartDetail(string paraSessionID, string paraCurrentObjectName, long TransH_ID, long PRH_ID, string paraUserID, string TransferStatus, long FromSiteID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void FinalSaveIssuePartDetail_Transfer(string paraSessionID, string paraCurrentObjectName, long MINH_ID, long PRH_ID, string paraUserID, string IssueStatus, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long SetIntoMINHead_Transfer(PORtMINHead MINHead, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long GetStatusOfIssueHead_Transfer(string SessionID, string UserID, string ObjectName, long RequestID, string[] conn);

        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //List<POR_SP_GetPartDetails_OfMIN_Transfer_Result> RemovePartFromTransfer_TempData(string paraSessionID, string paraUserID, string paraCurrentObjectName, int paraSequence, string[] conn);
        
    }
}
