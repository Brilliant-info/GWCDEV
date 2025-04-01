using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Activation;
using System.Configuration;
using System.ServiceModel;
using System.Xml.Linq;
using Domain.Tempdata;
using System;
using System.Data.Objects;
using System.Data;
using Domain.Server;
using Interface.Tax;
namespace Domain.Tax
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]

    public partial class UC_StatutoryInfo : Interface.Tax.iUC_StatutoryInfo
    {
        DataHelper datahelper = new DataHelper();
        Domain.Server.Server svr = new Server.Server();

        #region GetStatutoryListToBind()
        /// <summary>
        /// 
        /// Get Records From SP_GetStatutoryDetails_Result By ObjectName RferenceID
        /// </summary>
        /// <param name="paraReferenceID"></param>
        /// <param name="paraUserID"></param>
        /// <param name="ParaObjectName"></param>
        /// <param name="ParaCompanyID"></param>
        /// <returns></returns>
        public List<SP_GetStatutoryDetails_Result> GetStatutoryListToBind(long paraReferenceID, string paraUserID, string ParaObjectName, long ParaCompanyID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetStatutoryDetails_Result> StatutoryList = new List<SP_GetStatutoryDetails_Result>();
            StatutoryList = (from view in db.SP_GetStatutoryDetails(paraReferenceID, paraUserID, ParaObjectName, ParaCompanyID)
                             select view).ToList();

            return StatutoryList;
        }

        #endregion

        #region FinalSaveToTStatutoryDetails
        /// <summary>
        /// Save Record To tStatutryDetails
        /// </summary>
        /// <param name="ObjStatutory"></param>
        /// <param name="paraObjectName"></param>
        /// <param name="paraReferenceID"></param>
        /// <param name="paraUserID"></param>
        /// <param name="paraCompanyID"></param>
        public void FinalSaveToTStatutoryDetails(List<tStatutoryDetail> ObjStatutory, string paraObjectName, long paraReferenceID, string paraUserID, long paraCompanyID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            if (ObjStatutory.Count > 0)
            {
                XElement xmlEleStatutory = new XElement("StatutoryList", from rec in ObjStatutory
                                                                         select new XElement("Statutory",
                                                                         new XElement("ObjectName", paraObjectName),
                                                                         new XElement("ReferenceID", paraReferenceID),
                                                                         new XElement("StatutoryID", rec.StatutoryID),
                                                                         new XElement("StatutoryValue", rec.StatutoryValue),
                                                                         new XElement("Active", rec.Active),
                                                                         new XElement("CreatedBy", paraUserID),
                                                                         new XElement("CreatedDate", rec.CreatedDate),
                                                                         new XElement("LastEditBy", paraUserID),
                                                                         new XElement("LastEditDate", rec.LastEditDate),
                                                                         new XElement("CompanyID", rec.CompanyID)
                                                                         ));

                ObjectParameter _paraXML = new ObjectParameter("xmlData", typeof(string));
                _paraXML.Value = xmlEleStatutory.ToString();

                ObjectParameter _paraObjectName = new ObjectParameter("paraObjectName", typeof(string));
                _paraObjectName.Value = paraObjectName;

                ObjectParameter _paraReferenceID = new ObjectParameter("paraReferenceID", typeof(string));
                _paraReferenceID.Value = paraReferenceID;

                ObjectParameter _paraUserID = new ObjectParameter("paraUserID", typeof(string));
                _paraUserID.Value = paraUserID;

                ObjectParameter _paraCompanyID = new ObjectParameter("paraCompanyID", typeof(string));
                _paraCompanyID.Value = paraCompanyID;

                ObjectParameter[] obj = new ObjectParameter[] { _paraXML, _paraObjectName, _paraReferenceID, _paraUserID, _paraCompanyID };
                db.ExecuteFunction("SP_InsertIntotStatutoryDetails", obj);
                db.SaveChanges();


            }




        }

        #endregion
    }
}
