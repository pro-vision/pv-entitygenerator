using System;
using System.Data.OleDb;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Collections;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using PVFramework;
using PVFramework.Util;
using PVEntityGenerator.Util;
using PVEntityGenerator.XMLSchema;

namespace PVEntityGenerator {

  public class DBDataTransfer {

    public delegate void TransferDataMsgEventHandler(object pSender, TransferDataMsgEventArgs pArgs);
    public event TransferDataMsgEventHandler TransferDataMsg;

    private bool mfExport = true;
    private string mFilename = null;
    private dbplatformdefinition mDbPlatformDef = null;
    private projectsettingsDbplatformsDbplatform mDbPlatform = null;
    private dbdefinition mDBDefinition = null;
    private projectsettings mProjectSettings = null;
    private IDbConnection mServerConnectionInternal = null;
    private projectsettingsDbplatformsDbplatformDbprovidersDbprovider mProvider = null;
    private dbplatformdefinitionDbproviderdefinition mProviderDef = null;
    private Hashtable mhashDBPlatform = null;

    private bool mfCancel = false;

    public bool Cancel {
      get { return mfCancel; }
      set { mfCancel=value; }
    }

    private IDbConnection ServerConnection {
      get {
        if (mServerConnectionInternal == null) {
          mServerConnectionInternal = DBServerHelper.GetConnection(mhashDBPlatform, mProjectSettings);
        }
        return mServerConnectionInternal;
      }
    }

    public void CloseServerConnection() {
      if (mServerConnectionInternal != null) {
        mServerConnectionInternal.Close();
      }
    }

    public DBDataTransfer(bool pfExport, string pFilename,
      dbplatformdefinition pDbPlatformDef, projectsettingsDbplatformsDbplatform pDbPlatform,
      dbdefinition pDBDefinition, projectsettings pProjectSettings,
      Hashtable phashDBPlatform) {

      mfExport = pfExport;
      mFilename = pFilename;
      mDbPlatformDef = pDbPlatformDef;
      mDbPlatform = pDbPlatform;
      mDBDefinition=pDBDefinition;
      mProjectSettings = pProjectSettings;
      mhashDBPlatform = phashDBPlatform;

      foreach (projectsettingsDbplatformsDbplatformDbprovidersDbprovider p in
        mDbPlatform.dbproviders.dbprovider) {
        if (p.name.Equals(mDbPlatform.dbproviders.selected)) {
          mProvider = p;
          break;
        }
      }
      if (mProvider==null)
        throw new PVException("No or invalid db provider selected.");

      foreach (dbplatformdefinitionDbproviderdefinition pd in pDbPlatformDef.dbproviderdefinitions) {
        if (pd.name.Equals(mProvider.name)) {
          mProviderDef = pd;
          break;
        }
      }
      if (mProviderDef==null)
        throw new PVException("No provider definition found for provider '" + mProvider.name + "'.");

    }

    private void WriteLog(string pMsg) {
      if (TransferDataMsg!=null) TransferDataMsg(this,new TransferDataMsgEventArgs(pMsg));
    }

    private void WriteLogError(string pMsg) {
      WriteLog("ERROR: " + pMsg);
    }

    public void ExportToScript(string pFilename, StatusHandler pStatusHandler) {
      if (pFilename == null || pFilename.Length == 0) return;

      if (pFilename.EndsWith(".sql")) {
        ExportToScript_SQLFile(pFilename, pStatusHandler);
      }
      else if (pFilename.EndsWith(".xml")) {
        ExportToScript_DBUnitXMLFile(pFilename, pStatusHandler);
      }
      else {
        MessageBox.Show("Invalid filename for script export: " + pFilename + "\n"
          + "Expected file extensions: .sql or .xml.");
      }
    }

    public void ExportToScript_SQLFile(string pFilename, StatusHandler pStatusHandler) {
      bool fSmartRef=PVFormatUtil.ParseBoolean(DBServerHelper.GetDBPlatformParam(mDbPlatform,"smart-references"));
      bool fSmartRefOverDB=mDbPlatform.name.ToLower().Equals("sqlserver");
      bool fUnicode=PVFormatUtil.ParseBoolean(DBServerHelper.GetDBPlatformParam(mDbPlatform,"unicode"));

      //Obtain connetion to mdb-file
      ADODB.Connection conAccess=DBServerHelper.GetMDBConnectionADODB(mFilename);

      pStatusHandler.InitStatus("Generating script...",0);

      ADOX.Catalog cat=null;
      ArrayList aDroppedKeys=new ArrayList();

      if (fSmartRef && fSmartRefOverDB) {
        ADODB.Connection conSmartRef=GetConnectionSmartRef();
        cat=new ADOX.Catalog();
        cat.ActiveConnection = conSmartRef;
      }

      string strSQL="";
      ArrayList aSQL=new ArrayList();

      //DELETE-Statements
      foreach (projectsettingsEntityexportExportentity exp in
        new SortedEntities(mProjectSettings.entityexport.exportentities,false).Entity) {
        if (!exp.exportdata || exp.exportdrop) continue;

        strSQL="DELETE FROM " + exp.entity;
        if (!mDbPlatformDef.scriptdelimiternewline)
          strSQL+=mDbPlatformDef.scriptdelimiter;

        aSQL.Add(strSQL);

        if (mDbPlatformDef.scriptdelimiternewline)
          aSQL.Add(mDbPlatformDef.scriptdelimiter);
      }

      int intGroupStatements=PVFormatUtil.ParseInt(mDbPlatformDef.groupstatements);
      if (intGroupStatements==0) intGroupStatements=1;

      //INSERT-Statements
      foreach (projectsettingsEntityexportExportentity exp in
        new SortedEntities(mProjectSettings.entityexport.exportentities,true).Entity) {
        if (!exp.exportdata || exp.exportdrop) continue;

        dbdefinitionEntity entity=DBServerHelper.GetEntity(exp.entity,mDBDefinition);
        if (entity==null) continue;

        pStatusHandler.SetStatus("Generating data for table '" + entity.name + "'...", 0);

        if (fSmartRef) {
          if (fSmartRefOverDB) {
            //Read existing foreign-key definitions from DB
            ADOX.Table tbl=cat.Tables[entity.name];
            foreach (ADOX.Key fkey in tbl.Keys) {

              //Check only foreign keys referring to the same table
              if (fkey.Type!=ADOX.KeyTypeEnum.adKeyForeign) continue;
              if (!fkey.RelatedTable.Equals(entity.name)) continue;

              aDroppedKeys.Add(new DroppedKeyInfo(fkey));

              strSQL="ALTER TABLE " + entity.name + " DROP CONSTRAINT " + fkey.Name;
              if (mDbPlatformDef.scriptdelimiternewline)
                strSQL+="\n";
              strSQL+=mDbPlatformDef.scriptdelimiter;
              aSQL.Add(strSQL);
            }
          } else {
            //Read foreign key definitions from XML
            if (entity.keys.foreignkey!=null) {
              foreach (dbdefinitionEntityKeysForeignkey fkey in entity.keys.foreignkey) {

                //Check only foreign keys referring to the same table
                if (!fkey.foreignentity.Equals(entity.name)) continue;

                aDroppedKeys.Add(new DroppedKeyInfo(fkey));

                strSQL= "ALTER TABLE " + entity.name + " DROP CONSTRAINT " + fkey.name;
                if (mDbPlatformDef.scriptdelimiternewline)
                  strSQL+="\n";
                strSQL+=mDbPlatformDef.scriptdelimiter;
                aSQL.Add(strSQL);
              }
            }
          }
        }

        object recs =null;
        strSQL="SELECT * FROM [" + entity.name + "]";
        ADODB.Recordset rs = conAccess.Execute(strSQL, out recs, 0);

        int intStatementCounter=0;
        while (!rs.EOF) {
          strSQL="INSERT INTO " + entity.name + "(" + GetFields(rs,true) + ") VALUES("
            + GetValues(rs,fUnicode,true) + ")";

          if (++intStatementCounter>=intGroupStatements) {
            if (mDbPlatformDef.scriptdelimiternewline)
              strSQL+="\n";

            strSQL+=mDbPlatformDef.scriptdelimiter;
            intStatementCounter=0;
          }

          aSQL.Add(strSQL);

          rs.MoveNext();
        }
        rs.Close();

        if (intStatementCounter!=0)
          aSQL.Add(mDbPlatformDef.scriptdelimiter);

        if (fSmartRef && aDroppedKeys.Count>0) {
          foreach (DroppedKeyInfo key in aDroppedKeys) {

            strSQL="ALTER TABLE " + entity.name + " ADD CONSTRAINT " + key.Name + " FOREIGN KEY ("
              + key.Columns + ") REFERENCES " + key.RelatedTable + " (" + key.ColumnsRelated + ")";

            if (mDbPlatformDef.scriptdelimiternewline)
              strSQL+="\n";
            strSQL+=mDbPlatformDef.scriptdelimiter;
            aSQL.Add(strSQL);
          }

          aDroppedKeys.Clear();
        }
      }

      int intStep=0;
      pStatusHandler.InitStatus("Writing script...",aSQL.Count);

      StreamWriter writer = File.CreateText(pFilename);
      foreach (string strSQLExecute in aSQL) {
        writer.WriteLine(strSQLExecute);
        pStatusHandler.SetStatus("Writing script...", ++intStep);
      }

      writer.Flush();
      writer.Close();

      pStatusHandler.SetStatus("Script generated sucessfully.",0);
    }

