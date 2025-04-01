using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Data.Entity;
using System.Data;


namespace Interface.Address
{

    [ServiceContract]
    public partial interface iRouteMaster
    {
       
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int InsertmRoute(mRoute rut, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int UpdatemRoute(mRoute updaterut, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        mRoute GetRouteListByID(int RouteId, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string checkDuplicateRecord(string RouteName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string checkDuplicateRecordEdit(int RouteID, string RouteName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetRouteRecordToBind(string[] conn);

    }
}
