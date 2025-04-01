using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Interface.Inventory;
using System.Data.SqlClient;
using System.ServiceModel.Activation;
using System.ServiceModel;

namespace Domain.Inventory
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public partial class PRSReports : Interface.Inventory.iPRSReports
    {
        Server.Server svr = new Server.Server();
        public DataSet GetDefaultViewData(string WhereCondition, string ObjectName, bool IsApproval, long UserID, string[] conn)
        {
            DataSet ds = new DataSet();
            if (ObjectName == "PartRequisition")
            {
                ds = fillds("Select * from vPRSRequisitions " + WhereCondition + " Order by LastModifiedDate Desc  ", conn);
            }
            else if (ObjectName == "PartIssue")
            {
                ds = fillds("Select * from vPRSIssueDetails " + WhereCondition + " Order by LastModifiedDate Desc  ", conn);
            }
            else if (ObjectName == "PartReceipt")
            {
                ds = fillds("Select * from vPRSGRN " + WhereCondition + " Order by LastModifiedDate Desc  ", conn);
            }
            else if (ObjectName == "PartConsumption")
            {
                ds = fillds("Select * from vPRSConsumption " + WhereCondition + " Order by LastModifiedDate Desc  ", conn);
            }
            else if (ObjectName == "PartStock")
            {
                ds = fillds("Select * from vPRSStockRegister " + WhereCondition + " Order by LastModifiedDate Desc  ", conn);
            }
            return ds;
        }

        protected DataSet fillds(String strquery, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            DataSet ds = new DataSet();

            System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection("Data Source=" + conn[0] + ";Initial Catalog=" + conn[1] + "; User ID=" + conn[3] + "; Password=" + conn[2] + ";");
            SqlDataAdapter da = new SqlDataAdapter(strquery, sqlConn);
            ds.Reset();
            da.Fill(ds);
            return ds;
        }

        public DataSet GetQueryData(string site,string engn,string prd,string fdt,string tdt,long userid,string[] conn )
        {
            DataSet ds = new DataSet();
            ds = fillds("declare @Snm varchar(5000);declare @Eng varchar (5000);declare @Pid varchar (5000);declare @sdt datetime;declare @edt datetime;set @Snm='" + site + "' set @Eng='" + engn + "' set @Pid='" + prd + "' set @sdt='" + fdt + "' set @edt='" + tdt + "' select sum(amount) as amount,Name,SiteName,EngineSerial,IssueQty,ProductId,Description,ConsumptionDate  from vprsdashboard where SiteName in(select part from SplitString(@Snm,',')) and EngineSerial in (select part from SplitString(@Eng,',')) and Name in(select part from SplitString(@Pid,',')) and ConsumptionDate between @sdt and @edt  group by Name,SiteName,EngineSerial,IssueQty,ProductId,Description,ConsumptionDate", conn);

            return ds;
        }
    }
}
