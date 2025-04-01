using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Activation;
using System.ServiceModel;
using System.Data;
using Domain.Tempdata;
using System.Xml.Linq;
using System.Data.Objects;
//using System.Web.Mail;
using System.Net.Mail;
using System.Net;
using Interface.PowerOnRent;
namespace Domain.PowerOnRent
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public partial class PartConsumption : iPartConsumption
    {
        Domain.Server.Server svr = new Server.Server();
        DataHelper datahelper = new DataHelper();

        #region Part Consumption Head
        public PORtConsumptionHead GetConsumptionHeadByConsumptionID(long ConsumptionID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            PORtConsumptionHead PartCons = new PORtConsumptionHead();
            PartCons = db.PORtConsumptionHeads.Where(r => r.ConH_ID == ConsumptionID).FirstOrDefault();
            db.PORtConsumptionHeads.Detach(PartCons);
            return PartCons;
        }

        public long SetIntoPartConsumptionHead(PORtConsumptionHead ConsumptionHead, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            if (ConsumptionHead.ConH_ID == 0)
            {
                db.PORtConsumptionHeads.AddObject(ConsumptionHead);
            }
            else
            {
                db.PORtConsumptionHeads.Attach(ConsumptionHead);
                db.ObjectStateManager.ChangeObjectState(ConsumptionHead, EntityState.Modified);
            }

            if (ConsumptionHead.StatusID == 9 && ConsumptionHead.ReferenceID != 0)
            {
                PORtGRNHead grnhead = new PORtGRNHead();
                grnhead = db.PORtGRNHeads.Where(g => g.GRNH_ID == ConsumptionHead.ReferenceID).FirstOrDefault();
                db.PORtGRNHeads.Detach(grnhead);
                grnhead.StatusID = 9;

                db.PORtGRNHeads.Attach(grnhead);
                db.ObjectStateManager.ChangeObjectState(grnhead, EntityState.Modified);
            }
            db.SaveChanges();

            return ConsumptionHead.ConH_ID;
        }

        public List<mStatu> GetStatusListForConsumption(string Remark, string state, long UserID, string[] conn)
        {
            List<mStatu> statusdetail = new List<mStatu>();
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                string[] RemarkArr = Remark.Split(',');
                if (Remark != "")
                {
                    statusdetail = (from st in db.mStatus
                                    where (st.ObjectName == "MaterialRequest" && RemarkArr.Contains(st.Remark))
                                    select st).OrderBy(st => st.Sequence).ToList();
                }
            }
            catch { }
            finally { }
            return statusdetail;
        }
        #endregion

        #region Consumption Part Detail
        public List<POR_SP_GetPartDetails_OfConsumption_Result> GetConsumptionPartDetailByReceiptID(long ReceiptID, long siteID, string sessionID, string userID, string CurrentObject, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<POR_SP_GetPartDetails_OfConsumption_Result> PartDetail = new List<POR_SP_GetPartDetails_OfConsumption_Result>();
            PartDetail = (from sp in db.POR_SP_GetPartDetails_OfConsumption(ReceiptID, 0, siteID, "0", "0", 0)
                          select sp).ToList();
            SaveTempDataToDB(PartDetail, sessionID, userID, CurrentObject, conn);
            return PartDetail;
        }

        public List<POR_SP_GetPartDetails_OfConsumption_Result> GetConsumptionPartDetailByConsumptionID(long ConsumptionID, long siteID, string sessionID, string userID, string CurrentObject, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<POR_SP_GetPartDetails_OfConsumption_Result> PartDetail = new List<POR_SP_GetPartDetails_OfConsumption_Result>();
            PartDetail = (from sp in db.POR_SP_GetPartDetails_OfConsumption(0, ConsumptionID, siteID, "0", "0", 0)
                          select sp).ToList();
            SaveTempDataToDB(PartDetail, sessionID, userID, CurrentObject, conn);
            return PartDetail;
        }

        protected void SaveTempDataToDB(List<POR_SP_GetPartDetails_OfConsumption_Result> paraobjList, string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            /*Begin : Remove Existing Records*/
            ClearTempDataFromDB(paraSessionID, paraUserID, paraCurrentObjectName, conn);
            /*End*/

            /*Begin : Serialize MergedAddToCartList*/
            string xml = "";
            xml = datahelper.SerializeEntity(paraobjList);
            /*End*/

            /*Begin : Save Serialized List into TempData */
            TempData tempdata = new TempData();
            tempdata.Data = xml;
            tempdata.XmlData = "";
            tempdata.LastUpdated = DateTime.Now;
            tempdata.SessionID = paraSessionID.ToString();
            tempdata.UserID = paraUserID.ToString();
            tempdata.ObjectName = paraCurrentObjectName.ToString();
            tempdata.TableName = "table";
            db.AddToTempDatas(tempdata);
            db.SaveChanges();
            /*End*/

        }

        public void ClearTempDataFromDB(string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            TempData tempdata = new TempData();
            tempdata = (from rec in db.TempDatas
                        where rec.SessionID == paraSessionID
                        && rec.UserID == paraUserID
                        && rec.ObjectName == paraCurrentObjectName
                        select rec).FirstOrDefault();
            if (tempdata != null) { db.DeleteObject(tempdata); db.SaveChanges(); }
        }

        public List<POR_SP_GetPartDetails_OfConsumption_Result> GetExistingTempDataBySessionIDObjectName(string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<POR_SP_GetPartDetails_OfConsumption_Result> objtProductList = new List<POR_SP_GetPartDetails_OfConsumption_Result>();

            TempData tempdata = new TempData();
            tempdata = (from temp in db.TempDatas
                        where temp.SessionID == paraSessionID
                        && temp.ObjectName == paraCurrentObjectName
                        && temp.UserID == paraUserID
                        select temp).FirstOrDefault();
            if (tempdata != null)
            {
                objtProductList = datahelper.DeserializeEntity1<POR_SP_GetPartDetails_OfConsumption_Result>(tempdata.Data);
            }
            return objtProductList;
        }

        public List<POR_SP_GetPartDetails_OfConsumption_Result> AddPartIntoConsumption_TempDataByPartIDs(string paraProductIDs, string paraSessionID, string paraUserID, string paraCurrentObjectName, long SiteID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            /*Begin : Get Existing Records from TempData*/
            List<POR_SP_GetPartDetails_OfConsumption_Result> existingList = new List<POR_SP_GetPartDetails_OfConsumption_Result>();
            existingList = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, paraCurrentObjectName, conn);
            /*End*/

            long MaxSequenceNo = 0;
            if (existingList.Count > 0)
            {
                MaxSequenceNo = Convert.ToInt64((from lst in existingList
                                                 select lst.Sequence).Max().Value);
            }

            /*Get Product Details*/
            List<POR_SP_GetPartDetails_OfConsumption_Result> getnewRec = new List<POR_SP_GetPartDetails_OfConsumption_Result>();
            getnewRec = (from view in db.POR_SP_GetPartDetails_OfConsumption(0, 0, SiteID, "0", paraProductIDs, MaxSequenceNo)
                         orderby view.Sequence
                         select view).ToList();
            /*End*/

            /*Begin : Merge (Existing + Newly Added) Products to Create TempData of AddToCart*/
            List<POR_SP_GetPartDetails_OfConsumption_Result> mergedList = new List<POR_SP_GetPartDetails_OfConsumption_Result>();
            mergedList.AddRange(existingList);
            mergedList.AddRange(getnewRec);
            /*End*/

            /*Begin : Serialize & Save MergedAddToCartList*/
            SaveTempDataToDB(mergedList, paraSessionID, paraUserID, paraCurrentObjectName, conn);
            /*End*/

            return mergedList;
        }

        public List<POR_SP_GetPartDetails_OfConsumption_Result> AddPartIntoConsumption_TempDataByGrdIDs(string GrdIDs, string paraSessionID, string paraUserID, string paraCurrentObjectName, long SiteID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            /*Begin : Get Existing Records from TempData*/
            List<POR_SP_GetPartDetails_OfConsumption_Result> existingList = new List<POR_SP_GetPartDetails_OfConsumption_Result>();
            existingList = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, paraCurrentObjectName, conn);
            /*End*/

            long MaxSequenceNo = 0;
            if (existingList.Count > 0)
            {
                MaxSequenceNo = Convert.ToInt64((from lst in existingList
                                                 select lst.Sequence).Max().Value);
            }

            /*Get Product Details*/
            List<POR_SP_GetPartDetails_OfConsumption_Result> getnewRec = new List<POR_SP_GetPartDetails_OfConsumption_Result>();
            getnewRec = (from view in db.POR_SP_GetPartDetails_OfConsumption(0, 0, SiteID, GrdIDs, "0", MaxSequenceNo)
                         orderby view.Sequence
                         select view).ToList();
            /*End*/

            /*Begin : Merge (Existing + Newly Added) Products to Create TempData of AddToCart*/
            List<POR_SP_GetPartDetails_OfConsumption_Result> mergedList = new List<POR_SP_GetPartDetails_OfConsumption_Result>();
            mergedList.AddRange(existingList);
            mergedList.AddRange(getnewRec);
            /*End*/

            /*Begin : Serialize & Save MergedAddToCartList*/
            SaveTempDataToDB(mergedList, paraSessionID, paraUserID, paraCurrentObjectName, conn);
            /*End*/

            return mergedList;
        }

        public List<POR_SP_GetPartDetails_OfConsumption_Result> RemovePartFrom_TempData(string paraSessionID, string paraUserID, string paraCurrentObjectName, int paraSequence, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            /*Begin : Get Existing Records from TempData*/
            List<POR_SP_GetPartDetails_OfConsumption_Result> existingList = new List<POR_SP_GetPartDetails_OfConsumption_Result>();
            existingList = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, paraCurrentObjectName, conn);
            /*End*/

            /*Get Filter List [Filter By paraSequence]*/
            List<POR_SP_GetPartDetails_OfConsumption_Result> filterList = new List<POR_SP_GetPartDetails_OfConsumption_Result>();
            filterList = (from exist in existingList
                          where exist.Sequence != paraSequence
                          select exist).ToList();
            /*End*/

            /*Save result to TempData*/
            SaveTempDataToDB(filterList, paraSessionID, paraUserID, paraCurrentObjectName, conn);
            /*End*/

            return filterList;
        }

        public void UpdatePartConsumedQty_TempData(string SessionID, string CurrentObjectName, string UserID, POR_SP_GetPartDetails_OfConsumption_Result Consumption, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<POR_SP_GetPartDetails_OfConsumption_Result> getRec = new List<POR_SP_GetPartDetails_OfConsumption_Result>();
            getRec = GetExistingTempDataBySessionIDObjectName(SessionID, UserID, CurrentObjectName, conn);

            POR_SP_GetPartDetails_OfConsumption_Result updateRec = new POR_SP_GetPartDetails_OfConsumption_Result();
            updateRec = getRec.Where(g => g.Sequence == Consumption.Sequence).FirstOrDefault();

            updateRec.ConsumedQty = Consumption.ConsumedQty;
            SaveTempDataToDB(getRec, SessionID, UserID, CurrentObjectName, conn);
        }

        public void FinalSavePartDetail(string paraSessionID, string paraCurrentObjectName, long paraReferenceID, string paraUserID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<POR_SP_GetPartDetails_OfConsumption_Result> finalSaveLst = new List<POR_SP_GetPartDetails_OfConsumption_Result>();
            finalSaveLst = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, paraCurrentObjectName, conn);

            XElement xmlEle = new XElement("Consumed", from rec in finalSaveLst
                                                       select new XElement("PartList",
                                                       new XElement("ConH_ID", paraReferenceID),
                                                       new XElement("Prod_ID", Convert.ToInt64(rec.Prod_ID)),
                                                       new XElement("ConsumedQty", Convert.ToDecimal(rec.ConsumedQty)),
                                                       new XElement("Sequence", Convert.ToInt64(rec.Sequence)),
                                                       new XElement("GRND_ID", rec.GRND_ID)));

            ObjectParameter _ConsumptionID = new ObjectParameter("ConsumptionID", typeof(long));
            _ConsumptionID.Value = paraReferenceID;

            ObjectParameter _xmlData = new ObjectParameter("xmlData", typeof(string));
            _xmlData.Value = xmlEle.ToString();


            ObjectParameter[] obj = new ObjectParameter[] { _ConsumptionID, _xmlData };
            db.ExecuteFunction("POR_SP_InsertIntoPORtConsumptionDetail", obj);

            db.SaveChanges();
            ClearTempDataFromDB(paraSessionID, paraUserID, paraCurrentObjectName, conn);
        }
        #endregion

        #region Consumption Summary
        public List<POR_SP_GetConsumptionSummaryBySiteIDsOrUserIDOrReceiptIDsOrConsumptionIDs_Result> GetConsumptionSummayByUserID(long UserID, string[] conn)
        {
            List<POR_SP_GetConsumptionSummaryBySiteIDsOrUserIDOrReceiptIDsOrConsumptionIDs_Result> ConsumptionList = new List<POR_SP_GetConsumptionSummaryBySiteIDsOrUserIDOrReceiptIDsOrConsumptionIDs_Result>();
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                ConsumptionList = db.POR_SP_GetConsumptionSummaryBySiteIDsOrUserIDOrReceiptIDsOrConsumptionIDs("0", UserID, "0", "0").OrderByDescending(o => o.ConH_ID).ToList();
            }
            catch { }
            finally { }
            return ConsumptionList;
        }

        public List<POR_SP_GetConsumptionSummaryBySiteIDsOrUserIDOrReceiptIDsOrConsumptionIDs_Result> GetConsumptionSummayBySiteIDs(string SiteIDs, string[] conn)
        {
            List<POR_SP_GetConsumptionSummaryBySiteIDsOrUserIDOrReceiptIDsOrConsumptionIDs_Result> Consumption = new List<POR_SP_GetConsumptionSummaryBySiteIDsOrUserIDOrReceiptIDsOrConsumptionIDs_Result>();
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                Consumption = db.POR_SP_GetConsumptionSummaryBySiteIDsOrUserIDOrReceiptIDsOrConsumptionIDs(SiteIDs, 0, "0", "0").OrderByDescending(o => o.ConH_ID).ToList();
            }
            catch { }
            finally { }
            return Consumption;
        }

        public List<POR_SP_GetConsumptionSummaryBySiteIDsOrUserIDOrReceiptIDsOrConsumptionIDs_Result> GetConsumptionSummayByReceiptIDs(string ReceiptIDs, string[] conn)
        {
            List<POR_SP_GetConsumptionSummaryBySiteIDsOrUserIDOrReceiptIDsOrConsumptionIDs_Result> ConsumptionList = new List<POR_SP_GetConsumptionSummaryBySiteIDsOrUserIDOrReceiptIDsOrConsumptionIDs_Result>();
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                ConsumptionList = db.POR_SP_GetConsumptionSummaryBySiteIDsOrUserIDOrReceiptIDsOrConsumptionIDs("0", 0, ReceiptIDs, "0").OrderByDescending(o => o.ConH_ID).ToList();
            }
            catch { }
            finally { }
            return ConsumptionList;
        }

        public List<POR_SP_GetConsumptionSummaryBySiteIDsOrUserIDOrReceiptIDsOrConsumptionIDs_Result> GetConsumptionSummayByConsumptionIDs(string ConsumptionIDs, string[] conn)
        {
            List<POR_SP_GetConsumptionSummaryBySiteIDsOrUserIDOrReceiptIDsOrConsumptionIDs_Result> ConsumptionList = new List<POR_SP_GetConsumptionSummaryBySiteIDsOrUserIDOrReceiptIDsOrConsumptionIDs_Result>();
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                ConsumptionList = db.POR_SP_GetConsumptionSummaryBySiteIDsOrUserIDOrReceiptIDsOrConsumptionIDs("0", 0, "0", ConsumptionIDs).OrderByDescending(o => o.ConH_ID).ToList();
            }
            catch { }
            finally { }
            return ConsumptionList;
        }
        #endregion


    }
}
