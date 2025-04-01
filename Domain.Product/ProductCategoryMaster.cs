using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Activation;
using System.Configuration;
using Interface.Product;
using System.ServiceModel;
using System.Xml.Linq;
using Domain.Server;
namespace Domain.Product
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]

    public partial class ProductCategoryMaster : Interface.Product.iProductCategoryMaster
    {
        Domain.Server.Server svr = new Server.Server();

        #region GetProductCategoryList
        /// <summary>
        /// GetProductCategoryList is providing List of ProductCategory
        /// </summary>
        /// <returns></returns>
        /// 
        public List<vGetProductCagetoryList> GetProductCategoryList(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<vGetProductCagetoryList> ProductCategory = new List<vGetProductCagetoryList>();
            ProductCategory = (from p in ce.vGetProductCagetoryLists
                               orderby p.Sequence
                               select p).ToList();
            return ProductCategory;
        }
        #endregion

        #region GetProductCategoryListForAsset
        /// <summary>
        /// GetProductCategoryList is providing List of ProductCategory
        /// </summary>
        /// <returns></returns>
        /// 
        public List<vGetProductCagetoryList> GetProductCategoryListForAsset(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<vGetProductCagetoryList> ProductCategory = new List<vGetProductCagetoryList>();
            ProductCategory = (from p in ce.vGetProductCagetoryLists
                               where p.ID >=4 
                               orderby p.Sequence
                               select p).ToList();
            return ProductCategory;
        }
        #endregion


        #region InsertmProductCategory
        public int InsertmProductCategory(mProductCategory prdCategory, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.mProductCategories.AddObject(prdCategory);
            ce.SaveChanges();
            return 1;
        }
        #endregion

        #region updatemProductCategory
        public int updatemProductCategory(mProductCategory updatePrdCategory, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.mProductCategories.Attach(updatePrdCategory);
            ce.ObjectStateManager.ChangeObjectState(updatePrdCategory, EntityState.Modified);
            ce.SaveChanges();
            return 1;
        }
        #endregion

        #region GetProductCategoryListByID
        /// <summary>
        /// GetProductCategoryListByID is providing List of ProductCategory ByID for Update record
        /// </summary>
        /// <returns></returns>
        /// 
        public mProductCategory GetProductCategoryListByID(int prdCategoryId, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            mProductCategory ProductCategoryID = new mProductCategory();
            ProductCategoryID = (from p in ce.mProductCategories
                                 where p.ID == prdCategoryId
                                 select p).FirstOrDefault();
            ce.Detach(ProductCategoryID);
            return ProductCategoryID;
        }
        #endregion

        #region checkDuplicateRecord
        /// <summary>
        /// checkDuplicateRecord is providing List of ProductCategory by ProductCategoryName 
        /// </summary>
        /// <returns></returns>
        /// 
        public string checkDuplicateRecord(string prdCategoryName, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            var output = (from p in ce.mProductCategories
                          where p.Name == prdCategoryName
                          select new { p.Name }).FirstOrDefault();

            if (output != null)
            {
                result = "[ " + prdCategoryName + " ] ProductCategory Name allready exist";
            }
            return result;

        }
        #endregion

        #region checkDuplicateRecordEdit
        /// <summary>
        /// checkDuplicateRecord for Edit is providing List of ProductCategory by ProductCategory and ID
        /// </summary>
        /// <returns></returns>
        /// 
        public string checkDuplicateRecordEdit(string prdCategoryName, int prdCategoryID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            var output = (from p in ce.mProductCategories
                          where p.Name == prdCategoryName && p.ID != prdCategoryID
                          select new { p.Name }).FirstOrDefault();

            if (output != null)
            {
                result = "[ " + prdCategoryName + " ] ProductCategory Name  allready exist";
            }
            return result;
        }
        #endregion

        #region GetPrdCategoryRecordToBindGrid
        /// <summary>
        /// GetPrdCategoryRecordToBindGrid is providing List of Prdcategory for bind grid with Actine Yes/No
        /// </summary>
        /// <returns></returns>
        /// 
        public DataSet GetPrdCategoryRecordToBindGrid(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mProductCategory> LeadSource = new List<mProductCategory>();
            XElement xmlPrdcategoryMaster = new XElement("PrdcategoryList", from m in ce.mProductCategories.AsEnumerable()
                                                                            orderby m.Sequence
                                                                            select new XElement("Prdcategory",
                                                                        new XElement("ID", m.ID),
                                                                        new XElement("Name", m.Name),
                                                                        new XElement("Sequence", m.Sequence),
                                                                        new XElement("Active", m.Active == "Y" ? "Yes" : "No")
                                                                        ));
            DataSet ds = new DataSet();
            ds.ReadXml(xmlPrdcategoryMaster.CreateReader());
            DataTable dt = new DataTable();
            if (ds.Tables.Count <= 0)
            {
                dt = ds.Tables.Add("Prdcategory1");
            }
            return ds;
        }
        #endregion

        public List<mDropdownValue> GetActivityList(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mDropdownValue> ActivityList = new List<mDropdownValue>();
            ActivityList = (from c in ce.mDropdownValues
                            where c.Parameter == "Event"
                               select c).ToList();
            return ActivityList;
        }

        public List<mDropdownValue> GetMessageList(string[] conn)
        {
             BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
             List<mDropdownValue> MessageList = new List<mDropdownValue>();
            MessageList = (from c in ce.mDropdownValues
                           where c.Parameter == "Message Type"
                               select c).ToList();
            return MessageList;
        }

        public long InsertEmailTemplate(mMessageEMailTemplate Template, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            try
            {
                ce.mMessageEMailTemplates.AddObject(Template);
                ce.SaveChanges();
                return Template.ID;
            }
            catch { return 0; }
        }

        public List<vGetTemplateList1> GetTemplateListForGrid(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<vGetTemplateList1> TemplateList = new List<vGetTemplateList1>();
            TemplateList = (from c in ce.vGetTemplateList1
                            where c.Active=="Yes"
                           select c).ToList();
            return TemplateList;
        }

        public List<vGetTemplateList1> GetTemplateListForGridAdmin(long UserID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<vGetTemplateList1> TemplateList = new List<vGetTemplateList1>();
            TemplateList = (from c in ce.vGetTemplateList1
                            join t in ce.mUserTerritoryDetails
                            on c.DepartmentID equals t.TerritoryID
                            where t.UserID==UserID && c.Active=="Yes"
                            select c).ToList();
            return TemplateList;
        }


        public void UpdateEmailTemplate(string ID, string MailSubject, string MailBody, long ModifiedBy, long CompanyID, long DepartmentID, long ActivityID, long MessageID, string TemplateTitle, string Active, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_Update_EmailTemplateDetails";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("ID", ID);
            cmd.Parameters.AddWithValue("MailSubject", MailSubject);
            cmd.Parameters.AddWithValue("MailBody",MailBody);

            cmd.Parameters.AddWithValue("ModifiedBy",ModifiedBy);
            cmd.Parameters.AddWithValue("CompanyID",CompanyID);
            cmd.Parameters.AddWithValue("DepartmentID",DepartmentID);
            cmd.Parameters.AddWithValue("ActivityID", ActivityID);
            cmd.Parameters.AddWithValue("MessageID", MessageID);
            cmd.Parameters.AddWithValue("TemplateTitle",TemplateTitle);
            cmd.Parameters.AddWithValue("Active", Active);
            cmd.ExecuteNonQuery();
        }


        public string GetAutoCancellationStatus(long DeptID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            var output = (from p in ce.mTerritories
                          where p.ID == DeptID
                          select new { p.AutoCancel }).FirstOrDefault();

            return result=output.ToString();
        }

        public string checkDuplicateTemplate(long company, long Department, long activity, long MessageType, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            var output = (from p in ce.mMessageEMailTemplates
                          where p.CompanyID == company && p.DepartmentID == Department && p.ActivityID == activity && p.MessageID == MessageType
                          select new { p.ID }).FirstOrDefault();

            if (output != null)
            {
                result = "Same Email Template Already Exist";
            }
            return result;

        }

        public string checkDuplicateTemplateEdit(long TemplateID,long company, long Department, long activity, long MessageType, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            var output = (from p in ce.mMessageEMailTemplates
                          where p.CompanyID == company && p.DepartmentID == Department && p.ActivityID == activity && p.MessageID == MessageType && p.ID != TemplateID && p.Active =="Yes"
                          select new { p.ID }).FirstOrDefault();

            if (output != null)
            {
                result = "Same Employee Code Already Exist";

            }
            return result;
        }


        public string checkDuplicateInterface(string Tablename,string FieldName, string DataType, string IsNull, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            var output = (from p in ce.mInterfaceMaps
                          where p.TableName == Tablename && p.Fieldname == FieldName && p.FieldDataType == DataType && p.IsNull == IsNull
                          select new { p.Id }).FirstOrDefault();

            if (output != null)
            {
                result = "Same Interface Information Already Exist";
            }
            return result;

        }

        public string checkDuplicateInterfaceEdit(long ID, string Tablename, string FieldName, string DataType, string IsNull, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            var output = (from p in ce.mInterfaceMaps
                          where p.TableName == Tablename && p.Fieldname == FieldName && p.FieldDataType == DataType && p.IsNull == IsNull && p.Id != ID
                          select new { p.Id }).FirstOrDefault();

            if (output != null)
            {
                result = "Same Interface Information Already Exist";

            }
            return result;
        }

        public List<mDropdownValue> GetDestination(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mDropdownValue> Destinationlist = new List<mDropdownValue>();
            Destinationlist = (from cl in ce.mDropdownValues
                           where cl.Parameter == "Destination"
                           select cl).ToList();
            return Destinationlist;
        }

        public List<mDropdownValue> GetActionType(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mDropdownValue> Actiontype = new List<mDropdownValue>();
            Actiontype = (from cl in ce.mDropdownValues
                           where cl.Parameter == "ActionType"
                           select cl).ToList();
            return Actiontype;
        }

        public List<mDropdownValue> GetObject(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mDropdownValue> objectlst = new List<mDropdownValue>();
            objectlst = (from cl in ce.mDropdownValues
                          where cl.Parameter == "Table"
                          select cl).ToList();
            return objectlst;
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

        public List<mInterfaceMap> FieldList(string TableName, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mInterfaceMap> FieldList = new List<mInterfaceMap>();
            FieldList = (from cl in ce.mInterfaceMaps
                          where cl.TableName == TableName
                          select cl).ToList();
            return FieldList;
        }

        public void DeleteMessageTemptable(string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_DeleteMessageTemptable";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.ExecuteNonQuery();
        }

        public void InsertMessageIntoTemptable(string Title, string Destination, string Type, string Purpose, string Object1, long Field1, long Sequence, string[] conn)
        {
        
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_InsertMessageIntoTemptable";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("description", Title);
            cmd.Parameters.AddWithValue("Destination", Destination);
            cmd.Parameters.AddWithValue("ActionType", Type);
            cmd.Parameters.AddWithValue("TableName", Object1);
            cmd.Parameters.AddWithValue("Remark", Purpose);
            cmd.Parameters.AddWithValue("sequence", Sequence);
            cmd.Parameters.AddWithValue("FieldID", Field1);
            cmd.ExecuteNonQuery();
        }

         public void InsertMessageHeader(string Destination, string ActionType, string TableName, string description, long CreatedBy, string Remark, string[] conn)
        {
        
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_InsertMessageHeader";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("Destination", Destination);
            cmd.Parameters.AddWithValue("ActionType", ActionType);
            cmd.Parameters.AddWithValue("TableName", TableName);
            cmd.Parameters.AddWithValue("description", description);
            cmd.Parameters.AddWithValue("CreatedBy", CreatedBy);
            cmd.Parameters.AddWithValue("Remark", Remark);
            cmd.ExecuteNonQuery();
        }

         public DataSet GetMessageTempData (string[] conn)
         {
            DataSet ds = new DataSet();
             ds = fillds("select * from dbo.tMsgDefinationTemp", conn);
             return ds;
         }
         public DataSet GetMessHeaderID(string[] conn)
         {
             DataSet ds = new DataSet();
             ds = fillds("select Top 1 Id from mMessageHeader order by Id desc", conn);
             return ds;
         }

         public void DeleteFieldFromList(long ID,string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_DeleteFieldFromList";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("Id", ID);
            cmd.ExecuteNonQuery();
        }
         public void DeleteFieldFromListWhenEdit(long ID, string[] conn)
         {
             SqlCommand cmd = new SqlCommand();
             cmd.CommandType = CommandType.StoredProcedure;
             cmd.CommandText = "SP_DeleteMessageDetails";
             cmd.Connection = svr.GetSqlConn(conn);
             cmd.Parameters.Clear();
             cmd.Parameters.AddWithValue("Id", ID);
             cmd.ExecuteNonQuery();
         }
         public void InsrtIntoMessageHeader(long HeaderId,long Sequence, long FieldID, long CreatedBy, string[] conn)
         {

             SqlCommand cmd = new SqlCommand();
             cmd.CommandType = CommandType.StoredProcedure;
             cmd.CommandText = "SP_InsertIntoMessageDetails";
             cmd.Connection = svr.GetSqlConn(conn);
             cmd.Parameters.Clear();
             cmd.Parameters.AddWithValue("Sequence", Sequence);
             cmd.Parameters.AddWithValue("FieldId", FieldID);
             cmd.Parameters.AddWithValue("CreatedBy", CreatedBy);
             cmd.Parameters.AddWithValue("messageHdrId", HeaderId);
             cmd.ExecuteNonQuery();
         }

         public DataSet GetFieldDetails(string[] conn)
         {
             DataSet ds = new DataSet();
             ds = fillds("select temp.Id, temp.FieldID,M.Fieldname,M.FieldDataType, M.IsNull from tMsgDefinationTemp temp left outer join mInterfaceMap M on M.Id = temp.FieldID", conn);
             return ds;
         }

         public DataSet GetFieldDetailsFromMessageTable( long MessageID,string[] conn)
         {
             DataSet ds = new DataSet();
             ds = fillds(" select M.Id, M.Sequence,M.messageHdrId,M.FieldId,D.Fieldname,D.FieldDataType,D.IsNull from mMessageDetails M left outer join mInterfaceMap D on M.FieldId = D.Id where M.messageHdrId ='" + MessageID + "'", conn);
             return ds;
         }

         public void UpdateMessageHeader(long MessageheaderID,string Destination, string ActionType, string TableName, string description, long ModifyBy, string Remark, string[] conn)
        {
        
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_UpdateMessageHeader";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("Id", MessageheaderID);
            cmd.Parameters.AddWithValue("Destination", Destination);
            cmd.Parameters.AddWithValue("ActionType", ActionType);
            cmd.Parameters.AddWithValue("TableName", TableName);
            cmd.Parameters.AddWithValue("description", description);
            cmd.Parameters.AddWithValue("ModifiedBy", ModifyBy);
            cmd.Parameters.AddWithValue("Remark", Remark);
            cmd.ExecuteNonQuery();
        }

         public void InsertMessageDetails(long MsgHeadID, long Field, long Sequence, long CreatedBy, string[] conn)
         {

             SqlCommand cmd = new SqlCommand();
             cmd.CommandType = CommandType.StoredProcedure;
             cmd.CommandText = "SP_InsertMessageDetails";
             cmd.Connection = svr.GetSqlConn(conn);
             cmd.Parameters.Clear();
             cmd.Parameters.AddWithValue("messageHdrId", MsgHeadID);
             cmd.Parameters.AddWithValue("FieldId", Field);
             cmd.Parameters.AddWithValue("Sequence", Sequence);
             cmd.Parameters.AddWithValue("CreatedBy", CreatedBy);
             cmd.ExecuteNonQuery();
         }

        
    }
}
