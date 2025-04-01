using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Activation;
namespace Interface.UCToolbar
{
    [ServiceContract]
    public partial interface iUCToolbar
    {
        [OperationContract]
        string _AddNewUC();

        [OperationContract]
        string _SaveUC();

        [OperationContract]
        string _ClearUC();

        [OperationContract]
        string _SearchUC();

        [OperationContract]
        string _ImportUC();

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        mUserRolesDetail GetUserRightsBy_ObjectNameUserID(string ObjectName, long UserID, string[] conn);
    }
}
