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

    public partial class ProductSubCategoryMaster : Interface.Product.iProductSubCategoryMaster
    {
        Domain.Server.Server svr = new Server.Server();

        #region GetProductSubCategoryList
        /// <summary>
        /// GetProductSubCategoryList is providing List of ProductSubCategory
        /// </summary>
        /// <returns></returns>
        /// 
        public List<mProductSubCategory> GetProductSubCategoryList(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mProductSubCategory> ProductSubCategory = new List<mProductSubCategory>();
            ProductSubCategory = (from p in ce.mProductSubCategories
                                  orderby p.Sequence
                                  select p).ToList();
            return ProductSubCategory;
        }
        #endregion

        #region InsertmProductSubCategory
        public int InsertmProductSubCategory(mProductSubCategory prdSubCategory, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.mProductSubCategories.AddObject(prdSubCategory);
            ce.SaveChanges();
            return 1;
        }
        #endregion

        #region updatemProductSubCategory
        public int updatemProductSubCategory(mProductSubCategory updateprdSubCategory, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.mProductSubCategories.Attach(updateprdSubCategory);
            ce.ObjectStateManager.ChangeObjectState(updateprdSubCategory, EntityState.Modified);
            ce.SaveChanges();
            return 1;
        }
        #endregion

        #region GetProductSubCategoryListByID
        /// <summary>
        /// GetProductSubCategoryListByID is providing List of ProductSubCategory By ID
        /// </summary>
        /// <returns></returns>
        /// 
        public mProductSubCategory GetProductSubCategoryListByID(int ProductSubCategoryId, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            mProductSubCategory PrdSubCategoryID = new mProductSubCategory();
            PrdSubCategoryID = (from p in ce.mProductSubCategories
                                where p.ID == ProductSubCategoryId
                                select p).FirstOrDefault();
            ce.Detach(PrdSubCategoryID);
            return PrdSubCategoryID;

        }
        #endregion

        #region checkDuplicateRecord
        /// <summary>
        /// checkDuplicateRecord is providing List of ProductSubCategory by ProductSubCategoryName ProductCategory
        /// </summary>
        /// <returns></returns>
        /// 
        public string checkDuplicateRecord(string productSubCategoryName, int prdCategoryID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            var output = (from p in ce.mProductSubCategories
                          where p.Name == productSubCategoryName && p.ProductCategoryID == prdCategoryID
                          select new { p.Name }).FirstOrDefault();
            if (output != null)
            {
                result = "[ " + productSubCategoryName + " ]  ProductSubCategory for the ProductCategory allready exist";
            }
            return result;
        }
        #endregion

        #region checkDuplicateRecordEdit
        /// <summary>
        /// checkDuplicateRecord for Edit is providing List of ProductSubCategory by ProductSubCategoryName and ProductSubCategoryID and ProductCategoryID
        /// </summary>
        /// <returns></returns>
        /// 
        public string checkDuplicateRecordEdit(int productSubCategoryID, string productSubCategoryName, int productCategoryID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            var output = (from p in ce.mProductSubCategories
                          where p.Name == productSubCategoryName && p.ID != productSubCategoryID && p.ProductCategoryID == productCategoryID
                          select new { p.Name }).FirstOrDefault();
            if (output != null)
            {
                result = "[ " + productSubCategoryName + " ] ProductSubCategory for the ProductCategory[" + productCategoryID + "] allready exist";
            }
            return result;
        }
        #endregion

        #region GetPrdSubCategoryRecordToBind
        /// <summary>
        /// GetPrdSubCategoryRecordToBind is providing List of PrdSubCategory for bind grid with Actine Yes/No
        /// </summary>
        /// <returns></returns>
        /// 
        public DataSet GetPrdSubCategoryRecordToBind(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<vProductSubCategory> LeadSource = new List<vProductSubCategory>();
            XElement xmlPrdSubCategoryMaster = new XElement("PrdSubCategoryList", from m in ce.vProductSubCategories.AsEnumerable()
                                                                                  orderby m.Sequence
                                                                                  select new XElement("PrdSubCategory",
                                                                                  new XElement("ID", m.ID),
                                                                                  new XElement("Name", m.Name),
                                                                                  new XElement("PrdCategoryName", m.PrdCategoryName),
                                                                                  new XElement("Sequence", m.Sequence),
                                                                                  new XElement("Active", m.Active == "Y" ? "Yes" : "No")
                                                                                  ));
            DataSet ds = new DataSet();
            ds.ReadXml(xmlPrdSubCategoryMaster.CreateReader());
            DataTable dt = new DataTable();
            if (ds.Tables.Count <= 0)
            {
                dt = ds.Tables.Add("PrdSubCategory1");
            }

            return ds;
        }

        /// <summary>
        /// Get ProductSubCategory list by ProductCategoryID to fill ProductSubCategory Dropdown
        /// </summary>
        /// <param name="productCategoryID"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public List<vGetProductSubCagetoryList> GetProductSubCategoryByProductCategoryID(long productCategoryID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<vGetProductSubCagetoryList> ProductSubCagetoryList = new List<vGetProductSubCagetoryList>();
            ProductSubCagetoryList = (from o in db.vGetProductSubCagetoryLists
                                      where o.ProductCategoryID == productCategoryID
                                      orderby o.Sequence
                                      select o).ToList();

            return ProductSubCagetoryList;
        }
        #endregion
    }
}
