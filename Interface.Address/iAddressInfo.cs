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
    public partial interface iAddressInfo
    {
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mRoute> GetRouteList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void ClearTempDataFromDB(string paraSessionID, string paraUserID, string TargetObjectName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetAddressListToBindGrid_Result> GetAddressTempData(string TargetObjectName, long BillingSeq, long ShippingSeq, string SessionID, string UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void FinalSaveAddress(string SessionID, string ReferenceObjectName, long ReferenceID, string TargetObjectName, string UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void InsertIntoTemp(SP_GetAddressListToBindGrid_Result Address, string SessionID, string UserID, string TargetObjectName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void SetValuesToTempData_onChange(string SessionID, string UserID, string TargetObjectName, int Sequence, SP_GetAddressListToBindGrid_Result paraInput, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string CheckDuplicateAddress(string AddressLine1, string Country, string State, string City, string SessionID, string TargetObjectName, string UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        SP_GetAddressListToBindGrid_Result GetAddressTempDataBySequence(long SequenceNo, string SessionID, string TargetObjectName, string UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetAddressListToBindGrid_Result> GetAddressByObjectNameReferenceID(string SourceObjectName, long ReferenceID, string TargetObjectName, string SessionID, string UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<tAddress> GetCityList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mZone> GetZoneList(string Country, string State, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mSubZone> GetSubZoneList(long ZoneID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void SetAddressArchive(string Ids, string isDeleted, string userId, string TargetObjectName, string SessionID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetDeptIDstoUpdateconatct(long ReferenceID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void Updatecontactref(long param, long ID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
       DataSet GridFillAddressByObjectNameReferenceID(long ReferenceID,string[] conn);
    }
}
