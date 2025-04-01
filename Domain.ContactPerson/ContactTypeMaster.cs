using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Activation;
using System.Configuration;
using Interface.ContactPerson;
using System.ServiceModel;
using System.Xml.Linq;
using Domain.Server;

namespace Domain.ContactPerson
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]

     public partial class ContactTypeMaster : Interface.ContactPerson.iContactTypeMaster
     {
         Domain.Server.Server svr = new Server.Server();
       

        #region GetContactTypeList 
        /// <summary>
        /// GetContactTypeList is providing List of ContactType
        /// </summary>
        /// <returns></returns>
        /// 
         public List<mContactType> GetContactTypeList(string[] conn)
         {
             BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mContactType> ContactType = new List<mContactType>();
            ContactType = (from p in ce.mContactTypes
                           where p.Active=="Y"
                           orderby p.Sequence
                           select p).ToList();   
            return ContactType;

        }
        #endregion

        #region InsertmContactType
         public int InsertmContactType(mContactType ct, string[] conn)
         {
             BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.mContactTypes.AddObject(ct);
            ce.SaveChanges();
            return 1;
        }
        #endregion

        #region UpdatemContactType
         public int UpdatemContactType(mContactType updatect, string[] conn)
         {
             BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.mContactTypes.Attach(updatect);
            ce.ObjectStateManager.ChangeObjectState(updatect, EntityState.Modified);
            ce.SaveChanges();
            return 1;
        }
        #endregion

        #region GetContactTypeListByID
         /// <summary>
         /// GetLeadSourceList is providing List of LeadSource
         /// </summary>
         /// <returns></returns>
         /// 

         public mContactType GetContactTypeListByID(int ContactTypeId, string[] conn)
         {
             BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            mContactType ContactType1 = new mContactType();
            ContactType1 = (from p in ce.mContactTypes
                            where p.ID == ContactTypeId
                            select p).FirstOrDefault();
            ce.Detach(ContactType1);
             return ContactType1;
         }
         #endregion

        #region checkDuplicateRecord
         /// <summary>
         /// checkDuplicateRecord is providing List of ContactType by ContactType 
         /// </summary>
         /// <returns></returns>
         /// 
         public string checkDuplicateRecord(string ContactTypeName, string[] conn)
         {
             BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
             string result = "";
             var output = (from p in ce.mContactTypes
                           where p.ContactType == ContactTypeName
                           select new { p.ContactType }).FirstOrDefault();

             if (output != null)
             {
                 result = "[ " + ContactTypeName + " ] Contact Type Name allready exist";
             }
             return result;
         }
         #endregion

        #region checkDuplicateRecordEdit
         /// <summary>
         /// checkDuplicateRecord is providing List of ContactType by ContactType and ID
         /// </summary>
         /// <returns></returns>
         /// 
         public string checkDuplicateRecordEdit(int ContactTypeID, string ContactTypeName, string[] conn)
         {
             BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
             string result = "";
             var output = (from p in ce.mContactTypes
                           where p.ContactType == ContactTypeName && p.ID != ContactTypeID
                           select new { p.ContactType }).FirstOrDefault();
             if (output != null)
             {
                 result = "[ " + ContactTypeName + " ] Contact Type Name allready exist";
             }
             return result;
         }
         #endregion

     

        #region GetContactTypeToBind
         /// <summary>
         /// GetContactTypeRecordToBindGrid is providing List of ContactType for bind grid with Actine Yes/No
         /// </summary>
         /// <returns></returns>
         /// 
         public DataSet GetContactTypeToBind(string[] conn)
         {
             BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
             List<mContactType> ContactType = new List<mContactType>();
             XElement xmlContactTypeMaster = new XElement("ContactTypeList", from m in ce.mContactTypes.AsEnumerable()
                                                                           orderby m.Sequence
                                                                             select new XElement("Contact",
                                                                           new XElement("ID", m.ID),
                                                                           new XElement("ContactType", m.ContactType),
                                                                           new XElement("Sequence", m.Sequence),
                                                                           new XElement("Remark", m.Remark),
                                                                           new XElement("Active", m.Active == "Y" ? "Yes" : "No")
                                                                           ));
             DataSet ds = new DataSet();
             ds.ReadXml(xmlContactTypeMaster.CreateReader());
             DataTable dt = new DataTable();
             if (ds.Tables.Count <= 0)
             {
                 dt = ds.Tables.Add("Contact1");
             }
             return ds;
         }
         #endregion
    }
}
