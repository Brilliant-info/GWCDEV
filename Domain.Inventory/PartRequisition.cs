using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Activation;
using System.Configuration;
using Interface.Inventory;
using System.ServiceModel;
using System.Data.Objects;
using System.Globalization;
using System.Reflection;
using System.Xml.Linq;

using Domain.Server;
using Domain.Tempdata;

namespace Domain.Inventory
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]

    public partial class PartRequisition : Interface.Inventory.iPartRequisition
    {
        DataHelper datahelper = new DataHelper();
        Domain.Server.Server svr = new Server.Server();

        #region PartRequsition
        #region GetStatusDetailList
        /// <summary>
        /// GetDeparmentList is providing List of SiteMaste
        /// </summary>
        /// <returns></returns>
        /// 
        //public List<tStatusDetail> GetStatusDetailList(string[] conn)
        //{
        //    BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));

        //    List<tStatusDetail> StatusDetailList1 = new List<tStatusDetail>();
        //    var StatusDetailList = (from p in ce.mStatus 
        //                            select new { p.Status }).Distinct();

        //    var v = ce.tStatusDetails.Select(p => p.Status).Distinct();
        //    StatusDetailList1 = (from p in StatusDetailList
        //                         select new mStatu  { Status = p.Status }).ToList();

        //    return StatusDetailList1;
        //}
        public List<mStatu> GetStatusList(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mStatu> StatusList = new List<mStatu>();
            StatusList = ce.mStatus.Where(p => p.ObjectName == "MaterialRequest").ToList();
            return StatusList;
        }
        #endregion


        #region GetPartReq Data
        /// <summary>
        /// GetDeparmentList is providing List of SiteMaste
        /// </summary>
        /// <returns></returns>
        /// 
        public List<SP_GetPartReqData_Result> GetPartReqDataList(string userType, long siteID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));

            List<SP_GetPartReqData_Result> PartReqDataList = new List<SP_GetPartReqData_Result>();
            PartReqDataList = (from p in ce.SP_GetPartReqData()
                               select p).ToList();
            if (userType == "Admin")
            {
                PartReqDataList = (from p in ce.SP_GetPartReqData()
                                   select p).ToList();
            }
            else if (userType == "User")
            {
                PartReqDataList = (from p in ce.SP_GetPartReqData().Where(p => p.SiteID == siteID)
                                   select p).ToList();

            }

            return PartReqDataList;
        }
        #endregion

        #region GetUserMasterList
        /// <summary>
        /// GetUserMaster List is providing List of Users
        /// </summary>
        /// <returns></returns>
        /// 
        public List<vGetUserProfileList> GetmUserList(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<vGetUserProfileList> UserMaster = new List<vGetUserProfileList>();
            UserMaster = (from p in ce.vGetUserProfileLists
                          select p).ToList();
            if (UserMaster.Count == 0)
            {
                UserMaster = null;
            }
            return UserMaster;
        }
        #endregion

        public DataSet FillSearchedParts(string wherecondition, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            DataSet ds = new DataSet();
            ds.Reset();
            string str = "select ID,ProductCode, ProductTypeID,ProductCategoryID,ProductSubCategoryID,UOMID,Name,Description,PrincipalPrice,Active,OpeningBalance,CurrentBalance,ReorderLevel,MaxBalanceQuantity,MinOrderQuantity,CompanyID,ProductType,ProductCategory,ProductSubCategory,UOM from GetProductDetails where ID IN(" + wherecondition + ")";
            ds = fillds(str, conn);
            return ds;
        }

        protected DataSet fillds(String strquery, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            DataSet ds = new DataSet();

            System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection("Data Source=" + conn[0] + ";Initial Catalog=" + conn[1] + "; User ID=" + conn[3] + "; Password=" + conn[2] + ";");
            SqlDataAdapter da = new SqlDataAdapter(strquery, sqlConn);
            ds.Reset();
            da.Fill(ds);
            return ds;
        }

        #region GetRequisitions
        /// <summary>
        /// GetRequisitions is providing List of Requisitions
        /// </summary>
        /// <returns></returns>
        /// 
        public List<tPartRequisition> GettRequisitions(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<tPartRequisition> Requisitions = new List<tPartRequisition>();
            Requisitions = (from p in ce.tPartRequisitions
                            select p).ToList();
            if (Requisitions.Count == 0)
            {
                Requisitions = null;
            }
            return Requisitions;
        }
        #endregion

        #region GetRequisitions
        /// <summary>
        /// GetRequisitions is providing List of Requisitions
        /// </summary>
        /// <returns></returns>
        /// 
        public DataSet GetdsFromSql(string strSql, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            DataSet ds = new DataSet();
            ds.Reset();
            ds = fillds(strSql, conn);
            return ds;
        }
        #endregion

        #region GetRequisitionByID
        /// <summary>
        /// GetRequisitionByID is providing List of Requisition ByID
        /// </summary>
        /// <returns></returns>
        /// 
        public tPartRequisition GetRequisitionByID(long RecID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)); ;
            tPartRequisition Requisition = new tPartRequisition();
            Requisition = ce.tPartRequisitions.Where(p => p.PRM_ID == RecID).FirstOrDefault();
            ce.Detach(Requisition);
            return Requisition;
        }
        #endregion
        #region GetRequisitionDetailsByID
        /// <summary>
        /// GetRequisitionDetailsByID is providing List of RequisitionDetails ByID
        /// </summary>
        /// <returns></returns>
        /// 
        public tPartReqDetail GetRequisitionDetailsByID(long RecID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)); ;
            tPartReqDetail ReqDetails = new tPartReqDetail();
            ReqDetails = ce.tPartReqDetails.Where(p => p.PRD_ID == RecID).FirstOrDefault();
            ce.Detach(ReqDetails);
            return ReqDetails;
        }
        #endregion

        #region GetRequisitionDetailsByID
        /// <summary>
        /// GetRequisitionDetailsByID is providing List of RequisitionDetails ByID
        /// </summary>
        /// <returns></returns>
        /// 
        public tPartReqDetail GetRequisitionDetailsByPRD_ID(long RecID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)); ;
            tPartReqDetail ReqDetails = new tPartReqDetail();
            ReqDetails = ce.tPartReqDetails.Where(p => p.PRM_ID == RecID).First();
            ce.Detach(ReqDetails);
            return ReqDetails;
        }
        #endregion

        #region InsertRequisitions
        public long InsertRequisitions(tPartRequisition objRequisitions, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.tPartRequisitions.AddObject(objRequisitions);
            ce.SaveChanges();
            return objRequisitions.PRM_ID;
        }
        #endregion

        #region updateRequisitions
        public long updateRequisitions(tPartRequisition objRequisitions, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.tPartRequisitions.Attach(objRequisitions);
            ce.ObjectStateManager.ChangeObjectState(objRequisitions, EntityState.Modified);
            ce.SaveChanges();
            return 1;
        }
        #endregion


        #region InsertReqDetails
        public long InsertReqDetails(DataTable dt, long PRM_ID, string[] conn)
        {
            try
            {
                DataTable dtPR = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter("Select * from tPartReqDetail where PRM_ID = 0", svr.GetSqlConn(conn));
                da.Fill(dtPR);

                int SrNo = 1;
                foreach (DataRow dr in dt.Rows)
                {
                    dtPR.Rows.Add();
                    dtPR.Rows[dtPR.Rows.Count - 1]["PRM_ID"] = PRM_ID;
                    dtPR.Rows[dtPR.Rows.Count - 1]["Prod_ID"] = dr["ID"];
                    dtPR.Rows[dtPR.Rows.Count - 1]["ProdDescription"] = dr["PartDesc"];
                    dtPR.Rows[dtPR.Rows.Count - 1]["RequestQty"] = dr["ReqQty"];
                    dtPR.Rows[dtPR.Rows.Count - 1]["RemaningQty"] = dr["ReqQty"];
                    dtPR.Rows[dtPR.Rows.Count - 1]["Sequence"] = SrNo;
                    // dtPR.Rows[dtPR.Rows.Count - 1]["ProbableShippingDt"] = null;
                    SrNo += 1;
                }

                SqlCommand cmd = new SqlCommand("Delete from dbo.tPartReqDetail where PRM_ID = " + PRM_ID, svr.GetSqlConn(conn));
                cmd.Connection = svr.GetSqlConn(conn);
                if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                if (cmd.Connection.State == ConnectionState.Open) cmd.Connection.Close();
                SqlCommandBuilder cb = new SqlCommandBuilder(da);
                da.Update(dtPR);
                return 1;
            }
            catch { }
            finally { }
            return 0;
        }
        #endregion
        #region UpdateReqDetails
        public long UpdateReqDetails(tPartReqDetail objReqDetails, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.tPartReqDetails.Attach(objReqDetails);
            ce.ObjectStateManager.ChangeObjectState(objReqDetails, EntityState.Modified);
            ce.SaveChanges();

            return 1;
        }
        #endregion




        #region CheckForPartRefIdExist
        /// <summary>
        /// GetMaterialIssueDetailsByID is providing List of Material Issue Details ByID
        /// </summary>
        /// <returns></returns>
        /// 
        public string CheckForPartRefIdExist(long PartRecID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string i;
            List<tMIN> objtmin = new List<tMIN>();
            objtmin = ce.tMINs.Where(p => p.ReferenceID == PartRecID).ToList();

            if (objtmin.Count > 0)
            {
                i = "1";//record exist in issue table 
            }
            else
            {
                i = "0";//record doesn't exist in issue
            }

            return i;
        }
        #endregion


        #region GetDataForDefaultView
        /// <summary>
        /// GetDeparmentList is providing List of SiteMaste
        /// </summary>
        /// <returns></returns>
        /// 
        public DataSet GetDataForDefaultView(string userType, long siteID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            DataSet ds = new DataSet();
            ds.Reset();
            string str = "exec SP_GetIssueData";
            ds = fillds(str, conn);
            return ds;


        }
        #endregion


        public string  GetRequestStatus(string userType, long ReqID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));

            SP_GetPartReqData_Result PartReqDataList = new SP_GetPartReqData_Result();
           string Status;
          //  PartReqDataList = (from p in ce.SP_GetPartReqData().Where(p=>p.ReferenceID==ReqID select p)).ToList();
            //PartReqDataList = (from p in ce.SP_GetPartReqData()
            //          where p.ReferenceID == ReqID select p).FirstOrDefault();

            var output = (from p in ce.SP_GetPartReqData()
                          where p.ReferenceID == ReqID
                          select p).FirstOrDefault();
            Status = output.status.ToString();
            return Status;
        }

        #region

        public List<SP_GetIssueData_Result> GetPartReqDataIssueList(string userType, long siteID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));

            List<SP_GetIssueData_Result> PartReqDataList = new List<SP_GetIssueData_Result>();
            //PartReqDataList = (from p in ce.SP_GetIssueData()
            //                   select p).ToList();
            if (userType == "Admin")
            {
                PartReqDataList = (from p in ce.SP_GetIssueData()
                                   select p).ToList();
            }
            else if (userType == "User")
            {
                PartReqDataList = (from p in ce.SP_GetIssueData().Where(p => p.SiteID == siteID)
                                   select p).ToList();

            }

            return PartReqDataList;
        }


        #endregion
        #endregion



        #region PartRequsitionDetls

        public List<SP_GetProductListForPartRequisition_Result> FillInventoryProductDetailByInventoryID(string paraSessionID, long paraInventoryID, long paraSiteID, string paraUserID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetProductListForPartRequisition_Result> lst = new List<SP_GetProductListForPartRequisition_Result>();
            lst = (from db in ce.SP_GetProductListForPartRequisition(paraInventoryID, paraSiteID)
                   select db).ToList();
            SaveTempData(lst, paraSessionID, paraUserID, conn);
            return lst;
        }

        
        public void SaveTempData(List<SP_GetProductListForPartRequisition_Result> saveLst, string paraSessionID, string paraUserID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            TempData tempdata = new TempData();
            TempData tempdata1 = new TempData();
            tempdata = (ce.TempDatas.Where(a => a.ObjectName == "PartRequisitionProduct" && a.SessionID == paraSessionID)).FirstOrDefault();
            tempdata1 = (ce.TempDatas.Where(a => a.ObjectName == "PartRequisitionProduct" && a.SessionID == paraSessionID)).FirstOrDefault();
            string xml = "";
            xml = datahelper.SerializeEntity(saveLst);

            if (tempdata == null) { tempdata = new TempData(); }

            tempdata.Data = xml;
            tempdata.XmlData = "";
            tempdata.LastUpdated = DateTime.Now;
            tempdata.SessionID = paraSessionID.ToString();
            tempdata.UserID = paraUserID.ToString();
            tempdata.ObjectName = "PartRequisitionProduct";
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
                        && rec.ObjectName == "PartRequisitionProduct"
                        select rec).FirstOrDefault();
            if (tempdata != null) { ce.DeleteObject(tempdata); ce.SaveChanges(); }

        }


        public List<SP_GetProductListForPartRequisition_Result> AddProduct(long[] paraProductIDs, string paraSessionID,  string paraUserID, long SiteID,string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetProductListForPartRequisition_Result> lst = new List<SP_GetProductListForPartRequisition_Result>();
            List<SP_GetProductListForPartRequisition_Result> Existinglst = new List<SP_GetProductListForPartRequisition_Result>();
            Existinglst = GetTempDataByObjectNameSessionID(paraSessionID, paraUserID, conn);

            List<SP_GetProductListForPartRequisition_Result> NewList = new List<SP_GetProductListForPartRequisition_Result>();
            NewList = (from db in ce.SP_GetProductListForPartRequisition(0, SiteID)
                       where paraProductIDs.Contains(Convert.ToInt32(db.ProductID))
                       select db).ToList();


            /*Begin : Merge (Existing + Newly Added) Products to Create TempData of AddToCart*/
            List<SP_GetProductListForPartRequisition_Result> mergedList = new List<SP_GetProductListForPartRequisition_Result>();
            mergedList.AddRange(Existinglst);
            mergedList.AddRange(NewList);

            lst = SetSequence(mergedList);
            SaveTempData(lst, paraSessionID, paraUserID, conn);
            return lst;
        }


        public List<SP_GetProductListForPartRequisition_Result> GetTempDataByObjectNameSessionID(string paraSessionID, string paraUserID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetProductListForPartRequisition_Result> lst = new List<SP_GetProductListForPartRequisition_Result>();
            TempData tempdata = new TempData();
            tempdata = (from temp in ce.TempDatas
                        where temp.SessionID == paraSessionID
                        && temp.ObjectName == "PartRequisitionProduct"
                        && temp.UserID == paraUserID
                        select temp).FirstOrDefault();
            if (tempdata != null)
            {
                lst = datahelper.DeserializeEntity1<SP_GetProductListForPartRequisition_Result>(tempdata.Data);
            }
            return lst;
        }


        public List<SP_GetProductListForPartRequisition_Result> SetSequence(List<SP_GetProductListForPartRequisition_Result> lst)
        {
            long setRowNo = 1;
            var setSequence = from rec in lst
                              select new SP_GetProductListForPartRequisition_Result()
                              {
                                  ID = rec.ID,
                                  Sequence = setRowNo++,
                                  ProductID = rec.ProductID,
                                  ProductCode = rec.ProductCode,
                                  ProductName = rec.ProductName,
                                  UOM = rec.UOM,
                                  Description=rec.Description,
                                  ReqQty = rec.ReqQty,
                                  ProductRate = rec.ProductRate,
                                  IssueQty=rec.IssueQty,
                                  ReciptQty=rec.ReciptQty,
                                  SiteProd_Bal=rec.SiteProd_Bal,
                                  SiteID=rec.SiteID
                                
                              };

            List<SP_GetProductListForPartRequisition_Result> finalList = new List<SP_GetProductListForPartRequisition_Result>();
            finalList = setSequence.ToList<SP_GetProductListForPartRequisition_Result>();
            return finalList;
        }

        public void UpdateProductDetail(string paraSessionID, string paraUserID, SP_GetProductListForPartRequisition_Result order, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetProductListForPartRequisition_Result> lst = new List<SP_GetProductListForPartRequisition_Result>();
            lst = GetTempDataByObjectNameSessionID(paraSessionID, paraUserID, conn);
            SP_GetProductListForPartRequisition_Result updateRec = new SP_GetProductListForPartRequisition_Result();
            updateRec = lst.Where(u => u.Sequence == order.Sequence).FirstOrDefault();
            if (updateRec == null)
            {
                SP_GetProductListForPartRequisition_Result updateRec1 = new SP_GetProductListForPartRequisition_Result();
                updateRec1.ReqQty = order.ReqQty;
            }
            else
            {
                updateRec.ReqQty = order.ReqQty;
            }
            SaveTempData(lst, paraSessionID, paraUserID, conn);
        }

        public void FinalSaveToDBtInventoryProductDetail(string paraSessionID, long paraReferenceID, string paraUserID, string paraContainer, string paraEngineModel, string paraEngineSerial, string paraFailureCauses, string paraFailureNature, string paraGenerateModel, long paraFailureHrs, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetProductListForPartRequisition_Result> paraobjList = new List<SP_GetProductListForPartRequisition_Result>();
            paraobjList = GetTempDataByObjectNameSessionID(paraSessionID, paraUserID, conn);

            XElement xmlEle = new XElement("PartRequisitionProductDetailList", from rec in paraobjList
                                                                              select new XElement("PartRequisitionProduct",
                                                               new XElement("ID", rec.ID),
                                                               new XElement("Sequence", rec.Sequence),
                                                               new XElement("ProductID", rec.ProductID),
                                                               new XElement("ProductCode", rec.ProductCode),
                                                               new XElement("ProductName", rec.ProductName),
                                                               new XElement("Description", rec.Description),
                                                               new XElement("UOM", rec.UOM),
                                                               new XElement("ReqQty", rec.ReqQty),
                                                               new XElement("ProductRate", rec.ProductRate),
                                                               new XElement("IssueQty", rec.IssueQty),
                                                               new XElement("ReciptQty", rec.ReciptQty),
                                                               new XElement("SiteProd_Bal", rec.SiteProd_Bal)
                                                               
                                                               ));
            TempData tempdata = new TempData();

            tempdata = (ce.TempDatas.Where(a => a.ObjectName == "PartRequisitionProduct" && a.SessionID == paraSessionID)).FirstOrDefault();
            tempdata.XmlData = xmlEle.ToString();
            tempdata.LastUpdated = DateTime.Now;
            tempdata.SessionID = paraSessionID.ToString();
            tempdata.UserID = paraUserID.ToString();
            tempdata.ObjectName = "PartRequisitionProduct";
            tempdata.TableName = "table";
            ce.SaveChanges();

            ObjectParameter _paraSessionID = new ObjectParameter("paraSessionID", typeof(string));
            _paraSessionID.Value = paraSessionID;

            ObjectParameter _paraObjectName = new ObjectParameter("paraObjectName", typeof(string));
            _paraObjectName.Value = "PartRequisitionProduct";

            ObjectParameter _paraReferenceID = new ObjectParameter("paraReferenceID", typeof(long));
            _paraReferenceID.Value = paraReferenceID;

            ObjectParameter _paraUserID = new ObjectParameter("paraUserID", typeof(string));
            _paraUserID.Value = paraUserID;

            ObjectParameter _paraContainer = new ObjectParameter("paraContainer", typeof(string));
            _paraContainer.Value = paraContainer;

            ObjectParameter _paraEngineModel = new ObjectParameter("paraEngineModel", typeof(string));
            _paraEngineModel.Value = paraEngineModel;

            ObjectParameter _paraEngineSerial = new ObjectParameter("paraEngineSerial", typeof(string));
            _paraEngineSerial.Value = paraEngineSerial;

            ObjectParameter _paraFailureCauses = new ObjectParameter("paraFailureCauses", typeof(string));
            _paraFailureCauses.Value = paraFailureCauses;

            ObjectParameter _paraFailureNature = new ObjectParameter("paraFailureNature", typeof(string));
            _paraFailureNature.Value = paraFailureNature;

            ObjectParameter _paraGenerateModel = new ObjectParameter("paraGenerateModel", typeof(string));
            _paraGenerateModel.Value = paraGenerateModel;

            ObjectParameter _paraFailureHrs = new ObjectParameter("paraFailureHrs", typeof(long));
            _paraFailureHrs.Value = paraFailureHrs;


            ObjectParameter[] obj = new ObjectParameter[] { _paraSessionID, _paraObjectName, _paraReferenceID, _paraUserID, _paraContainer, _paraEngineModel, _paraEngineSerial, _paraFailureCauses, _paraFailureNature, _paraGenerateModel, _paraFailureHrs };
            ce.ExecuteFunction("SP_InsertIntotPartReqDetail", obj);
            ce.SaveChanges();
            ClearTempData(paraSessionID, paraUserID, conn);

        }


        public List<SP_GetProductListForPartRequisition_Result> RemoveProductFromTempDataList(string paraSessionID, string paraUserID, int paraSequence, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            /*Begin : Get Existing Records from TempData*/
            List<SP_GetProductListForPartRequisition_Result> existingDiscountList = new List<SP_GetProductListForPartRequisition_Result>();
            existingDiscountList = GetTempDataByObjectNameSessionID(paraSessionID, paraUserID, conn);
            /*End*/

            /*Get Filter List [Filter By paraSequence]*/
            List<SP_GetProductListForPartRequisition_Result> filterList = new List<SP_GetProductListForPartRequisition_Result>();
            filterList = (from exist in existingDiscountList
                          where exist.Sequence != paraSequence
                          select exist).ToList();
            /*End*/

            List<SP_GetProductListForPartRequisition_Result> result = new List<SP_GetProductListForPartRequisition_Result>();
            /*Set Sequence*/
            result = SetSequence(filterList);

            /*End*/

            /*Save result to TempData*/
            SaveTempData(result, paraSessionID, paraUserID, conn);
            /*End*/

            return result;
        }



        #endregion

    }
}
