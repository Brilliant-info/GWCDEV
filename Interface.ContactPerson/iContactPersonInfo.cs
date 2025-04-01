using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Data.Entity;

using System.Data;

namespace Interface.ContactPerson
{
    [ServiceContract]
    public partial interface iContactPersonInfo
    {
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mContactType> GetContactTypeList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void SaveTempDataToDB(List<SP_GetContactPersonListToBindGrid_Result> paraobjList, string paraSessionID, string paraUserID, string currentformid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void ClearTempDataFromDB(string paraSessionID, string paraUserID, string currentformid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetContactPersonListToBindGrid_Result> InsertIntoTemp(SP_GetContactPersonListToBindGrid_Result ContactPerson, string paraSessionID, string paraUserID, string currentformid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void SetValuesToTempData_onChange(string SessionID, string UserID, string TargetObjectName, int Sequence, SP_GetContactPersonListToBindGrid_Result paraInput, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        SP_GetContactPersonListToBindGrid_Result GetContactDetailFromTempTableBySequence(string paraSessionID, string paraUserID, string currentFormID, int paraSequence, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetContactPersonListToBindGrid_Result> GetExistingTempDataBySessionIDFormID(string paraSessionID, string paraUserID, string currentformid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string CheckDuplicateContactPersonName(string SessionID, string Name, long Sequence, string UserID, string TargetObjectName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void FinalSaveToDBtAddToContactPerson(string paraSessionID, string TargetObjectName, long paraReferenceID, string paraUserID, string paraSaveObjectName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        SP_GetContactPersonListToBindGrid_Result GetContactPersonTempDataBySequence(long SequenceNo, string SessionID, string TargetObjectName, string UserID, string[] conn);
                
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetContactPersonListToBindGrid_Result> GetContactPersonByObjectNameReferenceID(string SourceObjectName, long ReferenceID, string TargetObjectName, string SessionID, string UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetContactPersonListToBindGrid_Result> GetContactPersonTempData(string TargetObjectName, long SelectedSeq, string SessionID, string UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void SetContactPersonArchive(string Ids, string isDeleted, string userId, string TargetObjectName, string SessionID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetContactPersonListToBindGridLead_Result> GetContactPersonByObjectNameReferenceIDLead(string SourceObjectName, long ReferenceID, string TargetObjectName, string SessionID, string UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetDepartmentcontact(long ParentID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet Getcontactbydeptid(long Deartment, string[] conn);
    }
}
