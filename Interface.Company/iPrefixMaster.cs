using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Entity;
using System.ServiceModel;


namespace Interface.Company
{
    [ServiceContract]

    public partial interface iPrefixMaster
    {
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mPrefixMaster> GetPrefixList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int InsertmPrefixMaster(mPrefixMaster Pref, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int UpdatemPrefixMaster(mPrefixMaster UpdtPref, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        mPrefixMaster GetPrefixMasterByID(int PrefID, string[] conn);

        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //string checkDuplicateRecord(string ObjName, int ObjPreID, string[] conn);

        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //string checkDuplicateRecordEdit(int PrID, string ObjNm, string[] conn);

        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //DataSet GetPrefixRecordToBindGrid(string[] conn);

        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //List<mPrefixMaster> GetObjectListToBindDDL(string[] conn);

    }
}
