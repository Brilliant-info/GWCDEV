using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Data.Entity;

using System.Data;


namespace Interface.Inventory
{
    [ServiceContract]
    public partial interface iEngineMaster
    {
        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        List<v_GetEngineDetails> GetEngineMasterList(string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int InsertmEngine(mEngine SiteMaster, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        v_GetEngineDetails GetmEngineListByID(int EngineId, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        int updatemEngine(mEngine ObjmEngine, string[] conn);

        [OperationContract, XmlSerializerFormat(Style = OperationFormatStyle.Document)]
        DataSet GetEngineOfSite(string SId, string[] conn);

       


    }
}
