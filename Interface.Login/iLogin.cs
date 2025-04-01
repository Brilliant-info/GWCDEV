using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

namespace Interface.Login
{
    [ServiceContract]
    public partial interface iLogin
    {
        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //int GetLoginUserNameExist(string LoginUserName, string password);

        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //String FillRoleMasterData(long RoleId, long UserId, long ComapnyID);

        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //string ErrorTracking(string data, string getType, string message, string source, string UserID, string ConnectionString);

        //[OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        //void ErrorTracking(ErrorLog ex, string ConnectionString);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void ErrorTracking(ErrorLog ex, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void ClearTempDataBySessionID(string SessionID, string UserID, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        void ErrorTracking1(string Data, string GetType, string InnerException, string Message, string Source, DateTime DateTime, string UserID, string[] conn);
    }


}
