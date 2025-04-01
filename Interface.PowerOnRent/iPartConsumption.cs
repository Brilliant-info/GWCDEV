using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;


namespace Interface.PowerOnRent
{
    [ServiceContract]
    public partial interface iPartConsumption
    {
        #region Part Consumption Head
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        PORtConsumptionHead GetConsumptionHeadByConsumptionID(long ConsumptionID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long SetIntoPartConsumptionHead(PORtConsumptionHead ConsumptionHead, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mStatu> GetStatusListForConsumption(string Remark, string state, long UserID, string[] conn);
        #endregion

        #region Consumption Part Detail
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetails_OfConsumption_Result> GetConsumptionPartDetailByReceiptID(long ReceiptID, long siteID, string sessionID, string userID, string CurrentObject, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetails_OfConsumption_Result> GetConsumptionPartDetailByConsumptionID(long ConsumptionID, long siteID, string sessionID, string userID, string CurrentObject, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void ClearTempDataFromDB(string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetails_OfConsumption_Result> GetExistingTempDataBySessionIDObjectName(string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetails_OfConsumption_Result> AddPartIntoConsumption_TempDataByPartIDs(string paraProductIDs, string paraSessionID, string paraUserID, string paraCurrentObjectName, long SiteID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetails_OfConsumption_Result> AddPartIntoConsumption_TempDataByGrdIDs(string GrdIDs, string paraSessionID, string paraUserID, string paraCurrentObjectName, long SiteID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetPartDetails_OfConsumption_Result> RemovePartFrom_TempData(string paraSessionID, string paraUserID, string paraCurrentObjectName, int paraSequence, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdatePartConsumedQty_TempData(string SessionID, string CurrentObjectName, string UserID, POR_SP_GetPartDetails_OfConsumption_Result Consumption, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void FinalSavePartDetail(string paraSessionID, string paraCurrentObjectName, long paraReferenceID, string paraUserID, string[] conn);
        #endregion

        #region Consumption Summary
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetConsumptionSummaryBySiteIDsOrUserIDOrReceiptIDsOrConsumptionIDs_Result> GetConsumptionSummayByUserID(long UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetConsumptionSummaryBySiteIDsOrUserIDOrReceiptIDsOrConsumptionIDs_Result> GetConsumptionSummayBySiteIDs(string SiteIDs, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetConsumptionSummaryBySiteIDsOrUserIDOrReceiptIDsOrConsumptionIDs_Result> GetConsumptionSummayByReceiptIDs(string ReceiptIDs, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_GetConsumptionSummaryBySiteIDsOrUserIDOrReceiptIDsOrConsumptionIDs_Result> GetConsumptionSummayByConsumptionIDs(string ConsumptionIDs, string[] conn);
        #endregion

    }
}
