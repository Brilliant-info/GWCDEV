using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.ServiceModel.Activation;
using System.Configuration;
using System.ServiceModel;
using System.Data.Entity;
using System.Runtime.Serialization;


namespace Interface.UserManagement
{
    [ServiceContract]
    public partial interface iBindMenu
    {
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<ProBindMenu> UserCode(ProBindMenu objParaProBindMenu);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<ProBindMenu> CheckOperaction(ProBindMenu objParaProBindMenu);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<ProBindMenu> BindUserMenu(ProBindMenu objParaProBindMenu);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        ProBindMenu ActiveButtonClickEvent(ProBindMenu objProBindMenu);

    }


    public partial class ProBindMenu
    {
        long _intUserCode;
        String _strMenuCode;
        long _intCompanyCode;
        Boolean _bolBtnAdd;
        Boolean _bolBtnEdit;
        Boolean _bolBtnView;
        Boolean _bolBtnDelete;
        Boolean _bolBtnSave;
        Boolean _bolBtnClear;
        Boolean _bolBtnExport;
        Boolean _bolBtnImport;
        Boolean _bolBtnMail;
        Boolean _bolBtnPrint;
        Boolean _bolBtnApproval;
        Boolean _bolBtnAssignTask;
        String _strObjectCode;
        Boolean _bolBtnConvertTo;
        Object _objActiveButton;
        String _strDivName;



        public string[] _constP = new string[3];
        public long UserCode { get { return _intUserCode; } set { _intUserCode = value; } }
        public String MenuCode { get { return _strMenuCode; } set { _strMenuCode = value; } }
        public String ObjectCode { get { return _strObjectCode; } set { _strObjectCode = value; } }
        public string this[int index] { get { return _constP[index]; } set { _constP[index] = value; } }
        public long CompanyCode { get { return _intCompanyCode; } set { _intCompanyCode = value; } }
        public String DivName { get { return _strDivName; } set { _strDivName = value; } }
        public Boolean BtnAdd { get { return _bolBtnAdd; } set { _bolBtnAdd = value; } }
        public Boolean BtnEdit { get { return _bolBtnEdit; } set { _bolBtnEdit = value; } }
        public Boolean BtnView { get { return _bolBtnView; } set { _bolBtnView = value; } }
        public Boolean BtnDelete { get { return _bolBtnDelete; } set { _bolBtnDelete = value; } }
        public Boolean BtnSave { get { return _bolBtnSave; } set { _bolBtnSave = value; } }
        public Boolean BtnClear { get { return _bolBtnClear; } set { _bolBtnClear = value; } }
        public Boolean BtnExport { get { return _bolBtnExport; } set { _bolBtnExport = value; } }
        public Boolean BtnImport { get { return _bolBtnImport; } set { _bolBtnImport = value; } }
        public Boolean BtnMail { get { return _bolBtnMail; } set { _bolBtnMail = value; } }
        public Boolean BtnPrint { get { return _bolBtnPrint; } set { _bolBtnPrint = value; } }
        public Boolean BtnApproval { get { return _bolBtnApproval; } set { _bolBtnApproval = value; } }
        public Boolean BtnConvertTo { get { return _bolBtnConvertTo; } set { _bolBtnConvertTo = value; } }
        public Boolean BtnAssignTask { get { return _bolBtnAssignTask; } set { _bolBtnAssignTask = value; } }

        public Object ActiveButton { get { return _objActiveButton; } set { _objActiveButton = value; } }

    }
}
