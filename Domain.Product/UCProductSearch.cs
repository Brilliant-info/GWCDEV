using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Activation;
using System.Configuration;
using Interface.Product;
using System.ServiceModel;
using Domain.Server;

namespace Domain.Product
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public partial class UCProductSearch : Interface.Product.iUCProductSearch
    {

        Domain.Server.Server svr = new Server.Server();
        public List<GetProductDetail> GetProductList(string filter, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<GetProductDetail> result = new List<GetProductDetail>();
            if (filter == "")
            {
                result = (from lst in db.GetProductDetails
                          select lst).ToList();
            }
            else
            {
                List<GetProductDetail> filterList = new List<GetProductDetail>();
                filterList = db.GetProductDetails.Where(p => p.ProductCode.Contains(filter)).ToList();
                if (filterList.Count > 0) result.AddRange(filterList);

                filterList = new List<GetProductDetail>();
                filterList = db.GetProductDetails.Where(p => p.Name.Contains(filter)).ToList();
                if (filterList.Count > 0) result.AddRange(filterList);

                filterList = new List<GetProductDetail>();
                filterList = db.GetProductDetails.Where(p => p.Description.Contains(filter)).ToList();
                if (filterList.Count > 0) result.AddRange(filterList);

                filterList = new List<GetProductDetail>();
                filterList = db.GetProductDetails.Where(p => p.ProductCategory.Contains(filter)).ToList();
                if (filterList.Count > 0) result.AddRange(filterList);

                filterList = new List<GetProductDetail>();
                filterList = db.GetProductDetails.Where(p => p.ProductSubCategory.Contains(filter)).ToList();
                if (filterList.Count > 0) result.AddRange(filterList);

            }
            if (result.Count == 0)
            {
                result = null;
            }
            return result;
        }

        public List<GetProductDetail> GetProductList1(int pageIndex, string filter, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<GetProductDetail> result = new List<GetProductDetail>();
            pageIndex = pageIndex + 1;
            if (filter == "")
            {
                result = (from lst in db.GetProductDetails
                          select lst).Take(50 * pageIndex).ToList();
            }
            else
            {
                List<GetProductDetail> filterList = new List<GetProductDetail>();
                filterList = db.GetProductDetails.Where(p => p.ProductCode.Contains(filter)).ToList();
                if (filterList.Count > 0) result.AddRange(filterList);

                filterList = new List<GetProductDetail>();
                filterList = db.GetProductDetails.Where(p => p.Name.Contains(filter)).ToList();
                if (filterList.Count > 0) result.AddRange(filterList);

                filterList = new List<GetProductDetail>();
                filterList = db.GetProductDetails.Where(p => p.Description.Contains(filter)).ToList();
                if (filterList.Count > 0) result.AddRange(filterList);

                filterList = new List<GetProductDetail>();
                filterList = db.GetProductDetails.Where(p => p.ProductCategory.Contains(filter)).ToList();
                if (filterList.Count > 0) result.AddRange(filterList);

                filterList = new List<GetProductDetail>();
                filterList = db.GetProductDetails.Where(p => p.ProductSubCategory.Contains(filter)).ToList();
                if (filterList.Count > 0) result.AddRange(filterList);

                result = result.Take(50 * pageIndex).ToList();

            }
            if (result.Count == 0)
            {
                result = null;
            }
            return result;
        }

        public List<VW_GetSKUDetailsWithPack> GetSKUListDeptWise(int pageIndex, string filter, long UserID, long DeptID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<VW_GetSKUDetailsWithPack> result = new List<VW_GetSKUDetailsWithPack>();

            //var DeptID = (from d in db.mUserTerritoryDetails
            //               where d.UserID == UserID
            //               select d.TerritoryID).First();

            pageIndex = pageIndex + 1;
            if (filter == "")
            {
                result = (from lst in db.VW_GetSKUDetailsWithPack
                          where lst.StoreId == DeptID
                          select lst).Take(50 * pageIndex).ToList();
            }
            else
            {
                List<VW_GetSKUDetailsWithPack> filterList = new List<VW_GetSKUDetailsWithPack>();
                //filterList = db.VW_GetSKUDetailsWithPack.Where(p => (p.ProductCode.Contains(filter) || p.Name.Contains(filter) || p.Packkey.Contains(filter) || p.GroupSet.Contains(filter)) && p.StoreId == DeptID).ToList();
                //  if (filterList.Count > 0) result.AddRange(filterList);

                result = (from lst in db.VW_GetSKUDetailsWithPack
                          where (lst.ProductCode.Contains(filter) || lst.Name.Contains(filter) || lst.Packkey.Contains(filter) || lst.GroupSet.Contains(filter))
                          && lst.StoreId == DeptID
                          select lst).Take(50 * pageIndex).ToList();



                //filterList = new List<VW_GetSKUDetailsWithPack>();
                //filterList = db.VW_GetSKUDetailsWithPack.Where(p => p.Name.Contains(filter) && p.StoreId == DeptID).ToList();
                //if (filterList.Count > 0) result.AddRange(filterList);

                //filterList = new List<VW_GetSKUDetailsWithPack>();
                //filterList = db.VW_GetSKUDetailsWithPack.Where(p => p.Description.Contains(filter) && p.StoreId == DeptID).ToList();
                //if (filterList.Count > 0) result.AddRange(filterList);

                //filterList = new List<VW_GetSKUDetailsWithPack>();
                //filterList = db.VW_GetSKUDetailsWithPack.Where(p => p.Packkey.Contains(filter) && p.StoreId == DeptID).ToList();
                //if (filterList.Count > 0) result.AddRange(filterList);

                //filterList = new List<VW_GetSKUDetailsWithPack>();
                //filterList = db.VW_GetSKUDetailsWithPack.Where(p => p.GroupSet.Contains(filter) && p.StoreId == DeptID).ToList();
                //if (filterList.Count > 0) result.AddRange(filterList);


                //  result = result.Take(50 * pageIndex).ToList();
            }
            //if (result.Count == 0)
            //{
            //    result = null;                
            //}
            result = result.Where(a => a.Active == "Y").ToList();
            return result;
        }


        public DataSet GetSkuListDeptWiseNewe(int pageIndex, string filter, long UserID, long DeptID, string[] conn)
        {
            DataSet ds = new DataSet();
            pageIndex = pageIndex + 1;
            if (filter == "")
            {
                filter = "0";
            }
            using (SqlCommand cmd = new SqlCommand())
            {
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_GetSkuListDeptWiseNewe";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("DeptID", DeptID);
                    cmd.Parameters.AddWithValue("filter", filter);
                    cmd.Connection = svr.GetSqlConn(conn);
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    cmd.Connection.Close();
                }
            }
            return ds;
        }
        public List<VW_GetSKUDetailsWithPack> GetSKUList(int pageIndex, string filter, long DeptID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<VW_GetSKUDetailsWithPack> result = new List<VW_GetSKUDetailsWithPack>();
            pageIndex = pageIndex + 1;
            if (filter == "")
            {
                result = (from lst in db.VW_GetSKUDetailsWithPack
                          where lst.StoreId == DeptID
                          select lst).Take(50 * pageIndex).ToList();
            }
            else
            {
                List<VW_GetSKUDetailsWithPack> filterList = new List<VW_GetSKUDetailsWithPack>();
                filterList = db.VW_GetSKUDetailsWithPack.Where(p => p.ProductCode.Contains(filter) || p.Name.Contains(filter) || p.Description.Contains(filter) || p.Packkey.Contains(filter) || p.GroupSet.Contains(filter) && p.StoreId == DeptID).ToList();
                if (filterList.Count > 0) result.AddRange(filterList);

                //filterList = new List<VW_GetSKUDetailsWithPack>();                
                //filterList = db.VW_GetSKUDetailsWithPack.Where(p => p.Name.Contains(filter)).ToList();
                //if (filterList.Count > 0) result.AddRange(filterList);

                //filterList = new List<VW_GetSKUDetailsWithPack>();                
                //filterList = db.VW_GetSKUDetailsWithPack.Where(p => p.Description.Contains(filter)).ToList();
                //if (filterList.Count > 0) result.AddRange(filterList);

                //filterList = new List<VW_GetSKUDetailsWithPack>();
                //filterList = db.VW_GetSKUDetailsWithPack.Where(p => p.Packkey.Contains(filter)).ToList();
                //if (filterList.Count > 0) result.AddRange(filterList);

                //filterList = new List<VW_GetSKUDetailsWithPack>();
                //filterList = db.VW_GetSKUDetailsWithPack.Where(p => p.GroupSet.Contains(filter)).ToList();
                //if (filterList.Count > 0) result.AddRange(filterList);


                result = result.Take(50 * pageIndex).ToList();
            }
            if (result.Count == 0)
            {
                result = null;
            }
            return result;
        }

        public List<mProduct> SKUListForGrid(int CompanyID, int DeptID, int GroupSetID, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<mProduct> SKUlst = new List<mProduct>();

            SKUlst = (from lst in db.mProducts
                      where lst.CompanyID == CompanyID && lst.StoreId == DeptID && lst.BOMHeaderId == GroupSetID
                      select lst).ToList();
            return SKUlst;
        }

        #region Real Stock Count Suraj

        protected DataSet fillds(string strquery, string[] conn)
        {
            BISPL_CRMDBEntities ce = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            DataSet ds = new DataSet();
            System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection("Data Source=" + conn[0] + ";Initial Catalog=" + conn[1] + "; User ID=" + conn[3] + "; Password=" + conn[2] + ";");
            SqlDataAdapter da = new SqlDataAdapter(strquery, sqlConn);
            ds.Reset();
            da.Fill(ds);
            return ds;
        }

        public DataSet GetProdAvailableBal(long Pid, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            using (SqlDataAdapter da = new SqlDataAdapter())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetProdAvailableBal";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@SKUId", Pid);
                da.SelectCommand = cmd;
                da.Fill(ds);
                cmd.Connection.Close();
            }
            return ds;
        }

        public void Createtransaction(string SKUCode, string DepartmentCode, decimal gtotal, string[] conn)
        {
            decimal gqty = 0, ResurveQty = 0, AvailableBalance = 0, ClosingBalance = 0;
            string SkuId = "", maxSkuId = "";
            long DeptCode = long.Parse(DepartmentCode.ToString());
            DataSet ds = new DataSet();
            ds.Reset();
            using (SqlCommand cmd = new SqlCommand())
            using (SqlDataAdapter da = new SqlDataAdapter())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SP_GetProdtIdReal";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@SKUCode", SKUCode);
                cmd.Parameters.AddWithValue("@DeptCode", DeptCode);
                da.SelectCommand = cmd;
                da.Fill(ds);
                cmd.Connection.Close();
            }
            if (ds.Tables[0].Rows.Count > 0)
            {
                SkuId = Convert.ToString(ds.Tables[0].Rows[0]["Id"]);
            }
            ds.Dispose();
            long sid = Convert.ToInt64(SkuId);
            DataSet ds1 = new DataSet();
            ds1.Reset();
            using (SqlCommand cmd1 = new SqlCommand())
            using (SqlDataAdapter da1 = new SqlDataAdapter())
            {
                cmd1.CommandType = CommandType.StoredProcedure;
                cmd1.CommandText = "SP_GetProdtstockReal";
                cmd1.Connection = svr.GetSqlConn(conn);
                cmd1.Parameters.Clear();
                cmd1.Parameters.AddWithValue("@sid", sid);
                cmd1.Parameters.AddWithValue("@DeptCode", DeptCode);
                da1.SelectCommand = cmd1;
                da1.Fill(ds1);
                cmd1.Connection.Close();
            }
            if (ds1.Tables[0].Rows.Count > 0)
            {
                gqty = Convert.ToDecimal(ds1.Tables[0].Rows[0]["SumofAvailbleAndResurveqty"].ToString());
                ResurveQty = Convert.ToDecimal(ds1.Tables[0].Rows[0]["ResurveQty"].ToString());
                AvailableBalance = Convert.ToDecimal(ds1.Tables[0].Rows[0]["AvailableBalance"].ToString());
            }
            ds1.Dispose();
            if (gqty == gtotal)
            { }
            else
            {
                decimal qtydiff = gtotal - gqty;
                if (qtydiff > 0)
                {
                    ClosingBalance = qtydiff - ResurveQty;
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "SP_InsertIntotInventry";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@SKUId", SkuId);
                        cmd.Parameters.AddWithValue("@StoreId", DeptCode);
                        cmd.Parameters.AddWithValue("@Quantity", qtydiff);
                        cmd.Parameters.AddWithValue("@ResurveQty", ResurveQty);
                        cmd.Parameters.AddWithValue("@ClosingBalance", gtotal);
                        cmd.Parameters.AddWithValue("@Flag", "QuentityReceive");
                        cmd.Connection = svr.GetSqlConn(conn);
                        cmd.ExecuteNonQuery();
                        cmd.Connection.Close();
                    }
                }
                else
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "SP_InsertIntotInventry";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@SKUId", SkuId);
                        cmd.Parameters.AddWithValue("@StoreId", DeptCode);
                        cmd.Parameters.AddWithValue("@Quantity", Math.Abs(qtydiff));
                        cmd.Parameters.AddWithValue("@ResurveQty", ResurveQty);
                        cmd.Parameters.AddWithValue("@ClosingBalance", gtotal);
                        cmd.Parameters.AddWithValue("@Flag", "QuentityDispatch");
                        cmd.Connection = svr.GetSqlConn(conn);
                        cmd.ExecuteNonQuery();
                        cmd.Connection.Close();
                    }
                }

                decimal CurrentBal = 0;
                decimal availbal = gtotal - Convert.ToDecimal(ResurveQty);
                if (availbal > 0)
                {
                    CurrentBal = availbal;
                }
                else
                {
                    CurrentBal = 0;
                }
                //update tProductStockDetails for AvailableBalance                
                using (SqlCommand cmdtInventry = new SqlCommand())
                {
                    cmdtInventry.CommandType = CommandType.StoredProcedure;
                    cmdtInventry.CommandText = "SP_InsertIntotInventry";
                    cmdtInventry.Parameters.Clear();
                    cmdtInventry.Parameters.AddWithValue("@AvailableBalance", CurrentBal);
                    cmdtInventry.Parameters.AddWithValue("@StoreId", DeptCode);
                    cmdtInventry.Parameters.AddWithValue("@SKUId", SkuId);
                    cmdtInventry.Parameters.AddWithValue("@Flag", "updatetProductStockDetails");
                    cmdtInventry.Connection = svr.GetSqlConn(conn);
                    cmdtInventry.ExecuteNonQuery();
                    cmdtInventry.Connection.Close();
                }
            }
        }


        public DataSet GetSkuListDeptWiseToUpdate(int pageIndex, string filter, long UserID, long DeptID, string[] conn)
        {
            DataSet ds = new DataSet();
            pageIndex = pageIndex + 1;
            if (filter == "")
            {
                filter = "0";
            }
            using (SqlCommand cmd = new SqlCommand())
            {
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_GetSkuListDeptWiseToUpdate";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@DeptID", DeptID);
                    cmd.Parameters.AddWithValue("@filter", filter);
                    cmd.Connection = svr.GetSqlConn(conn);
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    cmd.Connection.Close();
                }
            }
            return ds;
        }

        #endregion
        #region Serial Number


        public List<GetProductSerialDetail> GetProductSerialDetailNEW(long skuid, string[] conn)
        {
            BISPL_CRMDBEntities db = new BISPL_CRMDBEntities(svr.GetEntityConnection(conn));
            List<GetProductSerialDetail> SKUlst = new List<GetProductSerialDetail>();

            SKUlst = (from lst in db.GetProductSerialDetails                      
                      select lst).ToList();
            return SKUlst;
        }



        public DataSet GetProductSerialDetails(long Oid, long Sid, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_GetProductSerialDetails";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@OrdId", Oid);
                    cmd.Parameters.AddWithValue("@Skudid", Sid);
                    cmd.Connection = svr.GetSqlConn(conn);
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    cmd.Connection.Close();
                }
            }
            return ds;
        }

        public DataSet GetSkuDeptSchemaData( long Sid, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_GetSkuDeptSchemaData";
                    cmd.Parameters.Clear();                   
                    cmd.Parameters.AddWithValue("@Skudid", Sid);
                    cmd.Connection = svr.GetSqlConn(conn);
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    cmd.Connection.Close();
                }
            }
            return ds;
        }
        public DataSet GetSkuDataWithSerial(long Sid, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            {
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_GetSkuDataWithSerial";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@Skudid", Sid);
                    cmd.Connection = svr.GetSqlConn(conn);
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    cmd.Connection.Close();
                }
            }
            return ds;
        }

        #endregion


        #region UpdateStock
        public DataSet getProductdetailsUpdate(long cntrecord,string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            using (SqlDataAdapter da = new SqlDataAdapter())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "getProductdetailsUpdate";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@cntrecourd", cntrecord);
                da.SelectCommand = cmd;
                da.Fill(ds);
                cmd.Connection.Close();
            }
            return ds;
        }
        public DataSet updateIsreadFlag(string productcode,string storecode, string[] conn)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand())
            using (SqlDataAdapter da = new SqlDataAdapter())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "updateIsreadFlag";
                cmd.Connection = svr.GetSqlConn(conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@Productcode", productcode);
                cmd.Parameters.AddWithValue("@Storecode", storecode);
                da.SelectCommand = cmd;
                da.Fill(ds);
                cmd.Connection.Close();
            }
            return ds;
        }
        #endregion
        public long insertlog(string skuCode,long deptID,string transactiontype,decimal AVLqty,decimal Resqty,decimal realtimestkqty, string[] conn)
        {
            long result = 0;
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_inventrylog";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@skucode", skuCode);
                cmd.Parameters.AddWithValue("@storeid", deptID);
                cmd.Parameters.AddWithValue("@transactiontype", transactiontype);
                cmd.Parameters.AddWithValue("@OMSAVLQty", AVLqty);
                cmd.Parameters.AddWithValue("@OMSresqty", Resqty);
                cmd.Parameters.AddWithValue("@WMSRealtimeQty", realtimestkqty);                
                cmd.Connection = svr.GetSqlConn(conn);
                result = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }
            return result;
        }
        public long insertlogNew(string skuCode, long deptID, string transactiontype, decimal AVLqty, decimal Resqty, decimal realtimestkqty, string[] conn)
        {
            long result = 0;
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_inventrylogNew";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@Prodcode", skuCode);
                cmd.Parameters.AddWithValue("@storeid", deptID);
                cmd.Parameters.AddWithValue("@transactiontype", transactiontype);
                cmd.Parameters.AddWithValue("@OMSAVLQty", AVLqty);
                cmd.Parameters.AddWithValue("@OMSresqty", Resqty);
                cmd.Parameters.AddWithValue("@WMSRealtimeQty", realtimestkqty);
                cmd.Connection = svr.GetSqlConn(conn);
                result = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }
            return result;
        }
    }
}