    public void ExportToScript_DBUnitXMLFile(string pFilename, StatusHandler pStatusHandler) {
      ADODB.Connection conAccess = DBServerHelper.GetMDBConnectionADODB(mFilename);

      pStatusHandler.InitStatus("Generating script...", 0);

      XmlWriter writer = XmlTextWriter.Create(pFilename, App.XML_WRITER_SETTINGS);

      writer.WriteStartDocument();
      writer.WriteStartElement("dataset");

      foreach (projectsettingsEntityexportExportentity exp in
        new SortedEntities(mProjectSettings.entityexport.exportentities, true).Entity) {
        if (!exp.exportdata || exp.exportdrop) continue;

        dbdefinitionEntity entity = DBServerHelper.GetEntity(exp.entity, mDBDefinition);
        if (entity == null) continue;

        pStatusHandler.SetStatus("Generating data for table '" + entity.name + "'...", 0);

        object recs = null;
        string strSQL = "SELECT * FROM [" + entity.name + "]";
        ADODB.Recordset rs = conAccess.Execute(strSQL, out recs, 0);

        // table element
        writer.WriteStartElement("table");
        writer.WriteAttributeString("name", entity.name);

        // column names
        foreach (ADODB.Field field in rs.Fields) {
          writer.WriteStartElement("column");
          writer.WriteValue(field.Name);
          writer.WriteEndElement();
        }

        // data
        while (!rs.EOF) {
          writer.WriteStartElement("row");

          foreach (ADODB.Field field in rs.Fields) {
            if (field.Value == DBNull.Value) {
              writer.WriteStartElement("null");
              writer.WriteEndElement();
            }
            else {
              writer.WriteStartElement("value");
              if (field.Value is bool) {
                writer.WriteValue(((bool)field.Value) ? 1 : 0);
              }
              else if (field.Value is DateTime) {
                writer.WriteValue(((DateTime)field.Value).ToString("yyyy-MM-dd HH:mm:ss"));
              }
              else {
                writer.WriteValue(field.Value);
              }
              writer.WriteEndElement();
            }
          }

          writer.WriteEndElement();

          rs.MoveNext();
        }
        rs.Close();

        writer.WriteEndElement();
      }

      writer.WriteEndElement();
      writer.WriteEndDocument();

      writer.Flush();
      writer.Close();

      pStatusHandler.SetStatus("Script generated sucessfully.", 0);
    }

    private string RepeatString(string p, string pDelim, int pCount, bool pfAppendNr) {
      StringBuilder str=new StringBuilder();

      for (int i=0;i<pCount;i++) {
        str.Append(p);
        if (pfAppendNr)
          str.Append(i.ToString());
        str.Append(pDelim);
      }

      if (str.Length==0) return "";
      else return (str.ToString(0,str.Length-pDelim.Length));
    }

    private int GetAttrCount(dbdefinitionEntity pEntity,bool pfSkipBinary) {
      int intCount=0;
      foreach (dbdefinitionEntityAttribute attr in pEntity.attributes) {
        if (pfSkipBinary && attr.type==type_AttributeType.BLOB) continue;

        intCount++;
      }

      return intCount;
    }

    private int GetAttrCount(ADODB.Recordset pRs,bool pfSkipBinary) {
      int intCount=0;

      IEnumerator fields=pRs.Fields.GetEnumerator();
      while (fields.MoveNext()) {

        ADODB.InternalField fld=(ADODB.InternalField)fields.Current;
        if (pfSkipBinary && (
          fld.Type==ADODB.DataTypeEnum.adBinary ||
          fld.Type==ADODB.DataTypeEnum.adVarBinary ||
          fld.Type==ADODB.DataTypeEnum.adLongVarBinary)) continue;

        intCount++;
      }

      return intCount;
    }

