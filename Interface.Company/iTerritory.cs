using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Data.Entity;
using System.Data;

using Domain.Server;

namespace Interface.Company
{
    [ServiceContract]
    public partial interface iTerritory
    {
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mTerritory> GetTerritoryGroupList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mTerritory> GetTerritoryList(long Level, long ParentID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<vGetUserProfileList> GetUserListByTerritory(long Level, long ParentID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mTerritory> GetDepartmentList(long CompanyID, string[] conn);

       
    }
}
