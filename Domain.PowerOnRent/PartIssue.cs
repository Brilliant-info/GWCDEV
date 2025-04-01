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
    public partial class PartIssue : iPartIssue
    {
        Domain.Server.Server svr = new Server.Server();
        DataHelper datahelper = new DataHelper();

        #region Part Issue Head
        public PORtMINHead GetIssueHeadByIssueID(long IssueID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            PORtMINHead PartIssue = new PORtMINHead();
            PartIssue = db.PORtMINHeads.Where(r => r.MINH_ID == IssueID).FirstOrDefault();
            db.PORtMINHeads.Detach(PartIssue);
            return PartIssue;
        }

        /// <summary>
        /// Get status of Material Issue when record save
        /// -When All part issue against Request then Status is Fully Issued [7]
        /// -When Partial part issue against Request then Status is Partial Issued [6]
        /// </summary>
        /// <param name="SessionID"></param>
        /// <param name="UserID"></param>
        /// <param name="ObjectName"></param>
        /// <param name="RequestID"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public long GetStatusOfIssueHead(string SessionID, string UserID, string ObjectName, long RequestID, string[] conn)
        {
            long StatusID = 0;
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            try
            {
                List<POR_SP_GetPartDetails_OfMIN_Result> CurrentPartList = new List<POR_SP_GetPartDetails_OfMIN_Result>();
                CurrentPartList = GetExistingTempDataBySessionIDObjectName(SessionID, UserID, ObjectName, conn).ToList();
                var CurrentPRD_ID = (from c in CurrentPartList
                                     select c.PRD_ID);
                List<PORtPartRequestDetail> DBRequestPartList = new List<PORtPartRequestDetail>();
                DBRequestPartList = db.PORtPartRequestDetails.Where(rd => rd.PRH_ID == RequestID).ToList();

                var PendingPartList = DBRequestPartList.Where(dp => !CurrentPRD_ID.Contains(dp.PRD_ID)).ToList();

                if (PendingPartList.Count == 0)
                {
                    PendingPartList = (from d in DBRequestPartList
                                       from c in CurrentPartList
                                       where d.PRD_ID == c.PRD_ID && d.RemaningQty != c.IssuedQty
                                       select d).ToList();
                }
                else  //add by suresh
                {
                     for(int k=0;k<=PendingPartList.Count;k++)
                    {
                        PendingPartList = (from d in DBRequestPartList
                                           from p in PendingPartList
                                           where d.PRD_ID ==p.PRD_ID && d.RemaningQty !=0
                                           select d).ToList();
                    }
                                         
                }

                if (PendingPartList.Count > 0)
                {
                    StatusID = 6;
                }
                else if (PendingPartList.Count == 0)
                {
                    StatusID = 7;
                }
            }
            catch { }
            finally { }
            return StatusID;
        }

        public long SetIntoMINHead(PORtMINHead MINHead, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            if (MINHead.MINH_ID == 0)
            {
                db.PORtMINHeads.AddObject(MINHead);
            }
            else
            {
                db.PORtMINHeads.Attach(MINHead);
                db.ObjectStateManager.ChangeObjectState(MINHead, EntityState.Modified);
            }
            db.SaveChanges();
            if (MINHead.StatusID != 1 && MINHead.StatusID != 10)
            {
                /*Update Request Status*/
                PORtPartRequestHead RequestHead = new PORtPartRequestHead();
                RequestHead = db.PORtPartRequestHeads.Where(r => r.PRH_ID == MINHead.PRH_ID).FirstOrDefault();
                db.PORtPartRequestHeads.Detach(RequestHead);
                RequestHead.StatusID = MINHead.StatusID;
                db.PORtPartRequestHeads.Attach(RequestHead);
                db.ObjectStateManager.ChangeObjectState(RequestHead, EntityState.Modified);
                db.SaveChanges();


                /*Insert into ReceiptHead & ReceiptPartDetails*/
                PORtGRNHead ReceiptHead = new PORtGRNHead();
                ReceiptHead.SiteID = MINHead.SiteID;
                ReceiptHead.ObjectName = "MaterialIssue";
                ReceiptHead.ReferenceID = MINHead.MINH_ID;
                ReceiptHead.GRN_No = "N/A";
                ReceiptHead.ReceivedByUserID = 0;
                ReceiptHead.StatusID = 1;
                ReceiptHead.IsSubmit = false;
                ReceiptHead.CreatedBy = MINHead.CreatedBy;
                ReceiptHead.CreationDt = DateTime.Now;
                db.PORtGRNHeads.AddObject(ReceiptHead);
                db.SaveChanges();

            }
            return MINHead.MINH_ID;
        }

        public List<mStatu> GetStatusListForIssue(string Remark, string state, long UserID, string[] conn)
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
                else
                {
                    statusdetail = (from st in db.mStatus
                                    where (st.ObjectName == "MaterialRequest")
                                    select st).OrderBy(st => st.Sequence).ToList();
                }

            }
            catch { }
            finally { }
            return statusdetail;
        }

        public bool CheckPendingIssueListToDecideAddNewAccess(long RequestID, string[] conn)
        {
            bool result = false;
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                List<PORtPartRequestDetail> reqPartList = new List<PORtPartRequestDetail>();
                reqPartList = db.PORtPartRequestDetails.Where(r => r.PRH_ID == RequestID && r.RemaningQty > 0).ToList();
                if (reqPartList.Count > 0)
                {
                    result = true;
                }
            }
            catch { }
            return result;
        }
        #endregion

        #region Part Issue Detail
        public List<POR_SP_GetPartDetails_OfMIN_Result> GetIssuePartDetailByRequestID(long RequestID, string sessionID, string userID, string CurrentObject, string IssuedQtySameAs, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<POR_SP_GetPartDetails_OfMIN_Result> PartDetail = new List<POR_SP_GetPartDetails_OfMIN_Result>();
            PartDetail = db.POR_SP_GetPartDetails_OfMIN("0", 0, RequestID, IssuedQtySameAs).ToList();
            SaveTempDataToDB(PartDetail, sessionID, userID, CurrentObject, conn);
            return PartDetail;
        }

        public List<POR_SP_GetPartDetails_OfMIN_Result> GetIssuePartDetailByIssueID(long IssueID, string sessionID, string userID, string CurrentObject, string IssuedQtySameAs, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<POR_SP_GetPartDetails_OfMIN_Result> PartDetail = new List<POR_SP_GetPartDetails_OfMIN_Result>();
            PartDetail = db.POR_SP_GetPartDetails_OfMIN("0", IssueID, 0, IssuedQtySameAs).ToList();
            SaveTempDataToDB(PartDetail, sessionID, userID, CurrentObject, conn);
            return PartDetail;
        }

        protected void SaveTempDataToDB(List<POR_SP_GetPartDetails_OfMIN_Result> paraobjList, string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn)
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

        public List<POR_SP_GetPartDetails_OfMIN_Result> GetExistingTempDataBySessionIDObjectName(string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<POR_SP_GetPartDetails_OfMIN_Result> objtAddToCartProductDetailList = new List<POR_SP_GetPartDetails_OfMIN_Result>();

            TempData tempdata = new TempData();
            tempdata = (from temp in db.TempDatas
                        where temp.SessionID == paraSessionID
                        && temp.ObjectName == paraCurrentObjectName
                        && temp.UserID == paraUserID
                        select temp).FirstOrDefault();
            if (tempdata != null)
            {
                objtAddToCartProductDetailList = datahelper.DeserializeEntity1<POR_SP_GetPartDetails_OfMIN_Result>(tempdata.Data);
            }
            return objtAddToCartProductDetailList;
        }

        //add by suresh
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
        //add by suresh


        public List<POR_SP_GetPartDetails_OfMIN_Result> AddPartIntoIssue_TempData(string PRD_IDs, long IssueID, long RequestID, string IssuedQtySameAs, string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            /*Begin : Get Existing Records from TempData*/
            List<POR_SP_GetPartDetails_OfMIN_Result> existingList = new List<POR_SP_GetPartDetails_OfMIN_Result>();
            existingList = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, paraCurrentObjectName, conn);
            /*End*/

            /*Get Product Details*/
            List<POR_SP_GetPartDetails_OfMIN_Result> getnewRec = new List<POR_SP_GetPartDetails_OfMIN_Result>();
            getnewRec = (from view in db.POR_SP_GetPartDetails_OfMIN(PRD_IDs, IssueID, RequestID, IssuedQtySameAs)
                         orderby view.Sequence
                         select view).ToList();
            /*End*/

            /*Begin : Merge (Existing + Newly Added) Products to Create TempData of AddToCart*/
            List<POR_SP_GetPartDetails_OfMIN_Result> mergedList = new List<POR_SP_GetPartDetails_OfMIN_Result>();
            mergedList.AddRange(existingList);
            mergedList.AddRange(getnewRec);
            /*End*/

            /*Begin : Serialize & Save MergedAddToCartList*/
            SaveTempDataToDB(mergedList, paraSessionID, paraUserID, paraCurrentObjectName, conn);
            /*End*/

            return mergedList;
        }

        public List<POR_SP_GetPartDetails_OfMIN_Result> RemovePartFromIssue_TempData(string paraSessionID, string paraUserID, string paraCurrentObjectName, int paraSequence, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            /*Begin : Get Existing Records from TempData*/
            List<POR_SP_GetPartDetails_OfMIN_Result> existingList = new List<POR_SP_GetPartDetails_OfMIN_Result>();
            existingList = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, paraCurrentObjectName, conn);
            /*End*/

            /*Get Filter List [Filter By paraSequence]*/
            List<POR_SP_GetPartDetails_OfMIN_Result> filterList = new List<POR_SP_GetPartDetails_OfMIN_Result>();
            filterList = (from exist in existingList
                          where exist.Sequence != paraSequence
                          select exist).ToList();
            /*End*/

            /*Save result to TempData*/
            SaveTempDataToDB(filterList, paraSessionID, paraUserID, paraCurrentObjectName, conn);
            /*End*/

            return filterList;
        }

        public string UpdatePartIssue_TempData(string SessionID, string CurrentObjectName, string UserID, POR_SP_GetPartDetails_OfMIN_Result Issue, string[] conn)
        {
            string RemainingQty = "0";           
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                List<POR_SP_GetPartDetails_OfMIN_Result> getRec = new List<POR_SP_GetPartDetails_OfMIN_Result>();
                getRec = GetExistingTempDataBySessionIDObjectName(SessionID, UserID, CurrentObjectName, conn);

                POR_SP_GetPartDetails_OfMIN_Result updateRec = new POR_SP_GetPartDetails_OfMIN_Result();
                updateRec = getRec.Where(g => g.Sequence == Issue.Sequence).FirstOrDefault();

                updateRec.IssuedQty = Convert.ToDecimal(Issue.IssuedQty);
                RemainingQty = (updateRec.RequestQty - updateRec.IssuedQty).ToString();
                updateRec.RemaningQty = Convert.ToDecimal(RemainingQty);
                //add by suresh
                //HQstock = (updateRec.CurrentStockHQ - updateRec.IssuedQty).ToString();
                //updateRec.CurrentStockHQ = Convert.ToDecimal(HQstock);

                SaveTempDataToDB(getRec, SessionID, UserID, CurrentObjectName, conn);
            }
            catch { }
            finally { }
            return RemainingQty;
        }

        //add by suresh
        public string UpdateHQStock_TempData(string SessionID, string CurrentObjectName, string UserID, POR_SP_GetPartDetails_OfMIN_Result Issue, string[] conn)
        {
             string HQstock = "0";
             string RemainingQty = "0";          
             try
             {
                 BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                 List<POR_SP_GetPartDetails_OfMIN_Result> getRec = new List<POR_SP_GetPartDetails_OfMIN_Result>();
                 getRec = GetExistingTempDataBySessionIDObjectName(SessionID, UserID, CurrentObjectName, conn);

                 POR_SP_GetPartDetails_OfMIN_Result updateRec = new POR_SP_GetPartDetails_OfMIN_Result();
                 updateRec = getRec.Where(g => g.Sequence == Issue.Sequence).FirstOrDefault();

                 updateRec.IssuedQty = Convert.ToDecimal(Issue.IssuedQty);
                 RemainingQty = (updateRec.RequestQty - updateRec.IssuedQty).ToString();
                 updateRec.RemaningQty = Convert.ToDecimal(RemainingQty);
                 HQstock = (updateRec.CurrentStockHQ - updateRec.IssuedQty).ToString();
                 updateRec.CurrentStockHQ = Convert.ToDecimal(HQstock);

                 SaveTempDataToDB(getRec, SessionID, UserID, CurrentObjectName, conn);
             }
             catch { }
             finally { }
             return HQstock;
        }

        public void FinalSaveIssuePartDetail(string paraSessionID, string paraCurrentObjectName, long MINH_ID, long PRH_ID, string paraUserID, string IssueStatus, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<POR_SP_GetPartDetails_OfMIN_Result> finalSaveLst = new List<POR_SP_GetPartDetails_OfMIN_Result>();
            finalSaveLst = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, paraCurrentObjectName, conn);

            XElement xmlEle = new XElement("Issue", from rec in finalSaveLst
                                                    select new XElement("PartList",
                                                    new XElement("MINH_ID", MINH_ID),
                                                    new XElement("PRD_ID", Convert.ToInt64(rec.PRD_ID)),
                                                    new XElement("Prod_ID", Convert.ToInt64(rec.Prod_ID)),
                                                    new XElement("Prod_Name", rec.Prod_Name),
                                                    new XElement("Prod_Description", rec.Prod_Description),
                                                    new XElement("IssuedQty", Convert.ToDecimal(rec.IssuedQty)),
                                                    new XElement("Sequence", Convert.ToInt64(rec.Sequence)),
                                                    new XElement("UOMID", Convert.ToInt64(rec.UOMID)),
                                                    new XElement("Prod_Code", rec.Prod_Code)));

            ObjectParameter _IssueID = new ObjectParameter("IssueID", typeof(long));
            _IssueID.Value = MINH_ID;

            ObjectParameter _xmlData = new ObjectParameter("xmlData", typeof(string));
            _xmlData.Value = xmlEle.ToString();


            ObjectParameter[] obj = new ObjectParameter[] { _IssueID, _xmlData };
            db.ExecuteFunction("POR_SP_InsertIntoPORtMINDetail", obj);
            db.SaveChanges();

            /*Insert into GRNPartDetail if status is Issue*/
            if (IssueStatus == "Issue")
            {
                List<PORtMINDetail> PartList = new List<PORtMINDetail>();
                PartList = db.PORtMINDetails.Where(r => r.MINH_ID == MINH_ID).ToList();
                PORtGRNHead ReceiptHead = new PORtGRNHead();
                ReceiptHead = db.PORtGRNHeads.Where(r => r.ReferenceID == MINH_ID && r.ObjectName == "MaterialIssue").FirstOrDefault();

                foreach (PORtMINDetail part in PartList)
                {
                    PORtGRNDetail ReceiptPart = new PORtGRNDetail();
                    ReceiptPart.GRNH_ID = ReceiptHead.GRNH_ID;
                    ReceiptPart.Prod_ID = part.Prod_ID;
                    ReceiptPart.ChallanQty = part.IssuedQty;
                    ReceiptPart.ReceivedQty = part.IssuedQty;
                    ReceiptPart.ShortQty = 0;
                    ReceiptPart.ExcessQty = 0;
                    ReceiptPart.ReceivedQty = part.IssuedQty;
                    ReceiptPart.Sequence = part.Sequence;
                    ReceiptPart.MIND_ID = part.MIND_ID;
                    ReceiptPart.UOMID = part.UOMID;
                    db.PORtGRNDetails.AddObject(ReceiptPart);
                    db.SaveChanges();
                }
                //add by suresh
                foreach (PORtMINDetail p in PartList)
                {
                    tProductStockDetail psd = (from s in db.tProductStockDetails
                                               where s.SiteID == 1 && s.ProdID == p.Prod_ID
                                               select s).SingleOrDefault();
                    psd.AvailableBalance = psd.AvailableBalance - p.IssuedQty;
                   // db.tProductStockDetails.AddObject(psd);
                    db.SaveChanges();
                }

                EmailSendWhenMaterialIssued(PRH_ID, MINH_ID, finalSaveLst, conn);
            }
            ClearTempDataFromDB(paraSessionID, paraUserID, paraCurrentObjectName, conn);
            /*if (result == finalSaveLst.Count)
            {
               
            }*/
        }
        #endregion

        #region Display Pending Issue Part List
        public List<POR_SP_GetPartDetails_OfMIN_Result> GetPendingIssuePartList(string SessionID, string UserID, string ObjectName, long RequestID, string[] conn)
        {
            List<POR_SP_GetPartDetails_OfMIN_Result> PendingPartList = new List<POR_SP_GetPartDetails_OfMIN_Result>();
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            try
            {
                List<POR_SP_GetPartDetails_OfMIN_Result> CurrentPartList = new List<POR_SP_GetPartDetails_OfMIN_Result>();
                CurrentPartList = GetExistingTempDataBySessionIDObjectName(SessionID, UserID, ObjectName, conn).ToList();
                var CurrentPRD_ID = (from c in CurrentPartList
                                     select c.PRD_ID);
                List<POR_SP_GetPartDetails_OfMIN_Result> DBRequestPartList = new List<POR_SP_GetPartDetails_OfMIN_Result>();
                DBRequestPartList = db.POR_SP_GetPartDetails_OfMIN("0", 0, RequestID, "true").ToList();

                PendingPartList = (from d in DBRequestPartList
                                   where !CurrentPRD_ID.Contains(d.PRD_ID)
                                   select d).ToList();
            }
            catch { }
            finally { }
            return PendingPartList;
        }
        #endregion

        #region Issue Summary
      /*  public List<POR_SP_GetIssueSummaryBySiteIDsOrUserIDOrRequestIDOrIssueIDs_Result> GetIssueSummayByUserID(long UserID, string[] conn)
        {
            List<POR_SP_GetIssueSummaryBySiteIDsOrUserIDOrRequestIDOrIssueIDs_Result> IssueSummary = new List<POR_SP_GetIssueSummaryBySiteIDsOrUserIDOrRequestIDOrIssueIDs_Result>();
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                IssueSummary = db.POR_SP_GetIssueSummaryBySiteIDsOrUserIDOrRequestIDOrIssueIDs("0", UserID, 0, "0").OrderByDescending(i => i.MINH_ID).ToList();
            }
            catch { }
            finally { }
            return IssueSummary;
        }

        public List<POR_SP_GetIssueSummaryBySiteIDsOrUserIDOrRequestIDOrIssueIDs_Result> GetIssueSummayBySiteIDs(string SiteIDs, string[] conn)
        {
            List<POR_SP_GetIssueSummaryBySiteIDsOrUserIDOrRequestIDOrIssueIDs_Result> IssueSummary = new List<POR_SP_GetIssueSummaryBySiteIDsOrUserIDOrRequestIDOrIssueIDs_Result>();
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                IssueSummary = db.POR_SP_GetIssueSummaryBySiteIDsOrUserIDOrRequestIDOrIssueIDs(SiteIDs, 0, 0, "0").OrderByDescending(i => i.MINH_ID).ToList();
            }
            catch { }
            finally { }
            return IssueSummary;
        }

        public List<POR_SP_GetIssueSummaryBySiteIDsOrUserIDOrRequestIDOrIssueIDs_Result> GetIssueSummayByRequestID(long RequestID, string[] conn)
        {
            List<POR_SP_GetIssueSummaryBySiteIDsOrUserIDOrRequestIDOrIssueIDs_Result> IssueSummary = new List<POR_SP_GetIssueSummaryBySiteIDsOrUserIDOrRequestIDOrIssueIDs_Result>();
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                IssueSummary = db.POR_SP_GetIssueSummaryBySiteIDsOrUserIDOrRequestIDOrIssueIDs("0", 0, RequestID, "0").OrderByDescending(i => i.MINH_ID).ToList();
            }
            catch { }
            finally { }
            return IssueSummary;
        }

        public List<POR_SP_GetIssueSummaryBySiteIDsOrUserIDOrRequestIDOrIssueIDs_Result> GetIssueSummayByIssueIDs(string IssueIDs, string[] conn)
        {
            List<POR_SP_GetIssueSummaryBySiteIDsOrUserIDOrRequestIDOrIssueIDs_Result> IssueSummary = new List<POR_SP_GetIssueSummaryBySiteIDsOrUserIDOrRequestIDOrIssueIDs_Result>();
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                IssueSummary = db.POR_SP_GetIssueSummaryBySiteIDsOrUserIDOrRequestIDOrIssueIDs("0", 0, 0, IssueIDs).OrderByDescending(i => i.MINH_ID).ToList();
            }
            catch { }
            finally { }
            return IssueSummary;
        }*/
        #endregion

        #region Issue Mail
        public void SendMail(string MailBody, string MailSubject, string ToEmailIDs)
        {
            try
            {
                SmtpClient smtpClient = new SmtpClient("10.228.134.54", 25);
                MailMessage message = new MailMessage();
                //  SmtpClient smtpClient = new SmtpClient("smtpout.asia.secureserver.net", 25);
                //  MailMessage message = new MailMessage();
                // MailAddress fromAddress = new MailAddress("admin@elegantcrm.com", "Part Request System");
                MailAddress fromAddress = new MailAddress("OMSTest@gulfwarehousing.com", "GWC");
                //From address will be given as a MailAddress Object
                message.From = fromAddress;
                // To address collection of MailAddress
                message.To.Add(ToEmailIDs);
                message.Subject = MailSubject;
                //Body can be Html or text format
                //Specify true if it  is html message
                message.IsBodyHtml = true;
                // Message body content
                message.Body = MailBody;
                smtpClient.EnableSsl = false;
                // Send SMTP mail
                smtpClient.UseDefaultCredentials = false;
                // NetworkCredential basicCredential = new NetworkCredential("admin@elegantcrm.com", "fortis11");
                NetworkCredential basicCredential = new NetworkCredential("OMSTest@gulfwarehousing.com", "");
                smtpClient.Credentials = basicCredential;

                smtpClient.Send(message);
            }
            catch { }
        }

        protected string EMailGetRequestDetail(POR_SP_GetRequestByRequestIDs_Result Request)
        {
            string result = "";
            result = "Request No. : <b>" + Request.PRH_ID.ToString() +
                     "</b><br/>" +
                     "Request Date : <b>" + Request.RequestDate.Value.ToString("dd-MMM-yyyy") +
                     "</b><br/>" +
                     "Status : <b>" + Request.RequestStatus +
                     "</b><br/>" +
                     "Site / Warehouse : <b>" + Request.SiteName +
                     "</b><br/>" +
                     "Request Type : <b>" + Request.RequestType +
                     "</b><br/>" +
                     "Requested By : <b>" + Request.RequestByUserName + "</b>";

            return result;
        }

        protected string EMailGetIssuePratDetail(List<POR_SP_GetPartDetails_OfMIN_Result> PartList1, string[] conn)
        {
            string result = "";
            try
            {
                List<POR_SP_GetPartDetails_OfMIN_Result> htmlList = new List<POR_SP_GetPartDetails_OfMIN_Result>();
                htmlList = PartList1;
                /*if (IssueID != 0)
                {
                    BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                    htmlList = db.POR_SP_GetPartDetails_OfMIN("0", IssueID, 0, "true").ToList();
                }*/

                int srno = 0;
                XElement xmlEle = new XElement("table", from rec in htmlList
                                                        select new XElement("tr",
                                                        new XElement("td", (srno = srno + 1) + "."),
                                                        new XElement("td", rec.Prod_Code),
                                                        new XElement("td", rec.Prod_Name),
                                                        new XElement("td", rec.Prod_Description),
                                                        new XElement("td1", Convert.ToDecimal(rec.IssuedQty).ToString("0.00")),
                                                        new XElement("td", rec.UOM),
                                                        new XElement("td1", rec.RemaningQty)));



                string tblHeader = "<br /><table cellpadding='2' cellspacing='5' style='text-align: left; font-family: Tahoma; font-size: 12px;'>";
                tblHeader = tblHeader + "<tr><td colspan='7'><b>Issue Part Details : </b></td></tr>" +
                                        "<tr>" +
                                        "<td style='font-weight: bold; text-align: center; border-bottom: solid 1px gray; border-top: solid 1px gray;'>Sr.No.</td>" +
                                        "<td style='font-weight: bold; text-align: center; border-bottom: solid 1px gray; border-top: solid 1px gray;'>Part Code</td>" +
                                        "<td style='font-weight: bold; text-align: center; border-bottom: solid 1px gray; border-top: solid 1px gray;'>Part Name</td>" +
                                        "<td style='font-weight: bold; text-align: center; border-bottom: solid 1px gray; border-top: solid 1px gray;'>Part Description</td>" +
                                        "<td style='font-weight: bold; text-align: center; border-bottom: solid 1px gray; border-top: solid 1px gray;'>Issued Qty</td>" +
                                        "<td style='font-weight: bold; text-align: center; border-bottom: solid 1px gray; border-top: solid 1px gray;'>UOM</td>" +
                                        "<td style='font-weight: bold; text-align: center; border-bottom: solid 1px gray; border-top: solid 1px gray;'>Still <br />Pending Issue</td></tr>";

                result = result + xmlEle.ToString().Replace("<table>", tblHeader).Replace("<td1>", "<td style='text-align: right;'>").Replace("</td1>", "</td>");


            }
            catch { }
            return result;
        }

        // add bu suresh
        protected string EMailGetIssuePratDetail(List<POR_SP_GetPartDetails_OfMIN_Transfer_Result> PartList1, string[] conn)
        {
            string result = "";
            try
            {
                List<POR_SP_GetPartDetails_OfMIN_Transfer_Result> htmlList = new List<POR_SP_GetPartDetails_OfMIN_Transfer_Result>();
                htmlList = PartList1;
                /*if (IssueID != 0)
                {
                    BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                    htmlList = db.POR_SP_GetPartDetails_OfMIN("0", IssueID, 0, "true").ToList();
                }*/

                int srno = 0;
                XElement xmlEle = new XElement("table", from rec in htmlList
                                                        select new XElement("tr",
                                                        new XElement("td", (srno = srno + 1) + "."),
                                                        new XElement("td", rec.Prod_Code),
                                                        new XElement("td", rec.Prod_Name),
                                                        new XElement("td", rec.Prod_Description),
                                                        new XElement("td1", Convert.ToDecimal(rec.IssuedQty).ToString("0.00")),
                                                        new XElement("td", rec.UOM),
                                                        new XElement("td1", rec.RemaningQty)));



                string tblHeader = "<br /><table cellpadding='2' cellspacing='5' style='text-align: left; font-family: Tahoma; font-size: 12px;'>";
                tblHeader = tblHeader + "<tr><td colspan='7'><b>Issue Part Details : </b></td></tr>" +
                                        "<tr>" +
                                        "<td style='font-weight: bold; text-align: center; border-bottom: solid 1px gray; border-top: solid 1px gray;'>Sr.No.</td>" +
                                        "<td style='font-weight: bold; text-align: center; border-bottom: solid 1px gray; border-top: solid 1px gray;'>Part Code</td>" +
                                        "<td style='font-weight: bold; text-align: center; border-bottom: solid 1px gray; border-top: solid 1px gray;'>Part Name</td>" +
                                        "<td style='font-weight: bold; text-align: center; border-bottom: solid 1px gray; border-top: solid 1px gray;'>Part Description</td>" +
                                        "<td style='font-weight: bold; text-align: center; border-bottom: solid 1px gray; border-top: solid 1px gray;'>Issued Qty</td>" +
                                        "<td style='font-weight: bold; text-align: center; border-bottom: solid 1px gray; border-top: solid 1px gray;'>UOM</td>" +
                                        "<td style='font-weight: bold; text-align: center; border-bottom: solid 1px gray; border-top: solid 1px gray;'>Still <br />Pending Issue</td></tr>";

                result = result + xmlEle.ToString().Replace("<table>", tblHeader).Replace("<td1>", "<td style='text-align: right;'>").Replace("</td1>", "</td>");


            }
            catch { }
            return result;
        }
        // add bu suresh

        protected string EmailGetIssueDetail(long IssueID, string[] conn)
        {
            string result = "";
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                POR_SP_GetIssueSummaryBySiteIDsOrUserIDOrRequestIDOrIssueIDs_Result IssueHead = new POR_SP_GetIssueSummaryBySiteIDsOrUserIDOrRequestIDOrIssueIDs_Result();
                IssueHead = db.POR_SP_GetIssueSummaryBySiteIDsOrUserIDOrRequestIDOrIssueIDs("0", 0, 0, IssueID.ToString()).FirstOrDefault();
                if (IssueHead != null)
                {
                    result = "<br/><br/>===================================================" +
                            "<br/><b>Material Issue Details </b><br/>" +
                            "Issue No. : <b>" + IssueHead.MINH_ID.ToString() +
                            "</b><br/>" +
                            "Issue Date : <b>" + IssueHead.IssueDate.Value.ToString("dd-MMM-yyyy") +
                            "</b><br/>" +
                            "Status : <b>" + IssueHead.IssueStatus +
                            "</b><br/>" +
                            "Issued By : <b>" + IssueHead.IssuedByUserName +
                            "</b><br/><br/>===================================================" +
                            "<br/><b>Transport Detail </b><br/>" +
                            "<br/>" +
                            "Airway Bill : <b>" + IssueHead.AirwayBill +
                            "</b><br/>" +
                            "Shipping Type : <b>" + IssueHead.ShippingType +
                            "</b><br/>" +
                            "Transporter Name : <b>" + IssueHead.TransporterName +
                            "</b><br/>" +
                            "Shipping Date : <b>";

                    if (IssueHead.ShippingDate != null) { result = result + IssueHead.ShippingDate.Value.ToString("dd-MMM-yyyy"); }
                    else if (IssueHead.ShippingDate == null) { result = result + "N/A"; }
                    result = result + "</b><br/>Exp.Delivery Date : <b>";

                    if (IssueHead.ExpectedDelDate != null) { result = result + IssueHead.ExpectedDelDate.Value.ToString("dd-MMM-yyyy"); }
                    else if (IssueHead.ExpectedDelDate == null) { result = result + "N/A"; }
                    result = result + "</b>";
                }
            }
            catch { }
            return result;
        }

        protected string EmailGetEmailIDsByUserID(long UserID, string[] conn)
        {
            string result;
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            mUserProfileHead user = new mUserProfileHead();
            user = db.mUserProfileHeads.Where(u => u.ID == UserID).FirstOrDefault();
            result = user.EmailID;
            return result;
        }

        protected string[] EmailGetEmailIDsBySiteIDApprovalLevel(long SiteID, long ApprovalLevel, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string[] result = new string[] { "", "" };

            if (ApprovalLevel > 0)
            {
                var obj = from u in db.mUserProfileHeads
                          join ald in db.mApprovalLevelDetails on u.ID equals ald.UserID
                          join al in db.mApprovalLevels on ald.ApprovalLevelID equals al.ID
                          where al.TerritoryID == SiteID && al.ApprovalLevel == ApprovalLevel
                          select u;

                foreach (mUserProfileHead o in obj)
                {
                    if (result[0] != "")
                    {
                        result[0] = result[0] + " | " + (o.FirstName + " " + o.MiddelName + " " + o.LastName);
                        result[1] = result[1] + "," + o.EmailID;
                    }
                    else
                    {
                        result[0] = o.FirstName + " " + o.MiddelName + " " + o.LastName;
                        result[1] = o.EmailID;
                    }
                }
            }
            else if (ApprovalLevel == 0)
            {
                var obj = from u in db.mUserProfileHeads
                          join mtd in db.mUserTerritoryDetails on u.ID equals mtd.UserID
                          where mtd.TerritoryID == SiteID && mtd.Level == 1
                          select u;

                foreach (mUserProfileHead o in obj)
                {
                    if (result[0] != "")
                    {
                        result[0] = result[0] + " | " + (o.FirstName + " " + o.MiddelName + " " + o.LastName);
                        result[1] = result[1] + "," + o.EmailID;
                    }
                    else
                    {
                        result[0] = o.FirstName + " " + o.MiddelName + " " + o.LastName;
                        result[1] = o.EmailID;
                    }
                }
            }
            return result;
        }

        protected string MailGetFooter()
        {
            string MailFooter = "<br/><br/>" +
                                "<a href='http://elegantcrm.com/porapp/Login/Login.aspx?ID=13' target='_blank' style='font-size: 18px; color: #3BB9FF; font-family: Comic Sans MS; text-decoration: none;'>Go to Power on Rent</a>" +
                                "<br/><br/>" +
                                "Thank you, <br/>" +
                                "Support Team<br/>" +
                                "<br/><br/><hr/>" +
                                "<b>SELF EXPRESSION. BY Power On Rent </b>" +
                                "<br/>This e-mail, and any attachments hereto, is intended for use only by the addressee(s) named herein, and may contain legally privileged and/or confidential information. If you are not an intended recipient of this e-mail, you are notified that any dissemination, distribution or copying of this e-mail, and any attachments hereto, is strictly prohibited. If you have received this e-mail in error, please notify the sender by reply e-mail, and permanently delete this e-mail, and any copies or printouts. " +
                                "<br/><br/>PLEASE CONSIDER THE ENVIRONMENT BEFORE PRINTING THIS EMAIL.";

            return MailFooter;
        }

        protected void EmailSendWhenMaterialIssued(long RequestID, long IssueID, List<POR_SP_GetPartDetails_OfMIN_Result> PartList, string[] conn)
        {
            try
            {
                string MailSubject;
                string MailBody;
                int E_ID;

                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                POR_SP_GetRequestByRequestIDs_Result Request = new POR_SP_GetRequestByRequestIDs_Result();
                Request = db.POR_SP_GetRequestByRequestIDs(RequestID.ToString()).FirstOrDefault();
                string partdetail = EMailGetIssuePratDetail(PartList, conn);

                /*Acknowledgement Email to Requestor*/
                //MailSubject = "Material Issued : Part Request No. " + Request.RequestNo + "[ " + Request.RequestType + " ] Site : " + Request.SiteName + "";
                MailSubject = "Material Issued against Material Request for " + Request.SiteName + " - ID " + Request.PRH_ID.ToString() + " [ " + Request.RequestType.ToString() + " ]";
                MailBody = " Hello, <br/><b> " + Request.RequestByUserName + " </b> <br/><br/>" +
                           " This is an automatically generated message in reference to a Material request for " + Request.SiteName + " - ID " + Request.PRH_ID.ToString() +
                           " Material has been issued against above mentioned request." +
                           " <br/>" +
                           " Request details & Issue Details are provided below : ";
                MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request);
                MailBody = MailBody + EmailGetIssueDetail(IssueID, conn);
                MailBody = MailBody + partdetail;

                SendMail(MailBody + MailGetFooter(), MailSubject, EmailGetEmailIDsByUserID(Convert.ToInt64(Request.RequestBy), conn));

               // SaveInboxData(Convert.ToInt64(Request.RequestBy), Request.SiteID, "Material Issue", MailSubject, MailBody, Convert.ToInt64(Request.StatusID), conn);
                /*End*/

                /*Information mail to Operation Manger*/
                string[] MailTo = new string[] { };
                MailTo = EmailGetEmailIDsBySiteIDApprovalLevel(Request.SiteID, 1, conn);
                string[] MailToName = MailTo[0].Split('|');
                string[] MailToEmailID = MailTo[1].Split(',');
                for (int i = 0; i < MailToName.Count(); i++)
                {
                    MailSubject = "Notification of Material Issue : Material Request for " + Request.SiteName + " - ID " + Request.PRH_ID.ToString() + " [ " + Request.RequestType.ToString() + " ] ";

                    MailBody = " Hello, <br/><b> " + MailToName[i] + " </b> <br/><br/>" +
                                " This is an automatically generated message in reference to a Material request for " + Request.SiteName + " - ID " + Request.PRH_ID.ToString() +
                                " This is to inform you that, Material has been issued against above mentioned request." +
                                " <br/>" +
                                " Request details & Issue Details are provided below : ";
                    MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request);
                    MailBody = MailBody + EmailGetIssueDetail(IssueID, conn);
                    MailBody = MailBody + partdetail;

                    SendMail(MailBody + MailGetFooter(), MailSubject, MailToEmailID[i]);

                   // E_ID = Convert.ToInt32(GetIDFromEmailName(MailTo[0], MailTo[1], conn));
                   // SaveInboxData(E_ID, Request.SiteID, "Material Issue", MailSubject, MailBody, Convert.ToInt64(Request.StatusID), conn);
                }


                MailTo = new string[] { };
                MailTo = EmailGetEmailIDsBySiteIDApprovalLevel(1, 0, conn);
                MailToName = new string[] { };
                MailToEmailID = new string[] { };
                MailToName = MailTo[0].Split('|');
                MailToEmailID = MailTo[1].Split(',');
                for (int i = 0; i < MailToName.Count(); i++)
                {
                    /*Acknowledgement Email to Project Lead */
                    MailSubject = "Acknowledgement of Material Issue : Material Request for " + Request.SiteName + " - ID " + Request.PRH_ID.ToString() + " [ " + Request.RequestType.ToString() + " ] ";
                    MailBody = " Hello,<br/> <b> " + MailToName[i] + " </b> <br/><br/>" +
                               " Thank you for Material Issue" +
                               " <br/>" +
                              " Request details & Issue Details are provided below : ";
                    MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request);
                    MailBody = MailBody + EmailGetIssueDetail(IssueID, conn);
                    MailBody = MailBody + partdetail;
                    /*End*/
                  
                    SendMail(MailBody + MailGetFooter(), MailSubject, MailToEmailID[i]);

                  //  E_ID = Convert.ToInt32(GetIDFromEmailName(MailTo[0], MailTo[1], conn));
                  //  SaveInboxData(E_ID, Request.SiteID, "Material Issue", MailSubject, MailBody, Convert.ToInt64(Request.StatusID), conn);
                }
            }
            catch { }
            finally { }
        }

        //Add by suresh
        protected void EmailSendWhenMaterialIssued(long RequestID, long IssueID, List<POR_SP_GetPartDetails_OfMIN_Transfer_Result> PartList, string[] conn)
        {
            try
            {
                string MailSubject;
                string MailBody;
                int E_ID;

                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                POR_SP_GetRequestByRequestIDs_Result Request = new POR_SP_GetRequestByRequestIDs_Result();
                Request = db.POR_SP_GetRequestByRequestIDs(RequestID.ToString()).FirstOrDefault();
                string partdetail = EMailGetIssuePratDetail(PartList, conn);

                /*Acknowledgement Email to Requestor*/
                //MailSubject = "Material Issued : Part Request No. " + Request.RequestNo + "[ " + Request.RequestType + " ] Site : " + Request.SiteName + "";
                MailSubject = "Material Issued against Material Request for " + Request.SiteName + " - ID " + Request.PRH_ID.ToString() + " [ " + Request.RequestType.ToString() + " ]";
                MailBody = " Hello, <br/><b> " + Request.RequestByUserName + " </b> <br/><br/>" +
                           " This is an automatically generated message in reference to a Material request for " + Request.SiteName + " - ID " + Request.PRH_ID.ToString() +
                           " Material has been issued against above mentioned request." +
                           " <br/>" +
                           " Request details & Issue Details are provided below : ";
                MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request);
                MailBody = MailBody + EmailGetIssueDetail(IssueID, conn);
                MailBody = MailBody + partdetail;

                SendMail(MailBody + MailGetFooter(), MailSubject, EmailGetEmailIDsByUserID(Convert.ToInt64(Request.RequestBy), conn));

                // SaveInboxData(Convert.ToInt64(Request.RequestBy), Request.SiteID, "Material Issue", MailSubject, MailBody, Convert.ToInt64(Request.StatusID), conn);
                /*End*/

                /*Information mail to Operation Manger*/
                string[] MailTo = new string[] { };
                MailTo = EmailGetEmailIDsBySiteIDApprovalLevel(Request.SiteID, 1, conn);
                string[] MailToName = MailTo[0].Split('|');
                string[] MailToEmailID = MailTo[1].Split(',');
                for (int i = 0; i < MailToName.Count(); i++)
                {
                    MailSubject = "Notification of Material Issue : Material Request for " + Request.SiteName + " - ID " + Request.PRH_ID.ToString() + " [ " + Request.RequestType.ToString() + " ] ";

                    MailBody = " Hello, <br/><b> " + MailToName[i] + " </b> <br/><br/>" +
                                " This is an automatically generated message in reference to a Material request for " + Request.SiteName + " - ID " + Request.PRH_ID.ToString() +
                                " This is to inform you that, Material has been issued against above mentioned request." +
                                " <br/>" +
                                " Request details & Issue Details are provided below : ";
                    MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request);
                    MailBody = MailBody + EmailGetIssueDetail(IssueID, conn);
                    MailBody = MailBody + partdetail;

                    SendMail(MailBody + MailGetFooter(), MailSubject, MailToEmailID[i]);

                    // E_ID = Convert.ToInt32(GetIDFromEmailName(MailTo[0], MailTo[1], conn));
                    // SaveInboxData(E_ID, Request.SiteID, "Material Issue", MailSubject, MailBody, Convert.ToInt64(Request.StatusID), conn);
                }


                MailTo = new string[] { };
                MailTo = EmailGetEmailIDsBySiteIDApprovalLevel(1, 0, conn);
                MailToName = new string[] { };
                MailToEmailID = new string[] { };
                MailToName = MailTo[0].Split('|');
                MailToEmailID = MailTo[1].Split(',');
                for (int i = 0; i < MailToName.Count(); i++)
                {
                    /*Acknowledgement Email to Project Lead */
                    MailSubject = "Acknowledgement of Material Issue : Material Request for " + Request.SiteName + " - ID " + Request.PRH_ID.ToString() + " [ " + Request.RequestType.ToString() + " ] ";
                    MailBody = " Hello,<br/> <b> " + MailToName[i] + " </b> <br/><br/>" +
                               " Thank you for Material Issue" +
                               " <br/>" +
                              " Request details & Issue Details are provided below : ";
                    MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request);
                    MailBody = MailBody + EmailGetIssueDetail(IssueID, conn);
                    MailBody = MailBody + partdetail;
                    /*End*/

                    SendMail(MailBody + MailGetFooter(), MailSubject, MailToEmailID[i]);

                    //  E_ID = Convert.ToInt32(GetIDFromEmailName(MailTo[0], MailTo[1], conn));
                    //  SaveInboxData(E_ID, Request.SiteID, "Material Issue", MailSubject, MailBody, Convert.ToInt64(Request.StatusID), conn);
                }
            }
            catch { }
            finally { }
        }
        #endregion

        #region System Inbox
        protected void SaveInboxData(long ToUserID, long SiteID, string ObjectName, string Subject, string Details, long StatusID, string[] conn)
        {
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                PORtInboxData Data = new PORtInboxData();
                Data.ToUserID = ToUserID;
                Data.SiteID = SiteID;
                Data.ObjectName = ObjectName;
                Data.Subject = Subject;
                Data.Details = Details;
                Data.StatusID = StatusID;
                Data.IsRead = false;
                Data.IsArchive = false;
                Data.FolderName = "Inbox";
                Data.CreationDate = DateTime.Now.ToLocalTime();
                db.AddToPORtInboxDatas(Data);
                db.SaveChanges();
            }
            catch
            {
            }
        }
        #endregion

        protected string GetIDFromEmailName(string name, string emailid, string[] conn)
        {
            string ide = "";
            string[] m = new string[] { };
            m = new string[] { };
            string[] n = new string[] { };
            n = new string[] { };
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                mUserProfileHead user = new mUserProfileHead();
                m = emailid.Split(',');
                n = name.Split('|');

                for (int i = 0; i < emailid.Count(); i++)
                {
                    user = db.mUserProfileHeads.Where(u => u.EmailID == emailid[i].ToString()).FirstOrDefault();
                    ide = Convert.ToString(user.ID);
                }
            }
            catch { }
            finally { }
            return ide;
        }


        //Add By Suresh For Transfer
        public long SetIntoTransHead(PORtTransHead TransHead, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            if (TransHead.TransH_ID == 0)
            {
                db.PORtTransHeads.AddObject(TransHead);
            }
            else
            {
                db.PORtTransHeads.Attach(TransHead);                
                db.ObjectStateManager.ChangeObjectState(TransHead, EntityState.Modified);
            }
            db.SaveChanges();          
           
            return TransHead.TransH_ID;
        }

        public void FinalSaveTransferPartDetail(string paraSessionID, string paraCurrentObjectName, long TransH_ID, long PRH_ID, string paraUserID, string TransferStatus, long FromSiteID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            //List<POR_SP_GetPartDetails_OfMIN_Result> finalSaveLst = new List<POR_SP_GetPartDetails_OfMIN_Result>();
            List<POR_SP_GetPartDetails_OfMIN_Transfer_Result> finalSaveLst = new List<POR_SP_GetPartDetails_OfMIN_Transfer_Result>();
            finalSaveLst = GetExistingTempDataBySessionIDObjectName_Transfer(paraSessionID, paraUserID, paraCurrentObjectName, conn);

            XElement xmlEle = new XElement("Transfer", from rec in finalSaveLst
                                                    select new XElement("PartList",
                                                    new XElement("TransH_ID", TransH_ID),
                                                    new XElement("PRD_ID", Convert.ToInt64(rec.PRD_ID)),
                                                    new XElement("Prod_ID", Convert.ToInt64(rec.Prod_ID)),
                                                    new XElement("Prod_Name", rec.Prod_Name),
                                                    new XElement("Prod_Description", rec.Prod_Description),
                                                    new XElement("IssuedQty", Convert.ToDecimal(rec.IssuedQty)),
                                                    new XElement("Sequence", Convert.ToInt64(rec.Sequence)),
                                                    new XElement("UOMID", Convert.ToInt64(rec.UOMID)),
                                                    new XElement("Prod_Code", rec.Prod_Code)));

            ObjectParameter _TransferID = new ObjectParameter("TransferID", typeof(long));
            _TransferID.Value = TransH_ID;

            ObjectParameter _xmlData = new ObjectParameter("xmlData", typeof(string));
            _xmlData.Value = xmlEle.ToString();


            ObjectParameter[] obj = new ObjectParameter[] { _TransferID, _xmlData };
            db.ExecuteFunction("POR_SP_InsertIntoPORtTransDetail", obj);
            db.SaveChanges();

                           
            List<PORtTransDetail> PartList = new List<PORtTransDetail>();
            PartList = db.PORtTransDetails.Where(r => r.TransH_ID == TransH_ID).ToList();
                foreach (PORtTransDetail p in PartList)
                {
                    tProductStockDetail psd = (from s in db.tProductStockDetails
                                               where s.SiteID == FromSiteID  && s.ProdID == p.Prod_ID
                                               select s).SingleOrDefault();
                    psd.AvailableBalance = psd.AvailableBalance - p.IssuedQty;
                    // db.tProductStockDetails.AddObject(psd);
                    db.SaveChanges();
                }

                EmailSendWhenMaterialIssued(PRH_ID, TransH_ID, finalSaveLst, conn);
            
           // ClearTempDataFromDB(paraSessionID, paraUserID, paraCurrentObjectName, conn);            
        }


        public long SetIntoMINHead_Transfer(PORtMINHead MINHead, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            
                db.PORtMINHeads.Attach(MINHead);
                db.ObjectStateManager.ChangeObjectState(MINHead, EntityState.Modified);
            
            db.SaveChanges();
            if (MINHead.StatusID != 1 && MINHead.StatusID != 10)
            {
                /*Update Request Status*/
                PORtPartRequestHead RequestHead = new PORtPartRequestHead();
                RequestHead = db.PORtPartRequestHeads.Where(r => r.PRH_ID == MINHead.PRH_ID).FirstOrDefault();
                db.PORtPartRequestHeads.Detach(RequestHead);
                RequestHead.StatusID = MINHead.StatusID;
                db.PORtPartRequestHeads.Attach(RequestHead);
                db.ObjectStateManager.ChangeObjectState(RequestHead, EntityState.Modified);
                db.SaveChanges();


                /*Insert into ReceiptHead & ReceiptPartDetails*/
                PORtGRNHead ReceiptHead = new PORtGRNHead();
                ReceiptHead.SiteID = MINHead.SiteID;
                ReceiptHead.ObjectName = "MaterialIssue";
                ReceiptHead.ReferenceID = MINHead.MINH_ID;
                ReceiptHead.GRN_No = "N/A";
                ReceiptHead.ReceivedByUserID = 0;
                ReceiptHead.StatusID = 1;
                ReceiptHead.IsSubmit = false;
                ReceiptHead.CreatedBy = MINHead.CreatedBy;
                ReceiptHead.CreationDt = DateTime.Now;
                db.PORtGRNHeads.AddObject(ReceiptHead);
                db.SaveChanges();

            }
            return MINHead.MINH_ID;
        }

        public void FinalSaveIssuePartDetail_Transfer(string paraSessionID, string paraCurrentObjectName, long MINH_ID, long PRH_ID, string paraUserID, string IssueStatus, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<POR_SP_GetPartDetails_OfMIN_Transfer_Result> finalSaveLst = new List<POR_SP_GetPartDetails_OfMIN_Transfer_Result>();
            finalSaveLst = GetExistingTempDataBySessionIDObjectName_Transfer(paraSessionID, paraUserID, paraCurrentObjectName, conn);

            XElement xmlEle = new XElement("Issue", from rec in finalSaveLst
                                                    select new XElement("PartList",
                                                    new XElement("MINH_ID", MINH_ID),
                                                    new XElement("PRD_ID", Convert.ToInt64(rec.PRD_ID)),
                                                    new XElement("Prod_ID", Convert.ToInt64(rec.Prod_ID)),
                                                    new XElement("Prod_Name", rec.Prod_Name),
                                                    new XElement("Prod_Description", rec.Prod_Description),
                                                    new XElement("IssuedQty", Convert.ToDecimal(rec.IssuedQty)),
                                                    new XElement("Sequence", Convert.ToInt64(rec.Sequence)),
                                                    new XElement("UOMID", Convert.ToInt64(rec.UOMID)),
                                                    new XElement("Prod_Code", rec.Prod_Code)));

            ObjectParameter _IssueID = new ObjectParameter("IssueID", typeof(long));
            _IssueID.Value = MINH_ID;

            ObjectParameter _xmlData = new ObjectParameter("xmlData", typeof(string));
            _xmlData.Value = xmlEle.ToString();


            ObjectParameter[] obj = new ObjectParameter[] { _IssueID, _xmlData };
            db.ExecuteFunction("POR_SP_InsertIntoPORtMINDetail", obj);
            db.SaveChanges();

            /*Insert into GRNPartDetail if status is Issue*/
            if (IssueStatus == "Issue")
            {
                List<PORtMINDetail> PartList = new List<PORtMINDetail>();
                PartList = db.PORtMINDetails.Where(r => r.MINH_ID == MINH_ID).ToList();
                PORtGRNHead ReceiptHead = new PORtGRNHead();
                ReceiptHead = db.PORtGRNHeads.Where(r => r.ReferenceID == MINH_ID && r.ObjectName == "MaterialIssue").FirstOrDefault();

                foreach (PORtMINDetail part in PartList)
                {
                    PORtGRNDetail ReceiptPart = new PORtGRNDetail();
                    ReceiptPart.GRNH_ID = ReceiptHead.GRNH_ID;
                    ReceiptPart.Prod_ID = part.Prod_ID;
                    ReceiptPart.ChallanQty = part.IssuedQty;
                    ReceiptPart.ReceivedQty = part.IssuedQty;
                    ReceiptPart.ShortQty = 0;
                    ReceiptPart.ExcessQty = 0;
                    ReceiptPart.ReceivedQty = part.IssuedQty;
                    ReceiptPart.Sequence = part.Sequence;
                    ReceiptPart.MIND_ID = part.MIND_ID;
                    ReceiptPart.UOMID = part.UOMID;
                    db.PORtGRNDetails.AddObject(ReceiptPart);
                    db.SaveChanges();
                }
                //add by suresh
                //foreach (PORtMINDetail p in PartList)
                //{
                //    tProductStockDetail psd = (from s in db.tProductStockDetails
                //                               where s.SiteID == 1 && s.ProdID == p.Prod_ID
                //                               select s).SingleOrDefault();
                //    psd.AvailableBalance = psd.AvailableBalance - p.IssuedQty;
                //    // db.tProductStockDetails.AddObject(psd);
                //    db.SaveChanges();
                //}

                //EmailSendWhenMaterialIssued(PRH_ID, MINH_ID, finalSaveLst, conn);
            }
            ClearTempDataFromDB(paraSessionID, paraUserID, paraCurrentObjectName, conn);
            /*if (result == finalSaveLst.Count)
            {
               
            }*/
        }


        public long GetStatusOfIssueHead_Transfer(string SessionID, string UserID, string ObjectName, long RequestID, string[] conn)
        {
            long StatusID = 0;
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            try
            {
                List<POR_SP_GetPartDetails_OfMIN_Transfer_Result> CurrentPartList = new List<POR_SP_GetPartDetails_OfMIN_Transfer_Result>();
                CurrentPartList = GetExistingTempDataBySessionIDObjectName_Transfer(SessionID, UserID, ObjectName, conn).ToList();
                var CurrentPRD_ID = (from c in CurrentPartList
                                     select c.PRD_ID);
                List<PORtPartRequestDetail> DBRequestPartList = new List<PORtPartRequestDetail>();
                DBRequestPartList = db.PORtPartRequestDetails.Where(rd => rd.PRH_ID == RequestID).ToList();

                var PendingPartList = DBRequestPartList.Where(dp => !CurrentPRD_ID.Contains(dp.PRD_ID)).ToList();

                if (PendingPartList.Count == 0)
                {
                    PendingPartList = (from d in DBRequestPartList
                                       from c in CurrentPartList
                                       where d.PRD_ID == c.PRD_ID && d.RemaningQty != c.IssuedQty
                                       select d).ToList();
                }
                else  //add by suresh
                {
                    for (int k = 0; k <= PendingPartList.Count; k++)
                    {
                        PendingPartList = (from d in DBRequestPartList
                                           from p in PendingPartList
                                           where d.PRD_ID == p.PRD_ID && d.RemaningQty != 0
                                           select d).ToList();
                    }

                }

                if (PendingPartList.Count > 0)
                {
                    StatusID = 6;
                }
                else if (PendingPartList.Count == 0)
                {
                    StatusID = 7;
                }
            }
            catch { }
            finally { }
            return StatusID;
        }

        //public List<POR_SP_GetPartDetails_OfMIN_Transfer_Result> RemovePartFromTransfer_TempData(string paraSessionID, string paraUserID, string paraCurrentObjectName, int paraSequence, string[] conn)
        //{
        //    BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
        //    /*Begin : Get Existing Records from TempData*/
        //    List<POR_SP_GetPartDetails_OfMIN_Transfer_Result> existingList = new List<POR_SP_GetPartDetails_OfMIN_Transfer_Result>();
        //    existingList = GetExistingTempDataBySessionIDObjectName_Transfer(paraSessionID, paraUserID, paraCurrentObjectName, conn);
        //    /*End*/

        //    /*Get Filter List [Filter By paraSequence]*/
        //    List<POR_SP_GetPartDetails_OfMIN_Transfer_Result> filterList = new List<POR_SP_GetPartDetails_OfMIN_Transfer_Result>();
        //    filterList = (from exist in existingList
        //                  where exist.Sequence != paraSequence
        //                  select exist).ToList();
        //    /*End
      

        //    /*Save result to TempData*/
        //    SaveTempDataToDB_Transfer(filterList, paraSessionID, paraUserID, paraCurrentObjectName, conn);
        //    /*End*/

        //    return filterList;
        //}

        //protected void SaveTempDataToDB_Transfer(List<POR_SP_GetPartDetails_OfMIN_Transfer_Result> paraobjList, string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn)
        //{
        //    BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
        //    /*Begin : Remove Existing Records*/
        //    ClearTempDataFromDB(paraSessionID, paraUserID, paraCurrentObjectName, conn);
        //    /*End*/

        //    /*Begin : Serialize MergedAddToCartList*/
        //    string xml = "";
        //    xml = datahelper.SerializeEntity(paraobjList);
        //    /*End*/

        //    /*Begin : Save Serialized List into TempData */
        //    TempData tempdata = new TempData();
        //    tempdata.Data = xml;
        //    tempdata.XmlData = "";
        //    tempdata.LastUpdated = DateTime.Now;
        //    tempdata.SessionID = paraSessionID.ToString();
        //    tempdata.UserID = paraUserID.ToString();
        //    tempdata.ObjectName = paraCurrentObjectName.ToString();
        //    tempdata.TableName = "table";
        //    db.AddToTempDatas(tempdata);
        //    db.SaveChanges();
        //    /*End*/

        //}
       
    }
}