    private string GetValues(ADODB.Recordset pRs, bool pfUnicode, bool pfSkipBinary) {
      StringBuilder str=new StringBuilder();

      IEnumerator fields=pRs.Fields.GetEnumerator();
      while (fields.MoveNext()) {

        ADODB.InternalField fld=(ADODB.InternalField)fields.Current;
        if (pfSkipBinary && (
          fld.Type==ADODB.DataTypeEnum.adBinary ||
          fld.Type==ADODB.DataTypeEnum.adVarBinary ||
          fld.Type==ADODB.DataTypeEnum.adLongVarBinary)) continue;

        string strLine="NULL";
        if (fld.Value!=null & !fld.Value.Equals(DBNull.Value)) {

          switch (fld.Type) {
            case (ADODB.DataTypeEnum.adVarChar):
            case (ADODB.DataTypeEnum.adWChar):
            case (ADODB.DataTypeEnum.adVarWChar):
            case (ADODB.DataTypeEnum.adLongVarChar):
            case (ADODB.DataTypeEnum.adLongVarWChar):
              strLine=SQLStringFormat((string)fld.Value);
              if (pfUnicode && !mDbPlatform.name.ToLower().Equals("mysql"))
                strLine="N" + strLine;
              break;

            case (ADODB.DataTypeEnum.adBoolean):
              strLine="0";
              if ((bool)fld.Value)
                strLine="1";
              break;

            case (ADODB.DataTypeEnum.adTinyInt):
            case (ADODB.DataTypeEnum.adSmallInt):
            case (ADODB.DataTypeEnum.adInteger):
            case (ADODB.DataTypeEnum.adSingle):
            case (ADODB.DataTypeEnum.adDouble):
            case (ADODB.DataTypeEnum.adCurrency):
              strLine=fld.Value.ToString().Replace(",",".");
              break;
            case (ADODB.DataTypeEnum.adNumeric):
              strLine=fld.Value.ToString().Replace(",",".");
              break;
            case (ADODB.DataTypeEnum.adDate):
              strLine=SQLDateTimeFormat((DateTime)fld.Value);
              break;
            case (ADODB.DataTypeEnum.adLongVarBinary):
            case (ADODB.DataTypeEnum.adBinary):
            case (ADODB.DataTypeEnum.adVarBinary):
              strLine="NULL";
              break;
            case (ADODB.DataTypeEnum.adGUID):
              strLine="'" + ((string)fld.Value).Substring(2,36) + "'";
              break;

            default:
              throw new Exception("Unsupported Field Type in field <" + fld.Name + ">.");
          }
        }

        str.Append(strLine);
        str.Append(",");
      }

      if (str.Length==0) return "";
      else return (str.ToString(0,str.Length-1));

    }

    private string GetFields(dbdefinitionEntity pEntity, bool pfSkipBinary, bool pfMSAccess) {
      StringBuilder str=new StringBuilder();

      foreach (dbdefinitionEntityAttribute attr in pEntity.attributes) {
        if (pfSkipBinary && attr.type==type_AttributeType.BLOB) continue;

        if (pfMSAccess) str.Append("[");

        str.Append(attr.name);

        if (pfMSAccess) str.Append("]");

        str.Append(",");
      }

      if (str.Length==0) return "";
      else return (str.ToString(0,str.Length-1));
    }

    private string GetFields(dbdefinitionEntity pEntity, bool pfSkipBinary) {
      return GetFields(pEntity,pfSkipBinary,false);
    }

    private string GetFields(ADODB.Recordset pRs, bool pfSkipBinary) {
      StringBuilder str=new StringBuilder();

      IEnumerator fields=pRs.Fields.GetEnumerator();
      while (fields.MoveNext()) {

        ADODB.InternalField fld=(ADODB.InternalField)fields.Current;
        if (pfSkipBinary && (
          fld.Type==ADODB.DataTypeEnum.adBinary ||
          fld.Type==ADODB.DataTypeEnum.adVarBinary ||
          fld.Type==ADODB.DataTypeEnum.adLongVarBinary)) continue;

        str.Append(fld.Name);
        str.Append(",");
      }

      if (str.Length==0) return "";
      else return (str.ToString(0,str.Length-1));
    }

    private IDbCommand GetInsertCommand(dbdefinitionEntity pEntity, OleDbConnection pcon, bool pfSkipBinary) {
      string strParamPrefix="@param";
      string strParam=null;
      strParam=RepeatString(strParamPrefix,",",GetAttrCount(pEntity,pfSkipBinary),true);

      string strSQL="INSERT INTO [" + pEntity.name + "] (" + GetFields(pEntity,pfSkipBinary,true)
        + ") VALUES(" + strParam + ")";

      IDbCommand com=pcon.CreateCommand();
      com.CommandType=CommandType.Text;
      com.CommandText=strSQL;
      int intCounter=0;

      //Es werden schonmal Werte für die Parameter vorgegeben in der Hoffnung, dass Prepare funzt (tut es aber nicht)
      foreach (dbdefinitionEntityAttribute attr in pEntity.attributes) {
        if (pfSkipBinary && attr.type==type_AttributeType.BLOB) continue;

        IDataParameter param=com.CreateParameter();

        switch (attr.type) {
          case (type_AttributeType.BIGINT):
            break;
          case (type_AttributeType.BIT):
            param.DbType=DbType.Boolean;
            param.Value=false;
            break;
          case (type_AttributeType.BLOB):
            param.Value=new byte[1]{1};
            param.DbType=DbType.Binary;
            break;
          case (type_AttributeType.BYTE):
            param.Value=0;
            param.DbType=DbType.Byte;
            break;
          case (type_AttributeType.VARCHAR):
          case (type_AttributeType.CLOB):
            param.Value="";
            param.DbType=DbType.String;
            break;
          case (type_AttributeType.FLOAT):
            param.Value = 0;
            param.DbType = DbType.Double;
            break;
          case (type_AttributeType.DECIMAL):
            param.Value = 0;
            param.DbType = DbType.Double;
            break;
          case (type_AttributeType.ID):
          case (type_AttributeType.VSTAMP):
          case (type_AttributeType.INTEGER):
            param.Value=0;
            param.DbType=DbType.Int32;
            break;
          case (type_AttributeType.SMALLINT):
            param.Value=0;
            param.DbType=DbType.Int16;
            break;
          case (type_AttributeType.TIMESTAMP):
            param.Value=DateTime.Now;
            param.DbType=DbType.DateTime;
            break;
          default:
            throw new Exception("Unsupported ata type: " + attr.type);
        }

        param.ParameterName=strParamPrefix + (intCounter++).ToString();
        com.Parameters.Add(param);

      }

      //TODO: IDBCommand.Prepare() einsetzen. Funktioniert nicht korrekt mit Parameter.
      //com.Prepare();

      return com;
    }

