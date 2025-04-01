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
    public partial interface iGRN
    {
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        tGRN GetGRNByID(long RecID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        tGRNDetail GetGRNDetailsByID(long RecID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long InsertGRN(tGRN objGRN, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long updateGRN(tGRN objGRN, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        long InsertGRNDetails(DataTable dt, long GRN_ID, string[] conn);
    }
}
