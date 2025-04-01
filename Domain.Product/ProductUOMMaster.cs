using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
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

    public partial class ProductUOMMaster : Interface.Product.iProductUOM
    {
        Domain.Server.Server svr = new Server.Server();

        #region GetProductUOMList
        public List<mUOM> GetProductUOMList(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mUOM> ListObj = new List<mUOM>();
            ListObj = (from p in ce.mUOMs
                       orderby p.Sequence
                       select p).ToList();
            return ListObj;
        }
        #endregion

        #region InsertmProductUOM
        public int InsertmProductUOM(mUOM prdUOM, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.mUOMs.AddObject(prdUOM);
            ce.SaveChanges();
            return 1;
        }
        #endregion

        #region updatemProductUOM
        public int updatemProductUOM(mUOM updatePrdUOM, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.mUOMs.Attach(updatePrdUOM);
            ce.ObjectStateManager.ChangeObjectState(updatePrdUOM, EntityState.Modified);
            ce.SaveChanges();
            return 1;
        }
        #endregion

        #region GetProductUOMListByID
        public mUOM GetProductUOMListByID(int prdUOMId, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            mUOM ProductUOMID = new mUOM();
            ProductUOMID = (from p in ce.mUOMs
                                 where p.ID == prdUOMId
                                 select p).FirstOrDefault();
            ce.Detach(ProductUOMID);
            return ProductUOMID;
        }
        #endregion

        #region checkDuplicateRecord
        public string checkDuplicateRecord(string prdUOMName, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            var output = (from p in ce.mUOMs
                          where p.Name == prdUOMName
                          select new { p.Name }).FirstOrDefault();

            if (output != null)
            {
                result = "[ " + prdUOMName + " ] ProductUOM Name already exist";
            }
            return result;
        }
        #endregion

        #region checkDuplicateRecordEdit
        public string checkDuplicateRecordEdit(string prdUOMName, int prdUOMID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            var output = (from p in ce.mUOMs
                          where p.Name == prdUOMName && p.ID != prdUOMID
                          select new { p.Name }).FirstOrDefault();

            if (output != null)
            {
                result = "[ " + prdUOMName + " ] ProductUOM Name already exist";
            }
            return result;
        }
        #endregion

        #region GetPrdUOMRecordToBindGrid
        public DataSet GetPrdUOMRecordToBindGrid(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mUOM> LeadSource = new List<mUOM>();
            XElement xmlPrdUOMMaster = new XElement("PrdUOMList", from m in ce.mUOMs.AsEnumerable()
                                                                        orderby m.Sequence
                                                                        select new XElement("PrdUOM",
                                                                        new XElement("ID", m.ID),
                                                                        new XElement("Name", m.Name),
                                                                        new XElement("Sequence", m.Sequence),
                                                                        new XElement("Active", m.Active == "Y" ? "Yes" : "No")
                                                                        ));
            DataSet ds = new DataSet();
            ds.ReadXml(xmlPrdUOMMaster.CreateReader());
            DataTable dt = new DataTable();
            if (ds.Tables.Count <= 0)
            {
                dt = ds.Tables.Add("PrdUOM1");
            }
            return ds;
        }
        #endregion
    }
}
