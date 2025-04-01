using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Activation;
using System.Configuration;
using System.ServiceModel;
using System.Data.Entity;
using Domain.Server;
using Domain.Tempdata;
using System.Xml.Linq;
using System.Data.Objects;
using Interface.AccountSearchDetails;

namespace Domain.AccountSearchDetail
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]

    public partial class AccountSearchDetails : iCustomer
    {
        Domain.Server.Server svr = new Server.Server();
        DataHelper datahelper = new DataHelper();

        #region GetAccountList
        /// <summary>
        /// GetAccountList is providing List of Account
        /// </summary>
        /// <returns></returns>
        /// 
        public List<GetCustomerDetail> GetGetCustomerDetail(long UserID, string LoginUserType, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));

            List<GetCustomerDetail> AccountList = new List<GetCustomerDetail>();

            if (LoginUserType == "Admin")
            {
                AccountList = (from p in ce.GetCustomerDetails
                               orderby p.ID descending
                               select p).ToList();
            }
            else
            {
                AccountList = (from p in ce.GetCustomerDetails
                               //where p.BranchID == UserID
                               orderby p.ID descending
                               select p).ToList();
            }

            if (AccountList.Count == 0)
            {
                AccountList = null;
            }
            return AccountList;
        }

        //        public List<mFre

        public List<GetCustomerDetail> GetGetCustomerDetailForApplication(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<GetCustomerDetail> AccountListApplication = new List<GetCustomerDetail>();
            // List<GetCustomerDetail1> AccountListApplication = new List<GetCustomerDetail1>();
            AccountListApplication = (from a in ce.GetCustomerDetails
                                      // where a.Status == "Application" || a.Status == "Reject" || a.Status == "Composing"
                                      select a).ToList();

            if (AccountListApplication.Count == 0)
            {
                AccountListApplication = null;
            }
            return AccountListApplication;
        }

        public List<GetCustomerDetail> GetGetCustomerDetailForAdmission(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<GetCustomerDetail> AdmissionList = new List<GetCustomerDetail>();
            //AdmissionList = (from adms in ce.GetCustomerDetails
            //                 where adms.Status == "Approved"
            //                 || adms.Status == "Application"
            //                 || adms.Status == "Pending For Approval"
            //                 select adms).ToList();
            if (AdmissionList.Count == 0)
            {
                AdmissionList = null;
            }
            return AdmissionList;
        }

        #endregion

        #region AllMethodsForAccountMaster
        #region GetLeadSector
        /// <summary>
        /// GetLeadSector is providing List of Lead Sector for binding Dropdown
        /// </summary>
        /// <returns></returns>
        /// 
        public List<mSector> GetLeadSector(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mSector> ObjLeadSector = new List<mSector>();
            ObjLeadSector = (from p in ce.mSectors
                             where p.Active == "Y"
                             orderby p.Name
                             select p).ToList();
            return ObjLeadSector;
        }
        #endregion

        #region GetCompanyType
        /// <summary>
        /// GetCompanyType is providing List of Company Types for binding Dropdown
        /// </summary>
        /// <returns></returns>
        /// 
        public List<mCustomerType> GetCompanyType(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mCustomerType> ObjCompanyType = new List<mCustomerType>();
            ObjCompanyType = (from p in ce.mCustomerTypes
                              where p.Active == "Y"
                              orderby p.CompanyType
                              select p).ToList();
            return ObjCompanyType;
        }
        #endregion

        #region SaveCustomerDetails
        protected DataSet fillds(string strquery, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            DataSet ds = new DataSet();
            System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection("Data Source=" + conn[0] + ";Initial Catalog=" + conn[1] + "; User ID=" + conn[3] + "; Password=" + conn[2] + ";");
            SqlDataAdapter da = new SqlDataAdapter(strquery, sqlConn);
            ds.Reset();
            da.Fill(ds);
            return ds;
        }

        public DataSet getProductIDByCourseID(long CourseID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "sp_getProductIDByCourseID";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("CourseID", CourseID);
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;
        }

        public void SavePymentDetails(long RegID, long CourseID, string TransactionAmt, DateTime TransactionDate, string PayMode, DateTime DateCreated, long Product_Id, DateTime StartDate, DateTime EndDate, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[sp_SavePayemntDetails]";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("RegID", RegID);
            cmd.Parameters.AddWithValue("CourseID", CourseID);
            cmd.Parameters.AddWithValue("TransactionAmt", TransactionAmt);
            cmd.Parameters.AddWithValue("TransactionDate", TransactionDate);
            cmd.Parameters.AddWithValue("PayMode", PayMode);
            cmd.Parameters.AddWithValue("DateCreated", DateCreated);
            cmd.Parameters.AddWithValue("Product_Id", Product_Id);
            cmd.Parameters.AddWithValue("StartDate", StartDate);
            cmd.Parameters.AddWithValue("EndDate", EndDate);
            cmd.ExecuteNonQuery();
        }

        public void SaveContactDetails(long RegID, string email, string Mobile, DateTime DateCreated, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[sp_SaveContactDetails]";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("RegID", RegID);
            cmd.Parameters.AddWithValue("email", email);
            cmd.Parameters.AddWithValue("Mobile", Mobile);
            cmd.Parameters.AddWithValue("Active", "Y");
            cmd.Parameters.AddWithValue("DateCreated", DateCreated);
            //cmd.Parameters.AddWithValue("datemodified", datemodified);
            cmd.ExecuteNonQuery();
        }
        public void SaveContactDetails1(long RegID, string email, string Mobile, DateTime datemodified, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[sp_SaveContactDetails1]";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("RegID", RegID);
            cmd.Parameters.AddWithValue("email", email);
            cmd.Parameters.AddWithValue("Mobile", Mobile);
            cmd.Parameters.AddWithValue("Active", "Y");
            //cmd.Parameters.AddWithValue("DateCreated", DateCreated);
            cmd.Parameters.AddWithValue("datemodified", datemodified);
            cmd.ExecuteNonQuery();
        }



        public int SaveCustomerDetails(tCustomerHead cust, string State, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            if (State == "AddNew")
            {
                ce.tCustomerHeads.AddObject(cust);
                ce.SaveChanges();
            }
            else if (State == "Edit")
            {
                ce.tCustomerHeads.Attach(cust);
                ce.ObjectStateManager.ChangeObjectState(cust, EntityState.Modified);
                ce.SaveChanges();
            }
            return Convert.ToInt32(cust.ID);
        }

        //public int SaveRegistrationDetails(tbl_Registration Regis, string state, string[] conn)
        //{
        //    int result = 0;
        //    BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
        //    if (state == "AddNew")
        //    {
        //        ce.tbl_Registration.AddObject(Regis);
        //        ce.SaveChanges();

        //        result = Convert.ToInt32(Regis.Id);
        //    }
        //    else if (state == "Edit")
        //    {
        //        //ce.tbl_Registration.Attach(Regis);
        //        //ce.ObjectStateManager.ChangeObjectState(Regis, EntityState.Modified);
        //        //ce.SaveChanges();

        //        tbl_Registration reg = null;

        //        using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
        //        {
        //            ce.ContextOptions.LazyLoadingEnabled = false;
        //            reg = (from r in db.tbl_Registration
        //                   where r.StudID == Regis.StudID
        //                   select r).FirstOrDefault();
        //        }
        //        if (reg == null)
        //        {
        //            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
        //            try
        //            {
        //                db.tbl_Registration.AddObject(Regis);
        //                db.SaveChanges();
        //            }
        //            catch { result = Convert.ToInt32(Regis.Id); }
        //        }
        //        else
        //        {
        //            reg.Title = Regis.Title;
        //            reg.Fname = Regis.Fname;
        //            reg.Mname = Regis.Mname;
        //            reg.Lname = Regis.Lname;
        //            reg.Gender = Regis.Gender;
        //            reg.Active = Regis.Active;
        //            reg.datemodified = DateTime.Now;
        //            reg.Medium = Regis.Medium;
        //            reg.StudID = Regis.StudID;

        //            using (BISPL_CRMDBEntities ce1 = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
        //            {
        //                ce1.tbl_Registration.Attach(reg);
        //                ce1.ObjectStateManager.ChangeObjectState(reg, System.Data.EntityState.Modified);
        //                ce1.SaveChanges();

        //                result = Convert.ToInt32(reg.Id);
        //            }
        //        }
        //    }
        //    return result;
        //}

        //public int SaveLoginDetails(tbl_Login Login, string state, string[] conn)
        //{
        //    BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
        //    if (state == "AddNew")
        //    {
        //        ce.tbl_Login.AddObject(Login);
        //        ce.SaveChanges();
        //    }
        //    else
        //    {
        //        //ce.tbl_Login.Attach(Login);
        //        //ce.ObjectStateManager.ChangeObjectState(Login, EntityState.Modified);
        //        //ce.SaveChanges();

        //        tbl_Login log = null;
        //        using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
        //        {
        //            ce.ContextOptions.LazyLoadingEnabled = false;
        //            log = (from l in db.tbl_Login
        //                   where l.RegID == Login.RegID
        //                   select l).FirstOrDefault();
        //        }
        //        if (log == null)
        //        {
        //            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
        //            try
        //            {
        //                db.tbl_Login.AddObject(Login);
        //                db.SaveChanges();
        //            }
        //            catch { return 0; }
        //        }
        //        else
        //        {
        //            log.RegID = Login.RegID;
        //            log.UserName = Login.UserName;
        //            log.Password = Login.Password;
        //            log.Active = Login.Active;
        //            log.datemodified = DateTime.Now;

        //            using (BISPL_CRMDBEntities ce1 = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
        //            {
        //                ce1.tbl_Login.Attach(log);
        //                ce1.ObjectStateManager.ChangeObjectState(log, System.Data.EntityState.Modified);
        //                ce1.SaveChanges();
        //            }
        //        }

        //    }
        //    return Convert.ToInt32(Login.Id);
        //}


        public DataSet GetOptionalSubject(string[] conn)
        {
            DataSet ds = new DataSet();
            ds.Reset();
            string str = "select ID,Subject from msubject where optional=0";
            ds = fillds(str, conn);
            return ds;
        }


        #endregion

        public int SaveOpeningBalance(tOpeningBalance ObjOpeningBal, string state, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            if (state == "AddNew")
            {
                ce.tOpeningBalances.AddObject(ObjOpeningBal);
                ce.SaveChanges();
            }
            else if (state == "Edit")
            {
                ce.tOpeningBalances.Attach(ObjOpeningBal);
                ce.ObjectStateManager.ChangeObjectState(ObjOpeningBal, EntityState.Modified);
                ce.SaveChanges();
            } return Convert.ToInt32(ObjOpeningBal.ID);
        }


        #region Select data from tOpeningBalance
        public tOpeningBalance GetTOpeningBalanceDtls(long CustomerId, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            tOpeningBalance obj = new tOpeningBalance();
            obj = (from p in ce.tOpeningBalances
                   where p.ReferenceID == CustomerId
                   select p).FirstOrDefault();
            //ce.Detach(obj);
            if (obj == null) { tOpeningBalance obj1 = new tOpeningBalance(); obj = obj1; }
            return obj;
        }
        #endregion

        #region GetCustomerHeadDetailByCustomerID
        /// <summary>
        /// GetCustomerHeadDetail is providing List of Customers
        /// </summary>
        /// <returns></returns>
        /// 
        //public List<tCustomerHead> GetCustomerHeadDetail()
        public tCustomerHead GetCustomerHeadDetailByCustomerID(long customerID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            tCustomerHead ObjCust = new tCustomerHead();
            ObjCust = (from p in ce.tCustomerHeads
                       where p.ID == customerID
                       select p).FirstOrDefault();
            ce.Detach(ObjCust);
            return ObjCust;

        }

        public GetCustomerDetail GetAllCustomerDetailsByID(long CustomerID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            GetCustomerDetail ObjCust = new GetCustomerDetail();
            ObjCust = (from p in ce.GetCustomerDetails
                       where p.ID == CustomerID
                       select p).FirstOrDefault();
            ce.Detach(ObjCust);
            return ObjCust;
        }
        #endregion

        #region checkDuplicateRecordForVendorName
        /// <summary>
        /// checkDuplicateRecord is providing List of vendor by VendorName 
        /// </summary>
        /// <returns></returns>
        /// 
        public string checkDuplicateRecord(string AccountName, string userID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            // long FormNo = long.Parse(AccountName);
            //var output = (from p in ce.tCustomerHeads
            //              //where p.Form_No == AccountName && p.CreatedBy == userID
            //              select new { p.Form_No }).FirstOrDefault();

            //if (output != null)
            //{
            //    result = "Form Number Already Exist";
            //}
            return result;

        }
        #endregion

        #region checkDuplicateRecordForVendorNameWithId
        /// <summary>
        /// checkDuplicateRecord for Edit is providing List of Department by Vendor Name and ID
        /// </summary>
        /// <returns></returns>
        /// 
        public string checkDuplicateRecordEdit(int CustomerID, string AccountName, string UserID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            //long FormNo = long.Parse(AccountName);
            //var output = (from p in ce.tCustomerHeads
            //              where p.Form_No == AccountName && p.ID != CustomerID && p.CreatedBy == UserID
            //              select new { p.Form_No }).FirstOrDefault();


            //if (output != null)
            //{
            //    result = "Form Number Already Exist";
            //}
            return result;

        }
        #endregion

        #endregion

        #region AccountHistory

        public List<SP_AcountHistory_Result> GetAccountHistoryDtls(string paraInvoiceId, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            //ce.Connection.ChangeDatabase("BISPL_CRMDB_new"); 
            List<SP_AcountHistory_Result> AccountHistoryList = new List<SP_AcountHistory_Result>();

            AccountHistoryList = (from db in ce.SP_AcountHistory(paraInvoiceId)
                                  select db).ToList();

            if (AccountHistoryList.Count == 0)
            {
                AccountHistoryList = null;
            }
            return AccountHistoryList;
        }
        #endregion

        #region AccountCourse

        public SP_CourseStudentWise_Result GetCourseTempData(string SessionID, string TargetObjectName, string UserID, string[] conn)
        {
            List<SP_CourseStudentWise_Result> CourseList = new List<SP_CourseStudentWise_Result>();
            CourseList = GetTempDataofCourse(TargetObjectName, SessionID, UserID, conn);

            SP_CourseStudentWise_Result Course = new SP_CourseStudentWise_Result();
            Course = CourseList.FirstOrDefault();

            return Course;
        }

        public List<SP_CourseStudentWise_Result> GetTempDataofCourse(string TargetObjectName, string SessionID, string UserID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_CourseStudentWise_Result> objAddToCourse = new List<SP_CourseStudentWise_Result>();

            TempData tempdata = new TempData();
            tempdata = (from temp in db.TempDatas
                        where temp.SessionID == SessionID
                        && temp.ObjectName == "Course_Course"
                        && temp.UserID == UserID
                        select temp).FirstOrDefault();
            List<SP_CourseStudentWise_Result> objAddToCourse2 = new List<SP_CourseStudentWise_Result>();
            if (tempdata != null)
            {
                objAddToCourse = datahelper.DeserializeEntity1<SP_CourseStudentWise_Result>(tempdata.Data);
            }
            return objAddToCourse;
        }

        public void SetValuesToTempData_onChangeCourse(string SessionID, string UserID, string TargetObjectName, int Sequence, SP_CourseStudentWise_Result paraInput, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_CourseStudentWise_Result> existingList = new List<SP_CourseStudentWise_Result>();
            existingList = GetTempDataofCourse(TargetObjectName, SessionID, UserID, conn);

            SP_CourseStudentWise_Result editRow = new SP_CourseStudentWise_Result();
            editRow = (from exist in existingList
                       where exist.ID == Sequence
                       select exist).FirstOrDefault();
            editRow = paraInput;
            existingList = existingList.Where(e => e.ID != Sequence).ToList();
            existingList.Add(editRow);
            existingList = (from e in existingList
                            orderby e.ID
                            select e).ToList();

            SaveTempDataToDbCrse(existingList, SessionID, UserID, TargetObjectName, conn);
        }

        public void InsertIntoTempCourse(SP_CourseStudentWise_Result Course, string SessionID, string UserID, string TargetObjectName, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_CourseStudentWise_Result> existingCourseList = new List<SP_CourseStudentWise_Result>();
            existingCourseList = GetTempDataofCourse(TargetObjectName, SessionID, UserID, conn);

            List<SP_CourseStudentWise_Result> mergeAddToCourseList = new List<SP_CourseStudentWise_Result>();
            mergeAddToCourseList.AddRange(existingCourseList);
            Course.ID = existingCourseList.Count + 1;
            mergeAddToCourseList.Add(Course);

            SaveTempDataToDbCrse(mergeAddToCourseList, SessionID, UserID, TargetObjectName, conn);
        }

        protected void SaveTempDataToDbCrse(List<SP_CourseStudentWise_Result> paraObjList, string paraSessionID, string paraUserID, string TargetObjectName, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            /*Begin : Remove Existing Records*/
            ClearTempDataFromDB(paraSessionID, paraUserID, TargetObjectName, conn);
            /*End*/

            string xml = "";
            xml = datahelper.SerializeEntity(paraObjList);
            /*End*/

            /*Begin : Save Serialized List into TempData */
            TempData tempdata = new TempData();
            tempdata.Data = xml;
            tempdata.XmlData = "";
            tempdata.LastUpdated = DateTime.Now;
            tempdata.SessionID = paraSessionID.ToString();
            tempdata.UserID = paraUserID.ToString();
            tempdata.ObjectName = TargetObjectName.ToString();
            tempdata.TableName = "-";
            db.AddToTempDatas(tempdata);
            db.SaveChanges();
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

        public List<SP_CourseStudentWise_Result> GetCourseDataByStudentID(long StudID, string sessionID, string userID, string CurrentObject, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_CourseStudentWise_Result> CourseDetail = new List<SP_CourseStudentWise_Result>();
            CourseDetail = (from sp in db.SP_CourseStudentWise(StudID)
                            select sp).ToList();
            SaveTempDataToDbCrse(CourseDetail, sessionID, userID, CurrentObject, conn);
            return CourseDetail;
        }

        public List<SP_CourseStudentWise_Result> GetCourseTempDataToBindGrid(string TargetObjectName, string SessionID, string UserID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_CourseStudentWise_Result> objAddToCourseList = new List<SP_CourseStudentWise_Result>();
            TempData tempdata = new TempData();
            tempdata = (from temp in db.TempDatas
                        where temp.SessionID == SessionID
                        && temp.ObjectName == "Course_Course"
                        && temp.UserID == UserID
                        select temp).FirstOrDefault();
            List<SP_CourseStudentWise_Result> objAddToCourseList1 = new List<SP_CourseStudentWise_Result>();
            if (tempdata != null)
            {
                objAddToCourseList = datahelper.DeserializeEntity1<SP_CourseStudentWise_Result>(tempdata.Data);
            }
            SaveTempDataToDbCrse(objAddToCourseList, SessionID, UserID, TargetObjectName, conn);
            return objAddToCourseList.ToList();
        }

        public string GetProductNameFromID(long PrdCode, string[] conn)
        {
            string ProductCode = "";
            DataSet ds = new DataSet();
            ds.Reset();
            string str = "select ProductCode from mProduct where ID=" + PrdCode + "";
            ds = fillds(str, conn);
            ProductCode = ds.Tables[0].Rows[0]["ProductCode"].ToString();

            return ProductCode;
        }

        public void FinalSaveStudentCourse(string paraSessionID, string paraCurrentObjectName, long StudID, string paraUserID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_CourseStudentWise_Result> finalSaveLst = new List<SP_CourseStudentWise_Result>();
            finalSaveLst = GetExistingTempDataBySessionIDObjectNameCourse(paraSessionID, paraUserID, "Course_Course", conn);
            XElement xmlEle = new XElement("Course", from rec in finalSaveLst
                                                     select new XElement("CourseList",
                                                         new XElement("CustomerID", StudID),
                                                         new XElement("CourseID", rec.CourseID)
                                                         ));

            ObjectParameter _xmlData = new ObjectParameter("xmlData", typeof(string));
            _xmlData.Value = xmlEle.ToString();

            ObjectParameter[] obj = new ObjectParameter[] { _xmlData };
            db.ExecuteFunction("SP_InsertInto_tCustomerCourse", obj);

            db.SaveChanges();
            ClearTempDataFromDB(paraSessionID, paraUserID, "Course_Course", conn);
        }

        public List<SP_CourseStudentWise_Result> GetExistingTempDataBySessionIDObjectNameCourse(string paraSessionID, string paraUserID, string paraCurrentObjectName, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_CourseStudentWise_Result> objAddToDBCourseLst = new List<SP_CourseStudentWise_Result>();

            TempData tempdata = new TempData();
            tempdata = (from temp in db.TempDatas
                        where temp.SessionID == paraSessionID
                        && temp.ObjectName == paraCurrentObjectName
                        && temp.UserID == paraUserID
                        select temp).FirstOrDefault();
            if (tempdata != null)
            {
                objAddToDBCourseLst = datahelper.DeserializeEntity1<SP_CourseStudentWise_Result>(tempdata.Data);
            }
            return objAddToDBCourseLst;
        }

        public List<SP_CourseStudentWise_Result> RemoveCourseFromTempDataList(string paraSessionID, string paraUserID, string paraCurrentObjectName, int paraSequence, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_CourseStudentWise_Result> existingCourseList = new List<SP_CourseStudentWise_Result>();

            existingCourseList = GetExistingTempDataBySessionIDObjectNameCourse(paraSessionID, paraUserID, "Course_Course", conn);

            List<SP_CourseStudentWise_Result> FilterList = new List<SP_CourseStudentWise_Result>();
            FilterList = (from exist in existingCourseList
                          where exist.ID != paraSequence
                          select exist).ToList();

            SaveTempDataToDbCrse(FilterList, paraSessionID, paraUserID, paraCurrentObjectName, conn);

            return FilterList;
        }

        public string ChangeStatusOfSelectedRecord(string selectedRecd, string[] conn)
        {
            DataSet ds = new DataSet();
            ds.Reset();
            string str = "update tcustomerhead set Status='Approved' where id in (" + selectedRecd + ")";
            ds = fillds(str, conn);

            return "true";
        }

        public string ChangeStatusOfSelectedRecordToStud(string selectedRecd, string[] conn)
        {
            DataSet ds = new DataSet();
            ds.Reset();
            string str = "update tcustomerhead set Status='Student' where id in (" + selectedRecd + ")";
            ds = fillds(str, conn);

            return "true";
        }

        public void AllocateCenterToSelStudent(string selectedStuds, long AllocatedCenter, string[] conn)
        {
            DataSet ds = new DataSet();
            ds.Reset();
            string str = "update tcustomerhead set BranchID=" + AllocatedCenter + " where id in (" + selectedStuds + ")";
            ds = fillds(str, conn);
        }
        #endregion

        public int SaveApplicationFeeDetails(tInvoiceHead invc, string State, tAddToCartProductDetail PrdDetail, string[] conn)
        {
            long invcID;
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            if (State == "AddNew")
            {
                ce.tInvoiceHeads.AddObject(invc);
                ce.SaveChanges();
            }
            else if (State == "Edit")
            {
                //ce.tInvoiceHeads.Attach(invc);
                //ce.ObjectStateManager.ChangeObjectState(invc, EntityState.Modified);                
                //ce.SaveChanges();

                tInvoiceHead invH = null;

                using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
                {
                    ce.ContextOptions.LazyLoadingEnabled = false;
                    invH = (from i in db.tInvoiceHeads
                            where i.ID == invc.ID
                            select i).FirstOrDefault();
                }
                if (invH == null)
                {
                    BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                    try
                    {
                        db.tInvoiceHeads.AddObject(invc);
                        db.SaveChanges();
                    }
                    catch { invcID = Convert.ToInt32(invc.ID); }
                }
                else
                {
                    invH.InvoiceNo = invc.InvoiceNo;
                    invH.ObjectName = invc.ObjectName;
                    invH.InvoiceDate = invc.InvoiceDate;
                    invH.InvoiceType = invc.InvoiceType;
                    invH.CustomerHeadID = invc.CustomerHeadID;
                    invH.LeadSourceID = invc.LeadSourceID;
                    invH.InvoiceStatus = invc.InvoiceStatus;
                    invH.SubTotal = invc.SubTotal;
                    invH.ProductLevelTotalDiscount = invc.ProductLevelTotalDiscount;
                    invH.DiscountOnSubTotal = invc.DiscountOnSubTotal;
                    invH.DiscountOnSubTotalPercent = invc.DiscountOnSubTotalPercent;
                    invH.TotalTax = invc.TotalTax;
                    invH.ShippingCharges = invc.ShippingCharges;
                    invH.OtherCharges = invc.OtherCharges;
                    invH.TotalAmount = invc.TotalAmount;
                    invH.Active = invc.Active;
                    invH.LastModifiedBy = invc.CreatedBy;
                    invH.LastModifiedDate = DateTime.Now;
                    invH.TotalDiscount = invc.TotalDiscount;
                    invH.TotalAfterDiscount = invc.TotalAfterDiscount;
                    invH.ProductLevelTotalTax = invc.ProductLevelTotalTax;
                    invH.CompanyID = invc.CompanyID;
                    invH.BillingAddressID = invc.BillingAddressID;
                    invH.ShippingAddressID = invc.ShippingAddressID;
                    invH.ConperID = invc.ConperID;
                    invH.Title = invc.Title;
                    //  invH.Outstanding = invc.Outstanding;


                    using (BISPL_CRMDBEntities ce1 = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
                    {
                        ce1.tInvoiceHeads.Attach(invH);
                        ce1.ObjectStateManager.ChangeObjectState(invH, System.Data.EntityState.Modified);
                        ce1.SaveChanges();

                        //result = Convert.ToInt32(reg.Id);
                    }
                }

            }

            invcID = invc.ID;

            PrdDetail.ReferenceID = invcID;
            if (State == "AddNew")
            {
                ce.tAddToCartProductDetails.AddObject(PrdDetail);
                ce.SaveChanges();
            }
            else if (State == "Edit")
            {
                //ce.tAddToCartProductDetails.Attach(PrdDetail);
                //ce.ObjectStateManager.ChangeObjectState(PrdDetail, EntityState.Modified);
                //ce.SaveChanges();

                tAddToCartProductDetail prd = null;

                using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
                {
                    ce.ContextOptions.LazyLoadingEnabled = false;
                    prd = (from p in db.tAddToCartProductDetails
                           where p.ReferenceID == PrdDetail.ReferenceID
                           select p).FirstOrDefault();
                }
                if (prd == null)
                {
                    BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                    try
                    {
                        db.tAddToCartProductDetails.AddObject(PrdDetail);
                        db.SaveChanges();
                    }
                    catch { invcID = Convert.ToInt32(invc.ID); }
                }
                else
                {
                    prd.ObjectName = PrdDetail.ObjectName;
                    prd.ReferenceID = PrdDetail.ReferenceID;
                    prd.Sequence = PrdDetail.Sequence;
                    prd.ProductID = PrdDetail.ProductID;
                    prd.ProductCode = PrdDetail.ProductCode;
                    prd.ProductName = PrdDetail.ProductName;
                    prd.ProductDescription = PrdDetail.ProductDescription;
                    prd.ProductPrice = PrdDetail.ProductPrice;
                    prd.Quantity = PrdDetail.Quantity;
                    prd.PerUnitDiscount = PrdDetail.PerUnitDiscount;
                    prd.IsDiscountPercent = PrdDetail.IsDiscountPercent;
                    prd.DiscountID = PrdDetail.DiscountID;
                    prd.TotalTaxAmount = PrdDetail.TotalTaxAmount;

                    using (BISPL_CRMDBEntities ce1 = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
                    {
                        ce1.tAddToCartProductDetails.Attach(prd);
                        ce1.ObjectStateManager.ChangeObjectState(prd, System.Data.EntityState.Modified);
                        ce1.SaveChanges();

                        //result = Convert.ToInt32(reg.Id);
                    }
                }
            }
            return Convert.ToInt32(invc.ID);
        }

        public long GetNewInvoiceNo(string[] conn)
        {
            long InvcNo;
            DataSet ds = new DataSet();
            ds.Reset();
            string str = "select max(InvoiceNo)+1 NewInvNo from tInvoiceHead";
            ds = fillds(str, conn);
            InvcNo = Convert.ToInt64(ds.Tables[0].Rows[0]["NewInvNo"].ToString());

            return InvcNo;
        }

        public DataSet GetPrdDetail(long PrdID, string[] conn)
        {
            DataSet ds = new DataSet();
            ds.Reset();
            string str = "select *  from mproduct where id=" + PrdID + "";
            ds = fillds(str, conn);
            return ds;
        }

        public long GetInvoiceIdOfCustomer(long CustID, string[] conn)
        {
            long InvID = 0;
            DataSet ds = new DataSet();
            ds.Reset();
            string str = "select ID from tInvoiceHead where CustomerHeadID=" + CustID + " and Title='Application Fee Invoice'";
            ds = fillds(str, conn);
            InvID = Convert.ToInt64(ds.Tables[0].Rows[0]["ID"].ToString());
            return InvID;
        }

        //public DataSet CheckCoupen(string CoupnNo, long UserID,string[] conn)
        public DataSet CheckCoupen(long UserID, string[] conn)
        {

            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "sp_CheckCoupen";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            //cmd.Parameters.AddWithValue("CoupnNo", CoupnNo);
            cmd.Parameters.AddWithValue("UserID", UserID);
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;
        }

        public DataSet UpdateCoupen(string CoupenNo, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "sp_updateCoupon";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("CoupnNo", CoupenNo);
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;
        }

        public void DeletetImportStudentData(string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Sp_DeletetImportStudentData";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.ExecuteNonQuery();
        }


        public DataSet GetimportStudentdata(string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Sp_GetStudentImportData";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;
        }

        public DataSet GettempImportStudentData(string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Sp_Getimportstudentnotavailable";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;
        }


        public void InsertStudInRegistration(string Title, string Fname, string Mname, string Lname, string Gender, string Usertype, string Active, DateTime Datecreated, string PartnerType, long PartnerID, long CourseID, string Medium, string[] conn)
        {

            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "sp_InsertStudInRegistration";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("Title", Title);
            cmd.Parameters.AddWithValue("Fname", Fname);
            cmd.Parameters.AddWithValue("Mname", Mname);
            cmd.Parameters.AddWithValue("Lname", Lname);
            cmd.Parameters.AddWithValue("Gender", Gender);
            cmd.Parameters.AddWithValue("Usertype", Usertype);
            cmd.Parameters.AddWithValue("Active", Active);
            cmd.Parameters.AddWithValue("DateCreated", Datecreated);
            cmd.Parameters.AddWithValue("PartnerType", PartnerType);
            cmd.Parameters.AddWithValue("PartnerID", PartnerID);
            cmd.Parameters.AddWithValue("CourseID", CourseID);
            cmd.Parameters.AddWithValue("Medium", Medium);
            cmd.ExecuteNonQuery();
        }

        public void InsertStudInCustomerHead(long SectorID, string Name, string DisplayName, string Active, long CreatedBy, DateTime CreationDate, long CompanyID, string ConperID, long BillingAddressID, long ShippingAddressID, long BranchID, string Fname, string Mname, string Lname, string Gender, DateTime Date_of_Registration, string EmailID, string MobileNo, long CourseID, string Medium, string payment, string[] conn)
        {

            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "sp_InsertStudInCustomerHead";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("SectorID", SectorID);
            cmd.Parameters.AddWithValue("Name", Name);
            cmd.Parameters.AddWithValue("DisplayName", DisplayName);
            cmd.Parameters.AddWithValue("Active", Active);
            cmd.Parameters.AddWithValue("CreatedBy", CreatedBy);
            cmd.Parameters.AddWithValue("CreationDate", CreationDate);
            cmd.Parameters.AddWithValue("CompanyID", CompanyID);
            cmd.Parameters.AddWithValue("ConperID", ConperID);
            cmd.Parameters.AddWithValue("BillingAddressID", BillingAddressID);
            cmd.Parameters.AddWithValue("ShippingAddressID", ShippingAddressID);
            cmd.Parameters.AddWithValue("BranchID", BranchID);
            cmd.Parameters.AddWithValue("FirstName", Fname);
            cmd.Parameters.AddWithValue("Middle_Name", Mname);
            cmd.Parameters.AddWithValue("Last_Name", Lname);
            cmd.Parameters.AddWithValue("Gender", Gender);
            cmd.Parameters.AddWithValue("Date_of_Registration", Date_of_Registration);
            cmd.Parameters.AddWithValue("Email_ID", EmailID);
            cmd.Parameters.AddWithValue("Mobile_Number", MobileNo);
            cmd.Parameters.AddWithValue("CourseID", CourseID);
            cmd.Parameters.AddWithValue("Medium", Medium);
            cmd.Parameters.AddWithValue("payment", payment);
            cmd.ExecuteNonQuery();
        }

        public DataSet GetRegID(string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "sp_GetRegID";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;
        }

        public void InsrtIntoMediumTable(long RegID, string Medium, DateTime StartDate, DateTime EndDate, string[] conn)
        {

            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "sp_InsrtIntoMediumTable";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("RegId", RegID);
            cmd.Parameters.AddWithValue("Medium", Medium);
            cmd.Parameters.AddWithValue("StartDate", StartDate);
            cmd.Parameters.AddWithValue("EndDate", EndDate);
            cmd.ExecuteNonQuery();
        }

        public void UpdateMediumTable(long RegID, string Medium, string[] conn)
        {
            DataSet ds = new DataSet();
            ds.Reset();
            string str = "update tbl_medium set Medium='" + Medium + "' where regid=" + RegID + "";
            ds = fillds(str, conn);
        }

        public void InsertStudDataInLoginTable(long RegID, string UserName, string Password, string Active, DateTime Datecreated, int AccessPeriod, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "sp_InsertStudDataInLoginTable";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("RegID", RegID);
            cmd.Parameters.AddWithValue("UserName", UserName);
            cmd.Parameters.AddWithValue("Password", Password);
            cmd.Parameters.AddWithValue("Active", Active);
            cmd.Parameters.AddWithValue("Datecreated", Datecreated);
            cmd.Parameters.AddWithValue("AccessPeriod", AccessPeriod);
            cmd.ExecuteNonQuery();
        }
        public void InsertStudDataInContactTable(long RegID, string EmailID, string MobileNo, string Active, DateTime Datecreated, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "sp_InsertStudDataInContactTable";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("RegID", RegID);
            cmd.Parameters.AddWithValue("email", EmailID);
            cmd.Parameters.AddWithValue("Mobile", MobileNo);
            cmd.Parameters.AddWithValue("Active", Active);
            cmd.Parameters.AddWithValue("Datecreated", Datecreated);
            cmd.ExecuteNonQuery();
        }

        public void InsertStudDataInPaymentTable(long RegID, long CourseID, string TransactionAmt, DateTime TransactionDate, string PayMode, DateTime Datecreated, long Product_Id, DateTime StartDate, DateTime EndDate, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "sp_InsertStudDataInPayemntTable";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("RegID", RegID);
            cmd.Parameters.AddWithValue("CourseID", CourseID);
            cmd.Parameters.AddWithValue("TransactionAmt", TransactionAmt);
            cmd.Parameters.AddWithValue("TransactionDate", TransactionDate);
            cmd.Parameters.AddWithValue("PayMode", PayMode);
            cmd.Parameters.AddWithValue("DateCreated", Datecreated);
            cmd.Parameters.AddWithValue("Product_Id", Product_Id);
            cmd.Parameters.AddWithValue("StartDate", StartDate);
            cmd.Parameters.AddWithValue("EndDate", EndDate);
            cmd.ExecuteNonQuery();
        }


        public DataSet UpdateMediumInRegister(long RegID, string Medium, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "sp_UpdateMediumInRegister";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("RegId", RegID);
            cmd.Parameters.AddWithValue("Medium", Medium);
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;
        }


        public DataSet GetCustID(string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "sp_GetCustID";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;
        }

        public DataSet UpdateStudIDinRegister(long CustID, long RegID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "sp_UpdateStudIDInRegister";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("RegId", RegID);
            cmd.Parameters.AddWithValue("StudID", CustID);
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;
        }

        public DataSet GetRegisterIDByAccountID(long AccountID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "sp_GetRegisterID";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("AccountID", AccountID);
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;

        }

        public DataSet GetloginDetailsByRegisterId(long RegisterID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "sp_GetloginDetailsByRegisterId";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("RegisterID", RegisterID);
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;

        }

        public DataSet GetMediumDetailsByRegisterId(long RegisterID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "sp_GetMediumDetailsByRegisterId";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("RegisterID", RegisterID);
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;

        }

        public void InsertMediumInMeduimTable(long RegID, string medium, DateTime StartDate, DateTime EndDate, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "sp_InsertMediumInMeduimTable";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("RegId", RegID);
            cmd.Parameters.AddWithValue("Medium", medium);
            cmd.Parameters.AddWithValue("StartDate", StartDate);
            cmd.Parameters.AddWithValue("EndDate", EndDate);
            cmd.ExecuteNonQuery();

        }

        public int GetTotalNoofLicences(string SelStud, string[] conn)
        {
            int TotalLicence = 0;
            DataSet ds = new DataSet();
            ds.Reset();
            string str = "select count(ID) Licence from tbl_Medium where REgID in(select ID from tbl_registration where StudID in(" + SelStud + "))";
            ds = fillds(str, conn);
            TotalLicence = Convert.ToInt16(ds.Tables[0].Rows[0]["Licence"].ToString());
            return TotalLicence;
        }

        public int InsertPaymentBooking(decimal Amount, decimal TotalLicense, string StudID, string ddlPaymentMode, DateTime hdnDT, string txtBankName, string txtTransactionNo, string txtChequeNo, string txtRemark, long CustomerID, string[] conn)
        {
            try
            {
                // Get Last Invoice Number
                int InvNo;
                DataSet ds1 = new DataSet();
                ds1.Reset();
                string str1 = "select InvoiceNo from tInvoiceHEad where ID=(select top 1 ID from tInvoiceHEad order by ID desc)";
                ds1 = fillds(str1, conn);
                InvNo = Convert.ToInt16(ds1.Tables[0].Rows[0]["InvoiceNo"].ToString());
                InvNo = InvNo + 1;

                //Insert Into tInvoiceHead
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_InsertPaymentBookingIntCustomerHead";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("InvoiceNo", InvNo.ToString());
                cmd.Parameters.AddWithValue("InvoiceDate", hdnDT);
                cmd.Parameters.AddWithValue("CustomerHeadID", CustomerID);
                cmd.Parameters.AddWithValue("SubTotal", Amount);
                cmd.Parameters.AddWithValue("CreatedBy", CustomerID.ToString());
                cmd.Parameters.AddWithValue("StudID", StudID);
                cmd.ExecuteNonQuery();

                //Inserted Invoice ID
                long InsertedInvID;
                DataSet ds2 = new DataSet();
                ds2.Reset();
                string str2 = "select top 1 ID from tInvoiceHEad order by ID desc";
                ds2 = fillds(str2, conn);
                InsertedInvID = Convert.ToInt64(ds2.Tables[0].Rows[0]["ID"].ToString());

                //Insert Into tAddToCartProductDetail
                SqlCommand cmd1 = new SqlCommand();
                cmd1.CommandType = CommandType.StoredProcedure;
                cmd1.CommandText = "SP_InsertPaymentBookingIntAddToCartProductDetail";
                cmd1.Connection = svr.GetSqlConn(conn);
                cmd1.Parameters.Clear();
                cmd1.Parameters.AddWithValue("ReferenceID", InsertedInvID);
                cmd1.Parameters.AddWithValue("Quantity", TotalLicense);
                cmd1.ExecuteNonQuery();

                //Insert into tInvoicePaymentBooking
                SqlCommand cmd3 = new SqlCommand();
                cmd3.CommandType = CommandType.StoredProcedure;
                cmd3.CommandText = "SP_InsertPaymentBookingIntInvoicePaymentBooking";
                cmd3.Connection = svr.GetSqlConn(conn);
                cmd3.Parameters.Clear();
                cmd3.Parameters.AddWithValue("InvoiceID", InsertedInvID);
                cmd3.Parameters.AddWithValue("PaymentREceiveDate", hdnDT.Date);
                cmd3.Parameters.AddWithValue("PaymentAmount", Amount);
                cmd3.Parameters.AddWithValue("PaymentMode", ddlPaymentMode);
                cmd3.Parameters.AddWithValue("Remark", txtRemark);
                cmd3.Parameters.AddWithValue("CreatedBy", CustomerID.ToString());
                cmd3.Parameters.AddWithValue("ReceiptNo", txtChequeNo);
                cmd3.Parameters.AddWithValue("BankName", txtBankName);
                cmd3.Parameters.AddWithValue("TransactionNo", txtTransactionNo);
                cmd3.ExecuteNonQuery();

                //Update tCustomerHead
                DataSet ds3 = new DataSet();
                ds3.Reset();
                string str3 = "update tCustomerHead set Payment='Paid' where ID in (" + StudID + ")";
                ds3 = fillds(str3, conn);


            }
            catch { }
            finally { }

            return 1;
        }

        public string GetOTP(string[] conn)
        {
            DataSet dsOTP = new DataSet();
            string str = "select ABS(CHECKSUM(NewId())) % 123456 OTP";
            dsOTP = fillds(str, conn);
            string OTP = dsOTP.Tables[0].Rows[0]["OTP"].ToString();
            return OTP;
        }

        public void AddIntomSLA(mSLA ObjSLA, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            if (ObjSLA.DeptID == 0)
            {
                ce.mSLAs.AddObject(ObjSLA);
                ce.SaveChanges();
            }
            else
            {
                DataSet ds = new DataSet();
                ds = fillds("select * from mSLA where DeptID=" + ObjSLA.DeptID + "", conn);
                if (ds.Tables[0].Rows.Count > 0)
                {//Modify
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_Update_mSLA";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("PrimeDays", ObjSLA.PrimeDays);
                    cmd.Parameters.AddWithValue("ExpressDays", ObjSLA.ExpressDays);
                    cmd.Parameters.AddWithValue("RegularDays", ObjSLA.RegularDays);
                    cmd.Parameters.AddWithValue("ModifiedBy", ObjSLA.CreatedBy);
                    cmd.Parameters.AddWithValue("ModifiedDate", ObjSLA.CreatedDate);
                    cmd.Parameters.AddWithValue("DeptID", ObjSLA.DeptID);
                    cmd.ExecuteNonQuery();
                }
                else
                {//Add
                    ce.mSLAs.AddObject(ObjSLA);
                    ce.SaveChanges();
                }

                long DEPTID =long.Parse(ObjSLA.DeptID.ToString());
                DataSet dsMail = new DataSet();
                dsMail = fillds("select * from mMessageEMailTemplates where DepartmentID=" + DEPTID + " and ActivityID in(61,62)", conn);
                if (dsMail.Tables[0].Rows.Count == 0)
                {
                    BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));

                    DataSet dsCmpny = new DataSet();
                    dsCmpny = fillds("select ParentID from mTerritory where ID=" + DEPTID + "", conn);
                    long CompanyID = long.Parse(dsCmpny.Tables[0].Rows[0]["ParentID"].ToString());

                    mMessageEMailTemplate mEmailOD = new mMessageEMailTemplate();
                    mEmailOD.MailSubject = "Order Out For Delivery ";
                    mEmailOD.MailBody = "Below Order has been Out For Delivery,";
                    mEmailOD.Active = "Yes";
                    mEmailOD.CreatedBy = long.Parse(ObjSLA.CreatedBy.ToString());
                    mEmailOD.CreationDate = DateTime.Now;
                    mEmailOD.CompanyID = CompanyID;
                    mEmailOD.DepartmentID = DEPTID;
                    mEmailOD.ActivityID = 61;
                    mEmailOD.MessageID = 12;
                    mEmailOD.TemplateTitle = "Order Out For Delivery";

                    db.mMessageEMailTemplates.AddObject(mEmailOD);
                    db.SaveChanges();

                    mMessageEMailTemplate mEmailRT = new mMessageEMailTemplate();
                    mEmailRT.MailSubject = "Order Returned ";
                    mEmailRT.MailBody = "Below Order is Returned,";
                    mEmailRT.Active = "Yes";
                    mEmailRT.CreatedBy = long.Parse(ObjSLA.CreatedBy.ToString());
                    mEmailRT.CreationDate = DateTime.Now;
                    mEmailRT.CompanyID = CompanyID;
                    mEmailRT.DepartmentID = DEPTID;
                    mEmailRT.ActivityID = 62;
                    mEmailRT.MessageID = 12;
                    mEmailRT.TemplateTitle = "Order Returned";

                    db.mMessageEMailTemplates.AddObject(mEmailRT);
                    db.SaveChanges();
                }
            }
        }

        public mSLA GetSLADetailsDeptWise(long DptID,string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            mSLA SL = new mSLA();
            SL = (from s in ce.mSLAs
                  where s.DeptID == DptID
                  select s).FirstOrDefault();
            return SL;
        }

        public void DeleteFanancialApprover(long DptID, string[] conn)
        {
            //BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            DataSet dss = new DataSet();
            dss.Reset();
            string sttr = "Update mTerritory set FinApproverId=0,PriceChange=0 where ID=" + DptID + "";
            dss = fillds(sttr, conn);
                      
        }

    }
}