    private IDbCommand GetInsertCommand(string pTable,ADODB.Recordset pRs,IDbConnection pcon, bool pfSkipBinary) {
      string strParam=null;
      if (mProviderDef.sqlparameterprefix!=null)
        strParam=RepeatString(mProviderDef.sqlparameterprefix + "param",",",GetAttrCount(pRs,pfSkipBinary),true);
      else
        strParam=RepeatString("?",",",GetAttrCount(pRs,pfSkipBinary),false);

      string strSQL="INSERT INTO " + pTable + "(" + GetFields(pRs,pfSkipBinary)
        + ") VALUES(" + strParam + ")";


      bool fBooleanToNumericOracle = mDbPlatform.name.ToLower().Equals("oracle");
      bool fBooleanToNumericPostgreSQL = mDbPlatform.name.ToLower().Equals("postgresql");

      IDbCommand com=pcon.CreateCommand();
      com.CommandType=CommandType.Text;
      com.CommandText=strSQL;

      int intCounter=0;
      IEnumerator fields=pRs.Fields.GetEnumerator();

      //Es werden schonmal Werte für die Parameter vorgegeben in der Hoffnung,
      //dass Prepare funzt (tut es aber nicht)
      while (fields.MoveNext()) {

        ADODB.InternalField fld=(ADODB.InternalField)fields.Current;
        if (pfSkipBinary && (
          fld.Type==ADODB.DataTypeEnum.adBinary ||
          fld.Type==ADODB.DataTypeEnum.adVarBinary ||
          fld.Type==ADODB.DataTypeEnum.adLongVarBinary)) continue;

        IDataParameter param=com.CreateParameter();

        switch (fld.Type) {
          case (ADODB.DataTypeEnum.adBoolean):
            if (fBooleanToNumericOracle)
            {
              param.DbType=DbType.Byte;
              param.Value=1;
            } else if(fBooleanToNumericPostgreSQL) {
                            param.DbType=DbType.Int16;
              param.Value=1;
            } else {
              param.DbType=DbType.Boolean;
              param.Value=false;
            }
            break;

          case (ADODB.DataTypeEnum.adVarChar):
          case (ADODB.DataTypeEnum.adWChar):
          case (ADODB.DataTypeEnum.adVarWChar):
          case (ADODB.DataTypeEnum.adLongVarChar):
          case (ADODB.DataTypeEnum.adLongVarWChar):
            param.Value="";
            param.DbType=DbType.String;
            break;

          case (ADODB.DataTypeEnum.adTinyInt):
            param.Value=0;
            param.DbType=DbType.Byte;
            break;

          case (ADODB.DataTypeEnum.adSmallInt):
            param.Value=0;
            param.DbType=DbType.Int16;
            break;

          case (ADODB.DataTypeEnum.adInteger):
            param.Value=0;
            param.DbType=DbType.Int32;
            break;

          case (ADODB.DataTypeEnum.adSingle):
            param.Value=0;
            param.DbType=DbType.Single;
            break;

          case (ADODB.DataTypeEnum.adDouble):
            param.Value=0;
            param.DbType=DbType.Double;
            break;

          case (ADODB.DataTypeEnum.adCurrency):
          case (ADODB.DataTypeEnum.adNumeric):
            param.Value=0;
            param.DbType=DbType.Decimal;
            break;

          case (ADODB.DataTypeEnum.adDate):
            param.Value=DateTime.Now;
            param.DbType=DbType.DateTime;
            break;

          case (ADODB.DataTypeEnum.adLongVarBinary):
          case (ADODB.DataTypeEnum.adBinary):
          case (ADODB.DataTypeEnum.adVarBinary):
            param.Value=new byte[1]{1};
            param.DbType=DbType.Binary;
            break;

          case (ADODB.DataTypeEnum.adGUID):
            param.Value=new Guid("");
            param.DbType=DbType.Guid;
            break;
        }

        if (mProviderDef.sqlparameterprefix!=null)
          param.ParameterName=mProviderDef.sqlparameterprefix + "param" + (intCounter++).ToString();

        com.Parameters.Add(param);
      }

      //TODO: IDBCommand.Prepare() einsetzen. Funktioniert nicht korrekt mit Parameter.
      //com.Prepare();

      return com;
    }

    private void SetInsertCommandParams(IDbCommand pcom,ADODB.Recordset pRs, bool pfSkipBinary) {
      int intCount=0;
      IEnumerator fields=pRs.Fields.GetEnumerator();
      bool fPostgreSQL = mDbPlatform.name.ToLower().Equals("postgresql");
      bool fMySQL = mDbPlatform.name.ToLower().Equals("mysql");
      while (fields.MoveNext()) {

        ADODB.InternalField fld=(ADODB.InternalField)fields.Current;
        if (pfSkipBinary && (
          fld.Type==ADODB.DataTypeEnum.adBinary ||
          fld.Type==ADODB.DataTypeEnum.adVarBinary ||
          fld.Type==ADODB.DataTypeEnum.adLongVarBinary))
        {
          continue;
        }

        if (fPostgreSQL && fld.Type == ADODB.DataTypeEnum.adBoolean) {
          if ((bool)fld.Value == true) {
            ((IDataParameter)pcom.Parameters[intCount++]).Value = 1;
          }
          else {
            ((IDataParameter)pcom.Parameters[intCount++]).Value = 0;
          }
        }
        // -- commented out: it seems this is no longer required with the current mysql provider, and corrupts data if it contains backslashes
        /*else if ((fMySQL) && (
          fld.Type==ADODB.DataTypeEnum.adVarChar ||
          fld.Type==ADODB.DataTypeEnum.adWChar ||
          fld.Type==ADODB.DataTypeEnum.adVarWChar ||
          fld.Type==ADODB.DataTypeEnum.adLongVarChar ||
          fld.Type==ADODB.DataTypeEnum.adLongVarWChar) &&
          fld.Value is string && fld.Value!=null && ((string)fld.Value).Length>0) {

          //With MySQL '\' is used in escape sequences, replace it with '\\'
          ((IDataParameter)pcom.Parameters[intCount++]).Value=((string)fld.Value).Replace("\\","\\\\");
        }*/
        else {
          ((IDataParameter)pcom.Parameters[intCount++]).Value = fld.Value;
        }
      }
    }

