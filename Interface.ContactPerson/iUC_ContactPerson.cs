using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Data.Entity;


namespace Interface.ContactPerson
{

    [ServiceContract]
    public partial interface iUC_ContactPerson
    {
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetContactPersonsByReferenceID_Result> GetContactPersonByReferenceId(string saveobjectName, long ReferenceID, string paraSessionID, string paraUserID, string SenderObject, long SenderID, string currentformid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetContactPersonsByReferenceID_Result> InsertIntoTemp(SP_GetContactPersonsByReferenceID_Result ContactPerson, string paraSessionID, string paraUserID, string currentformid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void ClearTempDataFromDB(string paraSessionID, string paraUserID, string currentformid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetContactPersonsByReferenceID_Result> SetValuesToTempData_onChange(string paraSessionID, string paraUserID, string currentformid, int paraSequence, SP_GetContactPersonsByReferenceID_Result paraInput, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void FinalSaveToDBtAddToContactPerson(string paraSessionID, string currentformid, long paraReferenceID, string paraUserID, string paraSaveObjectName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        SP_GetContactPersonsByReferenceID_Result GetContactDetailFromTempTableBySequence(string paraSessionID, string paraUserID, string currentFormID, int paraSequence, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetContactPersonByName(string paraSessionID, string Name, string ContactType, string paraUserID, string currentFormID, string[] conn);
    }

}
