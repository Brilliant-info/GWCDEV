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
using System.Xml.Linq;
using Domain.Server;

namespace Domain.Address
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]

    public partial class RouteMaster : Interface.Address.iRouteMaster
    {
        //BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities();
        Domain.Server.Server svr = new Server.Server();

        #region InsertmRoute
        public int InsertmRoute(mRoute rut, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.mRoutes.AddObject(rut);
            ce.SaveChanges();
            return 1;
        }
        #endregion

        #region UpdatemRoute
        public int UpdatemRoute(mRoute updaterut, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ce.mRoutes.Attach(updaterut);
            ce.ObjectStateManager.ChangeObjectState(updaterut, EntityState.Modified);
            ce.SaveChanges();
            return 1;
        }
        #endregion

        #region GetRouteListByID
        /// <summary>
        /// GetRouteList is providing List of Route
        /// </summary>
        /// <returns></returns>
        /// 
        public mRoute GetRouteListByID(int RouteId, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            mRoute RouteList = new mRoute();
            RouteList = (from p in ce.mRoutes
                        where p.ID == RouteId
                        select p).FirstOrDefault();
            ce.Detach(RouteList);
            return RouteList;

        }
        #endregion

        #region checkDuplicateRecord
        /// <summary>
        /// checkDuplicateRecord is providing List of Route by RouteName 
        /// </summary>
        /// <returns></returns>
        /// 
        public string checkDuplicateRecord(string RouteName, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            var output = (from p in ce.mRoutes
                          where p.Name == RouteName
                          select new { p.Name }).FirstOrDefault();

            if (output != null)
            {
                result = "[ " + RouteName + " ] Route Name allready exist";
            }
            return result;

        }
        #endregion

        #region checkDuplicateRecordEdit
        /// <summary>
        /// checkDuplicateRecord for Edit is providing List of Route by RouteName and ID
        /// </summary>
        /// <returns></returns>
        /// 
        public string checkDuplicateRecordEdit(int RouteID, string RouteName, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            string result = "";
            var output = (from p in ce.mRoutes
                          where p.Name == RouteName && p.ID != RouteID
                          select new { p.Name }).FirstOrDefault();


            if (output != null)
            {
                result = "[ " + RouteName + " ] Route Name allready exist";
            }
            return result;

        }
        #endregion

        #region GetRouteRecordToBind
        /// <summary>
        /// GetRouteRecordToBind is providing List of Route for bind grid with Actine Yes/No
        /// </summary>
        /// <returns></returns>
        /// 
        public DataSet GetRouteRecordToBind(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mRoute> LeadSource = new List<mRoute>();
            XElement xmlRouteMaster = new XElement("RouteList", from m in ce.mRoutes.AsEnumerable()
                                                                          orderby m.Sequence
                                                                          select new XElement("Route",
                                                                          new XElement("ID", m.ID),
                                                                          new XElement("Name", m.Name),
                                                                          new XElement("Details", m.Details),
                                                                          new XElement("Sequence", m.Sequence),      
                                                                          new XElement("Active", m.Active == "Y" ? "Yes" : "No")
                                                                          ));
            DataSet ds = new DataSet();
            ds.ReadXml(xmlRouteMaster.CreateReader());
            DataTable dt = new DataTable();
            if (ds.Tables.Count <= 0)
            {
                dt = ds.Tables.Add("Route1");
            }
            return ds;
        }
        #endregion
    }
}
