using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Activation;
using System.ServiceModel;
using Interface.Company;

namespace Domain.Company
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public partial class Territory : Interface.Company.iTerritory
    {
        Domain.Server.Server svr = new Server.Server();
        public List<mTerritory> GetTerritoryGroupList(string[] conn)
        {
            List<mTerritory> TerritoryList = new List<mTerritory>();
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            try
            {
                var lst = (from t in db.mTerritories
                           where t.Level > 1
                           orderby t.Level
                           select new { t.Level, t.GroupTitle, t.IsMandatory }).Distinct();
                TerritoryList = lst.AsEnumerable().Select(t => new mTerritory { Level = t.Level, GroupTitle = t.GroupTitle, IsMandatory = t.IsMandatory }).ToList();

            }
            catch { }
            finally {  }
            return TerritoryList;
        }

        public List<mTerritory> GetTerritoryList(long Level, long ParentID, string[] conn)
        {
            List<mTerritory> TerritoryList = new List<mTerritory>();
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            try
            {
                TerritoryList = db.mTerritories.Where(t => t.Level == Level && t.ParentID == ParentID).OrderBy(t => t.Territory).ToList();
            }
            catch { }
            finally {  }
            return TerritoryList;
        }

        public List<vGetUserProfileList> GetUserListByTerritory(long Level, long ParentID, string[] conn)
        {
            List<vGetUserProfileList> UserList = new List<vGetUserProfileList>();
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            try
            {
                UserList = (from vu in db.vGetUserProfileLists
                            join ut in db.mUserTerritoryDetails on vu.userID equals ut.UserID
                            where ut.TerritoryID == ParentID && ut.Level == Level
                            select vu).ToList();
            }
            catch { }
            finally {  }
            return UserList;

        }

        public List<mTerritory> GetDepartmentList(long CompanyID, string[] conn)
        {
            List<mTerritory> TerritoryList = new List<mTerritory>();
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            try
            {


                TerritoryList = (from t in db.mTerritories
                                 where t.ParentID == CompanyID
                                 select t).ToList();

            }
            catch { }
            finally { }
            return TerritoryList;
        }
       

       

    }
}
