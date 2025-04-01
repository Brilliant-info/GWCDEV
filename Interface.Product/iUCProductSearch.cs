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
    public partial interface iUCProductSearch
    {
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<GetProductDetail> GetProductList(string filter, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<GetProductDetail> GetProductList1(int pageIndex, string filter, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<VW_GetSKUDetailsWithPack> GetSKUList(int pageIndex, string filter, long DeptID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mProduct> SKUListForGrid(int CompanyID, int DeptID, int GroupSetID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<VW_GetSKUDetailsWithPack> GetSKUListDeptWise(int pageIndex, string filter, long UserID, long DeptID, string[] conn);

        #region Real Stock Count Suraj

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetProdAvailableBal(long Pid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void Createtransaction(string SKUCode, string DepartmentCode, decimal gtotal, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetSkuListDeptWiseToUpdate(int pageIndex, string filter, long UserID, long DeptID, string[] conn);

        #endregion


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetSkuListDeptWiseNewe(int pageIndex, string filter, long UserID, long DeptID, string[] conn);
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<GetProductSerialDetail> GetProductSerialDetailNEW(long skuid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetProductSerialDetails(long Oid, long Sid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetSkuDeptSchemaData(long Sid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetSkuDataWithSerial(long Sid, string[] conn);

        #region updatestock
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet getProductdetailsUpdate(long cntrecord,string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]

        DataSet updateIsreadFlag(string productcode, string storecode, string[] conn);
        #endregion
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long insertlog(string skuCode, long deptID, string transactiontype, decimal AVLqty, decimal Resqty, decimal realtimestkqty, string[] conn);
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long insertlogNew(string skuCode, long deptID, string transactiontype, decimal AVLqty, decimal Resqty, decimal realtimestkqty, string[] conn);
    }



}
