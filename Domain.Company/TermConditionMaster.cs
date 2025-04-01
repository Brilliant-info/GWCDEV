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

namespace Domain.Company
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]

    public partial class TermConditionMaster : Interface.Company.iTermConditionMaster
    {
        Domain.Server.Server svr = new Server.Server();
        //BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities();

        #region GetTermConditionList
        /// <summary>
        /// GetTermConditionList is providing List of TermCondition
        /// </summary>
        /// <returns></returns>
        /// 
        public List<mTermsCondition> GetTermConditionList(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mTermsCondition> Designation = new List<mTermsCondition>();
            Designation = (from p in ce.mTermsConditions
                           orderby p.Sequence
                           select p).ToList();
            return Designation;
        }
        #endregion

        #region InsertmTermsCondition
        public int InsertmTermsCondition(mTermsCondition term, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.mTermsConditions.AddObject(term);
            ce.SaveChanges();
            return 1;
        }
        #endregion

        #region updatemTermsCondition
        public int updatemTermsCondition(mTermsCondition updateTerm, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.mTermsConditions.Attach(updateTerm);
            ce.ObjectStateManager.ChangeObjectState(updateTerm, EntityState.Modified);
            ce.SaveChanges();
            return 1;
        }
        #endregion

        #region GetTermConditionListByID
        /// <summary>
        /// GetTermConditionListByID is providing List of TermCondition By ID
        /// </summary>
        /// <returns></returns>
        /// 
        public mTermsCondition GetTermConditionListByID(int termId, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            mTermsCondition DesignationID = new mTermsCondition();
            DesignationID = (from p in ce.mTermsConditions
                             where p.ID == termId
                             select p).FirstOrDefault();
            ce.Detach(DesignationID);
            return DesignationID;

        }
        #endregion

        #region checkDuplicateRecord
        /// <summary>
        /// checkDuplicateRecord is providing List of TermCondition by TermName GroupID
        /// </summary>
        /// <returns></returns>
        /// 
        public string checkDuplicateRecord(string termName, int groupID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            var output = (from p in ce.mTermsConditions
                          where p.Term == termName && p.TCGroupID == groupID
                          select new { p.Term }).FirstOrDefault();

            if (output != null)
            {
                result = "[ " + termName + " ]  Term for the Group allready exist";
            }
            return result;
        }
        #endregion

        #region checkDuplicateRecordEdit
        /// <summary>
        /// checkDuplicateRecord for Edit is providing List of TermCondition by TermName and TermID GroupID
        /// </summary>
        /// <returns></returns>
        /// 
        public string checkDuplicateRecordEdit(int termID, string termName, int groupID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            var output = (from p in ce.mTermsConditions
                          where p.Term == termName && p.ID != termID && p.TCGroupID == groupID
                          select new { p.Term }).FirstOrDefault();

            if (output != null)
            {
                result = "[ " + termName + " ]  Term for the Group allready exist";
            }
            return result;
        }
        #endregion

        #region GetTermRecordToBindGrid
        /// <summary>
        /// GetTermRecordToBindGrid is providing List of Term for bind grid with Actine Yes/No
        /// </summary>
        /// <returns></returns>
        /// 
        public DataSet GetTermRecordToBindGrid(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<vTermCondition> LeadSource = new List<vTermCondition>();
            XElement xmlTermConditionMaster = new XElement("TermList", from m in ce.vTermConditions.AsEnumerable()
                                                                            orderby m.Sequence
                                                                            select new XElement("TermCondition",
                                                                          new XElement("ID", m.ID),
                                                                          new XElement("GroupName", m.GroupName),
                                                                          new XElement("Term", m.Term),
                                                                          new XElement("Condition", m.Condition),
                                                                          new XElement("Sequence", m.Sequence),
                                                                          new XElement("Active", m.Active == "Y" ? "Yes" : "No")
                                                                          ));
            DataSet ds = new DataSet();
            ds.ReadXml(xmlTermConditionMaster.CreateReader());
            //DataTable dt = new DataTable();
            if (ds.Tables.Count <= 0)
            {
                ds.Tables.Add("TermCondition1");
            }
            return ds;
        }
        #endregion

        #region GetGroupListToBindDDL
        /// <summary>
        /// GetGroupListToBindDDL is providing List of Group for bind DDLGroup
        /// </summary>
        /// <returns></returns>
        /// 
        public List<mTermsConditionsGroup> GetGroupListToBindDDL(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mTermsConditionsGroup> Group = new List<mTermsConditionsGroup>();
            Group = (from p in ce.mTermsConditionsGroups
                           orderby p.Sequence
                           select p).ToList();
            return Group;
        }
        #endregion

        #region GetTermRecordToBindGridUC
        /// <summary>
        /// GetTermRecordToBindGridUC is providing List of Term for bind grid with Actine Yes/No
        /// </summary>
        /// <returns></returns>
        /// 
        public DataSet GetTermRecordToBindGridUC(string groupname, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<vTermCondition> TermsCondition = new List<vTermCondition>();
            XElement xmlTermConditionMaster = new XElement("TermList", from m in ce.vTermConditions.AsEnumerable()
                                                                       where m.GroupName==groupname && m.Active=="Y"
                                                                       orderby m.Sequence
                                                                       select new XElement("TermCondition",
                                                                     new XElement("ID", m.ID),
                                                                     new XElement("TCGroupID", m.TCGroupID),
                                                                     new XElement("GroupName", m.GroupName),
                                                                     new XElement("Term", m.Term),
                                                                     new XElement("Condition", m.Condition),
                                                                     new XElement("Sequence", m.Sequence),
                                                                     new XElement("Active", m.Active == "Y" ? "Yes" : "No")
                                                                     ));
            DataSet ds = new DataSet();
            ds.ReadXml(xmlTermConditionMaster.CreateReader());
            DataTable dt = new DataTable();
            if (ds.Tables.Count <= 0)
            {
                dt = ds.Tables.Add("TermCondition1");
            }
            return ds;
        }
        #endregion
    }
}
