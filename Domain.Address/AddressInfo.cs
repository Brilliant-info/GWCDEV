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
    public partial class AddressInfo : Interface.Address.iAddressInfo
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
            return RouteList;
        }
        #endregion

        #region GetCityList()

        public List<tAddress> GetCityList(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<tAddress> CityList = new List<tAddress>();

            CityList = (from City in ce.tAddresses
                        select City).Distinct().ToList();

            //CityList = (from City in ce.tAddresses
            //            group City by City.City into g
            //            select new { 
            //                id = g.Key,
            //                newfield = g.Count()
            //            }).ToList();

            return CityList;
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
        protected void SaveTempDataToDB(List<SP_GetAddressListToBindGrid_Result> paraobjList, string paraSessionID, string paraUserID, string TargetObjectName, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            /*Begin : Remove Existing Records*/
            ClearTempDataFromDB(paraSessionID, paraUserID, TargetObjectName, conn);
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
            tempdata.ObjectName = TargetObjectName.ToString();
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
        public void ClearTempDataFromDB(string paraSessionID, string paraUserID, string TargetObjectName, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            TempData tempdata = new TempData();
            tempdata = (from rec in ce.TempDatas
                        where rec.SessionID == paraSessionID
                        && rec.UserID == paraUserID
                        && rec.ObjectName == TargetObjectName
                        select rec).FirstOrDefault();
            if (tempdata != null) { ce.DeleteObject(tempdata); ce.SaveChanges(); }
        }
        #endregion

        #region GetTempData
        public List<SP_GetAddressListToBindGrid_Result> GetAddressTempData(string TargetObjectName, long BillingSeq, long ShippingSeq, string SessionID, string UserID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetAddressListToBindGrid_Result> ObjAddToAddressList = new List<SP_GetAddressListToBindGrid_Result>();
            TempData tempdata = new TempData();
            tempdata = (from temp in db.TempDatas
                        where temp.SessionID == SessionID
                        && temp.ObjectName == TargetObjectName
                        && temp.UserID == UserID
                        select temp).FirstOrDefault();

            List<SP_GetAddressListToBindGrid_Result> ObjAddToAddressList2 = new List<SP_GetAddressListToBindGrid_Result>();
            if (tempdata != null)
            {
                ObjAddToAddressList = datahelper.DeserializeEntity1<SP_GetAddressListToBindGrid_Result>(tempdata.Data);
                foreach (SP_GetAddressListToBindGrid_Result lst in ObjAddToAddressList.Where(obj => obj.BillIsChecked == "true" || obj.ShipIsChecked == "true").ToList())
                {
                    lst.BillIsChecked = "false"; lst.ShipIsChecked = "false";
                }
                foreach (SP_GetAddressListToBindGrid_Result lst in ObjAddToAddressList.Where(obj => obj.Sequence == BillingSeq).ToList())
                { lst.BillIsChecked = "true"; }

                foreach (SP_GetAddressListToBindGrid_Result lst in ObjAddToAddressList.Where(obj => obj.Sequence == ShippingSeq).ToList())
                { lst.ShipIsChecked = "true"; }
            }

            SaveTempDataToDB(ObjAddToAddressList, SessionID, UserID, TargetObjectName, conn);
            return ObjAddToAddressList.Where(a => a.Active != "Y").ToList();
        }

        protected List<SP_GetAddressListToBindGrid_Result> GetAddressTempData(string TargetObjectName, string SessionID, string UserID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetAddressListToBindGrid_Result> ObjAddToAddressList = new List<SP_GetAddressListToBindGrid_Result>();
            TempData tempdata = new TempData();
            tempdata = (from temp in db.TempDatas
                        where temp.SessionID == SessionID
                        && temp.ObjectName == TargetObjectName
                        && temp.UserID == UserID
                        select temp).FirstOrDefault();
            List<SP_GetAddressListToBindGrid_Result> ObjAddToAddressList2 = new List<SP_GetAddressListToBindGrid_Result>();
            if (tempdata != null)
            {
                ObjAddToAddressList = datahelper.DeserializeEntity1<SP_GetAddressListToBindGrid_Result>(tempdata.Data);
            }
            return ObjAddToAddressList;
        }
        #endregion

        #region GetAddressByObjectNameReferenceID
        public List<SP_GetAddressListToBindGrid_Result> GetAddressByObjectNameReferenceID(string SourceObjectName, long ReferenceID, string TargetObjectName, string SessionID, string UserID, string[] conn)
        {
            List<SP_GetAddressListToBindGrid_Result> AddressList = new List<SP_GetAddressListToBindGrid_Result>();
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            AddressList = (from sp in db.SP_GetAddressListToBindGrid(SourceObjectName, ReferenceID)
                           orderby sp.Sequence
                           where sp.Active != "Y"  // active=isArchive
                           select sp).ToList();
            SaveTempDataToDB(AddressList, SessionID, UserID, TargetObjectName, conn);
            return AddressList;
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
        public void FinalSaveAddress(string SessionID, string ReferenceObjectName, long ReferenceID, string TargetObjectName, string UserID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetAddressListToBindGrid_Result> finalSaveLst = new List<SP_GetAddressListToBindGrid_Result>();
            finalSaveLst = GetAddressTempData(TargetObjectName, SessionID, UserID, conn);
            if (finalSaveLst.Count > 0)
            {
                XElement xmlEle = new XElement("AddToAddressList", from rec in finalSaveLst
                                                                   select new XElement("AddressList",
                                                                   new XElement("ID", rec.ID),
                                                                   new XElement("ObjectName", ReferenceObjectName),
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
                                                                   new XElement("CompanyID", rec.CompanyID),
                                                                   new XElement("Zone", rec.Zone),
                                                                   new XElement("SubZone", rec.SubZone)
                                                                   ));

                TempData tempdata = new TempData();
                tempdata = (from t in ce.TempDatas
                            where t.ObjectName == TargetObjectName && t.SessionID == SessionID && t.UserID == UserID
                            select t).FirstOrDefault();
                tempdata.XmlData = xmlEle.ToString();
                ce.ObjectStateManager.ChangeObjectState(tempdata, EntityState.Modified);
                ce.SaveChanges();

                ObjectParameter _paraSessionID = new ObjectParameter("paraSessionID", typeof(string));
                _paraSessionID.Value = SessionID;

                ObjectParameter _paraSaveObjectName = new ObjectParameter("paraSaveObjectName", typeof(string));
                _paraSaveObjectName.Value = ReferenceObjectName;

                ObjectParameter _paraReferenceID = new ObjectParameter("paraReferenceID", typeof(long));
                _paraReferenceID.Value = ReferenceID;


                ObjectParameter _paraUserID = new ObjectParameter("paraUserID", typeof(string));
                _paraUserID.Value = UserID;


                ObjectParameter _paraCurrentFormID = new ObjectParameter("paraCurrentFormID", typeof(string));
                _paraCurrentFormID.Value = TargetObjectName;

                ObjectParameter[] obj = new ObjectParameter[] { _paraSessionID, _paraSaveObjectName, _paraReferenceID, _paraUserID, _paraCurrentFormID };
                ce.ExecuteFunction("SP_InsertIntoAddressList", obj);
                ce.SaveChanges();

            }
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
        public void InsertIntoTemp(SP_GetAddressListToBindGrid_Result Address, string SessionID, string UserID, string TargetObjectName, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            /*Begin : Get Existing Records from TempData*/
            List<SP_GetAddressListToBindGrid_Result> existingAddressList = new List<SP_GetAddressListToBindGrid_Result>();
            existingAddressList = GetAddressTempData(TargetObjectName, SessionID, UserID, conn);
            /*End*/

            /*Begin : Merge (Existing + Newly Added) Products to Create TempData of AddToCart*/
            List<SP_GetAddressListToBindGrid_Result> mergedAddToAddressList = new List<SP_GetAddressListToBindGrid_Result>();
            mergedAddToAddressList.AddRange(existingAddressList);
            Address.Sequence = existingAddressList.Count + 1;
            mergedAddToAddressList.Add(Address);
            /*End*/

            /*Begin : Serialize & Save MergedAddToCartList*/
            SaveTempDataToDB(mergedAddToAddressList, SessionID, UserID, TargetObjectName, conn);
            /*End*/

        }
        #endregion

        #region  SetValuesToTempData
        public void SetValuesToTempData_onChange(string SessionID, string UserID, string TargetObjectName, int Sequence, SP_GetAddressListToBindGrid_Result paraInput, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<SP_GetAddressListToBindGrid_Result> existingList = new List<SP_GetAddressListToBindGrid_Result>();
            existingList = GetAddressTempData(TargetObjectName, SessionID, UserID, conn);

            SP_GetAddressListToBindGrid_Result editRow = new SP_GetAddressListToBindGrid_Result();
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

        public string CheckDuplicateAddress(string AddressLine1, string Country, string State, string City, string SessionID, string TargetObjectName, string UserID, string[] conn)
        {
            List<SP_GetAddressListToBindGrid_Result> AddressList = new List<SP_GetAddressListToBindGrid_Result>();
            AddressList = GetAddressTempData(TargetObjectName, SessionID, UserID, conn);

            List<SP_GetAddressListToBindGrid_Result> CheckDuplicateAddress = new List<SP_GetAddressListToBindGrid_Result>();
            CheckDuplicateAddress = (from obj in AddressList
                                     where obj.AddressLine1 == AddressLine1 && obj.County == Country && obj.State == State && obj.City == City
                                     select obj).ToList();
            var Id = CheckDuplicateAddress.FirstOrDefault();
            if (CheckDuplicateAddress.Count > 0)
            {
                if (CheckDuplicateAddress.Where(add => add.Active == "Y").FirstOrDefault() != null)
                {
                    return Id.Sequence.ToString();
                }
                else
                {

                    return "Same address details already exists, do you want to continue";
                }
            }
            else { return ""; }
        }

        public SP_GetAddressListToBindGrid_Result GetAddressTempDataBySequence(long SequenceNo, string SessionID, string TargetObjectName, string UserID, string[] conn)
        {
            List<SP_GetAddressListToBindGrid_Result> AddressList = new List<SP_GetAddressListToBindGrid_Result>();
            AddressList = GetAddressTempData(TargetObjectName, SessionID, UserID, conn);

            SP_GetAddressListToBindGrid_Result Address = new SP_GetAddressListToBindGrid_Result();
            //Address = AddressList.Where(add => add.Active != "Y").FirstOrDefault();
            Address = AddressList.Where(add => add.Sequence == SequenceNo).FirstOrDefault();

            return Address;
        }

        public List<mZone> GetZoneList(string Country, string State, string[] conn)
        {
            List<mZone> ZoneList = new List<mZone>();
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            try
            {
                ZoneList = db.mZones.Where(z => z.Country == Country && z.State == State).OrderBy(z => z.Zone).ToList();
                return ZoneList;
            }
            catch (Exception ex)
            { return ZoneList; }
            finally
            { db.Dispose(); }
        }

        public List<mSubZone> GetSubZoneList(long ZoneID, string[] conn)
        {
            List<mSubZone> SubZoneList = new List<mSubZone>();
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            try
            {
                SubZoneList = db.mSubZones.Where(z => z.ZoneID == ZoneID).OrderBy(z => z.SubZone).ToList();
                return SubZoneList;
            }
            catch (Exception ex)
            { return SubZoneList; }
            finally
            { db.Dispose(); }
        }

        public void SetAddressArchive(string Ids, string isDeleted, string userId, string TargetObjectName, string SessionID, string[] conn)
        {
            List<SP_GetAddressListToBindGrid_Result> AddressList = new List<SP_GetAddressListToBindGrid_Result>();
            AddressList = GetAddressTempData(TargetObjectName, SessionID, userId, conn);

            string[] SeqArr = Ids.Split(',');

            int[] SeqInts = SeqArr.Select(x => int.Parse(x)).ToArray();

            for (int i = 0; i < SeqInts.Length; i++)
            {
                SP_GetAddressListToBindGrid_Result address = new SP_GetAddressListToBindGrid_Result();
                address = AddressList.Where(a => a.Sequence == SeqInts[i]).FirstOrDefault();
                address.Active = isDeleted;
            }

            SaveTempDataToDB(AddressList, SessionID, userId, TargetObjectName, conn);
        }

        public DataSet GetDeptIDstoUpdateconatct(long ReferenceID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_GetDeptToUpdatecontRef";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("ReferenceID", ReferenceID);
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;
        }

        public void Updatecontactref(long param, long ID, string[] conn)
        {
             
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_Updatecontactrefence";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("param", param);
            cmd.Parameters.AddWithValue("ID", ID);
            cmd.ExecuteNonQuery();
        }

        public DataSet GridFillAddressByObjectNameReferenceID(long ReferenceID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_GetAddressDetailToBindgrid";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("ReferenceID", ReferenceID);
            da.SelectCommand = cmd;
            da.Fill(ds);
            return ds;
        }
    }

}
