using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Activation;
using System.ServiceModel;
//using ElegantCRM.Model;
using System.Data;
using Domain.Tempdata;
using System.Xml.Linq;
using System.Data.Objects;
using System.Net;
using System.Net.Mail;
using Interface.PowerOnRent;
namespace Domain.PowerOnRent
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public partial class HQGoodsReceipt : iHQGoodsReceipt
    {
        Domain.Server.Server svr = new Server.Server();
        DataHelper datahelper = new DataHelper();

        #region [HQ] Part Receipt Head
        public PORtGRNHead GetReceiptHeadByReceiptID(long ReceiptID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            PORtGRNHead PartReceipt = new PORtGRNHead();
            PartReceipt = db.PORtGRNHeads.Where(g => g.GRNH_ID == ReceiptID).FirstOrDefault();
            db.PORtGRNHeads.Detach(PartReceipt);
            return PartReceipt;
        }

        public long SetIntoGRNHead(PORtGRNHead GRNHead, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            if (GRNHead.GRNH_ID == 0)
            {
                db.PORtGRNHeads.AddObject(GRNHead);
            }
            else
            {
                db.PORtGRNHeads.Attach(GRNHead);
                db.ObjectStateManager.ChangeObjectState(GRNHead, EntityState.Modified);
            }
            db.SaveChanges();

            return GRNHead.GRNH_ID;
        }

        public List<mStatu> GetStatusListForGRN(string Remark, string state, long UserID, string[] conn)
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

        #region [HQ] Parts Details Of Receipt

        public List<POR_SP_GetPartDetails_OfGRN_HQ_Result> GetReceiptPartDetailByReceiptID(long ReceiptID, long SiteID, string sessionID, string userID, string CurrentObject, string IssuedQtySameAs, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<POR_SP_GetPartDetails_OfGRN_HQ_Result> PartDetail = new List<POR_SP_GetPartDetails_OfGRN_HQ_Result>();
            PartDetail = db.POR_SP_GetPartDetails_OfGRN_HQ("0", ReceiptID, 0, SiteID, 0).ToList();
            SaveTempDataToDB(PartDetail, sessionID, userID, CurrentObject, conn);
            return PartDetail;
        }

        public List<POR_SP_GetPartDetails_OfGRN_HQ_Result> GetReceiptPartDetailByIssueID(long IssueID, long SiteID, string sessionID, string userID, string CurrentObject, string IssuedQtySameAs, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<POR_SP_GetPartDetails_OfGRN_HQ_Result> PartDetail = new List<POR_SP_GetPartDetails_OfGRN_HQ_Result>();
            PartDetail = db.POR_SP_GetPartDetails_OfGRN_HQ("0", 0, IssueID, SiteID, 0).ToList();
            SaveTempDataToDB(PartDetail, sessionID, userID, CurrentObject, conn);
            return PartDetail;
        }


        // Add by Suresh
        public List<POR_SP_GetPartDetails_OfMIN_Transfer_Result> GetIssuePartDetailByIssueID_Transfer(long IssueID, string sessionID, string userID, string CurrentObject, string IssuedQtySameAs, long SiteID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<POR_SP_GetPartDetails_OfMIN_Transfer_Result> PartDetail = new List<POR_SP_GetPartDetails_OfMIN_Transfer_Result>();
            PartDetail = db.POR_SP_GetPartDetails_OfMIN_Transfer("0", IssueID, 0, IssuedQtySameAs,SiteID).ToList();
            SaveTempDataToDB(PartDetail, sessionID, userID, CurrentObject, conn);
            return PartDetail;
        }

        protected void SaveTempDataToDB(List<POR_SP_GetPartDetails_OfMIN_Transfer_Result> paraobjList, string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn)
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



        protected void SaveTempDataToDB(List<POR_SP_GetPartDetails_OfGRN_HQ_Result> paraobjList, string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn)
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

        public List<POR_SP_GetPartDetails_OfGRN_HQ_Result> GetExistingTempDataBySessionIDObjectName(string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<POR_SP_GetPartDetails_OfGRN_HQ_Result> objtAddToCartProductDetailList = new List<POR_SP_GetPartDetails_OfGRN_HQ_Result>();

            TempData tempdata = new TempData();
            tempdata = (from temp in db.TempDatas
                        where temp.SessionID == paraSessionID
                        && temp.ObjectName == paraCurrentObjectName
                        && temp.UserID == paraUserID
                        select temp).FirstOrDefault();
            if (tempdata != null)
            {
                objtAddToCartProductDetailList = datahelper.DeserializeEntity1<POR_SP_GetPartDetails_OfGRN_HQ_Result>(tempdata.Data);
            }
            return objtAddToCartProductDetailList;
        }

        public List<POR_SP_GetPartDetails_OfMIN_Transfer_Result> GetExistingTempDataBySessionIDObjectName_Transfer(string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<POR_SP_GetPartDetails_OfMIN_Transfer_Result> objtAddToCartProductDetailList = new List<POR_SP_GetPartDetails_OfMIN_Transfer_Result>();

            TempData tempdata = new TempData();
            tempdata = (from temp in db.TempDatas
                        where temp.SessionID == paraSessionID
                        && temp.ObjectName == paraCurrentObjectName
                        && temp.UserID == paraUserID
                        select temp).FirstOrDefault();
            if (tempdata != null)
            {
                objtAddToCartProductDetailList = datahelper.DeserializeEntity1<POR_SP_GetPartDetails_OfMIN_Transfer_Result>(tempdata.Data);
            }
            return objtAddToCartProductDetailList;
        }


        public List<POR_SP_GetPartDetails_OfGRN_HQ_Result> AddPartIntoReceipt_TempData(string PartIDs, long SiteID, string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            /*Begin : Get Existing Records from TempData*/
            List<POR_SP_GetPartDetails_OfGRN_HQ_Result> existingList = new List<POR_SP_GetPartDetails_OfGRN_HQ_Result>();
            existingList = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, paraCurrentObjectName, conn);
            /*End*/
            long MaxSequenceNo = 0;
            if (existingList.Count > 0)
            {
                MaxSequenceNo = Convert.ToInt64((from lst in existingList
                                                 select lst.Sequence).Max().Value);
            }

            /*Get Product Details*/
            List<POR_SP_GetPartDetails_OfGRN_HQ_Result> getnewRec = new List<POR_SP_GetPartDetails_OfGRN_HQ_Result>();
            getnewRec = (from view in db.POR_SP_GetPartDetails_OfGRN_HQ(PartIDs, 0, 0,SiteID, MaxSequenceNo)
                         orderby view.Sequence
                         select view).ToList();
            /*End*/

            /*Begin : Merge (Existing + Newly Added) Products to Create TempData of AddToCart*/
            List<POR_SP_GetPartDetails_OfGRN_HQ_Result> mergedList = new List<POR_SP_GetPartDetails_OfGRN_HQ_Result>();
            mergedList.AddRange(existingList);
            mergedList.AddRange(getnewRec);
            /*End*/

            /*Begin : Serialize & Save MergedAddToCartList*/
            SaveTempDataToDB(mergedList, paraSessionID, paraUserID, paraCurrentObjectName, conn);
            /*End*/

            return mergedList;
        }

        public List<POR_SP_GetPartDetails_OfGRN_HQ_Result> RemovePartFromReceipt_TempData(string paraSessionID, string paraUserID, string paraCurrentObjectName, int paraSequence, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            /*Begin : Get Existing Records from TempData*/
            List<POR_SP_GetPartDetails_OfGRN_HQ_Result> existingList = new List<POR_SP_GetPartDetails_OfGRN_HQ_Result>();
            existingList = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, paraCurrentObjectName, conn);
            /*End*/

            /*Get Filter List [Filter By paraSequence]*/
            List<POR_SP_GetPartDetails_OfGRN_HQ_Result> filterList = new List<POR_SP_GetPartDetails_OfGRN_HQ_Result>();
            filterList = (from exist in existingList
                          where exist.Sequence != paraSequence
                          select exist).ToList();
            /*End*/

            /*Save result to TempData*/
            SaveTempDataToDB(filterList, paraSessionID, paraUserID, paraCurrentObjectName, conn);
            /*End*/

            return filterList;
        }

        public string[] UpdatePartReceipt_TempData(string SessionID, string CurrentObjectName, string UserID, POR_SP_GetPartDetails_OfGRN_HQ_Result Receipt, string[] conn)
        {
            string[] result;
            result = new string[] { "0", "0" };
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                List<POR_SP_GetPartDetails_OfGRN_HQ_Result> getRec = new List<POR_SP_GetPartDetails_OfGRN_HQ_Result>();
                getRec = GetExistingTempDataBySessionIDObjectName(SessionID, UserID, CurrentObjectName, conn);

                POR_SP_GetPartDetails_OfGRN_HQ_Result updateRec = new POR_SP_GetPartDetails_OfGRN_HQ_Result();
                updateRec = getRec.Where(g => g.Sequence == Receipt.Sequence).FirstOrDefault();

                updateRec.ReceivedQty = Receipt.ReceivedQty;
                updateRec.ExcessQty = 0;
                updateRec.ShortQty = 0;
                if (updateRec.ReceivedQty > updateRec.ChallanQty)
                {
                    updateRec.ExcessQty = updateRec.ReceivedQty - updateRec.ChallanQty;
                }
                else if (updateRec.ReceivedQty < updateRec.ChallanQty)
                {
                    updateRec.ShortQty = updateRec.ChallanQty - updateRec.ReceivedQty;
                }
                result = new string[] { updateRec.ExcessQty.Value.ToString(), updateRec.ShortQty.Value.ToString() };
                SaveTempDataToDB(getRec, SessionID, UserID, CurrentObjectName, conn);
            }
            catch { }
            finally { }
            return result;
        }

        public List<POR_SP_GetPartDetails_OfGRN_HQ_Result> RemovePartFromHQReceipt_TempData(string paraSessionID, string paraUserID, string paraCurrentObjectName, int paraSequence, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            /*Begin : Get Existing Records from TempData*/
            List<POR_SP_GetPartDetails_OfGRN_HQ_Result> existingList = new List<POR_SP_GetPartDetails_OfGRN_HQ_Result>();
            existingList = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, paraCurrentObjectName, conn);
            /*End*/

            /*Get Filter List [Filter By paraSequence]*/
            List<POR_SP_GetPartDetails_OfGRN_HQ_Result> filterList = new List<POR_SP_GetPartDetails_OfGRN_HQ_Result>();
            filterList = (from exist in existingList
                          where exist.Sequence != paraSequence
                          select exist).ToList();
            /*End*/

            /*Save result to TempData*/
            SaveTempDataToDB(filterList, paraSessionID, paraUserID, paraCurrentObjectName, conn);
            /*End*/

            return filterList;
        }

        public void FinalSaveReceiptPartDetail(string paraSessionID, string paraCurrentObjectName, long GRNH_ID, string paraUserID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<POR_SP_GetPartDetails_OfGRN_HQ_Result> finalSaveLst = new List<POR_SP_GetPartDetails_OfGRN_HQ_Result>();
            finalSaveLst = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, paraCurrentObjectName, conn);

            XElement xmlEle = new XElement("Receipt", from rec in finalSaveLst
                                                      select new XElement("PartList",
                                                      new XElement("GRNH_ID", GRNH_ID),
                                                      new XElement("Prod_ID", Convert.ToInt64(rec.ProdID)),
                                                      new XElement("ChallanQty", Convert.ToInt64(rec.ChallanQty)),
                                                      new XElement("ReceivedQty", Convert.ToInt64(rec.ReceivedQty)),
                                                      new XElement("ExcessQty", Convert.ToInt64(rec.ExcessQty)),
                                                      new XElement("ShortQty", Convert.ToInt64(rec.ShortQty)),
                                                      new XElement("Sequence", Convert.ToInt64(rec.Sequence)),
                                                      new XElement("MIND_ID", Convert.ToInt64(0)),
                                                      new XElement("UOMID", Convert.ToInt64(rec.UOMID))));

            ObjectParameter _ReceiptID = new ObjectParameter("ReceiptID", typeof(long));
            _ReceiptID.Value = GRNH_ID;

            ObjectParameter _xmlData = new ObjectParameter("xmlData", typeof(string));
            _xmlData.Value = xmlEle.ToString();


            ObjectParameter[] obj = new ObjectParameter[] { _ReceiptID, _xmlData };
            db.ExecuteFunction("POR_SP_InsertIntoPORtGRNDetail", obj);

            db.SaveChanges();
            ClearTempDataFromDB(paraSessionID, paraUserID, paraCurrentObjectName, conn);
            /*if (result == finalSaveLst.Count)
            {
               
            }*/
        }

        #endregion

        #region [HQ] Receipt Summary

        public List<POR_SP_GetHQReceiptSummaryBySiteIDsOrUserIDOrReceiptIDs_Result> GetReceiptSummaryByUserID(long UserID, string[] conn)
        {
            List<POR_SP_GetHQReceiptSummaryBySiteIDsOrUserIDOrReceiptIDs_Result> ReceiptSummary = new List<POR_SP_GetHQReceiptSummaryBySiteIDsOrUserIDOrReceiptIDs_Result>();
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                ReceiptSummary = db.POR_SP_GetHQReceiptSummaryBySiteIDsOrUserIDOrReceiptIDs("0", UserID, "0").OrderByDescending(o => o.GRNH_ID).ToList();
            }
            catch { }
            finally { }
            return ReceiptSummary;
        }

        public List<POR_SP_GetHQReceiptSummaryBySiteIDsOrUserIDOrReceiptIDs_Result> GetReceiptSummaryBySiteIDs(string SiteIDs, string[] conn)
        {
            List<POR_SP_GetHQReceiptSummaryBySiteIDsOrUserIDOrReceiptIDs_Result> ReceiptSummary = new List<POR_SP_GetHQReceiptSummaryBySiteIDsOrUserIDOrReceiptIDs_Result>();
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                ReceiptSummary = db.POR_SP_GetHQReceiptSummaryBySiteIDsOrUserIDOrReceiptIDs(SiteIDs, 0, "0").OrderByDescending(o => o.GRNH_ID).ToList();
            }
            catch { }
            finally { }
            return ReceiptSummary;
        }

        public List<POR_SP_GetHQReceiptSummaryBySiteIDsOrUserIDOrReceiptIDs_Result> GetReceiptSummaryByReceiptIDs(string ReceiptIDs, string[] conn)
        {
            List<POR_SP_GetHQReceiptSummaryBySiteIDsOrUserIDOrReceiptIDs_Result> ReceiptSummary = new List<POR_SP_GetHQReceiptSummaryBySiteIDsOrUserIDOrReceiptIDs_Result>();
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                ReceiptSummary = db.POR_SP_GetHQReceiptSummaryBySiteIDsOrUserIDOrReceiptIDs("0", 0, ReceiptIDs).OrderByDescending(o => o.GRNH_ID).ToList();
            }
            catch { }
            finally { }
            return ReceiptSummary;
        }

        #endregion

    }
}
