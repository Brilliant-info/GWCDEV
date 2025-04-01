using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using Interface.UCToolbar;

namespace Domain.UCToolbar
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class UCToolbar : Interface.UCToolbar.iUCToolbar
    {
        Domain.Server.Server svr = new Server.Server();
        public string _AddNewUC()
        { return "AddNew1"; }

        public string _SaveUC()
        { return "Save1"; }

        public string _ClearUC()
        { return "Clear1"; }

        public string _SearchUC()
        { return "Search1"; }

        public string _ImportUC()
        { return "Import1"; }

        public mUserRolesDetail GetUserRightsBy_ObjectNameUserID(string ObjectName, long UserID, string[] conn)
        {
            mUserRolesDetail result = new mUserRolesDetail();
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                result = db.mUserRolesDetails.Where(ur => ur.UserID == UserID && ur.ObjectName == ObjectName).FirstOrDefault();
            }
            catch { }
            return result;
        }
    }
}
