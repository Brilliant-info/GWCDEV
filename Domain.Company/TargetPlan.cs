using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Activation;
using System.Configuration;
using Interface.Company;
using System.ServiceModel;
using Domain.Tempdata;
using System.Data.EntityClient;
using System.Data.Objects;
using System.Globalization;
using System.Reflection;
using System.Xml.Linq;
using Domain.Server;

namespace Domain.Company
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]

    public partial class TargetPlan  :Interface.Company.iTargetPlan
    {

         Domain.Server.Server svr = new Server.Server();
        DataHelper datahelper = new DataHelper();

        #region InserttTargetPlan
        public int InserttTargetPlan(tTargetPlan target, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            //db.tDiscountHeads.AddObject(disc); 
            db.AddTotTargetPlans(target);
            db.SaveChanges();
            return Convert.ToInt32(target.ID);
        }
        #endregion

        #region UpdatetTargetPlan
        public int UpdatetTargetPlan(tTargetPlan updatetarget, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            db.tTargetPlans.Attach(updatetarget);
            db.ObjectStateManager.ChangeObjectState(updatetarget, EntityState.Modified);
            db.SaveChanges();
            return 1;
        }
        #endregion
         
        #region GetTargetListByID
        /// <summary>
        /// GetDiscountListByID is providing List of DiscountList By ID for Edit mode
        /// </summary>
        /// <returns></returns>
        /// 
        public tTargetPlan GetTargetListByID(int targetId, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            tTargetPlan TargetID = new tTargetPlan();
            TargetID = (from p in db.tTargetPlans
                          where p.ID == targetId
                          select p).FirstOrDefault();

            db.Detach(TargetID);
            return TargetID;
        }
        #endregion

        //#region checkDuplicateRecord
        ///// <summary>
        ///// checkDuplicateRecord is providing List of Discount by DiscountName 
        ///// </summary>
        ///// <returns></returns>
        ///// 
        //public string checkDuplicateRecord(string disName, string[] conn)
        //{
        //    BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
        //    string result = "";
        //    var output = (from p in db.tDiscountHeads
        //                  where p.Name == disName
        //                  select new { p.Name }).FirstOrDefault();

        //    if (output != null)
        //    {
        //        result = "[ " + disName + " ] Discount Name allready exist";
        //    }
        //    return result;

        //}
        //#endregion

        //#region checkDuplicateRecordEdit
        ///// <summary>
        ///// checkDuplicateRecord for Edit is providing List of Discount by DiscountName and ID
        ///// </summary>
        ///// <returns></returns>
        ///// 
        //public string checkDuplicateRecordEdit(string disName, int disID, string[] conn)
        //{
        //    BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
        //    string result = "";
        //    var output = (from p in db.tDiscountHeads
        //                  where p.Name == disName && p.ID != disID
        //                  select new { p.Name }).FirstOrDefault();

        //    if (output != null)
        //    {
        //        result = "[ " + disName + " ] Discount Name  allready exist";
        //    }
        //    return result;
        //}
        //#endregion

        #region GetTargetRecordToBindGrid
        /// <summary>
        /// GetDiscountRecordToBindGrid is providing List of Discount for bind grid with Actine Yes/No
        /// </summary>
        /// <returns></returns>
        /// 
        public DataSet GetTargetRecordToBindGrid(string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<vTargetPlan> LeadSource = new List<vTargetPlan>();
            XElement xmlTargetMaster = new XElement("TargetList", from m in db.vTargetPlans.AsEnumerable()
                                                                      orderby m.Sequence
                                                                      select new XElement("Target",
                                                                  new XElement("ID", m.ID),
                                                                  new XElement("Name", m.Name),
                                                                  new XElement("Month", m.Month),
                                                                  new XElement("Year", m.Year),
                                                                  new XElement("AlertDate", string.Format("{0:dd-MMM-yyyy}", m.AlertDate)),
                                                                  new XElement("Remark", m.Remark),
                                                                  new XElement("Executive", m.Executive),
                                                                  new XElement("ExecutiveID", m.ExecutiveID),
                                                                  new XElement("FromDate", string.Format("{0:dd-MMM-yyyy}", m.FromDate)),
                                                                  new XElement("ToDate", string.Format("{0:dd-MMM-yyyy}", m.ToDate)),
                                                                  new XElement("Sequence", m.Sequence),
                                                                  new XElement("Active", m.Active == "Y" ? "Yes" : "No")
                                                                  ));
            DataSet ds = new DataSet();
            ds.ReadXml(xmlTargetMaster.CreateReader());
            if (ds.Tables.Count <= 0)
            {
                ds.Tables.Add("Target1");
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
        // public List<SP_GetProductListForTargetPlan_Result> CreateProductTempDataList(long[] paraProductIDs, string paraSessionID, string paraUserID, string paraObjectName)
        public List<SP_GetProductListForTargetPlan_Result> CreateProductTempDataList(long[] paraProductIDs, string paraSessionID, long paraTargetID, string paraUserID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            /*Begin : Get Existing Records from TempData*/
            List<SP_GetProductListForTargetPlan_Result> existingProductList = new List<SP_GetProductListForTargetPlan_Result>();
            existingProductList = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, conn);
            /*End*/

            /*Get Product Details*/
            List<SP_GetProductListForTargetPlan_Result> getnewRec = new List<SP_GetProductListForTargetPlan_Result>();
            getnewRec = (from view in db.SP_GetProductListForTargetPlan(0, paraUserID)
                         where paraProductIDs.Contains(Convert.ToInt32(view.ProductID))
                         select view).ToList();
            /*End*/

            /*Begin : Merge (Existing + Newly Added) Products to Create TempData of AddToCart*/
            List<SP_GetProductListForTargetPlan_Result> mergedtargetList = new List<SP_GetProductListForTargetPlan_Result>();
            mergedtargetList.AddRange(existingProductList);
            mergedtargetList.AddRange(getnewRec);


            List<SP_GetProductListForTargetPlan_Result> finalmergedTargetList = new List<SP_GetProductListForTargetPlan_Result>();
            finalmergedTargetList = SetSequenceList(mergedtargetList);
            /*End*/

            /*Begin : Serialize & Save MergedAddToCartList*/
            SaveTempDataToDB(finalmergedTargetList, paraSessionID, paraUserID, conn);
            /*End*/

            return finalmergedTargetList;
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
        public List<SP_GetProductListForTargetPlan_Result> GetExistingTempDataBySessionIDObjectName(string paraSessionID, string paraUserID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetProductListForTargetPlan_Result> objtTargetGridProductDetailList = new List<SP_GetProductListForTargetPlan_Result>();

            TempData tempdata = new TempData();
            tempdata = (from temp in db.TempDatas
                        where temp.SessionID == paraSessionID
                        && temp.ObjectName == "Target"
                        && temp.UserID == paraUserID
                        select temp).FirstOrDefault();
            if (tempdata != null)
            {
                objtTargetGridProductDetailList = datahelper.DeserializeEntity1<SP_GetProductListForTargetPlan_Result>(tempdata.Data);
            }
            return objtTargetGridProductDetailList;
        }

        /// <summary>
        /// 3. SaveTempDataToDB            
        ///     a.  paraobjList Save to DB [ TempData ]
        /// </summary>
        /// <param name="paraobjList"></param>
        /// <param name="paraSessionID"></param>
        /// <param name="paraUserID"></param>
        /// <param name="paraObjectName"></param>
        protected void SaveTempDataToDB(List<SP_GetProductListForTargetPlan_Result> paraobjList, string paraSessionID, string paraUserID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            TempData tempdata = new TempData();
            TempData tempdata1 = new TempData();
            tempdata = (db.TempDatas.Where(a => a.ObjectName == "Target" && a.SessionID == paraSessionID)).FirstOrDefault();
            tempdata1 = (db.TempDatas.Where(a => a.ObjectName == "Target" && a.SessionID == paraSessionID)).FirstOrDefault();

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
            tempdata.ObjectName = "Target";
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
        public List<SP_GetProductListForTargetPlan_Result> GetTargetListByTargetID(string paraSessionID, long paraTargetID, string paraUserID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            // List<SP_GetProductListForTargetPlan_Result> returndDiscountListByReferenceID = new List<SP_GetProductListForTargetPlan_Result>();
            List<SP_GetProductListForTargetPlan_Result> newRecList = new List<SP_GetProductListForTargetPlan_Result>();

            /*Begin : Fetch AddToCartList from tAddToCartProductDetail by ReferenceID & ObjectName*/
            newRecList = (from addtocart in db.SP_GetProductListForTargetPlan(paraTargetID, paraUserID).AsEnumerable()
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
                        && rec.ObjectName == "Target"
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
        public List<SP_GetProductListForTargetPlan_Result> RemoveProductFromTempDataList(string paraSessionID, string paraUserID, int paraSequence, string[] conn)
        {

            /*Begin : Get Existing Records from TempData*/
            List<SP_GetProductListForTargetPlan_Result> existingTargetList = new List<SP_GetProductListForTargetPlan_Result>();
            existingTargetList = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, conn);
            /*End*/

            /*Get Filter List [Filter By paraSequence]*/
            List<SP_GetProductListForTargetPlan_Result> filterList = new List<SP_GetProductListForTargetPlan_Result>();
            filterList = (from exist in existingTargetList
                          where exist.Sequence != paraSequence
                          select exist).ToList();
            /*End*/

            List<SP_GetProductListForTargetPlan_Result> result = new List<SP_GetProductListForTargetPlan_Result>();
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
        protected List<SP_GetProductListForTargetPlan_Result> SetSequenceList(List<SP_GetProductListForTargetPlan_Result> list)
        {
            long setRowNo = 1;
            var setSequence = from rec in list
                              select new SP_GetProductListForTargetPlan_Result()
                              {
                                  ID = rec.ID,
                                  Sequence = setRowNo++,
                                  ProductID = rec.ProductID,
                                  ProductCode = rec.ProductCode,
                                  ProductName = rec.ProductName,
                                  PrincipalPrice = rec.PrincipalPrice,
                                  UOM = rec.UOM,
                                  Quantity = rec.Quantity,
                                  TargetAmount = rec.TargetAmount,
                                  Active = rec.Active,
                                  CreatedBy = rec.CreatedBy,
                                  CreationDate = rec.CreationDate,
                                  LastModifiedBy = rec.LastModifiedBy,
                                  LastModifiedDate = rec.LastModifiedDate

                              };

            List<SP_GetProductListForTargetPlan_Result> finalList = new List<SP_GetProductListForTargetPlan_Result>();
            finalList = setSequence.ToList<SP_GetProductListForTargetPlan_Result>();
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
        public void FinalSaveToDBtTargetPlanDetails(string paraSessionID, long paraTargetID, string paraUserID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetProductListForTargetPlan_Result> finalSaveLst = new List<SP_GetProductListForTargetPlan_Result>();
            finalSaveLst = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, conn);

            XElement xmlEle = new XElement("ProducttargetList", from rec in finalSaveLst
                                                                select new XElement("Producttarget",
                                                            new XElement("ID", rec.ID),
                                                            new XElement("Sequence", rec.Sequence),
                                                            new XElement("ProductID", rec.ProductID),
                                                            new XElement("ProductCode", rec.ProductCode),
                                                            new XElement("ProductName", rec.ProductName),
                                                            new XElement("PrincipalPrice", rec.PrincipalPrice),
                                                            new XElement("UOM", rec.UOM),
                                                            new XElement("Quantity", rec.Quantity),
                                                            new XElement("TargetAmount", rec.TargetAmount),
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

            tempdata = (db.TempDatas.Where(a => a.ObjectName == "Target" && a.SessionID == paraSessionID)).FirstOrDefault();
            tempdata.XmlData = xmlEle.ToString();
            tempdata.LastUpdated = DateTime.Now;
            tempdata.SessionID = paraSessionID.ToString();
            tempdata.UserID = paraUserID.ToString();
            tempdata.ObjectName = "Target";
            tempdata.TableName = "table";
            //db.TempDatas.Attach(tempdata);
            // db.ObjectStateManager.ChangeObjectState(tempdata, EntityState.Modified);
            db.SaveChanges();

            ObjectParameter _paraSessionID = new ObjectParameter("paraSessionID", typeof(string));
            _paraSessionID.Value = paraSessionID;

            //ObjectParameter _paraObjectName = new ObjectParameter("paraObjectName", typeof(string));
            //_paraObjectName.Value = "Discount";

            ObjectParameter _paratargetID = new ObjectParameter("paraTargetID", typeof(long));
            _paratargetID.Value = paraTargetID;

            ObjectParameter _paraUserID = new ObjectParameter("paraUserID", typeof(string));
            _paraUserID.Value = paraUserID;

            ObjectParameter[] obj = new ObjectParameter[] { _paraSessionID, _paratargetID, _paraUserID };
            db.ExecuteFunction("SP_InsertIntotTargetPlanDetails", obj);
            db.SaveChanges();

            ClearTempDataFromDB(paraSessionID, paraUserID, conn);
        }

        public void UpdateRecord(string paraSessionID, string paraUserID, SP_GetProductListForTargetPlan_Result order, string[] conn)
        {
            List<SP_GetProductListForTargetPlan_Result> getRec = new List<SP_GetProductListForTargetPlan_Result>();
            getRec = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, conn);

            SP_GetProductListForTargetPlan_Result updateRec = new SP_GetProductListForTargetPlan_Result();
            updateRec = getRec.Where(g => g.Sequence == order.Sequence).FirstOrDefault();

            if (updateRec == null)
            {
                SP_GetProductListForTargetPlan_Result updateRec1 = new SP_GetProductListForTargetPlan_Result();
                updateRec1.Quantity = order.Quantity;
                updateRec1.TargetAmount = order.TargetAmount;
                updateRec1.Active = order.Active;
                SaveTempDataToDB(getRec, paraSessionID, paraUserID, conn);
            }
            else
            {
                updateRec.Quantity = order.Quantity;
                updateRec.TargetAmount = order.TargetAmount;
                updateRec.Active = order.Active;
                SaveTempDataToDB(getRec, paraSessionID, paraUserID, conn);
            }
        }

    }

    
}
