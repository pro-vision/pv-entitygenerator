using System;
using System.Windows.Forms;
using System.Data;
using System.Collections;
using System.Reflection;
using PVFramework;
using PVEntityGenerator.Util;
using PVEntityGenerator.XMLSchema;

namespace PVEntityGenerator {

  public class DBServerHelper {

    public static string getDatabaseOleDbConnectionString(String pFilename) {
      String connect = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pFilename;
      if (pFilename.ToLower().EndsWith(".accdb")) {
        connect = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + pFilename;
      }
      return connect;
    }

    public static IDbConnection GetConnection(Hashtable phashDBPlatform, projectsettings pProjectSettings) {

      String strDbPlatform = pProjectSettings.dbplatforms.selected;
      dbplatformdefinition dbplatformdef = (dbplatformdefinition)phashDBPlatform[strDbPlatform];
      if (dbplatformdef==null) {
        throw new PVException("No db platform definition for dbplatform '" + strDbPlatform + "' found.");
      }

      projectsettingsDbplatformsDbplatform dbplatform = null;
      foreach (projectsettingsDbplatformsDbplatform p in pProjectSettings.dbplatforms.dbplatform) {
        if (p.name.Equals(strDbPlatform)) {
          dbplatform = p;
          break;
        }
      }
      if (dbplatform==null) {
        throw new PVException("Db platform settings not found.");
      }

      return GetConnection(dbplatformdef, dbplatform);
    }

