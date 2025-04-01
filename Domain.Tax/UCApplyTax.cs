using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Activation;
using System.Configuration;
using System.ServiceModel;
using System.Xml.Linq;
using System.Collections;
using System.Data.Objects;
using Domain.Server;
using Interface.Tax;
namespace Domain.Tax
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public partial class UCApplyTax : Interface.Tax.iUCApplyTax
    {
        Domain.Server.Server svr = new Server.Server();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="CurrentObjectName"></param>
        /// <param name="SessionID"></param>
        /// <param name="CartSequence"></param>
        /// <returns></returns>
        public List<TempCartProductLevelTaxDetail> GetTaxListBySequence(string CurrentObjectName, string SessionID, long CartSequence, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<TempCartProductLevelTaxDetail> result = new List<TempCartProductLevelTaxDetail>();
            result = (from k in db.TempCartProductLevelTaxDetails
                      where k.SessionID == SessionID && k.ProductDetailSequence == CartSequence && k.CurrentForm == CurrentObjectName
                      orderby k.ParentTaxID, k.TaxID, k.TaxSequence
                      select k).ToList();
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CurrentObjectName"></param>
        /// <param name="SessionID"></param>
        /// <param name="CartSequence"></param>
        /// <param name="TaxableAmount"></param>
        /// <param name="SelectedTaxIDs"></param>
        /// <returns></returns>
        public string UpdateCalculatedTaxList(string CurrentObjectName, string SessionID, long CartSequence, decimal TaxableAmount, string SelectedTaxIDs, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));

            ObjectParameter _CurrentObjectName = new ObjectParameter("CurrentObjectName", typeof(string));
            _CurrentObjectName.Value = CurrentObjectName;

            ObjectParameter _SessionID = new ObjectParameter("SessionID", typeof(string));
            _SessionID.Value = SessionID;

            ObjectParameter _CartSequence = new ObjectParameter("CartSequence", typeof(long));
            _CartSequence.Value = CartSequence;

            ObjectParameter _TaxableAmount = new ObjectParameter("TaxableAmount", typeof(decimal));
            _TaxableAmount.Value = TaxableAmount;

            ObjectParameter _SelectedTaxIDs = new ObjectParameter("SelectedTaxIDs", typeof(string));
            _SelectedTaxIDs.Value = SelectedTaxIDs;

            ObjectParameter[] obj = new ObjectParameter[] { _CurrentObjectName, _SessionID, _CartSequence, _TaxableAmount, _SelectedTaxIDs };
            db.ExecuteFunction("SP_CartUpdateProductLevelTaxDetail", obj);
            db.SaveChanges();

            return GetCalculatedTaxAmount(GetTaxListBySequence(CurrentObjectName, SessionID, CartSequence, conn)).ToString();

        }

        public List<SP_CartBeforeUpdateProductLevelTaxDetail_Result> GetCalculatedTaxListBeforeUpdate(string CurrentObjectName, string SessionID, long CartSequence, decimal TaxableAmount, string SelectedTaxIDs, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_CartBeforeUpdateProductLevelTaxDetail_Result> result = new List<SP_CartBeforeUpdateProductLevelTaxDetail_Result>();
            result = (from sp in db.SP_CartBeforeUpdateProductLevelTaxDetail(CurrentObjectName, SessionID, CartSequence, TaxableAmount, SelectedTaxIDs)
                      select sp).ToList();

            return result;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="TaxDetailList"></param>
        /// <returns></returns>
        public Decimal GetCalculatedTaxAmount(List<TempCartProductLevelTaxDetail> TaxDetailList)
        {
            decimal TaxAmount = 0;
            if (TaxDetailList.Count > 0)
            {
                TaxAmount = Convert.ToDecimal((from lst in TaxDetailList
                                               select lst.TaxAmount).Sum());
            }
            return TaxAmount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CurrentObjectName"></param>
        /// <param name="ReferenceID"></param>
        /// <param name="SessionID"></param>
        /// <returns></returns>
        public bool FinalSaveProductLevelTax(string CurrentObjectName, long ReferenceID, string SessionID, string[] conn)
        {
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                ObjectParameter _CurrentObjectName = new ObjectParameter("CurrentObjectName", typeof(string));
                _CurrentObjectName.Value = CurrentObjectName;

                ObjectParameter _SessionID = new ObjectParameter("SessionID", typeof(string));
                _SessionID.Value = SessionID;

                ObjectParameter _ReferenceID = new ObjectParameter("ReferenceID", typeof(long));
                _ReferenceID.Value = ReferenceID;

                ObjectParameter[] obj = new ObjectParameter[] { _CurrentObjectName, _SessionID, _ReferenceID };
                db.ExecuteFunction("SP_CartInsertIntoProductLevelTaxDetail", obj);
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CurrentObjectName"></param>
        /// <param name="SessionID"></param>
        /// <param name="CartSequence"></param>
        public bool RemoveTaxDetailBySequence(string CurrentObjectName, string SessionID, long CartSequence, string[] conn)
        {
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                ObjectParameter _CurrentObjectName = new ObjectParameter("CurrentObjectName", typeof(string));
                _CurrentObjectName.Value = CurrentObjectName;

                ObjectParameter _SessionID = new ObjectParameter("SessionID", typeof(string));
                _SessionID.Value = SessionID;

                ObjectParameter _CartSequence = new ObjectParameter("CartSequence", typeof(long));
                _CartSequence.Value = CartSequence;

                ObjectParameter[] obj = new ObjectParameter[] { _CurrentObjectName, _SessionID, _CartSequence };
                db.ExecuteFunction("SP_CartDeleteProductLevelTaxDetail", obj);
                db.SaveChanges();
                return true;

            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CurrentObjectName"></param>
        /// <param name="SessionID"></param>
        public void ClearTempDataByCurrentObjectSessionID(string CurrentObjectName, string SessionID, string[] conn)
        {
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                ObjectParameter _CurrentObjectName = new ObjectParameter("CurrentObjectName", typeof(string));
                _CurrentObjectName.Value = CurrentObjectName;

                ObjectParameter _SessionID = new ObjectParameter("SessionID", typeof(string));
                _SessionID.Value = SessionID;

                ObjectParameter _CartSequence = new ObjectParameter("CartSequence", typeof(long));
                _CartSequence.Value = 0;

                ObjectParameter[] obj = new ObjectParameter[] { _CurrentObjectName, _SessionID, _CartSequence };
                db.ExecuteFunction("SP_CartDeleteProductLevelTaxDetail", obj);
                db.SaveChanges();

            }
            catch
            {

            }
        }

    }
}
