using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Data.Entity;

using System.Data;
using System.Data.SqlClient;
namespace Interface.Inventory
{
     [ServiceContract]
   public partial interface iMaterialIssue
    {
         [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
         tMIN GetMaterialIssueByID(long RecID, string[] conn);

         [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
         tMINDetail GetMaterialIssueDetailsByID(long RecID, string[] conn);

         [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
         long InsertMaterialIssue(tMIN objMaterialIssue, string[] conn);

         [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
         long updateMaterialIssue(tMIN objMaterialIssue, string[] conn);

         [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
         long InsertMaterialIssueDetails(DataTable dt, long PRM_ID, string[] conn);

         [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
         long UpdateMaterialIssueDetail(tMINDetail objMaterialIssueDetails, string[] conn);

        
       
    }
}