    private void SetInsertCommandParams(IDbCommand pcom,IDataReader pDr,dbdefinitionEntity pEntity, bool pfSkipBinary) {

      for (int i=0;i<pEntity.attributes.Length;i++) {
        object val = pDr.GetValue(i);
        if (val is DateTime) {
          ((IDataParameter)pcom.Parameters[i]).Value = ((DateTime)val).Subtract(new TimeSpan(0, 0, 0, 0, ((DateTime)val).Millisecond));
        }
        else {
          ((IDataParameter)pcom.Parameters[i]).Value = val;
        }

      }
    }

    private string SQLStringFormat(string p) {
      return "'" + p.Replace("'","''") + "'";
    }

    private string SQLDateTimeFormat(DateTime pdat) {
      if (mDbPlatform.name.ToLower().Equals("oracle"))
        return "TO_DATE('" + PVFormatUtil.FormatDateTime(pdat) + "','DD.MM.YYYY HH24:MI')";
      else if (mDbPlatform.name.ToLower().Equals("mysql"))
        return "'" + pdat.ToString("yyyy-MM-dd HH:mm:ss") + "'";

      return "{ts '" + pdat.ToString("yyyy\\-MM\\-dd HH\\:mm\\:ss") + "'}";
    }

    private class SortedEntities {
      private ArrayList mSorted;

      private class NrComparer: IComparer {
        bool mfAsc=true;
        public NrComparer(bool pfAsc) {
          mfAsc=pfAsc;
        }
        public int Compare(object x, object y) {
          int intX=PVFormatUtil.ParseInt(((projectsettingsEntityexportExportentity)x).sortno);
          int intY=PVFormatUtil.ParseInt(((projectsettingsEntityexportExportentity)y).sortno);

          if (mfAsc) {
            if (intX>intY) return 1;
            if (intX<intY) return -1;
          }
          else {
            if (intX>intY) return -1;
            if (intX<intY) return 1;
          }
          return 0;
        }
      }

      public SortedEntities(projectsettingsEntityexportExportentity[] pEntities,bool pfAsc) {
        mSorted = new ArrayList();
        foreach (projectsettingsEntityexportExportentity exp in pEntities) {
          mSorted.Add(exp);
        }

        mSorted.Sort(new NrComparer(pfAsc));
      }

      public ArrayList Entity {
        get {return mSorted;}
      }
    }

    private ADODB.Connection GetConnectionSmartRef() {

      bool trustedAuth = false;
      foreach (projectsettingsDbplatformsDbplatform dbplatform in mProjectSettings.dbplatforms.dbplatform) {
        if (dbplatform.name.Equals(mProjectSettings.dbplatforms.selected)) {
          foreach (projectsettingsDbplatformsDbplatformDbprovidersDbprovider dbprovider in dbplatform.dbproviders.dbprovider) {
            if (dbprovider.name.Equals(dbplatform.dbproviders.selected)) {
              foreach (dbplatformdefinitionDbproviderdefinition pd in mDbPlatformDef.dbproviderdefinitions) {
                if (pd.name.Equals(dbprovider.name)) {
                  trustedAuth = pd.trustedauth;
                }
              }
            }
          }
          break;
        }
      }

      dbplatformdefinitionDbproviderdefinition providerdef = null;
      foreach (dbplatformdefinitionDbproviderdefinition pd in mDbPlatformDef.dbproviderdefinitions) {
        if (pd.type==dbplatformdefinitionDbproviderdefinitionType.OLEDB && pd.connectionstring!=null && pd.connectionstring.Length>0
            && pd.trustedauth==trustedAuth) {
          providerdef = pd;
          break;
        }
      }

      projectsettingsDbplatformsDbplatformDbprovidersDbprovider provider = null;
      foreach (projectsettingsDbplatformsDbplatformDbprovidersDbprovider p in mDbPlatform.dbproviders.dbprovider) {
        if (p.name.Equals(mDbPlatform.dbproviders.selected)) {
          provider = p;
          break;
        }
      }

      string strConnect = providerdef.connectionstring;
      if (provider.parameters!=null) {
        foreach (parametersParameter param in provider.parameters) {
          string strValue = param.Value;
          if (param.name.Equals("password")) {
            try {
              strValue = PasswordHelper.DecryptPassword(strValue);
            }
            catch (Exception) {}
          }
          strConnect = strConnect.Replace("{" + param.name + "}", strValue);
        }
      }
      ADODB.Connection con = new ADODB.ConnectionClass();

      con.Open(strConnect, "", "", 0);
      return con;
    }

    public void TransferData(ProgressBar pBar) {
      bool fSQLServer = mDbPlatform.name.ToLower().Equals("sqlserver");

      bool fSmartRef = PVFormatUtil.ParseBoolean(DBServerHelper.GetDBPlatformParam(mDbPlatform, "smart-references"));
      bool fSmartRefOverDB = fSQLServer;

      bool fSimpleForeignKeyCheck = mDbPlatformDef.foreignkeychecksoff != null && mDbPlatformDef.foreignkeychecksoff.Length > 0
        && mDbPlatformDef.foreignkeycheckson != null && mDbPlatformDef.foreignkeycheckson.Length > 0;

      // Obtain connetion to mdb-file
      ADODB.Connection conLocal = DBServerHelper.GetMDBConnectionADODB(mFilename);

      // Conection for smart reference checks
      ADODB.Connection conServer = null;

      if (fSmartRef && fSmartRefOverDB) {
        if (fSmartRefOverDB) {
          conServer = GetConnectionSmartRef();
        }
      }

      if (mfExport) {
        // MDB -> Server
        TransferData_LocalToServer(conLocal, conServer,
          fSmartRef, fSmartRefOverDB, fSimpleForeignKeyCheck, pBar);
      }
      else {
        // Server -> MDB
        TransferData_ServerToLocal(conLocal, conServer,
          fSmartRef, fSmartRefOverDB, fSimpleForeignKeyCheck, pBar);
      }

      if (conServer != null) {
        conServer.Close();
      }

      conLocal.Close();
    }

