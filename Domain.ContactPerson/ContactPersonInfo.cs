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

    public partial class ContactPersonInfo : Interface.ContactPerson.iContactPersonInfo
    {

        Domain.Server.Server svr = new Server.Server();
        DataHelper datahelper = new DataHelper();

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
                           where p.Active == "Y"
                           orderby p.Sequence
                           select p).ToList();
            return ContactType;

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

        public void SaveTempDataToDB(List<SP_GetContactPersonListToBindGrid_Result> paraobjList, string paraSessionID, string paraUserID, string currentformid, string[] conn)
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

        public void SaveTempDataToDBLead(List<SP_GetContactPersonListToBindGridLead_Result> paraobjList, string paraSessionID, string paraUserID, string currentformid, string[] conn)
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

        #region InsertIntoTemp
        /// <summary>
        /// This Is Method ToInsert Record In to TempData table
        /// </summary>
        /// <param name="ContactPerson"></param>
        /// <param name="paraSessionID"></param>
        /// <param name="paraUserID"></param>
        /// <param name="paraObjectName"></param>
        /// <returns></returns>

        public List<SP_GetContactPersonListToBindGrid_Result> InsertIntoTemp(SP_GetContactPersonListToBindGrid_Result ContactPerson, string paraSessionID, string paraUserID, string currentformid, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            /*Begin : Get Existing Records from TempData*/
            List<SP_GetContactPersonListToBindGrid_Result> existingContactPersonList = new List<SP_GetContactPersonListToBindGrid_Result>();
            existingContactPersonList = GetExistingTempDataBySessionIDFormID(paraSessionID, paraUserID, currentformid, conn);
            /*End*/

            /*Begin : Merge (Existing + Newly Added) Products to Create TempData of AddToCart*/
            List<SP_GetContactPersonListToBindGrid_Result> mergedContactPersonList = new List<SP_GetContactPersonListToBindGrid_Result>();
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

        #region  SetValuesToTempData
        public void SetValuesToTempData_onChange(string SessionID, string UserID, string TargetObjectName, int Sequence, SP_GetContactPersonListToBindGrid_Result paraInput, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetContactPersonListToBindGrid_Result> existingList = new List<SP_GetContactPersonListToBindGrid_Result>();
            existingList = GetContactPersonTempData(TargetObjectName, SessionID, UserID, conn);

            List<SP_GetContactPersonListToBindGrid_Result> mergedAddToContactPersonList = new List<SP_GetContactPersonListToBindGrid_Result>();
            //mergedAddToContactPersonList.AddRange(existingList);
            //paraInput.Sequence = existingList.Count + 1;
            //mergedAddToContactPersonList.Add(paraInput);


            SP_GetContactPersonListToBindGrid_Result editRow = new SP_GetContactPersonListToBindGrid_Result();
            editRow = (from exist in existingList
                       where exist.Sequence == Sequence
                       select exist).FirstOrDefault();
            editRow = paraInput;
            existingList = existingList.Where(e => e.Sequence != Sequence).ToList();
            existingList.Add(editRow);
            existingList = (from e in existingList
                            orderby e.Sequence
                            select e).ToList();

            SaveTempDataToDB(existingList, SessionID, UserID, TargetObjectName, conn);

        }
        #endregion

        #region GetContactDetailFromTempTableBySequence()

        public SP_GetContactPersonListToBindGrid_Result GetContactDetailFromTempTableBySequence(string paraSessionID, string paraUserID, string currentFormID, int paraSequence, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            /*Begin : Get Existing Records from TempData*/
            List<SP_GetContactPersonListToBindGrid_Result> existingAddToCartList = new List<SP_GetContactPersonListToBindGrid_Result>();
            existingAddToCartList = GetExistingTempDataBySessionIDFormID(paraSessionID, paraUserID, currentFormID, conn);
            /*End*/

            /*Get Filter List [Filter By paraSequence]*/
            SP_GetContactPersonListToBindGrid_Result filterList = new SP_GetContactPersonListToBindGrid_Result();
            filterList = (from exist in existingAddToCartList
                          where exist.Sequence == paraSequence
                          select exist).FirstOrDefault();
            return filterList;
        }

        #endregion

        public SP_GetContactPersonListToBindGrid_Result GetContactPersonTempDataBySequence(long SequenceNo, string SessionID, string TargetObjectName, string UserID, string[] conn)
        {
            List<SP_GetContactPersonListToBindGrid_Result> ContactPersonList = new List<SP_GetContactPersonListToBindGrid_Result>();
            ContactPersonList = GetContactPersonTempData(TargetObjectName, SessionID, UserID, conn);

            SP_GetContactPersonListToBindGrid_Result ContactPerson = new SP_GetContactPersonListToBindGrid_Result();
            ContactPerson = ContactPersonList.Where(add => add.Sequence == SequenceNo).FirstOrDefault();

            return ContactPerson;
        }

        protected List<SP_GetContactPersonListToBindGrid_Result> GetContactPersonTempData(string TargetObjectName, string SessionID, string UserID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetContactPersonListToBindGrid_Result> ObjAddToContactPersonList = new List<SP_GetContactPersonListToBindGrid_Result>();
            TempData tempdata = new TempData();
            tempdata = (from temp in db.TempDatas
                        where temp.SessionID == SessionID
                        && temp.ObjectName == TargetObjectName
                        && temp.UserID == UserID
                        select temp).FirstOrDefault();
            //List<SP_GetContactPersonListToBindGrid_Result> ObjAddToCOntactPersonList2 = new List<SP_GetContactPersonListToBindGrid_Result>();
            if (tempdata != null)
            {
                ObjAddToContactPersonList = datahelper.DeserializeEntity1<SP_GetContactPersonListToBindGrid_Result>(tempdata.Data);
            }
            return ObjAddToContactPersonList;
        }

        public List<SP_GetContactPersonListToBindGrid_Result> GetContactPersonTempData(string TargetObjectName, long SelectedSeq, string SessionID, string UserID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetContactPersonListToBindGrid_Result> ObjAddToContactPersonList = new List<SP_GetContactPersonListToBindGrid_Result>();
            TempData tempdata = new TempData();
            tempdata = (from temp in db.TempDatas
                        where temp.SessionID == SessionID
                        && temp.ObjectName == TargetObjectName
                        && temp.UserID == UserID
                        select temp).FirstOrDefault();
            if (tempdata != null)
            {
                ObjAddToContactPersonList = datahelper.DeserializeEntity1<SP_GetContactPersonListToBindGrid_Result>(tempdata.Data);

                foreach (SP_GetContactPersonListToBindGrid_Result lst in ObjAddToContactPersonList.Where(obj => obj.selected == "true").ToList())
                { lst.selected = "false"; }

                foreach (SP_GetContactPersonListToBindGrid_Result lst in ObjAddToContactPersonList.Where(obj => obj.Sequence == SelectedSeq).ToList())
                { lst.selected = "true"; }
            }
            return ObjAddToContactPersonList.Where(a => a.Active != "Y").ToList();
        }


        #region GetContactPersonByReferenceId()
        /// <summary>
        /// GetContactPersonByReferenceId Is The Method To Get ContactPerson By Reference ID
        /// </summary>
        /// <param name="objectName"></param>
        /// <param name="ReferenceID"></param>
        /// <returns></returns>

        public List<SP_GetContactPersonListToBindGrid_Result> GetContactPersonByObjectNameReferenceID(string SourceObjectName, long ReferenceID, string TargetObjectName, string SessionID, string UserID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetContactPersonListToBindGrid_Result> ContactPerson = new List<SP_GetContactPersonListToBindGrid_Result>();

            ContactPerson = (from p in ce.SP_GetContactPersonListToBindGrid(SourceObjectName, ReferenceID)
                             select p).ToList();

            if (ContactPerson.Count > 0)
            {
                SaveTempDataToDB(ContactPerson, SessionID, UserID, TargetObjectName, conn);
            }
            return ContactPerson.Where(a => a.Active != "Y").ToList();
        }

        public List<SP_GetContactPersonListToBindGridLead_Result> GetContactPersonByObjectNameReferenceIDLead(string SourceObjectName, long ReferenceID, string TargetObjectName, string SessionID, string UserID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetContactPersonListToBindGridLead_Result> ContactPerson = new List<SP_GetContactPersonListToBindGridLead_Result>();

            ContactPerson = (from p in ce.SP_GetContactPersonListToBindGridLead(ReferenceID)
                             select p).ToList();

            if (ContactPerson.Count > 0)
            {
                SaveTempDataToDBLead(ContactPerson, SessionID, UserID, TargetObjectName, conn);
            }
            return ContactPerson.Where(a => a.Active != "Y").ToList();
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
        public List<SP_GetContactPersonListToBindGrid_Result> GetExistingTempDataBySessionIDFormID(string paraSessionID, string paraUserID, string currentformid, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetContactPersonListToBindGrid_Result> ObjcontactPersonList = new List<SP_GetContactPersonListToBindGrid_Result>();
            TempData tempdata = new TempData();
            SP_GetContactPersonListToBindGrid_Result tcontact = new SP_GetContactPersonListToBindGrid_Result();
            tempdata = (from temp in ce.TempDatas
                        where temp.SessionID == paraSessionID
                        && temp.ObjectName == currentformid
                        && temp.UserID == paraUserID

                        select temp).FirstOrDefault();

            if (tempdata != null)
            {
                ObjcontactPersonList = datahelper.DeserializeEntity1<SP_GetContactPersonListToBindGrid_Result>(tempdata.Data);
            }
            return ObjcontactPersonList;
        }
        #endregion


        #region CheckDuplicateContactPersonName
        public string CheckDuplicateContactPersonName(string SessionID, string Name, long Sequence, string UserID, string TargetObjectName, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            /*Begin : Get Existing Records from TempData*/
            List<SP_GetContactPersonListToBindGrid_Result> existingAddToCartList = new List<SP_GetContactPersonListToBindGrid_Result>();
            existingAddToCartList = GetExistingTempDataBySessionIDFormID(SessionID, UserID, TargetObjectName, conn);
            /*End*/

            /*Get Filter List [Filter By paraSequence]*/
            List<SP_GetContactPersonListToBindGrid_Result> filterList = new List<SP_GetContactPersonListToBindGrid_Result>();
            filterList = (from exist in existingAddToCartList
                          where exist.Name == Name && exist.Sequence != Sequence
                          select exist).ToList();

            var Id = filterList.FirstOrDefault();

            if (filterList.Count > 0)
            {
                if (filterList.Where(add => add.Active == "Y").FirstOrDefault() != null)
                {
                    return Id.Sequence.ToString();
                }
                else
                {
                    return "Same Contact Person details already exists, do you want to continue";
                }
            }
            else { return ""; }
        }
        #endregion


        #region FinalSaveToDBtAddToContactPerson

        public void FinalSaveToDBtAddToContactPerson(string paraSessionID, string TargetObjectName, long paraReferenceID, string paraUserID, string paraSaveObjectName, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetContactPersonListToBindGrid_Result> finalSaveLst = new List<SP_GetContactPersonListToBindGrid_Result>();
            finalSaveLst = GetExistingTempDataBySessionIDFormID(paraSessionID, paraUserID, TargetObjectName, conn);
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
                            where t.ObjectName == TargetObjectName && t.SessionID == paraSessionID && t.UserID == paraUserID
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

            ObjectParameter _TargetObjectName = new ObjectParameter("paraCurrentFormID", typeof(string));
            _TargetObjectName.Value = TargetObjectName;

            ObjectParameter[] obj = new ObjectParameter[] { _paraSessionID, _paraSaveObjectName, _paraReferenceID, _paraUserID, _TargetObjectName };
            ce.ExecuteFunction("SP_InsertIntotContactPersonDetail", obj);
            ce.SaveChanges();

            ClearTempDataFromDB(paraSessionID, paraUserID, paraSaveObjectName, conn);
        }

        #endregion


        public void SetContactPersonArchive(string Ids, string isDeleted, string userId, string TargetObjectName, string SessionID, string[] conn)
        {
            List<SP_GetContactPersonListToBindGrid_Result> ContactPersonList = new List<SP_GetContactPersonListToBindGrid_Result>();
            ContactPersonList = GetContactPersonTempData(TargetObjectName, SessionID, userId, conn);

            string[] SeqArr = Ids.Split(',');

            int[] SeqInts = SeqArr.Select(x => int.Parse(x)).ToArray();

            for (int i = 0; i < SeqInts.Length; i++)
            {
                SP_GetContactPersonListToBindGrid_Result ContactPerson = new SP_GetContactPersonListToBindGrid_Result();
                ContactPerson = ContactPersonList.Where(a => a.Sequence == SeqInts[i]).FirstOrDefault();
                ContactPerson.Active = isDeleted;
            }

            SaveTempDataToDB(ContactPersonList, SessionID, userId, TargetObjectName, conn);
        }

        public DataSet GetDepartmentcontact(long ParentID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_GetDepartmentContact";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("ParentID", ParentID);
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;
        }

        public DataSet Getcontactbydeptid(long Department, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_ContactDetailforDept";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("Department", Department);
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;
        }
    }
}
