using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Data.Entity;
using System.Data;


namespace Interface.Company
{
    [ServiceContract]
    public partial interface iTermConditionMaster
    {
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mTermsCondition> GetTermConditionList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int InsertmTermsCondition(mTermsCondition term, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int updatemTermsCondition(mTermsCondition updateTerm, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        mTermsCondition GetTermConditionListByID(int termId, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string checkDuplicateRecord(string termName, int groupID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string checkDuplicateRecordEdit(int termID, string termName, int groupID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetTermRecordToBindGrid(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mTermsConditionsGroup> GetGroupListToBindDDL(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetTermRecordToBindGridUC(string groupname, string[] conn);
    }
}