    public void TransferData_LocalToServer(ADODB.Connection pconLocal, ADODB.Connection pconServer,
        bool pSmartRef, bool pSmartRefOverDB, bool pSimpleForeignKeyCheck,
        ProgressBar pBar) {

      ADOX.Catalog catLocal = new ADOX.Catalog();
      catLocal.ActiveConnection = pconLocal;
      ADOX.Catalog catServer = null;
      if (pconServer != null) {
        catServer = new ADOX.Catalog();
        catServer.ActiveConnection = pconServer;
      }

      bool fMySQL = mDbPlatform.name.ToLower().Equals("mysql");

      ArrayList aDroppedKeys = new ArrayList();
      string strSQL = null;

      //DELETE-Statements
      int intCount = 0;
      IDbCommand com = this.ServerConnection.CreateCommand();
      foreach (projectsettingsEntityexportExportentity exp in
        new SortedEntities(mProjectSettings.entityexport.exportentities, false).Entity) {

        if (!exp.exportdata) continue;
        if (mfCancel) break;

        if (pSmartRef && pSimpleForeignKeyCheck) {
          SimpleForeignKeyCheck(this.ServerConnection, exp.entity, false);
        }

        com.CommandText = "DELETE FROM " + exp.entity;
        com.ExecuteNonQuery();
        WriteLog("Deleted data from table " + exp.entity + ".");

        if (pSmartRef && pSimpleForeignKeyCheck) {
          SimpleForeignKeyCheck(this.ServerConnection, exp.entity, true);
        }

        intCount++;
      }

      WriteLog("");
      if (intCount == 1) {
        WriteLog("Data deleted from 1 table.");
      }
      else {
        WriteLog("Data deleted from " + intCount + " tables.");
      }

      if (mfCancel) {
        return;
      }

      pBar.Maximum = intCount;

      //statistics variables
      intCount = 0;
      DateTime datStart = DateTime.Now;
      int intCountTotalRecords = 0;
      int intCountTotalTables = 0;

      foreach (projectsettingsEntityexportExportentity exp in
        new SortedEntities(mProjectSettings.entityexport.exportentities, true).Entity) {
        if (!exp.exportdata || exp.exportdrop) continue;
        if (mfCancel) break;

        dbdefinitionEntity entity = DBServerHelper.GetEntity(exp.entity, mDBDefinition);
        if (entity == null) continue;

        //INSERT-Statements
        if (pSmartRef) {
          com = this.ServerConnection.CreateCommand();
          com.CommandType = CommandType.Text;

          if (pSimpleForeignKeyCheck) {
            SimpleForeignKeyCheck(this.ServerConnection, entity.name, false);
          }
          else {
            if (pSmartRefOverDB) {
              //Read existing foreign-key definitions from DB
              ADOX.Table tbl = catServer.Tables[entity.name];
              foreach (ADOX.Key fkey in tbl.Keys) {

                //Check only foreign keys referring to the same table
                if (fkey.Type != ADOX.KeyTypeEnum.adKeyForeign) continue;
                if (!fkey.RelatedTable.Equals(entity.name)) continue;

                aDroppedKeys.Add(new DroppedKeyInfo(fkey));

                strSQL = "ALTER TABLE " + entity.name + " DROP CONSTRAINT " + fkey.Name;
                com.CommandText = strSQL;
                com.ExecuteNonQuery();
                WriteLog("Constraint (foreign key) " + fkey.Name + " deleted.");
              }

            }
            else {
              //Read foreign key definitions from XML
              if (entity.keys.foreignkey != null) {
                foreach (dbdefinitionEntityKeysForeignkey fkey in entity.keys.foreignkey) {

                  //Check only foreign keys referring to the same table
                  if (!fkey.foreignentity.Equals(entity.name)) continue;

                  //SQL-Script for dropping a constraint differs from db to db
                  if (fMySQL) {
                    strSQL = "ALTER TABLE " + entity.name + " DROP FOREIGN KEY " + fkey.name;
                  }
                  else {
                    strSQL = "ALTER TABLE " + entity.name + " DROP CONSTRAINT " + fkey.name;
                  }

                  com.CommandText = strSQL;
                  bool fConstraintFound = false;
                  try {
                    com.ExecuteNonQuery();
                    fConstraintFound = true;
                  }
                  catch (Oracle.DataAccess.Client.OracleException pex) {
                    //Oracle throws a special exception if constraint is not found
                    //MySQL just ignores the drop statement if no constraint is found
                    if (pex.Message.IndexOf("ORA-02443") == -1) throw new PVException(pex);
                  }
                  catch (System.Data.OleDb.OleDbException pex) {
                    //same here for oledb drivers
                    if (pex.Message.IndexOf("ORA-02443") == -1) throw new PVException(pex);
                  }

                  if (fConstraintFound) {
                    aDroppedKeys.Add(new DroppedKeyInfo(fkey));
                    WriteLog("Constraint (foreign key) " + fkey.name + " deleted.");
                  }
                  else {
                    WriteLog("Constraint (foreign key) " + fkey.name + " not found.");
                  }
                }
              }
            }
          }
        }

        int intTotalTable = GetRecordCount("[" + entity.name + "]", pconLocal);

        object recs = null;
        strSQL = "SELECT * FROM [" + entity.name + "]";
        ADODB.Recordset rs = pconLocal.Execute(strSQL, out recs, 0);

        intCount = 0;
        if (!rs.EOF) {
          com = GetInsertCommand(entity.name, rs, this.ServerConnection, false);

          while (!rs.EOF) {
            if (mfCancel) break;

            SetInsertCommandParams(com, rs, false);
            com.ExecuteNonQuery();

            intCount++;
            intCountTotalRecords++;
            rs.MoveNext();

            if (intCount % 100 == 0) {
              WriteLog("Table " + entity.name + ": inserted " + intCount + " records (" + RecordCompletionPercent(intCount, intTotalTable) + ").");
            }
          }
        }
        rs.Close();

        if (!mfCancel) {
          if (intCount == 1) {
            WriteLog("Table " + entity.name + ": inserted 1 record (" + RecordCompletionPercent(intCount, intTotalTable) + ").");
            intCountTotalTables++;
          }
          else if (intCount > 0) {
            WriteLog("Table " + entity.name + ": inserted " + intCount + " records (" + RecordCompletionPercent(intCount, intTotalTable) + ").");
            intCountTotalTables++;
          }

          if (pSmartRef) {
            if (pSimpleForeignKeyCheck) {
              SimpleForeignKeyCheck(this.ServerConnection, entity.name, true);
            }
            else if (aDroppedKeys.Count > 0) {
              foreach (DroppedKeyInfo key in aDroppedKeys) {

                strSQL = "ALTER TABLE " + entity.name + " ADD CONSTRAINT " + key.Name + " FOREIGN KEY ("
                  + key.Columns + ") REFERENCES " + key.RelatedTable + " (" + key.ColumnsRelated + ")";

                com = this.ServerConnection.CreateCommand();
                com.CommandType = CommandType.Text;
                com.CommandText = strSQL;
                com.ExecuteNonQuery();

                WriteLog("Constraint (foreign key) " + key.Name + " restored.");
              }

              aDroppedKeys.Clear();
            }
          }

        }
        else {
          //Bei "Stop transfer" die foreign keys wieder einschalten
          if (pSmartRef && pSimpleForeignKeyCheck) {
            SimpleForeignKeyCheck(this.ServerConnection, entity.name, true);
          }
          break;
        }
        pBar.Increment(1);
        Application.DoEvents();
      }

      TimeSpan spn = DateTime.Now.Subtract(datStart);

      WriteLog("");
      string strMsg = "Data transfer complete: ";
      if (mfCancel) {
        strMsg = "Data transfer aborted by user: ";
      }
      WriteLog(strMsg + "imported " + intCountTotalRecords + " records from "
        + intCountTotalTables + " tables.");
      WriteLog("Transfer time: " + spn.Minutes.ToString("00") + ":" + spn.Seconds.ToString("00") + " min, "
        + (intCountTotalRecords / ((spn.TotalSeconds) / 60)).ToString("#,##0") + " records/min");
    }

