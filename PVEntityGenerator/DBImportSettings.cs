using System;
using System.Data;
using System.Data.OleDb;
using System.Collections;
using PVEntityGenerator.XMLSchema;
using PVEntityGenerator.Util;

namespace PVEntityGenerator {

  public class DBImportSettings {

    private bool mfSysTableExport=false;
    private bool mfSysTablePVEntityGenerator=false;
    private string mMDBFilename=null;
    private projectsettings mProjectSettings=null;
    private OleDbConnection mcon=null;
    private ArrayList malImportParms=null;
    private string mPlatform="";
    private string mDBPlatform="";
    private dbdefinition mDBDefinition;

    public DBImportSettings(bool pfSysTableExport, bool pfSysTablePVEntityGenerator,
      string pMDBFilename,projectsettings pProjectSettings,dbdefinition pDBDefinition,
      OleDbConnection pcon) {

      mfSysTableExport=pfSysTableExport;
      mfSysTablePVEntityGenerator=pfSysTablePVEntityGenerator;
      mMDBFilename=pMDBFilename;
      mProjectSettings=pProjectSettings;
      mDBDefinition=pDBDefinition;
      mcon=pcon;
    }

    public void ImportTable_Export() {
      string strSQL="SELECT Tabelle,Reihenfolge,Export,ExportData,DropOnly "
        + "FROM _Export ORDER BY Reihenfolge";

      OleDbCommand cmd = mcon.CreateCommand();
      cmd.CommandText = strSQL;
      OleDbDataReader dr = null;

      try {
        dr = cmd.ExecuteReader();

        if (mProjectSettings.entityexport==null)
          mProjectSettings.entityexport=new projectsettingsEntityexport();

        ArrayList alEntityExport=new ArrayList();
        while (dr.Read()) {
          string strTable=dr.GetString(dr.GetOrdinal("Tabelle"));

          //ignore System-Tables
          if (strTable.ToLower().Equals("_export") || strTable.ToLower().Equals("_pventitygenerator")) continue;

          projectsettingsEntityexportExportentity e=new projectsettingsEntityexportExportentity();
          e.entity=strTable;

          object val=dr.GetValue(dr.GetOrdinal("Reihenfolge"));
          if (!val.Equals(DBNull.Value))
            e.sortno=val.ToString();

          e.exportstructure=false;
          val=dr.GetValue(dr.GetOrdinal("Export"));
          if (!val.Equals(DBNull.Value))
            e.exportstructure=(bool)val;

          e.exportdata=false;
          val=dr.GetValue(dr.GetOrdinal("ExportData"));
          if (!val.Equals(DBNull.Value))
            e.exportdata=(bool)val;

          e.exportdrop=false;
          val=dr.GetValue(dr.GetOrdinal("DropOnly"));
          if (!val.Equals(DBNull.Value))
            e.exportdrop=(bool)val;

          alEntityExport.Add(e);
        }
        dr.Close();

        if (alEntityExport.Count>0)
          mProjectSettings.entityexport.exportentities=(projectsettingsEntityexportExportentity[])
            alEntityExport.ToArray(typeof(projectsettingsEntityexportExportentity));
      } finally {
        if (dr!=null) dr.Close();
      }
    }

    public void FetchOldParameters() {
      string strSQL="SELECT Entity,Param,Value FROM _PVEntityGenerator "
        + "WHERE NOT Entity IS NULL AND NOT Param IS NULL AND NOT Value IS NULL "
        + "ORDER BY Entity,Param";

      OleDbCommand cmd = mcon.CreateCommand();
      cmd.CommandText = strSQL;
      OleDbDataReader dr = null;

      try {
        malImportParms=new ArrayList();

        //Read all old settings (parameters)
        dr = cmd.ExecuteReader();
        while (dr.Read()) {
          string strParam=(string)dr.GetValue(dr.GetOrdinal("Param"));
          string strEntity=dr.GetString(dr.GetOrdinal("Entity"));
          string strVal=(string) dr.GetValue(dr.GetOrdinal("Value"));

          if (strParam.Length<=0 || strEntity.Length<=0 || strVal.Length<=0) continue;

          malImportParms.Add(new OldParameterInfo(strEntity,strParam,strVal));

          if (strEntity.ToLower().Equals("_pventitygenerator")) {
            if (strParam.ToLower().Equals("platform"))
              mPlatform=strVal;

            if (strParam.ToLower().Equals("dbplatforms"))
              mDBPlatform=strVal;
          }
        }
        dr.Close();
      } finally {
        if (dr!=null) dr.Close();
      }
    }

    public string Platform {
      get { return mPlatform; }
    }

    public string DBPlatform {
      get { return mDBPlatform; }
    }

