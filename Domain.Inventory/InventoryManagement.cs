using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Activation;
using System.Configuration;
using Interface.Inventory;
using System.ServiceModel;
using Interface.Inventory;
using Domain.Tempdata;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Data.Objects;
using System.Globalization;
using System.Reflection;
using System.Xml.Linq;
using System.Data;
using System.Collections;
using Domain.Server;
   
namespace Domain.Inventory
{    
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public partial class InventoryManagement : Interface.Inventory.iInventoryManagment
    {
        Server.Server svr = new Server.Server();
       
        DataHelper datahelper = new DataHelper();
        #region AllMethodsFor_tInventoryHead

        #region InsertInvetory
        //<summary>
        //insert record of Invetory into tDocument
        //</summary>
        //<returns></returns>
        public int InsertInvetory(tInventoryHead Inv, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.tInventoryHeads.AddObject(Inv);
            ce.SaveChanges();
            return Convert.ToInt32(Inv.ID);
        }
        #endregion

        #region GetInventoryHeadListByDescending
        //<summary>
        //GetDocumentList is providing List of Product
        //</summary>
        //<returns></returns>

        public List<tInventoryHead> GetInventoryHeadListByDescending(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<tInventoryHead> InventoryHead = new List<tInventoryHead>();
            var InventoryHeadList = (from p in ce.tInventoryHeads
                                     orderby p.ID descending
                                     select p).ToList();

            foreach (var v in InventoryHeadList)
            {
                tInventoryHead ObjInventoryHeadList = new tInventoryHead();
                ObjInventoryHeadList.ID = v.ID;
                ObjInventoryHeadList.Type = v.Type;
                ObjInventoryHeadList.PONo = v.PONo;
                ObjInventoryHeadList.Date = v.Date;
                ObjInventoryHeadList.VendorID = v.VendorID;
                ObjInventoryHeadList.CreatedBy = v.CreatedBy;
                ObjInventoryHeadList.CreationDate = v.CreationDate;
                ObjInventoryHeadList.LastModifiedBy = v.LastModifiedBy;
                ObjInventoryHeadList.LastModifiedDate = v.LastModifiedDate;
                ObjInventoryHeadList.Remark = v.Remark;
                InventoryHead.Add(ObjInventoryHeadList);
            }
            return InventoryHead;

        }
        #endregion

        #region GetVendorList()
        public List<tVendorHead> GetVendorList1(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<tVendorHead> VendorList = new List<tVendorHead>();
            VendorList = (from C in ce.tVendorHeads
                          select C).ToList();
            return VendorList;
        }
        #endregion
        
        #region GetInvertoy_VendorList
        //<summary>
        //GetDocumentList is providing List of Product
        //</summary>
        //<returns></returns>

        public List<vGetInventoryVendorDetail> GetInvertoy_VendorList(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<vGetInventoryVendorDetail> listOfInventoryVendor = new List<vGetInventoryVendorDetail>();


            listOfInventoryVendor = (from C in ce.vGetInventoryVendorDetails
                          select C).ToList();           
            return listOfInventoryVendor;
        }
        #endregion

        #region updateInvetoryHead
        public int updateInvetoryHead(tInventoryHead updateInvetoryHead, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.tInventoryHeads.Attach(updateInvetoryHead);
            ce.ObjectStateManager.ChangeObjectState(updateInvetoryHead, System.Data.EntityState.Modified);
            ce.SaveChanges();
            return Convert.ToInt32(updateInvetoryHead.ID);
        }
        #endregion

        #endregion

        #region AllMethodsFor_tInventoryDetail

        public List<SP_GetProductListForInventory_Result> FillInventoryProductDetailByInventoryID(string paraSessionID, long paraInventoryID, string paraUserID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetProductListForInventory_Result> lst = new List<SP_GetProductListForInventory_Result>();
            lst = (from db in ce.SP_GetProductListForInventory(paraInventoryID, paraUserID)
                   select db).ToList();
            SaveTempData(lst, paraSessionID, paraUserID,conn);
            return lst;
        }

        public void SaveTempData(List<SP_GetProductListForInventory_Result> saveLst, string paraSessionID, string paraUserID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            TempData tempdata = new TempData();
            TempData tempdata1 = new TempData();
            tempdata = (ce.TempDatas.Where(a => a.ObjectName == "InventoryProduct" && a.SessionID == paraSessionID)).FirstOrDefault();
            tempdata1 = (ce.TempDatas.Where(a => a.ObjectName == "InventoryProduct" && a.SessionID == paraSessionID)).FirstOrDefault();
            string xml = "";
            xml = datahelper.SerializeEntity(saveLst);

            if (tempdata == null) { tempdata = new TempData(); }

            tempdata.Data = xml;
            tempdata.XmlData = "";
            tempdata.LastUpdated = DateTime.Now;
            tempdata.SessionID = paraSessionID.ToString();
            tempdata.UserID = paraUserID.ToString();
            tempdata.ObjectName = "InventoryProduct";
            tempdata.TableName = "table";
            if (tempdata1 == null) { ce.AddToTempDatas(tempdata); }
            ce.SaveChanges();
        }

        public void ClearTempData(string paraSessionID, string paraUserID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            TempData tempdata = new TempData();
            tempdata = (from rec in ce.TempDatas
                        where rec.SessionID == paraSessionID
                        && rec.UserID == paraUserID
                        && rec.ObjectName == "InventoryProduct"
                        select rec).FirstOrDefault();
            if (tempdata != null) { ce.DeleteObject(tempdata); ce.SaveChanges(); }

        }

        public List<SP_GetProductListForInventory_Result> AddProduct(long[] paraProductIDs, string paraSessionID, long paraVendorID, string paraUserID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetProductListForInventory_Result> lst = new List<SP_GetProductListForInventory_Result>();
            List<SP_GetProductListForInventory_Result> Existinglst = new List<SP_GetProductListForInventory_Result>();
            Existinglst = GetTempDataByObjectNameSessionID(paraSessionID, paraUserID,conn);

            List<SP_GetProductListForInventory_Result> NewList = new List<SP_GetProductListForInventory_Result>();
            NewList = (from db in ce.SP_GetProductListForInventory(0, paraUserID)
                       where paraProductIDs.Contains(Convert.ToInt32(db.ProductID))
                       select db).ToList();


            /*Begin : Merge (Existing + Newly Added) Products to Create TempData of AddToCart*/
            List<SP_GetProductListForInventory_Result> mergedList = new List<SP_GetProductListForInventory_Result>();
            mergedList.AddRange(Existinglst);
            mergedList.AddRange(NewList);

            lst = SetSequence(mergedList);
            SaveTempData(lst, paraSessionID, paraUserID,conn);
            return lst;
        }

        public List<SP_GetProductListForInventory_Result> GetTempDataByObjectNameSessionID(string paraSessionID, string paraUserID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetProductListForInventory_Result> lst = new List<SP_GetProductListForInventory_Result>();
            TempData tempdata = new TempData();
            tempdata = (from temp in ce.TempDatas
                        where temp.SessionID == paraSessionID
                        && temp.ObjectName == "InventoryProduct"
                        && temp.UserID == paraUserID
                        select temp).FirstOrDefault();
            if (tempdata != null)
            {
                lst = datahelper.DeserializeEntity1<SP_GetProductListForInventory_Result>(tempdata.Data);
            }
            return lst;
        }

        public List<SP_GetProductListForInventory_Result> SetSequence(List<SP_GetProductListForInventory_Result> lst)
        {
            long setRowNo = 1;
            var setSequence = from rec in lst
                              select new SP_GetProductListForInventory_Result()
                              {
                                  ID = rec.ID,
                                  Sequence = setRowNo++,
                                  ProductID = rec.ProductID,
                                  ProductCode = rec.ProductCode,
                                  ProductName = rec.ProductName,
                                  UOM = rec.UOM,
                                  Quantity=rec.Quantity,
                                  ProductRate=rec.ProductRate,                               
                                  Remark = rec.Remark                                 
                              };

            List<SP_GetProductListForInventory_Result> finalList = new List<SP_GetProductListForInventory_Result>();
            finalList = setSequence.ToList<SP_GetProductListForInventory_Result>();
            return finalList;
        }

        public void UpdateProductDetail(string paraSessionID, string paraUserID, SP_GetProductListForInventory_Result order, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetProductListForInventory_Result> lst = new List<SP_GetProductListForInventory_Result>();
            lst = GetTempDataByObjectNameSessionID(paraSessionID, paraUserID,conn);
            SP_GetProductListForInventory_Result updateRec = new SP_GetProductListForInventory_Result();
            updateRec = lst.Where(u => u.Sequence == order.Sequence).FirstOrDefault();
            if (updateRec == null)
            {
                SP_GetProductListForInventory_Result updateRec1 = new SP_GetProductListForInventory_Result();
                updateRec1.ProductRate = order.ProductRate;
                updateRec1.Quantity = order.Quantity;
                updateRec1.Remark = order.Remark;

            }
            else
            {
                updateRec.ProductRate = order.ProductRate;
                updateRec.Quantity = order.Quantity;
                updateRec.Remark = order.Remark;
            }
            SaveTempData(lst, paraSessionID, paraUserID,conn);
        }
        public void FinalSaveToDBtInventoryProductDetail(string paraSessionID, long paraReferenceID, string paraUserID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetProductListForInventory_Result> paraobjList = new List<SP_GetProductListForInventory_Result>();
            paraobjList = GetTempDataByObjectNameSessionID(paraSessionID, paraUserID,conn);

            XElement xmlEle = new XElement("InventoryProductDetailList", from rec in paraobjList
                                                                      select new XElement("InventoryProduct",
                                                            new XElement("ID", rec.ID),
                                                            new XElement("Sequence", rec.Sequence),
                                                            new XElement("ProductID", rec.ProductID),
                                                            new XElement("ProductName", rec.ProductName),
                                                            new XElement("UOM", rec.UOM),
                                                            new XElement("Quantity", rec.Quantity),
                                                            new XElement("ProductRate", rec.ProductRate),
                                                            new XElement("Remark", rec.Remark)
                                                            ));
            TempData tempdata = new TempData();

            tempdata = (ce.TempDatas.Where(a => a.ObjectName == "InventoryProduct" && a.SessionID == paraSessionID)).FirstOrDefault();
            tempdata.XmlData = xmlEle.ToString();
            tempdata.LastUpdated = DateTime.Now;
            tempdata.SessionID = paraSessionID.ToString();
            tempdata.UserID = paraUserID.ToString();
            tempdata.ObjectName = "InventoryProduct";
            tempdata.TableName = "table";
            ce.SaveChanges();

            ObjectParameter _paraSessionID = new ObjectParameter("paraSessionID", typeof(string));
            _paraSessionID.Value = paraSessionID;

            ObjectParameter _paraObjectName = new ObjectParameter("paraObjectName", typeof(string));
            _paraObjectName.Value = "InventoryProduct";

            ObjectParameter _paraReferenceID = new ObjectParameter("paraReferenceID", typeof(long));
            _paraReferenceID.Value = paraReferenceID;

            ObjectParameter _paraUserID = new ObjectParameter("paraUserID", typeof(string));
            _paraUserID.Value = paraUserID;          


            ObjectParameter[] obj = new ObjectParameter[] { _paraSessionID, _paraObjectName, _paraReferenceID, _paraUserID};
            ce.ExecuteFunction("SP_InsertIntoTInventoryDetail", obj);
            ce.SaveChanges();
            ClearTempData(paraSessionID, paraUserID,conn);

        }

        public List<SP_GetProductListForInventory_Result> RemoveProductFromTempDataList(string paraSessionID, string paraUserID, int paraSequence, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            /*Begin : Get Existing Records from TempData*/
            List<SP_GetProductListForInventory_Result> existingDiscountList = new List<SP_GetProductListForInventory_Result>();
            existingDiscountList = GetTempDataByObjectNameSessionID(paraSessionID, paraUserID,conn);
            /*End*/

            /*Get Filter List [Filter By paraSequence]*/
            List<SP_GetProductListForInventory_Result> filterList = new List<SP_GetProductListForInventory_Result>();
            filterList = (from exist in existingDiscountList
                          where exist.Sequence != paraSequence
                          select exist).ToList();
            /*End*/

            List<SP_GetProductListForInventory_Result> result = new List<SP_GetProductListForInventory_Result>();
            /*Set Sequence*/
            result = SetSequence(filterList);

            /*End*/

            /*Save result to TempData*/
            SaveTempData(result, paraSessionID, paraUserID,conn);
            /*End*/

            return result;
        }



        #endregion
       


    
    }
}