    public int GetRecordCount(string entityName, ADODB.Connection con) {
      string strSQL = "SELECT COUNT(*) FROM " + entityName;
      object recs = null;
      ADODB.Recordset rs = con.Execute(strSQL, out recs);
      try {
        if (!rs.EOF) {
          return (int)rs.Fields[0].Value;
        }
      }
      finally {
        rs.Close();
      }
      return 0;
    }

    public int GetRecordCount(string entityName, IDbConnection con) {
      string strSQL = "SELECT COUNT(*) FROM " + entityName;
      using (IDbCommand cmd = con.CreateCommand()) {
        cmd.CommandText = strSQL;
        cmd.CommandType = CommandType.Text;
        return (int)(long)cmd.ExecuteScalar();
      }
    }

    public string RecordCompletionPercent(int count, int totalCount) {
      return PVFormatUtil.FormatNumber((double)count / (double)totalCount, "0%");
    }

    public void TransferData_ServerToLocal(ADODB.Connection pconLocal, ADODB.Connection pconServer,
        bool pSmartRef, bool pSmartRefOverDB, bool pSimpleForeignKeyCheck,
        ProgressBar pBar) {

      ADOX.Catalog catLocal = new ADOX.Catalog();
      catLocal.ActiveConnection = pconLocal;

      ArrayList aDroppedKeys = new ArrayList();
      int intCount = 0;

      //DELETE-Statements
      foreach (projectsettingsEntityexportExportentity exp in
        new SortedEntities(mProjectSettings.entityexport.exportentities, false).Entity) {
        if (!exp.exportdata) continue;
        if (mfCancel) break;

        if (pSmartRef) {
          ADOX.Table tbl = catLocal.Tables[exp.entity];
          foreach (ADOX.Key fkey in tbl.Keys) {

            //Check only foreign keys referring to the same table
            if (fkey.Type != ADOX.KeyTypeEnum.adKeyForeign) {
              continue;
            }
            if (!fkey.RelatedTable.ToLower().Equals(exp.entity.ToLower())) {
              continue;
            }

            aDroppedKeys.Add(new DroppedKeyInfo(fkey));
          }
          foreach (DroppedKeyInfo key in aDroppedKeys) {
            tbl.Keys.Delete(key.Name);
            WriteLog("Constraint (foreign key) " + key.Name + " deleted.");
          }
        }

        object recs;
        pconLocal.Execute("DELETE * FROM [" + exp.entity + "]", out recs, 0);

        if (pSmartRef && aDroppedKeys.Count > 0) {
          ADOX.Table tbl = catLocal.Tables[exp.entity];
          foreach (DroppedKeyInfo key in aDroppedKeys) {
            key.AppendToTableADOX(tbl, pconLocal);
            WriteLog("Constraint (foreign key) " + key.Name + " restored.");
          }
        }

        aDroppedKeys.Clear();
        intCount++;
      }

      if (mfCancel) {
        return;
      }

      // reload local catalog because of ddl changes using direct sql commands
      catLocal = new ADOX.Catalog();
      catLocal.ActiveConnection = pconLocal;

      if (intCount == 1) {
        WriteLog("Data deleted from 1 table.");
      }
      else {
        WriteLog("Data deleted from " + intCount + " tables.");
      }

      OleDbConnection conOle = DBServerHelper.GetMDBConnectionOLEDB(mFilename);
      DateTime datStart = DateTime.Now;
      int intCountTotalRecords = 0;
      int intCountTotalTables = 0;

      foreach (projectsettingsEntityexportExportentity exp in
        new SortedEntities(mProjectSettings.entityexport.exportentities, true).Entity) {

        if (!exp.exportdata || exp.exportdrop) continue;

        dbdefinitionEntity entity = DBServerHelper.GetEntity(exp.entity, mDBDefinition);
        if (entity == null) continue;

        if (pSmartRef) {
          ADOX.Table tbl = catLocal.Tables[exp.entity];
          foreach (ADOX.Key fkey in tbl.Keys) {

            //Check only foreign keys referring to the same table
            if (fkey.Type != ADOX.KeyTypeEnum.adKeyForeign) continue;
            if (!fkey.RelatedTable.ToLower().Equals(exp.entity.ToLower())) continue;

            aDroppedKeys.Add(new DroppedKeyInfo(fkey));
          }
          foreach (DroppedKeyInfo key in aDroppedKeys) {
            tbl.Keys.Delete(key.Name);
            WriteLog("Constraint (foreign key) " + key.Name + " deleted.");
          }
        }

        int intTotalTable = GetRecordCount(entity.name, this.ServerConnection);

        //INSERT-Statements
        IDbCommand com = this.ServerConnection.CreateCommand();
        com.CommandType = CommandType.Text;
        com.CommandText = "SELECT " + GetFields(entity, false) + " FROM " + entity.name;

        IDataReader dr = com.ExecuteReader();
        IDbCommand comInsert = GetInsertCommand(entity, conOle, false);

        intCount = 0;
        while (dr.Read()) {
          if (mfCancel) break;

          //TODO: binärdaten
          SetInsertCommandParams(comInsert, dr, entity, false);
          comInsert.ExecuteNonQuery();

          intCount++;
          intCountTotalRecords++;

          if (intCount % 100 == 0)
            WriteLog("Table " + entity.name + ": inserted " + intCount + " records (" + RecordCompletionPercent(intCount, intTotalTable) + ").");
        }
        dr.Close();

        if (!mfCancel) {
          if (intCount == 1) {
            WriteLog("Table " + entity.name + ": inserted 1 record.");
            intCountTotalTables++;
          }
          else if (intCount > 0) {
            WriteLog("Table " + entity.name + ": inserted " + intCount + " records (" + RecordCompletionPercent(intCount, intTotalTable) + ").");
            intCountTotalTables++;
          }

          if (pSmartRef && aDroppedKeys.Count > 0) {
            ADOX.Table tbl = catLocal.Tables[exp.entity];
            foreach (DroppedKeyInfo key in aDroppedKeys) {
              key.AppendToTableADOX(tbl, pconLocal);
              WriteLog("Constraint (foreign key) " + key.Name + " restored (" + RecordCompletionPercent(intCount, intTotalTable) + ").");
            }
          }
          aDroppedKeys.Clear();
        }
        else {
          break;
        }
      }

      conOle.Close();

      long lngSecs = DateTime.Now.Subtract(datStart).Seconds;

      string strMsg = "Data transfer complete: ";
      if (mfCancel) {
        strMsg = "Data transfer aborted by user: ";
      }
      WriteLog(strMsg + "imported " + intCountTotalRecords + " records from "
        + intCountTotalTables + " tables.");
      WriteLog("Transfer time: " + lngSecs / 60 + ":" + (lngSecs % 60).ToString("00") + " min, "
        + (intCountTotalRecords / (((double)lngSecs) / 60)).ToString("#,##0") + " records/min");
    }

