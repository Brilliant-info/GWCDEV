using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel.Activation;
using System.Configuration;
using Interface.Approval;
using System.ServiceModel;
using System.Data.EntityClient;
using System.Data.Objects;
using Domain.Server;

namespace Domain.Approval
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]

    public partial class UC_Approval : Interface.Approval.iUC_Approval
    {
        Domain.Server.Server svr = new Server.Server();
        public string FinalUpdateUCApproval(string Status, string Remark, string tApprovalIDs, long StatusChangedBy, string[] connstr)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(connstr));

            ObjectParameter _Status = new ObjectParameter("Status", typeof(string));
            _Status.Value = Status;

            ObjectParameter _StatusChangedBy = new ObjectParameter("StatusChangedBy", typeof(long));
            _StatusChangedBy.Value = StatusChangedBy;

            ObjectParameter _Remark = new ObjectParameter("Remark", typeof(string));
            _Remark.Value = Remark;

            ObjectParameter _tApprovalIDs = new ObjectParameter("tApprovalIDs", typeof(string));
            _tApprovalIDs.Value = tApprovalIDs;


            ObjectParameter[] obj = new ObjectParameter[] { _Status, _StatusChangedBy, _Remark, _tApprovalIDs };
            db.ExecuteFunction("SP_Update_tApprovalDetail", obj);
            db.SaveChanges();

            return "Records save successfully";
        }

        public tApprovalDetail chekcApprovalPermission(string ObjectName, long ReferenceID, long UserID, string[] conn)
        {
            tApprovalDetail ApprovalDetail = new tApprovalDetail();
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                ApprovalDetail = db.tApprovalDetails.Where(t => t.ObjectName == ObjectName && t.ReferenceID == ReferenceID && t.ApproverUserID == UserID).FirstOrDefault();
            }
            catch { }
            return ApprovalDetail;
        }
    }
}
