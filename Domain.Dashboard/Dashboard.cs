using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interface.Dashboard;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Data;



namespace Domain.Dashboard
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public partial class Dashboard : iDashboard
    {
        Domain.Server.Server svr = new Server.Server();
        public List<POR_Dashboard_UserWise> GetDashboardsByUserID(long UserID, string[] conn)
        {
            List<POR_Dashboard_UserWise> DashboardList = new List<POR_Dashboard_UserWise>();
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                DashboardList = db.POR_Dashboard_UserWise.Where(d => d.UserID == UserID && d.IsActive == true).OrderBy(d => d.DisplaySequence).ToList();
            }
            catch { }
            return DashboardList;
        }

        public POR_Dashboard_UserWise GetDashboardsByDashboardID(long DashboardID, string[] conn)
        {
            POR_Dashboard_UserWise Dashboard = new POR_Dashboard_UserWise();
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                Dashboard = db.POR_Dashboard_UserWise.Where(d => d.ID == DashboardID).FirstOrDefault();
            }
            catch { }
            return Dashboard;
        }

        public DataSet GetDashboardDataByQuery(string DashbaordQuery, string[] conn)
        {
            DataSet ds = new DataSet();
            try
            {
                ds = svr.FillDataSet(DashbaordQuery, conn);
            }
            catch { }
            finally { }
            return ds;
        }
    }
}
