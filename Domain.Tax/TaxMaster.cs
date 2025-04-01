using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Activation;
using System.Configuration;
using Interface.Tax;
using System.ServiceModel;
using System.Xml.Linq;
using Domain.Server;
namespace Domain.Tax
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]

    public partial class TaxMaster : Interface.Tax.iTaxMaster
    {
        Domain.Server.Server svr = new Server.Server();

        #region GetTaxList
        /// <summary>
        /// GetTaxList is providing List of Tax
        /// </summary>
        /// <returns></returns>
        /// 
        public List<mTaxSetup> GetTaxList(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mTaxSetup> Tax = new List<mTaxSetup>();
            Tax = (from p in ce.mTaxSetups
                   orderby p.Sequence
                   select p).ToList();
            return Tax;
        }
        #endregion

        #region InsertmTaxSetup
        public int InsertmTaxSetup(mTaxSetup tax, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.mTaxSetups.AddObject(tax);
            ce.SaveChanges();
            return 1;
        }
        #endregion

        #region updatemTaxSetup
        public int updatemTaxSetup(mTaxSetup updateTax, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.mTaxSetups.Attach(updateTax);
            ce.ObjectStateManager.ChangeObjectState(updateTax, EntityState.Modified);
            ce.SaveChanges();
            return 1;
        }
        #endregion

        #region GetTaxListByID
        /// <summary>
        /// GetTaxListByID is providing List of Tax By ID
        /// </summary>
        /// <returns></returns>
        /// 
        public mTaxSetup GetTaxListByID(int taxId, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            mTaxSetup TaxID = new mTaxSetup();
            TaxID = (from p in ce.mTaxSetups
                     where p.ID == taxId
                     select p).FirstOrDefault();
            ce.Detach(TaxID);
            return TaxID;
        }
        #endregion

        #region checkDuplicateRecord
        /// <summary>
        /// checkDuplicateRecord is providing List of Tax by TaxName and TaxType
        /// </summary>
        /// <returns></returns>
        /// 
        public string checkDuplicateRecord(string taxName, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            var output = (from p in ce.mTaxSetups
                          where p.Name == taxName
                          select new { p.Name }).FirstOrDefault();
            if (output != null)
            {
                result = "[ " + taxName + " ]  Tax Name for the Tax Type allready exist";
            }
            return result;
        }
        #endregion

        #region checkDuplicateRecordEdit
        /// <summary>
        /// checkDuplicateRecord for Edit is providing List of Tax by TaxName And TaxType And TaxID
        /// </summary>
        /// <returns></returns>
        /// 
        public string checkDuplicateRecordEdit(int taxID, string taxName, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            var output = (from p in ce.mTaxSetups
                          where p.Name == taxName && p.ID != taxID
                          select new { p.Name }).FirstOrDefault();
            if (output != null)
            {
                result = "[ " + taxName + " ]  Tax Name for the Tax Type allready exist";
            }
            return result;
        }
        #endregion

        #region GetTaxRecordToBindGrid
        /// <summary>
        /// GetTaxRecordToBindGrid is providing List of Tax for bind grid with Actine Yes/No
        /// </summary>
        /// <returns></returns>
        /// 
        public DataSet GetTaxRecordToBindGrid(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<vTaxSetupDetail> LeadSource = new List<vTaxSetupDetail>();
            XElement xmlTaxMaster = new XElement("TaxList", from m in ce.vTaxSetupDetails.AsEnumerable()
                                                            orderby m.Sequence
                                                            select new XElement("Tax",
                                                            new XElement("ID", m.ID == null ? 0 : m.ID),
                                                            new XElement("Name", m.Name == null ? "N/A" : m.Name),
                                                            new XElement("Type", m.Type),
                                                            new XElement("Percent", m.Percent),
                                                            new XElement("TaxMapping", m.TaxMapping),
                                                            new XElement("Sequence", m.Sequence),
                                                            new XElement("Active", m.Active == "Y" ? "Yes" : "No"),
                                                             new XElement("Description", m.Description)
                                                            ));
            DataSet ds = new DataSet();
            ds.ReadXml(xmlTaxMaster.CreateReader());
            if (ds.Tables.Count <= 0)
            {
                ds.Tables.Add("TaxList");
                ds.Tables[0].Columns.Add("ID");
                ds.Tables[0].Columns.Add("Name");
                ds.Tables[0].Columns.Add("Type");
                ds.Tables[0].Columns.Add("Percent");
                ds.Tables[0].Columns.Add("TaxMapping");
                ds.Tables[0].Columns.Add("Sequence");
                ds.Tables[0].Columns.Add("Active");
                ds.Tables[0].Columns.Add("Description");
            }

            return ds;
        }
        #endregion

        #region GetTaxRecordToBindTaxMappingGrid
        /// <summary>
        /// GetTaxRecordToBindTaxMappingGrid is providing List of Tax for bind grid with Actine Yes/No and only  "Tax On Principal" records
        /// </summary>
        /// <returns></returns>
        /// 
        public DataSet GetTaxRecordToBindTaxMappingGrid(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<vTaxSetupDetail> Tax = new List<vTaxSetupDetail>();
            XElement xmlTaxMapping = new XElement("TaxList", from m in ce.vTaxSetupDetails.AsEnumerable()
                                                             orderby m.Sequence
                                                             where m.Type == "Tax On Principal"
                                                             select new XElement("Tax",
                                                             new XElement("ID", m.ID),
                                                             new XElement("Name", m.Name),
                                                             new XElement("Type", m.Type),
                                                             new XElement("Percent", m.Percent),
                                                             new XElement("TaxMapping", m.TaxMapping),
                                                             new XElement("Sequence", m.Sequence),
                                                             new XElement("Active", m.Active == "Y" ? "Yes" : "No"),
                                                             new XElement("Description", m.Description)
                                                             ));
            DataSet ds = new DataSet();
            ds.ReadXml(xmlTaxMapping.CreateReader());
            if (ds.Tables.Count <= 0)
            {
                ds.Tables.Add("TaxList");
                ds.Tables[0].Columns.Add("ID");
                ds.Tables[0].Columns.Add("Name");
                ds.Tables[0].Columns.Add("Type");
                ds.Tables[0].Columns.Add("Percent");
                ds.Tables[0].Columns.Add("TaxMapping");
                ds.Tables[0].Columns.Add("Sequence");
                ds.Tables[0].Columns.Add("Active");
                ds.Tables[0].Columns.Add("Description");
            }
            return ds;
        }
        #endregion

    }
}
