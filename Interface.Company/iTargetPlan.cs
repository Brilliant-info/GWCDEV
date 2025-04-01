using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Data.Entity;

using System.ServiceModel.Activation;

namespace Interface.Company
{
    [ServiceContract]
    public partial interface iTargetPlan
    {
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int InserttTargetPlan(tTargetPlan target, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int UpdatetTargetPlan(tTargetPlan updatetarget, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        tTargetPlan GetTargetListByID(int targetId, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetTargetRecordToBindGrid(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetProductListForTargetPlan_Result> CreateProductTempDataList(long[] paraProductIDs, string paraSessionID, long paraTargetID, string paraUserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetProductListForTargetPlan_Result> GetExistingTempDataBySessionIDObjectName(string paraSessionID, string paraUserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetProductListForTargetPlan_Result> GetTargetListByTargetID(string paraSessionID, long paraTargetID, string paraUserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void ClearTempDataFromDB(string paraSessionID, string paraUserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetProductListForTargetPlan_Result> RemoveProductFromTempDataList(string paraSessionID, string paraUserID, int paraSequence, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void FinalSaveToDBtTargetPlanDetails(string paraSessionID, long paraTargetID, string paraUserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateRecord(string paraSessionID, string paraUserID, SP_GetProductListForTargetPlan_Result order, string[] conn);
    }
}
