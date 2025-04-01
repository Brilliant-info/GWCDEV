using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Data.Entity;
using System.Data;
using ElegantCRM.Model;

namespace Interface.Report
{
    [ServiceContract]
    public partial interface iReportBuilder
    {
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<ElegantCRM.Model.sysmModule> GetModuleList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<ElegantCRM.Model.sysmModule> GetMList(string[] conn);

        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //List<systObjectModuleMapping> GetObjectList(String ObjId);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetObjectList(string ModuleName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<sysTableAliceMaster> FillAlice(String m, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetDatatype(String m, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<sysmObjectDetail> GetChildObjectName(String ObjName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetColumnName(string ObjName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mOperationalMaster> GetOperationalColumn(string objname, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet filltreeView(String mergeobjectname, String HfValue, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet fillOperationalGrid(String hfvalue, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet fillField(String ColName, String tableName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet fillDatatype(String selectedField, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet fillbtnCriteria(String HFValue, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet fillselectQuery(String ObjName, String childObj, string[] conn);

        //  [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //DataSet fillTableName(String ObjName);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int InsertReportMaster(mReportMaster QB, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet FillHomeGV(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet CheckRptName(String RptName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int UpdateReportMaster(mReportMaster QB, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        mReportMaster GetRecordForUpdate(Int64 RptId, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetInfoForRequest(Int64 ID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetObjListWit(Int64 ID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetDates(String ObjName, string[] conn);
    }
}
