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

    public partial class UCTermCondition : Interface.Company.iUCTermCondition
    {
        Domain.Server.Server svr = new Server.Server();
       
        DataHelper datahelper = new DataHelper();

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
        // public List<SP_GetTermConditionListForUCTermCondition_Result> CreateProductTempDataList(long[] paraProductIDs, string paraSessionID, string paraUserID, string paraObjectName)
        public List<SP_GetTermConditionListForUCTermCondition_Result> CreateTermCTempDataList(long[] paraProductIDs, string paraSessionID, long paraReferenceID, string paraUserID, string paraObjectName_Old, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            /*Begin : Get Existing Records from TempData*/
            List<SP_GetTermConditionListForUCTermCondition_Result> existingTermCList = new List<SP_GetTermConditionListForUCTermCondition_Result>();
            existingTermCList = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, paraObjectName_Old,conn);
            /*End*/

            /*Get Product Details*/
            List<SP_GetTermConditionListForUCTermCondition_Result> getnewRec = new List<SP_GetTermConditionListForUCTermCondition_Result>();
            getnewRec = (from view in db.SP_GetTermConditionListForUCTermCondition(0, paraUserID, paraObjectName_Old)
                         where paraProductIDs.Contains(Convert.ToInt32(view.TermID))
                         select view).ToList();
            /*End*/

            /*Begin : Merge (Existing + Newly Added) Products to Create TempData of AddToCart*/
            List<SP_GetTermConditionListForUCTermCondition_Result> mergedTermCList = new List<SP_GetTermConditionListForUCTermCondition_Result>();
            mergedTermCList.AddRange(existingTermCList);
            mergedTermCList.AddRange(getnewRec);


            List<SP_GetTermConditionListForUCTermCondition_Result> finalmergedTermCList = new List<SP_GetTermConditionListForUCTermCondition_Result>();
            finalmergedTermCList = SetSequenceList(mergedTermCList,conn);
            /*End*/

            /*Begin : Serialize & Save MergedAddToCartList*/
            SaveTempDataToDB(finalmergedTermCList, paraSessionID, paraUserID, paraObjectName_Old,conn);
            /*End*/

            return finalmergedTermCList;
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
        public List<SP_GetTermConditionListForUCTermCondition_Result> GetExistingTempDataBySessionIDObjectName(string paraSessionID, string paraUserID, string paraObjectName_Old, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetTermConditionListForUCTermCondition_Result> objtDiscountGridProductDetailList = new List<SP_GetTermConditionListForUCTermCondition_Result>();

            TempData tempdata = new TempData();
            tempdata = (from temp in db.TempDatas
                        where temp.SessionID == paraSessionID
                        && temp.ObjectName == paraObjectName_Old
                        && temp.UserID == paraUserID
                        select temp).FirstOrDefault();
            if (tempdata != null)
            {
                objtDiscountGridProductDetailList = datahelper.DeserializeEntity1<SP_GetTermConditionListForUCTermCondition_Result>(tempdata.Data);
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
        protected void SaveTempDataToDB(List<SP_GetTermConditionListForUCTermCondition_Result> paraobjList, string paraSessionID, string paraUserID, string paraObjectName_Old, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            TempData tempdata = new TempData();
            TempData tempdata1 = new TempData();
            tempdata = (db.TempDatas.Where(a => a.ObjectName == paraObjectName_Old && a.SessionID == paraSessionID)).FirstOrDefault();
            tempdata1 = (db.TempDatas.Where(a => a.ObjectName == paraObjectName_Old && a.SessionID == paraSessionID)).FirstOrDefault();

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
            tempdata.ObjectName = paraObjectName_Old.ToString();
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
        public List<SP_GetTermConditionListForUCTermCondition_Result> GetTermCListByparaReferenceID(string paraSessionID, long paraReferenceID, string paraUserID, string paraObjectName_Old, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            // List<SP_GetTermConditionListForUCTermCondition_Result> returndDiscountListByReferenceID = new List<SP_GetTermConditionListForUCTermCondition_Result>();
            List<SP_GetTermConditionListForUCTermCondition_Result> newRecList = new List<SP_GetTermConditionListForUCTermCondition_Result>();

            /*Begin : Fetch AddToCartList from tAddToCartProductDetail by ReferenceID & ObjectName*/
            newRecList = (from addtocart in db.SP_GetTermConditionListForUCTermCondition(paraReferenceID, paraUserID, paraObjectName_Old).AsEnumerable()
                          //where  addtocart.ID == paraReferenceID
                          //orderby addtocart.Sequence
                          select addtocart).ToList();

            /*End*/

            ///*Begin : Serialize & Save AddToCartList*/
            SaveTempDataToDB(newRecList, paraSessionID, paraUserID, paraObjectName_Old,conn);
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
        public void ClearTempDataFromDB(string paraSessionID, string paraUserID, string paraObjectName_Old, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            TempData tempdata = new TempData();
            tempdata = (from rec in db.TempDatas
                        where rec.SessionID == paraSessionID
                        && rec.UserID == paraUserID
                        && rec.ObjectName == paraObjectName_Old
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
        public List<SP_GetTermConditionListForUCTermCondition_Result> RemoveProductFromTempDataList(string paraSessionID, string paraUserID, int paraSequence, string paraObjectName_Old, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            /*Begin : Get Existing Records from TempData*/
            List<SP_GetTermConditionListForUCTermCondition_Result> existingDiscountList = new List<SP_GetTermConditionListForUCTermCondition_Result>();
            existingDiscountList = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, paraObjectName_Old,conn);
            /*End*/

            /*Get Filter List [Filter By paraSequence]*/
            List<SP_GetTermConditionListForUCTermCondition_Result> filterList = new List<SP_GetTermConditionListForUCTermCondition_Result>();
            filterList = (from exist in existingDiscountList
                          where exist.Sequence != paraSequence
                          select exist).ToList();
            /*End*/

            List<SP_GetTermConditionListForUCTermCondition_Result> result = new List<SP_GetTermConditionListForUCTermCondition_Result>();
            /*Set Sequence*/
            result = SetSequenceList(filterList,conn);

            /*End*/

            /*Save result to TempData*/
            SaveTempDataToDB(result, paraSessionID, paraUserID, paraObjectName_Old,conn);
            /*End*/

            return result;
        }

        /// <summary>
        /// 8. Set Sequence No.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        protected List<SP_GetTermConditionListForUCTermCondition_Result> SetSequenceList(List<SP_GetTermConditionListForUCTermCondition_Result> list, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            long setRowNo = 1;
            var setSequence = from rec in list
                              select new SP_GetTermConditionListForUCTermCondition_Result()
                              {
                                  ID = rec.ID,
                                  Sequence = setRowNo++,
                                  ObjectName =rec.ObjectName,
                                  ReferenceID= rec.ReferenceID,
                                  TermID = rec.TermID,
                                  Term = rec.Term,
                                  Condition = rec.Condition,
                                  Active = rec.Active,
                                  CreatedBy = rec.CreatedBy,
                                  CreatedDate = rec.CreatedDate,
                                  LastModifyBy = rec.LastModifyBy,
                                  LastModifyDate = rec.LastModifyDate,
                                  CompanyID =rec.CompanyID

                              };

            List<SP_GetTermConditionListForUCTermCondition_Result> finalList = new List<SP_GetTermConditionListForUCTermCondition_Result>();
            finalList = setSequence.ToList<SP_GetTermConditionListForUCTermCondition_Result>();
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
        /// <param name="paraReferenceID"></param>
        /// <param name="paraUserID"></param>
        public void FinalSaveToDBtDiscountMappingDetails(string paraSessionID, string paraObjectName_Old, long paraReferenceID, string paraUserID, string paraObjectName_New, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetTermConditionListForUCTermCondition_Result> finalSaveLst = new List<SP_GetTermConditionListForUCTermCondition_Result>();
            finalSaveLst = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, paraObjectName_Old, conn);

            XElement xmlEle = new XElement("TermConditionList", from rec in finalSaveLst
                                                                select new XElement("TermCondition",
                                                            new XElement("ID", rec.ID),
                                                            new XElement("Sequence", rec.Sequence),
                                                            new XElement("ObjectName", paraObjectName_New),
                                                            new XElement("ReferenceID", paraReferenceID),
                                                            new XElement("TermID", rec.TermID),
                                                            new XElement("Term", rec.Term),
                                                            new XElement("Condition", rec.Condition), 
                                                            new XElement("Active", rec.Active),
                                                            new XElement("CreatedBy", paraUserID),
                                                            new XElement("CreationDate", rec.CreatedDate),
                                                            new XElement("LastModifiedBy", paraUserID),
                                                            new XElement("LastModifiedDate", rec.LastModifyDate),
                                                            new XElement("CompanyID", rec.CompanyID)
                                                            ));

            TempData tempdata = new TempData();
            //tempdata = (from t in db.TempDatas
            //            where t.ObjectName == "Discount" && t.SessionID == paraSessionID && t.UserID == paraUserID
            //            select t).FirstOrDefault();

            tempdata = (db.TempDatas.Where(a => a.ObjectName == paraObjectName_Old && a.SessionID == paraSessionID)).FirstOrDefault();
            tempdata.XmlData = xmlEle.ToString();
            tempdata.LastUpdated = DateTime.Now;
            tempdata.SessionID = paraSessionID.ToString();
            tempdata.UserID = paraUserID.ToString();
            tempdata.ObjectName = paraObjectName_Old;
            tempdata.TableName = "table";
            //db.TempDatas.Attach(tempdata);
            // db.ObjectStateManager.ChangeObjectState(tempdata, EntityState.Modified);
            db.SaveChanges();

            ObjectParameter _paraSessionID = new ObjectParameter("paraSessionID", typeof(string));
            _paraSessionID.Value = paraSessionID;

            ObjectParameter _paraObjectName_Old = new ObjectParameter("paraObjectName_Old", typeof(string));
            _paraObjectName_Old.Value = paraObjectName_Old;

            ObjectParameter _paraObjectName_New = new ObjectParameter("paraObjectName_New", typeof(string));
            _paraObjectName_New.Value = paraObjectName_New;

            ObjectParameter _paraReferenceID = new ObjectParameter("paraReferenceID", typeof(long));
            _paraReferenceID.Value = paraReferenceID;

            ObjectParameter _paraUserID = new ObjectParameter("paraUserID", typeof(string));
            _paraUserID.Value = paraUserID;

            ObjectParameter[] obj = new ObjectParameter[] { _paraSessionID, _paraObjectName_Old, _paraReferenceID, _paraUserID, _paraObjectName_New };
            db.ExecuteFunction("SP_InsertIntotTermsConditionsDetail", obj);
            db.SaveChanges();

            ClearTempDataFromDB(paraSessionID, paraUserID, paraObjectName_Old,conn);
        }

        public void UpdateRecord(string paraSessionID, string paraUserID, SP_GetTermConditionListForUCTermCondition_Result order, string paraObjectName_Old, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetTermConditionListForUCTermCondition_Result> getRec = new List<SP_GetTermConditionListForUCTermCondition_Result>();
            getRec = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, paraObjectName_Old,conn);

            SP_GetTermConditionListForUCTermCondition_Result updateRec = new SP_GetTermConditionListForUCTermCondition_Result();
            updateRec = getRec.Where(g => g.Sequence == order.Sequence).FirstOrDefault();

            updateRec.Term = order.Term;
            updateRec.Condition = order.Condition;
            updateRec.Active = order.Active;
            SaveTempDataToDB(getRec, paraSessionID, paraUserID, paraObjectName_Old,conn);
           
        }
    }
}