    public static IDbConnection GetConnection(dbplatformdefinition pDbPlatformDef, projectsettingsDbplatformsDbplatform pDbPlatform) {

      // Aktuellen provider+providerdef ermitteln
      projectsettingsDbplatformsDbplatformDbprovidersDbprovider provider = null;
      foreach (projectsettingsDbplatformsDbplatformDbprovidersDbprovider p in pDbPlatform.dbproviders.dbprovider) {
        if (p.name.Equals(pDbPlatform.dbproviders.selected)) {
          provider = p;
          break;
        }
      }
      if (provider==null) {
        throw new PVException("No or invalid db provider selected.");
      }
      dbplatformdefinitionDbproviderdefinition providerdef = null;
      foreach (dbplatformdefinitionDbproviderdefinition pd in pDbPlatformDef.dbproviderdefinitions) {
        if (pd.name.Equals(provider.name)) {
          providerdef = pd;
          break;
        }
      }
      if (providerdef==null) {
        throw new PVException("No provider definition found for provider '" + provider.name + "'.");
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

      Assembly asm = null;
      try {
        asm = Assembly.Load(providerdef.providerassembly);
      }
      catch (Exception ex) {
        try {
          asm = Assembly.Load(providerdef.providerassembly);
        }
        catch (Exception) {
          throw ex;
        }
      }
      IDbConnection con = (IDbConnection)asm.CreateInstance(providerdef.providerclass);
      con.ConnectionString = strConnect;
      con.Open();
      return con;
    }

    private static string GetDelimiter(dbplatformdefinition pDbPlatformDef,string pScript) {
      if (pDbPlatformDef.alternativescriptdelimiter==null || pDbPlatformDef.alternativescriptdelimiter.Length<=0)
        return pDbPlatformDef.scriptdelimiter;

      bool fInComment=false;
      bool fSimpleComment=false;
      bool fInString=false;

      //Nach alternativ-delimiter suchen
      for (int i=0;i<pScript.Length;i++) {
        char ch=pScript[i];

        if (fInComment) {
          if (fSimpleComment && ch=='\n') {
            fInComment=false;
            continue;
          }  else
            if (!fSimpleComment && ch=='*' && pScript[i+1]=='/') {
            i++;
            fInComment=false;
            continue;
          } else
            continue;
        } else {
          if (ch=='/' && pScript[i+1]=='*') {
            fInComment=true;
            fSimpleComment=false;
            i++;
            continue;
          } else
            if (ch=='-' && pScript[i+1]=='-') {
            fInComment=true;
            fSimpleComment=true;
            i++;
            continue;
          }
        }

        if (ch=='\'') {
          fInString=!fInString;
          continue;
        }

        int intLenDelim=pDbPlatformDef.alternativescriptdelimiter.Length;
        if (!fInComment && !fInString ) {
          if (i+intLenDelim>=pScript.Length)
            return pDbPlatformDef.scriptdelimiter;
          else
            if (pScript.Substring(i,intLenDelim).ToLower().
              Equals(pDbPlatformDef.alternativescriptdelimiter.ToLower())) {
            return pDbPlatformDef.alternativescriptdelimiter;
          }
        }
      }

      return pDbPlatformDef.scriptdelimiter;
    }

    public static void ExecuteScript(dbplatformdefinition pDbPlatformDef,
      projectsettingsDbplatformsDbplatform pDbPlatform, string pScript, StatusHandler pStatusHandler) {

      bool fInComment=false;
      bool fSimpleComment=false;
      bool fInString=false;
      int intStartPos=0;

      if (pScript==null || pScript.Length<=0) {
        pStatusHandler.InitStatus("Empty script.",0);
        return;
      }

      pScript=pScript.Replace("\r\n","\n");
      string strDelim=GetDelimiter(pDbPlatformDef,pScript);

      ArrayList aSQL=new ArrayList();

      for (int i=0;i<pScript.Length;i++) {
        char ch=pScript[i];

        if (fInComment) {
          if (fSimpleComment && ch=='\n') {
            fInComment=false;
            intStartPos=++i;
            continue;
          }  else
            if (!fSimpleComment && ch=='*' && (i+1)<pScript.Length && pScript[i+1]=='/' ) {
            fInComment=false;
            i++;
            intStartPos=++i;
            continue;
          } else
            continue;
        } else {
          if (ch=='/' && (i+1)<pScript.Length && pScript[i+1]=='*') {
            fInComment=true;
            fSimpleComment=false;
            continue;
          } else
            if (ch=='-' && (i+1)<pScript.Length && pScript[i+1]=='-') {
            fInComment=true;
            fSimpleComment=true;
            continue;
          }
        }

        if (ch=='\'') {
          fInString=!fInString;
          continue;
        }

        int intLenDelim=strDelim.Length;

        if (!fInComment && !fInString ) {
          if (i+intLenDelim>=pScript.Length) {
            string strSQL=pScript.Substring(intStartPos,pScript.Length-intStartPos-intLenDelim).
              Trim(new char[]{' ','\n'});
            if (strSQL!=null && strSQL.Length>0)
              aSQL.Add(strSQL);
            break;
          }
          else
            if (pScript.Substring(i,intLenDelim+1).ToLower().Equals(strDelim.ToLower()+"\n")) {
            string strSQL=pScript.Substring(intStartPos,i-intStartPos).Trim(new char[]{' ','\n'});
            if (strSQL!=null && strSQL.Length>0)
              aSQL.Add(strSQL);
            intStartPos=i+intLenDelim;
            i+=intLenDelim;
          }
        }
      }

      int intStep=0;
      pStatusHandler.InitStatus("Executing SQL...",aSQL.Count);

      IDbConnection con=GetConnection(pDbPlatformDef,pDbPlatform);

      bool fError=false;
      int intCounter=0;
      IDbCommand cmd=con.CreateCommand();
      cmd.CommandType=CommandType.Text;

      while (true) {
        if (intCounter>=aSQL.Count) break;

        pStatusHandler.SetStatus("Executing SQL...", ++intStep);

        string strSQL = (string)aSQL[intCounter];
        try {
          cmd.CommandText = strSQL;
          cmd.ExecuteNonQuery();

          intCounter++;
        }
        catch (Exception pex) {
          string strMsg = pex.Message + "\n\n";
          // append first 1000 chars of sql string to error message
          if (strSQL.Length > 1000) {
            strMsg += strSQL.Substring(0, 1000) + "...";
          }
          else {
            strMsg += strSQL;
          }
          // ask if user wants to retry, ignore and continue or cancel
          DialogResult res = pStatusHandler.OnError(strMsg);
          if (res==DialogResult.Ignore) {
            intCounter++;
          }
          else if (res!=DialogResult.Retry) {
            pStatusHandler.ClearStatus("Execution aborted with errors.");
            fError=true;
            break;
          }
        }
      }

      con.Close();
      if (!fError)
        pStatusHandler.ClearStatus("Execution completed sucessfully.");

    }

    public static ADODB.Connection GetMDBConnectionADODB(string pFilename) {
      ADODB.Connection con = new ADODB.ConnectionClass();
      try {
        con.Open(DBServerHelper.getDatabaseOleDbConnectionString(pFilename), "", "", 0);
        return con;
      }
      catch (Exception ex) {
        throw new Exception("Unable to open database file:\n" + ex.Message);
      }
    }

    public static System.Data.OleDb.OleDbConnection GetMDBConnectionOLEDB(string pFilename) {
      try {
        System.Data.OleDb.OleDbConnection con = new System.Data.OleDb.OleDbConnection
          (DBServerHelper.getDatabaseOleDbConnectionString(pFilename));

        con.Open();
        return con;
      }
      catch (Exception ex) {
        throw new Exception("Unable to open database file:\n" + ex.Message);
      }
    }

    public static dbdefinitionEntity GetEntity(string pName, dbdefinition pDBDefinition) {
      foreach (dbdefinitionEntity entity in pDBDefinition.entities) {
        if (entity.name.Equals(pName)) return entity;
      }
      return null;
    }

    public static string GetDBPlatformParam(projectsettingsDbplatformsDbplatform pDbPlatform,string pName) {
      foreach (parametersParameter param in pDbPlatform.parameters) {
        if (param.name.Equals(pName))
          return param.Value;
      }

      return "";
    }
  }

}
