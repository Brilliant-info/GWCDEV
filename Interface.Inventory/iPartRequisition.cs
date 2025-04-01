using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Data.Entity;

using System.Data;
using System.Data.SqlClient;

namespace Interface.Inventory
{
    [ServiceContract]
    public partial interface iPartRequisition
    {
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mStatu> GetStatusList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<vGetUserProfileList> GetmUserList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet FillSearchedParts(string wherecondition, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<tPartRequisition> GettRequisitions(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        tPartRequisition GetRequisitionByID(long RecID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        tPartReqDetail GetRequisitionDetailsByID(long RecID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long InsertRequisitions(tPartRequisition objRequisitions, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long updateRequisitions(tPartRequisition objRequisitions, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long InsertReqDetails(DataTable dt, long PRM_ID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long UpdateReqDetails(tPartReqDetail objReqDetails, string[] conn);



        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetdsFromSql(string strSql, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetPartReqData_Result> GetPartReqDataList(string userType, long siteID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string CheckForPartRefIdExist(long PartRecID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetIssueData_Result> GetPartReqDataIssueList(string userType, long siteID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        tPartReqDetail GetRequisitionDetailsByPRD_ID(long RecID, string[] conn);

        // [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //string GetRequestStatus(string userType, long ReqID, string[] conn);



        #region PartReqDtls

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetProductListForPartRequisition_Result> FillInventoryProductDetailByInventoryID(string paraSessionID, long paraInventoryID, long paraSiteID, string paraUserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void SaveTempData(List<SP_GetProductListForPartRequisition_Result> saveLst, string paraSessionID, string paraUserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void ClearTempData(string paraSessionID, string paraUserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetProductListForPartRequisition_Result> AddProduct(long[] paraProductIDs, string paraSessionID, string paraUserID, long SiteID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetProductListForPartRequisition_Result> GetTempDataByObjectNameSessionID(string paraSessionID, string paraUserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetProductListForPartRequisition_Result> SetSequence(List<SP_GetProductListForPartRequisition_Result> lst);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateProductDetail(string paraSessionID, string paraUserID, SP_GetProductListForPartRequisition_Result order, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void FinalSaveToDBtInventoryProductDetail(string paraSessionID, long paraReferenceID, string paraUserID, string paraContainer, string paraEngineModel, string paraEngineSerial, string paraFailureCauses, string paraFailureNature, string paraGenerateModel, long paraFailureHrs, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetProductListForPartRequisition_Result> RemoveProductFromTempDataList(string paraSessionID, string paraUserID, int paraSequence, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetRequestStatus(string userType, long ReqID, string[] conn);

        #endregion
    }
}
