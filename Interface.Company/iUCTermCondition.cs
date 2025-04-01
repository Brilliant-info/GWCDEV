using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Data.Entity;

using System.ServiceModel.Activation;
using System.ServiceModel.Web;

namespace Interface.Company
{
     [ServiceContract]

    public partial interface iUCTermCondition
    {
          [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
         List<SP_GetTermConditionListForUCTermCondition_Result> CreateTermCTempDataList(long[] paraProductIDs, string paraSessionID, long paraReferenceID, string paraUserID, string paraObjectName_Old, string[] conn);

          [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
          List<SP_GetTermConditionListForUCTermCondition_Result> GetExistingTempDataBySessionIDObjectName(string paraSessionID, string paraUserID, string paraObjectName_Old, string[] conn);

          [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
          List<SP_GetTermConditionListForUCTermCondition_Result> GetTermCListByparaReferenceID(string paraSessionID, long paraReferenceID, string paraUserID, string paraObjectName_Old, string[] conn);

          [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
          void ClearTempDataFromDB(string paraSessionID, string paraUserID, string paraObjectName_Old, string[] conn);

          [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
          List<SP_GetTermConditionListForUCTermCondition_Result> RemoveProductFromTempDataList(string paraSessionID, string paraUserID, int paraSequence, string paraObjectName_Old, string[] conn);

          [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
          // void FinalSaveToDBtDiscountMappingDetails(string paraSessionID, long paraReferenceID, string paraUserID, string paraObjectName);
          void FinalSaveToDBtDiscountMappingDetails(string paraSessionID, string paraObjectName_Old, long paraReferenceID, string paraUserID, string paraObjectName_New, string[] conn);

          [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
          void UpdateRecord(string paraSessionID, string paraUserID, SP_GetTermConditionListForUCTermCondition_Result order, string paraObjectName_Old, string[] conn);

    }
}
