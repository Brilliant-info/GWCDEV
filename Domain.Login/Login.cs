using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Activation;
using System.Configuration;
using System.ServiceModel;
using Interface.Login;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Data.Objects;
using System.Globalization;
using System.Reflection;
using System.Xml.Linq;
using System.Data;
using System.Collections;
using Domain.Server;
using System.Data;
using System.Data.SqlClient;

namespace Domain.Login
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]

    public partial class Login : Interface.Login.iLogin
    {
        Server.Server svr = new Server.Server();

        public void ErrorTracking(ErrorLog ex, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            ErrorLog error = new ErrorLog();
            error.Data = ex.Data.ToString();
            error.GetType = ex.GetType.ToString();
            error.InnerException = ex.InnerException.ToString();
            error.Message = ex.Message.ToString();
            error.Source = ex.Source.ToString();
            error.DateTime = DateTime.Now;
            error.UserID = ex.UserID;
            //ce.AddToErrorLogs(error);
            //ce.SaveChanges();
                                                         
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Sp_EnterErrorTracking";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("Data", error.Data);
            cmd.Parameters.AddWithValue("GetType", error.GetType);
            cmd.Parameters.AddWithValue("InnerException", error.InnerException);
            cmd.Parameters.AddWithValue("Message", error.Message);
            cmd.Parameters.AddWithValue("Source", error.Source);
            cmd.Parameters.AddWithValue("DateTime", error.DateTime);
            cmd.Parameters.AddWithValue("UserID", error.UserID);
            cmd.ExecuteNonQuery();

        }

        public void ErrorTracking1(string Data, string GetType, string InnerException, string Message, string Source, DateTime DateTime, string UserID, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Sp_EnterErrorTracking";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("Data", Data);
            cmd.Parameters.AddWithValue("GetType", GetType);
            cmd.Parameters.AddWithValue("InnerException", InnerException);
            cmd.Parameters.AddWithValue("Message", Message);
            cmd.Parameters.AddWithValue("Source", Source);
            cmd.Parameters.AddWithValue("DateTime", DateTime);
            cmd.Parameters.AddWithValue("UserID", UserID);
            cmd.ExecuteNonQuery();
        }

        



        public void ClearTempDataBySessionID(string SessionID, string UserID, string[] conn)
        {

            SqlConnection con = new SqlConnection("Data Source=" + conn[0].ToString() + ";Initial Catalog=" + conn[1].ToString() + ";Persist Security Info=True;User ID=" + conn[3].ToString() + ";Password=" + conn[2].ToString() + ";");
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "Delete from TempData Where SessionID = '" + SessionID + "' and UserID = '" + UserID + "'  Delete from TempCartProductLevelTaxDetail Where SessionID = '" + SessionID + "'";
            cmd.Connection = con;
            if (con.State == ConnectionState.Closed) con.Open();
            cmd.ExecuteNonQuery();
            con.Close();
        }

    }
}