    private void SimpleForeignKeyCheck(IDbConnection pCon, string pEntity, bool pfOn) {
      IDbCommand com = pCon.CreateCommand();
      com.CommandType = CommandType.Text;
      if (pfOn) {
        com.CommandText = mDbPlatformDef.foreignkeycheckson.Replace("{entity}", pEntity);
      }
      else {
        com.CommandText = mDbPlatformDef.foreignkeychecksoff.Replace("{entity}", pEntity);
      }
      com.ExecuteNonQuery();

      string strOn = "ON";
      if (!pfOn) {
        strOn = "OFF";
      }
      WriteLog("Foreign key checks are turned " + strOn + " for table " + pEntity + ".");
    }

    private class DroppedKeyInfo {
      private string mKeyName;
      private string mRelatedTable;
      private string mColumns;
      private ArrayList maColumns;
      private string mColumnsRelated;
      private ArrayList maColumnsRelated;
      private ADOX.Key mADOXKey = null;
      private ADOX.RuleEnum mUpdateRule;
      private ADOX.RuleEnum mDeleteRule;

      public DroppedKeyInfo (dbdefinitionEntityKeysForeignkey pKey) {
        mKeyName = pKey.name;
        mRelatedTable = pKey.foreignentity;
        mColumns = pKey.attributeref.attribute;
        mColumnsRelated = pKey.attributeref.foreignattribute;
      }

      public DroppedKeyInfo(ADOX.Key pKey) {
        maColumns=new ArrayList();
        maColumnsRelated=new ArrayList();

        mUpdateRule = pKey.UpdateRule;
        mDeleteRule = pKey.DeleteRule;

        mADOXKey=pKey;
        mKeyName=pKey.Name;
        mRelatedTable = pKey.RelatedTable;

        StringBuilder strCol = new StringBuilder();
        StringBuilder strColRel = new StringBuilder();
        foreach (ADOX.Column col in pKey.Columns) {
          maColumns.Add(col.Name);
          maColumnsRelated.Add(col.RelatedColumn);

          strCol.Append(col.Name); strCol.Append(",");
          strColRel.Append(col.RelatedColumn); strColRel.Append(",");
        }

        mColumns="";
        mColumnsRelated="";
        if (strCol.Length != 0) {
          mColumns = strCol.ToString(0, strCol.Length - 1);
        }
        if (strColRel.Length != 0) {
          mColumnsRelated = strColRel.ToString(0, strColRel.Length - 1);
        }
      }

      public void AppendToTableADOX(ADOX.Table pTbl, ADODB.Connection pConLocal) {

        /*
        creating the foreign key constraints using ADOX does not work in all cases and is disabled:
        - restoring casacing update/delete constraints cannot be specifed
        - multiple column contrains do not work
        both problems work with ADOX, but not with ADOX over .NET (problem with append method and optional parameters)

        ADOX.Key key = new ADOX.KeyClass();
        key.Name = mKeyName;
        key.RelatedTable = mRelatedTable;
        key.Type = ADOX.KeyTypeEnum.adKeyForeign;
        key.UpdateRule = mUpdateRule;
        key.DeleteRule = mDeleteRule;


        for (int i=0;i<maColumns.Count;i++) {
          ADOX.Column col = new ADOX.Column();
          col.Name = (string)maColumns[i];
          col.RelatedColumn = (string)maColumnsRelated[i];
          key.Columns.Append(col, 0, 0);
        }

        // Due to problems with the Keys.append method only foreign keys with a single column are supported
        if (key.Columns.Count > 1) {
          throw new Exception("Foreign keys with multiple columns are not supported, unfortunately.");
        }

        pTbl.Keys.Append(key.Name, key.Type, key.Columns[0], key.RelatedTable, key.Columns[0].RelatedColumn);
        */

        // instead restore constraint using access SQL DDL
        StringBuilder sql = new StringBuilder();
        sql.Append("ALTER TABLE " + pTbl.Name + " "
          + "ADD CONSTRAINT " + mKeyName + " "
          + "FOREIGN KEY (");
        for (int i = 0; i < maColumns.Count; i++) {
          sql.Append(maColumns[i]);
          if (i < maColumns.Count - 1) {
            sql.Append(", ");
          }
        }
        sql.Append(") REFERENCES " + mRelatedTable + " (");
        for (int i = 0; i < maColumnsRelated.Count; i++) {
          sql.Append(maColumnsRelated[i]);
          if (i < maColumnsRelated.Count - 1) {
            sql.Append(", ");
          }
        }
        sql.Append(")");
        if (mUpdateRule == ADOX.RuleEnum.adRICascade) {
          sql.Append(" ON UPDATE CASCADE");
        }
        if (mDeleteRule == ADOX.RuleEnum.adRICascade) {
          sql.Append(" ON DELETE CASCADE");
        }

        object recs;
        pConLocal.Execute(sql.ToString(), out recs, 0);
      }

      public ADOX.RuleEnum UpdateRule {
        get { return mUpdateRule; }
      }

      public ADOX.RuleEnum DeleteRule {
        get { return mDeleteRule; }
      }

      public ADOX.Key KeyADOX {
        get { return mADOXKey; }
      }

      public string Name {
        get {return mKeyName; }
      }

      public ArrayList ArrayColumns {
        get {return maColumns;}
      }

      public ArrayList ArrayColumnsRelated {
        get {return maColumnsRelated;}
      }

      public string Columns {
        get {return mColumns;}
      }

      public string ColumnsRelated {
        get {return mColumnsRelated;}
      }

      public string RelatedTable {
        get {return mRelatedTable;}
      }
    }

    public class TransferDataMsgEventArgs : System.EventArgs {
      string mMsg;

      internal TransferDataMsgEventArgs(string pMsg) {
        mMsg=pMsg;
      }

      public string Message {
        get { return mMsg; }
      }
    }

  }
}
