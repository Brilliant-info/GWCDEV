using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.ServiceModel;
using System.Data.Entity;



namespace Interface.Product
{
    [ServiceContract]
    public partial interface iTitleMaster
    {
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mTitle> GetTitleList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int InsertIntomTitle(mTitle objmTitle, string state, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string checkDuplicateRecord(string TitleName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string checkDuplicateRecordEdit(string TitleName, int TitleID, string[] conn);
        
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        mTitle GetTitleByTitleID(long TitleId, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mTitle> GetActiveTitleRecords(string[] conn);
    }
}
