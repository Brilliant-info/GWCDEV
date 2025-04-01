using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Data;

namespace Interface.Dashboard
{
    [ServiceContract]
    public partial interface iDashboard
    {
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_Dashboard_UserWise> GetDashboardsByUserID(long UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        POR_Dashboard_UserWise GetDashboardsByDashboardID(long DashboardID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetDashboardDataByQuery(string DashbaordQuery, string[] conn);


    }
}