    private bool SetPlatformParameter(projectsettingsPlatformsPlatform pPlatform,
      string pParam, string pValue) {
      foreach (parametersParameter p in pPlatform.parameters) {
        if (p.name.Equals(pParam)) {
          p.Value=pValue;
          return true;
        }
      }
      return false;
    }

    private bool SetDBPlatformParameter(projectsettingsDbplatformsDbplatform pDbPlatform,
      string pParam, string pValue) {
      foreach (parametersParameter p in pDbPlatform.parameters) {
        if (p.name.Equals(pParam)) {
          p.Value=pValue;
          return true;
        }
      }
      return false;
    }

    private void ProcessGlobalParameter(OldParameterInfo pParam, projectsettingsPlatformsPlatform pPlatform,
      projectsettingsDbplatformsDbplatform pDBPlatform) {
      switch (pParam.Param) {
          //Platform parameters
        case ("OutputDir"):
          SetPlatformParameter(pPlatform,"path-entity",pParam.Value);
          break;

        case ("Package"):
          if (pPlatform.name.Equals("NET"))
            SetPlatformParameter(pPlatform,"entity-namespace",pParam.Value);
          else
            SetPlatformParameter(pPlatform,"entity-package",pParam.Value);
          break;

        case ("ClassHeader"):
          SetPlatformParameter(pPlatform,"file-header",pParam.Value);
          break;

        case ("GenerateJavaDoc"):
          SetPlatformParameter(pPlatform,"generate-comments",pParam.Value);
          break;

        case ("JUnitGenerateClasses"):
          SetPlatformParameter(pPlatform,"generate-unittest-suite",pParam.Value);
          break;

        case ("JUnitGenerateAllTests"):
          if (pPlatform.name.Equals("Java2"))
            SetPlatformParameter(pPlatform,"generate-unittest",pParam.Value);
          break;

        case ("JUnitOutputDir"):
          SetPlatformParameter(pPlatform,"path-unittest",pParam.Value);
          break;

        case ("JUnitBaseClass"):
          SetPlatformParameter(pPlatform,"unittest-baseclass",pParam.Value);
          break;

        case ("AttributeGetterSetter"):
          if (pPlatform.name.Equals("NET"))
            SetPlatformParameter(pPlatform,"getsetmethods",pParam.Value);
          break;

          //DBPlatform parameters
        case ("GenerateStoredProc"):
          //ignore (for now)
          break;

        case ("Unicode"):
          SetDBPlatformParameter(pDBPlatform,"unicode",pParam.Value);
          break;

        case ("StoredProcOutputDir"):
          SetDBPlatformParameter(pDBPlatform,"script-path",pParam.Value);
          break;

        case ("StoredProcGrantTo"):
          SetDBPlatformParameter(pDBPlatform,"grant-to",pParam.Value);
          break;

        case ("CheckEntityNameLength"):
          if (pDBPlatform.name.Equals("Oracle"))
            SetDBPlatformParameter(pDBPlatform,"name-max-length",pParam.Value);
          break;
      }
    }

    private type_generationGenerateentity FindEntityGenerate(string pEntity) {
      foreach (type_generationGenerateentity e in mProjectSettings.entitygeneration.generateentity) {
        if (e.entity.Equals(pEntity))
          return e;
      }

      return null;
    }

    private string GetEntityGenerationOldParamValue(string pEntity,string pParam) {
      foreach (OldParameterInfo param in malImportParms) {
        if (param.Entity.Equals(pEntity) && param.Param.Equals(pParam))
          return param.Value;
      }
      return "";
    }

    private string GetEntityGenerationOldParamValueBool(string pEntity,string pParam, string pDefault) {
      string strVal=GetEntityGenerationOldParamValue(pEntity,pParam);
      if (strVal==null || strVal.Length<=0)
        return pDefault;

      if (strVal.Equals("1")) return "true";

      return "false";
    }

    private void UpdateEntityGenerationParameters (type_generationGenerateentity pEntity,
      ref bool pfGenEnum) {

      foreach (parametersParameter p in pEntity.parameters) {
        switch (p.name) {
          case ("generate-entity"):
            p.Value=GetEntityGenerationOldParamValueBool(pEntity.entity,"GenClass","true");
            break;

          case ("generate-getter"):
            p.Value=GetEntityGenerationOldParamValueBool(pEntity.entity,"GenClass_get","true");
            break;

          case ("generate-setter"):
            p.Value=GetEntityGenerationOldParamValueBool(pEntity.entity,"GenClass_set","true");
            break;

          case ("generate-removecheck"):
            p.Value=GetEntityGenerationOldParamValueBool(pEntity.entity,"GenClass_RemoveDRI","true");
            break;

          case ("generate-home"):
            p.Value=GetEntityGenerationOldParamValueBool(pEntity.entity,"GenHome","true");
            break;

          case ("generate-default-create"):
            p.Value=GetEntityGenerationOldParamValueBool(pEntity.entity,"GenHome_create","true");
            break;

          case ("generate-default-create-object"):
            p.Value=GetEntityGenerationOldParamValueBool(pEntity.entity,"GenHome_createDbObject","true");
            break;

          case ("generate-findbyprimarykey"):
            p.Value=GetEntityGenerationOldParamValueBool(pEntity.entity,"GenHome_findByPrimaryKey","true");
            break;

          case ("generate-enumeration"):
            p.Value=GetEntityGenerationOldParamValueBool(pEntity.entity,"GenEnumeration","false");
            pfGenEnum=p.Value.Equals("true");
            break;

        }
      }
    }

