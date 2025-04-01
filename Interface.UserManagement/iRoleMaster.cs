using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Data.Entity;

using System.Data;

namespace Interface.UserManagement
{
    [ServiceContract]
    public partial interface iRoleMaster
    {
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetDataToBindRoleMaster_Result> GetDataToBindRoleMasterDetailsByRoleID(long RoleID, long CompanyID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GetDataToBindRoleMaster_Result> UpdateRoleIntoSessionList(List<SP_GetDataToBindRoleMaster_Result> sessionList, SP_GetDataToBindRoleMaster_Result updateRole, int rowindex, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void FinalSave(List<SP_GetDataToBindRoleMaster_Result> sessionList, mRole objmRole, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        mRole GetmRoleByID(long roleId, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<vGetRoleMasterData> BindRoleMasterSummary(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<vGetRoleMasterData> GetRoleMasterListByDepartmentIDDesignationID(long DepartmentID, long DesignationID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<VGetRollNameByDeptID> GetRoleMasterListByDepartmentID(long DepartmentID, long DesignationID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string checkDuplicateRecord(string RoleName, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        string checkDuplicateRecordEdit(int RoleId, string RoleName, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GWCGetDataToBindRoleMaster_Result> GetGWCDataToBindRoleMasterDetailsByRoleID(long RoleID, long CompanyID, string[] conn);


        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void FinalSaveRole(List<SP_GWCGetDataToBindRoleMaster_Result> sessionList, mRole objmRole, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<vGetGWCRoleMasterData> BindGWCRoleMasterSummary(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<mRole> GetRoleList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<SP_GWCGetDataToBindRoleMaster_Result> GWCUpdateRoleIntoSessionList(List<SP_GWCGetDataToBindRoleMaster_Result> sessionList, SP_GWCGetDataToBindRoleMaster_Result updateRole, int rowindex, string[] conn);
       
     
    }
}
