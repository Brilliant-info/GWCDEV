using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Activation;
using System.Configuration;
using Interface.UserManagement;
using System.ServiceModel;
using System.Xml.Linq;
using Domain.Server;
using Interface.UserManagement;

namespace Domain.UserManagement
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public partial class BindMenu : Interface.UserManagement.iBindMenu
    {
        Domain.Server.Server svr = new Server.Server();

        public List<ProBindMenu> UserCode(ProBindMenu objParaProBindMenu)
        {
            List<ProBindMenu> objProBindMenu = new List<ProBindMenu> { };

            return objProBindMenu;
        }

        public ProBindMenu ActiveButtonClickEvent(ProBindMenu objProBindMenu)
        {


            objProBindMenu.BtnExport = objProBindMenu.BtnImport = objProBindMenu.BtnMail = false;
            switch (objProBindMenu.ActiveButton.ToString())
            {

                case "PageLoad":
                    if (objProBindMenu.BtnAdd == true)
                    {
                        objProBindMenu.BtnClear = objProBindMenu.BtnSave = objProBindMenu.BtnEdit = objProBindMenu.BtnApproval = objProBindMenu.BtnPrint = objProBindMenu.BtnAssignTask = objProBindMenu.BtnDelete = objProBindMenu.BtnView = false;
                        objProBindMenu.BtnConvertTo = objProBindMenu.BtnAdd = true;
                    }
                    break;
                case "btnAddNew":
                    if (objProBindMenu.BtnAdd == true)
                    {
                        objProBindMenu.BtnPrint = objProBindMenu.BtnAdd = objProBindMenu.BtnEdit = objProBindMenu.BtnApproval = objProBindMenu.BtnAssignTask = objProBindMenu.BtnDelete = objProBindMenu.BtnView = false;
                        objProBindMenu.BtnClear = objProBindMenu.BtnSave = true;
                    }
                    break;

                case "btnEdit":
                    if (objProBindMenu.BtnEdit == true)
                    {
                        objProBindMenu.BtnAdd = objProBindMenu.BtnApproval = objProBindMenu.BtnAssignTask = objProBindMenu.BtnDelete = objProBindMenu.BtnEdit = objProBindMenu.BtnView = false;
                        objProBindMenu.BtnClear = objProBindMenu.BtnSave = objProBindMenu.BtnPrint = true;
                    }
                    break;

                case "btnSave":
                    if (objProBindMenu.BtnSave == true)
                    {
                        objProBindMenu.BtnPrint = objProBindMenu.BtnAdd = objProBindMenu.BtnEdit = objProBindMenu.BtnApproval = objProBindMenu.BtnAssignTask = objProBindMenu.BtnDelete = objProBindMenu.BtnView = objProBindMenu.BtnClear = objProBindMenu.BtnSave = true;
                    }
                    break;

                case "btnSaveEdit":
                    if (objProBindMenu.BtnSave == true)
                    {
                        objProBindMenu.BtnPrint = objProBindMenu.BtnAdd = objProBindMenu.BtnEdit = objProBindMenu.BtnApproval = objProBindMenu.BtnAssignTask = objProBindMenu.BtnDelete = objProBindMenu.BtnView = objProBindMenu.BtnClear = objProBindMenu.BtnSave = true;
                    }
                    break;

                case "btnClear":
                    if (objProBindMenu.BtnClear == true)
                    {
                        List<ProBindMenu> objListProBindMenu = new List<ProBindMenu> { };
                        objListProBindMenu = CheckOperaction(objProBindMenu);
                        objProBindMenu = objListProBindMenu[0];
                    }
                    break;
                case "btnExport":
                    if (objProBindMenu.BtnExport == true)
                    {

                    }
                    break;
                case "btnImport":
                    if (objProBindMenu.BtnImport == true)
                    {

                    }
                    break;
                case "btnMail":
                    if (objProBindMenu.BtnMail == true)
                    {

                    }
                    break;
                case "SelectRecord":
                    objProBindMenu.BtnApproval = objProBindMenu.BtnAssignTask = objProBindMenu.BtnDelete = objProBindMenu.BtnClear = objProBindMenu.BtnSave = false;
                    objProBindMenu.BtnPrint = objProBindMenu.BtnAdd = objProBindMenu.BtnEdit = objProBindMenu.BtnView = objProBindMenu.BtnPrint = true;
                    break;
                case "btnPrint":
                    if (objProBindMenu.BtnPrint == true)
                    {

                    }
                    break;
                case "btnConvertTo":
                    if (objProBindMenu.BtnAdd == true)
                    {

                    }
                    break;

                default:
                    objProBindMenu.BtnEdit = objProBindMenu.BtnClear = objProBindMenu.BtnSave = false;
                    break;

            }
            //objProBindMenu.BtnConvertTo = objProBindMenu.BtnAdd = objProBindMenu.BtnClear = objProBindMenu.BtnSave = objProBindMenu.BtnEdit = objProBindMenu.BtnApproval = objProBindMenu.BtnPrint = objProBindMenu.BtnAssignTask = objProBindMenu.BtnDelete = objProBindMenu.BtnView = true;
            return objProBindMenu;
        }

        public List<ProBindMenu> CheckOperaction(ProBindMenu objParaProBindMenu)
        {
            List<ProBindMenu> objProBindMenu = new List<ProBindMenu> { };
            SqlConnection con = new SqlConnection("Data Source=" + objParaProBindMenu._constP[0].ToString() + ";Initial Catalog=" + objParaProBindMenu._constP[1].ToString() + ";Persist Security Info=True;User ID=" + objParaProBindMenu._constP[3].ToString() + ";Password=" + objParaProBindMenu._constP[2].ToString() + ";");
            con.Open();
            SqlCommand cmd = new SqlCommand();
            SqlDataReader rd;
            cmd.CommandText = "select case when [Add]=1 then 'true' else 'false' end as [Add],case when [Edit]=1 then 'true' else 'false' end as [Edit],case when [View]=1 then 'true' else 'false' end as [View],case when [Delete]=1 then 'true' else 'false' end  as [Delete],case when [Approval] =1 then 'true' else 'false' end as [Approval] ,case when [AssignTask]=1 then 'true' else 'false' end as [AssignTask] ,ObjectName  from mUserRolesDetail where UserID='" + objParaProBindMenu.UserCode + "' and CompanyID = '" + objParaProBindMenu.CompanyCode + "' and ObjectName='" + objParaProBindMenu.ObjectCode + "'";
            cmd.Connection = con;
            rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                objParaProBindMenu.BtnAdd = Convert.ToBoolean(rd["Add"].ToString());
                objParaProBindMenu.BtnEdit = Convert.ToBoolean(rd["Edit"].ToString());
                objParaProBindMenu.BtnView = Convert.ToBoolean(rd["View"].ToString());
                objParaProBindMenu.BtnDelete = Convert.ToBoolean(rd["Delete"].ToString());
                objParaProBindMenu.BtnSave = Convert.ToBoolean(rd[0].ToString());
                objParaProBindMenu.BtnClear = true;
                if (Convert.ToBoolean(rd["Add"].ToString()) == true || Convert.ToBoolean(rd["Edit"].ToString()) == true || Convert.ToBoolean(rd["View"].ToString()) == true)
                {
                    objParaProBindMenu.BtnPrint = objParaProBindMenu.BtnExport = true;
                }
                else
                {
                    objParaProBindMenu.BtnPrint = objParaProBindMenu.BtnExport = false;
                }
                objParaProBindMenu.BtnImport = Convert.ToBoolean(rd["Add"].ToString());
                objParaProBindMenu.BtnMail = Convert.ToBoolean(rd["Add"].ToString());
                objParaProBindMenu.BtnApproval = Convert.ToBoolean(rd["Approval"].ToString());
                objParaProBindMenu.BtnAssignTask = Convert.ToBoolean(rd["AssignTask"].ToString());
                objProBindMenu.Add(objParaProBindMenu);
            }
            return objProBindMenu;
        }

        public List<ProBindMenu> BindUserMenu(ProBindMenu objParaProBindMenu)
        {
            List<ProBindMenu> objProBindMenu = new List<ProBindMenu> { };
            ProBindMenu objProBindMenuM = new ProBindMenu();
            StringBuilder strMenuString = new StringBuilder();
            string strFinalQuery = "";
            SqlConnection con = new SqlConnection("Data Source=" + objParaProBindMenu._constP[0].ToString() + ";Initial Catalog=" + objParaProBindMenu._constP[1].ToString() + ";Persist Security Info=True;User ID=" + objParaProBindMenu._constP[3].ToString() + ";Password=" + objParaProBindMenu._constP[2].ToString() + ";");
            con.Open();
            SqlCommand cmd = new SqlCommand();
            SqlDataReader rd;
            SqlDataReader rd2;
            cmd.CommandText = "Declare @Query varchar(max); Set  @Query ='Select  '; select @Query =@Query+ ' ( select FormUrl  from sysmObjects where HeaderMenu=''YS''  and  Sequence ='+CAST ( Sequence as varchar(10))+' )  + ' from sysmObjects left outer join mUserRolesDetail  on mUserRolesDetail.ObjectName=sysmObjects.ObjectName where   mUserRolesDetail.UserID=" + objParaProBindMenu.UserCode + " and mUserRolesDetail.CompanyID=" + objParaProBindMenu.CompanyCode + "  and (mUserRolesDetail.[Add]='True' or mUserRolesDetail.Edit ='True' or mUserRolesDetail.[View] ='True' or mUserRolesDetail.[Delete]='True' or mUserRolesDetail.Approval='True' or mUserRolesDetail.AssignTask='True' )   and HeaderMenu='YS'  order by Sequence Set  @Query =@Query+'+';  Select  replace ( @Query,'+ +','') ";
            cmd.Connection = con;
            rd = cmd.ExecuteReader();
            strMenuString.Append("<ul id='css3menu1' class='topmenu1'>");
            while (rd.Read())
            {
                if (rd[0].ToString() == "Select  +")
                {
                    goto brk;
                }
                strFinalQuery = rd[0].ToString();
                cmd.CommandText = strFinalQuery;
                rd.Close();

                rd2 = cmd.ExecuteReader();

                while (rd2.Read())
                {

                    strMenuString.Append("" + rd2[0].ToString() + "");
                    goto brk;
                }

            }
        brk:
            con.Close();
            strMenuString.Append("</ul>");
            objProBindMenuM.MenuCode = strMenuString.ToString();
            objProBindMenu.Add(objProBindMenuM);
            return objProBindMenu;
        }
               
    }


}