    private void ProcessEntityParameters(projectsettingsPlatformsPlatform pPlatform,
      projectsettingsDbplatformsDbplatform pDBPlatform) {
      if (malImportParms==null || malImportParms.Count<=0) return;

      foreach (type_generationGenerateentity e in mProjectSettings.entitygeneration.generateentity) {
        bool fGenEnum=false;
        UpdateEntityGenerationParameters(e,ref fGenEnum);

        if (fGenEnum) {
          //enum generation on

          //load enum defs from mdb
          bool fDefChanged=false;
          GridHelper.GetEnumeration(e, mMDBFilename, ref fDefChanged);

          //update enum defs with the ones found in the old _PVEntityGenerator table
          foreach (type_generationGenerateentityEnumerationentry enumAttr in e.enumerationentries) {
            enumAttr.description=GetEntityGenerationOldParamValue
              (e.entity,"Enumeration_Comment_" + enumAttr.id);

            enumAttr.generate=GetEntityGenerationOldParamValue
              (e.entity,"Enumeration_Generate_" + enumAttr.id).Equals("1");

            enumAttr.identifier=GetEntityGenerationOldParamValue
              (e.entity,"Enumeration_Constant_" + enumAttr.id);

            enumAttr.name=GetEntityGenerationOldParamValue
              (e.entity,"Enumeration_Name_" + enumAttr.id);
          }
        } else {
          //enum generation off
          e.enumerationentries=null;
        }

        //get count of custom create/findby methods
        int intCustomFindBy=0;
        try {intCustomFindBy=int.Parse(GetEntityGenerationOldParamValue
               (e.entity,"findCustom_Count"));} catch {}
        int intCustomCreate=0;
        try {intCustomCreate=int.Parse(GetEntityGenerationOldParamValue
               (e.entity,"createCustom_Count"));} catch {}

        //Custom create methods
        if (intCustomCreate>0) {
          ArrayList alMethods=new ArrayList();

          for (int i=0;i<intCustomCreate;i++) {
            string strVal=GetEntityGenerationOldParamValue
              (e.entity,"createCustom_" + i.ToString());

            if (strVal==null || strVal.Length<=0) continue;

            alMethods.Add(CreateCreateMethod(strVal));
          }

          e.customcreatemethods=(type_generationGenerateentityCustomcreatemethod[])
            alMethods.ToArray(typeof(type_generationGenerateentityCustomcreatemethod));
        } else
          e.customcreatemethods=null;


        //Custom FindBy methods
        if (intCustomFindBy>0) {
          ArrayList alMethods=new ArrayList();

          for (int i=0;i<intCustomFindBy;i++) {
            string strVal=GetEntityGenerationOldParamValue (e.entity,"findCustom_" + i.ToString());
            if (strVal==null || strVal.Length<=0) continue;

            alMethods.Add(CreateFindByMethod(strVal));
          }

          e.customfindmethods=(type_generationGenerateentityCustomfindmethod[])
            alMethods.ToArray(typeof(type_generationGenerateentityCustomfindmethod));
        } else
          e.customfindmethods=null;

        //Constraint messages
        ArrayList alConstraintMsg=new ArrayList();
        string strConstrPrefix="Constraint_";
        foreach (OldParameterInfo param in malImportParms) {
          //Get all params that start with "Constraint_"
          if (param.Entity.Equals(e.entity) && param.Param.StartsWith(strConstrPrefix)) {
            string strForeignEntity=param.Param.Substring(strConstrPrefix.Length,param.Param.Length-strConstrPrefix.Length);

            //check if foreign entity exists
            foreach (dbdefinitionEntity eToCheck in mDBDefinition.entities) {
              if (eToCheck.name.Equals(strForeignEntity)) {
                //yes, store the constraint message
                type_generationGenerateentityRemoveconstraintmessage msg=new type_generationGenerateentityRemoveconstraintmessage();
                msg.foreignentity=strForeignEntity;
                msg.Value=param.Value;
                alConstraintMsg.Add(msg);
                break;
              }
            }
          }
        }
        if (alConstraintMsg.Count>0) {
          e.removeconstraintmessages=(type_generationGenerateentityRemoveconstraintmessage[])
            alConstraintMsg.ToArray(typeof(type_generationGenerateentityRemoveconstraintmessage));
        } else
          e.removeconstraintmessages=null;

      }
    }

