using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Data.Entity;
using System.Data;

namespace Interface.Inbox
{
    [ServiceContract]
    public partial interface iInbox
    {
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetInboxData_Result> GetInboxDetailByUserID(long UserID, string isAll, string WhereValue, string[] condetails);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet bindInboxDetailData(string ObjectName, string ReferenceID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetInboxDataOfApproval_Result> GetInboxDetailBySiteUserID(long UserID, string Site, string[] condetails);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<POR_vGetInboxData> GetInboxDataByUserID(long UserID, string[] conn);

        #region GWC_Inbox
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetUserInbox(long UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetInbox(long UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void SetArchive(string SelectedRec, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetUserInboxWhere(long UserID, string WhereCondition, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetInboxWhere(long UserID, string WhereCondition, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DateTime GetLastPasswordChangeDate(long UserID, string[] conn);
        #endregion


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string ChkFillSecurityQue(long userid, string[] conn);
    }
}
