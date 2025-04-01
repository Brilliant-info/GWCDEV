using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Data;

namespace Interface.Inventory
{
    [ServiceContract]
    public partial interface iPRSReports
    {
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetDefaultViewData(string WhereCondition, string ObjectName, bool IsApproval, long UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetQueryData(string site, string engn, string prd, string fdt, string tdt, long userid, string[] conn);
    }
}