    private type_generationGenerateentityCustomcreatemethod CreateCreateMethod(string pDef) {
      //create new entry
      type_generationGenerateentityCustomcreatemethod m=new type_generationGenerateentityCustomcreatemethod();

      //get the single tokens
      string[] astrTokens=pDef.Split(',');
      for (int intIndex=0; intIndex<astrTokens.Length; intIndex++) {
        astrTokens[intIndex] = astrTokens[intIndex].Replace("%%;%%", ",");
      }

      //first token is the method name
      m.name=astrTokens[0];

      //get the attributes to filter the query
      ArrayList alAttr=new ArrayList();
      int intCount=1;
      while (intCount<astrTokens.Length) {
        type_generationGenerateentityCustomcreatemethodMethodattribute attr=
          new type_generationGenerateentityCustomcreatemethodMethodattribute();
        attr.name=astrTokens[intCount];

        alAttr.Add(attr);
        intCount++;
      }

      //Set the found attributes
      m.methodattribute=(type_generationGenerateentityCustomcreatemethodMethodattribute[])
        alAttr.ToArray(typeof(type_generationGenerateentityCustomcreatemethodMethodattribute));

      return m;
    }

    private type_generationGenerateentityCustomfindmethod CreateFindByMethod(string pDef) {
      //create new entry
      type_generationGenerateentityCustomfindmethod m=new type_generationGenerateentityCustomfindmethod();

      //get the single tokens
      string[] astrTokens=pDef.Split(',');
      for (int intIndex=0; intIndex<astrTokens.Length; intIndex++) {
        astrTokens[intIndex] = astrTokens[intIndex].Replace("%%;%%", ",");
      }

      //first token is the method name
      m.name=astrTokens[0];

      //get the attributes to filter the query
      ArrayList alAttr=new ArrayList();
      int intCount=1;
      while (intCount<astrTokens.Length) {
        if (astrTokens[intCount].Equals("1") || astrTokens[intCount].Equals("0")) {
          //Digits found, stop loop
          break;
        }
        type_generationGenerateentityCustomfindmethodMethodattribute attr=new type_generationGenerateentityCustomfindmethodMethodattribute();
        attr.name=astrTokens[intCount];

        alAttr.Add(attr);
        intCount++;
      }

      //Set the found attributes
      m.methodattribute=(type_generationGenerateentityCustomfindmethodMethodattribute[])
        alAttr.ToArray(typeof(type_generationGenerateentityCustomfindmethodMethodattribute));

      m.returnsmultiple=astrTokens[intCount++].Equals("1");
      m.generatetest=astrTokens[intCount++].Equals("1");
      m.whereexpression=astrTokens[intCount++];
      m.orderbyexpression=astrTokens[intCount++];
      m.description=astrTokens[intCount+2];

      return m;
    }

    public void ImportTable_PVEntityGenerator() {
      if (malImportParms==null) this.FetchOldParameters();

      //Get the selected platform
      projectsettingsPlatformsPlatform platform=null;
      foreach (projectsettingsPlatformsPlatform pl in mProjectSettings.platforms.platform) {
        if (pl.name.Equals(mProjectSettings.platforms.selected)) {
          platform=pl;
          break;
        }
      }

      //Get the selected dbplatform
      projectsettingsDbplatformsDbplatform dbplatform=null;
      foreach (projectsettingsDbplatformsDbplatform pl in mProjectSettings.dbplatforms.dbplatform) {
        if (pl.name.Equals(mProjectSettings.dbplatforms.selected)) {
          dbplatform=pl;
          break;
        }
      }

      if (platform==null || dbplatform==null)
        throw new Exception("DBImportSettings.ImportTable_PVEntityGenerator: No platform or dbplatform found in projectsettings.");

      //Write the parameter values dependend on the platform/dbplatform
      for (int i=malImportParms.Count-1;i>=0;i--) {
        OldParameterInfo param=(OldParameterInfo)malImportParms[i];
        if (param.Entity.ToLower().Equals("_pventitygenerator")) {
          ProcessGlobalParameter(param,platform,dbplatform);
          malImportParms.RemoveAt(i);
          continue;
        }
      }

      //Entity generation parameters
      ProcessEntityParameters(platform,dbplatform);
    }

    class OldParameterInfo {
      public OldParameterInfo(string pEntity, string pParam, string pValue) {
        Entity=pEntity;
        Param=pParam;
        Value=pValue;
      }
      public string Entity;
      public string Param;
      public string Value;
    }
  }
}
