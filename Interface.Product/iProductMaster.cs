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
    public partial interface iProductMaster
    {
        #region Bind Dropdown
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mProductType> GetProductTypeList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mUOM> GetProductUOMList(string[] conn);
        #endregion

        #region Product Info
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        mProduct GetmProductToUpdate(long productID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        GetProductDetail GetProductDetailByProductID(long productID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        mProduct GetProductDetailByProductIDForUpdate(long productID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long FinalSaveProductDetailByProductID(mProduct product, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<GetProductDetail> GetProductList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<GetProductDetail> GetProductListDeptWise(long UID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<GetProductDetail> GetAssetList(string[] conn);
        #endregion

        #region Prodcut Tax Setup
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateTempTaxSetup(string TaxID, string IsChecked, string sessionID, string userID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetProductMasterTaxSetupByProductID_Result> GetProductTaxDetailByProductID(long productID, string sessionID, string userID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetProductMasterTaxSetupByProductID_Result> GetTempSaveProductTaxDetailBySessionID(string sessionID, string userID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void FinalSaveProductTaxDetailByProductID(string sessionID, string userID, long productID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void ClearTempSaveProductTaxDetailBySessionID(string sessionID, string userid, string[] conn);
        #endregion

        #region Prodcut Specification
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mProductSpecificationDetail> AddProductSpecificationToTempData(mProductSpecificationDetail ProductSpecification, string sessionID, string userID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mProductSpecificationDetail> GetProductSpecificationDetailByProductID(long productID, string sessionID, string userID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void ClearTempSaveProductSpecificationDetailBySessionID(string sessionID, string userid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void FinalSaveProductSpecificationDetailByProductID(string sessionID, string userID, long productID, long companyID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mProductSpecificationDetail> SetValuesToTempData_onChange(long productID, string sessionID, string userID, string[] conn, int paraSequence, mProductSpecificationDetail paraInput);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        mProductSpecificationDetail GetSpecificationDetailFromTempTableBySequence(string paraSessionID, string paraUserID, long productID, int paraSequence, string[] conn);
        #endregion

        #region ProductMaster Images
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void AddTempProductImages(tImage AddImage, string sessionID, string userID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetProductImagesByProductID(long productID, string sessionID, string userID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetTempSaveProductImagesBySessionID(string sessionID, string userID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void FinalSaveProductImagesByProductID(string sessionID, string userID, long productID, string FilePath, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void ClearTempSaveProductImagesBySessionID(string sessionID, string userid, string[] conn);
        #endregion

        #region ProductMaster checkDuplicateRecord
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string checkDuplicateRecord(string ProdName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string checkDuplicateRecordEdit(int ProdID, string ProdName, string[] conn);

        #endregion

        #region ProductMaster change rates
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long SaveNewRates(mProductRateDetail NewRate, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mProductRateDetail> GetProductRateHistory(long ProductID, string[] conn);
        #endregion

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetProductofEngine(string frmdt, string todt, string SID, string EID, string[] conn);

        #region Inventory Code
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetSiteWiseInventoryByProductIDs_Result> GetInventoryDataByProductIDs(string ProductIDs, string sessionID, string userID, string CurrentObject, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void ClearTempDataFromDB(string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetSiteWiseInventoryByProductIDs_Result> GetExistingTempDataBySessionIDObjectName(string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long UpdateProductInvetory_TempData(string SessionID, string CurrentObjectName, string UserID, SP_GetSiteWiseInventoryByProductIDs_Result InventoryRec, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void FinalSaveProductInventory(string paraSessionID, string paraCurrentObjectName, long ProductID, DateTime EffectiveDate, string paraUserID, string[] conn);

        #endregion

        #region ToolSite

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long SaveToolSiteHistory(mToolSiteHistory mTool, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<VW_ToolSiteHistoryDetail> GetToolSiteHistory(long ProductID, string[] conn);

        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //List<VW_ToolSiteHistoryDetail> GetSitewiseTool(long SiteID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_SiteWiseTools_Result> GetSitewiseTool(long SiteID, string SessionID, string UserID, string CurrentObject, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_SiteWiseTools_Result> RemoveAssetFromCurrentAsset_TempData(string paraSessionID, string paraUserID, string paraCurrentObjectName, int paraSequence, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_SP_SiteWiseTools_Result> GetExistingTempDataBySessionIDObjectNamePrd(string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long SavetToolTransferHead(tToolTransferHead tToolHead, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void FinalSaveToolTransferDetails(string paraSessionID, string paraObjectName, string paraReferenceID, string UserID, string ToSite, DateTime TransferDate, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_VW_ToolTransferDetails> GetTransferList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        tToolTransferHead GetToolTransferHead(long TransferHeadID, string[] conn);
        #endregion

        # region  new product related code for WMS
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetDepartment(long ParentID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetCompanyname(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetUOMList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetUomShort(long ID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void InsertIntomPackUom(long SkuId, string ShortDescri, string Description, long Quantity, long Sequence, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetUOMPackDetails(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetEditmpackUOmdetail(long Id, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdatemPackUom(long Id, string ShortDescri, string Description, long Quantity, long Sequence, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetBomProductDetail(string Edit, long BOMheaderId, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void saveBomDetail(long BOMheaderId, long SKUId, long Quantity, long Sequence, string Remark, string state, long Id, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void RemoveBOMDetailSKu(long Id, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetBOMDetailById(long Id, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetUOMPackDetailsByProdId(long SkuId, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetBOmDeyailbyIdforedit(long BOMheaderId, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet getProductSpecification(long ProductID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet FillInventoryGrid(long SiteID, long ProdID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateInventryOpeningBal(decimal OpeningStock, long ProdID, long SiteID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdatePackUOMforSkuId(long SkuId, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateBOMforsSkuId(long BOMheaderId, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void DeleteMpackUOMBOMClear(string[] conn);
        # endregion

        #region WMS ImageImport Related code
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long GetSKUByFilename(string OMSSKUCode, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long getSKUIdfromtImage(long ReferenceID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void InsertintotImage(string ObjectName, long ReferenceID, string ImageName, string Path, string Extension, string CreatedBy, DateTime CreationDate, long CompanyID, byte[] SkuImage, string ImageTitle, string ImageDescr, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void InsertIntotImageImportLog(long ReferenceID, string ImageName, string Path, string CreatedBy, DateTime CreationDate, string Reason, long CompanyID, long DeptID, string OMSSkuCode, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetFailedImageDetail(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void deleteimportlogdata(long DeptID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void ClearImageUploadLog(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void SaveProductStockDetail(long SiteID, long ProdID, decimal AvailableBalance, decimal VirtualQty, decimal VirtualReQty, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        decimal UpdateProductStockQty(long ProdID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void InsertIntoDistribution(long TemplateID, long ContactID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateDistribution(long TemplateID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void DeleteDistribution(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void RemoveDistribution(long Id, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        decimal GetAvailStockById(long ProdID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetEditSpecifcdetail(long ID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void Updatespecification(long ID, string SpecificationDescription, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        tImage GetImageDetailByImageId(long ImgID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void EditTempProductImages(tImage AddImage, string sessionID, string userID, string state, string[] conn);
        #endregion

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetCreatedByDate(string SelImageID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void FinalSaveProductImagesByProductIDEdit(string sessionID, string userID, long productID, string FilePath, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long GetAVLBalance(string MinQty, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string checkDuplicateRecordSKUCODE(string omsskucode, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string checkDuplicateRecordEditSKUCODE(int ProdID, string omsskucode, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateImageInTImage(long ReferenceID, string ImageName, string Path, string Extension, string LastModifiedBy, long CompanyID, byte[] SkuImage, string ImageTitle, string ImageDescr, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetAvailQuantity(long ProdID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateVirtualBalance(long ProdID, decimal VirtualQty, decimal AvailVirtualQty, decimal VirtualReQty, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void InsertIntoInventry(long SKUId, long StoreId, DateTime Transactiondate, decimal Quantity, string[] conn);

        #region SKU Price Import
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void DeleteSKUPricetemp(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetSkuPriceTemp(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetPriceImportData(long StoreId, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateImportSkuPrice(string ProductCode, decimal PrincipalPrice, long StoreId, string[] conn);
        #endregion

        #region
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetLocation(long CompanyID, string[] conn);
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string getOrderFormatNumber(long StoreId, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long GetmaxDeliverydays(long ID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long SaveOrderHeaderImport(long StoreId, DateTime Orderdate, DateTime Deliverydate, long AddressId, long Status, long CreatedBy, DateTime Creationdate, string Title, DateTime ApprovalDate, decimal TotalQty, decimal GrandTotal, string OrderNo, long LocationID, long ProjectID, long SiteID,long segmentID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void SaveOrderDetailImport(long OrderHeadId, long SkuId, decimal OrderQty, long UOMID, long Sequence, string Prod_Name, string Prod_Description, string Prod_Code, decimal Price, decimal Total, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void updateproductstockdetailimport(long SiteID, long ProdID, decimal AvailableBalance, decimal TotalDispatchQty, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void ImportMsgTransHeader(long RequestID, string[] conn);
        #endregion

        #region New Ecommerce Code For Virtual Import
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetVirtualSKUTemp(long CreatedBy, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateUserIDTempVirtualSKU(long CreatedBy, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void DeleteTempVirtualSKUImport(long CreatedBy, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet validateVSKUImportData(long CreatedBy, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void InsertIntomProduct(string ProductCode, string Name, string Description, decimal PrincipalPrice, long StoreId, string Packkey, string CreatedBy, decimal VirtualSkuQty, decimal VirtualReOrderQty, long CompanyID, string[] conn);


        // virtaul Stock Import
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void DeleteVistualStockImport(long CreatedBy, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateUserIDtoVirtualStock(long CreatedBy, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetVirtualStocktemp(long CreatedBy, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateVirtualStockbyInport(long ProdID, long SiteID, decimal VirtualQty, long LastModifiedBy, string[] conn);


        #endregion


        #region Get real time stock configuration schema
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetDatabaseSchemaforDept(long DeptId, string[] conn);

       

        #endregion


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet BindDept(string comid, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetProductList1(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetProductListDeptWise1(long UID, string[] conn);

        //SKU Serial Changes

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateSerialFlag(string skuid, string[] conn);

        #region sku serial flag update change VT

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateserialflagProduct(long SKUID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetCompanySerial(long CompanyID, string[] conn);
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet UpdateSRNewIMPBeforeValidate(string[] conn);
        #endregion

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetcompanyRealtimestock(long deptid, string[] conn);


        //#region Bulk Order Import

        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        // DataSet GetTempBulkDirectOrder(string[] conn);

        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //DataSet GetDistinctPaymentmethodCodes(string[] conn);

        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //DataSet GetTotalForBulkOrderHead(long ID, long locationId, long paymentid, string[] conn);

        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //DataSet GetBulkImportDatabyDisDeptLocId(long ID, long locationId, long ProjectID, long SIteID, string[] conn);

        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //long SaveBulkOrderHeaderImport(long StoreId, DateTime Orderdate, DateTime Deliverydate, long AddressId, long Status, long CreatedBy, DateTime Creationdate, string Title, DateTime ApprovalDate, decimal TotalQty, decimal GrandTotal, string OrderNo, long LocationID, long Paymentid, long SiteID, string[] conn);

        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //DataSet GetDirectBulkOrderData(string[] conn);


        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //void SaveBulkOrderDetailImport(long OrderHeadId, long SkuId, decimal OrderQty, long UOMID, long Sequence, string Prod_Name, string Prod_Description, string Prod_Code, decimal Price, decimal Total, string[] conn);


        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //void DeleteBulkOrderImport(long uid, string[] conn);


        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //DataSet UpdateRealStockBeforeValidatebulk(string[] conn);

        //#endregion

        //#region import Change
        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //void DeleteOrderImport(string[] conn);
        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //DataSet GetDirectOrderData(string[] conn);



        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //DataSet GetDistinctPLCodes(string[] conn);

        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //DataSet UpdateRealStockBeforeValidate(string[] conn);

        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //DataSet GetTempDirectOrder(string[] conn);

        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //DataSet GetImportDatabyDisDeptLocId(long ID, long locationId, long ProjectID, long SIteID, string[] conn);

        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //DataSet GetTotalForOrderHead(long ID, long locationId, long ProjectID, long SIteID, string[] conn);


        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //DataSet insertImportStatus(long userID,string Obj, string[] conn);

        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //DataSet getImportStatus(string Obj,string[] conn);

        //#endregion


        #region Direct import  Change
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void DeleteOrderImport(long userID, string[] conn);
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetDirectOrderData(long userID, string[] conn);



        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetDistinctPLCodes(long userID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet UpdateRealStockBeforeValidate(long userID, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetTempDirectOrder(long userID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetImportDatabyDisDeptLocId(long ID, long locationId, long ProjectID, long SIteID, long userID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetTotalForOrderHead(long ID, long locationId, long ProjectID, long SIteID, long userID, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet insertImportStatus(long userID,string obj, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet getImportStatus(string obj,string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void CancelOrderImport(long UserID, string[] conn);

        #endregion

        #region Bulk Order Import with user

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetTempBulkDirectOrder(long userID,string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetDistinctPaymentmethodCodes(long userID,string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetTotalForBulkOrderHead(long ID, long locationId, long paymentid,long userID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetBulkImportDatabyDisDeptLocId(long ID, long locationId, long ProjectID, long SIteID,long userID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long SaveBulkOrderHeaderImport(long StoreId, DateTime Orderdate, DateTime Deliverydate, long AddressId, long Status, long CreatedBy, DateTime Creationdate, string Title, DateTime ApprovalDate, decimal TotalQty, decimal GrandTotal, string OrderNo, long LocationID, long Paymentid, long SiteID,long segmentID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetDirectBulkOrderData(long userID,string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void SaveBulkOrderDetailImport(long OrderHeadId, long SkuId, decimal OrderQty, long UOMID, long Sequence, string Prod_Name, string Prod_Description, string Prod_Code, decimal Price, decimal Total, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void DeleteBulkOrderImport(long uid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet UpdateRealStockBeforeValidatebulk(long userID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]

      void  CancelBulkOrderImport(long UserID, string[] conn);
        #endregion
    }

}
