using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

using System.ServiceModel.Activation;
using System.ServiceModel.Web;


namespace Interface.Inventory
{
    [ServiceContract]
    public partial interface iInventoryManagment
    {
         #region AllMethodsFortInvetroyHead
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int InsertInvetory(tInventoryHead Inv, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<tVendorHead> GetVendorList1( string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<vGetInventoryVendorDetail> GetInvertoy_VendorList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int updateInvetoryHead(tInventoryHead updateInvetoryHead, string[] conn);

             #endregion

        #region AllMethodsFortInvetroyProductDetail
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetProductListForInventory_Result> FillInventoryProductDetailByInventoryID(string paraSessionID, long paraInventoryID, string paraUserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void SaveTempData(List<SP_GetProductListForInventory_Result> saveLst, string paraSessionID, string paraUserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void ClearTempData(string paraSessionID, string paraUserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetProductListForInventory_Result> AddProduct(long[] paraProductIDs, string paraSessionID, long paraVendorID, string paraUserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetProductListForInventory_Result> GetTempDataByObjectNameSessionID(string paraSessionID, string paraUserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetProductListForInventory_Result> SetSequence(List<SP_GetProductListForInventory_Result> lst);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateProductDetail(string paraSessionID, string paraUserID, SP_GetProductListForInventory_Result order, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void FinalSaveToDBtInventoryProductDetail(string paraSessionID, long paraReferenceID, string paraUserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetProductListForInventory_Result> RemoveProductFromTempDataList(string paraSessionID, string paraUserID, int paraSequence, string[] conn);

        #endregion

    }
}
