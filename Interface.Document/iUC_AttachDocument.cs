using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Data.Entity;
using System.Data;

namespace Interface.Document
{
    [ServiceContract]

    public partial interface iUC_AttachDocument
    {
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetDocumentList_Result> GetDocumentByReferenceId(string BaseObjectName, string TargetObjectName, long ReferenceID, string LoginID, string paraSessionID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void ClearTempData(string paraSessionID, string paraUserID, string paraObjectName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void FinalSaveToDBtDocument(string SessionID, long ReferenceID, string UserID, string TargetObjectName, string HttpAppPath, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetDocumentList_Result> InsertIntoTemp(SP_GetDocumentList_Result newDocument, string SessionID, string UserID, string TargetObjectName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string CheckDuplicateDocumentTitle(string paraSessionID, string DocumentTitle, string paraUserID, string currentFormID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetDocumentList_Result> DeleteDocumentFormTemp(long DeletedSeq, string paraSessionID, string paraUserID, string paraTargetObjectName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetDocumentList_Result> GetExistingTempDataBySessionIDObjectNameToRebind(string paraSessionID, string paraUserID, string paraObjectName, string[] conn);
        #region Dipti
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetImportTemplateData(string ImportId, string[] conn);

        DataSet bindGridQueryList(string objectName, long companyID, string[] conn);
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetImportDesignDetailData_Edit(string getQueryId, string[] conn);
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetEditedImportData(string impid, string TableName, string[] conn);
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetQueryBuilderObject(string companyID, string[] conn);
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet FillObjectDDL(string[] conn);
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet getCustomer(long CompanyID, string[] conn);
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetQueryBuilderObjectNew(long CompanyID, string[] conn);
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int SaveColumnData(string dataColumn, string objectName, string viewName, long companyID, long customerID, long importID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet getCountImportTemplate(string importtitle, long customerid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet getObjectName(string Objectname, string[] conn);

        #endregion
    }
}
