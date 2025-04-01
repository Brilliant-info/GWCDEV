using System;
using System.Linq;
using System.ServiceModel;


namespace Interface.Approval
{
    [ServiceContract]
    public partial interface iUC_Approval
    {
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string FinalUpdateUCApproval(string Status, string Remark, string tApprovalIDs, long StatusChangedBy, string[] connstr);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        tApprovalDetail chekcApprovalPermission(string ObjectName, long ReferenceID, long UserID, string[] conn);
    }
}
