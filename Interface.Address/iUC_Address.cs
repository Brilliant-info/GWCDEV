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
    public partial interface iUC_Address
    {
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mRoute> GetRouteList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<tAddress> GetAddressByReferenceId(string saveobjectName, long ReferenceID, string paraSessionID, string paraUserID, string currentformid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<tAddress> InsertIntoTemp(tAddress Address, string paraSessionID, string paraUserID, string currentformid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void ClearTempDataFromDB(string paraSessionID, string paraUserID, string currentformid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<tAddress> SetValuesToTempData_onChange(string paraSessionID, string paraUserID, string currentformid, int paraSequence, tAddress paraInput, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void FinalSaveToDBttAddress(string paraSessionID, string currentformid, long paraReferenceID, string paraUserID, string paraSaveObjectName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        tAddress GetAddressFromTempTableBySequence(string paraSessionID, string paraUserID, string currentFormID, int paraSequence, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetAddressByLine1(string paraSessionID, string Address, string City, string Country, string state, string paraUserID, string currentFormID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<tAddress> GetExistingTempDataBySessionIDFormID(string paraSessionID, string paraUserID, string currentformid, string[] conn);


    }
}
