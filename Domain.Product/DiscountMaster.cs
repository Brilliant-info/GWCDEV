using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Activation;
using System.Configuration;
using Interface.Product;
using System.ServiceModel;
using Domain.Tempdata;
using System.Data.EntityClient;
using System.Data.Objects;
using System.Globalization;
using System.Reflection;
using System.Xml.Linq;
using Domain.Server;

namespace Domain.Product
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]

    public partial class DiscountMaster : Interface.Product.iDiscountMaster
    {
        Domain.Server.Server svr = new Server.Server();
        DataHelper datahelper = new DataHelper();

        #region GetDiscountList
        /// <summary>
        /// GetDiscountList is providing List of DiscountList
        /// </summary>
        /// <returns></returns>
        /// 
        public List<tDiscountHead> GetDiscountList(string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<tDiscountHead> Discount = new List<tDiscountHead>();
            Discount = (from p in db.tDiscountHeads
                        select p).ToList();
            return Discount;
        }
        #endregion

        #region InserttDiscountHead
        public int InserttDiscountHead(tDiscountHead disc, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            //db.tDiscountHeads.AddObject(disc); 
            db.AddTotDiscountHeads(disc);
            db.SaveChanges();
            return Convert.ToInt32(disc.ID);
        }
        #endregion

        #region UpdatetDiscountHead
        public int UpdatetDiscountHead(tDiscountHead updatedisc, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            db.tDiscountHeads.Attach(updatedisc);
            db.ObjectStateManager.ChangeObjectState(updatedisc, EntityState.Modified);
            db.SaveChanges();
            return 1;
        }
        #endregion

        #region GetDiscountListByID
        /// <summary>
        /// GetDiscountListByID is providing List of DiscountList By ID for Edit mode
        /// </summary>
        /// <returns></returns>
        /// 
        public tDiscountHead GetDiscountListByID(int discountId, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            tDiscountHead ActivityID = new tDiscountHead();
            ActivityID = (from p in db.tDiscountHeads
                          where p.ID == discountId
                          select p).FirstOrDefault();

            db.Detach(ActivityID);
            return ActivityID;
        }
        #endregion

        #region checkDuplicateRecord
        /// <summary>
        /// checkDuplicateRecord is providing List of Discount by DiscountName 
        /// </summary>
        /// <returns></returns>
        /// 
        public string checkDuplicateRecord(string disName, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            var output = (from p in db.tDiscountHeads
                          where p.Name == disName
                          select new { p.Name }).FirstOrDefault();

            if (output != null)
            {
                result = "[ " + disName + " ] Discount Name allready exist";
            }
            return result;

        }
        #endregion

        #region checkDuplicateRecordEdit
        /// <summary>
        /// checkDuplicateRecord for Edit is providing List of Discount by DiscountName and ID
        /// </summary>
        /// <returns></returns>
        /// 
        public string checkDuplicateRecordEdit(string disName, int disID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            var output = (from p in db.tDiscountHeads
                          where p.Name == disName && p.ID != disID
                          select new { p.Name }).FirstOrDefault();

            if (output != null)
            {
                result = "[ " + disName + " ] Discount Name  allready exist";
            }
            return result;
        }
        #endregion

        #region GetDiscountRecordToBindGrid
        /// <summary>
        /// GetDiscountRecordToBindGrid is providing List of Discount for bind grid with Actine Yes/No
        /// </summary>
        /// <returns></returns>
        /// 
        public DataSet GetDiscountRecordToBindGrid(string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<tDiscountHead> LeadSource = new List<tDiscountHead>();
            XElement xmlDiscountMaster = new XElement("DiscountList", from m in db.tDiscountHeads.AsEnumerable()
                                                                      orderby m.Sequence
                                                                      select new XElement("Discount",
                                                                  new XElement("ID", m.ID),
                                                                  new XElement("Name", m.Name),
                                                                  new XElement("FromDate", string.Format("{0:dd-MMM-yyyy}", m.FromDate)),
                                                                  new XElement("ToDate", string.Format("{0:dd-MMM-yyyy}", m.ToDate)),
                                                                  new XElement("Sequence", m.Sequence),
                                                                  new XElement("Active", m.Active == "Y" ? "Yes" : "No")
                                                                  ));
            DataSet ds = new DataSet();
            ds.ReadXml(xmlDiscountMaster.CreateReader());
            if (ds.Tables.Count <= 0)
            {
                ds.Tables.Add("Discount1");
            }
            return ds;
        }
        #endregion


        /// <summary>
        /// 1. CreateProductTempDataList
        ///    a. Call : GetExistingProductListBySessionIDObjectName
        ///    b. Getproduct Details for paraProductIDs
        ///    c. Merge with existing records. 
        ///    d. Merged Data Serialize
        ///    e. Call : SaveTempDataToDB
        ///    f. Return Merged List
        /// </summary>
        /// <param name="paraProductIDs"></param>
        /// <param name="paraSessionID"></param>
        /// <param name="paraUserID"></param>
        /// <param name="paraObjectName"></param>
        /// <returns></returns>
        // public List<SP_GetProductListForDiscountMaster_Result> CreateProductTempDataList(long[] paraProductIDs, string paraSessionID, string paraUserID, string paraObjectName)
        public List<SP_GetProductListForDiscountMaster_Result> CreateProductTempDataList(long[] paraProductIDs, string paraSessionID, long paraDiscountID, string paraUserID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            /*Begin : Get Existing Records from TempData*/
            List<SP_GetProductListForDiscountMaster_Result> existingProductList = new List<SP_GetProductListForDiscountMaster_Result>();
            existingProductList = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, conn);
            /*End*/

            /*Get Product Details*/
            List<SP_GetProductListForDiscountMaster_Result> getnewRec = new List<SP_GetProductListForDiscountMaster_Result>();
            getnewRec = (from view in db.SP_GetProductListForDiscountMaster(0, paraUserID)
                         where paraProductIDs.Contains(Convert.ToInt32(view.ProductID))
                         select view).ToList();
            /*End*/

            /*Begin : Merge (Existing + Newly Added) Products to Create TempData of AddToCart*/
            List<SP_GetProductListForDiscountMaster_Result> mergedDiscountList = new List<SP_GetProductListForDiscountMaster_Result>();
            mergedDiscountList.AddRange(existingProductList);
            mergedDiscountList.AddRange(getnewRec);


            List<SP_GetProductListForDiscountMaster_Result> finalmergedDiscountList = new List<SP_GetProductListForDiscountMaster_Result>();
            finalmergedDiscountList = SetSequenceList(mergedDiscountList);
            /*End*/

            /*Begin : Serialize & Save MergedAddToCartList*/
            SaveTempDataToDB(finalmergedDiscountList, paraSessionID, paraUserID, conn);
            /*End*/

            return finalmergedDiscountList;
        }

        /// <summary>
        /// 2. GetExistingTempDataBySessionIDObjectName
        ///     a. Fetch TempData by Session ID & Object Name  from DB [TempData]  
        ///     b. DeSerialize fetched data
        /// </summary>
        /// <param name="paraSessionID"></param>
        /// <param name="paraUserID"></param>
        /// <param name="paraObjectName"></param>
        /// <returns></returns>
        public List<SP_GetProductListForDiscountMaster_Result> GetExistingTempDataBySessionIDObjectName(string paraSessionID, string paraUserID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetProductListForDiscountMaster_Result> objtDiscountGridProductDetailList = new List<SP_GetProductListForDiscountMaster_Result>();

            TempData tempdata = new TempData();
            tempdata = (from temp in db.TempDatas
                        where temp.SessionID == paraSessionID
                        && temp.ObjectName == "Discount"
                        && temp.UserID == paraUserID
                        select temp).FirstOrDefault();
            if (tempdata != null)
            {
                objtDiscountGridProductDetailList = datahelper.DeserializeEntity1<SP_GetProductListForDiscountMaster_Result>(tempdata.Data);
            }
            return objtDiscountGridProductDetailList;
        }

        /// <summary>
        /// 3. SaveTempDataToDB            
        ///     a.  paraobjList Save to DB [ TempData ]
        /// </summary>
        /// <param name="paraobjList"></param>
        /// <param name="paraSessionID"></param>
        /// <param name="paraUserID"></param>
        /// <param name="paraObjectName"></param>
        protected void SaveTempDataToDB(List<SP_GetProductListForDiscountMaster_Result> paraobjList, string paraSessionID, string paraUserID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            TempData tempdata = new TempData();
            TempData tempdata1 = new TempData();
            tempdata = (db.TempDatas.Where(a => a.ObjectName == "Discount" && a.SessionID == paraSessionID)).FirstOrDefault();
            tempdata1 = (db.TempDatas.Where(a => a.ObjectName == "Discount" && a.SessionID == paraSessionID)).FirstOrDefault();

            /*Begin : Remove Existing Records*/
            // ClearTempDataFromDB(paraSessionID, paraUserID);
            /*End*/

            /*Begin : Serialize MergedAddToCartList*/
            string xml = "";
            xml = datahelper.SerializeEntity(paraobjList);
            /*End*/

            /*Begin : Save Serialized List into TempData */
            if (tempdata == null) { tempdata = new TempData(); }
            tempdata.Data = xml;
            tempdata.XmlData = "";
            tempdata.LastUpdated = DateTime.Now;
            tempdata.SessionID = paraSessionID.ToString();
            tempdata.UserID = paraUserID.ToString();
            tempdata.ObjectName = "Discount";
            tempdata.TableName = "table";
            if (tempdata1 == null) { db.AddToTempDatas(tempdata); }
            //db.AddToTempDatas(tempdata);
            db.SaveChanges();
            /*End*/

        }

        /// <summary>
        /// 4. GetDiscountListByReferenceIDObjectName
        ///     a. Fetch data from DB by ReferenceID & ObjectName order by Sequence [ tAddToCartProductDetail ]
        ///     b. Fetched data serialize
        ///     c. Serialized data save to DB [ TempData ]
        ///     d. Return fetched list
        /// </summary>
        /// <param name="paraSessionID"></param>
        /// <param name="paraUserID"></param>
        /// <param name="paraObjectName"></param>
        /// <param name="paraReferenceID"></param>
        /// <returns></returns>
        public List<SP_GetProductListForDiscountMaster_Result> GetDiscountListByDiscountID(string paraSessionID, long paraDiscountID, string paraUserID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            // List<SP_GetProductListForDiscountMaster_Result> returndDiscountListByReferenceID = new List<SP_GetProductListForDiscountMaster_Result>();
            List<SP_GetProductListForDiscountMaster_Result> newRecList = new List<SP_GetProductListForDiscountMaster_Result>();

            /*Begin : Fetch AddToCartList from tAddToCartProductDetail by ReferenceID & ObjectName*/
            newRecList = (from addtocart in db.SP_GetProductListForDiscountMaster(paraDiscountID, paraUserID).AsEnumerable()
                          //where  addtocart.ID == paraDiscountID
                          //orderby addtocart.Sequence
                          select addtocart).ToList();

            /*End*/

            ///*Begin : Serialize & Save AddToCartList*/
            SaveTempDataToDB(newRecList, paraSessionID, paraUserID, conn);
            /*End*/
            return newRecList;
        }

        /// <summary>
        /// 5. ClearTempDataFromDB
        ///     a. Fetch TempData by SessionID & ObjectName  from DB [TempData] By ObjectName & ReferenceID   
        ///     b. Delete from DB [ TempData ]where Data = fetcheddata
        /// </summary>
        /// <param name="paraSessionID"></param>
        /// <param name="paraUserID"></param>
        /// <param name="paraObjectName"></param>
        public void ClearTempDataFromDB(string paraSessionID, string paraUserID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            TempData tempdata = new TempData();
            tempdata = (from rec in db.TempDatas
                        where rec.SessionID == paraSessionID
                        && rec.UserID == paraUserID
                        && rec.ObjectName == "Discount"
                        select rec).FirstOrDefault();
            if (tempdata != null) { db.DeleteObject(tempdata); db.SaveChanges(); }

        }

        /// <summary>
        /// 7. Remove Prodcut From AddToCartList
        /// </summary>
        /// <param name="paraSessionID"></param>
        /// <param name="paraUserID"></param>
        /// <param name="paraObjectName"></param>
        /// <param name="paraSequence"></param>
        /// <returns></returns>
        public List<SP_GetProductListForDiscountMaster_Result> RemoveProductFromTempDataList(string paraSessionID, string paraUserID, int paraSequence, string[] conn)
        {

            /*Begin : Get Existing Records from TempData*/
            List<SP_GetProductListForDiscountMaster_Result> existingDiscountList = new List<SP_GetProductListForDiscountMaster_Result>();
            existingDiscountList = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, conn);
            /*End*/

            /*Get Filter List [Filter By paraSequence]*/
            List<SP_GetProductListForDiscountMaster_Result> filterList = new List<SP_GetProductListForDiscountMaster_Result>();
            filterList = (from exist in existingDiscountList
                          where exist.Sequence != paraSequence
                          select exist).ToList();
            /*End*/

            List<SP_GetProductListForDiscountMaster_Result> result = new List<SP_GetProductListForDiscountMaster_Result>();
            /*Set Sequence*/
            result = SetSequenceList(filterList);

            /*End*/

            /*Save result to TempData*/
            SaveTempDataToDB(result, paraSessionID, paraUserID, conn);
            /*End*/

            return result;
        }

        /// <summary>
        /// 8. Set Sequence No.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        protected List<SP_GetProductListForDiscountMaster_Result> SetSequenceList(List<SP_GetProductListForDiscountMaster_Result> list)
        {
            long setRowNo = 1;
            var setSequence = from rec in list
                              select new SP_GetProductListForDiscountMaster_Result()
                              {
                                  ID = rec.ID,
                                  Sequence = setRowNo++,
                                  ProductID = rec.ProductID,
                                  ProductCode = rec.ProductCode,
                                  ProductName = rec.ProductName,
                                  PrincipalPrice = rec.PrincipalPrice,
                                  UOM = rec.UOM,
                                  DiscountRate = rec.DiscountRate,
                                  IsDiscountPercent = rec.IsDiscountPercent,
                                  MinOrderQuantity = rec.MinOrderQuantity,
                                  AmountAfterDiscount = rec.AmountAfterDiscount,
                                  Active = rec.Active,
                                  CreatedBy = rec.CreatedBy,
                                  CreationDate = rec.CreationDate,
                                  LastModifiedBy = rec.LastModifiedBy,
                                  LastModifiedDate = rec.LastModifiedDate

                              };

            List<SP_GetProductListForDiscountMaster_Result> finalList = new List<SP_GetProductListForDiscountMaster_Result>();
            finalList = setSequence.ToList<SP_GetProductListForDiscountMaster_Result>();
            return finalList;
        }

        /// <summary>
        /// 9. FinalSaveToDBtDiscountMappingDetails
        ///     a. Define ObjectParameter to Passvalues for SQL StoreProcedure
        ///     b. Call : SP_InsertIntoAddToCartProductDetail [ SQL StoreProcedure for BulkInsert ]
        ///     c. Call : ClearTempDataFromDB 
        /// </summary>
        /// <param name="paraSessionID"></param>
        /// <param name="paraObjectName"></param>
        /// <param name="paraDiscountID"></param>
        /// <param name="paraUserID"></param>
        public void FinalSaveToDBtDiscountMappingDetails(string paraSessionID, long paraDiscountID, string paraUserID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetProductListForDiscountMaster_Result> finalSaveLst = new List<SP_GetProductListForDiscountMaster_Result>();
            finalSaveLst = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, conn);

            XElement xmlEle = new XElement("ProductDiscountList", from rec in finalSaveLst
                                                                  select new XElement("ProductDiscount",
                                                            new XElement("ID", rec.ID),
                                                            new XElement("Sequence", rec.Sequence),
                                                            new XElement("ProductID", rec.ProductID),
                                                            new XElement("ProductCode", rec.ProductCode),
                                                            new XElement("ProductName", rec.ProductName),
                                                            new XElement("PrincipalPrice", rec.PrincipalPrice),
                                                            new XElement("UOM", rec.UOM),
                                                            new XElement("DiscountRate", rec.DiscountRate),
                                                            new XElement("IsDiscountPercent", rec.IsDiscountPercent),
                                                            new XElement("MinOrderQuantity", rec.MinOrderQuantity),
                                                            new XElement("AmountAfterDiscount",rec.AmountAfterDiscount),
                                                            new XElement("Active", rec.Active),
                                                            new XElement("CreatedBy", paraUserID),
                                                            new XElement("CreationDate", DateTime.Now),
                                                            new XElement("LastModifiedBy", paraUserID),
                                                            new XElement("LastModifiedDate", DateTime.Now)
                                                            ));

            TempData tempdata = new TempData();
            //tempdata = (from t in db.TempDatas
            //            where t.ObjectName == "Discount" && t.SessionID == paraSessionID && t.UserID == paraUserID
            //            select t).FirstOrDefault();

            tempdata = (db.TempDatas.Where(a => a.ObjectName == "Discount" && a.SessionID == paraSessionID)).FirstOrDefault();
            tempdata.XmlData = xmlEle.ToString();
            tempdata.LastUpdated = DateTime.Now;
            tempdata.SessionID = paraSessionID.ToString();
            tempdata.UserID = paraUserID.ToString();
            tempdata.ObjectName = "Discount";
            tempdata.TableName = "table";
            //db.TempDatas.Attach(tempdata);
            // db.ObjectStateManager.ChangeObjectState(tempdata, EntityState.Modified);
            db.SaveChanges();

            ObjectParameter _paraSessionID = new ObjectParameter("paraSessionID", typeof(string));
            _paraSessionID.Value = paraSessionID;

            //ObjectParameter _paraObjectName = new ObjectParameter("paraObjectName", typeof(string));
            //_paraObjectName.Value = "Discount";

            ObjectParameter _paraDiscountID = new ObjectParameter("paraDiscountID", typeof(long));
            _paraDiscountID.Value = paraDiscountID;

            ObjectParameter _paraUserID = new ObjectParameter("paraUserID", typeof(string));
            _paraUserID.Value = paraUserID;

            ObjectParameter[] obj = new ObjectParameter[] { _paraSessionID, _paraDiscountID, _paraUserID };
            db.ExecuteFunction("SP_InsertIntotDiscountMappingDetails", obj);
            db.SaveChanges();

            ClearTempDataFromDB(paraSessionID, paraUserID, conn);
        }

        public void UpdateRecord(string paraSessionID, string paraUserID, SP_GetProductListForDiscountMaster_Result order, string[] conn)
        {
            List<SP_GetProductListForDiscountMaster_Result> getRec = new List<SP_GetProductListForDiscountMaster_Result>();
            getRec = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, conn);

            SP_GetProductListForDiscountMaster_Result updateRec = new SP_GetProductListForDiscountMaster_Result();
            updateRec = getRec.Where(g => g.Sequence == order.Sequence).FirstOrDefault();

            if (updateRec == null)
            {
                SP_GetProductListForDiscountMaster_Result updateRec1 = new SP_GetProductListForDiscountMaster_Result();
                updateRec1.DiscountRate = order.DiscountRate;
                updateRec1.IsDiscountPercent = order.IsDiscountPercent;
                updateRec1.MinOrderQuantity = order.MinOrderQuantity;
                updateRec1.AmountAfterDiscount = order.AmountAfterDiscount;//add 24Aug
                updateRec1.Active = order.Active;
                SaveTempDataToDB(getRec, paraSessionID, paraUserID, conn);
            }
            else
            {
                updateRec.DiscountRate = order.DiscountRate;
                updateRec.IsDiscountPercent = order.IsDiscountPercent;
                updateRec.MinOrderQuantity = order.MinOrderQuantity;
                updateRec.AmountAfterDiscount = order.AmountAfterDiscount;//add 24Aug
                updateRec.Active = order.Active;
                SaveTempDataToDB(getRec, paraSessionID, paraUserID, conn);
            }
        }
    }

}
