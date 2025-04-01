using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Data.Entity;

using System.Data;

namespace Interface.Product
{
    [ServiceContract]
    public partial interface iProductCategoryMaster
    {
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<vGetProductCagetoryList> GetProductCategoryList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //int InsertmProductCategory(mProductCategory prdCategory, string[] conn);
        int InsertmProductCategory(mProductCategory prdCategory, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int updatemProductCategory(mProductCategory updatePrdCategory, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        mProductCategory GetProductCategoryListByID(int prdCategoryId, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string checkDuplicateRecord(string prdCategoryName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string checkDuplicateRecordEdit(string prdCategoryName, int prdCategoryID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //DataSet GetPrdCategoryRecordToBindGrid(, string[] conn);
        DataSet GetPrdCategoryRecordToBindGrid(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<vGetProductCagetoryList> GetProductCategoryListForAsset(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mDropdownValue> GetActivityList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mDropdownValue> GetMessageList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long InsertEmailTemplate(mMessageEMailTemplate Template, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<vGetTemplateList1> GetTemplateListForGrid(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateEmailTemplate(string ID, string MailSubject, string MailBody, long ModifiedBy, long CompanyID, long DepartmentID, long ActivityID, long MessageID, string TemplateTitle, string Active, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetAutoCancellationStatus(long DeptID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string checkDuplicateTemplate(long company, long Department, long activity, long MessageType, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string checkDuplicateTemplateEdit(long TemplateID, long company, long Department, long activity, long MessageType, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string checkDuplicateInterface(string Tablename, string FieldName, string DataType, string IsNull, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string checkDuplicateInterfaceEdit(long ID, string Tablename, string FieldName, string DataType, string IsNull, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mDropdownValue> GetDestination(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mDropdownValue> GetActionType(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //DataSet GetObject(string[] conn);
        List<mDropdownValue> GetObject(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mInterfaceMap> FieldList(string TableName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void InsertMessageIntoTemptable(string Title, string Destination, string Type, string Purpose, string Object1, long Field1, long Sequence, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void DeleteMessageTemptable(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void InsertMessageHeader(string Destination, string ActionType, string TableName, string description, long CreatedBy, string Remark, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetMessHeaderID(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void InsrtIntoMessageHeader(long HeaderId, long Sequence, long FieldID, long CreatedBy, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetMessageTempData(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetFieldDetails(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void DeleteFieldFromList(long ID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void DeleteFieldFromListWhenEdit(long ID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetFieldDetailsFromMessageTable(long MessageID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateMessageHeader(long MessageheaderID, string Destination, string ActionType, string TableName, string description, long ModifyBy, string Remark, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void InsertMessageDetails(long MsgHeadID, long Field, long Sequence, long CreatedBy, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<vGetTemplateList1> GetTemplateListForGridAdmin(long UserID, string[] conn);
    }
}
