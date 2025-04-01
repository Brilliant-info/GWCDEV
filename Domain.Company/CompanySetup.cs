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
using System.Xml.Linq;
using Domain.Server;
using System.Data.Objects;
using System.Globalization;
using System.Reflection;
using System.Xml.Linq;
using System.Data;
using System.Collections;

namespace Domain.Company
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]

    public partial class CompanySetup : Interface.Company.iCompanySetup
    {
        Domain.Server.Server svr = new Server.Server();

        #region GetGroupCompany
        /// <summary>
        /// GetGroupCompany is providing List of Group Company
        /// </summary>
        /// <returns></returns>
        /// 
        public List<mCompany> GetGroupCompany(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));

            List<mCompany> GroupCompany = new List<mCompany>();
            GroupCompany = (from p in ce.mCompanies
                            select p).ToList();
            if (GroupCompany.Count == 0)
            {
                GroupCompany = null;
            }
            return GroupCompany;
        }
        #endregion

        #region InsertmCompany

        /// <summary>
        ///  InsertmCompany is the Method To Insert Record In mCompany Table
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public long InsertmCompany(mCompany Company, string[] conn)
        {
            try
            {
                BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));

                if (Company.ID == null || Company.ID == 0) { ce.mCompanies.AddObject(Company); }
                else
                {
                    SqlConnection sqlconn = new SqlConnection("Data Source=" + conn[0] + ";Initial Catalog=" + conn[1] + "; User ID=" + conn[3] + "; Password=" + conn[2] + ";");
                    SqlCommand cmd = new SqlCommand("dbcc checkident (mCompany, reseed, " + (Company.ID - 1) + ")", sqlconn);
                    sqlconn.Open();
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.ExecuteNonQuery();
                    sqlconn.Close();
                    ce.mCompanies.AddObject(Company);

                }

                ce.SaveChanges();
                return Company.ID;
            }
            catch (Exception ex) { return 0; }
        }
        #endregion

        #region UpdateCompany
        /// <summary>
        /// Updatestatus Is Method To Update mCompany Table
        /// </summary>
        /// <param name="Updatestatus"></param>
        /// <returns></returns>
        /// 
        public int UpdateCompany(mCompany UpdateCompany, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));

            ce.mCompanies.Attach(UpdateCompany);
            ce.ObjectStateManager.ChangeObjectState(UpdateCompany, EntityState.Modified);
            ce.SaveChanges();
            return 1;
        }
        #endregion

        #region Dupliacte

        public string checkDuplicateRecord(string CompanyName, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));

            string result = "";
            var output = (from p in ce.mCompanies
                          where p.Name == CompanyName
                          select new { p.Name }).FirstOrDefault();

            if (output != null)
            {
                result = "Same Company Name Already Exist";
            }
            return result;

        }
        #endregion

        #region chechDuplicateEdit
        public string checkDuplicateRecordEdit(string CompanyName, int CompanyID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));

            string result = "";
            var output = (from p in ce.mCompanies
                          where p.Name == CompanyName && p.ID != CompanyID
                          select new { p.Name }).FirstOrDefault();

            if (output != null)
            {
                result = "Same Company Name Already Exist";
            }
            return result;
        }
        #endregion

        #region GetCompanyById
        /// <summary>
        /// GetGroupCompany is providing List of Group Company
        /// </summary>
        /// <returns></returns>
        /// 
        public mCompany GetCompanyById(Int64 CompanyID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));

            mCompany Company = new mCompany();
            Company = (from p in ce.mCompanies
                       where p.ID == CompanyID
                       select p).FirstOrDefault();

            return Company;
        }
        #endregion

        #region InsertmCompanyRegistration

        /// <summary>
        ///  InsertmCompany is the Method To Insert Record In mCompany Table
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public int InsertmCompanyRegistration(tCompanyRegistrationDetail Company, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));

            ce.tCompanyRegistrationDetails.AddObject(Company);
            ce.SaveChanges();
            return 1;
        }
        #endregion

        #region UpdateCompanyRegistration
        /// <summary>
        /// Updatestatus Is Method To Update mCompany Table
        /// </summary>
        /// <param name="Updatestatus"></param>
        /// <returns></returns>
        /// 
        public int UpdateCompanyRegistration(tCompanyRegistrationDetail UpdateCompany, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));

            ce.tCompanyRegistrationDetails.Attach(UpdateCompany);
            ce.ObjectStateManager.ChangeObjectState(UpdateCompany, EntityState.Modified);
            ce.SaveChanges();
            return 1;
        }
        #endregion

        public List<vGetCompanyDetail> GetCompanyList(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<vGetCompanyDetail> mcomp = new List<vGetCompanyDetail>();
            mcomp = (from cm in ce.vGetCompanyDetails
                     orderby cm.ParentID, cm.ID
                     select cm).ToList();
            return mcomp;
        }

        #region GetCompanyById
        /// <summary>
        /// GetGroupCompany is providing List of Group Company
        /// </summary>
        /// <returns></returns>
        /// 
        public tCompanyRegistrationDetail GetCompanyRegisById(Int64 CompanyID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));

            tCompanyRegistrationDetail Company = new tCompanyRegistrationDetail();
            Company = (from p in ce.tCompanyRegistrationDetails
                       where p.CompanyID == CompanyID
                       select p).FirstOrDefault();

            return Company;
        }
        #endregion

        #region ChechkCompanyServerNameDuplicate
        public string ChechkCompanyServerNameDuplicate(string ServerName, string[] conn)
        {
            string Result = "";
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mCompany> Company = new List<mCompany>();
            Company = (from p in ce.mCompanies
                       where p.DataSource == ServerName
                       select p).ToList();
            if (Company.Count == 0)
            {
                Result = "";
            }
            else
            {
                Result = "Server Name already exist";
            }
            return Result;
        }

        #endregion

        #region ChechkCompanyDatabaseNameDuplicate
        public string ChechkCompanyDatabaseNameDuplicate(string DataBaseName, string[] conn)
        {
            string Result = "";
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mCompany> Company = new List<mCompany>();
            Company = (from p in ce.mCompanies
                       where p.DataBaseName == DataBaseName
                       select p).ToList();
            if (Company.Count == 0)
            {
                Result = "";
            }
            else
            {
                Result = "Same DataBase Name Already Exist";
            }
            return Result;
        }

        #endregion

        #region ChechkCompanyNameDuplicate
        public string ChechkCompanyNameDuplicate(string CompanyName, string[] conn)
        {
            string Result = "";
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mCompany> Company = new List<mCompany>();
            Company = (from p in ce.mCompanies
                       where p.DataBaseName == CompanyName
                       select p).ToList();
            if (Company.Count == 0)
            {
                Result = "";
            }
            else
            {
                Result = "Same Company Name Already Exist";
            }
            return Result;
        }



        #region ChechkCompanyServerNameDuplicate
        public string ChechkCompanyServer_DataBaseNameDuplicate(string ServerName, string Database, string[] conn)
        {
            string Result = "";
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mCompany> Company = new List<mCompany>();
            Company = (from p in ce.mCompanies
                       where p.DataSource == ServerName && p.DataBaseName == Database
                       select p).ToList();
            if (Company.Count == 0)
            {
                Result = "";
            }
            else
            {
                Result = "Same Server  Name Already Exist";
            }
            return Result;
        }

        #endregion

        #endregion

        #region RestoreDatabase
        /// <summary>
        /// GetGroupCompany is providing List of Group Company
        /// </summary>
        /// <returns></returns>
        /// 
        public void RestoreDatabase(string DataBaseName, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ObjectParameter _paraDataBaseName = new ObjectParameter("DataBaseName", typeof(string));
            _paraDataBaseName.Value = DataBaseName;

            //ObjectParameter _paraObjectName = new ObjectParameter("paraObjectName", typeof(string));
            //_paraObjectName.Value = "VendorProduct";

            //ObjectParameter _paraReferenceID = new ObjectParameter("paraReferenceID", typeof(long));
            //_paraReferenceID.Value = paraReferenceID;

            //ObjectParameter _paraUserID = new ObjectParameter("paraUserID", typeof(string));
            //_paraUserID.Value = paraUserID;

            ObjectParameter[] obj = new ObjectParameter[] { _paraDataBaseName };

            ce.ExecuteFunction("SP_RestoreDataBase", obj);
            ce.SaveChanges();

        }
        #endregion

        # region New Customer or company code for GWC project
        public DataSet GetCompanyName(long ID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Sp_GetCompanyNameID";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("ID", ID);
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;
        }


        public DataSet GetDepartmentListforgrid(long ParentID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_GetDepartmentforGrid";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("ParentID", ParentID);
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;
        }

        public void Savecustdeptinfo(long ParentID, string Territory, long Sequence, string StoreCode, long ApprovalLevel, string AutoCancel, long cancelDays, string CreatedBy, DateTime CreationDate, string Active, string ApprovalRem, long ApproRemSchedul, string AutoCancRen, long AutoRemSchedule, bool GwcDeliveries, bool ECommerce, string OrderFormat, long MaxDeliveryDays, long AddressType, bool PriceChange, string location, int MerchantCode, decimal TokenExptime, long UserID, decimal PreOrderExpTime, string CustAnalytics, bool emoney, bool AutoApproval, int ExpiryDays, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_InsertCustDeptInfo";           
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("ParentID", ParentID);
            cmd.Parameters.AddWithValue("Territory", Territory);
            cmd.Parameters.AddWithValue("Sequence", Sequence);
            cmd.Parameters.AddWithValue("StoreCode", StoreCode);
            cmd.Parameters.AddWithValue("ApprovalLevel", ApprovalLevel);
            cmd.Parameters.AddWithValue("AutoCancel", AutoCancel);
            cmd.Parameters.AddWithValue("cancelDays", cancelDays);
            cmd.Parameters.AddWithValue("CreatedBy", CreatedBy);
            cmd.Parameters.AddWithValue("CreationDate", CreationDate);
            cmd.Parameters.AddWithValue("Active", Active);
            cmd.Parameters.AddWithValue("ApprovalRem", ApprovalRem);
            cmd.Parameters.AddWithValue("ApproRemSchedul", ApproRemSchedul);
            cmd.Parameters.AddWithValue("AutoCancRen", AutoCancRen);
            cmd.Parameters.AddWithValue("AutoRemSchedule", AutoRemSchedule);
            cmd.Parameters.AddWithValue("GwcDeliveries", GwcDeliveries);
            cmd.Parameters.AddWithValue("ECommerce", ECommerce);
            cmd.Parameters.AddWithValue("OrderFormat", OrderFormat);
            cmd.Parameters.AddWithValue("MaxDeliveryDays", MaxDeliveryDays);
            cmd.Parameters.AddWithValue("AddressType", AddressType);
            cmd.Parameters.AddWithValue("PriceChange", PriceChange);
            cmd.Parameters.AddWithValue("location", location);
            cmd.Parameters.AddWithValue("MerchantCode", MerchantCode);
            cmd.Parameters.AddWithValue("TokenExptime", TokenExptime);
            cmd.Parameters.AddWithValue("UserID", UserID);
            cmd.Parameters.AddWithValue("PreOrderExpTime", PreOrderExpTime);
            cmd.Parameters.AddWithValue("CustAnalytics", CustAnalytics);
            cmd.Parameters.AddWithValue("eMoney", emoney);
            cmd.Parameters.AddWithValue("AutoApproval", AutoApproval);
            cmd.Parameters.AddWithValue("ExpiryDays", ExpiryDays);
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.ExecuteNonQuery();
            InsertEmailMessages(CreatedBy, conn);
        }

        public void InsertEmailMessages(string CreatedBy, string[] conn)
        {
            try
            {
                DataSet ds = new DataSet();
                ds = fillds("select top (1) ID , ParentID from mTerritory order by id desc", conn);
                long DeptID = long.Parse(ds.Tables[0].Rows[0]["ID"].ToString());
                long CompanyID = long.Parse(ds.Tables[0].Rows[0]["ParentID"].ToString());

                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));

                // mMessageEMailTemplate mEmail = new mMessageEMailTemplate();

                mMessageEMailTemplate mEmail1 = new mMessageEMailTemplate();
                mEmail1.MailSubject = "OMS Order Submitted Successfully waiting for Approval ";
                mEmail1.MailBody = "Below order has been created successfully and is waiting for Approval,";
                mEmail1.Active = "Yes";
                mEmail1.CreatedBy = long.Parse(CreatedBy);
                mEmail1.CreationDate = DateTime.Now;
                mEmail1.CompanyID = CompanyID;
                mEmail1.DepartmentID = DeptID;
                mEmail1.ActivityID = 6;
                mEmail1.MessageID = 12;
                mEmail1.TemplateTitle = "Order Submit Information";

                db.mMessageEMailTemplates.AddObject(mEmail1);
                db.SaveChanges();

                mMessageEMailTemplate mEmail2 = new mMessageEMailTemplate();
                mEmail2.MailSubject = "OMS Order Submitted Successfully waiting for Approval ";
                mEmail2.MailBody = "Below order has been created successfully and is waiting for Approval,";
                mEmail2.Active = "Yes";
                mEmail2.CreatedBy = long.Parse(CreatedBy);
                mEmail2.CreationDate = DateTime.Now;
                mEmail2.CompanyID = CompanyID;
                mEmail2.DepartmentID = DeptID;
                mEmail2.ActivityID = 6;
                mEmail2.MessageID = 13;
                mEmail2.TemplateTitle = "Order Submitted Action";

                db.mMessageEMailTemplates.AddObject(mEmail2);
                db.SaveChanges();

                mMessageEMailTemplate mEmail3 = new mMessageEMailTemplate();
                mEmail3.MailSubject = "Order Approve Reminder";
                mEmail3.MailBody = "Please Approve Order Before Cancellation Days";
                mEmail3.Active = "Yes";
                mEmail3.CreatedBy = long.Parse(CreatedBy);
                mEmail3.CreationDate = DateTime.Now;
                mEmail3.CompanyID = CompanyID;
                mEmail3.DepartmentID = DeptID;
                mEmail3.ActivityID = 6;
                mEmail3.MessageID = 14;
                mEmail3.TemplateTitle = "Order Approve Reminder";

                db.mMessageEMailTemplates.AddObject(mEmail3);
                db.SaveChanges();

                mMessageEMailTemplate mEmail4 = new mMessageEMailTemplate();
                mEmail4.MailSubject = "OMS Order Approved Successfully";
                mEmail4.MailBody = "Below Order has been approved by approver for warehouse processing,";
                mEmail4.Active = "Yes";
                mEmail4.CreatedBy = long.Parse(CreatedBy);
                mEmail4.CreationDate = DateTime.Now;
                mEmail4.CompanyID = CompanyID;
                mEmail4.DepartmentID = DeptID;
                mEmail4.ActivityID = 7;
                mEmail4.MessageID = 13;
                mEmail4.TemplateTitle = "OMS Order Approved Successfully";

                db.mMessageEMailTemplates.AddObject(mEmail4);
                db.SaveChanges();

                mMessageEMailTemplate mEmail5 = new mMessageEMailTemplate(); //Need To Change
                mEmail5.MailSubject = "OMS Order Cancellation By Warehouse ";
                mEmail5.MailBody = "Below Order has been Cancelled by the warehouse,";
                mEmail5.Active = "Yes";
                mEmail5.CreatedBy = long.Parse(CreatedBy);
                mEmail5.CreationDate = DateTime.Now;
                mEmail5.CompanyID = CompanyID;
                mEmail5.DepartmentID = DeptID;
                mEmail5.ActivityID = 11;// 7; 
                mEmail5.MessageID = 12; // 14; 
                mEmail5.TemplateTitle = "OMS Order Cancellation By Warehouse";

                db.mMessageEMailTemplates.AddObject(mEmail5);
                db.SaveChanges();

                mMessageEMailTemplate mEmail6 = new mMessageEMailTemplate();
                mEmail6.MailSubject = "OMS Order Rejected by Approver";
                mEmail6.MailBody = "Below Order has been Rejected By the Approver,";
                mEmail6.Active = "Yes";
                mEmail6.CreatedBy = long.Parse(CreatedBy);
                mEmail6.CreationDate = DateTime.Now;
                mEmail6.CompanyID = CompanyID;
                mEmail6.DepartmentID = DeptID;
                mEmail6.ActivityID = 8;
                mEmail6.MessageID = 12;
                mEmail6.TemplateTitle = "OMS Order Rejected by Approver";

                db.mMessageEMailTemplates.AddObject(mEmail6);
                db.SaveChanges();

                mMessageEMailTemplate mEmail7 = new mMessageEMailTemplate();
                mEmail7.MailSubject = "OMS Order Ready for Pickup";
                mEmail7.MailBody = "Below Order is picked and ready for pickup,";
                mEmail7.Active = "Yes";
                mEmail7.CreatedBy = long.Parse(CreatedBy);
                mEmail7.CreationDate = DateTime.Now;
                mEmail7.CompanyID = CompanyID;
                mEmail7.DepartmentID = DeptID;
                mEmail7.ActivityID = 9;
                mEmail7.MessageID = 12;
                mEmail7.TemplateTitle = "OMS Order Ready for Pickup ";

                db.mMessageEMailTemplates.AddObject(mEmail7);
                db.SaveChanges();

                mMessageEMailTemplate mEmail8 = new mMessageEMailTemplate();
                mEmail8.MailSubject = "OMS Order Delivered";
                mEmail8.MailBody = "Below Order has been delivered";
                mEmail8.Active = "Yes";
                mEmail8.CreatedBy = long.Parse(CreatedBy);
                mEmail8.CreationDate = DateTime.Now;
                mEmail8.CompanyID = CompanyID;
                mEmail8.DepartmentID = DeptID;
                mEmail8.ActivityID = 10;
                mEmail8.MessageID = 12;
                mEmail8.TemplateTitle = "OMS Order Delivered ";

                db.mMessageEMailTemplates.AddObject(mEmail8);
                db.SaveChanges();

                mMessageEMailTemplate mEmail9 = new mMessageEMailTemplate();
                mEmail9.MailSubject = "OMS Order Auto Cancelled ";
                mEmail9.MailBody = "Below Order is Cancelled due to Auto Cancellation Policy,";
                mEmail9.Active = "Yes";
                mEmail9.CreatedBy = long.Parse(CreatedBy);
                mEmail9.CreationDate = DateTime.Now;
                mEmail9.CompanyID = CompanyID;
                mEmail9.DepartmentID = DeptID;
                mEmail9.ActivityID = 11;
                mEmail9.MessageID = 14;
                mEmail9.TemplateTitle = "OMS Order Auto Cancelled ";

                db.mMessageEMailTemplates.AddObject(mEmail9);
                db.SaveChanges();

                /*Order Out For Delivery */
                mMessageEMailTemplate mEmailOD = new mMessageEMailTemplate();
                mEmailOD.MailSubject = "Order Out For Delivery ";
                mEmailOD.MailBody = "Below Order has been Out For Delivery,";
                mEmailOD.Active = "Yes";
                mEmailOD.CreatedBy = long.Parse(CreatedBy);
                mEmailOD.CreationDate = DateTime.Now;
                mEmailOD.CompanyID = CompanyID;
                mEmailOD.DepartmentID = DeptID;
                mEmailOD.ActivityID = 61;
                mEmailOD.MessageID = 12;
                mEmailOD.TemplateTitle = "Order Out For Delivery";

                db.mMessageEMailTemplates.AddObject(mEmailOD);
                db.SaveChanges();

                /*Order Return */
                mMessageEMailTemplate mEmailRT = new mMessageEMailTemplate();
                mEmailRT.MailSubject = "Order Returned ";
                mEmailRT.MailBody = "Below Order is Returned,";
                mEmailRT.Active = "Yes";
                mEmailRT.CreatedBy = long.Parse(CreatedBy);
                mEmailRT.CreationDate = DateTime.Now;
                mEmailRT.CompanyID = CompanyID;
                mEmailRT.DepartmentID = DeptID;
                mEmailRT.ActivityID = 62;
                mEmailRT.MessageID = 12;
                mEmailRT.TemplateTitle = "Order Returned";

                db.mMessageEMailTemplates.AddObject(mEmailRT);
                db.SaveChanges();

                /*order cancel by requester*/
                mMessageEMailTemplate mEmailCR = new mMessageEMailTemplate();
                mEmailCR.MailSubject = "Cancel by Requester";
                mEmailCR.MailBody = "Order Cancel by Requester,";
                mEmailCR.Active = "Yes";
                mEmailCR.CreatedBy = long.Parse(CreatedBy);
                mEmailCR.CreationDate = DateTime.Now;
                mEmailCR.CompanyID = CompanyID;
                mEmailCR.DepartmentID = DeptID;
                mEmailCR.ActivityID = 66;
                mEmailCR.MessageID = 12;
                mEmailCR.TemplateTitle = "Cancel by Requester";

                db.mMessageEMailTemplates.AddObject(mEmailCR);
                db.SaveChanges();

                /*Approved With Revision*/

                mMessageEMailTemplate mEmailAR = new mMessageEMailTemplate();
                mEmailAR.MailSubject = "Approved With Revision";
                mEmailAR.MailBody = "Order Approved With Revision,";
                mEmailAR.Active = "Yes";
                mEmailAR.CreatedBy = long.Parse(CreatedBy);
                mEmailAR.CreationDate = DateTime.Now;
                mEmailAR.CompanyID = CompanyID;
                mEmailAR.DepartmentID = DeptID;
                mEmailAR.ActivityID = 67;
                mEmailAR.MessageID = 12;
                mEmailAR.TemplateTitle = "Approved With Revision";

                db.mMessageEMailTemplates.AddObject(mEmailAR);
                db.SaveChanges();

                /*Direct Order Approval  */
                mMessageEMailTemplate mEmailDo = new mMessageEMailTemplate();
                mEmailDo.MailSubject = "Direct Order Approved";
                mEmailDo.MailBody = "Below order has been submitted with auto approved and sent to warehouse for further processing,";
                mEmailDo.Active = "Yes";
                mEmailDo.CreatedBy = long.Parse(CreatedBy);
                mEmailDo.CreationDate = DateTime.Now;
                mEmailDo.CompanyID = CompanyID;
                mEmailDo.DepartmentID = DeptID;
                mEmailDo.ActivityID = 68;
                mEmailDo.MessageID = 13;
                mEmailDo.TemplateTitle = "Direct Order Approved";

                db.mMessageEMailTemplates.AddObject(mEmailDo);
                db.SaveChanges();

                /* Price Change Email For Approver*/
                mMessageEMailTemplate mEmailPC = new mMessageEMailTemplate();
                mEmailPC.MailSubject = "OMS Order Submitted Successfully waiting for Approval ";
                mEmailPC.MailBody = "This order is awaiting your approval due to a change of price. ";
                mEmailPC.Active = "Yes";
                mEmailPC.CreatedBy = long.Parse(CreatedBy);
                mEmailPC.CreationDate = DateTime.Now;
                mEmailPC.CompanyID = CompanyID;
                mEmailPC.DepartmentID = DeptID;
                mEmailPC.ActivityID = 76;
                mEmailPC.MessageID = 13;
                mEmailPC.TemplateTitle = "Order Submitted Action";

                db.mMessageEMailTemplates.AddObject(mEmailPC);
                db.SaveChanges();

                /*Order Submission with cost change Financial Approver */
                mMessageEMailTemplate mEmailfin = new mMessageEMailTemplate();
                mEmailfin.MailSubject = "OMS Order Submitted Successfully waiting for Financial Approval ";
                mEmailfin.MailBody = "This order is awaiting your approval due to a change of price. ";
                mEmailfin.Active = "Yes";
                mEmailfin.CreatedBy = long.Parse(CreatedBy);
                mEmailfin.CreationDate = DateTime.Now;
                mEmailfin.CompanyID = CompanyID;
                mEmailfin.DepartmentID = DeptID;
                mEmailfin.ActivityID = 78;
                mEmailfin.MessageID = 13;
                mEmailfin.TemplateTitle = "Order Submitted Action";

                db.mMessageEMailTemplates.AddObject(mEmailfin);
                db.SaveChanges();

                /*Order Submission with cost Center Approval */
                mMessageEMailTemplate mEmailcost = new mMessageEMailTemplate();
                mEmailcost.MailSubject = "OMS Order Submitted Successfully waiting for Cost Center Approval ";
                mEmailcost.MailBody = "This order is awaiting your approval due to FOC payment. ";
                mEmailcost.Active = "Yes";
                mEmailcost.CreatedBy = long.Parse(CreatedBy);
                mEmailcost.CreationDate = DateTime.Now;
                mEmailcost.CompanyID = CompanyID;
                mEmailcost.DepartmentID = DeptID;
                mEmailcost.ActivityID = 79;
                mEmailcost.MessageID = 13;
                mEmailcost.TemplateTitle = "Order Submitted Action";

                db.mMessageEMailTemplates.AddObject(mEmailcost);
                db.SaveChanges();

                /*Save SLA Details OF Department*/
                SaveSLA(DeptID, conn);
                InsertPMethod(DeptID, conn);
            }
            catch { }
            finally { }
        }

        public void InsertPMethod(long deptid, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "Insert into mDeptPaymentMethod(DeptID,PMethodID,Sequence)values(" + deptid + ",1,1)";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();

            cmd.ExecuteNonQuery();
        }

        public void SaveSLA(long DeptID, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select ID from mSLA where DeptID=0", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                long SLAID = long.Parse(ds.Tables[0].Rows[0]["ID"].ToString());

                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "update mSLA set DeptID=" + DeptID + " where ID=" + SLAID + "";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();

                cmd.ExecuteNonQuery();
            }
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
        public DataSet GetDepartmentToEdit(long ID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_GetDepartmentToEdit";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("ID", ID);
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;
        }


        public void UpdateDeptInfo(long ID, string Territory, string StoreCode, long ApprovalLevel, string AutoCancel, long cancelDays, string CreatedBy, DateTime CreationDate, string Active, string ApprovalRem, long ApproRemSchedul, string AutoCancRen, long AutoRemSchedule, bool GwcDeliveries, bool ECommerce, string OrderFormat, long MaxDeliveryDays, long FinApproverID, long AddressType, bool PriceChange, string location, int MerchantCode, decimal TokenExptime, long UserID, decimal PreOrderExpTime, string CustAnalytics,bool emoney,bool AutoApproval, int ExpiryDays, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Sp_UpdateDeptInfoEdit";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("ID", ID);
            cmd.Parameters.AddWithValue("Territory", Territory);
            cmd.Parameters.AddWithValue("StoreCode", StoreCode);
            cmd.Parameters.AddWithValue("ApprovalLevel", ApprovalLevel);
            cmd.Parameters.AddWithValue("AutoCancel", AutoCancel);
            cmd.Parameters.AddWithValue("cancelDays", cancelDays);
            //cmd.Parameters.AddWithValue("CreatedBy", CreatedBy);
            //cmd.Parameters.AddWithValue("CreationDate", CreationDate);
            cmd.Parameters.AddWithValue("Active", Active);
            cmd.Parameters.AddWithValue("ApprovalRem", ApprovalRem);
            cmd.Parameters.AddWithValue("ApproRemSchedul", ApproRemSchedul);
            cmd.Parameters.AddWithValue("AutoCancRen", AutoCancRen);
            cmd.Parameters.AddWithValue("AutoRemSchedule", AutoRemSchedule);
            cmd.Parameters.AddWithValue("GwcDeliveries", GwcDeliveries);
            cmd.Parameters.AddWithValue("ECommerce", ECommerce);
            cmd.Parameters.AddWithValue("OrderFormat", OrderFormat);
            cmd.Parameters.AddWithValue("MaxDeliveryDays", MaxDeliveryDays);
            cmd.Parameters.AddWithValue("FinApproverID", FinApproverID);
            cmd.Parameters.AddWithValue("AddressType", AddressType);
            cmd.Parameters.AddWithValue("PriceChange", PriceChange);
            cmd.Parameters.AddWithValue("location", location);
            cmd.Parameters.AddWithValue("MerchantCode", MerchantCode);
            cmd.Parameters.AddWithValue("TokenExptime", TokenExptime);
            cmd.Parameters.AddWithValue("UserID", UserID);
            cmd.Parameters.AddWithValue("PreOrderExpTime", PreOrderExpTime);
            cmd.Parameters.AddWithValue("CustAnalytics", CustAnalytics);
            cmd.Parameters.AddWithValue("LastModifiedBy", CreatedBy);
            cmd.Parameters.AddWithValue("LastModifiedDate", CreationDate);
            cmd.Parameters.AddWithValue("eMoney", emoney);
            cmd.Parameters.AddWithValue("AutoApproval", AutoApproval);
            cmd.Parameters.AddWithValue("ExpiryDays", ExpiryDays);
            cmd.ExecuteNonQuery();
            InsertEcommMails(ID, ECommerce, CreatedBy, conn);
        }


        public void InsertEcommMails(long ID, bool ECommerce, string CreatedBy, string[] conn)
        {
            if (ECommerce == true)
            {
                string Ecomm = "Ecomm";
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_UpdateEmailActiveEcomm";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("DepartmentID", ID);
                cmd.Parameters.AddWithValue("Ecomm", Ecomm);
                cmd.ExecuteNonQuery();
                long MailCount = GetEcommMailCount(ID, conn);
                if (MailCount == 0)
                {
                    insertEcommMail(CreatedBy, ID, conn);
                }
            }
            else
            {
                string Ecomm = "";
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_UpdateEmailActiveEcomm";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("DepartmentID", ID);
                cmd.Parameters.AddWithValue("Ecomm", Ecomm);
                cmd.ExecuteNonQuery();
            }
        }

        public long GetEcommMailCount(long DeptID, string[] conn)
        {
            long result = 0;
            SqlDataReader dr;
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_GetEcommMailCount";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("DepartmentID", DeptID);
            dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                result = long.Parse(dr["countmail"].ToString()); ;
            }
            dr.Close();
            return result;
        }

        public void insertEcommMail(string CreatedBy, long DeptID, string[] conn)
        {
            try
            {
                DataSet ds = new DataSet();
                ds = fillds("select ParentID from mTerritory where ID =" + DeptID + "", conn);
                long CompanyID = long.Parse(ds.Tables[0].Rows[0]["ParentID"].ToString());

                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));

                // mMessageEMailTemplate mEmail = new mMessageEMailTemplate();

                mMessageEMailTemplate mEmail1 = new mMessageEMailTemplate();
                mEmail1.MailType = "Device and SIM";
                mEmail1.MailSubject = "Online Order Submitted Successfully waiting for Approval 1";
                mEmail1.MailBody = "Below order has been created successfully and is waiting for Approval,";
                mEmail1.Active = "Yes";
                mEmail1.CreatedBy = long.Parse(CreatedBy);
                mEmail1.CreationDate = DateTime.Now;
                mEmail1.CompanyID = CompanyID;
                mEmail1.DepartmentID = DeptID;
                mEmail1.ActivityID = 6;
                mEmail1.MessageID = 12;
                mEmail1.TemplateTitle = "Order Submit Information";
                mEmail1.EcommMail = "Yes";

                db.mMessageEMailTemplates.AddObject(mEmail1);
                db.SaveChanges();

                mMessageEMailTemplate mEmail2 = new mMessageEMailTemplate();
                mEmail2.MailType = "Device and SIM";
                mEmail2.MailSubject = "Online Order Submitted Successfully waiting for Approval 2";
                mEmail2.MailBody = "Below order has been successfully approved by Fulfilment team and is waiting for your Approval.";
                mEmail2.Active = "Yes";
                mEmail2.CreatedBy = long.Parse(CreatedBy);
                mEmail2.CreationDate = DateTime.Now;
                mEmail2.CompanyID = CompanyID;
                mEmail2.DepartmentID = DeptID;
                mEmail2.ActivityID = 6;
                mEmail2.MessageID = 13;
                mEmail2.TemplateTitle = "Order Submitted Action";
                mEmail2.EcommMail = "Yes";

                db.mMessageEMailTemplates.AddObject(mEmail2);
                db.SaveChanges();

                // Not ncluded in Harryes mail need to discuss
                mMessageEMailTemplate mEmail3 = new mMessageEMailTemplate();
                mEmail3.MailType = "Device and SIM";
                mEmail3.MailSubject = "Order Approve Reminder";
                mEmail3.MailBody = "Please Approve Order Before Cancellation Days";
                mEmail3.Active = "Yes";
                mEmail3.CreatedBy = long.Parse(CreatedBy);
                mEmail3.CreationDate = DateTime.Now;
                mEmail3.CompanyID = CompanyID;
                mEmail3.DepartmentID = DeptID;
                mEmail3.ActivityID = 6;
                mEmail3.MessageID = 14;
                mEmail3.TemplateTitle = "Order Approve Reminder";
                mEmail3.EcommMail = "Yes";

                db.mMessageEMailTemplates.AddObject(mEmail3);
                db.SaveChanges();

                mMessageEMailTemplate mEmail4 = new mMessageEMailTemplate();
                mEmail4.MailType = "Device and SIM";
                mEmail4.MailSubject = "Online Order Approved Successfully";
                mEmail4.MailBody = "Below order has been Approved, please proceed further,";
                mEmail4.Active = "Yes";
                mEmail4.CreatedBy = long.Parse(CreatedBy);
                mEmail4.CreationDate = DateTime.Now;
                mEmail4.CompanyID = CompanyID;
                mEmail4.DepartmentID = DeptID;
                mEmail4.ActivityID = 7;
                mEmail4.MessageID = 13;
                mEmail4.TemplateTitle = "OMS Order Approved Successfully";
                mEmail4.EcommMail = "Yes";

                db.mMessageEMailTemplates.AddObject(mEmail4);
                db.SaveChanges();

                // Not ncluded in Harryes mail need to discuss
                mMessageEMailTemplate mEmail5 = new mMessageEMailTemplate(); //Need To Change
                mEmail5.MailSubject = "Online Order Cancellation By Warehouse ";
                mEmail5.MailBody = "Below Order has been Cancelled by the warehouse,";
                mEmail5.Active = "Yes";
                mEmail5.CreatedBy = long.Parse(CreatedBy);
                mEmail5.CreationDate = DateTime.Now;
                mEmail5.CompanyID = CompanyID;
                mEmail5.DepartmentID = DeptID;
                mEmail5.ActivityID = 11;// 7; 
                mEmail5.MessageID = 12; // 14; 
                mEmail5.TemplateTitle = "OMS Order Cancellation By Warehouse";
                mEmail5.EcommMail = "Yes";

                db.mMessageEMailTemplates.AddObject(mEmail5);
                db.SaveChanges();

                // Not ncluded in Harryes mail need to discuss
                mMessageEMailTemplate mEmail6 = new mMessageEMailTemplate();
                mEmail6.MailSubject = "Online Order Rejected by Approver";
                mEmail6.MailBody = "Below Order has been Rejected By the Approver,";
                mEmail6.Active = "Yes";
                mEmail6.CreatedBy = long.Parse(CreatedBy);
                mEmail6.CreationDate = DateTime.Now;
                mEmail6.CompanyID = CompanyID;
                mEmail6.DepartmentID = DeptID;
                mEmail6.ActivityID = 8;
                mEmail6.MessageID = 12;
                mEmail6.TemplateTitle = "Online Order Rejected by Approver";
                mEmail6.EcommMail = "Yes";
                db.mMessageEMailTemplates.AddObject(mEmail6);
                db.SaveChanges();

                //suraj change
                mMessageEMailTemplate mEmail7 = new mMessageEMailTemplate();
                mEmail7.MailType = "Device and SIM";
                mEmail7.MailSubject = "Online Order Ready for Activation";
                mEmail7.MailBody = "Below order is ready for activation and is waiting for your action.";
                mEmail7.Active = "Yes";
                mEmail7.CreatedBy = long.Parse(CreatedBy);
                mEmail7.CreationDate = DateTime.Now;
                mEmail7.CompanyID = CompanyID;
                mEmail7.DepartmentID = DeptID;
                mEmail7.ActivityID = 83;
                mEmail7.MessageID = 12;
                mEmail7.TemplateTitle = "Online Order Ready for Activation";
                mEmail7.EcommMail = "Yes";

                db.mMessageEMailTemplates.AddObject(mEmail7);
                db.SaveChanges();


                mMessageEMailTemplate mEmail8 = new mMessageEMailTemplate();
                mEmail8.MailType = "Device and SIM";
                mEmail8.MailSubject = "Online Order Activated";
                mEmail8.MailBody = "Below order is ready for dispatch and is waiting for your action.";
                mEmail8.Active = "Yes";
                mEmail8.CreatedBy = long.Parse(CreatedBy);
                mEmail8.CreationDate = DateTime.Now;
                mEmail8.CompanyID = CompanyID;
                mEmail8.DepartmentID = DeptID;
                mEmail8.ActivityID = 84;
                mEmail8.MessageID = 12;
                mEmail8.TemplateTitle = "Online Order Activated";
                mEmail8.EcommMail = "Yes";

                db.mMessageEMailTemplates.AddObject(mEmail8);
                db.SaveChanges();

                //suraj change
                mMessageEMailTemplate mEmail9 = new mMessageEMailTemplate();
                mEmail9.MailType = "Device and SIM";
                mEmail9.MailSubject = "Online Order Delivered - Documents Uploaded";
                mEmail9.MailBody = "We have successfully uploaded the documents for below order, please proceed further";
                mEmail9.Active = "Yes";
                mEmail9.CreatedBy = long.Parse(CreatedBy);
                mEmail9.CreationDate = DateTime.Now;
                mEmail9.CompanyID = CompanyID;
                mEmail9.DepartmentID = DeptID;
                mEmail9.ActivityID = 85;
                mEmail9.MessageID = 12;
                mEmail9.TemplateTitle = "Online Order Delivered - Documents Uploaded";
                mEmail9.EcommMail = "Yes";

                db.mMessageEMailTemplates.AddObject(mEmail9);
                db.SaveChanges();

                mMessageEMailTemplate mEmail10 = new mMessageEMailTemplate();
                mEmail10.MailSubject = "Online Order Auto Cancelled ";
                mEmail10.MailBody = "Below Order is Cancelled due to Auto Cancellation Policy,";
                mEmail10.Active = "Yes";
                mEmail10.CreatedBy = long.Parse(CreatedBy);
                mEmail10.CreationDate = DateTime.Now;
                mEmail10.CompanyID = CompanyID;
                mEmail10.DepartmentID = DeptID;
                mEmail10.ActivityID = 11;
                mEmail10.MessageID = 14;
                mEmail10.TemplateTitle = "OMS Order Auto Cancelled ";
                mEmail10.EcommMail = "Yes";

                db.mMessageEMailTemplates.AddObject(mEmail10);
                db.SaveChanges();

                /*Order Out For Delivery */
                mMessageEMailTemplate mEmailOD = new mMessageEMailTemplate();
                mEmailOD.MailSubject = "Order Out For Delivery ";
                mEmailOD.MailBody = "Below Order has been Out For Delivery,";
                mEmailOD.Active = "Yes";
                mEmailOD.CreatedBy = long.Parse(CreatedBy);
                mEmailOD.CreationDate = DateTime.Now;
                mEmailOD.CompanyID = CompanyID;
                mEmailOD.DepartmentID = DeptID;
                mEmailOD.ActivityID = 61;
                mEmailOD.MessageID = 12;
                mEmailOD.TemplateTitle = "Order Out For Delivery";
                mEmailOD.EcommMail = "Yes";

                db.mMessageEMailTemplates.AddObject(mEmailOD);
                db.SaveChanges();

                /*Order Return */
                mMessageEMailTemplate mEmailRT = new mMessageEMailTemplate();
                mEmailRT.MailSubject = "Order Returned ";
                mEmailRT.MailBody = "Below Order is Returned,";
                mEmailRT.Active = "Yes";
                mEmailRT.CreatedBy = long.Parse(CreatedBy);
                mEmailRT.CreationDate = DateTime.Now;
                mEmailRT.CompanyID = CompanyID;
                mEmailRT.DepartmentID = DeptID;
                mEmailRT.ActivityID = 62;
                mEmailRT.MessageID = 12;
                mEmailRT.TemplateTitle = "Order Returned";
                mEmailRT.EcommMail = "Yes";

                db.mMessageEMailTemplates.AddObject(mEmailRT);
                db.SaveChanges();


                // Customer Mails Reject Arabic
                mMessageEMailTemplate mEmailCust = new mMessageEMailTemplate();
                mEmailCust.MailType = "Customer";
                mEmailCust.MailSubject = "تم الغاء الأمر او الطلب";
                mEmailCust.MailBody = "تم إلغاء طلبك";
                mEmailCust.Active = "Yes";
                mEmailCust.CreatedBy = long.Parse(CreatedBy);
                mEmailCust.CreationDate = DateTime.Now;
                mEmailCust.CompanyID = CompanyID;
                mEmailCust.DepartmentID = DeptID;
                mEmailCust.ActivityID = 86;
                mEmailCust.MessageID = 12;
                mEmailCust.TemplateTitle = "تم الغاء الأمر او الطلب";
                mEmailCust.Language = "Arabic";
                mEmailCust.EcommMail = "Yes";

                db.mMessageEMailTemplates.AddObject(mEmailCust);
                db.SaveChanges();

                // Customer Mails Reject English
                mMessageEMailTemplate mEmailCustE = new mMessageEMailTemplate();
                mEmailCustE.MailType = "Customer";
                mEmailCustE.MailSubject = "Order Cancelled";
                mEmailCustE.MailBody = "Your order has been cancelled";
                mEmailCustE.Active = "Yes";
                mEmailCustE.CreatedBy = long.Parse(CreatedBy);
                mEmailCustE.CreationDate = DateTime.Now;
                mEmailCustE.CompanyID = CompanyID;
                mEmailCustE.DepartmentID = DeptID;
                mEmailCustE.ActivityID = 86;
                mEmailCustE.MessageID = 12;
                mEmailCustE.TemplateTitle = "Order Cancelled";
                mEmailCustE.Language = "English";
                mEmailCustE.EcommMail = "Yes";

                db.mMessageEMailTemplates.AddObject(mEmailCustE);
                db.SaveChanges();

                // Customer Mails Order Dispatch cash On Delivery in Arebic
                mMessageEMailTemplate mEmailDispCOD = new mMessageEMailTemplate();
                mEmailDispCOD.MailType = "Cash On Delivery";
                mEmailDispCOD.MailSubject = "النظام خارج للتسليم";
                mEmailDispCOD.MailBody = "كان طلبك خارج للتسليم،";
                mEmailDispCOD.Active = "Yes";
                mEmailDispCOD.CreatedBy = long.Parse(CreatedBy);
                mEmailDispCOD.CreationDate = DateTime.Now;
                mEmailDispCOD.CompanyID = CompanyID;
                mEmailDispCOD.DepartmentID = DeptID;
                mEmailDispCOD.ActivityID = 61;
                mEmailDispCOD.MessageID = 12;
                mEmailDispCOD.TemplateTitle = "النظام خارج للتسليم";
                mEmailDispCOD.Language = "Arabic";
                mEmailDispCOD.EcommMail = "Yes";

                db.mMessageEMailTemplates.AddObject(mEmailDispCOD);
                db.SaveChanges();

                // Customer Mails Order Dispatch cash On Delivery in English
                mMessageEMailTemplate mEmailDispCODE = new mMessageEMailTemplate();
                mEmailDispCODE.MailType = "Cash On Delivery";
                mEmailDispCODE.MailSubject = "Order Out For Delivery";
                mEmailDispCODE.MailBody = "Your Order has been Out For Delivery,";
                mEmailDispCODE.Active = "Yes";
                mEmailDispCODE.CreatedBy = long.Parse(CreatedBy);
                mEmailDispCODE.CreationDate = DateTime.Now;
                mEmailDispCODE.CompanyID = CompanyID;
                mEmailDispCODE.DepartmentID = DeptID;
                mEmailDispCODE.ActivityID = 61;
                mEmailDispCODE.MessageID = 12;
                mEmailDispCODE.TemplateTitle = "Order Out For Delivery";
                mEmailDispCODE.Language = "English";
                mEmailDispCODE.EcommMail = "Yes";

                db.mMessageEMailTemplates.AddObject(mEmailDispCODE);
                db.SaveChanges();

                // Customer Mails Order Dispatch Credit Card Payment in Arebic
                mMessageEMailTemplate mEmailDispCC = new mMessageEMailTemplate();
                mEmailDispCC.MailType = "Credit Card";
                mEmailDispCC.MailSubject = "النظام خارج للتسليم";
                mEmailDispCC.MailBody = "كان طلبك خارج للتسليم،";
                mEmailDispCC.Active = "Yes";
                mEmailDispCC.CreatedBy = long.Parse(CreatedBy);
                mEmailDispCC.CreationDate = DateTime.Now;
                mEmailDispCC.CompanyID = CompanyID;
                mEmailDispCC.DepartmentID = DeptID;
                mEmailDispCC.ActivityID = 61;
                mEmailDispCC.MessageID = 12;
                mEmailDispCC.TemplateTitle = "النظام خارج للتسليم";
                mEmailDispCC.Language = "Arabic";
                mEmailDispCC.EcommMail = "Yes";

                db.mMessageEMailTemplates.AddObject(mEmailDispCC);
                db.SaveChanges();

                // Customer Mails Order Dispatch Credit Card Payment in English
                mMessageEMailTemplate mEmailDispCCE = new mMessageEMailTemplate();
                mEmailDispCCE.MailType = "Credit Card";
                mEmailDispCCE.MailSubject = "Order Out For Delivery";
                mEmailDispCCE.MailBody = "Your Order has been Out For Delivery,";
                mEmailDispCCE.Active = "Yes";
                mEmailDispCCE.CreatedBy = long.Parse(CreatedBy);
                mEmailDispCCE.CreationDate = DateTime.Now;
                mEmailDispCCE.CompanyID = CompanyID;
                mEmailDispCCE.DepartmentID = DeptID;
                mEmailDispCCE.ActivityID = 61;
                mEmailDispCCE.MessageID = 12;
                mEmailDispCCE.TemplateTitle = "Order Out For Delivery";
                mEmailDispCCE.Language = "English";
                mEmailDispCCE.EcommMail = "Yes";

                db.mMessageEMailTemplates.AddObject(mEmailDispCCE);
                db.SaveChanges();


                /*Save SLA Details OF Department*/
                SaveSLA(DeptID, conn);
                InsertPMethod(DeptID, conn);
            }
            catch { }
            finally { }
        }

        public long chkDeptDuplicate(string Territory, string StoreCode, string[] conn)
        {
            long result = 0;
            SqlDataReader dr;
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Sp_CheckDeptDuplicate";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("Territory", Territory);
            cmd.Parameters.AddWithValue("StoreCode", StoreCode);
            dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                result = long.Parse(dr[0].ToString());
            }
            dr.Close();
            return result;
        }

        public DataSet GetDeptListWithSLA(long ParentID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_GetDeptListForGrid";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("ParentID", ParentID);
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;
        }

        public DataSet GetLocationList(long ID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_GetLocationDetailList";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("ID", ID);
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;
        }

        public long SaveLocContactInContact(string ObjectName, long ReferenceID, long CustomerHeadID, long Sequence, string Name, string EmailID, string MobileNo, long ContactTypeID, string Active, string CreatedBy, DateTime CreationDate, long CompanyID, string[] conn)
        {
            long ContactID = 0;
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_SaveLocationContact";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("ObjectName", ObjectName);
            cmd.Parameters.AddWithValue("ReferenceID", ReferenceID);
            cmd.Parameters.AddWithValue("CustomerHeadID", CustomerHeadID);
            cmd.Parameters.AddWithValue("Sequence", Sequence);
            cmd.Parameters.AddWithValue("Name", Name);
            cmd.Parameters.AddWithValue("EmailID", EmailID);
            cmd.Parameters.AddWithValue("MobileNo", MobileNo);
            cmd.Parameters.AddWithValue("ContactTypeID", ContactTypeID);
            cmd.Parameters.AddWithValue("Active", Active);
            cmd.Parameters.AddWithValue("CreatedBy", CreatedBy);
            cmd.Parameters.AddWithValue("CreationDate", CreationDate);
            cmd.Parameters.AddWithValue("CompanyID", CompanyID);

            //cmd.ExecuteNonQuery();
            ContactID = long.Parse(cmd.ExecuteScalar().ToString());

            return ContactID;
        }

        public void SaveEditLocation(long ID, long CompanyID, long ReferenceID, string LocationCode, string AddressLine1, string AddressLine2, string County, string State, string City, string zipcode, string landmark, string FaxNo, string Active, string ContactName, string ContactEmail, string LocationName, long MobileNo, string CreatedBy, DateTime CreationDate, string hdnstate, long ShippingID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_SaveEditLocation";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("ID", ID);
            cmd.Parameters.AddWithValue("CompanyID", CompanyID);
            cmd.Parameters.AddWithValue("ReferenceID", ReferenceID);
            cmd.Parameters.AddWithValue("LocationCode", LocationCode);
            cmd.Parameters.AddWithValue("AddressLine1", AddressLine1);
            cmd.Parameters.AddWithValue("AddressLine2", AddressLine2);
            cmd.Parameters.AddWithValue("County", County);
            cmd.Parameters.AddWithValue("State", State);
            cmd.Parameters.AddWithValue("City", City);
            cmd.Parameters.AddWithValue("zipcode", zipcode);
            cmd.Parameters.AddWithValue("landmark", landmark);
            cmd.Parameters.AddWithValue("FaxNo", FaxNo);
            cmd.Parameters.AddWithValue("Active", Active);
            cmd.Parameters.AddWithValue("ContactName", ContactName);
            cmd.Parameters.AddWithValue("ContactEmail", ContactEmail);
            cmd.Parameters.AddWithValue("LocationName", LocationName);
            cmd.Parameters.AddWithValue("MobileNo", MobileNo);
            cmd.Parameters.AddWithValue("CreatedBy", CreatedBy);
            cmd.Parameters.AddWithValue("CreationDate", CreationDate);
            cmd.Parameters.AddWithValue("hdnstate", hdnstate);
            cmd.Parameters.AddWithValue("ShippingID", ShippingID);
            cmd.ExecuteNonQuery();
        }

        public void UpdateLocationDetails(long ID, long CompanyID, long ReferenceID, string LocationCode, string AddressLine1, string AddressLine2, string County, string State, string City, string zipcode, string landmark, string FaxNo, string Active, string ContactName, string ContactEmail, long MobileNo, string CreatedBy, DateTime CreationDate, string hdnstate, string LocationName, long ShippingID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_UpdateLocationDetails";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("ID", ID);
            cmd.Parameters.AddWithValue("CompanyID", CompanyID);
            cmd.Parameters.AddWithValue("ReferenceID", ReferenceID);
            cmd.Parameters.AddWithValue("LocationCode", LocationCode);
            cmd.Parameters.AddWithValue("AddressLine1", AddressLine1);
            cmd.Parameters.AddWithValue("AddressLine2", AddressLine2);
            cmd.Parameters.AddWithValue("County", County);
            cmd.Parameters.AddWithValue("State", State);
            cmd.Parameters.AddWithValue("City", City);
            cmd.Parameters.AddWithValue("zipcode", zipcode);
            cmd.Parameters.AddWithValue("landmark", landmark);
            cmd.Parameters.AddWithValue("FaxNo", FaxNo);
            cmd.Parameters.AddWithValue("Active", Active);
            cmd.Parameters.AddWithValue("ContactName", ContactName);
            cmd.Parameters.AddWithValue("ContactEmail", ContactEmail);
            cmd.Parameters.AddWithValue("MobileNo", MobileNo);
            cmd.Parameters.AddWithValue("CreatedBy", CreatedBy);
            cmd.Parameters.AddWithValue("CreationDate", CreationDate);
            cmd.Parameters.AddWithValue("hdnstate", hdnstate);
            cmd.Parameters.AddWithValue("LocationName", LocationName);
            cmd.Parameters.AddWithValue("ShippingID", ShippingID);
            cmd.ExecuteNonQuery();
        }

        public string getFinApprovername(long ID, string[] conn)
        {
            string result = "";
            SqlDataReader dr;
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_GetFiananceApprover";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("ID", ID);
            dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                result = dr[0].ToString();
            }
            dr.Close();
            return result;
        }

        public DataSet GetAddressType(string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_GetAddresstypeValues";
            cmd.Connection = svr.GetSqlConn(conn);
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;
        }

        public long chkecommerceduplicate(long ParentID, long ID, string[] conn)
        {
            long result = 0;
            SqlDataReader dr;
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_CheckEcommerce";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("ParentID", ParentID);
            cmd.Parameters.AddWithValue("ID", ID);
            dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                result = long.Parse(dr[0].ToString());
            }
            dr.Close();
            return result;
        }

        public void InsertIntoDeptPayment(long DeptID, long PMethodID, int Sequence, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_InsertDeptPMethod";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("DeptID", DeptID);
            cmd.Parameters.AddWithValue("PMethodID", PMethodID);
            cmd.Parameters.AddWithValue("Sequence", Sequence);
            cmd.ExecuteNonQuery();
        }

        public void DeleteRecordWithZeroQty(string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_DeleteRecordWithZeroQty";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.ExecuteNonQuery();
        }

        public void RemoveDeptPMethod(long ID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_RemoveDeptMethod";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("ID", ID);
            cmd.ExecuteNonQuery();
        }

        public void UpdateDeptPaymentMethod(long DeptID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_UpdateDeptPMethod";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("DeptID", DeptID);
            cmd.ExecuteNonQuery();
        }

        public DataSet GetCostCenterList(long CompanyID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_GetCompanyCostCenter";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("CompanyID", CompanyID);
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;
        }

        public void DeleteZeroCompanyIDCostCenter(string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_DeleteCostCenterZeroQty";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.ExecuteNonQuery();
        }

        public void SaveCostCenter(string CenterName, string Code, long ApproverID, long CompanyID, string Remark, DateTime CreationDate, long CreatedBy, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_InsertIntoCostCenter";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("CenterName", CenterName);
            cmd.Parameters.AddWithValue("Code", Code);
            cmd.Parameters.AddWithValue("ApproverID", ApproverID);
            cmd.Parameters.AddWithValue("CompanyID", CompanyID);
            cmd.Parameters.AddWithValue("Remark", Remark);
            cmd.Parameters.AddWithValue("CreationDate", CreationDate);
            cmd.Parameters.AddWithValue("CreatedBy", CreatedBy);
            cmd.ExecuteNonQuery();
        }

        public void RemoveCostCenter(long ID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_RemoveCostCenter";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("ID", ID);
            cmd.ExecuteNonQuery();
        }

        public void UpdateCostCenterCmpanyID(long CompanyID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_UpdateCostCenterCompanyID";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("CompanyID", CompanyID);
            cmd.ExecuteNonQuery();
        }

        public long Duplicatecostcenter(string CenterName, string Code, string[] conn)
        {
            long result = 0;
            SqlDataReader dr;
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_CheckCostCenterDuplicate";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("CenterName", CenterName);
            cmd.Parameters.AddWithValue("Code", Code);
            dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                result = long.Parse(dr[0].ToString());
            }
            dr.Close();
            return result;
        }

        public long CheckDuplicatePMethod(long PMethodID, long DeptID, string[] conn)
        {
            long result = 0;
            SqlDataReader dr;
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_CheckDuplicatePmethod";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("PMethodID", PMethodID);
            cmd.Parameters.AddWithValue("DeptID", DeptID);
            dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                result = long.Parse(dr[0].ToString());
            }
            dr.Close();
            return result;
        }


        public int CheckLocationIDForAssignedUser(long Location, string[] conn)
        {
            int id = 0; ;
            DataTable dt = new DataTable();
            DataSet ds = new System.Data.DataSet();
            SqlDataAdapter da = new SqlDataAdapter();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "select * from mUserLocation where LocationID=" + Location + "";
            da.SelectCommand = cmd;
            da.Fill(ds, "tbl");
            dt = ds.Tables[0];
            if (dt.Rows.Count > 0)
            {
                id = 1;
            }
            return id;
        }

        public long GetContactIdasShippingId(long ID, string[] conn)
        {
            long ContactTableid = 0;
            SqlDataReader dr;
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_GetLocationContactID";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("ID", ID);
            dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                ContactTableid = long.Parse(dr["ShippingID"].ToString());
            }
            dr.Close();
            return ContactTableid;
        }

        public void UpdateContacttableDetail(string Name, string EmailID, string MobileNo, long ID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_UpdateLocationContactTable";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("Name", Name);
            cmd.Parameters.AddWithValue("EmailID", EmailID);
            cmd.Parameters.AddWithValue("MobileNo", MobileNo);
            cmd.Parameters.AddWithValue("ID", ID);
            cmd.ExecuteNonQuery();
        }

        public List<tContactPersonDetail> GetContactPersonListDeptWise(long CompanyID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<tContactPersonDetail> ConPerLst = new List<tContactPersonDetail>();
            ConPerLst = (from dpt in ce.tContactPersonDetails
                         where dpt.CompanyID == CompanyID
                         select dpt).ToList();
            return ConPerLst;
        }

        public DataSet GetContactPersonLocList(long CompanyID, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from  tContactPersonDetail where CompanyID = " + CompanyID + " order by ID desc", conn);
            return ds;
        }

        public DataSet CheckLocationCode(string LocationCode, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from  tAddress where LocationCode = '" + LocationCode + "' order by ID desc", conn);
            return ds;
        }
        #endregion


        #region Department version3

        public void AddData(long CompanyId, string CompanyNm, long DeptId, string DeptNm, string SchemaNm, string DatabaseName, string ConnectionString, string Deptcode, string Active, string wmsstorecode, long CreatedBy, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_InsertDepartWithConnectionstring";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@CompanyId", CompanyId);
            cmd.Parameters.AddWithValue("@CompanyNm", CompanyNm);
            cmd.Parameters.AddWithValue("@DeptId", DeptId);
            cmd.Parameters.AddWithValue("@DeptNm", DeptNm);
            cmd.Parameters.AddWithValue("@SchemaNm", SchemaNm);
            cmd.Parameters.AddWithValue("@DatabaseName", DatabaseName);
            cmd.Parameters.AddWithValue("@ConnectionString", ConnectionString);
            cmd.Parameters.AddWithValue("@Flag", "Insert");
            cmd.Parameters.AddWithValue("@DeptCode", Deptcode);
            cmd.Parameters.AddWithValue("@Active", Active);
            cmd.Parameters.AddWithValue("@CreatedBy", CreatedBy);
            cmd.Parameters.AddWithValue("@Wmsstorecode", wmsstorecode);
            cmd.ExecuteNonQuery();
        }

        public void UpdateData(long Id, long CompanyId, string CompanyNm, long DeptId, string DeptNm, string SchemaNm, string DatabaseName, string ConnectionString, string Deptcode, string Active, string wmsstorecode, long ModifiedBy, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_InsertDepartWithConnectionstring";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@CompanyId", CompanyId);
            cmd.Parameters.AddWithValue("@CompanyNm", CompanyNm);
            cmd.Parameters.AddWithValue("@DeptId", DeptId);
            cmd.Parameters.AddWithValue("@DeptNm", DeptNm);
            cmd.Parameters.AddWithValue("@SchemaNm", SchemaNm);
            cmd.Parameters.AddWithValue("@DatabaseName", DatabaseName);
            cmd.Parameters.AddWithValue("@ConnectionString", ConnectionString);
            cmd.Parameters.AddWithValue("@Flag", "Update");
            cmd.Parameters.AddWithValue("@ID", Id);
            cmd.Parameters.AddWithValue("@DeptCode", Deptcode);
            cmd.Parameters.AddWithValue("@Active", Active);
            cmd.Parameters.AddWithValue("@CreatedBy", ModifiedBy);
            cmd.Parameters.AddWithValue("@Wmsstorecode", wmsstorecode);
            cmd.ExecuteNonQuery();
        }

        public DataSet BindGrid(string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from  mDeptWithConnectionString", conn);
            return ds;
        }


        public DataSet GetDataByID(long ID, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from  mDeptWithConnectionString where id=" + ID + "	", conn);
            return ds;
        }

        public DataSet BindCompany(string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from  mcompany order by name", conn);
            return ds;
        }

        public long FindMaxNumber(long CompanyId, long DeptId, string SchemaNm, string DatabaseName, string ConnectionString, string[] conn)
        {
            long IDNO = 0;
            DataSet ds = new DataSet();
            ds = fillds("select ID from  mDeptWithConnectionString where CompanyId=" + CompanyId + " and DeptId=" + DeptId + " and SchemaNm='" + SchemaNm + "' and DatabaseName='" + DatabaseName + "' and ConnectionString='" + ConnectionString + "' 	", conn);
            IDNO = Convert.ToInt64(ds.Tables[0].Rows[0]["ID"]);
            return IDNO;
        }


        public string GetDepartmentCode(long CompanyId, long DeptId, string[] conn)
        {
            string storeid = "";
            DataSet ds = new DataSet();
            ds = fillds("select storecode from mTerritory where id=" + DeptId + "", conn);
            storeid = Convert.ToString(ds.Tables[0].Rows[0]["storecode"]);
            return storeid;
        }

        public DataSet GetDBConfigureDataByID(long DeptId, string[] conn)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                DataSet ds = new DataSet();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetSchemaConfigureData";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("DeptId", DeptId);
                cmd.Connection = svr.GetSqlConn(conn);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }


        #endregion

        #region Reason code


        public DataSet GetReasonCodeData(string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("SELECT mCompany.Name, mTerritory.Territory, mReasonCode.* FROM mReasonCode INNER JOIN mCompany ON mReasonCode.companyId = mCompany.ID INNER JOIN mTerritory ON mReasonCode.DeptId = mTerritory.ID", conn);
            return ds;
        }
        public void InsertReasonCodeData(string comid, string deptid, string typeid, string rcode, string rcodedes, string defaultvalue, string active, string reasonarb, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_InsertIntoReasonCode";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@SequenceCode", 0);
            cmd.Parameters.AddWithValue("@ReasonCodee", rcode);
            cmd.Parameters.AddWithValue("@ReasonDetails", rcodedes);
            cmd.Parameters.AddWithValue("@Type", typeid);
            cmd.Parameters.AddWithValue("@Active", active);
            cmd.Parameters.AddWithValue("@DefaultValue", defaultvalue);
            cmd.Parameters.AddWithValue("@companyId", comid);
            cmd.Parameters.AddWithValue("@DeptId", deptid);
            cmd.Parameters.AddWithValue("@Flag", "insertdata");
            cmd.Parameters.AddWithValue("@ReasonCodeArb", reasonarb);
            cmd.ExecuteNonQuery();
        }


        public void UpdateReasonCodeData(long rid, string comid, string deptid, string typeid, string rcode, string rcodedes, string defaultvalue, string active, string reasonarb, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_InsertIntoReasonCode";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@SequenceCode", 0);
            cmd.Parameters.AddWithValue("@ReasonCodee", rcode);
            cmd.Parameters.AddWithValue("@ReasonDetails", rcodedes);
            cmd.Parameters.AddWithValue("@Type", typeid);
            cmd.Parameters.AddWithValue("@Active", active);
            cmd.Parameters.AddWithValue("@DefaultValue", defaultvalue);
            cmd.Parameters.AddWithValue("@companyId", comid);
            cmd.Parameters.AddWithValue("@DeptId", deptid);
            cmd.Parameters.AddWithValue("@Id", rid);
            cmd.Parameters.AddWithValue("@ReasonCodeArb", reasonarb);
            cmd.Parameters.AddWithValue("@Flag", "updatedata");
            cmd.ExecuteNonQuery();
        }


        public DataSet BindSearchDataIntoGrid(string deptid, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("SELECT mCompany.Name, mTerritory.Territory, mReasonCode.* FROM mReasonCode INNER JOIN mCompany ON mReasonCode.companyId = mCompany.ID INNER JOIN mTerritory ON  mReasonCode.DeptId = mTerritory.ID where  mReasonCode.DeptId = " + deptid + " ", conn);
            return ds;
        }


        public string CheckDefaultValueIsPresentOrNot(string comid, string deptid, string typeid, string[] conn)
        {
            string result = "";
            DataSet ds = new DataSet();
            ds = fillds("select * from mReasonCode where companyId=" + comid + " and DeptId=" + deptid + " and Active='Yes' and type='" + typeid + "' and DefaultValue='Yes'", conn);
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

        public string GetReasoncodeId(string comid, string deptid, string rcode, string typeid, string rcodedes, string active, string defaultvalue, string[] conn)
        {
            string result = "";

            DataSet ds = new DataSet();
            ds = fillds("select id from mReasonCode where companyId=" + comid + " and DeptId=" + deptid + " and Active='" + active + "' and type='" + typeid + "' and DefaultValue='" + defaultvalue + "' ", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                result = ds.Tables[0].Rows[0]["id"].ToString();
            }
            return result;
        }

        public void UpdateReasonCodeDataUsingId(long id, string defaultvalue, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_InsertIntoReasonCode";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();

            cmd.Parameters.AddWithValue("@DefaultValue", defaultvalue);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Flag", "updatedatabyid");
            cmd.ExecuteNonQuery();
        }

        public string ResoncodeCheckDuplicate(string comid, string deptid, string typeid, string rcode, string[] conn)
        {
            string result = "";
            DataSet ds = new DataSet();
            ds = fillds(" select * from mreasoncode where companyid=" + comid + " and deptid=" + deptid + " and reasoncode='" + rcode + "' and type in('Rejection','Cancellation') ", conn);
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


        //public List<mTerritory> GetDepartmentList(int ComapnyID, string[] conn)
        //{

        //    BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
        //    List<mTerritory> DeptList = new List<mTerritory>();
        //    DeptList = (from dl in ce.mTerritories
        //                where dl.ParentID == ComapnyID
        //                orderby dl.Territory
        //                select dl).ToList();
        //    return DeptList;
        //}


        public List<mTerritory> GetDepartmentList1(int ComapnyID, string[] conn)
        {

            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mTerritory> DeptList = new List<mTerritory>();
            DeptList = (from dl in ce.mTerritories
                        where dl.ParentID == ComapnyID
                        orderby dl.Territory
                        select dl).ToList();
            return DeptList;
        }


        public List<mCompany> GetUserCompanyNameNEW(long UID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mCompany> companylist = new List<mCompany>();
            companylist = (from cl in ce.mCompanies
                           orderby cl.Name
                           select cl).ToList();
            return companylist;
        }

        public void UpdateRecord(string comid, string deptid, string typeid, string[] conn)
        {
            //SqlCommand cmd = new SqlCommand();
            //cmd.CommandType = CommandType.StoredProcedure;
            //cmd.CommandText = "SP_InsertIntoReasonCode";
            //cmd.Connection = svr.GetSqlConn(conn);
            //cmd.Parameters.Clear();
            //cmd.Parameters.AddWithValue("@companyId", comid);
            //cmd.Parameters.AddWithValue("@DeptId", deptid);
            //cmd.Parameters.AddWithValue("@Type", typeid);        
            //cmd.Parameters.AddWithValue("@Flag", "updateoldallrecord");
            //cmd.ExecuteNonQuery();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "update mReasonCode set DefaultValue='No' where companyId=" + comid + "  and DeptId=" + deptid + "  and Type='" + typeid + "'";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.ExecuteNonQuery();
        }


        #endregion


        public DataSet BindCompanyUserWise(long companyid, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from  mcompany where id=" + companyid + "", conn);
            return ds;
        }

        public DataSet BindSFTPGrid(string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_BindSFTPGrid";
            cmd.Connection = svr.GetSqlConn(conn);
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;
        }

        public long checkmerchantcode(long deptid, string[] conn)
        {
            long result = 0;
            SqlDataReader dr;
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Sp_CheckMerchantCode";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("deptid", deptid);
            dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                result = long.Parse(dr[0].ToString());
            }
            dr.Close();
            return result;
        }



        #region Dedicated driver

        public DataSet GetEcommerceCompany(string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_Ecomcompany";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }


        public List<mTerritory> GetDepartmentList2(int ComapnyID, string[] conn)
        {

            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mTerritory> DeptList = new List<mTerritory>();
            DeptList = (from dl in ce.mTerritories
                        where dl.ParentID == ComapnyID && dl.ECommerce == true
                        orderby dl.Territory
                        select dl).ToList();

            return DeptList;
        }




        public DataSet GrtDriverList(long companyd, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GrtDedeicatedDriverList";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("companyd", companyd);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }

        public string CHKDuplicateDedicateddr(string State, string Driverid, string Customer, string Dept, string[] conn)
        {
            string result = "";
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_CHKDuplicateDedicateddr";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@State", State);
                cmd.Parameters.AddWithValue("@Driverid", Driverid);
                cmd.Parameters.AddWithValue("@Customer", Customer);
                cmd.Parameters.AddWithValue("@Dept", Dept);
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




        public void ADDDedicatedDR(long id, string State, string Driverid, string Customer, string Dept, long userid, string[] conn)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                DataSet ds = new DataSet();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_ADDDedicatedDR";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@State", State);
                cmd.Parameters.AddWithValue("@Driverid", Driverid);
                cmd.Parameters.AddWithValue("@Customer", Customer);
                cmd.Parameters.AddWithValue("@Dept", Dept);
                cmd.Parameters.AddWithValue("@userid", userid);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }




        public DataSet GetDRList(string filter, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetDRList";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("filter", filter);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }


        public DataSet GetDriverData(string ID, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetDriverData";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("ID", ID);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }



        public string ChkDrAssignActiveOrd(string drid, string[] conn)
        {
            string result = "";
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_ChkDrAssignActiveOrd";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@drid", drid);
                da.SelectCommand = cmd;
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    result = "3";
                }
                else
                {
                    result = "1";
                }
            }
            return result;
        }



        #endregion

        public DataSet GetPrdLst(long StoreId, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetPrdLst";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@StoreId", StoreId);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }

        public DataSet GetPaymentLst(long StoreId, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetPaymentLst";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@StoreId", StoreId);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }



        #region VFC Bulk import


        public void SaveUserDetails(string compid, string deptid, string usersid, long uid, string[] conn)
        {

            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                DataSet ds = new DataSet();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_SaveUserDetails";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@compid", compid);
                cmd.Parameters.AddWithValue("@deptid", deptid);
                cmd.Parameters.AddWithValue("@usersid", usersid);
                cmd.Parameters.AddWithValue("@uid", uid);
                cmd.ExecuteNonQuery();
            }

        }

        public DataSet GetUserDetails(string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetUserDetails";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }


        public DataSet GetCompanyListNEw(string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetCompanylist";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }



        public void DeleteUserConfig(string id, long uid, string[] conn)
        {

            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                DataSet ds = new DataSet();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_DeleteUserConfig";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@uid", uid);               
                cmd.ExecuteNonQuery();
            }

        }


        #endregion

        public List<mCompany> GetCompanyDropDown(long companyid, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mCompany> Company = new List<mCompany>();
            Company = (from p in ce.mCompanies
                       where p.Active == "Y" && p.ID == companyid
                       select p).ToList();
            return Company;
        }
        public DataSet GetCustomerID(long UserID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_WMS_GetCustomerByUserID";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("UserID", UserID);
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;
        }
    }




}
