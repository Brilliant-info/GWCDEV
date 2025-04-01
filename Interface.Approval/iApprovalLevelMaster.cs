using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Data.Entity;

using System.Data;

namespace Interface.Approval
{
    [ServiceContract]
    public partial interface iApprovalLevelMaster
    {
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mApprovalLevel> GetActivityList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int InsertmApprovalLevel(mApprovalLevel approval, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int updatemApprovalLevel(mApprovalLevel updateApproval, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        mApprovalLevel GetApprovalRecordByID(int approvalId, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mApprovalLevelDetail> GetApprovalDetailRecordByID(int approvalDetailId, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<sysmObject> GetObjectList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        Int32 GetApprovalLevelMax(string objName, long TerritoryID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetApprovalRecordToBindGrid(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mApprovalLevelDetail> GetUserListForEdit(int approvalid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetUserForApprovalMaster_Result> GetUserListForEditbySP(int approvalid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        bool SaveApprovalLevelDetail(string paraUserIDs, long paraApprovalLevelID, mApprovalLevelDetail paraInput, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetUserForApprovalMaster_Result> GetUserListForEditbySPResult(int approvalid, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        Int32 GetApprovalLevel(long CompanyID, long DeptID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GWCGetUserForApprovalMaster_Result> GWCGetUserForApprovalMaster(long CurrentApprovalLevelID, string UserIDS, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<vGWCGetApprovelevelDetail> GetGWCApprovalRecordToBindGrid(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long GetCancelDays(long DeptID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void InsertGWCApprovalDetails(mApprovalLevelDetail ApprovalDetails, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //List<vGWCGetApprovalDetailsForEdit> GWCGetApprovalDetailsForEdit(long ApprovalLevelID, string[] conn);
        DataSet GWCGetApprovalDetailsForEdit(long ApprovalLevelID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GWCGetApprovalDetailForEditWithZero(long ApprovalLevelID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetApproverList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GWCGetApproverListBySp_Result> GetApproverListBySp(long CurrentApprovalLevelID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void DeleteApproverFromGrid(long ID,long UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateApprovallevelID(long ApprovalLevelID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateMApproveHeader(long ID, int ApprovalLevel, int NoOfApprovers, string LastModifiedBy, DateTime LastModifiedDate, long TerritoryID, long OrderCancelInDays, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void DeleteZeroDetailaproval(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetApprovalgridList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int GetApprovalRecordByApvrlLevelID(int ApprovalID, long TerritoryID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string GetApprovalLevelApproverLogicvalue(long ApprovalID, string[] conn);

        #region ERP

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetTriggerList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetReTriggerOrderHead(long Id, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetReTriggerOrderDetails(long Id, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateReTriggerDetails(long Id, string RequestStatus, string ServiceDescription, string reqeustresponse, string[] conn);
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void UpdateStatus(long OrderID,string status, string[] conn);
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet getErpAPROrderhead(long orderID, string[] conn);
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet getErpAPROrderdetail(long orderID, string[] conn);
        #endregion


    }
}
