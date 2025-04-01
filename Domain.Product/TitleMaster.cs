using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Data;
using System.ServiceModel.Activation;
using Interface.Product;
using Domain.Tempdata;
using Interface.Product;

namespace Domain.Product
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public partial class TitleMaster : Interface.Product.iTitleMaster
    {
        Domain.Server.Server svr = new Server.Server();
        DataHelper datahelper = new DataHelper();

        #region  Insert into mTitle
        public int InsertIntomTitle(mTitle objmTitle, string state, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            if (state == "AddNew")
            {
                ce.mTitles.AddObject(objmTitle);
                ce.SaveChanges();
            }
            else if (state == "Edit")
            {
                ce.mTitles.Attach(objmTitle);
                ce.ObjectStateManager.ChangeObjectState(objmTitle, EntityState.Modified);
                ce.SaveChanges();
            }
            return Convert.ToInt32(objmTitle.ID);
        }

        #endregion


        #region GetMTitleRecord
        public List<mTitle> GetTitleList(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mTitle> TitleList = new List<mTitle>();
            TitleList = (from p in ce.mTitles
                         select p).ToList();

            return TitleList;

        }
        #endregion

        #region checkDuplicateRecord
        /// <summary>
        /// checkDuplicateRecord is providing List of Discount by DiscountName 
        /// </summary>
        /// <returns></returns>
        /// 
        public string checkDuplicateRecord(string TitleName, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            var output = (from p in db.mTitles
                          where p.Title == TitleName
                          select new { p.Title }).FirstOrDefault();

            if (output != null)
            {
                result = "[ " + TitleName + " ] Title Name already exist";
            }
            return result;

        }
        #endregion

        #region checkDuplicateRecordEdit
        /// <summary>
        /// checkDuplicateRecord for Edit is providing List of Title by TitleName and ID
        /// </summary>
        /// <returns></returns>
        /// 
        public string checkDuplicateRecordEdit(string TitleName, int TitleID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            var output = (from p in db.mTitles
                          where p.Title == TitleName && p.ID != TitleID
                          select new { p.Title }).FirstOrDefault();

            if (output != null)
            {
                result = "[ " + TitleName + " ] Title Name  already exist";
            }
            return result;
        }
        #endregion


        #region
        public mTitle GetTitleByTitleID(long TitleId, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            mTitle TitleResult = new mTitle();
            TitleResult = (from p in db.mTitles
                           where p.ID == TitleId
                           select p).FirstOrDefault();
            return TitleResult;
        }
        #endregion

        #region GetActiveTitleRecords
        public List<mTitle> GetActiveTitleRecords(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mTitle> TitleList = new List<mTitle>();
            TitleList = (from p in ce.mTitles
                         where p.Active == "Y"
                         select p).ToList();
            return TitleList;
        }
        #endregion


    }
}
