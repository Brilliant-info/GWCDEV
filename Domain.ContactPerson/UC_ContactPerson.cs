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
using Domain.Tempdata;
using System.Xml.Linq;
using System.Data.Objects;
using Domain.Server;

namespace Domain.ContactPerson
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]

    public partial class UC_ContactPerson : Interface.ContactPerson.iUC_ContactPerson
    {
        Domain.Server.Server svr = new Server.Server();

        DataHelper datahelper = new DataHelper();

        #region GetContactPersonByReferenceId()
        /// <summary>
        /// GetContactPersonByReferenceId Is The Method To Get ContactPerson By Reference ID
        /// </summary>
        /// <param name="objectName"></param>
        /// <param name="ReferenceID"></param>
        /// <returns></returns>

        public List<SP_GetContactPersonsByReferenceID_Result> GetContactPersonByReferenceId(string saveobjectName, long ReferenceID, string paraSessionID, string paraUserID, string SenderObject, long SenderID, string currentformid, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetContactPersonsByReferenceID_Result> ContactPerson = new List<SP_GetContactPersonsByReferenceID_Result>();
            
            ContactPerson = (from p in ce.SP_GetContactPersonsByReferenceID(saveobjectName, ReferenceID, SenderObject, SenderID)
                             select p).ToList();

            if (ContactPerson.Count > 0)
            {
                SaveTempDataToDB(ContactPerson, paraSessionID, paraUserID, currentformid, conn);
            }
            return ContactPerson;
        }

        #endregion

        #region GetExistingTempDataBySessionIDFormID

        /// <summary>
        /// Tis Is Method To Get Existing Record From tContactPersonDetail and tempdata and merge it
        /// </summary>
        /// <param name="paraSessionID"></param>
        /// <param name="paraUserID"></param>
        /// <param name="paraObjectName"></param>
        /// <returns></returns>
        protected List<SP_GetContactPersonsByReferenceID_Result> GetExistingTempDataBySessionIDFormID(string paraSessionID, string paraUserID, string currentformid, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetContactPersonsByReferenceID_Result> ObjcontactPersonList = new List<SP_GetContactPersonsByReferenceID_Result>();
            TempData tempdata = new TempData();
            SP_GetContactPersonsByReferenceID_Result tcontact = new SP_GetContactPersonsByReferenceID_Result();
            tempdata = (from temp in ce.TempDatas
                        where temp.SessionID == paraSessionID
                        && temp.ObjectName == currentformid
                        && temp.UserID == paraUserID

                        select temp).FirstOrDefault();

            if (tempdata != null)
            {
                ObjcontactPersonList = datahelper.DeserializeEntity1<SP_GetContactPersonsByReferenceID_Result>(tempdata.Data);
            }
            return ObjcontactPersonList;
        }
        #endregion

        #region InsertIntoTemp
        /// <summary>
        /// This Is Method ToInsert Record In to TempData table
        /// </summary>
        /// <param name="ContactPerson"></param>
        /// <param name="paraSessionID"></param>
        /// <param name="paraUserID"></param>
        /// <param name="paraObjectName"></param>
        /// <returns></returns>

        public List<SP_GetContactPersonsByReferenceID_Result> InsertIntoTemp(SP_GetContactPersonsByReferenceID_Result ContactPerson, string paraSessionID, string paraUserID, string currentformid, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            /*Begin : Get Existing Records from TempData*/
            List<SP_GetContactPersonsByReferenceID_Result> existingContactPersonList = new List<SP_GetContactPersonsByReferenceID_Result>();
            existingContactPersonList = GetExistingTempDataBySessionIDFormID(paraSessionID, paraUserID, currentformid, conn);
            /*End*/

            /*Begin : Merge (Existing + Newly Added) Products to Create TempData of AddToCart*/
            List<SP_GetContactPersonsByReferenceID_Result> mergedContactPersonList = new List<SP_GetContactPersonsByReferenceID_Result>();
            mergedContactPersonList.AddRange(existingContactPersonList);
            ContactPerson.Sequence = existingContactPersonList.Count + 1;
            mergedContactPersonList.Add(ContactPerson);
            /*End*/


            /*Begin : Serialize & Save MergedAddToCartList*/
            SaveTempDataToDB(mergedContactPersonList, paraSessionID, paraUserID, currentformid, conn);
            /*End*/

            return mergedContactPersonList;
        }
        #endregion

        #region SaveTempDataToDB

        /// <summary>
        /// This Is For Insert Temporory Data From TempData In to 'tContactPersonDetail' table
        /// </summary>
        /// <param name="paraobjList"></param>
        /// <param name="paraSessionID"></param>
        /// <param name="paraUserID"></param>
        /// <param name="paraObjectName"></param>
        /// 

        protected void SaveTempDataToDB(List<SP_GetContactPersonsByReferenceID_Result> paraobjList, string paraSessionID, string paraUserID, string currentformid, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));

            /*Begin : Remove Existing Records*/
            ClearTempDataFromDB(paraSessionID, paraUserID, currentformid, conn);
            /*End*/

            /*Begin : Serialize MergedAddToCartList*/
            string xml = "";
            xml = datahelper.SerializeEntity(paraobjList);
            /*End*/

            /*Begin : Save Serialized List into TempData */
            TempData tempdata = new TempData();
            tempdata.Data = xml;
            tempdata.XmlData = "";
            tempdata.LastUpdated = DateTime.Now;
            tempdata.SessionID = paraSessionID.ToString();
            tempdata.UserID = paraUserID.ToString();
            tempdata.ObjectName = currentformid.ToString();
            tempdata.TableName = "-";
            ce.AddToTempDatas(tempdata);
            ce.SaveChanges();
            /*End*/

        }

        #endregion

        #region ClearTempDataFromDB

        /// <summary>
        /// ClearTempDataFromDB is the method to clear Records From TempData Table
        /// </summary>
        /// <param name="paraSessionID"></param>
        /// <param name="paraUserID"></param>
        /// <param name="paraObjectName"></param>
        public void ClearTempDataFromDB(string paraSessionID, string paraUserID, string currentformid, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            TempData tempdata = new TempData();
            tempdata = (from rec in ce.TempDatas
                        where rec.SessionID == paraSessionID
                        && rec.UserID == paraUserID
                        && rec.ObjectName == currentformid
                        select rec).FirstOrDefault();
            if (tempdata != null) { ce.DeleteObject(tempdata); ce.SaveChanges(); }



        }

        #endregion

        #region  SetValuesToTempData

        public List<SP_GetContactPersonsByReferenceID_Result> SetValuesToTempData_onChange(string paraSessionID, string paraUserID, string currentformid, int paraSequence, SP_GetContactPersonsByReferenceID_Result paraInput, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetContactPersonsByReferenceID_Result> existingList = new List<SP_GetContactPersonsByReferenceID_Result>();
            existingList = GetExistingTempDataBySessionIDFormID(paraSessionID, paraUserID, currentformid, conn);
            SP_GetContactPersonsByReferenceID_Result editRow = new SP_GetContactPersonsByReferenceID_Result();
            editRow = (from exist in existingList
                       where exist.Sequence == paraSequence
                       select exist).FirstOrDefault();
            editRow = paraInput;
            existingList = existingList.Where(e => e.Sequence != paraSequence).ToList();
            existingList.Add(editRow);
            existingList = (from e in existingList
                            orderby e.Sequence
                            select e).ToList();
            SaveTempDataToDB(existingList, paraSessionID, paraUserID, currentformid, conn);
            return existingList;

        }

        #endregion

        #region FinalSaveToDBtAddToContactPerson

        public void FinalSaveToDBtAddToContactPerson(string paraSessionID, string currentformid, long paraReferenceID, string paraUserID, string paraSaveObjectName, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetContactPersonsByReferenceID_Result> finalSaveLst = new List<SP_GetContactPersonsByReferenceID_Result>();
            finalSaveLst = GetExistingTempDataBySessionIDFormID(paraSessionID, paraUserID, currentformid, conn);
            if (finalSaveLst.Count > 0)
            {
                XElement xmlEle = new XElement("AddToContactList", from rec in finalSaveLst
                                                                   select new XElement("ContactList",
                                                                   new XElement("ObjectName", paraSaveObjectName),
                                                                   new XElement("ReferenceID", rec.ReferenceID),
                                                                   new XElement("CustomerHeadID", rec.CustomerHeadID),
                                                                   new XElement("Sequence", rec.Sequence),
                                                                   new XElement("Name", rec.Name),
                                                                   new XElement("Department", rec.Department),
                                                                   new XElement("Designation", rec.Designation),
                                                                   new XElement("EmailID", rec.EmailID),
                                                                   new XElement("OfficeNo", rec.OfficeNo),
                                                                   new XElement("MobileNo", rec.MobileNo),
                                                                   new XElement("ContactTypeID", rec.ContactTypeID),
                                                                   new XElement("InterestedIn", rec.InterestedIn),
                                                                   new XElement("Hobbies", rec.Hobbies),
                                                                   new XElement("FacebookID", rec.FacebookID),
                                                                   new XElement("FacebookPassword", rec.FacebookPassword),
                                                                   new XElement("TwitterID", rec.TwitterID),
                                                                   new XElement("TwitterPassword", rec.TwitterPassword),
                                                                   new XElement("OtherID", rec.OtherID),
                                                                   new XElement("HighestQualification", rec.HighestQualification),
                                                                   new XElement("CollegeOrUniversity", rec.CollegeOrUniversity),
                                                                   new XElement("HighSchool", rec.HighSchool),
                                                                   new XElement("Remark", rec.Remark),
                                                                   new XElement("Active", rec.Active),
                                                                   new XElement("CreatedBy", rec.CreatedBy),
                                                                   new XElement("CreationDate", rec.CreationDate),
                                                                   new XElement("LastModifiedBy", rec.LastModifiedBy),
                                                                   new XElement("LastModifiedDate", rec.LastModifiedDate),
                                                                   new XElement("CompanyID", rec.CompanyID),
                                                                   new XElement("Remark", rec.Remark)
                                                                         )
                                                               );

                TempData tempdata = new TempData();
                tempdata = (from t in ce.TempDatas
                            where t.ObjectName == currentformid && t.SessionID == paraSessionID && t.UserID == paraUserID
                            select t).FirstOrDefault();
                tempdata.XmlData = xmlEle.ToString();


                ce.ObjectStateManager.ChangeObjectState(tempdata, EntityState.Modified);
                ce.SaveChanges();
                //ClearTempDataFromDB(paraSessionID, paraUserID, paraObjectName);
            }


            ObjectParameter _paraSessionID = new ObjectParameter("paraSessionID", typeof(string));
            _paraSessionID.Value = paraSessionID;

            ObjectParameter _paraSaveObjectName = new ObjectParameter("paraSaveObjectName", typeof(string));
            _paraSaveObjectName.Value = paraSaveObjectName;

            ObjectParameter _paraReferenceID = new ObjectParameter("paraReferenceID", typeof(long));
            _paraReferenceID.Value = paraReferenceID;

            ObjectParameter _paraUserID = new ObjectParameter("paraUserID", typeof(string));
            _paraUserID.Value = paraUserID;

            ObjectParameter _currentformid = new ObjectParameter("paraCurrentFormID", typeof(string));
            _currentformid.Value = currentformid;

            ObjectParameter[] obj = new ObjectParameter[] { _paraSessionID, _paraSaveObjectName, _paraReferenceID, _paraUserID, _currentformid };
            ce.ExecuteFunction("SP_InsertIntotContactPersonDetail", obj);
            ce.SaveChanges();

            ClearTempDataFromDB(paraSessionID, paraUserID, paraSaveObjectName, conn);
        }

        #endregion

        #region GetContactDetailFromTempTableBySequence()

        public SP_GetContactPersonsByReferenceID_Result GetContactDetailFromTempTableBySequence(string paraSessionID, string paraUserID, string currentFormID, int paraSequence, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            /*Begin : Get Existing Records from TempData*/
            List<SP_GetContactPersonsByReferenceID_Result> existingAddToCartList = new List<SP_GetContactPersonsByReferenceID_Result>();
            existingAddToCartList = GetExistingTempDataBySessionIDFormID(paraSessionID, paraUserID, currentFormID, conn);
            /*End*/

            /*Get Filter List [Filter By paraSequence]*/
            SP_GetContactPersonsByReferenceID_Result filterList = new SP_GetContactPersonsByReferenceID_Result();
            filterList = (from exist in existingAddToCartList
                          where exist.Sequence == paraSequence
                          select exist).FirstOrDefault();
            return filterList;
        }

        #endregion

        public string GetContactPersonByName(string paraSessionID, string Name, string ContactType, string paraUserID, string currentFormID, string[] conn)
        {
            string Result = "";
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            /*Begin : Get Existing Records from TempData*/
            List<SP_GetContactPersonsByReferenceID_Result> existingAddToCartList = new List<SP_GetContactPersonsByReferenceID_Result>();
            existingAddToCartList = GetExistingTempDataBySessionIDFormID(paraSessionID, paraUserID, currentFormID, conn);
            /*End*/

            /*Get Filter List [Filter By paraSequence]*/
            SP_GetContactPersonsByReferenceID_Result filterList = new SP_GetContactPersonsByReferenceID_Result();
            filterList = (from exist in existingAddToCartList
                          where exist.Name == Name && exist.ContactType == ContactType
                          select exist).FirstOrDefault();

            if (filterList == null)
            {

            }
            else
            {
                Result = "Same Contact Name Already Exist";
            }


            return Result;
        }

    }


}
