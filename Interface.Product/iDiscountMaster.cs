using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Data.Entity;

using System.ServiceModel.Activation;
using System.ServiceModel.Web;

namespace Interface.Product
{
     [ServiceContract]

      public partial interface iDiscountMaster

    {
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<tDiscountHead> GetDiscountList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int InserttDiscountHead(tDiscountHead disc, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int UpdatetDiscountHead(tDiscountHead updatedisc, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        tDiscountHead GetDiscountListByID(int discountId, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string checkDuplicateRecord(string disName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string checkDuplicateRecordEdit(string disName, int disID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetDiscountRecordToBindGrid(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)] 
        List<SP_GetProductListForDiscountMaster_Result> CreateProductTempDataList(long[] paraProductIDs, string paraSessionID, long paraDiscountID, string paraUserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)] 
        List<SP_GetProductListForDiscountMaster_Result> GetExistingTempDataBySessionIDObjectName(string paraSessionID, string paraUserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetProductListForDiscountMaster_Result> GetDiscountListByDiscountID(string paraSessionID, long paraDiscountID, string paraUserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void ClearTempDataFromDB(string paraSessionID, string paraUserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]  
        List<SP_GetProductListForDiscountMaster_Result> RemoveProductFromTempDataList(string paraSessionID, string paraUserID, int paraSequence, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void FinalSaveToDBtDiscountMappingDetails(string paraSessionID, long paraDiscountID, string paraUserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateRecord(string paraSessionID, string paraUserID, SP_GetProductListForDiscountMaster_Result order, string[] conn);

    }
}
