using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Activation;
using System.ServiceModel;
using System.Configuration;
//using ElegantCRM.Model;
//using System.ServiceModel;
using System.Xml.Linq;
using System.Data.Objects;
using Domain.Server;
using System.Data.Linq;
using System.Data.Linq.SqlClient;
using System.Data;
using System.Data.SqlClient;
using Interface.PowerOnRent;
using System.Data.Entity;
namespace Domain.PowerOnRent
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]

    public partial class UCCommonFilter : iUCCommonFilter
    {
        Domain.Server.Server svr = new Server.Server();

        #region GetSiteNameByUserID

        public List<mTerritory> GetSiteNameByUserID(long uid, string[] conn)
        {
            using (BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                List<mTerritory> SiteName = new List<mTerritory>();
                SiteName = (from mT in ce.mTerritories
                            join mU in ce.mUserTerritoryDetails on mT.ID equals mU.TerritoryID
                            where mU.UserID == uid
                            orderby mT.Territory
                            select mT).ToList();
                return SiteName;
            }
        }


        public DataSet GetSiteNameByUserIDNew(long uid, string[] conn)
        {
            DataSet dsdept = new DataSet();
            dsdept = fillds("select * from mterritory MT left outer join mUserTerritoryDetail MUT on MT.id=MUT.TerritoryID where MT.Userid=" + uid + "   order by MT.Territory", conn);
            return dsdept;
        }

        public List<mTerritory> GetAllSites(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mTerritory> SiteName = new List<mTerritory>();

            SiteName = (from mT in ce.mTerritories
                        orderby mT.Territory
                        select mT).ToList();
            return SiteName;
        }
        #endregion



        public List<v_GetEngineDetails> GetEngineOfSite(string SId, string[] conn)
        {
            string[] SiteID1 = SId.Split(',').ToArray();
            long[] ids = Array.ConvertAll(SiteID1, long.Parse);
            using (BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                List<v_GetEngineDetails> ListEngines = new List<v_GetEngineDetails>();
                //ListEngines = ce.v_GetEngineDetails.Where(v => SiteID1.Contains(v.SiteID)).ToList();
                ListEngines = (from v in ce.v_GetEngineDetails
                               join s in ids.AsEnumerable() on v.SiteID equals s
                               select v).ToList();
                return ListEngines;
            }
        }


        public List<mStatu> GetStatusListByOjbect(string ObjectName, string Remark, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mStatu> statusdetail = new List<mStatu>();
            string[] RemarkArr = Remark.Split(',');
            if (Remark != "")
            {
                statusdetail = (from st in db.mStatus
                                where (st.ObjectName == ObjectName && RemarkArr.Contains(st.Remark))
                                select st).OrderBy(st => st.Sequence).ToList();
            }
            else
            {
                statusdetail = (from st in db.mStatus
                                where (st.ObjectName == ObjectName)
                                select st).OrderBy(st => st.Sequence).ToList();
            }
            return statusdetail;
        }

        public List<vGetUserProfileByUserID> GetUserListBySiteID(long siteID, string[] conn)
        {
            List<vGetUserProfileByUserID> UsersList = new List<vGetUserProfileByUserID>();
            List<vGetUserProfileByUserID> UsersListDistinct = new List<vGetUserProfileByUserID>();
            try
            {
                using (BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
                {
                    if (siteID == 0)
                    {
                        UsersList = (from v in db.vGetUserProfileByUserIDs
                                     join tu in db.mUserTerritoryDetails on v.userID equals tu.UserID
                                     select v).OrderBy(v => v.userName).ToList();
                    }
                    else
                    {
                        UsersList = (from v in db.vGetUserProfileByUserIDs
                                     join tu in db.mUserTerritoryDetails on v.userID equals tu.UserID
                                     where tu.TerritoryID == siteID
                                     select v).OrderBy(v => v.userName).ToList();
                    }

                    UsersListDistinct = (from u in UsersList
                                         select u).Distinct().ToList();
                }
            }
            catch { }
            return UsersListDistinct;
        }

        //public List<GetProductDetail> GetProductOfEngine(string EngId, string[] conn)
        //{
        //    string[] EngID1 = EngId.Split(',').ToArray();
        //    long[] idP = Array.ConvertAll(EngID1, long.Parse);
        //    BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
        //    List<GetProductDetail> ProductList = new List<GetProductDetail>();
        //    ProductList = (from v in ce.GetProductDetails
        //                   join s in idP.AsEnumerable() on v.ID equals s
        //                   select v).ToList();
        //    return ProductList;
        //}

        public DataSet GetProductOfSelectedEngine(string EngId, string filter, string frmdt, string todt, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<v_GetEngineDetails> ListEngines = new List<v_GetEngineDetails>();
            DataSet ds = new DataSet();
            if (filter == "")
            {
                //ds = fillds("select distinct PD.ID,pd.ProductCode,PD.Name,PD.PrincipalPrice,PD.ProductCategory,PD.ProductSubCategory,PD.UOM from GetProductDetails as PD right outer join dbo.PORtConsumptionDetail as CD on PD.id=CD.Prod_ID left outer join dbo.PORtConsumptionHead  as CH on CH.ConH_ID=CD.ConH_ID left outer join dbo.mEngine as ME on CH.EngineSerial=ME.EngineSerial where me.EngineSerial in (" + EngId + ") and ConsumptionDate >='"+ frmdt +"' and ConsumptionDate <='" + todt + "' ", conn);
                ds = fillds("DECLARE @d1 DATETIME DECLARE @d2 DATETIME SET @d1= '" + frmdt + "' SET @d2= '" + todt + "' select distinct PD.ID,pd.ProductCode,PD.Name,PD.PrincipalPrice,PD.ProductCategory,PD.ProductSubCategory,PD.UOM from GetProductDetails as PD right outer join dbo.PORtConsumptionDetail as CD on PD.id=CD.Prod_ID left outer join dbo.PORtConsumptionHead  as CH on CH.ConH_ID=CD.ConH_ID left outer join dbo.mEngine as ME on CH.EngineSerial=ME.EngineSerial where me.EngineSerial in (" + EngId + ") and CH.ConsumptionDate >= convert(varchar(12) ,@d1 , 101) and CH.ConsumptionDate <= convert(varchar(12) ,@d2 , 101) ", conn);
            }
            else
            {
                // ds = fillds("select distinct PD.ID,pd.ProductCode,PD.Name,PD.PrincipalPrice,PD.ProductCategory,PD.ProductSubCategory,PD.UOM from GetProductDetails as PD right outer join dbo.PORtConsumptionDetail as CD on PD.id=CD.Prod_ID left outer join dbo.PORtConsumptionHead  as CH on CH.ConH_ID=CD.ConH_ID left outer join dbo.mEngine as ME on CH.EngineSerial=ME.EngineSerial where me.EngineSerial in (" + EngId + ") and(pd.ProductCode like '%" + filter + "%' or PD.Name like '%" + filter + "%' or PD.ProductCategory like '%" + filter + "%' ) and ConsumptionDate >='" + frmdt + "' and ConsumptionDate <='" + todt + "'   ", conn);
                ds = fillds("DECLARE @d1 DATETIME DECLARE @d2 DATETIME SET @d1= '" + frmdt + "' SET @d2= '" + todt + "' select distinct PD.ID,pd.ProductCode,PD.Name,PD.PrincipalPrice,PD.ProductCategory,PD.ProductSubCategory,PD.UOM from GetProductDetails as PD right outer join dbo.PORtConsumptionDetail as CD on PD.id=CD.Prod_ID left outer join dbo.PORtConsumptionHead  as CH on CH.ConH_ID=CD.ConH_ID left outer join dbo.mEngine as ME on CH.EngineSerial=ME.EngineSerial where me.EngineSerial in (" + EngId + ") and CH.ConsumptionDate >= convert(varchar(12) ,@d1 , 101) and CH.ConsumptionDate <= convert(varchar(12) ,@d2 , 101) and (pd.ProductCode like '%" + filter + "%' or PD.Name like '%" + filter + "%' or PD.ProductCategory like '%" + filter + "%' ) ", conn);
            }
            return ds;
        }

        public DataSet GetProductofRequest(string reqid, string filter, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            //List<GetProductDetail> ProductList = new List<GetProductDetail>();
            DataSet ds = new DataSet();
            if (filter == "")
            {
                ds = fillds("select id,ProductCode,Name,Description,ProductCategory from GetProductDetails where id in (select prod_Id from PORtPartRequestDetail where PRH_ID in(" + reqid + ") )", conn);
            }
            else
            {
                ds = fillds("select id,ProductCode,Name,Description,ProductCategory from GetProductDetails where id in (select prod_Id from PORtPartRequestDetail where PRH_ID in(" + reqid + ") ) and (ProductCode like '%" + filter + "%' or Name like '%" + filter + "%'  or  ProductCategory like '%" + filter + "%'  ) ", conn);
            }
            return ds;
        }

        public DataSet GetProductofIssue(string issueid, string filter, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            //List<GetProductDetail> ProductList = new List<GetProductDetail>();
            DataSet ds = new DataSet();
            if (filter == "")
            {
                ds = fillds("select * from GetProductDetails where id in(select Prod_id from PORtMINDetail where MINH_ID in(" + issueid + "))", conn);
            }
            else
            {
                ds = fillds("select * from GetProductDetails where id in(select Prod_id from PORtMINDetail where MINH_ID in(" + issueid + ")) and (ProductCode like '%" + filter + "%' or Name like '%" + filter + "%'  or  ProductCategory like '%" + filter + "%'  )  ", conn);
            }
            return ds;
        }

        public DataSet GetProductofReceipt(string receiptID, string filter, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            //List<GetProductDetail> ProductList = new List<GetProductDetail>();
            DataSet ds = new DataSet();
            if (filter == "")
            {
                ds = fillds("select * from GetProductDetails where id in(select Prod_id from PORtGRNDetail where GRNH_ID in(" + receiptID + "))", conn);
            }
            else
            {
                ds = fillds("select * from GetProductDetails where id in(select Prod_id from PORtGRNDetail where GRNH_ID in(" + receiptID + ")) and (ProductCode like '%" + filter + "%' or Name like '%" + filter + "%'  or  ProductCategory like '%" + filter + "%'  ) ", conn);
            }
            return ds;
        }

        public DataSet GetReportData(string SiteId, string EngLst, string PrdLst, string frmdt, string todt, string objectname, string[] conn)
        {
            DataSet ds = new DataSet();
            //ds = fillds("select * from PORvGetConsumption where SiteId in(" + SiteId + ") and EngineId in(" + EngLst + ") and ProductId in (" + PrdLst + ") and ConsumptionDate between '" + frmdt + "' and '" + todt + "' and StartDate <='" + frmdt + "' and EndDate >= '" + todt + "' ", conn);
            //ds = fillds(" select f.FN_Date,case when f.FN_Date=por.ConsumptionDate then ConsumedQty else 0 end as ConsumedQty,amount,ProductCode,[Description],Name,Rate,StartDate,EndDate,ConsumptionNo,EngineModel,EngineSerial,GeneratorModel,ConsumptionDate,ConsumedByUserID,Territory,ProductId,EngineId,SiteId,Category from PORvGetConsumption por  right outer join dbo.FT_GetInBetweenDates('" + frmdt + "','" + todt + "') f on por.ConsumptionDate=f.FN_Date   and SiteId in(" + SiteId + ") and EngineId in(" + EngLst + ") and ProductId in (" + PrdLst + ") and    StartDate <='" + frmdt + "' and EndDate >= '" + todt + "' ", conn);
            // ds = fillds("select * from [dbo].[POR_FN_GetConsumptionDayWise1] (' " + frmdt +  " ',' " + todt + " ') WHERE SiteID in (" + SiteId + ") and (ProductID in (" + PrdLst + ") or ProductID is null)", conn);
            ds = fillds("select distinct fnDate,ConsumedQty,amount,ProductCode,ProductId,Name,Rate,ConsumptionNo,SiteName,SiteId,Category,PreviousStock from [dbo].[POR_FN_GetConsumptionDayWise1] (' " + frmdt + " ',' " + todt + " ') WHERE SiteID in (" + SiteId + ") and (ProductID in (" + PrdLst + ") or ProductID is null)", conn);
            return ds;
        }

        public DataSet GetAllReportData(string SiteId, string frmdt, string todt, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select distinct fnDate,ConsumedQty,amount,ProductCode,ProductId,Name,Rate,ConsumptionNo,SiteName,SiteId,Category,PreviousStock,AvailableBalance  from [dbo].[POR_FN_GetConsumptionDayWise1] (' " + frmdt + " ',' " + todt + " ') WHERE SiteID in (" + SiteId + ") ", conn);
            return ds;
        }

        //public DataSet GetReportDataAllEngine(string SiteId, string EngLst, string PrdLst, string frmdt, string todt, string objectname, string[] conn)
        //{
        //    DataSet ds = new DataSet();             
        //    ds = fillds("select distinct fnDate,ConsumedQty,amount,ProductCode,ProductId,Name,Rate,ConsumptionNo,SiteName,SiteId,Category,PreviousStock from [dbo].[POR_FN_GetConsumptionDayWise1] (' " + frmdt + " ',' " + todt + " ') WHERE SiteID in (" + SiteId + ") and (ProductID in (" + PrdLst + ") or ProductID is null)", conn);
        //    return ds;
        //}

        //public DataSet GetReportDataAllPrd(string SiteId, string EngLst, string PrdLst, string frmdt, string todt, string objectname, string[] conn)
        //{
        //    DataSet ds = new DataSet();             
        //    ds = fillds("select distinct fnDate,ConsumedQty,amount,ProductCode,ProductId,Name,Rate,ConsumptionNo,SiteName,SiteId,Category,PreviousStock from [dbo].[POR_FN_GetConsumptionDayWise1] (' " + frmdt + " ',' " + todt + " ') WHERE SiteID in (" + SiteId + ") and (ProductID in (" + PrdLst + ") or ProductID is null)", conn);
        //    return ds;
        //}

        public DataSet GetRequisitionData(string SId, string ReqLst, string PLst, string fdt, string tdt, string objectnm, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("declare @reqid varchar(100);set @reqid=('" + ReqLst + "')  select pr.PRH_ID,pr.SiteID,pr.RequestNo,pr.RequestDate,pr.RequestBy,pr.EngineSerial,pr.EngineModel,pr.GeneratorModel,pr.GeneratorSerial,pr.TransformerSerial,  pr.Container,pd.Prod_ID,pd.Prod_Name,pd.Prod_Description,pd.RemaningQty,pd.RequestQty,pd.Prod_Code ,uph.FirstName,uph.LastName,mt.Territory,MS.[Status],PR.StatusID,pr.title from dbo.mUserProfileHead as UPH right outer join  dbo.PORtPartRequestHead as PR  on PR.RequestBy=UPH.ID left outer join  dbo.mTerritory as MT on PR.SiteID = mt.ID  left outer join dbo.mstatus as MS on  pr.StatusID=MS.ID left outer join dbo.PORtPartRequestDetail as PD  on pr.PRH_ID=pd.PRH_ID where pr.SiteID in (" + SId + ") and pr.PRH_ID in(select part from SplitString(@reqid,',')) and pd.Prod_ID in(" + PLst + ") and  pr.RequestDate between '" + fdt + "' and '" + tdt + "' ", conn);
            return ds;
        }
        public DataSet GetAllRequisitionData(string SId, string fdt, string tdt, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select pr.PRH_ID,pr.SiteID,pr.RequestNo,pr.RequestDate,pr.RequestBy,pr.EngineSerial,pr.EngineModel,pr.GeneratorModel,pr.GeneratorSerial,pr.TransformerSerial,  pr.Container,pd.Prod_ID,pd.Prod_Name,pd.Prod_Description,pd.RemaningQty,pd.RequestQty,pd.Prod_Code ,uph.FirstName,uph.LastName,mt.Territory,MS.[Status],PR.StatusID,pr.title from dbo.mUserProfileHead as UPH right outer join  dbo.PORtPartRequestHead as PR  on PR.RequestBy=UPH.ID left outer join  dbo.mTerritory as MT on PR.SiteID = mt.ID  left outer join dbo.mstatus as MS on  pr.StatusID=MS.ID left outer join dbo.PORtPartRequestDetail as PD  on pr.PRH_ID=pd.PRH_ID where pr.SiteID in (" + SId + ") and  pr.RequestDate between '" + fdt + "' and '" + tdt + "' ", conn);
            return ds;
        }
        public DataSet GetAllRequisitionDataAllRequest(string SId, string ReqLst, string PLst, string fdt, string tdt, string objectnm, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select pr.PRH_ID,pr.SiteID,pr.RequestNo,pr.RequestDate,pr.RequestBy,pr.EngineSerial,pr.EngineModel,pr.GeneratorModel,pr.GeneratorSerial,pr.TransformerSerial,  pr.Container,pd.Prod_ID,pd.Prod_Name,pd.Prod_Description,pd.RemaningQty,pd.RequestQty,pd.Prod_Code ,uph.FirstName,uph.LastName,mt.Territory,MS.[Status],PR.StatusID,pr.title from dbo.mUserProfileHead as UPH right outer join  dbo.PORtPartRequestHead as PR  on PR.RequestBy=UPH.ID left outer join  dbo.mTerritory as MT on PR.SiteID = mt.ID  left outer join dbo.mstatus as MS on  pr.StatusID=MS.ID left outer join dbo.PORtPartRequestDetail as PD  on pr.PRH_ID=pd.PRH_ID where pr.SiteID in (" + SId + ") and  pd.Prod_ID in(" + PLst + ") and  pr.RequestDate between '" + fdt + "' and '" + tdt + "' ", conn);
            return ds;
        }

        public DataSet GetAllRequisitionDataAllPrd(string SId, string ReqLst, string PLst, string fdt, string tdt, string objectnm, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("declare @reqid varchar(100);set @reqid=('" + ReqLst + "')  select pr.PRH_ID,pr.SiteID,pr.RequestNo,pr.RequestDate,pr.RequestBy,pr.EngineSerial,pr.EngineModel,pr.GeneratorModel,pr.GeneratorSerial,pr.TransformerSerial,  pr.Container,pd.Prod_ID,pd.Prod_Name,pd.Prod_Description,pd.RemaningQty,pd.RequestQty,pd.Prod_Code ,uph.FirstName,uph.LastName,mt.Territory,MS.[Status],PR.StatusID,pr.title from dbo.mUserProfileHead as UPH right outer join  dbo.PORtPartRequestHead as PR  on PR.RequestBy=UPH.ID left outer join  dbo.mTerritory as MT on PR.SiteID = mt.ID  left outer join dbo.mstatus as MS on  pr.StatusID=MS.ID left outer join dbo.PORtPartRequestDetail as PD  on pr.PRH_ID=pd.PRH_ID where pr.SiteID in (" + SId + ") and pr.PRH_ID in(select part from SplitString(@reqid,',')) and  pr.RequestDate between '" + fdt + "' and '" + tdt + "' ", conn);
            return ds;
        }



        public DataSet GetIssueData(string SiteId, string IssueLst, string PLst, string fdt, string tdt, string objectnm, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select mh.MINH_ID,mh.MIN_NO,mh.SiteID,mh.IssueDate,mh.IssuedByUserID,md.Prod_ID,md.Prod_Code,md.Prod_Description,md.Prod_Name,md.IssuedQty,mt.Territory,ms.[Status],mh.title from dbo.PORtMINHead as MH left outer join  dbo.mTerritory as mt on mh.SiteID=mt.ID left outer join dbo.mStatus as MS on MH.StatusID=ms.ID left outer join dbo.PORtMINDetail as MD on mh.MINH_ID=md.MINH_ID where mh.SiteID in(" + SiteId + ") and mh.MINH_ID in(" + IssueLst + ") and md.Prod_ID in(" + PLst + ") and mh.IssueDate between '" + fdt + "' and '" + tdt + "' ", conn);
            // ds = fillds("select mh.MINH_ID,mh.PRH_ID,PRD.RequestQty,Isnull(MD.IssuedQty,0) IssuedQty,PRD.Prod_Code,MT.Territory,MS.[Status],MH.StatusID,PRD.Prod_ID,PRD.Prod_Name,PRD.RemaningQty from dbo.PORtMINHead as MH left outer join  dbo.PORtMINDetail MD ON MH.MINH_ID=MD.MINH_ID left outer join dbo.PORtPartRequestDetail as PRD ON MH.PRH_ID=PRD.PRH_ID and MD.Prod_ID=PRD.Prod_ID left outer join  dbo.mTerritory MT ON MH.SiteID=MT.ID left outer join dbo.mStatus MS on MH.StatusID =MS.ID where MH.SiteID in (" + SiteId + ") and mh.MINH_ID in (" + IssueLst + ") and PRD.Prod_ID in (" + PLst + ") and MH.IssueDate between '" + fdt + "' and '" + tdt + "' ", conn);
            return ds;
        }

        public DataSet GetAllIssueData(string SiteId, string fdt, string tdt, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select mh.MINH_ID,mh.MIN_NO,mh.SiteID,mh.IssueDate,mh.IssuedByUserID,md.Prod_ID,md.Prod_Code,md.Prod_Description,md.Prod_Name,md.IssuedQty,mt.Territory,ms.[Status],mh.title from dbo.PORtMINHead as MH left outer join  dbo.mTerritory as mt on mh.SiteID=mt.ID left outer join dbo.mStatus as MS on MH.StatusID=ms.ID left outer join dbo.PORtMINDetail as MD on mh.MINH_ID=md.MINH_ID where mh.SiteID in(" + SiteId + ")  and mh.IssueDate between '" + fdt + "' and '" + tdt + "' ", conn);
            return ds;
        }

        public DataSet GetIssueDataAllIssue(string SiteId, string IssueLst, string PLst, string fdt, string tdt, string objectnm, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select mh.MINH_ID,mh.MIN_NO,mh.SiteID,mh.IssueDate,mh.IssuedByUserID,md.Prod_ID,md.Prod_Code,md.Prod_Description,md.Prod_Name,md.IssuedQty,mt.Territory,ms.[Status],mh.title from dbo.PORtMINHead as MH left outer join  dbo.mTerritory as mt on mh.SiteID=mt.ID left outer join dbo.mStatus as MS on MH.StatusID=ms.ID left outer join dbo.PORtMINDetail as MD on mh.MINH_ID=md.MINH_ID where mh.SiteID in(" + SiteId + ")  and md.Prod_ID in(" + PLst + ") and mh.IssueDate between '" + fdt + "' and '" + tdt + "' ", conn);
            return ds;
        }

        public DataSet GetIssueDataAllPrd(string SiteId, string IssueLst, string PLst, string fdt, string tdt, string objectnm, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select mh.MINH_ID,mh.MIN_NO,mh.SiteID,mh.IssueDate,mh.IssuedByUserID,md.Prod_ID,md.Prod_Code,md.Prod_Description,md.Prod_Name,md.IssuedQty,mt.Territory,ms.[Status],mh.title from dbo.PORtMINHead as MH left outer join  dbo.mTerritory as mt on mh.SiteID=mt.ID left outer join dbo.mStatus as MS on MH.StatusID=ms.ID left outer join dbo.PORtMINDetail as MD on mh.MINH_ID=md.MINH_ID where mh.SiteID in(" + SiteId + ") and mh.MINH_ID in(" + IssueLst + ")  and mh.IssueDate between '" + fdt + "' and '" + tdt + "' ", conn);
            return ds;
        }


        public DataSet GetReceiptData(string SiteId, string ReceiptLst, string PrdLst, string fdt, string tdt, string objectnm, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select gh.GRNH_ID,gh.SiteID,gh.GRN_No,gh.ReceivedByUserID,gh.GRN_Date,gd.Prod_ID,gd.ReceivedQty,gd.AcceptedQty,mt.Territory,mp.ProductCode,mp.name,ms.[status],gh.Title from dbo.PORtGRNHead as GH left outer join dbo.mTerritory as mt on gh.SiteID=mt.ID left outer join dbo.mStatus as MS on gh.StatusID=MS.ID left outer join dbo.PORtGRNDetail as GD on gh.GRNH_ID=gd.GRNH_ID left outer join dbo.mproduct as mp on gd.Prod_ID = mp.ID Where gh.SiteID in (" + SiteId + ") and gh.GRNH_ID in(" + ReceiptLst + ") and gd.Prod_ID in(" + PrdLst + ") and gh.GRN_Date between '" + fdt + "' and '" + tdt + "' ", conn);
            return ds;
        }

        public DataSet GetAllReceiptData(string SiteId, string fdt, string tdt, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select gh.GRNH_ID,gh.SiteID,gh.GRN_No,gh.ReceivedByUserID,gh.GRN_Date,gd.Prod_ID,gd.ReceivedQty,gd.AcceptedQty,mt.Territory,mp.ProductCode,mp.name,ms.[status],gh.Title from dbo.PORtGRNHead as GH left outer join dbo.mTerritory as mt on gh.SiteID=mt.ID left outer join dbo.mStatus as MS on gh.StatusID=MS.ID left outer join dbo.PORtGRNDetail as GD on gh.GRNH_ID=gd.GRNH_ID left outer join dbo.mproduct as mp on gd.Prod_ID = mp.ID Where gh.SiteID in (" + SiteId + ") and gh.GRN_Date between '" + fdt + "' and '" + tdt + "' ", conn);
            return ds;
        }

        public DataSet GetReceiptDataAllReceipt(string SiteId, string ReceiptLst, string PrdLst, string fdt, string tdt, string objectnm, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select gh.GRNH_ID,gh.SiteID,gh.GRN_No,gh.ReceivedByUserID,gh.GRN_Date,gd.Prod_ID,gd.ReceivedQty,gd.AcceptedQty,mt.Territory,mp.ProductCode,mp.name,ms.[status],gh.Title from dbo.PORtGRNHead as GH left outer join dbo.mTerritory as mt on gh.SiteID=mt.ID left outer join dbo.mStatus as MS on gh.StatusID=MS.ID left outer join dbo.PORtGRNDetail as GD on gh.GRNH_ID=gd.GRNH_ID left outer join dbo.mproduct as mp on gd.Prod_ID = mp.ID Where gh.SiteID in (" + SiteId + ") and  gd.Prod_ID in(" + PrdLst + ") and gh.GRN_Date between '" + fdt + "' and '" + tdt + "' ", conn);
            return ds;
        }

        public DataSet GetReceiptDataAllPrd(string SiteId, string ReceiptLst, string PrdLst, string fdt, string tdt, string objectnm, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select gh.GRNH_ID,gh.SiteID,gh.GRN_No,gh.ReceivedByUserID,gh.GRN_Date,gd.Prod_ID,gd.ReceivedQty,gd.AcceptedQty,mt.Territory,mp.ProductCode,mp.name,ms.[status],gh.Title from dbo.PORtGRNHead as GH left outer join dbo.mTerritory as mt on gh.SiteID=mt.ID left outer join dbo.mStatus as MS on gh.StatusID=MS.ID left outer join dbo.PORtGRNDetail as GD on gh.GRNH_ID=gd.GRNH_ID left outer join dbo.mproduct as mp on gd.Prod_ID = mp.ID Where gh.SiteID in (" + SiteId + ") and gh.GRNH_ID in(" + ReceiptLst + ")  and gh.GRN_Date between '" + fdt + "' and '" + tdt + "' ", conn);
            return ds;
        }

        public DataSet GetPenComRequestData(string SiteId, string fdt, string tdt, string[] conn)
        {
            DataSet ds = new DataSet();
            //ds = fillds("select f.FN_Date,mt.territory,*,case when por.StatusID='8' then '1' else '0' end as pending from PORtPartRequestHead por  right outer join  dbo.FT_GetInBetweenDates(' " + fdt + " ',' " + tdt + " ') f on por.RequestDate=f.FN_Date left outer join dbo.mTerritory mT on por.SiteID = mT.ID  and SiteId in(" + SiteId + ")", conn);
            ds = fillds("Select * from DBO.POR_FN_GetRequestStatusMonthWise('" + fdt + "','" + tdt + "') where TerritoryID in (" + SiteId + ")", conn);
            return ds;
        }

        public DataSet GetWeeklyConsumption(string Site, string ftd, string tdt, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from [dbo].[POR_FN_WeekWiseConsumption] ('" + ftd + "','" + tdt + "') where SiteID in (" + Site + ")", conn);
            return ds;
        }

        public DataSet GetConsumableStock(string Category, string Site, string fdt, string tdt, string[] conn)
        {
            DataSet ds = new DataSet();
            //ds = fillds("select * from [dbo].[POR_FN_GetConsumableStockMonthWise] ('" + fdt + "','" + tdt + "') where SiteID in (" + Site + ")", conn);
            ds = fillds("Select * from  [dbo].[POR_FN_GetMonthWisePrd_CategoryTotal]('" + fdt + "','" + tdt + "') where Territoryid in(" + Site + ") and Prod_CategoryID=" + Category + " ", conn);
            return ds;
        }

        public DataSet GetProductBalanceOfSite(string Site, string PrdList, string AllPrd, string excludeZero, string[] conn)
        {
            DataSet ds = new DataSet();
            if (AllPrd != "1" && excludeZero != "1")
            {
                ds = fillds("select * from V_GetProductAvailableBalance where siteid in (" + Site + ") and prodid in (" + PrdList + ")", conn);
            }
            else if (AllPrd == "1" && excludeZero != "1")
            {
                ds = fillds("select * from V_GetProductAvailableBalance where siteid in (" + Site + ") ", conn);
            }
            else if (AllPrd != "1" && excludeZero == "1")
            {
                ds = fillds("select * from V_GetProductAvailableBalance where siteid in (" + Site + ") and prodid in (" + PrdList + ") and AvailableBalance > 0", conn);
            }
            else if (AllPrd == "1" && excludeZero == "1")
            {
                ds = fillds("select * from V_GetProductAvailableBalance where siteid in (" + Site + ") and  AvailableBalance > 0", conn);
            }
            return ds;
        }

        public List<GetPrdDetail> AllProductOnSite(string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<GetPrdDetail> PrdList = new List<GetPrdDetail>();

            PrdList = (from p in db.GetPrdDetails
                       select p).ToList();
            return PrdList;
        }

        public int GetEngineCount(string refId, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));

            DataSet ds = new DataSet();
            ds = fillds("select * from mEngine where EngineSerial in (" + refId + ") ", conn);

            int EngList = Convert.ToInt16(ds.Tables[0].Rows.Count);

            return EngList;
        }

        public int GetEngineCountAll(string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));

            DataSet ds = new DataSet();
            ds = fillds("select * from mEngine", conn);
            int EngList = Convert.ToInt16(ds.Tables[0].Rows.Count);
            return EngList;
        }


        public List<mTerritory> GetSiteNameByUserID_IssueID(long IssueID, long uid, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mTerritory> SiteName = new List<mTerritory>();

            DataSet ds = new DataSet();
            ds = fillds("select SiteID from PORtMINHead where MINH_ID=" + IssueID + "", conn);

            long SiteID = Convert.ToInt64(ds.Tables[0].Rows[0]["SiteID"]);


            SiteName = (from mT in ce.mTerritories
                        join mU in ce.mUserTerritoryDetails on mT.ID equals mU.TerritoryID
                        where mU.UserID == uid && mT.ID != SiteID && mT.ID != 1
                        select mT).ToList();
            return SiteName;
        }

        public long GetSiteID(long IssueID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            DataSet ds = new DataSet();
            ds = fillds("select SiteID from PORtMINHead where MINH_ID=" + IssueID + "", conn);

            long SiteID = Convert.ToInt64(ds.Tables[0].Rows[0]["SiteID"]);

            return SiteID;
        }


        public List<mTerritory> GetSiteNameByUserID_Transfer(long uid, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mTerritory> SiteName = new List<mTerritory>();

            SiteName = (from mT in ce.mTerritories
                        join mU in ce.mUserTerritoryDetails on mT.ID equals mU.TerritoryID
                        where mU.UserID == uid && mT.ID != 1
                        select mT).ToList();
            return SiteName;
        }

        public List<mTerritory> GetToSiteName_Transfer(long FrmSiteID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mTerritory> FrmSiteName = new List<mTerritory>();

            FrmSiteName = (from mT in ce.mTerritories
                           where mT.ID != 1 && mT.ID != FrmSiteID
                           select mT).ToList();
            return FrmSiteName;
        }

        public DataSet GetTransferRptData(string toSiteID, string fromSiteID, string fdt, string tdt, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from POR_VGetTransferReport where ToSiteID=" + toSiteID + " and FromSiteID=" + fromSiteID + " and TransferDate >= '" + fdt + "'  and TransferDate <= '" + tdt + "' ", conn);
            return ds;
        }



        //protected DataSet fillds(String strquery, string[] conn)
        //{
        //    BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
        //    DataSet ds = new DataSet();

        //    System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection("Data Source=" + conn[0] + ";Initial Catalog=" + conn[1] + "; User ID=" + conn[3] + "; Password=" + conn[2] + ";");
        //    SqlDataAdapter da = new SqlDataAdapter(strquery, sqlConn);
        //    ds.Reset();
        //    da.Fill(ds);
        //    return ds;
        //}

        protected DataSet fillds(string strquery, string[] conn)
        {
            using (DataSet ds = new DataSet())
            {
                using (System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection("Data Source=" + conn[0] + ";Initial Catalog=" + conn[1] + "; User ID=" + conn[3] + "; Password=" + conn[2] + ";"))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(strquery, sqlConn))
                    {
                        try
                        {
                            ds.Reset();
                            sqlConn.Open();
                            da.Fill(ds);
                            sqlConn.Close();
                        }
                        catch
                        {
                            sqlConn.Close();
                        }
                    }
                }
                return ds;
            }
        }

        public DataSet GetAssetRptData(string toSiteID, string fromSiteID, string fdt, string tdt, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from VW_AssetTransferDetails where TransferFromSite=" + fromSiteID + " and TransferToSite=" + toSiteID + " and TransferDate >= '" + fdt + "' and TransferDate <='" + tdt + "'", conn);
            return ds;
        }

        ///Reports











        ////public DataSet GetAllUserDataSelectedRow(string CompanyID, string DeptID, string[] conn)
        ////{
        ////    DataSet ds = new DataSet();
        ////    ds = fillds("Select * from VW_GetUserInformation where CompanyID =" + CompanyID + " and StoreId = " + DeptID + " ", conn);
        ////    return ds;
        ////}



        public DataSet GetRequestDetails(string CompanyID, string DeptID, string status, string UserID, string FromDate, string ToDate, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("Select * from VW_GetSKUAndRequestDetails where CompanyID=" + CompanyID + " and DeptID = " + DeptID + " and StatusID= " + status + " and RequestBy = " + UserID + " and RequestDate >= '" + FromDate + "' and RequestDate <='" + ToDate + "'", conn);
            return ds;

        }


        public List<VW_GetSKUAndRequestDetails> AllSKUDetailsByRequest(long CompanyID, long DeptID, string status, long UserID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<VW_GetSKUAndRequestDetails> list = new List<VW_GetSKUAndRequestDetails>();
            list = (from cl in ce.VW_GetSKUAndRequestDetails
                    where cl.CompanyID == CompanyID && cl.DeptID == DeptID && cl.Status == status && cl.RequestBy == UserID
                    select cl).ToList();
            return list;
        }

        public DataSet SKUDetailsBySelectedRequestID(string selectedRequest, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("Select * from VW_GetSKUAndRequestDetails where PRH_ID in(" + selectedRequest + ")", conn);
            return ds;
        }

        public DataSet GetAllOrderData(string FromDate, string ToDate, string CompanyID, string DeptID, string Status, string User, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("Select * from VW_GetSKUAndRequestDetails where CompanyID = '" + CompanyID + "' and DeptID= '" + DeptID + "' and StatusID= " + Status + " and RequestBy = '" + User + "' and RequestDate >= '" + FromDate + "' and RequestDate <='" + ToDate + "' ", conn);
            return ds;
        }

        public DataSet GetAllOrderDataBySelectedOrder(string SelectedOrder, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("Select * from VW_GetSKUAndRequestDetails where PRH_ID = " + SelectedOrder + "", conn);
            return ds;
        }

        public DataSet GetAllOrderDataBySelectedSKU(string SelectedKU, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("Select * from VW_GetSKUAndRequestDetails where Prod_ID in (" + SelectedKU + ")", conn);
            return ds;
        }


        public DataSet GetAllOrderDataBySelectedSKUAndOrder(string SelectedProduct, string SelectedOrder, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("Select * from VW_GetSKUAndRequestDetails where Prod_ID in (" + SelectedProduct + ") and PRH_ID in (" + SelectedOrder + ") ", conn);
            return ds;
        }

        #region [GWC]all report Dropdown

        public List<mCompany> GetCompanyName(string[] conn)
        {
            using (BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                List<mCompany> companylist = new List<mCompany>();
                companylist = (from cl in ce.mCompanies
                               orderby cl.Name
                               select cl).ToList();
                return companylist;
            }
        }


        public DataSet GetCompanyNameNew(string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("Select * from mcompany order by name", conn);
            return ds;
        }


        public List<mTerritory> GetDepartmentList(int ComapnyID, string[] conn)
        {
            using (BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                List<mTerritory> DeptList = new List<mTerritory>();
                DeptList = (from dl in ce.mTerritories
                            where dl.ParentID == ComapnyID && dl.Emoney!=true
                            orderby dl.Territory
                            select dl).ToList();
                return DeptList;
            }
        }

       

        public List<mTerritory> GetAllDepartmentList(string[] conn)
        {
            using (BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                List<mTerritory> DeptList = new List<mTerritory>();
                DeptList = (from dl in ce.mTerritories
                            orderby dl.Territory
                            select dl).ToList();
                return DeptList;
            }
        }

        public List<mBOMHeader> GetGroupSet(int CompanyID, int DeptID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mBOMHeader> GroupSetList = new List<mBOMHeader>();
            GroupSetList = (from glist in ce.mBOMHeaders
                            join p in ce.mProducts on glist.Id equals p.BOMHeaderId
                            where p.CompanyID == CompanyID && p.StoreId == DeptID
                            select glist).Distinct().ToList();
            return GroupSetList;
        }

        public List<mBOMHeader> GetAllGroupsetList(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mBOMHeader> GroupSetList = new List<mBOMHeader>();
            GroupSetList = (from glist in ce.mBOMHeaders
                            select glist).Distinct().ToList();
            return GroupSetList;
        }


        public List<mBOMHeader> GetGroupSetByDept(int DeptID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mBOMHeader> GroupSetList = new List<mBOMHeader>();
            //GroupSetList = (from glist in ce.mBOMHeaders
            //                join p in ce.mProducts on glist.Id equals p.BOMHeaderId
            //                where p.StoreId == DeptID
            //                select glist).Distinct().ToList();
            return GroupSetList;
        }


        public List<mBOMHeader> GetGroupSetByCompany(int CompanyID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mBOMHeader> GroupSetList = new List<mBOMHeader>();
            //GroupSetList = (from glist in ce.mBOMHeaders
            //                join p in ce.mProducts on glist.Id equals p.BOMHeaderId
            //                where p.CompanyID == CompanyID
            //                select glist).Distinct().ToList();
            return GroupSetList;
        }

        public List<mStatu> GetStatus(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mStatu> Statuslist = new List<mStatu>();
            Statuslist = (from cl in ce.mStatus
                          where cl.ObjectName == "MaterialRequest"
                          where cl.Status != "Composing" && cl.Status != "Partial Issued" && cl.Status != "Consumed" && cl.Status != "Transfered" && cl.Status != "Approve with Revision" && cl.Status != "Pending For Approval Level 2" && cl.Status != "Pending For Approval Level 3"
                          orderby cl.Status
                          select cl).ToList();
            return Statuslist;
        }

        public List<VW_GetUserInformation> GetUser(int CompanyID, int DeptID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<VW_GetUserInformation> userlst = new List<VW_GetUserInformation>();
            userlst = (from cl in ce.VW_GetUserInformation
                       where cl.CompanyID == CompanyID && cl.DepartmentID == DeptID
                       select cl).ToList();
            return userlst;
        }

        public DataSet GetUsrLst(string CompanyID, string DeptID, string[] conn)
        {
            if (CompanyID == "0") CompanyID = "";
            if (DeptID == "0") DeptID = "";
            DataSet ds = new DataSet();
            ds = fillds("Select * from VW_GetUserInformation WHERE CompanyID like '%" + CompanyID + "%' and DepartmentID like '%" + DeptID + "%' ", conn);
            return ds;
        }

        public List<SP_GWC_GetUserInfo_Result> GetUsrLst1(string CompanyID, string DeptID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            if (CompanyID == "0") CompanyID = "";
            if (DeptID == "0") DeptID = "";
            List<SP_GWC_GetUserInfo_Result> usrlst = new List<SP_GWC_GetUserInfo_Result>();
            usrlst = (from cl in ce.SP_GWC_GetUserInfo(CompanyID, DeptID)
                      orderby cl.Name
                      select cl).ToList();
            return usrlst;
        }



        #endregion

        #region PartRequest_DropDown
        public List<tContactPersonDetail> GetContactPersonListDeptWise(long Dept, string[] conn)
        {
            using (BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                List<tContactPersonDetail> ConPerLst = new List<tContactPersonDetail>();
                ConPerLst = (from dpt in ce.tContactPersonDetails
                             where dpt.Department == Dept
                             select dpt).ToList();
                return ConPerLst;
            }
        }

        public List<tContactPersonDetail> GetContactPersonList(long Dept, string[] conn)
        {
            using (BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                List<tContactPersonDetail> ConPerLst = new List<tContactPersonDetail>();
                ConPerLst = (from dpt in ce.tContactPersonDetails
                             where dpt.CompanyID == Dept
                             select dpt).ToList();
                return ConPerLst;
            }
        }

        public List<tContactPersonDetail> GetContactPerson2List(long Dept, long Cont1, string[] conn)
        {
            using (BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                List<tContactPersonDetail> ConPerLst = new List<tContactPersonDetail>();
                ConPerLst = (from dpt in ce.tContactPersonDetails
                             where dpt.CompanyID == Dept
                             && dpt.ID != Cont1
                             select dpt).ToList();
                return ConPerLst;
            }
        }


        public List<tAddress> GetDeptAddressList(long Dept, string[] conn)
        {
            using (BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                List<tAddress> AdrsLst = new List<tAddress>();
                AdrsLst = (from cl in ce.tAddresses
                           where cl.CompanyID == Dept && cl.ObjectName == "Account"
                           select cl).ToList();
                return AdrsLst;
            }
        }

        public List<tAddress> GetDeptAddressListAdrsType(long CompanyID, long DeptID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<tAddress> AdrsLst = new List<tAddress>();
            DataSet ds = new DataSet();
            ds = fillds("SELECT T.ID,T.Territory, D.Value,T.AddressType,D.StatusID FROM mTerritory T left outer join mDropdownValues D on T.AddressType=D.Id where T.ID=" + DeptID + "", conn);
            int AdrsType = int.Parse(ds.Tables[0].Rows[0]["AddressType"].ToString());
            if (AdrsType == 63)
            {
                AdrsLst = (from cl in ce.tAddresses
                           where cl.CompanyID == CompanyID && cl.ObjectName == "Account" && cl.AddressType == "Location" && cl.ReferenceID == DeptID && cl.Active == "Y"
                           select cl).ToList();
            }
            else if (AdrsType == 64)
            {
                AdrsLst = (from cl in ce.tAddresses
                           where cl.CompanyID == CompanyID && cl.ObjectName == "Account" && cl.AddressType == "none" && cl.ReferenceID == DeptID && cl.Active == "Y"
                           select cl).ToList();
            }
            else if (AdrsType == 65)
            {
                AdrsLst = (from cl in ce.tAddresses
                           where cl.CompanyID == CompanyID && cl.ObjectName == "Account" && cl.Active == "Y"
                           select cl).ToList();
            }
            return AdrsLst;
        }

        public List<tAddress> GetUserLocation(long UserID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<tAddress> AdrsLst = new List<tAddress>();
            //AdrsLst = (from a in ce.tAddresses
            //           join m in ce.mUserLocations
            //               on a.ID equals m.LocationID
            //           where m.UserID == UserID && a.Active=="Y"
            //           select a).ToList();
            AdrsLst = (from a in ce.tAddresses
                       join m in ce.mUserLocations
                       on a.ID equals m.LocationID
                       join c in ce.tContactPersonDetails
                       on a.ShippingID equals c.ID
                       where m.UserID == UserID && a.Active == "Y"
                       select a).ToList();
            return AdrsLst;
        }

        public List<VW_GetUserLocation> GetLocationOfUser(long UserID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<VW_GetUserLocation> LocList = new List<VW_GetUserLocation>();
            LocList = (from l in ce.VW_GetUserLocation
                       where l.UserID == UserID && l.Active == "Y"
                       select l).ToList();
            return LocList;
        }

        public string GetAdrsType(long DeptID, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("SELECT T.ID,T.Territory, D.Value,T.AddressType,D.StatusID FROM mTerritory T left outer join mDropdownValues D on T.AddressType=D.Id where T.ID=" + DeptID + "", conn);
            string AdrsType = ds.Tables[0].Rows[0]["Value"].ToString();
            return AdrsType;
        }

        public List<mCompany> GetUserCompanyName(long UID, string[] conn)
        {
            using (BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                List<mCompany> companylist = new List<mCompany>();
                long CmpID = (from cm in ce.mUserProfileHeads
                              where cm.ID == UID
                              select cm.CompanyID).FirstOrDefault();

                companylist = (from cl in ce.mCompanies
                               where cl.ID == CmpID
                               select cl).ToList();
                return companylist;
            }
        }

        public int GetUserCompanyID(long UID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mCompany> companylist = new List<mCompany>();
            long CmpID = (from cm in ce.mUserProfileHeads
                          where cm.ID == UID
                          select cm.CompanyID).FirstOrDefault();
            int companyid = Convert.ToInt16(CmpID);

            return companyid;
        }

        public List<mTerritory> GetDepartmentListUserWise(int UID, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mTerritory> DeptList = new List<mTerritory>();

            long DeptID = (from d in ce.mUserProfileHeads
                           where d.ID == UID
                           select d.DepartmentID).FirstOrDefault();

            DeptList = (from dl in ce.mTerritories
                        where dl.ID == DeptID
                        select dl).ToList();
            return DeptList;
        }

        public long GetSiteIdOfUser(long UID, string[] conn)
        {
            DataSet ds = new DataSet();
            long DeptID = 0;
            //ds = fillds("select TerritoryID from mUserTerritoryDetail where userid=" + UID + "", conn);
            ds = fillds("select UTD.TerritoryID from mUserTerritoryDetail UTD left outer join mterritory MT on UTD.TerritoryID=MT.ID where UTD.userid=" + UID + " order by MT.Territory", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                DeptID = long.Parse(ds.Tables[0].Rows[0]["TerritoryID"].ToString());
            }
            return DeptID;
        }

        public long GetCompanyIDFromSiteID(long SiteID, string[] conn)
        {
            DataSet ds = new DataSet();
            long companyId = 0;
            ds = fillds("select ID from mcompany  where ID=(select ParentID from mTerritory where ID=" + SiteID + ")", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                companyId = long.Parse(ds.Tables[0].Rows[0]["ID"].ToString());
            }
            return companyId;
        }
        #endregion

        public DataSet GetAllDrivers(string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select CONCAT(FirstName,' ',LastName) as DName from mUserProfileHead where UserType='Driver'", conn);
            return ds;
        }


        public DataSet GetSku(string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("Select * from VW_GetSKUDetails", conn);
            return ds;
        }

        public DataSet GetAllOrder(string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("Select * from VW_GetAllOrder where Status = 'Approved'", conn);
            return ds;
        }



        public List<VW_GWC_SkuFilter> SkuLstRpt(string SelectedCompany, string SelectedDepartment, string SelectedGroupSet, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<VW_GWC_SkuFilter> SkuLst = new List<VW_GWC_SkuFilter>();
            if (SelectedCompany == null || SelectedCompany == "0") SelectedCompany = "";
            if (SelectedDepartment == null || SelectedDepartment == "0") SelectedDepartment = "";
            if (SelectedGroupSet == null || SelectedGroupSet == "0") SelectedGroupSet = "";

            SkuLst = (from l in db.VW_GWC_SkuFilter
                      where SqlMethods.Like(l.CompanyID.ToString(), "%" + SelectedCompany + "%") && SqlMethods.Like(l.StoreId.ToString(), "%" + SelectedDepartment + "%") && SqlMethods.Like(l.GroupSet.ToString(), "%" + SelectedGroupSet + "%")
                      select l).ToList();
            return SkuLst;
        }

        public DataSet SkulistReport(string SelectedCompany, string SelectedDepartment, string SelectedGroupSet, string SelectedImage, string[] conn)
        {
            DataSet ds = new DataSet();
            if (SelectedCompany == null || SelectedCompany == "0") SelectedCompany = "";
            if (SelectedDepartment == null || SelectedDepartment == "0") SelectedDepartment = "";
            if (SelectedGroupSet == null || SelectedGroupSet == "0") SelectedGroupSet = "";
            if (SelectedImage == null || SelectedImage == "0" || SelectedImage == "1") SelectedImage = "";
            ds = fillds("select * from VW_GWC_SkuFilter where Emoney=0 and CompanyID like '%" + SelectedCompany + "%' and StoreId like '%" + SelectedDepartment + "%' and GroupSet like '%" + SelectedGroupSet + "%' and Image like '%" + SelectedImage + "%'", conn);
            return ds;
        }

        public DataSet SkulistReportSearch(string SelectedCompany, string SelectedDepartment, string SelectedGroupSet, string SelectedImage, string SearchedValue, string[] conn)
        {
            DataSet ds = new DataSet();
            if (SelectedCompany == null || SelectedCompany == "0") SelectedCompany = "";
            if (SelectedDepartment == null || SelectedDepartment == "0") SelectedDepartment = "";
            if (SelectedGroupSet == null || SelectedGroupSet == "0") SelectedGroupSet = "";
            if (SelectedImage == null || SelectedImage == "0" || SelectedImage == "1") SelectedImage = "";
            ds = fillds("select * from VW_GWC_SkuFilter Emoney=0 and where CompanyID like '%" + SelectedCompany + "%' and StoreId like '%" + SelectedDepartment + "%' and GroupSet like '%" + SelectedGroupSet + "%' and Image like '%" + SelectedImage + "%' and (Productcode like '%" + SearchedValue + "%' OR Name like '%" + SearchedValue + "%' OR Description like '%" + SearchedValue + "%')", conn);
            return ds;
        }

        public DataSet GetAllSKUData(string CompanyID, string DeptID, string GroupSetID, string Image, string WithZero, string[] conn)
        {
            DataSet ds = new DataSet();
            if (CompanyID == null || CompanyID == "0") CompanyID = "";
            if (DeptID == null || DeptID == "0") DeptID = "";
            if (GroupSetID == null || GroupSetID == "0") GroupSetID = "";
            if (Image == "1") Image = "";
            ds = fillds("Select * from VW_GWC_SkuFilter Emoney=0 and where CompanyID like '%" + CompanyID + "%' and StoreId like '%" + DeptID + "%' and GroupSet like '%" + GroupSetID + "%' and Image like '%" + Image + "%'  ", conn);//and Availablebalance>=" + WithZero + "
            return ds;
        }

        public DataSet GetAllSKUDataSelectedRow(string SelectedProducts, string Image, string WithZero, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("Select * from VW_GWC_SkuFilter where Emoney=0 and ID in(" + SelectedProducts + ") and Image like '%" + Image + "%' and Availablebalance>=" + WithZero + "", conn);
            return ds;
        }

        public DataSet GetSKUDetailsReprtData(string CompanyID, string DeptID, string GroupSetID, string[] conn)
        {
            DataSet ds = new DataSet();
            if (CompanyID == null || CompanyID == "0") CompanyID = "";
            if (DeptID == null || DeptID == "0") DeptID = "";
            if (GroupSetID == null || GroupSetID == "0") GroupSetID = "";
            ds = fillds("Select * from VW_SKUDetails where Emoney=0 and CompanyID like '%" + CompanyID + "%' and StoreId like '%" + DeptID + "%' and GroupSet like '%" + GroupSetID + "%' ", conn);
            return ds;
        }

        public DataSet GetSKUDetailsSelectedRow(string SelectedProducts, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("Select * from VW_SKUDetails where Emoney=0 and ID in(" + SelectedProducts + ")", conn);
            return ds;
        }

        public DataSet GetBOMDetailsReprtData(string CompanyID, string DeptID, string GroupSetID, string[] conn)
        {
            DataSet ds = new DataSet();
            if (CompanyID == null || CompanyID == "0") CompanyID = "";
            if (DeptID == null || DeptID == "0") DeptID = "";
            if (GroupSetID == null || GroupSetID == "0") GroupSetID = "";
            ds = fillds("Select * from VW_GWC_BOMDetails where Emoney=0 and  CompanyID like '%" + CompanyID + "%' and StoreId like '%" + DeptID + "%'", conn);
            return ds;
        }

        public DataSet GetBOMDetailsSelectedRow(string SelectedProducts, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("Select * from VW_GWC_BOMDetails where Emoney=0 and BOMHeaderId in(" + SelectedProducts + ")", conn);
            return ds;
        }

        public DataSet GetAllOrderDetails(string FrmDt, string Todt, string SelectedCompany, string SelectedDepartment, string SelectedUser, string SelectedStatus, string[] conn)
        {
            DataSet ds = new DataSet();
            if (SelectedCompany == null || SelectedCompany == "0") SelectedCompany = "";
            if (SelectedDepartment == null || SelectedDepartment == "0") SelectedDepartment = "";
            if (SelectedUser == null || SelectedUser == "0") SelectedUser = "";
            if (SelectedStatus == null || SelectedStatus == "0") SelectedStatus = "";
            // ds = fillds("Select * from VW_GetAllOrder   where ParentID like '%" + SelectedCompany + "%' and StoreId like '%" + SelectedDepartment + "%' and  RequestBy Like '%" + SelectedUser + "%' and status like '%" + SelectedStatus + "%'", conn);
            ds = fillds("DECLARE @d1 DATETIME DECLARE @d2 DATETIME  SET @d1= '" + FrmDt + "' SET @d2= '" + Todt + "' Select Id,case when Ordernumber=''  then 'NA' else Ordernumber end Ordernumber,Title,Orderdate,DeliveryDate,ContactID1,GrandTotal,OrderNo,case when CName1 IS NULL then 'NA' else CName1 end CName1,case when Cname2 IS NULL then 'NA' else Cname2 end Cname2,ContactId2,case when Remark='' then 'NA' else Remark end Remark,Status,StatusName,AddressId,case when AddressLine1 IS NULL then 'NA' else AddressLine1 end AddressLine1,RequestBy,UserName,StoreId,Territory,ParentID,CompanyName,StoreCode,PaymentMethod  from VW_GetAllOrder where Emoney=0 and ParentID like '%" + SelectedCompany + "%' and StoreId like '%" + SelectedDepartment + "%' and  RequestBy Like '%" + SelectedUser + "%' and status like '%" + SelectedStatus + "%' and Orderdate >= CONVERT(datetime ,@d1 , 103) AND Orderdate <=CONVERT(datetime ,@d2 , 103)", conn);

            return ds;
        }

        //New Added For Report Payment Detail
        public List<SP_AllOrderLocationPaymentMode_Result> GetAllOrderLocationPaymentModeNew(string FrmDt, string Todt, string SelectedCompany, string SelectedDepartment, string SelectedLocation, string SelectedPaymentMode, string IncludeECommerce, string[] conn)
        {
            List<SP_AllOrderLocationPaymentMode_Result> reqLst = new List<SP_AllOrderLocationPaymentMode_Result>();
            try
            {
                BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
                reqLst = db.SP_AllOrderLocationPaymentMode(FrmDt, Todt, SelectedCompany, SelectedDepartment, SelectedLocation, SelectedPaymentMode, IncludeECommerce).ToList();
            }
            catch { }
            finally { }
            return reqLst;
        }
        public DataSet GetAllOrderLocationPaymentMode(string FrmDt, string Todt, string SelectedCompany, string SelectedDepartment, string SelectedLocation, string SelectedPaymentMode, string IncludeECommerce, string[] conn)
        {
            DataSet ds = new DataSet();
            if (SelectedCompany == null || SelectedCompany == "0") SelectedCompany = "";
            if (SelectedDepartment == null || SelectedDepartment == "0") SelectedDepartment = "";
            if (SelectedLocation == null || SelectedLocation == "0") SelectedLocation = "";
            if (SelectedPaymentMode == null || SelectedPaymentMode == "0") SelectedPaymentMode = "";
            if (IncludeECommerce == "0")
            {
                ds = fillds("DECLARE @d1 DATETIME DECLARE @d2 DATETIME  SET @d1= '" + FrmDt + "' SET @d2= '" + Todt + "' Select Id,case when Ordernumber=''  then 'NA' else Ordernumber end Ordernumber,Title,Orderdate,DeliveryDate,ContactID1,GrandTotal,OrderNo,case when CName1 IS NULL then 'NA' else CName1 end CName1,case when Cname2 IS NULL then 'NA' else Cname2 end Cname2,ContactId2,case when Remark='' then 'NA' else Remark end Remark,Status,StatusName,AddressId,case when AddressLine1 IS NULL then 'NA' else AddressLine1 end AddressLine1,RequestBy,UserName,StoreId,Territory,ParentID,CompanyName,StoreCode,PaymentMethod,PaymentID,LocationID,CustomerName,StatutoryValue,orderType  from VW_GetAllOrder where ParentID like '%" + SelectedCompany + "%' and StoreId like '%" + SelectedDepartment + "%' and  PaymentID Like '%" + SelectedPaymentMode + "%' and LocationID like '%" + SelectedLocation + "%' and Orderdate >= CONVERT(datetime ,@d1 , 103) AND Orderdate <=CONVERT(datetime ,@d2 , 103)", conn);
            }
            else
            {
                ds = fillds("DECLARE @d1 DATETIME DECLARE @d2 DATETIME  SET @d1= '" + FrmDt + "' SET @d2= '" + Todt + "' Select Id,case when Ordernumber=''  then 'NA' else Ordernumber end Ordernumber,Title,Orderdate,DeliveryDate,ContactID1,GrandTotal,OrderNo,case when CName1 IS NULL then 'NA' else CName1 end CName1,case when Cname2 IS NULL then 'NA' else Cname2 end Cname2,ContactId2,case when Remark='' then 'NA' else Remark end Remark,Status,StatusName,AddressId,case when AddressLine1 IS NULL then 'NA' else AddressLine1 end AddressLine1,RequestBy,UserName,StoreId,Territory,ParentID,CompanyName,StoreCode,PaymentMethod,PaymentID,LocationID,CustomerName,StatutoryValue,orderType  from VW_GetAllOrder where  orderType !='ECommerce' and  ParentID like '%" + SelectedCompany + "%' and StoreId like '%" + SelectedDepartment + "%' and  PaymentID Like '%" + SelectedPaymentMode + "%' and LocationID like '%" + SelectedLocation + "%' and Orderdate >= CONVERT(datetime ,@d1 , 103) AND Orderdate <=CONVERT(datetime ,@d2 , 103)", conn);
            }
            return ds;
        }

        public DataSet GetAllOrderLocationPaymentModeRpt(string FrmDt, string Todt, string SelectedCompany, string SelectedDepartment, string SelectedLocation, string SelectedPaymentMode, string IncludeEcommerce, string[] conn)
        {
            DataSet ds = new DataSet();
            if (SelectedCompany == null || SelectedCompany == "0") SelectedCompany = "";
            if (SelectedDepartment == null || SelectedDepartment == "0") SelectedDepartment = "";
            if (SelectedLocation == null || SelectedLocation == "0") SelectedLocation = "";
            if (SelectedPaymentMode == null || SelectedPaymentMode == "0") SelectedPaymentMode = "";
            if (IncludeEcommerce == "1")
            {
                ds = fillds("DECLARE @d1 DATETIME DECLARE @d2 DATETIME  SET @d1= '" + FrmDt + "' SET @d2= '" + Todt + "' Select Id,case when Ordernumber=''  then 'NA' else Ordernumber end Ordernumber,Title,Orderdate,DeliveryDate,ContactID1,GrandTotal,OrderNo,case when CName1 IS NULL then 'NA' else CName1 end CName1,case when Cname2 IS NULL then 'NA' else Cname2 end Cname2,ContactId2,case when Remark='' then 'NA' else Remark end Remark,Status,StatusName,AddressId,case when AddressLine1 IS NULL then 'NA' else AddressLine1 end AddressLine1,RequestBy,UserName,StoreId,Territory,ParentID,CompanyName,StoreCode,PaymentMethod,PaymentID,LocationID,LocationCode,MobileNo,CustomerName,case when PaymentID=2 then StatutoryValue else '-' end StatutoryValue,orderType, ReciptNo from VW_GetAllOrder  where ParentID like '%" + SelectedCompany + "%' and StoreId like '%" + SelectedDepartment + "%' and  PaymentID Like '%" + SelectedPaymentMode + "%' and LocationID like '%" + SelectedLocation + "%'  and Orderdate >= CONVERT(datetime ,@d1 , 103) AND Orderdate <=CONVERT(datetime ,@d2 , 103)", conn);
            }
            else
            {
                ds = fillds("DECLARE @d1 DATETIME DECLARE @d2 DATETIME  SET @d1= '" + FrmDt + "' SET @d2= '" + Todt + "' Select Id,case when Ordernumber=''  then 'NA' else Ordernumber end Ordernumber,Title,Orderdate,DeliveryDate,ContactID1,GrandTotal,OrderNo,case when CName1 IS NULL then 'NA' else CName1 end CName1,case when Cname2 IS NULL then 'NA' else Cname2 end Cname2,ContactId2,case when Remark='' then 'NA' else Remark end Remark,Status,StatusName,AddressId,case when AddressLine1 IS NULL then 'NA' else AddressLine1 end AddressLine1,RequestBy,UserName,StoreId,Territory,ParentID,CompanyName,StoreCode,PaymentMethod,PaymentID,LocationID,LocationCode,MobileNo,CustomerName,case when PaymentID=2 then StatutoryValue else '-' end StatutoryValue,orderType,ReciptNo from VW_GetAllOrder  where orderType !='ECommerce' and  ParentID like '%" + SelectedCompany + "%' and StoreId like '%" + SelectedDepartment + "%' and  PaymentID Like '%" + SelectedPaymentMode + "%' and LocationID like '%" + SelectedLocation + "%'  and Orderdate >= CONVERT(datetime ,@d1 , 103) AND Orderdate <=CONVERT(datetime ,@d2 , 103)", conn);
            }
            return ds;
        }

        public DataSet GetAllOrderLocationPMode(string SelectedOrder, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("Select Id,case when Ordernumber=''  then 'NA' else Ordernumber end Ordernumber,Title,Orderdate,DeliveryDate,ContactID1,GrandTotal,OrderNo,case when CName1 IS NULL then 'NA' else CName1 end CName1,case when Cname2 IS NULL then 'NA' else Cname2 end Cname2,ContactId2,case when Remark='' then 'NA' else Remark end Remark,Status,StatusName,AddressId,case when AddressLine1 IS NULL then 'NA' else AddressLine1 end AddressLine1,RequestBy,UserName,StoreId,Territory,ParentID,CompanyName,StoreCode,PaymentMethod,PaymentID,LocationID,LocationCode,MobileNo,ReciptNo from VW_GetAllOrder  where Id in(" + SelectedOrder + ")", conn);
            return ds;
        }
        //New Added For Report Payment Detail

        public DataSet GetAllOrderDataSelectedRow(string SelectedOrder, string[] conn)
        {
            DataSet ds = new DataSet();
            //ds = fillds("Select Id,case when Ordernumber=''  then 'NA' else Ordernumber end Ordernumber,Title,Orderdate,DeliveryDate,ContactID1,GrandTotal,OrderNo,case when CName1 IS NULL then 'NA' else CName1 end CName1,case when Cname2 IS NULL then 'NA' else Cname2 end Cname2,ContactId2,case when Remark='' then 'NA' else Remark end Remark,Status,StatusName,AddressId,case when AddressLine1 IS NULL then 'NA' else AddressLine1 end AddressLine1,RequestBy,UserName,StoreId,Territory,ParentID,CompanyName,StoreCode,PaymentMethod from VW_GetAllOrder   where  Id in(" + SelectedOrder + ")", conn);
            ds = fillds("Select Id,case when Ordernumber=''  then 'NA' else Ordernumber end Ordernumber,Title,Orderdate,DeliveryDate,ContactID1,GrandTotal,OrderNo,case when CName1 IS NULL then 'NA' else CName1 end CName1,case when Cname2 IS NULL then 'NA' else Cname2 end Cname2,ContactId2,case when Remark='' then 'NA' else Remark end Remark,Status,StatusName,AddressId,case when AddressLine1 IS NULL then 'NA' else AddressLine1 end AddressLine1,RequestBy,UserName,StoreId,Territory,ParentID,CompanyName,StoreCode,PaymentMethod,Prod_Code,Prod_Name,OrderQty,total, newprice, Prod_Description,PrincipalPrice, ReciptNo from VW_GetAllOrderReport  where  Emoney=0 and Id in(" + SelectedOrder + ")", conn);
            return ds;
        }

        public DataSet AllOrderReports(string FrmDt, string Todt, string SelectedCompany, string SelectedDepartment, string SelectedUser, string SelectedStatus, string[] conn)
        {
            DataSet ds = new DataSet();
            if (SelectedCompany == null || SelectedCompany == "0") SelectedCompany = "";
            if (SelectedDepartment == null || SelectedDepartment == "0") SelectedDepartment = "";
            if (SelectedUser == null || SelectedUser == "0") SelectedUser = "";
            if (SelectedStatus == null || SelectedStatus == "0") SelectedStatus = "";
            // ds = fillds("Select * from VW_GetAllOrder   where ParentID like '%" + SelectedCompany + "%' and StoreId like '%" + SelectedDepartment + "%' and  RequestBy Like '%" + SelectedUser + "%' and status like '%" + SelectedStatus + "%'", conn);
            ds = fillds("DECLARE @d1 DATETIME DECLARE @d2 DATETIME  SET @d1= '" + FrmDt + "' SET @d2= '" + Todt + "' Select Id,case when Ordernumber=''  then 'NA' else Ordernumber end Ordernumber,Title,Orderdate,DeliveryDate,ContactID1,GrandTotal,OrderNo,case when CName1 IS NULL then 'NA' else CName1 end CName1,case when Cname2 IS NULL then 'NA' else Cname2 end Cname2,ContactId2,case when Remark='' then 'NA' else Remark end Remark,Status,StatusName,AddressId,case when AddressLine1 IS NULL then 'NA' else AddressLine1 end AddressLine1,RequestBy,UserName,StoreId,Territory,ParentID,CompanyName,StoreCode,PaymentMethod,Prod_Code,Prod_Name,OrderQty,total, newprice, Prod_Description,PrincipalPrice, ReciptNo  from VW_GetAllOrderReport where Emoney=0 and ParentID like '%" + SelectedCompany + "%' and StoreId like '%" + SelectedDepartment + "%' and  RequestBy Like '%" + SelectedUser + "%' and status like '%" + SelectedStatus + "%' and Orderdate >= CONVERT(datetime ,@d1 , 103) AND Orderdate <=CONVERT(datetime ,@d2 , 103)", conn);
            return ds;
        }

        public DataSet GetOrderDetailsReprtData(string SelectedCompany, string SelectedDepartment, string SelectedUser, string SelectedStatus, string[] conn)
        {
            DataSet ds = new DataSet();
            if (SelectedCompany == null || SelectedCompany == "0") SelectedCompany = "";
            if (SelectedDepartment == null || SelectedDepartment == "0") SelectedDepartment = "";
            if (SelectedUser == null || SelectedUser == "0") SelectedUser = "";
            if (SelectedStatus == null || SelectedStatus == "0") SelectedStatus = "";
            ds = fillds("Select * from VW_GetOrderDetails where Emoney=0 and ParentID like '%" + SelectedCompany + "%' and StoreId like '%" + SelectedDepartment + "%' and  RequestBy Like '%" + SelectedUser + "%' and statusID like '%" + SelectedStatus + "%' ", conn);
            return ds;
        }

        public DataSet GetAllOrderDetailsDataSelectedRow(string SelectedOrder, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("Select * from VW_GetOrderDetails   where Emoney=0 and  OrderHeadId in(" + SelectedOrder + ")", conn);
            return ds;
        }

        //public List<VW_GetUserInformation> GetUserInformation(long CompanyID, long DeptID, long RoleID, string Active, string[] conn)
        //{
        //    BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
        //    List<VW_GetUserInformation> UserList = new List<VW_GetUserInformation>();
        //    UserList = (from p in db.VW_GetUserInformation
        //                where p.CompanyID == CompanyID && p.DepartmentID == DeptID && p.RoleID == RoleID && p.Active == Active
        //                select p).ToList();
        //    return UserList;
        //}

        public List<SP_GWC_GetUserInfoRoleWise_Result> GetUserInformation(string CompanyID, string DeptID, string RoleID, string Active, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            if (CompanyID == "0") CompanyID = "";
            if (DeptID == "0") DeptID = "";
            if (RoleID == "0") RoleID = "";
            List<SP_GWC_GetUserInfoRoleWise_Result> UserList = new List<SP_GWC_GetUserInfoRoleWise_Result>();
            UserList = (from u in db.SP_GWC_GetUserInfoRoleWise(CompanyID, DeptID, RoleID, Active)
                        select u).ToList();
            return UserList;
        }

        public List<SP_GWC_GetUserInfoRoleWise_Result> GetUserInformationSearched(string CompanyID, string DeptID, string RoleID, string Active, string Searchedvalue, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            if (CompanyID == "0") CompanyID = "";
            if (DeptID == "0") DeptID = "";
            if (RoleID == "0") RoleID = "";
            List<SP_GWC_GetUserInfoRoleWise_Result> UserList = new List<SP_GWC_GetUserInfoRoleWise_Result>();
            UserList = (from u in db.SP_GWC_GetUserInfoRoleWise(CompanyID, DeptID, RoleID, Active)
                        select u).ToList();
            return UserList;
        }

        public DataSet GetAllUserData(string CompanyID, string DeptID, string Role, string Active, string[] conn)
        {
            DataSet ds = new DataSet();
            if (CompanyID == null || CompanyID == "0") CompanyID = "";
            if (DeptID == null || DeptID == "0") DeptID = "";
            if (Role == null || Role == "0") Role = "";
            ds = fillds("Select * from VW_GetUserInformation where CompanyID like '%" + CompanyID + "%' and DepartmentID like '%" + DeptID + "%' and RoleID like  '%" + Role + "%' and Active = '" + Active + "' ", conn);
            return ds;
        }

        public DataSet GetAllUserDataSelectedRow(string SelectedUser, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("Select * from VW_GetUserInformation where ID in(" + SelectedUser + ")", conn);
            return ds;
        }

        public DataSet GetAllOrderLeadReprtData(string FrmDt, string Todt, string SelectedCompany, string SelectedDepartment, string SelectedStatus, string SelectedUser, string[] conn)
        {
            DataSet ds = new DataSet();
            if (SelectedCompany == null || SelectedCompany == "0") SelectedCompany = "";
            if (SelectedDepartment == null || SelectedDepartment == "0") SelectedDepartment = "";
            if (SelectedStatus == null || SelectedStatus == "0") SelectedStatus = "";
            if (SelectedUser == null || SelectedUser == "0") SelectedUser = "";
            // ds = fillds("select * from VW_GWC_LeadTime where Status like '%" + SelectedStatus + "%' and ParentId like '%" + SelectedCompany + "%' and StoreId like '%" + SelectedDepartment + "%' and RequestBy like '%" + SelectedUser + "%'", conn);
            ds = fillds("DECLARE @d1 DATETIME DECLARE @d2 DATETIME SET @d1= '" + FrmDt + "' SET @d2= '" + Todt + "' select * from VW_OrderLeadTimeReport where Emoney=0 and Status like '%" + SelectedStatus + "%' and ParentId like '%" + SelectedCompany + "%' and StoreId like '%" + SelectedDepartment + "%' and RequestBy like '%" + SelectedUser + "%'  and  Orderdate >=  CONVERT(datetime ,@d1 , 103) AND Convert(varchar(10), Orderdate,103) <=CONVERT(varchar(10) ,@d2 , 103)", conn);
            return ds;
        }

        public DataSet GetAllOrderSelectedOrderRpt(string SelectedOrder, string[] conn)
        {
            DataSet ds = new DataSet();
            //ds = fillds("Select * from VW_GWC_LeadTime where Id in(" + SelectedOrder + ")", conn);
            ds = fillds("Select * from VW_OrderLeadTimeReport where Emoney=0 and Id in(" + SelectedOrder + ")", conn);
            return ds;
        }

        public DataSet GetImageAuditAllPrd(string FrmDt, string Todt, string SelectedCompany, string SelectedDepartment, string SelectedUser, string[] conn)
        {
            DataSet ds = new DataSet();
            if (SelectedCompany == null || SelectedCompany == "0") SelectedCompany = "";
            if (SelectedDepartment == null || SelectedDepartment == "0") SelectedDepartment = "";
            if (SelectedUser == null || SelectedUser == "0") SelectedUser = "";
            ds = fillds("DECLARE @d1 DATETIME DECLARE @d2 DATETIME SET @d1= '" + FrmDt + "' SET @d2= '" + Todt + "'  select * from VW_GWC_ImageAudit where Emoney=0 and CompanyID like '%" + SelectedCompany + "%' and StoreID like '%" + SelectedDepartment + "%' and CreatedBy like '%" + SelectedUser + "%' and  creationdate >=  CONVERT(datetime ,@d1 , 103) AND Convert(varchar(10), creationdate,103) <=CONVERT(varchar(10) ,@d2 , 103) ", conn);
            return ds;
        }

        public DataSet GetImageAuditAllPrdSearched(string FrmDt, string Todt, string SelectedCompany, string SelectedDepartment, string SelectedUser, string SearchedValue, string[] conn)
        {
            DataSet ds = new DataSet();
            if (SelectedCompany == null || SelectedCompany == "0") SelectedCompany = "";
            if (SelectedDepartment == null || SelectedDepartment == "0") SelectedDepartment = "";
            if (SelectedUser == null || SelectedUser == "0") SelectedUser = "";
            ds = fillds("DECLARE @d1 DATETIME DECLARE @d2 DATETIME SET @d1= '" + FrmDt + "' SET @d2= '" + Todt + "'  select * from VW_GWC_ImageAudit where Emoney=0 and CompanyID like '%" + SelectedCompany + "%' and StoreID like '%" + SelectedDepartment + "%' and CreatedBy like '%" + SelectedUser + "%' and  creationdate >=  CONVERT(datetime ,@d1 , 103) AND Convert(varchar(10), creationdate,103) <=CONVERT(varchar(10) ,@d2 , 103) and (Productcode like '%" + SearchedValue + "%' OR Name like '%" + SearchedValue + "%' OR Description like '%" + SearchedValue + "%')", conn);
            return ds;
        }

        public DataSet GetImageAuditFailPrdLst(string FrmDt, string Todt, string SelectedCompany, string SelectedDepartment, string SelectedUser, string[] conn)
        {
            DataSet ds = new DataSet();
            if (SelectedCompany == null || SelectedCompany == "0") SelectedCompany = "";
            if (SelectedDepartment == null || SelectedDepartment == "0") SelectedDepartment = "";
            if (SelectedUser == null || SelectedUser == "0") SelectedUser = "";
            ds = fillds("DECLARE @d1 DATETIME DECLARE @d2 DATETIME SET @d1= '" + FrmDt + "' SET @d2= '" + Todt + "'  select ID, ImageName ProductCode,CreatedBy,UserName,CreationDate,Reason Description,CompanyId,CompanyName,DeptID,Territory,OMSSkuCode Name,SkuImage from VW_GWC_ImageAuditFail where Emoney=0 and CompanyID like '%" + SelectedCompany + "%' and DeptID like '%" + SelectedDepartment + "%' and CreatedBy like '%" + SelectedUser + "%' and Convert(varchar(10), creationdate,103) >=  CONVERT(varchar(10) ,@d1 , 103) AND Convert(varchar(10), creationdate,103) <=CONVERT(varchar(10) ,@d2 , 103) ", conn);
            return ds;
        }

        public DataSet GetImageAuditFailPrdLstSearched(string FrmDt, string Todt, string SelectedCompany, string SelectedDepartment, string SelectedUser, string SearchedValue, string[] conn)
        {
            DataSet ds = new DataSet();
            if (SelectedCompany == null || SelectedCompany == "0") SelectedCompany = "";
            if (SelectedDepartment == null || SelectedDepartment == "0") SelectedDepartment = "";
            if (SelectedUser == null || SelectedUser == "0") SelectedUser = "";
            ds = fillds("DECLARE @d1 DATETIME DECLARE @d2 DATETIME SET @d1= '" + FrmDt + "' SET @d2= '" + Todt + "'  select ID, ImageName ProductCode,CreatedBy,UserName,CreationDate,Reason Description,CompanyId,CompanyName,DeptID,Territory,OMSSkuCode Name,SkuImage from VW_GWC_ImageAuditFail where Emoney=0 and CompanyID like '%" + SelectedCompany + "%' and DeptID like '%" + SelectedDepartment + "%' and CreatedBy like '%" + SelectedUser + "%' and Convert(varchar(10), creationdate,103) >=  CONVERT(varchar(10) ,@d1 , 103) AND Convert(varchar(10), creationdate,103) <=CONVERT(varchar(10) ,@d2 , 103) and (ImageName like '%" + SearchedValue + "%' OR Reason like '%" + SearchedValue + "%' OR OMSSkuCode like '%" + SearchedValue + "%') ", conn);
            return ds;
        }

        public DataSet GetImageAuditSelectedPrd(string FrmDt, string Todt, string SelectedProducts, string SelectedUser, string[] conn)
        {
            DataSet ds = new DataSet();
            if (SelectedUser == null || SelectedUser == "0") SelectedUser = "";
            ds = fillds("DECLARE @d1 DATETIME DECLARE @d2 DATETIME SET @d1= '" + FrmDt + "' SET @d2= '" + Todt + "' select * from VW_GWC_ImageAudit where Emoney=0 and ID in(" + SelectedProducts + ") and CreatedBy like '%" + SelectedUser + "%' and Convert(varchar(10), creationdate,103) >=  CONVERT(varchar(10) ,@d1 , 103) AND Convert(varchar(10), creationdate,103) <=CONVERT(varchar(10) ,@d2 , 103) ", conn);
            return ds;
        }

        public DataSet GetImageAuditFail(string FrmDt, string Todt, string SelectedCompany, string SelectedDepartment, string SelectedUser, string[] conn)
        {
            DataSet ds = new DataSet();
            if (SelectedCompany == null || SelectedCompany == "0") SelectedCompany = "";
            if (SelectedDepartment == null || SelectedDepartment == "0") SelectedDepartment = "";
            if (SelectedUser == null || SelectedUser == "0") SelectedUser = "";
            ds = fillds("DECLARE @d1 DATETIME DECLARE @d2 DATETIME SET @d1= '" + FrmDt + "' SET @d2= '" + Todt + "' select * from VW_GWC_ImageAuditFail where Emoney=0 and CompanyID like '%" + SelectedCompany + "%' and DeptID like '%" + SelectedDepartment + "%' and CreatedBy like '%" + SelectedUser + "%' and Convert(varchar(10), creationdate,103) >=  CONVERT(varchar(10) ,@d1 , 103) AND Convert(varchar(10), creationdate,103) <=CONVERT(varchar(10) ,@d2 , 103)", conn);
            return ds;
        }

        public DataSet GetImageAuditFailSelectedProduct(string FrmDt, string Todt, string SelectedProducts, string SelectedCompany, string SelectedDepartment, string SelectedUser, string[] conn)
        {
            DataSet ds = new DataSet();
            if (SelectedCompany == null || SelectedCompany == "0") SelectedCompany = "";
            if (SelectedDepartment == null || SelectedDepartment == "0") SelectedDepartment = "";
            if (SelectedUser == null || SelectedUser == "0") SelectedUser = "";
            ds = fillds("DECLARE @d1 DATETIME DECLARE @d2 DATETIME SET @d1= '" + FrmDt + "' SET @d2= '" + Todt + "'  select ID, ImageName ,CreatedBy,UserName,CreationDate,Reason ,CompanyId,CompanyName,DeptID,Territory,OMSSkuCode ,SkuImage from VW_GWC_ImageAuditFail where Emoney=0 and CompanyID like '%" + SelectedCompany + "%' and DeptID like '%" + SelectedDepartment + "%' and CreatedBy like '%" + SelectedUser + "%' and Convert(varchar(10), creationdate,103) >=  CONVERT(varchar(10) ,@d1 , 103) AND Convert(varchar(10), creationdate,103) <=CONVERT(varchar(10) ,@d2 , 103) and ID in(" + SelectedProducts + ") ", conn);
            return ds;
        }

        public tContactPersonDetail GetContactPersonDetailsByID(long ContactID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            tContactPersonDetail cont = new tContactPersonDetail();
            cont = db.tContactPersonDetails.Where(c => c.ID == ContactID).FirstOrDefault();
            return cont;
        }

        public tAddress GetAddressDetailsByID(long AdrsID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            tAddress cont = new tAddress();
            cont = db.tAddresses.Where(c => c.ID == AdrsID).FirstOrDefault();
            return cont;
        }

        public VW_GetUserLocation GetLocationDetailsByID(long AdrsID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            VW_GetUserLocation loc = new VW_GetUserLocation();
            loc = db.VW_GetUserLocation.Where(l => l.ID == AdrsID).FirstOrDefault();
            return loc;
        }

        public void EditContactPerson(tContactPersonDetail ConPerD, string[] conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_GWC_UpdatetContactPersonDetail";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("Name", ConPerD.Name);
            cmd.Parameters.AddWithValue("EmailID", ConPerD.EmailID);
            cmd.Parameters.AddWithValue("MobileNo", ConPerD.MobileNo);
            cmd.Parameters.AddWithValue("LastModifiedBy", ConPerD.LastModifiedBy);
            cmd.Parameters.AddWithValue("LastModifiedDate", ConPerD.LastModifiedDate);
            cmd.Parameters.AddWithValue("ID", ConPerD.ID);
            cmd.ExecuteNonQuery();
        }
        public void EditAddress(tAddress Adrs, string[] conn)
        {
            if (Adrs.City == "" || Adrs.City == null)
            {
                DataSet ds = new DataSet();
                ds = fillds("update taddress set AddressLine1='" + Adrs.AddressLine1 + "' where ID=" + Adrs.ID + "", conn);
            }
            else
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GWC_UpdatettAddress";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("AddressLine1", Adrs.AddressLine1);
                cmd.Parameters.AddWithValue("County", Adrs.County);
                cmd.Parameters.AddWithValue("State", Adrs.State);
                cmd.Parameters.AddWithValue("City", Adrs.City);
                cmd.Parameters.AddWithValue("LastModifiedBy", Adrs.LastModifiedBy);
                cmd.Parameters.AddWithValue("LastModifiedDate", Adrs.LastModifiedDate);
                cmd.Parameters.AddWithValue("ID", Adrs.ID);
                cmd.ExecuteNonQuery();
            }
        }

        public void EditLocation(tContactPersonDetail Con, long AdrsID, string[] conn)
        {
            long ConID = 0;
            DataSet ds = new DataSet();
            ds = fillds("select ID from tContactPersonDetail where ID=(select ShippingID from taddress where ID=" + AdrsID + ")", conn);
            ConID = long.Parse(ds.Tables[0].Rows[0]["ID"].ToString());
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SP_GWC_UpdatettContactPersonDetail";
            cmd.Connection = svr.GetSqlConn(conn);
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("Name", Con.Name);
            cmd.Parameters.AddWithValue("MobileNo", Con.MobileNo);
            cmd.Parameters.AddWithValue("EmailID", Con.EmailID);
            cmd.Parameters.AddWithValue("ID", ConID);
            cmd.ExecuteNonQuery();
        }
        public void AddIntotContactpersonDetail(tContactPersonDetail ConPerD, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            db.AddTotContactPersonDetails(ConPerD);
            db.SaveChanges();
        }

        public void AddIntotAddress(tAddress Adrs, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            db.AddTotAddresses(Adrs);
            db.SaveChanges();
        }
        public string getContact1NameByID(long EdtCon1, string[] conn)
        {
            string Contact1Name = "";
            DataSet ds = new DataSet();
            ds = fillds("select Name from tcontactpersondetail where ID = " + EdtCon1 + " ", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                Contact1Name = ds.Tables[0].Rows[0]["Name"].ToString();
            }
            return Contact1Name;
        }

        public string getContact2NamesByIDs(string EdtCon2, string[] conn)
        {
            string Contact1Name = "";
            DataSet ds = new DataSet();
            ds = fillds("select Name from tcontactpersondetail where ID IN( " + EdtCon2 + " )", conn);
            int cnt = ds.Tables[0].Rows.Count;
            if (cnt > 0)
            {
                for (int i = 0; i < cnt; i++)
                {
                    if (i == 0) { Contact1Name = ds.Tables[0].Rows[i]["Name"].ToString(); }
                    else
                    {
                        Contact1Name = Contact1Name + "," + ds.Tables[0].Rows[i]["Name"].ToString();
                    }
                }
            }
            return Contact1Name;
        }

        public string GetAddressLineByAdrsID(long EdtAddress, string[] conn)
        {
            string Address = "";
            DataSet ds = new DataSet();
            ds = fillds("select AddressLine1 from tAddress where ID =" + EdtAddress + " ", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                Address = ds.Tables[0].Rows[0]["AddressLine1"].ToString();
            }
            return Address;
        }
        
        public string GetLocationCodeName(long EdtAddress, string[] conn)
        {
            string LocCodeName = "";
            DataSet ds = new DataSet();
            ds = fillds("select LocationCode,LocationName from tAddress where ID =" + EdtAddress + " ", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                string LocationCode = ds.Tables[0].Rows[0]["LocationCode"].ToString();
                string LocationName = ds.Tables[0].Rows[0]["LocationName"].ToString();
                LocCodeName = LocationCode + " " + LocationName;
            }
            return LocCodeName;
        }

        public DataSet GetStateList(string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from   QatarState order by state", conn);
            return ds;
        }

        public DataSet GetSKUTransaction(string SelectedProducts, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select VW.Id,VW.SKUId,VW.Name,VW.StoreId,VW.TransactionId,VW.Transactiondate,VW.TransactionType,VW.DispatchQty,VW.ReceivedQty,VW.RequestBY,VW.Approvers,Ter.Territory,Com.Name CompanyName from VW_GWC_PartTransactionDetails VW left outer join mProduct PRD on VW.SKUId=PRD.ID  left outer join mTerritory Ter on Prd.StoreId =Ter.ID left outer join mCompany Com on Ter.ParentID=Com.ID where VW.SKUId=" + SelectedProducts + " order by VW.transactiondate", conn);
            return ds;
        }

        public DataSet GetUserTransaction(string UserSelectedRec, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from VUserTransaction where UserID=" + UserSelectedRec + "", conn);
            return ds;
        }

        public List<mCompany> GetUserCompanyNameNEW(long UID, string[] conn)
        {
            using (BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                List<mCompany> companylist = new List<mCompany>();

                companylist = (from cmp in ce.mCompanies
                               join ter in ce.mTerritories
                               on cmp.ID equals ter.ParentID
                               join utd in ce.mUserTerritoryDetails
                               on ter.ID equals utd.TerritoryID
                               where utd.UserID == UID
                               select cmp).GroupBy(x => x.ID).Select(z => z.OrderBy(i => i.ID).FirstOrDefault()).ToList();

                return companylist;
            }
        }

        public List<mTerritory> GetAddedDepartmentList(int ComapnyID, long UserID, string[] conn)
        {
            using (BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn)))
            {
                List<mTerritory> DeptList = new List<mTerritory>();
                DeptList = (from t in ce.mTerritories
                            join u in ce.mUserTerritoryDetails
                            on t.ID equals u.TerritoryID
                            where u.UserID == UserID && t.ParentID == ComapnyID && t.Emoney!=true
                            orderby t.Territory
                            select t).ToList();
                DeptList = (from u in DeptList
                            select u).Distinct().ToList();
                return DeptList;
            }
        }



        public List<mTerritory> GetAddedALLDepartmentList(string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mTerritory> DeptList = new List<mTerritory>();
            DeptList = (from t in ce.mTerritories
                        orderby t.Territory
                        select t).ToList();
            DeptList = (from u in DeptList
                        select u).Distinct().ToList();
            return DeptList;
        }

        public DataSet GetAddedDepartmentListDS(int ComapnyID, long UserID, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select t.ID,t.Territory from mTerritory t left outer join mUserTerritoryDetail u on t.ID= u.TerritoryID where u.UserID=" + UserID + " and t.ParentID=" + ComapnyID + " order by t.Territory", conn);
            return ds;
        }

        public DataSet GetAllDriverList(string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select ID, CONCAT(FirstName,' ',LastName) as DName from mUserProfileHead where UserType='Driver'", conn);
            return ds;
        }

        public DataSet GetAllOrderDelivery(string FrmDt, string Todt, string SelectedCompany, string SelectedDepartment, string SelectedDriver, string SelectedPaymentMode, string[] conn)
        {
            DataSet ds = new DataSet();
            if (SelectedCompany == null || SelectedCompany == "0") SelectedCompany = "";
            if (SelectedDepartment == null || SelectedDepartment == "0") SelectedDepartment = "";
            if (SelectedDriver == "0") SelectedDriver = "";
            if (SelectedPaymentMode == null || SelectedPaymentMode == "0") SelectedPaymentMode = "";
            // ds = fillds("Select * from VW_GetAllOrder   where ParentID like '%" + SelectedCompany + "%' and StoreId like '%" + SelectedDepartment + "%' and  RequestBy Like '%" + SelectedUser + "%' and status like '%" + SelectedStatus + "%'", conn);
            //ds = fillds("DECLARE @d1 DATETIME DECLARE @d2 DATETIME SET @d1= '" + FrmDt + "' SET @d2= '" + Todt + "' Select * from GetOrderDeliveryDetails where CompanyID like '%" + SelectedCompany + "%' and DepartmentID like '%" + SelectedDepartment + "%' and DriverID like '%" + SelectedDriver + "%' and PaymentMode like '%" + SelectedPaymentMode + "%'", conn);

            ds = fillds("DECLARE @d1 DATETIME DECLARE @d2 DATETIME SET @d1= '" + FrmDt + "' SET @d2= '" + Todt + "' Select * from GetOrderDeliveryDetails where Emoney=0 and CompanyID like '%" + SelectedCompany + "%' and DepartmentID like '%" + SelectedDepartment + "%' and DriverID like '%" + SelectedDriver + "%' and PaymentID like '%" + SelectedPaymentMode + "%' and  Convert(date, Orderdate) >= CONVERT(date, @d1) AND Convert(date, Orderdate) <= CONVERT(date, @d2) ", conn);
            return ds;
        }

        public DataSet GetAllOrderDeliveryDataSelectedRow(string SelectedOrder, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("Select * from GetOrderDeliveryDetails where Emoney=0 and  Id in(" + SelectedOrder + ")", conn);
            return ds;
        }

        public DataSet GetAllSlaData(string FrmDt, string Todt, string SelectedCompany, string SelectedDepartment, string Status, string SelectedDriver, string SelectedDeliveryType, string[] conn)
        {
            DataSet ds = new DataSet();
            if (SelectedCompany == null || SelectedCompany == "0") SelectedCompany = "";
            if (SelectedDepartment == null || SelectedDepartment == "0") SelectedDepartment = "";
            if (SelectedDriver == null || SelectedDriver == "0") SelectedDriver = "";
            if (SelectedDeliveryType == null || SelectedDeliveryType == "0") SelectedDeliveryType = "";
            if (Status == null || Status == "0") Status = "";
            // ds = fillds("Select * from VW_GetAllOrder   where ParentID like '%" + SelectedCompany + "%' and StoreId like '%" + SelectedDepartment + "%' and  RequestBy Like '%" + SelectedUser + "%' and status like '%" + SelectedStatus + "%'", conn);

            ds = fillds("DECLARE @d1 DATETIME DECLARE @d2 DATETIME SET @d1= '" + FrmDt + "' SET @d2= '" + Todt + "' Select * from GetSlaDetails where Emoney=0 and orderType='Ecommerce' and CompanyID like '%" + SelectedCompany + "%' and DepartmentID like '%" + SelectedDepartment + "%' and StatusID like'%" + Status + "%' and DriverID like '%" + SelectedDriver + "%' and DeliveryType like '%" + SelectedDeliveryType + "%'", conn);
            return ds;
        }

        public DataSet GetAllSlaDataSelectedRow(string SelectedOrder, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("Select * from GetSlaDetails where Emoney=0 and Id in(" + SelectedOrder + ")", conn);
            return ds;
        }

        public DataSet GetToTalDeliveryVSTotalReq(string FrmDt, string Todt, string SelectedCompany, string SelectedDepartment, string[] conn)
        {
            DataSet ds = new DataSet();
            if (SelectedCompany == null || SelectedCompany == "0") SelectedCompany = "";
            if (SelectedDepartment == null || SelectedDepartment == "0") SelectedDepartment = "";



            ds = fillds("DECLARE @d1 DATETIME DECLARE @d2 DATETIME SET @d1= '" + FrmDt + "' SET @d2= '" + Todt + "' Select * from ToTalDeliveryVsTotalRequest where Emoney=0 and CompanyID like '%" + SelectedCompany + "%' and DepartmentID like '%" + SelectedDepartment + "%'", conn);
            return ds;
        }

        public DataSet GetToTalDeliveryVSTotalReqDataSelectedRow(string FrmDt, string Todt, string SelectedOrder, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("DECLARE @d1 DATETIME DECLARE @d2 DATETIME SET @d1= '" + FrmDt + "' SET @d2= '" + Todt + "' Select * from ToTalDeliveryVsTotalRequest where Emoney=0 and SkuId in(" + SelectedOrder + ")", conn);
            return ds;
        }

        public List<SP_GetUsers_Result> GetUsersDepartmentWise(string SelectedCompany, string SelectedDepartment, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            if (SelectedCompany == "0") SelectedDepartment = "";
            if (SelectedDepartment == "0") SelectedDepartment = "";
            List<SP_GetUsers_Result> UsersList = new List<SP_GetUsers_Result>();
            UsersList = (from cl in ce.SP_GetUsers(SelectedCompany, SelectedDepartment)
                         orderby cl.Name
                         select cl).ToList();
            return UsersList;
        }

        #region DeliveryLogReport

        public DataSet GetDriverList(string companyid, string departmentid, string[] conn)
        {
            DataSet ds = new DataSet();
            if (companyid == "0") companyid = "";
            if (departmentid == "0") departmentid = "";
            ds = fillds(" select MU.Id,FirstName +' '+ LastName as Name  from muserprofilehead MU left outer join mRole MR on  MU.RoleID=MR.ID  where MR.RoleName='Driver'     ", conn);
            return ds;
        }

        public DataSet GetDriverStatusList(string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds(" select Id,status from mstatus where id in (25,26,8) ", conn);
            return ds;
        }

        public DataSet GetDriverLogRpt(string FrmDt, string Todt, string SelectedCustomer, string SelectedDepartment, string SelectedStatus, string SelectedUser, string SelectedPaymentMode, string[] conn)
        {
            DataSet ds = new DataSet();
            if (SelectedCustomer == null || SelectedCustomer == "0") SelectedCustomer = "";
            if (SelectedDepartment == null || SelectedDepartment == "0") SelectedDepartment = "";
            if (SelectedStatus == null || SelectedStatus == "0") SelectedStatus = "";
            if (SelectedUser == null || SelectedUser == "0") SelectedUser = "";
            if (SelectedPaymentMode == null || SelectedPaymentMode == "0") SelectedPaymentMode = "";//
            ds = fillds("DECLARE @d1 DATETIME DECLARE @d2 DATETIME SET @d1= '" + FrmDt + "' SET @d2= '" + Todt + "' select OrderNo as Id,RequestNo as OrderNo, OrderDate,Uname as UserName  from  View_DeliveryLogReport where Emoney=0 and  OrderStatusId like '%" + SelectedStatus + "%' and USERId like '%" + SelectedUser + "%' and StoreId like '%" + SelectedDepartment + "%' and PaymentID like '%" + SelectedPaymentMode + "%' and  Convert(varchar(10), Orderdate,103) >=  CONVERT(varchar(10) ,@d1 , 103) AND Convert(varchar(10), Orderdate,103) <=CONVERT(varchar(10) ,@d2 , 103) ", conn);
            return ds;
        }

        public DataSet GetDriverlogReport(string selectedRequest, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from View_DeliveryLogReport where Emoney=0 and OrderNo  in(" + selectedRequest + ")", conn);
            return ds;
        }

        public DataSet AllGetDriverlogReport(string FromDt, string ToDt, string SelectedCompany, string SelectedDepartment, string SelectedStatus, string SelectedUser, string SelectedPaymentMode, string[] conn)
        {
            DataSet ds = new DataSet();
            if (SelectedCompany == null || SelectedCompany == "0") SelectedCompany = "";
            if (SelectedDepartment == null || SelectedDepartment == "0") SelectedDepartment = "";
            if (SelectedStatus == null || SelectedStatus == "0") SelectedStatus = "";
            if (SelectedUser == null || SelectedUser == "0") SelectedUser = "";
            if (SelectedPaymentMode == null || SelectedPaymentMode == "0") SelectedPaymentMode = "";
            ds = fillds(" DECLARE @d1 DATETIME DECLARE @d2 DATETIME SET @d1= '" + FromDt + "' SET @d2= '" + ToDt + "' select * from View_DeliveryLogReport  where  Emoney=0 and  parentid like  '%" + SelectedCompany + "%' and OrderStatusId like '%" + SelectedStatus + "%' and USERId like '%" + SelectedUser + "%' and StoreId like '%" + SelectedDepartment + "%' and PaymentID like '%" + SelectedPaymentMode + "%' and  Convert(varchar(10), Orderdate,103) >=  CONVERT(varchar(10) ,@d1 , 103) AND Convert(varchar(10), Orderdate,103) <=CONVERT(varchar(10) ,@d2 , 103) ", conn);
            return ds;
        }


        public string GetcompanyNamerpt(long companyid, string[] conn)
        {
            string companyName = "";
            DataSet ds = new DataSet();
            ds = fillds(" select name from mcompany where id= " + companyid + " ", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                companyName = ds.Tables[0].Rows[0]["name"].ToString();
            }
            return companyName;
        }

        public string GetdepartmentNamerpt(long departmentid, string[] conn)
        {
            string departmentName = "";
            DataSet ds = new DataSet();
            ds = fillds(" 	 select territory from mTerritory where id =" + departmentid + " ", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                departmentName = ds.Tables[0].Rows[0]["territory"].ToString();
            }
            return departmentName;
        }

        public string GetpaymentmodeNamerpt(long paymentmodeid, string[] conn)
        {
            string paymentmodeName = "";
            DataSet ds = new DataSet();
            ds = fillds(" 	 select methodname from mPaymentMethodMain where id =" + paymentmodeid + " ", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                paymentmodeName = ds.Tables[0].Rows[0]["methodname"].ToString();
            }
            return paymentmodeName;
        }

        #endregion

        #region oredrleadtime

        public DataSet OrderLeadReport(string selectedRequest, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("Select orderno as Id,case when Ordernumber=''  then 'NA' else Ordernumber end Ordernumber,Title,Orderdate,DeliveryDate,ContactID1,GrandTotal,OrderNo,case when CName1 IS NULL then 'NA' else CName1 end CName1,case when Cname2 IS NULL then 'NA' else Cname2 end Cname2,ContactId2,case when Remark='' then 'NA' else Remark end Remark,Status,StatusName,AddressId,case when AddressLine1 IS NULL then 'NA' else AddressLine1 end AddressLine1,RequestBy,UserName,StoreId,Territory,ParentID,CompanyName,StoreCode,PaymentMethod  from VW_GetAllOrder where Emoney=0 and id  in(" + selectedRequest + ")", conn);
            return ds;
        }

        #endregion


        #region Ecommerce Report


        public DataSet GetAllEcommerce1Data(string FrmDt, string Todt, string SelectedCustomer, string SelectedDepartment, string SelectedStatus, string SelectedUser, string SelectedPaymentMode, string[] conn)
        {
            DataSet ds = new DataSet();
            if (SelectedCustomer == null || SelectedCustomer == "0") SelectedCustomer = "";
            if (SelectedDepartment == null || SelectedDepartment == "0") SelectedDepartment = "";
            if (SelectedStatus == null || SelectedStatus == "0") SelectedStatus = "";
            if (SelectedUser == null || SelectedUser == "0") SelectedUser = "";
            if (SelectedPaymentMode == null || SelectedPaymentMode == "0") SelectedPaymentMode = "";
            //  ds = fillds("DECLARE @d1 DATETIME DECLARE @d2 DATETIME SET @d1= '" + FrmDt + "' SET @d2= '" + Todt + "' select OH.Id,OH.OrderNo,OH.orderType,OH.Orderdate,   ISNULL( UPH.FirstName +''+ UPH.Lastname , 'N/A')  as UserName from tOrderHead OH  left outer join  mUserProfileHead UPH on OH.requestby =UPH.id where orderType in('PreOrder','NormalOrder') and StoreId like '%" + SelectedDepartment + "%' and Status like '%" + SelectedStatus + "%' and RequestBy like '%" + SelectedUser + "%' and PaymentID like '%" + SelectedPaymentMode + "%' and  Convert(varchar(10), Orderdate,103) >=  CONVERT(varchar(10) ,@d1 , 103) AND Convert(varchar(10), Orderdate,103) <=CONVERT(varchar(10) ,@d2 , 103) ", conn);
            ds = fillds("DECLARE @d1 DATETIME DECLARE @d2 DATETIME SET @d1= '" + FrmDt + "' SET @d2= '" + Todt + "'   select * from View_EcommerceReport1  where Emoney=0 and    StoreId like '%" + SelectedDepartment + "%' and Status like '%" + SelectedStatus + "%' and RequestBy like '%" + SelectedUser + "%' and PaymentID like '%" + SelectedPaymentMode + "%' and  Convert(varchar(10), Orderdate,111) >=  CONVERT(varchar(10) ,@d1 , 111) AND Convert(varchar(10), Orderdate,111) <=CONVERT(varchar(10) ,@d2 , 111) ", conn);
            return ds;
        }

        public DataSet GetAllEcommerce1DataSelectedRequestID(string selectedRequest, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds(" select OH.Id,OH.OrderNo,OH.orderType,OH.Orderdate,   ISNULL( UPH.FirstName +''+ UPH.Lastname , 'N/A')  as UserName from tOrderHead OH  left outer join  mUserProfileHead UPH on OH.requestby =UPH.id where orderType in('PreOrder','NormalOrder') and  OH.Id in(" + selectedRequest + ")", conn);
            return ds;
        }

        public DataSet GetAllEcommerce1DataReport1(string selectedRequest, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds(" select * from View_EcommerceReport1 where Emoney=0 and orderid in(" + selectedRequest + ")", conn);
            return ds;
        }

        public string GetStatusName(long statusid, string[] conn)
        {
            string result = "";
            DataSet ds = new DataSet();
            ds = fillds(" select status from mstatus where id=" + statusid + "", conn);
            result = ds.Tables[0].Rows[0]["status"].ToString();
            return result;
        }

        #endregion

        #region Normal Order Delivery note report

        //created by suraj khopade
        public DataSet GetNormalOrderDeliveryNoteDataGrid(string FrmDt, string Todt, string SelectedCustomer, string SelectedDepartment, string OrderNumber, string[] conn)
        {
            DataSet ds = new DataSet();
            if (SelectedCustomer == null || SelectedCustomer == "0") SelectedCustomer = "";
            if (SelectedDepartment == null || SelectedDepartment == "0") SelectedDepartment = "";
            if (OrderNumber == null || OrderNumber == "0" || OrderNumber == "") OrderNumber = "";

            //ds = fillds(" select OH.Id,OH.OrderNo,OH.orderType,OH.Orderdate,   ISNULL( UPH.FirstName +''+ UPH.Lastname , 'N/A')  as UserName from tOrderHead OH  left outer join  mUserProfileHead UPH on OH.requestby =UPH.id where orderType not in('PreOrder','NormalOrder') and   StoreId like '%%' and(Ordernumber like '%" + OrderNumber + "%' OR OrderNo like'%" + OrderNumber + "%') and and OH.status in(7,8,10,25,26,34,35,38,39,10034,10036) ", conn);
            //return ds;

            //DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_getNormalOrderDumpData";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@OrderNumber", OrderNumber);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }


        public DataSet GetNormalOrderReportDataDelivery(long OrderID, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_getNormalOrderReportDataDelivery";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@OrderID", OrderID);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
            //ds = fillds("SELECT PMM.Methodname , TCPD.Mobileno, Tod.Prod_Name, Tod.Prod_code, Tod.Price, Tod.Orderqty, oh.OrderNumber, Mt.Territory, ta.AddressLine1, ta.AddressLine2,ta.City,Ta.state,Ta.County,Ta.Zipcode, Mdb.OrderNoBarCode,Mdb.AmtPaidBarCode, oh.OrderNo,oh.OrderNumber,convert(varchar,oh.Orderdate,103)Orderdate, oh.GrandTotal, convert(varchar,oh.Orderdate,103)OrderCreationDate, convert(varchar,OH.Deliverydate,103)IDExpiryDate FROM  Torderhead oh LEFT OUTER JOIN mPaymentMethodMain as PMM on  Oh.PaymentID = Pmm.Id Left Outer Join tContactPersonDetail TCPD on oh.ContactID1 = TCPD.ID LEFT OUTER JOIN Torderdetail Tod on oh.id = Tod.Orderheadid LEFT OUTER JOIN mDeliveryBarcode Mdb ON oh.Id = Mdb.OrderID LEFT OUTER JOIN   mTerritory Mt on oh.Storeid = mt.id LEFT OUTER JOIN  tAddress Ta on oh.locationid = ta.Id where  oh.id = " + OrderID + "", conn);

            //return ds;


        }


        public DataSet GetNormalOrderDeliveryNoteData(string FrmDt, string Todt, string SelectedCustomer, string SelectedDepartment, string OrderNumber, string[] conn)
        {
            DataSet ds = new DataSet();
            if (SelectedCustomer == null || SelectedCustomer == "0") SelectedCustomer = "";
            if (SelectedDepartment == null || SelectedDepartment == "0") SelectedDepartment = "";
            if (OrderNumber == null || OrderNumber == "0" || OrderNumber == "") OrderNumber = "";

            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_getNormalOrderDeliveryNoteData";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@OrderNumber", OrderNumber);
                cmd.Parameters.AddWithValue("@FrmDt", FrmDt);
                cmd.Parameters.AddWithValue("@Todt", Todt);
                cmd.Parameters.AddWithValue("@SelectedDepartment", SelectedDepartment);

                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }

            //ds = fillds("DECLARE @d1 DATETIME DECLARE @d2 DATETIME SET @d1= '" + FrmDt + "' SET @d2= '" + Todt + "' SELECT PMM.Methodname , TCPD.Mobileno, Tod.Prod_Name, Tod.Prod_code, Tod.Price, Tod.Orderqty, oh.OrderNumber, Mt.Territory, ta.AddressLine1, ta.AddressLine2,ta.City,Ta.state,Ta.County,Ta.Zipcode, Mdb.OrderNoBarCode,Mdb.AmtPaidBarCode, oh.OrderNo,oh.OrderNumber,convert(varchar,oh.Orderdate,103)Orderdate, oh.GrandTotal, convert(varchar,oh.Orderdate,103)OrderCreationDate, convert(varchar,OH.Deliverydate,103)IDExpiryDate FROM  Torderhead oh LEFT OUTER JOIN mPaymentMethodMain as PMM on  Oh.PaymentID = Pmm.Id Left Outer Join tContactPersonDetail TCPD on oh.ContactID1 = TCPD.ID LEFT OUTER JOIN Torderdetail Tod on oh.id = Tod.Orderheadid LEFT OUTER JOIN mDeliveryBarcode Mdb ON oh.Id = Mdb.OrderID LEFT OUTER JOIN   mTerritory Mt on oh.Storeid = mt.id LEFT OUTER JOIN  tAddress Ta on oh.locationid = ta.Id where StoreId like '%" + SelectedDepartment + "%' and(OrderNo like'%" + OrderNumber + "%')and  Convert(varchar(10), OrderCreationDate,103) >=  CONVERT(varchar(10) ,@d1 , 103) AND Convert(varchar(10), OrderCreationDate,103) <=CONVERT(varchar(10) ,@d2 , 103)  ", conn);
            //return ds;
        }


        public string GetcompanyNameNormalOrder(long companyid, string[] conn)
        {
            string companyName = "";
            DataSet ds = new DataSet();
            ds = fillds(" select name from mcompany where id= " + companyid + " ", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                companyName = ds.Tables[0].Rows[0]["name"].ToString();
            }
            return companyName;
        }

        public string GetdepartmentNameNormalOrder(long departmentid, string[] conn)
        {
            string departmentName = "";
            DataSet ds = new DataSet();
            ds = fillds(" 	 select territory from mTerritory where id =" + departmentid + " ", conn);
            if (ds.Tables[0].Rows.Count > 0)
            {
                departmentName = ds.Tables[0].Rows[0]["territory"].ToString();
            }
            return departmentName;
        }
        //created by suraj khopade

        #endregion

        //created by suraj khopade
        public DataSet GetTransactionCategory(string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_BindDDLTransactionCategory";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }

        public DataSet GetcriteriaList(int categoryID, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_BindDDLTransactioncriteria";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@categoryID", categoryID);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }

        //public string GetcriteriaList(int categoryID, string[] conn)
        //{
        //    DataSet ds = new DataSet();
        //    string criterianame = "";
        //    using (SqlCommand cmd = new SqlCommand())
        //    {
        //        SqlDataAdapter da = new SqlDataAdapter();
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        cmd.CommandText = "SP_BindDDLTransactioncriteria";
        //        cmd.Connection = svr.GetSqlConn(conn);
        //        cmd.Parameters.Clear();
        //        cmd.Parameters.AddWithValue("@categoryID", categoryID);
        //        da.SelectCommand = cmd;
        //        da.Fill(ds);
        //        if (ds.Tables[0].Rows.Count > 0)
        //        {
        //                criterianame = ds.Tables[0].Rows[0]["Criteria"].ToString();
        //        }
        //        //return ds;
        //        return criterianame;
        //    }
        //}
        //created by suraj khopade
        #region Delivery note report

        public DataSet GetReportDataDelivery(Int64 OrderID, string[] conn)
        {
            DataSet ds = new DataSet();
            //  ds = fillds("select tOrderDetail.OrderHeadId,convert(date,tECommHead.OrderCreationDate,113)OrderCreationDate,tECommHead.CustomerFirstName +' '+ tECommHead.CustomerLastName as cname,tECommHead.BuildingName +' '+tECommHead.Streetname +' '+ tECommHead.City+' '+tECommHead.Country as daddress,tECommHead.vodafoneStoreName as rname,tECommHead.BuildingName2 +' '+tECommHead.Streetname2 +' '+ tECommHead.City2+' '+tECommHead.Country2 as rdaddress,tECommHead.Idt,tECommHead.PaymentMethod,tECommHead.ContactNumber,tECommHead.IDExpiryDate  ,tOrderDetail.Prod_Name,mProductCategory.Name,tOrderDetail.OrderQty,tOrderDetail.Price,tOrderDetail.Total,AmountPaid,AmounttobeCollected from tOrderDetail  left outer join  mProduct on mProduct.ID=tOrderDetail.SkuId   left outer join mProductCategory on mProductCategory.ID=mProduct.ProductCategoryID    left outer join tECommHead on tECommHead.OrderID=tOrderDetail.OrderHeadId  where  tOrderDetail.OrderHeadId=" + OrderID + " ", conn);
            // ds = fillds("SELECT dbo.tOrderDetail.OrderHeadId,convert(date,tECommHead.OrderCreationDate,113)OrderCreationDate,tECommHead.CustomerFirstName +' '+ tECommHead.CustomerLastName as cname,tECommHead.BuildingName +' '+tECommHead.Streetname +' '+ tECommHead.City+' '+tECommHead.Country as daddress,tECommHead.vodafoneStoreName as rname,tECommHead.BuildingName2 +' '+tECommHead.Streetname2 +' '+ tECommHead.City2+' '+tECommHead.Country2 as rdaddress,tECommHead.IDExpiryDate, dbo.tOrderDetail.Prod_Name, dbo.mProductCategory.Name,dbo.mProduct.Productcode, dbo.tOrderDetail.OrderQty, dbo.tOrderDetail.Price, dbo.tOrderDetail.Total, dbo.tECommHead.AmountPaid, dbo.tECommHead.AmountToBeCollected, dbo.mProduct.ProductType, dbo.mDeliveryBarcode.OrderNoBarCode, dbo.mDeliveryBarcode.AmtPaidBarCode, dbo.tOrderHead.OrderNo, dbo.tOrderHead.GrandTotal, dbo.tECommHead.Idt, dbo.tECommHead.PaymentMethod, dbo.tECommHead.ContactNumber, dbo.tECommHead.EcomOrderNumber FROM   dbo.tOrderDetail LEFT OUTER JOIN dbo.mProduct ON dbo.mProduct.ID = dbo.tOrderDetail.SkuId LEFT OUTER JOIN   dbo.mProductCategory ON dbo.mProductCategory.ID = dbo.mProduct.ProductCategoryID LEFT OUTER JOIN   dbo.tECommHead ON dbo.tECommHead.OrderID = dbo.tOrderDetail.OrderHeadId LEFT OUTER JOIN    dbo.mDeliveryBarcode ON dbo.mDeliveryBarcode.OrderID = dbo.tOrderDetail.OrderHeadId LEFT OUTER JOIN dbo.tOrderHead ON dbo.tOrderHead.Id = dbo.tOrderDetail.OrderHeadId where  tOrderDetail.OrderHeadId=" + OrderID + "", conn);
            ds = fillds("SELECT dbo.tECommHead.PaymentMethod, dbo.tECommHead.ContactNumber, dbo.tECommDetail.ProductType, dbo.tECommDetail.ProductName, dbo.tECommDetail.ProductCode, dbo.tECommDetail.UnitPrice,  dbo.tECommDetail.Quantity, dbo.tECommDetail.PackageId, dbo.tECommDetail.PackageStatus, dbo.tECommDetail.UniqueNo, dbo.tECommDetail.VoucherCode, dbo.tECommDetail.Token, dbo.tECommHead.EcomOrderNumber,  dbo.tECommHead.CustomerFirstName, dbo.tECommHead.CustomerLastName, dbo.tECommHead.VodafoneStoreName, dbo.tECommHead.BuildingName2, dbo.tECommHead.Streetname2, dbo.tECommHead.City2, dbo.tECommHead.Country2, dbo.tECommHead.POBox, dbo.tECommHead.PartialPayment, dbo.tECommHead.AmountPaid, dbo.tECommHead.AmountToBeCollected, dbo.mDeliveryBarcode.OrderNoBarCode,dbo.mDeliveryBarcode.AmtPaidBarCode, dbo.tOrderHead.OrderNo, dbo.tOrderHead.GrandTotal, dbo.tECommHead.Idt, convert(varchar,dbo.tECommHead.OrderCreationDate,103)OrderCreationDate, convert(varchar,dbo.tECommHead.IDExpiryDate,103)IDExpiryDate,tECommHead.DeliveryType FROM  dbo.tECommHead LEFT OUTER JOIN  dbo.tOrderHead LEFT OUTER JOIN dbo.mDeliveryBarcode ON dbo.tOrderHead.Id = dbo.mDeliveryBarcode.OrderID ON dbo.tECommHead.OrderID = dbo.tOrderHead.Id RIGHT OUTER JOIN dbo.tECommDetail ON dbo.tECommHead.ID = dbo.tECommDetail.EcomHeadID where  tOrderHead.id=" + OrderID + "", conn);

            return ds;
        }

        public DataSet GetDeliveryNoteDataGrid(string FrmDt, string Todt, string SelectedCustomer, string SelectedDepartment, string OrderNumber, string[] conn)
        {
            DataSet ds = new DataSet();
            if (SelectedCustomer == null || SelectedCustomer == "0") SelectedCustomer = "";
            if (SelectedDepartment == null || SelectedDepartment == "0") SelectedDepartment = "";
            if (OrderNumber == null || OrderNumber == "0" || OrderNumber == "") OrderNumber = "";
            //old ds = fillds("DECLARE @d1 DATETIME DECLARE @d2 DATETIME SET @d1= '" + FrmDt + "' SET @d2= '" + Todt + "' select OH.Id,OH.OrderNo,OH.orderType,OH.Orderdate,   ISNULL( UPH.FirstName +''+ UPH.Lastname , 'N/A')  as UserName from tOrderHead OH  left outer join  mUserProfileHead UPH on OH.requestby =UPH.id where orderType in('PreOrder','NormalOrder') and   StoreId like '%"+SelectedDepartment+"%'    and(Ordernumber like '%"+OrderNumber+"%' OR OrderNo like'%"+ OrderNumber + "%') and  Convert(varchar(10), Orderdate,111) >=  CONVERT(varchar(10) ,@d1 , 111) AND Convert(varchar(10), Orderdate,111) <=CONVERT(varchar(10) ,@d2 , 111) ", conn);
            ds = fillds(" select OH.Id,OH.OrderNo,OH.orderType,OH.Orderdate,   ISNULL( UPH.FirstName +''+ UPH.Lastname , 'N/A')  as UserName from tOrderHead OH  left outer join  mUserProfileHead UPH on OH.requestby =UPH.id where orderType in('PreOrder','NormalOrder') and   StoreId like '%%'    and(Ordernumber like '%" + OrderNumber + "%' OR OrderNo like'%" + OrderNumber + "%') and OH.status in(7,8,10,25,26,34,35,38,39,10034,10036,10041) ", conn);
            return ds;
        }

        public DataSet GetDeliveryNoteData(string FrmDt, string Todt, string SelectedCustomer, string SelectedDepartment, string OrderNumber, string[] conn)
        {
            DataSet ds = new DataSet();
            if (SelectedCustomer == null || SelectedCustomer == "0") SelectedCustomer = "";
            if (SelectedDepartment == null || SelectedDepartment == "0") SelectedDepartment = "";
            if (OrderNumber == null || OrderNumber == "0" || OrderNumber == "") OrderNumber = "";
            ds = fillds("DECLARE @d1 DATETIME DECLARE @d2 DATETIME SET @d1= '" + FrmDt + "' SET @d2= '" + Todt + "' SELECT dbo.tECommHead.PaymentMethod, dbo.tECommHead.ContactNumber, dbo.tECommDetail.ProductType, dbo.tECommDetail.ProductName, dbo.tECommDetail.ProductCode, dbo.tECommDetail.UnitPrice,  dbo.tECommDetail.Quantity, dbo.tECommDetail.PackageId, dbo.tECommDetail.PackageStatus, dbo.tECommDetail.UniqueNo, dbo.tECommDetail.VoucherCode, dbo.tECommDetail.Token, dbo.tECommHead.EcomOrderNumber,  dbo.tECommHead.CustomerFirstName, dbo.tECommHead.CustomerLastName, dbo.tECommHead.VodafoneStoreName, dbo.tECommHead.BuildingName2, dbo.tECommHead.Streetname2, dbo.tECommHead.City2, dbo.tECommHead.Country2, dbo.tECommHead.POBox, dbo.tECommHead.PartialPayment, dbo.tECommHead.AmountPaid, dbo.tECommHead.AmountToBeCollected, dbo.mDeliveryBarcode.OrderNoBarCode,dbo.mDeliveryBarcode.AmtPaidBarCode, dbo.tOrderHead.OrderNo, dbo.tOrderHead.GrandTotal, dbo.tECommHead.Idt, convert(varchar,dbo.tECommHead.OrderCreationDate,103)OrderCreationDate, convert(varchar,dbo.tECommHead.IDExpiryDate,103)IDExpiryDate,tECommHead.DeliveryType,tOrderHead.storeid FROM  dbo.tECommHead LEFT OUTER JOIN  dbo.tOrderHead LEFT OUTER JOIN dbo.mDeliveryBarcode ON dbo.tOrderHead.Id = dbo.mDeliveryBarcode.OrderID ON dbo.tECommHead.OrderID = dbo.tOrderHead.Id RIGHT OUTER JOIN dbo.tECommDetail ON dbo.tECommHead.ID = dbo.tECommDetail.EcomHeadID   left outer join mTerritory on mTerritory.ID=tOrderHead.storeid where  StoreId like '%" + SelectedDepartment + "%' and (EcomOrdernumber like '%" + OrderNumber + "%' OR OrderNo like'%" + OrderNumber + "%') and  Convert(varchar(10), OrderCreationDate,103) >=  CONVERT(varchar(10) ,@d1 , 103) AND Convert(varchar(10), OrderCreationDate,103) <=CONVERT(varchar(10) ,@d2 , 103) ", conn);
            //  ds = fillds("DECLARE @d1 DATETIME DECLARE @d2 DATETIME SET @d1= '" + FrmDt + "' SET @d2= '" + Todt + "'   select * from View_EcommerceReport1  where    StoreId like '%" + SelectedDepartment + "%' and Status like '%" + SelectedStatus + "%' and RequestBy like '%" + SelectedUser + "%' and PaymentID like '%" + SelectedPaymentMode + "%' and  Convert(varchar(10), Orderdate,103) >=  CONVERT(varchar(10) ,@d1 , 103) AND Convert(varchar(10), Orderdate,103) <=CONVERT(varchar(10) ,@d2 , 103) ", conn);
            return ds;
        }


        #endregion


        #region Ecomtransaction suraj j
        public DataSet GetAllEcommDetailDataReport1(string selectedRequest, string[] conn)
        {
            DataSet ds = new DataSet();
            //ds = fillds(" select * from VW_ecomtransdetail where Orderid in(" + selectedRequest + ")", conn);
            ds = fillds(" SELECT        tECommDetail.*,tECommHead.Orderid FROM  tECommDetail LEFT OUTER JOIN tECommHead ON tECommDetail.EcomHeadID = tECommHead.ID LEFT OUTER JOIN tOrderHead ON tECommHead.OrderID = tOrderHead.Id where Orderid in(" + selectedRequest + ")", conn);
            return ds;
        }

        public DataSet ExportDatainExcel(string FrmDt, string Todt, string SelectedCustomer, string SelectedDepartment, string SelectedStatus, string SelectedUser, string SelectedPaymentMode, string[] conn)
        {
            if (SelectedCustomer == null || SelectedCustomer == "0") SelectedCustomer = "";
            if (SelectedDepartment == null || SelectedDepartment == "0") SelectedDepartment = "";
            if (SelectedStatus == null || SelectedStatus == "0") SelectedStatus = "";
            if (SelectedUser == null || SelectedUser == "0") SelectedUser = "";
            if (SelectedPaymentMode == null || SelectedPaymentMode == "0") SelectedPaymentMode = "";
            DataSet ds = new DataSet();
            ds = fillds("DECLARE @d1 DATETIME DECLARE @d2 DATETIME SET @d1= '" + FrmDt + "' SET @d2= '" + Todt + "' select * from VW_ExportData where StoreId like '%" + SelectedDepartment + "%'  and Status like '%" + SelectedStatus + "%' and RequestBy like '%" + SelectedUser + "%' and PaymentID like '%" + SelectedPaymentMode + "%'             and  Convert(varchar(10), Orderdate, 103) >= CONVERT(varchar(10), @d1, 103) AND Convert(varchar(10), Orderdate, 103) <= CONVERT(varchar(10), @d2, 103) ", conn);
            return ds;
        }
        public DataSet ExportDatainExcelNew(string FrmDt, string Todt,string orderno, string SelectedCustomer, string SelectedDepartment, string SelectedStatus, string SelectedUser, string SelectedPaymentMode, string[] conn)
        {
           
                if (orderno == null || orderno == "0") orderno = "";
            if (SelectedCustomer == null || SelectedCustomer == "0") SelectedCustomer = "";
            if (SelectedDepartment == null || SelectedDepartment == "0") SelectedDepartment = "";
            if (SelectedStatus == null || SelectedStatus == "0") SelectedStatus = "";
            if (SelectedUser == null || SelectedUser == "0") SelectedUser = "";
            if (SelectedPaymentMode == null || SelectedPaymentMode == "0") SelectedPaymentMode = "";
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_getEcommerceDumpData";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@FrmDt", FrmDt);
                cmd.Parameters.AddWithValue("@Todt", Todt);
                cmd.Parameters.AddWithValue("@selectOrder", orderno);                
                cmd.Parameters.AddWithValue("@SelectedDepartment", SelectedDepartment);
                cmd.Parameters.AddWithValue("@SelectedStatus", SelectedStatus);
                cmd.Parameters.AddWithValue("@SelectedUser", SelectedUser);
                cmd.Parameters.AddWithValue("@SelectedPaymentMode", SelectedPaymentMode);                
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }

        #endregion

        #region QNBN Report

        public DataSet GetUserList(string comid, string deptid, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select m.ID, m.Firstname + ' ' + m.Lastname as Name from mUserProfileHead m left outer join mTerritory t on m.ID = t.UserID where t.id =" + deptid + " ", conn);
            return ds;
        }

        public DataSet GetQNBNStatus(string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from mstatus where status in('Composing','Approved','Rejected','Ready for Dispatch','Cancelled','Pending For Approval Level 2','Pending For Approval Level 3','Cancel by Requester','Pending For Financial Approver','Pending For Cost Center Approver','Request Sent for Approval','Dispatch','Out For Delivery','Return') order by status", conn);
            return ds;
        }

        public DataSet GetEcommStatus(string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select * from mstatus where status in ('Approved', 'Rejected', 'Ready for Dispatch', 'Cancelled', 'Pending For Approval Level 2', 'Request Sent for Approval', 'Dispatch', 'Out For Delivery', 'Return', 'Delivered', 'Upload Document', 'Pending For Document', 'Activated', 'Ready For Activation','Delivered to Hub') order by status", conn);
            return ds;
        }

        public DataSet GetUserListall(string comid, string deptid, string[] conn)
        {
            DataSet ds = new DataSet();
            ds = fillds("select distinct  m.ID, m.Firstname + ' ' + m.Lastname as Name from mUserTerritoryDetail t  left outer join mUserProfileHead m on m.ID = t.UserID where  m.UserType = 'Requestor' and m.active = 1", conn);
            return ds;
        }



        #endregion

        #region Project Type

        public void ADDorEditProjectType(string ID, string projecttype, string Active, long userid, long CompanyID, string state, string[] conn)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                DataSet ds = new DataSet();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_ADDorEditProjectType";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@ID", ID);
                cmd.Parameters.AddWithValue("@projecttype", projecttype);
                cmd.Parameters.AddWithValue("@Active", Active);
                cmd.Parameters.AddWithValue("@userid", userid);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyID);
                cmd.Parameters.AddWithValue("@state", state);
                cmd.ExecuteNonQuery();
            }
        }

        public DataSet GetProjectTypeList(string filter,string place, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetProjectTypeList";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@filter", filter);
                cmd.Parameters.AddWithValue("@place", place);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }

        public string CHKDuplicateProjectType(string ptype, string state, string[] conn)
        {
            string result = "";
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    DataSet ds = new DataSet();
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_CHKDuplicateProjectType";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@ptype", ptype);
                    cmd.Parameters.AddWithValue("@state", state);
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        result = "Yes";
                    }
                    else
                    {
                        result = "No";
                    }
                }
            }
            catch
            {
            }
            return result;
        }



        public DataSet GetProjectTypeData(string ID, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetProjectTypeData";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@ID", ID);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }

        public DataSet BindSiteList(string filter, string pid, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_BindSiteList";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@filter", filter);
                cmd.Parameters.AddWithValue("@pid", pid);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }

        public DataSet GetProjectTypeListDDL(long ID, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetProjectTypeListDDL";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@ID", ID);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }

        public void ADDorEditSite(string ID, string projectype, string sitecode, string sitename, string Longitude, string Latitude, string AccessRequirement, string active, long UId, long CompanyID, string state, string[] conn)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_InsertDataInSiteMaster";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@ID", ID);
                cmd.Parameters.AddWithValue("@ProjectType", projectype);
                cmd.Parameters.AddWithValue("@SiteCode", sitecode);
                cmd.Parameters.AddWithValue("@SiteName", sitename);
                cmd.Parameters.AddWithValue("@Latitude", Latitude);
                cmd.Parameters.AddWithValue("@Longitude", Longitude);
                cmd.Parameters.AddWithValue("@AccessRequirement", AccessRequirement);
                cmd.Parameters.AddWithValue("@Active", active);
                cmd.Parameters.AddWithValue("@CreationBy", UId);
                cmd.Parameters.AddWithValue("@CompanyId", CompanyID);
                cmd.Parameters.AddWithValue("@state", state);
                cmd.ExecuteNonQuery();
            }
        }
        public DataSet GetSiteCodeData(long ID, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetSiteCodeData";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@ID", ID);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }



        public string CHKDuplicatesiteCode(string ptype, string state, string[] conn)
        {
            string result = "";
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    DataSet ds = new DataSet();
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_CHKDuplicatesiteCode";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@ptype", ptype);
                    cmd.Parameters.AddWithValue("@state", state);
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        result = "Yes";
                    }
                    else
                    {
                        result = "No";
                    }
                }
            }
            catch
            {
            }
            return result;
        }
        #endregion


        #region SKU,Site and department tracking report


        public DataSet BindDDLSku(long companyid, long depetid,long uid, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_BindDDLSku";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@companyid", companyid);
                cmd.Parameters.AddWithValue("@depetid", depetid);
                cmd.Parameters.AddWithValue("@uid", uid);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }

        public DataSet GetSkutrackDataGrid(string frmdate, string todate, string skuid, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetSkutrackDataGrid";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@frmdate", frmdate);
                cmd.Parameters.AddWithValue("@todate", todate);
                cmd.Parameters.AddWithValue("@skuid", skuid);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }


        public DataSet GetAllSkudtata(string fdate, string tdate, string skuid, string selectall,string selectedrec,long Uid,long companyid, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetAllSkudtata";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@fdate", fdate);
                cmd.Parameters.AddWithValue("@tdate", tdate);
                cmd.Parameters.AddWithValue("@skuid", skuid);
                cmd.Parameters.AddWithValue("@selectall", selectall);
                cmd.Parameters.AddWithValue("@selectedrec", selectedrec);
                cmd.Parameters.AddWithValue("@Uid", Uid);
                cmd.Parameters.AddWithValue("@companyid", companyid);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }

        public DataSet BindDDLSitecode(long companyid, long depetid, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_BindDDLSitecode";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@companyid", companyid);
                cmd.Parameters.AddWithValue("@depetid", depetid);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }


        public DataSet GetSitetrackDataGrid(string frmdate, string todate, string siteid, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetSitetrackDataGrid";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@frmdate", frmdate);
                cmd.Parameters.AddWithValue("@todate", todate);
                cmd.Parameters.AddWithValue("@siteid", siteid);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }



        public DataSet GetAllSitedtata(string fdate, string tdate, string siteid, string selectall, string selectedrec, long Uid, long companyid, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetAllSitedtata";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@fdate", fdate);
                cmd.Parameters.AddWithValue("@tdate", tdate);
                cmd.Parameters.AddWithValue("@siteid", siteid);
                cmd.Parameters.AddWithValue("@selectall", selectall);
                cmd.Parameters.AddWithValue("@selectedrec", selectedrec);
                cmd.Parameters.AddWithValue("@Uid", Uid);
                cmd.Parameters.AddWithValue("@companyid", companyid);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }

        public DataSet GetDepttrackDataGrid(string frmdate, string todate, string deptid, long uid, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetDepttrackDataGrid";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@frmdate", frmdate);
                cmd.Parameters.AddWithValue("@todate", todate);
                cmd.Parameters.AddWithValue("@deptid", deptid);
                cmd.Parameters.AddWithValue("@uid", uid);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }
      

        public DataSet GetAllDeptData(string fdate, string tdate, string deptid, string selectall, string selectedrec, long Uid, long companyid, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetAllDeptData";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@fdate", fdate);
                cmd.Parameters.AddWithValue("@tdate", tdate);
                cmd.Parameters.AddWithValue("@deptid", deptid);
                cmd.Parameters.AddWithValue("@selectall", selectall);
                cmd.Parameters.AddWithValue("@selectedrec", selectedrec);
                cmd.Parameters.AddWithValue("@Uid", Uid);
                cmd.Parameters.AddWithValue("@companyid", companyid);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }
        public DataSet GetTechnicalDept(long uid, long companyid, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetTechnicalDept";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@companyid", companyid);
                cmd.Parameters.AddWithValue("@uid", uid);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }


        public string ISProjectSiteDetails(string companyid, string[] conn)
        {
            string result = "";
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_ISProjectSiteDetails";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@companyid", companyid);                
                da.SelectCommand = cmd;
                da.Fill(ds);
                if(ds.Tables[0].Rows.Count >0)
                {
                    result = "Yes";
                }
                else
                {
                    result = "No";
                }
            }

            return result;
        }

        #endregion

        public void UpdateOrderEditFlagucc(long Id, string[] conn)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                DataSet ds = new DataSet();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_UpdateOrderEditFlag";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@OrderID", Id);
                cmd.ExecuteNonQuery();
               // EmailSendToRequestor(Id, conn);
            }
        }


        #region CAF with delivery note report

      
        public DataSet GetAllDetaPrepaid(string FromDt, string ToDt, string hdnSelectedOrdcategory, string hdnSelectedOrdtype, string hdnSelectedfromtype, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetAllDetaPrepaid";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@FromDt", FromDt);
                cmd.Parameters.AddWithValue("@ToDt", ToDt);
                cmd.Parameters.AddWithValue("@hdnSelectedOrdcategory", hdnSelectedOrdcategory);
                cmd.Parameters.AddWithValue("@hdnSelectedOrdtype", hdnSelectedOrdtype);
                cmd.Parameters.AddWithValue("@hdnSelectedfromtype", hdnSelectedfromtype);                
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }

        public DataSet GetAllDetaPostpaid(string FromDt, string ToDt, string hdnSelectedOrdcategory, string hdnSelectedOrdtype, string hdnSelectedfromtype, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetAllDetaPostpaid";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@FromDt", FromDt);
                cmd.Parameters.AddWithValue("@ToDt", ToDt);
                cmd.Parameters.AddWithValue("@hdnSelectedOrdcategory", hdnSelectedOrdcategory);
                cmd.Parameters.AddWithValue("@hdnSelectedOrdtype", hdnSelectedOrdtype);
                cmd.Parameters.AddWithValue("@hdnSelectedfromtype", hdnSelectedfromtype);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }

        public DataSet GetAllDeliveryNote(string FromDt, string ToDt, string hdnSelectedOrdcategory, string hdnSelectedOrdtype, string hdnSelectedfromtype, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetDN";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@FromDt", FromDt);
                cmd.Parameters.AddWithValue("@ToDt", ToDt);
                cmd.Parameters.AddWithValue("@hdnSelectedOrdcategory", hdnSelectedOrdcategory);
                cmd.Parameters.AddWithValue("@hdnSelectedOrdtype", hdnSelectedOrdtype);
                cmd.Parameters.AddWithValue("@hdnSelectedfromtype", hdnSelectedfromtype);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }

        public DataSet GetOrdDetails(string FromDt, string ToDt, string hdnSelectedOrdcategory, string hdnSelectedOrdtype, string hdnSelectedfromtype, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetAllOrdercnt";
                cmd.Connection = svr.GetSqlConn(conn);               
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@FromDt", FromDt);
                cmd.Parameters.AddWithValue("@ToDt", ToDt);
                cmd.Parameters.AddWithValue("@hdnSelectedOrdcategory", hdnSelectedOrdcategory);
                cmd.Parameters.AddWithValue("@hdnSelectedOrdtype", hdnSelectedOrdtype);
                cmd.Parameters.AddWithValue("@hdnSelectedfromtype", hdnSelectedfromtype);
                da.SelectCommand = cmd;
                da.Fill(ds);
              
                return ds;
            }
        }
        #region VAT

        public DataSet AllVATOrderReports(string FrmDt, string Todt, string SelectedCompany, string SelectedDepartment, string SelectedUser, string SelectedStatus, string[] conn)
        {
           
            if (SelectedCompany == null || SelectedCompany == "0") SelectedCompany = "";
            if (SelectedDepartment == null || SelectedDepartment == "0") SelectedDepartment = "";
            if (SelectedUser == null || SelectedUser == "0") SelectedUser = "";
            if (SelectedStatus == null || SelectedStatus == "0") SelectedStatus = "";           

            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_getAllvatOrder";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@FromDt", FrmDt);
                cmd.Parameters.AddWithValue("@ToDt", Todt);
                cmd.Parameters.AddWithValue("@Company", SelectedCompany);
                cmd.Parameters.AddWithValue("@Department", SelectedDepartment);
                cmd.Parameters.AddWithValue("@status", SelectedStatus);
                cmd.Parameters.AddWithValue("@UserID", SelectedUser);
                da.SelectCommand = cmd;
                da.Fill(ds);

                return ds;
            }
        }
        public DataSet getVATOrderbyID(string SelectedOrder, string[] conn)
        {         

            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_getvatOrderbyID";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@ID", SelectedOrder);                
                da.SelectCommand = cmd;
                da.Fill(ds);

                return ds;
            }
        }
        #endregion

        #endregion

        #region Segment Type

        public void ADDorEditSegmentType(string ID, string segmenttype, string Active, long userid, long CompanyID, string state, string[] conn)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                DataSet ds = new DataSet();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_ADDorEditSegmentType";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@ID", ID);
                cmd.Parameters.AddWithValue("@segmenttype", segmenttype);
                cmd.Parameters.AddWithValue("@Active", Active);
                cmd.Parameters.AddWithValue("@userid", userid);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyID);
                cmd.Parameters.AddWithValue("@state", state);
                cmd.ExecuteNonQuery();
            }
        }

        public DataSet GetSegmentTypeList(string filter, string place, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetSegmentTypeList";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@filter", filter);
                cmd.Parameters.AddWithValue("@place", place);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }

        public string CHKDuplicateSegmentType(string ptype, string state, string[] conn)
        {
            string result = "";
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    DataSet ds = new DataSet();
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_CHKDuplicateSegmentType";
                    cmd.Connection = svr.GetSqlConn(conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@Stype", ptype);
                    cmd.Parameters.AddWithValue("@state", state);
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        result = "Yes";
                    }
                    else
                    {
                        result = "No";
                    }
                }
            }
            catch
            {
            }
            return result;
        }



        public DataSet GetSegmentTypeData(string ID, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetSegmentTypeData";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@ID", ID);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }

       

        public DataSet GetSegmentTypeListDDL(long ID, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetSegmentTypeListDDL";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@ID", ID);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }

        public DataSet getsegmentbyOrderID(long ID, string[] conn)
        {
            DataSet ds = new DataSet();
            string segmenttype = "";
            using (SqlCommand cmd = new SqlCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "getsegmenttype";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@orderID", ID);
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }
        }
        #endregion
    }
}
