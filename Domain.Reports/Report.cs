using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Activation;
using System.Configuration;
using ElegantCRM.Model;
using System.ServiceModel;
using System.Xml.Linq;
using Domain.Server;

namespace Domain.Reports
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]

    public partial class Report : Interface.Report.iReport
    {
        Domain.Server.Server svr = new Server.Server();

        #region GetAllTermsAndCondition
        /// <summary>
        /// Get All Records From tTermsConditionsDetail By ObjectName And ReferenceID
        /// </summary>
        /// <param name="ObjectName"></param>
        /// <param name="ReferenceID"></param>
        /// <returns></returns>
        public DataSet GetAllTermsAndCondition(string ObjectName, string ReferenceID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            DataSet ds = new DataSet();

            string[] strings = new string[] { };
            strings = ReferenceID.Split(',');
            long[] arrayIDs = strings.Select(x => long.Parse(x)).ToArray();

            //List<tTermsConditionsDetail> ObjtermsAndCondition = new List<tTermsConditionsDetail>();

            var ListofID = from a in arrayIDs.AsEnumerable()
                           select new { NewID = a };

            XElement TermsAndCondition = new XElement("TermsAndConditionList", from a in ListofID
                                                                               join m in ce.tTermsConditionsDetails.AsEnumerable() on a.NewID equals m.ReferenceID into newTermsList
                                                                               from newList in newTermsList.DefaultIfEmpty()
                                                                               where newList.ObjectName == ObjectName && newList.Active == "Y"
                                                                               select new XElement("TermsAndCondition",
                                                                               new XElement("TermAndCondition", newList.Term == null ? "None" : newList.Term + "-" + newList.Condition)

                                                                       ));

            ds.ReadXml(TermsAndCondition.CreateReader());
            if (ds.Tables.Count <= 0)
            {
                ds.Tables.Add("Terms");
            }

            return ds;
        }

        #endregion

        #region GetAlllProductDetails
        /// <summary>
        /// Get All Records From vGetAddToCartProductDetail_OldRec
        /// </summary>
        /// <param name="ObjectName"></param>
        /// <param name="ReferenceID"></param>
        /// <returns></returns>
        public DataSet GetAlllProductDetails(string ObjectName, string ReferenceID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            DataSet ds = new DataSet();

            string[] strings = new string[] { };
            strings = ReferenceID.Split(',');
            long[] arrayIDs = strings.Select(x => long.Parse(x)).ToArray();

            // List<vGetAddToCartProductDetail_OldRec> ObjAddToCart = new List<vGetAddToCartProductDetail_OldRec>();

            var ListofID = from a in arrayIDs.AsEnumerable()
                           select new { NewID = a };

            XElement ProductDetails = new XElement("ProductDetailList", from a in ListofID
                                                                        join m in ce.vGetAddToCartProductDetail_OldRec.AsEnumerable() on a.NewID equals m.ReferenceID into NewProductList
                                                                        from rec in NewProductList.DefaultIfEmpty()
                                                                        select new XElement("AddToCartProduct",
                                                                        new XElement("ProductID", rec.ProductID == null ? 0 : rec.ProductID),
                                                                        new XElement("ProductCode", rec.ProductCode == null ? "0" : rec.ProductCode),
                                                                        new XElement("ProductName", rec.ProductName == null ? "None" : rec.ProductName),
                                                                        new XElement("ProductDescription", rec.ProductDescription == null ? "None" : rec.ProductDescription),
                                                                        new XElement("UOMID", rec.UOMID == null ? 0 : rec.UOMID),
                                                                        new XElement("UOM", rec.UOM == null ? "0" : rec.UOM),
                                                                        new XElement("ProductPrice", rec.ProductPrice == null ? 0 : rec.ProductPrice),
                                                                        new XElement("PerUnitDiscount", rec.PerUnitDiscount == null ? 0 : rec.PerUnitDiscount),
                                                                        new XElement("IsDiscountPercent", rec.IsDiscountPercent == null ? false : rec.IsDiscountPercent),
                                                                        new XElement("DiscountID", rec.DiscountID == null ? 0 : rec.DiscountID),
                                                                        new XElement("RateAfterDiscount", rec.RateAfterDiscount == null ? 0 : rec.RateAfterDiscount),
                                                                        new XElement("Quantity", rec.Quantity == null ? 0 : rec.Quantity),
                                                                        new XElement("AmountAfterDiscount", rec.AmountAfterDiscount == null ? 0 : rec.AmountAfterDiscount),
                                                                        new XElement("TotalTaxAmount", rec.TotalTaxAmount == null ? 0 : rec.TotalTaxAmount),
                                                                        new XElement("AmountAfterTax", rec.AmountAfterTax == null ? 0 : rec.AmountAfterTax),
                                                                        new XElement("Remark", rec.Remark == null ? "None" : rec.Remark)
                                                      ));




            ds.ReadXml(ProductDetails.CreateReader());
            if (ds.Tables.Count <= 0)
            {
                ds.Tables.Add("product");
            }

            return ds;
        }

        #endregion

        #region GetAllQutationDetails
        /// <summary>
        /// Get All Records From v_GetAllQuotationForView
        /// </summary>
        /// <param name="ObjectName"></param>
        /// <param name="ReferenceID"></param>
        /// <returns></returns>
        public DataSet GetAllQutationDetails(string ObjectName, string ReferenceID, string[] conn)
        {
            DataSet ds = new DataSet();
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string[] strings = new string[] { };
            strings = ReferenceID.Split(',');
            long[] arrayIDs = strings.Select(x => long.Parse(x)).ToArray();


            //List<v_GetAllQuotationForView> ObjQuotation = new List<v_GetAllQuotationForView>();

            var ListofID = from a in arrayIDs.AsEnumerable()
                           select new { NewID = a };

            XElement QuotationHeadList = new XElement("QuotationList", from a in ListofID
                                                                       join m in ce.v_GetAllQuotationForView.AsEnumerable() on a.NewID equals m.ID into NewQuotationList
                                                                       from rec in NewQuotationList.DefaultIfEmpty()
                                                                       select new XElement("QuotationLst",
                                                                       new XElement("ID", rec.ID == null ? 0 : rec.ID),
                                                                       new XElement("QuotationNo", rec.QuotationNo == null ? "0" : rec.QuotationNo),
                                                                       new XElement("QuotationDate", rec.QuotationDate == null ? DateTime.Now : rec.QuotationDate),
                                                                       new XElement("QuotationStatus", rec.QuotationStatus == null ? "" : rec.QuotationStatus),
                                                                       new XElement("QuotationValidityDays", rec.QuotationValidityDays == null ? 0 : rec.QuotationValidityDays),
                                                                       new XElement("ExpectedOrderDate", rec.ExpectedOrderDate == null ? DateTime.Now : rec.ExpectedOrderDate),
                                                                       new XElement("ExpOrderAmount", rec.ExpOrderAmount == null ? 0 : rec.ExpOrderAmount),
                                                                       new XElement("LeadSource", rec.LeadSource == null ? "" : rec.LeadSource),
                                                                       new XElement("OtherCharges", rec.OtherCharges == null ? 0 : rec.OtherCharges),
                                                                       new XElement("ProductLevelTotalDiscount", rec.ProductLevelTotalDiscount == null ? 0 : rec.ProductLevelTotalDiscount),
                                                                       new XElement("Sector", rec.Sector == null ? "" : rec.Sector),
                                                                       new XElement("ShippingCharges", rec.ShippingCharges == null ? 0 : rec.ShippingCharges),
                                                                       new XElement("TotalAfterDiscount", rec.TotalAfterDiscount == null ? 0 : rec.TotalAfterDiscount),
                                                                       new XElement("TotalAmount", rec.TotalAmount == null ? 0 : rec.TotalAmount),
                                                                       new XElement("TotalDiscount", rec.TotalDiscount == null ? 0 : rec.TotalDiscount),
                                                                       new XElement("TotalTax", rec.TotalTax == null ? 0 : rec.TotalTax),
                                                                       new XElement("DiscountOnSubTotal", rec.DiscountOnSubTotal == null ? 0 : rec.DiscountOnSubTotal),
                                                                       new XElement("CustomerName", rec.CustomerName == null ? "" : rec.CustomerName),
                                                                       new XElement("DiscountOnSubTotal", rec.DiscountOnSubTotal == null ? 0 : rec.DiscountOnSubTotal),
                                                                       new XElement("EmailID", rec.EmailID == null ? "" : rec.EmailID),
                                                                       new XElement("AddressLine1", rec.AddressLine1 == null ? "" : rec.AddressLine1 + " " + rec.City),
                                                                       new XElement("ContactPersonName", rec.Name == null ? "" : rec.CompanyName)
                                                     ));




            ds.ReadXml(QuotationHeadList.CreateReader());
            if (ds.Tables.Count <= 0)
            {
                ds.Tables.Add("Quotation");
            }

            return ds;
        }

        #endregion


        public DataSet GetAllRecords(string ObjectName, string ReferenceID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            DataSet ds = new DataSet();

            string[] strings = new string[] { };
            strings = ReferenceID.Split(',');
            long[] arrayIDs = strings.Select(x => long.Parse(x)).ToArray();

            var ListofID = from a in arrayIDs.AsEnumerable()
                           select new { NewID = a };

            XElement QuotationHeadList = new XElement("QuotationList", from a in ListofID
                                                                       join m in ce.v_GetAllQuotationForViewForReport.AsEnumerable() on a.NewID equals m.ID into NewQuotationList
                                                                       from rec in NewQuotationList.DefaultIfEmpty()
                                                                       where a.NewID == rec.ID
                                                                       select new XElement("QuotationLst",
                                                                       new XElement("ID", rec.ID == null ? 0 : rec.ID),
                                                                       new XElement("QuotationNo", rec.QuotationNo == null ? "0" : rec.QuotationNo),
                                                                           //new XElement("QuotationDate", rec.QuotationDate == null ? DateTime.Now : rec.QuotationDate),
                                                                       new XElement("QuotationStatus", rec.QuotationStatus == null ? "" : rec.QuotationStatus),
                                                                       new XElement("QuotationValidityDays", rec.QuotationValidityDays == null ? 0 : rec.QuotationValidityDays),
                                                                           // new XElement("ExpectedOrderDate", rec.ExpectedOrderDate == null ? DateTime.Now : rec.ExpectedOrderDate),
                                                                       new XElement("ExpOrderAmount", rec.ExpOrderAmount == null ? 0 : rec.ExpOrderAmount),
                                                                       new XElement("LeadSource", rec.LeadSource == null ? "" : rec.LeadSource),
                                                                       new XElement("OtherCharges", rec.OtherCharges == null ? 0 : rec.OtherCharges),
                                                                       new XElement("ProductLevelTotalDiscount", rec.ProductLevelTotalDiscount == null ? 0 : rec.ProductLevelTotalDiscount),
                                                                       new XElement("Sector", rec.Sector == null ? "" : rec.Sector),
                                                                       new XElement("ShippingCharges", rec.ShippingCharges == null ? 0 : rec.ShippingCharges),
                                                                       new XElement("TotalAfterDiscount", rec.TotalAfterDiscount == null ? 0 : rec.TotalAfterDiscount),
                                                                       new XElement("TotalAmount", rec.TotalAmount == null ? 0 : rec.TotalAmount),
                                                                       new XElement("TotalDiscount", rec.TotalDiscount == null ? 0 : rec.TotalDiscount),
                                                                       new XElement("TotalTax", rec.TotalTax == null ? 0 : rec.TotalTax),
                                                                       new XElement("DiscountOnSubTotal", rec.DiscountOnSubTotal == null ? 0 : rec.DiscountOnSubTotal),
                                                                       new XElement("CustomerName", rec.CustomerName == null ? "" : rec.CustomerName),
                                                                       new XElement("DiscountOnSubTotal", rec.DiscountOnSubTotal == null ? 0 : rec.DiscountOnSubTotal),
                                                                       new XElement("EmailID", rec.EmailID == null ? "" : rec.EmailID),
                                                                       new XElement("AddressLine1", rec.AddressLine1 == null ? "" : rec.AddressLine1 + " " + rec.City),
                                                                       new XElement("ContactPersonName", rec.Name == null ? "" : rec.CompanyName),
                                                                       new XElement("ProductID", rec.ProductID == null ? 0 : rec.ProductID),
                                                                        new XElement("ProductCode", rec.ProductCode == null ? "0" : rec.ProductCode),
                                                                        new XElement("ProductName", rec.ProductName == null ? "None" : rec.ProductName),
                                                                        new XElement("ProductDescription", rec.ProductDescription == null ? "None" : rec.ProductDescription),
                                                                        new XElement("UOMID", rec.UOMID == null ? 0 : rec.UOMID),
                                                                        new XElement("UOM", rec.UOM == null ? "0" : rec.UOM),
                                                                        new XElement("ProductPrice", rec.ProductPrice == null ? 0 : rec.ProductPrice),
                                                                        new XElement("PerUnitDiscount", rec.PerUnitDiscount == null ? 0 : rec.PerUnitDiscount),
                                                                        new XElement("IsDiscountPercent", rec.IsDiscountPercent == null ? false : rec.IsDiscountPercent),
                                                                        new XElement("DiscountID", rec.DiscountID == null ? 0 : rec.DiscountID),
                                                                        new XElement("RateAfterDiscount", rec.RateAfterDiscount == null ? 0 : rec.RateAfterDiscount),
                                                                        new XElement("Quantity", rec.Quantity == null ? 0 : rec.Quantity),
                                                                        new XElement("AmountAfterDiscount", rec.AmountAfterDiscount == null ? 0 : rec.AmountAfterDiscount),
                                                                        new XElement("TotalTaxAmount", rec.TotalTaxAmount == null ? 0 : rec.TotalTaxAmount),
                                                                        new XElement("AmountAfterTax", rec.AmountAfterTax == null ? 0 : rec.AmountAfterTax),
                                                                        new XElement("Remark", rec.Remark == null ? "None" : rec.Remark),
                                                                        new XElement("TermAndCondition", rec.Term == null ? "None" : rec.Term + "-" + rec.Condition)
                                                      ));





            ds.ReadXml(QuotationHeadList.CreateReader());
            if (ds.Tables.Count <= 0)
            {
                ds.Tables.Add("Quotation");
            }

            return ds;
        }

        public DataSet GridBind(string ObjectName, string ReferenceID, string[] conn)
        {
            DataSet ds = new DataSet();
            ds.Reset();
            //string str = "select * from v_GetAllQuotationForReport where id in(" + ReferenceID + ")  select * from  tAddToCartProductDetail where ObjectName= '" + ObjectName + "' and ReferenceID in(" + ReferenceID + ")";
            string str = "select * from v_GetAllQuotationForReport where id in(" + ReferenceID + ")";
            ds = fillds(str, conn);

            return ds;

        }

        public DataSet GridBindForSalesOrder(string ObjectName, string ReferenceID, string[] conn)
        {
            DataSet ds = new DataSet();
            ds.Reset();
            //string str = "select * from v_GetAllSalesOrderForViewReport where id in(" + ReferenceID + ")  select * from  tAddToCartProductDetail where ObjectName= '" + ObjectName + "' and ReferenceID in(" + ReferenceID + ")";
            string str = "select * from v_GetAllSalesOrderForReport where id in(" + ReferenceID + ")";
            ds = fillds(str, conn);
            return ds;

        }

        public DataSet GridBindForInvoice(string ObjectName, string ReferenceID, string[] conn)
        {
            DataSet ds = new DataSet();
            ds.Reset();
            string str = "select * from v_GetAllInvoiceForReport where id in(" + ReferenceID + ")";
            ds = fillds(str, conn);

            return ds;

        }

        protected DataSet fillds(String strquery, string[] conn)
        {
            DataSet ds = new DataSet();
            System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection("Data Source=" + conn[0] + ";Initial Catalog=" + conn[1] + "; User ID=" + conn[3] + "; Password=" + conn[2] + ";");
            SqlDataAdapter da = new SqlDataAdapter(strquery, sqlConn);
            ds.Reset();
            da.Fill(ds);
            return ds;

        }
        //public List<v_GetAllQuotationForViewForReport > GetInvReportParaDetails1(string[] conn)
        //{
        //    BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
        //    List<v_GetAllQuotationForViewForReport> QuotationList = new List<v_GetAllQuotationForViewForReport>();

        //    QuotationList= (from QuotList in ce.v_GetAllQuotationForViewForReport
        //                where QuotList.ID > 0
        //               select new v_GetAllQuotationForViewForReport
        //               {
        //                   ID = QuotList.ID,
        //                   CustomerName = QuotList.CustomerName,
        //                   TotalAmount = QuotList.TotalAmount,
        //                   QuotationNo = QuotList.QuotationNo 
        //              }).ToList();

        //    return QuotationList;
        //}

        public DataSet GetReportParaDetails(string[] conn, string objName)
        {
            string sql;
            sql = GetSqlForListReport(objName);
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            DataSet ds = new DataSet();
            ds = svr.FillDataSet(sql, conn);
            return ds;
        }

        private string GetSqlForListReport(string ObjName)
        {
            string bl_Sql;
            bl_Sql = "";
            switch (ObjName)
            {
                case "Lead":
                    bl_Sql = "";
                    //bl_Sql = "Select as DocNo, as DocDate, as AccountName, as Amount, as RefDocNo, as ColseDate from v_GetAllLeadDetailsForReport";
                    break;
                case "Opportunity":
                    bl_Sql = "";
                    //bl_Sql = "Select as DocNo, as DocDate, as AccountName, as Amount, as RefDocNo, as ColseDate from v_GetAllOpportunutyForReport";
                    break;
                case "Quotation":
                    bl_Sql = "Select QuotationNo as DocNo,QuotationDate as DocDate,CustomerName as AccountName,TotalAmount as Amount,ReferenceID as RefDocNo,ExpectedOrderDate as ColseDate from v_GetAllQuotationForViewForReport";
                    //bl_Sql = "Select as DocNo, as DocDate, as AccountName, as Amount, as RefDocNo, as ColseDate from v_GetAllQuotationForReport";
                    break;
                case "Sales Order":
                    bl_Sql = "Select as DocNo, as DocDate, as AccountName, as Amount, as RefDocNo, as ColseDate from v_GetAllSalesOrderForViewReport";
                    break;
                case "Invoice":
                    bl_Sql = "Select as DocNo, as DocDate, as AccountName, as Amount, as RefDocNo, as ColseDate from v_GetAllInvoiceForReport";
                    break;
            }
            return bl_Sql;
        }

    }
}
