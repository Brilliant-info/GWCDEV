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
using System.Net;
using Interface.PowerOnRent;
using System.Net.Mail;
using System.Data.SqlClient;
using System.Net.Mime;
using System.Globalization;


namespace Domain.PowerOnRent
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public partial class PartRequest : iPartRequest
    {
        Domain.Server.Server svr = new Server.Server();
        DataHelper datahelper = new DataHelper();

        #region Part Request Head
        public PORtPartRequestHead GetRequestHeadByRequestID(long RequestID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            PORtPartRequestHead PartReq = new PORtPartRequestHead();
            PartReq = db.PORtPartRequestHeads.Where(r => r.PRH_ID == RequestID).FirstOrDefault();
            db.PORtPartRequestHeads.Detach(PartReq);
            return PartReq;
        }

        public tOrderHead GetOrderHeadByOrderID(long OrderID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            tOrderHead PartReq = new tOrderHead();
            PartReq = db.tOrderHeads.Where(r => r.Id == OrderID).FirstOrDefault();
            db.tOrderHeads.Detach(PartReq);
            return PartReq;
        }

        public long SetIntoPartRequestHead(PORtPartRequestHead PartRequest, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            if (PartRequest.PRH_ID == 0)
            {
                db.PORtPartRequestHeads.AddObject(PartRequest);
            }
            else
            {
                db.PORtPartRequestHeads.Attach(PartRequest);
                db.ObjectStateManager.ChangeObjectState(PartRequest, EntityState.Modified);
            }
            db.SaveChanges();
            return PartRequest.PRH_ID;
        }

        public List<mStatu> GetStatusListForRequest(string Remark, string state, long UserID, string[] conn)
        {
            List<mStatu> statusdetail = new List<mStatu>();
            try
            {
                using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
                {

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

                    //if (state == "Add" || state == "Edit")
                    //{
                    //    mUserRolesDetail RoleDetail = new mUserRolesDetail();
                    //    RoleDetail = db.mUserRolesDetails.Where(r => r.UserID == UserID && r.ObjectName == "MaterialRequest").FirstOrDefault();
                    //    if (RoleDetail != null)
                    //    {
                    //        if (RoleDetail.Approval == false)
                    //        {
                    //            statusdetail = statusdetail.Where(s => s.ID != 3 && s.ID != 4 && s.ID != 21 && s.ID != 22 && s.ID != 24).ToList();

                    //        }
                    //    }
                    //}
                }
            }
            catch { }
            finally { }
            return statusdetail;
        }
        #endregion

        #region Request Part Detail
        public List<POR_SP_GetPartDetail_ForRequest_Result> GetRequestPartDetailByRequestID(long RequestID, long siteID, string sessionID, string userID, string CurrentObject, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                List<POR_SP_GetPartDetail_ForRequest_Result> PartDetail = new List<POR_SP_GetPartDetail_ForRequest_Result>();
                PartDetail = (from sp in db.POR_SP_GetPartDetail_ForRequest("0", 0, siteID, RequestID)
                              select sp).ToList();
                SaveTempDataToDB(PartDetail, sessionID, userID, CurrentObject, conn);
                return PartDetail;
            }
        }

        protected void SaveTempDataToDB(List<POR_SP_GetPartDetail_ForRequest_Result> paraobjList, string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
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

        }

        public void ClearTempDataFromDB(string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                TempData tempdata = new TempData();
                tempdata = (from rec in db.TempDatas
                            where rec.SessionID == paraSessionID
                            && rec.UserID == paraUserID
                            && rec.ObjectName == paraCurrentObjectName
                            select rec).FirstOrDefault();
                if (tempdata != null) { db.DeleteObject(tempdata); db.SaveChanges(); }
            }
        }

        public void ClearTempDataFromDBNEW(string paraSessionID, string paraUserID, string CurrentObjectName, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                TempData tempdata = new TempData();
                tempdata = (from rec in db.TempDatas
                            where rec.UserID == paraUserID
                            && rec.ObjectName == CurrentObjectName
                            select rec).FirstOrDefault();
                if (tempdata != null) { db.DeleteObject(tempdata); db.SaveChanges(); }
            }
        }

        public List<POR_SP_GetPartDetail_ForRequest_Result> GetExistingTempDataBySessionIDObjectName(string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                List<POR_SP_GetPartDetail_ForRequest_Result> objtAddToCartProductDetailList = new List<POR_SP_GetPartDetail_ForRequest_Result>();

                TempData tempdata = new TempData();
                tempdata = (from temp in db.TempDatas
                            where temp.SessionID == paraSessionID
                            && temp.ObjectName == paraCurrentObjectName
                            && temp.UserID == paraUserID
                            select temp).FirstOrDefault();
                if (tempdata != null)
                {
                    objtAddToCartProductDetailList = datahelper.DeserializeEntity1<POR_SP_GetPartDetail_ForRequest_Result>(tempdata.Data);
                }
                return objtAddToCartProductDetailList;
            }
        }





        public List<POR_SP_GetPartDetail_ForRequest_Result> AddPartIntoRequest_TempData(string paraProductIDs, string paraSessionID, string paraUserID, string paraCurrentObjectName, long SiteID, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                /*Begin : Get Existing Records from TempData*/
                List<POR_SP_GetPartDetail_ForRequest_Result> existingList = new List<POR_SP_GetPartDetail_ForRequest_Result>();
                existingList = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, paraCurrentObjectName, conn);
                /*End*/
                long MaxSequenceNo = 0;
                if (existingList.Count > 0)
                {
                    MaxSequenceNo = Convert.ToInt64((from lst in existingList
                                                     select lst.Sequence).Max().Value);
                }

                /*Get Product Details*/
                List<POR_SP_GetPartDetail_ForRequest_Result> getnewRec = new List<POR_SP_GetPartDetail_ForRequest_Result>();
                getnewRec = (from view in db.POR_SP_GetPartDetail_ForRequest(paraProductIDs, MaxSequenceNo, SiteID, 0)
                             orderby view.Sequence
                             select view).ToList();
                /*End*/

                /*Begin : Merge (Existing + Newly Added) Products to Create TempData of AddToCart*/
                List<POR_SP_GetPartDetail_ForRequest_Result> mergedList = new List<POR_SP_GetPartDetail_ForRequest_Result>();
                mergedList.AddRange(existingList);
                mergedList.AddRange(getnewRec);
                /*End*/

                /*Begin : Serialize & Save MergedAddToCartList*/
                SaveTempDataToDB(mergedList, paraSessionID, paraUserID, paraCurrentObjectName, conn);
                /*End*/

                return mergedList;
            }
        }

        public List<POR_SP_GetPartDetail_ForRequest_Result> RemovePartFromRequest_TempData(string paraSessionID, string paraUserID, string paraCurrentObjectName, int paraSequence, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                /*Begin : Get Existing Records from TempData*/
                List<POR_SP_GetPartDetail_ForRequest_Result> existingList = new List<POR_SP_GetPartDetail_ForRequest_Result>();
                existingList = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, paraCurrentObjectName, conn);
                /*End*/

                /*Get Filter List [Filter By paraSequence]*/
                List<POR_SP_GetPartDetail_ForRequest_Result> filterList = new List<POR_SP_GetPartDetail_ForRequest_Result>();
                filterList = (from exist in existingList
                              where exist.Sequence != paraSequence
                              select exist).ToList();
                /*End*/

                /*Save result to TempData*/
                SaveTempDataToDB(filterList, paraSessionID, paraUserID, paraCurrentObjectName, conn);
                /*End*/

                /*Newly Added Code By Suresh For Update Sequene No After Remove Paart From List*/
                int cnt = filterList.Count;
                List<POR_SP_GetPartDetail_ForRequest_Result> NewList = new List<POR_SP_GetPartDetail_ForRequest_Result>();
                NewList = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, paraCurrentObjectName, conn);

                POR_SP_GetPartDetail_ForRequest_Result UpdRec = new POR_SP_GetPartDetail_ForRequest_Result();

                if (cnt > 0)
                {
                    for (int i = paraSequence; i <= cnt; i++)
                    {
                        UpdRec = NewList.Where(u => u.Sequence == i + 1).FirstOrDefault();

                        UpdRec.Sequence = i;
                        SaveTempDataToDB(NewList, paraSessionID, paraUserID, paraCurrentObjectName, conn);
                    }
                }
                /*End*/
                if (cnt > 0)
                { return NewList; }
                else
                { return filterList; }
            }
        }

        public void UpdatePartRequest_TempData(string SessionID, string CurrentObjectName, string UserID, POR_SP_GetPartDetail_ForRequest_Result Request, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                List<POR_SP_GetPartDetail_ForRequest_Result> getRec = new List<POR_SP_GetPartDetail_ForRequest_Result>();
                getRec = GetExistingTempDataBySessionIDObjectName(SessionID, UserID, CurrentObjectName, conn);

                POR_SP_GetPartDetail_ForRequest_Result updateRec = new POR_SP_GetPartDetail_ForRequest_Result();
                updateRec = getRec.Where(g => g.Sequence == Request.Sequence).FirstOrDefault();

                updateRec.RequestQty = Request.RequestQty;
                SaveTempDataToDB(getRec, SessionID, UserID, CurrentObjectName, conn);
            }
        }
        public void UpdatePartRequest_TempData1(string SessionID, string CurrentObjectName, string UserID, POR_SP_GetPartDetail_ForRequest_Result Request, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                List<POR_SP_GetPartDetail_ForRequest_Result> getRec = new List<POR_SP_GetPartDetail_ForRequest_Result>();
                getRec = GetExistingTempDataBySessionIDObjectName(SessionID, UserID, CurrentObjectName, conn);

                POR_SP_GetPartDetail_ForRequest_Result updateRec = new POR_SP_GetPartDetail_ForRequest_Result();
                updateRec = getRec.Where(g => g.Sequence == Request.Sequence).FirstOrDefault();

                updateRec.RequestQty = Request.RequestQty;
                updateRec.UOM = Request.UOM;
                updateRec.UOMID = Request.UOMID;
                updateRec.Total = Request.Total;
                SaveTempDataToDB(getRec, SessionID, UserID, CurrentObjectName, conn);
            }
        }

        public void UpdatePartRequest_TempData12(string SessionID, string CurrentObjectName, string UserID, POR_SP_GetPartDetail_ForRequest_Result Request, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                List<POR_SP_GetPartDetail_ForRequest_Result> getRec = new List<POR_SP_GetPartDetail_ForRequest_Result>();
                getRec = GetExistingTempDataBySessionIDObjectName(SessionID, UserID, CurrentObjectName, conn);

                POR_SP_GetPartDetail_ForRequest_Result updateRec = new POR_SP_GetPartDetail_ForRequest_Result();
                updateRec = getRec.Where(g => g.Sequence == Request.Sequence).FirstOrDefault();

                updateRec.RequestQty = Request.RequestQty; updateRec.UOM = Request.UOM;
                updateRec.UOMID = Request.UOMID;
                updateRec.Total = Request.Total;
                updateRec.Price = Request.Price;
                updateRec.IsPriceChange = Request.IsPriceChange;
                SaveTempDataToDB(getRec, SessionID, UserID, CurrentObjectName, conn);
            }
        }
        public int FinalSaveRequestPartDetail(string paraSessionID, string paraCurrentObjectName, long paraReferenceID, string paraUserID, int StatusID, long DepartmentID, long PreviousStatusID, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                var torderdetails = 0;
                int Result = 0;
                try
                {
                    if (PreviousStatusID == 2)
                    {
                        tOrderDetailHistory OD = new tOrderDetailHistory();
                        DataSet dsOldProducts = new DataSet();
                        dsOldProducts = fillds(" Select * from tOrderDetail where OrderHeadId=" + paraReferenceID + "", conn);
                        int CntPrds = dsOldProducts.Tables[0].Rows.Count;
                        if (CntPrds > 0)
                        {
                            for (int i = 0; i < CntPrds - 1; i++)
                            {
                                OD.OrderHeadId = paraReferenceID;
                                OD.SkuId = Convert.ToInt64(dsOldProducts.Tables[0].Rows[i]["SkuId"].ToString());
                                OD.OrderQty = Convert.ToDecimal(dsOldProducts.Tables[0].Rows[i]["OrderQty"].ToString());
                                OD.UOMID = Convert.ToInt64(dsOldProducts.Tables[0].Rows[i]["UOMID"].ToString());
                                OD.Sequence = Convert.ToInt64(dsOldProducts.Tables[0].Rows[i]["Sequence"].ToString());
                                OD.Prod_Name = dsOldProducts.Tables[0].Rows[i]["Prod_Name"].ToString();
                                OD.Prod_Description = dsOldProducts.Tables[0].Rows[i]["Prod_Description"].ToString();
                                OD.Prod_Code = dsOldProducts.Tables[0].Rows[i]["Prod_Code"].ToString();
                                OD.RemaningQty = Convert.ToDecimal(dsOldProducts.Tables[0].Rows[i]["RemainingQty"].ToString());

                                db.tOrderDetailHistories.Attach(OD);
                                db.SaveChanges();
                            }
                        }
                    }

                    List<POR_SP_GetPartDetail_ForRequest_Result> finalSaveLst = new List<POR_SP_GetPartDetail_ForRequest_Result>();
                    finalSaveLst = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, paraCurrentObjectName, conn);

                    XElement xmlEle = new XElement("Request", from rec in finalSaveLst
                                                              select new XElement("PartList",
                                                              new XElement("PRH_ID", paraReferenceID),
                                                              new XElement("Prod_ID", Convert.ToInt64(rec.Prod_ID)),
                                                              new XElement("Prod_Name", rec.Prod_Name),
                                                              new XElement("Prod_Description", rec.Prod_Description),
                                                              new XElement("RequestQty", Convert.ToDecimal(rec.RequestQty)),
                                                              new XElement("RemaningQty", Convert.ToDecimal(rec.RequestQty)),
                                                              new XElement("Sequence", Convert.ToInt64(rec.Sequence)),
                                                              new XElement("UOMID", Convert.ToInt64(rec.UOMID)),
                                                              new XElement("Price", Convert.ToDecimal(rec.Price)),
                                                              new XElement("VATPercent", Convert.ToDecimal(rec.VATPercent)),
                                                              new XElement("VATAmount", Convert.ToDecimal(rec.VATAmount)),
                                                              new XElement("Total", Convert.ToDecimal(rec.Total)),
                                                              new XElement("IsPriceChange", Convert.ToInt16(rec.IsPriceChange)),
                                                              new XElement("Prod_Code", rec.Prod_Code),
                                                               new XElement("SkuTotalAmt", rec.SkuTotalAmt),
                                                                new XElement("SKUTotalAmtExclVAT", rec.SKUTotalAmtExclVAT)
                                                              ));

                    ObjectParameter _PRH_ID = new ObjectParameter("PRH_ID", typeof(long));
                    _PRH_ID.Value = paraReferenceID;

                    ObjectParameter _xmlData = new ObjectParameter("xmlData", typeof(string));
                    _xmlData.Value = xmlEle.ToString();


                    ObjectParameter[] obj = new ObjectParameter[] { _PRH_ID, _xmlData };
                    //db.ExecuteFunction("POR_SP_InsertIntoPORtPartRequestDetail", obj);
                    db.ExecuteFunction("POR_SP_InsertIntotOrderDetail", obj);

                    db.SaveChanges(); torderdetails = 1; Result = 1;


                    addUpdateGrandTotal(paraReferenceID, conn);

                    /*Add Record Of User into table tOrderWiseAccess*/
                    DataSet dsChkRecordOfUser = new DataSet();
                    dsChkRecordOfUser = fillds("select * from tOrderwiseAccess  where orderID=" + paraReferenceID + " and Usertype='User'", conn);
                    if (dsChkRecordOfUser.Tables[0].Rows.Count == 0)
                    {
                        int IsPriceChenged = 0;
                        DataSet dsIsPriceChange = new DataSet();
                        dsIsPriceChange = fillds("select * from torderdetail where orderheadid=" + paraReferenceID + " and IsPriceChange=1", conn);
                        int rowcount = dsIsPriceChange.Tables[0].Rows.Count;
                        if (rowcount > 0)
                        { IsPriceChenged = 1; }
                        else { IsPriceChenged = 0; }

                        tOrderWiseAccess ODA = new tOrderWiseAccess();
                        ODA.UserApproverID = long.Parse(paraUserID);
                        ODA.ApprovalLevel = 0;

                        if (StatusID == 1) { }
                        else
                        {
                            if (IsPriceChenged == 1)
                            {
                                ODA.PriceEdit = false;
                                ODA.SkuQtyEdit = false;

                                /*Coading for New Status Pending For Financial Approver Start*/
                                tOrderHead OH = new tOrderHead();
                                OH = (from or in db.tOrderHeads
                                      where or.Id == paraReferenceID
                                      select or).FirstOrDefault();
                                OH.Status = 31; //Pending For Financial Approver
                                db.SaveChanges();
                                /*Coading for New Status Pending For Financial Approver End*/
                            }
                            else
                            {
                                ODA.PriceEdit = false;
                                ODA.SkuQtyEdit = true;
                            }
                            ODA.UserType = "User";
                            ODA.OrderID = paraReferenceID;
                            ODA.Date = DateTime.Now;
                            db.AddTotOrderWiseAccesses(ODA);
                            db.SaveChanges();
                        }
                    }
                    /*Add Record Of User into table tOrderWiseAccess*/
                    if (StatusID == 1) { }
                    else
                    {
                        tOrderDetail ODT = new tOrderDetail();
                        ODT = db.tOrderDetails.Where(r => r.OrderHeadId == paraReferenceID).FirstOrDefault();
                        if (ODT != null)
                        {
                            Result = SetApproverDataafterSave(paraCurrentObjectName, paraReferenceID, paraUserID, StatusID, DepartmentID, PreviousStatusID, conn);
                        }
                        else
                        {
                            long OrderID = paraReferenceID;
                            // RollBack(OrderID, conn);
                            Result = 0;
                        }
                    }
                    ClearTempDataFromDB(paraSessionID, paraUserID, paraCurrentObjectName, conn);
                    /*if (result == finalSaveLst.Count)
                    {

                    }*/
                }
                catch (System.Exception ex)
                {
                    if (torderdetails == 0)
                    {
                        long OrderID = paraReferenceID;
                        RollBack(OrderID, conn);
                        Result = 0;
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "Sp_EnterErrorTracking";
                            cmd.Connection = svr.GetSqlConn(conn);
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("Data", ex.Data.ToString());
                            cmd.Parameters.AddWithValue("GetType", ex.Source.ToString());
                            cmd.Parameters.AddWithValue("InnerException", "Error");
                            cmd.Parameters.AddWithValue("Message", ex.Message.ToString());
                            cmd.Parameters.AddWithValue("Source", "FinalSaveRequestPartDetail");
                            cmd.Parameters.AddWithValue("DateTime", DateTime.Now);
                            cmd.Parameters.AddWithValue("UserID", paraUserID);
                            cmd.ExecuteNonQuery();
                            cmd.Connection.Close();
                        }
                    }
                }
                finally { }
                return Result;
            }
        }


        public void addUpdateGrandTotal(long OrderHeadId, string[] conn)
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dt = new DataTable();
                ds = fillds("select sum(RemaningQty) totalOrder,sum(Total) grandTotal from tOrderDetail where OrderHeadId =" + OrderHeadId + "", conn);
                dt = ds.Tables[0];
                for (int i = 0; i <= ds.Tables[0].Rows.Count - 1; i++)
                {
                    using (SqlCommand cmd2 = new SqlCommand())
                    {
                        cmd2.CommandType = CommandType.StoredProcedure;
                        cmd2.CommandText = "SP_UpdateGrandTotalnQty";
                        cmd2.Connection = svr.GetSqlConn(conn);
                        cmd2.Parameters.Clear();
                        cmd2.Parameters.AddWithValue("ID", OrderHeadId);
                        cmd2.Parameters.AddWithValue("TotalQty", decimal.Parse(ds.Tables[0].Rows[i]["totalOrder"].ToString()));
                        cmd2.Parameters.AddWithValue("GrandTotal", decimal.Parse(ds.Tables[0].Rows[i]["grandTotal"].ToString()));
                        cmd2.ExecuteNonQuery();
                        cmd2.Connection.Close();
                    }
                }
            }
            catch (System.Exception ex)
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "Sp_EnterErrorTracking";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("Data", ex.Data.ToString());
                    cmd.Parameters.AddWithValue("GetType", ex.Source.ToString());
                    cmd.Parameters.AddWithValue("InnerException", "Error");
                    cmd.Parameters.AddWithValue("Message", ex.Message.ToString());
                    cmd.Parameters.AddWithValue("Source", "addUpdateGrandTotal");
                    cmd.Parameters.AddWithValue("DateTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("UserID", 123);
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
            }
            finally { }
        }

        public int SetApproverDataafterSave(string paraCurrentObjectName, long paraReferenceID, string paraUserID, int StatusID, long DepartmentID, long PreviousStatusID, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                var ApprovalDetail = 0;
                string autoapproval = "False";
                int Rslt = 0, Suecc = 0;
                try
                {
                    if (PreviousStatusID == 2) { }
                    else
                    {
                        if (StatusID == 2)
                        {
                            /* Insert record of Approval Level 1 in tApprovalTrans Table===>>>> */

                            /* If Price is change of product then add Financial Approver at Approval Level 1  START*/
                            /*Check if Price is Changed or not*/
                            int IsPriceChenged = 0;
                            DataSet dsIsPriceChange = new DataSet();
                            dsIsPriceChange = fillds("select * from torderdetail where orderheadid=" + paraReferenceID + " and IsPriceChange=1", conn);
                            int rowcount = dsIsPriceChange.Tables[0].Rows.Count;
                            if (rowcount > 0)
                            {
                                IsPriceChenged = 1; long FinanAppId = 0;
                                DataSet dsFinanAppID = new DataSet();
                                dsFinanAppID = fillds("select FinApproverID from mterritory where id=" + DepartmentID + "", conn);
                                FinanAppId = Convert.ToInt64(dsFinanAppID.Tables[0].Rows[0]["FinApproverID"].ToString());

                                using (SqlCommand cmd = new SqlCommand())
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.CommandText = "SP_Insert_tapprovaltrans";
                                    cmd.Connection = svr.GetSqlConn(conn);
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("OrderId", paraReferenceID);
                                    cmd.Parameters.AddWithValue("StoreId", DepartmentID);
                                    cmd.Parameters.AddWithValue("UserId", Convert.ToInt64(paraUserID));
                                    cmd.Parameters.AddWithValue("ApprovalId", 1);
                                    cmd.Parameters.AddWithValue("ApproverID", FinanAppId);
                                    cmd.Parameters.AddWithValue("Status", StatusID);
                                    cmd.ExecuteNonQuery();
                                    cmd.Connection.Close();
                                }



                                /*Add Record Of User into table tOrderWiseAccess*/
                                tOrderWiseAccess ODA = new tOrderWiseAccess();
                                ODA.UserApproverID = FinanAppId;
                                ODA.ApprovalLevel = 1;
                                ODA.PriceEdit = true;
                                ODA.SkuQtyEdit = false;
                                ODA.UserType = "Financial Approver";
                                ODA.OrderID = paraReferenceID;
                                ODA.Date = DateTime.Now;
                                ODA.ApproverLogic = "AND";
                                db.AddTotOrderWiseAccesses(ODA);
                                db.SaveChanges();

                                // Check for ERP Auto Approval Department***

                                autoapproval = CheckStoreERPAutoApproved(DepartmentID, conn);
                                if (autoapproval != "True")
                                {
                                    /*Add Record Of User into table tOrderWiseAccess*/
                                    AddAllApprovalLevel(IsPriceChenged, paraReferenceID, DepartmentID, conn);
                                }


                                Rslt = EmailSendToApprover(FinanAppId, paraReferenceID, 0, conn);
                            }
                            else
                            {
                                // Check for ERP Auto Approval Department***

                                autoapproval = CheckStoreERPAutoApproved(DepartmentID, conn);
                                if (autoapproval != "True")
                                {

                                    IsPriceChenged = 0;
                                    /* If Price is change of product then add Financial Approver at Approval Level 1  END*/
                                    /*Add Record Of User into table tOrderWiseAccess*/
                                    AddAllApprovalLevel(IsPriceChenged, paraReferenceID, DepartmentID, conn);

                                    /*New Code After tOrderWiseAccess able added for order wise approval level start*/
                                    DataSet dsFirstApprover = new DataSet();
                                    dsFirstApprover = fillds("select OW.ID,OW.UserApproverID,OW.ApprovalLevel,OW.PriceEdit,OW.SkuQtyEdit,OW.UserType,OW.OrderID,OW.ApproverLogic ,Dl.DeligateTo from tOrderWiseAccess  OW left outer join tOrderHead OH on OW.OrderID=OH.ID left outer join mDeligate AS Dl ON OW.UserApproverID = Dl.DeligateFrom and CONVERT(VARCHAR(10), GETDATE(), 111) <=Convert(VARCHAR(10), Dl.ToDate,111) and CONVERT(VARCHAR(10), GETDATE(), 111) >=Convert(VARCHAR(10), Dl.FromDate,111)  and OH.StoreId=Dl.DeptID where OW.OrderID=" + paraReferenceID + " and OW.UserType != 'User' and OW.ApprovalLevel=1", conn);
                                    int CntFirstApprover = dsFirstApprover.Tables[0].Rows.Count;
                                    if (CntFirstApprover > 0)
                                    {
                                        for (int i = 0; i <= CntFirstApprover - 1; i++)
                                        {
                                            using (SqlCommand cmd1 = new SqlCommand())
                                            {
                                                cmd1.CommandType = CommandType.StoredProcedure;
                                                cmd1.CommandText = "SP_Insert_tapprovaltrans";
                                                cmd1.Connection = svr.GetSqlConn(conn);
                                                cmd1.Parameters.Clear();
                                                cmd1.Parameters.AddWithValue("OrderId", paraReferenceID);
                                                cmd1.Parameters.AddWithValue("StoreId", DepartmentID);
                                                cmd1.Parameters.AddWithValue("UserId", Convert.ToInt64(paraUserID));
                                                cmd1.Parameters.AddWithValue("ApprovalId", 1);
                                                cmd1.Parameters.AddWithValue("ApproverID", Convert.ToInt64(dsFirstApprover.Tables[0].Rows[i]["UserApproverID"].ToString()));
                                                cmd1.Parameters.AddWithValue("Status", StatusID);
                                                cmd1.ExecuteNonQuery();
                                            }

                                            ApprovalDetail = 1;

                                            /*Send Email to Approvers*/
                                            Rslt = EmailSendToApprover(Convert.ToInt64(dsFirstApprover.Tables[0].Rows[i]["UserApproverID"].ToString()), paraReferenceID, i, conn);
                                        }
                                    }
                                }
                                else
                                {
                                    // If auto approval Yes send to ERP API for Approval


                                    /* Update tProductstockDetails Reserve Quantity & Available Balance START>>>>>>>> */
                                    UpdateTProductStockReserveQtyAvailBalance(paraReferenceID, conn);
                                    /* <<<<<<<<Update tProductstockDetails Reserve Quantity & Available Balance END */
                                    /*Send Email to Approvers*/
                                   

                                }



                            }
                            /*New Code After tOrderWiseAccess able added for order wise approval level end*/

                            tApprovalTran APRT = new tApprovalTran();
                            APRT = db.tApprovalTrans.Where(rec => rec.OrderId == paraReferenceID).FirstOrDefault();
                            if (APRT != null)
                            {

                                Rslt = EmailSendWhenRequestSubmit(paraReferenceID, conn); //if Rslt =2 then mail sent to requestor else if Rslt=3 then mail not sent to requestoe
                                                                                          /*Insert record of Auto Cancellation Reminder & Approval reminder in tCorrespond table START*/
                                mTerritory Dept = new mTerritory();
                                Dept = db.mTerritories.Where(r => r.ID == DepartmentID).FirstOrDefault();
                                long OrderCancelDays = 0, ApprovalReminderSchedule = 0, AutoCancelReminderSchedule = 0;
                                if (Dept != null)
                                {
                                    OrderCancelDays = long.Parse(Dept.cancelDays.ToString());
                                    ApprovalReminderSchedule = long.Parse(Dept.ApproRemSchedul.ToString());
                                    AutoCancelReminderSchedule = long.Parse(Dept.AutoRemSchedule.ToString());
                                }

                                DataSet dsGetOrderDate = new DataSet();
                                dsGetOrderDate = fillds("select OrderDate from torderhead where id=" + paraReferenceID + "", conn);
                                DateTime OrdrDate = Convert.ToDateTime(dsGetOrderDate.Tables[0].Rows[0]["OrderDate"].ToString());

                                DataSet dsAutocancel = new DataSet();
                                dsAutocancel = fillds("select * from mMessageEMailTemplates where ActivityID=(select Id from mDropdownValues where StatusID=10) and MessageID=(select Id from mDropdownValues where Value='Reminder') and DepartmentID=" + DepartmentID + " ", conn);
                                if (dsAutocancel.Tables[0].Rows.Count > 0)
                                {
                                    tCorrespond Cor = new tCorrespond();
                                    Cor.OrderHeadId = paraReferenceID;
                                    Cor.MessageTitle = dsAutocancel.Tables[0].Rows[0]["MailSubject"].ToString();
                                    Cor.Message = dsAutocancel.Tables[0].Rows[0]["MailBody"].ToString();
                                    Cor.date = DateTime.Now;
                                    Cor.MessageSource = "Scheduler";
                                    Cor.MessageType = "Reminder";
                                    Cor.DepartmentID = DepartmentID;
                                    // Cor.OrderDate = DateTime.Now;
                                    Cor.OrderDate = OrdrDate;
                                    Cor.CurrentOrderStatus = StatusID;
                                    Cor.MailStatus = 0;
                                    Cor.OrderCancelDays = OrderCancelDays;
                                    Cor.AutoCancelReminderSchedule = AutoCancelReminderSchedule;
                                    //Cor.ApprovalReminderSchedule = ApprovalReminderSchedule;
                                    Cor.NxtAutoCancelReminderDate = DateTime.Now.AddDays(AutoCancelReminderSchedule);
                                    Cor.Archive = false;

                                    db.tCorresponds.AddObject(Cor);
                                    db.SaveChanges();
                                }

                                DataSet dsApprovalReminder = new DataSet();
                                dsApprovalReminder = fillds("select * from mMessageEMailTemplates where ActivityID=(select Id from mDropdownValues where StatusID=2 and value='Order Submit') and MessageID=(select Id from mDropdownValues where Value='Reminder') and DepartmentID=" + DepartmentID + " ", conn);
                                if (dsApprovalReminder.Tables[0].Rows.Count > 0)
                                {
                                    tCorrespond Cor = new tCorrespond();
                                    Cor.OrderHeadId = paraReferenceID;
                                    Cor.MessageTitle = dsApprovalReminder.Tables[0].Rows[0]["MailSubject"].ToString();
                                    Cor.Message = dsApprovalReminder.Tables[0].Rows[0]["MailBody"].ToString();
                                    Cor.date = DateTime.Now;
                                    Cor.MessageSource = "Scheduler";
                                    Cor.MessageType = "Reminder";
                                    Cor.DepartmentID = DepartmentID;
                                    Cor.OrderDate = OrdrDate; //DateTime.Now;
                                    Cor.CurrentOrderStatus = StatusID;
                                    Cor.MailStatus = 0;
                                    // Cor.OrderCancelDays = OrderCancelDays;
                                    // Cor.AutoCancelReminderSchedule = AutoCancelReminderSchedule;
                                    Cor.ApprovalReminderSchedule = ApprovalReminderSchedule;
                                    Cor.NxtApprovalReminderDate = DateTime.Now.AddDays(ApprovalReminderSchedule);
                                    Cor.Archive = false;

                                    db.tCorresponds.AddObject(Cor);
                                    db.SaveChanges();
                                }


                                /* Update tProductstockDetails Reserve Quantity & Available Balance START>>>>>>>> */
                                UpdateTProductStockReserveQtyAvailBalance(paraReferenceID, conn);
                                /* <<<<<<<<Update tProductstockDetails Reserve Quantity & Available Balance END */
                                Suecc = 1;
                            }
                            else
                            {
                                long OrdrID = paraReferenceID;

                                if (autoapproval == "True")
                                {
                                    Suecc = 1;
                                }
                                else
                                {
                                    Suecc = 0;
                                    RollBack(OrdrID, conn);
                                }

                            }

                        }
                        else if (StatusID == 3)
                        {
                            //Add Message into mMessageTrans Table After Approve Order

                            DataSet dsChkOredrIsEcomOrNot = new DataSet();
                            dsChkOredrIsEcomOrNot = fillds("select * from tOrderHead where orderType in('PreOrder','NormalOrder') and id=" + paraReferenceID + " ", conn);
                            if (dsChkOredrIsEcomOrNot.Tables[0].Rows.Count > 0)
                            {
                               // AddIntomMessageTransForEcommerce(paraReferenceID, conn);
                                UpdateIseApprflag(paraReferenceID, conn);
                            }
                            else
                            {

                                AddIntomMessageTrans(paraReferenceID, conn);
                                //UpdateIseApprflag(paraReferenceID, conn);
                            }
                             AddIntomMessageTrans(paraReferenceID, conn);
                            //UpdateIseApprflag(paraReferenceID, conn);
                        }
                        else
                        {
                        }
                    }
                }

                catch (System.Exception ex)
                {
                    if (ApprovalDetail == 0)
                    {
                        long OrdrID = paraReferenceID;
                        RollBack(OrdrID, conn);
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "Sp_EnterErrorTracking";
                            cmd.Connection = svr.GetSqlConn(conn);
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("Data", ex.Data.ToString());
                            cmd.Parameters.AddWithValue("GetType", ex.Source.ToString());
                            cmd.Parameters.AddWithValue("InnerException", "Error");
                            cmd.Parameters.AddWithValue("Message", ex.Message.ToString());
                            cmd.Parameters.AddWithValue("Source", "SetApproverDataafterSave");
                            cmd.Parameters.AddWithValue("DateTime", DateTime.Now);
                            cmd.Parameters.AddWithValue("UserID", paraUserID);
                            cmd.ExecuteNonQuery();
                            cmd.Connection.Close();
                        }
                    }
                }
                finally { }

                //ClearTempDataFromDB(paraSessionID, paraUserID, paraCurrentObjectName, conn);
                return Suecc;
            }
        }

        protected void AddAllApprovalLevel(int IsPriceChenged, long paraReferenceID, long DepartmentID, string[] conn)
        {
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                DataSet dsApproverUserID = new DataSet();
                dsApproverUserID = fillds("select VWA.ID,VWA.ApprovalLevelID,VWA.UserID,VWA.ApproverName,VWA.EmailID,VWA.CompanyID,VWA.DepartmentID,VWA.ApproverLogic,VWA.ApprovalLevel,dl.DeligateTo from VW_ApprovalLevelDetail VWA left outer join mDeligate AS Dl ON VWA.UserID = Dl.DeligateFrom AND CONVERT(VARCHAR(10), GETDATE(), 111) <=Convert(VARCHAR(10), Dl.ToDate,111) and  CONVERT(VARCHAR(10), GETDATE(), 111) >=Convert(VARCHAR(10), Dl.FromDate,111) AND VWA.DepartmentID = Dl.DeptID where VWA.DepartmentID=" + DepartmentID + " order by VWA.ApprovalLevel", conn);
                int cnt = dsApproverUserID.Tables[0].Rows.Count;
                if (cnt > 0)
                {
                    for (int i = 0; i <= cnt - 1; i++)
                    {
                        int AppLvl = Convert.ToInt16(dsApproverUserID.Tables[0].Rows[i]["ApprovalLevel"].ToString());
                        if (IsPriceChenged == 1) { AppLvl = AppLvl + 1; }

                        SqlCommand cmd2 = new SqlCommand();
                        cmd2.CommandType = CommandType.StoredProcedure;
                        cmd2.CommandText = "SP_Insert_tOrderWiseAccess";
                        cmd2.Connection = svr.GetSqlConn(conn);
                        cmd2.Parameters.Clear();
                        cmd2.Parameters.AddWithValue("UserApproverID", Convert.ToInt64(dsApproverUserID.Tables[0].Rows[i]["UserID"].ToString()));
                        cmd2.Parameters.AddWithValue("ApprovalLevel", AppLvl);
                        cmd2.Parameters.AddWithValue("PriceEdit", false);
                        cmd2.Parameters.AddWithValue("SkuQtyEdit", true);
                        cmd2.Parameters.AddWithValue("UserType", "General Approver");
                        cmd2.Parameters.AddWithValue("OrderID", paraReferenceID);
                        cmd2.Parameters.AddWithValue("ApproverLogic", dsApproverUserID.Tables[0].Rows[i]["ApproverLogic"].ToString());
                        cmd2.ExecuteNonQuery();
                    }
                }

                //SqlCommand cmd3 = new SqlCommand();
                //cmd3.CommandType = CommandType.StoredProcedure;
                //cmd3.CommandText = "SP_PaymentMethodFOC";
                //cmd3.Connection = svr.GetSqlConn(conn);
                //cmd3.Parameters.Clear();
                //cmd3.Parameters.AddWithValue("OrderID", paraReferenceID);
                //cmd3.ExecuteNonQuery();
            }
            catch (System.Exception ex)
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "Sp_EnterErrorTracking";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("Data", ex.Data.ToString());
                cmd.Parameters.AddWithValue("GetType", ex.Source.ToString());
                cmd.Parameters.AddWithValue("InnerException", "Error");
                cmd.Parameters.AddWithValue("Message", ex.Message.ToString());
                cmd.Parameters.AddWithValue("Source", "AddAllApprovalLevel");
                cmd.Parameters.AddWithValue("DateTime", DateTime.Now);
                cmd.Parameters.AddWithValue("UserID", 123);
                cmd.ExecuteNonQuery();
            }
            finally { }

        }

        protected void RollBack(long OrderID, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                try
                {
                    /*Update tProductStockDetails Start*/
                    DataSet dsPrdDetails = new DataSet();
                    dsPrdDetails = fillds("Select * from tOrderDetail where OrderHeadId=" + OrderID + "", conn);
                    int PrdCnt = dsPrdDetails.Tables[0].Rows.Count;
                    //if (PrdCnt > 0)
                    //{
                    //    for (int i = 0; i <= PrdCnt - 1; i++)
                    //    {
                    //        long SkuID = long.Parse(dsPrdDetails.Tables[0].Rows[i]["SkuId"].ToString());
                    //        decimal Qty = decimal.Parse(dsPrdDetails.Tables[0].Rows[i]["OrderQty"].ToString());

                    //        mProduct prd = new mProduct();
                    //        prd = db.mProducts.Where(p => p.ID == SkuID).FirstOrDefault();
                    //        if (prd.GroupSet == "Yes")
                    //        {
                    //            DataSet dsBomProds = new DataSet();
                    //            dsBomProds = fillds("select SKUId,Quantity from mBOMDetail BD where BD.BOMheaderId=" + SkuID + "", conn);
                    //            if (dsBomProds.Tables[0].Rows.Count > 0)
                    //            {
                    //                for (int b = 0; b <= dsBomProds.Tables[0].Rows.Count - 1; b++)
                    //                {
                    //                    long bomPrd = long.Parse(dsBomProds.Tables[0].Rows[b]["SKUId"].ToString());
                    //                    decimal bomQty = decimal.Parse(dsBomProds.Tables[0].Rows[b]["Quantity"].ToString());

                    //                    decimal FinalQty = Qty * bomQty;

                    //                    SqlCommand cmd = new SqlCommand();
                    //                    cmd.CommandType = CommandType.StoredProcedure;
                    //                    cmd.CommandText = "GWC_SP_UpdatetProductstockDetailsAvailableResurveQtyRej";
                    //                    cmd.Connection = svr.GetSqlConn(conn);
                    //                    cmd.Parameters.Clear();
                    //                    cmd.Parameters.AddWithValue("SkuID", bomPrd);
                    //                    cmd.Parameters.AddWithValue("Qty", FinalQty);
                    //                    cmd.ExecuteNonQuery();
                    //                }
                    //            }
                    //        }
                    //        else if (prd.GroupSet == "No")
                    //        {
                    //            SqlCommand cmd = new SqlCommand();
                    //            cmd.CommandType = CommandType.StoredProcedure;
                    //            cmd.CommandText = "GWC_SP_UpdatetProductstockDetailsAvailableResurveQtyRej";
                    //            cmd.Connection = svr.GetSqlConn(conn);
                    //            cmd.Parameters.Clear();
                    //            cmd.Parameters.AddWithValue("SkuID", SkuID);
                    //            cmd.Parameters.AddWithValue("Qty", Qty);
                    //            cmd.ExecuteNonQuery();
                    //        }
                    //    }
                    //}
                    /*Update tProductStockDetails End*/

                    /* Delete Record From tCorrespond,tApprovalTrans,tOrderDetail,tOrderHead Start*/
                    using (SqlCommand cmdR = new SqlCommand())
                    {
                        cmdR.CommandType = CommandType.StoredProcedure;
                        cmdR.CommandText = "GWC_SP_RollBack";
                        cmdR.Connection = svr.GetSqlConn(conn);
                        cmdR.Parameters.Clear();
                        cmdR.Parameters.AddWithValue("OrderID", OrderID);
                        cmdR.ExecuteNonQuery();
                        cmdR.Connection.Close();
                    }
                    /* Delete Record From tCorrespond,tApprovalTrans,tOrderDetail,tOrderHead End*/
                }
                catch (System.Exception ex) { }
                finally { }
            }
        }

        protected void UpdateTProductStockReserveQtyAvailBalance(long RequestID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            DataSet dsHstry = new DataSet();
            dsHstry = fillds(" select * from tOrderDetailHistory where OrderHeadId=" + RequestID + "", conn);
            int CntHstry = dsHstry.Tables[0].Rows.Count;
            if (CntHstry > 0)
            {
                for (int j = 0; j < CntHstry - 1; j++)
                {
                    long HstrySkuID = long.Parse(dsHstry.Tables[0].Rows[j]["SkuId"].ToString());
                    decimal HstryQty = decimal.Parse(dsHstry.Tables[0].Rows[j]["OrderQty"].ToString());

                    decimal FnlQty = 0;
                    DataSet dsNewPrdLst = new DataSet();
                    dsNewPrdLst = fillds("Select * from tOrderDetail where OrderHeadId=" + RequestID + " and SkuId=" + HstrySkuID + "", conn);
                    if (dsNewPrdLst.Tables[0].Rows.Count > 0)
                    {
                        decimal NewAdSkuQty = decimal.Parse(dsNewPrdLst.Tables[0].Rows[0]["OrderQty"].ToString());

                        FnlQty = HstryQty - NewAdSkuQty;
                    }

                    tProductStockDetail psd1 = new tProductStockDetail();
                    psd1 = db.tProductStockDetails.Where(a => a.ProdID == HstrySkuID).FirstOrDefault();
                    if (psd1 != null)
                    {
                        db.tProductStockDetails.Detach(psd1);
                        psd1.ResurveQty = psd1.ResurveQty + FnlQty;
                        psd1.AvailableBalance = psd1.AvailableBalance - FnlQty;
                        db.tProductStockDetails.Attach(psd1);
                        db.ObjectStateManager.ChangeObjectState(psd1, EntityState.Modified);
                        db.SaveChanges();
                    }

                }
            }
            else
            {
                DataSet dsPrdDetails = new DataSet();
                dsPrdDetails = fillds("Select * from tOrderDetail where OrderHeadId=" + RequestID + "", conn);
                int PrdCnt = dsPrdDetails.Tables[0].Rows.Count;
                if (PrdCnt > 0)
                {
                    for (int i = 0; i <= PrdCnt - 1; i++)
                    {
                        //update tProductstockDetails set ResurveQty=ResurveQty+@Qty,AvailableBalance=AvailableBalance-@Qty where ProdID=@Prd ;

                        //tProductStockDetail psd = new tProductStockDetail();
                        long SkuID = long.Parse(dsPrdDetails.Tables[0].Rows[i]["SkuId"].ToString());
                        decimal Qty = decimal.Parse(dsPrdDetails.Tables[0].Rows[i]["OrderQty"].ToString());
                        //psd = db.tProductStockDetails.Where(a => a.ProdID == SkuID).FirstOrDefault();
                        //if (psd != null)
                        //{
                        //    db.tProductStockDetails.Detach(psd);
                        //    psd.ResurveQty = psd.ResurveQty + Qty;
                        //    psd.AvailableBalance = psd.AvailableBalance - Qty;
                        //    db.tProductStockDetails.Attach(psd);
                        //    db.ObjectStateManager.ChangeObjectState(psd, EntityState.Modified);
                        //    db.SaveChanges();
                        //}

                        //SqlCommand cmd = new SqlCommand();
                        //cmd.CommandType = CommandType.StoredProcedure;
                        //cmd.CommandText = "GWC_SP_UpdatetProductstockDetailsAvailableResurveQty";
                        //cmd.Connection = svr.GetSqlConn(conn);
                        //cmd.Parameters.Clear();
                        //cmd.Parameters.AddWithValue("SkuID", SkuID);
                        //cmd.Parameters.AddWithValue("Qty", Qty);                        
                        //cmd.ExecuteNonQuery();
                        mProduct prd = new mProduct();
                        prd = db.mProducts.Where(p => p.ID == SkuID).FirstOrDefault();
                        if (prd.GroupSet == "Yes")
                        {
                            DataSet dsBomProds = new DataSet();
                            dsBomProds = fillds("select SKUId,Quantity from mBOMDetail BD where BD.BOMheaderId=" + SkuID + "", conn);
                            if (dsBomProds.Tables[0].Rows.Count > 0)
                            {
                                for (int b = 0; b <= dsBomProds.Tables[0].Rows.Count - 1; b++)
                                {
                                    long bomPrd = long.Parse(dsBomProds.Tables[0].Rows[b]["SKUId"].ToString());
                                    decimal bomQty = decimal.Parse(dsBomProds.Tables[0].Rows[b]["Quantity"].ToString());

                                    decimal FinalQty = Qty * bomQty;

                                    SqlCommand cmd = new SqlCommand();
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.CommandText = "GWC_SP_UpdatetProductstockDetailsAvailableResurveQty";
                                    cmd.Connection = svr.GetSqlConn(conn);
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("SkuID", bomPrd);
                                    cmd.Parameters.AddWithValue("Qty", FinalQty);
                                    cmd.ExecuteNonQuery();

                                    // InsertIntotInventory(bomPrd, RequestID, FinalQty, "Dispatch", conn);
                                }
                            }
                        }
                        else if (prd.GroupSet == "No")
                        {
                            SqlCommand cmd = new SqlCommand();
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "GWC_SP_UpdatetProductstockDetailsAvailableResurveQty";
                            cmd.Connection = svr.GetSqlConn(conn);
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("SkuID", SkuID);
                            cmd.Parameters.AddWithValue("Qty", Qty);
                            cmd.ExecuteNonQuery();

                            //InsertIntotInventory(SkuID, RequestID, Qty, "Dispatch", conn);
                        }
                    }
                }
            }
        }

        #endregion

        #region GridRequestList Summary
        public List<POR_SP_GetRequestBySiteIDsOrUserID_Result> GetRequestSummayByUserID(long UserID, string[] conn, string Ordtype)
        {
            List<POR_SP_GetRequestBySiteIDsOrUserID_Result> RequestList = new List<POR_SP_GetRequestBySiteIDsOrUserID_Result>();
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));

                RequestList = db.POR_SP_GetRequestBySiteIDsOrUserID("0", UserID, Ordtype).OrderByDescending(o => o.ID).ToList();

            }
            catch { }
            finally { }
            return RequestList;
        }

        public DataSet GetRequestSummayByUserIDNew(long UserID, string[] conn, string Ordtype)
        {
            DataSet ds = new DataSet();

            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "POR_SP_GetRequestBySiteIDsOrUserID";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@SiteIDs", "0");
                cmd.Parameters.AddWithValue("@UserID", UserID);
                cmd.Parameters.AddWithValue("@OrdType", Ordtype);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;

            }
        }
        public List<POR_SP_GetRequestByForAdvanceSearch_Result> GetRequestSummayByUserIDAdvanceSearch(string fdate, string todate, string ordcat, string ordno, string lcode, string passport, string ordtype, string misidn, long UserID, string Invoker, string[] conn)
        {
            List<POR_SP_GetRequestByForAdvanceSearch_Result> RequestList = new List<POR_SP_GetRequestByForAdvanceSearch_Result>();
            try
            {
                if (ordtype == "0" || ordtype == "") { ordtype = ""; }
                if (ordno == "0" || ordno == "") { ordno = ""; }
                if (passport == "0" || passport == "") { passport = ""; }
                if (ordcat == "0" || ordcat == "") { ordcat = ""; }
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                RequestList = db.POR_SP_GetRequestByForAdvanceSearch(Convert.ToDateTime(fdate), Convert.ToDateTime(todate), ordcat, ordno, lcode, passport, ordtype, misidn, UserID, Invoker).OrderByDescending(o => o.ID).ToList();
                RequestList = RequestList.Where(r => r.ImgIssue == "green").ToList();

            }
            catch { }
            finally { }
            return RequestList;
        }

        //public DataSet GetRequestSummayByUserIDAdvanceSearch(string fdate, string todate, string ordcat, string ordno, string lcode, string passport, string ordtype, string misidn, long UserID, string[] conn)
        //{
        //    DataSet ds = new DataSet();
        //    SqlCommand cmd = new SqlCommand();
        //    SqlDataAdapter da = new SqlDataAdapter();
        //    cmd.CommandType = CommandType.StoredProcedure;
        //    cmd.CommandText = "POR_SP_GetRequestByForAdvanceSearch";
        //    cmd.Connection = svr.GetSqlConn(conn);
        //    cmd.Parameters.Clear();
        //    cmd.Parameters.AddWithValue("@Fdate", fdate);
        //    cmd.Parameters.AddWithValue("@Tdate", todate);
        //    cmd.Parameters.AddWithValue("@Ordcat", ordcat);
        //    cmd.Parameters.AddWithValue("@OrdNo", ordno);
        //    cmd.Parameters.AddWithValue("@Lcode", lcode);
        //    cmd.Parameters.AddWithValue("@Passport", passport);
        //    cmd.Parameters.AddWithValue("@OrdType", ordtype);
        //    cmd.Parameters.AddWithValue("@UserID", UserID);
        //    cmd.Parameters.AddWithValue("@Invoker", "Request");
        //    da.SelectCommand = cmd;
        //    da.Fill(ds);
        //    return ds;
        //}



        public List<POR_SP_GetRequestByForAdvanceSearch_Result> GetRequestSummayByUserIDIssueAdvanceSearch(string fdate, string todate, string ordcat, string ordno, string lcode, string passport, string ordtype, string misidn, long UserID, string Invoker, string[] conn)
        {
            List<POR_SP_GetRequestByForAdvanceSearch_Result> RequestList = new List<POR_SP_GetRequestByForAdvanceSearch_Result>();
            try
            {
                if (ordtype == "0" || ordtype == "") { ordtype = ""; }
                if (ordno == "0" || ordno == "") { ordno = ""; }
                if (passport == "0" || passport == "") { passport = ""; }
                if (ordcat == "0" || ordcat == "") { ordcat = ""; }
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                RequestList = db.POR_SP_GetRequestByForAdvanceSearch(Convert.ToDateTime(fdate), Convert.ToDateTime(todate), ordcat, ordno, lcode, passport, ordtype, misidn, UserID, Invoker).OrderByDescending(o => o.ID).ToList();
                RequestList = RequestList.Where(r => r.ImgIssue == "green").ToList();

            }
            catch { }
            finally { }
            return RequestList;
        }



        public List<POR_SP_GetRequestBySiteIDsOrUserID_Result> GetRequestSummayByUserIDIssue(long UserID, string[] conn, string Ordtype)
        {
            List<POR_SP_GetRequestBySiteIDsOrUserID_Result> RequestList = new List<POR_SP_GetRequestBySiteIDsOrUserID_Result>();
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                RequestList = db.POR_SP_GetRequestBySiteIDsOrUserID("0", UserID, Ordtype).OrderByDescending(o => o.ID).ToList();
                RequestList = RequestList.Where(r => r.ImgIssue == "green").ToList();

            }
            catch { }
            finally { }
            return RequestList;
        }


        //public DataSet GetRequestSummayByUserIDIssueAdvanceSearch(string fdate, string todate, string ordcat, string ordno, string lcode, string passport, string ordtype, string misidn, long UserID, string[] conn)
        //{
        //    DataSet ds = new DataSet();
        //    SqlCommand cmd = new SqlCommand();
        //    SqlDataAdapter da = new SqlDataAdapter();
        //    cmd.CommandType = CommandType.StoredProcedure;
        //    cmd.CommandText = "POR_SP_GetRequestByForAdvanceSearch";
        //    cmd.Connection = svr.GetSqlConn(conn);
        //    cmd.Parameters.Clear();
        //    cmd.Parameters.AddWithValue("@Fdate", fdate);
        //    cmd.Parameters.AddWithValue("@Tdate", todate);
        //    cmd.Parameters.AddWithValue("@Ordcat", ordcat);
        //    cmd.Parameters.AddWithValue("@OrdNo", ordno);
        //    cmd.Parameters.AddWithValue("@Lcode", lcode);
        //    cmd.Parameters.AddWithValue("@Passport", passport);
        //    cmd.Parameters.AddWithValue("@OrdType", ordtype);
        //    cmd.Parameters.AddWithValue("@UserID", UserID);
        //    cmd.Parameters.AddWithValue("@Invoker", "Issue");
        //    da.SelectCommand = cmd;
        //    da.Fill(ds);
        //    return ds;
        //}


        public List<POR_SP_GetRequestBySiteIDsOrUserID_Result> GetRequestSummayOfUser(long UserID, string[] conn, string Ordtype)
        {
            List<POR_SP_GetRequestBySiteIDsOrUserID_Result> RequestList = new List<POR_SP_GetRequestBySiteIDsOrUserID_Result>();
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                RequestList = db.POR_SP_GetRequestBySiteIDsOrUserID("0", UserID, Ordtype).OrderByDescending(o => o.ID).ToList();
                RequestList = RequestList.Where(r => r.RequestBy == UserID).ToList();
            }
            catch { }
            finally { }
            return RequestList;
        }






        public List<POR_SP_GetRequestBySiteIDsOrUserID_Result> GetRequestSummayOfRetailUser(long UserID, string[] conn, string Ordtype)
        {
            List<POR_SP_GetRequestBySiteIDsOrUserID_Result> RequestList = new List<POR_SP_GetRequestBySiteIDsOrUserID_Result>();
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                RequestList = db.POR_SP_GetRequestBySiteIDsOrUserID("0", UserID, Ordtype).OrderByDescending(o => o.ID).ToList();
                RequestList = RequestList.Where(r => r.ImgIssue == "green").ToList();
            }
            catch { }
            finally { }
            return RequestList;
        }

        public List<POR_SP_GetRequestBySiteIDsOrUserID_Result> GetRequestSummayOfUserIssue(long UserID, string[] conn, string Ordtype)
        {
            List<POR_SP_GetRequestBySiteIDsOrUserID_Result> RequestList = new List<POR_SP_GetRequestBySiteIDsOrUserID_Result>();
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                RequestList = db.POR_SP_GetRequestBySiteIDsOrUserID("0", UserID, Ordtype).OrderByDescending(o => o.ID).ToList();
                RequestList = RequestList.Where(r => r.ImgIssue == "green").ToList();
                RequestList = RequestList.Where(r => r.RequestBy == UserID).ToList();
            }
            catch { }
            finally { }
            return RequestList;
        }


        public List<POR_SP_GetRequestBySiteIDsOrUserID_Result> GetRequestSummayBySiteIDs(string SiteIDs, string[] conn, string OrdType)
        {
            List<POR_SP_GetRequestBySiteIDsOrUserID_Result> RequestList = new List<POR_SP_GetRequestBySiteIDsOrUserID_Result>();
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                RequestList = db.POR_SP_GetRequestBySiteIDsOrUserID(SiteIDs, 0, OrdType).OrderByDescending(o => o.ID).ToList();
            }
            catch { }
            finally { }
            return RequestList;
        }

        public List<POR_SP_GetRequestByRequestIDs_Result> GetRequestSummayByRequestIDs(string RequestIDs, string[] conn)
        {
            List<POR_SP_GetRequestByRequestIDs_Result> RequestList = new List<POR_SP_GetRequestByRequestIDs_Result>();
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                RequestList = db.POR_SP_GetRequestByRequestIDs(RequestIDs).OrderByDescending(o => o.PRH_ID).ToList();
            }
            catch { }
            finally { }
            return RequestList;
        }
        #endregion

        #region Approval Code
        public string SaveApprovalStatus(long RequestID, string ApprovalStatus, string ApprovalRemark, long UserID, string[] conn)
        {
            string result = "";
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                tApprovalDetail rec = new tApprovalDetail();
                rec = db.tApprovalDetails.Where(a => a.ObjectName == "MaterialRequest" && a.ReferenceID == RequestID).FirstOrDefault();
                if (rec != null)
                {
                    PORtPartRequestHead RequestHead = new PORtPartRequestHead();
                    RequestHead = GetRequestHeadByRequestID(RequestID, conn);

                    db.tApprovalDetails.Detach(rec);
                    rec.Remark = ApprovalRemark;
                    if (ApprovalStatus.ToLower() == "true" || ApprovalStatus.ToLower() == "false")
                    {
                        if (ApprovalStatus.ToLower() == "true")
                        {
                            rec.Status = "Approved";
                            RequestHead.StatusID = db.mStatus.Where(s => s.Status == "Approved").FirstOrDefault().ID;
                        }
                        else if (ApprovalStatus.ToLower() == "false")
                        {
                            rec.Status = "Rejected";
                            RequestHead.StatusID = db.mStatus.Where(s => s.Status == "Rejected").FirstOrDefault().ID;
                        }

                        rec.StatusChangedBy = UserID;
                        rec.ApprovedDate = DateTime.Now;

                        db.tApprovalDetails.Attach(rec);
                        db.ObjectStateManager.ChangeObjectState(rec, EntityState.Modified);
                        db.SaveChanges();

                        SetIntoPartRequestHead(RequestHead, conn);

                        if (ApprovalStatus.ToLower() == "true")
                        {
                            /*Insert into IssueHead & IssuePartDetails*/
                            PORtMINHead IssueHead = new PORtMINHead();
                            IssueHead.PRH_ID = RequestHead.PRH_ID;
                            IssueHead.SiteID = RequestHead.SiteID;
                            IssueHead.IssuedByUserID = 0;
                            IssueHead.StatusID = 1;
                            IssueHead.IsSubmit = false;
                            IssueHead.CreatedBy = UserID;
                            IssueHead.CreationDt = DateTime.Now;
                            db.PORtMINHeads.AddObject(IssueHead);
                            db.SaveChanges();

                            List<PORtPartRequestDetail> PartList = new List<PORtPartRequestDetail>();
                            PartList = db.PORtPartRequestDetails.Where(r => r.PRH_ID == RequestHead.PRH_ID).ToList();

                            foreach (PORtPartRequestDetail part in PartList)
                            {
                                PORtMINDetail IssuePart = new PORtMINDetail();
                                IssuePart.MINH_ID = IssueHead.MINH_ID;
                                IssuePart.PRD_ID = part.PRD_ID;
                                IssuePart.Prod_ID = part.Prod_ID;
                                IssuePart.Prod_Name = part.Prod_Name;
                                IssuePart.Prod_Description = part.Prod_Description;
                                IssuePart.IssuedQty = part.RemaningQty;
                                IssuePart.Sequence = part.Sequence;
                                IssuePart.UOMID = part.UOMID;
                                IssuePart.Prod_Code = part.Prod_Code;
                                db.PORtMINDetails.AddObject(IssuePart);
                                db.SaveChanges();
                            }

                            /*End*/

                            EmailSendofApproved(RequestHead.PRH_ID, conn);
                        }
                        else
                        {
                            EMailSendWhenRequestRejected(RequestHead.PRH_ID, conn);
                        }
                        result = "true";
                    }
                }

            }
            catch { result = "false"; }
            finally { }
            return result;
        }

        public tApprovalDetail GetApprovalDetailsByReqestID(long RequestID, string[] conn)
        {
            tApprovalDetail rec = new tApprovalDetail();
            try
            {
                using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
                {
                    rec = db.tApprovalDetails.Where(a => a.ObjectName == "MaterialRequest" && a.ReferenceID == RequestID).FirstOrDefault();
                }
            }
            catch { }
            finally { }
            return rec;
        }
        #endregion

        #region RequestMail

        public void SendMail(string MailBody, string MailSubject, string ToEmailIDs)
        {
            try
            {
                //SmtpClient smtpClient = new SmtpClient("smtpout.asia.secureserver.net", 25);
                SmtpClient smtpClient = new SmtpClient("10.228.134.54", 25);
                // SmtpClient smtpClient = new SmtpClient("mail.brilliantinfosys.com", 587);
                MailMessage message = new MailMessage();


                // MailAddress fromAddress = new MailAddress("admin@brilliantinfosys.com", "GWC");
                MailAddress fromAddress = new MailAddress("OMSTest@gulfwarehousing.com", "GWC");
                // MailAddress fromAddress = new MailAddress("suraj@brilliantinfosys.com", "GWC");

                //From address will be given as a MailAddress Object
                message.From = fromAddress;

                //To address collection of MailAddress
                message.To.Add(ToEmailIDs);
                message.Subject = MailSubject;

                //Body can be Html or text format
                //Specify true if it  is html message
                message.IsBodyHtml = true;

                //Message body content
                message.Body = MailBody;

                smtpClient.EnableSsl = false;
                //Send SMTP mail
                smtpClient.UseDefaultCredentials = false;
                //NetworkCredential basicCredential = new NetworkCredential("suraj@brilliantinfosys.com", "6march1986");
                NetworkCredential basicCredential = new NetworkCredential("OMSTest@gulfwarehousing.com", "");
                smtpClient.Credentials = basicCredential;

                smtpClient.Send(message);
            }
            catch { }
        }

        protected string EMailGetRequestDetail(POR_SP_GetRequestByRequestIDs_Result Request)
        {
            string result = "";

            DateTime date1 = new DateTime();
            date1 = Convert.ToDateTime(Request.RequestDate.Value.ToString());
            string OrderDate = date1.ToString("dd/MM/yyyy");



            result = "Request No. : <b>" + Request.PRH_ID.ToString() + "</b>" +
                     "<br/>" +
                          // "Request Date : <b>" + Request.RequestDate.Value.ToString("dd-MMM-yyyy") + "</b>" +
                          //   "Request Date : <b>" + Request.RequestDate.Value.ToString("dd/mm/yyyy") + "</b>" +
                          "Request Date : <b>" + OrderDate + "</b>" +
                     "<br/>" +
                     "Status : <b>" + Request.RequestStatus + "</b>" +
                     "<br/>" +
                     "Site / Warehouse : <b>" + Request.SiteName + "</b>" +
                     "<br/>" +
                     "Request Type : <b>" + Request.RequestType + "</b>" +
                     "<br/>" +
                     "Requested By : <b>" + Request.RequestByUserName + "</b>";
            return result;
        }

        protected string EMailGetRequestPartDetail(long RequestID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            DataSet dsOrdrDetail = new DataSet();
            dsOrdrDetail = fillds("select OD.Sequence,OD.Prod_Code,OD.Prod_Description,OD.OrderQty ,PRD.GroupSet,OD.Total  ,ROW_NUMBER() OVER(ORDER BY Prod_Code ASC) AS SEQ  from torderdetail OD left outer join mProduct PRD on OD.SkuId =PRD.ID where OD.Orderheadid=" + RequestID + " order by SEQ ", conn);

            string messageBody = "<font><b>Order Details :  </b> </font><br/><br/>";

            string htmlTableStart = "<table style=\"border-collapse:collapse; text-align:center;\" >";
            string htmlTableEnd = "</table>";
            string htmlHeaderRowStart = "<tr style =\"background-color:#6FA1D2; color:#ffffff;\">";
            string htmlHeaderRowEnd = "</tr>";
            string htmlTrStart = "<tr style =\"color:black; text-align: center;\">";
            string htmlTrEnd = "</tr>";
            string htmlTdStart = "<td style=\" border-color:black; border-style:solid; border-width:thin; padding: 5px; text-align: center;font-size: 10pt;\">";
            string htmlTdEnd = "</td>";

            messageBody += htmlTableStart;

            messageBody += htmlHeaderRowStart;
            messageBody += htmlTdStart + "Sr. No." + htmlTdEnd;
            messageBody += htmlTdStart + "Item Code" + htmlTdEnd;
            messageBody += htmlTdStart + "Description" + htmlTdEnd;
            messageBody += htmlTdStart + "Qty" + htmlTdEnd;
            messageBody += htmlTdStart + "Group Set" + htmlTdEnd;
            messageBody += htmlHeaderRowEnd;
            if (dsOrdrDetail.Tables[0].Rows.Count > 0)
            {
                for (int r = 0; r <= dsOrdrDetail.Tables[0].Rows.Count - 1; r++)
                {
                    messageBody += htmlTrStart;
                    messageBody += htmlTdStart + " " + dsOrdrDetail.Tables[0].Rows[r]["SEQ"].ToString() + " " + htmlTdEnd;
                    messageBody += htmlTdStart + " " + dsOrdrDetail.Tables[0].Rows[r]["Prod_Code"].ToString() + " " + htmlTdEnd;
                    messageBody += htmlTdStart + " " + dsOrdrDetail.Tables[0].Rows[r]["Prod_Description"].ToString() + " " + htmlTdEnd;
                    messageBody += htmlTdStart + " " + dsOrdrDetail.Tables[0].Rows[r]["OrderQty"].ToString() + " " + htmlTdEnd;
                    messageBody += htmlTdStart + " " + dsOrdrDetail.Tables[0].Rows[r]["GroupSet"].ToString() + " " + htmlTdEnd;
                    messageBody += htmlTrEnd;
                }
            }
            messageBody += htmlTableEnd;
            return messageBody;
        }

        protected string EMailGetRequestPartDetailEcommerce(long RequestID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            DataSet dsOrdrDetail = new DataSet();
            dsOrdrDetail = fillds("select OD.Sequence,OD.Prod_Code,OD.Prod_Description,OD.OrderQty ,PRD.GroupSet,OD.Total,OD.price,ROW_NUMBER() OVER(ORDER BY Prod_Code ASC) AS SEQ  from torderdetail OD left outer join mProduct PRD on OD.SkuId =PRD.ID where OD.Orderheadid=" + RequestID + " order by SEQ ", conn);

            DataSet dtgrntot = new DataSet();
            dtgrntot = fillds(" select GrandTotal from torderhead where id=" + RequestID + "", conn);
            string messageBody = "<font><b>Order Details :  </b> </font><br/><br/>";

            string htmlTableStart = "<table style=\"border-collapse:collapse; text-align:center;\" >";
            string htmlTableEnd = "</table>";
            string htmlHeaderRowStart = "<tr style =\"background-color:#6FA1D2; color:#ffffff;\">";
            string htmlHeaderRowEnd = "</tr>";
            string htmlTrStart = "<tr style =\"color:black; text-align: center;\">";
            string htmlTrEnd = "</tr>";
            string htmlTdStart = "<td style=\" border-color:black; border-style:solid; border-width:thin; padding: 5px; text-align: center;font-size: 10pt;\">";
            string htmlTdEnd = "</td>";

            messageBody += htmlTableStart;

            messageBody += htmlHeaderRowStart;
            messageBody += htmlTdStart + "Sr. No." + htmlTdEnd;
            messageBody += htmlTdStart + "Sku Code" + htmlTdEnd;
            messageBody += htmlTdStart + "Description" + htmlTdEnd;
            messageBody += htmlTdStart + "Qty" + htmlTdEnd;
            messageBody += htmlTdStart + "Price" + htmlTdEnd;
            messageBody += htmlHeaderRowEnd;
            if (dsOrdrDetail.Tables[0].Rows.Count > 0)
            {
                for (int r = 0; r <= dsOrdrDetail.Tables[0].Rows.Count - 1; r++)
                {
                    // r = r + 1;
                    messageBody += htmlTrStart;
                    messageBody += htmlTdStart + " " + dsOrdrDetail.Tables[0].Rows[r]["SEQ"].ToString() + " " + htmlTdEnd;
                    //essageBody += htmlTdStart + " " + r + " " + htmlTdEnd;
                    messageBody += htmlTdStart + " " + dsOrdrDetail.Tables[0].Rows[r]["Prod_Code"].ToString() + " " + htmlTdEnd;
                    messageBody += htmlTdStart + " " + dsOrdrDetail.Tables[0].Rows[r]["Prod_Description"].ToString() + " " + htmlTdEnd;
                    messageBody += htmlTdStart + " " + dsOrdrDetail.Tables[0].Rows[r]["OrderQty"].ToString() + " " + htmlTdEnd;
                    messageBody += htmlTdStart + " " + dsOrdrDetail.Tables[0].Rows[r]["price"].ToString() + " " + htmlTdEnd;
                    messageBody += htmlTrEnd;
                }
            }

            messageBody += htmlTdStart + "  " + htmlTdEnd;
            messageBody += htmlTdStart + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + htmlTdEnd;
            messageBody += htmlTdStart + " Grand Total " + htmlTdEnd;
            messageBody += htmlTdStart + " " + dtgrntot.Tables[0].Rows[0]["GrandTotal"].ToString() + " " + htmlTdEnd;
            messageBody += htmlTableEnd;
            return messageBody;

        }



        protected string EMailGetRequestDetailVodafoneTechnical(GWC_SP_GetRequestHeadByRequestIDs_Result Request, string[] conn)
        {
            //string result = "";

            DateTime date1 = new DateTime();
            date1 = Convert.ToDateTime(Request.OrderDate.Value.ToString());
            string OrderDate = date1.ToString("dd/MM/yyyy");


            DateTime date2 = new DateTime();
            date2 = Convert.ToDateTime(Request.Deliverydate.Value.ToString());
            string Deliverydate = date2.ToString("dd/MM/yyyy");

            string locationcode = "";
            string LocationName = "";
            string messageBody = "<font><b>Order Summary :  </b> </font><br/><br/>";

            string htmlTableStart = "<table style=\"border-collapse:collapse; text-align:center;\" >";
            string htmlTableEnd = "</table>";
            string htmlHeaderRowStart = "<tr style =\"background-color:#6FA1D2; color:#ffffff;\">";
            string htmlHeaderRowEnd = "</tr>";
            string htmlTrStart = "<tr style =\"color:black; text-align: center;\">";
            string htmlTrEnd = "</tr>";
            string htmlTdStart = "<td style=\" border-color:black; border-style:solid; border-width:thin; padding: 5px; text-align: center;font-size: 10pt;\">";
            string htmlTdEnd = "</td>";

            messageBody += htmlTableStart;

            messageBody += htmlHeaderRowStart;
            messageBody += htmlTdStart + "Order Date" + htmlTdEnd;
            messageBody += htmlTdStart + "Order Id" + htmlTdEnd;
            //messageBody += htmlTdStart + "Customer Order Reference No." + htmlTdEnd;          
            messageBody += htmlTdStart + "Exp. Delivery Date" + htmlTdEnd;
            messageBody += htmlTdStart + "Status" + htmlTdEnd;
            messageBody += htmlTdStart + "Department" + htmlTdEnd;
            messageBody += htmlTdStart + "Location Code" + htmlTdEnd;
            messageBody += htmlTdStart + "Location Name" + htmlTdEnd;
            messageBody += htmlTdStart + "Request Type" + htmlTdEnd;
            messageBody += htmlTdStart + "Requested By" + htmlTdEnd;
            messageBody += htmlTdStart + "Remark" + htmlTdEnd;

            messageBody += htmlTdStart + "Project Type" + htmlTdEnd;
            messageBody += htmlTdStart + "Site Code" + htmlTdEnd;
            messageBody += htmlTdStart + "Site Name" + htmlTdEnd;
            messageBody += htmlTdStart + "Latitude" + htmlTdEnd;
            messageBody += htmlTdStart + "Longitude" + htmlTdEnd;
            messageBody += htmlTdStart + "Access Requirement" + htmlTdEnd;
            messageBody += htmlHeaderRowEnd;

            messageBody += htmlTrStart;
            messageBody += htmlTdStart + " " + OrderDate + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Request.OrderNo.ToString() + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Deliverydate + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Request.RequestStatus + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Request.SiteName + " " + htmlTdEnd;
            if (Request.LocationID == 0)
            {
                locationcode = "N/A";
                LocationName = "N/A";
            }
            else
            {
                DataSet dsGetLocation = new DataSet();
                dsGetLocation = fillds("select LocationCode,LocationName from tAddress where ID=" + Request.LocationID + "", conn);

                locationcode = dsGetLocation.Tables[0].Rows[0]["LocationCode"].ToString();
                LocationName = dsGetLocation.Tables[0].Rows[0]["LocationName"].ToString();
            }
            messageBody += htmlTdStart + " " + locationcode + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + LocationName + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Request.Priority + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Request.RequestByUserName + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Request.Remark + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Request.ProjectType + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Request.SiteCode + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Request.SiteNM + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Request.Latitude + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Request.Longitude + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Request.AccessRequirement + " " + htmlTdEnd;

            messageBody += htmlTrEnd;

            messageBody += htmlTableEnd;

            //result = "Order Id. : <b>" + Request.ID.ToString() + "</b>" +
            //         "<br/>" +
            //         "Customer Order Reference No.: <b>" + Request.OrderNumber+ "</b>"+
            //         "<br/>" +
            //         "Order Date : <b>" + Request.OrderDate.Value.ToString("dd-MMM-yyyy") + "</b>" +
            //         "<br/>" +
            //         "Exp. Delivery Date : <b>" + Request.Deliverydate.Value.ToString("dd-MMM-yyyy") + "</b>" +
            //         "<br/>" +
            //         "Status : <b>" + Request.RequestStatus + "</b>" +
            //         "<br/>" +
            //         "Department : <b>" + Request.SiteName + "</b>" +
            //         "<br/>" +
            //         "Request Type : <b>" + Request.Priority + "</b>" +
            //         "<br/>" +
            //         "Requested By : <b>" + Request.RequestByUserName + "</b>" +
            //         "<br/>" +
            //         "Remark : <b>" + Request.Remark + "</b>";                    

            return messageBody;
        }

        protected string EMailGetRequestDetail(GWC_SP_GetRequestHeadByRequestIDs_Result Request, string[] conn)
        {
            //string result = "";

            DateTime date1 = new DateTime();
            date1 = Convert.ToDateTime(Request.OrderDate.Value.ToString());
            string OrderDate = date1.ToString("dd/MM/yyyy");


            DateTime date2 = new DateTime();
            date2 = Convert.ToDateTime(Request.Deliverydate.Value.ToString());
            string Deliverydate = date2.ToString("dd/MM/yyyy");

            string locationcode = "";
            string LocationName = "";
            string messageBody = "<font><b>Order Summary :  </b> </font><br/><br/>";

            string htmlTableStart = "<table style=\"border-collapse:collapse; text-align:center;\" >";
            string htmlTableEnd = "</table>";
            string htmlHeaderRowStart = "<tr style =\"background-color:#6FA1D2; color:#ffffff;\">";
            string htmlHeaderRowEnd = "</tr>";
            string htmlTrStart = "<tr style =\"color:black; text-align: center;\">";
            string htmlTrEnd = "</tr>";
            string htmlTdStart = "<td style=\" border-color:black; border-style:solid; border-width:thin; padding: 5px; text-align: center;font-size: 10pt;\">";
            string htmlTdEnd = "</td>";

            messageBody += htmlTableStart;

            messageBody += htmlHeaderRowStart;
            messageBody += htmlTdStart + "Order Date" + htmlTdEnd;
            messageBody += htmlTdStart + "Order Id" + htmlTdEnd;
            //messageBody += htmlTdStart + "Customer Order Reference No." + htmlTdEnd;          
            messageBody += htmlTdStart + "Exp. Delivery Date" + htmlTdEnd;
            messageBody += htmlTdStart + "Status" + htmlTdEnd;
            messageBody += htmlTdStart + "Department" + htmlTdEnd;
            messageBody += htmlTdStart + "Location Code" + htmlTdEnd;
            messageBody += htmlTdStart + "Location Name" + htmlTdEnd;
            messageBody += htmlTdStart + "Request Type" + htmlTdEnd;
            messageBody += htmlTdStart + "Requested By" + htmlTdEnd;
            messageBody += htmlTdStart + "Remark" + htmlTdEnd;
            messageBody += htmlHeaderRowEnd;

            messageBody += htmlTrStart;
            // messageBody += htmlTdStart + " " + Request.OrderDate.Value.ToString("dd/mm/yyyy") + " " + htmlTdEnd;
            //  messageBody += htmlTdStart + " " + Request.OrderDate.Value.ToString("dd/mm/yyyy") + " " + htmlTdEnd;
            //  messageBody += htmlTdStart + " " + Request.OrderDate.Value.ToString("dd-MMM-yyyy") + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + OrderDate + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Request.OrderNo.ToString() + " " + htmlTdEnd;
            // messageBody += htmlTdStart + " " + Request.OrderNumber + " " + htmlTdEnd;            
            //  messageBody += htmlTdStart + " " + Request.Deliverydate.Value.ToString("dd-MMM-yyyy") + " " + htmlTdEnd;
            //  messageBody += htmlTdStart + " " + Request.Deliverydate.Value.ToString("dd/mm/yyyy") + " " + htmlTdEnd;
            //   messageBody += htmlTdStart + " " + Request.Deliverydate.Value.ToString("dd/mm/yyyy") + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Deliverydate + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Request.RequestStatus + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Request.SiteName + " " + htmlTdEnd;
            if (Request.LocationID == 0)
            {
                locationcode = "N/A";
                LocationName = "N/A";
            }
            else
            {
                DataSet dsGetLocation = new DataSet();
                dsGetLocation = fillds("select LocationCode,LocationName from tAddress where ID=" + Request.LocationID + "", conn);

                locationcode = dsGetLocation.Tables[0].Rows[0]["LocationCode"].ToString();
                LocationName = dsGetLocation.Tables[0].Rows[0]["LocationName"].ToString();
            }
            messageBody += htmlTdStart + " " + locationcode + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + LocationName + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Request.Priority + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Request.RequestByUserName + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Request.Remark + " " + htmlTdEnd;
            messageBody += htmlTrEnd;

            messageBody += htmlTableEnd;

            //result = "Order Id. : <b>" + Request.ID.ToString() + "</b>" +
            //         "<br/>" +
            //         "Customer Order Reference No.: <b>" + Request.OrderNumber+ "</b>"+
            //         "<br/>" +
            //         "Order Date : <b>" + Request.OrderDate.Value.ToString("dd-MMM-yyyy") + "</b>" +
            //         "<br/>" +
            //         "Exp. Delivery Date : <b>" + Request.Deliverydate.Value.ToString("dd-MMM-yyyy") + "</b>" +
            //         "<br/>" +
            //         "Status : <b>" + Request.RequestStatus + "</b>" +
            //         "<br/>" +
            //         "Department : <b>" + Request.SiteName + "</b>" +
            //         "<br/>" +
            //         "Request Type : <b>" + Request.Priority + "</b>" +
            //         "<br/>" +
            //         "Requested By : <b>" + Request.RequestByUserName + "</b>" +
            //         "<br/>" +
            //         "Remark : <b>" + Request.Remark + "</b>";                    

            return messageBody;
        }

        protected string EmailGetAddressDetails(GWC_SP_GetRequestHeadByRequestIDs_Result Request, string[] conn)
        {
            string location = "";
            string Address = "";
            string messageBody = "<font><b>Address Details:  </b> </font><br/><br/>";

            string htmlTableStart = "<table style=\"border-collapse:collapse; text-align:center;\" >";
            string htmlTableEnd = "</table>";
            string htmlHeaderRowStart = "<tr style =\"background-color:#6FA1D2; color:#ffffff;\">";
            string htmlHeaderRowEnd = "</tr>";
            string htmlTrStart = "<tr style =\"color:black; text-align: center;\">";
            string htmlTrEnd = "</tr>";
            string htmlTdStart = "<td style=\" border-color:black; border-style:solid; border-width:thin; padding: 5px; text-align: center;font-size: 10pt;\">";
            string htmlTdEnd = "</td>";

            messageBody += htmlTableStart;
            messageBody += htmlHeaderRowStart;
            messageBody += htmlTdStart + "Customer Address" + htmlTdEnd;
            messageBody += htmlTdStart + "Location" + htmlTdEnd;
            messageBody += htmlHeaderRowEnd;
            if (Request.LocationID == 0)
            {
                location = "N/A";
            }
            else
            {
                DataSet dsGetLocations = new DataSet();
                dsGetLocations = fillds("select AddressLine1 from tAddress where ID=" + Request.LocationID + "", conn);
                location = dsGetLocations.Tables[0].Rows[0]["AddressLine1"].ToString();
            }
            DataSet dsGetAddressid = new DataSet();
            dsGetAddressid = fillds("select AddressId from torderhead where Id=" + Request.ID + "", conn);
            long AddressId = Convert.ToInt64(dsGetAddressid.Tables[0].Rows[0]["AddressId"].ToString());
            if (AddressId == 0)
            {
                Address = "N/A";
            }
            else
            {
                DataSet dsGetAddress = new DataSet();
                dsGetAddress = fillds("select AddressLine1 from tAddress where ID=" + AddressId + "", conn);
                Address = dsGetAddress.Tables[0].Rows[0]["AddressLine1"].ToString();
            }
            messageBody += htmlTrStart;
            messageBody += htmlTdStart + " " + Address + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + location + " " + htmlTdEnd;
            messageBody += htmlTrEnd;
            messageBody += htmlTableEnd;
            return messageBody;
        }


        protected string EMailGetRequestPratDetail(long RequestID, long SiteID, string[] conn, List<POR_SP_GetPartDetail_ForRequest_Result> PartList = null)
        {
            string result = "";
            try
            {
                List<POR_SP_GetPartDetail_ForRequest_Result> htmlList = new List<POR_SP_GetPartDetail_ForRequest_Result>();
                if (RequestID != 0)
                {
                    BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                    htmlList = db.POR_SP_GetPartDetail_ForRequest("0", 0, SiteID, RequestID).ToList();
                }
                else { htmlList = PartList; }
                int srno = 0;
                XElement xmlEle = new XElement("table", from rec in htmlList
                                                        select new XElement("tr",
                                                        new XElement("td", (srno = srno + 1) + "."),
                                                        new XElement("td", rec.Prod_Code),
                                                        new XElement("td", rec.Prod_Name),
                                                        new XElement("td", rec.Prod_Description),
                                                        new XElement("td1", rec.RequestQty),
                                                        new XElement("td", rec.UOM)));


                string tblHeader = "<br /><table cellpadding='2' cellspacing='5' style='text-align: left; font-family: Tahoma; font-size: 10px;'>";
                tblHeader = tblHeader + "<tr><td colspan='6'><b>Part Details : </b></td></tr>" +
                                        "<tr>" +
                                        "<td style='font-weight: bold; text-align: center; border-bottom: solid 1px gray; border-top: solid 1px gray;'>Sr.No.</td>" +
                                        "<td style='font-weight: bold; text-align: center; border-bottom: solid 1px gray; border-top: solid 1px gray;'>Part Code</td>" +
                                        "<td style='font-weight: bold; text-align: center; border-bottom: solid 1px gray; border-top: solid 1px gray;'>Part Name</td>" +
                                        "<td style='font-weight: bold; text-align: center; border-bottom: solid 1px gray; border-top: solid 1px gray;'>Part Description</td>" +
                                        "<td style='font-weight: bold; text-align: center; border-bottom: solid 1px gray; border-top: solid 1px gray;'>Request Qty</td>" +
                                        "<td style='font-weight: bold; text-align: center; border-bottom: solid 1px gray; border-top: solid 1px gray;'>UOM</td></tr>";

                //result = result + xmlEle.ToString().Replace("<table>", tblHeader);
                result = result + xmlEle.ToString().Replace("<table>", tblHeader).Replace("<td1>", "<td style='text-align: right;'>").Replace("</td1>", "</td>");
            }
            catch { }
            return result;
        }

        protected string EmailGetEmailIDsByUserID(long UserID, string[] conn)
        {
            string result;
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                mUserProfileHead user = new mUserProfileHead();
                user = db.mUserProfileHeads.Where(u => u.ID == UserID).FirstOrDefault();
                result = user.EmailID;
                return result;
            }
        }

        protected string GetUserNameByUserID(long UserID, string[] conn)
        {
            string result;
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                mUserProfileHead user = new mUserProfileHead();
                user = db.mUserProfileHeads.Where(u => u.ID == UserID).FirstOrDefault();
                result = user.FirstName + " " + user.LastName;
                return result;
            }
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
                                //"<a href='http://elegantcrm.com/gwc/Login/Login.aspx' target='_blank' style='font-size: 18px; color: #3BB9FF; font-family: Comic Sans MS; text-decoration: none;'>Go to GWC</a>" +
                                "Please <a href='https://oms.gwclogistics.com/GWCProdV2/Login/Login.aspx' target='_blank' style='color: #3BB9FF;  text-decoration: none;'>click here </a>  to view the order details." +
                                "<br/><br/>" +
                                "Thank you, <br/>" +
                                "OMS Notification Team<br/>" +
                                "<br/><br/><hr/>" +
                                "<b>SELF EXPRESSION. BY GWC </b>" +
                                "<br/>This email including its attachments are confidential and intended solely for the use of the individual or entity to whom they are addressed. If you have received this email in error, please delete it from your system and notify the sender immediately. If you are not the intended recipient you are notified that disclosing, copying or distributing the content of this information is strictly prohibited. " +
                                //"<br/>This e-mail, and any attachments hereto, is intended for use only by the addressee(s) named herein, and may contain legally privileged and/or confidential information. If you are not an intended recipient of this e-mail, you are notified that any dissemination, distribution or copying of this e-mail, and any attachments hereto, is strictly prohibited. If you have received this e-mail in error, please notify the sender by reply e-mail, and permanently delete this e-mail, and any copies or printouts. " +
                                "<br/><br/>PLEASE CONSIDER THE ENVIRONMENT BEFORE PRINTING THIS EMAIL.";

            return MailFooter;
        }

        protected void EmailSendofRequestReject(long RequestBy, long RequestID, string reasoncode, string[] conn)
        {
            string MailSubject = "";
            string MailBody = "";
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                try
                {
                    GWC_SP_GetRequestHeadByRequestIDs_Result Request = new GWC_SP_GetRequestHeadByRequestIDs_Result();
                    Request = db.GWC_SP_GetRequestHeadByRequestIDs(RequestID.ToString()).FirstOrDefault();

                    long Orderstatus = long.Parse(Request.Status.ToString());
                    long DepartmentID = long.Parse(Request.SiteID.ToString());
                    long Status = long.Parse(Request.Status.ToString());

                    DataSet dsMailSubBody = new DataSet();
                    dsMailSubBody = fillds("select * from mMessageEMailTemplates where ActivityID=(select Id from mDropdownValues where StatusID=4) and MessageID=(select Id from mDropdownValues where Value='Information') and active='Yes' and DepartmentID=" + DepartmentID + "", conn);
                    string ISProjectSite = "";
                    ISProjectSite = ISProjectSiteDetailsNew(Convert.ToString(Request.parentid), conn);
                    DataSet dsOrderApprovers = new DataSet();
                    dsOrderApprovers = fillds("select UserApproverID from torderwiseaccess where OrderID=" + RequestID + " and UserType!='User'", conn);
                    int dscnt = dsOrderApprovers.Tables[0].Rows.Count;
                    if (dscnt > 0)
                    {
                        for (int i = 0; i <= dscnt - 1; i++)
                        {
                            MailSubject = dsMailSubBody.Tables[0].Rows[0]["MailSubject"].ToString() + ", Order No # " + Request.OrderNo + "";

                            //MailBody = "Dear " + GetUserNameByUserID(RequestBy, conn) + ",";
                            MailBody = "Dear " + GetUserNameByUserID(Convert.ToInt64(dsOrderApprovers.Tables[0].Rows[i]["UserApproverID"].ToString()), conn) + ",";
                            MailBody = dsMailSubBody.Tables[0].Rows[0]["MailBody"].ToString();



                            if (ISProjectSite == "Yes")// if (Request.parentid == 10266)
                            {
                                MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetailVodafoneTechnical(Request, conn);
                            }
                            else
                            {
                                MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                            }
                            MailBody = MailBody + "<br/><br/>" + EmailGetAddressDetails(Request, conn);
                            MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetail(RequestID, conn);

                            //SendMail(MailBody + MailGetFooter(), MailSubject, EmailGetEmailIDsByUserID(RequestBy, conn));
                            SendMail(MailBody + MailGetFooter(), MailSubject, EmailGetEmailIDsByUserID(Convert.ToInt64(dsOrderApprovers.Tables[0].Rows[i]["UserApproverID"].ToString()), conn));
                            SaveCorrespondsData(RequestID, MailSubject, MailBody, DepartmentID, Status, conn);
                        }
                    }
                    long TemplateID = long.Parse(dsMailSubBody.Tables[0].Rows[0]["ID"].ToString());
                    AdditionalDistribution(RequestID, TemplateID, conn);

                    //send email for end customer for order is reject
                    //send mail for end customer
                    DataSet dsChkOredrIsEcomOrNot = new DataSet();
                    dsChkOredrIsEcomOrNot = fillds("select * from tOrderHead where orderType in('PreOrder','NormalOrder') and id=" + RequestID + " ", conn);
                    if (dsChkOredrIsEcomOrNot.Tables[0].Rows.Count > 0)
                    {
                        //  GWC_SP_GetRequestHeadByRequestIDs_Result EcomMsg = new GWC_SP_GetRequestHeadByRequestIDs_Result();
                        SendMailRejectOrderCustomer(Request, RequestID, Convert.ToUInt16(reasoncode), conn);
                    }
                }
                catch { }
                finally { }
            }

        }

        private void SendMailRejectOrderCustomer(GWC_SP_GetRequestHeadByRequestIDs_Result EcomMsg, long RequestID, int reasoncode, string[] conn)
        {
            string MailSubject;
            string MailBody = "";
            string Lanuage = "", CustomerEmail = "", Paymentmethod = "";
            long DepartmentID = long.Parse(EcomMsg.SiteID.ToString());
            long Status = long.Parse(EcomMsg.Status.ToString());

            DataSet dsMailSubBody1 = new DataSet();
            dsMailSubBody1 = fillds("select * from torderhead where id=" + RequestID + "", conn);
            string ordernumbr = dsMailSubBody1.Tables[0].Rows[0]["Ordernumber"].ToString();

            DataSet dsdata = new DataSet();
            dsdata = fillds(" select orderno from torderhead where id=" + RequestID + " ", conn);
            string ordno = "";
            ordno = dsdata.Tables[0].Rows[0]["orderno"].ToString();

            DataSet dsLanuage = new DataSet();
            dsLanuage = fillds(" select PreferredLanguage,Email,Paymentmethod from tECommHead where OrderID=" + RequestID + " ", conn);
            Lanuage = Convert.ToString(dsLanuage.Tables[0].Rows[0]["PreferredLanguage"]);
            CustomerEmail = Convert.ToString(dsLanuage.Tables[0].Rows[0]["Email"]);
            Paymentmethod = Convert.ToString(dsLanuage.Tables[0].Rows[0]["Paymentmethod"]);
            DataSet dsMailSubBody = new DataSet();
            dsMailSubBody = fillds("select * from mMessageEMailTemplates where ActivityID=(select Id from mDropdownValues where StatusID=100) and MessageID=(select Id from mDropdownValues where Value='Information') and DepartmentID=" + DepartmentID + " and Active='Yes' ", conn);
            DataSet dsAllUsers = new DataSet();
            if (Lanuage == "English" || Lanuage == "" || Lanuage == null)
            {
                MailSubject = dsMailSubBody.Tables[0].Rows[1]["MailSubject"].ToString() + ", Order No # " + ordernumbr + "";
                MailBody = MailBody + "<br/>" + EMailGetRequestDetailForCancelInEnglish(EcomMsg, conn);
                MailBody = MailBody + "<br/>" + EMailGetRequestPartDetailCancelInEnglish(ordno, RequestID, reasoncode, conn);
                MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetailMessage(RequestID, Paymentmethod, conn);
                MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetailFooter(RequestID, conn);
            }
            else
            {
                MailSubject = dsMailSubBody.Tables[0].Rows[0]["MailSubject"].ToString() + ", Order No # " + ordernumbr + "";
                MailBody = MailBody + "<br/>" + EMailGetRequestDetailForCancelInEnglish(EcomMsg, conn);
                MailBody = MailBody + "<br/>" + EMailGetRequestPartDetailCancelInARB(ordno, RequestID, reasoncode, conn);
                MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetailMessageARB(RequestID, Paymentmethod, conn);
                MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetailFooterARB(RequestID, conn);
            }
            SendMailForCustomer(MailBody, MailSubject, CustomerEmail, DepartmentID, conn);
            SaveCorrespondsData(RequestID, MailSubject, MailBody, DepartmentID, Status, conn);
        }

        public void EmailSendofApproved(long UserID, long RequestID, int id, string[] conn)
        {
            string MailSubject;
            string MailBody;
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                try
                {
                    GWC_SP_GetRequestHeadByRequestIDs_Result Request = new GWC_SP_GetRequestHeadByRequestIDs_Result();
                    Request = db.GWC_SP_GetRequestHeadByRequestIDs(RequestID.ToString()).FirstOrDefault();
                    string ISProjectSite = "";
                    ISProjectSite = ISProjectSiteDetailsNew(Convert.ToString(Request.parentid), conn);
                    long Orderstatus = long.Parse(Request.Status.ToString());
                    long DepartmentID = long.Parse(Request.SiteID.ToString());
                    long Status = long.Parse(Request.Status.ToString());

                    string ordertype = "";
                    tOrderHead ordrHd = new tOrderHead();
                    ordrHd = (from t in db.tOrderHeads
                              where t.Id == RequestID
                              select t).FirstOrDefault();
                    ordertype = ordrHd.orderType;

                    DataSet dsMailSubBody = new DataSet();
                    if (ordertype == "Direct Import")
                    {
                        dsMailSubBody = fillds("select * from mMessageEMailTemplates where ActivityID=(select Id from mDropdownValues where StatusID=30) and MessageID=(select Id from mDropdownValues where Value='Action') and DepartmentID=" + DepartmentID + "", conn);
                    }
                    else
                    {
                        dsMailSubBody = fillds("select * from mMessageEMailTemplates where ActivityID=(select Id from mDropdownValues where StatusID=3) and MessageID=(select Id from mDropdownValues where Value='Action') and DepartmentID=" + DepartmentID + " and Active='Yes'", conn);
                    }

                    MailSubject = dsMailSubBody.Tables[0].Rows[0]["MailSubject"].ToString() + ", Order No # " + Request.OrderNo + "";

                    MailBody = "Dear " + GetUserNameByUserID(UserID, conn) + ", <br/>";
                    MailBody = MailBody + dsMailSubBody.Tables[0].Rows[0]["MailBody"].ToString();

                    DataSet dsChkOredrIsEcomOrNot = new DataSet();

                    dsChkOredrIsEcomOrNot = fillds("select * from tOrderHead where orderType in('PreOrder','NormalOrder') and id=" + RequestID + " ", conn);
                    if (dsChkOredrIsEcomOrNot.Tables[0].Rows.Count > 0)
                    {
                        MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetailForEcomm(Request);
                        MailBody = MailBody + "<br/><br/>" + EmailGetAddressDetailsEcom(Request, conn);
                        MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetailEcommerce(RequestID, conn);
                    }
                    else
                    {
                        if (ISProjectSite == "Yes") //if (Request.parentid == 10266)
                        {
                            MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetailVodafoneTechnical(Request, conn);
                        }
                        else
                        {
                            MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                        }
                        MailBody = MailBody + "<br/><br/>" + EmailGetAddressDetails(Request, conn);
                        MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetail(RequestID, conn);
                    }

                    //SendMail(MailBody + MailGetFooter(), MailSubject, EmailGetEmailIDsByUserID(UserID, conn));
                    long TemplateID = long.Parse(dsMailSubBody.Tables[0].Rows[0]["ID"].ToString());
                    SaveCorrespondsDataNew(RequestID, MailSubject, MailBody, DepartmentID, Status, EmailGetEmailIDsByUserID(UserID, conn), conn);
                    if (id == 0)
                    {
                        AdditionalDistribution(RequestID, TemplateID, conn);
                    }
                }
                catch { }
                finally { }
            }
        }

        private string GetUserNameByLogistic(long RequestID, string[] conn)
        {
            string result = "", StoreId = ""; ;
            DataSet ds = new DataSet();
            ds = fillds("select StoreId from tOrderHead where id=" + RequestID + "", conn);
            StoreId = ds.Tables[0].Rows[0]["StoreId"].ToString();
            DataSet ds1 = new DataSet();
            ds1 = fillds("select StoreId from tOrderHead where id=" + RequestID + "", conn);


            return result;
        }
        protected int EmailSendToApprover(long ApproverID, long RequestID, int id, string[] conn)
        {
            int Result = 0;
            try
            {
                string MailSubject;
                string MailBody;
                // string partdetail = EMailGetRequestPratDetail(0, 0, conn, ReqPartDetils);
                using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
                {
                    GWC_SP_GetRequestHeadByRequestIDs_Result Request = new GWC_SP_GetRequestHeadByRequestIDs_Result();
                    Request = db.GWC_SP_GetRequestHeadByRequestIDs(RequestID.ToString()).FirstOrDefault();
                    string ISProjectSite = "";
                    ISProjectSite = ISProjectSiteDetailsNew(Convert.ToString(Request.parentid), conn);
                    long Orderstatus = long.Parse(Request.Status.ToString());
                    long DepartmentID = long.Parse(Request.SiteID.ToString());
                    long Status = long.Parse(Request.Status.ToString());
                    DataSet dsMailSubBody = new DataSet();
                    dsMailSubBody = fillds("select * from mMessageEMailTemplates where ActivityID=(select Id from mDropdownValues where StatusID=2 and value='Order Submit') and MessageID=(select Id from mDropdownValues where Value='Action') and DepartmentID=" + DepartmentID + " and Active='Yes'", conn);
                    if (dsMailSubBody.Tables[0].Rows.Count > 0)
                    {
                        MailSubject = dsMailSubBody.Tables[0].Rows[0]["MailSubject"].ToString() + ", Order No # " + Request.OrderNo + "";

                        MailBody = "Dear " + GetUserNameByUserID(ApproverID, conn) + ", <br/>";

                        DataSet dsFA = new DataSet();
                        dsFA = fillds("select UserType from tOrderWiseAccess  where OrderID=" + RequestID + " and UserApproverID=" + ApproverID + "", conn);
                        string UsrType = dsFA.Tables[0].Rows[0]["UserType"].ToString();
                        if (UsrType == "General Approver")
                        {
                            MailBody = MailBody + dsMailSubBody.Tables[0].Rows[0]["MailBody"].ToString();
                        }
                        else
                        {
                            if (UsrType == "Financial Approver")
                            {
                                DataSet dsFAMailBody = new DataSet();
                                dsFAMailBody = fillds("select * from mMessageEMailTemplates where ActivityID=(select Id from mDropdownValues where StatusID=2 and value='Order Submit Price Change') and MessageID=(select Id from mDropdownValues where Value='Action') and DepartmentID=" + DepartmentID + "", conn);
                                MailBody = MailBody + dsFAMailBody.Tables[0].Rows[0]["MailBody"].ToString();
                            }
                            else if (UsrType == "CostCenter Approver")
                            {
                                DataSet dsFAMailBody = new DataSet();
                                dsFAMailBody = fillds("select * from mMessageEMailTemplates where ActivityID=(select Id from mDropdownValues where StatusID=32 ) and MessageID=(select Id from mDropdownValues where Value='Action') and DepartmentID=" + DepartmentID + "", conn);
                                MailBody = MailBody + dsFAMailBody.Tables[0].Rows[0]["MailBody"].ToString();
                            }
                        }
                        DataSet dsChkOredrIsEcomOrNot = new DataSet();

                        dsChkOredrIsEcomOrNot = fillds("select * from tOrderHead where orderType in('PreOrder','NormalOrder') and id=" + RequestID + " ", conn);
                        if (dsChkOredrIsEcomOrNot.Tables[0].Rows.Count > 0)
                        {
                            MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetailForEcomm(Request);//for ecommerce
                            MailBody = MailBody + "<br/><br/>" + EmailGetAddressDetailsEcom(Request, conn);//for ecommerce
                            MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetailEcommerce(RequestID, conn);//for ecommerce
                        }
                        else
                        {
                            if (ISProjectSite == "Yes")
                            {
                                MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetailVodafoneTechnical(Request, conn);
                            }
                            else
                            {
                                MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                            }
                            // MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                            MailBody = MailBody + "<br/><br/>" + EmailGetAddressDetails(Request, conn);
                            MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetail(RequestID, conn);
                        }


                        //SendMail(MailBody + MailGetFooter(), MailSubject, EmailGetEmailIDsByUserID(ApproverID, conn));
                        Result = 2;//Mail Send Successfully to approver
                        long TemplateID = long.Parse(dsMailSubBody.Tables[0].Rows[0]["ID"].ToString());
                        SaveCorrespondsDataNew(RequestID, MailSubject, MailBody, DepartmentID, Status, EmailGetEmailIDsByUserID(ApproverID, conn), conn);
                        if (id == 0)
                        {
                            AdditionalDistribution(RequestID, TemplateID, conn);
                        }

                    }


                }
            }
            catch (System.Exception ex)
            {
                Result = 3;//Mail Not Send Successfully to approver
            }
            finally { }
            return Result;

        }
        protected void SaveCorrespondsDataNew(long RequestID, string MailSubject, string MailBody, long DepartmentID, long StatusID, string emailid, string[] conn)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                DataSet ds = new DataSet();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_SaveCorrespondsDataNew";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@OrderHeadId", RequestID);
                cmd.Parameters.AddWithValue("@MessageTitle", MailSubject);
                cmd.Parameters.AddWithValue("@Message", MailBody);
                cmd.Parameters.AddWithValue("@MessageSource", "Scheduler");
                cmd.Parameters.AddWithValue("@MessageType", "Approval");
                cmd.Parameters.AddWithValue("@DepartmentID", DepartmentID);
                cmd.Parameters.AddWithValue("@CurrentOrderStatus", StatusID);
                cmd.Parameters.AddWithValue("@MailStatus", 0);
                cmd.Parameters.AddWithValue("@Archive", false);
                cmd.Parameters.AddWithValue("@Useremailid", emailid);
                cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }

        }


        //add for ecommerce email purpose
        protected string EMailGetRequestDetailForEcomm(GWC_SP_GetRequestHeadByRequestIDs_Result Request)//
        {
            DateTime date1 = new DateTime();
            date1 = Convert.ToDateTime(Request.OrderDate.Value.ToString());
            string OrderDate = date1.ToString("dd/MM/yyyy");


            DateTime date2 = new DateTime();
            date2 = Convert.ToDateTime(Request.Deliverydate.Value.ToString());
            string Deliverydate = date2.ToString("dd/MM/yyyy");


            string messageBody = "<font><b>Order Summary :  </b> </font><br/><br/>";

            string htmlTableStart = "<table style=\"border-collapse:collapse; text-align:center;\" >";
            string htmlTableEnd = "</table>";
            string htmlHeaderRowStart = "<tr style =\"background-color:#6FA1D2; color:#ffffff;\">";
            string htmlHeaderRowEnd = "</tr>";
            string htmlTrStart = "<tr style =\"color:black; text-align: center;\">";
            string htmlTrEnd = "</tr>";
            string htmlTdStart = "<td style=\" border-color:black; border-style:solid; border-width:thin; padding: 5px; text-align: center;font-size: 10pt;\">";
            string htmlTdEnd = "</td>";

            messageBody += htmlTableStart;
            messageBody += htmlHeaderRowStart;
            messageBody += htmlTdStart + "Order Date" + htmlTdEnd;
            messageBody += htmlTdStart + "Order Id" + htmlTdEnd;
            // messageBody += htmlTdStart + "Ecomm Order No." + htmlTdEnd;
            messageBody += htmlTdStart + "Exp. Delivery Date" + htmlTdEnd;
            messageBody += htmlTdStart + "Status" + htmlTdEnd;
            messageBody += htmlTdStart + "Remark" + htmlTdEnd;
            messageBody += htmlHeaderRowEnd;

            messageBody += htmlTrStart;
            // messageBody += htmlTdStart + " " + Request.OrderDate.Value.ToString("dd-MMM-yyyy") + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + OrderDate + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Request.OrderNo.ToString() + " " + htmlTdEnd;
            // messageBody += htmlTdStart + " " + Request.OrderNumber + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Deliverydate + " " + htmlTdEnd;
            //  messageBody += htmlTdStart + " " + Request.Deliverydate.Value.ToString("dd-MMM-yyyy") + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Request.RequestStatus + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Request.Remark + " " + htmlTdEnd;
            messageBody += htmlTrEnd;

            messageBody += htmlTableEnd;
            return messageBody;
        }


        //add for ecommerce email purpose
        protected string EmailGetAddressDetailsEcom(GWC_SP_GetRequestHeadByRequestIDs_Result Request, string[] conn)
        {
            string location = "";
            string Address = "";
            string messageBody = "<font><b>Address Details:  </b> </font><br/><br/>";

            string htmlTableStart = "<table style=\"border-collapse:collapse; text-align:center;\" >";
            string htmlTableEnd = "</table>";
            string htmlHeaderRowStart = "<tr style =\"background-color:#6FA1D2; color:#ffffff;\">";
            string htmlHeaderRowEnd = "</tr>";
            string htmlTrStart = "<tr style =\"color:black; text-align: center;\">";
            string htmlTrEnd = "</tr>";
            string htmlTdStart = "<td style=\" border-color:black; border-style:solid; border-width:thin; padding: 5px; text-align: center;font-size: 10pt;\">";
            string htmlTdEnd = "</td>";

            messageBody += htmlTableStart;
            messageBody += htmlHeaderRowStart;
            messageBody += htmlTdStart + "Customer Address" + htmlTdEnd;
            messageBody += htmlTdStart + "Location" + htmlTdEnd;
            messageBody += htmlHeaderRowEnd;
            if (Request.LocationID == 0)
            {
                location = "N/A";
            }
            else
            {
                DataSet dsGetLocations = new DataSet();
                dsGetLocations = fillds("select AddressLine1 from tAddress where ID=" + Request.LocationID + "", conn);
                location = dsGetLocations.Tables[0].Rows[0]["AddressLine1"].ToString();
            }
            DataSet dsGetAddressid = new DataSet();
            dsGetAddressid = fillds("select 'Name - '+ECH.CustomerFirstName + ' ' + ECH.CustomerLastName +', Address -' + ECH.BuildingName + ',' + ECH.Streetname + ',' + ECH.City + ',' + ECH.Country + ',  Alternate Cont. No. - ' + ECH.AltContactNumber + ', EmailID - ' + ECH.Email AS HomeAddress,'Name -'+ECH.VodafoneStoreName+ ', Address -' + ECH.BuildingName2 + ',' + ECH.Streetname2 + ',' + ECH.City2 + ',' + ECH.Country2 + ',' + ECH.POBox AS RetailAddress, case when (ECH.CustomerFirstName + ' ' + ECH.CustomerLastName) =' ' then null else ECH.CustomerFirstName + ' ' + ECH.CustomerLastName  end CustomerName,ECH.VodafoneStoreName ,ECH.DeliveryType from tECommHead ECH where ECH.OrderID = " + Request.ID + "", conn);
            string CustomerName = dsGetAddressid.Tables[0].Rows[0]["CustomerName"].ToString();
            string DeliveryType = dsGetAddressid.Tables[0].Rows[0]["DeliveryType"].ToString();
            // if (string.IsNullOrEmpty(CustomerName))
            if (DeliveryType == "HUB")
            {
                Address = dsGetAddressid.Tables[0].Rows[0]["RetailAddress"].ToString();
            }
            else
            {
                // Address = dsGetAddressid.Tables[0].Rows[0]["HomeAddress"].ToString();
                Address = dsGetAddressid.Tables[0].Rows[0]["RetailAddress"].ToString();
            }
            messageBody += htmlTrStart;
            messageBody += htmlTdStart + " " + Address + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + location + " " + htmlTdEnd;
            messageBody += htmlTrEnd;
            messageBody += htmlTableEnd;
            return messageBody;
        }


        // protected void EmailSendWhenRequestSubmit(long RequestID, List<POR_SP_GetPartDetail_ForRequest_Result> ReqPartDetils, string[] conn)
        public int EmailSendWhenRequestSubmit(long RequestID, string[] conn)
        {
            int Result = 0;
            try
            {
                string MailSubject;
                string MailBody;

                using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
                {

                    GWC_SP_GetRequestHeadByRequestIDs_Result Request = new GWC_SP_GetRequestHeadByRequestIDs_Result();
                    Request = db.GWC_SP_GetRequestHeadByRequestIDs(RequestID.ToString()).FirstOrDefault();

                    long Orderstatus = long.Parse(Request.Status.ToString());
                    long DepartmentID = long.Parse(Request.SiteID.ToString());
                    long Status = long.Parse(Request.Status.ToString());
                    string ISProjectSite = "";
                    ISProjectSite = ISProjectSiteDetailsNew(Convert.ToString(Request.parentid), conn);
                    DataSet dsMailSubBody = new DataSet();
                    dsMailSubBody = fillds("select * from mMessageEMailTemplates where ActivityID=(select Id from mDropdownValues where StatusID=2 and value='Order Submit') and MessageID=(select Id from mDropdownValues where Value='Information') and DepartmentID=" + DepartmentID + "", conn);
                    if (dsMailSubBody.Tables[0].Rows.Count > 0)
                    {
                        DataSet ds = new DataSet();
                        ds = fillds("select orderType from tOrderHead where Id=" + RequestID + "", conn);
                        string ordertype = ds.Tables[0].Rows[0]["Ordertype"].ToString();
                        if (Status == 2)
                        {
                            MailSubject = dsMailSubBody.Tables[0].Rows[0]["MailSubject"].ToString() + ", Order No # " + Request.OrderNo + "";
                        }
                        else if (ordertype == "Direct Import" && Status == 3)
                        {
                            MailSubject = "Order has been submitted sucessfully with Direct Order Import , Order No # " + Request.OrderNo + "";
                        }
                        else
                        {
                            string CrntStatus = Request.RequestStatus.ToString();
                            MailSubject = "Order " + CrntStatus + ", Order No # " + Request.OrderNo + "";
                        }
                        
                        MailBody = "Dear " + GetUserNameByUserID(Convert.ToInt64(Request.RequestBy), conn) + ", <br/>";
                        MailBody = MailBody + "This is an automatically generated message in reference to a order request. An approval action is required before OMS can proceed. <br/> Thank you for your request. Before we can proceed, we need approver's formal approval to proceed. <br/>";
                        if (ordertype == "Direct Import" && Status == 3)
                        {
                            MailBody = MailBody + "<br/><br/>" + "Order has been submitted sucessfully with Direct Order Import";
                        }
                        MailBody = MailBody + dsMailSubBody.Tables[0].Rows[0]["MailBody"].ToString();

                        if (ISProjectSite == "Yes")/// if (Request.parentid == 10266)
                        {
                            MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetailVodafoneTechnical(Request, conn);
                        }
                        else
                        {
                            MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                        }
                        //MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                        MailBody = MailBody + "<br/><br/>" + EmailGetAddressDetails(Request, conn);
                        MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetail(RequestID, conn);

                        if (ordertype == "Direct Import" && Status == 3)
                        {
                            MailBody = MailBody + "<br/><br/>" + "Note : The order processes through direct import doesn't required any approver. These Order send to WMS for further processing ";
                        }

                        SendMail(MailBody + MailGetFooter(), MailSubject, EmailGetEmailIDsByUserID(Convert.ToInt64(Request.RequestBy), conn));
                        
                        Result = 2;// Mail sent to requestor
                        long TemplateID = long.Parse(dsMailSubBody.Tables[0].Rows[0]["ID"].ToString());
                        if (Status == 2)
                        {
                            AdditionalDistribution(RequestID, TemplateID, conn);
                        }
                        SaveCorrespondsData(RequestID, MailSubject, MailBody, DepartmentID, Status, conn);
                        if (Request.ContactId1 > 0)
                        {
                            string Con1Name = Request.Contact1Name; string Con1Email = Request.Con1Email;
                            MailBody = "";
                            MailBody = "Dear " + Con1Name + ", <br/>";
                            MailBody = MailBody + "This is an automatically generated message in reference to a order request. An approval action is required before OMS can proceed. <br/> Thank you for your request. Before we can proceed, we need approver's formal approval to proceed. <br/>";
                            if (ordertype == "Direct Import" && Status == 3)
                            {
                                MailBody = MailBody + "<br/><br/>" + "Order has been submitted sucessfully with Direct Order Import";
                            }

                            MailBody = MailBody + dsMailSubBody.Tables[0].Rows[0]["MailBody"].ToString();

                            if (ISProjectSite == "Yes")
                            {
                                MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetailVodafoneTechnical(Request, conn);
                            }
                            else
                            {
                                MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                            }
                            // MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                            MailBody = MailBody + "<br/><br/>" + EmailGetAddressDetails(Request, conn);
                            MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetail(RequestID, conn);
                            if (ordertype == "Direct Import" && Status == 3)
                            {
                                MailBody = MailBody + "<br/><br/>" + "Note : The order processes through direct import doesn't required any approver. These Order send to WMS for further processing ";
                            }
                            SendMail(MailBody + MailGetFooter(), MailSubject, Con1Email);

                            string Contact2 = Request.Con2.ToString();
                            //long Con2ID = long.Parse(Request.ContactId2.ToString());
                            //if (Con2ID > 0)
                            if (Contact2 != "0")
                            {
                                string Contact2Emails = GetContact2Email(Contact2, conn);
                                // string Con2Name = Request.Contact2Name; string Con2Email = Request.Con2Email;
                                MailBody = "";
                                MailBody = "Hi, <br/>";
                                MailBody = MailBody + "This is an automatically generated message in reference to a order request. An approval action is required before OMS can proceed. <br/> Thank you for your request. Before we can proceed, we need approver's formal approval to proceed. <br/>";
                                if (ordertype == "Direct Import" && Status == 3)
                                {
                                    MailBody = MailBody + "<br/><br/>" + "Order has been submitted sucessfully with Direct Order Import";
                                }
                                MailBody = MailBody + dsMailSubBody.Tables[0].Rows[0]["MailBody"].ToString();
                                if (ISProjectSite == "Yes")
                                {
                                    MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetailVodafoneTechnical(Request, conn);
                                }
                                else
                                {
                                    MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                                }
                                // MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                                MailBody = MailBody + "<br/><br/>" + EmailGetAddressDetails(Request, conn);
                                MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetail(RequestID, conn);
                                if (ordertype == "Direct Import" && Status == 3)
                                {
                                    MailBody = MailBody + "<br/><br/>" + "Note : The order processes through direct import doesn't required any approver. These Order send to WMS for further processing ";
                                }
                                SendMail(MailBody + MailGetFooter(), MailSubject, Contact2Emails);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "Sp_EnterErrorTracking";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("Data", ex.Data.ToString());
                    cmd.Parameters.AddWithValue("GetType", ex.Source.ToString());
                    cmd.Parameters.AddWithValue("InnerException", "Error");
                    cmd.Parameters.AddWithValue("Message", ex.Message.ToString());
                    cmd.Parameters.AddWithValue("Source", " EmailSendWhenRequestSubmit");
                    cmd.Parameters.AddWithValue("DateTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("UserID", 20000);
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
                Result = 3;//mail not sent to requestor.
            }
            finally { }
            return Result;
        }

        public string GetContact2Email(string Contact2, string[] conn)
        {
            string Contact1Name = "";
            DataSet ds = new DataSet();
            ds = fillds("select EmailID from tcontactpersondetail where ID IN( " + Contact2 + " )", conn);
            int cnt = ds.Tables[0].Rows.Count;
            if (cnt > 0)
            {
                for (int i = 0; i < cnt; i++)
                {
                    if (i == 0) { Contact1Name = ds.Tables[0].Rows[i]["EmailID"].ToString(); }
                    else
                    {
                        Contact1Name = Contact1Name + "," + ds.Tables[0].Rows[i]["EmailID"].ToString();
                    }
                }
            }
            return Contact1Name;
        }



        protected void InsertCorrespondsData(long RequestID, string MailSubject, string MailBody, long DepartmentID, long StatusID, string[] conn)
        {
            tCorrespond Cor = new tCorrespond();
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            Cor.OrderHeadId = RequestID;
            Cor.MessageTitle = MailSubject;
            Cor.Message = MailBody;
            Cor.date = DateTime.Now;
            Cor.MessageSource = "Scheduler";
            Cor.MessageType = "Information";
            Cor.DepartmentID = DepartmentID;
            Cor.CurrentOrderStatus = StatusID;
            Cor.MailStatus = 0;
            Cor.Archive = false;
            db.tCorresponds.AddObject(Cor);
            db.SaveChanges();
        }

        protected void SaveCorrespondsData(long RequestID, string MailSubject, string MailBody, long DepartmentID, long StatusID, string[] conn)
        {
            tCorrespond Cor = new tCorrespond();
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                Cor.OrderHeadId = RequestID;
                Cor.MessageTitle = MailSubject;
                Cor.Message = MailBody;
                Cor.date = DateTime.Now;
                Cor.MessageSource = "Correspondance";
                Cor.MessageType = "Information";
                Cor.DepartmentID = DepartmentID;
                Cor.CurrentOrderStatus = StatusID;
                Cor.MailStatus = 1;
                Cor.Archive = false;
                db.tCorresponds.AddObject(Cor);
                db.SaveChanges();
            }
        }

        protected void AdditionalDistribution_New(long RequestID, long TemplateID, string[] conn)
        {
            try
            {
                DataSet dsAddDist = new DataSet();
                string Usermail = "";
                dsAddDist = fillds("select * from GWV_VW_DistributionList where TemplateID=" + TemplateID + "", conn);
                int cnt = dsAddDist.Tables[0].Rows.Count;
                if (cnt > 0)
                {
                    for (int i = 0; i <= cnt - 1; i++)
                    {
                        if (i == 0)
                        {
                            Usermail = dsAddDist.Tables[0].Rows[i]["EmailID"].ToString();
                        }
                        else
                        {
                            Usermail = Usermail + "," + dsAddDist.Tables[0].Rows[i]["EmailID"].ToString();
                        }

                    }
                    string MailSubject = "";
                    string MailBody = "";
                    using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
                    {
                        GWC_SP_GetRequestHeadByRequestIDs_Result Request = new GWC_SP_GetRequestHeadByRequestIDs_Result();
                        Request = db.GWC_SP_GetRequestHeadByRequestIDs(RequestID.ToString()).FirstOrDefault();
                        long Orderstatus = long.Parse(Request.Status.ToString());
                        long DepartmentID = long.Parse(Request.SiteID.ToString());
                        long Status = long.Parse(Request.Status.ToString());
                        string ISProjectSite = "";
                        ISProjectSite = ISProjectSiteDetailsNew(Convert.ToString(Request.parentid), conn);
                        string AdDistName = "AddDistribution User";
                        string UserEmailnew = "";
                        string[] eorder = Usermail.Split(',');
                        var distinctArray = eorder.Distinct().ToArray();
                        for (int s = 0; s <= distinctArray.Count() - 1; s++)
                        {
                            if (s == 0)
                            {
                                UserEmailnew = distinctArray[s].ToString();
                            }
                            else
                            {
                                UserEmailnew = UserEmailnew + "," + distinctArray[s].ToString();
                            }
                        }

                        DataSet dsMailSubBody = new DataSet();
                        // dsMailSubBody = fillds("select * from mMessageEMailTemplates where ActivityID=(select Id from mDropdownValues where StatusID=2) and MessageID=(select Id from mDropdownValues where Value='Action') and DepartmentID=" + DepartmentID + "", conn);

                        dsMailSubBody = fillds("select * from mMessageEMailTemplates where ID=" + TemplateID + " ", conn);

                        if (Status == 2)
                        {
                            MailSubject = dsMailSubBody.Tables[0].Rows[0]["MailSubject"].ToString() + ", Order No # " + Request.OrderNo + "";
                        }
                        else
                        {
                            string CrntStatus = Request.RequestStatus.ToString();
                            MailSubject = "Order " + CrntStatus + ", Order No # " + Request.OrderNo + "";
                        }

                        MailBody = "Dear " + AdDistName + ", <br/>";
                        MailBody = MailBody + dsMailSubBody.Tables[0].Rows[0]["MailBody"].ToString();

                        DataSet dsChkOredrIsEcomOrNot = new DataSet();
                        dsChkOredrIsEcomOrNot = fillds("select * from tOrderHead where orderType in('PreOrder','NormalOrder') and id=" + RequestID + " ", conn);
                        if (dsChkOredrIsEcomOrNot.Tables[0].Rows.Count > 0)
                        {
                            // GWC_SP_GetRequestHeadByRequestIDs_Result EcomMsg = new GWC_SP_GetRequestHeadByRequestIDs_Result();
                            MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetailForEcomm(Request);
                            MailBody = MailBody + "<br/><br/>" + EmailGetAddressDetailsEcom(Request, conn);
                            MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetailEcommerce(RequestID, conn);
                        }
                        else
                        {
                            if (ISProjectSite == "Yes")// if (Request.parentid == 10266)
                            {
                                MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetailVodafoneTechnical(Request, conn);
                            }
                            else
                            {
                                MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                            }
                            // MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                            MailBody = MailBody + "<br/><br/>" + EmailGetAddressDetails(Request, conn);
                            MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetail(RequestID, conn);
                        }
                        SendMail(MailBody + MailGetFooter(), MailSubject, UserEmailnew);
                        SaveCorrespondsData_New(RequestID, MailSubject, MailBody, DepartmentID, Status, conn);
                    }
                }
            }
            catch { }
            finally { }
        }


        protected void AdditionalDistribution(long RequestID, long TemplateID, string[] conn)
        {
            try
            {
                DataSet dsAddDist = new DataSet();
                string Usermail = "";
                dsAddDist = fillds("select * from GWV_VW_DistributionList where TemplateID=" + TemplateID + "", conn);
                int cnt = dsAddDist.Tables[0].Rows.Count;
                if (cnt > 0)
                {
                    for (int i = 0; i <= cnt - 1; i++)
                    {
                        if (i == 0)
                        {
                            Usermail = dsAddDist.Tables[0].Rows[i]["EmailID"].ToString();
                        }
                        else
                        {
                            Usermail = Usermail + "," + dsAddDist.Tables[0].Rows[i]["EmailID"].ToString();
                        }

                    }
                    string MailSubject = "";
                    string MailBody = "";
                    using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
                    {
                        GWC_SP_GetRequestHeadByRequestIDs_Result Request = new GWC_SP_GetRequestHeadByRequestIDs_Result();
                        Request = db.GWC_SP_GetRequestHeadByRequestIDs(RequestID.ToString()).FirstOrDefault();
                        long Orderstatus = long.Parse(Request.Status.ToString());
                        long DepartmentID = long.Parse(Request.SiteID.ToString());
                        long Status = long.Parse(Request.Status.ToString());
                        string ISProjectSite = "";
                        ISProjectSite = ISProjectSiteDetailsNew(Convert.ToString(Request.parentid), conn);
                        string AdDistName = "AddDistribution User";
                        string UserEmailnew = "";
                        string[] eorder = Usermail.Split(',');
                        var distinctArray = eorder.Distinct().ToArray();
                        for (int s = 0; s <= distinctArray.Count() - 1; s++)
                        {
                            if (s == 0)
                            {
                                UserEmailnew = distinctArray[s].ToString();
                            }
                            else
                            {
                                UserEmailnew = UserEmailnew + "," + distinctArray[s].ToString();
                            }
                        }

                        DataSet dsMailSubBody = new DataSet();
                        // dsMailSubBody = fillds("select * from mMessageEMailTemplates where ActivityID=(select Id from mDropdownValues where StatusID=2) and MessageID=(select Id from mDropdownValues where Value='Action') and DepartmentID=" + DepartmentID + "", conn);

                        dsMailSubBody = fillds("select * from mMessageEMailTemplates where ID=" + TemplateID + " ", conn);

                        if (Status == 2)
                        {
                            MailSubject = dsMailSubBody.Tables[0].Rows[0]["MailSubject"].ToString() + ", Order No # " + Request.OrderNo + "";
                        }
                        else
                        {
                            string CrntStatus = Request.RequestStatus.ToString();
                            MailSubject = "Order " + CrntStatus + ", Order No # " + Request.OrderNo + "";
                        }

                        MailBody = "Dear " + AdDistName + ", <br/>";
                        MailBody = MailBody + dsMailSubBody.Tables[0].Rows[0]["MailBody"].ToString();

                        DataSet dsChkOredrIsEcomOrNot = new DataSet();
                        dsChkOredrIsEcomOrNot = fillds("select * from tOrderHead where orderType in('PreOrder','NormalOrder') and id=" + RequestID + " ", conn);
                        if (dsChkOredrIsEcomOrNot.Tables[0].Rows.Count > 0)
                        {
                            // GWC_SP_GetRequestHeadByRequestIDs_Result EcomMsg = new GWC_SP_GetRequestHeadByRequestIDs_Result();
                            MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetailForEcomm(Request);
                            MailBody = MailBody + "<br/><br/>" + EmailGetAddressDetailsEcom(Request, conn);
                            MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetailEcommerce(RequestID, conn);
                        }
                        else
                        {
                            if (ISProjectSite == "Yes")// if (Request.parentid == 10266)
                            {
                                MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetailVodafoneTechnical(Request, conn);
                            }
                            else
                            {
                                MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                            }
                            // MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                            MailBody = MailBody + "<br/><br/>" + EmailGetAddressDetails(Request, conn);
                            MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetail(RequestID, conn);
                        }
                        SendMail(MailBody + MailGetFooter(), MailSubject, UserEmailnew);
                        SaveCorrespondsData(RequestID, MailSubject, MailBody, DepartmentID, Status, conn);
                    }
                }
            }
            catch { }
            finally { }
        }


        protected void EmailSendofApproved(long RequestID, string[] conn)
        {
            try
            {
                string MailSubject;
                string MailBody;
                //int E_ID;

                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                POR_SP_GetRequestByRequestIDs_Result Request = new POR_SP_GetRequestByRequestIDs_Result();
                Request = db.POR_SP_GetRequestByRequestIDs(RequestID.ToString()).FirstOrDefault();
                string partdetail = EMailGetRequestPratDetail(Request.PRH_ID, Request.SiteID, conn);


                /*Notification Email to Requestor*/
                //MailSubject = "Approved : Part Request No. " + Request.RequestNo + "[ " + Request.RequestType + " ] Site : " + Request.SiteName + "";
                MailSubject = "Approval Notification : Material Request for " + Request.SiteName + " - ID " + Request.PRH_ID.ToString() + " is Approved";
                MailBody = " Hello, <br/><b> " + Request.RequestByUserName + " </b> <br/><br/>" +
                           " This is an automatically generated message in reference to a Material request for " + Request.SiteName + " - ID " + Request.PRH_ID.ToString() +
                           " Approval has been received from Operation Manager." +
                           " <br/>" +
                           " Cummins Team will now begin work on this request.  If you have questions or comments, please contact the Operation Manager directly." +
                           " Your request details are provided below : ";
                //  MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail((Request,conn);

                MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetail(RequestID, conn);
                MailBody = MailBody + partdetail;
                SendMail(MailBody + MailGetFooter(), MailSubject, EmailGetEmailIDsByUserID(Convert.ToInt64(Request.RequestBy), conn));


                //  SaveInboxData(Convert.ToInt64(Request.RequestBy), Request.SiteID, "Material Request", MailSubject, MailBody, Convert.ToInt64(Request.StatusID), conn);
                /*End*/

                string[] MailTo = new string[] { };
                MailTo = new string[] { };
                MailTo = EmailGetEmailIDsBySiteIDApprovalLevel(1, 0, conn);
                string[] MailToName = MailTo[0].Split('|');
                string[] MailToEmailID = MailTo[1].Split(',');
                for (int i = 0; i < MailToName.Count(); i++)
                {
                    /*Information Email to ProjectLead */
                    MailSubject = "Pending for Issue : Material Request for " + Request.SiteName + " - ID " + Request.PRH_ID.ToString() + " is Approved";
                    MailBody = " Hello,<br/> <b> " + MailToName[i] + " </b> <br/><br/>" +
                               " This is an automatically generated message in reference to a Material request for " + Request.SiteName + " - ID " + Request.PRH_ID.ToString() +
                               " Approval has been received from Operation Manager." +
                               " <br/>" +
                               " Now it's pending for issue." +
                               " Part Request details are provided below : ";
                    //MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                    MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetail(RequestID, conn);
                    MailBody = MailBody + partdetail;
                    /*End*/
                    SendMail(MailBody + MailGetFooter(), MailSubject, MailToEmailID[i]);


                    //E_ID = Convert.ToInt32(GetIDFromEmailName(MailTo[0], MailTo[1], conn));
                    //SaveInboxData(E_ID, Request.SiteID, "Material Request", MailSubject, MailBody, Convert.ToInt64(Request.StatusID), conn);
                }


                MailTo = new string[] { };
                MailTo = EmailGetEmailIDsBySiteIDApprovalLevel(Request.SiteID, 1, conn);
                MailToName = new string[] { };
                MailToEmailID = new string[] { };
                MailToName = MailTo[0].Split('|');
                MailToEmailID = MailTo[1].Split(',');
                for (int i = 0; i < MailToName.Count(); i++)
                {
                    /*Acknowledgement Email to OperationManager when approved */
                    MailSubject = "Approval Acknowledgement : Material Request for " + Request.SiteName + " - ID " + Request.PRH_ID.ToString() + " is Approved";
                    MailBody = " Hello,<br/> <b> " + MailToName[i] + " </b> <br/><br/>" +
                               " This is an automatically generated message in reference to a Material request for " + Request.SiteName + " - ID " + Request.PRH_ID.ToString() +
                               " Thank you for giving approval." +
                               " <br/>" +
                               " Cummins Team will now begin work on this request." +
                               " Part Request details are provided below : ";
                    // MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                    MailBody = MailBody + partdetail;
                    /*End*/
                    SendMail(MailBody + MailGetFooter(), MailSubject, MailToEmailID[i]);


                    // E_ID = Convert.ToInt32(GetIDFromEmailName(MailTo[0], MailTo[1], conn));
                    //SaveInboxData(E_ID, Request.SiteID, "Material Request", MailSubject, MailBody, Convert.ToInt64(Request.StatusID), conn);
                }


            }
            catch { }
            finally { }
        }

        protected void EMailSendWhenRequestRejected(long RequestID, string[] conn)
        {
            try
            {
                string MailSubject;
                string MailBody;
                int E_ID;

                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                POR_SP_GetRequestByRequestIDs_Result Request = new POR_SP_GetRequestByRequestIDs_Result();
                Request = db.POR_SP_GetRequestByRequestIDs(RequestID.ToString()).FirstOrDefault();
                string partdetail = EMailGetRequestPratDetail(Request.PRH_ID, Request.SiteID, conn);

                /*Notification Email to Requestor*/
                MailSubject = "Rejection Notification : Material Request for " + Request.SiteName + " - ID " + Request.PRH_ID.ToString() + " is Rejected";
                MailBody = " Hello, <br/><b> " + Request.RequestByUserName + " </b> <br/><br/>" +
                           " This is an automatically generated message in reference to a Material request for " + Request.SiteName + " - ID " + Request.PRH_ID.ToString() +
                           " Material Request has been rejected by Operation Manager" +
                           " <br/>" +
                           " If you have questions or comments, please contact the Operation Manager directly." +
                           " Your request details are provided below : ";
                //MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                MailBody = MailBody + partdetail;
                SendMail(MailBody + MailGetFooter(), MailSubject, EmailGetEmailIDsByUserID(Convert.ToInt64(Request.RequestBy), conn));


                //   SaveInboxData(Convert.ToInt64(Request.RequestBy), Request.SiteID, "Material Request", MailSubject, MailBody, Convert.ToInt64(Request.StatusID), conn);
                /*End*/

                string[] MailTo = new string[] { };
                MailTo = new string[] { };
                MailTo = EmailGetEmailIDsBySiteIDApprovalLevel(1, 0, conn);
                string[] MailToName = MailTo[0].Split('|');
                string[] MailToEmailID = MailTo[1].Split(',');
                for (int i = 0; i < MailToName.Count(); i++)
                {
                    /*Information Email to ProjectLead */
                    MailSubject = "Rejection Notification : Material Request for " + Request.SiteName + " - ID " + Request.PRH_ID.ToString() + " is Rejected";
                    MailBody = " Hello,<br/> <b> " + MailToName[i] + " </b> <br/><br/>" +
                               " This is an automatically generated message in reference to a Material request for " + Request.SiteName + " - ID " + Request.PRH_ID.ToString() +
                               " Material Request has been rejected by Operation Manager" +
                               " <br/>" +
                               " If you have questions or comments, please contact the Operation Manager directly." +
                               " Part Request details are provided below : ";
                    // MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                    MailBody = MailBody + partdetail;
                    /*End*/
                    SendMail(MailBody + MailGetFooter(), MailSubject, MailToEmailID[i]);


                    //   E_ID = Convert.ToInt32(GetIDFromEmailName(MailTo[0], MailTo[1], conn));
                    //   SaveInboxData(E_ID, Request.SiteID, "Material Request", MailSubject, MailBody, Convert.ToInt64(Request.StatusID), conn);
                }



                MailTo = new string[] { };
                MailTo = EmailGetEmailIDsBySiteIDApprovalLevel(Request.SiteID, 1, conn);
                MailToName = new string[] { };
                MailToEmailID = new string[] { };
                MailToName = MailTo[0].Split('|');
                MailToEmailID = MailTo[1].Split(',');
                for (int i = 0; i < MailToName.Count(); i++)
                {
                    /*Acknowledgement Email to OperationManager when Rejected */
                    MailSubject = "Rejection Acknowledgement : Material Request for " + Request.SiteName + " - ID " + Request.PRH_ID.ToString() + " is Rejected";
                    MailBody = " Hello,<br/> <b> " + MailToName[i] + " </b> <br/><br/>" +
                               " This is an automatically generated message in reference to a Material request for " + Request.SiteName + " - ID " + Request.PRH_ID.ToString() +
                               " The request has been rejected by you." +
                               " <br/>" +
                               " Cummins Team will now begin work on this request.";
                    // MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                    MailBody = MailBody + partdetail;
                    /*End*/
                    SendMail(MailBody + MailGetFooter(), MailSubject, MailToEmailID[i]);


                    //  E_ID = Convert.ToInt32(GetIDFromEmailName(MailTo[0], MailTo[1], conn));
                    //  SaveInboxData(E_ID, Request.SiteID, "Material Request", MailSubject, MailBody, Convert.ToInt64(Request.StatusID), conn);
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

        #region GWC
        //protected DataSet fillds(String strquery, string[] conn)
        //{
        //    BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
        //    DataSet ds = new DataSet();

        //    System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection("Data Source=" + conn[0] + ";Initial Catalog=" + conn[1] + "; User ID=" + conn[3] + "; Password=" + conn[2] + ";");
        //    SqlDataAdapter da = new SqlDataAdapter(strquery, sqlConn);
        //    ds.Reset();
        //    da.Fill(ds);
        //    return ds;
        //}

        protected DataSet fillds(string strquery, string[] conn)
        {
            using (DataSet ds = new DataSet())
            {
                using (System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection("Data Source=" + conn[0] + ";Initial Catalog=" + conn[1] + "; User ID=" + conn[3] + "; Password=" + conn[2] + ";"))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(strquery, sqlConn))
                    {
                        try
                        {
                            ds.Reset();
                            sqlConn.Open();
                            da.Fill(ds);
                            sqlConn.Close();
                        }
                        catch
                        {
                            sqlConn.Close();
                        }
                    }
                }
                return ds;
            }
        }



        public DataSet GetTemplateDetails(long UserID, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from VW_GetTemplateDetails where createdBy=" + UserID + "   union select * from VW_GetTemplateDetails where createdBy!=" + UserID + "  and Accesstype='Public' ", conn);
            return ds;
        }

        public DataSet GetTemplateDetailsSuperAdmin(string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from VW_GetTemplateDetails", conn);
            return ds;
        }

        public DataSet GetTemplateDetailsAdmin(long UserID, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from VW_GetTemplateDetails where Department in (select TerritoryID from mUserTerritoryDetail where UserID=" + UserID + ")", conn);
            return ds;
        }

        public DataSet GetTemplateDetailsBind(long UserID, long DeptID, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from VW_GetTemplateDetails where createdBy=" + UserID + " and Department=" + DeptID + "  union select * from VW_GetTemplateDetails where createdBy!=" + UserID + "  and Accesstype='Public' and Department=" + DeptID + "", conn);
            return ds;
        }

        //public DataSet GetTemplateDetailsBind(long UserID, long DeptID, string[] conn)
        //{
        //    DataSet ds = new DataSet();
        //    ds = fillds("select * from VW_GetTemplateDetails where createdBy=" + UserID + " and Department=" + DeptID + "  union select * from VW_GetTemplateDetails where createdBy!=" + UserID + "  and Accesstype='Public' and Department=" + DeptID + " ", conn);
        //    return ds;
        //}

        public DataSet GetGetInterfaceDetails(string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from mInterfaceMap", conn);
            return ds;
        }

        public DataSet GetGetMessageDetails(string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from mMessageHeader", conn);
            return ds;
        }

        public DataSet GetApprovalDetailsNew(long OrderID, long ApproverID, string[] conn)
        {
            DataSet ds = new DataSet();
            // ds = fillds("select * from VW_ApprovalTransDetails where (DeligateTo=" + ApproverID + " or ApproverID=" + ApproverID + ") and OrderID=" + OrderID + "", conn);
            //ds = fillds("select * from VW_ApprovalTransDetails where (DeligateTo=" + ApproverID + " or ApproverID=" + ApproverID + ")  and OrderID=" + OrderID + "  union all select * from VW_ApprovalTransDetails  where   OrderID=" + OrderID + " and  Status=3 ", conn);
            // ds = fillds("select distinct id,OrderId,StoreId,UserId,UserName,EmailID,ApprovalId,ApproverID,ApproverName,ApproverEmailID,Status,StatusName,Date,Remark,DeligateFrom,DeligateUser,DeligateTo,ImgApproval,ImgReject,ImgApprovewithRevision from (select * from VW_ApprovalTransDetails where (DeligateTo=" + ApproverID + " or ApproverID=" + ApproverID + ")  and OrderID=" + OrderID + "  union all select * from VW_ApprovalTransDetails  where   OrderID=" + OrderID + " and  Status in(3,24,4))aaa ", conn);

            ds = fillds("select distinct id,OrderId,OrderCurrentStatus,StoreId,UserId,UserName,EmailID,ApprovalId,ApproverID,ApproverName,ApproverEmailID,Status,StatusName,Date,Remark,DeligateFrom,DeligateUser,DeligateTo,case when OrderCurrentStatus IN(4,10) then 'gray' else ImgApproval end ImgApproval,case when OrderCurrentStatus IN(4,10) then 'gray' else ImgReject end ImgReject,case when OrderCurrentStatus IN(4,10) then 'gray' else ImgApprovewithRevision end ImgApprovewithRevision from (select * from VW_ApprovalTransDetails where (DeligateTo=" + ApproverID + " or ApproverID=" + ApproverID + ")  and OrderID=" + OrderID + "  union all select * from VW_ApprovalTransDetails  where   OrderID=" + OrderID + " and  Status in(3,24,4))aaa ", conn);
            return ds;
        }

        public DataSet GetApprovalDetailsNewAdmin(long OrderID, string[] conn)
        {
            DataSet ds = new DataSet();
            //ds = fillds("select distinct id,OrderId,StoreId,UserId,UserName,EmailID,ApprovalId,ApproverID,ApproverName,ApproverEmailID,Status,StatusName,Date,Remark,DeligateFrom,DeligateUser,DeligateTo,ImgApproval,ImgReject,ImgApprovewithRevision from (select * from VW_ApprovalTransDetails  where   OrderID=" + OrderID + " )aaa ", conn);
            ds = fillds("select distinct id,OrderId,OrderCurrentStatus,StoreId,UserId,UserName,EmailID,ApprovalId,ApproverID,ApproverName,ApproverEmailID,Status,StatusName,Date,Remark,DeligateFrom,DeligateUser,DeligateTo,case when OrderCurrentStatus IN(4,10) then 'gray' else ImgApproval end ImgApproval,case when OrderCurrentStatus IN(4,10) then 'gray' else ImgReject end ImgReject,case when OrderCurrentStatus IN(4,10) then 'gray' else ImgApprovewithRevision end ImgApprovewithRevision from (select * from VW_ApprovalTransDetails  where   OrderID=" + OrderID + " )aaa ", conn);
            return ds;
        }

        public DataSet GetApprovalDetailsAllApproved(string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select 'Approver Level1' as ApproverLevel, 'Bispl Admin1' as ApproverName, '26/08/2016' as  ApprovedDate ,'Approved' as Remark,'Approved' as Status,'green' as ImgApproval union select 'Approver Level2' as ApproverLevel, 'Bispl Admin2' as ApproverName, '27/08/2016' as  ApprovedDate ,'Approved' as Remark,'Approved' as Status,'green' as ImgApproval union select 'Approver Level3' as ApproverLevel, 'Bispl Admin3' as ApproverName, '28/08/2016' as  ApprovedDate ,'Approved' as Remark,'Approved' as Status,'green' as ImgApproval ", conn);
            return ds;
        }

        public DataSet GetUOMofSelectedProduct(int ProdID, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select PackUomid,SkuId,ShortDescri,Description,Quantity,UOMID,Sequence, (CONVERT(VARCHAR(15),UOMID) + ':' + CONVERT(VARCHAR(15),Quantity)) as UMOGroup from VW_SkuUOMDetails where SkuId=" + ProdID + " and quantity !=0 order by Sequence ", conn);
            return ds;
        }

        public long SetIntotOrderHead_New(long CreatedBy, string Creationdate, long Id, long LastModifiedBy, string LastModifiedDt, long StoreId, string OrderNumber,
                                        string Priority, long Status,string Title, string Orderdate, long RequestBy, string Remark, string Deliverydate, long ContactId1, 
                                        long AddressId, string Con2, long PaymentID,decimal TotalQty, decimal GrandTotal, long LocationID, long ProjTypeID,
                                        long SiteID, string ASNNo, string RefundCode, string OrderNo, string orderType, long segmentID, string[] conn)
        {
            SqlDataReader dr;
            long headId = 0;
            try
            {
              
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_SetIntotOrderHead";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                    cmd.Parameters.AddWithValue("@Creationdate", Creationdate);
                    cmd.Parameters.AddWithValue("@Id", Id);
                    cmd.Parameters.AddWithValue("@LastModifiedBy", LastModifiedBy);
                    cmd.Parameters.AddWithValue("@LastModifiedDt", LastModifiedDt);
                    cmd.Parameters.AddWithValue("@StoreId", StoreId);
                    cmd.Parameters.AddWithValue("@OrderNumber", OrderNumber);
                    cmd.Parameters.AddWithValue("@Priority", Priority);
                    cmd.Parameters.AddWithValue("@Status", Status);
                    cmd.Parameters.AddWithValue("@Title", Title);
                    cmd.Parameters.AddWithValue("@Orderdate", Orderdate);
                    cmd.Parameters.AddWithValue("@RequestBy", RequestBy);
                    cmd.Parameters.AddWithValue("@Remark", Remark);
                    cmd.Parameters.AddWithValue("@Deliverydate", Deliverydate);
                    cmd.Parameters.AddWithValue("@ContactId1", ContactId1);
                    cmd.Parameters.AddWithValue("@AddressId", AddressId);
                    cmd.Parameters.AddWithValue("@Con2", Con2);
                    cmd.Parameters.AddWithValue("@PaymentID", PaymentID);
                    cmd.Parameters.AddWithValue("@TotalQty", TotalQty);
                    cmd.Parameters.AddWithValue("@GrandTotal", GrandTotal);
                    cmd.Parameters.AddWithValue("@LocationID", LocationID);
                    cmd.Parameters.AddWithValue("@ProjTypeID", ProjTypeID);
                    cmd.Parameters.AddWithValue("@SiteID", SiteID);
                    cmd.Parameters.AddWithValue("@ASNNo", ASNNo);
                    cmd.Parameters.AddWithValue("@RefundCode", RefundCode);
                    cmd.Parameters.AddWithValue("@OrderNo", OrderNo);
                    cmd.Parameters.AddWithValue("@orderType", orderType);
                    cmd.Parameters.AddWithValue("@segmentID", segmentID);
                    dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        headId = long.Parse(dr["OrderId"].ToString());
                    }
                    dr.Close();
                    cmd.Connection.Close();
                }
            }
            catch (System.Exception ex)
            {
                
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "Sp_EnterErrorTracking";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("Data", ex.Data.ToString());
                    cmd.Parameters.AddWithValue("GetType", ex.Source.ToString());
                    cmd.Parameters.AddWithValue("InnerException", "Error");
                    cmd.Parameters.AddWithValue("Message", ex.Message.ToString());
                    cmd.Parameters.AddWithValue("Source", headId + "| SetIntotOrderHead_New");
                    cmd.Parameters.AddWithValue("DateTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("UserID", Convert.ToString(CreatedBy));
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
                headId = 0;
            }
            
            return headId;
        }

        public long SetIntotOrderHead(tOrderHead PartRequest, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                if (PartRequest.Id == 0)
                {
                    db.tOrderHeads.AddObject(PartRequest);
                }
                else
                {
                    //string ONO = "";
                    //DataSet dsONO = new DataSet();
                    //dsONO = fillds("select OrderNo from torderhead where id="+ PartRequest.Id +"", conn);
                    //ONO = dsONO.Tables[0].Rows[0]["OrderNo"].ToString();
                    //PartRequest.OrderNo = ONO;
                    db.tOrderHeads.Attach(PartRequest);
                    db.ObjectStateManager.ChangeObjectState(PartRequest, EntityState.Modified);

                    tOrderHeadHistory OH = new tOrderHeadHistory();
                    OH.Id = PartRequest.Id;
                    OH.OrderNumber = PartRequest.OrderNumber;
                    OH.StoreId = PartRequest.StoreId;
                    OH.Orderdate = PartRequest.Orderdate;
                    OH.Deliverydate = PartRequest.Deliverydate;
                    OH.Priority = PartRequest.Priority;
                    OH.ContactId1 = PartRequest.ContactId1;
                    OH.ContactId2 = PartRequest.ContactId2;
                    OH.AddressId = PartRequest.AddressId;
                    OH.Remark = PartRequest.Remark;
                    OH.Status = PartRequest.Status;
                    OH.CreatedBy = PartRequest.CreatedBy;
                    OH.Creationdate = PartRequest.Creationdate;
                    OH.Title = PartRequest.Title;
                    OH.RequestBy = PartRequest.RequestBy;

                    db.tOrderHeadHistories.Attach(OH);
                }
                db.SaveChanges();
                return PartRequest.Id;
            }
        }

        public long InsertIntomRequestTemplateHead(mRequestTemplateHead ReqTemplHead, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            if (ReqTemplHead.ID == 0)
            {
                db.mRequestTemplateHeads.AddObject(ReqTemplHead);
            }
            else
            {
                db.mRequestTemplateHeads.Attach(ReqTemplHead);
                db.ObjectStateManager.ChangeObjectState(ReqTemplHead, EntityState.Modified);
            }
            db.SaveChanges();
            return ReqTemplHead.ID;
        }


        public void FinalSavemRequestTemplateDetail(string paraSessionID, string paraCurrentObjectName, long paraReferenceID, string paraUserID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<POR_SP_GetPartDetail_ForRequest_Result> finalSaveLst = new List<POR_SP_GetPartDetail_ForRequest_Result>();
            finalSaveLst = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, paraCurrentObjectName, conn);

            XElement xmlEle = new XElement("Request", from rec in finalSaveLst
                                                      select new XElement("PartList",
                                                      new XElement("PRH_ID", paraReferenceID),
                                                      new XElement("Prod_ID", Convert.ToInt64(rec.Prod_ID)),
                                                      new XElement("RequestQty", Convert.ToDecimal(rec.RequestQty)),
                                                      new XElement("UOMID", Convert.ToInt64(rec.UOMID)),
                                                      new XElement("Price", Convert.ToDecimal(rec.Price)),
                                                      new XElement("Total", Convert.ToDecimal(rec.Total)),
                                                      new XElement("IsPriceChange", Convert.ToInt16(rec.IsPriceChange))
                                                      ));

            ObjectParameter _PRH_ID = new ObjectParameter("PRH_ID", typeof(long));
            _PRH_ID.Value = paraReferenceID;

            ObjectParameter _xmlData = new ObjectParameter("xmlData", typeof(string));
            _xmlData.Value = xmlEle.ToString();


            ObjectParameter[] obj = new ObjectParameter[] { _PRH_ID, _xmlData };
            db.ExecuteFunction("POR_SP_mRequestTemplateDetail", obj);

            //ClearTempDataFromDB(paraSessionID, paraUserID, paraCurrentObjectName, conn);
        }

        public void FinalSavemRequestTemplateDetailTemplate(string paraSessionID, string paraCurrentObjectName, long paraReferenceID, string paraUserID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<POR_SP_GetPartDetail_ForRequest_Result> finalSaveLst = new List<POR_SP_GetPartDetail_ForRequest_Result>();
            finalSaveLst = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, paraCurrentObjectName, conn);

            XElement xmlEle = new XElement("Request", from rec in finalSaveLst
                                                      select new XElement("PartList",
                                                      new XElement("PRH_ID", paraReferenceID),
                                                      new XElement("Prod_ID", Convert.ToInt64(rec.Prod_ID)),
                                                      new XElement("RequestQty", Convert.ToDecimal(rec.RequestQty)),
                                                      new XElement("UOMID", Convert.ToInt64(rec.UOMID)),
                                                      new XElement("Price", Convert.ToDecimal(rec.Price)),
                                                      new XElement("Total", Convert.ToDecimal(rec.Total)),
                                                      new XElement("IsPriceChange", Convert.ToInt16(rec.IsPriceChange))
                                                      ));

            ObjectParameter _PRH_ID = new ObjectParameter("PRH_ID", typeof(long));
            _PRH_ID.Value = paraReferenceID;

            ObjectParameter _xmlData = new ObjectParameter("xmlData", typeof(string));
            _xmlData.Value = xmlEle.ToString();


            ObjectParameter[] obj = new ObjectParameter[] { _PRH_ID, _xmlData };
            db.ExecuteFunction("POR_SP_mRequestTemplateDetail", obj);

            ClearTempDataFromDB(paraSessionID, paraUserID, paraCurrentObjectName, conn);
        }

        public mRequestTemplateHead GetTemplateOrderHead(long TemplateID, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                mRequestTemplateHead TempHead = new mRequestTemplateHead();
                TempHead = db.mRequestTemplateHeads.Where(t => t.ID == TemplateID).FirstOrDefault();
                if (TempHead != null)
                {
                    db.mRequestTemplateHeads.Detach(TempHead);
                }
                return TempHead;
            }
        }

        public DataSet GetTemplatePartLstByTemplateID(long TemplateID, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from mRequestTemplateDetail where Templateheadid=" + TemplateID + "", conn);
            return ds;
        }

        public void UpdatetApprovalTransAfterApproval(long ApprovalID, long RequestID, long statusID, string Remark, long ApproverID, string InvoiceNo, string Tag, string ismoney, string ERPfinacial, string[] conn)
        {
            try
            {
                using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
                {

                    /*Update InvoiceNo in tOrderHead*/
                    if (InvoiceNo != "")
                    {
                        DataSet dsEditAccessRemove = new DataSet();
                        dsEditAccessRemove = fillds("update torderhead set InvoiceNo='" + InvoiceNo + "' where id=" + RequestID + "", conn);
                    }
                    /*Update InvoiceNo in tOrderHead*/
                    DataSet dsDeliverytype = new DataSet();
                    string DeliveryType = "", OrderType = "", Ecomm = "No";
                    dsDeliverytype = fillds("select DeliveryType, OrderType from tecommhead where orderid = " + RequestID + "", conn);
                    if (dsDeliverytype.Tables[0].Rows.Count > 0)
                    {
                        Ecomm = "Yes";
                        DeliveryType = dsDeliverytype.Tables[0].Rows[0]["DeliveryType"].ToString();
                        OrderType = dsDeliverytype.Tables[0].Rows[0]["OrderType"].ToString();
                    }


                    //get exp delivery date date when order is approved
                    DateTime expdate = DateTime.Today;
                    expdate = GetDeiverydate(RequestID, DeliveryType, ApproverID, conn);
                    //new change for ecommerce order only for SIM only and Hub                     

                    if (DeliveryType.ToUpper() == "HUB" && OrderType.ToUpper() == "SIM ONLY")
                    {//order disptch

                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "GWC_SP_OrderDispatchStatus";
                            cmd.Connection = svr.GetSqlConn(conn);
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("ApprovalID", ApprovalID);
                            cmd.Parameters.AddWithValue("ApproverID", ApproverID);
                            cmd.Parameters.AddWithValue("RequestID", RequestID);
                            cmd.Parameters.AddWithValue("Remark", Remark);
                            cmd.Parameters.AddWithValue("expdate", expdate);
                            cmd.Parameters.AddWithValue("Tag", Tag);
                            cmd.ExecuteNonQuery();
                        }

                        EmailSendofApproved(Convert.ToInt64(ApproverID), RequestID, 0, conn);

                        // AddIntomMessageTransForEcommerce(RequestID, conn);
                    }
                    else if (DeliveryType.ToUpper() == "HOME" && OrderType.ToUpper() == "FIXED ONLY")
                    {
                        //order disptch

                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "GWC_SP_OrderDispatchStatus";
                            cmd.Connection = svr.GetSqlConn(conn);
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("ApprovalID", ApprovalID);
                            cmd.Parameters.AddWithValue("ApproverID", ApproverID);
                            cmd.Parameters.AddWithValue("RequestID", RequestID);
                            cmd.Parameters.AddWithValue("Remark", Remark);
                            cmd.Parameters.AddWithValue("expdate", expdate);
                            cmd.Parameters.AddWithValue("Tag", Tag);
                            cmd.ExecuteNonQuery();
                        }
                        EmailSendofApproved(Convert.ToInt64(ApproverID), RequestID, 0, conn);

                    }
                    else if (OrderType.ToUpper() == "FMS INDOOR")
                    {
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "GWC_SP_OrderApprovedStatus";
                            cmd.Connection = svr.GetSqlConn(conn);
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("ApprovalID", ApprovalID);
                            cmd.Parameters.AddWithValue("ApproverID", ApproverID);
                            cmd.Parameters.AddWithValue("RequestID", RequestID);
                            cmd.Parameters.AddWithValue("Remark", Remark);
                            cmd.Parameters.AddWithValue("expdate", expdate);
                            cmd.Parameters.AddWithValue("Tag", Tag);
                            cmd.ExecuteNonQuery();
                        }
                        EmailSendofApproved(Convert.ToInt64(ApproverID), RequestID, 0, conn);
                        AddIntomMessageTrans(RequestID, conn);
                        //UpdateIseApprflag(RequestID, conn);
                        UpdateTProductStockReserveQtyTotalDispatchQty(RequestID, conn);
                    }
                    else if (DeliveryType.ToUpper() == "HOME" && OrderType.ToUpper() == "FMS OUTDOOR")
                    {
                        //order status as disptch and stock not manage.
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "GWC_SP_OrderDispatchStatus";
                            cmd.Connection = svr.GetSqlConn(conn);
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("ApprovalID", ApprovalID);
                            cmd.Parameters.AddWithValue("ApproverID", ApproverID);
                            cmd.Parameters.AddWithValue("RequestID", RequestID);
                            cmd.Parameters.AddWithValue("Remark", Remark);
                            cmd.Parameters.AddWithValue("expdate", expdate);
                            cmd.Parameters.AddWithValue("Tag", Tag);
                            cmd.ExecuteNonQuery();
                        }
                        EmailSendofApproved(Convert.ToInt64(ApproverID), RequestID, 0, conn);
                    }
                    else
                    {
                        //change tOrderwiseAccess to 0 after approve order
                        tOrderWiseAccess OrdrAccess = new tOrderWiseAccess();
                        OrdrAccess = db.tOrderWiseAccesses.Where(a => a.OrderID == RequestID && a.UserApproverID == ApproverID && a.ApprovalLevel == ApprovalID).FirstOrDefault();
                        if (OrdrAccess != null)
                        {
                            db.tOrderWiseAccesses.Detach(OrdrAccess);
                            OrdrAccess.PriceEdit = false;
                            OrdrAccess.SkuQtyEdit = false;
                            db.tOrderWiseAccesses.Attach(OrdrAccess);
                            db.ObjectStateManager.ChangeObjectState(OrdrAccess, EntityState.Modified);
                            db.SaveChanges();
                        }
                        tApprovalTran rec = new tApprovalTran();
                        //rec = db.tApprovalTrans.Where(a => a.Id == ApprovalID && a.OrderId == RequestID && a.ApproverID == ApproverID).FirstOrDefault();

                        DataSet dsDeligateApprover = new DataSet();
                        long DeligateFrom = 0;

                        //old query dsDeligateApprover = fillds("select * from VW_ApprovalTransDetails where   OrderID=" + RequestID + " and DeligateTo=" + ApproverID + " and Status!=3", conn);
                        //new query updated by suraj j. 10-02-2020
                        dsDeligateApprover = fillds("select * from VW_ApprovalTransDetailsNEW where   OrderID=" + RequestID + " and DeligateTo=" + ApproverID + " and Status!=3", conn);
                        if (dsDeligateApprover.Tables[0].Rows.Count > 0)
                        {
                            DeligateFrom = long.Parse(dsDeligateApprover.Tables[0].Rows[0]["DeligateFrom"].ToString());
                            ApproverID = DeligateFrom;

                            rec = db.tApprovalTrans.Where(a => a.OrderId == RequestID && a.ApproverID == ApproverID).FirstOrDefault();   //Change Add ApprovalId
                            if (rec != null)
                            {
                                db.tApprovalTrans.Detach(rec);
                                rec.Remark = Remark;
                                rec.Date = DateTime.Now;
                                rec.Status = statusID;
                                rec.Tag = Tag;
                                db.tApprovalTrans.Attach(rec);
                                db.ObjectStateManager.ChangeObjectState(rec, EntityState.Modified);
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            rec = db.tApprovalTrans.Where(a => a.OrderId == RequestID && a.ApproverID == ApproverID && a.ApprovalId == ApprovalID).FirstOrDefault();    //Change Add ApprovalId
                            if (rec != null)
                            {
                                db.tApprovalTrans.Detach(rec);
                                rec.Remark = Remark;
                                rec.Date = DateTime.Now;
                                rec.Status = statusID;
                                rec.Tag = Tag;
                                db.tApprovalTrans.Attach(rec);
                                db.ObjectStateManager.ChangeObjectState(rec, EntityState.Modified);
                                db.SaveChanges();
                            }
                        }

                        //Send mail to approver, second approver if any, second level approver(s) if any & requester

                        long DeptID = Convert.ToInt64(rec.StoreId);
                        DataSet ds1 = new DataSet();
                        string ApproverLogic = ""; int ApprovalLevel = 0;
                        // ds1 = fillds("select * from VW_ApprovalLevelDetail where DepartmentID=" + DeptID + " and UserID=" + ApproverID + "", conn);//Change  select * from tOrderWiseAccess where OrderId=1062 and UserApproverID=128  and ApprovalLevel=(select ApprovalId from tapprovalTrans where OrderId=1062 and ApproverID=128)
                        ds1 = fillds("select * from tOrderWiseAccess where OrderId=" + RequestID + " and UserApproverID=" + ApproverID + "  and ApprovalLevel=" + ApprovalID + "", conn);

                        ApproverLogic = ds1.Tables[0].Rows[0]["ApproverLogic"].ToString();
                        ApprovalLevel = Convert.ToInt16(ds1.Tables[0].Rows[0]["ApprovalLevel"].ToString());

                        DataSet ds2 = new DataSet();
                        long SecondApproverID = 0;
                        long status = 0;
                        //ds2 = fillds("select * from VW_ApprovalLevelDetail where DepartmentID=" + DeptID + " and UserID!=" + ApproverID + " and Approvallevel=" + ApprovalLevel + "", conn);//Change  select * from tOrderWiseAccess where OrderId=1062 and UserApproverID!=128 and ApprovalLevel=1
                        ds2 = fillds("select * from tOrderWiseAccess where OrderId=" + RequestID + " and UserApproverID!=" + ApproverID + " and ApprovalLevel=" + ApprovalLevel + " ", conn);
                        DataSet ds3 = new DataSet();
                        if (ds2.Tables[0].Rows.Count > 0)
                        {  //Require For Loop Here
                           //SecondApproverID = Convert.ToInt64(ds2.Tables[0].Rows[0]["UserID"].ToString());
                            SecondApproverID = Convert.ToInt64(ds2.Tables[0].Rows[0]["UserApproverID"].ToString());
                            // old query ds3 = fillds("select * from VW_ApprovalTransDetails where (DeligateTo=" + SecondApproverID + " or ApproverID=" + SecondApproverID + ") and OrderID=" + RequestID + " and ApprovalId=" + ApprovalLevel + "", conn);
                            //new query updated by suraj j. 10-02-2020
                            ds3 = fillds("select * from VW_ApprovalTransDetailsNEW where (DeligateTo=" + SecondApproverID + " or ApproverID=" + SecondApproverID + ") and OrderID=" + RequestID + " and ApprovalId=" + ApprovalLevel + "", conn);
                            status = Convert.ToInt64(ds3.Tables[0].Rows[0]["Status"].ToString());
                        }
                        else
                        {
                            // old query ds3 = fillds("select * from VW_ApprovalTransDetails where  OrderID=" + RequestID + "", conn);
                            //new query updated by suraj j. 10-02-2020
                            ds3 = fillds("select * from VW_ApprovalTransDetailsNEW where  OrderID=" + RequestID + "", conn);
                            status = Convert.ToInt64(ds3.Tables[0].Rows[0]["Status"].ToString());
                        }

                        //if (status > 2)
                        if ((status == 3) || (status == 24))
                        {
                            //if next approval level is available then get o.w. change Request status to Approved
                            DataSet ds4 = new DataSet();
                            ApprovalLevel++;
                            // ds4 = fillds("select * from VW_ApprovalLevelDetail where DepartmentID=" + DeptID + " and ApprovalLevel=" + ApprovalLevel + "", conn); //Change  select * from tOrderWiseAccess where OrderId=1062 and ApprovalLevel=1
                            ds4 = fillds("select * from tOrderWiseAccess where OrderId=" + RequestID + " and ApprovalLevel=" + ApprovalLevel + " ", conn);
                            if (ds4.Tables[0].Rows.Count > 0)
                            {
                                for (int i = 0; i <= ds4.Tables[0].Rows.Count - 1; i++)
                                {
                                    tApprovalTran AppTran = new tApprovalTran();
                                    AppTran.OrderId = RequestID;
                                    AppTran.UserId = Convert.ToInt64(ds3.Tables[0].Rows[0]["UserId"].ToString());
                                    AppTran.StoreId = DeptID;
                                    AppTran.ApprovalId = ApprovalLevel;
                                    AppTran.ApproverID = Convert.ToInt64(ds4.Tables[0].Rows[i]["UserApproverID"].ToString());   //Old UserID   New UserApproverID
                                    AppTran.Tag = Tag;
                                    if (ApprovalLevel == 2) { AppTran.Status = 21; }
                                    else if (ApprovalLevel == 3) { AppTran.Status = 22; }
                                    else { AppTran.Status = 22; }
                                    db.tApprovalTrans.AddObject(AppTran);
                                    db.SaveChanges();
                                    //Send mail to Approvers 
                                    EmailSendToApprover(Convert.ToInt64(ds4.Tables[0].Rows[i]["UserApproverID"].ToString()), RequestID, i, conn);   //Old UserID   New UserApproverID
                                }

                                /*New Change For CostCenter Approver */
                                string userType = ds4.Tables[0].Rows[0]["UserType"].ToString();

                                tOrderHead Order = new tOrderHead();
                                Order = db.tOrderHeads.Where(r => r.Id == RequestID).FirstOrDefault();
                                if (Order != null)
                                {
                                    db.tOrderHeads.Detach(Order);
                                    if (Convert.ToString(userType).Trim().ToUpper() == Convert.ToString("CostCenter Approver").Trim().ToUpper())
                                    // if (userType == "CostCenter Approver")
                                    {
                                        Order.Status = 32; //Pending For Cost Center Approver
                                    }
                                    else
                                    {
                                        if (ApprovalLevel == 2) { Order.Status = 21; }
                                        else if (ApprovalLevel == 3) { Order.Status = 22; }
                                    }
                                    // Order.Status = status;

                                    db.tOrderHeads.Attach(Order);
                                    db.ObjectStateManager.ChangeObjectState(Order, EntityState.Modified);
                                    db.SaveChanges();

                                }

                                tCorrespond Cor = new tCorrespond();
                                Cor = db.tCorresponds.Where(c => c.OrderHeadId == RequestID).FirstOrDefault();
                                if (Cor != null)
                                {
                                    db.tCorresponds.Detach(Cor);
                                    if (ApprovalLevel == 2) { Cor.CurrentOrderStatus = 21; }
                                    else if (ApprovalLevel == 3) { Cor.CurrentOrderStatus = 22; }
                                    db.tCorresponds.Attach(Cor);
                                    db.ObjectStateManager.ChangeObjectState(Cor, EntityState.Modified);
                                    db.SaveChanges();
                                }
                                //for direct order submit
                                //suraj.j commited 25/11/2019  EmailSendWhenRequestSubmit(RequestID, conn);

                            }
                            else
                            {
                                /*Add Cost Center Approver*/
                                //change request status to approved.
                                tOrderHead Order = new tOrderHead();
                                Order = db.tOrderHeads.Where(r => r.Id == RequestID).FirstOrDefault();
                                if (Order != null)
                                {
                                    db.tOrderHeads.Detach(Order);
                                    Order.Status = status;

                                    Order.ApprovalDate = DateTime.Now;


                                    db.tOrderHeads.Attach(Order);
                                    db.ObjectStateManager.ChangeObjectState(Order, EntityState.Modified);
                                    db.SaveChanges();

                                }
                                if (ismoney == "No")
                                {
                                    //Add Message into mMessageTrans Table After Approve Order
                                    DataSet dsChkOredrIsEcomOrNot = new DataSet();
                                    dsChkOredrIsEcomOrNot = fillds("select * from tOrderHead where orderType in('PreOrder','NormalOrder') and id=" + RequestID + " ", conn);


                                    if (dsChkOredrIsEcomOrNot.Tables[0].Rows.Count > 0)
                                    {  //instend of mmessagetrans we use isaprflag
                                        AddIntomMessageTransForEcommerce(RequestID, conn);
                                        //UpdateIseApprflag(RequestID, conn);
                                    }
                                    else
                                    {

                                        //instend of mmessagetrans we use isaprflag
                                         AddIntomMessageTrans(RequestID, conn);
                                        //UpdateIseApprflag(RequestID, conn);

                                    }
                                }
                                //else
                                //{
                                //}
                                //  AddIntomMessageTrans(RequestID, conn);
                                //UpdateIseApprflag(RequestID, conn);
                                //Update tProductStockDetails TotalDispatchQty
                                UpdateTProductStockReserveQtyTotalDispatchQty(RequestID, conn);

                                tCorrespond Cor = new tCorrespond();
                                Cor = db.tCorresponds.Where(c => c.OrderHeadId == RequestID).FirstOrDefault();
                                if (Cor != null)
                                {
                                    db.tCorresponds.Detach(Cor);
                                    Cor.CurrentOrderStatus = status;

                                    db.tCorresponds.Attach(Cor);
                                    db.ObjectStateManager.ChangeObjectState(Cor, EntityState.Modified);
                                    db.SaveChanges();
                                }

                                //Send Email of Status Approved      New Change Send Email To all Approvers 

                                DataSet dsAllApplLevel = new DataSet();
                                dsAllApplLevel = fillds("select * from tOrderWiseAccess where OrderId=" + RequestID + "   and ApprovalLevel>0", conn);
                                int ApplCnt = dsAllApplLevel.Tables[0].Rows.Count;
                                if (ApplCnt > 0)
                                {
                                    for (int a = 0; a <= ApplCnt - 1; a++)
                                    {
                                        EmailSendofApproved(Convert.ToInt64(dsAllApplLevel.Tables[0].Rows[a]["UserApproverID"].ToString()), RequestID, a, conn);
                                        //EmailSendofApproved(Convert.ToInt64(ds1.Tables[0].Rows[a]["UserApproverID"].ToString()), RequestID, conn);  ///Old UserID   New UserApproverID  
                                    }
                                }
                                //suraj.j commited 25/11/2019 EmailSendWhenRequestSubmit(RequestID, conn);
                            }
                        }
                        else //if (status == 2)
                        {
                            DataSet dsSAI = new DataSet();
                            long SecondApproverID2 = 0;
                            // dsSAI = fillds("select * from VW_ApprovalLevelDetail where DepartmentID=" + DeptID + " and UserID!=" + ApproverID + " and Approvallevel=" + ApprovalLevel + "", conn);  //Change  select * from tOrderWiseAccess where OrderId=1062 and UserApproverID!=128 and ApprovalLevel=1
                            dsSAI = fillds("select * from tOrderWiseAccess where OrderId=" + RequestID + " and UserApproverID!=" + ApproverID + " and ApprovalLevel=" + ApprovalLevel + " ", conn);
                            int CntdsSAI = dsSAI.Tables[0].Rows.Count;

                            if (ApproverLogic == "AND")
                            {
                                //Send Mail of pending approval
                                // EmailSendToApprover(SecondApproverID, RequestID, conn);

                                if (CntdsSAI > 0)
                                {
                                    for (int d = 0; d <= CntdsSAI - 1; d++)
                                    {
                                        SecondApproverID2 = Convert.ToInt64(dsSAI.Tables[0].Rows[d]["UserApproverID"].ToString()); ////Old UserID   New UserApproverID
                                        EmailSendToApprover(SecondApproverID2, RequestID, d, conn);
                                    }
                                }
                            }
                            else if (ApproverLogic == "OR")
                            {
                                //SecondApproverID
                                if (CntdsSAI > 0)
                                {
                                    for (int d = 0; d <= CntdsSAI - 1; d++)
                                    {
                                        SecondApproverID2 = Convert.ToInt64(dsSAI.Tables[0].Rows[d]["UserApproverID"].ToString());  ////Old UserID   New UserApproverID
                                        rec = db.tApprovalTrans.Where(a => a.OrderId == RequestID && a.ApproverID == SecondApproverID2).FirstOrDefault();
                                        // rec = db.tApprovalTrans.Where(a => a.OrderId == RequestID && a.ApproverID == SecondApproverID).FirstOrDefault();
                                        if (rec != null)
                                        {
                                            db.tApprovalTrans.Detach(rec);
                                            rec.Remark = Remark;
                                            rec.Date = DateTime.Now;
                                            rec.Status = statusID;
                                            rec.Tag = Tag;
                                            db.tApprovalTrans.Attach(rec);
                                            db.ObjectStateManager.ChangeObjectState(rec, EntityState.Modified);
                                            db.SaveChanges();
                                        }
                                    }
                                }

                                //Update Request Status
                                DataSet ds4 = new DataSet();
                                ApprovalLevel++;
                                // ds4 = fillds("select * from VW_ApprovalLevelDetail where DepartmentID=" + DeptID + " and ApprovalLevel=" + ApprovalLevel + "", conn); //Change select * from tOrderWiseAccess where OrderId=1062  and ApprovalLevel=1
                                ds4 = fillds("select * from tOrderWiseAccess where OrderId=" + RequestID + "  and ApprovalLevel=" + ApprovalLevel + " ", conn);
                                if (ds4.Tables[0].Rows.Count > 0)
                                {
                                    for (int i = 0; i <= ds4.Tables[0].Rows.Count - 1; i++)
                                    {
                                        tApprovalTran AppTran = new tApprovalTran();
                                        AppTran.OrderId = RequestID;
                                        AppTran.UserId = Convert.ToInt64(ds3.Tables[0].Rows[0]["UserId"].ToString());
                                        AppTran.StoreId = DeptID;
                                        AppTran.ApprovalId = ApprovalLevel;
                                        AppTran.ApproverID = Convert.ToInt64(ds4.Tables[0].Rows[i]["UserApproverID"].ToString());  ////Old UserID   New UserApproverID
                                        AppTran.Tag = Tag;
                                        if (ApprovalLevel == 2) { AppTran.Status = 21; }
                                        else if (ApprovalLevel == 3) { AppTran.Status = 22; }
                                        else { AppTran.Status = 22; }
                                        db.tApprovalTrans.AddObject(AppTran);
                                        db.SaveChanges();

                                        //Send mail to Approvers 
                                        EmailSendToApprover(Convert.ToInt64(ds4.Tables[0].Rows[i]["UserApproverID"].ToString()), RequestID, i, conn);  ////Old UserID   New UserApproverID
                                    }

                                    //if (ApprovalLevel >= 3)
                                    if (ApprovalLevel > 3)
                                    {
                                        DataSet ds6 = new DataSet();

                                        // ds6 = fillds("select * from VW_ApprovalLevelDetail where DepartmentID=" + DeptID + " and ApprovalLevel=" + ApprovalLevel + "", conn); //Change select * from tOrderWiseAccess where OrderId=1062  and ApprovalLevel=1
                                        ds6 = fillds("select * from tOrderWiseAccess where OrderId=" + RequestID + "  and ApprovalLevel=" + ApprovalLevel + "", conn);
                                        if (ds6.Tables[0].Rows.Count > 0)
                                        {

                                        }
                                        else
                                        {

                                            //Change request status to Approved
                                            tOrderHead Order = new tOrderHead();
                                            Order = db.tOrderHeads.Where(r => r.Id == RequestID).FirstOrDefault();
                                            if (Order != null)
                                            {
                                                db.tOrderHeads.Detach(Order);
                                                Order.Status = status;
                                                Order.ApprovalDate = DateTime.Now;
                                                db.tOrderHeads.Attach(Order);
                                                db.ObjectStateManager.ChangeObjectState(Order, EntityState.Modified);
                                                db.SaveChanges();

                                            }
                                            if (ismoney == "No")
                                            {
                                                //Add Message into mMessageTrans Table After Approve Order
                                                //add by suraj
                                                DataSet dsChkOredrIsEcomOrNot = new DataSet();
                                                dsChkOredrIsEcomOrNot = fillds("select * from tOrderHead where orderType in('PreOrder','NormalOrder') and id=" + RequestID + " ", conn);
                                                if (dsChkOredrIsEcomOrNot.Tables[0].Rows.Count > 0)
                                                {
                                                     AddIntomMessageTransForEcommerce(RequestID, conn);
                                                   // UpdateIseApprflag(RequestID, conn);
                                                }
                                                else
                                                {


                                                     AddIntomMessageTrans(RequestID, conn);
                                                   // UpdateIseApprflag(RequestID, conn);

                                                }
                                            }
                                            //else { }
                                            //Update tProductStockDetails TotalDispatchQty
                                            UpdateTProductStockReserveQtyTotalDispatchQty(RequestID, conn);

                                            tCorrespond Cor = new tCorrespond();
                                            Cor = db.tCorresponds.Where(c => c.OrderHeadId == RequestID).FirstOrDefault();
                                            if (Cor != null)
                                            {
                                                db.tCorresponds.Detach(Cor);
                                                Cor.CurrentOrderStatus = status;

                                                db.tCorresponds.Attach(Cor);
                                                db.ObjectStateManager.ChangeObjectState(Cor, EntityState.Modified);
                                                db.SaveChanges();
                                            }

                                            //select * from tOrderWiseAccess where OrderId=1073   and ApprovalLevel>0
                                            DataSet dsAllApplLevel = new DataSet();
                                            dsAllApplLevel = fillds("select * from tOrderWiseAccess where OrderId=" + RequestID + "   and ApprovalLevel>0", conn);
                                            int ApplCnt = dsAllApplLevel.Tables[0].Rows.Count;
                                            if (ApplCnt > 0)
                                            {
                                                for (int a = 0; a <= ApplCnt - 1; a++)
                                                {
                                                    EmailSendofApproved(Convert.ToInt64(dsAllApplLevel.Tables[0].Rows[a]["UserApproverID"].ToString()), RequestID, a, conn);
                                                    // EmailSendofApproved(Convert.ToInt64(ds1.Tables[0].Rows[0]["UserApproverID"].ToString()), RequestID, conn);  ////Old UserID   New UserApproverID
                                                }
                                            }

                                            // suraj.j commited 25/11/2019 EmailSendWhenRequestSubmit(RequestID, conn);

                                            /* After request approved delete records of Auto cancellation & Approval Reminder START */
                                            tCorrespond COR = new tCorrespond();
                                            COR = db.tCorresponds.Where(i => i.OrderHeadId == RequestID && i.MessageType == "Reminder").FirstOrDefault();
                                            db.tCorresponds.DeleteObject(COR);
                                            /* After request approved delete records of Auto cancellation & Approval Reminder END */


                                        }
                                    }
                                    else
                                    {
                                        /* New Code For status Pending for Cost Center Approval*/
                                        string userType = ds4.Tables[0].Rows[0]["UserType"].ToString();

                                        tOrderHead Order = new tOrderHead();
                                        Order = db.tOrderHeads.Where(r => r.Id == RequestID).FirstOrDefault();
                                        if (Order != null)
                                        {
                                            db.tOrderHeads.Detach(Order);
                                            //if (Convert.ToString(userType).Trim().ToUpper() == Convert.ToString("CostCenter Approver").Trim().ToUpper())
                                            if (userType == "CostCenter Approver")
                                            {
                                                Order.Status = 32; //Pending For Cost Center Approver
                                            }
                                            else if (userType != "CostCenter Approver")
                                            {
                                                if (ApprovalLevel == 2) { Order.Status = 21; }
                                                else if (ApprovalLevel == 3) { Order.Status = 22; }
                                            }////
                                             //
                                             // Order.Status = status;

                                            db.tOrderHeads.Attach(Order);
                                            db.ObjectStateManager.ChangeObjectState(Order, EntityState.Modified);
                                            db.SaveChanges();
                                        }

                                        tCorrespond Cor = new tCorrespond();
                                        Cor = db.tCorresponds.Where(c => c.OrderHeadId == RequestID).FirstOrDefault();
                                        if (Cor != null)
                                        {
                                            db.tCorresponds.Detach(Cor);
                                            Cor.CurrentOrderStatus = status;
                                            db.tCorresponds.Attach(Cor);
                                            db.ObjectStateManager.ChangeObjectState(Cor, EntityState.Modified);
                                            db.SaveChanges();
                                        }
                                    }
                                }
                                else
                                {
                                    //Change request status to Approved
                                    tOrderHead Order = new tOrderHead();
                                    Order = db.tOrderHeads.Where(r => r.Id == RequestID).FirstOrDefault();
                                    if (Order != null)
                                    {

                                        db.tOrderHeads.Detach(Order);
                                        //Order.Status = status;
                                        Order.Status = 3;

                                        Order.ApprovalDate = DateTime.Now;



                                        db.tOrderHeads.Attach(Order);
                                        db.ObjectStateManager.ChangeObjectState(Order, EntityState.Modified);
                                        db.SaveChanges();
                                    }

                                    if (ismoney == "No")
                                    {
                                        //Add Message into mMessageTrans Table After Approve Order
                                        DataSet dsChkOredrIsEcomOrNot = new DataSet();
                                        dsChkOredrIsEcomOrNot = fillds("select * from tOrderHead where orderType in('PreOrder','NormalOrder') and id=" + RequestID + " ", conn);
                                        if (dsChkOredrIsEcomOrNot.Tables[0].Rows.Count > 0)
                                        {
                                             AddIntomMessageTransForEcommerce(RequestID, conn);
                                           // UpdateIseApprflag(RequestID, conn);
                                        }
                                        else
                                        {
                                            if (ERPfinacial != "True") {
                                                AddIntomMessageTrans(RequestID, conn);
                                                //  UpdateIseApprflag(RequestID, conn);
                                            }
                                        }
                                    }
                                    else { }
                                    //  AddIntomMessageTrans(RequestID, conn);
                                   // UpdateIseApprflag(RequestID, conn);
                                    //Update tProductStockDetails TotalDispatchQty
                                    UpdateTProductStockReserveQtyTotalDispatchQty(RequestID, conn);



                                    tCorrespond Cor = new tCorrespond();
                                    Cor = db.tCorresponds.Where(c => c.OrderHeadId == RequestID).FirstOrDefault();
                                    if (Cor != null)
                                    {
                                        db.tCorresponds.Detach(Cor);
                                        Cor.CurrentOrderStatus = 3;

                                        db.tCorresponds.Attach(Cor);
                                        db.ObjectStateManager.ChangeObjectState(Cor, EntityState.Modified);
                                        db.SaveChanges();
                                    }


                                    DataSet dsAllApplLevel = new DataSet();
                                    dsAllApplLevel = fillds("select * from tOrderWiseAccess where OrderId=" + RequestID + "   and ApprovalLevel>0", conn);
                                    int ApplCnt = dsAllApplLevel.Tables[0].Rows.Count;
                                    if (ApplCnt > 0)
                                    {
                                        for (int a = 0; a <= ApplCnt - 1; a++)
                                        {
                                            EmailSendofApproved(Convert.ToInt64(dsAllApplLevel.Tables[0].Rows[a]["UserApproverID"].ToString()), RequestID, a, conn);
                                            // EmailSendofApproved(Convert.ToInt64(ds1.Tables[0].Rows[0]["UserApproverID"].ToString()), RequestID, conn);   ////Old UserID   New UserApproverID
                                        }
                                    }
                                    //suraj.j commited 25/11/2019 EmailSendWhenRequestSubmit(RequestID, conn);

                                    /* After request approved delete records of Auto cancellation & Approval Reminder START */
                                    tCorrespond COR = new tCorrespond();
                                    COR = db.tCorresponds.Where(i => i.OrderHeadId == RequestID && i.MessageType == "Reminder").FirstOrDefault();
                                    db.tCorresponds.DeleteObject(COR);
                                    /* After request approved delete records of Auto cancellation & Approval Reminder END */
                                }
                            }
                        }

                        //update delivery date after approved only for Ecommerce order..
                        if (Ecomm == "Yes")
                        {
                            UpdateDeliverydate(RequestID, expdate, conn);
                        }
                        if (ERPfinacial == "True")
                        {
                            updateOrderaprStatus(RequestID, conn);
                        }
                    }
                    if (ismoney == "No")
                    {
                        //Check message no 1 is insert in the staging table then send notification to our user
                        ChkandSendInterfacemsgnumber1(RequestID, conn);
                    }
                }
            }
            catch (System.Exception ex)
            {

                ChkandSendInterfacemsgnumber1(RequestID, conn);
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "Sp_EnterErrorTracking";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("Data", ex.Data.ToString());
                    cmd.Parameters.AddWithValue("GetType", ex.Source.ToString());
                    cmd.Parameters.AddWithValue("InnerException", "Error");
                    cmd.Parameters.AddWithValue("Message", ex.Message.ToString());
                    cmd.Parameters.AddWithValue("Source", RequestID + "| UpdatetApprovalTransAfterApproval");
                    cmd.Parameters.AddWithValue("DateTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("UserID", ApproverID);
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
            }
        }

        public DateTime GetDeiverydate(long RequestID, string DeliveryTypeNew, long ApproverID, string[] conn)
        {
            DateTime expdate = DateTime.Today;

            try
            {
                // DateTime sdate = DateTime.Now;
                //update exp delivery date  date when order is approved   add by suraj               
                DateTime today = DateTime.Today;
                string OnlyTime = DateTime.Now.ToString("HH:MM");
                DateTime s;
                string chktime = "1130";
                DataSet dsChkOrdIsEcomOrNot = new DataSet();
                dsChkOrdIsEcomOrNot = fillds("select * from tOrderHead where orderType in('PreOrder','NormalOrder') and id=" + RequestID + " ", conn);
                if (dsChkOrdIsEcomOrNot.Tables[0].Rows.Count > 0)
                {
                    // Ecomm = "Yes";
                    if (DeliveryTypeNew.ToUpper() == "HUB")
                    {
                        OnlyTime = OnlyTime.Replace(":", ".");
                        chktime = chktime.Insert(2, ".");
                        if (Convert.ToDecimal(OnlyTime) > Convert.ToDecimal(chktime))
                        {
                            today = today.AddDays(1);
                        }
                    }
                    else
                    {
                        today = today.AddDays(1);
                    }

                    DataSet ds1 = new DataSet();
                    ds1 = fillds("select * from mholiday where convert(date,hdate)=convert(date,'" + today + "')", conn);
                    if (ds1.Tables[0].Rows.Count > 0)
                    {
                        s = today.AddDays(1);
                        for (int x = 0; x < 100; x++)
                        {
                            DataSet ds2 = new DataSet();
                            ds2 = fillds("select * from mholiday where convert(date,hdate)=convert(date,'" + s + "')", conn);
                            if (ds2.Tables[0].Rows.Count > 0)
                            {
                                s = s.AddDays(1);
                                // s = DateTime.Now.AddDays(1);
                            }
                            else
                            {
                                expdate = s;
                                break;
                            }
                        }
                    }
                    else
                    {
                        expdate = today;
                    }
                }
                //suraj              

            }
            catch (System.Exception ex)
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "Sp_EnterErrorTracking";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("Data", ex.Data.ToString());
                    cmd.Parameters.AddWithValue("GetType", ex.Source.ToString());
                    cmd.Parameters.AddWithValue("InnerException", "Error");
                    cmd.Parameters.AddWithValue("Message", ex.Message.ToString());
                    cmd.Parameters.AddWithValue("Source", "GetDeiverydatePartrequestdomain");
                    cmd.Parameters.AddWithValue("DateTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("UserID", ApproverID);
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
            }
            finally
            {

            }
            return expdate;
        }


        public void ChkandSendInterfacemsgnumber1(long RequestID, string[] conn)
        {
            try
            {
                //Check message no 1 is insert in the staging table then send notification to our user
                string ChkOrderIsApprovedorNot = "";
                ChkOrderIsApprovedorNot = FunChkOrderIsApprovedorNot(RequestID, conn);
                if (ChkOrderIsApprovedorNot == "Yes")
                {
                    string ChkmsgispresentOrNot = "";
                    ChkmsgispresentOrNot = FunChkmsgispresentOrNotforapprovalNew(RequestID, conn);
                    //if (ChkmsgispresentOrNot == "No")
                    //{
                    //   // SendMessageInterfacenotificationLocal(RequestID, conn);                       
                    //  // InterfacestaggingfailOrder(RequestID, conn);
                    //    //stock adjustment if is not done.
                    //    string Chkstockadjust = "";
                    //    Chkstockadjust = CheckStockadjustmentisdone(RequestID, conn);
                    //    if (Chkstockadjust == "No")
                    //    {
                    //        UpdateTProductStockReserveQtyTotalDispatchQty(RequestID, conn);
                    //    }
                    //}
                }
            }
            catch (System.Exception ex)
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "Sp_EnterErrorTracking";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("Data", ex.Data.ToString());
                    cmd.Parameters.AddWithValue("GetType", ex.Source.ToString());
                    cmd.Parameters.AddWithValue("InnerException", "Error");
                    cmd.Parameters.AddWithValue("Message", ex.Message.ToString());
                    cmd.Parameters.AddWithValue("Source", "ChkandSendInterfacemsgnumber1");
                    cmd.Parameters.AddWithValue("DateTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("UserID", 00000);
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
            }
        }

        public void UpdateTProductStockReserveQtyTotalDispatchQtyapproval(long RequestID, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                tProductStockDetail psd = new tProductStockDetail();
                try
                {
                    DataSet dsPrdDetails = new DataSet();
                    dsPrdDetails = fillds("Select * from tOrderDetail where OrderHeadId=" + RequestID + "", conn);
                    int PrdCnt = dsPrdDetails.Tables[0].Rows.Count;
                    if (PrdCnt > 0)
                    {
                        for (int i = 0; i <= PrdCnt - 1; i++)
                        {
                            //update tProductstockDetails set ResurveQty=ResurveQty-@Qty1,TotalDispatchQty=TotalDispatchQty+@Qty1  where ProdID=@SkuId1
                            long SkuID = long.Parse(dsPrdDetails.Tables[0].Rows[i]["SkuId"].ToString());
                            decimal Qty = decimal.Parse(dsPrdDetails.Tables[0].Rows[i]["OrderQty"].ToString());
                            //added by suraj J. for product "SimFMSIn" not maintin stock adjust for Delivery type is HUB
                            string ChkOrderDeliveryType = ""; long EcommId = 0;
                            DataSet dsOrdType = new DataSet();
                            dsOrdType = fillds("select * from tecommhead where deliverytype='HUB' and orderid= " + RequestID + "", conn);
                            if (dsOrdType.Tables[0].Rows.Count > 0)
                            {
                                ChkOrderDeliveryType = "HUB";
                                EcommId = Convert.ToInt64(dsOrdType.Tables[0].Rows[0]["ID"].ToString());
                            }
                            else
                            {
                                ChkOrderDeliveryType = "Home";
                            }

                            if (ChkOrderDeliveryType == "HUB")
                            {
                                mProduct prd = new mProduct();
                                prd = db.mProducts.Where(p => p.ID == SkuID).FirstOrDefault();
                                if (prd.GroupSet == "Yes")
                                {
                                    DataSet dsBomProds = new DataSet();
                                    dsBomProds = fillds("select SKUId,Quantity from mBOMDetail BD where BD.BOMheaderId=" + SkuID + "", conn);
                                    if (dsBomProds.Tables[0].Rows.Count > 0)
                                    {
                                        for (int b = 0; b <= dsBomProds.Tables[0].Rows.Count - 1; b++)
                                        {
                                            long bomPrd = long.Parse(dsBomProds.Tables[0].Rows[b]["SKUId"].ToString());
                                            decimal bomQty = decimal.Parse(dsBomProds.Tables[0].Rows[b]["Quantity"].ToString());

                                            decimal FinalQty = Qty * bomQty;

                                            using (SqlCommand cmd = new SqlCommand())
                                            {
                                                cmd.CommandType = CommandType.StoredProcedure;
                                                cmd.CommandText = "GWC_SP_UpdatetProductstockDetailsDispatchResurveQty";
                                                cmd.Connection = svr.GetSqlConn(conn);
                                                cmd.Parameters.Clear();
                                                cmd.Parameters.AddWithValue("SkuID", bomPrd);
                                                cmd.Parameters.AddWithValue("Qty", FinalQty);
                                                cmd.ExecuteNonQuery();
                                                cmd.Connection.Close();
                                            }

                                            // InsertIntotInventory(bomPrd, RequestID, FinalQty, "Dispatch", conn);
                                        }
                                    }
                                }
                                else if (prd.GroupSet == "No")
                                {
                                    if (prd.ProductType == "Virtual SKU" && prd.ProductType != null)
                                    {
                                        using (SqlCommand cmd = new SqlCommand())
                                        {
                                            cmd.CommandType = CommandType.StoredProcedure;
                                            cmd.CommandText = "GWC_SP_UpdatetProductstockDetailsDispatchResurveQtyforVirtualSku";
                                            cmd.Connection = svr.GetSqlConn(conn);
                                            cmd.Parameters.Clear();
                                            cmd.Parameters.AddWithValue("SkuID", SkuID);
                                            cmd.Parameters.AddWithValue("Qty", Qty);
                                            cmd.Parameters.AddWithValue("@OderID", RequestID);
                                            cmd.ExecuteNonQuery();
                                            cmd.Connection.Close();
                                        }
                                    }
                                    else
                                    {
                                        //added by suraj J.
                                        string productisSimFMSIn = "";
                                        DataSet dsSimFMSIn = new DataSet();
                                        dsSimFMSIn = fillds("select * from tecommdetail where ecomheadid=" + EcommId + " and productcode='SimFMSIn' and packagetype='FMS-INDOOR' and productcode='" + prd.ProductCode + "' ", conn);
                                        if (dsSimFMSIn.Tables[0].Rows.Count > 0)
                                        {

                                        }
                                        else
                                        {
                                            using (SqlCommand cmd = new SqlCommand())
                                            {
                                                cmd.CommandType = CommandType.StoredProcedure;
                                                cmd.CommandText = "GWC_SP_UpdatetProductstockDetailsDispatchResurveQty";
                                                cmd.Connection = svr.GetSqlConn(conn);
                                                cmd.Parameters.Clear();
                                                cmd.Parameters.AddWithValue("SkuID", SkuID);
                                                cmd.Parameters.AddWithValue("Qty", Qty);
                                                cmd.ExecuteNonQuery();
                                                cmd.Connection.Close();
                                            }
                                        }
                                    }
                                    // InsertIntotInventory(SkuID, RequestID, Qty, "Dispatch", conn);
                                }
                            }
                            else
                            {
                                mProduct prd = new mProduct();
                                prd = db.mProducts.Where(p => p.ID == SkuID).FirstOrDefault();
                                if (prd.GroupSet == "Yes")
                                {
                                    DataSet dsBomProds = new DataSet();
                                    dsBomProds = fillds("select SKUId,Quantity from mBOMDetail BD where BD.BOMheaderId=" + SkuID + "", conn);
                                    if (dsBomProds.Tables[0].Rows.Count > 0)
                                    {
                                        for (int b = 0; b <= dsBomProds.Tables[0].Rows.Count - 1; b++)
                                        {
                                            long bomPrd = long.Parse(dsBomProds.Tables[0].Rows[b]["SKUId"].ToString());
                                            decimal bomQty = decimal.Parse(dsBomProds.Tables[0].Rows[b]["Quantity"].ToString());

                                            decimal FinalQty = Qty * bomQty;

                                            using (SqlCommand cmd = new SqlCommand())
                                            {
                                                cmd.CommandType = CommandType.StoredProcedure;
                                                cmd.CommandText = "GWC_SP_UpdatetProductstockDetailsDispatchResurveQty";
                                                cmd.Connection = svr.GetSqlConn(conn);
                                                cmd.Parameters.Clear();
                                                cmd.Parameters.AddWithValue("SkuID", bomPrd);
                                                cmd.Parameters.AddWithValue("Qty", FinalQty);
                                                cmd.ExecuteNonQuery();
                                                cmd.Connection.Close();
                                            }

                                            // InsertIntotInventory(bomPrd, RequestID, FinalQty, "Dispatch", conn);
                                        }
                                    }
                                }
                                else if (prd.GroupSet == "No")
                                {

                                    if (prd.ProductType == "Virtual SKU" && prd.ProductType != null)
                                    {
                                        using (SqlCommand cmd = new SqlCommand())
                                        {
                                            cmd.CommandType = CommandType.StoredProcedure;
                                            cmd.CommandText = "GWC_SP_UpdatetProductstockDetailsDispatchResurveQtyforVirtualSku";
                                            cmd.Connection = svr.GetSqlConn(conn);
                                            cmd.Parameters.Clear();
                                            cmd.Parameters.AddWithValue("SkuID", SkuID);
                                            cmd.Parameters.AddWithValue("Qty", Qty);
                                            cmd.Parameters.AddWithValue("@OderID", RequestID);
                                            cmd.ExecuteNonQuery();
                                            cmd.Connection.Close();
                                        }
                                    }
                                    else
                                    {
                                        using (SqlCommand cmd = new SqlCommand())
                                        {
                                            cmd.CommandType = CommandType.StoredProcedure;
                                            cmd.CommandText = "GWC_SP_UpdatetProductstockDetailsDispatchResurveQty";
                                            cmd.Connection = svr.GetSqlConn(conn);
                                            cmd.Parameters.Clear();
                                            cmd.Parameters.AddWithValue("SkuID", SkuID);
                                            cmd.Parameters.AddWithValue("Qty", Qty);
                                            cmd.ExecuteNonQuery();
                                            cmd.Connection.Close();
                                        }
                                    }
                                    // InsertIntotInventory(SkuID, RequestID, Qty, "Dispatch", conn);
                                }
                            }
                        }
                    }
                }
                catch { }
                finally { }
            }
        }

        public string CheckStockadjustmentisdone(long RequestID, string[] conn)
        {
            string result = "";
            DataSet ds = new DataSet();
            ds = fillds(" select * from tInventry where TransactionId=" + RequestID + " and TransactionType='Dispatch'", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                result = "Yes";
            }
            else
            {
                result = "No";
            }
            return result;
        }
        public string CheckStockadjustmentisdoneapproval(long RequestID, string[] conn)
        {
            string result = "";
            DataSet ds = new DataSet();
            ds = fillds(" select * from tInventry where TransactionId=" + RequestID + " and TransactionType='Dispatch'", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                result = "Yes";
            }
            else
            {
                result = "No";
            }
            return result;
        }

        public string FunChkOrderIsApprovedorNot(long RequestID, string[] conn)
        {
            string result = "";
            DataSet ds = new DataSet();
            ds = fillds("select * from torderhead where status=3 and id=" + RequestID + "", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                result = "Yes";
            }
            else
            {
                result = "No";
            }
            return result;
        }
        public string FunChkOrderIsApprovedorNotapproval(long RequestID, string[] conn)
        {
            string result = "";
            DataSet ds = new DataSet();
            ds = fillds("select * from torderhead where status=3 and id=" + RequestID + "", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                result = "Yes";
            }
            else
            {
                result = "No";
            }
            return result;
        }

        public string Getmailidformsgno1(string[] conn)
        {
            string result = "";
            DataSet ds = new DataSet();
            ds = fillds("select value from mdropdownvalues where parameter='InterfacemessageEmail'", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                result = Convert.ToString(ds.Tables[0].Rows[0]["value"]);
            }
            else
            {
                result = "balaji@brilliantinfosys.com,rahul@brilliantinfosys.com,pradeep@brilliantwms.com";
            }
            return result;
        }

        public void SendMessageInterfacenotificationLocal(long RequestID, string[] conn)
        {
            try
            {
                //get oms order number
                string ordernumbr = "", msgbody = "";
                DataSet dsMailSubBody = new DataSet();
                dsMailSubBody = fillds("select * from torderhead where id=" + RequestID + "", conn);
                if (dsMailSubBody.Tables[0].Rows.Count > 0)
                {
                    ordernumbr = dsMailSubBody.Tables[0].Rows[0]["Orderno"].ToString();
                }

                //get email id 
                string emailid = "";
                emailid = Getmailidformsgno1(conn);

                SmtpClient smtpClient = new SmtpClient("smtpout.asia.secureserver.net", 25);
                // SmtpClient smtpClient = new SmtpClient("10.228.134.54", 25);
                MailMessage message = new MailMessage();


                MailAddress fromAddress = new MailAddress("admin@brilliantinfosys.com", "GWC");
                // MailAddress fromAddress = new MailAddress("OMSTest@gulfwarehousing.com", "GWC");

                //From address will be given as a MailAddress Object
                message.From = fromAddress;

                //To address collection of MailAddress
                // message.To.Add("suraj@brilliantinfosys.com,minal@brilliantinfosys.com");
                message.To.Add(emailid);
                message.Subject = "Interface Message number 1 is not inserted in staging table.";

                //Body can be Html or text format
                //Specify true if it  is html message
                message.IsBodyHtml = true;

                //Message body content
                msgbody = "<br/>" + "Dear User,";
                msgbody = msgbody + "<br/><br/>" + "Message number 1 is not inserted in staging table for order number <b>" + ordernumbr + "" + "</b>." + "<br/><br/>";
                msgbody = msgbody + "Please insert message number 1 into staging table to reflect in WMS.";
                message.Body = msgbody + MailGetFooter();

                smtpClient.EnableSsl = false;
                //Send SMTP mail
                smtpClient.UseDefaultCredentials = false;
                // NetworkCredential basicCredential = new NetworkCredential("admin@brilliantinfosys.com", "6march1986");
                NetworkCredential basicCredential = new NetworkCredential("OMSTest@gulfwarehousing.com", "");
                smtpClient.Credentials = basicCredential;

                smtpClient.Send(message);
            }
            catch { }
        }

        public void SendMessageInterfacenotification(long RequestID, string[] conn)
        {
            try
            {
                //get oms order number
                string ordernumbr = "", msgbody = "";
                DataSet dsMailSubBody = new DataSet();
                dsMailSubBody = fillds("select * from torderhead where id=" + RequestID + "", conn);
                if (dsMailSubBody.Tables[0].Rows.Count > 0)
                {
                    ordernumbr = dsMailSubBody.Tables[0].Rows[0]["Orderno"].ToString();
                }

                //get email id 
                string emailid = "";
                emailid = Getmailidformsgno1(conn);

                //SmtpClient smtpClient = new SmtpClient("smtpout.asia.secureserver.net", 25);
                SmtpClient smtpClient = new SmtpClient("10.228.134.54", 25);
                MailMessage message = new MailMessage();


                // MailAddress fromAddress = new MailAddress("admin@brilliantinfosys.com", "GWC");
                MailAddress fromAddress = new MailAddress("OMSTest@gulfwarehousing.com", "GWC");

                //From address will be given as a MailAddress Object
                message.From = fromAddress;

                //To address collection of MailAddress
                //   message.To.Add("suraj@brilliantinfosys.com,minal@brilliantinfosys.com");
                message.To.Add(emailid);
                message.Subject = "Interface Message number 1 is not inserted in staging table.";

                //Body can be Html or text format
                //Specify true if it  is html message
                message.IsBodyHtml = true;

                //Message body content
                msgbody = "<br/>" + "Dear User,";
                msgbody = msgbody + "<br/><br/>" + "Message number 1 is not inserted in staging table for order number <b>" + ordernumbr + "" + "</b>." + "<br/><br/>";
                msgbody = msgbody + "Please insert message number 1 into staging table to reflect in WMS.";
                message.Body = msgbody + MailGetFooter();

                smtpClient.EnableSsl = false;
                //Send SMTP mail
                smtpClient.UseDefaultCredentials = false;
                // NetworkCredential basicCredential = new NetworkCredential("admin@brilliantinfosys.com", "6march1986");
                NetworkCredential basicCredential = new NetworkCredential("OMSTest@gulfwarehousing.com", "");
                smtpClient.Credentials = basicCredential;

                smtpClient.Send(message);
            }
            catch { }
        }

        public string FunChkmsgispresentOrNotforapproval(long RequestID, string[] conn)
        {
            string result = "";
            using (SqlCommand cmd = new SqlCommand())
            {
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    DataSet ds = new DataSet();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_ChkmsgispresentOrNot";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("RequestID", RequestID);
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i <= ds.Tables[0].Rows.Count - 1; i++)
                        {
                            string message = "";
                            message = Convert.ToString(ds.Tables[0].Rows[i]["MsgDescription"]);
                            using (SqlCommand cmd1 = new SqlCommand())
                            {
                                using (SqlDataAdapter da1 = new SqlDataAdapter())
                                {
                                    DataSet ds1 = new DataSet();
                                    cmd1.CommandType = CommandType.StoredProcedure;
                                    cmd1.CommandText = "SP_ChkmsgispresentOrNotwithstring";
                                    cmd1.Connection = svr.GetSqlConn(conn);
                                    cmd1.Parameters.Clear();
                                    cmd1.Parameters.AddWithValue("@message", message);
                                    da1.SelectCommand = cmd1;
                                    da1.Fill(ds1);
                                    if (ds1.Tables[0].Rows.Count > 0)
                                    {
                                        string orderid = "0";
                                        orderid = Convert.ToString(ds1.Tables[0].Rows[0]["part"]);
                                        if (orderid.Trim() == Convert.ToString(RequestID).Trim())
                                        {
                                            result = "Yes";
                                            break;
                                        }
                                        else
                                        {
                                            result = "No";
                                        }
                                    }
                                    else
                                    {
                                        result = "No";
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        result = "No";
                    }
                }
            }
            return result;
        }
        public string FunChkmsgispresentOrNotforapprovalNew(long RequestID, string[] conn)
        {
            string result = "";
            using (SqlCommand cmd = new SqlCommand())
            {
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    DataSet ds = new DataSet();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_ChkmsgispresentOrNot";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("RequestID", RequestID);
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                }
                return result;
            }
        }

        public string FunChkmsgispresentOrNot(long RequestID, string[] conn)
        {
            string result = "";
            using (SqlCommand cmd = new SqlCommand())
            {
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    DataSet ds = new DataSet();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_ChkmsgispresentOrNot";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("RequestID", RequestID);
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i <= ds.Tables[0].Rows.Count - 1; i++)
                        {
                            string message = "";
                            message = Convert.ToString(ds.Tables[0].Rows[i]["MsgDescription"]);
                            using (SqlCommand cmd1 = new SqlCommand())
                            {
                                using (SqlDataAdapter da1 = new SqlDataAdapter())
                                {
                                    DataSet ds1 = new DataSet();
                                    cmd1.CommandType = CommandType.StoredProcedure;
                                    cmd1.CommandText = "SP_ChkmsgispresentOrNotwithstring";
                                    cmd1.Connection = svr.GetSqlConn(conn);
                                    cmd1.Parameters.Clear();
                                    cmd1.Parameters.AddWithValue("@message", message);
                                    da1.SelectCommand = cmd1;
                                    da1.Fill(ds1);
                                    if (ds1.Tables[0].Rows.Count > 0)
                                    {
                                        string orderid = "0";
                                        orderid = Convert.ToString(ds1.Tables[0].Rows[0]["part"]);
                                        if (orderid.Trim() == Convert.ToString(RequestID).Trim())
                                        {
                                            result = "Yes";
                                            break;
                                        }
                                        else
                                        {
                                            result = "No";
                                        }
                                    }
                                    else
                                    {
                                        result = "No";
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        result = "No";
                    }
                }
            }
            return result;
        }
        public string InterfacestaggingfailOrder(long RequestID, string[] conn)
        {
            string result = "";
            using (SqlCommand cmd = new SqlCommand())
            {
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    DataSet ds = new DataSet();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_ChkmsgispresentOrNotNew";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("RequestID", RequestID);
                    da.SelectCommand = cmd;
                    da.Fill(ds);

                }
                return result;
            }
        }
        private void UpdateDeliverydate(long requestID, DateTime expdate, string[] conn)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_UpdateDeliverydate";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@requestID", requestID);
                cmd.Parameters.AddWithValue("@expdate", expdate);
                cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }
        }

        public void AddIntomMessageTransForEcommerce(long RequestID, string[] conn)
        {

            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                mMessageTran Msg = new mMessageTran();

                GWCEcomm_VW_MessageOrderHead MsgHD = new GWCEcomm_VW_MessageOrderHead();
                MsgHD = db.GWCEcomm_VW_MessageOrderHead.Where(h => h.Id == RequestID).FirstOrDefault();
                // Address For Home or Retail
                string CustomerName = MsgHD.CustomerName;
                string Address = "";
                string DeliveryType = MsgHD.DeliveryType;
                if (DeliveryType == "HUB")
                {
                    Address = MsgHD.RetailAddress;
                }
                else
                {
                    //Address = MsgHD.HomeAddress;
                    Address = MsgHD.RetailAddress;
                }
                string ContactPerson2Names = "NA";
                string LocationCode = "NA";
                string RequestorName = "NA";
                string RequestorMobileNo = "NA";
                string ConsigneePhone = "NA";

                //Added by suraj 
                string ContactMobile2 = "";
                ContactMobile2 = GetContactMobile2(RequestID, conn);
                //Get PaymentMethodValue
                string PaymentMethodValue = "";
                PaymentMethodValue = GetPaymentMethodValue(RequestID, conn);
                //address2 and paymentmethdnm
                string address2 = "", paymentmethdnm = "";
                DataTable dtadd2 = new DataTable();
                dtadd2 = GetAddressLine2(RequestID, conn);
                if (dtadd2.Rows.Count > 0)
                {
                    address2 = dtadd2.Rows[0]["AddressLine2"].ToString();
                    paymentmethdnm = dtadd2.Rows[0]["MethodName"].ToString();
                }

                Msg.MsgDescription = MsgHD.Id + " | " + MsgHD.OrderNumber + " | " + MsgHD.Orderdate.Value.ToShortDateString() + " | " + MsgHD.Deliverydate.Value.ToShortDateString() + " | " + MsgHD.StoreCode + " | " + MsgHD.Con1 + " | " + ContactPerson2Names + " | " + CheckString(Address) + " | " + CheckString(MsgHD.Remark) + " | " + MsgHD.OrderNo + " | " + MsgHD.InvoiceNo + " | " + LocationCode + " | " + RequestorName + " | " + RequestorMobileNo + " | " + MsgHD.ConsigneeName + " | " + CheckString(MsgHD.ConsigneeAddress) + " | " + ConsigneePhone + " | " + MsgHD.Con1MobileNo + " | " + ContactMobile2 + " | " + CheckString(address2) + " | " + paymentmethdnm + " | " + PaymentMethodValue + " | " + MsgHD.ProjectType + " | " + MsgHD.SiteCode + " | " + MsgHD.SiteName + " | " + MsgHD.Latitude + " | " + MsgHD.Longitude + " | " + MsgHD.AccessRequirement; ;
                DataSet ds = new DataSet();
                // ds = fillds("select * from GWC_VW_MsgOrderDetail where OrderHeadId=" + RequestID + " and ProductType is null ", conn);
                ds = fillds("select * from GWC_VW_MsgOrderDetail where OrderHeadId=" + RequestID + " and (ProductType is null Or ProductType='General SKU')", conn);

                int Cnt = ds.Tables[0].Rows.Count;
                if (Cnt > 0)
                {
                    for (int i = 0; i <= Cnt - 1; i++)
                    {
                        /*long PrdID = long.Parse(ds.Tables[0].Rows[i]["SkuId"].ToString());
                        mProduct prd = new mProduct();
                        prd = db.mProducts.Where(p => p.ID == PrdID).FirstOrDefault();
                        if (prd.GroupSet == "Yes")
                        {
                            DataSet dsBomProds = new DataSet();
                            dsBomProds = fillds("select SKUId,Quantity from mBOMDetail BD where BD.BOMheaderId=" + PrdID + "");
                            if (dsBomProds.Tables[0].Rows.Count > 0)
                            {
                                for (int b = 0; b <= dsBomProds.Tables[0].Rows.Count - 1; b++)
                                {
                                    decimal bomQty = decimal.Parse(dsBomProds.Tables[0].Rows[b]["Quantity"].ToString());
                                    decimal Qty = decimal.Parse(ds.Tables[0].Rows[i]["OrderQty"].ToString());
                                    decimal FinalQty = Qty * bomQty;

                                    long bomPrd = long.Parse(dsBomProds.Tables[0].Rows[b]["SKUId"].ToString());
                                    DataSet dsPrdName = new DataSet();
                                    dsPrdName = fillds("select ProductCode from mproduct where ID=" + bomPrd + "");
                                    string ProductCode = dsPrdName.Tables[0].Rows[0]["ProductCode"].ToString();

                                    Msg.MsgDescription = Msg.MsgDescription + " | " + ProductCode + " | " + ds.Tables[0].Rows[i]["UOM"].ToString() + " | " + FinalQty;
                                }
                            }
                        }
                        else if (prd.GroupSet == "No")
                        {*/
                        Msg.MsgDescription = Msg.MsgDescription + " | " + ds.Tables[0].Rows[i]["Prod_Code"].ToString() + " | " + ds.Tables[0].Rows[i]["UOM"].ToString() + " | " + ds.Tables[0].Rows[i]["SerialNumber"].ToString() + " | " + ds.Tables[0].Rows[i]["serialflag"].ToString() + " | " + ds.Tables[0].Rows[i]["OrderQty"].ToString();
                        // }
                    }
                }
                Msg.MessageHdrId = 1;
                Msg.Object = "Order";
                // Msg.Destination = "WMS";
                Msg.Destination = MsgHD.StoreCode;
                Msg.Status = 0;
                Msg.CreationDate = DateTime.Now;
                Msg.Createdby = "OMS";
                db.mMessageTrans.AddObject(Msg);
                db.SaveChanges();
            }
        }
        //public void AddIntomMessageTransForEcommerce(long RequestID, string[] conn)
        //{

        //    using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
        //    {
        //        mMessageTran Msg = new mMessageTran();

        //        //GWC_VW_MessOrderHeadImport MsgHD = new GWC_VW_MessOrderHeadImport();
        //        GWCEcomm_VW_MessageOrderHead MsgHD = new GWCEcomm_VW_MessageOrderHead();
        //        MsgHD = db.GWCEcomm_VW_MessageOrderHead.Where(h => h.Id == RequestID).FirstOrDefault();
        //        // Address For Home or Retail
        //        string CustomerName = MsgHD.CustomerName;
        //        string Address = "";
        //        string DeliveryType = MsgHD.DeliveryType;
        //        if (DeliveryType == "HUB")
        //        {
        //            Address = MsgHD.RetailAddress;
        //        }
        //        else
        //        {
        //            //Address = MsgHD.HomeAddress;
        //            Address = MsgHD.RetailAddress;
        //        }
        //        string ContactPerson2Names = "NA";
        //        string LocationCode = "NA";
        //        string RequestorName = "NA";
        //        string RequestorMobileNo = "NA";
        //        string ConsigneePhone = "NA";

        //        Msg.MsgDescription = MsgHD.Id + " | " + MsgHD.OrderNumber + " | " + MsgHD.Orderdate.Value.ToShortDateString() + " | " + MsgHD.Deliverydate.Value.ToShortDateString() + " | " + MsgHD.StoreCode + " | " + MsgHD.Con1 + " | " + ContactPerson2Names + " | " + CheckString(Address) + " | " + CheckString(MsgHD.Remark) + " | " + MsgHD.OrderNo + " | " + MsgHD.InvoiceNo + " | " + LocationCode + " | " + RequestorName + " | " + RequestorMobileNo + " | " + MsgHD.ConsigneeName + " | " + MsgHD.ConsigneeAddress + " | " + ConsigneePhone + " | " + MsgHD.Con1MobileNo + " | " + MsgHD.Con2MobileNo + " | " + CheckString(MsgHD.HomeAddress) + " | " + MsgHD.Methodname + " | " + MsgHD.PaymentMethodValue;
        //        DataSet ds = new DataSet();
        //        // ds = fillds("select * from GWC_VW_MsgOrderDetail where OrderHeadId=" + RequestID + " and ProductType is null ", conn);
        //        ds = fillds("select * from GWC_VW_MsgOrderDetail where OrderHeadId=" + RequestID + " and (ProductType is null Or ProductType='General SKU')", conn);

        //        int Cnt = ds.Tables[0].Rows.Count;
        //        if (Cnt > 0)
        //        {
        //            for (int i = 0; i <= Cnt - 1; i++)
        //            {
        //                /*long PrdID = long.Parse(ds.Tables[0].Rows[i]["SkuId"].ToString());
        //                mProduct prd = new mProduct();
        //                prd = db.mProducts.Where(p => p.ID == PrdID).FirstOrDefault();
        //                if (prd.GroupSet == "Yes")
        //                {
        //                    DataSet dsBomProds = new DataSet();
        //                    dsBomProds = fillds("select SKUId,Quantity from mBOMDetail BD where BD.BOMheaderId=" + PrdID + "");
        //                    if (dsBomProds.Tables[0].Rows.Count > 0)
        //                    {
        //                        for (int b = 0; b <= dsBomProds.Tables[0].Rows.Count - 1; b++)
        //                        {
        //                            decimal bomQty = decimal.Parse(dsBomProds.Tables[0].Rows[b]["Quantity"].ToString());
        //                            decimal Qty = decimal.Parse(ds.Tables[0].Rows[i]["OrderQty"].ToString());
        //                            decimal FinalQty = Qty * bomQty;

        //                            long bomPrd = long.Parse(dsBomProds.Tables[0].Rows[b]["SKUId"].ToString());
        //                            DataSet dsPrdName = new DataSet();
        //                            dsPrdName = fillds("select ProductCode from mproduct where ID=" + bomPrd + "");
        //                            string ProductCode = dsPrdName.Tables[0].Rows[0]["ProductCode"].ToString();

        //                            Msg.MsgDescription = Msg.MsgDescription + " | " + ProductCode + " | " + ds.Tables[0].Rows[i]["UOM"].ToString() + " | " + FinalQty;
        //                        }
        //                    }
        //                }
        //                else if (prd.GroupSet == "No")
        //                {*/
        //                Msg.MsgDescription = Msg.MsgDescription + " | " + ds.Tables[0].Rows[i]["Prod_Code"].ToString() + " | " + ds.Tables[0].Rows[i]["UOM"].ToString() + " | " + ds.Tables[0].Rows[i]["OrderQty"].ToString();
        //                // }
        //            }
        //        }
        //        Msg.MessageHdrId = 1;
        //        Msg.Object = "Order";
        //        // Msg.Destination = "WMS";
        //        Msg.Destination = MsgHD.StoreCode;
        //        Msg.Status = 0;
        //        Msg.CreationDate = DateTime.Now;
        //        Msg.Createdby = "OMS";
        //        db.mMessageTrans.AddObject(Msg);
        //        db.SaveChanges();
        //    }
        //}

        public void UpdatetApprovalTransAfterReject(long ApprovalID, long RequestID, long statusID, string Remark, long ApproverID, string reasoncode, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                //change tOrderwiseAccess to 0 after approve order
                DataSet dsEditAccessRemove = new DataSet();
                dsEditAccessRemove = fillds("update torderwiseaccess set PriceEdit=0,SkuQtyEdit=0 where orderid=" + RequestID + "", conn);

                tApprovalTran rec = new tApprovalTran();
                // rec = db.tApprovalTrans.Where(a => a.Id == ApprovalID && a.OrderId == RequestID && a.ApproverID == ApproverID).FirstOrDefault();
                rec = db.tApprovalTrans.Where(a => a.OrderId == RequestID && a.ApproverID == ApproverID).FirstOrDefault();
                if (rec != null)
                {
                    db.tApprovalTrans.Detach(rec);
                    rec.Remark = Remark;
                    rec.Date = DateTime.Now;
                    rec.Status = statusID;

                    db.tApprovalTrans.Attach(rec);
                    db.ObjectStateManager.ChangeObjectState(rec, EntityState.Modified);
                    db.SaveChanges();
                }

                tOrderHead Order = new tOrderHead();
                Order = db.tOrderHeads.Where(r => r.Id == RequestID).FirstOrDefault();
                if (Order != null)
                {

                    db.tOrderHeads.Detach(Order);
                    Order.Status = statusID;
                    // add by suraj for reasoncode
                    Order.ReasonCode = long.Parse(reasoncode.ToString());

                    db.tOrderHeads.Attach(Order);
                    db.ObjectStateManager.ChangeObjectState(Order, EntityState.Modified);
                    db.SaveChanges();
                }

                tCorrespond Cor = new tCorrespond();
                Cor = db.tCorresponds.Where(c => c.OrderHeadId == RequestID).FirstOrDefault();
                if (Cor != null)
                {
                    db.tCorresponds.Detach(Cor);
                    Cor.CurrentOrderStatus = statusID;

                    db.tCorresponds.Attach(Cor);
                    db.ObjectStateManager.ChangeObjectState(Cor, EntityState.Modified);
                    db.SaveChanges();
                }

                //Send mail to Approvers & Requester 
                EmailSendofRequestReject(long.Parse(Order.RequestBy.ToString()), RequestID, reasoncode, conn);
                // EmailSendWhenRequestSubmit(RequestID, conn);

                //Update Available balance of Ordered Product 
                UpdateAvailableBalanceAfterRequestReject(RequestID, conn);
            }
        }

        protected void UpdateAvailableBalanceAfterRequestReject(long RequestID, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                string ordtype = "";
                DataSet dsOrdType = new DataSet();
                dsOrdType = fillds("Select * from torderhead where id=" + RequestID + "", conn);
                if (dsOrdType.Tables[0].Rows.Count > 0)
                {
                    ordtype = Convert.ToString(dsOrdType.Tables[0].Rows[0]["ordertype"]);

                    if (ordtype == "PreOrder")
                    {
                        DataSet dsPrdDetails = new DataSet();
                        dsPrdDetails = fillds("Select * from tOrderDetail where OrderHeadId=" + RequestID + "", conn);
                        int PrdCnt = dsPrdDetails.Tables[0].Rows.Count;
                        if (PrdCnt > 0)
                        {
                            for (int i = 0; i <= PrdCnt - 1; i++)
                            {
                                //update tProductstockDetails set ResurveQty=ResurveQty-@Qty1,TotalDispatchQty=TotalDispatchQty+@Qty1  where ProdID=@SkuId1                     
                                tProductStockDetail psd = new tProductStockDetail();
                                long SkuID = long.Parse(dsPrdDetails.Tables[0].Rows[i]["SkuId"].ToString());
                                decimal Qty = decimal.Parse(dsPrdDetails.Tables[0].Rows[i]["OrderQty"].ToString());

                                mProduct prd = new mProduct();
                                prd = db.mProducts.Where(p => p.ID == SkuID).FirstOrDefault();
                                if (prd.GroupSet == "Yes")
                                {
                                    DataSet dsBomProds = new DataSet();
                                    dsBomProds = fillds("select SKUId,Quantity from mBOMDetail BD where BD.BOMheaderId=" + SkuID + "", conn);
                                    if (dsBomProds.Tables[0].Rows.Count > 0)
                                    {
                                        for (int b = 0; b <= dsBomProds.Tables[0].Rows.Count - 1; b++)
                                        {
                                            long bomPrd = long.Parse(dsBomProds.Tables[0].Rows[b]["SKUId"].ToString());
                                            decimal bomQty = decimal.Parse(dsBomProds.Tables[0].Rows[b]["Quantity"].ToString());

                                            decimal FinalQty = Qty * bomQty;

                                            using (SqlCommand cmd = new SqlCommand())
                                            {
                                                cmd.CommandType = CommandType.StoredProcedure;
                                                cmd.CommandText = "GWC_SP_UpdatetProductstockDetailsAvailableResurveQtyRej";
                                                cmd.Connection = svr.GetSqlConn(conn);
                                                cmd.Parameters.Clear();
                                                cmd.Parameters.AddWithValue("SkuID", bomPrd);
                                                cmd.Parameters.AddWithValue("Qty", FinalQty);
                                                cmd.ExecuteNonQuery();
                                            }

                                            //psd = db.tProductStockDetails.Where(a => a.ProdID == bomPrd).FirstOrDefault();
                                            //if (psd != null)
                                            //{
                                            //    db.tProductStockDetails.Detach(psd);
                                            //    psd.ResurveQty = psd.ResurveQty - FinalQty;
                                            //    psd.AvailableBalance = psd.AvailableBalance + FinalQty;
                                            //    db.tProductStockDetails.Attach(psd);
                                            //    db.ObjectStateManager.ChangeObjectState(psd, EntityState.Modified);
                                            //    db.SaveChanges();
                                            //}
                                            InsertIntotInventory(bomPrd, RequestID, FinalQty, "Reject", conn);
                                        }
                                    }
                                }
                                else if (prd.GroupSet == "No")
                                {
                                    //psd = db.tProductStockDetails.Where(a => a.ProdID == SkuID).FirstOrDefault();
                                    //if (psd != null)
                                    //{
                                    //    db.tProductStockDetails.Detach(psd);
                                    //    psd.ResurveQty = psd.ResurveQty - Qty;
                                    //    psd.AvailableBalance = psd.AvailableBalance + Qty;
                                    //    db.tProductStockDetails.Attach(psd);
                                    //    db.ObjectStateManager.ChangeObjectState(psd, EntityState.Modified);
                                    //    db.SaveChanges();
                                    //}

                                    using (SqlCommand cmd = new SqlCommand())
                                    {
                                        cmd.CommandType = CommandType.StoredProcedure;
                                        cmd.CommandText = "GWC_SP_UpdatetPrdstkDetailsAvailableResurveQtyRejforpreorder";
                                        cmd.Connection = svr.GetSqlConn(conn);
                                        cmd.Parameters.Clear();
                                        cmd.Parameters.AddWithValue("SkuID", SkuID);
                                        cmd.Parameters.AddWithValue("Qty", Qty);
                                        cmd.ExecuteNonQuery();
                                    }

                                    InsertIntotInventory(SkuID, RequestID, Qty, "Reject", conn);
                                }
                            }
                        }
                    }
                    else
                    {
                        DataSet dsPrdDetails = new DataSet();
                        dsPrdDetails = fillds("Select * from tOrderDetail where OrderHeadId=" + RequestID + "", conn);
                        int PrdCnt = dsPrdDetails.Tables[0].Rows.Count;
                        if (PrdCnt > 0)
                        {
                            for (int i = 0; i <= PrdCnt - 1; i++)
                            {
                                //update tProductstockDetails set ResurveQty=ResurveQty-@Qty1,TotalDispatchQty=TotalDispatchQty+@Qty1  where ProdID=@SkuId1                     
                                tProductStockDetail psd = new tProductStockDetail();
                                long SkuID = long.Parse(dsPrdDetails.Tables[0].Rows[i]["SkuId"].ToString());
                                decimal Qty = decimal.Parse(dsPrdDetails.Tables[0].Rows[i]["OrderQty"].ToString());

                                //added by suraj J. for product "SimFMSIn" not maintin stock adjust for Delivery type is HUB
                                string ChkOrderDeliveryType = ""; long EcommId = 0;
                                DataSet dsOrddeliType = new DataSet();
                                dsOrddeliType = fillds("select * from tecommhead where deliverytype='HUB' and orderid= " + RequestID + "", conn);
                                if (dsOrddeliType.Tables[0].Rows.Count > 0)
                                {
                                    ChkOrderDeliveryType = "HUB";
                                    EcommId = Convert.ToInt64(dsOrddeliType.Tables[0].Rows[0]["ID"].ToString());
                                }
                                else
                                {
                                    ChkOrderDeliveryType = "Home";
                                }

                                if (ChkOrderDeliveryType == "HUB")
                                {
                                    mProduct prd = new mProduct();
                                    prd = db.mProducts.Where(p => p.ID == SkuID).FirstOrDefault();
                                    if (prd.GroupSet == "Yes")
                                    {
                                        DataSet dsBomProds = new DataSet();
                                        dsBomProds = fillds("select SKUId,Quantity from mBOMDetail BD where BD.BOMheaderId=" + SkuID + "", conn);
                                        if (dsBomProds.Tables[0].Rows.Count > 0)
                                        {
                                            for (int b = 0; b <= dsBomProds.Tables[0].Rows.Count - 1; b++)
                                            {
                                                long bomPrd = long.Parse(dsBomProds.Tables[0].Rows[b]["SKUId"].ToString());
                                                decimal bomQty = decimal.Parse(dsBomProds.Tables[0].Rows[b]["Quantity"].ToString());

                                                decimal FinalQty = Qty * bomQty;

                                                using (SqlCommand cmd = new SqlCommand())
                                                {
                                                    cmd.CommandType = CommandType.StoredProcedure;
                                                    cmd.CommandText = "GWC_SP_UpdatetProductstockDetailsAvailableResurveQtyRej";
                                                    cmd.Connection = svr.GetSqlConn(conn);
                                                    cmd.Parameters.Clear();
                                                    cmd.Parameters.AddWithValue("SkuID", bomPrd);
                                                    cmd.Parameters.AddWithValue("Qty", FinalQty);
                                                    cmd.ExecuteNonQuery();
                                                }

                                                //psd = db.tProductStockDetails.Where(a => a.ProdID == bomPrd).FirstOrDefault();
                                                //if (psd != null)
                                                //{
                                                //    db.tProductStockDetails.Detach(psd);
                                                //    psd.ResurveQty = psd.ResurveQty - FinalQty;
                                                //    psd.AvailableBalance = psd.AvailableBalance + FinalQty;
                                                //    db.tProductStockDetails.Attach(psd);
                                                //    db.ObjectStateManager.ChangeObjectState(psd, EntityState.Modified);
                                                //    db.SaveChanges();
                                                //}
                                                InsertIntotInventory(bomPrd, RequestID, FinalQty, "Reject", conn);
                                            }
                                        }
                                    }
                                    else if (prd.GroupSet == "No")
                                    {
                                        //psd = db.tProductStockDetails.Where(a => a.ProdID == SkuID).FirstOrDefault();
                                        //if (psd != null)
                                        //{
                                        //    db.tProductStockDetails.Detach(psd);
                                        //    psd.ResurveQty = psd.ResurveQty - Qty;
                                        //    psd.AvailableBalance = psd.AvailableBalance + Qty;
                                        //    db.tProductStockDetails.Attach(psd);
                                        //    db.ObjectStateManager.ChangeObjectState(psd, EntityState.Modified);
                                        //    db.SaveChanges();
                                        //}
                                        //added by suraj J.
                                        string productisSimFMSIn = "";
                                        DataSet dsSimFMSIn = new DataSet();
                                        dsSimFMSIn = fillds("select * from tecommdetail where ecomheadid=" + EcommId + " and productcode='SimFMSIn' and packagetype='FMS-INDOOR' and productcode='" + prd.ProductCode + "' ", conn);
                                        if (dsSimFMSIn.Tables[0].Rows.Count > 0)
                                        {

                                        }
                                        else
                                        {
                                            using (SqlCommand cmd = new SqlCommand())
                                            {
                                                cmd.CommandType = CommandType.StoredProcedure;
                                                cmd.CommandText = "GWC_SP_UpdatetProductstockDetailsAvailableResurveQtyRej";
                                                cmd.Connection = svr.GetSqlConn(conn);
                                                cmd.Parameters.Clear();
                                                cmd.Parameters.AddWithValue("SkuID", SkuID);
                                                cmd.Parameters.AddWithValue("Qty", Qty);
                                                cmd.ExecuteNonQuery();
                                            }
                                        }

                                        InsertIntotInventory(SkuID, RequestID, Qty, "Reject", conn);
                                    }
                                }
                                else
                                {
                                    mProduct prd = new mProduct();
                                    prd = db.mProducts.Where(p => p.ID == SkuID).FirstOrDefault();
                                    if (prd.GroupSet == "Yes")
                                    {
                                        DataSet dsBomProds = new DataSet();
                                        dsBomProds = fillds("select SKUId,Quantity from mBOMDetail BD where BD.BOMheaderId=" + SkuID + "", conn);
                                        if (dsBomProds.Tables[0].Rows.Count > 0)
                                        {
                                            for (int b = 0; b <= dsBomProds.Tables[0].Rows.Count - 1; b++)
                                            {
                                                long bomPrd = long.Parse(dsBomProds.Tables[0].Rows[b]["SKUId"].ToString());
                                                decimal bomQty = decimal.Parse(dsBomProds.Tables[0].Rows[b]["Quantity"].ToString());

                                                decimal FinalQty = Qty * bomQty;

                                                using (SqlCommand cmd = new SqlCommand())
                                                {
                                                    cmd.CommandType = CommandType.StoredProcedure;
                                                    cmd.CommandText = "GWC_SP_UpdatetProductstockDetailsAvailableResurveQtyRej";
                                                    cmd.Connection = svr.GetSqlConn(conn);
                                                    cmd.Parameters.Clear();
                                                    cmd.Parameters.AddWithValue("SkuID", bomPrd);
                                                    cmd.Parameters.AddWithValue("Qty", FinalQty);
                                                    cmd.ExecuteNonQuery();
                                                }

                                                //psd = db.tProductStockDetails.Where(a => a.ProdID == bomPrd).FirstOrDefault();
                                                //if (psd != null)
                                                //{
                                                //    db.tProductStockDetails.Detach(psd);
                                                //    psd.ResurveQty = psd.ResurveQty - FinalQty;
                                                //    psd.AvailableBalance = psd.AvailableBalance + FinalQty;
                                                //    db.tProductStockDetails.Attach(psd);
                                                //    db.ObjectStateManager.ChangeObjectState(psd, EntityState.Modified);
                                                //    db.SaveChanges();
                                                //}
                                                InsertIntotInventory(bomPrd, RequestID, FinalQty, "Reject", conn);
                                            }
                                        }
                                    }
                                    else if (prd.GroupSet == "No")
                                    {
                                        //psd = db.tProductStockDetails.Where(a => a.ProdID == SkuID).FirstOrDefault();
                                        //if (psd != null)
                                        //{
                                        //    db.tProductStockDetails.Detach(psd);
                                        //    psd.ResurveQty = psd.ResurveQty - Qty;
                                        //    psd.AvailableBalance = psd.AvailableBalance + Qty;
                                        //    db.tProductStockDetails.Attach(psd);
                                        //    db.ObjectStateManager.ChangeObjectState(psd, EntityState.Modified);
                                        //    db.SaveChanges();
                                        //}

                                        using (SqlCommand cmd = new SqlCommand())
                                        {
                                            cmd.CommandType = CommandType.StoredProcedure;
                                            cmd.CommandText = "GWC_SP_UpdatetProductstockDetailsAvailableResurveQtyRej";
                                            cmd.Connection = svr.GetSqlConn(conn);
                                            cmd.Parameters.Clear();
                                            cmd.Parameters.AddWithValue("SkuID", SkuID);
                                            cmd.Parameters.AddWithValue("Qty", Qty);
                                            cmd.ExecuteNonQuery();
                                        }

                                        InsertIntotInventory(SkuID, RequestID, Qty, "Reject", conn);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        protected void UpdateTProductStockReserveQtyTotalDispatchQty(long RequestID, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                tProductStockDetail psd = new tProductStockDetail();
                try
                {
                    DataSet dsPrdDetails = new DataSet();
                    dsPrdDetails = fillds("Select * from tOrderDetail where OrderHeadId=" + RequestID + "", conn);
                    int PrdCnt = dsPrdDetails.Tables[0].Rows.Count;
                    if (PrdCnt > 0)
                    {
                        // insertqty(3,Convert.ToInt64(PrdCnt), "loop", conn);
                        for (int i = 0; i <= PrdCnt - 1; i++)
                        {
                            //update tProductstockDetails set ResurveQty=ResurveQty-@Qty1,TotalDispatchQty=TotalDispatchQty+@Qty1  where ProdID=@SkuId1
                            long SkuID = long.Parse(dsPrdDetails.Tables[0].Rows[i]["SkuId"].ToString());
                            decimal Qty = decimal.Parse(dsPrdDetails.Tables[0].Rows[i]["OrderQty"].ToString());
                            //insertqty(Qty, SkuID,"start",conn);
                            //added by suraj J. for product "SimFMSIn" not maintin stock adjust for Delivery type is HUB
                            string ChkOrderDeliveryType = ""; long EcommId = 0;
                            DataSet dsOrdType = new DataSet();
                            dsOrdType = fillds("select * from tecommhead where deliverytype='HUB' and orderid= " + RequestID + "", conn);
                            if (dsOrdType.Tables[0].Rows.Count > 0)
                            {
                                ChkOrderDeliveryType = "HUB";
                                EcommId = Convert.ToInt64(dsOrdType.Tables[0].Rows[0]["ID"].ToString());
                            }
                            else
                            {
                                ChkOrderDeliveryType = "Home";
                            }

                            if (ChkOrderDeliveryType == "HUB")
                            {
                                mProduct prd = new mProduct();
                                prd = db.mProducts.Where(p => p.ID == SkuID).FirstOrDefault();
                                if (prd.GroupSet == "Yes")
                                {
                                    DataSet dsBomProds = new DataSet();
                                    dsBomProds = fillds("select SKUId,Quantity from mBOMDetail BD where BD.BOMheaderId=" + SkuID + "", conn);
                                    if (dsBomProds.Tables[0].Rows.Count > 0)
                                    {
                                        for (int b = 0; b <= dsBomProds.Tables[0].Rows.Count - 1; b++)
                                        {
                                            long bomPrd = long.Parse(dsBomProds.Tables[0].Rows[b]["SKUId"].ToString());
                                            decimal bomQty = decimal.Parse(dsBomProds.Tables[0].Rows[b]["Quantity"].ToString());

                                            decimal FinalQty = Qty * bomQty;

                                            using (SqlCommand cmd = new SqlCommand())
                                            {
                                                cmd.CommandType = CommandType.StoredProcedure;
                                                cmd.CommandText = "GWC_SP_UpdatetProductstockDetailsDispatchResurveQty";
                                                cmd.Connection = svr.GetSqlConn(conn);
                                                cmd.Parameters.Clear();
                                                cmd.Parameters.AddWithValue("SkuID", bomPrd);
                                                cmd.Parameters.AddWithValue("Qty", FinalQty);
                                                cmd.ExecuteNonQuery();
                                                cmd.Connection.Close();
                                            }
                                            // InsertIntotInventory(bomPrd, RequestID, FinalQty, "Dispatch", conn);
                                        }
                                    }
                                }
                                else if (prd.GroupSet == "No")
                                {
                                    if (prd.ProductType == "Virtual SKU" && prd.ProductType != null)
                                    {
                                        using (SqlCommand cmd = new SqlCommand())
                                        {
                                            cmd.CommandType = CommandType.StoredProcedure;
                                            cmd.CommandText = "GWC_SP_UpdatetProductstockDetailsDispatchResurveQtyforVirtualSku";
                                            cmd.Connection = svr.GetSqlConn(conn);
                                            cmd.Parameters.Clear();
                                            cmd.Parameters.AddWithValue("SkuID", SkuID);
                                            cmd.Parameters.AddWithValue("Qty", Qty);
                                            cmd.Parameters.AddWithValue("@OderID", RequestID);
                                            cmd.ExecuteNonQuery();
                                            cmd.Connection.Close();
                                        }
                                    }
                                    else
                                    {
                                        //added by suraj J.
                                        string productisSimFMSIn = "";
                                        DataSet dsSimFMSIn = new DataSet();
                                        dsSimFMSIn = fillds("select * from tecommdetail where ecomheadid=" + EcommId + " and productcode='SimFMSIn' and packagetype='FMS-INDOOR' and productcode='" + prd.ProductCode + "' ", conn);
                                        if (dsSimFMSIn.Tables[0].Rows.Count > 0)
                                        {

                                        }
                                        else
                                        {
                                            using (SqlCommand cmd = new SqlCommand())
                                            {
                                                cmd.CommandType = CommandType.StoredProcedure;
                                                cmd.CommandText = "GWC_SP_UpdatetProductstockDetailsDispatchResurveQty";
                                                cmd.Connection = svr.GetSqlConn(conn);
                                                cmd.Parameters.Clear();
                                                cmd.Parameters.AddWithValue("SkuID", SkuID);
                                                cmd.Parameters.AddWithValue("Qty", Qty);
                                                cmd.ExecuteNonQuery();
                                                cmd.Connection.Close();
                                            }
                                        }
                                    }
                                    // InsertIntotInventory(SkuID, RequestID, Qty, "Dispatch", conn);
                                }
                            }
                            else
                            {
                                mProduct prd = new mProduct();
                                prd = db.mProducts.Where(p => p.ID == SkuID).FirstOrDefault();
                                if (prd.GroupSet == "Yes")
                                {
                                    DataSet dsBomProds = new DataSet();
                                    dsBomProds = fillds("select SKUId,Quantity from mBOMDetail BD where BD.BOMheaderId=" + SkuID + "", conn);
                                    if (dsBomProds.Tables[0].Rows.Count > 0)
                                    {
                                        for (int b = 0; b <= dsBomProds.Tables[0].Rows.Count - 1; b++)
                                        {
                                            long bomPrd = long.Parse(dsBomProds.Tables[0].Rows[b]["SKUId"].ToString());
                                            decimal bomQty = decimal.Parse(dsBomProds.Tables[0].Rows[b]["Quantity"].ToString());

                                            decimal FinalQty = Qty * bomQty;

                                            using (SqlCommand cmd = new SqlCommand())
                                            {
                                                cmd.CommandType = CommandType.StoredProcedure;
                                                cmd.CommandText = "GWC_SP_UpdatetProductstockDetailsDispatchResurveQty";
                                                cmd.Connection = svr.GetSqlConn(conn);
                                                cmd.Parameters.Clear();
                                                cmd.Parameters.AddWithValue("SkuID", bomPrd);
                                                cmd.Parameters.AddWithValue("Qty", FinalQty);
                                                cmd.ExecuteNonQuery();
                                                cmd.Connection.Close();
                                            }
                                            //  InsertIntotInventory(bomPrd, RequestID, FinalQty, "Dispatch", conn);
                                        }
                                    }
                                }
                                else if (prd.GroupSet == "No")
                                {

                                    if (prd.ProductType == "Virtual SKU" && prd.ProductType != null)
                                    {
                                        using (SqlCommand cmd = new SqlCommand())
                                        {
                                            cmd.CommandType = CommandType.StoredProcedure;
                                            cmd.CommandText = "GWC_SP_UpdatetProductstockDetailsDispatchResurveQtyforVirtualSku";
                                            cmd.Connection = svr.GetSqlConn(conn);
                                            cmd.Parameters.Clear();
                                            cmd.Parameters.AddWithValue("SkuID", SkuID);
                                            cmd.Parameters.AddWithValue("Qty", Qty);
                                            cmd.Parameters.AddWithValue("@OderID", RequestID);
                                            cmd.ExecuteNonQuery();
                                            cmd.Connection.Close();
                                        }

                                        // insertqty(Qty, SkuID, "second", conn);
                                    }
                                    else
                                    {
                                        using (SqlCommand cmd = new SqlCommand())
                                        {
                                            cmd.CommandType = CommandType.StoredProcedure;
                                            cmd.CommandText = "GWC_SP_UpdatetProductstockDetailsDispatchResurveQty";
                                            cmd.Connection = svr.GetSqlConn(conn);
                                            cmd.Parameters.Clear();
                                            cmd.Parameters.AddWithValue("SkuID", SkuID);
                                            cmd.Parameters.AddWithValue("Qty", Qty);
                                            cmd.ExecuteNonQuery();
                                            cmd.Connection.Close();
                                        }
                                    }
                                    // InsertIntotInventory(SkuID, RequestID, Qty, "Dispatch", conn);
                                }
                            }
                        }
                    }
                }
                catch { }
                finally { }
            }
        }
        protected void insertqty(decimal qty, long skuid, string text, string[] conn)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "insertqty";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@qty", qty);
                cmd.Parameters.AddWithValue("@skuid", skuid);
                cmd.Parameters.AddWithValue("@text", text);
                cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }
        }
        public void InsertIntotInventory(long SkuID, long RequestID, decimal Qty, string TransactionType, string[] conn)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GWC_SP_InsertIntotInventry";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("SKUId", SkuID);
                cmd.Parameters.AddWithValue("TransactionId", RequestID);
                cmd.Parameters.AddWithValue("TransactionType", TransactionType);
                cmd.Parameters.AddWithValue("Quantity", Qty);
                cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }
        }

        public long GetPreviousStatusID(long RequestId, string[] conn)
        {
            long PrvStatusId = 0;
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                tOrderHead Order = new tOrderHead();
                Order = db.tOrderHeads.Where(r => r.Id == RequestId).FirstOrDefault();
                PrvStatusId = long.Parse(Order.Status.ToString());
                return PrvStatusId;
            }
        }

        public long GetPreviousStatusID_New(long RequestId, string[] conn)
        {
            SqlDataReader dr;
            long PrvStatusId = 0;
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetPreviousStatusID";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@RequestId", RequestId);
                dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    PrvStatusId = long.Parse(dr["ApprovedFlag"].ToString());
                }
                dr.Close();
                cmd.Connection.Close();
            }
            return PrvStatusId;
        }

        public void InsertIntotCorrespond(tCorrespond Msg, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            db.tCorresponds.AddObject(Msg);
            db.SaveChanges();
        }

        public DataSet GetCorrespondance(long RequestID, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from VW_CorrespondanceDetail where OrderHeadId=" + RequestID + " and MailStatus!=0 order by Id Desc ", conn);
            return ds;
        }

        public tCorrespond GetCorrespondanceDetail(long CORID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            tCorrespond tCor = new tCorrespond();
            tCor = db.tCorresponds.Where(c => c.Id == CORID).FirstOrDefault();
            db.tCorresponds.Detach(tCor);
            return tCor;
        }

        public void AddIntomMessageTrans(long RequestID, string[] conn)
        {

            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                //mMessageTran Msg = new mMessageTran();
                string Msgnew = "";

                GWC_VW_MsgOrderHead MsgHD = new GWC_VW_MsgOrderHead();
                MsgHD = db.GWC_VW_MsgOrderHead.Where(h => h.Id == RequestID).FirstOrDefault();

                string ContactPerson2 = MsgHD.ContactP2;

                string ContactPerson2Names = "";
                if (ContactPerson2 == "NA" || ContactPerson2 == "" || ContactPerson2 == null) { ContactPerson2Names = "NA"; }
                else
                { ContactPerson2Names = GetContactPersonNames(ContactPerson2, conn); }

                /*New Code For Contact 2 MobileNo*/
                string ContactMobileNo = "";
                if (ContactPerson2Names == "NA")
                {
                    ContactMobileNo = MsgHD.Con1MobileNo.ToString();
                }
                else
                {
                    ContactMobileNo = GetContactPersonMobileNo(ContactPerson2, conn);
                }
                /*New Code For Contact 2 MobileNo*/

                //Added by suraj 
                string ContactMobile2 = "";
                ContactMobile2 = GetContactMobile2(RequestID, conn);
                //Get PaymentMethodValue
                string PaymentMethodValue = "";
                PaymentMethodValue = GetPaymentMethodValue(RequestID, conn);
                //address2 and paymentmethdnm
                string address2 = "", paymentmethdnm = "";
                DataTable dtadd2 = new DataTable();
                dtadd2 = GetAddressLine2(RequestID, conn);
                if (dtadd2.Rows.Count > 0)
                {
                    address2 = dtadd2.Rows[0]["AddressLine2"].ToString();
                    paymentmethdnm = dtadd2.Rows[0]["MethodName"].ToString();
                }
                // Msg.MsgDescription = MsgHD.Id + " | " + CheckString(MsgHD.OrderNumber) + " | " + MsgHD.Orderdate.Value.ToShortDateString() + " | " + MsgHD.Deliverydate.Value.ToShortDateString() + " | " + MsgHD.StoreCode + " | " + MsgHD.Con1 + " | " + ContactPerson2Names + " | " + CheckString(MsgHD.AddressLine1) + " | " + CheckString(MsgHD.Remark) + " | " + MsgHD.OrderNo + " | " + MsgHD.InvoiceNo + " | " + MsgHD.LocationCode + " | " + MsgHD.RequestorName + " | " + MsgHD.RequestorMobileNo + " | " + MsgHD.ConsigneeName + " | " + MsgHD.ConsigneeAddress + " | " + MsgHD.ConsigneePhone + " | " + ContactMobileNo;
                Msgnew = MsgHD.Id + " | " + CheckString(MsgHD.OrderNumber) + " | " + MsgHD.Orderdate.Value.ToShortDateString() + " | " + MsgHD.Deliverydate.Value.ToShortDateString() + " | " + MsgHD.StoreCode + " | " + MsgHD.Con1 + " | " + ContactPerson2Names + " | " + CheckString(MsgHD.AddressLine1) + " | " + CheckString(MsgHD.Remark) + " | " + MsgHD.OrderNo + " | " + MsgHD.InvoiceNo + " | " + MsgHD.LocationCode + " | " + MsgHD.RequestorName + " | " + MsgHD.RequestorMobileNo + " | " + MsgHD.ConsigneeName + " | " + MsgHD.ConsigneeAddress + " | " + MsgHD.ConsigneePhone + " | " + ContactMobileNo + " | " + ContactMobile2 + " | " + CheckString(address2) + " | " + paymentmethdnm + " | " + PaymentMethodValue + " | " + MsgHD.ProjectType + " | " + MsgHD.SiteCode + " | " + MsgHD.SiteName + " | " + MsgHD.Latitude + " | " + MsgHD.Longitude + " | " + MsgHD.AccessRequirement;
                // old code DataSet ds = new DataSet();
                //ds = fillds("select * from GWC_VW_MsgOrderDetail where OrderHeadId=" + RequestID + " ", conn);
                //int Cnt = ds.Tables[0].Rows.Count;
                //if (Cnt > 0)
                //{
                //    for (int i = 0; i <= Cnt - 1; i++)
                //    {
                //        long PrdID = long.Parse(ds.Tables[0].Rows[i]["SkuId"].ToString());
                //        mProduct prd = new mProduct();
                //        prd = db.mProducts.Where(p => p.ID == PrdID).FirstOrDefault();
                //        if (prd.GroupSet == "Yes")
                //        {
                //            DataSet dsBomProds = new DataSet();
                //            dsBomProds = fillds("select SKUId,Quantity from mBOMDetail BD where BD.BOMheaderId=" + PrdID + "", conn);
                //            if (dsBomProds.Tables[0].Rows.Count > 0)
                //            {
                //                for (int b = 0; b <= dsBomProds.Tables[0].Rows.Count - 1; b++)
                //                {
                //                    decimal bomQty = decimal.Parse(dsBomProds.Tables[0].Rows[b]["Quantity"].ToString());
                //                    decimal Qty = decimal.Parse(ds.Tables[0].Rows[i]["OrderQty"].ToString());
                //                    decimal FinalQty = Qty * bomQty;

                //                    long bomPrd = long.Parse(dsBomProds.Tables[0].Rows[b]["SKUId"].ToString());
                //                    DataSet dsPrdName = new DataSet();
                //                    dsPrdName = fillds("select ProductCode from mproduct where ID=" + bomPrd + "", conn);
                //                    string ProductCode = dsPrdName.Tables[0].Rows[0]["ProductCode"].ToString();

                //                    // Msg.MsgDescription = Msg.MsgDescription + " | " + ProductCode + " | " + ds.Tables[0].Rows[i]["UOM"].ToString() + " | " + FinalQty;
                //                    Msgnew = Msgnew + " | " + ProductCode + " | " + ds.Tables[0].Rows[i]["UOM"].ToString() + " | " + FinalQty;
                //                }
                //            }
                //        }
                //        else if (prd.GroupSet == "No")
                //        {
                //            //  Msg.MsgDescription = Msg.MsgDescription + " | " + ds.Tables[0].Rows[i]["Prod_Code"].ToString() + " | " + ds.Tables[0].Rows[i]["UOM"].ToString() + " | " + ds.Tables[0].Rows[i]["OrderQty"].ToString();
                //            Msgnew = Msgnew + " | " + ds.Tables[0].Rows[i]["Prod_Code"].ToString() + " | " + ds.Tables[0].Rows[i]["UOM"].ToString() + " | " + ds.Tables[0].Rows[i]["OrderQty"].ToString();
                //        }
                //    }
                //}


                //***condition to check if Serial No changes flag applicable to Company i.e. Vodafone technical
                DataSet ds = new DataSet();
                if (MsgHD.ChkApproval == "Yes")
                {
                    ds = GetMessageserialvalues(RequestID, conn);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        //  Msg.MsgDescription = Msg.MsgDescription + ds.Tables[0].Rows[0]["SerialNo"].ToString();
                        Msgnew = Msgnew + ds.Tables[0].Rows[0]["SerialNo"].ToString();
                    }
                }
                else
                {
                    ds = fillds("select * from GWC_VW_MsgOrderDetail where OrderHeadId=" + RequestID + " ", conn);
                    int Cnt = ds.Tables[0].Rows.Count;
                    if (Cnt > 0)
                    {
                        for (int i = 0; i <= Cnt - 1; i++)
                        {
                            long PrdID = long.Parse(ds.Tables[0].Rows[i]["SkuId"].ToString());
                            mProduct prd = new mProduct();
                            prd = db.mProducts.Where(p => p.ID == PrdID).FirstOrDefault();
                            if (prd.GroupSet == "Yes")
                            {
                                DataSet dsBomProds = new DataSet();
                                dsBomProds = fillds("select SKUId,Quantity from mBOMDetail BD where BD.BOMheaderId=" + PrdID + "", conn);
                                if (dsBomProds.Tables[0].Rows.Count > 0)
                                {
                                    for (int b = 0; b <= dsBomProds.Tables[0].Rows.Count - 1; b++)
                                    {
                                        decimal bomQty = decimal.Parse(dsBomProds.Tables[0].Rows[b]["Quantity"].ToString());
                                        decimal Qty = decimal.Parse(ds.Tables[0].Rows[i]["OrderQty"].ToString());
                                        decimal FinalQty = Qty * bomQty;

                                        long bomPrd = long.Parse(dsBomProds.Tables[0].Rows[b]["SKUId"].ToString());
                                        DataSet dsPrdName = new DataSet();
                                        dsPrdName = fillds("select ProductCode from mproduct where ID=" + bomPrd + "", conn);
                                        string ProductCode = dsPrdName.Tables[0].Rows[0]["ProductCode"].ToString();

                                        Msgnew = Msgnew + " | " + ProductCode + " | " + ds.Tables[0].Rows[i]["UOM"].ToString() + " | " + "NA" + " | " + "No" + " | " + FinalQty;
                                    }
                                }
                            }
                            else if (prd.GroupSet == "No")
                            {
                                Msgnew = Msgnew + " | " + ds.Tables[0].Rows[i]["Prod_Code"].ToString() + " | " + ds.Tables[0].Rows[i]["UOM"].ToString() + " | " + ds.Tables[0].Rows[i]["SerialNumber"].ToString() + " | " + ds.Tables[0].Rows[i]["serialflag"].ToString() + " | " + ds.Tables[0].Rows[i]["OrderQty"].ToString();
                            }
                        }
                    }
                }


                //Msg.MessageHdrId = 1;
                //Msg.Object = "Order";
                //// Msg.Destination = "WMS";
                //Msg.Destination = MsgHD.StoreCode;
                //Msg.Status = 0;
                //Msg.CreationDate = DateTime.Now;
                //Msg.Createdby = "OMS";

                //db.mMessageTrans.AddObject(Msg);
                //db.SaveChanges();

                using (SqlCommand c = new SqlCommand())
                {
                    c.CommandType = CommandType.StoredProcedure;
                    c.CommandText = "SP_SavemMessageTrans";
                    c.Connection = svr.GetSqlConn(conn);
                    c.Parameters.Clear();
                    c.Parameters.AddWithValue("MessageHdrId", "1");
                    c.Parameters.AddWithValue("MsgDescription", Msgnew);
                    c.Parameters.AddWithValue("Object", "Order");
                    c.Parameters.AddWithValue("Destination", MsgHD.StoreCode);
                    c.Parameters.AddWithValue("Status", "0");
                    c.Parameters.AddWithValue("CreationDate", DateTime.Now);
                    c.Parameters.AddWithValue("Createdby", "OMS");
                    c.ExecuteNonQuery();
                }
            }

        }



        public string GetContectName(long RequestID, string[] conn)
        {
            string Contactid = "";
            DataSet ds = new DataSet();
            ds = fillds("select con2 from torderhead where ID " + RequestID + "", conn);
            int cnt = ds.Tables[0].Rows.Count;
            if (cnt > 0)
            {
                Contactid = Convert.ToString(ds.Tables[0].Rows[0]["Name"].ToString());
            }
            return Contactid;
        }

        public string GetPaymentMethodValue(long RequestID, string[] conn)
        {
            string result = "";
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                DataSet ds = new DataSet();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetPaymentMethodValue";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@RequestID", RequestID);
                da.SelectCommand = cmd;
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    result = Convert.ToString(ds.Tables[0].Rows[0]["PaymentValue"].ToString());
                }
                else
                {
                    result = "0";
                }
                cmd.Connection.Close();
            }
            return result;
        }



        public string GetContactMobile2(long RequestID, string[] conn)
        {
            string result = "";
            using (SqlCommand cmd = new SqlCommand())
            {

                SqlDataAdapter da = new SqlDataAdapter();
                DataSet ds = new DataSet();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetContactMobile2";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@RequestID", RequestID);
                da.SelectCommand = cmd;
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    result = Convert.ToString(ds.Tables[0].Rows[0]["con2"].ToString());
                }
                cmd.Connection.Close();
            }
            return result;
        }

        public DataTable GetAddressLine2(long id, string[] conn)
        {
            DataTable dt1 = new DataTable();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetAddressLine2";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@RequestID", id);
                da.SelectCommand = cmd;
                da.Fill(dt1);
                cmd.Connection.Close();
            }
            return dt1;
        }


        //public void AddIntomMessageTrans(long RequestID, string[] conn)
        //{

        //    using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
        //    {
        //        mMessageTran Msg = new mMessageTran();
        //        string Msgnew = "";

        //        GWC_VW_MsgOrderHead MsgHD = new GWC_VW_MsgOrderHead();

        //        MsgHD = db.GWC_VW_MsgOrderHead.Where(h => h.Id == RequestID).FirstOrDefault();
        //        //Get Contact2 name and contactno 
        //        //string ContactPerson2 = MsgHD.ContactP2;
        //        string ContactPerson2 = "";
        //        ContactPerson2 = GetContectName(RequestID, conn);
        //        string ContactPerson2Names = "";
        //        if (ContactPerson2 == "NA" || ContactPerson2 == "" || ContactPerson2 == null || ContactPerson2 == "0") { ContactPerson2Names = "NA"; }
        //        else
        //        { ContactPerson2Names = GetContactPersonNames(ContactPerson2, conn); }

        //        /*New Code For Contact 2 MobileNo*/
        //        string ContactMobileNo2 = "";
        //        if (ContactPerson2Names == "NA")
        //        {
        //            ContactMobileNo2 = MsgHD.Con1MobileNo.ToString();
        //        }
        //        else
        //        {
        //            ContactMobileNo2 = GetContactPersonMobileNo(ContactPerson2, conn);
        //        }
        //        /*New Code For Contact 2 MobileNo*/
        //        //Get PaymentMethodValue
        //        string PaymentMethodValue = "";
        //        PaymentMethodValue = GetPaymentMethodValue(RequestID, conn);

        //        // Msg.MsgDescription = MsgHD.Id + " | " + CheckString(MsgHD.OrderNumber) + " | " + MsgHD.Orderdate.Value.ToShortDateString() + " | " + MsgHD.Deliverydate.Value.ToShortDateString() + " | " + MsgHD.StoreCode + " | " + MsgHD.Con1 + " | " + ContactPerson2Names + " | " + CheckString(MsgHD.AddressLine1) + " | " + CheckString(MsgHD.Remark) + " | " + MsgHD.OrderNo + " | " + MsgHD.InvoiceNo + " | " + MsgHD.LocationCode + " | " + MsgHD.RequestorName + " | " + MsgHD.RequestorMobileNo + " | " + MsgHD.ConsigneeName + " | " + MsgHD.ConsigneeAddress + " | " + MsgHD.ConsigneePhone + " | " + ContactMobileNo;
        //        Msgnew = MsgHD.Id + " | " + CheckString(MsgHD.OrderNumber) + " | " + MsgHD.Orderdate.Value.ToShortDateString() + " | " + MsgHD.Deliverydate.Value.ToShortDateString() + " | " + MsgHD.StoreCode + " | " + MsgHD.Con1 + " | " + ContactPerson2Names + " | " + CheckString(MsgHD.AddressLine1) + " | " + CheckString(MsgHD.Remark) + " | " + MsgHD.OrderNo + " | " + MsgHD.InvoiceNo + " | " + MsgHD.LocationCode + " | " + MsgHD.RequestorName + " | " + MsgHD.RequestorMobileNo + " | " + MsgHD.ConsigneeName + " | " + MsgHD.ConsigneeAddress + " | " + MsgHD.ConsigneePhone + " | " + MsgHD.Con1MobileNo.ToString() + " | " + ContactMobileNo2 + " | " + MsgHD.AddressLine2.ToString() + " | " + MsgHD.Methodname.ToString() + " | " + PaymentMethodValue;
        //        DataSet ds = new DataSet();
        //        ds = fillds("select * from GWC_VW_MsgOrderDetail where OrderHeadId=" + RequestID + " ", conn);
        //        int Cnt = ds.Tables[0].Rows.Count;
        //        if (Cnt > 0)
        //        {
        //            for (int i = 0; i <= Cnt - 1; i++)
        //            {
        //                long PrdID = long.Parse(ds.Tables[0].Rows[i]["SkuId"].ToString());
        //                mProduct prd = new mProduct();
        //                prd = db.mProducts.Where(p => p.ID == PrdID).FirstOrDefault();
        //                if (prd.GroupSet == "Yes")
        //                {
        //                    DataSet dsBomProds = new DataSet();
        //                    dsBomProds = fillds("select SKUId,Quantity from mBOMDetail BD where BD.BOMheaderId=" + PrdID + "", conn);
        //                    if (dsBomProds.Tables[0].Rows.Count > 0)
        //                    {
        //                        for (int b = 0; b <= dsBomProds.Tables[0].Rows.Count - 1; b++)
        //                        {
        //                            decimal bomQty = decimal.Parse(dsBomProds.Tables[0].Rows[b]["Quantity"].ToString());
        //                            decimal Qty = decimal.Parse(ds.Tables[0].Rows[i]["OrderQty"].ToString());
        //                            decimal FinalQty = Qty * bomQty;

        //                            long bomPrd = long.Parse(dsBomProds.Tables[0].Rows[b]["SKUId"].ToString());
        //                            DataSet dsPrdName = new DataSet();
        //                            dsPrdName = fillds("select ProductCode from mproduct where ID=" + bomPrd + "", conn);
        //                            string ProductCode = dsPrdName.Tables[0].Rows[0]["ProductCode"].ToString();

        //                            //  Msg.MsgDescription = Msg.MsgDescription + " | " + ProductCode + " | " + ds.Tables[0].Rows[i]["UOM"].ToString() + " | " + FinalQty;
        //                            Msgnew = Msgnew + " | " + ProductCode + " | " + ds.Tables[0].Rows[i]["UOM"].ToString() + " | " + FinalQty;
        //                        }
        //                    }
        //                }
        //                else if (prd.GroupSet == "No")
        //                {
        //                    //Msg.MsgDescription = Msg.MsgDescription + " | " + ds.Tables[0].Rows[i]["Prod_Code"].ToString() + " | " + ds.Tables[0].Rows[i]["UOM"].ToString() + " | " + ds.Tables[0].Rows[i]["OrderQty"].ToString();
        //                    Msgnew = Msgnew + " | " + ds.Tables[0].Rows[i]["Prod_Code"].ToString() + " | " + ds.Tables[0].Rows[i]["UOM"].ToString() + " | " + ds.Tables[0].Rows[i]["OrderQty"].ToString();
        //                }
        //            }
        //        }


        //        //Msg.MessageHdrId = 1;
        //        //Msg.Object = "Order";
        //        //// Msg.Destination = "WMS";
        //        //Msg.Destination = MsgHD.StoreCode;
        //        //Msg.Status = 0;
        //        //Msg.CreationDate = DateTime.Now;
        //        //Msg.Createdby = "OMS";

        //        //db.mMessageTrans.AddObject(Msg);
        //        //db.SaveChanges();

        //        using (SqlCommand c = new SqlCommand())
        //        {
        //            c.CommandType = CommandType.StoredProcedure;
        //            c.CommandText = "SP_SavemMessageTrans";
        //            c.Connection = svr.GetSqlConn(conn);
        //            c.Parameters.Clear();
        //            c.Parameters.AddWithValue("MessageHdrId", "1");
        //            c.Parameters.AddWithValue("MsgDescription", Msgnew);
        //            c.Parameters.AddWithValue("Object", "Order");
        //            c.Parameters.AddWithValue("Destination", MsgHD.StoreCode);
        //            c.Parameters.AddWithValue("Status", "0");
        //            // c.Parameters.AddWithValue("CreationDate", DateTime.Now);
        //            c.Parameters.AddWithValue("Createdby", "OMS");
        //            c.ExecuteNonQuery();
        //        }
        //    }

        //}


        public string GetContactPersonNames(string ContactPerson2, string[] conn)
        {
            string Contact1Name = "";
            DataSet ds = new DataSet();
            //ds = fillds("select Name from tcontactpersondetail where ID IN( " + ContactPerson2 + " )", conn);
            ds = fillds("select case when Name = '' then 'NA'  when Name is null  then 'NA' else Name end Name from tcontactpersondetail where ID IN( " + ContactPerson2 + " )", conn);
            int cnt = ds.Tables[0].Rows.Count;
            if (cnt > 0)
            {
                for (int i = 0; i < cnt; i++)
                {
                    if (i == 0) { Contact1Name = ds.Tables[0].Rows[i]["Name"].ToString(); }
                    else
                    {
                        Contact1Name = Contact1Name + "," + ds.Tables[0].Rows[i]["Name"].ToString();
                    }
                }
            }
            return Contact1Name;
        }

        public string GetContactPersonMobileNo(string ContactPerson2Names, string[] conn)
        {
            string Contact2MobileNo = "";
            DataSet ds = new DataSet();
            ds = fillds("select case when MobileNo = '' then 'NA'  when MobileNo is null  then 'NA' else MobileNo end MobileNo from tcontactpersondetail where ID IN( " + ContactPerson2Names + " )", conn);
            int cnt = ds.Tables[0].Rows.Count;
            if (cnt > 0)
            {
                for (int i = 0; i < cnt; i++)
                {
                    if (i == 0) { Contact2MobileNo = ds.Tables[0].Rows[i]["MobileNo"].ToString(); }
                    else
                    {
                        Contact2MobileNo = Contact2MobileNo + "," + ds.Tables[0].Rows[i]["MobileNo"].ToString();
                    }
                }
            }
            return Contact2MobileNo;
        }


        public DataSet GetBomDetails(string PrdID, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select BD.Id,BD.BOMheaderId,BD.SKUId,MP.ProductCode,MP.Name,MP.Description,BD.Quantity,BD.Sequence,BD.Remark from mBOMDetail BD left outer join mProduct MP on BD.SKUId=MP.ID where BD.BOMheaderId=" + PrdID + " ", conn);
            return ds;
        }

        public string GetSelectedUom(long OrderId, long ProdID, string[] conn)
        {
            string uomid = "";
            DataSet ds = new DataSet();
            //ds = fillds("select UOMID from tOrderDetail where Orderheadid="+ OrderId +" and SkuId="+ ProdID +"", conn);
            ds = fillds("select (CONVERT(VARCHAR(15),UOMID) + ':' + CONVERT(VARCHAR(15),Quantity)) as UMOGroup from VW_SkuUOMDetails where SkuId=" + ProdID + "  and  Quantity !=0 and UOMID=(select UOMID from tOrderDetail where Orderheadid=" + OrderId + " and SkuId=" + ProdID + ")", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                uomid = ds.Tables[0].Rows[0]["UMOGroup"].ToString();
            }
            else { uomid = "0"; }
            return uomid;
        }

        public string GetSelectedUomTemplate(long OrderId, long ProdID, string[] conn)
        {
            string uomid = "0";
            try
            {
                DataSet ds = new DataSet();
                //  ds = fillds("select (CONVERT(VARCHAR(15),UOMID) + ':' + CONVERT(VARCHAR(15),Quantity)) as UMOGroup from VW_SkuUOMDetails where SkuId=" + ProdID + " and Quantity !=0 and UOMID=(select UOMID from mRequestTemplateDetail where TemplateHeadID=" + OrderId + " and PrdID=" + ProdID + ")", conn);
                ds = fillds("select (CONVERT(VARCHAR(15),UOMID) + ':' + CONVERT(VARCHAR(15),Quantity)) as UMOGroup from VW_SkuUOMDetails where SkuId=" + ProdID + " and Quantity !=0 and UOMID=(select UOMID from mRequestTemplateDetail where TemplateHeadID=" + OrderId + " and PrdID=" + ProdID + ")", conn);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    uomid = ds.Tables[0].Rows[0]["UMOGroup"].ToString();
                }
                else { uomid = "0"; }
            }
            catch
            {
                DataSet ds1 = new DataSet();
                // ds1 = fillds("select (CONVERT(VARCHAR(15),UOMID) + ':' + CONVERT(VARCHAR(15),Quantity)) as UMOGroup from VW_SkuUOMDetails where SkuId=" + ProdID + " and Quantity !=0 ", conn);
                ds1 = fillds("select (CONVERT(VARCHAR(15),UOMID) + ':' + CONVERT(VARCHAR(15),Quantity)) as UMOGroup from VW_SkuUOMDetails where SkuId=" + ProdID + " and Quantity !=0 ", conn);
                if (ds1.Tables[0].Rows.Count > 0)
                {
                    uomid = ds1.Tables[0].Rows[2]["UMOGroup"].ToString();
                }
                else { uomid = "0"; }

            }
            finally { }
            return uomid;
        }


        public int GridRowCount(string paraSessionID, string paraCurrentObjectName, string paraUserID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<POR_SP_GetPartDetail_ForRequest_Result> finalSaveLst = new List<POR_SP_GetPartDetail_ForRequest_Result>();
            finalSaveLst = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, paraCurrentObjectName, conn);
            int rCount;
            rCount = finalSaveLst.Count;
            return rCount;
        }

        public List<POR_SP_GetPartDetail_ForRequest_Result> GridRowsTemplate(string paraSessionID, string paraCurrentObjectName, string paraUserID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<POR_SP_GetPartDetail_ForRequest_Result> finalSaveLst = new List<POR_SP_GetPartDetail_ForRequest_Result>();
            finalSaveLst = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, paraCurrentObjectName, conn);
            return finalSaveLst;
        }


        public long ChkTemplateTitle(string TemplateTitle, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from mRequestTemplateHead where TemplateTitle='" + TemplateTitle + "'	", conn);
            long result = ds.Tables[0].Rows.Count;
            return result;
        }
        #endregion

        #region GWC_Deliveries
        public VW_OrderDeliveryDetails GetOrderDeliveryDetails(long OrderID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            VW_OrderDeliveryDetails ODD = new VW_OrderDeliveryDetails();
            ODD = db.VW_OrderDeliveryDetails.Where(d => d.OrderID == OrderID).FirstOrDefault();
            db.VW_OrderDeliveryDetails.Detach(ODD);
            return ODD;
        }

        //public int GetDispatchedOrders(string SelectedOrder, string[] conn)
        //{
        //    DataSet ds = new DataSet();
        //    // ds = fillds("select * from torderhead where id in(" + SelectedOrder + ") and status=8 ", conn);
        //    ds = fillds("select * from torderhead where id in(" + SelectedOrder + ") and status in(2,3,4,6,8,9,10,21,22,26,28,30,31,33,35,36,37,38,39) ", conn);
        //    int cnt = ds.Tables[0].Rows.Count;
        //    return cnt;
        //}

        public int GetDispatchedOrders(string SelectedOrder, string[] conn)
        {
            int cnt = 0;
            using (SqlCommand cmd = new SqlCommand())
            {
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    using (DataSet ds = new DataSet())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "SP_GetDispatchedOrders";
                        cmd.Connection = svr.GetSqlConn(conn);
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@OrderId", SelectedOrder);
                        da.SelectCommand = cmd;
                        da.Fill(ds);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            int viporder = 0;
                            viporder = Convert.ToInt32(ds.Tables[0].Rows[0]["result"].ToString());
                            if (viporder > 0)
                            {
                                cnt = 3;
                            }
                            else { cnt = 2; }

                        }
                        else
                        {
                            cnt = Cntforreadyfordispatch(SelectedOrder, conn);
                        }
                    }
                }
            }
            // ds = fillds("select * from torderhead where id in(" + SelectedOrder + ") and status=8 ", conn);
            // ds = fillds(" select * from torderhead where id in(" + SelectedOrder + ") and status in(2,3,4,6,8,9,10,21,22,26,28,30,31,32,34,38,39,10036) ", conn);
            // int cnt = ds.Tables[0].Rows.Count;
            return cnt;
        }


        public int Cntforreadyfordispatch(string SelectedOrder, string[] conn)
        {
            int result = 0;
            using (SqlCommand cmd = new SqlCommand())
            {
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    using (DataSet ds = new DataSet())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "SP_Cntforreadyfordispatch";
                        cmd.Connection = svr.GetSqlConn(conn);
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@OrderId", SelectedOrder);
                        da.SelectCommand = cmd;
                        da.Fill(ds);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            result = 1;
                        }
                        else
                        {
                            result = 0;
                        }
                    }
                }
            }
            return result;
        }



        public int AssignSelectedDriver(long orderNo, long DriverID, string TruckDetails, long AssignBy, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_AssignDriverToOrder";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("DriverId", DriverID);
            cmd.Parameters.AddWithValue("TruckDetail", TruckDetails);
            cmd.Parameters.AddWithValue("AssignBy", AssignBy);
            cmd.Parameters.AddWithValue("OrderId", orderNo);
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
            CreateDeliveryLog(orderNo, DriverID, conn);

            EmailSendofRequestOutForDelivery(orderNo, conn);

            return 1;
        }

        private void CreateDeliveryLog(long orderNo, long driverID, string[] conn)
        {
            DataSet dscdate = new DataSet();
            dscdate = fillds("select * from torderhead where id=" + orderNo + "", conn);
            string cdate = dscdate.Tables[0].Rows[0]["Creationdate"].ToString();
            DataSet ds = new DataSet();
            DataSet dsupadte = new DataSet();
            ds = fillds("select * from tDeliveryLog where orderno=" + orderNo + "", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                dsupadte = fillds("update tDeliveryLog set OrderDownload=getdate(),OrderStatusId=25,UserId=" + driverID + "  where OrderNo=" + orderNo + " ", conn);
            }
            else
            {
                dsupadte = fillds("insert into tDeliveryLog (OrderNo, UserId, OrderStatusId,  OrderCreateDate, OrderDownload) values (" + orderNo + "," + driverID + ",25,'" + cdate + "',getdate())", conn);
            }

        }
        #endregion

        #region GWCVer2
        public DataSet GetDeptWisePaymentMethod(long selectedDept, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("SELECT  DPM.ID, DPM.DeptID, DPM.PMethodID, DPM.Sequence,PM.MethodName FROM mDeptPaymentMethod DPM left outer join mPaymentMethodMain PM on DPM.PMethodID=PM.ID where DPM.DeptID=" + selectedDept + "", conn);
            return ds;
        }

        public DataSet GetPaymentMethodFields(long SelpaymentMethod, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("SELECT  ID, PMethodID, FieldName, ControlType, Mandetory, Query, Sequence, '' as StatutoryValue FROM mPaymentMethodDetail where PMethodID=" + SelpaymentMethod + "", conn);
            return ds;
        }

        public List<mPaymentMethodDetail> GetPMFields(long SelpaymentMethod, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mPaymentMethodDetail> PMD = new List<mPaymentMethodDetail>();
            PMD = db.mPaymentMethodDetails.Where(p => p.PMethodID == SelpaymentMethod).ToList();
            return PMD;
        }

        public List<VW_DeptWisePaymentMethod> GEtDeptPaymentmethod(long selectedDept, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                List<VW_DeptWisePaymentMethod> PMD = new List<VW_DeptWisePaymentMethod>();
                PMD = db.VW_DeptWisePaymentMethod.Where(d => d.DeptID == selectedDept).ToList();
                return PMD;
            }
        }

        public List<mCostCenterMain> GetCostCenter(long DeptId, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                mTerritory cmpny = new mTerritory();
                cmpny = (from c in db.mTerritories
                         where c.ID == DeptId
                         select c).FirstOrDefault();
                long cmpnyID = long.Parse(cmpny.ParentID.ToString());
                List<mCostCenterMain> cost = new List<mCostCenterMain>();
                cost = db.mCostCenterMains.Where(t => t.CompanyID == cmpnyID).ToList();
                return cost;
            }

        }

        public decimal GetTotalFromTempData(string SessionID, string CurrentObjectName, string UserID, string[] conn)
        {
            decimal totPrice = 0;
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                List<POR_SP_GetPartDetail_ForRequest_Result> getRec = new List<POR_SP_GetPartDetail_ForRequest_Result>();
                getRec = GetExistingTempDataBySessionIDObjectName(SessionID, UserID, CurrentObjectName, conn);

                POR_SP_GetPartDetail_ForRequest_Result updateRec = new POR_SP_GetPartDetail_ForRequest_Result();
                totPrice = getRec.Sum(s => s.Total);
                return totPrice;
            }
        }

        public decimal GetTotalQTYFromTempData(string SessionID, string CurrentObjectName, string UserID, string[] conn)
        {
            decimal totPrice = 0;
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                List<POR_SP_GetPartDetail_ForRequest_Result> getRec = new List<POR_SP_GetPartDetail_ForRequest_Result>();
                getRec = GetExistingTempDataBySessionIDObjectName(SessionID, UserID, CurrentObjectName, conn);

                POR_SP_GetPartDetail_ForRequest_Result updateRec = new POR_SP_GetPartDetail_ForRequest_Result();
                totPrice = getRec.Sum(s => s.RequestQty);
                return totPrice;
            }
        }

        public long GetMaxDeliveryDaysofDept(long Dept, string[] conn)
        {
            long mdd = 0;
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                mTerritory d = new mTerritory();
                d = (from c in db.mTerritories
                     where c.ID == Dept
                     select c).FirstOrDefault();
                mdd = long.Parse(d.MaxDeliveryDays.ToString());
                return mdd;
            }
        }

        public string GetMandatoryFields(long pm, string[] conn)
        {
            string seq = "";
            DataSet ds = new DataSet();
            ds = fillds("select Sequence from mPaymentMethodDetail where PMethodID=" + pm + " and Mandetory>0", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i <= ds.Tables[0].Rows.Count - 1; i++)
                {
                    if (i == 0) { seq = ds.Tables[0].Rows[i]["Sequence"].ToString(); }
                    else
                    {
                        seq = seq + ',' + ds.Tables[0].Rows[i]["Sequence"].ToString();
                    }
                }
            }
            return seq;
        }

        public long GetStatutoryID(string PMLabel, long pmID, string[] conn)
        {
            long PMDID = 0;
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                mPaymentMethodDetail pmd = new mPaymentMethodDetail();
                pmd = (from d in db.mPaymentMethodDetails
                       where d.FieldName == PMLabel && d.PMethodID == pmID
                       select d).FirstOrDefault();
                PMDID = long.Parse(pmd.ID.ToString());
                return PMDID;
            }
        }

        public void AddIntotStatutory(tStatutoryDetail pmd, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                long ReqID = pmd.ReferenceID;
                //db.tStatutoryDetails.AddObject(pmd);
                //db.SaveChanges();

                using (SqlCommand c = new SqlCommand())
                {
                    c.CommandType = CommandType.StoredProcedure;
                    c.CommandText = "SP_InsertUpdate_update tStatutoryDetail";
                    c.Connection = svr.GetSqlConn(conn);
                    c.Parameters.Clear();
                    c.Parameters.AddWithValue("ObjectName", pmd.ObjectName);
                    c.Parameters.AddWithValue("ReferenceID", pmd.ReferenceID);
                    c.Parameters.AddWithValue("StatutoryID", pmd.StatutoryID);
                    c.Parameters.AddWithValue("StatutoryValue", pmd.StatutoryValue);
                    c.Parameters.AddWithValue("CreatedBy", pmd.CreatedBy);
                    c.Parameters.AddWithValue("Sequence", pmd.Sequence);
                    c.Parameters.AddWithValue("ApproverID", pmd.ApproverID);
                    c.ExecuteNonQuery();
                    c.Connection.Close();
                }

                tOrderHead oh = new tOrderHead();
                oh = db.tOrderHeads.Where(i => i.Id == ReqID).FirstOrDefault();
                long StatusID = Convert.ToInt64(oh.Status);
                if (StatusID == 1) { }
                else
                {
                    using (SqlCommand cmd3 = new SqlCommand())
                    {
                        cmd3.CommandType = CommandType.StoredProcedure;
                        cmd3.CommandText = "SP_PaymentMethodFOC";
                        cmd3.Connection = svr.GetSqlConn(conn);
                        cmd3.Parameters.Clear();
                        cmd3.Parameters.AddWithValue("OrderID", ReqID);
                        cmd3.ExecuteNonQuery();
                        cmd3.Connection.Close();
                    }
                }

                if (pmd.StatutoryID == 8)
                {
                    using (SqlCommand cmd2 = new SqlCommand())
                    {
                        SqlDataAdapter da2 = new SqlDataAdapter();
                        DataSet ds2 = new DataSet();
                        cmd2.CommandType = CommandType.Text;
                        cmd2.CommandText = " Select PMM.MethodName from tStatutoryDetail TS left outer join mPaymentMethodDetail MPD on TS.StatutoryID=MPD.ID left outer join mPaymentMethodMain PMM on MPD.PMethodID=PMM.ID where TS.ReferenceID=" + pmd.ReferenceID + "";
                        cmd2.Connection = svr.GetSqlConn(conn);
                        cmd2.Parameters.Clear();
                        da2.SelectCommand = cmd2;
                        da2.Fill(ds2, "tbl3");

                        string PMode = ds2.Tables[0].Rows[0]["MethodName"].ToString();
                        using (SqlCommand cmd1 = new SqlCommand())
                        {
                            SqlDataAdapter da = new SqlDataAdapter();
                            DataSet ds = new DataSet();
                            cmd1.CommandType = CommandType.Text;
                            cmd1.CommandText = "Insert into mPaymentDetail(OrderId,PaymentMode,CardNo)values(" + pmd.ReferenceID + ",'" + PMode + "'," + pmd.StatutoryValue + ")";
                            cmd1.Connection = svr.GetSqlConn(conn);
                            cmd1.Parameters.Clear();
                            da.SelectCommand = cmd1;
                            da.Fill(ds, "tbl1");
                        }
                    }
                }
            }
        }
        public void AddIntotStatutoryNEW(string ObjectName, long ReferenceID, long StatutoryID, string StatutoryValue, long CreatedBy, long Sequence, long ApproverID, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {

                using (SqlCommand c = new SqlCommand())
                {
                    c.CommandType = CommandType.StoredProcedure;
                    c.CommandText = "SP_InsertUpdateStatutoryDetail";
                    c.Connection = svr.GetSqlConn(conn);
                    c.Parameters.Clear();
                    c.Parameters.AddWithValue("ObjectName", ObjectName);
                    c.Parameters.AddWithValue("ReferenceID", ReferenceID);
                    c.Parameters.AddWithValue("StatutoryID", StatutoryID);
                    c.Parameters.AddWithValue("StatutoryValue", StatutoryValue);
                    c.Parameters.AddWithValue("CreatedBy", CreatedBy);
                    c.Parameters.AddWithValue("Sequence", Sequence);
                    c.Parameters.AddWithValue("ApproverID", ApproverID);
                    c.ExecuteNonQuery();
                    c.Connection.Close();
                }
                tOrderHead oh = new tOrderHead();
                oh = db.tOrderHeads.Where(i => i.Id == ReferenceID).FirstOrDefault();
                long StatusID = Convert.ToInt64(oh.Status);
                if (StatusID == 1) { }
                else
                {
                    using (SqlCommand cmd3 = new SqlCommand())
                    {
                        cmd3.CommandType = CommandType.StoredProcedure;
                        cmd3.CommandText = "SP_PaymentMethodFOC";
                        cmd3.Connection = svr.GetSqlConn(conn);
                        cmd3.Parameters.Clear();
                        cmd3.Parameters.AddWithValue("OrderID", ReferenceID);
                        cmd3.ExecuteNonQuery();
                        cmd3.Connection.Close();
                    }
                }
            }

        }

        public int GetDeptPriceChange(long deptID, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                mTerritory rec = new mTerritory();
                rec = db.mTerritories.Where(t => t.ID == deptID).FirstOrDefault();
                bool prch = Convert.ToBoolean(rec.PriceChange);

                int PC = 0;
                //DataSet ds = new DataSet();
                //ds = fillds("select PriceChange from mterritory where ID="+ deptID +"", conn);
                //string pricechng =ds.Tables[0].Rows[0]["PriceChange"].ToString();
                //if (pricechng == "" || pricechng == null) { PC = 0; }
                //else 
                if (prch == false) { PC = 0; }
                else if (prch == true) { PC = 1; }
                return PC;
            }
        }

        public string GetNewOrderNo(long StoreId, string[] conn)
        {
            DataSet ds = new DataSet();
            string NewONO = "";
            ds = fillds("select * from FN_GetOrderNo(" + StoreId + ")", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                NewONO = ds.Tables[0].Rows[0]["NewOrderNo"].ToString();
            }
            return NewONO;
        }

        public void UpdateStatutoryDetails(long RequestID, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("update tStatutoryDetail set ReferenceID=" + RequestID + " where ReferenceID=0", conn);
        }

        public int IsPriceChanged(int ProdID, decimal price, string[] conn)
        {
            int chng = 0; decimal pp = 0;
            DataSet ds = new DataSet();
            ds = fillds("select PrincipalPrice from mproduct where id= " + ProdID + "", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                pp = Convert.ToDecimal(ds.Tables[0].Rows[0]["PrincipalPrice"].ToString());
            }
            if (pp == price) { chng = 0; }
            else { chng = 1; }
            return chng;
        }

        public DataSet GetSelectedCostCenter(long RequestId, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select StatutoryValue from tStatutoryDetail	where ReferenceID=" + RequestId + "", conn);
            //long SelCostCenterID = long.Parse(ds.Tables[0].Rows[0]["StatutoryValue"].ToString());
            return ds;
        }

        public DataSet GetAddedAdditionalFields(long RequestID, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select SD.ID,SD.ObjectName,SD.ReferenceID,SD.StatutoryID,ST.Name FieldName,SD.StatutoryValue,SD.Active,SD.CreatedBy,SD.CreatedDate,SD.CompanyID from tStatutoryDetail	SD left outer join mStatutory	ST on SD.StatutoryID=ST.ID	where SD.ReferenceID=" + RequestID + "", conn);
            return ds;
        }

        public int GetPartAccessofUser(long requestID, long UserID, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from torderwiseaccess where OrderID=" + requestID + " and  UserApproverID=" + UserID + " and (PriceEdit=1 or SkuQtyEdit=1)", conn);
            int AccessYN = ds.Tables[0].Rows.Count;
            return AccessYN;
        }

        public DataSet GetProductOfOrder(long OrderID, int Sequence, string[] conn)
        {
            //    DataSet ds = new DataSet();
            //    ds = fillds("select OD.Id, OD.OrderHeadId,OD.SkuId,OD.OrderQty,OD.UOMID,OD.Sequence,OD.Prod_Name,OD.Prod_Description,OD.Prod_Code,OD.Price,OD.Total,OD.IsPriceChange,P.moq,PSD.AvailableBalance,PSD.ResurveQty from torderdetail OD left outer join mProduct P on OD.SkuId=P.Id left outer join tProductStockDetails PSD on OD.SkuId=PSD.ProdID  where OD.OrderHeadId=" + OrderID + " and OD.Sequence=" + Sequence + "", conn);
            //    return ds;

            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetProductOfOrder";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@OrderID", OrderID);
                cmd.Parameters.AddWithValue("@Sequence", Sequence);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }


        }

        public DataSet GetOrderProductAccess(long requestID, long UserID, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from torderwiseaccess where OrderID=" + requestID + " and  UserApproverID=" + UserID + " and (PriceEdit=1 or SkuQtyEdit=1)", conn);
            return ds;
        }

        public int IsPriceEditYN(long OrderID, long UserID, string[] conn)
        {
            int ChangePrice = 0;
            DataSet ds = new DataSet();
            ds = fillds("select * from torderwiseaccess where OrderID=" + OrderID + " and  UserApproverID=" + UserID + " and PriceEdit=1 ", conn);
            int cnt = ds.Tables[0].Rows.Count;
            if (cnt > 0) { ChangePrice = 1; } else { ChangePrice = 0; }
            return ChangePrice;
        }
        public int IsSkuChangeYN(long OrderID, long UserID, string[] conn)
        {
            int ChangeSku = 0;
            DataSet ds = new DataSet();
            ds = fillds("select * from torderwiseaccess where OrderID=" + OrderID + " and  UserApproverID=" + UserID + " and SkuQtyEdit=1 ", conn);
            int cnt = ds.Tables[0].Rows.Count;
            if (cnt > 0) { ChangeSku = 1; } else { ChangeSku = 0; }
            return ChangeSku;
        }

        public DataSet GetQtyofSelectedUOM(long SelectedProduct, long SelectedUOM, string[] conn)
        {
            DataSet ds = new DataSet();
            // ds = fillds("select Quantity from VW_SkuUOMDetails where SkuId=" + SelectedProduct + " and UOMID=" + SelectedUOM + "  and Quantity!=0 ", conn);
            ds = fillds("select Quantity from VW_SkuUOMDetails where SkuId=" + SelectedProduct + " and UOMID=" + SelectedUOM + "  and Quantity!=0 ", conn);
            return ds;
        }

        public int UpdateOrderQtyTotal(decimal OrderQty, decimal Price, decimal Total, long OrderID, int Sequence, long UserID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_Update_OrderQtyTotal";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("OrderQty", OrderQty);
            cmd.Parameters.AddWithValue("Price", Price);
            cmd.Parameters.AddWithValue("Total", Total);
            cmd.Parameters.AddWithValue("orderid", OrderID);
            cmd.Parameters.AddWithValue("Sequence", Sequence);
            cmd.Parameters.AddWithValue("UserID", UserID);
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();

            return 1;
        }

        protected void EmailSendofRequestOutForDelivery(long RequestID, string[] conn)
        {
            string MailSubject = "";
            string MailBody = "";
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));

            GWC_SP_GetRequestHeadByRequestIDs_Result Request = new GWC_SP_GetRequestHeadByRequestIDs_Result();
            Request = db.GWC_SP_GetRequestHeadByRequestIDs(RequestID.ToString()).FirstOrDefault();
            string ISProjectSite = "";
            ISProjectSite = ISProjectSiteDetailsNew(Convert.ToString(Request.parentid), conn);
            long Orderstatus = long.Parse(Request.Status.ToString());
            long DepartmentID = long.Parse(Request.SiteID.ToString());
            long Status = long.Parse(Request.Status.ToString());
            int SMail = 0;

            DataSet dsMailSubBody = new DataSet();
            dsMailSubBody = fillds("select * from mMessageEMailTemplates where ActivityID=(select Id from mDropdownValues where StatusID=25) and MessageID=(select Id from mDropdownValues where Value='Information') and  Active='Yes' and DepartmentID=" + DepartmentID + "", conn);
            string MsgTTL = dsMailSubBody.Tables[0].Rows[0]["MailSubject"].ToString();
            DataSet dsChkSendMail = new DataSet();
            dsChkSendMail = fillds("select MailStatus from tCorrespond where OrderHeadID=" + RequestID + " and MessageTitle='" + MsgTTL + "'", conn);
            if (dsChkSendMail.Tables[0].Rows.Count > 0)
            {
                SMail = Convert.ToInt16(dsChkSendMail.Tables[0].Rows[0]["MailStatus"].ToString());
                if (SMail == 1) { }
                else
                {
                    DataSet dsOrderApprovers = new DataSet();
                    dsOrderApprovers = fillds("select UserApproverID from torderwiseaccess where OrderID=" + RequestID + "", conn);
                    int dscnt = dsOrderApprovers.Tables[0].Rows.Count;
                    if (dscnt > 0)
                    {
                        for (int i = 0; i <= dscnt - 1; i++)
                        {
                            MailSubject = dsMailSubBody.Tables[0].Rows[0]["MailSubject"].ToString() + ", Order No # " + Request.OrderNo + "";

                            MailBody = "Dear " + GetUserNameByUserID(Convert.ToInt64(dsOrderApprovers.Tables[0].Rows[i]["UserApproverID"].ToString()), conn) + ",";
                            MailBody = dsMailSubBody.Tables[0].Rows[0]["MailBody"].ToString();
                            if (ISProjectSite == "Yes")
                            {
                                MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetailVodafoneTechnical(Request, conn);
                            }
                            else
                            {
                                MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                            }
                            MailBody = MailBody + "<br/><br/>" + EmailGetAddressDetails(Request, conn);
                            MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetail(RequestID, conn);


                            SendMail(MailBody + MailGetFooter(), MailSubject, EmailGetEmailIDsByUserID(Convert.ToInt64(dsOrderApprovers.Tables[0].Rows[i]["UserApproverID"].ToString()), conn));
                        }
                    }
                    long TemplateID = long.Parse(dsMailSubBody.Tables[0].Rows[0]["ID"].ToString());
                    SaveCorrespondsData(RequestID, MailSubject, MailBody, DepartmentID, Status, conn);
                    AdditionalDistribution(RequestID, TemplateID, conn);
                }

            }
            else
            {
                DataSet dsOrderApprovers = new DataSet();
                dsOrderApprovers = fillds("select UserApproverID from torderwiseaccess where OrderID=" + RequestID + "", conn);
                int dscnt = dsOrderApprovers.Tables[0].Rows.Count;
                if (dscnt > 0)
                {
                    for (int i = 0; i <= dscnt - 1; i++)
                    {
                        MailSubject = dsMailSubBody.Tables[0].Rows[0]["MailSubject"].ToString() + ", Order No # " + Request.OrderNo + "";

                        MailBody = "Dear " + GetUserNameByUserID(Convert.ToInt64(dsOrderApprovers.Tables[0].Rows[i]["UserApproverID"].ToString()), conn) + ",";
                        MailBody = dsMailSubBody.Tables[0].Rows[0]["MailBody"].ToString();

                        if (ISProjectSite == "Yes")
                        {
                            MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetailVodafoneTechnical(Request, conn);
                        }
                        else
                        {
                            MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                        }
                        MailBody = MailBody + "<br/><br/>" + EmailGetAddressDetails(Request, conn);
                        MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetail(RequestID, conn);

                        SendMail(MailBody + MailGetFooter(), MailSubject, EmailGetEmailIDsByUserID(Convert.ToInt64(dsOrderApprovers.Tables[0].Rows[i]["UserApproverID"].ToString()), conn));
                    }
                }
                long TemplateID = long.Parse(dsMailSubBody.Tables[0].Rows[0]["ID"].ToString());
                SaveCorrespondsData(RequestID, MailSubject, MailBody, DepartmentID, Status, conn);
                AdditionalDistribution(RequestID, TemplateID, conn);
            }

            //send mail to end customer if order id ecommerce
            DataSet dsChkOredrIsEcomOrNot = new DataSet();
            dsChkOredrIsEcomOrNot = fillds("select * from tOrderHead where orderType in('PreOrder','NormalOrder') and id=" + RequestID + " ", conn);
            if (dsChkOredrIsEcomOrNot.Tables[0].Rows.Count > 0)
            {
                SendEmailToCustomerForOutForDelivery(RequestID, conn);
            }

        }


        //semd email to customer for out for delivery
        private void SendEmailToCustomerForOutForDelivery(long RequestID, string[] conn)
        {
            string MailSubject;
            string MailBody = "";
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            GWC_SP_GetRequestHeadByRequestIDs_Result EcomMsg = new GWC_SP_GetRequestHeadByRequestIDs_Result();
            EcomMsg = db.GWC_SP_GetRequestHeadByRequestIDs(RequestID.ToString()).FirstOrDefault();

            DataSet dsstatus = new DataSet();
            dsstatus = fillds(" select id from mstatus where status='Out For Delivery' ", conn);
            long statusNew = long.Parse(dsstatus.Tables[0].Rows[0]["id"].ToString());

            DataSet dsMailSubBody1 = new DataSet();
            dsMailSubBody1 = fillds("select * from torderhead where id=" + RequestID + "", conn);
            string ordernumbr = dsMailSubBody1.Tables[0].Rows[0]["Ordernumber"].ToString();


            string Lanuage = "", CustomerEmail = "", CustName = "", Address1 = "", Address2 = "", Paymentmethod = "", DeliveryType = "";
            long DepartmentID = long.Parse(EcomMsg.SiteID.ToString());
            long Status = long.Parse(EcomMsg.Status.ToString());
            string OrderNo = Convert.ToString(EcomMsg.OrderNo);
            DataSet dsLanuage = new DataSet();
            //  dsLanuage = fillds(" select PreferredLanguage,Email,CustomerFirstName,CustomerLastName,BuildingName,Streetname,City,Country,paymentmethod   from tECommHead where OrderID=" + RequestID + " ", conn);
            dsLanuage = fillds(" select *   from tECommHead where OrderID=" + RequestID + " ", conn);
            Lanuage = dsLanuage.Tables[0].Rows[0]["PreferredLanguage"].ToString();
            CustomerEmail = dsLanuage.Tables[0].Rows[0]["Email"].ToString();
            CustName = dsLanuage.Tables[0].Rows[0]["CustomerFirstName"].ToString() + " " + dsLanuage.Tables[0].Rows[0]["CustomerLastName"].ToString();
            Address1 = dsLanuage.Tables[0].Rows[0]["BuildingName2"].ToString() + ", " + dsLanuage.Tables[0].Rows[0]["Streetname2"].ToString();
            Address2 = dsLanuage.Tables[0].Rows[0]["City2"].ToString() + ", " + dsLanuage.Tables[0].Rows[0]["Country2"].ToString();
            Paymentmethod = dsLanuage.Tables[0].Rows[0]["paymentmethod"].ToString();
            DataSet dsMailSubBody = new DataSet();
            dsMailSubBody = fillds("select * from mMessageEMailTemplates where ActivityID=(select Id from mDropdownValues where StatusID=25) and MessageID=(select Id from mDropdownValues where Value='Information') and Active='Yes' and DepartmentID=" + DepartmentID + "", conn);
            if ((Paymentmethod == "Card" && Lanuage == "English") || Lanuage == "")
            {
                MailSubject = dsMailSubBody.Tables[0].Rows[4]["MailSubject"].ToString() + ", Order No # " + ordernumbr + "";
            }
            else if (Paymentmethod == "Card" && Lanuage == "Arabic")
            {
                MailSubject = dsMailSubBody.Tables[0].Rows[3]["MailSubject"].ToString() + ", Order No # " + ordernumbr + "";
            }
            else if ((Paymentmethod == "COD" && Lanuage == "English") || Lanuage == "")
            {
                MailSubject = dsMailSubBody.Tables[0].Rows[2]["MailSubject"].ToString() + ", Order No # " + ordernumbr + "";
            }
            else if (Paymentmethod == "COD" && Lanuage == "Arabic")
            {
                MailSubject = dsMailSubBody.Tables[0].Rows[1]["MailSubject"].ToString() + ", Order No # " + ordernumbr + "";
            }
            else
            {
                MailSubject = dsMailSubBody.Tables[0].Rows[0]["MailSubject"].ToString() + ", Order No # " + ordernumbr + "";
            }
            DataSet dsAllUsers = new DataSet();
            if (Lanuage == "English" || Lanuage == "" || Lanuage == null)
            {
                MailBody = MailBody + "<br/>" + EMailGetRequestDetailOutforDeliveryEnglish(EcomMsg, CustName, Address1, Address2, conn, Paymentmethod);//OutforDelivery
            }
            else
            {
                MailBody = MailBody + "<br/>" + EMailGetRequestDetailOutforDeliveryArebic(EcomMsg, CustName, Address1, Address2, conn, Paymentmethod);//OutforDelivery
            }

            //SendMailForCustomer(MailBody, MailSubject, CustomerEmail, DepartmentID, conn);  old
            // SaveCorrespondsData(RequestID, MailSubject, MailBody, DepartmentID, Status, conn);  old
            InsertCorrespondsData(RequestID, MailSubject, MailBody, DepartmentID, statusNew, conn);   //new
        }

        //out for deleivery
        protected string EMailGetRequestDetailOutforDeliveryEnglish(GWC_SP_GetRequestHeadByRequestIDs_Result Request, string CustName, string Address1, string Address2, string[] conn, string PaymentMethod)
        {
            DataSet dsMailSubBody = new DataSet();
            dsMailSubBody = fillds("select * from torderhead where id=" + Request.ID + "", conn);
            string ordernumbr = dsMailSubBody.Tables[0].Rows[0]["Ordernumber"].ToString();
            string messageBody = "";
            //#7f7f95
            messageBody = messageBody + " <html> <body> <table  width='100%'> <tr> <td  style='text-align: right'> <img src ='cid:companylogo'  width='70' height='70'> </td> </tr> </table> </body></html><br/><font face='Verdana' style='font-size:11;' color=#515161>Thank you for your order. This is your e-receipt. Please keep a copy for your records.</font><br/><br/>";
            messageBody = messageBody + " <font face='Verdana' style='font-size:11;' color=#515161><b>Your Order number  <font  color='red'> (" + ordernumbr + ")</font><font face='Verdana' style='font-size:11;' color=#515161>  is</font><font face='Verdana' style='font-size:11;' color='red'> out for delivery and will be with you today</font></font>! </b><br/><br/>  <font face='Verdana' style='font-size:14;' color=#5d5d6f><b> Delivery information: </b></font><br/> <br/> ";
            messageBody = messageBody + " <font face='Verdana' style='font-size:11;'  color=#515161>The package will be under the name: (" + CustName + ") Your order will be available at the following  </font><br/><font face='Verdana' style='font-size:11;'  color=#515161>location: </font><br/> <br/><font face='Verdana' style='font-size:11;'  color=#515161>Address line: 1 " + Address1 + " </font><br/><font face='Verdana' style='font-size:11;'  color=#515161>Address line: 2," + Address2 + " </font><br/><br/>";
            //insert link
            messageBody = messageBody + "<font face='Verdana' style='font-size:11;' color=#515161>For store timings, please visit <a href='http://vf.qa/stores'>vf.qa/stores</a></font><br/><br/>";
            if (PaymentMethod == "COD")
            {
                messageBody = messageBody + "<font face='Verdana' style='font-size:11;' color=#515161>To collect your order, you must present the ID used to place the order.</font><br/><br/>";
            }
            else
            {
                messageBody = messageBody + "<font face='Verdana' style='font-size:11;' color=#515161>To collect your order, you must present the ID and credit card used to place the order.</font><br/><br/>";
            }
            messageBody = messageBody + "<font face='Verdana' style='font-size:11;' color=#515161>If you cannot be present to receive your order and would like to send a representative to collect it on</font><br/>";
            messageBody = messageBody + "<font face='Verdana' style='font-size:11;' color=#515161>your behalf, then please complete the Letter of Authorisation found  <a href='http://vfq-preprod.pjmedia.co.uk/files/pdf/authorisationletter.pdf'>here</a>.</font><br/><br/>";
            if (PaymentMethod == "COD")
            {
                messageBody = messageBody + " <font face='Verdana' style='font-size:14;' color=#5d5d6f><b>Pay for the order: </b> </font><br/> <font face='Verdana' style='font-size:11;'>You have selected to pay Cash on delivery for your order. You will have to pay the total amount at<font face='Verdana' style='font-size:11;'><br/><font face='Verdana' style='font-size:11;'>collection.</font> <br/> <br/>";
            }
            messageBody = messageBody + "<font face='Verdana' style='font-size:14;' color=#5d5d6f><b>Change or Cancel: </b> </font><br/><font face='Verdana' style='font-size:11;' color=#515161>If you'd like to change or cancel your order, please contact us through any of the below channels:</font>  <br/>";
            messageBody = messageBody + "<font face='Verdana' style='font-size:11;'  color=#515161>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;•&nbsp;&nbsp;Chat with us from My Vodafone app </font><br/> <font face='Verdana' style='font-size:11;' color=#515161>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;•&nbsp;&nbsp;Call us on 111</font><br/>";
            messageBody = messageBody + "<font face='Verdana' style='font-size:11;'  color=#515161>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;•&nbsp;&nbsp;Visit our stores</font><br/><font face='Verdana' style='font-size:11;'  color=#515161>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;•&nbsp;&nbsp;E-mail us at care.qa@vodafone.com</font><br/><br/>";
            messageBody = messageBody + "<font face='Verdana' style='font-size:11;'  color=#515161>In most cases your package is prepared for shipment immediately and cannot be changed, but we will </font><br/><font face='Verdana' style='font-size:11;' color=#515161>try to assist you whenever possible. You can find information on all of Vodafone’s products and services</font><br/>";
            messageBody = messageBody + "<font face='Verdana' style='font-size:11;' color=#515161>at <a href='http://www.vodafone.qa/'>www.vodafone.qa</a>  </font><br/><br/><font face='Verdana' style='font-size:11;' color=#515161>Kind regards,</font><br/><br/>";
            messageBody = messageBody + "<font face='Verdana' style='font-size:11;'  color=#515161>The Vodafone Qatar Team</font><br/><br/><a href='https://www.vodafone.qa/en/my-vodafone-app'>  <img src ='cid:comicon'  width = '90' height = '90'></a> <br/>";
            messageBody = messageBody + "<font face='Arial' style='color: rgb(120, 120, 120);font-size:20;'>Download the My Vodafone App</font><br/> <a href='https://play.google.com/store/apps/details?id=qa.vodafone.myvodafone'>  <img src ='cid:googleplay'  width = '150' height = '50'></a>  ";
            messageBody = messageBody + " <a href='https://itunes.apple.com/us/app/my-vodafone/id1076290326?ls=1&mt=8'><img src ='cid:appstore'  width = '150' height = '50'></a>";

            messageBody = messageBody + "<br/><br/><br/><font face='Verdana' style='font-size:11;'  color=#515161>This email was sent from a no-reply email address at Vodafone Qatar.</font>";
            //            messageBody = messageBody + " <html> <body> <table  width='100%'> <tr> <td  style='text-align: right'> <img src ='cid:companylogo'  width='70' height='70'> </td> </tr> </table> </body></html><br/><font face='Verdana' size='1px'>Thank you for your order. This is your e-receipt. Please keep a copy for your records.</font><br/></br>";
            //            messageBody = messageBody + " <font size='1px' face='Verdana'><b>Your Order number  <font  face='Verdana' color='red'> (" + ordernumbr + ")</font><font face='Verdana'>  is</font><font face='Verdana' color='red'> out for delivery and will be with you today</font></font>! </b></br></br>  <font face='Verdana' size='2px' ><b> Delivery information: </b></font></br> </br> ";
            //            messageBody = messageBody + " <font size='1px' face='Verdana'>The package will be under the name: (" + CustName + ") Your order will be available at the following  </font><br/><font size='1px' face='Verdana'>location: </font><br/> <br/><font size='1px' face='Verdana'>Address line: 1 " + Address1 + " </font><br/><font size='1px' face='Verdana'>Address line: 2," + Address2 + " </font><br/></br>";
            ////insert link
            //            messageBody = messageBody + "<font size='1px' face='Verdana'>For store timings, please visit [INSERT LINK TO TIMINGS]</font><br/></br>";

            //            messageBody = messageBody + "<font size='1px' face='Verdana'>To collect your order, you must present the ID used to place the order.</font><br/></br><font size='1px' face='Verdana'>If you cannot be present to receive your order and would like to send a representative to collect it on</font><br/>";
            //            messageBody = messageBody + "<font size='1px'  face='Verdana' color='black'>your behalf, then please complete the Letter of Authorisation found  <a href='http://vfq-preprod.pjmedia.co.uk/files/pdf/authorisationletter.pdf'>here</a>.</font></br></br><font size='2px' face='Verdana'><b>Pay for the order: </b> </font></br> <font face='Verdana' size='1px'>You have selected to pay Cash on delivery for your order. You will have to pay the total amount at<font></br>";
            //            messageBody = messageBody + "<font face='Verdana'>collection.</font> <br/> <br/><font size='2px' face='Verdana'><b>Change or Cancel: </b> </font></br><font size='1px' face='Verdana'>If you'd like to change or cancel your order, please contact us through any of the below channels:</font>  <br/>";
            //            messageBody = messageBody + "<font size='1px' face='Verdana'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;•&nbsp;&nbsp;Chat with us from My Vodafone app </font><br/> <font size='1px' face='Verdana'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;•&nbsp;&nbsp;Call us on 111</font><br/>";
            //            messageBody = messageBody + "<font size='1px' face='Verdana'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;•&nbsp;&nbsp;Visit our stores</font><br/><font size='1px' face='Verdana'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;•&nbsp;&nbsp;E-mail us at care.qa@vodafone.com</font><br/><br/>";
            //            messageBody = messageBody + "<font size='1px' face='Verdana'>In most cases your package is prepared for shipment immediately and cannot be changed, but we will </font><br/><font size='1px' face='Verdana'>try to assist you whenever possible. You can find information on all of Vodafone’s products and services</font><br/>";
            //            messageBody = messageBody + "<font size='1px' face='Verdana'>at <a href='http://www.vodafone.qa/'>www.vodafone.qa</a>  </font><br/></br><font size='1px' face='Verdana'>Kind regards,</font><br/></br>";
            //            messageBody = messageBody + "<font size='1px' face='Verdana'>The Vodafone Qatar Team</font><br/><br/><a href='https://www.vodafone.qa/en/my-vodafone-app'>  <img src ='cid:comicon'  width = '70' height = '70'></a> </br>";
            //            messageBody = messageBody + "<font size='4px'  face='Arial'  style='color: rgb(120, 120, 120); '>Download the My Vodafone App</font><br/> <a href='https://play.google.com/store/apps/details?id=qa.vodafone.myvodafone'>  <img src ='cid:googleplay'  width = '150' height = '70'></a>  ";
            //            messageBody = messageBody + " <a href='https://itunes.apple.com/us/app/my-vodafone/id1076290326?ls=1&mt=8'><img src = 'cid:appstore'  width = '150' height = '70'></a>";
            //           // messageBody = messageBody + "";         



            //messageBody = messageBody + "<br/><br/>   <html> <body> <table  width='100%'> <tr> <td  style='text-align: right'> <img src ='cid:companylogo'  width='70' height='70'> </td> </tr> </table> </body></html><br/>";
            //messageBody = messageBody + "<font>Thank you for your order. This is your e - receipt.Please keep a copy for your records.</font><br/></br>";
            //messageBody = messageBody + "<font><b>Your Order number </b> (" + ordernumbr + ") is out for delivery and will be with you today! </font> </br>  </br>";
            //messageBody = messageBody + "<font><h3><b> Delivery information: </b></h3> </font>  </br><font>The package will be under the name: (" + CustName + ") Your order will be available at the following  </font><br/><font>location: </font><br/> <br/>";
            //messageBody = messageBody + "<font>Address line: " + Address1 + " </font><br/><font>Address line: " + Address2 + " </font><br/></br><font>For store timings, please visit [INSERT LINK TO TIMINGS]</font><br/></br>";
            //messageBody = messageBody + "<font>To collect your order, you must present the ID used to place the order.</font><br/></br><font>If you cannot be present to receive your order and would like to send a representative to collect it on</font><br/>";
            //messageBody = messageBody + "<font>your behalf, then please complete the Letter of Authorisation found <a href='http://vfq-preprod.pjmedia.co.uk/files/pdf/authorisationletter.pdf'>here </a>.";


            //if (PaymentMethod == "COD")
            //{
            //    messageBody = messageBody + "<font><h3><b>Pay for the order: </b></h3> </font></br> <font>You have selected to pay Cash on delivery for your order. You will have to pay the total amount at collection.<font></br>";
            //}

            //messageBody = messageBody + " <br/> <br/><font><h3><b>Change or Cancel: </b></h3> </font></br><font>If you'd like to change or cancel your order, please contact us through any of the below channels:</font>  <br/>";
            //messageBody = messageBody + "<font>•  Chat with us from My Vodafone app </font><br/><font>•  Call us on 111 </font><br/><font>•  Visit our stores  </font><br/>";
            //messageBody = messageBody + "<font>•  E-mail us at care.qa@vodafone.com </font><br/></br><font>In most cases your package is prepared for shipment immediately and cannot be changed, but we will </font><br/>";
            //messageBody = messageBody + "<font>try to assist you whenever possible. You can find information on all of Vodafone’s products and services  </font><br/>";
            //messageBody = messageBody + "<font>at www.vodafone.qa </font><br/></br><font>Kind regards,</font><br/></br><font>The Vodafone Qatar Team</font><br/><a href='https://www.vodafone.qa/en/my-vodafone-app'>  <img src ='cid:comicon'  width = '70' height = '70'></a> </br>";
            //messageBody = messageBody + "<font style='color: rgb(120, 120, 120); '><h3>Download the My Vodafone App</h3></font> <a href='https://play.google.com/store/apps/details?id=qa.vodafone.myvodafone'>  <img src ='cid:googleplay'  width = '150' height = '70'></a>";
            //messageBody = messageBody + "  <a href='https://itunes.apple.com/us/app/my-vodafone/id1076290326?ls=1&mt=8'><img src = 'cid:appstore'  width = '150' height = '70'></a> ";

            return messageBody;
        }

        protected string EMailGetRequestDetailOutforDeliveryArebic(GWC_SP_GetRequestHeadByRequestIDs_Result Request, string CustName, string Address1, string Address2, string[] conn, string PaymentMethod)
        {
            DataSet dsMailSubBody = new DataSet();
            dsMailSubBody = fillds("select * from torderhead where id=" + Request.ID + "", conn);
            string ordernumbr = dsMailSubBody.Tables[0].Rows[0]["Ordernumber"].ToString();

            string messageBody = "";
            messageBody = messageBody + "<html> <body> <table  width='100%'> <tr> <td  style='text-align: right'> <img src ='cid:companylogo'  width='70' height='70'> </td> </tr> </table> </body></html><br/><table width='100%'>";
            messageBody = messageBody + "<tr><td style='text-align: right'><font face='Simplified Arabic' style='font-size:11;' color=#515161>.شكراً لك على طلبك. هذا هو الإيصال الإلكتروني. يرجى الاحتفاظ بنسخة منها معك";
            messageBody = messageBody + "</font></td></tr><tr><td></td></tr><tr><td></td></tr><tr><td></td></tr><tr><td style='text-align: right'><font face='Times New Roman' style='font-size:11;' color=#515161><b>.لقد تم إرسال طلبك رقم (" + ordernumbr + ") وستحصل عليه اليوم.</b></font></td></tr>";
            messageBody = messageBody + "<tr><td></td></tr> <tr><td></td></tr><tr><td></td></tr><tr><td></td></tr><tr><td></td></tr><tr><td></td></tr><tr><td style='text-align: right'><font face='Times New Roman' style='font-size:14;' color=#515161><b> : معلومات التوصيل </b></font></td></tr><tr><td></td></tr><tr><td></td></tr><tr><td></td></tr><tr><td style='text-align: right'><font face='Simplified Arabic' style='font-size:11;' color=#515161> (" + CustName + ") ";
            messageBody = messageBody + "سيكون الطلب مسجلاً باسم: </font></td></tr> <tr><td></td></tr><tr><td></td></tr><tr><td></td></tr> ";
            messageBody = messageBody + "<tr><td style='text-align: right'><font face='Simplified Arabic' style='font-size:11;' color=#515161> : سيتم توصيل الطلب إلى العنوان التالي";
            messageBody = messageBody + "</font></td></tr> <tr><td></td></tr><tr><td></td></tr><tr><td></td></tr><tr><td style='text-align: right'>  <font face='Simplified Arabic' style='font-size:11;' color=#515161>" + Address1 + " 1 : العنوان";
            messageBody = messageBody + "</font></td></tr><tr><td></td></tr><tr><td></td></tr><tr><td></td></tr> <tr><td style='text-align: right'><font face='Simplified Arabic' style='font-size:11;' color=#515161>" + Address2 + " 2 : العنوان";
            messageBody = messageBody + "</font></td></tr> <tr><td></td></tr><tr><td></td></tr><tr><td></td></tr> <tr><td style='text-align: right'><font face='Simplified Arabic' style='font-size:11;' color=#515161><a href='http://vf.qa/stores'>vf.qa/stores</a> لمعرفة أوقات عمل المتاجر، يرجى زيارة ";
            messageBody = messageBody + "</font></td></tr>";
            if (PaymentMethod == "COD")
            {
                messageBody = messageBody + " <tr><td></td></tr><tr><td></td></tr><tr><td></td></tr> <tr><td style='text-align: right'><font face='Simplified Arabic' style='font-size:11;' color=#515161>.لاستلام طلبك، يجب عليك ابراز البطاقة الشخصية المستخدمة عند تقديم طلب الشراء" + "</font></td></tr>";
            }
            else
            {
                messageBody = messageBody + " <tr><td></td></tr><tr><td></td></tr><tr><td></td></tr> <tr><td style='text-align: right'><font face='Simplified Arabic' style='font-size:11;' color=#515161>.استلام طلبك، يجب عليك ابراز البطاقة الشخصية والبطاقة الائتمانية المستخدمة عند تقديم طلب الشراء" + "</font></td></tr>";
            }
            messageBody = messageBody + " <tr><td></td></tr><tr><td></td></tr><tr><td></td></tr><tr><td style='text-align: right'><font face='Simplified Arabic' style='font-size:11;' color=#515161>.<a href='http://vfq-preprod.pjmedia.co.uk/files/pdf/authorisationletter.pdf'>هنا</a>  إن لم يكن بإمكانك الحضور شخصياً لاستلام الطلب وترغب بإرسال شخصاً لاستلام الطلب بالنيابة عنك، يرجى تعبئة نموذج التوكيل الموجود ";
            messageBody = messageBody + "</font></td></tr> <tr><td></td></tr><tr><td></td></tr><tr><td></td></tr>";

            if (PaymentMethod == "COD")
            {
                messageBody = messageBody + " <tr><td style='text-align: right'><font face='Simplified Arabic' style='font-size:14;' color=#515161><b> دفع ثمن الطلب";
                messageBody = messageBody + "</b></font></td></tr> <tr><td style='text-align: right'><font face='Simplified Arabic' style='font-size:14;' color=#515161>. لقد اخترت أن تدفع ثمن طلب الشراء نقداً عند الاستلام. يجب عليك أن تدفع المبلغ بالكامل عند استلام الطلب";
                messageBody = messageBody + "</font></td></tr> <tr><td></td></tr><tr><td></td></tr><tr><td></td></tr>";
            }

            messageBody = messageBody + " <tr><td style='text-align: right'><font face='Simplified Arabic' style='font-size:14;' color=#515161><b>: لتغيير الطلب أو إلغاءه";
            messageBody = messageBody + "</b></font></td></tr><tr><td style='text-align: right'><font face='Simplified Arabic' style='font-size:14;' color=#515161>:إن كنت ترغب بتغيير الطلب أو إلغاءه، يرجى التواصل معنا عبر أي من وسائل الاتصال التالية";
            messageBody = messageBody + "</font></td></tr> <tr><td style='text-align: right'><font face='Simplified Arabic' style='font-size:16;' color=#515161>.<b>My Vodafone</b> المحادثة المباشرة معنا عبر تطبيق ";
            messageBody = messageBody + "&nbsp;&nbsp;&nbsp;&nbsp;•&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</font></td></tr> <tr><td style='text-align: right'><font face='Simplified Arabic' style='font-size:16;' color=#515161><b>111</b> 	الاتصال بنا عبر  ";
            messageBody = messageBody + "&nbsp;&nbsp;&nbsp;&nbsp;•&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</font></td></tr> <tr><td style='text-align: right'><font face='Simplified Arabic' style='font-size:16;' color=#515161>	زيارة أي من متاجرنا &nbsp;&nbsp;&nbsp;&nbsp;•&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</font></td></tr>";
            messageBody = messageBody + "<tr><td style='text-align: right'><font face='Simplified Arabic' style='font-size:16;' color=#515161><a href='mailto:care.qa@vodafone.com'><b>care.qa@vodafone.com</b></a>		رسالة بريد إلكتروني إلى  &nbsp;&nbsp;&nbsp;&nbsp;•&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</font></td></tr>";
            messageBody = messageBody + "<tr><td></td></tr><tr><td></td></tr><tr><td></td></tr><tr><td></td></tr><tr><td></td></tr><tr><td></td></tr> <tr><td style='text-align: right'><font face='Simplified Arabic' style='font-size:16;' color=#515161>. يتم في الغالب تجهيز طلبك وإعداده للشحن مباشرة ولا يمكن تغيير الطلب، ولكننا سنحاول مساعدتك قدر المستطاع";
            messageBody = messageBody + "</font></td></tr> <tr><td style='text-align: right'><font face='Simplified Arabic' style='font-size:16;' color=#515161> <a href='http://www.vodafone.qa/'>www.vodafone.qa</a>";
            messageBody = messageBody + "للحصول على المزيد من المعلومات حول جميع منتجات وخدمات فودافون يرجى زيارة  </font></td></tr><tr><td></td></tr><tr><td></td></tr><tr><td></td></tr>";
            messageBody = messageBody + "<tr><td style='text-align: right'><font face='Simplified Arabic' style='font-size:16;' color=#515161> مع أطيب التحيات </font></td></tr><tr><td></td></tr><tr><td></td></tr><tr><td></td></tr>";
            messageBody = messageBody + "<tr><td style='text-align: right'><font face='Simplified Arabic' style='font-size:16;' color=#515161> فريق فودافون قطر";
            messageBody = messageBody + "</font></td></tr> <tr> <td style='text-align: left'><a href='https://www.vodafone.qa/en/my-vodafone-app'>  <img src = 'cid:comicon'  width = '90' height = '90'></a></td> </tr>";
            messageBody = messageBody + "<tr> <td style='text-align: left'><font face='Arial' style='font-size:20;' color=#515161>My Vodafone  حمل تطبيق    </font> </td> </tr> <tr> <td style='text-align: left'> <a href='https://play.google.com/store/apps/details?id=qa.vodafone.myvodafone'>  <img src = 'cid:googleplay'  width = '150' height = '50'></a>";
            messageBody = messageBody + "<a href='https://itunes.apple.com/us/app/my-vodafone/id1076290326?ls=1&mt=8'><img src = 'cid:appstore'  width = '150' height = '50'></a>  </td> </tr> </table>   </body></html> </table>";

            messageBody = messageBody + "<br/><br/><br/><table  width='100%'><tr><td style='text-align: right'> <font face='Simplified Arabic' style='font-size:16;' color=#515161> تم إرسال هذه الرسالة بواسطة عنوان بريد إلكتروني غير قابل للرد من فودافون قطر. </font></td></tr></table> ";

            return messageBody;
        }


        public List<tOrderHead> GetOrderHeadByOrderIDQTYTotal(long OrderID, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                List<tOrderHead> PartReq = new List<tOrderHead>();
                PartReq = db.tOrderHeads.Where(r => r.Id == OrderID).ToList();
                //db.tOrderHeads.Detach(PartReq);
                return PartReq;
            }
        }

        public int CancelSelectedOrder(long SelectedOrder, int reasoncode, long UserID, string[] conn)
        {
            int result = 0;
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            tOrderHead PartReq = new tOrderHead();
            PartReq = db.tOrderHeads.Where(c => c.Id == SelectedOrder && c.RequestBy == UserID).FirstOrDefault();
            if (PartReq != null)
            {
                long OrderStatus = long.Parse(PartReq.Status.ToString());
                if (OrderStatus == 2 || OrderStatus == 21 || OrderStatus == 22)
                {
                    //DataSet ds = new DataSet();
                    //DataSet ds1 = new DataSet();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_CancelOrderWithReasonCode";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@OrderId", SelectedOrder);
                    cmd.Parameters.AddWithValue("@ReasonCode", reasoncode);
                    cmd.ExecuteNonQuery();

                    //  ds = fillds("update torderhead set Status=28, ReasonCode=" + reasoncode + " where id=" + SelectedOrder + "", conn); // Update Order Status
                    //  ds1 = fillds("delete from tApprovalTrans where orderid=" + SelectedOrder + " and status !=3", conn); // tApprovalTrans order delete record

                    UpdateAvailableBalanceAfterRequestReject(SelectedOrder, conn); // Update Stock
                    string yesno = ChkOrderSerialedCompany(SelectedOrder, conn);
                    if (yesno == "Yes")
                    {
                        using (SqlCommand cmd1 = new SqlCommand())
                        {
                            SqlDataAdapter da = new SqlDataAdapter();
                            DataSet ds = new DataSet();
                            cmd1.CommandType = CommandType.StoredProcedure;
                            cmd1.CommandText = "SP_DeleteResurveSerialnumber";
                            cmd1.Connection = svr.GetSqlConn(conn);
                            cmd1.Parameters.Clear();
                            cmd1.Parameters.AddWithValue("@OrderId", SelectedOrder);
                            cmd1.Parameters.AddWithValue("@Userid", UserID);
                            cmd1.ExecuteNonQuery();
                        }
                    }
                    SendEmailWhenOrderCancelByRequestor(SelectedOrder, reasoncode, conn);
                    //Email 
                    result = 1;
                }
                else
                { result = 0; }
            }
            else
            {
                result = 0;
            }
            return result;
        }

        public void SendEmailWhenOrderCancelByRequestor(long RequestID, int reasoncode, string[] conn)
        {
            string MailSubject;
            string MailBody = "";
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            GWC_SP_GetRequestHeadByRequestIDs_Result Request = new GWC_SP_GetRequestHeadByRequestIDs_Result();
            Request = db.GWC_SP_GetRequestHeadByRequestIDs(RequestID.ToString()).FirstOrDefault();
            long Orderstatus = long.Parse(Request.Status.ToString());
            long DepartmentID = long.Parse(Request.SiteID.ToString());
            long Status = long.Parse(Request.Status.ToString());
            long UserID = 0;
            string ISProjectSite = "";
            ISProjectSite = ISProjectSiteDetailsNew(Convert.ToString(Request.parentid), conn);
            DataSet dsMailSubBody = new DataSet();
            dsMailSubBody = fillds("select * from mMessageEMailTemplates where ActivityID=(select Id from mDropdownValues where StatusID=28) and MessageID=(select Id from mDropdownValues where Value='Information') and DepartmentID=" + DepartmentID + "", conn);
            if (dsMailSubBody.Tables[0].Rows.Count > 0)
            {
                MailSubject = dsMailSubBody.Tables[0].Rows[0]["MailSubject"].ToString() + ", Order No # " + Request.OrderNo + "";
                DataSet dsAllUsers = new DataSet();
                dsAllUsers = fillds(" select UserApproverID from tOrderWiseAccess where OrderId=" + RequestID + "", conn);
                int cnt = dsAllUsers.Tables[0].Rows.Count;
                if (cnt > 0)
                {
                    for (int i = 0; i <= cnt - 1; i++)
                    {
                        UserID = long.Parse(dsAllUsers.Tables[0].Rows[i]["UserApproverID"].ToString());
                        MailBody = "Dear " + GetUserNameByUserID(UserID, conn) + ", <br/>";
                        MailBody = MailBody + dsMailSubBody.Tables[0].Rows[0]["MailBody"].ToString();
                        if (ISProjectSite == "Yes")
                        {
                            MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetailVodafoneTechnical(Request, conn);
                        }
                        else
                        {
                            MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                        }

                        MailBody = MailBody + "<br/><br/>" + EmailGetAddressDetails(Request, conn);
                        MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetail(RequestID, conn);
                        SendMail(MailBody + MailGetFooter(), MailSubject, EmailGetEmailIDsByUserID(UserID, conn));
                        SaveCorrespondsData(RequestID, MailSubject, MailBody, DepartmentID, Status, conn);
                    }
                }
                long TemplateID = long.Parse(dsMailSubBody.Tables[0].Rows[0]["ID"].ToString());
                AdditionalDistribution(RequestID, TemplateID, conn);
            }
            //send mail for end customer
            DataSet dsChkOredrIsEcomOrNot = new DataSet();
            dsChkOredrIsEcomOrNot = fillds("select * from tOrderHead where orderType in('PreOrder','NormalOrder') and id=" + RequestID + " ", conn);
            if (dsChkOredrIsEcomOrNot.Tables[0].Rows.Count > 0)
            {
                // GWC_SP_GetRequestHeadByRequestIDs_Result EcomMsg = new GWC_SP_GetRequestHeadByRequestIDs_Result();
                SendMailCancelOrderCustomer(Request, RequestID, reasoncode, conn);
            }

        }

        private void SendMailCancelOrderCustomer(GWC_SP_GetRequestHeadByRequestIDs_Result EcomMsg, long RequestID, int reasoncode, string[] conn)
        {
            string MailSubject;
            string MailBody = "";

            string Lanuage = "", CustomerEmail = "", Paymentmethod = "";
            long DepartmentID = long.Parse(EcomMsg.SiteID.ToString());
            long Status = long.Parse(EcomMsg.Status.ToString());
            string OrderNo = EcomMsg.OrderNo.ToString();
            DataSet dsLanuage = new DataSet();
            dsLanuage = fillds(" select PreferredLanguage,Email,Paymentmethod from tECommHead where OrderID=" + RequestID + " ", conn);
            Lanuage = Convert.ToString(dsLanuage.Tables[0].Rows[0]["PreferredLanguage"]);
            CustomerEmail = Convert.ToString(dsLanuage.Tables[0].Rows[0]["Email"]);
            Paymentmethod = Convert.ToString(dsLanuage.Tables[0].Rows[0]["Paymentmethod"]);

            DataSet dsMailSubBody = new DataSet();
            dsMailSubBody = fillds("select * from mMessageEMailTemplates where ActivityID=(select Id from mDropdownValues where StatusID=100) and MessageID=(select Id from mDropdownValues where Value='Information') and DepartmentID=" + DepartmentID + " and Active='Yes' ", conn);
            DataSet dsAllUsers = new DataSet();
            if (Lanuage == "English" || Lanuage == "" || Lanuage == null)
            {
                MailSubject = dsMailSubBody.Tables[0].Rows[1]["MailSubject"].ToString() + ", Order No # " + EcomMsg.OrderNo + "";
                MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetailForCancelInEnglish(EcomMsg, conn);
                MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetailCancelInEnglish(OrderNo, RequestID, reasoncode, conn);
                MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetailMessage(RequestID, Paymentmethod, conn);
                MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetailFooter(RequestID, conn);
            }
            else
            {
                MailSubject = dsMailSubBody.Tables[0].Rows[0]["MailSubject"].ToString() + ", Order No # " + EcomMsg.OrderNo + "";
                MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetailForCancelInEnglish(EcomMsg, conn);
                MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetailCancelInARB(OrderNo, RequestID, reasoncode, conn);
                MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetailMessageARB(RequestID, Paymentmethod, conn);
                MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetailFooterARB(RequestID, conn);
            }
            SendMailForCustomer(MailBody, MailSubject, CustomerEmail, DepartmentID, conn);
            SaveCorrespondsData(RequestID, MailSubject, MailBody, DepartmentID, Status, conn);
        }


        public void SendMailForCustomer(string MailBody, string MailSubject, string ToEmailIDs, long DepartmentID, string[] conn)
        {
            try
            {

                DataSet dsSourceEmail = new DataSet();
                dsSourceEmail = fillds(" select C.SourceEmail from mCompany C left outer join mTerritory T on c.id=T.ParentID where T.ID=" + DepartmentID + " ", conn);
                string SourceEmail = Convert.ToString(dsSourceEmail.Tables[0].Rows[0]["SourceEmail"]);

                // SmtpClient smtpClient = new SmtpClient("smtpout.asia.secureserver.net", 25);

                SmtpClient smtpClient = new SmtpClient("10.228.134.54", 25);
                MailMessage message = new MailMessage();


                MailAddress fromAddress = new MailAddress("donotreply@vodafone.com.qa", "Vodafone");
                // MailAddress fromAddress = new MailAddress(SourceEmail, "Vodafone");
                // MailAddress fromAddress = new MailAddress("admin@brilliantinfosys.com", "GWC");
                //  MailAddress fromAddress = new MailAddress("OMSNotification@gulfwarehousing.com", "GWC");


                //  add by  by suraj for company source emailid
                // MailAddress fromAddress = new MailAddress(SourceEmail, "GWC");                

                //From address will be given as a MailAddress Object
                message.From = fromAddress;

                //To address collection of MailAddress
                message.To.Add(ToEmailIDs);
                message.Subject = MailSubject;

                //Body can be Html or text format
                //Specify true if it  is html message
                message.IsBodyHtml = true;


                ///add by suraj for email images    comicon googleplay  appstore


                //  AlternateView view = AlternateView.CreateAlternateViewFromString(Regex.Replace(MailBody, "<[^>]+?>", string.Empty),Encoding.UTF8,MediaTypeNames.Text.Plain);


                AlternateView view = AlternateView.CreateAlternateViewFromString(MailBody, null, MediaTypeNames.Text.Html);


                LinkedResource pic1 = new LinkedResource("C:\\GWCProductionVersion2\\GWCProdV2\\images\\vodaphone.png", MediaTypeNames.Image.Jpeg);
                pic1.ContentId = "companylogo";
                LinkedResource pic2 = new LinkedResource("C:\\GWCProductionVersion2\\GWCProdV2\\images\\comicon.png", MediaTypeNames.Image.Jpeg);
                pic2.ContentId = "comicon";

                LinkedResource pic3 = new LinkedResource("C:\\GWCProductionVersion2\\GWCProdV2\\images\\googleplay.png", MediaTypeNames.Image.Jpeg);
                pic3.ContentId = "googleplay";
                LinkedResource pic4 = new LinkedResource("C:\\GWCProductionVersion2\\GWCProdV2\\images\\appstore.png", MediaTypeNames.Image.Jpeg);
                pic4.ContentId = "appstore";

                view.LinkedResources.Add(pic1);
                view.LinkedResources.Add(pic2);
                view.LinkedResources.Add(pic3);
                view.LinkedResources.Add(pic4);

                message.AlternateViews.Add(view);


                //Message body content
                message.Body = MailBody;

                smtpClient.EnableSsl = false;
                //Send SMTP mail
                smtpClient.UseDefaultCredentials = false;

                //   NetworkCredential basicCredential = new NetworkCredential("admin@brilliantinfosys.com", "6march1986");

                //  add by  by suraj for company source emailid
                NetworkCredential basicCredential = new NetworkCredential("donotreply@vodafone.com.qa", "");
                // NetworkCredential basicCredential = new NetworkCredential(SourceEmail, "");


                // NetworkCredential basicCredential = new NetworkCredential("OMSNotification@gulfwarehousing.com", "");
                smtpClient.Credentials = basicCredential;

                smtpClient.Send(message);

                //SaveEmailSenddata(MailSubject, ToEmailIDs, conn);
            }
            catch { }
        }

        private void SaveEmailSenddata(string mailSubject, string toEmailIDs, string[] conn)
        {
            DataSet dssave = new DataSet();
            dssave = fillds(" insert into MailSendDataSave values('" + mailSubject + "','" + toEmailIDs + "')  ", conn);
        }

        protected string EMailGetRequestPartDetailCancelInARB(string OrderNo, long Request, int reasoncode, string[] conn)
        {
            DataSet dsMailSubBody = new DataSet();
            dsMailSubBody = fillds("select * from torderhead where id=" + Request + "", conn);
            string ordernumbr = dsMailSubBody.Tables[0].Rows[0]["Ordernumber"].ToString();
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            DataSet dsOrdrDetail = new DataSet();
            dsOrdrDetail = fillds("select OD.Sequence,OD.Prod_Code,OD.Prod_Description,OD.OrderQty ,PRD.GroupSet,OD.Price,OD.Prod_Name from torderdetail OD left outer join mProduct PRD on OD.SkuId =PRD.ID where OD.Orderheadid=" + Request + "", conn);

            DataSet dsreasoncode = new DataSet();
            string reasonnm = "";
            dsreasoncode = fillds("select ReasonCode,ReasonCodeArb from mReasonCode where id=" + reasoncode + "", conn);
            if (dsreasoncode.Tables[0].Rows.Count > 0)
            {
                reasonnm = dsreasoncode.Tables[0].Rows[0]["ReasonCodeArb"].ToString();
                if (reasonnm == " ")
                {
                    reasonnm = dsreasoncode.Tables[0].Rows[0]["ReasonCode"].ToString();
                }
            }

            string messageBody = "<table width='100%'><tr><td style='text-align: right'><font face='Simplified Arabic' style='font-size:11;' color=#515161>شكراً لك على طلبك. هذا هو الإيصال الإلكتروني. يرجى الاحتفاظ بنسخة منها معك </font>";
            messageBody = messageBody + "</td></tr><tr><td></td></tr><tr><td></td></tr><tr><td></td></tr><tr><td style='text-align: right'> <font face='Times New Roman' style='font-size:11;' color=#515161><b>.تم إلغاء طلبك رقم (" + ordernumbr + ") الذي يحتوي على المنتج (" + reasonnm + ") المذكورة أدناه</b> </font></td></tr><tr><td><br/></td></tr></table><br/>";
            string htmlTableStart = "<table style=\" width: 100%; text-align:center;\" >";
            string htmlTableEnd = "</table>";
            string htmlHeaderRowStart = "<tr style =\"background-color:#E5E4E2; color:#E5E4E2;\">";
            string htmlHeaderRowEnd = "</tr>";
            string htmlTrStart = "<tr style =\"color:black; text-align: center;\">";
            string htmlTrEnd = "</tr>";
            string htmlTdStart = "<td style=\" width: 20%;   padding: 5px;color:black; text-align: center;font-size: 10pt;\">";
            string htmlTdEnd = "</td>";

            messageBody += htmlTableStart;

            messageBody += htmlHeaderRowStart;
            // messageBody += htmlTdStart + "Sr. No." + htmlTdEnd;
            messageBody += htmlTdStart + "<font face='Times New Roman' style='font-size:11;'><b>المنتج  </b></font>" + htmlTdEnd;
            // messageBody += htmlTdStart + "Description" + htmlTdEnd;
            messageBody += htmlTdStart + "<font face='Times New Roman' style='font-size:11;'><b>السعر" + "</b></font>" + htmlTdEnd;
            messageBody += htmlTdStart + "<font face='Times New Roman' style='font-size:11;'><b>كَمِّيّة " + "</b></font>" + htmlTdEnd;
            messageBody += htmlTdStart + "<font face='Times New Roman' style='font-size:11;'><b>التكلفة الشهرية " + "</b></font>" + htmlTdEnd;
            // messageBody += htmlTdStart + "Group Set" + htmlTdEnd;
            messageBody += htmlTdStart + "<font face='Times New Roman' style='font-size:11;'><b>الإجمالي" + "</b></font>" + htmlTdEnd;

            messageBody += htmlHeaderRowEnd;
            if (dsOrdrDetail.Tables[0].Rows.Count > 0)
            {
                for (int r = 0; r <= dsOrdrDetail.Tables[0].Rows.Count - 1; r++)
                {
                    messageBody += htmlTrStart;
                    //   messageBody += htmlTdStart + " " + dsOrdrDetail.Tables[0].Rows[r]["Sequence"].ToString() + " " + htmlTdEnd;
                    messageBody += htmlTdStart + " " + dsOrdrDetail.Tables[0].Rows[r]["Prod_Name"].ToString() + " " + htmlTdEnd;
                    // messageBody += htmlTdStart + " " + dsOrdrDetail.Tables[0].Rows[r]["Prod_Description"].ToString() + " " + htmlTdEnd;
                    messageBody += htmlTdStart + " " + dsOrdrDetail.Tables[0].Rows[r]["Price"].ToString() + " " + htmlTdEnd;
                    messageBody += htmlTdStart + " " + dsOrdrDetail.Tables[0].Rows[r]["OrderQty"].ToString() + " " + htmlTdEnd;
                    messageBody += htmlTdStart + "0 " + htmlTdEnd;
                    decimal Toatl = Convert.ToDecimal(dsOrdrDetail.Tables[0].Rows[r]["Price"].ToString()) * Convert.ToDecimal(dsOrdrDetail.Tables[0].Rows[r]["OrderQty"].ToString());
                    messageBody += htmlTdStart + " " + Toatl + " " + htmlTdEnd;
                    messageBody += htmlTrEnd;
                }
            }
            messageBody += htmlTableEnd;
            return messageBody;
        }

        protected string EMailGetRequestPartDetailMessageARB(long Request, string Paymentmethod, string[] conn)
        {
            string messageEnd = "";
            messageEnd = "<table width='100%'><tr><td style='text-align: right'><font face='Simplified Arabic' style=';' color=#515161><b>:لاسترجاع الأموال </b></font></td></tr>";
            messageEnd = messageEnd + "<tr><td></td></tr><tr><td></td></tr><tr><td></td></tr><tr><td style='text-align: right'>  <font face='Simplified Arabic' style='font-size:16;' color=#515161><b>: بالنسبة للطلبات التي تم دفع ثمنها عبر البطاقة الائتمانية <br/></td></tr> <tr><td></td></tr><tr><td></td></tr><tr><td></td></tr><tr>";
            messageEnd = messageEnd + "<tr><td style='text-align: right'><font face='Simplified Arabic' style='font-size:16;' color=#515161> سيتم إرجاع الأموال التي تم دفعها عند الشراء من متجر فودافون الإلكتروني بواسطة البطاقة الائتمانية مباشرة إلى رصيد   </td></tr>";
            messageEnd = messageEnd + "<tr><td style='text-align: right'><font face='Simplified Arabic' style='font-size:16;' color=#515161> البطاقة التي تم استخدامها في عملية الشراء. لا يمكن استرجاع الأموال نقداً أو عبر الدفع بشيك أو أي وسيلة تحويل أموال    </font></td></tr>";
            messageEnd = messageEnd + "<tr><td style='text-align: right'> <font face='Simplified Arabic' style='font-size:16;' color=#515161>  أخرى. يرجى السماح بفترة تصل 10 أيام عمل ليصل المبلغ إلى بطاقتك الائتمانية.      </font></td></tr><tr><td><br/></td></tr><tr><td></td></tr><tr><td></td></tr><tr><td></td></tr><tr>";
            messageEnd = messageEnd + "<tr><td style='text-align: right'> <font face='Simplified Arabic' style='font-size:16;' color=#515161><b>:للطلبات التي تم دفع ثمنها نقداً  </b></font></td></tr><tr><td></td></tr><tr><td></td></tr><tr><td></td></tr><tr><tr><td style='text-align: right'> <font face='Simplified Arabic' style='font-size:16;' color=#515161> سيتم إرجاع الأموال التي تم دفعها نقداً عند استلام المنتج الذي تم شراؤه من متجر فودافون الإلكتروني عبر حوالة    </font></td></tr>";
            messageEnd = messageEnd + "<tr><td style='text-align: right'> <font face='Simplified Arabic' style='font-size:16;' color=#515161>  مصرفية إلى حساب العميل المصرفي. عند المطالبة باسترجاع الأموال، يجب على العميل تقديم معلومات حسابه   </font></td></tr><tr><td style='text-align: right'> <font face='Simplified Arabic' style='font-size:16;' color=#515161>.المصرفي. يرجى السماح بفترة تصل 10 أيام عمل ليصل المبلغ إلى بطاقتك الائتمانية   </font></td></tr>";
            messageEnd = messageEnd + "<tr><td></td></tr><tr><td></td></tr><tr><td></td></tr><tr><tr><td style='text-align: right'> <font face='Simplified Arabic' style='font-size:16;' color=#515161>. للمزيد من التفاصيل حول سياسة الاسترداد والضمان في فودافون، يرجى الضغط هنا   </font></td></tr><tr><td></td></tr><tr><td></td></tr><tr><td></td></tr><tr>";
            messageEnd = messageEnd + "<tr><td style='text-align: right'> <font face='Simplified Arabic' style='font-size:16;' color=#515161> لتقديم طلب شراء جديد أو للحصول على المزيد من المعلومات حول جميع منتجات وخدمات فودافون يرجى زيارة    </font></td></tr>";
            messageEnd = messageEnd + "<tr><td style='text-align: right'> <font face='Simplified Arabic' style='font-size:16;' color=#515161><a href='http://www.vodafone.qa/'><b>  www.vodafone.qa .   </b></a></font></td></tr>";
            messageEnd = messageEnd + "<tr><td><br/></td></tr><tr><td style='text-align: right'> <font face='Simplified Arabic' style='font-size:16;' color=#515161>    </font></td></tr></table>";

            return messageEnd;
        }



        protected string EMailGetRequestPartDetailMessage(long Request, string Paymentmethod, string[] conn)
        {
            string messageEnd = "", PaymentType = "";
            messageEnd = " <font face='Verdana' style='font-size:11;' color=#68687d><b>To process a refund:</b></font>" + "<br/><br/>";

            messageEnd = messageEnd + "<font face='Verdana' style='font-size:11;' color=#68687d><b>For orders paid by credit card:</b></font>" + "<br/>";
            messageEnd = messageEnd + "<font face='Verdana' style='font-size:11;' color=#515161>Any purchase made on the Vodafone Online Store and paid by credit card will be refunded back to the </font>" + "<br/>";
            messageEnd = messageEnd + "<font face='Verdana' style='font-size:11;' color=#515161>card used to carry out the transaction. No cash, cheque or other money transfer method will be provided</font>" + " <br/> ";
            messageEnd = messageEnd + "<font face='Verdana' style='font-size:11;' color=#515161>as a refund. Please allow 10 working days for the refund to reach your credit card.</font>" + " <br/><br/> ";

            messageEnd = messageEnd + "<font face='Verdana' style='font-size:11;' color=#68687d><b>For orders paid in cash:</b></font>" + "<br/>";
            messageEnd = messageEnd + "<font face='Verdana' style='font-size:11;' color=#515161>Any purchase made on the Vodafone Online Store and paid with cash on delivery will be refunded by</font>" + "<br/>";
            messageEnd = messageEnd + "<font face='Verdana' style='font-size:11;' color=#515161>bank transfer to the customer’s bank account. When claiming a refund, the customer is required to</font>" + "<br/>";
            messageEnd = messageEnd + "<font face='Verdana' style='font-size:11;' color=#515161>provide valid bank account details. Please allow 10 working days for the money to reach the bank</font> <br/><font face='Verdana' style='font-size:11;' color=#515161> account.</font>" + "  ";

            return messageEnd;
        }



        protected string EMailGetRequestPartDetailFooterARB(long Request, string[] conn)
        {
            string messageEnd = "";
            // messageEnd = "";
            messageEnd = messageEnd + "<table  width='100%'><tr><td style='text-align: right'> <font face='Simplified Arabic' style='font-size:16;' color=#515161> في حال وجود أي استفسارات يرجى التواصل معنا عبر أي من وسائل الاتصال التالية:   </font></td></tr>";
            messageEnd = messageEnd + "<tr><td style='text-align: right'> <font face='Simplified Arabic' style='font-size:16;' color=#515161> <b>.My Vodafone</b>  	المحادثة المباشرة معنا عبر تطبيق  &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<b>•</b>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; </font></td></tr>";
            messageEnd = messageEnd + "<tr><td style='text-align: right'> <font face='Simplified Arabic' style='font-size:16;' color=#515161> <b>111</b>  	الاتصال بنا عبر  &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<b>•</b>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</font></td></tr>";
            messageEnd = messageEnd + "<tr><td style='text-align: right'> <font face='Simplified Arabic' style='font-size:16;' color=#515161>   		زيارة أي من متاجرنا &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<b>•</b>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</font></td></tr>";
            messageEnd = messageEnd + "<tr><td style='text-align: right'> <font face='Simplified Arabic' style='font-size:16;' color=#515161><a href='http://are.qa@vodafone.com'> <b>care.qa@vodafone.com</b></a>  			رسالة بريد إلكتروني إلى  &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<b>•</b>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</font></td></tr><tr><td></td></tr><tr><td></td></tr><tr><td></td></tr><tr>";
            messageEnd = messageEnd + "<tr><td style='text-align: right'> <font face='Simplified Arabic' style='font-size:16;' color=#515161>  مع أطيب التحيات  </font></td></tr><tr><td></td></tr><tr><td></td></tr><tr><td></td></tr><tr>";
            messageEnd = messageEnd + "<tr><td style='text-align: right'> <font face='Simplified Arabic' style='font-size:16;' color=#515161>  فريق فودافون قطر  </font></td></tr><tr><td></td></tr><tr><td></td></tr><tr><td></td></tr><tr>";
            messageEnd = messageEnd + "<tr> <td style='text-align: left'><a href='https://www.vodafone.qa/en/my-vodafone-app'>  <img src = 'cid:comicon'  width = '90' height = '90'></a></td> </tr>";
            messageEnd = messageEnd + "<tr> <td style='text-align: left'><font face='Arial' style='font-size:16;' color=#515161>My Vodafone  حمل تطبيق    </font> </td> </tr> <tr> <td style='text-align: left'> <a href='https://play.google.com/store/apps/details?id=qa.vodafone.myvodafone'>  <img src = 'cid:googleplay'  width = '150' height = '50'></a>";
            messageEnd = messageEnd + " <a href='https://itunes.apple.com/us/app/my-vodafone/id1076290326?ls=1&mt=8'><img src = 'cid:appstore'  width = '150' height = '50'></a>  </td> </tr> </table>   </body></html> </table>";

            messageEnd = messageEnd + "<br/><br/><br/><table  width='100%'><tr><td style='text-align: right'> <font face='Simplified Arabic' style='font-size:16;' color=#515161> تم إرسال هذه الرسالة بواسطة عنوان بريد إلكتروني غير قابل للرد من فودافون قطر. </font></td></tr></table> ";
            //messageEnd = messageEnd + " <tr> <td style='text-align: right'> فريق فودافون قطر  </td> </tr>   <tr> <td style='text-align: right'> <img src = 'https://oms.gwclogistics.com/GWCProdV2/images/comicon.png'  width = '70' height = '70'> ";
            //messageEnd = messageEnd + " </td> </tr>  <tr> <td style='text-align: right'> <h3> حمل تطبيق  My Vodafone </h3> </td> </tr> <tr> <td style='text-align: right'>  <img src = 'https://testoms.gwclogistics.com/GWCEcommTestV3/images/googleplay.png'  width = '150' height = '70'>";
            //messageEnd = messageEnd + " <img src = 'https://testoms.gwclogistics.com/GWCEcommTestV3/images/appstore.png'  width = '150' height = '70'>  </td> </tr> </table>   </body></html> ";

            return messageEnd;
        }
        protected string EMailGetRequestPartDetailFooter(long Request, string[] conn)
        {
            string messageEnd = "";

            messageEnd = " <html><head><style> h3 {color: #708090;}</style></head><body><table><tr><td><p><font face='Verdana' style='font-size:11;' color=#515161> For more details on Vodafone Return and Warranty policy, please click <a href='http://www.vodafone.qa/en/support/main-topics/other-topics/returns-and-warranty-policy'>here</a></font> <p></br>";
            messageEnd = messageEnd + " <p><font face='Verdana' style='font-size:11;' color=#515161> To place a new order or to get information on all Vodafone’s products and services, please visit</font></td></tr>";
            messageEnd = messageEnd + "<tr><td><font face='Verdana' style='font-size:11;' color=#515161><a href='http://www.vodafone.qa/'>www.vodafone.qa</a>.</font></td></tr><tr><td><br/></td></tr>";
            messageEnd = messageEnd + "   <tr><td><font face='Verdana' style='font-size:11;' color=#515161>  For any concerns please contact us through any of the below channels:</font></td></tr>";
            messageEnd = messageEnd + "<tr><td><ul>  <li><font face='Verdana' style='font-size:11;' color=#515161> Chat with us from My Vodafone app</font> </li>  <li><font face='Verdana' style='font-size:11;' color=#515161> Call us on 111</font> </li>  <li><font face='Verdana' style='font-size:11;' color=#515161> Visit our stores </font></li> <li><font face='Verdana' style='font-size:11;' color=#515161> E-mail us at care.qa@vodafone.com </font></li>";
            messageEnd = messageEnd + " </ul></td></tr><tr><td> <p><font face='Verdana' style='font-size:11;' color=#515161> Kind regards,</font><br/><br/><font face='Verdana' style='font-size:11;' color=#515161> The Vodafone Qatar Team </font></td>   </tr><tr><td><a href='https://www.vodafone.qa/en/my-vodafone-app'><img src = 'cid:comicon'  width = '90' height = '90'></a><br/> <font face='Arial' style='color: rgb(120, 120, 120);font-size:20;'>Download the My Vodafone App</font><br/> ";
            messageEnd = messageEnd + "  <a href='https://play.google.com/store/apps/details?id=qa.vodafone.myvodafone'>  <img src = 'cid:googleplay'  width = '150' height = '50'></a><a href='https://itunes.apple.com/us/app/my-vodafone/id1076290326?ls=1&mt=8'><img src = 'cid:appstore'  width = '150' height = '50'></a>   </td></tr></table></body></html>";

            messageEnd = messageEnd + "<br/><br/><br/><font face='Verdana' style='font-size:11;' color=#515161>This email was sent from a no-reply email address at Vodafone Qatar.</font>";

            //messageEnd = " <html><head><style> h3 {color: #708090;}</style></head><body><table><tr><td><p> For more details on Vodafone Return and Warranty policy, please click <a href='http://www.vodafone.qa/en/support/main-topics/other-topics/returns-and-warranty-policy'>here</a><p></br>";
            //messageEnd = messageEnd + " <p> To place a new order or to get information on all Vodafone’s products and services, please visit www.vodafone.qa.  </td></tr><tr><td> <p> For any concerns please contact us through any of the below channels:";
            //messageEnd = messageEnd + "<ul>  <li> Chat with us from My Vodafone app </li>  <li> Call us on 111 </li>  <li> Visit our stores </li> <li> E - mail us at care.qa @vodafone.com </li>";
            //messageEnd = messageEnd + " </ul></td></tr><tr><td> <p> Kind regards,<br/><br/> The Vodafone Qatar Team </td>   </tr><tr><td><img src = 'https://testoms.gwclogistics.com/GWCEcommTestV3/images/comicon.png'  width = '70' height = '70'><h3> Download the My Vodafone App</h3>";
            //messageEnd = messageEnd + "  <img src = 'https://testoms.gwclogistics.com/GWCEcommTestV3/images/googleplay.png'  width = '150' height = '70'><img src = 'https://testoms.gwclogistics.com/GWCEcommTestV3/images/appstore.png'  width = '150' height = '70'>   </td></tr></table></body></html>";

            return messageEnd;
        }

        protected string EMailGetRequestDetailForCancelInEnglish(GWC_SP_GetRequestHeadByRequestIDs_Result Request, string[] conn)
        {
            string messageBody = "";

            //  messageBody = "  <html> <body> <table  width='100%'> <tr> <td  style='text-align: right'> <img src ='https://oms.gwclogistics.com/GWCProdV2/images/appstore.png'  width='70' height='70'> </td> </tr> </table> </body></html>";
            messageBody = " <html> <body> <table  width='100%'> <tr> <td  style='text-align: right'> <img src ='cid:companylogo'  width='70' height='70'> </td> </tr> </table> </body></html>";
            return messageBody;
        }

        //for cancel order mail to customer
        protected string EMailGetRequestPartDetailCancelInEnglish(string OrderNo, long RequestID, int reasoncode, string[] conn)
        {
            DataSet dsMailSubBody = new DataSet();
            dsMailSubBody = fillds("select * from torderhead where id=" + RequestID + "", conn);
            string ordernumbr = dsMailSubBody.Tables[0].Rows[0]["Ordernumber"].ToString();

            DataSet dsOrdrDetail = new DataSet();
            dsOrdrDetail = fillds("select OD.Sequence,OD.Prod_Code,OD.Prod_Description,OD.OrderQty ,PRD.GroupSet,OD.Price,OD.Prod_Name from torderdetail OD left outer join mProduct PRD on OD.SkuId =PRD.ID where OD.Orderheadid=" + RequestID + "", conn);
            DataSet dsreasoncode = new DataSet();
            dsreasoncode = fillds("select ReasonCode from mReasonCode where id=" + reasoncode + "", conn);
            string reasonnm = dsreasoncode.Tables[0].Rows[0]["ReasonCode"].ToString();

            //string messageBody = "<font>Thank you for your order. This is your e-receipt. Please keep a copy for your records. </font><br/><br/>";
            //messageBody = messageBody + "<font><b> Your Order number <font color='red'> (" + ordernumbr + ")</font> containing below item(s) has been <font color='red'> cancelled </font> (" + reasonnm + ")!</b></font><br/><br/>";
            string messageBody = "<font face='Verdana' style='font-size:11;' color=#515161>Thank you for your order. This is your e-receipt. Please keep a copy for your records. </font><br/><br/>";
            messageBody = messageBody + "<font face='Verdana' style='font-size:11;' color=#515161><b> Your Order number <font face='Verdana' style='font-size:11;' color='red'> (" + ordernumbr + ")</font> containing below item(s) has been <font face='Verdana' style='font-size:11;' color='red'> cancelled </font> (" + reasonnm + ")!</b></font><br/><br/>";

            string htmlTableStart = "<table style=\" width: 100%; text-align:center;\" >";
            string htmlTableEnd = "</table>";
            string htmlHeaderRowStart = "<tr style =\"background-color:#E5E4E2; color:#E5E4E2;\">";
            string htmlHeaderRowEnd = "</tr>";
            string htmlTrStart = "<tr style =\"color:black; text-align: center;\">";
            string htmlTrEnd = "</tr>";
            string htmlTdStart = "<td style=\" width: 20%;   padding: 5px;color:black; text-align: center;font-size: 10pt;\">";
            string htmlTdEnd = "</td>";

            messageBody += htmlTableStart;

            messageBody += htmlHeaderRowStart;
            // messageBody += htmlTdStart + "Sr. No." + htmlTdEnd;
            messageBody += htmlTdStart + "<font face='Verdana' style='font-size:11;'><b>Item Selected</b></font>" + htmlTdEnd;
            // messageBody += htmlTdStart + "Description" + htmlTdEnd;
            messageBody += htmlTdStart + "<font face='Verdana' style='font-size:11;'><b>Price</b></font>" + htmlTdEnd;
            messageBody += htmlTdStart + "<font face='Verdana' style='font-size:11;'><b>Quantity</b></font>" + htmlTdEnd;
            messageBody += htmlTdStart + "<font face='Verdana' style='font-size:11;'><b>Monthly payments</b></font> " + htmlTdEnd;
            // messageBody += htmlTdStart + "Group Set" + htmlTdEnd;
            messageBody += htmlTdStart + "<font face='Verdana' style='font-size:11;'><b>Total</b></font>" + htmlTdEnd;

            messageBody += htmlHeaderRowEnd;
            if (dsOrdrDetail.Tables[0].Rows.Count > 0)
            {
                for (int r = 0; r <= dsOrdrDetail.Tables[0].Rows.Count - 1; r++)
                {
                    messageBody += htmlTrStart;
                    //   messageBody += htmlTdStart + " " + dsOrdrDetail.Tables[0].Rows[r]["Sequence"].ToString() + " " + htmlTdEnd;
                    messageBody += htmlTdStart + " " + dsOrdrDetail.Tables[0].Rows[r]["Prod_Name"].ToString() + " " + htmlTdEnd;
                    // messageBody += htmlTdStart + " " + dsOrdrDetail.Tables[0].Rows[r]["Prod_Description"].ToString() + " " + htmlTdEnd;
                    messageBody += htmlTdStart + " " + dsOrdrDetail.Tables[0].Rows[r]["Price"].ToString() + " " + htmlTdEnd;
                    messageBody += htmlTdStart + " " + dsOrdrDetail.Tables[0].Rows[r]["OrderQty"].ToString() + " " + htmlTdEnd;
                    messageBody += htmlTdStart + "0 " + htmlTdEnd;
                    decimal Toatl = Convert.ToDecimal(dsOrdrDetail.Tables[0].Rows[r]["Price"].ToString()) * Convert.ToDecimal(dsOrdrDetail.Tables[0].Rows[r]["OrderQty"].ToString());
                    messageBody += htmlTdStart + " " + Toatl + " " + htmlTdEnd;
                    messageBody += htmlTrEnd;
                }
            }
            messageBody += htmlTableEnd;
            return messageBody;

        }
        public string GetUOMName(long UOMID, string[] conn)
        {
            DataSet ds = new DataSet();
            string UOM = "";
            ds = fillds("select Description from muom where ID=" + UOMID + "", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                UOM = ds.Tables[0].Rows[0]["Description"].ToString();
            }
            return UOM;
        }

        public long GetCostCenterApproverID(long StatutoryValue, string[] conn)
        {
            DataSet ds = new DataSet();
            long ApproverID = 0;
            ds = fillds("select ApproverID  FROM mCostCenterMain where ID=" + StatutoryValue + "", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                ApproverID = Convert.ToInt64(ds.Tables[0].Rows[0]["ApproverID"].ToString());
            }
            return ApproverID;
        }

        public string GetInvoiceNoofOrder(long OrderID, string[] conn)
        {
            DataSet ds = new DataSet();
            string InvNo = "";
            ds = fillds("select InvoiceNo from torderhead where id=" + OrderID + "", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                InvNo = ds.Tables[0].Rows[0]["InvoiceNo"].ToString();
            }
            if (InvNo == "" || InvNo == null) InvNo = "0";
            return InvNo;
        }

        public void RemoveFromTStatutory(long OrderID, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("delete from tstatutorydetail where ReferenceID=" + OrderID + "", conn);
        }

        public DataSet GetApproverDepartmentWise(long Deptid, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select UserId from mApprovalLevelDetail where DepartmentID=" + Deptid + "", conn);
            return ds;
        }

        public string CheckString(string value)
        {
            value = value.Replace("&", "and");
            value = value.Replace("|", " ");
            return value;
        }


        #endregion

        // code by Rahul To Update ApprovalFlag
        public void updateApprovedflag(long OrderID, int ApprovalLevel, long UserApproverID, string[] conn)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_UpdateApprovedFlag";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("OrderID", OrderID);
                cmd.Parameters.AddWithValue("ApprovalLevel", ApprovalLevel);
                cmd.Parameters.AddWithValue("UserApproverID", UserApproverID);
                cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }
        }

        public long CheckForAllreadyApproved(long OrderID, int ApprovalLevel, long UserApproverID, string[] conn)
        {
            SqlDataReader dr;
            long ApprovalFlag = 0;
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_SelectApprovedFlag";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("OrderID", OrderID);
                cmd.Parameters.AddWithValue("ApprovalLevel", ApprovalLevel);
                cmd.Parameters.AddWithValue("UserApproverID", UserApproverID);
                dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    ApprovalFlag = long.Parse(dr["ApprovedFlag"].ToString());
                }
                dr.Close();
                cmd.Connection.Close();
            }
            return ApprovalFlag;
        }

        public DataSet checkLocationForDepartment(long deptid, string[] conn)
        {

            DataSet ds = new DataSet();
            ds = fillds("Select Location,isnull(emoney,0)Emoney from mTerritory where ID=" + deptid + "", conn);
            return ds;

        }

        #region New Change Requirment

        public DataSet GetDeptReqUser(long ID, string[] conn)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    using (DataSet ds = new DataSet())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "SP_GetUserbyDepartmentID";
                        cmd.Connection = svr.GetSqlConn(conn);
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("ID", ID);
                        da.SelectCommand = cmd;
                        da.Fill(ds);
                        return ds;
                    }
                }
            }
        }

        public decimal GetCurrentStockofProduct(long pid, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                decimal crntAtk = 0;
                tProductStockDetail psd = new tProductStockDetail();
                psd = (from a in db.tProductStockDetails
                       where a.ProdID == pid
                       select a).FirstOrDefault();
                crntAtk = Convert.ToDecimal(psd.AvailableBalance.ToString());
                return crntAtk;
            }
        }

        public decimal GetCurrentStockofProduct_New(long pid, string[] conn)
        {
            decimal crntAtk = 0;
            try
            {
                SqlDataReader dr;
              
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetCurrentStockofProduct";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@ProdID", pid);
                dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    crntAtk = decimal.Parse(dr["AvailableBalance"].ToString());
                }
                dr.Close();
                cmd.Connection.Close();
               
            }
            catch (System.Exception ex)
            {
              
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "Sp_EnterErrorTracking";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("Data", ex.Data.ToString());
                    cmd.Parameters.AddWithValue("GetType", ex.Source.ToString());
                    cmd.Parameters.AddWithValue("InnerException", "Error");
                    cmd.Parameters.AddWithValue("Message", ex.Message.ToString());
                    cmd.Parameters.AddWithValue("Source", pid + "| GetCurrentStockofProduct_New");
                    cmd.Parameters.AddWithValue("DateTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("UserID", "0");
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                    crntAtk = 0;
                }
            }

            return crntAtk;

        }
        #endregion


        #region Driver assign 25 order

        public DataSet CheckPreviousOrderCount(long driverid, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select count(*) cnt from tDriverAssignDetail left outer join tOrderHead on tDriverAssignDetail.orderid=tOrderHead.id where driverid=" + driverid + " and tOrderHead.status=25", conn);
            return ds;

        }

        #endregion


        #region Show Dispatch Grid information

        public DataSet ShowDispatchGrid(long OrderId, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select TD.SkuCode,TD.DeliveryStatus,TD.MSSIDN, TD.ReadyForActivation,TD.Activated,TD.OutForDelivery,TD.PendingForDocument,TD.UploadDelivered,TD.SkuDescription,TD.OrderNo,MS.Status,TD.Id,TD.PackageID from tOrderDispatchStatus TD left outer join mstatus   MS on TD.DeliveryStatus=MS.ID where TD.OrderNO=" + OrderId + "", conn);
            return ds;
        }


        public DataSet GetOrderdate(long OrderID, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from torderhead where id=" + OrderID + "", conn);
            // //string orderdate = ds.Tables[0].Rows[0]["Orderdate"].ToString();
            // if (orderdate == "" || orderdate == null) orderdate = "0";
            return ds;
        }

        public string UpdatePendingforActivation(long RequestID, string SkuCode, string[] conn)
        {
            string result = "";
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_UpdateDispatchActivity";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@PendingforActivation", "green");
            cmd.Parameters.AddWithValue("@OrderNo", RequestID);
            cmd.Parameters.AddWithValue("@skucode", SkuCode);
            cmd.Parameters.AddWithValue("@Flag", "UpdatePendingforActivation");
            cmd.ExecuteNonQuery();
            result = "true";

            //send mail for activated
            ActivtaedSendEmail(RequestID, conn);

            return result;
        }



        public DataSet GetReportData(Int64 OrderID, string[] conn)
        {
            DataSet ds = new DataSet();
            //  ds = fillds("select tOrderDetail.OrderHeadId,convert(date,tECommHead.OrderCreationDate,113)OrderCreationDate,tECommHead.CustomerFirstName +' '+ tECommHead.CustomerLastName as cname,tECommHead.BuildingName +' '+tECommHead.Streetname +' '+ tECommHead.City+' '+tECommHead.Country as daddress,tECommHead.vodafoneStoreName as rname,tECommHead.BuildingName2 +' '+tECommHead.Streetname2 +' '+ tECommHead.City2+' '+tECommHead.Country2 as rdaddress,tECommHead.Idt,tECommHead.PaymentMethod,tECommHead.ContactNumber,tECommHead.IDExpiryDate  ,tOrderDetail.Prod_Name,mProductCategory.Name,tOrderDetail.OrderQty,tOrderDetail.Price,tOrderDetail.Total,AmountPaid,AmounttobeCollected from tOrderDetail  left outer join  mProduct on mProduct.ID=tOrderDetail.SkuId   left outer join mProductCategory on mProductCategory.ID=mProduct.ProductCategoryID    left outer join tECommHead on tECommHead.OrderID=tOrderDetail.OrderHeadId  where  tOrderDetail.OrderHeadId=" + OrderID + " ", conn);
            // ds = fillds("SELECT dbo.tOrderDetail.OrderHeadId,convert(date,tECommHead.OrderCreationDate,113)OrderCreationDate,tECommHead.CustomerFirstName +' '+ tECommHead.CustomerLastName as cname,tECommHead.BuildingName +' '+tECommHead.Streetname +' '+ tECommHead.City+' '+tECommHead.Country as daddress,tECommHead.vodafoneStoreName as rname,tECommHead.BuildingName2 +' '+tECommHead.Streetname2 +' '+ tECommHead.City2+' '+tECommHead.Country2 as rdaddress,tECommHead.IDExpiryDate, dbo.tOrderDetail.Prod_Name, dbo.mProductCategory.Name,dbo.mProduct.Productcode, dbo.tOrderDetail.OrderQty, dbo.tOrderDetail.Price, dbo.tOrderDetail.Total, dbo.tECommHead.AmountPaid, dbo.tECommHead.AmountToBeCollected, dbo.mProduct.ProductType, dbo.mDeliveryBarcode.OrderNoBarCode, dbo.mDeliveryBarcode.AmtPaidBarCode, dbo.tOrderHead.OrderNo, dbo.tOrderHead.GrandTotal, dbo.tECommHead.Idt, dbo.tECommHead.PaymentMethod, dbo.tECommHead.ContactNumber, dbo.tECommHead.EcomOrderNumber FROM   dbo.tOrderDetail LEFT OUTER JOIN dbo.mProduct ON dbo.mProduct.ID = dbo.tOrderDetail.SkuId LEFT OUTER JOIN   dbo.mProductCategory ON dbo.mProductCategory.ID = dbo.mProduct.ProductCategoryID LEFT OUTER JOIN   dbo.tECommHead ON dbo.tECommHead.OrderID = dbo.tOrderDetail.OrderHeadId LEFT OUTER JOIN    dbo.mDeliveryBarcode ON dbo.mDeliveryBarcode.OrderID = dbo.tOrderDetail.OrderHeadId LEFT OUTER JOIN dbo.tOrderHead ON dbo.tOrderHead.Id = dbo.tOrderDetail.OrderHeadId where  tOrderDetail.OrderHeadId=" + OrderID + "", conn);
            // ds = fillds("SELECT dbo.tECommHead.PaymentMethod, dbo.tECommHead.ContactNumber, dbo.tECommDetail.ProductType, dbo.tECommDetail.ProductName, dbo.tECommDetail.ProductCode, dbo.tECommDetail.UnitPrice,  dbo.tECommDetail.Quantity, dbo.tECommDetail.PackageId, dbo.tECommDetail.PackageStatus, dbo.tECommDetail.UniqueNo, dbo.tECommDetail.VoucherCode, dbo.tECommDetail.Token, dbo.tECommHead.EcomOrderNumber,  dbo.tECommHead.CustomerFirstName, dbo.tECommHead.CustomerLastName, dbo.tECommHead.VodafoneStoreName, dbo.tECommHead.BuildingName2, dbo.tECommHead.Streetname2, dbo.tECommHead.City2, dbo.tECommHead.Country2, dbo.tECommHead.POBox, dbo.tECommHead.PartialPayment, dbo.tECommHead.AmountPaid, dbo.tECommHead.AmountToBeCollected, dbo.mDeliveryBarcode.OrderNoBarCode,dbo.mDeliveryBarcode.AmtPaidBarCode, dbo.tOrderHead.OrderNo, dbo.tOrderHead.GrandTotal, dbo.tECommHead.Idt, convert(varchar,dbo.tECommHead.OrderCreationDate,103)OrderCreationDate, convert(varchar,dbo.tECommHead.IDExpiryDate,103)IDExpiryDate,tECommHead.DeliveryType FROM  dbo.tECommHead LEFT OUTER JOIN  dbo.tOrderHead LEFT OUTER JOIN dbo.mDeliveryBarcode ON dbo.tOrderHead.Id = dbo.mDeliveryBarcode.OrderID ON dbo.tECommHead.OrderID = dbo.tOrderHead.Id RIGHT OUTER JOIN dbo.tECommDetail ON dbo.tECommHead.ID = dbo.tECommDetail.EcomHeadID where  tOrderHead.id=" + OrderID + "", conn);
            ds = fillds("Select * from VW_GetReportData where  torderheadid=" + OrderID + "", conn);
            return ds;
        }

        public DataSet GetBoardbandReportData(Int64 OrderID, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds(" select * from View_BroadbandReport where orderid=" + OrderID + "", conn);  //and mProduct.ProductCategoryID=3
            return ds;
        }


        public string CheckAllProductISActiveOrNot(Int64 OrderID, string[] conn)
        {
            string result = "";
            DataSet ds = new DataSet();
            ds = fillds("select count(*)cnt from tOrderDispatchStatus where orderno=" + OrderID + " and ReadyForActivation='red'", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                if (Convert.ToInt32(ds.Tables[0].Rows[0]["cnt"]) > 0)
                {
                    result = "red";
                }
                else
                {
                    result = "green";
                }
            }

            return result;
        }

        public string UpdateVirtualProduct(long Orderno, string[] conn)
        {
            string result = "";
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_UpdateDispatchActivity";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@PendingForDriverAllocation", "green");
            cmd.Parameters.AddWithValue("@OrderNo", Orderno);
            cmd.Parameters.AddWithValue("@Flag", "UpdatePendingForDriverAllocation");
            cmd.ExecuteNonQuery();
            result = "true";
            return result;
        }

        public string UpdateDocumentation(long Orderno, string skucode, string[] conn)
        {
            string result = "";
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_UpdateDispatchActivity";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@PendingForDriverAllocation", "green");
            cmd.Parameters.AddWithValue("@OrderNo", Orderno);
            cmd.Parameters.AddWithValue("@skucode", skucode);
            cmd.Parameters.AddWithValue("@Flag", "UpdateDocumentation");
            cmd.ExecuteNonQuery();
            result = "true";

            SendEmailUploadComplete(Orderno, conn);

            return result;
        }

        public void SendEmailUploadComplete(long orderNo, string[] conn)
        {
            string RequestID = orderNo.ToString();
            string MailSubject = "";
            string MailBody;
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            try
            {
                DataSet dsDelivery = new DataSet();
                dsDelivery = fillds(" select * from  tecommhead where orderid=" + RequestID + "", conn);
                string dsDeliveryType = dsDelivery.Tables[0].Rows[0]["DeliveryType"].ToString();
                DataSet dsAllApplLevel = new DataSet();
                dsAllApplLevel = fillds("select * from tOrderWiseAccess where OrderId=" + RequestID + "   and ApprovalLevel>0", conn);
                int ApplCnt = dsAllApplLevel.Tables[0].Rows.Count;
                if (ApplCnt > 0)
                {
                    long TemplateID = 0;
                    GWC_SP_GetRequestHeadByRequestIDs_Result Request = new GWC_SP_GetRequestHeadByRequestIDs_Result();
                    Request = db.GWC_SP_GetRequestHeadByRequestIDs(RequestID.ToString()).FirstOrDefault();
                    long DepartmentID = long.Parse(Request.SiteID.ToString());
                    long Status = long.Parse(Request.Status.ToString());
                    string OrderNumber = Convert.ToString(Request.OrderNo);
                    DataSet dsMailSubBody = new DataSet();
                    dsMailSubBody = fillds(" select * from mMessageEMailTemplates where ActivityID=(select Id from mDropdownValues where StatusID=39) and MessageID=(select Id from mDropdownValues where Value='Information') and DepartmentID=" + DepartmentID + " and Active='Yes'", conn);
                    if (dsMailSubBody.Tables[0].Rows.Count > 0)
                    {
                        TemplateID = long.Parse(dsMailSubBody.Tables[0].Rows[0]["ID"].ToString());
                        MailSubject = dsMailSubBody.Tables[0].Rows[0]["MailSubject"].ToString() + ", Order No # " + OrderNumber + "";
                        AdditionalDistribution(Convert.ToInt64(RequestID), TemplateID, conn);
                    }
                    if (dsDeliveryType.ToUpper() == "HOME")
                    {
                        for (int a = 0; a <= ApplCnt - 1; a++)
                        {
                            MailBody = "Dear Fulfilment Team,<br/>";
                            MailBody = MailBody + dsMailSubBody.Tables[0].Rows[0]["MailBody"].ToString();
                            MailBody = MailBody + "<br/><br/>" + EMailUploadComplete(Request);
                            SendMail(MailBody + MailGetFooter(), MailSubject, EmailGetEmailIDsByUserID(Convert.ToInt64(dsAllApplLevel.Tables[0].Rows[a]["UserApproverID"].ToString()), conn));
                            SaveCorrespondsData(Convert.ToInt64(RequestID), MailSubject, MailBody, DepartmentID, Status, conn);
                        }
                    }
                    //send email notification for Retail Admin

                    if (dsDeliveryType.ToUpper() == "HUB")
                    {
                        DataSet dsadmin = new DataSet();
                        dsadmin = fillds(" select * from mUserTerritoryDetail UT  left outer join muserprofilehead UP on UP.id = UT.userid   where UT.territoryid = " + DepartmentID + "  and UP.Usertype = 'Retail User Admin'", conn);
                        int dsadmincnt = dsadmin.Tables[0].Rows.Count;
                        if (dsadmincnt > 0)
                        {
                            for (int a = 0; a <= dsadmincnt - 1; a++)
                            {
                                MailBody = "Dear Retail Admin User,<br/>";
                                MailBody = MailBody + dsMailSubBody.Tables[0].Rows[0]["MailBody"].ToString();
                                MailBody = MailBody + "<br/><br/>" + EMailUploadComplete(Request);
                                SendMail(MailBody + MailGetFooter(), MailSubject, EmailGetEmailIDsByUserID(Convert.ToInt64(dsadmin.Tables[0].Rows[a]["UserID"].ToString()), conn));
                                SaveCorrespondsData(Convert.ToInt64(RequestID), MailSubject, MailBody, DepartmentID, Status, conn);
                            }
                        }
                    }
                }
                else
                {

                    long TemplateID = 0;
                    GWC_SP_GetRequestHeadByRequestIDs_Result Request = new GWC_SP_GetRequestHeadByRequestIDs_Result();
                    Request = db.GWC_SP_GetRequestHeadByRequestIDs(RequestID.ToString()).FirstOrDefault();
                    long DepartmentID = long.Parse(Request.SiteID.ToString());
                    long Status = long.Parse(Request.Status.ToString());
                    string OrderNumber = Convert.ToString(Request.OrderNo);
                    DataSet dsAllApplLevel1 = new DataSet();
                    //get fulfillment user list
                    dsAllApplLevel1 = fillds(" select * from mApprovalLevelDetail AL left outer join muserprofilehead UP on UP.id=AL.userid   where AL.departmentid=" + DepartmentID + "    and UP.Usertype='Fulfilment'", conn);
                    int ApplCnt1 = dsAllApplLevel1.Tables[0].Rows.Count;

                    DataSet dsMailSubBody = new DataSet();
                    dsMailSubBody = fillds(" select * from mMessageEMailTemplates where ActivityID=(select Id from mDropdownValues where StatusID=39) and MessageID=(select Id from mDropdownValues where Value='Information') and DepartmentID=" + DepartmentID + " and Active='Yes'", conn);
                    if (dsMailSubBody.Tables[0].Rows.Count > 0)
                    {
                        TemplateID = long.Parse(dsMailSubBody.Tables[0].Rows[0]["ID"].ToString());
                        MailSubject = dsMailSubBody.Tables[0].Rows[0]["MailSubject"].ToString() + ", Order No # " + OrderNumber + "";
                        AdditionalDistribution(Convert.ToInt64(RequestID), TemplateID, conn);
                    }
                    if (dsDeliveryType.ToUpper() == "HOME")
                    {
                        for (int a = 0; a <= ApplCnt1 - 1; a++)
                        {
                            MailBody = "Dear Fulfilment Team,<br/>";
                            MailBody = MailBody + dsMailSubBody.Tables[0].Rows[0]["MailBody"].ToString();
                            MailBody = MailBody + "<br/><br/>" + EMailUploadComplete(Request);
                            SendMail(MailBody + MailGetFooter(), MailSubject, EmailGetEmailIDsByUserID(Convert.ToInt64(dsAllApplLevel1.Tables[0].Rows[a]["Userid"].ToString()), conn));
                            SaveCorrespondsData(Convert.ToInt64(RequestID), MailSubject, MailBody, DepartmentID, Status, conn);
                        }
                    }
                    //send email notification for Retail Admin                   
                    if (dsDeliveryType == "HUB")
                    {
                        DataSet dsadmin = new DataSet();
                        dsadmin = fillds(" select * from mUserTerritoryDetail UT  left outer join muserprofilehead UP on UP.id = UT.userid   where UT.territoryid = " + DepartmentID + "  and UP.Usertype = 'Retail User Admin'", conn);
                        int dsadmincnt = dsadmin.Tables[0].Rows.Count;
                        if (dsadmincnt > 0)
                        {
                            for (int a = 0; a <= dsadmincnt - 1; a++)
                            {
                                MailBody = "Dear Retail Admin User,<br/>";
                                MailBody = MailBody + dsMailSubBody.Tables[0].Rows[0]["MailBody"].ToString();
                                MailBody = MailBody + "<br/><br/>" + EMailUploadComplete(Request);
                                SendMail(MailBody + MailGetFooter(), MailSubject, EmailGetEmailIDsByUserID(Convert.ToInt64(dsadmin.Tables[0].Rows[a]["UserID"].ToString()), conn));
                                SaveCorrespondsData(Convert.ToInt64(RequestID), MailSubject, MailBody, DepartmentID, Status, conn);
                            }
                        }
                    }
                }
            }
            catch { }
            finally { }
        }
        protected string EMailUploadComplete(GWC_SP_GetRequestHeadByRequestIDs_Result Request)
        {
            string messageBody = "";

            DateTime date1 = new DateTime();
            date1 = Convert.ToDateTime(Request.OrderDate.Value.ToString());
            string OrderDate = date1.ToString("dd/MM/yyyy");

            DateTime date2 = new DateTime();
            date2 = Convert.ToDateTime(Request.Deliverydate.Value.ToString());
            string Deliverydate = date2.ToString("dd/MM/yyyy");

            messageBody = messageBody + "<font><b>Order Summary: </b></font>";
            string htmlTableStart = "<table style=\"border-collapse:collapse; text-align:center;\" >";
            string htmlTableEnd = "</table>";
            string htmlHeaderRowStart = "<tr style =\"background-color:#6FA1D2; color:#ffffff;\">";
            string htmlHeaderRowEnd = "</tr>";
            string htmlTrStart = "<tr style =\"color:black; text-align: center;\">";
            string htmlTrEnd = "</tr>";
            string htmlTdStart = "<td style=\" border-color:black; border-style:solid; border-width:thin; padding: 5px; text-align: center;font-size: 10pt;\">";
            string htmlTdEnd = "</td>";

            messageBody += htmlTableStart;
            messageBody += htmlHeaderRowStart;
            messageBody += htmlTdStart + "Order Date" + htmlTdEnd;
            messageBody += htmlTdStart + "Order Id" + htmlTdEnd;
            messageBody += htmlTdStart + "Exp. Delivery Date" + htmlTdEnd;
            messageBody += htmlTdStart + "Status" + htmlTdEnd;
            messageBody += htmlTdStart + "Remark" + htmlTdEnd;

            messageBody += htmlHeaderRowEnd;

            messageBody += htmlTrStart;
            // messageBody += htmlTdStart + " " + Request.OrderDate.Value.ToString("dd-MMM-yyyy") + " " + htmlTdEnd;
            // messageBody += htmlTdStart + " " + Request.OrderDate.Value.ToString("dd/mm/yyyy") + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + OrderDate + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Request.OrderNo.ToString() + " " + htmlTdEnd;
            //messageBody += htmlTdStart + " " + Request.Deliverydate.Value.ToString("dd/mm/yyyy") + " " + htmlTdEnd;
            // messageBody += htmlTdStart + " " + Request.Deliverydate.Value.ToString("dd-MMM-yyyy") + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Deliverydate + " " + htmlTdEnd;
            messageBody += htmlTdStart + " Delivered - Documents Uploaded " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Request.Remark + " " + htmlTdEnd;
            messageBody += htmlTrEnd;

            messageBody += htmlTableEnd;
            return messageBody;
        }


        public string GetUserNameByForUploadDocumentEmail(long storeid, string[] conn)
        {
            string result = "";
            DataSet ds = new DataSet();
            ds = fillds("select MUPH.FirstName,MUPH.LastName from mUserTerritoryDetail MUT inner join mUserProfileHead MUPH on MUT.UserID = MUPH.ID  and MUT.TerritoryID = " + storeid + " and MUPH.UserType = 'Fulfilment' ", conn);
            return result;
        }


        public string ISCheckPendingforAvtivation(string OrderID, string Skucode, string[] conn)
        {
            string result = "";
            DataSet ds = new DataSet();
            ds = fillds("select * from tOrderDispatchStatus where OrderNo=" + OrderID + " and skucode='" + Skucode + "' and PendingForDocument='red'", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                result = "Yes";
            }
            else
            {
                result = "No";
            }
            return result;
        }


        public string GetOrderNo(long OrderID, string[] conn)
        {
            string result = "";
            DataSet ds = new DataSet();
            ds = fillds("select OrderNo from tOrderHead where id=" + OrderID + "", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                result = ds.Tables[0].Rows[0]["OrderNo"].ToString();
            }
            return result;
        }


        public string ISCompleteOrderOrNot(string OrderID, string Skucode, string[] conn)
        {
            string result = "";
            DataSet ds = new DataSet();
            ds = fillds("select * from tOrderDispatchStatus where OrderNo=" + OrderID + " and skucode='" + Skucode + "' and UploadDelivered='red'", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                result = "Yes";
            }
            else
            {
                result = "No";
            }
            return result;
        }

        public string UpdateCompletedOrder(long Orderno, string skucode, string[] conn)
        {
            string result = "", Deliverytype = "";
            DataSet dsdispatch = new DataSet();
            dsdispatch = fillds(" select Deliverytype from tecommhead where orderid=" + Orderno + "  ", conn);
            if (dsdispatch.Tables[0].Rows.Count > 0)
            {
                Deliverytype = dsdispatch.Tables[0].Rows[0]["Deliverytype"].ToString();
            }


            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_UpdateDispatchActivity";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@PendingForDriverAllocation", "green");
            cmd.Parameters.AddWithValue("@OrderNo", Orderno);
            cmd.Parameters.AddWithValue("@skucode", skucode);
            cmd.Parameters.AddWithValue("@Flag", "UpdateCompletedOrder");
            cmd.ExecuteNonQuery();
            result = "true";
            return result;
        }


        public string UpdateMSISDN(long Id, string MSSIDN, string Skucode, string[] conn)
        {
            string result = "";
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_UpdateDispatchActivity";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@Id", Id);
            cmd.Parameters.AddWithValue("@MSSIDN", MSSIDN);
            cmd.Parameters.AddWithValue("@skucode", Skucode);
            cmd.Parameters.AddWithValue("@Flag", "UpdateMSISDN");
            cmd.ExecuteNonQuery();
            result = "true";
            return result;
        }


        public DataSet BindReasonCodeDDL(string ordno, string[] conn)
        {
            DataSet ds1 = new DataSet();
            DataSet ds = new DataSet();
            ds1 = fillds("select StoreId from tOrderHead where id=" + ordno + "", conn);
            if (ds1.Tables[0].Rows.Count > 0)
            {
                string deptid = ds1.Tables[0].Rows[0]["StoreId"].ToString();
                ds = fillds("select Id,Reasondetails from mReasonCode where DeptId=" + deptid + " and type='Rejection' and Active='Yes' order by ReasonCode", conn);
            }
            return ds;
        }

        public DataSet BindReasonCodeDDLForCancel(string ordno, string[] conn)
        {
            DataSet ds1 = new DataSet();
            ds1 = fillds("select StoreId from tOrderHead where id=" + ordno + "", conn);
            string deptid = ds1.Tables[0].Rows[0]["StoreId"].ToString();

            DataSet ds = new DataSet();
            ds = fillds("select Id,ReasonCode from mReasonCode where DeptId=" + deptid + " and  type='Cancellation' and Active='Yes' order by ReasonCode", conn);
            return ds;
        }


        public string GetOrderType(long Id, string skucode, int packageid, string[] conn)
        {
            string result = "";
            DataSet ds1 = new DataSet();
            // ds1 = fillds("select ordertype from tecommhead where orderid=" + Id + "", conn);
            ds1 = fillds("select * from tOrderDispatchStatus where orderno=" + Id + " and skucode='" + skucode + "' and packageid=" + packageid + "", conn);
            if (ds1.Tables[0].Rows.Count > 0)
            {
                // result = ds1.Tables[0].Rows[0]["skucode"].ToString();PackageType
                result = ds1.Tables[0].Rows[0]["PackageType"].ToString();
            }
            return result;
        }


        public DataSet GetOrderDetails(long Id, string[] conn)
        {
            DataSet ds1 = new DataSet();
            ds1 = fillds("   select  * from torderhead toh left outer join tECommHead teh on toh.Id=teh.OrderID where toh.Id=" + Id + "", conn);
            return ds1;
        }


        public string ChkBarcode(long Id, string[] conn)
        {
            string result = "No";
            DataSet ds1 = new DataSet();
            ds1 = fillds("select * from mdeliveryBarcode where OrderId=" + Id + "", conn);
            if (ds1.Tables[0].Rows.Count > 0)
            {
                result = "Yes";
            }
            return result;
        }


        public void InsertintomDeliverybarCode(long Id, byte[] ono, byte[] amtpaid, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_InsertintomDeliverybarCode";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@OrderID", Id);
            cmd.Parameters.AddWithValue("@OrderNoBarCode", ono);
            cmd.Parameters.AddWithValue("@AmtPaidBarCode", amtpaid);
            cmd.ExecuteNonQuery();

        }

        #endregion
        #region Show Normal order Info

      
        public DataSet GetNormalOrderDetails(long Id, string[] conn)
        {
            DataSet ds1 = new DataSet();
            ds1 = fillds("select  * from torderhead  where Id=" + Id + "", conn);
            return ds1;
        }

        public string ChkNormalOrderBarcode(long Id, string[] conn)
        {
            string result = "No";
            DataSet ds1 = new DataSet();
            ds1 = fillds("select * from mdeliveryBarcode where OrderId=" + Id + "", conn);
            if (ds1.Tables[0].Rows.Count > 0)
            {
                result = "Yes";
            }
            return result;
        }
        public void InsertintoNormalOrdermDeliverybarCode(long Id, byte[] ono, byte[] amtpaid, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_InsertintomDeliverybarCode";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@OrderID", Id);
            cmd.Parameters.AddWithValue("@OrderNoBarCode", ono);
            cmd.Parameters.AddWithValue("@AmtPaidBarCode", amtpaid);
            cmd.ExecuteNonQuery();

        }

        //change by suraj khopade
        #endregion

        #region OrderParameter

        public DataSet BindGVOrderParameter(long OrderId, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from tECommOrderPara TEO left outer join tECommHead TEH on TEO.EcomHeadID=TEH.ID left outer join tOrderHead TOH on TOH.Id = TEH.OrderID where TOH.id = " + OrderId + "", conn);
            return ds;
        }

        #endregion




        #region Cust analytics

        public DataSet GetCustomerAnalyticsData(string orderno, string[] conn)
        {
            string qid = "", passportid = "";
            DataSet ds = new DataSet();
            ds = fillds("select * from tECommHead where OrderID=" + orderno + " ", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                qid = ds.Tables[0].Rows[0]["KahramaaID"].ToString();
                passportid = ds.Tables[0].Rows[0]["Idt"].ToString();
            }
            DataSet ds1 = new DataSet();
            ds1 = fillds("select  top(10) TEC.CustomerFirstName +' '+TEC.CustomerLastName As CustNM,TOH.OrderNo,TEC.KahramaaID,TEC.Idt,TEC.OrderCreationDate,MS.Status,TEC.IdType from tECommHead TEC left outer join tOrderHead TOH on TEC.OrderID=TOH.Id left outer join mStatus MS on MS.ID=TOH.Status  where TEC.KahramaaID like '%" + qid + "%' and TEC.Idt like '%" + passportid + "%' order by OrderCreationDate desc ", conn);

            return ds1;
        }

        #endregion


        #region Advancesearch
        //public DataSet GetRequestSearch(string fdate, string todate, string ordcat, string ordno, string lcode, string passport, string ordertype, string misidn, string email, long UserID, string semserial, string paymenttype, string[] conn)
        //{
        //    DataSet dsustype = new DataSet();
        //    dsustype = fillds("select UserType from mUserProfileHead where id=" + UserID + "", conn);
        //    string usertype = dsustype.Tables[0].Rows[0]["UserType"].ToString();
        //    DataSet ds = new DataSet();
        //    if (ordertype == "0" || ordertype == "") { ordertype = ""; }
        //    if (ordno == "0" || ordno == "") { ordno = ""; }
        //    if (passport == "0" || passport == "") { passport = ""; }
        //    if (ordcat == "0" || ordcat == "") { ordcat = ""; }
        //    if (email == "0" || email == "") { email = ""; }
        //    if (semserial == "0" || semserial == "") { semserial = ""; }
        //    if (paymenttype == "0" || paymenttype == "") { paymenttype = ""; }

        //    StringBuilder sb = new StringBuilder();
        //    //sb.AppendLine(" declare  @Fdate datetime set @Fdate='" + fdate + "' declare @Tdate datetime;set @Tdate='" + todate + "' ");
        //    //sb.AppendLine(" SELECT  DISTINCT  tPR.Id AS ID, mT.Territory AS SiteName, mT.ID AS SiteID, tPR.OrderNumber, tPR.Orderdate, tPR.Title, tPR.Priority, tPR.RequestBy, dbo.FS_GetUserNameByUserID(tPR.RequestBy) AS RequestByUserName, tPR.Status,  ");
        //    //sb.AppendLine(" mS.Status AS RequestStatus, tEC.OrderCategory, tEC.OrderType, tEC.CustomerFirstName, tEC.CustomerLastName, tEC.DeliveryType, tEC.AmountPaid, tEC.TotalPrice, tEC.AmountToBeCollected, mPM.MethodName, ");
        //    //sb.AppendLine(" CASE WHEN mS.ID = 1 THEN 'red' WHEN mS.ID = 28 THEN 'orange' ELSE 'green' END AS ImgRequest, CASE WHEN mS.ID = 1 THEN 'gray' WHEN mS.ID IN (2, 21, 22, 31, 32) THEN 'red' WHEN mS.ID IN (4, 28)  ");
        //    //sb.AppendLine(" THEN 'orange' WHEN mS.ID NOT IN (1, 2, 21, 22, 31, 32) THEN 'green' END AS ImgApproval, CASE WHEN mS.ID IN (1, 2, 4, 21, 22, 31, 32) THEN 'gray' WHEN mS.ID IN (3, 5, 25, 34, 35, 37, 38, 39, 10034)  ");
        //    //sb.AppendLine(" THEN 'red' WHEN mS.ID IN (6) THEN 'greenRed' WHEN mS.ID IN (10, 26, 28) THEN 'orange' WHEN mS.ID IN (7, 8) THEN 'green' END AS ImgIssue, CASE WHEN mS.ID IN (1, 2, 3, 4, 5) THEN 'gray' WHEN mS.ID IN (6, 7)  ");
        //    //sb.AppendLine(" THEN 'red' WHEN mS.ID IN (8) THEN 'green' END AS ImgReceipt, CASE WHEN mS.ID IN (1, 2, 3, 4, 5, 6, 7) THEN 'gray' WHEN mS.ID IN (8) THEN 'red' WHEN mS.ID IN (9) THEN 'green' END AS ImgConsumption,  ");
        //    //sb.AppendLine(" tPR.Deliverydate, tPR.OrderNo, tEC.Idt, TA.LocationCode, tEC.EcomOrderNumber, tEC.Email,tEC.Deliverytype ");
        //    //if (string.IsNullOrEmpty(misidn))
        //    //{ }
        //    //else
        //    //{ sb.AppendLine("  , tED.ProductName "); }         

        //    //sb.AppendLine(" FROM dbo.tOrderHead AS tPR INNER JOIN  dbo.mTerritory AS mT ON tPR.StoreId = mT.ID INNER JOIN dbo.mStatus AS mS ON tPR.Status = mS.ID LEFT OUTER JOIN dbo.mCustomerDetail AS CD ON tPR.Id = CD.OrderID LEFT OUTER JOIN ");
        //    //sb.AppendLine(" dbo.tECommHead AS tEC ON tEC.OrderID = tPR.Id LEFT OUTER JOIN    dbo.mPaymentMethodMain AS mPM ON tPR.PaymentID = mPM.ID LEFT OUTER JOIN    dbo.tAddress AS TA ON TA.ID = tPR.LocationID ");
        //    //if (string.IsNullOrEmpty(misidn))
        //    //{ }
        //    //else
        //    //{ sb.AppendLine(" LEFT OUTER JOIN   dbo.tECommDetail AS tED ON tED.EcomHeadID = tEC.ID  "); }         
        //    // sb.AppendLine(" WHERE (tPR.orderType IN ('PreOrder', 'NormalOrder')) and  tEC.orderType like  '%" + ordertype + "%'  and OrderNo like '%" + ordno + "%'   and  Idt like '%" + passport + "%'  and OrderCategory like  '%" + ordcat + "%'   and email like '%" + email + "%' ");
        //    //if (string.IsNullOrEmpty(misidn))
        //    //{ }
        //    //else
        //    //{ sb.AppendLine(" and  ProductName like'%" + misidn + "%'  "); }    
        //    //if (usertype == "Retail User" || usertype == "Retail User Admin")
        //    //{
        //    //    sb.AppendLine("  and tEC.Deliverytype='Hub' ");
        //    //}           
        //    //sb.AppendLine("  and  convert(varchar(10) ,OrderDate , 111)  >= convert(varchar(10) ,@fdate , 111)    and  convert(varchar(10) ,OrderDate , 111)  <= convert(varchar(10) ,@Tdate , 111) order by id desc ");
        //    //sb.AppendLine(" ");
        //    sb.AppendLine(" declare  @Fdate datetime set @Fdate='" + fdate + "' declare @Tdate datetime;set @Tdate='" + todate + "' ");
        //    sb.AppendLine(" SELECT  DISTINCT  tPR.Id AS ID, mT.Territory AS SiteName, mT.ID AS SiteID, tPR.OrderNumber, tPR.Orderdate, tPR.Title, tPR.Priority, tPR.RequestBy, dbo.FS_GetUserNameByUserID(tPR.RequestBy) AS RequestByUserName, tPR.Status,  ");
        //    sb.AppendLine(" mS.Status AS RequestStatus, tEC.OrderCategory, tEC.OrderType, tEC.CustomerFirstName, tEC.CustomerLastName, tEC.DeliveryType, tEC.AmountPaid, tEC.TotalPrice, tEC.AmountToBeCollected, mPM.MethodName, ");
        //    sb.AppendLine(" CASE WHEN mS.ID = 1 THEN 'red' WHEN mS.ID = 28 THEN 'orange' ELSE 'green' END AS ImgRequest, CASE WHEN mS.ID = 1 THEN 'gray' WHEN mS.ID IN (2, 21, 22, 31, 32) THEN 'red' WHEN mS.ID IN (4, 28)  ");
        //    sb.AppendLine(" THEN 'orange' WHEN mS.ID NOT IN (1, 2, 21, 22, 31, 32) THEN 'green' END AS ImgApproval, CASE WHEN mS.ID IN (1, 2, 4, 21, 22, 31, 32) THEN 'gray' WHEN mS.ID IN (3,5,7,8,25,33,34,35,36,37,38)  ");
        //    sb.AppendLine(" THEN 'red' WHEN mS.ID IN (6) THEN 'greenRed' WHEN mS.ID IN (10, 26, 28) THEN 'orange' WHEN mS.ID IN (39) THEN 'green' END AS ImgIssue, CASE WHEN mS.ID IN (1, 2, 3, 4, 5) THEN 'gray' WHEN mS.ID IN (6, 7)  ");
        //    sb.AppendLine(" THEN 'red' WHEN mS.ID IN (8) THEN 'green' END AS ImgReceipt, CASE WHEN mS.ID IN (1, 2, 3, 4, 5, 6, 7) THEN 'gray' WHEN mS.ID IN (8) THEN 'red' WHEN mS.ID IN (9) THEN 'green' END AS ImgConsumption,  ");
        //    sb.AppendLine(" tPR.Deliverydate, tPR.OrderNo, tEC.Idt, TA.LocationCode, tEC.EcomOrderNumber, tEC.Email,tPR.PaymentID ");
        //    if (string.IsNullOrEmpty(misidn))
        //    { }
        //    else
        //    { sb.AppendLine("  , tED.ProductName "); }

        //    if (string.IsNullOrEmpty(semserial)) { }
        //    else
        //    {
        //        sb.AppendLine("  ,Tods.MSSIDN ");
        //    }

        //    sb.AppendLine(" FROM dbo.tOrderHead AS tPR INNER JOIN  dbo.mTerritory AS mT ON tPR.StoreId = mT.ID INNER JOIN dbo.mStatus AS mS ON tPR.Status = mS.ID LEFT OUTER JOIN dbo.mCustomerDetail AS CD ON tPR.Id = CD.OrderID LEFT OUTER JOIN ");
        //    sb.AppendLine(" dbo.tECommHead AS tEC ON tEC.OrderID = tPR.Id LEFT OUTER JOIN    dbo.mPaymentMethodMain AS mPM ON tPR.PaymentID = mPM.ID LEFT OUTER JOIN    dbo.tAddress AS TA ON TA.ID = tPR.LocationID ");
        //    if (string.IsNullOrEmpty(misidn))
        //    { }
        //    else
        //    { sb.AppendLine(" LEFT OUTER JOIN   dbo.tECommDetail AS tED ON tED.EcomHeadID = tEC.ID  "); }

        //    if (string.IsNullOrEmpty(semserial)) { }
        //    else
        //    {
        //        sb.AppendLine("  left outer join   tOrderDispatchStatus as Tods on  Tods.orderno=tPR.id ");
        //    }
        //    sb.AppendLine(" WHERE (tPR.orderType IN ('PreOrder', 'NormalOrder')) and  tEC.orderType like  '%" + ordertype + "%'  and tPR.OrderNo like '%" + ordno + "%'   and  Idt like '%" + passport + "%'  and OrderCategory like  '%" + ordcat + "%'   and email like '%" + email + "%'  and   tPR.PaymentID like'%" + paymenttype + "%' ");
        //    if (string.IsNullOrEmpty(misidn))
        //    { }
        //    else
        //    { sb.AppendLine(" and  ProductName like'%" + misidn + "%'  "); }
        //    if (usertype == "Retail User" || usertype == "Retail User Admin")
        //    {

        //        sb.AppendLine("  and tEC.Deliverytype='Hub' ");
        //    }
        //    if (string.IsNullOrEmpty(semserial)) { }
        //    else
        //    {
        //        sb.AppendLine(" and  Tods.MSSIDN  like'%" + semserial + "%'  ");
        //    }

        //    sb.AppendLine("  and  convert(varchar(10) ,OrderDate , 111)  >= convert(varchar(10) ,@fdate , 111)    and  convert(varchar(10) ,OrderDate , 111)  <= convert(varchar(10) ,@Tdate , 111) order by id desc ");

        //    ds = fillds(Convert.ToString(sb), conn);
        //    return ds;
        //}


        //public DataSet GetRequestSummayOfRetailUserSearch(string fdate, string todate, string ordcat, string ordno, string lcode, string passport, string ordertype, string misidn, string email, long UserID, string semserial, string paymenttype, string[] conn)
        //{
        //    DataSet dsustype = new DataSet();
        //    dsustype = fillds("select UserType from mUserProfileHead where id=" + UserID + "", conn);
        //    string usertype = dsustype.Tables[0].Rows[0]["UserType"].ToString();
        //    DataSet ds = new DataSet();
        //    if (ordertype == "0" || ordertype == "") { ordertype = ""; }
        //    if (ordno == "0" || ordno == "") { ordno = ""; }
        //    if (passport == "0" || passport == "") { passport = ""; }
        //    if (ordcat == "0" || ordcat == "") { ordcat = ""; }
        //    if (email == "0" || email == "") { email = ""; }
        //    if (semserial == "0" || semserial == "") { semserial = ""; }
        //    if (paymenttype == "0" || paymenttype == "") { paymenttype = ""; }

        //    StringBuilder sb = new StringBuilder();
        //    // sb.AppendLine(" declare  @Fdate datetime set @Fdate='" + fdate + "' declare @Tdate datetime;set @Tdate='" + todate + "' ");
        //    //sb.AppendLine(" SELECT  DISTINCT  tPR.Id AS ID, mT.Territory AS SiteName, mT.ID AS SiteID, tPR.OrderNumber, tPR.Orderdate, tPR.Title, tPR.Priority, tPR.RequestBy, dbo.FS_GetUserNameByUserID(tPR.RequestBy) AS RequestByUserName, tPR.Status,  ");
        //    //sb.AppendLine(" mS.Status AS RequestStatus, tEC.OrderCategory, tEC.OrderType, tEC.CustomerFirstName, tEC.CustomerLastName, tEC.DeliveryType, tEC.AmountPaid, tEC.TotalPrice, tEC.AmountToBeCollected, mPM.MethodName, ");
        //    //sb.AppendLine(" CASE WHEN mS.ID = 1 THEN 'red' WHEN mS.ID = 28 THEN 'orange' ELSE 'green' END AS ImgRequest, CASE WHEN mS.ID = 1 THEN 'gray' WHEN mS.ID IN (2, 21, 22, 31, 32) THEN 'red' WHEN mS.ID IN (4, 28)  ");
        //    //sb.AppendLine(" THEN 'orange' WHEN mS.ID NOT IN (1, 2, 21, 22, 31, 32) THEN 'green' END AS ImgApproval, CASE WHEN mS.ID IN (1, 2, 4, 21, 22, 31, 32) THEN 'gray' WHEN mS.ID IN (3, 5, 25, 34, 35, 37, 38, 39, 10034)  ");
        //    //sb.AppendLine(" THEN 'red' WHEN mS.ID IN (6) THEN 'greenRed' WHEN mS.ID IN (10, 26, 28) THEN 'orange' WHEN mS.ID IN (7, 8) THEN 'green' END AS ImgIssue, CASE WHEN mS.ID IN (1, 2, 3, 4, 5) THEN 'gray' WHEN mS.ID IN (6, 7)  ");
        //    //sb.AppendLine(" THEN 'red' WHEN mS.ID IN (8) THEN 'green' END AS ImgReceipt, CASE WHEN mS.ID IN (1, 2, 3, 4, 5, 6, 7) THEN 'gray' WHEN mS.ID IN (8) THEN 'red' WHEN mS.ID IN (9) THEN 'green' END AS ImgConsumption,  ");
        //    //sb.AppendLine(" tPR.Deliverydate, tPR.OrderNo, tEC.Idt, TA.LocationCode, tEC.EcomOrderNumber, tEC.Email,tEC.Deliverytype ");
        //    //if (string.IsNullOrEmpty(misidn))
        //    //{ }
        //    //else
        //    //{ sb.AppendLine("  , tED.ProductName "); }

        //    //sb.AppendLine(" FROM dbo.tOrderHead AS tPR INNER JOIN  dbo.mTerritory AS mT ON tPR.StoreId = mT.ID INNER JOIN dbo.mStatus AS mS ON tPR.Status = mS.ID LEFT OUTER JOIN dbo.mCustomerDetail AS CD ON tPR.Id = CD.OrderID LEFT OUTER JOIN ");
        //    //sb.AppendLine(" dbo.tECommHead AS tEC ON tEC.OrderID = tPR.Id LEFT OUTER JOIN    dbo.mPaymentMethodMain AS mPM ON tPR.PaymentID = mPM.ID LEFT OUTER JOIN    dbo.tAddress AS TA ON TA.ID = tPR.LocationID ");
        //    //if (string.IsNullOrEmpty(misidn))
        //    //{ }
        //    //else
        //    //{ sb.AppendLine(" LEFT OUTER JOIN   dbo.tECommDetail AS tED ON tED.EcomHeadID = tEC.ID  "); }
        //    //sb.AppendLine(" WHERE (tPR.orderType IN ('PreOrder', 'NormalOrder')) and  tEC.orderType like  '%" + ordertype + "%'  and OrderNo like '%" + ordno + "%'   and  Idt like '%" + passport + "%'  and OrderCategory like  '%" + ordcat + "%'   and email like '%" + email + "%' ");
        //    //if (string.IsNullOrEmpty(misidn))
        //    //{ }
        //    //else
        //    //{ sb.AppendLine(" and  ProductName like'%" + misidn + "%'  "); }

        //    //{
        //    //    sb.AppendLine("  and tEC.Deliverytype='Hub' ");
        //    //}
        //    //sb.AppendLine("  and  convert(varchar(10) ,OrderDate , 111)  >= convert(varchar(10) ,@fdate , 111)    and  convert(varchar(10) ,OrderDate , 111)  <= convert(varchar(10) ,@Tdate , 111) order by id desc ");

        //    sb.AppendLine(" declare  @Fdate datetime set @Fdate='" + fdate + "' declare @Tdate datetime;set @Tdate='" + todate + "' ");
        //    sb.AppendLine(" SELECT  DISTINCT  tPR.Id AS ID, mT.Territory AS SiteName, mT.ID AS SiteID, tPR.OrderNumber, tPR.Orderdate, tPR.Title, tPR.Priority, tPR.RequestBy, dbo.FS_GetUserNameByUserID(tPR.RequestBy) AS RequestByUserName, tPR.Status,  ");
        //    sb.AppendLine(" mS.Status AS RequestStatus, tEC.OrderCategory, tEC.OrderType, tEC.CustomerFirstName, tEC.CustomerLastName, tEC.DeliveryType, tEC.AmountPaid, tEC.TotalPrice, tEC.AmountToBeCollected, mPM.MethodName, ");
        //    sb.AppendLine(" CASE WHEN mS.ID = 1 THEN 'red' WHEN mS.ID = 28 THEN 'orange' ELSE 'green' END AS ImgRequest, CASE WHEN mS.ID = 1 THEN 'gray' WHEN mS.ID IN (2, 21, 22, 31, 32) THEN 'red' WHEN mS.ID IN (4, 28)  ");
        //    sb.AppendLine(" THEN 'orange' WHEN mS.ID NOT IN (1, 2, 21, 22, 31, 32) THEN 'green' END AS ImgApproval, CASE WHEN mS.ID IN (1, 2, 4, 21, 22, 31, 32) THEN 'gray' WHEN mS.ID IN (3,5,7,8,25,33,34,35,36,37,38)  ");
        //    sb.AppendLine(" THEN 'red' WHEN mS.ID IN (6) THEN 'greenRed' WHEN mS.ID IN (10, 26, 28) THEN 'orange' WHEN mS.ID IN (39) THEN 'green' END AS ImgIssue, CASE WHEN mS.ID IN (1, 2, 3, 4, 5) THEN 'gray' WHEN mS.ID IN (6, 7)  ");
        //    sb.AppendLine(" THEN 'red' WHEN mS.ID IN (8) THEN 'green' END AS ImgReceipt, CASE WHEN mS.ID IN (1, 2, 3, 4, 5, 6, 7) THEN 'gray' WHEN mS.ID IN (8) THEN 'red' WHEN mS.ID IN (9) THEN 'green' END AS ImgConsumption,  ");
        //    sb.AppendLine(" tPR.Deliverydate, tPR.OrderNo, tEC.Idt, TA.LocationCode, tEC.EcomOrderNumber, tEC.Email,tPR.PaymentID ");
        //    if (string.IsNullOrEmpty(misidn))
        //    { }
        //    else
        //    { sb.AppendLine("  , tED.ProductName "); }

        //    if (string.IsNullOrEmpty(semserial)) { }
        //    else
        //    {
        //        sb.AppendLine("  ,Tods.MSSIDN ");
        //    }

        //    sb.AppendLine(" FROM dbo.tOrderHead AS tPR INNER JOIN  dbo.mTerritory AS mT ON tPR.StoreId = mT.ID INNER JOIN dbo.mStatus AS mS ON tPR.Status = mS.ID LEFT OUTER JOIN dbo.mCustomerDetail AS CD ON tPR.Id = CD.OrderID LEFT OUTER JOIN ");
        //    sb.AppendLine(" dbo.tECommHead AS tEC ON tEC.OrderID = tPR.Id LEFT OUTER JOIN    dbo.mPaymentMethodMain AS mPM ON tPR.PaymentID = mPM.ID LEFT OUTER JOIN    dbo.tAddress AS TA ON TA.ID = tPR.LocationID ");
        //    if (string.IsNullOrEmpty(misidn))
        //    { }
        //    else
        //    { sb.AppendLine(" LEFT OUTER JOIN   dbo.tECommDetail AS tED ON tED.EcomHeadID = tEC.ID  "); }

        //    if (string.IsNullOrEmpty(semserial)) { }
        //    else
        //    {
        //        sb.AppendLine("  left outer join   tOrderDispatchStatus as Tods on  Tods.orderno=tPR.id ");
        //    }
        //    sb.AppendLine(" WHERE (tPR.orderType IN ('PreOrder', 'NormalOrder')) and  tEC.orderType like  '%" + ordertype + "%'  and tPR.OrderNo like '%" + ordno + "%'   and  Idt like '%" + passport + "%'  and OrderCategory like  '%" + ordcat + "%'   and email like '%" + email + "%'  and   tPR.PaymentID like'%" + paymenttype + "%' ");
        //    if (string.IsNullOrEmpty(misidn))
        //    { }
        //    else
        //    { sb.AppendLine(" and  ProductName like'%" + misidn + "%'  "); }
        //    if (usertype == "Retail User" || usertype == "Retail User Admin")
        //    {

        //        sb.AppendLine("  and tEC.Deliverytype='Hub' ");
        //    }
        //    if (string.IsNullOrEmpty(semserial)) { }
        //    else
        //    {
        //        sb.AppendLine(" and  Tods.MSSIDN  like'%" + semserial + "%'  ");
        //    }
        //    sb.AppendLine("  and  convert(varchar(10) ,OrderDate , 111)  >= convert(varchar(10) ,@fdate , 111)    and  convert(varchar(10) ,OrderDate , 111)  <= convert(varchar(10) ,@Tdate , 111) order by id desc ");
        //    ds = fillds(Convert.ToString(sb), conn);
        //    return ds;
        //}


        public DataSet GetRequestSearch(string fdate, string todate, string ordcat, string ordno, string lcode, string passport, string ordertype, string misidn, string email, long UserID, string semserial, string paymenttype, string[] conn)
        {
            DataSet dsustype = new DataSet();
            dsustype = fillds("select UserType from mUserProfileHead where id=" + UserID + "", conn);
            string usertype = dsustype.Tables[0].Rows[0]["UserType"].ToString();
            DataSet ds = new DataSet();
            if (ordertype == "0" || ordertype == "") { ordertype = ""; }
            if (ordno == "0" || ordno == "") { ordno = ""; }
            if (passport == "0" || passport == "") { passport = ""; }
            if (ordcat == "0" || ordcat == "") { ordcat = ""; }
            if (email == "0" || email == "") { email = ""; }
            if (semserial == "0" || semserial == "") { semserial = ""; }
            if (paymenttype == "0" || paymenttype == "") { paymenttype = ""; }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(" declare  @Fdate datetime set @Fdate='" + fdate + "' declare @Tdate datetime;set @Tdate='" + todate + "' ");
            sb.AppendLine(" SELECT  DISTINCT  tPR.Id AS ID, mT.Territory AS SiteName, mT.ID AS SiteID, case when tPR.OrderNumber='' then 'NA' else isnull(tPR.OrderNumber,'NA')   end OrderNumber, tPR.Orderdate, tPR.Title, tPR.Priority, tPR.RequestBy, dbo.FS_GetUserNameByUserID(tPR.RequestBy) AS RequestByUserName, tPR.Status,  ");
            sb.AppendLine(" mS.Status AS RequestStatus, isnull(tEC.OrderCategory,'NA')OrderCategory, isnull(tEC.Ordertype,'NA')Ordertype,isnull(tEC.CustomerFirstName,'NA')CustomerFirstName, isnull(tEC.CustomerLastName,'NA')CustomerLastName,isnull(tEC.DeliveryType,'NA')DeliveryType,isnull(tEC.AmountPaid,0)AmountPaid,isnull(tEC.TotalPrice,0)TotalPrice,isnull(tEC.AmounttobeCollected,0)AmounttobeCollected, mPM.MethodName,isnull(tEC.VIPCustomer,'N') as VIPCustomer,");
            sb.AppendLine(" CASE WHEN mS.ID = 1 THEN 'red' WHEN mS.ID = 28 THEN 'orange' ELSE 'green' END AS ImgRequest, CASE WHEN mS.ID = 1 THEN 'gray' WHEN mS.ID IN (2, 21, 22, 31, 32) THEN 'red' WHEN mS.ID IN (4, 28)  ");
            sb.AppendLine(" THEN 'orange' WHEN mS.ID NOT IN (1, 2, 21, 22, 31, 32) THEN 'green' END AS ImgApproval, CASE WHEN mS.ID IN (1, 2, 4, 21, 22, 31, 32) THEN 'gray' WHEN mS.ID IN (3,5,7,8,25,34,35,37,38,39,10034)  ");
            sb.AppendLine(" THEN 'red' WHEN mS.ID IN (6) THEN 'greenRed' WHEN mS.ID IN (10, 26, 28,10037) THEN 'orange' WHEN mS.ID IN (10036) THEN 'green' END AS ImgIssue, CASE WHEN mS.ID IN (1, 2, 3, 4, 5) THEN 'gray' WHEN mS.ID IN (6, 7)  ");
            sb.AppendLine(" THEN 'red' WHEN mS.ID IN (8) THEN 'green' END AS ImgReceipt, CASE WHEN mS.ID IN (1, 2, 3, 4, 5, 6, 7) THEN 'gray' WHEN mS.ID IN (8) THEN 'red' WHEN mS.ID IN (9) THEN 'green' END AS ImgConsumption,  ");
            sb.AppendLine(" tPR.Deliverydate, tPR.OrderNo, tEC.Idt, TA.LocationCode, tEC.EcomOrderNumber, tEC.Email,tPR.PaymentID ");
            if (string.IsNullOrEmpty(misidn))
            { }
            else
            { sb.AppendLine("  , tED.ProductName "); }

            if (string.IsNullOrEmpty(semserial)) { }
            else
            {
                sb.AppendLine("  ,Tods.MSSIDN ");
            }

            sb.AppendLine(" FROM dbo.tOrderHead AS tPR INNER JOIN  dbo.mTerritory AS mT ON tPR.StoreId = mT.ID INNER JOIN dbo.mStatus AS mS ON tPR.Status = mS.ID LEFT OUTER JOIN dbo.mCustomerDetail AS CD ON tPR.Id = CD.OrderID LEFT OUTER JOIN ");
            sb.AppendLine(" dbo.tECommHead AS tEC ON tEC.OrderID = tPR.Id LEFT OUTER JOIN    dbo.mPaymentMethodMain AS mPM ON tPR.PaymentID = mPM.ID LEFT OUTER JOIN    dbo.tAddress AS TA ON TA.ID = tPR.LocationID ");
            if (string.IsNullOrEmpty(misidn))
            { }
            else
            { sb.AppendLine(" LEFT OUTER JOIN   dbo.tECommDetail AS tED ON tED.EcomHeadID = tEC.ID  "); }

            if (string.IsNullOrEmpty(semserial)) { }
            else
            {
                sb.AppendLine("  left outer join   tOrderDispatchStatus as Tods on  Tods.orderno=tPR.id ");
            }
            sb.AppendLine(" WHERE tPR.orderType in  ('PreOrder','NormalOrder','Direct Import') and mt.ecommerce=1 and  isnull(tEC.orderType,'Na') like  '%" + ordertype + "%'  and  isnull(tPR.OrderNo,'NA') like '%" + ordno + "%'   and  isnull(tEC.Idt,'NA') like '%" + passport + "%'  and isnull(tEC.OrderCategory,'NA') like  '%" + ordcat + "%'   and isnull(email,'NA') like '%" + email + "%'  and  isnull(tPR.PaymentID,0) like'%" + paymenttype + "%' ");
            if (string.IsNullOrEmpty(misidn))
            { }
            else
            { sb.AppendLine(" and  isnull(ProductName,'NA') like'%" + misidn + "%'  "); }
            if (usertype == "Retail User" || usertype == "Retail User Admin")
            {

                sb.AppendLine("  and tEC.Deliverytype='Hub' ");
            }
            if (string.IsNullOrEmpty(semserial)) { }
            else
            {
                sb.AppendLine(" and  isnull(Tods.MSSIDN,'NA')  like'%" + semserial + "%'  ");
            }
            sb.AppendLine("  and  convert(date ,OrderDate )  >= convert(date ,@fdate )    and  convert(date ,OrderDate)  <= convert(date ,@Tdate ) order by id desc ");

            ds = fillds(Convert.ToString(sb), conn);
            return ds;
        }

        //for test project
        public DataSet GetRequestSummayOfRetailUserSearch(string fdate, string todate, string ordcat, string ordno, string lcode, string passport, string ordertype, string misidn, string email, long UserID, string semserial, string paymenttype, string[] conn)
        {
            DataSet dsustype = new DataSet();
            dsustype = fillds("select UserType from mUserProfileHead where id=" + UserID + "", conn);
            string usertype = dsustype.Tables[0].Rows[0]["UserType"].ToString();
            DataSet ds = new DataSet();
            if (ordertype == "0" || ordertype == "") { ordertype = ""; }
            if (ordno == "0" || ordno == "") { ordno = ""; }
            if (passport == "0" || passport == "") { passport = ""; }
            if (ordcat == "0" || ordcat == "") { ordcat = ""; }
            if (email == "0" || email == "") { email = ""; }
            if (semserial == "0" || semserial == "") { semserial = ""; }
            if (paymenttype == "0" || paymenttype == "") { paymenttype = ""; }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(" declare  @Fdate datetime set @Fdate='" + fdate + "' declare @Tdate datetime;set @Tdate='" + todate + "' ");
            sb.AppendLine(" SELECT  DISTINCT  tPR.Id AS ID, mT.Territory AS SiteName, mT.ID AS SiteID, case when tPR.OrderNumber='' then 'NA' else isnull(tPR.OrderNumber,'NA')   end OrderNumber, tPR.Orderdate, tPR.Title, tPR.Priority, tPR.RequestBy, dbo.FS_GetUserNameByUserID(tPR.RequestBy) AS RequestByUserName, tPR.Status,  ");
            sb.AppendLine(" mS.Status AS RequestStatus, isnull(tEC.OrderCategory,'NA')OrderCategory, isnull(tEC.Ordertype,'NA')Ordertype,isnull(tEC.CustomerFirstName,'NA')CustomerFirstName, isnull(tEC.CustomerLastName,'NA')CustomerLastName,isnull(tEC.DeliveryType,'NA')DeliveryType,isnull(tEC.AmountPaid,0)AmountPaid,isnull(tEC.TotalPrice,0)TotalPrice,isnull(tEC.AmounttobeCollected,0)AmounttobeCollected, mPM.MethodName,isnull(tEC.VIPCustomer,'N') as VIPCustomer,");
            sb.AppendLine(" CASE WHEN mS.ID = 1 THEN 'red' WHEN mS.ID = 28 THEN 'orange' ELSE 'green' END AS ImgRequest, CASE WHEN mS.ID = 1 THEN 'gray' WHEN mS.ID IN (2, 21, 22, 31, 32) THEN 'red' WHEN mS.ID IN (4, 28)  ");
            sb.AppendLine(" THEN 'orange' WHEN mS.ID NOT IN (1, 2, 21, 22, 31, 32) THEN 'green' END AS ImgApproval, CASE WHEN mS.ID IN (1, 2, 4, 21, 22, 31, 32) THEN 'gray' WHEN mS.ID IN (3,5,7,8,25,34,35,37,38,39,10034)  ");
            sb.AppendLine(" THEN 'red' WHEN mS.ID IN (6) THEN 'greenRed' WHEN mS.ID IN (10, 26, 28,10037) THEN 'orange' WHEN mS.ID IN (10036) THEN 'green' END AS ImgIssue, CASE WHEN mS.ID IN (1, 2, 3, 4, 5) THEN 'gray' WHEN mS.ID IN (6, 7)  ");
            sb.AppendLine(" THEN 'red' WHEN mS.ID IN (8) THEN 'green' END AS ImgReceipt, CASE WHEN mS.ID IN (1, 2, 3, 4, 5, 6, 7) THEN 'gray' WHEN mS.ID IN (8) THEN 'red' WHEN mS.ID IN (9) THEN 'green' END AS ImgConsumption,  ");
            sb.AppendLine(" tPR.Deliverydate, tPR.OrderNo, tEC.Idt, TA.LocationCode, tEC.EcomOrderNumber, tEC.Email,tPR.PaymentID ");
            if (string.IsNullOrEmpty(misidn))
            { }
            else
            { sb.AppendLine("  , tED.ProductName "); }

            if (string.IsNullOrEmpty(semserial)) { }
            else
            {
                sb.AppendLine("  ,Tods.MSSIDN ");
            }

            sb.AppendLine(" FROM dbo.tOrderHead AS tPR INNER JOIN  dbo.mTerritory AS mT ON tPR.StoreId = mT.ID INNER JOIN dbo.mStatus AS mS ON tPR.Status = mS.ID LEFT OUTER JOIN dbo.mCustomerDetail AS CD ON tPR.Id = CD.OrderID LEFT OUTER JOIN ");
            sb.AppendLine(" dbo.tECommHead AS tEC ON tEC.OrderID = tPR.Id LEFT OUTER JOIN    dbo.mPaymentMethodMain AS mPM ON tPR.PaymentID = mPM.ID LEFT OUTER JOIN    dbo.tAddress AS TA ON TA.ID = tPR.LocationID ");
            if (string.IsNullOrEmpty(misidn))
            { }
            else
            { sb.AppendLine(" LEFT OUTER JOIN   dbo.tECommDetail AS tED ON tED.EcomHeadID = tEC.ID  "); }

            if (string.IsNullOrEmpty(semserial)) { }
            else
            {
                sb.AppendLine("  left outer join   tOrderDispatchStatus as Tods on  Tods.orderno=tPR.id ");
            }
            sb.AppendLine(" WHERE tPR.orderType in  ('PreOrder','NormalOrder','Direct Import') and mt.ecommerce=1 and  isnull(tEC.orderType,'Na') like  '%" + ordertype + "%'  and  isnull(tPR.OrderNo,'NA') like '%" + ordno + "%'   and  isnull(tEC.Idt,'NA') like '%" + passport + "%'  and isnull(tEC.OrderCategory,'NA') like  '%" + ordcat + "%'   and isnull(email,'NA') like '%" + email + "%'  and  isnull(tPR.PaymentID,0) like'%" + paymenttype + "%' ");
            if (string.IsNullOrEmpty(misidn))
            { }
            else
            { sb.AppendLine(" and  isnull(ProductName,'NA') like'%" + misidn + "%'  "); }
            if (usertype == "Retail User" || usertype == "Retail User Admin")
            {

                sb.AppendLine("  and tEC.Deliverytype='Hub' ");
            }
            if (string.IsNullOrEmpty(semserial)) { }
            else
            {
                sb.AppendLine(" and  isnull(Tods.MSSIDN,'NA')  like'%" + semserial + "%'  ");
            }
            sb.AppendLine("  and  convert(date ,OrderDate )  >= convert(date ,@fdate )    and  convert(date ,OrderDate)  <= convert(date ,@Tdate ) order by id desc ");
            ds = fillds(Convert.ToString(sb), conn);
            return ds;
        }


        public string ISeCommerceOrNot(string userid, string[] conn)
        {
            string result = "";
            DataSet ds1 = new DataSet();
            DataSet ds = new DataSet();
            ds = fillds("select TerritoryID from mUserTerritoryDetail where UserID=" + userid + "", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i <= ds.Tables[0].Rows.Count - 1; i++)
                {
                    long territoryID = Convert.ToInt64(ds.Tables[0].Rows[i]["TerritoryID"]);
                    ds1 = fillds("select ECommerce from mTerritory where id=" + territoryID + "", conn);
                    if (Convert.ToInt16(ds1.Tables[0].Rows[0]["ECommerce"]) == 1)
                    {
                        result = "Yes";
                        break;
                    }
                    else
                    {
                        result = "No";
                    }
                }
            }
            return result;
        }
        #endregion


        #region EcommerceNotification

        public void ActivtaedSendEmail(long RequestID, string[] conn)
        {
            string MailSubject = "";
            string MailBody = "";
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            try
            {
                GWC_SP_GetRequestHeadByRequestIDs_Result Request = new GWC_SP_GetRequestHeadByRequestIDs_Result();
                Request = db.GWC_SP_GetRequestHeadByRequestIDs(RequestID.ToString()).FirstOrDefault();

                long DepartmentID = long.Parse(Request.SiteID.ToString());
                long Status = long.Parse(Request.Status.ToString());
                DataSet dsMailSubBody = new DataSet();
                dsMailSubBody = fillds(" select * from mMessageEMailTemplates where ActivityID=(select Id from mDropdownValues where StatusID=35) and MessageID=(select Id from mDropdownValues where Value='Information') and DepartmentID=" + DepartmentID + " and Active='Yes'", conn);
                MailSubject = dsMailSubBody.Tables[0].Rows[0]["MailSubject"].ToString() + ", Order No # " + Request.OrderNo + "";

                DataSet dsstatus = new DataSet();
                dsstatus = fillds(" select id from mstatus where status='Activated' ", conn);
                long statusNew = long.Parse(dsstatus.Tables[0].Rows[0]["id"].ToString());

                DataSet ds = new DataSet();
                ds = fillds("select UserID from mUserTerritoryDetail MUT inner join mUserProfileHead MUPH on MUT.UserID = MUPH.ID  and MUT.TerritoryID = " + DepartmentID + " and MUPH.UserType in('Admin','Super Admin') ", conn);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    //  for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    //  {
                    //MailBody = "Dear Admin/Super Admin <br/>"; //for Admin only
                    //  MailBody = MailBody + dsMailSubBody.Tables[0].Rows[0]["MailBody"].ToString();
                    // GWC_SP_GetRequestHeadByRequestIDs_Result EcomMsg = new GWC_SP_GetRequestHeadByRequestIDs_Result();
                    MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetailActivated(Request, conn);
                    MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetailOrderActivated(Request, conn);
                    MailBody = MailBody + "<br/><br/>" + EmailGetAddressDetailsEcom(Request, conn);
                    MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetailEcommerce(RequestID, conn);

                    // SendMail(MailBody + MailGetFooter(), MailSubject, EmailGetEmailIDsByUserID(Convert.ToInt64(ds.Tables[0].Rows[i]["UserID"]), conn));

                    long TemplateID = long.Parse(dsMailSubBody.Tables[0].Rows[0]["ID"].ToString());
                    InsertCorrespondsData(RequestID, MailSubject, MailBody, DepartmentID, statusNew, conn);  ///add new 
                    // SaveCorrespondsData(RequestID, MailSubject, MailBody, DepartmentID, Status, conn);   //old
                    AdditionalDistribution(RequestID, TemplateID, conn);
                    //  }
                }
            }
            catch { }
            finally { }
        }

        protected string EMailGetRequestDetailOrderActivated(GWC_SP_GetRequestHeadByRequestIDs_Result Request, string[] conn)
        {
            //string result = "";    

            DateTime date1 = new DateTime();
            date1 = Convert.ToDateTime(Request.OrderDate.Value.ToString());
            string OrderDate = date1.ToString("dd/MM/yyyy");

            DateTime date2 = new DateTime();
            date2 = Convert.ToDateTime(Request.Deliverydate.Value.ToString());
            string Deliverydate = date2.ToString("dd/MM/yyyy");

            string messageBody = "<font><b>Order Summary :  </b> </font><br/><br/>";
            string htmlTableStart = "<table style=\"border-collapse:collapse; text-align:center;\" >";
            string htmlTableEnd = "</table>";
            string htmlHeaderRowStart = "<tr style =\"background-color:#6FA1D2; color:#ffffff;\">";
            string htmlHeaderRowEnd = "</tr>";
            string htmlTrStart = "<tr style =\"color:black; text-align: center;\">";
            string htmlTrEnd = "</tr>";
            string htmlTdStart = "<td style=\" border-color:black; border-style:solid; border-width:thin; padding: 5px; text-align: center;font-size: 10pt;\">";
            string htmlTdEnd = "</td>";

            messageBody += htmlTableStart;

            messageBody += htmlHeaderRowStart;
            messageBody += htmlTdStart + "Order Date" + htmlTdEnd;
            messageBody += htmlTdStart + "Order Id" + htmlTdEnd;
            //messageBody += htmlTdStart + "Customer Order Reference No." + htmlTdEnd;          
            messageBody += htmlTdStart + "Exp. Delivery Date" + htmlTdEnd;
            messageBody += htmlTdStart + "Status" + htmlTdEnd;
            messageBody += htmlTdStart + "Remark" + htmlTdEnd;
            messageBody += htmlHeaderRowEnd;

            messageBody += htmlTrStart;
            //  messageBody += htmlTdStart + " " + Request.OrderDate.Value.ToString("dd-MMM-yyyy") + " " + htmlTdEnd;
            // messageBody += htmlTdStart + " " + Request.OrderDate.Value.ToString("dd/mm/yyyy") + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + OrderDate + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Request.OrderNo.ToString() + " " + htmlTdEnd;
            // messageBody += htmlTdStart + " " + Request.OrderNumber + " " + htmlTdEnd;            
            //   messageBody += htmlTdStart + " " + Request.Deliverydate.Value.ToString("dd-MMM-yyyy") + " " + htmlTdEnd;
            // messageBody += htmlTdStart + " " + Request.Deliverydate.Value.ToString("dd/mm/yyyy") + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Deliverydate + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Request.RequestStatus + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Request.Remark + " " + htmlTdEnd;
            messageBody += htmlTrEnd;
            messageBody += htmlTableEnd;

            return messageBody;
        }
        protected string EMailGetRequestDetailActivated(GWC_SP_GetRequestHeadByRequestIDs_Result Request, string[] conn)
        {
            string misidn = "";
            DataSet ds = new DataSet();
            ds = fillds("select * from torderhead  TOH left join torderdispatchstatus TODS on TOH.id=TODS.Orderno where TOH.id=" + Request.ID + " ", conn);
            misidn = Convert.ToString(ds.Tables[0].Rows[0]["MSSIDN"].ToString());

            string messageBody = "<font>Dear GWC Team,  </font><br/><font>Below order is ready for dispatch and is waiting for your action.</font><br/>";
            messageBody = messageBody + "<font><b> Activation Details:</b></font>";
            string htmlTableStart = "<table style=\"border-collapse:collapse; text-align:center;\" >";
            string htmlTableEnd = "</table>";
            string htmlHeaderRowStart = "<tr style =\"background-color:#6FA1D2; color:#ffffff;\">";
            string htmlHeaderRowEnd = "</tr>";
            string htmlTrStart = "<tr style =\"color:black; text-align: center;\">";
            string htmlTrEnd = "</tr>";
            string htmlTdStart = "<td style=\" border-color:black; border-style:solid; border-width:thin; padding: 5px; text-align: center;font-size: 10pt;\">";
            string htmlTdEnd = "</td>";

            messageBody += htmlTableStart;
            messageBody += htmlHeaderRowStart;
            messageBody += htmlTdStart + "MSISDN" + htmlTdEnd;
            messageBody += htmlTdStart + "SIM Serial" + htmlTdEnd;
            messageBody += htmlTdStart + "Plan Name" + htmlTdEnd;
            messageBody += htmlTdStart + "Remark" + htmlTdEnd;
            messageBody += htmlHeaderRowEnd;

            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int r = 0; r <= ds.Tables[0].Rows.Count - 1; r++)
                {
                    messageBody += htmlTrStart;
                    messageBody += htmlTdStart + "  " + htmlTdEnd;
                    messageBody += htmlTdStart + " " + ds.Tables[0].Rows[r]["MSSIDN"].ToString() + "  " + htmlTdEnd;
                    messageBody += htmlTdStart + "   " + htmlTdEnd;
                    messageBody += htmlTdStart + " " + Request.Remark + " " + htmlTdEnd;
                    messageBody += htmlTrEnd;
                }
            }


            messageBody += htmlTableEnd;
            return messageBody;
        }

        private string GetUserNameByForActivtaedEmail(long storeid, string[] conn)
        {
            string result = "", emailid = "";
            DataSet ds = new DataSet();
            ds = fillds("select MUPH.FirstName,MUPH.LastName,MUPH.EmailID from mUserTerritoryDetail MUT inner join mUserProfileHead MUPH on MUT.UserID = MUPH.ID  and MUT.TerritoryID = " + storeid + " and MUPH.UserType = 'Admin' ", conn);
            result = Convert.ToString(ds.Tables[0].Rows[0]["FirstName"]) + " " + Convert.ToString(ds.Tables[0].Rows[0]["LastName"]);
            emailid = Convert.ToString(ds.Tables[0].Rows[0]["EmailID"]);
            return result;
        }

        private string EmailGetEmailIDsForEcommerce(long storeid, string[] conn)
        {
            string emailid = "";
            DataSet ds = new DataSet();
            ds = fillds("select MUPH.EmailID from mUserTerritoryDetail MUT inner join mUserProfileHead MUPH on MUT.UserID = MUPH.ID  and MUT.TerritoryID = " + storeid + " and MUPH.UserType = 'Admin' ", conn);
            emailid = Convert.ToString(ds.Tables[0].Rows[0]["EmailID"]);
            return emailid;
        }

        public string CheckDeptIsEcomOrNot(long orderno, string[] conn)
        {
            string storeid = "", ans = "";
            DataSet ds = new DataSet();
            ds = fillds("select StoreId from torderhead where id=" + orderno + " ", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                storeid = Convert.ToString(ds.Tables[0].Rows[0]["StoreId"]);
            }

            DataSet ds1 = new DataSet();
            ds1 = fillds("select CustAnalytics from mTerritory where id=" + storeid + " ", conn);
            if (ds1.Tables[0].Rows.Count > 0)
            {
                ans = Convert.ToString(ds1.Tables[0].Rows[0]["CustAnalytics"]);
            }

            return ans;
        }

        public string ISEcommOrNot(long orderno, string[] conn)
        {
            string yesornot = "";
            DataSet ds = new DataSet();
            ds = fillds("select * from tOrderDispatchStatus where OrderNo=" + orderno + " ", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                yesornot = "Yes";
            }
            else
            {
                yesornot = "No";
            }
            return yesornot;
        }

        public string ChkOrdtypeIsOnlyDevice(long orderno, string[] conn)
        {
            string yesornot = "";
            DataSet ds = new DataSet();
            ds = fillds("select * from tecommhead where ordertype='Only Device' and orderid=" + orderno + " ", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                yesornot = "Yes";
            }
            else
            {
                yesornot = "No";
            }
            return yesornot;
        }


        public string ChangeStatus(string OrderID, string cngvalue, string[] conn)
        {
            string result = "true";
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_UpdateChangeStatus";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@OrderID", OrderID);
            cmd.Parameters.AddWithValue("@Value", cngvalue);
            cmd.ExecuteNonQuery();
            return result;
        }

        #region Project 2 change status CR 03-10-2023
        //Added by suraj khopade
        public DataSet Getcngstatus(long OrderID, string UserType, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_BindDDLcngstatus";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@OrderID", OrderID);
                cmd.Parameters.AddWithValue("@UserType", UserType);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }
        #endregion
        public long GetStatus(string orderno, string[] conn)
        {
            long yesornot = 0;
            DataSet ds = new DataSet();
            ds = fillds("select status from torderhead where id=" + orderno + " ", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                yesornot = Convert.ToInt64(ds.Tables[0].Rows[0]["status"]);
            }

            return yesornot;
        }

        public int GetOrdersCnt(string orderno, string[] conn)
        {
            int cnt = 0;
            DataSet ds = new DataSet();
            ds = fillds("select * from tecommhead where orderid=" + orderno + " ", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                long oid = Convert.ToInt64(ds.Tables[0].Rows[0]["id"]);
                DataSet ds1 = new DataSet();
                ds1 = fillds("select count(*)cnt from  tecommdetail where ecomheadid=" + oid + " and productcode in('110037','110038') ", conn);
                cnt = Convert.ToInt16(ds1.Tables[0].Rows[0]["cnt"]);
            }
            return cnt;
        }


        public string CheckStatusForDriverAllocation(long orderno, string[] conn)
        {
            string result = "";
            DataSet ds = new DataSet();
            ds = fillds("select * from torderhead TH left outer join mstatus MS on TH.status=MS.id where  TH.id=" + orderno + " and MS.status in ('Ready for Dispatch','Activated','Out For Delivery') ", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                result = "Yes";
            }
            else
            {
                result = "No";
            }
            return result;
        }


        public string GetDeliveryType(string OrderNo, string[] conn)
        {
            string result = "";
            DataSet ds = new DataSet();
            ds = fillds("select deliverytype from  tecommhead where orderid=" + OrderNo + " ", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                result = ds.Tables[0].Rows[0]["deliverytype"].ToString();
            }
            return result;
        }

        public string GetMobilenumberforpostpaidreport(string OrderNo, string Skucode, string PackageID, string[] conn)
        {
            string result = "";
            DataSet ds = new DataSet();
            if (Skucode == "999999")
            {
                ds = fillds("select ted.ProductName,ted.ProductName as packname from  tecommdetail ted left outer join tecommhead teh on teh.id=ted.ecomheadid where teh.orderid=" + OrderNo + " and ted.packageid=" + PackageID + " and ted.productcode='" + Skucode + "' ", conn);
            }
            else
            {
                ds = fillds("select ted.ProductName,ted.ProductName as packname from  tecommdetail ted left outer join tecommhead teh on teh.id=ted.ecomheadid where teh.orderid=" + OrderNo + " and ted.packageid=" + PackageID + " and ted.productcode='" + Skucode + "' ", conn);

            }
            if (ds.Tables[0].Rows.Count > 0)
            {
                result = ds.Tables[0].Rows[0]["ProductName"].ToString();
            }
            return result;
        }


        public string GetMobilePackName(string OrderNo, string Skucode, string PackageID, string[] conn)
        {
            string result = "";
            DataSet ds = new DataSet();
            ds = fillds("select ted.ProductName,ted.ProductName as packname,ted.ProductType from  tecommdetail ted left outer join tecommhead teh on teh.id=ted.ecomheadid where teh.orderid=" + OrderNo + " and ted.packageid=" + PackageID + " and producttype='PricePlan' ", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                result = ds.Tables[0].Rows[0]["ProductName"].ToString();
            }
            return result;
        }

        public string GetExtrasvalues(string OrderNo, string[] conn)
        {
            string result = "";
            DataSet ds = new DataSet();
            ds = fillds("select tod.productname from torderhead toh left outer join tecommhead teh on toh.id=teh.orderid left outer join tecommdetail tod on tod.ecomheadid=teh.id where toh.id=" + OrderNo + " and tod.producttype='Extra' ", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i <= ds.Tables[0].Rows.Count - 1; i++)
                {
                    result = result + ", " + ds.Tables[0].Rows[i]["productname"].ToString().Trim();
                }
                result = result.Remove(0, 2);
            }

            return result;
        }


        #endregion



        #region QNBN Report

        public string CkhUserType(long userid, string[] conn)
        {
            string result = "";
            DataSet ds = new DataSet();
            ds = fillds("select * from mUserTerritoryDetail muh left outer join mterritory mt on mt.id = muh.TerritoryID where mt.ecommerce = 1 and muh.userid = " + userid + "  ", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                result = "Ecomm";
            }
            else
            {

                DataSet ds2 = new DataSet();
                ds2 = fillds("select * from mUserTerritoryDetail muh left outer join mterritory mt on mt.id = muh.TerritoryID where  mt.territory = 'QNBN01' and muh.userid  = " + userid + " ", conn);
                if (ds2.Tables[0].Rows.Count > 0)
                {
                    result = "QNBN";
                }
                else
                {
                    DataSet ds3 = new DataSet();
                    ds3 = fillds("select * from mUserTerritoryDetail muh left outer join mterritory mt on mt.id = muh.TerritoryID where  mt.parentid in  (select Id from mcompany where isProjectsitedetails='Yes' ) and muh.userid  = " + userid + " ", conn);
                    if (ds3.Tables[0].Rows.Count > 0)
                    {
                        result = "vftechnical";
                    }
                    else
                    {

                        result = "Normal";
                    }
                }

            }
            return result;
        }


        #endregion

        #region Noor

        public string CheckOrderIsFMSOrNot(string OrderID, string[] conn)
        {
            string result = "Yes";
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_checkIsOrdIsFMS";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@OrderId", OrderID);
            da.SelectCommand = cmd;
            da.Fill(ds);
            if (ds.Tables[0].Rows.Count > 0)
            {
                result = "Yes";
            }
            else
            {
                result = "No";
            }
            cmd.Connection.Close();
            return result;
        }


        public DataSet GetFMSReportData(Int64 OrderID, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from  VW_FMSReport where orderid=" + OrderID + "", conn);
            return ds;
        }

        public int GetDispatchedstatusOrders(string SelectedOrder, string[] conn)
        {
            //DataSet ds = new DataSet();
            //ds = fillds("select * from torderhead where id in(" + SelectedOrder + ") and status in(2,3,4,6,7,25,9,10,21,22,26,28,30,31,32,34,35,38,39,10036) ", conn);
            //int cnt = ds.Tables[0].Rows.Count;
            //return cnt;
            int cnt = 0;
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_chkOrderStatus";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@ID", SelectedOrder);
                da.SelectCommand = cmd;
               // da.Fill(ds);
               //  cnt = ds.Tables[0].Rows.Count;
                 cnt = int.Parse(cmd.ExecuteScalar().ToString());
                return cnt;
            }


        }

        //for production status
        //  public int GetDispatchedstatusOrders(string SelectedOrder, string[] conn)
        //{
        //    DataSet ds = new DataSet();
        //    ds = fillds("select * from torderhead where id in(" + SelectedOrder + ") and status in(2,3,4,6,25,9,10,21,22,26,28,30,31,32,33,34,36,37,39) ", conn);
        //    int cnt = ds.Tables[0].Rows.Count;
        //    return cnt;
        //}


        
        public int GetDispstatusOrdforfullfillment(string SelectedOrder, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds(" select  * from torderhead t left outer join tecommhead teh on teh.orderid = t.id where ((teh.ordertype = 'Fixed Only' and  teh.deliverytype = 'Home') or(teh.ordertype = 'FMS OutDoor')) and t.status = 8 and t.id in (" + SelectedOrder + ")", conn);
            int cnt = ds.Tables[0].Rows.Count;
            return cnt;

        }


        #region Project 2 change status CR 03-10-2023
        // Added by suraj khopade 
        public int GestatusOrdforSuperAdmin1(long OId, string userType, string[] conn)
        {

            int cnt = 0;
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ChkChangeOrderStatus";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@userType", userType);
                cmd.Parameters.AddWithValue("@OrderId", OId);
                da.SelectCommand = cmd;
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    //cnt = ds.Tables[0].Rows.Count;
                    //userid = Convert.ToInt64(ds.Tables[0].Rows[0]["userid"].ToString());
                     cnt = Convert.ToInt32(ds.Tables[0].Rows[0]["id"].ToString());
                    if(cnt == 0)
                    {
                        cnt = 0;
                    }

                }
                else
                {
                    cnt = 0;
                }
            }
            return cnt;
        }

        #endregion

        #endregion

        public string ChkOrdIsApproved(long OrdId, string[] conn)
        {
            DataSet ds = new DataSet();
            string result = "";
            ds = fillds("select * from torderhead where id=" + OrdId + " and status=3", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                result = "Approved";
            }
            else
            {
                result = "NotApproved";
            }
            return result;
        }
        public DataSet GetProjectSiteDetails(long ID, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetProjectSiteDetails";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@RequestId", ID);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }

        public string SaveEditmodedata(string ID, string pid, string sid, long uid, string[] conn)
        {
            string result = "";
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    SqlDataAdapter da = new SqlDataAdapter();
                    DataSet ds = new DataSet();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_SaveEditmodedata";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@ID", ID);
                    cmd.Parameters.AddWithValue("@pid", pid);
                    cmd.Parameters.AddWithValue("@sid", sid);
                    cmd.Parameters.AddWithValue("@uid", uid);
                    cmd.ExecuteNonQuery();
                }
                result = "1";

            }
            catch
            {
                result = "1";
            }
            return result;
        }

        public string ISProjectSiteDetailsNew(string companyid, string[] conn)
        {
            string result = "";
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_ISProjectSiteDetails";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@companyid", companyid);
                da.SelectCommand = cmd;
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    result = "Yes";
                }
                else
                {
                    result = "No";
                }
            }

            return result;
        }


        public string ChkISFlextProduct(long Oid, string[] conn)
        {
            string result = "";
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_ChkISFlextProduct";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@Oid", Oid);
                da.SelectCommand = cmd;
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    result = "Yes";
                }
                else
                {
                    result = "No";
                }
            }

            return result;
        }



        #region SerialNumber        
        public DataSet CheckISvft(long Oid, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_CheckISvft";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@Oid", Oid);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }



        public void UpdateOrderEditFlag(long Id, string[] conn)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                DataSet ds = new DataSet();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_UpdateOrderEditFlag";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@OrderID", Id);
                cmd.ExecuteNonQuery();
                EmailSendToRequestor(Id, conn);
            }
        }

        public int EmailSendToRequestor(long RequestID, string[] conn)
        {
            int Result = 0;
            try
            {
                string MailSubject;
                string MailBody;

                using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
                {
                    tOrderHead PartReq = new tOrderHead();
                    PartReq = db.tOrderHeads.Where(r => r.Id == RequestID).FirstOrDefault();
                    GWC_SP_GetRequestHeadByRequestIDs_Result Request = new GWC_SP_GetRequestHeadByRequestIDs_Result();
                    Request = db.GWC_SP_GetRequestHeadByRequestIDs(RequestID.ToString()).FirstOrDefault();
                    long DepartmentID = Convert.ToInt64(PartReq.StoreId);
                    long Status = Convert.ToInt64(PartReq.Status);
                    MailSubject = "Please revisit the Order No # " + Request.OrderNo + " and select the available Serial or keep it blank ";
                    MailBody = "Dear " + GetUserNameByUserID(Convert.ToInt64(PartReq.CreatedBy), conn) + ", <br/>";
                    MailBody = MailBody + "This is an automatically generated message in reference to a order request.Some of the reserved serial in Order# " + Request.OrderNo + " is not available, please revisit the order and select the available Serial or keep it blank <br/>";
                    MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetailVodafoneTechnical(Request, conn);
                    MailBody = MailBody + "<br/><br/>" + EmailGetAddressDetails(Request, conn);
                    MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetail(RequestID, conn);
                    SendMail(MailBody + MailGetFooter(), MailSubject, EmailGetEmailIDsByUserID(Convert.ToInt64(Request.RequestBy), conn));
                    // SendMailtemp(MailBody + MailGetFooter(), MailSubject, EmailGetEmailIDsByUserID(Convert.ToInt64(Request.RequestBy), conn));
                    SaveCorrespondsData(RequestID, MailSubject, MailBody, DepartmentID, Status, conn);
                }
            }
            catch (Exception ex)
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "Sp_EnterErrorTracking";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("Data", ex.Data.ToString());
                    cmd.Parameters.AddWithValue("GetType", ex.Source.ToString());
                    cmd.Parameters.AddWithValue("InnerException", "Error");
                    cmd.Parameters.AddWithValue("Message", ex.Message.ToString());
                    cmd.Parameters.AddWithValue("Source", " EmailSendToRequestor");
                    cmd.Parameters.AddWithValue("DateTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("UserID", 20000);
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
                Result = 3;//mail not sent to requestor.
            }
            finally { }
            return Result;
        }

        public void SendMailtemp(string MailBody, string MailSubject, string ToEmailIDs)
        {
            try
            {
                SmtpClient smtpClient = new SmtpClient("mail.brilliantinfosys.com", 587);
                MailMessage message = new MailMessage();
                MailAddress fromAddress = new MailAddress("suraj@brilliantinfosys.com", "GWC");
                message.From = fromAddress;

                message.To.Add(ToEmailIDs);
                message.Subject = MailSubject;

                message.IsBodyHtml = true;

                //Message body content
                message.Body = MailBody;

                smtpClient.EnableSsl = false;
                //Send SMTP mail
                smtpClient.UseDefaultCredentials = false;
                NetworkCredential basicCredential = new NetworkCredential("suraj@brilliantinfosys.com", "6march1986");
                smtpClient.Credentials = basicCredential;
                smtpClient.Send(message);
            }
            catch { }
        }
        public string ChkISOrdCreatorOrNot(long Oid, long Uid, string[] conn)
        {
            string result = "";
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_ChkISOrdCreatorOrNot";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@Oid", Oid);
                cmd.Parameters.AddWithValue("@Uid", Uid);
                da.SelectCommand = cmd;
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    result = "Yes";
                }
                else
                {
                    result = "No";
                }
            }

            return result;
        }

        public string ChkSerialedCompany(long Companyid, string[] conn)
        {
            string result = "";
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_ChkSerialCompany";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@Companyid", Companyid);
                da.SelectCommand = cmd;
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    result = "Yes";
                }
                else
                {
                    result = "No";
                }
            }
            return result;
        }

        #endregion


        #region Serial Number
        /// <summary>
        /// SerialNumber
        /// </summary>
        /// <param name="RequestID"></param>
        /// <param name="siteID"></param>
        /// <param name="sessionID"></param>
        /// <param name="userID"></param>
        /// <param name="CurrentObject"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public List<SP_GetPartSerialDetail_Result> GetPartSerialDetailByRequestID(long RequestID, long skuid, string sessionID, string userID, string CurrentObject, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                List<SP_GetPartSerialDetail_Result> existingList = new List<SP_GetPartSerialDetail_Result>();
                existingList = GetExistingTempDataBySessionIDSerialNumber(sessionID, userID, CurrentObject, conn);
                if (existingList.Count > 0)
                {
                    if (RequestID == 0)
                    {
                        existingList = (from lst in existingList
                                        where lst.Skuid == skuid && lst.CreationBy == Convert.ToInt64(userID) && lst.OrderID == RequestID
                                        select lst).ToList();
                    }
                    else
                    {
                        existingList = (from lst in existingList
                                        where lst.Skuid == skuid && lst.OrderID == RequestID  //lst.CreationBy == Convert.ToInt64(userID) &&
                                        select lst).ToList();
                    }
                }

                return existingList;
            }
        }

        protected void SaveTempDataToSerialDB(List<SP_GetPartSerialDetail_Result> paraobjList, string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
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

        }

        public List<SP_GetPartSerialDetail_Result> GetExistingTempDataBySessionIDSerialNumber(string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                List<SP_GetPartSerialDetail_Result> objtAddToCartProductDetailList = new List<SP_GetPartSerialDetail_Result>();

                TempData tempdata = new TempData();
                tempdata = (from temp in db.TempDatas
                            where temp.SessionID == paraSessionID
                            && temp.ObjectName == paraCurrentObjectName
                            && temp.UserID == paraUserID
                            select temp).FirstOrDefault();
                if (tempdata != null)
                {
                    objtAddToCartProductDetailList = datahelper.DeserializeEntity1<SP_GetPartSerialDetail_Result>(tempdata.Data);
                }
                return objtAddToCartProductDetailList;
            }
        }


        public List<SP_GetPartSerialDetail_Result> AddPartIntoRqstSeialNumber_TempData(string paraProductIDs, string paraSessionID, string paraUserID, string paraCurrentObjectName, long SiteID, long Skuid, long Orderid, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                /*Begin : Get Existing Records from TempData*/
                List<SP_GetPartSerialDetail_Result> existingList = new List<SP_GetPartSerialDetail_Result>();
                existingList = GetExistingTempDataBySessionIDSerialNumber(paraSessionID, paraUserID, paraCurrentObjectName, conn);
                /*End*/
                long MaxSequenceNo = 0;
                if (existingList.Count > 0)
                {
                    MaxSequenceNo = Convert.ToInt64((from lst in existingList
                                                     select lst.Sequence).Max().Value);
                }

                /*Get Product Details*/
                List<SP_GetPartSerialDetail_Result> getnewRec = new List<SP_GetPartSerialDetail_Result>();
                getnewRec = (from view in db.SP_GetPartSerialDetail(paraProductIDs, MaxSequenceNo, Orderid, Skuid, SiteID, Convert.ToInt64(paraUserID))
                             orderby view.Sequence
                             select view).ToList();
                /*End*/

                /*Begin : Merge (Existing + Newly Added) Products to Create TempData of AddToCart*/
                List<SP_GetPartSerialDetail_Result> mergedList = new List<SP_GetPartSerialDetail_Result>();
                mergedList.AddRange(existingList);
                mergedList.AddRange(getnewRec);
                /*End*/

                /*Begin : Serialize & Save MergedAddToCartList*/
                SaveTempDataToSerialDB(mergedList, paraSessionID, paraUserID, paraCurrentObjectName, conn);
                /*End*/

                return mergedList;
            }
        }

        public int GetSelectedSKUQty(string paraSessionID, string paraUserID, string paraCurrentObjectName, string skuid, string[] conn)
        {
            int qty = 0;
            List<POR_SP_GetPartDetail_ForRequest_Result> existingList = new List<POR_SP_GetPartDetail_ForRequest_Result>();
            existingList = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, paraCurrentObjectName, conn);
            /*End*/

            if (existingList.Count > 0)
            {
                qty = Convert.ToInt32((from lst in existingList
                                       where lst.PRD_ID == skuid
                                       select lst.RequestQty));
            }
            return qty;
        }

        public List<POR_SP_GetPartDetail_ForRequest_Result> GetSelectedSKUQty1(string paraSessionID, string paraUserID, string paraCurrentObjectName, string skuid, string[] conn)
        {
            int qty = 0;
            List<POR_SP_GetPartDetail_ForRequest_Result> existingList = new List<POR_SP_GetPartDetail_ForRequest_Result>();
            existingList = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, paraCurrentObjectName, conn);
            /*End*/

            //if (existingList.Count > 0)
            //{
            //    qty = Convert.ToInt32((from lst in existingList
            //                           where lst.PRD_ID == skuid
            //                           select lst.RequestQty));
            //}
            return existingList;
        }



        public string ChkSerialedCompanyDeptID(long Deptid, string[] conn)
        {
            string result = "";
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_ChkSerialCompanyDeptID";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@Deptid", Deptid);
                da.SelectCommand = cmd;
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    result = "Yes";
                }
                else
                {
                    result = "No";
                }
            }
            return result;
        }



       
        
        public int FinalSaveRequestPartDetailSerial_New(string paraSessionID, string paraCurrentObjectName, long paraReferenceID, string paraUserID, long deptid, long PreviousStatusID, string[] conn)
        {
            
                var torderdetails = 0;
                int Result = 0;
                try
                {
                    // First solution Start

                    // Step 1: Get existing data from TempData
                    List<SP_GetPartSerialDetail_Result> finalSaveLst = new List<SP_GetPartSerialDetail_Result>();
                        finalSaveLst = GetExistingTempDataBySessionIDSerialNumber(paraSessionID, paraUserID, paraCurrentObjectName, conn);

                    // Step 2: Check if the data is valid (finalSaveLst should not be empty)
                    if (finalSaveLst != null && finalSaveLst.Count > 0)
                    {
                        // Step 3: Manually build the XML from finalSaveLst
                        XElement xmlEle = new XElement("Request",
                            from rec in finalSaveLst
                            select new XElement("PartList",
                                new XElement("OrderID", paraReferenceID),
                                new XElement("StoreID", Convert.ToInt64(deptid)),
                                new XElement("Skuid", Convert.ToInt64(rec.Skuid)),
                                new XElement("AvailableQty", Convert.ToInt32(rec.AvailableQty)),
                                new XElement("SerialNumber", Convert.ToString(rec.SerialNumber)),
                                new XElement("Object", Convert.ToString(rec.Object)),
                                new XElement("CreationDate", Convert.ToDateTime(rec.CreationDate)),
                                new XElement("CreationBy", Convert.ToInt64(paraUserID))
                            )
                        );

                        // Step 4: Insert the data into SKU Transaction History by executing the stored procedure

                        InsertIntotSkuTransactionHistory(paraReferenceID, xmlEle.ToString(), paraUserID, conn);

                        torderdetails = 1; Result = 1;

                        //Update resurve serial number
                        UpdateOrderSerialQty(paraReferenceID, conn);
                        ClearTempDataFromDB_New(paraSessionID, paraUserID, paraCurrentObjectName, conn);


                        //string storedProc = "SP_InsertIntotSkuTransactionHistory";
                        //using (SqlConnection procConnection = new SqlConnection(conn[0]))
                        //{
                        //    procConnection.Open();

                        //    using (SqlCommand procCommand = new SqlCommand(storedProc, procConnection))
                        //    {
                        //        procCommand.CommandType = CommandType.StoredProcedure;

                        //        // Add parameters for stored procedure
                        //        procCommand.Parameters.AddWithValue("@PRH_ID", paraReferenceID);
                        //        procCommand.Parameters.AddWithValue("@xmlData", xmlEle.ToString());

                        //        // Execute stored procedure
                        //        procCommand.ExecuteNonQuery();
                        //    }
                        //}
                    }
                
                }
                catch (System.Exception ex)
                {
                    Result = 0;
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "Sp_EnterErrorTracking";
                        cmd.Connection = svr.GetSqlConn(conn);
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("Data", ex.Data.ToString());
                        cmd.Parameters.AddWithValue("GetType", ex.Source.ToString());
                        cmd.Parameters.AddWithValue("InnerException", "Error");
                        cmd.Parameters.AddWithValue("Message", ex.Message.ToString());
                        cmd.Parameters.AddWithValue("Source", paraReferenceID + "| FinalSaveRequestPartDetailSerial");
                        cmd.Parameters.AddWithValue("DateTime", DateTime.Now);
                        cmd.Parameters.AddWithValue("UserID", paraUserID);
                        cmd.ExecuteNonQuery();
                        cmd.Connection.Close();
                    }
                }
                finally { }
                return Result;
            
        }

        public void InsertIntotSkuTransactionHistory(long paraReferenceID, string xmlEle, string paraUserID, string[] conn)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    SqlDataAdapter da = new SqlDataAdapter();
                    DataSet ds = new DataSet();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_InsertIntotSkuTransactionHistory";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@PRH_ID", paraReferenceID);
                    cmd.Parameters.AddWithValue("@xmlData", xmlEle);
                    cmd.ExecuteNonQuery();

                }
            }
            catch (System.Exception ex)
            {
                
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "Sp_EnterErrorTracking";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("Data", ex.Data.ToString());
                    cmd.Parameters.AddWithValue("GetType", ex.Source.ToString());
                    cmd.Parameters.AddWithValue("InnerException", "Error");
                    cmd.Parameters.AddWithValue("Message", ex.Message.ToString());
                    cmd.Parameters.AddWithValue("Source", paraReferenceID + "| InsertIntotSkuTransactionHistory");
                    cmd.Parameters.AddWithValue("DateTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("UserID", paraUserID);
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
            }
            finally { }
            
        }


        public void ClearTempDataFromDB_New(string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    SqlDataAdapter da = new SqlDataAdapter();
                    DataSet ds = new DataSet();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_ClearTempDataFromDB";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@SessionID", paraSessionID);
                    cmd.Parameters.AddWithValue("@UserID", paraUserID);
                    cmd.Parameters.AddWithValue("@ObjectName", paraCurrentObjectName);
                    cmd.ExecuteNonQuery();

                }
            }
            catch (System.Exception ex)
            {

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "Sp_EnterErrorTracking";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("Data", ex.Data.ToString());
                    cmd.Parameters.AddWithValue("GetType", ex.Source.ToString());
                    cmd.Parameters.AddWithValue("InnerException", "Error");
                    cmd.Parameters.AddWithValue("Message", ex.Message.ToString());
                    cmd.Parameters.AddWithValue("Source", paraSessionID + "| ClearTempDataFromDB_New");
                    cmd.Parameters.AddWithValue("DateTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("UserID", paraUserID);
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
            }
            finally { }

        }

        public int FinalSaveRequestPartDetailSerial(string paraSessionID, string paraCurrentObjectName, long paraReferenceID, string paraUserID, long deptid, long PreviousStatusID, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                var torderdetails = 0;
                int Result = 0;
                try
                {

                    List<SP_GetPartSerialDetail_Result> finalSaveLst = new List<SP_GetPartSerialDetail_Result>();
                    finalSaveLst = GetExistingTempDataBySessionIDSerialNumber(paraSessionID, paraUserID, paraCurrentObjectName, conn);

                    XElement xmlEle = new XElement("Request", from rec in finalSaveLst
                                                              select new XElement("PartList",
                                                              new XElement("OrderID", paraReferenceID),
                                                              new XElement("StoreID", Convert.ToInt64(deptid)),
                                                              new XElement("Skuid", Convert.ToInt64(rec.Skuid)),
                                                              new XElement("AvailableQty", Convert.ToInt32(rec.AvailableQty)),
                                                              new XElement("SerialNumber", Convert.ToString(rec.SerialNumber)),
                                                              new XElement("Object", Convert.ToString(rec.Object)),
                                                              new XElement("CreationDate", Convert.ToDateTime(rec.CreationDate)),
                                                              new XElement("CreationBy", Convert.ToInt64(paraUserID))
                                                            ));

                    ObjectParameter _PRH_ID = new ObjectParameter("PRH_ID", typeof(long));
                    _PRH_ID.Value = paraReferenceID;

                    ObjectParameter _xmlData = new ObjectParameter("xmlData", typeof(string));
                    _xmlData.Value = xmlEle.ToString();


                    ObjectParameter[] obj = new ObjectParameter[] { _PRH_ID, _xmlData };
                    db.ExecuteFunction("SP_InsertIntotSkuTransactionHistory", obj);
                    db.SaveChanges(); torderdetails = 1; Result = 1;
                    //Update resurve serial number
                    UpdateOrderSerialQty(paraReferenceID, conn);
                    ClearTempDataFromDB(paraSessionID, paraUserID, paraCurrentObjectName, conn);

                }
                catch (System.Exception ex)
                {
                    Result = 0;
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "Sp_EnterErrorTracking";
                        cmd.Connection = svr.GetSqlConn(conn);
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("Data", ex.Data.ToString());
                        cmd.Parameters.AddWithValue("GetType", ex.Source.ToString());
                        cmd.Parameters.AddWithValue("InnerException", "Error");
                        cmd.Parameters.AddWithValue("Message", ex.Message.ToString());
                        cmd.Parameters.AddWithValue("Source", paraReferenceID + "| FinalSaveRequestPartDetailSerial");
                        cmd.Parameters.AddWithValue("DateTime", DateTime.Now);
                        cmd.Parameters.AddWithValue("UserID", paraUserID);
                        cmd.ExecuteNonQuery();
                        cmd.Connection.Close();
                    }
                }
                finally { }
                return Result;
            }
        }


        public void UpdateOrderSerialQty(long oid, string[] conn)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                DataSet ds = new DataSet();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_UpdateOrderSerialQty";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@OrderID", oid);
                cmd.ExecuteNonQuery();
            }
        }


        public List<POR_SP_GetPartDetail_ForRequest_Result> AddPartIntoRequest_TempDataNew(string paraProductIDs, string paraSessionID, string paraUserID, string paraCurrentObjectName, long SiteID, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                /*Begin : Get Existing Records from TempData*/
                List<POR_SP_GetPartDetail_ForRequest_Result> existingList = new List<POR_SP_GetPartDetail_ForRequest_Result>();
                existingList = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, paraCurrentObjectName, conn);
                /*End*/
                long MaxSequenceNo = 0;
                if (existingList.Count > 0)
                {
                    MaxSequenceNo = Convert.ToInt64((from lst in existingList
                                                     select lst.Sequence).Max().Value);
                }

                /*Get Product Details*/
                List<POR_SP_GetPartDetail_ForRequest_Result> getnewRec = new List<POR_SP_GetPartDetail_ForRequest_Result>();
                getnewRec = (from view in db.POR_SP_GetPartDetail_ForRequest(paraProductIDs, MaxSequenceNo, SiteID, 0)
                             orderby view.Sequence
                             select view).ToList();
                /*End*/

                /*Begin : Merge (Existing + Newly Added) Products to Create TempData of AddToCart*/
                List<POR_SP_GetPartDetail_ForRequest_Result> mergedList = new List<POR_SP_GetPartDetail_ForRequest_Result>();
                mergedList.AddRange(existingList);
                mergedList.AddRange(getnewRec);
                /*End*/

                /*Begin : Serialize & Save MergedAddToCartList*/
                SaveTempDataToDB(mergedList, paraSessionID, paraUserID, paraCurrentObjectName, conn);
                /*End*/

                return mergedList;
            }
        }


        public List<SP_GetPartSerialDetail_Result> GetRequestPartserialDetailByRequestID(long RequestID, long siteID, string sessionID, string userID, string CurrentObject, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                List<SP_GetPartSerialDetail_Result> PartDetail = new List<SP_GetPartSerialDetail_Result>();

                PartDetail = (from sp in db.SP_GetPartSerialDetail("0", 0, RequestID, 0, siteID, Convert.ToInt64(userID))
                              select sp).ToList();
                SaveTempDataToSerialDB(PartDetail, sessionID, userID, CurrentObject, conn);
                return PartDetail;
            }
        }



        public void DeleteSerialTemptable(long UserID, string[] conn)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                DataSet ds = new DataSet();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_DeleteSerialTemptable";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@UserID", UserID);
                cmd.ExecuteNonQuery();
            }
        }
        public DataSet GetOnlySerialsku(long Oid, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetOnlySerialsku";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@Oid", Oid);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }




        public List<SP_GetPartSerialDetail_Result> RemoveSkuserialFromRequest_TempData(string paraSessionID, string paraUserID, string paraCurrentObjectName, long paraSequence, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                /*Begin : Get Existing Records from TempData*/
                List<SP_GetPartSerialDetail_Result> existingList = new List<SP_GetPartSerialDetail_Result>();
                existingList = GetExistingTempDataBySessionIDSerialNumber(paraSessionID, paraUserID, paraCurrentObjectName, conn);
                /*End*/

                /*Get Filter List [Filter By paraSequence]*/
                List<SP_GetPartSerialDetail_Result> filterList = new List<SP_GetPartSerialDetail_Result>();
                filterList = (from exist in existingList
                              where exist.Skuid != paraSequence
                              select exist).ToList();
                /*End*/


                /*Save result to TempData*/
                SaveTempDataToSerialDB(filterList, paraSessionID, paraUserID, paraCurrentObjectName, conn);
                /*End*/

                return filterList;
            }
        }

        public void DeleteSkuwiseSerialTemptable(long UserID, long skuid, long Oid, string[] conn)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                DataSet ds = new DataSet();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_DeleteSkuwiseSerialTemptable";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@UserID", UserID);
                cmd.Parameters.AddWithValue("@Skuid", skuid);
                cmd.Parameters.AddWithValue("@Ordid", Oid);
                cmd.ExecuteNonQuery();
            }
        }
        public void DeleteSkuwiseSerialFromqtychange(long UserID, long skuid, long Oid, string eventnm, string[] conn)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                DataSet ds = new DataSet();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_DeleteSkuwiseSerialFromqtychange";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@UserID", UserID);
                cmd.Parameters.AddWithValue("@Skuid", skuid);
                cmd.Parameters.AddWithValue("@Ordid", Oid);
                cmd.Parameters.AddWithValue("@eventnm", eventnm);
                cmd.ExecuteNonQuery();
            }
        }


        public string ChkOrderSerialedCompany(long OId, string[] conn)
        {
            string result = "";
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_ChkOrderSerialedCompany";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@OrdID", OId);
                da.SelectCommand = cmd;
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    result = "Yes";
                }
                else
                {
                    result = "No";
                }
            }
            return result;
        }

        public DataSet GetMessageserialvalues(long OrderHeadID, string[] conn)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    using (DataSet ds = new DataSet())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "SP_GetMessageSerialValuesstring";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("OrderHeadID", OrderHeadID);
                        cmd.Connection = svr.GetSqlConn(conn);
                        da.SelectCommand = cmd;
                        da.Fill(ds);
                        cmd.Connection.Close();
                        return ds;

                    }
                }
            }
        }

        public void DeleteResurveSerialnumber(string paraSessionID, string paraUserID, string paraCurrentObjectName, long orderid, string[] conn)
        {
            ClearTempDataFromDB(paraSessionID, paraUserID, paraCurrentObjectName, conn);
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                DataSet ds = new DataSet();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_DeleteResurveSerialnumber";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@OrderId", orderid);
                cmd.Parameters.AddWithValue("@Userid", paraUserID);
                cmd.ExecuteNonQuery();
            }
        }

        public void SaveTempDataToDBNew(List<POR_SP_GetPartDetail_ForRequest_Result> paraobjList, string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
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

        }

        public void UpdatePartRequest_TempDataNew(string SessionID, string UserID, string CurrentObjectName, long sid, decimal qty, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                List<POR_SP_GetPartDetail_ForRequest_Result> getRec = new List<POR_SP_GetPartDetail_ForRequest_Result>();
                getRec = GetExistingTempDataBySessionIDObjectName(SessionID, UserID, CurrentObjectName, conn);

                POR_SP_GetPartDetail_ForRequest_Result updateRec = new POR_SP_GetPartDetail_ForRequest_Result();
                updateRec = getRec.Where(g => g.Prod_ID == sid).FirstOrDefault();

                updateRec.RequestQty = qty;

                SaveTempDataToDB(getRec, SessionID, UserID, CurrentObjectName, conn);
            }
        }


        public void EmailSendToRequestorchnage(long RequestID, string[] conn)
        {
            int Result = 0;
            try
            {
                string MailSubject;
                string MailBody;

                using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
                {
                    tOrderHead PartReq = new tOrderHead();
                    PartReq = db.tOrderHeads.Where(r => r.Id == RequestID).FirstOrDefault();
                    GWC_SP_GetRequestHeadByRequestIDs_Result Request = new GWC_SP_GetRequestHeadByRequestIDs_Result();
                    Request = db.GWC_SP_GetRequestHeadByRequestIDs(RequestID.ToString()).FirstOrDefault();
                    long DepartmentID = Convert.ToInt64(PartReq.StoreId);
                    long Status = Convert.ToInt64(PartReq.Status);
                    MailSubject = "Please revisit the Order No # " + Request.OrderNo + " and select the available Serial or keep it blank ";
                    MailBody = "Dear " + GetUserNameByUserID(Convert.ToInt64(PartReq.CreatedBy), conn) + ", <br/>";
                    MailBody = MailBody + "This is an automatically generated message in reference to a order request.Some of the reserved serial in Order# " + Request.OrderNo + " is not available, please revisit the order and select the available Serial or keep it blank <br/>";
                    MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetailVodafoneTechnical(Request, conn);
                    MailBody = MailBody + "<br/><br/>" + EmailGetAddressDetails(Request, conn);
                    MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetail(RequestID, conn);
                    SendMail(MailBody + MailGetFooter(), MailSubject, EmailGetEmailIDsByUserID(Convert.ToInt64(Request.RequestBy), conn));
                    // SendMailtemp(MailBody + MailGetFooter(), MailSubject, EmailGetEmailIDsByUserID(Convert.ToInt64(Request.RequestBy), conn));
                    SaveCorrespondsData(RequestID, MailSubject, MailBody, DepartmentID, Status, conn);
                }
            }
            catch (Exception ex)
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "Sp_EnterErrorTracking";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("Data", ex.Data.ToString());
                    cmd.Parameters.AddWithValue("GetType", ex.Source.ToString());
                    cmd.Parameters.AddWithValue("InnerException", "Error");
                    cmd.Parameters.AddWithValue("Message", ex.Message.ToString());
                    cmd.Parameters.AddWithValue("Source", " EmailSendToRequestor");
                    cmd.Parameters.AddWithValue("DateTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("UserID", 20000);
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
                Result = 3;//mail not sent to requestor.
            }
            finally { }
            // return Result;
        }

        public void Updateallowflag(long orderid, string[] conn)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                DataSet ds = new DataSet();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_Updateallowflag";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@orderid", orderid);
                cmd.ExecuteNonQuery();
            }
        }

        //public void UpdateSerialAvailableBalance(long orderid,string [] conn)
        //{
        //    using (SqlCommand cmd = new SqlCommand())
        //    {
        //        SqlDataAdapter da = new SqlDataAdapter();
        //        DataSet ds = new DataSet();
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        cmd.CommandText = "SP_UpdateSerialAvailableBalance";
        //        cmd.Connection = svr.GetSqlConn(conn);
        //        cmd.Parameters.Clear();
        //        cmd.Parameters.AddWithValue("@OrderId", orderid);             
        //        cmd.ExecuteNonQuery();
        //    }
        //}


        public void EmailSendWhenRequestreSubmit_New(long RequestID, string[] conn)
        {
            int Result = 0;
            try
            {
                string MailSubject;
                string MailBody;

                DataSet ds1 = new DataSet();
                ds1 = GetRequestHeadByRequestID(RequestID.ToString(), conn);
                if (ds1.Tables[0].Rows.Count > 0)
                {

                    //DataTable firstTable = ds.Tables[0]; // Assuming the result is in the first table
                    //foreach (DataRow row in firstTable.Rows)
                    //{
                    //    // Process each row
                    //    Console.WriteLine(row["ColumnName"]); // Replace with actual column names
                    //}

                    long userid = 0;
                    long Orderstatus = long.Parse(ds1.Tables[0].Rows[0]["Status"].ToString());
                    long DepartmentID = long.Parse(ds1.Tables[0].Rows[0]["SiteID"].ToString());
                    long Status = long.Parse(ds1.Tables[0].Rows[0]["Status"].ToString());
                    string ISProjectSite = "";
                    string parentid = ds1.Tables[0].Rows[0]["parentid"].ToString();
                    // ISProjectSite = ISProjectSiteDetailsNew(Convert.ToString(Request.parentid), conn);
                    ISProjectSite = ISProjectSiteDetailsNew(parentid, conn);
                    DataSet dsMailSubBody = new DataSet();
                    dsMailSubBody = fillds("select * from mMessageEMailTemplates where ActivityID=(select Id from mDropdownValues where StatusID=2 and value='Order Submit') and MessageID=(select Id from mDropdownValues where Value='Information') and DepartmentID=" + DepartmentID + "", conn);
                    if (dsMailSubBody.Tables[0].Rows.Count > 0)
                    {
                        DataSet ds = new DataSet();
                        ds = GetFinalLevelapproval(RequestID, conn);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            for (int p = 0; p <= ds.Tables[0].Rows.Count - 1; p++)
                            {
                                userid = Convert.ToInt64(ds.Tables[0].Rows[p]["userid"].ToString());
                                if (Status == 2)
                                {

                                    // MailSubject = dsMailSubBody.Tables[0].Rows[0]["MailSubject"].ToString() + ", Order No # " + Request.OrderNo + "";
                                    MailSubject = dsMailSubBody.Tables[0].Rows[0]["MailSubject"].ToString() + ", Order No # " + ds1.Tables[0].Rows[0]["OrderNo"].ToString() + "";
                                }
                                else
                                {
                                    string CrntStatus = ds1.Tables[0].Rows[0]["RequestStatus"].ToString();
                                    MailSubject = "Order " + CrntStatus + ", Order No # " + ds1.Tables[0].Rows[0]["OrderNo"].ToString() + "";
                                    //string CrntStatus = Request.RequestStatus.ToString();
                                    //MailSubject = "Order " + CrntStatus + ", Order No # " + Request.OrderNo + "";
                                }
                                MailBody = "Dear " + GetUserNameByUserID(userid, conn) + ", <br/>";
                                MailBody = MailBody + "This is an automatically generated message in reference to a order request. An approval action is required before OMS can proceed. <br/> Thank you for your request. Before we can proceed, we need approver's formal approval to proceed. <br/>";
                                MailBody = MailBody + dsMailSubBody.Tables[0].Rows[0]["MailBody"].ToString();

                                if (ISProjectSite == "Yes")/// if (Request.parentid == 10266)
                                {
                                    //MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetailVodafoneTechnical(Request, conn);
                                    MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetailVodafoneTechnical_New(ds1, conn);
                                }
                                else
                                {
                                    MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail_new(ds1, conn);
                                }
                                //MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                                MailBody = MailBody + "<br/><br/>" + EmailGetAddressDetails_New(ds1, conn);
                                MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetail(RequestID, conn);
                                //SendMail(MailBody + MailGetFooter(), MailSubject, EmailGetEmailIDsByUserID(Convert.ToInt64(userid), conn));
                                SendMail(MailBody + MailGetFooter(), MailSubject, EmailGetEmailIDsByUserID_New(Convert.ToInt64(userid), conn));
                                SaveCorrespondsData_New(RequestID, MailSubject, MailBody, DepartmentID, Status, conn);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "Sp_EnterErrorTracking";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("Data", ex.Data.ToString());
                    cmd.Parameters.AddWithValue("GetType", ex.Source.ToString());
                    cmd.Parameters.AddWithValue("InnerException", "Error");
                    cmd.Parameters.AddWithValue("Message", ex.Message.ToString());
                    cmd.Parameters.AddWithValue("Source", " EmailSendWhenRequestSubmit");
                    cmd.Parameters.AddWithValue("DateTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("UserID", 20000);
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
                Result = 3;//mail not sent to requestor.
            }
            finally { }
        }



        protected void SaveCorrespondsData_New(long RequestID, string MailSubject, string MailBody, long DepartmentID, long StatusID, string[] conn)
        {
            //tCorrespond Cor = new tCorrespond();
            //using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            //{
            //    Cor.OrderHeadId = RequestID;
            //    Cor.MessageTitle = MailSubject;
            //    Cor.Message = MailBody;
            //    Cor.date = DateTime.Now;
            //    Cor.MessageSource = "Correspondance";
            //    Cor.MessageType = "Information";
            //    Cor.DepartmentID = DepartmentID;
            //    Cor.CurrentOrderStatus = StatusID;
            //    Cor.MailStatus = 1;
            //    Cor.Archive = false;
            //    db.tCorresponds.AddObject(Cor);
            //    db.SaveChanges();
            //}

            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_SaveCorrespondsData";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@OrderHeadId", RequestID);
                    cmd.Parameters.AddWithValue("@MessageTitle", MailSubject);
                    cmd.Parameters.AddWithValue("@MessageBody", MailBody);
                    cmd.Parameters.AddWithValue("@MessageSource", "Correspondance");
                    cmd.Parameters.AddWithValue("@MessageType", "Information");
                    cmd.Parameters.AddWithValue("@DepartmentID", DepartmentID);
                    cmd.Parameters.AddWithValue("@CurrentOrderStatus", StatusID);
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
            }
            catch (Exception ex)
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "Sp_EnterErrorTracking";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("Data", ex.Data.ToString());
                    cmd.Parameters.AddWithValue("GetType", ex.Source.ToString());
                    cmd.Parameters.AddWithValue("InnerException", "Error");
                    cmd.Parameters.AddWithValue("Message", ex.Message.ToString());
                    cmd.Parameters.AddWithValue("Source", RequestID + "| SaveCorrespondsData_New");
                    cmd.Parameters.AddWithValue("DateTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("UserID", Convert.ToString(RequestID));
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
            }
            
           
        }

        protected string EmailGetEmailIDsByUserID_New(long UserID, string[] conn)
        {
            //string result;
            //using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            //{
            //    mUserProfileHead user = new mUserProfileHead();
            //    user = db.mUserProfileHeads.Where(u => u.ID == UserID).FirstOrDefault();
            //    result = user.EmailID;
            //    return result;
            //}
            string result = "";
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    SqlDataAdapter da = new SqlDataAdapter();
                    DataSet ds = new DataSet();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_EmailGetEmailIDsByUserID";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@UserID", UserID);
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        result = Convert.ToString(ds.Tables[0].Rows[0]["EmailID"].ToString());
                    }
                    cmd.Connection.Close();
                }
            }
            catch (Exception ex)
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "Sp_EnterErrorTracking";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("Data", ex.Data.ToString());
                    cmd.Parameters.AddWithValue("GetType", ex.Source.ToString());
                    cmd.Parameters.AddWithValue("InnerException", "Error");
                    cmd.Parameters.AddWithValue("Message", ex.Message.ToString());
                    cmd.Parameters.AddWithValue("Source", UserID + "| EmailGetEmailIDsByUserID_New");
                    cmd.Parameters.AddWithValue("DateTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("UserID", UserID);
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
                result = "Fail";
            }
            
            return result;
        }

        protected string EmailGetAddressDetails_New(DataSet dsRequest, string[] conn)
        {
            string location = "";
            string Address = "";
            string messageBody = "<font><b>Address Details:  </b> </font><br/><br/>";

            string htmlTableStart = "<table style=\"border-collapse:collapse; text-align:center;\" >";
            string htmlTableEnd = "</table>";
            string htmlHeaderRowStart = "<tr style =\"background-color:#6FA1D2; color:#ffffff;\">";
            string htmlHeaderRowEnd = "</tr>";
            string htmlTrStart = "<tr style =\"color:black; text-align: center;\">";
            string htmlTrEnd = "</tr>";
            string htmlTdStart = "<td style=\" border-color:black; border-style:solid; border-width:thin; padding: 5px; text-align: center;font-size: 10pt;\">";
            string htmlTdEnd = "</td>";

            messageBody += htmlTableStart;
            messageBody += htmlHeaderRowStart;
            messageBody += htmlTdStart + "Customer Address" + htmlTdEnd;
            messageBody += htmlTdStart + "Location" + htmlTdEnd;
            messageBody += htmlHeaderRowEnd;
            //if (Request.LocationID == 0)
            if (Convert.ToInt32(dsRequest.Tables[0].Rows[0]["LocationID"]) == 0)
            {
                location = "N/A";
            }
            else
            {
                
                DataSet dsGetLocations = new DataSet();
                //dsGetLocations = fillds("select AddressLine1 from tAddress where ID=" + Request.LocationID + "", conn);
                dsGetLocations = fillds("select AddressLine1 from tAddress where ID=" + Convert.ToInt32(dsRequest.Tables[0].Rows[0]["LocationID"]) + "", conn);
                location = dsGetLocations.Tables[0].Rows[0]["AddressLine1"].ToString();
            }
            DataSet dsGetAddressid = new DataSet();
            //dsGetAddressid = fillds("select AddressId from torderhead where Id=" + Request.ID + "", conn);
            dsGetAddressid = fillds("select AddressId from torderhead where Id=" + Convert.ToInt32(dsRequest.Tables[0].Rows[0]["ID"])  + "", conn);
            long AddressId = Convert.ToInt64(dsGetAddressid.Tables[0].Rows[0]["AddressId"].ToString());
            if (AddressId == 0)
            {
                Address = "N/A";
            }
            else
            {
                DataSet dsGetAddress = new DataSet();
                dsGetAddress = fillds("select AddressLine1 from tAddress where ID=" + AddressId + "", conn);
                Address = dsGetAddress.Tables[0].Rows[0]["AddressLine1"].ToString();
            }
            messageBody += htmlTrStart;
            messageBody += htmlTdStart + " " + Address + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + location + " " + htmlTdEnd;
            messageBody += htmlTrEnd;
            messageBody += htmlTableEnd;
            return messageBody;
        }

        protected string EMailGetRequestDetail_new(DataSet dsRequest, string[] conn)
        {
            //string result = "";

            DateTime date1 = new DateTime();
            //  date1 = Convert.ToDateTime(Request.OrderDate.Value.ToString());
            date1 = Convert.ToDateTime(dsRequest.Tables[0].Rows[0]["OrderDate"].ToString());
            string OrderDate = date1.ToString("dd/MM/yyyy");


            DateTime date2 = new DateTime();
            //date2 = Convert.ToDateTime(Request.Deliverydate.Value.ToString());
            date2 = Convert.ToDateTime(dsRequest.Tables[0].Rows[0]["Deliverydate"].ToString());
            string Deliverydate = date2.ToString("dd/MM/yyyy");

            string locationcode = "";
            string LocationName = "";
            string messageBody = "<font><b>Order Summary :  </b> </font><br/><br/>";

            string htmlTableStart = "<table style=\"border-collapse:collapse; text-align:center;\" >";
            string htmlTableEnd = "</table>";
            string htmlHeaderRowStart = "<tr style =\"background-color:#6FA1D2; color:#ffffff;\">";
            string htmlHeaderRowEnd = "</tr>";
            string htmlTrStart = "<tr style =\"color:black; text-align: center;\">";
            string htmlTrEnd = "</tr>";
            string htmlTdStart = "<td style=\" border-color:black; border-style:solid; border-width:thin; padding: 5px; text-align: center;font-size: 10pt;\">";
            string htmlTdEnd = "</td>";

            messageBody += htmlTableStart;

            messageBody += htmlHeaderRowStart;
            messageBody += htmlTdStart + "Order Date" + htmlTdEnd;
            messageBody += htmlTdStart + "Order Id" + htmlTdEnd;         
            messageBody += htmlTdStart + "Exp. Delivery Date" + htmlTdEnd;
            messageBody += htmlTdStart + "Status" + htmlTdEnd;
            messageBody += htmlTdStart + "Department" + htmlTdEnd;
            messageBody += htmlTdStart + "Location Code" + htmlTdEnd;
            messageBody += htmlTdStart + "Location Name" + htmlTdEnd;
            messageBody += htmlTdStart + "Request Type" + htmlTdEnd;
            messageBody += htmlTdStart + "Requested By" + htmlTdEnd;
            messageBody += htmlTdStart + "Remark" + htmlTdEnd;
            messageBody += htmlHeaderRowEnd;

            messageBody += htmlTrStart;
           
            messageBody += htmlTdStart + " " + OrderDate + " " + htmlTdEnd;
            //messageBody += htmlTdStart + " " + Request.OrderNo.ToString() + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + dsRequest.Tables[0].Rows[0]["OrderNo"].ToString() + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Deliverydate + " " + htmlTdEnd;
            // messageBody += htmlTdStart + " " + Request.RequestStatus + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + dsRequest.Tables[0].Rows[0]["RequestStatus"].ToString()  + " " + htmlTdEnd;
            // messageBody += htmlTdStart + " " + Request.SiteName + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + dsRequest.Tables[0].Rows[0]["SiteName"].ToString()  + " " + htmlTdEnd;

            //if (Request.LocationID == 0)
            if (Convert.ToInt32(dsRequest.Tables[0].Rows[0]["LocationID"]) == 0)
            {
                locationcode = "N/A";
                LocationName = "N/A";
            }
            else
            {
                DataSet dsGetLocation = new DataSet();
                //dsGetLocation = fillds("select LocationCode,LocationName from tAddress where ID=" + Request.LocationID + "", conn);
                dsGetLocation = fillds("select LocationCode,LocationName from tAddress where ID=" + Convert.ToInt32(dsRequest.Tables[0].Rows[0]["LocationID"]) + "", conn);

                locationcode = dsGetLocation.Tables[0].Rows[0]["LocationCode"].ToString();
                LocationName = dsGetLocation.Tables[0].Rows[0]["LocationName"].ToString();
            }
            messageBody += htmlTdStart + " " + locationcode + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + LocationName + " " + htmlTdEnd;
            //messageBody += htmlTdStart + " " + Request.Priority + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + dsRequest.Tables[0].Rows[0]["Priority"] + " " + htmlTdEnd;
            //messageBody += htmlTdStart + " " + Request.RequestByUserName + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + dsRequest.Tables[0].Rows[0]["RequestByUserName"] + " " + htmlTdEnd;
            // messageBody += htmlTdStart + " " + Request.Remark + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + dsRequest.Tables[0].Rows[0]["Remark"] + " " + htmlTdEnd;
            messageBody += htmlTrEnd;

            messageBody += htmlTableEnd;
            
            return messageBody;
        }


        protected string EMailGetRequestDetailVodafoneTechnical_New(DataSet dsRequest, string[] conn)
        {
            //string result = "";

            DateTime date1 = new DateTime();
            //date1 = Convert.ToDateTime(Request.OrderDate.Value.ToString());
            date1 = Convert.ToDateTime(dsRequest.Tables[0].Rows[0]["OrderDate"].ToString());
            //    long Orderstatus = long.Parse(ds1.Tables[0].Rows[0]["Status"].ToString());
            string OrderDate = date1.ToString("dd/MM/yyyy");


            DateTime date2 = new DateTime();
            //date2 = Convert.ToDateTime(Request.Deliverydate.Value.ToString());
            date2 = Convert.ToDateTime(dsRequest.Tables[0].Rows[0]["Deliverydate"].ToString());
            string Deliverydate = date2.ToString("dd/MM/yyyy");

            string locationcode = "";
            string LocationName = "";
            string messageBody = "<font><b>Order Summary :  </b> </font><br/><br/>";

            string htmlTableStart = "<table style=\"border-collapse:collapse; text-align:center;\" >";
            string htmlTableEnd = "</table>";
            string htmlHeaderRowStart = "<tr style =\"background-color:#6FA1D2; color:#ffffff;\">";
            string htmlHeaderRowEnd = "</tr>";
            string htmlTrStart = "<tr style =\"color:black; text-align: center;\">";
            string htmlTrEnd = "</tr>";
            string htmlTdStart = "<td style=\" border-color:black; border-style:solid; border-width:thin; padding: 5px; text-align: center;font-size: 10pt;\">";
            string htmlTdEnd = "</td>";

            messageBody += htmlTableStart;

            messageBody += htmlHeaderRowStart;
            messageBody += htmlTdStart + "Order Date" + htmlTdEnd;
            messageBody += htmlTdStart + "Order Id" + htmlTdEnd;
            //messageBody += htmlTdStart + "Customer Order Reference No." + htmlTdEnd;          
            messageBody += htmlTdStart + "Exp. Delivery Date" + htmlTdEnd;
            messageBody += htmlTdStart + "Status" + htmlTdEnd;
            messageBody += htmlTdStart + "Department" + htmlTdEnd;
            messageBody += htmlTdStart + "Location Code" + htmlTdEnd;
            messageBody += htmlTdStart + "Location Name" + htmlTdEnd;
            messageBody += htmlTdStart + "Request Type" + htmlTdEnd;
            messageBody += htmlTdStart + "Requested By" + htmlTdEnd;
            messageBody += htmlTdStart + "Remark" + htmlTdEnd;

            messageBody += htmlTdStart + "Project Type" + htmlTdEnd;
            messageBody += htmlTdStart + "Site Code" + htmlTdEnd;
            messageBody += htmlTdStart + "Site Name" + htmlTdEnd;
            messageBody += htmlTdStart + "Latitude" + htmlTdEnd;
            messageBody += htmlTdStart + "Longitude" + htmlTdEnd;
            messageBody += htmlTdStart + "Access Requirement" + htmlTdEnd;
            messageBody += htmlHeaderRowEnd;

            messageBody += htmlTrStart;
            messageBody += htmlTdStart + " " + OrderDate + " " + htmlTdEnd;
            //messageBody += htmlTdStart + " " + Request.OrderNo.ToString() + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + dsRequest.Tables[0].Rows[0]["OrderNo"].ToString() + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Deliverydate + " " + htmlTdEnd;
            //  messageBody += htmlTdStart + " " + Request.RequestStatus + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + dsRequest.Tables[0].Rows[0]["RequestStatus"].ToString() + " " + htmlTdEnd;
            // messageBody += htmlTdStart + " " + Request.SiteName + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + dsRequest.Tables[0].Rows[0]["SiteName"].ToString() + " " + htmlTdEnd;

            if (Convert.ToInt32(dsRequest.Tables[0].Rows[0]["LocationID"]) == 0)
            //if (Request.LocationID == 0)
            {
                locationcode = "N/A";
                LocationName = "N/A";
            }
            else
            {
                DataSet dsGetLocation = new DataSet();
                // dsGetLocation = fillds("select LocationCode,LocationName from tAddress where ID=" + Request.LocationID + "", conn);
                dsGetLocation = fillds("select LocationCode,LocationName from tAddress where ID=" + Convert.ToInt32(dsRequest.Tables[0].Rows[0]["LocationID"]) + "", conn);

                locationcode = dsGetLocation.Tables[0].Rows[0]["LocationCode"].ToString();
                LocationName = dsGetLocation.Tables[0].Rows[0]["LocationName"].ToString();
            }
            messageBody += htmlTdStart + " " + locationcode + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + LocationName + " " + htmlTdEnd;
            //messageBody += htmlTdStart + " " + Request.Priority + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + dsRequest.Tables[0].Rows[0]["Priority"].ToString() + " " + htmlTdEnd;
            // messageBody += htmlTdStart + " " + Request.RequestByUserName + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + dsRequest.Tables[0].Rows[0]["RequestByUserName"].ToString() + " " + htmlTdEnd;
            //messageBody += htmlTdStart + " " + Request.Remark + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + dsRequest.Tables[0].Rows[0]["Remark"].ToString() + " " + htmlTdEnd;
            //messageBody += htmlTdStart + " " + Request.ProjectType + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + dsRequest.Tables[0].Rows[0]["ProjectType"].ToString() + " " + htmlTdEnd;
            //messageBody += htmlTdStart + " " + Request.SiteCode + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + dsRequest.Tables[0].Rows[0]["SiteCode"].ToString() + " " + htmlTdEnd;
            //messageBody += htmlTdStart + " " + Request.SiteNM + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + dsRequest.Tables[0].Rows[0]["SiteNM"].ToString() + " " + htmlTdEnd;
            //messageBody += htmlTdStart + " " + Request.Latitude + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + dsRequest.Tables[0].Rows[0]["Latitude"].ToString() + " " + htmlTdEnd;
            //messageBody += htmlTdStart + " " + Request.Longitude + " " + Longitudez;
            messageBody += htmlTdStart + " " + dsRequest.Tables[0].Rows[0]["Longitude"].ToString() + " " + htmlTdEnd;
            //messageBody += htmlTdStart + " " + Request.AccessRequirement + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + dsRequest.Tables[0].Rows[0]["AccessRequirement"].ToString() + " " + htmlTdEnd;

            messageBody += htmlTrEnd;

            messageBody += htmlTableEnd;

            //result = "Order Id. : <b>" + Request.ID.ToString() + "</b>" +
            //         "<br/>" +
            //         "Customer Order Reference No.: <b>" + Request.OrderNumber+ "</b>"+
            //         "<br/>" +
            //         "Order Date : <b>" + Request.OrderDate.Value.ToString("dd-MMM-yyyy") + "</b>" +
            //         "<br/>" +
            //         "Exp. Delivery Date : <b>" + Request.Deliverydate.Value.ToString("dd-MMM-yyyy") + "</b>" +
            //         "<br/>" +
            //         "Status : <b>" + Request.RequestStatus + "</b>" +
            //         "<br/>" +
            //         "Department : <b>" + Request.SiteName + "</b>" +
            //         "<br/>" +
            //         "Request Type : <b>" + Request.Priority + "</b>" +
            //         "<br/>" +
            //         "Requested By : <b>" + Request.RequestByUserName + "</b>" +
            //         "<br/>" +
            //         "Remark : <b>" + Request.Remark + "</b>";                    

            return messageBody;
        }


        public DataSet GetRequestHeadByRequestID(string RequestID, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GWC_SP_GetRequestHeadByRequestIDs";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@RequestIDs", RequestID);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }

        public void EmailSendWhenRequestreSubmit(long RequestID, string[] conn)
        {
            int Result = 0;
            try
            {
                string MailSubject;
                string MailBody;

                using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
                {

                    GWC_SP_GetRequestHeadByRequestIDs_Result Request = new GWC_SP_GetRequestHeadByRequestIDs_Result();
                    Request = db.GWC_SP_GetRequestHeadByRequestIDs(RequestID.ToString()).FirstOrDefault();
                    long userid = 0;
                    long Orderstatus = long.Parse(Request.Status.ToString());
                    long DepartmentID = long.Parse(Request.SiteID.ToString());
                    long Status = long.Parse(Request.Status.ToString());
                    string ISProjectSite = "";
                    ISProjectSite = ISProjectSiteDetailsNew(Convert.ToString(Request.parentid), conn);
                    DataSet dsMailSubBody = new DataSet();
                    dsMailSubBody = fillds("select * from mMessageEMailTemplates where ActivityID=(select Id from mDropdownValues where StatusID=2 and value='Order Submit') and MessageID=(select Id from mDropdownValues where Value='Information') and DepartmentID=" + DepartmentID + "", conn);
                    if (dsMailSubBody.Tables[0].Rows.Count > 0)
                    {
                        DataSet ds = new DataSet();
                        ds = GetFinalLevelapproval(RequestID, conn);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            for (int p = 0; p <= ds.Tables[0].Rows.Count - 1; p++)
                            {
                                userid = Convert.ToInt64(ds.Tables[0].Rows[p]["userid"].ToString());
                                if (Status == 2)
                                {
                                    MailSubject = dsMailSubBody.Tables[0].Rows[0]["MailSubject"].ToString() + ", Order No # " + Request.OrderNo + "";
                                }
                                else
                                {
                                    string CrntStatus = Request.RequestStatus.ToString();
                                    MailSubject = "Order " + CrntStatus + ", Order No # " + Request.OrderNo + "";
                                }
                                MailBody = "Dear " + GetUserNameByUserID(userid, conn) + ", <br/>";
                                MailBody = MailBody + "This is an automatically generated message in reference to a order request. An approval action is required before OMS can proceed. <br/> Thank you for your request. Before we can proceed, we need approver's formal approval to proceed. <br/>";
                                MailBody = MailBody + dsMailSubBody.Tables[0].Rows[0]["MailBody"].ToString();

                                if (ISProjectSite == "Yes")/// if (Request.parentid == 10266)
                                {
                                    MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetailVodafoneTechnical(Request, conn);
                                }
                                else
                                {
                                    MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                                }
                                //MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                                MailBody = MailBody + "<br/><br/>" + EmailGetAddressDetails(Request, conn);
                                MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetail(RequestID, conn);
                                SendMail(MailBody + MailGetFooter(), MailSubject, EmailGetEmailIDsByUserID(Convert.ToInt64(userid), conn));
                                SaveCorrespondsData(RequestID, MailSubject, MailBody, DepartmentID, Status, conn);
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "Sp_EnterErrorTracking";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("Data", ex.Data.ToString());
                    cmd.Parameters.AddWithValue("GetType", ex.Source.ToString());
                    cmd.Parameters.AddWithValue("InnerException", "Error");
                    cmd.Parameters.AddWithValue("Message", ex.Message.ToString());
                    cmd.Parameters.AddWithValue("Source", " EmailSendWhenRequestSubmit");
                    cmd.Parameters.AddWithValue("DateTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("UserID", 20000);
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
                Result = 3;//mail not sent to requestor.
            }
            finally { }
            //  return Result;
        }


        public DataSet GetFinalLevelapproval(long OrdId, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetFinalLevelapproval";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@UserID", OrdId);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }

        public void AddIntomMessageTransNew(long RequestID, string[] conn)
        {

            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                //mMessageTran Msg = new mMessageTran();
                string Msgnew = "";

                GWC_VW_MsgOrderHead MsgHD = new GWC_VW_MsgOrderHead();
                MsgHD = db.GWC_VW_MsgOrderHead.Where(h => h.Id == RequestID).FirstOrDefault();

                string ContactPerson2 = MsgHD.ContactP2;

                string ContactPerson2Names = "";
                if (ContactPerson2 == "NA" || ContactPerson2 == "" || ContactPerson2 == null) { ContactPerson2Names = "NA"; }
                else
                { ContactPerson2Names = GetContactPersonNames(ContactPerson2, conn); }

                /*New Code For Contact 2 MobileNo*/
                string ContactMobileNo = "";
                if (ContactPerson2Names == "NA")
                {
                    ContactMobileNo = MsgHD.Con1MobileNo.ToString();
                }
                else
                {
                    ContactMobileNo = GetContactPersonMobileNo(ContactPerson2, conn);
                }
                /*New Code For Contact 2 MobileNo*/

                //Added by suraj 
                string ContactMobile2 = "";
                ContactMobile2 = GetContactMobile2(RequestID, conn);
                //Get PaymentMethodValue
                string PaymentMethodValue = "";
                PaymentMethodValue = GetPaymentMethodValue(RequestID, conn);
                //address2 and paymentmethdnm
                string address2 = "", paymentmethdnm = "";
                DataTable dtadd2 = new DataTable();
                dtadd2 = GetAddressLine2(RequestID, conn);
                if (dtadd2.Rows.Count > 0)
                {
                    address2 = dtadd2.Rows[0]["AddressLine2"].ToString();
                    paymentmethdnm = dtadd2.Rows[0]["MethodName"].ToString();
                }
                // Msg.MsgDescription = MsgHD.Id + " | " + CheckString(MsgHD.OrderNumber) + " | " + MsgHD.Orderdate.Value.ToShortDateString() + " | " + MsgHD.Deliverydate.Value.ToShortDateString() + " | " + MsgHD.StoreCode + " | " + MsgHD.Con1 + " | " + ContactPerson2Names + " | " + CheckString(MsgHD.AddressLine1) + " | " + CheckString(MsgHD.Remark) + " | " + MsgHD.OrderNo + " | " + MsgHD.InvoiceNo + " | " + MsgHD.LocationCode + " | " + MsgHD.RequestorName + " | " + MsgHD.RequestorMobileNo + " | " + MsgHD.ConsigneeName + " | " + MsgHD.ConsigneeAddress + " | " + MsgHD.ConsigneePhone + " | " + ContactMobileNo;
                Msgnew = MsgHD.Id + " | " + CheckString(MsgHD.OrderNumber) + " | " + MsgHD.Orderdate.Value.ToShortDateString() + " | " + MsgHD.Deliverydate.Value.ToShortDateString() + " | " + MsgHD.StoreCode + " | " + MsgHD.Con1 + " | " + ContactPerson2Names + " | " + CheckString(MsgHD.AddressLine1) + " | " + CheckString(MsgHD.Remark) + " | " + MsgHD.OrderNo + " | " + MsgHD.InvoiceNo + " | " + MsgHD.LocationCode + " | " + MsgHD.RequestorName + " | " + MsgHD.RequestorMobileNo + " | " + MsgHD.ConsigneeName + " | " + MsgHD.ConsigneeAddress + " | " + MsgHD.ConsigneePhone + " | " + ContactMobileNo + " | " + ContactMobile2 + " | " + CheckString(address2) + " | " + paymentmethdnm + " | " + PaymentMethodValue + " | " + MsgHD.ProjectType + " | " + MsgHD.SiteCode + " | " + MsgHD.SiteName + " | " + MsgHD.Latitude + " | " + MsgHD.Longitude + " | " + MsgHD.AccessRequirement;
                // old code DataSet ds = new DataSet();
                //ds = fillds("select * from GWC_VW_MsgOrderDetail where OrderHeadId=" + RequestID + " ", conn);
                //int Cnt = ds.Tables[0].Rows.Count;
                //if (Cnt > 0)
                //{
                //    for (int i = 0; i <= Cnt - 1; i++)
                //    {
                //        long PrdID = long.Parse(ds.Tables[0].Rows[i]["SkuId"].ToString());
                //        mProduct prd = new mProduct();
                //        prd = db.mProducts.Where(p => p.ID == PrdID).FirstOrDefault();
                //        if (prd.GroupSet == "Yes")
                //        {
                //            DataSet dsBomProds = new DataSet();
                //            dsBomProds = fillds("select SKUId,Quantity from mBOMDetail BD where BD.BOMheaderId=" + PrdID + "", conn);
                //            if (dsBomProds.Tables[0].Rows.Count > 0)
                //            {
                //                for (int b = 0; b <= dsBomProds.Tables[0].Rows.Count - 1; b++)
                //                {
                //                    decimal bomQty = decimal.Parse(dsBomProds.Tables[0].Rows[b]["Quantity"].ToString());
                //                    decimal Qty = decimal.Parse(ds.Tables[0].Rows[i]["OrderQty"].ToString());
                //                    decimal FinalQty = Qty * bomQty;

                //                    long bomPrd = long.Parse(dsBomProds.Tables[0].Rows[b]["SKUId"].ToString());
                //                    DataSet dsPrdName = new DataSet();
                //                    dsPrdName = fillds("select ProductCode from mproduct where ID=" + bomPrd + "", conn);
                //                    string ProductCode = dsPrdName.Tables[0].Rows[0]["ProductCode"].ToString();

                //                    // Msg.MsgDescription = Msg.MsgDescription + " | " + ProductCode + " | " + ds.Tables[0].Rows[i]["UOM"].ToString() + " | " + FinalQty;
                //                    Msgnew = Msgnew + " | " + ProductCode + " | " + ds.Tables[0].Rows[i]["UOM"].ToString() + " | " + FinalQty;
                //                }
                //            }
                //        }
                //        else if (prd.GroupSet == "No")
                //        {
                //            //  Msg.MsgDescription = Msg.MsgDescription + " | " + ds.Tables[0].Rows[i]["Prod_Code"].ToString() + " | " + ds.Tables[0].Rows[i]["UOM"].ToString() + " | " + ds.Tables[0].Rows[i]["OrderQty"].ToString();
                //            Msgnew = Msgnew + " | " + ds.Tables[0].Rows[i]["Prod_Code"].ToString() + " | " + ds.Tables[0].Rows[i]["UOM"].ToString() + " | " + ds.Tables[0].Rows[i]["OrderQty"].ToString();
                //        }
                //    }
                //}

                //***condition to check if Serial No changes flag applicable to Company i.e. Vodafone technical
                DataSet ds = new DataSet();
                if (MsgHD.ChkApproval == "Yes")
                {
                    ds = GetMessageserialvalues(RequestID, conn);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        //  Msg.MsgDescription = Msg.MsgDescription + ds.Tables[0].Rows[0]["SerialNo"].ToString();
                        Msgnew = Msgnew + ds.Tables[0].Rows[0]["SerialNo"].ToString();
                    }
                }
                else
                {
                    ds = fillds("select * from GWC_VW_MsgOrderDetail where OrderHeadId=" + RequestID + " ", conn);
                    int Cnt = ds.Tables[0].Rows.Count;
                    if (Cnt > 0)
                    {
                        for (int i = 0; i <= Cnt - 1; i++)
                        {
                            long PrdID = long.Parse(ds.Tables[0].Rows[i]["SkuId"].ToString());
                            mProduct prd = new mProduct();
                            prd = db.mProducts.Where(p => p.ID == PrdID).FirstOrDefault();
                            if (prd.GroupSet == "Yes")
                            {
                                DataSet dsBomProds = new DataSet();
                                dsBomProds = fillds("select SKUId,Quantity from mBOMDetail BD where BD.BOMheaderId=" + PrdID + "", conn);
                                if (dsBomProds.Tables[0].Rows.Count > 0)
                                {
                                    for (int b = 0; b <= dsBomProds.Tables[0].Rows.Count - 1; b++)
                                    {
                                        decimal bomQty = decimal.Parse(dsBomProds.Tables[0].Rows[b]["Quantity"].ToString());
                                        decimal Qty = decimal.Parse(ds.Tables[0].Rows[i]["OrderQty"].ToString());
                                        decimal FinalQty = Qty * bomQty;

                                        long bomPrd = long.Parse(dsBomProds.Tables[0].Rows[b]["SKUId"].ToString());
                                        DataSet dsPrdName = new DataSet();
                                        dsPrdName = fillds("select ProductCode from mproduct where ID=" + bomPrd + "", conn);
                                        string ProductCode = dsPrdName.Tables[0].Rows[0]["ProductCode"].ToString();

                                        Msgnew = Msgnew + " | " + ProductCode + " | " + ds.Tables[0].Rows[i]["UOM"].ToString() + " | " + "NA" + " | " + "No" + " | " + FinalQty;
                                    }
                                }
                            }
                            else if (prd.GroupSet == "No")
                            {
                                Msgnew = Msgnew + " | " + ds.Tables[0].Rows[i]["Prod_Code"].ToString() + " | " + ds.Tables[0].Rows[i]["UOM"].ToString() + " | " + ds.Tables[0].Rows[i]["SerialNumber"].ToString() + " | " + ds.Tables[0].Rows[i]["serialflag"].ToString() + " | " + ds.Tables[0].Rows[i]["OrderQty"].ToString();
                            }
                        }
                    }
                }


                //Msg.MessageHdrId = 1;
                //Msg.Object = "Order";
                //// Msg.Destination = "WMS";
                //Msg.Destination = MsgHD.StoreCode;
                //Msg.Status = 0;
                //Msg.CreationDate = DateTime.Now;
                //Msg.Createdby = "OMS";

                //db.mMessageTrans.AddObject(Msg);
                //db.SaveChanges();

                using (SqlCommand c = new SqlCommand())
                {
                    c.CommandType = CommandType.StoredProcedure;
                    c.CommandText = "SP_SavemMessageTrans";
                    c.Connection = svr.GetSqlConn(conn);
                    c.Parameters.Clear();
                    c.Parameters.AddWithValue("MessageHdrId", "1");
                    c.Parameters.AddWithValue("MsgDescription", Msgnew);
                    c.Parameters.AddWithValue("Object", "Order");
                    c.Parameters.AddWithValue("Destination", MsgHD.StoreCode);
                    c.Parameters.AddWithValue("Status", "0");
                    c.Parameters.AddWithValue("CreationDate", DateTime.Now);
                    c.Parameters.AddWithValue("Createdby", "OMS");
                    c.ExecuteNonQuery();
                }
            }

        }

        #endregion

        #region New vodafone technical Import Code

        public DataSet GetUserCustomer(long UserID, long CompanyID, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetImportCustomer";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@UserID", UserID);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyID);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }

        #endregion


        public DataSet GetReportDataNew(string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_VW_1";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }

        public DataSet GetReportDataNew1(string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_VW_2";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }

        public DataSet GetReportDataNew3(string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_VW_3";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }



        public DataSet GetDriverDetails(string[] conn)
        {
            //  DataSet ds = new DataSet();
            //  ds = fillds("select ID,FirstName+' '+LastName Name,EmailID,MobileNo from muserprofilehead where Usertype='Driver' ", conn);
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_NormalDriverList";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@filter", "");
                da.SelectCommand = cmd;
                da.Fill(ds);

            }
            return ds;
        }

        public DataSet GetFilteredDriverList(string SearchValue, string[] conn)
        {
            //  DataSet ds = new DataSet();
            //  ds = fillds("select ID,FirstName+' '+LastName Name,EmailID,MobileNo from muserprofilehead where Usertype='Driver' and (Firstname like '%" + SearchValue + "%' or LastName like '%" + SearchValue + "%' or EmailID like '%" + SearchValue + "%') ", conn);

            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_NormalDriverList";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@filter", SearchValue);
                da.SelectCommand = cmd;
                da.Fill(ds);

            }
            return ds;
        }



        #region Bulk Order Import


        public int SetApproverDataafterSaveBulk(string paraCurrentObjectName, long paraReferenceID, string paraUserID, int StatusID, long DepartmentID, long PreviousStatusID, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {


                string autoapproval = "False";
                var ApprovalDetail = 0;
                int Rslt = 0, Suecc = 0;
                try
                {
                    addUpdateGrandTotal(paraReferenceID, conn);
                    DataSet dsChkRecordOfUser = new DataSet();
                    dsChkRecordOfUser = fillds("select * from tOrderwiseAccess  where orderID=" + paraReferenceID + " and Usertype='User'", conn);
                    if (dsChkRecordOfUser.Tables[0].Rows.Count == 0)
                    {
                        int IsPriceChenged = 0;
                        DataSet dsIsPriceChange = new DataSet();
                        dsIsPriceChange = fillds("select * from torderdetail where orderheadid=" + paraReferenceID + " and IsPriceChange=1", conn);
                        int rowcount = dsIsPriceChange.Tables[0].Rows.Count;
                        if (rowcount > 0)
                        { IsPriceChenged = 1; }
                        else { IsPriceChenged = 0; }

                        tOrderWiseAccess ODA = new tOrderWiseAccess();
                        ODA.UserApproverID = long.Parse(paraUserID);
                        ODA.ApprovalLevel = 0;

                        if (StatusID == 1) { }
                        else
                        {
                            if (IsPriceChenged == 1)
                            {
                                ODA.PriceEdit = false;
                                ODA.SkuQtyEdit = false;

                                /*Coading for New Status Pending For Financial Approver Start*/
                                tOrderHead OH = new tOrderHead();
                                OH = (from or in db.tOrderHeads
                                      where or.Id == paraReferenceID
                                      select or).FirstOrDefault();
                                OH.Status = 31; //Pending For Financial Approver
                                db.SaveChanges();
                                /*Coading for New Status Pending For Financial Approver End*/
                            }
                            else
                            {
                                ODA.PriceEdit = false;
                                ODA.SkuQtyEdit = true;
                            }
                            ODA.UserType = "User";
                            ODA.OrderID = paraReferenceID;
                            ODA.Date = DateTime.Now;
                            db.AddTotOrderWiseAccesses(ODA);
                            db.SaveChanges();
                        }
                    }



                    if (PreviousStatusID == 2) { }
                    else
                    {
                        if (StatusID == 2)
                        {
                            /* Insert record of Approval Level 1 in tApprovalTrans Table===>>>> */

                            /* If Price is change of product then add Financial Approver at Approval Level 1  START*/
                            /*Check if Price is Changed or not*/
                            int IsPriceChenged = 0;
                            DataSet dsIsPriceChange = new DataSet();
                            dsIsPriceChange = fillds("select * from torderdetail where orderheadid=" + paraReferenceID + " and IsPriceChange=1", conn);
                            int rowcount = dsIsPriceChange.Tables[0].Rows.Count;
                            if (rowcount > 0)
                            {
                                IsPriceChenged = 1; long FinanAppId = 0;
                                DataSet dsFinanAppID = new DataSet();
                                dsFinanAppID = fillds("select FinApproverID from mterritory where id=" + DepartmentID + "", conn);
                                FinanAppId = Convert.ToInt64(dsFinanAppID.Tables[0].Rows[0]["FinApproverID"].ToString());

                                using (SqlCommand cmd = new SqlCommand())
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.CommandText = "SP_Insert_tapprovaltrans";
                                    cmd.Connection = svr.GetSqlConn(conn);
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("OrderId", paraReferenceID);
                                    cmd.Parameters.AddWithValue("StoreId", DepartmentID);
                                    cmd.Parameters.AddWithValue("UserId", Convert.ToInt64(paraUserID));
                                    cmd.Parameters.AddWithValue("ApprovalId", 1);
                                    cmd.Parameters.AddWithValue("ApproverID", FinanAppId);
                                    cmd.Parameters.AddWithValue("Status", StatusID);
                                    cmd.ExecuteNonQuery();
                                    cmd.Connection.Close();
                                }



                                /*Add Record Of User into table tOrderWiseAccess*/
                                tOrderWiseAccess ODA = new tOrderWiseAccess();
                                ODA.UserApproverID = FinanAppId;
                                ODA.ApprovalLevel = 1;
                                ODA.PriceEdit = true;
                                ODA.SkuQtyEdit = false;
                                ODA.UserType = "Financial Approver";
                                ODA.OrderID = paraReferenceID;
                                ODA.Date = DateTime.Now;
                                ODA.ApproverLogic = "AND";
                                db.AddTotOrderWiseAccesses(ODA);
                                db.SaveChanges();
                                // CHeck For Department Auto Approval Flag if True do not add approvars and send request details to ERP API for Approval **

                                autoapproval = CheckStoreERPAutoApproved(DepartmentID, conn);
                                if (autoapproval != "True")
                                {
                                    /*Add Record Of User into table tOrderWiseAccess*/
                                    AddAllApprovalLevel(IsPriceChenged, paraReferenceID, DepartmentID, conn);
                                }
                                // Rslt = EmailSendToApprover(FinanAppId, paraReferenceID, 0, conn);
                            }
                            else
                            {

                                // CHeck For Department Auto Approval Flag if True do not add approvars and send request details to ERP API for Approval **

                                autoapproval = CheckStoreERPAutoApproved(DepartmentID, conn);
                                if (autoapproval != "Ture")
                                {
                                    IsPriceChenged = 0;
                                    /* If Price is change of product then add Financial Approver at Approval Level 1  END*/
                                    /*Add Record Of User into table tOrderWiseAccess*/
                                    AddAllApprovalLevel(IsPriceChenged, paraReferenceID, DepartmentID, conn);

                                    /*New Code After tOrderWiseAccess able added for order wise approval level start*/
                                    DataSet dsFirstApprover = new DataSet();
                                    dsFirstApprover = fillds("select OW.ID,OW.UserApproverID,OW.ApprovalLevel,OW.PriceEdit,OW.SkuQtyEdit,OW.UserType,OW.OrderID,OW.ApproverLogic ,Dl.DeligateTo from tOrderWiseAccess  OW left outer join tOrderHead OH on OW.OrderID=OH.ID left outer join mDeligate AS Dl ON OW.UserApproverID = Dl.DeligateFrom and CONVERT(VARCHAR(10), GETDATE(), 111) <=Convert(VARCHAR(10), Dl.ToDate,111) and CONVERT(VARCHAR(10), GETDATE(), 111) >=Convert(VARCHAR(10), Dl.FromDate,111)  and OH.StoreId=Dl.DeptID where OW.OrderID=" + paraReferenceID + " and OW.UserType != 'User' and OW.ApprovalLevel=1", conn);
                                    int CntFirstApprover = dsFirstApprover.Tables[0].Rows.Count;
                                    if (CntFirstApprover > 0)
                                    {
                                        for (int i = 0; i <= CntFirstApprover - 1; i++)
                                        {
                                            using (SqlCommand cmd1 = new SqlCommand())
                                            {
                                                cmd1.CommandType = CommandType.StoredProcedure;
                                                cmd1.CommandText = "SP_Insert_tapprovaltrans";
                                                cmd1.Connection = svr.GetSqlConn(conn);
                                                cmd1.Parameters.Clear();
                                                cmd1.Parameters.AddWithValue("OrderId", paraReferenceID);
                                                cmd1.Parameters.AddWithValue("StoreId", DepartmentID);
                                                cmd1.Parameters.AddWithValue("UserId", Convert.ToInt64(paraUserID));
                                                cmd1.Parameters.AddWithValue("ApprovalId", 1);
                                                cmd1.Parameters.AddWithValue("ApproverID", Convert.ToInt64(dsFirstApprover.Tables[0].Rows[i]["UserApproverID"].ToString()));
                                                cmd1.Parameters.AddWithValue("Status", StatusID);
                                                cmd1.ExecuteNonQuery();
                                            }

                                            ApprovalDetail = 1;

                                            /*Send Email to Approvers*/
                                            //  Rslt = EmailSendToApprover(Convert.ToInt64(dsFirstApprover.Tables[0].Rows[i]["UserApproverID"].ToString()), paraReferenceID, i, conn);
                                        }
                                    }
                                }
                                else
                                {
                                    // If Auto Approval True send to ERP API for Approval **



                                    /* Update tProductstockDetails Reserve Quantity & Available Balance START>>>>>>>> */
                                    UpdateTProductStockReserveQtyAvailBalance(paraReferenceID, conn);
                                    /* <<<<<<<<Update tProductstockDetails Reserve Quantity & Available Balance END */
                                    Suecc = 1;

                                }
                            }

                            /*New Code After tOrderWiseAccess able added for order wise approval level end*/

                            tApprovalTran APRT = new tApprovalTran();
                            APRT = db.tApprovalTrans.Where(rec => rec.OrderId == paraReferenceID).FirstOrDefault();
                            if (APRT != null)
                            {

                                Rslt = EmailSendWhenRequestSubmit(paraReferenceID, conn); //if Rslt =2 then mail sent to requestor else if Rslt=3 then mail not sent to requestoe
                                                                                          /*Insert record of Auto Cancellation Reminder & Approval reminder in tCorrespond table START*/
                                mTerritory Dept = new mTerritory();
                                Dept = db.mTerritories.Where(r => r.ID == DepartmentID).FirstOrDefault();
                                long OrderCancelDays = 0, ApprovalReminderSchedule = 0, AutoCancelReminderSchedule = 0;
                                if (Dept != null)
                                {
                                    OrderCancelDays = long.Parse(Dept.cancelDays.ToString());
                                    ApprovalReminderSchedule = long.Parse(Dept.ApproRemSchedul.ToString());
                                    AutoCancelReminderSchedule = long.Parse(Dept.AutoRemSchedule.ToString());
                                }

                                DataSet dsGetOrderDate = new DataSet();
                                dsGetOrderDate = fillds("select OrderDate from torderhead where id=" + paraReferenceID + "", conn);
                                DateTime OrdrDate = Convert.ToDateTime(dsGetOrderDate.Tables[0].Rows[0]["OrderDate"].ToString());

                                DataSet dsAutocancel = new DataSet();
                                dsAutocancel = fillds("select * from mMessageEMailTemplates where ActivityID=(select Id from mDropdownValues where StatusID=10) and MessageID=(select Id from mDropdownValues where Value='Reminder') and DepartmentID=" + DepartmentID + " ", conn);
                                if (dsAutocancel.Tables[0].Rows.Count > 0)
                                {
                                    tCorrespond Cor = new tCorrespond();
                                    Cor.OrderHeadId = paraReferenceID;
                                    Cor.MessageTitle = dsAutocancel.Tables[0].Rows[0]["MailSubject"].ToString();
                                    Cor.Message = dsAutocancel.Tables[0].Rows[0]["MailBody"].ToString();
                                    Cor.date = DateTime.Now;
                                    Cor.MessageSource = "Scheduler";
                                    Cor.MessageType = "Reminder";
                                    Cor.DepartmentID = DepartmentID;
                                    // Cor.OrderDate = DateTime.Now;
                                    Cor.OrderDate = OrdrDate;
                                    Cor.CurrentOrderStatus = StatusID;
                                    Cor.MailStatus = 0;
                                    Cor.OrderCancelDays = OrderCancelDays;
                                    Cor.AutoCancelReminderSchedule = AutoCancelReminderSchedule;
                                    //Cor.ApprovalReminderSchedule = ApprovalReminderSchedule;
                                    Cor.NxtAutoCancelReminderDate = DateTime.Now.AddDays(AutoCancelReminderSchedule);
                                    Cor.Archive = false;

                                    db.tCorresponds.AddObject(Cor);
                                    db.SaveChanges();
                                }

                                DataSet dsApprovalReminder = new DataSet();
                                dsApprovalReminder = fillds("select * from mMessageEMailTemplates where ActivityID=(select Id from mDropdownValues where StatusID=2 and value='Order Submit') and MessageID=(select Id from mDropdownValues where Value='Reminder') and DepartmentID=" + DepartmentID + " ", conn);
                                if (dsApprovalReminder.Tables[0].Rows.Count > 0)
                                {
                                    tCorrespond Cor = new tCorrespond();
                                    Cor.OrderHeadId = paraReferenceID;
                                    Cor.MessageTitle = dsApprovalReminder.Tables[0].Rows[0]["MailSubject"].ToString();
                                    Cor.Message = dsApprovalReminder.Tables[0].Rows[0]["MailBody"].ToString();
                                    Cor.date = DateTime.Now;
                                    Cor.MessageSource = "Scheduler";
                                    Cor.MessageType = "Reminder";
                                    Cor.DepartmentID = DepartmentID;
                                    Cor.OrderDate = OrdrDate; //DateTime.Now;
                                    Cor.CurrentOrderStatus = StatusID;
                                    Cor.MailStatus = 0;
                                    // Cor.OrderCancelDays = OrderCancelDays;
                                    // Cor.AutoCancelReminderSchedule = AutoCancelReminderSchedule;
                                    Cor.ApprovalReminderSchedule = ApprovalReminderSchedule;
                                    Cor.NxtApprovalReminderDate = DateTime.Now.AddDays(ApprovalReminderSchedule);
                                    Cor.Archive = false;

                                    db.tCorresponds.AddObject(Cor);
                                    db.SaveChanges();
                                }


                                /* Update tProductstockDetails Reserve Quantity & Available Balance START>>>>>>>> */
                                UpdateTProductStockReserveQtyAvailBalance(paraReferenceID, conn);
                                /* <<<<<<<<Update tProductstockDetails Reserve Quantity & Available Balance END */
                                Suecc = 1;
                            }
                            else
                            {
                                long OrdrID = paraReferenceID;
                                RollBack(OrdrID, conn);
                                Suecc = 0;
                            }

                        }

                    }
                }

                catch (System.Exception ex)
                {
                    if (ApprovalDetail == 0)
                    {
                        long OrdrID = paraReferenceID;
                        RollBack(OrdrID, conn);
                    }
                }
                finally { }
                return Suecc;
            }
        }


        public void SaveCostcenterApprover(string paymethodvalue, long paymenyid, long oid, long storeid, long uid, string[] conn)
        {
            using (SqlCommand cmd1 = new SqlCommand())
            {
                cmd1.CommandType = CommandType.StoredProcedure;
                cmd1.CommandText = "SP_SaveCostcenterApprover";
                cmd1.Connection = svr.GetSqlConn(conn);
                cmd1.Parameters.Clear();
                cmd1.Parameters.AddWithValue("paymenyid", paymenyid);
                cmd1.Parameters.AddWithValue("oid", oid);
                cmd1.Parameters.AddWithValue("storeid", storeid);
                cmd1.Parameters.AddWithValue("uid", uid);
                cmd1.Parameters.AddWithValue("paymethodvalue", paymethodvalue);
                cmd1.ExecuteNonQuery();
            }
        }

        #endregion

        #region Emoney
        public DataSet getismoneydept(long orderID, string[] conn)
        {

            DataSet ds = new DataSet();
            string result = "NO";
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "getorderbyid";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@orderID", orderID);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;

            }

        }
        #endregion

        #region 2022 combine CR document pending Approval list
        public DataSet GetPendingApprovalList(long UserID, string[] conn, string Ordtype)
        {
            DataSet ds = new DataSet();

            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "POR_SP_GetPendingApprovalRequest";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@SiteIDs", "0");
                cmd.Parameters.AddWithValue("@UserID", UserID);
                cmd.Parameters.AddWithValue("@OrdType", Ordtype);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;

            }
        }

        #endregion

        #region Erp
        public string CheckStoreERPAutoApproved(long StoreID, string[] conn)
        {

            DataSet ds = new DataSet();
            string result = "";
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_CheckERPAutoApproval";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@StoreID", StoreID);
                da.SelectCommand = cmd;
                da.Fill(ds);
                da.SelectCommand = cmd;
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    result = ds.Tables[0].Rows[0]["AutoApproval"].ToString();
                }
                else
                {
                    result = "No";
                }
                return result;

            }
        }

        public DataSet CheckStoreERPAutoByReqID(long orderID, long userID, string[] conn)
        {

            DataSet ds = new DataSet();

            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_CheckERPApprovalbyID";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@OrderID", orderID);
                cmd.Parameters.AddWithValue("@UserID", userID);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;

            }

        }
        public DataSet getErpOrderhead(long orderID, string[] conn)
        {

            DataSet ds = new DataSet();

            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_getOrderhead";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@OrderID", orderID);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;

            }

        }

        public DataSet getErpOrderdetail(long orderID, string[] conn)
        {

            DataSet ds = new DataSet();

            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_getOrderdetail";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@OrderID", orderID);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;

            }

        }
        public void Savefailedaprorder(long OrderID, string servicedesc,string status, string[] conn)
        {
            using (SqlCommand cmd1 = new SqlCommand())
            {
                cmd1.CommandType = CommandType.StoredProcedure;
                cmd1.CommandText = "sp_InsertTrigger";
                cmd1.Connection = svr.GetSqlConn(conn);
                cmd1.Parameters.Clear();
                cmd1.Parameters.AddWithValue("@RequestID", OrderID);
                cmd1.Parameters.AddWithValue("@servicedesc", servicedesc);
                cmd1.Parameters.AddWithValue("@Status", status);
                cmd1.ExecuteNonQuery();
            }
        }

        public DataSet updateOrderaprStatus(long orderID, string[] conn)
        {

            DataSet ds = new DataSet();

            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_updateOrderstatus";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@OrderID", orderID);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;

            }
            

        }

        public DataSet chkisERPAutoApproval(string deptID, string[] conn)
        {

            DataSet ds = new DataSet();

            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_isERPAutoApproval";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@deptID", deptID);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;

            }

        }

        public void InsetERPOrderNotificationLog(long OrderID, long Orderby, long storeID, string[] conn)
        {
            using (SqlCommand cmd1 = new SqlCommand())
            {
                cmd1.CommandType = CommandType.StoredProcedure;
                cmd1.CommandText = "insetErpOrderNotification";
                cmd1.Connection = svr.GetSqlConn(conn);
                cmd1.Parameters.Clear();
                cmd1.Parameters.AddWithValue("@RequestID", OrderID);
                cmd1.Parameters.AddWithValue("@OrderBy", Orderby);
                cmd1.Parameters.AddWithValue("@StoreID", storeID);
                cmd1.ExecuteNonQuery();
            }
        }
        #endregion

        #region 2022 combine CR document project 3 return, cancel & expire Order


        public int checkstatusforRefundupdate(long SelectedOrder, string UserType, string[] conn)
        {
            //DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "proc_CheckstatusforRefundupdate";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@ID", SelectedOrder);
                cmd.Parameters.AddWithValue("@UserType", UserType);
                cmd.Connection = svr.GetSqlConn(conn);
                da.SelectCommand = cmd;
                //da.Fill(ds);
                // int cnt = int.Parse(ds.Tables[0].Rows[0]["Returnvalue"].ToString());
                int cnt = int.Parse(cmd.ExecuteScalar().ToString());
                //ds.Dispose();
                return cnt;
            }


        }

        public DataSet GetRefundstatus(string UserType, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "proc_getrefundstatus";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@UserType", UserType);
                cmd.Connection = svr.GetSqlConn(conn);
                da.SelectCommand = cmd;
                da.Fill(ds);
                cmd.Connection.Close();
                return ds;
            }

        }

        public string UpdatefinRefundstatus(long OrderID, long statusID, string status, long userid, string Codevalue, string[] conn)
        {
            string result = "";
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "Proc_UpdateRefundStatus";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@OrderID", OrderID);
                    cmd.Parameters.AddWithValue("@statusID", statusID);
                    cmd.Parameters.AddWithValue("@status", status);
                    cmd.Parameters.AddWithValue("@userid", userid);
                    cmd.Parameters.AddWithValue("@Codevalue", Codevalue);
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                    result = "True";
                }
            }
            catch (Exception ex)
            {
                result = "Fail";
            }
            return result;
        }

        public DataSet emailGetOrderHead(long OrderID, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "Proc_GetEmailOrderheadbyOrderID";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@OrderID", OrderID);
                cmd.Connection = svr.GetSqlConn(conn);
                da.SelectCommand = cmd;
                da.Fill(ds);
                cmd.Connection.Close();
                return ds;
            }
        }

        public DataSet emailGetOrderDetail(long OrderID, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "Proc_GetEmailOrderDetailbyID";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@OrderID", OrderID);
                cmd.Connection = svr.GetSqlConn(conn);
                da.SelectCommand = cmd;
                da.Fill(ds);
                cmd.Connection.Close();
                return ds;
            }
        }

        public string GetAccountEmail(long OrderID, string[] conn)
        {
            string AccEmailID = "";
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "Sp_GetUserId";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("Od_id", OrderID);
                cmd.Connection = svr.GetSqlConn(conn);
                AccEmailID = cmd.ExecuteScalar().ToString();
                cmd.Connection.Close();
            }
            return AccEmailID;
        }

        public string GetExpiredCancelEmail(string[] conn)
        {
            string AccEmailID = "";
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "Sp_ExpirydorderUser";
                cmd.Parameters.Clear();
                cmd.Connection = svr.GetSqlConn(conn);
                AccEmailID = cmd.ExecuteScalar().ToString();
                cmd.Connection.Close();
            }
            return AccEmailID;
        }

        public void InsertOrderStatuslog(long OrderID, long statusID, string status, long userid, string[] conn)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "Proc_insertstatuslog";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@OrderID", OrderID);
                cmd.Parameters.AddWithValue("@statusID", statusID);
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@userid", userid);
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }
        }

        public void MailCompose(string UserEmail, long RequestID, string[] conn)
        {

            try
            {
                // UserEmail = "suraj@brilliantinfosys.com";

                // string message = "We are not able to retrigger service automatically because service is down at a moment." + "<br/><br/>" + "Please retrigger it manually through provided interface.";
                string message = "Below order is Cancelled by Retail admin please take appropriate action.";
                //SmtpClient smtpClient = new SmtpClient("mail.brilliantinfosys.com", 587);

                //SmtpClient smtpClient = new SmtpClient("smtpout.asia.secureserver.net", 25);
                SmtpClient smtpClient = new SmtpClient("10.228.134.54", 25);
                MailMessage message1 = new MailMessage();

                // MailAddress fromAddress = new MailAddress("rahul@brilliantinfosys.com", "GWC");

                MailAddress fromAddress = new MailAddress("OMSTest@gulfwarehousing.com", "GWC");

                //MailAddress fromAddress = new MailAddress("OMSNotification@gulfwarehousing.com", "GWC");

                //From address will be given as a MailAddress Object
                message1.From = fromAddress;

                //To address collection of MailAddress
                message1.To.Add(UserEmail);
                message1.Subject = "Order Cancelled By retail admin.";

                //Body can be Html or text format
                //Specify true if it  is html message
                message1.IsBodyHtml = true;

                //Message body content

                message1.Body = "Hi All<br/><br/>";
                message1.Body = message1.Body + message;

                message1.Body = message1.Body + "<br/><br/>" + ExpireCancelOrderHead(RequestID, conn);
                message1.Body = message1.Body + "<br/><br/>" + ExpireCancelOrderDetail(RequestID, conn);
                message1.Body = message1.Body + MailGetFooter();

                smtpClient.EnableSsl = false;
                //Send SMTP mail
                smtpClient.UseDefaultCredentials = false;
                NetworkCredential basicCredential = new NetworkCredential("OMSTest@gulfwarehousing.com", "");

                //NetworkCredential basicCredential = new NetworkCredential("OMSNotification@gulfwarehousing.com", "");
                // NetworkCredential basicCredential = new NetworkCredential("rahul@brilliantinfosys.com", "q6FH9{qX72Cs");

                smtpClient.Credentials = basicCredential;
                smtpClient.Send(message1);
            }
            catch (Exception ex)
            { }
        }

        protected string ExpireCancelOrderHead(long RequestID, string[] conn)
        {
            string messageBody = "";
            DataSet ds = new DataSet();
            ds = emailGetOrderHead(RequestID, conn);
            messageBody = "<font><b>Order Summary :  </b> </font><br/><br/>";
            string htmlTableStart = "<table style=\"border-collapse:collapse; text-align:center;\" >";
            string htmlTableEnd = "</table>";
            string htmlHeaderRowStart = "<tr style =\"background-color:#6FA1D2; color:#ffffff;\">";
            string htmlHeaderRowEnd = "</tr>";
            string htmlTrStart = "<tr style =\"color:#555555; text-align: center;\">";
            string htmlTrEnd = "</tr>";
            string htmlTdStart = "<td style=\" border-color:#5c87b2; border-style:solid; border-width:thin; padding: 5px; text-align: center;\">";
            string htmlTdEnd = "</td>";
            messageBody += htmlTableStart;
            messageBody += htmlHeaderRowStart;
            messageBody += htmlTdStart + "Order Date" + htmlTdEnd;
            messageBody += htmlTdStart + "Order Id" + htmlTdEnd;
            messageBody += htmlTdStart + "Exp. Delivery Date" + htmlTdEnd;
            messageBody += htmlTdStart + "Status" + htmlTdEnd;
            messageBody += htmlTdStart + "Department" + htmlTdEnd;
            messageBody += htmlTdStart + "Location Name" + htmlTdEnd;
            messageBody += htmlHeaderRowEnd;

            DateTime date1 = new DateTime();
            date1 = Convert.ToDateTime(ds.Tables[0].Rows[0]["Orderdate"].ToString());
            string OrderDate = date1.ToString("dd/MM/yyyy");
            string orderno = ds.Tables[0].Rows[0]["OrderID"].ToString();
            DateTime date2 = new DateTime();
            date2 = Convert.ToDateTime(ds.Tables[0].Rows[0]["Deliverydate"].ToString());
            string Deliverydate = date2.ToString("dd/MM/yyyy");
            string Status = ds.Tables[0].Rows[0]["status"].ToString();
            string Department = ds.Tables[0].Rows[0]["Department"].ToString();
            string LocationName = ds.Tables[0].Rows[0]["LocationName"].ToString();
            // string RequestedBy = ds.Tables[0].Rows[0]["Requestedby"].ToString();

            messageBody += htmlTrStart;
            messageBody += htmlTdStart + " " + OrderDate + " " + htmlTdEnd;

            messageBody += htmlTdStart + " " + orderno + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Deliverydate + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Status + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + Department + " " + htmlTdEnd;
            messageBody += htmlTdStart + " " + LocationName + " " + htmlTdEnd;
            messageBody += htmlTrEnd;
            messageBody += htmlTableEnd;

            return messageBody;
        }
        // ERPProductDetail
        protected string ExpireCancelOrderDetail(long RequestID, string[] conn)
        {
            string messageBody = "";
            try
            {
                DataTable dtdtP = new DataTable();
                DataSet ds = new DataSet();
                ds = emailGetOrderDetail(RequestID, conn);
                // dtdtP = getOrderDetails(RequestID);
                messageBody = "<font><b>Order Details :  </b> </font><br/><br/>";
                string htmlTableStart = "<table style=\"border-collapse:collapse; text-align:center;\" >";
                string htmlTableEnd = "</table>";
                string htmlHeaderRowStart = "<tr style =\"background-color:#6FA1D2; color:#ffffff;\">";
                string htmlHeaderRowEnd = "</tr>";
                string htmlTrStart = "<tr style =\"color:#555555; text-align: center;\">";
                string htmlTrEnd = "</tr>";
                string htmlTdStart = "<td style=\" border-color:#5c87b2; border-style:solid; border-width:thin; padding: 5px; text-align: center;\">";
                string htmlTdEnd = "</td>";
                messageBody += htmlTableStart;
                messageBody += htmlHeaderRowStart;
                messageBody += htmlTdStart + "Sr. No." + htmlTdEnd;
                messageBody += htmlTdStart + "Item Code" + htmlTdEnd;
                messageBody += htmlTdStart + "Description" + htmlTdEnd;
                messageBody += htmlTdStart + "Qty" + htmlTdEnd;

                messageBody += htmlHeaderRowEnd;
                if (ds.Tables[0].Rows.Count > 0)
                {
                    for (int r = 0; r <= ds.Tables[0].Rows.Count - 1; r++)
                    {
                        messageBody += htmlTrStart;
                        messageBody += htmlTdStart + " " + ds.Tables[0].Rows[0]["RowNum"].ToString() + " " + htmlTdEnd;
                        messageBody += htmlTdStart + " " + ds.Tables[0].Rows[0]["Productcode"].ToString() + " " + htmlTdEnd;
                        messageBody += htmlTdStart + " " + ds.Tables[0].Rows[0]["ProdDescription"].ToString() + " " + htmlTdEnd;
                        messageBody += htmlTdStart + " " + ds.Tables[0].Rows[0]["OrderQty"].ToString() + " " + htmlTdEnd;
                        messageBody += htmlTrEnd;
                    }
                }
                messageBody += htmlTableEnd;
            }
            catch (Exception ex)
            {
            }
            return messageBody;
        }

        public DataSet GetAccountPendingfinActivity(long UserID, string[] conn, string Ordtype)
        {
            DataSet ds = new DataSet();

            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "POR_SP_GetPendingFinanceActivity";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@SiteIDs", "0");
                cmd.Parameters.AddWithValue("@UserID", UserID);
                cmd.Parameters.AddWithValue("@OrdType", Ordtype);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;

            }
        }

        public int CheckRetailCancelMailsend(long OrderID, string[] conn)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "Proc_CheckRetailCancelMailsend";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@OrderID", OrderID);
                cmd.Connection = svr.GetSqlConn(conn);
                da.SelectCommand = cmd;               
                int cnt = int.Parse(cmd.ExecuteScalar().ToString());
                return cnt;
            }
        }

        public string getrefunddate(long Orderid,string[] conn)
        {
            // DateTime refunddate = new DateTime();
            string refunddate = "";
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "Proc_GetRefunddate";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@Orderid", Orderid);
                cmd.Connection = svr.GetSqlConn(conn);
                da.SelectCommand = cmd;
                // refunddate = Convert.ToDateTime(cmd.ExecuteScalar().ToString());
                refunddate = cmd.ExecuteScalar().ToString();
                return refunddate;
            }
        }

        public long UpdateIseApprflag(long requestID,string[] conn)
        {
            long isupdate = 0;
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_updateisApprflag";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@requestID", requestID);
                cmd.Connection = svr.GetSqlConn(conn);
                //isupdate =Convert.ToInt64(cmd.ExecuteScalar().ToString());
                cmd.Connection.Close();
            }
            return isupdate;
        }
        #endregion




        public int FinalSaveRequestPartDetail_New(string paraSessionID, string paraCurrentObjectName, long paraReferenceID, string paraUserID, int StatusID, long DepartmentID, long PreviousStatusID, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                var torderdetails = 0;
                int Result = 0;
                try
                {
                    if (PreviousStatusID == 2)
                    {
                        //tOrderDetailHistory OD = new tOrderDetailHistory();
                        DataSet dsOldProducts = new DataSet();
                        dsOldProducts = fillds(" Select * from tOrderDetail where OrderHeadId=" + paraReferenceID + "", conn);
                        int CntPrds = dsOldProducts.Tables[0].Rows.Count;
                        if (CntPrds > 0)
                        {
                            for (int i = 0; i < CntPrds - 1; i++)
                            {
                                //OD.OrderHeadId = paraReferenceID;
                                //OD.SkuId = Convert.ToInt64(dsOldProducts.Tables[0].Rows[i]["SkuId"].ToString());
                                //OD.OrderQty = Convert.ToDecimal(dsOldProducts.Tables[0].Rows[i]["OrderQty"].ToString());
                                //OD.UOMID = Convert.ToInt64(dsOldProducts.Tables[0].Rows[i]["UOMID"].ToString());
                                //OD.Sequence = Convert.ToInt64(dsOldProducts.Tables[0].Rows[i]["Sequence"].ToString());
                                //OD.Prod_Name = dsOldProducts.Tables[0].Rows[i]["Prod_Name"].ToString();
                                //OD.Prod_Description = dsOldProducts.Tables[0].Rows[i]["Prod_Description"].ToString();
                                //OD.Prod_Code = dsOldProducts.Tables[0].Rows[i]["Prod_Code"].ToString();
                                //OD.RemaningQty = Convert.ToDecimal(dsOldProducts.Tables[0].Rows[i]["RemainingQty"].ToString());

                                //db.tOrderDetailHistories.Attach(OD);
                                //db.SaveChanges();

                                long OrderHeadId = paraReferenceID;
                                long SkuId = Convert.ToInt64(dsOldProducts.Tables[0].Rows[i]["SkuId"].ToString());
                                decimal OrderQty = Convert.ToDecimal(dsOldProducts.Tables[0].Rows[i]["OrderQty"].ToString());
                                long UOMID = Convert.ToInt64(dsOldProducts.Tables[0].Rows[i]["UOMID"].ToString());
                                long Sequence = Convert.ToInt64(dsOldProducts.Tables[0].Rows[i]["Sequence"].ToString());
                                string Prod_Name = dsOldProducts.Tables[0].Rows[i]["Prod_Name"].ToString();
                                string Prod_Description = dsOldProducts.Tables[0].Rows[i]["Prod_Description"].ToString();
                                string Prod_Code = dsOldProducts.Tables[0].Rows[i]["Prod_Code"].ToString();
                                decimal RemaningQty = Convert.ToDecimal(dsOldProducts.Tables[0].Rows[i]["RemainingQty"].ToString());

                                insertOrderDetailHistory(OrderHeadId, SkuId, OrderQty, UOMID, Sequence, Prod_Name, Prod_Description, Prod_Code, RemaningQty,conn);
                            }
                        }
                    }

                    List<POR_SP_GetPartDetail_ForRequest_Result> finalSaveLst = new List<POR_SP_GetPartDetail_ForRequest_Result>();
                    finalSaveLst = GetExistingTempDataBySessionIDObjectName(paraSessionID, paraUserID, paraCurrentObjectName, conn);

                    XElement xmlEle = new XElement("Request", from rec in finalSaveLst
                                                              select new XElement("PartList",
                                                              new XElement("PRH_ID", paraReferenceID),
                                                              new XElement("Prod_ID", Convert.ToInt64(rec.Prod_ID)),
                                                              new XElement("Prod_Name", rec.Prod_Name),
                                                              new XElement("Prod_Description", rec.Prod_Description),
                                                              new XElement("RequestQty", Convert.ToDecimal(rec.RequestQty)),
                                                              new XElement("RemaningQty", Convert.ToDecimal(rec.RequestQty)),
                                                              new XElement("Sequence", Convert.ToInt64(rec.Sequence)),
                                                              new XElement("UOMID", Convert.ToInt64(rec.UOMID)),
                                                              new XElement("Price", Convert.ToDecimal(rec.Price)),
                                                              new XElement("VATPercent", Convert.ToDecimal(rec.VATPercent)),
                                                              new XElement("VATAmount", Convert.ToDecimal(rec.VATAmount)),
                                                              new XElement("Total", Convert.ToDecimal(rec.Total)),
                                                              new XElement("IsPriceChange", Convert.ToInt16(rec.IsPriceChange)),
                                                              new XElement("Prod_Code", rec.Prod_Code),
                                                               new XElement("SkuTotalAmt", rec.SkuTotalAmt),
                                                                new XElement("SKUTotalAmtExclVAT", rec.SKUTotalAmtExclVAT)
                                                              ));

                    ObjectParameter _PRH_ID = new ObjectParameter("PRH_ID", typeof(long));
                    _PRH_ID.Value = paraReferenceID;

                    ObjectParameter _xmlData = new ObjectParameter("xmlData", typeof(string));
                    _xmlData.Value = xmlEle.ToString();


                    ObjectParameter[] obj = new ObjectParameter[] { _PRH_ID, _xmlData };
                    //db.ExecuteFunction("POR_SP_InsertIntoPORtPartRequestDetail", obj);
                    db.ExecuteFunction("POR_SP_InsertIntotOrderDetail", obj);

                    db.SaveChanges(); torderdetails = 1; Result = 1;


                    addUpdateGrandTotal(paraReferenceID, conn);

                    /*Add Record Of User into table tOrderWiseAccess*/
                    DataSet dsChkRecordOfUser = new DataSet();
                    dsChkRecordOfUser = fillds("select * from tOrderwiseAccess  where orderID=" + paraReferenceID + " and Usertype='User'", conn);
                    if (dsChkRecordOfUser.Tables[0].Rows.Count == 0)
                    {
                        int IsPriceChenged = 0;
                        DataSet dsIsPriceChange = new DataSet();
                        dsIsPriceChange = fillds("select * from torderdetail where orderheadid=" + paraReferenceID + " and IsPriceChange=1", conn);
                        int rowcount = dsIsPriceChange.Tables[0].Rows.Count;
                        if (rowcount > 0)
                        { IsPriceChenged = 1; }
                        else { IsPriceChenged = 0; }

                        long PriceEdit = 0;
                        long SkuQtyEdit = 0;

                        // tOrderWiseAccess ODA = new tOrderWiseAccess();
                        long UserApproverID = long.Parse(paraUserID);
                        int ApprovalLevel = 0;

                        if (StatusID == 1) { }
                        else
                        {
                            if (IsPriceChenged == 1)
                            {
                                //ODA.PriceEdit = false;
                                //ODA.SkuQtyEdit = false;

                                 PriceEdit = 0;
                                 SkuQtyEdit = 0;

                                /*Coading for New Status Pending For Financial Approver Start*/
                               /* tOrderHead OH = new tOrderHead();
                                OH = (from or in db.tOrderHeads
                                      where or.Id == paraReferenceID
                                      select or).FirstOrDefault();
                                OH.Status = 31; //Pending For Financial Approver
                                db.SaveChanges();*/

                                UpdateOrderHeadStatus(paraReferenceID, conn);

                                /*Coading for New Status Pending For Financial Approver End*/
                            }
                            else
                            {
                                //ODA.PriceEdit = false;
                                //ODA.SkuQtyEdit = true;
                                 PriceEdit = 0;
                                 SkuQtyEdit = 1;
                            }
                            string UserType = "User";
                            long OrderID = paraReferenceID;
                            DateTime Date = new DateTime();
                            Date = DateTime.Now;
                            //db.AddTotOrderWiseAccesses(ODA);
                            //db.SaveChanges();
                            insertOrderWiseAccess(UserApproverID, ApprovalLevel, PriceEdit, SkuQtyEdit, UserType, OrderID, Date, conn);
                            
                        }
                    }
                    /*Add Record Of User into table tOrderWiseAccess*/
                    if (StatusID == 1) { }
                    else
                    {
                        /* tOrderDetail ODT = new tOrderDetail();
                         ODT = db.tOrderDetails.Where(r => r.OrderHeadId == paraReferenceID).FirstOrDefault();
                         if (ODT != null)
                         {
                             Result = SetApproverDataafterSave(paraCurrentObjectName, paraReferenceID, paraUserID, StatusID, DepartmentID, PreviousStatusID, conn);
                         }
                         else
                         {
                             long OrderID = paraReferenceID;
                             // RollBack(OrderID, conn);
                             Result = 0;
                         }*/
                        DataSet dsh = new DataSet();
                        dsh = checktOrderDetailOrder(paraReferenceID, conn);
                        if (dsh.Tables[0].Rows.Count > 0)
                        {
                            Result = SetApproverDataafterSave_New(paraCurrentObjectName, paraReferenceID, paraUserID, StatusID, DepartmentID, PreviousStatusID, conn);
                        }
                        else
                        {
                            long OrderID = paraReferenceID;
                            // RollBack(OrderID, conn);
                            Result = 0;
                        }
                    }
                    ClearTempDataFromDB(paraSessionID, paraUserID, paraCurrentObjectName, conn);
                    /*if (result == finalSaveLst.Count)
                    {

                    }*/
                }
                catch (System.Exception ex)
                {
                    if (torderdetails == 0)
                    {
                        long OrderID = paraReferenceID;
                        RollBack(OrderID, conn);
                        Result = 0;
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "Sp_EnterErrorTracking";
                            cmd.Connection = svr.GetSqlConn(conn);
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("Data", ex.Data.ToString());
                            cmd.Parameters.AddWithValue("GetType", ex.Source.ToString());
                            cmd.Parameters.AddWithValue("InnerException", "Error");
                            cmd.Parameters.AddWithValue("Message", ex.Message.ToString());
                            cmd.Parameters.AddWithValue("Source", "FinalSaveRequestPartDetail");
                            cmd.Parameters.AddWithValue("DateTime", DateTime.Now);
                            cmd.Parameters.AddWithValue("UserID", paraUserID);
                            cmd.ExecuteNonQuery();
                            cmd.Connection.Close();
                        }
                    }
                }
                finally { }
                return Result;
            }
        }


        public void insertOrderDetailHistory(long OrderHeadId, long SkuId, decimal OrderQty, long UOMID, long Sequence, string ProdName, string ProdDescription, string ProdCode, decimal RemaningQty, string[] conn)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_insertOrderDetailHistory";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@OrderHeadId", OrderHeadId);
                    cmd.Parameters.AddWithValue("@SkuId", SkuId);
                    cmd.Parameters.AddWithValue("@OrderQty", OrderQty);
                    cmd.Parameters.AddWithValue("@UOMID", UOMID);
                    cmd.Parameters.AddWithValue("@Sequence", Sequence);
                    cmd.Parameters.AddWithValue("@ProdName", ProdName);
                    cmd.Parameters.AddWithValue("@ProdDescription", ProdDescription);
                    cmd.Parameters.AddWithValue("@ProdCode", ProdCode);
                    cmd.Parameters.AddWithValue("@RemaningQty", RemaningQty);
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
            }
            catch (System.Exception ex)
            {
                
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "Sp_EnterErrorTracking";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("Data", ex.Data.ToString());
                    cmd.Parameters.AddWithValue("GetType", ex.Source.ToString());
                    cmd.Parameters.AddWithValue("InnerException", "Error");
                    cmd.Parameters.AddWithValue("Message", ex.Message.ToString());
                    cmd.Parameters.AddWithValue("Source", OrderHeadId + "| insertOrderDetailHistory");
                    cmd.Parameters.AddWithValue("DateTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("UserID", "00");
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
            }
            
        }

        

        public void insertOrderWiseAccess(long UserApproverID, int ApprovalLevel, long PriceEdit, long SkuQtyEdit, string UserType, long OrderID, DateTime Date, string[] conn)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_insertOrderWiseAccess"; 
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@UserApproverID", UserApproverID);
                    cmd.Parameters.AddWithValue("@ApprovalLevel", ApprovalLevel);
                    cmd.Parameters.AddWithValue("@PriceEdit", PriceEdit);
                    cmd.Parameters.AddWithValue("@SkuQtyEdit", SkuQtyEdit);
                    cmd.Parameters.AddWithValue("@UserType", UserType);
                    cmd.Parameters.AddWithValue("@OrderID", OrderID);
                    cmd.Parameters.AddWithValue("@Date", Date);
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
            }
            catch (System.Exception ex)
            {

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "Sp_EnterErrorTracking";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("Data", ex.Data.ToString());
                    cmd.Parameters.AddWithValue("GetType", ex.Source.ToString());
                    cmd.Parameters.AddWithValue("InnerException", "Error");
                    cmd.Parameters.AddWithValue("Message", ex.Message.ToString());
                    cmd.Parameters.AddWithValue("Source", OrderID + "| insertOrderWiseAccess");
                    cmd.Parameters.AddWithValue("DateTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("UserID", Convert.ToString(UserApproverID));
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
            }

        }


        public void UpdateOrderHeadStatus(long OrderID,  string[] conn)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_UpdateOrderHeadStatus";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@OrderID", OrderID);
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
            }
            catch (System.Exception ex)
            {

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "Sp_EnterErrorTracking";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("Data", ex.Data.ToString());
                    cmd.Parameters.AddWithValue("GetType", ex.Source.ToString());
                    cmd.Parameters.AddWithValue("InnerException", "Error");
                    cmd.Parameters.AddWithValue("Message", ex.Message.ToString());
                    cmd.Parameters.AddWithValue("Source", OrderID + "| UpdateOrderHeadStatus");
                    cmd.Parameters.AddWithValue("DateTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("UserID", "00");
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
            }

        }


        public DataSet checktOrderDetailOrder(long orderID, string[] conn)
        {

            DataSet ds = new DataSet();

            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_checkOrderDetail";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@OrderID", orderID);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;

            }

        }

        public int SetApproverDataafterSave_New(string paraCurrentObjectName, long paraReferenceID, string paraUserID, int StatusID, long DepartmentID, long PreviousStatusID, string[] conn)
        {
            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                var ApprovalDetail = 0;
                string autoapproval = "False";
                int Rslt = 0, Suecc = 0;
                try
                {
                    if (PreviousStatusID == 2) { }
                    else
                    {
                        if (StatusID == 2)
                        {
                            /* Insert record of Approval Level 1 in tApprovalTrans Table===>>>> */

                            /* If Price is change of product then add Financial Approver at Approval Level 1  START*/
                            /*Check if Price is Changed or not*/
                            int IsPriceChenged = 0;
                            DataSet dsIsPriceChange = new DataSet();
                            dsIsPriceChange = fillds("select * from torderdetail where orderheadid=" + paraReferenceID + " and IsPriceChange=1", conn);
                            int rowcount = dsIsPriceChange.Tables[0].Rows.Count;
                            if (rowcount > 0)
                            {
                                IsPriceChenged = 1; long FinanAppId = 0;
                                DataSet dsFinanAppID = new DataSet();
                                dsFinanAppID = fillds("select FinApproverID from mterritory where id=" + DepartmentID + "", conn);
                                FinanAppId = Convert.ToInt64(dsFinanAppID.Tables[0].Rows[0]["FinApproverID"].ToString());

                                using (SqlCommand cmd = new SqlCommand())
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.CommandText = "SP_Insert_tapprovaltrans";
                                    cmd.Connection = svr.GetSqlConn(conn);
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("OrderId", paraReferenceID);
                                    cmd.Parameters.AddWithValue("StoreId", DepartmentID);
                                    cmd.Parameters.AddWithValue("UserId", Convert.ToInt64(paraUserID));
                                    cmd.Parameters.AddWithValue("ApprovalId", 1);
                                    cmd.Parameters.AddWithValue("ApproverID", FinanAppId);
                                    cmd.Parameters.AddWithValue("Status", StatusID);
                                    cmd.ExecuteNonQuery();
                                    cmd.Connection.Close();
                                }



                                /*Add Record Of User into table tOrderWiseAccess*/
                               /* tOrderWiseAccess ODA = new tOrderWiseAccess();
                                ODA.UserApproverID = FinanAppId;
                                ODA.ApprovalLevel = 1;
                                ODA.PriceEdit = true;
                                ODA.SkuQtyEdit = false;
                                ODA.UserType = "Financial Approver";
                                ODA.OrderID = paraReferenceID;
                                ODA.Date = DateTime.Now;
                                ODA.ApproverLogic = "AND";
                                db.AddTotOrderWiseAccesses(ODA);
                                db.SaveChanges();*/

                                long UserApproverID = FinanAppId;
                                int ApprovalLevel = 1;
                                long PriceEdit = 1;
                                long SkuQtyEdit = 0;
                                string UserType = "Financial Approver";
                                long OrderID = paraReferenceID;
                                DateTime Date = new DateTime();
                                Date = DateTime.Now;
                                string ApproverLogic = "AND";


                                insert_OrderWiseAccess(UserApproverID,  ApprovalLevel,  PriceEdit,  SkuQtyEdit,  UserType,  OrderID,  Date, ApproverLogic, paraUserID, conn);
                                // Check for ERP Auto Approval Department***

                                autoapproval = CheckStoreERPAutoApproved(DepartmentID, conn);
                                if (autoapproval != "True")
                                {
                                    /*Add Record Of User into table tOrderWiseAccess*/
                                    AddAllApprovalLevel(IsPriceChenged, paraReferenceID, DepartmentID, conn);
                                }


                                Rslt = EmailSendToApprover_New(FinanAppId, paraReferenceID, 0, conn);
                            }
                            else
                            {
                                // Check for ERP Auto Approval Department***

                                autoapproval = CheckStoreERPAutoApproved(DepartmentID, conn);
                                if (autoapproval != "True")
                                {

                                    IsPriceChenged = 0;
                                    /* If Price is change of product then add Financial Approver at Approval Level 1  END*/
                                    /*Add Record Of User into table tOrderWiseAccess*/
                                    AddAllApprovalLevel(IsPriceChenged, paraReferenceID, DepartmentID, conn);

                                    /*New Code After tOrderWiseAccess able added for order wise approval level start*/
                                    DataSet dsFirstApprover = new DataSet();
                                    dsFirstApprover = fillds("select OW.ID,OW.UserApproverID,OW.ApprovalLevel,OW.PriceEdit,OW.SkuQtyEdit,OW.UserType,OW.OrderID,OW.ApproverLogic ,Dl.DeligateTo from tOrderWiseAccess  OW left outer join tOrderHead OH on OW.OrderID=OH.ID left outer join mDeligate AS Dl ON OW.UserApproverID = Dl.DeligateFrom and CONVERT(VARCHAR(10), GETDATE(), 111) <=Convert(VARCHAR(10), Dl.ToDate,111) and CONVERT(VARCHAR(10), GETDATE(), 111) >=Convert(VARCHAR(10), Dl.FromDate,111)  and OH.StoreId=Dl.DeptID where OW.OrderID=" + paraReferenceID + " and OW.UserType != 'User' and OW.ApprovalLevel=1", conn);
                                    int CntFirstApprover = dsFirstApprover.Tables[0].Rows.Count;
                                    if (CntFirstApprover > 0)
                                    {
                                        for (int i = 0; i <= CntFirstApprover - 1; i++)
                                        {
                                            using (SqlCommand cmd1 = new SqlCommand())
                                            {
                                                cmd1.CommandType = CommandType.StoredProcedure;
                                                cmd1.CommandText = "SP_Insert_tapprovaltrans";
                                                cmd1.Connection = svr.GetSqlConn(conn);
                                                cmd1.Parameters.Clear();
                                                cmd1.Parameters.AddWithValue("OrderId", paraReferenceID);
                                                cmd1.Parameters.AddWithValue("StoreId", DepartmentID);
                                                cmd1.Parameters.AddWithValue("UserId", Convert.ToInt64(paraUserID));
                                                cmd1.Parameters.AddWithValue("ApprovalId", 1);
                                                cmd1.Parameters.AddWithValue("ApproverID", Convert.ToInt64(dsFirstApprover.Tables[0].Rows[i]["UserApproverID"].ToString()));
                                                cmd1.Parameters.AddWithValue("Status", StatusID);
                                                cmd1.ExecuteNonQuery();
                                            }

                                            ApprovalDetail = 1;

                                            /*Send Email to Approvers*/
                                            Rslt = EmailSendToApprover_New(Convert.ToInt64(dsFirstApprover.Tables[0].Rows[i]["UserApproverID"].ToString()), paraReferenceID, i, conn);
                                        }
                                    }
                                }
                                else
                                {
                                    // If auto approval Yes send to ERP API for Approval


                                    /* Update tProductstockDetails Reserve Quantity & Available Balance START>>>>>>>> */
                                    UpdateTProductStockReserveQtyAvailBalance_New(paraReferenceID, conn);
                                    /* <<<<<<<<Update tProductstockDetails Reserve Quantity & Available Balance END */
                                    /*Send Email to Approvers*/


                                }



                            }
                            /*New Code After tOrderWiseAccess able added for order wise approval level end*/

                           // tApprovalTran APRT = new tApprovalTran();
                           // APRT = db.tApprovalTrans.Where(rec => rec.OrderId == paraReferenceID).FirstOrDefault();
                            //if (APRT != null)

                            DataSet dsh = new DataSet();
                            dsh = ChecktApprovalTran(paraReferenceID, conn);

                            if (dsh.Tables[0].Rows.Count > 0)
                            {

                                Rslt = EmailSendWhenRequestSubmit_New(paraReferenceID, conn); //if Rslt =2 then mail sent to requestor else if Rslt=3 then mail not sent to requestoe
                                                                                              /*Insert record of Auto Cancellation Reminder & Approval reminder in tCorrespond table START*/

                                /*mTerritory Dept = new mTerritory();
                                Dept = db.mTerritories.Where(r => r.ID == DepartmentID).FirstOrDefault();
                                long OrderCancelDays = 0, ApprovalReminderSchedule = 0, AutoCancelReminderSchedule = 0;
                                if (Dept != null)
                                {
                                    OrderCancelDays = long.Parse(Dept.cancelDays.ToString());
                                    ApprovalReminderSchedule = long.Parse(Dept.ApproRemSchedul.ToString());
                                    AutoCancelReminderSchedule = long.Parse(Dept.AutoRemSchedule.ToString());
                                }*/

                                long OrderCancelDays = 0, ApprovalReminderSchedule = 0, AutoCancelReminderSchedule = 0;
                                DataSet dst = new DataSet();
                                dst = GetmTerritoriesData(DepartmentID, conn);

                                if (dst.Tables[0].Rows.Count > 0)
                                {
                                    OrderCancelDays = long.Parse(dst.Tables[0].Rows[0]["cancelDays"].ToString());
                                    ApprovalReminderSchedule = long.Parse(dst.Tables[0].Rows[0]["ApproRemSchedul"].ToString());
                                    AutoCancelReminderSchedule = long.Parse(dst.Tables[0].Rows[0]["AutoRemSchedule"].ToString());
                                }

                                DataSet dsGetOrderDate = new DataSet();
                                dsGetOrderDate = fillds("select OrderDate from torderhead where id=" + paraReferenceID + "", conn);
                                DateTime OrdrDate = Convert.ToDateTime(dsGetOrderDate.Tables[0].Rows[0]["OrderDate"].ToString());

                                DataSet dsAutocancel = new DataSet();
                                dsAutocancel = fillds("select * from mMessageEMailTemplates where ActivityID=(select Id from mDropdownValues where StatusID=10) and MessageID=(select Id from mDropdownValues where Value='Reminder') and DepartmentID=" + DepartmentID + " ", conn);
                                if (dsAutocancel.Tables[0].Rows.Count > 0)
                                {
                                    //tCorrespond Cor = new tCorrespond();
                                    //Cor.OrderHeadId = paraReferenceID;
                                    //Cor.MessageTitle = dsAutocancel.Tables[0].Rows[0]["MailSubject"].ToString();
                                    //Cor.Message = dsAutocancel.Tables[0].Rows[0]["MailBody"].ToString();
                                    //Cor.date = DateTime.Now;
                                    //Cor.MessageSource = "Scheduler";
                                    //Cor.MessageType = "Reminder";
                                    //Cor.DepartmentID = DepartmentID;
                                    //// Cor.OrderDate = DateTime.Now;
                                    //Cor.OrderDate = OrdrDate;
                                    //Cor.CurrentOrderStatus = StatusID;
                                    //Cor.MailStatus = 0;
                                    //Cor.OrderCancelDays = OrderCancelDays;
                                    //Cor.AutoCancelReminderSchedule = AutoCancelReminderSchedule;
                                    ////Cor.ApprovalReminderSchedule = ApprovalReminderSchedule;
                                    //Cor.NxtAutoCancelReminderDate = DateTime.Now.AddDays(AutoCancelReminderSchedule);
                                    //Cor.Archive = false;

                                    //db.tCorresponds.AddObject(Cor);
                                    //db.SaveChanges();
                                   //-----------------------------------------------------------------------------------
                                    long OrderHeadId = paraReferenceID;
                                    string MessageTitle = dsAutocancel.Tables[0].Rows[0]["MailSubject"].ToString();
                                    string Message = dsAutocancel.Tables[0].Rows[0]["MailBody"].ToString();
                                    DateTime date = new DateTime();
                                    date = DateTime.Now;
                                    string MessageSource = "Scheduler";
                                    string MessageType = "Reminder";
                                    long DeptID = DepartmentID;
                                    DateTime OrderDate = new DateTime();
                                    OrderDate = OrdrDate;
                                    long CurrentOrderStatus = StatusID;
                                    int MailStatus = 0;
                                    long OrderCancelDay = OrderCancelDays;
                                    long AutoCancelReminderSchedules = AutoCancelReminderSchedule;
                                    DateTime NxtAutoCancelReminderDate = new DateTime();
                                    NxtAutoCancelReminderDate = DateTime.Now.AddDays(AutoCancelReminderSchedule);
                                    // Cor.Archive = false;

                                    

                                    InsertintCorrespond(OrderHeadId, MessageTitle, Message, date, MessageSource, MessageType, DeptID, OrderDate, CurrentOrderStatus, MailStatus, OrderCancelDay, AutoCancelReminderSchedules
                                        , NxtAutoCancelReminderDate,conn);
                                }

                                DataSet dsApprovalReminder = new DataSet();
                                dsApprovalReminder = fillds("select * from mMessageEMailTemplates where ActivityID=(select Id from mDropdownValues where StatusID=2 and value='Order Submit') and MessageID=(select Id from mDropdownValues where Value='Reminder') and DepartmentID=" + DepartmentID + " ", conn);
                                if (dsApprovalReminder.Tables[0].Rows.Count > 0)
                                {
                                    /* tCorrespond Cor = new tCorrespond();
                                     Cor.OrderHeadId = paraReferenceID;
                                     Cor.MessageTitle = dsApprovalReminder.Tables[0].Rows[0]["MailSubject"].ToString();
                                     Cor.Message = dsApprovalReminder.Tables[0].Rows[0]["MailBody"].ToString();
                                     Cor.date = DateTime.Now;
                                     Cor.MessageSource = "Scheduler";
                                     Cor.MessageType = "Reminder";
                                     Cor.DepartmentID = DepartmentID;
                                     Cor.OrderDate = OrdrDate; //DateTime.Now;
                                     Cor.CurrentOrderStatus = StatusID;
                                     Cor.MailStatus = 0;
                                     // Cor.OrderCancelDays = OrderCancelDays;
                                     // Cor.AutoCancelReminderSchedule = AutoCancelReminderSchedule;
                                     Cor.ApprovalReminderSchedule = ApprovalReminderSchedule;
                                     Cor.NxtApprovalReminderDate = DateTime.Now.AddDays(ApprovalReminderSchedule);
                                     Cor.Archive = false;

                                     db.tCorresponds.AddObject(Cor);
                                     db.SaveChanges();*/
                                    tCorrespond Cor = new tCorrespond();
                                    long OrderHeadId = paraReferenceID;
                                    string MessageTitle = dsApprovalReminder.Tables[0].Rows[0]["MailSubject"].ToString();
                                    string Message = dsApprovalReminder.Tables[0].Rows[0]["MailBody"].ToString();
                                    DateTime date = new DateTime();
                                    date = DateTime.Now;
                                    string MessageSource = "Scheduler";
                                    string MessageType = "Reminder";
                                    long DeptID = DepartmentID;
                                    DateTime OrderDate = new DateTime();
                                    OrderDate = OrdrDate; //DateTime.Now;
                                    long CurrentOrderStatus = StatusID;
                                    int MailStatus = 0;
                                    // Cor.OrderCancelDays = OrderCancelDays;
                                    // Cor.AutoCancelReminderSchedule = AutoCancelReminderSchedule;
                                    long ApprovalReminderSchedules = ApprovalReminderSchedule;
                                    DateTime NxtApprovalReminderDate = new DateTime();
                                    NxtApprovalReminderDate = DateTime.Now.AddDays(ApprovalReminderSchedule);
                                    //Cor.Archive = false;


                                    InsertintCorrespondApprovalReminder(OrderHeadId, MessageTitle, Message, date, MessageSource, MessageType, DeptID, OrderDate, CurrentOrderStatus, MailStatus, ApprovalReminderSchedules, NxtApprovalReminderDate, conn);

                                }


                                /* Update tProductstockDetails Reserve Quantity & Available Balance START>>>>>>>> */
                                //  UpdateTProductStockReserveQtyAvailBalance(paraReferenceID, conn);
                                UpdateTProductStockReserveQtyAvailBalance_New(paraReferenceID, conn);
                                /* <<<<<<<<Update tProductstockDetails Reserve Quantity & Available Balance END */
                                Suecc = 1;
                            }
                            else
                            {
                                long OrdrID = paraReferenceID;

                                if (autoapproval == "True")
                                {
                                    Suecc = 1;
                                }
                                else
                                {
                                    Suecc = 0;
                                    RollBack(OrdrID, conn);
                                }

                            }

                        }
                        else if (StatusID == 3)
                        {
                            //Add Message into mMessageTrans Table After Approve Order

                            DataSet dsChkOredrIsEcomOrNot = new DataSet();
                            dsChkOredrIsEcomOrNot = fillds("select * from tOrderHead where orderType in('PreOrder','NormalOrder') and id=" + paraReferenceID + " ", conn);
                            if (dsChkOredrIsEcomOrNot.Tables[0].Rows.Count > 0)
                            {
                                // AddIntomMessageTransForEcommerce(paraReferenceID, conn);
                                UpdateIseApprflag(paraReferenceID, conn);
                            }
                            else
                            {

                                AddIntomMessageTrans(paraReferenceID, conn);
                                //UpdateIseApprflag(paraReferenceID, conn);
                            }
                            AddIntomMessageTrans(paraReferenceID, conn);
                            //UpdateIseApprflag(paraReferenceID, conn);
                        }
                        else
                        {
                        }
                    }
                }

                catch (System.Exception ex)
                {
                    if (ApprovalDetail == 0)
                    {
                        long OrdrID = paraReferenceID;
                        RollBack(OrdrID, conn);
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "Sp_EnterErrorTracking";
                            cmd.Connection = svr.GetSqlConn(conn);
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("Data", ex.Data.ToString());
                            cmd.Parameters.AddWithValue("GetType", ex.Source.ToString());
                            cmd.Parameters.AddWithValue("InnerException", "Error");
                            cmd.Parameters.AddWithValue("Message", ex.Message.ToString());
                            cmd.Parameters.AddWithValue("Source", "SetApproverDataafterSave");
                            cmd.Parameters.AddWithValue("DateTime", DateTime.Now);
                            cmd.Parameters.AddWithValue("UserID", paraUserID);
                            cmd.ExecuteNonQuery();
                            cmd.Connection.Close();
                        }
                    }
                }
                finally { }

                //ClearTempDataFromDB(paraSessionID, paraUserID, paraCurrentObjectName, conn);
                return Suecc;
            }
        }


        protected int EmailSendToApprover_New(long ApproverID, long RequestID, int id, string[] conn)
        {
            int Result = 0;
            try
            {
                string MailSubject;
                string MailBody;
                // string partdetail = EMailGetRequestPratDetail(0, 0, conn, ReqPartDetils);
                using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
                {
                    GWC_SP_GetRequestHeadByRequestIDs_Result Request = new GWC_SP_GetRequestHeadByRequestIDs_Result();
                    Request = db.GWC_SP_GetRequestHeadByRequestIDs(RequestID.ToString()).FirstOrDefault();
                    string ISProjectSite = "";
                    ISProjectSite = ISProjectSiteDetailsNew(Convert.ToString(Request.parentid), conn);
                    long Orderstatus = long.Parse(Request.Status.ToString());
                    long DepartmentID = long.Parse(Request.SiteID.ToString());
                    long Status = long.Parse(Request.Status.ToString());
                    DataSet dsMailSubBody = new DataSet();
                    dsMailSubBody = fillds("select * from mMessageEMailTemplates where ActivityID=(select Id from mDropdownValues where StatusID=2 and value='Order Submit') and MessageID=(select Id from mDropdownValues where Value='Action') and DepartmentID=" + DepartmentID + " and Active='Yes'", conn);
                    if (dsMailSubBody.Tables[0].Rows.Count > 0)
                    {
                        MailSubject = dsMailSubBody.Tables[0].Rows[0]["MailSubject"].ToString() + ", Order No # " + Request.OrderNo + "";

                        string UserName = "";
                        UserName = GetUserNameByUserID_New(ApproverID, conn);
                        //MailBody = "Dear " + GetUserNameByUserID_New(ApproverID, conn) + ", <br/>";
                        MailBody = "Dear " + UserName + ", <br/>";

                        DataSet dsFA = new DataSet();
                        dsFA = fillds("select UserType from tOrderWiseAccess  where OrderID=" + RequestID + " and UserApproverID=" + ApproverID + "", conn);
                        string UsrType = dsFA.Tables[0].Rows[0]["UserType"].ToString();
                        if (UsrType == "General Approver")
                        {
                            MailBody = MailBody + dsMailSubBody.Tables[0].Rows[0]["MailBody"].ToString();
                        }
                        else
                        {
                            if (UsrType == "Financial Approver")
                            {
                                DataSet dsFAMailBody = new DataSet();
                                dsFAMailBody = fillds("select * from mMessageEMailTemplates where ActivityID=(select Id from mDropdownValues where StatusID=2 and value='Order Submit Price Change') and MessageID=(select Id from mDropdownValues where Value='Action') and DepartmentID=" + DepartmentID + "", conn);
                                MailBody = MailBody + dsFAMailBody.Tables[0].Rows[0]["MailBody"].ToString();
                            }
                            else if (UsrType == "CostCenter Approver")
                            {
                                DataSet dsFAMailBody = new DataSet();
                                dsFAMailBody = fillds("select * from mMessageEMailTemplates where ActivityID=(select Id from mDropdownValues where StatusID=32 ) and MessageID=(select Id from mDropdownValues where Value='Action') and DepartmentID=" + DepartmentID + "", conn);
                                MailBody = MailBody + dsFAMailBody.Tables[0].Rows[0]["MailBody"].ToString();
                            }
                        }
                        DataSet dsChkOredrIsEcomOrNot = new DataSet();

                        dsChkOredrIsEcomOrNot = fillds("select * from tOrderHead where orderType in('PreOrder','NormalOrder') and id=" + RequestID + " ", conn);
                        if (dsChkOredrIsEcomOrNot.Tables[0].Rows.Count > 0)
                        {
                            MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetailForEcomm(Request);//for ecommerce
                            MailBody = MailBody + "<br/><br/>" + EmailGetAddressDetailsEcom(Request, conn);//for ecommerce
                            MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetailEcommerce(RequestID, conn);//for ecommerce
                        }
                        else
                        {
                            if (ISProjectSite == "Yes")
                            {
                                MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetailVodafoneTechnical(Request, conn);
                            }
                            else
                            {
                                MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                            }
                            // MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                            MailBody = MailBody + "<br/><br/>" + EmailGetAddressDetails(Request, conn);
                            MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetail(RequestID, conn);
                        }


                        //SendMail(MailBody + MailGetFooter(), MailSubject, EmailGetEmailIDsByUserID(ApproverID, conn));
                        Result = 2;//Mail Send Successfully to approver
                        long TemplateID = long.Parse(dsMailSubBody.Tables[0].Rows[0]["ID"].ToString());
                        SaveCorrespondsDataNew(RequestID, MailSubject, MailBody, DepartmentID, Status, EmailGetEmailIDsByUserID(ApproverID, conn), conn);
                        if (id == 0)
                        {
                            AdditionalDistribution_New(RequestID, TemplateID, conn);
                        }

                    }


                }
            }
            catch (System.Exception ex)
            {
                Result = 3;//Mail Not Send Successfully to approver
            }
            finally { }
            return Result;

        }


        public string GetUserNameByUserID_New(long ApproverID, string[] conn)
        {
            string result = "";
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    SqlDataAdapter da = new SqlDataAdapter();
                    DataSet ds = new DataSet();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_GetUserNameByUserID";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@ApproverID", ApproverID);
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        result = Convert.ToString(ds.Tables[0].Rows[0]["UserName"].ToString());
                    }
                    cmd.Connection.Close();
                }
            }
            catch (System.Exception ex)
            {

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "Sp_EnterErrorTracking";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("Data", ex.Data.ToString());
                    cmd.Parameters.AddWithValue("GetType", ex.Source.ToString());
                    cmd.Parameters.AddWithValue("InnerException", "Error");
                    cmd.Parameters.AddWithValue("Message", ex.Message.ToString());
                    cmd.Parameters.AddWithValue("Source", ApproverID + "| insert_OrderWiseAccess");
                    cmd.Parameters.AddWithValue("DateTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("UserID", Convert.ToString(ApproverID));
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
            }
           
            return result;
        }

        public void insert_OrderWiseAccess(long UserApproverID, int ApprovalLevel, long PriceEdit, long SkuQtyEdit, string UserType, long OrderID, DateTime Date, string ApproverLogic, string UserID, string[] conn)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_insert_OrderWiseAccess";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@UserApproverID", UserApproverID);
                    cmd.Parameters.AddWithValue("@ApprovalLevel", ApprovalLevel);
                    cmd.Parameters.AddWithValue("@PriceEdit", PriceEdit);
                    cmd.Parameters.AddWithValue("@SkuQtyEdit", SkuQtyEdit);
                    cmd.Parameters.AddWithValue("@UserType", UserType);
                    cmd.Parameters.AddWithValue("@OrderID", OrderID);
                    cmd.Parameters.AddWithValue("@Date", Date);
                    cmd.Parameters.AddWithValue("@ApproverLogic", ApproverLogic);
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
            }
            catch (System.Exception ex)
            {

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "Sp_EnterErrorTracking";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("Data", ex.Data.ToString());
                    cmd.Parameters.AddWithValue("GetType", ex.Source.ToString());
                    cmd.Parameters.AddWithValue("InnerException", "Error");
                    cmd.Parameters.AddWithValue("Message", ex.Message.ToString());
                    cmd.Parameters.AddWithValue("Source", OrderID + "| insert_OrderWiseAccess");
                    cmd.Parameters.AddWithValue("DateTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("UserID", UserID);
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
            }

        }



        protected void UpdateTProductStockReserveQtyAvailBalance_New(long RequestID, string[] conn)
        {
            //BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            DataSet dsHstry = new DataSet();
            dsHstry = fillds(" select * from tOrderDetailHistory where OrderHeadId=" + RequestID + "", conn);
            int CntHstry = dsHstry.Tables[0].Rows.Count;
            if (CntHstry > 0)
            {
                for (int j = 0; j < CntHstry - 1; j++)
                {
                    long HstrySkuID = long.Parse(dsHstry.Tables[0].Rows[j]["SkuId"].ToString());
                    decimal HstryQty = decimal.Parse(dsHstry.Tables[0].Rows[j]["OrderQty"].ToString());

                    decimal FnlQty = 0;
                    DataSet dsNewPrdLst = new DataSet();
                    dsNewPrdLst = fillds("Select * from tOrderDetail where OrderHeadId=" + RequestID + " and SkuId=" + HstrySkuID + "", conn);
                    if (dsNewPrdLst.Tables[0].Rows.Count > 0)
                    {
                        decimal NewAdSkuQty = decimal.Parse(dsNewPrdLst.Tables[0].Rows[0]["OrderQty"].ToString());

                        FnlQty = HstryQty - NewAdSkuQty;
                    }

                   /* tProductStockDetail psd1 = new tProductStockDetail();
                    psd1 = db.tProductStockDetails.Where(a => a.ProdID == HstrySkuID).FirstOrDefault();
                    if (psd1 != null)
                    {
                        db.tProductStockDetails.Detach(psd1);
                        psd1.ResurveQty = psd1.ResurveQty + FnlQty;
                        psd1.AvailableBalance = psd1.AvailableBalance - FnlQty;
                        db.tProductStockDetails.Attach(psd1);
                        db.ObjectStateManager.ChangeObjectState(psd1, EntityState.Modified);
                        db.SaveChanges();
                    }*/
                    updateProductStockDetailQTY(HstrySkuID, FnlQty, conn);

                }
            }
            else
            {
                DataSet dsPrdDetails = new DataSet();
                dsPrdDetails = fillds("Select * from tOrderDetail where OrderHeadId=" + RequestID + "", conn);
                int PrdCnt = dsPrdDetails.Tables[0].Rows.Count;
                if (PrdCnt > 0)
                {
                    for (int i = 0; i <= PrdCnt - 1; i++)
                    {
                        //update tProductstockDetails set ResurveQty=ResurveQty+@Qty,AvailableBalance=AvailableBalance-@Qty where ProdID=@Prd ;

                        //tProductStockDetail psd = new tProductStockDetail();
                        long SkuID = long.Parse(dsPrdDetails.Tables[0].Rows[i]["SkuId"].ToString());
                        decimal Qty = decimal.Parse(dsPrdDetails.Tables[0].Rows[i]["OrderQty"].ToString());
                        //psd = db.tProductStockDetails.Where(a => a.ProdID == SkuID).FirstOrDefault();
                        //if (psd != null)
                        //{
                        //    db.tProductStockDetails.Detach(psd);
                        //    psd.ResurveQty = psd.ResurveQty + Qty;
                        //    psd.AvailableBalance = psd.AvailableBalance - Qty;
                        //    db.tProductStockDetails.Attach(psd);
                        //    db.ObjectStateManager.ChangeObjectState(psd, EntityState.Modified);
                        //    db.SaveChanges();
                        //}

                        //SqlCommand cmd = new SqlCommand();
                        //cmd.CommandType = CommandType.StoredProcedure;
                        //cmd.CommandText = "GWC_SP_UpdatetProductstockDetailsAvailableResurveQty";
                        //cmd.Connection = svr.GetSqlConn(conn);
                        //cmd.Parameters.Clear();
                        //cmd.Parameters.AddWithValue("SkuID", SkuID);
                        //cmd.Parameters.AddWithValue("Qty", Qty);                        
                        //cmd.ExecuteNonQuery();


                        //mProduct prd = new mProduct();
                       // prd = db.mProducts.Where(p => p.ID == SkuID).FirstOrDefault();

                        string GroupSet = "";
                        GroupSet = CheckProductGroupSet(SkuID, conn);
                        //if (prd.GroupSet == "Yes")
                        if (GroupSet == "Yes")
                        {
                            DataSet dsBomProds = new DataSet();
                            dsBomProds = fillds("select SKUId,Quantity from mBOMDetail BD where BD.BOMheaderId=" + SkuID + "", conn);
                            if (dsBomProds.Tables[0].Rows.Count > 0)
                            {
                                for (int b = 0; b <= dsBomProds.Tables[0].Rows.Count - 1; b++)
                                {
                                    long bomPrd = long.Parse(dsBomProds.Tables[0].Rows[b]["SKUId"].ToString());
                                    decimal bomQty = decimal.Parse(dsBomProds.Tables[0].Rows[b]["Quantity"].ToString());

                                    decimal FinalQty = Qty * bomQty;

                                    SqlCommand cmd = new SqlCommand();
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.CommandText = "GWC_SP_UpdatetProductstockDetailsAvailableResurveQty";
                                    cmd.Connection = svr.GetSqlConn(conn);
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("SkuID", bomPrd);
                                    cmd.Parameters.AddWithValue("Qty", FinalQty);
                                    cmd.ExecuteNonQuery();

                                    // InsertIntotInventory(bomPrd, RequestID, FinalQty, "Dispatch", conn);
                                }
                            }
                        }
                        // else if (prd.GroupSet == "No")
                        else if (GroupSet == "No")
                        {
                            SqlCommand cmd = new SqlCommand();
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "GWC_SP_UpdatetProductstockDetailsAvailableResurveQty";
                            cmd.Connection = svr.GetSqlConn(conn);
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("SkuID", SkuID);
                            cmd.Parameters.AddWithValue("Qty", Qty);
                            cmd.ExecuteNonQuery();

                            //InsertIntotInventory(SkuID, RequestID, Qty, "Dispatch", conn);
                        }
                    }
                }
            }
        }


        public string CheckProductGroupSet(long ProdID, string[] conn)
        {
            string result = "";
            using (SqlCommand cmd = new SqlCommand())
            {

                SqlDataAdapter da = new SqlDataAdapter();
                DataSet ds = new DataSet();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_CheckProductGroupSet";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@ProdID", ProdID);
                da.SelectCommand = cmd;
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    result = Convert.ToString(ds.Tables[0].Rows[0]["GroupSet"].ToString());
                }
                cmd.Connection.Close();
            }
            return result;
        }


        protected void updateProductStockDetailQTY(long SKUID, decimal QTY, string[] conn)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                DataSet ds = new DataSet();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_updateProductStockDetailQTY";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@SKUID", SKUID);
                cmd.Parameters.AddWithValue("@FnlQty", QTY);
                cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }

        }

        public DataSet ChecktApprovalTran(long orderID, string[] conn)
        {

            DataSet ds = new DataSet();

            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_ChecktApprovalTran";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@OrderID", orderID);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;

            }

        }


        public DataSet GetmTerritoriesData(long DepartmentID, string[] conn)
        {

            DataSet ds = new DataSet();

            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetmTerritoriesData";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@DepartmentID", DepartmentID);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;

            }

        }


        public int EmailSendWhenRequestSubmit_New(long RequestID, string[] conn)
        {
            int Result = 0;
            try
            {
                string MailSubject;
                string MailBody;

                using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
                {

                    GWC_SP_GetRequestHeadByRequestIDs_Result Request = new GWC_SP_GetRequestHeadByRequestIDs_Result();
                    Request = db.GWC_SP_GetRequestHeadByRequestIDs(RequestID.ToString()).FirstOrDefault();

                    long Orderstatus = long.Parse(Request.Status.ToString());
                    long DepartmentID = long.Parse(Request.SiteID.ToString());
                    long Status = long.Parse(Request.Status.ToString());
                    string ISProjectSite = "";
                    ISProjectSite = ISProjectSiteDetailsNew(Convert.ToString(Request.parentid), conn);
                    DataSet dsMailSubBody = new DataSet();
                    dsMailSubBody = fillds("select * from mMessageEMailTemplates where ActivityID=(select Id from mDropdownValues where StatusID=2 and value='Order Submit') and MessageID=(select Id from mDropdownValues where Value='Information') and DepartmentID=" + DepartmentID + "", conn);
                    if (dsMailSubBody.Tables[0].Rows.Count > 0)
                    {
                        DataSet ds = new DataSet();
                        ds = fillds("select orderType from tOrderHead where Id=" + RequestID + "", conn);
                        string ordertype = ds.Tables[0].Rows[0]["Ordertype"].ToString();
                        if (Status == 2)
                        {
                            MailSubject = dsMailSubBody.Tables[0].Rows[0]["MailSubject"].ToString() + ", Order No # " + Request.OrderNo + "";
                        }
                        else if (ordertype == "Direct Import" && Status == 3)
                        {
                            MailSubject = "Order has been submitted sucessfully with Direct Order Import , Order No # " + Request.OrderNo + "";
                        }
                        else
                        {
                            string CrntStatus = Request.RequestStatus.ToString();
                            MailSubject = "Order " + CrntStatus + ", Order No # " + Request.OrderNo + "";
                        }
                        string UserName = "";
                        UserName = GetUserNameByUserID_New(Convert.ToInt64(Request.RequestBy), conn);
                        MailBody = "Dear " + UserName + ", <br/>";
                        //MailBody = "Dear " + GetUserNameByUserID(Convert.ToInt64(Request.RequestBy), conn) + ", <br/>";
                        MailBody = MailBody + "This is an automatically generated message in reference to a order request. An approval action is required before OMS can proceed. <br/> Thank you for your request. Before we can proceed, we need approver's formal approval to proceed. <br/>";
                        if (ordertype == "Direct Import" && Status == 3)
                        {
                            MailBody = MailBody + "<br/><br/>" + "Order has been submitted sucessfully with Direct Order Import";
                        }
                        MailBody = MailBody + dsMailSubBody.Tables[0].Rows[0]["MailBody"].ToString();

                        if (ISProjectSite == "Yes")/// if (Request.parentid == 10266)
                        {
                            MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetailVodafoneTechnical(Request, conn);
                        }
                        else
                        {
                            MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                        }
                        //MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                        MailBody = MailBody + "<br/><br/>" + EmailGetAddressDetails(Request, conn);
                        MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetail(RequestID, conn);

                        if (ordertype == "Direct Import" && Status == 3)
                        {
                            MailBody = MailBody + "<br/><br/>" + "Note : The order processes through direct import doesn't required any approver. These Order send to WMS for further processing ";
                        }

                        //   SendMail(MailBody + MailGetFooter(), MailSubject, EmailGetEmailIDsByUserID(Convert.ToInt64(Request.RequestBy), conn));
                        SendMail(MailBody + MailGetFooter(), MailSubject, EmailGetEmailIDsByUserID_New(Convert.ToInt64(Request.RequestBy), conn));
                        Result = 2;// Mail sent to requestor
                        long TemplateID = long.Parse(dsMailSubBody.Tables[0].Rows[0]["ID"].ToString());
                        if (Status == 2)
                        {
                            // AdditionalDistribution(RequestID, TemplateID, conn);
                            AdditionalDistribution_New(RequestID, TemplateID, conn);
                        }
                        // SaveCorrespondsData(RequestID, MailSubject, MailBody, DepartmentID, Status, conn);
                        SaveCorrespondsData_New(RequestID, MailSubject, MailBody, DepartmentID, Status, conn);
                        if (Request.ContactId1 > 0)
                        {
                            string Con1Name = Request.Contact1Name; string Con1Email = Request.Con1Email;
                            MailBody = "";
                            MailBody = "Dear " + Con1Name + ", <br/>";
                            MailBody = MailBody + "This is an automatically generated message in reference to a order request. An approval action is required before OMS can proceed. <br/> Thank you for your request. Before we can proceed, we need approver's formal approval to proceed. <br/>";
                            if (ordertype == "Direct Import" && Status == 3)
                            {
                                MailBody = MailBody + "<br/><br/>" + "Order has been submitted sucessfully with Direct Order Import";
                            }

                            MailBody = MailBody + dsMailSubBody.Tables[0].Rows[0]["MailBody"].ToString();

                            if (ISProjectSite == "Yes")
                            {
                                MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetailVodafoneTechnical(Request, conn);
                            }
                            else
                            {
                                MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                            }
                            // MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                            MailBody = MailBody + "<br/><br/>" + EmailGetAddressDetails(Request, conn);
                            MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetail(RequestID, conn);
                            if (ordertype == "Direct Import" && Status == 3)
                            {
                                MailBody = MailBody + "<br/><br/>" + "Note : The order processes through direct import doesn't required any approver. These Order send to WMS for further processing ";
                            }
                            SendMail(MailBody + MailGetFooter(), MailSubject, Con1Email);

                            string Contact2 = Request.Con2.ToString();
                            //long Con2ID = long.Parse(Request.ContactId2.ToString());
                            //if (Con2ID > 0)
                            if (Contact2 != "0")
                            {
                                string Contact2Emails = GetContact2Email(Contact2, conn);
                                // string Con2Name = Request.Contact2Name; string Con2Email = Request.Con2Email;
                                MailBody = "";
                                MailBody = "Hi, <br/>";
                                MailBody = MailBody + "This is an automatically generated message in reference to a order request. An approval action is required before OMS can proceed. <br/> Thank you for your request. Before we can proceed, we need approver's formal approval to proceed. <br/>";
                                if (ordertype == "Direct Import" && Status == 3)
                                {
                                    MailBody = MailBody + "<br/><br/>" + "Order has been submitted sucessfully with Direct Order Import";
                                }
                                MailBody = MailBody + dsMailSubBody.Tables[0].Rows[0]["MailBody"].ToString();
                                if (ISProjectSite == "Yes")
                                {
                                    MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetailVodafoneTechnical(Request, conn);
                                }
                                else
                                {
                                    MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                                }
                                // MailBody = MailBody + "<br/><br/>" + EMailGetRequestDetail(Request, conn);
                                MailBody = MailBody + "<br/><br/>" + EmailGetAddressDetails(Request, conn);
                                MailBody = MailBody + "<br/><br/>" + EMailGetRequestPartDetail(RequestID, conn);
                                if (ordertype == "Direct Import" && Status == 3)
                                {
                                    MailBody = MailBody + "<br/><br/>" + "Note : The order processes through direct import doesn't required any approver. These Order send to WMS for further processing ";
                                }
                                SendMail(MailBody + MailGetFooter(), MailSubject, Contact2Emails);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "Sp_EnterErrorTracking";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("Data", ex.Data.ToString());
                    cmd.Parameters.AddWithValue("GetType", ex.Source.ToString());
                    cmd.Parameters.AddWithValue("InnerException", "Error");
                    cmd.Parameters.AddWithValue("Message", ex.Message.ToString());
                    cmd.Parameters.AddWithValue("Source", " EmailSendWhenRequestSubmit");
                    cmd.Parameters.AddWithValue("DateTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("UserID", 20000);
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
                Result = 3;//mail not sent to requestor.
            }
            finally { }
            return Result;
        }



        public void InsertintCorrespond(long OrderHeadId, string MessageTitle, string Message, DateTime date, string MessageSource, string MessageType,
                                    long DeptID, DateTime OrderDate, long CurrentOrderStatus, int MailStatus, long OrderCancelDay, long AutoCancelReminderSchedules
                                        , DateTime NxtAutoCancelReminderDate, string[] conn)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_InsertintCorrespond";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@OrderHeadId", OrderHeadId);
                    cmd.Parameters.AddWithValue("@MessageTitle", MessageTitle);
                    cmd.Parameters.AddWithValue("@Message", Message);
                    cmd.Parameters.AddWithValue("@date", date);
                    cmd.Parameters.AddWithValue("@MessageSource", MessageSource);
                    cmd.Parameters.AddWithValue("@MessageType", MessageType);
                    cmd.Parameters.AddWithValue("@DeptID", DeptID);
                    cmd.Parameters.AddWithValue("@OrderDate", OrderDate);
                    cmd.Parameters.AddWithValue("@CurrentOrderStatus", CurrentOrderStatus);
                    cmd.Parameters.AddWithValue("@MailStatus", MailStatus);
                    cmd.Parameters.AddWithValue("@OrderCancelDay", OrderCancelDay);
                    cmd.Parameters.AddWithValue("@AutoCancelReminderSchedules", AutoCancelReminderSchedules);
                    cmd.Parameters.AddWithValue("@NxtAutoCancelReminderDate", NxtAutoCancelReminderDate);
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
            }
            catch (System.Exception ex)
            {

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "Sp_EnterErrorTracking";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("Data", ex.Data.ToString());
                    cmd.Parameters.AddWithValue("GetType", ex.Source.ToString());
                    cmd.Parameters.AddWithValue("InnerException", "Error");
                    cmd.Parameters.AddWithValue("Message", ex.Message.ToString());
                    cmd.Parameters.AddWithValue("Source", OrderHeadId + "| insert_OrderWiseAccess");
                    cmd.Parameters.AddWithValue("DateTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("UserID", "123");
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
            }

        }


        public void InsertintCorrespondApprovalReminder(long OrderHeadId, string MessageTitle, string Message, DateTime date, string MessageSource, 
            string MessageType, long DeptID, DateTime OrderDate, long CurrentOrderStatus, int MailStatus, long ApprovalReminderSchedules, DateTime NxtApprovalReminderDate, string[] conn)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_InsertintCorrespondApprovalReminder";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@OrderHeadId", OrderHeadId);
                    cmd.Parameters.AddWithValue("@MessageTitle", MessageTitle);
                    cmd.Parameters.AddWithValue("@Message", Message);
                    cmd.Parameters.AddWithValue("@date", date);
                    cmd.Parameters.AddWithValue("@MessageSource", MessageSource);
                    cmd.Parameters.AddWithValue("@MessageType", MessageType);
                    cmd.Parameters.AddWithValue("@DeptID", DeptID);
                    cmd.Parameters.AddWithValue("@OrderDate", OrderDate);
                    cmd.Parameters.AddWithValue("@CurrentOrderStatus", CurrentOrderStatus);
                    cmd.Parameters.AddWithValue("@MailStatus", MailStatus);
                    cmd.Parameters.AddWithValue("@ApprovalReminderSchedules", ApprovalReminderSchedules);
                    cmd.Parameters.AddWithValue("@NxtApprovalReminderDate", NxtApprovalReminderDate);
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
            }
            catch (System.Exception ex)
            {

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "Sp_EnterErrorTracking";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("Data", ex.Data.ToString());
                    cmd.Parameters.AddWithValue("GetType", ex.Source.ToString());
                    cmd.Parameters.AddWithValue("InnerException", "Error");
                    cmd.Parameters.AddWithValue("Message", ex.Message.ToString());
                    cmd.Parameters.AddWithValue("Source", OrderHeadId + "| insert_OrderWiseAccess");
                    cmd.Parameters.AddWithValue("DateTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("UserID", "123");
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
            }

        }


        public void AddIntomMessageTrans_New(long RequestID, string[] conn)
        {

            using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                //mMessageTran Msg = new mMessageTran();
                string Msgnew = "";

               // GWC_VW_MsgOrderHead MsgHD = new GWC_VW_MsgOrderHead();
                //MsgHD = db.GWC_VW_MsgOrderHead.Where(h => h.Id == RequestID).FirstOrDefault();

                DataSet dsh = new DataSet();
                dsh = GWC_VW_MsgOrderHead(RequestID, conn);

                if (dsh.Tables[0].Rows.Count > 0)
                {
                    //string ContactPerson2 = MsgHD.ContactP2;
                    string ContactPerson2 = dsh.Tables[0].Rows[0]["ContactP2"].ToString();


                    string ContactPerson2Names = "";
                    if (ContactPerson2 == "NA" || ContactPerson2 == "" || ContactPerson2 == null) { ContactPerson2Names = "NA"; }
                    else
                    { ContactPerson2Names = GetContactPersonNames(ContactPerson2, conn); }

                    /*New Code For Contact 2 MobileNo*/
                    string ContactMobileNo = "";
                    if (ContactPerson2Names == "NA")
                    {
                        //ContactMobileNo = MsgHD.Con1MobileNo.ToString();
                        ContactMobileNo = dsh.Tables[0].Rows[0]["Con1MobileNo"].ToString();
                    }
                    else
                    {
                        ContactMobileNo = GetContactPersonMobileNo(ContactPerson2, conn);
                    }
                    /*New Code For Contact 2 MobileNo*/

                    //Added by suraj 
                    string ContactMobile2 = "";
                    ContactMobile2 = GetContactMobile2(RequestID, conn);
                    //Get PaymentMethodValue
                    string PaymentMethodValue = "";
                    PaymentMethodValue = GetPaymentMethodValue(RequestID, conn);
                    //address2 and paymentmethdnm
                    string address2 = "", paymentmethdnm = "";
                    DataTable dtadd2 = new DataTable();
                    dtadd2 = GetAddressLine2(RequestID, conn);
                    if (dtadd2.Rows.Count > 0)
                    {
                        address2 = dtadd2.Rows[0]["AddressLine2"].ToString();
                        paymentmethdnm = dtadd2.Rows[0]["MethodName"].ToString();
                    }

                    long ID = Convert.ToInt64(dsh.Tables[0].Rows[0]["Id"].ToString());
                    string OrderNumber = dsh.Tables[0].Rows[0]["OrderNumber"].ToString();
                    string Orderdate = string.Empty;
                    DateTime DT = Convert.ToDateTime(dsh.Tables[0].Rows[0]["Orderdate"]);
                    Orderdate = DT.ToShortDateString();

                    string Deliverydate = string.Empty;
                    DateTime DT1 = Convert.ToDateTime(dsh.Tables[0].Rows[0]["Deliverydate"]);
                    Deliverydate = DT1.ToShortDateString();

                    string StoreCode = dsh.Tables[0].Rows[0]["StoreCode"].ToString();
                    string Con1 = dsh.Tables[0].Rows[0]["Con1"].ToString();
                    string AddressLine1 = dsh.Tables[0].Rows[0]["AddressLine1"].ToString();
                    string Remark = dsh.Tables[0].Rows[0]["Remark"].ToString();
                    string OrderNo = dsh.Tables[0].Rows[0]["OrderNo"].ToString();
                    string InvoiceNo = dsh.Tables[0].Rows[0]["InvoiceNo"].ToString();
                    string LocationCode = dsh.Tables[0].Rows[0]["LocationCode"].ToString();
                    string RequestorName = dsh.Tables[0].Rows[0]["RequestorName"].ToString();
                    string RequestorMobileNo = dsh.Tables[0].Rows[0]["RequestorMobileNo"].ToString();
                    string ConsigneeName = dsh.Tables[0].Rows[0]["ConsigneeName"].ToString();
                    string ConsigneeAddress = dsh.Tables[0].Rows[0]["ConsigneeAddress"].ToString();
                    long ConsigneePhone = Convert.ToInt64(dsh.Tables[0].Rows[0]["ConsigneePhone"].ToString());
                    string ProjectType = dsh.Tables[0].Rows[0]["ProjectType"].ToString();
                    string SiteCode = dsh.Tables[0].Rows[0]["SiteCode"].ToString();
                    string SiteName = dsh.Tables[0].Rows[0]["SiteName"].ToString();
                    string Latitude = dsh.Tables[0].Rows[0]["Latitude"].ToString();
                    string Longitude = dsh.Tables[0].Rows[0]["Longitude"].ToString();
                    string AccessRequirement = dsh.Tables[0].Rows[0]["AccessRequirement"].ToString();

                    //  Msgnew = MsgHD.Id + " | " + CheckString(MsgHD.OrderNumber) + " | " + MsgHD.Orderdate.Value.ToShortDateString() + " | " + MsgHD.Deliverydate.Value.ToShortDateString() + " | " + MsgHD.StoreCode + " | " + MsgHD.Con1 + " | " + ContactPerson2Names + " | " + CheckString(MsgHD.AddressLine1) + " | " + CheckString(MsgHD.Remark) + " | " + MsgHD.OrderNo + " | " + MsgHD.InvoiceNo + " | " + MsgHD.LocationCode + " | " + MsgHD.RequestorName + " | " + MsgHD.RequestorMobileNo + " | " + MsgHD.ConsigneeName + " | " + MsgHD.ConsigneeAddress + " | " + MsgHD.ConsigneePhone + " | " + ContactMobileNo + " | " + ContactMobile2 + " | " + CheckString(address2) + " | " + paymentmethdnm + " | " + PaymentMethodValue + " | " + MsgHD.ProjectType + " | " + MsgHD.SiteCode + " | " + MsgHD.SiteName + " | " + MsgHD.Latitude + " | " + MsgHD.Longitude + " | " + MsgHD.AccessRequirement;

                    Msgnew = ID + " | " + CheckString(OrderNumber) + " | " + Orderdate + " | " + Deliverydate + " | " + StoreCode + " | " + Con1 + " | " + ContactPerson2Names 
                        + " | " + CheckString(AddressLine1) + " | " + CheckString(Remark) + " | " + OrderNo + " | " + InvoiceNo + " | " +
                        LocationCode + " | " + RequestorName + " | " + RequestorMobileNo + " | " + ConsigneeName + " | " + 
                        ConsigneeAddress + " | " + ConsigneePhone + " | " + ContactMobileNo + " | " + ContactMobile2 + " | " + CheckString(address2) + " | " + paymentmethdnm +
                        " | " + PaymentMethodValue + " | " + ProjectType + " | " + SiteCode + " | " + SiteName + " | " + Latitude + " | " + Longitude +
                        " | " + AccessRequirement;



                    //***condition to check if Serial No changes flag applicable to Company i.e. Vodafone technical
                    DataSet ds = new DataSet();
                    //if (MsgHD.ChkApproval == "Yes")
                    string ChkApproval = dsh.Tables[0].Rows[0]["ChkApproval"].ToString();
                    if (ChkApproval == "Yes")
                    {
                        ds = GetMessageserialvalues(RequestID, conn);
                        if (ds.Tables[0].Rows.Count > 0)
                        {

                            Msgnew = Msgnew + ds.Tables[0].Rows[0]["SerialNo"].ToString();
                        }
                    }
                    else
                    {
                        ds = fillds("select * from GWC_VW_MsgOrderDetail where OrderHeadId=" + RequestID + " ", conn);
                        int Cnt = ds.Tables[0].Rows.Count;
                        if (Cnt > 0)
                        {
                            for (int i = 0; i <= Cnt - 1; i++)
                            {
                                long PrdID = long.Parse(ds.Tables[0].Rows[i]["SkuId"].ToString());

                                //mProduct prd = new mProduct();
                                //prd = db.mProducts.Where(p => p.ID == PrdID).FirstOrDefault();

                                string GroupSet = "";
                                GroupSet = CheckProductGroupSet(PrdID, conn);

                                //if (prd.GroupSet == "Yes")
                                if (GroupSet == "Yes")
                                {
                                    DataSet dsBomProds = new DataSet();
                                    dsBomProds = fillds("select SKUId,Quantity from mBOMDetail BD where BD.BOMheaderId=" + PrdID + "", conn);
                                    if (dsBomProds.Tables[0].Rows.Count > 0)
                                    {
                                        for (int b = 0; b <= dsBomProds.Tables[0].Rows.Count - 1; b++)
                                        {
                                            decimal bomQty = decimal.Parse(dsBomProds.Tables[0].Rows[b]["Quantity"].ToString());
                                            decimal Qty = decimal.Parse(ds.Tables[0].Rows[i]["OrderQty"].ToString());
                                            decimal FinalQty = Qty * bomQty;

                                            long bomPrd = long.Parse(dsBomProds.Tables[0].Rows[b]["SKUId"].ToString());
                                            DataSet dsPrdName = new DataSet();
                                            dsPrdName = fillds("select ProductCode from mproduct where ID=" + bomPrd + "", conn);
                                            string ProductCode = dsPrdName.Tables[0].Rows[0]["ProductCode"].ToString();

                                            Msgnew = Msgnew + " | " + ProductCode + " | " + ds.Tables[0].Rows[i]["UOM"].ToString() + " | " + "NA" + " | " + "No" + " | " + FinalQty;
                                        }
                                    }
                                }
                                // else if (prd.GroupSet == "No")
                                else if (GroupSet == "No")
                                {
                                    Msgnew = Msgnew + " | " + ds.Tables[0].Rows[i]["Prod_Code"].ToString() + " | " + ds.Tables[0].Rows[i]["UOM"].ToString() + " | " + ds.Tables[0].Rows[i]["SerialNumber"].ToString() + " | " + ds.Tables[0].Rows[i]["serialflag"].ToString() + " | " + ds.Tables[0].Rows[i]["OrderQty"].ToString();
                                }
                            }
                        }
                    }

                  //  string StoreCode1 = dsh.Tables[0].Rows[0]["StoreCode"].ToString();
                    using (SqlCommand c = new SqlCommand())
                    {
                        c.CommandType = CommandType.StoredProcedure;
                        c.CommandText = "SP_SavemMessageTrans";
                        c.Connection = svr.GetSqlConn(conn);
                        c.Parameters.Clear();
                        c.Parameters.AddWithValue("MessageHdrId", "1");
                        c.Parameters.AddWithValue("MsgDescription", Msgnew);
                        c.Parameters.AddWithValue("Object", "Order");
                        c.Parameters.AddWithValue("Destination", StoreCode);
                        c.Parameters.AddWithValue("Status", "0");
                        c.Parameters.AddWithValue("CreationDate", DateTime.Now);
                        c.Parameters.AddWithValue("Createdby", "OMS");
                        c.ExecuteNonQuery();
                    }
                }     
            }

        }



        public DataSet GWC_VW_MsgOrderHead(long RequestID, string[] conn)
        {

            DataSet ds = new DataSet();

            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GWC_VW_MsgOrderHeadNew";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@RequestID", RequestID);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;

            }

        }



    }

}
