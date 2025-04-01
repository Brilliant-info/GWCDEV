using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Activation;
using System.ServiceModel;
using Interface.Company;
using Domain.Server;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Linq;
using System.Configuration;

namespace Domain.Company
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]

    public partial class PrefixMaster : Interface.Company.iPrefixMaster
    {
        Domain.Server.Server svr = new Server.Server();

        #region GetPrefixList
        public List<mPrefixMaster> GetPrefixList(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mPrefixMaster> ObjName = new List<mPrefixMaster>();

            ObjName = (from p in ce.mPrefixMasters
                       orderby p.PreObjectName
                       select p).ToList();
            return ObjName;
        }
        #endregion

        #region InsertmPrefixMaster
        public int InsertmPrefixMaster(mPrefixMaster Pref, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.mPrefixMasters.AddObject(Pref);
            ce.SaveChanges();
            return 1;
        }
        #endregion

        #region UpdatemPrefixMaster
        public int UpdatemPrefixMaster(mPrefixMaster UpdtPref, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.mPrefixMasters.Attach(UpdtPref);
            ce.ObjectStateManager.ChangeObjectState(UpdtPref, EntityState.Modified);
            ce.SaveChanges();
            return 1;
        }
        #endregion

        #region GetPrefixMasterByID
        public mPrefixMaster GetPrefixMasterByID(int PrefID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            mPrefixMaster PrID = new mPrefixMaster();
            PrID = (from p in ce.mPrefixMasters
                     where p.PreID == PrefID
                     select p).FirstOrDefault();
            ce.Detach(PrID);
            return PrID;
        }
        #endregion

        //#region checkDuplicateRecord
        //public string checkDuplicateRecord(string ObjName, int ObjPreID, string[] conn)
        //{
        //    BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
        //    string result = "";
        //    var output = (from p in ce.mPrefixMasters
        //                  where p.PreObjectName == ObjName && p.PreID == ObjPreID
        //                  select new { p.PreObjectName }).FirstOrDefault();

        //    if (output != null)
        //    {
        //        result = "[" + ObjName + " ] Object Name already exist";
        //    }
        //    return result;
        //}
        //#endregion

        //#region checkDuplicateRecordEdit
        //public string checkDuplicateRecordEdit(int PrID, string ObjNm, string[] conn)
        //{
        //    BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
        //    string result = "";
        //    var output = (from p in ce.mPrefixMasters
        //                  where p.PreObjectName == ObjNm && p.PreID == PrID
        //                  select new { p.PreObjectName }).FirstOrDefault();

        //    if (output != null)
        //    {
        //        result = "[" + ObjNm + "] Object Name already exist";
        //    }
        //    return result;
        //}
        //#endregion

        //#region GetPrefixRecordToBindGrid
        //public DataSet GetPrefixRecordToBindGrid(string[] conn)
        //{
        //    BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
        //    List<mPrefixMaster> PrefixGrd = new List<mPrefixMaster>();

        //    XElement xmlPrefixMaster = new XElement("PrefixList", from p in ce.mPrefixMasters
        //                                                          orderby p.PreObjectName
        //                                                          select new XElement("PrefixMaster",
        //                                                              new XElement("PreID", p.PreID),
        //                                                              new XElement("PreObjectName", p.PreObjectName),
        //                                                              new XElement("Prefix", p.Prefix),
        //                                                              new XElement("Seperator", p.Seperator),
        //                                                              new XElement("StartingNo", p.StartingNo)
        //                                                              ));

        //    DataSet DS = new DataSet();
        //    DS.ReadXml(xmlPrefixMaster.CreateReader());

        //    if (DS.Tables.Count <= 0)
        //    {
        //        DS.Tables.Add("PrefixMaster");
        //    }
        //    return DS;
        //}
        //#endregion

        //#region GetObjectListToBindDDL
        //public List<mPrefixMaster> GetObjectListToBindDDL(string[] conn)
        //{
        //    BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
        //    List<mPrefixMaster> ObjNM = new List<mPrefixMaster>();
        //    ObjNM = (from p in ce.mPrefixMasters
        //             orderby p.PreObjectName
        //             select p).ToList();

        //    return ObjNM;
        //}
        //#endregion
    }
}
