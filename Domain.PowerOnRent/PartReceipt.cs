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
    public partial class PartReceipt : iPartReceipt
    {
        Domain.Server.Server svr = new Server.Server();
        DataHelper datahelper = new DataHelper();

        #region Part Receipt Head
        public PORtGRNHead GetReceiptHeadByReceiptID(long ReceiptID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            PORtGRNHead PartReceipt = new PORtGRNHead();
            PartReceipt = db.PORtGRNHeads.Where(g => g.GRNH_ID == ReceiptID).FirstOrDefault();
            db.PORtGRNHeads.Detach(PartReceipt);
            return PartReceipt;
        }

        public PORtGRNHead GetReceiptHeadByIssueID(long IssueID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<PORtGRNHead> ReceiptHead = new List<PORtGRNHead>();
            PORtGRNHead ReturnReceiptHead = new PORtGRNHead();
            ReceiptHead = db.PORtGRNHeads.Where(g => g.ReferenceID == IssueID && g.ObjectName == "MaterialIssue").DefaultIfEmpty().ToList();
            if (ReceiptHead.Count > 0)
            {
                if (ReceiptHead[0] != null)
                {
                    db.PORtGRNHeads.Detach(ReceiptHead[0]);
                    ReturnReceiptHead = ReceiptHead[0];
                }
            }
            return ReturnReceiptHead;
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

            if (GRNHead.StatusID > 1)
            {
                /*Update Issue Status*/
                PORtMINHead MINHead = new PORtMINHead();
                MINHead = db.PORtMINHeads.Where(r => r.MINH_ID == GRNHead.ReferenceID).FirstOrDefault();
                db.PORtMINHeads.Detach(MINHead);
                MINHead.StatusID = GRNHead.StatusID;
                db.PORtMINHeads.Attach(MINHead);
                db.ObjectStateManager.ChangeObjectState(MINHead, EntityState.Modified);
                db.SaveChanges();

                /*Update Request Status*/
                PORtPartRequestHead RequestHead = new PORtPartRequestHead();
                RequestHead = db.PORtPartRequestHeads.Where(r => r.PRH_ID == MINHead.PRH_ID).FirstOrDefault();
                db.PORtPartRequestHeads.Detach(RequestHead);
                RequestHead.StatusID = GRNHead.StatusID;
                db.PORtPartRequestHeads.Attach(RequestHead);
                db.ObjectStateManager.ChangeObjectState(RequestHead, EntityState.Modified);
                db.SaveChanges();
                /*End*/


            }
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
        #endregion

        #region PartsDetails OfReceipt
        public List<POR_SP_GetPartDetails_OfGRN_Result> GetReceiptPartDetailByIssueID(long IssueID, long SiteID, string sessionID, string userID, string CurrentObject, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<POR_SP_GetPartDetails_OfGRN_Result> PartDetail = new List<POR_SP_GetPartDetails_OfGRN_Result>();
            PartDetail = db.POR_SP_GetPartDetails_OfGRN("0", IssueID, 0, SiteID).ToList();
            SaveTempDataToDB(PartDetail, sessionID, userID, CurrentObject, conn);
            return PartDetail;
        }

        public List<POR_SP_GetPartDetails_OfGRN_Result> GetReceiptPartDetailByReceiptID(long ReceiptID, long SiteID, string sessionID, string userID, string CurrentObject, string IssuedQtySameAs, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<POR_SP_GetPartDetails_OfGRN_Result> PartDetail = new List<POR_SP_GetPartDetails_OfGRN_Result>();
            PartDetail = db.POR_SP_GetPartDetails_OfGRN("0", 0, ReceiptID, SiteID).ToList();
            SaveTempDataToDB(PartDetail, sessionID, userID, CurrentObject, conn);
            return PartDetail;
        }

        protected void SaveTempDataToDB(List<POR_SP_GetPartDetails_OfGRN_Result> paraobjList, string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn)
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

        public List<POR_SP_GetPartDetails_OfGRN_Result> GetExistingTempDataBySessionIDObjectName(string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<POR_SP_GetPartDetails_OfGRN_Result> objtAddToCartProductDetailList = new List<POR_SP_GetPartDetails_OfGRN_Result>();

            TempData tempdata = new TempData();
            tempdata = (from temp in db.TempDatas
                        where temp.SessionID == paraSessionID
                        && temp.ObjectName == paraCurrentObjectName
                        && temp.UserID == paraUserID
                        select temp).FirstOrDefault();
            if (tempdata != null)
            {
                objtAddToCartProductDetailList = datahelper.DeserializeEntity1<POR_SP_GetPartDetails_OfGRN_Result>(tempdata.Data);
            }
            return objtAddToCartProductDetailList;
        }

        public List<POR_SP_GetPartDetails_OfGRN_Result> AddPartIntoReceipt_TempData(string MIND_IDs, long IssueID, long SiteID, string IssuedQtySameAs, string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            /*Begin : Get Existing Records from TempData*/
            List<POR_SP_GetPartDetails_OfGRN_Result> existingList = new List<POR_SP_GetPartDetails_OfGRN_Result>();
            existingList = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, paraCurrentObjectName, conn);
            /*End*/

            /*Get Product Details*/
            List<POR_SP_GetPartDetails_OfGRN_Result> getnewRec = new List<POR_SP_GetPartDetails_OfGRN_Result>();
            getnewRec = (from view in db.POR_SP_GetPartDetails_OfGRN(MIND_IDs, IssueID, 0, SiteID)
                         orderby view.Sequence
                         select view).ToList();
            /*End*/

            /*Begin : Merge (Existing + Newly Added) Products to Create TempData of AddToCart*/
            List<POR_SP_GetPartDetails_OfGRN_Result> mergedList = new List<POR_SP_GetPartDetails_OfGRN_Result>();
            mergedList.AddRange(existingList);
            mergedList.AddRange(getnewRec);
            /*End*/

            /*Begin : Serialize & Save MergedAddToCartList*/
            SaveTempDataToDB(mergedList, paraSessionID, paraUserID, paraCurrentObjectName, conn);
            /*End*/

            return mergedList;
        }

        public List<POR_SP_GetPartDetails_OfGRN_Result> RemovePartFromReceipt_TempData(string paraSessionID, string paraUserID, string paraCurrentObjectName, int paraSequence, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            /*Begin : Get Existing Records from TempData*/
            List<POR_SP_GetPartDetails_OfGRN_Result> existingList = new List<POR_SP_GetPartDetails_OfGRN_Result>();
            existingList = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, paraCurrentObjectName, conn);
            /*End*/

            /*Get Filter List [Filter By paraSequence]*/
            List<POR_SP_GetPartDetails_OfGRN_Result> filterList = new List<POR_SP_GetPartDetails_OfGRN_Result>();
            filterList = (from exist in existingList
                          where exist.Sequence != paraSequence
                          select exist).ToList();
            /*End*/

            /*Save result to TempData*/
            SaveTempDataToDB(filterList, paraSessionID, paraUserID, paraCurrentObjectName, conn);
            /*End*/

            return filterList;
        }

        public string[] UpdatePartReceipt_TempData(string SessionID, string CurrentObjectName, string UserID, POR_SP_GetPartDetails_OfGRN_Result Receipt, string[] conn)
        {
            string[] result;
            result = new string[] { "0", "0" };
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                List<POR_SP_GetPartDetails_OfGRN_Result> getRec = new List<POR_SP_GetPartDetails_OfGRN_Result>();
                getRec = GetExistingTempDataBySessionIDObjectName(SessionID, UserID, CurrentObjectName, conn);

                POR_SP_GetPartDetails_OfGRN_Result updateRec = new POR_SP_GetPartDetails_OfGRN_Result();
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

        public void FinalSaveReceiptPartDetail(string paraSessionID, string paraCurrentObjectName, long GRNH_ID, string paraUserID, string ReceiptStatus, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<POR_SP_GetPartDetails_OfGRN_Result> finalSaveLst = new List<POR_SP_GetPartDetails_OfGRN_Result>();
            finalSaveLst = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, paraCurrentObjectName, conn);

            XElement xmlEle = new XElement("Receipt", from rec in finalSaveLst
                                                      select new XElement("PartList",
                                                      new XElement("GRNH_ID", GRNH_ID),
                                                      new XElement("Prod_ID", Convert.ToInt64(rec.Prod_ID)),
                                                      new XElement("ChallanQty", Convert.ToDecimal(rec.ChallanQty)),
                                                      new XElement("ReceivedQty", Convert.ToDecimal(rec.ReceivedQty)),
                                                      new XElement("ExcessQty", Convert.ToDecimal(rec.ExcessQty)),
                                                      new XElement("ShortQty", Convert.ToDecimal(rec.ShortQty)),
                                                      new XElement("Sequence", Convert.ToInt64(rec.Sequence)),
                                                      new XElement("MIND_ID", Convert.ToInt64(rec.MIND_ID)),
                                                      new XElement("UOMID", Convert.ToInt64(rec.UOMID))));

            ObjectParameter _ReceiptID = new ObjectParameter("ReceiptID", typeof(long));
            _ReceiptID.Value = GRNH_ID;

            ObjectParameter _xmlData = new ObjectParameter("xmlData", typeof(string));
            _xmlData.Value = xmlEle.ToString();


            ObjectParameter[] obj = new ObjectParameter[] { _ReceiptID, _xmlData };
            db.ExecuteFunction("POR_SP_InsertIntoPORtGRNDetail", obj);

            db.SaveChanges();
            if (ReceiptStatus == "Received")
            {
                EmailSendWhenMaterialReceived(GRNH_ID, conn);
            }

            ClearTempDataFromDB(paraSessionID, paraUserID, paraCurrentObjectName, conn);
        }
        #endregion

        #region Receipt Summary

      /*  public List<POR_SP_GetReceiptSummaryBySiteIDsOrUserIDOrRequestID_Result> GetReceiptSummaryByUserID(long UserID, string[] conn)
        {
            List<POR_SP_GetReceiptSummaryBySiteIDsOrUserIDOrRequestID_Result> ReceiptSummary = new List<POR_SP_GetReceiptSummaryBySiteIDsOrUserIDOrRequestID_Result>();
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                ReceiptSummary = db.POR_SP_GetReceiptSummaryBySiteIDsOrUserIDOrRequestID("0", UserID, 0).OrderByDescending(o => o.GRNH_ID).ToList();
            }
            catch { }
            finally { }
            return ReceiptSummary;
        }

        public List<POR_SP_GetReceiptSummaryBySiteIDsOrUserIDOrRequestID_Result> GetReceiptSummaryBySiteIDs(string SiteIDs, string[] conn)
        {
            List<POR_SP_GetReceiptSummaryBySiteIDsOrUserIDOrRequestID_Result> ReceiptSummary = new List<POR_SP_GetReceiptSummaryBySiteIDsOrUserIDOrRequestID_Result>();
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                ReceiptSummary = db.POR_SP_GetReceiptSummaryBySiteIDsOrUserIDOrRequestID(SiteIDs, 0, 0).OrderByDescending(o => o.GRNH_ID).ToList();
            }
            catch { }
            finally { }
            return ReceiptSummary;
        }

        public List<POR_SP_GetReceiptSummaryBySiteIDsOrUserIDOrRequestID_Result> GetReceiptSummaryByRequestID(long RequestID, string[] conn)
        {
            List<POR_SP_GetReceiptSummaryBySiteIDsOrUserIDOrRequestID_Result> ReceiptSummary = new List<POR_SP_GetReceiptSummaryBySiteIDsOrUserIDOrRequestID_Result>();
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                ReceiptSummary = db.POR_SP_GetReceiptSummaryBySiteIDsOrUserIDOrRequestID("0", 0, RequestID).OrderByDescending(o => o.GRNH_ID).ToList();
            }
            catch { }
            finally { }
            return ReceiptSummary;
        }*/
        #endregion

        #region ReceiptMail
        public void SendMail(string MailBody, string MailSubject, string ToEmailIDs)
        {
            try
            {
                //SmtpClient smtpClient = new SmtpClient("smtpout.asia.secureserver.net", 25);
                //MailMessage message = new MailMessage();

                SmtpClient smtpClient = new SmtpClient("10.228.134.54", 25);
                MailMessage message = new MailMessage();

                //  MailAddress fromAddress = new MailAddress("admin@elegantcrm.com", "Part Request System");
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

        protected string EMailGetReceiptDetail(POR_SP_GetReceiptSummaryBySiteIDsOrUserIDOrRequestID_Result ReceiptRec)
        {


            string result = "";
            result = "<br/><br/>===================================================" +
                     "<br/><b>Material Receipt Details </b><br/>" +
                     "<br/>" +
                     "Receipt No. : <b>" + ReceiptRec.GRNH_ID.ToString() + "</b>" +
                     "<br/>" +
                     "Receipt Date : <b>" + ReceiptRec.GRN_Date.Value.ToString("dd-MMM-yyyy") + "</b>" +
                     "<br/>" +
                     "Status : <b>" + ReceiptRec.ReceiptStatus + "</b>" +
                     "<br/>" +
                     "Site / Warehouse : <b>" + ReceiptRec.SiteName + "</b>" +
                     "<br/>" +
                     "Received By : <b>" + ReceiptRec.ReceiptByUserName + "</b>";
            return result;
        }

        protected string EMailGetReceiptPratDetail(long ReceiptID, long SietID, string[] conn)
        {
            string result = "";
            try
            {
                List<POR_SP_GetPartDetails_OfGRN_Result> htmlList = new List<POR_SP_GetPartDetails_OfGRN_Result>();
                if (ReceiptID != 0)
                {
                    BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                    htmlList = db.POR_SP_GetPartDetails_OfGRN("0", 0, ReceiptID, SietID).ToList();
                }

                int srno = 0;
                XElement xmlEle = new XElement("table", from rec in htmlList
                                                        select new XElement("tr",
                                                        new XElement("td", (srno = srno + 1) + "."),
                                                        new XElement("td", rec.Prod_Code),
                                                        new XElement("td", rec.Prod_Name),
                                                        new XElement("td", rec.Prod_Description),
                                                        new XElement("td1", rec.ReceivedQty),
                                                        new XElement("td", rec.UOM),
                                                        new XElement("td1", rec.ChallanQty)));


                string tblHeader = "<br /><table cellpadding='2' cellspacing='5' style='text-align: left; font-family: Tahoma; font-size: 12px;'>";
                tblHeader = tblHeader + "<tr><td colspan='7'><b>Receipt Part Details : </b></td></tr>" +
                                        "<tr>" +
                                        "<td style='font-weight: bold; text-align: center; border-bottom: solid 1px gray; border-top: solid 1px gray;'>Sr.No.</td>" +
                                        "<td style='font-weight: bold; text-align: center; border-bottom: solid 1px gray; border-top: solid 1px gray;'>Part Code</td>" +
                                        "<td style='font-weight: bold; text-align: center; border-bottom: solid 1px gray; border-top: solid 1px gray;'>Part Name</td>" +
                                        "<td style='font-weight: bold; text-align: center; border-bottom: solid 1px gray; border-top: solid 1px gray;'>Part Description</td>" +
                                        "<td style='font-weight: bold; text-align: center; border-bottom: solid 1px gray; border-top: solid 1px gray;'>Receipt Qty</td>" +
                                        "<td style='font-weight: bold; text-align: center; border-bottom: solid 1px gray; border-top: solid 1px gray;'>UOM</td>" +
                                        "<td style='font-weight: bold; text-align: center; border-bottom: solid 1px gray; border-top: solid 1px gray;'>Issued Qty</td></tr>";

                //result = result + xmlEle.ToString().Replace("<table>", tblHeader);
                result = result + xmlEle.ToString().Replace("<table>", tblHeader).Replace("<td1>", "<td style='text-align: right;'>").Replace("</td1>", "</td>");


            }
            catch { }
            return result;
        }

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
                            "Issue No. : <b>" + IssueHead.IssueNo.ToString() + "</b>" +
                            "<br/>" +
                            "Issue Date : <b>" + IssueHead.IssueDate.Value.ToString("dd-MMM-yyyy") + "</b>" +
                            "<br/>" +
                            "Status : <b>" + IssueHead.IssueStatus + "</b>" +
                            "<br/>" +
                            "Issued By : <b>" + IssueHead.IssuedByUserName + "</b>" +
                            "<br/><br/>===================================================" +
                            "<br/>" +
                            "<b>Transport Detail </b><br/>" +
                            "<br/>" +
                            "Airway Bill : <b>" + IssueHead.AirwayBill + "</b>" +
                            "<br/>" +
                            "Shipping Type : <b>" + IssueHead.ShippingType + "</b>" +
                            "<br/>" +
                            "Shipping Date : <b>" + IssueHead.ShippingDate + "</b>" +
                            "<br/>" +
                            "Exp.Deliver Date : <b>" + IssueHead.ExpectedDelDate + "</b>" +
                            "<br/>" +
                            "Transporter Name : <b>" + IssueHead.TransporterName + "</b>";
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

        protected void EmailSendWhenMaterialReceived(long ReceiptID, string[] conn)
        {
            try
            {
                string MailSubject;
                string MailBody;
                int E_ID;

                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                PORtGRNHead GRNHead1 = new PORtGRNHead();
                GRNHead1 = db.PORtGRNHeads.Where(g => g.GRNH_ID == ReceiptID).FirstOrDefault();

                POR_SP_GetIssueSummaryBySiteIDsOrUserIDOrRequestIDOrIssueIDs_Result IssueRec = new POR_SP_GetIssueSummaryBySiteIDsOrUserIDOrRequestIDOrIssueIDs_Result();
                IssueRec = db.POR_SP_GetIssueSummaryBySiteIDsOrUserIDOrRequestIDOrIssueIDs(GRNHead1.SiteID.Value.ToString(), 0, 0, GRNHead1.ReferenceID.ToString()).FirstOrDefault();

                POR_SP_GetReceiptSummaryBySiteIDsOrUserIDOrRequestID_Result ReceiptRec = new POR_SP_GetReceiptSummaryBySiteIDsOrUserIDOrRequestID_Result();
                ReceiptRec = db.POR_SP_GetReceiptSummaryBySiteIDsOrUserIDOrRequestID(IssueRec.SiteID.ToString(), 0, IssueRec.PRH_ID).Where(r => r.GRNH_ID == GRNHead1.GRNH_ID).FirstOrDefault();
                string partdetail = EMailGetReceiptPratDetail(GRNHead1.GRNH_ID, Convert.ToInt32(GRNHead1.SiteID), conn);

                /*Acknowledgement Email to Issuer [Project Lead]*/
                MailSubject = "Acknowledgement of Material Receipt of " + ReceiptRec.SiteName + " & Receipt No. " + ReceiptRec.GRNH_ID.ToString() + "  against Issue No. " + IssueRec.MINH_ID.ToString();

                MailBody = " Hello, <br/><b> " + IssueRec.IssuedByUserName + " </b> <br/><br/>" +
                           " This is an automatically generated message in reference to a Material issued for " + ReceiptRec.SiteName + " - ID " + ReceiptRec.GRNH_ID.ToString() + "." +
                           " Material has been received at " + ReceiptRec.SiteName + ", received by  " + IssueRec.RequestByUserName + "." +
                           " <br/>" +
                           " Issue & Receipt Details are provided below : ";
                MailBody = MailBody + "<br/><br/>" + EmailGetIssueDetail(IssueRec.MINH_ID, conn);
                MailBody = MailBody + EMailGetReceiptDetail(ReceiptRec);
                MailBody = MailBody + partdetail;
                SendMail(MailBody + MailGetFooter(), MailSubject, EmailGetEmailIDsByUserID(Convert.ToInt64(IssueRec.IssuedByUserID), conn));
               // SaveInboxData(Convert.ToInt64(IssueRec.IssuedByUserID), IssueRec.SiteID, "Receipt", MailSubject, MailBody, Convert.ToInt64(IssueRec.StatusID), conn);
                /*End*/

                /*Information mail to Operation Manger*/
                string[] MailTo = new string[] { };
                MailTo = EmailGetEmailIDsBySiteIDApprovalLevel(IssueRec.SiteID, 1, conn);
                string[] MailToName = MailTo[0].Split('|');
                string[] MailToEmailID = MailTo[1].Split(',');
                for (int i = 0; i < MailToName.Count(); i++)
                {

                    MailBody = " Hello, <br/><b> " + MailToName[i] + " </b> <br/><br/>" +
                               " This is an automatically generated message in reference to a Material issued for " + ReceiptRec.SiteName + " - ID " + ReceiptRec.GRNH_ID.ToString() + "." +
                               " Material has been received at " + ReceiptRec.SiteName + ", received by " + IssueRec.RequestByUserName + "." +
                               " <br/>" +
                               " Issue & Receipt Details are provided below : ";
                    MailBody = MailBody + "<br/><br/>" + EmailGetIssueDetail(IssueRec.MINH_ID, conn);
                    MailBody = MailBody + EMailGetReceiptDetail(ReceiptRec);
                    MailBody = MailBody + partdetail;
                    SendMail(MailBody + MailGetFooter(), MailSubject, MailToEmailID[i]);
                 //   E_ID = Convert.ToInt32(GetIDFromEmailName(MailTo[0], MailTo[1], conn));
                  //  SaveInboxData(E_ID, IssueRec.SiteID, "Receipt", MailSubject, MailBody, Convert.ToInt64(IssueRec.StatusID), conn);
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
    }
}
