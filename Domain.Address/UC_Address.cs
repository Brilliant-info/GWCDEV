using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Activation;
using System.Configuration;
using Interface.Address;
using System.ServiceModel;
using Domain.Tempdata;
using System.Xml.Linq;
using System.Data.Objects;
using Domain.Server;

namespace Domain.Address
{

    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public partial class UC_Address : Interface.Address.iUC_Address
    {
        Domain.Server.Server svr = new Server.Server();

        DataHelper datahelper = new DataHelper();

        #region GetRouteList()

        public List<mRoute> GetRouteList(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mRoute> RouteList = new List<mRoute>();
            RouteList = (from Route in ce.mRoutes
                         where Route.Active == "Y"
                         orderby Route.Sequence
                         select Route).ToList();
            if (RouteList.Count == 0)
            {
                RouteList = null;
            }
            return RouteList;
        }
        #endregion

        #region GetAddressByReferenceId()
        /// <summary>
        /// GetContactPersonByReferenceId Is The Method To Get ContactPerson By Reference ID
        /// </summary>
        /// <param name="objectName"></param>
        /// <param name="ReferenceID"></param>
        /// <returns></returns>
        public List<tAddress> GetAddressByReferenceId(string saveobjectName, long ReferenceID, string paraSessionID, string paraUserID, string currentformid, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<tAddress> Address = new List<tAddress>();

            Address = (from p in ce.tAddresses
                       where p.ObjectName == saveobjectName && p.ReferenceID == ReferenceID
                       orderby p.Sequence
                       select p).ToList();

            if (Address.Count > 0)
            {
                SaveTempDataToDB(Address, paraSessionID, paraUserID, currentformid, conn);
            }
            return Address;
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
        public List<tAddress> GetExistingTempDataBySessionIDFormID(string paraSessionID, string paraUserID, string currentformid, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<tAddress> ObjAddToAddressList = new List<tAddress>();
            TempData tempdata = new TempData();
            tAddress taddress = new tAddress();
            tempdata = (from temp in ce.TempDatas
                        where temp.SessionID == paraSessionID
                        && temp.ObjectName == currentformid
                        && temp.UserID == paraUserID
                        select temp).FirstOrDefault();

            if (tempdata != null)
            {
                ObjAddToAddressList = datahelper.DeserializeEntity1<tAddress>(tempdata.Data);
            }
            return ObjAddToAddressList;
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

        public List<tAddress> InsertIntoTemp(tAddress Address, string paraSessionID, string paraUserID, string currentformid, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            /*Begin : Get Existing Records from TempData*/
            List<tAddress> existingAddressList = new List<tAddress>();
            existingAddressList = GetExistingTempDataBySessionIDFormID(paraSessionID, paraUserID, currentformid, conn);
            /*End*/

            /*Begin : Merge (Existing + Newly Added) Products to Create TempData of AddToCart*/
            List<tAddress> mergedAddToAddressList = new List<tAddress>();
            mergedAddToAddressList.AddRange(existingAddressList);
            Address.Sequence = existingAddressList.Count + 1;
            mergedAddToAddressList.Add(Address);
            /*End*/

            /*Begin : Serialize & Save MergedAddToCartList*/
            SaveTempDataToDB(mergedAddToAddressList, paraSessionID, paraUserID, currentformid, conn);
            /*End*/

            return mergedAddToAddressList;
        }

        #endregion

        #region SaveTempDataToDB

        /// <summary>
        /// This Is For Insert Temporory Data From TempData In to 'tAddress' table
        /// </summary>
        /// <param name="paraobjList"></param>
        /// <param name="paraSessionID"></param>
        /// <param name="paraUserID"></param>
        /// <param name="paraObjectName"></param>
        /// 
        protected void SaveTempDataToDB(List<tAddress> paraobjList, string paraSessionID, string paraUserID, string currentformid, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            /*Begin : Remove Existing Records*/
            ClearTempDataFromDB(paraSessionID, paraUserID, currentformid, conn);
            /*End*/


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
        public List<tAddress> SetValuesToTempData_onChange(string paraSessionID, string paraUserID, string currentformid, int paraSequence, tAddress paraInput, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<tAddress> existingList = new List<tAddress>();
            existingList = GetExistingTempDataBySessionIDFormID(paraSessionID, paraUserID, currentformid, conn);

            tAddress editRow = new tAddress();
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

        #region FinalSaveToDBttAddress

        /// <summary>
        /// This Is Method To Save Record In tAddress
        /// </summary>
        /// <param name="paraSessionID"></param>
        /// <param name="paraObjectName"></param>
        /// <param name="paraReferenceID"></param>
        /// <param name="paraUserID"></param>
        public void FinalSaveToDBttAddress(string paraSessionID, string currentformid, long paraReferenceID, string paraUserID, string paraSaveObjectName, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<tAddress> finalSaveLst = new List<tAddress>();
            finalSaveLst = GetExistingTempDataBySessionIDFormID(paraSessionID, paraUserID, currentformid, conn);
            if (finalSaveLst.Count > 0)
            {
                XElement xmlEle = new XElement("AddToAddressList", from rec in finalSaveLst
                                                                   select new XElement("AddressList",
                                                                   new XElement("ID", rec.ID),
                                                                   new XElement("ObjectName", paraSaveObjectName),
                                                                   new XElement("ReferenceID", rec.ReferenceID),
                                                                   new XElement("Sequence", rec.Sequence),
                                                                   new XElement("AddressType", rec.AddressType),
                                                                   new XElement("AddressLine1", rec.AddressLine1),
                                                                   new XElement("AddressLine2", rec.AddressLine2),
                                                                   new XElement("AddressLine3", rec.AddressLine3),
                                                                   new XElement("RouteID", rec.RouteID),
                                                                   new XElement("Landmark", rec.Landmark),
                                                                   new XElement("County", rec.County),
                                                                   new XElement("State", rec.State),
                                                                   new XElement("City", rec.City),
                                                                   new XElement("Zipcode", rec.Zipcode),
                                                                   new XElement("PhoneNo", rec.PhoneNo),
                                                                   new XElement("FaxNo", rec.FaxNo),
                                                                   new XElement("EmailID", rec.EmailID),
                                                                   new XElement("IsDefault", rec.IsDefault),
                                                                   new XElement("Active", rec.Active),
                                                                   new XElement("CompanyID", rec.CompanyID)));

                TempData tempdata = new TempData();
                tempdata = (from t in ce.TempDatas
                            where t.ObjectName == currentformid && t.SessionID == paraSessionID && t.UserID == paraUserID
                            select t).FirstOrDefault();
                tempdata.XmlData = xmlEle.ToString();
                ce.ObjectStateManager.ChangeObjectState(tempdata, EntityState.Modified);
                ce.SaveChanges();

                ObjectParameter _paraSessionID = new ObjectParameter("paraSessionID", typeof(string));
                _paraSessionID.Value = paraSessionID;

                ObjectParameter _paraSaveObjectName = new ObjectParameter("paraSaveObjectName", typeof(string));
                _paraSaveObjectName.Value = paraSaveObjectName;

                ObjectParameter _paraReferenceID = new ObjectParameter("paraReferenceID", typeof(long));
                _paraReferenceID.Value = paraReferenceID;


                ObjectParameter _paraUserID = new ObjectParameter("paraUserID", typeof(string));
                _paraUserID.Value = paraUserID;


                ObjectParameter _paraCurrentFormID = new ObjectParameter("paraCurrentFormID", typeof(string));
                _paraCurrentFormID.Value = currentformid;

                ObjectParameter[] obj = new ObjectParameter[] { _paraSessionID, _paraSaveObjectName, _paraReferenceID, _paraUserID, _paraCurrentFormID };
                ce.ExecuteFunction("SP_InsertIntoAddressList", obj);
                ce.SaveChanges();

            }
        }

        #endregion

        #region GetAddressFromTempTableBySequence()
        /// <summary>
        /// Get List Of Address Record By Sequence No 
        /// </summary>
        /// <param name="paraSessionID"></param>
        /// <param name="paraUserID"></param>
        /// <param name="paraObjectName"></param>
        /// <param name="paraSequence"></param>
        /// <returns></returns>
        public tAddress GetAddressFromTempTableBySequence(string paraSessionID, string paraUserID, string currentFormID, int paraSequence, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            /*Begin : Get Existing Records from TempData*/
            List<tAddress> existingAddressList = new List<tAddress>();
            existingAddressList = GetExistingTempDataBySessionIDFormID(paraSessionID, paraUserID, currentFormID, conn);
            /*End*/

            /*Get Filter List [Filter By paraSequence]*/
            tAddress filterList = new tAddress();
            filterList = (from exist in existingAddressList
                          where exist.Sequence == paraSequence
                          select exist).FirstOrDefault();
            return filterList;
        }


        #endregion

        public string GetAddressByLine1(string paraSessionID, string Address, string City, string Country, string state, string paraUserID, string currentFormID, string[] conn)
        {
            string Result = "";
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            /*Begin : Get Existing Records from TempData*/
            List<tAddress> existingAddressList = new List<tAddress>();
            existingAddressList = GetExistingTempDataBySessionIDFormID(paraSessionID, paraUserID, currentFormID, conn);
            /*End*/

            /*Get Filter List [Filter By paraSequence]*/
            tAddress filterList = new tAddress();
            filterList = (from exist in existingAddressList
                          where exist.AddressLine1 == Address && exist.County == Country && exist.City == City && exist.State == state
                          select exist).FirstOrDefault();
            if (filterList == null)
            {
                Result = "";
            }
            else
            {
                Result = "Same Address Already Exist";
            }
            return Result;
        }

        /*New Code*/


    }
}
