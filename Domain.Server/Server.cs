using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data.EntityClient;
using System.Data;

namespace Domain.Server
{
    public class Server
    {
        public EntityConnection GetEntityConnection(connectiondetails ApplicationConnection)
        {
            EntityConnection conn = new EntityConnection();
            if (ApplicationConnection.DataBaseName != null)
            {
                SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder();
                sqlBuilder.DataSource = ApplicationConnection.DataSource;// @"Server1\MySERVER";
                sqlBuilder.InitialCatalog = ApplicationConnection.DataBaseName;//"MyNewDatabase";
                sqlBuilder.IntegratedSecurity = false;
                sqlBuilder.MultipleActiveResultSets = true;
                sqlBuilder.UserID = ApplicationConnection.DataBaseName;
                sqlBuilder.Password = ApplicationConnection.DBPassword;

                EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder();
                entityBuilder.Provider = "System.Data.SqlClient";
                entityBuilder.ProviderConnectionString = sqlBuilder.ToString();
                entityBuilder.Metadata = @"res://*/ElegantCRMDataModel.csdl|res://*/ElegantCRMDataModel.ssdl|res://*/ElegantCRMDataModel.msl";

                conn = new EntityConnection(entityBuilder.ToString());

            }
            return conn;
        }

        public EntityConnection GetEntityConnection(string[] constr)
        {
            EntityConnection conn = new EntityConnection();
            if (constr != null)
            {
                SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder();

                sqlBuilder.DataSource = constr[0];//ApplicationConnection.DataSource;// @"Server1\MySERVER";
                sqlBuilder.InitialCatalog = constr[1];//ApplicationConnection.DataBaseName;//"MyNewDatabase";
                sqlBuilder.IntegratedSecurity = false;
                sqlBuilder.MultipleActiveResultSets = true;
                sqlBuilder.Password = constr[2];//ApplicationConnection.DBPassword;
                sqlBuilder.UserID = constr[3];//ApplicationConnection.DataBaseName;
                sqlBuilder.MaxPoolSize = 2000;
                sqlBuilder.ConnectTimeout = 0;

                EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder();
                entityBuilder.Provider = "System.Data.SqlClient";
                entityBuilder.ProviderConnectionString = sqlBuilder.ToString();
                entityBuilder.Metadata = @"res://*/ElegantCRMDataModel.csdl|res://*/ElegantCRMDataModel.ssdl|res://*/ElegantCRMDataModel.msl";

                conn = new EntityConnection(entityBuilder.ToString());

            }
            return conn;
        }

        public EntityConnection GetEntityConnection(string strDataSource, string strInitialCatalog, string strDBPassword, string strDataBaseName)
        {
            EntityConnection conn = new EntityConnection();
            if (strDataSource != null && strInitialCatalog != null && strDBPassword != null && strDataBaseName != null)
            {
                SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder();

                sqlBuilder.DataSource = strDataSource;//ApplicationConnection.DataSource;// @"Server1\MySERVER";
                sqlBuilder.InitialCatalog = strInitialCatalog;//ApplicationConnection.DataBaseName;//"MyNewDatabase";
                sqlBuilder.IntegratedSecurity = false;
                sqlBuilder.MultipleActiveResultSets = true;
                sqlBuilder.Password = strDBPassword;//ApplicationConnection.DBPassword;
                sqlBuilder.UserID = strDataBaseName;//ApplicationConnection.DataBaseName;

                EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder();
                entityBuilder.Provider = "System.Data.SqlClient";
                entityBuilder.ProviderConnectionString = sqlBuilder.ToString();
                entityBuilder.Metadata = @"res://*/ElegantCRMDataModel.csdl|res://*/ElegantCRMDataModel.ssdl|res://*/ElegantCRMDataModel.msl";

                conn = new EntityConnection(entityBuilder.ToString());

            }
            return conn;
        }

        public DataSet FillDataSet(string Query, string[] conn)
        {
            DataSet dsServer = new DataSet();
            System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection("Data Source=" + conn[0] + ";Initial Catalog=" + conn[1] + "; User ID=" + conn[3] + "; Password=" + conn[2] + ";");
            SqlDataAdapter da = new SqlDataAdapter(Query, sqlConn);
            da.Fill(dsServer);
            return dsServer;
        }

        public SqlConnection GetSqlConn(string[] conn)
        {
            System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection("Data Source=" + conn[0] + ";Initial Catalog=" + conn[1] + "; User ID=" + conn[3] + "; Password=" + conn[2] + ";");
            //{
                sqlConn.Open();
                return sqlConn;
            //}
        }
    }

    public class connectiondetails
    {
        public string DataSource { get; set; }
        public string DataBaseName { get; set; }
        public string DBPassword { get; set; }
    }

}
