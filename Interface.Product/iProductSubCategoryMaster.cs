using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Data.Entity;
using System.Data;


namespace Interface.Product
{
    [ServiceContract]
    public partial interface iProductSubCategoryMaster
    {
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mProductSubCategory> GetProductSubCategoryList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int InsertmProductSubCategory(mProductSubCategory prdSubCategory, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int updatemProductSubCategory(mProductSubCategory updateprdSubCategory, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        mProductSubCategory GetProductSubCategoryListByID(int ProductSubCategoryId, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string checkDuplicateRecord(string productSubCategoryName, int prdCategoryID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string checkDuplicateRecordEdit(int productSubCategoryID, string productSubCategoryName, int productCategoryID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetPrdSubCategoryRecordToBind(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<vGetProductSubCagetoryList> GetProductSubCategoryByProductCategoryID(long productCategoryID, string[] conn);
    }
}
