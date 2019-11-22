using System;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;
using System.Collections;
using System.IO;
using System.Windows.Forms;
using System.Text;
using PVEntityGenerator.Util;
using PVEntityGenerator.XMLSchema;
using PVFramework;

namespace PVEntityGenerator {

  public class FileGenerator {

    public const string GLOBAL_ITEMS = "(Global items)";

    private string mConfigDir = null;
    private pventitygeneratorconfig mConfig = null;
    private platformdefinition mPlatformDef = null;
    private Hashtable mhashDbPlatformDef = null;
    private dbdefinition mDbDefinition = null;
    private projectsettings mProjectSettings = null;
    private projectsettingsPlatformsPlatform mCurrentPlatform = null;
    private IList mSelectedEntities = null;
    private XmlDocument mDataDocument = null;
    private string mRelRootPath = null;
    private StatusHandler mStatusHandler = null;

    private Encoding ENCODING_ISO_8859_1 = Encoding.GetEncoding("ISO-8859-1");
    private Encoding ENCODING_UTF_8 = new UTF8Encoding(false);

    public FileGenerator(string pConfigDir, pventitygeneratorconfig pConfig,
        platformdefinition pPlatformDef, Hashtable phashDbPlatformDef,
        dbdefinition pDbDefinition, projectsettings pProjectSettings,
        projectsettingsPlatformsPlatform pCurrentPlatform,
        IList pSelectedEntities, string pDbDefinitionFile,
        StatusHandler pStatusHandler) {

      mConfigDir = pConfigDir;
      mConfig = pConfig;
      mPlatformDef = pPlatformDef;
      mhashDbPlatformDef = phashDbPlatformDef;
      mDbDefinition = pDbDefinition;
      mProjectSettings = pProjectSettings;
      mCurrentPlatform = pCurrentPlatform;
      mSelectedEntities = pSelectedEntities;
      mStatusHandler = pStatusHandler;

      // Combined XML Data Document generieren
      pventitygeneratordata data = new pventitygeneratordata();
      data.dbdefinition = mDbDefinition;
      data.projectsettings = mProjectSettings;
      data.generatedon = DateTime.Now;

      XmlSerializer serializer = new XmlSerializer(typeof(pventitygeneratordata));
      StringWriter writer = new StringWriter();
      serializer.Serialize(writer, data);

      mDataDocument = new XmlDocument();
      mDataDocument.LoadXml(writer.ToString());
      writer.Close();

      // Path to DB Definition file
      int intPos = pDbDefinitionFile.LastIndexOf("\\");
      if (intPos >= 0) {
        mRelRootPath = pDbDefinitionFile.Substring(0, intPos);
        Environment.CurrentDirectory = mRelRootPath;
      }
      else {
        mRelRootPath = Environment.CurrentDirectory;
      }

      // in debug mode: save combined xml to filesystem
      if (App.DebugMode) {
        mDataDocument.Save(mRelRootPath + "\\debug-data-document.xml");
      }

    }

    private string GetParamValue(parameterdefinitionsParameterdefinition[] pParams, string pParamName) {
      foreach (parameterdefinitionsParameterdefinition param in pParams) {
        if (param.name.Equals(pParamName) && param.Value != null)
          return param.Value;
      }

      return "";
    }

    public void GenerateFile_Documentation() {
      SaveFileDialog sfd = new SaveFileDialog();
      sfd.DefaultExt = "html";
      sfd.Filter = "HTML documents (*.html)|*.html|All files (*.*)|*.*";
      sfd.InitialDirectory = mRelRootPath;
      sfd.CheckPathExists = true;
      sfd.CheckFileExists = false;
      sfd.OverwritePrompt = true;
      if (sfd.ShowDialog()!=DialogResult.OK) {
        return;
      }
      String destFilename = sfd.FileName;

      // Remove existing file if generation result is empty
      if (File.Exists(destFilename)) {
        File.Delete(destFilename);
      }

      // generate and write html
      StreamWriter writer = new StreamWriter(destFilename, false, ENCODING_UTF_8);
      XslCompiledTransform transform = GetTransform(mConfigDir + "\\schema\\databasemodel-doc.xsl");
      transform.Transform(mDataDocument, null, writer);
      writer.Flush();
      writer.Close();
    }

    public void GenerateFiles_EntityGeneration() {
      string strPlatformDefPath = mConfigDir;
      foreach (pventitygeneratorconfigPlatform p in mConfig.platforms) {
        if (p.name.Equals(mPlatformDef.name)) {
          if (p.definitiondir!=null && p.definitiondir.Length!=0) {
            strPlatformDefPath += p.definitiondir;
          }
          break;
        }
      }
      if (strPlatformDefPath.Length!=0) {
        strPlatformDefPath += "\\";
      }

      int intSelectedEntityCount = mSelectedEntities.Count;
      if (mSelectedEntities.Contains(GLOBAL_ITEMS)) {
        intSelectedEntityCount--;
      }

      // Gesamtzahl der Schritte ermitteln
      int intStepsTotal = 0;
      if (mPlatformDef.entitygeneration!=null && mPlatformDef.entitygeneration.generateentity!=null
          && mPlatformDef.entitygeneration.generateentity.generatefile!=null) {
        intStepsTotal += mPlatformDef.entitygeneration.generateentity.generatefile.Length * intSelectedEntityCount;
      }
      if (mSelectedEntities.Contains(GLOBAL_ITEMS)) {
        if (mPlatformDef.entitygeneration!=null && mPlatformDef.entitygeneration.generateglobal!=null
            && mPlatformDef.entitygeneration.generateglobal.generatefile!=null) {
          intStepsTotal += mPlatformDef.entitygeneration.generateglobal.generatefile.Length;
        }
      }
      if (mProjectSettings.dbplatforms!=null && mProjectSettings.dbplatforms.dbplatform!=null) {
        foreach (projectsettingsDbplatformsDbplatform dbplatform in mProjectSettings.dbplatforms.dbplatform) {
          dbplatformdefinition dbplatformdef = (dbplatformdefinition)mhashDbPlatformDef[dbplatform.name];
          if (dbplatformdef !=null
              && dbplatformdef.scriptgeneration != null
              && dbplatformdef.scriptgeneration.generateglobal != null
              && dbplatformdef.scriptgeneration.generateglobal.generatefile!=null) {
            foreach (generatefile file in dbplatformdef.scriptgeneration.generateglobal.generatefile) {
              if (!file.name.Equals("entity-create") && !file.name.Equals("entity-patch")) {
                intStepsTotal++;
              }
            }
          }

        }
      }
      mStatusHandler.InitStatus("Generating files...", intStepsTotal);
      int intStep = 0;

      // Generate entity files
      if (mPlatformDef.entitygeneration!=null && mPlatformDef.entitygeneration.generateentity!=null
          && mPlatformDef.entitygeneration.generateentity.generatefile!=null) {

        foreach (generatefile file in mPlatformDef.entitygeneration.generateentity.generatefile) {
          XslCompiledTransform transform = GetTransform(strPlatformDefPath + file.transform);
          string strPath = ExpandPlatformParameters(file.path);
          if (strPath.Length!=0 && !strPath.EndsWith("\\")) {
            strPath = strPath += "\\";
          }

          foreach (dbdefinitionEntity entity in mDbDefinition.entities) {
            if (mSelectedEntities.Contains(entity.name)) {
              string strFilename = ExpandPlatformParameters(strPath + file.filename, entity.name);

              GenerateFile(transform, strFilename, mPlatformDef.parameterdefinitions, entity.name, file.encoding);
              mStatusHandler.SetStatus("Generated: " + strFilename, ++intStep);
            }
          }

        }
      }

      // Generate global
      if (mSelectedEntities.Contains(GLOBAL_ITEMS)) {

        // Generate global files
        if (mPlatformDef.entitygeneration!=null && mPlatformDef.entitygeneration.generateglobal!=null
            && mPlatformDef.entitygeneration.generateglobal.generatefile!=null) {
          foreach (generatefile file in mPlatformDef.entitygeneration.generateglobal.generatefile) {
            XslCompiledTransform transform = GetTransform(strPlatformDefPath + file.transform);
            string strPath = ExpandPlatformParameters(file.path);
            if (strPath.Length!=0 && !strPath.EndsWith("\\")) {
              strPath = strPath += "\\";
            }
            string strFilename = ExpandPlatformParameters(strPath + file.filename);

            GenerateFile(transform, strFilename,mPlatformDef.parameterdefinitions, file.encoding);
            mStatusHandler.SetStatus("Generated: " + strFilename, ++intStep);
          }
        }

        // Generate global scripts
        if (mProjectSettings.dbplatforms!=null && mProjectSettings.dbplatforms.dbplatform!=null) {
          foreach (projectsettingsDbplatformsDbplatform dbplatform in mProjectSettings.dbplatforms.dbplatform) {
            dbplatformdefinition dbplatformdef = (dbplatformdefinition)mhashDbPlatformDef[dbplatform.name];
            if (dbplatformdef != null && dbplatform != null) {

              string strDbPlatformDefPath = mConfigDir;
              foreach (pventitygeneratorconfigDbplatform p in mConfig.dbplatforms) {
                if (p.name.Equals(dbplatformdef.name)) {
                  if (p.definitiondir != null && p.definitiondir.Length != 0) {
                    strDbPlatformDefPath += p.definitiondir;
                  }
                  break;
                }
              }
              if (strDbPlatformDefPath.Length != 0) {
                strDbPlatformDefPath += "\\";
              }

              if (dbplatformdef.scriptgeneration != null && dbplatformdef.scriptgeneration.generateglobal != null
                  && dbplatformdef.scriptgeneration.generateglobal.generatefile != null) {

                foreach (generatefile file in dbplatformdef.scriptgeneration.generateglobal.generatefile) {
                  if (!file.name.Equals("entity-create") && !file.name.Equals("entity-patch")) {
                    XslCompiledTransform transform = GetTransform(strDbPlatformDefPath + file.transform);
                    string strPath = ExpandDbPlatformParameters(file.path, dbplatformdef, dbplatform);
                    if (strPath.Length != 0 && !strPath.EndsWith("\\")) {
                      strPath = strPath += "\\";
                    }
                    string strFilename = ExpandDbPlatformParameters(strPath + file.filename, dbplatformdef, dbplatform);

                    GenerateFile(transform, strFilename, dbplatformdef.parameterdefinitions, file.encoding);
                    mStatusHandler.SetStatus("Generated: " + strFilename, ++intStep);
                  }
                }
              }

            }
          }
        }

      }
      mStatusHandler.ClearStatus("Generated " + intStep + " file(s).");
    }

    public string GenerateScript_CreateEntities(projectsettingsDbplatformsDbplatform pCurrentDbPlatform) {
      return GenerateScript(pCurrentDbPlatform, "entity-create");
    }

    public string GenerateScript_PatchEntities(projectsettingsDbplatformsDbplatform pCurrentDbPlatform) {
      return GenerateScript(pCurrentDbPlatform, "entity-patch");
    }

    private string GenerateScript(projectsettingsDbplatformsDbplatform pCurrentDbPlatform, string pFile) {
      dbplatformdefinition dbplatformdef = (dbplatformdefinition)mhashDbPlatformDef[pCurrentDbPlatform.name];

      string strDbPlatformDefPath = mConfigDir;
      foreach (pventitygeneratorconfigDbplatform p in mConfig.dbplatforms) {
        if (p.name.Equals(dbplatformdef.name)) {
          if (p.definitiondir!=null && p.definitiondir.Length!=0) {
            strDbPlatformDefPath += p.definitiondir;
          }
          break;
        }
      }
      if (strDbPlatformDefPath.Length!=0) {
        strDbPlatformDefPath += "\\";
      }

      if (dbplatformdef.scriptgeneration!=null && dbplatformdef.scriptgeneration.generateglobal!=null
        && dbplatformdef.scriptgeneration.generateglobal.generatefile!=null) {

        foreach (generatefile file in dbplatformdef.scriptgeneration.generateglobal.generatefile) {
          if (file.name.Equals(pFile)) {
            XslCompiledTransform transform = GetTransform(strDbPlatformDefPath + file.transform);
            string strPath = ExpandDbPlatformParameters(file.path, dbplatformdef, pCurrentDbPlatform);
            if (strPath.Length!=0 && !strPath.EndsWith("\\")) {
              strPath = strPath += "\\";
            }

            return DoTransform(transform, null, generatefileEncoding.ISO88591);
          }
        }
      }

      throw new Exception("No transformation defined for '" + pFile + "' in db platform '" + pCurrentDbPlatform.name + "'.");
    }

    private void GenerateFile(XslCompiledTransform pTransform, string pDestFilename,parameterdefinitionsParameterdefinition[] paParams, generatefileEncoding pEncoding) {
      GenerateFile(pTransform, pDestFilename, paParams, null, pEncoding);
    }

    private string StripHeader(string pText, int pHeaderLines) {
      if (pText==null || pText.Length==0) return "";

      int intPos=0; bool fWorked=false;
      for (int i=0;i<pHeaderLines;i++) {
        fWorked=true;
        intPos=pText.IndexOf('\n',intPos);
        if (intPos < 0) {
          break;
        }
      }

      if (!fWorked) intPos=-1;

      if (intPos==-1) return pText;
      return pText.Substring(intPos+1,pText.Length-intPos-1);
    }

    private string CusomClassExt(string pText, string pNewExt,
      string pCustomClassExtStart, string pCustomClassExtEnd, bool pfRead) {
      if (!pfRead && (pNewExt==null || pNewExt.Length<=0)) return pText;

      int intPos=pText.IndexOf(pCustomClassExtStart);
      if (intPos==-1) {
        if (pfRead)
          return "";
        else
          return pText;
      }

      intPos+=pCustomClassExtStart.Length;

      int intPos2=pText.IndexOf(pCustomClassExtEnd,intPos);
      if (intPos2==-1) {
        if (pfRead)
          return "";
        else
          return pText;
      }

      if (pfRead)
        return pText.Substring(intPos,intPos2-intPos);
      else
        return pText.Substring(0,intPos) + pNewExt + pText.Substring(intPos2,pText.Length-intPos2);
    }

    private void WriteFile (string pText, string pDestFilename, generatefileEncoding pEncoding) {
      FileStream fs = File.Create(pDestFilename);
      StreamWriter sw;
      if (pEncoding == generatefileEncoding.UTF8) {
        sw = new StreamWriter(fs, ENCODING_UTF_8);
      }
      else {
        sw = new StreamWriter(fs, ENCODING_ISO_8859_1);
      }
      sw.Write(pText);
      sw.Flush();
      sw.Close();
      fs.Close();
    }

    private void GenerateFile(XslCompiledTransform pTransform, string pDestFilename,
      parameterdefinitionsParameterdefinition[] paParams, string pEntity, generatefileEncoding pEncoding) {

      try {
        string result = DoTransform(pTransform, pEntity, pEncoding).Trim();
        
        // remove BOM when checking for empty result string
        bool resultEmpty = string.IsNullOrWhiteSpace(result);

        // special empty check for generated xml files containing only xml declaration
        if (!resultEmpty && pDestFilename.EndsWith(".xml")) {
          String encoding = "iso-8859-1";
          if (pEncoding == generatefileEncoding.UTF8) {
            encoding = "utf-8";
          }
          resultEmpty = result.Equals("<?xml version=\"1.0\" encoding=\"" + encoding + "\"?>");
        }

        // Remove existing file if generation result is empty
        if (resultEmpty) {
          if (File.Exists(pDestFilename)) {
            File.Delete(pDestFilename);
          }
          return;
        }

        // If no existing file write result to file
        if (!File.Exists(pDestFilename)) {
          WriteFile(result,pDestFilename, pEncoding);
          return;
        }

        // otherwise load existing file and merge existing custom class extension sections
        else {
          string strCustomExtStart = "";
          string strCustomExtEnd = "";
          int intHeaderLines = 0;
          string strNewContent = result;

          // Get parameter defintions for custom class extensions
          if (paParams!=null) {
            strCustomExtStart = GetParamValue(paParams, "custom-class-extension-tag-start");
            strCustomExtEnd = GetParamValue(paParams, "custom-class-extension-tag-end");
            string strFileHeader = GetParamValue(paParams, "file-header");

            // Prepare line endings of parameters
            strCustomExtStart = strCustomExtStart.Replace("\r\n", "\n").Replace('\r','\n');
            strCustomExtEnd = strCustomExtEnd.Replace("\r\n", "\n").Replace('\r','\n');
            strFileHeader = strFileHeader.Replace("\r\n", "\n").Replace('\r','\n');

            foreach (char ch in strFileHeader) {
              if (ch=='\n') intHeaderLines++;
            }
          }

          // Check for cusotm class extensions
          if (strCustomExtStart.Length!=0 && strCustomExtEnd.Length!=0) {
            char[] aTrim = new char[2] {'\n','\r'};

            // Read existing file
            string strOldContent = ReadFile(pDestFilename, pEncoding);

            // Prepare existing and new file content
            strOldContent = strOldContent.Trim().Trim(aTrim).Replace("\r\n", "\n").Replace('\r','\n');
            strNewContent = strNewContent.Trim().Trim(aTrim).Replace("\r\n", "\n").Replace('\r','\n');

            // Extract Custom class extension from old content
            string strCustom = CusomClassExt(strOldContent, null, strCustomExtStart, strCustomExtEnd, true);

            // Merge the custom extensions with the new content
            strNewContent = CusomClassExt(strNewContent, strCustom, strCustomExtStart, strCustomExtEnd, false);

            if (StripHeader(strOldContent, intHeaderLines+1).Equals(StripHeader(strNewContent, intHeaderLines+1))) {
              // Files are identical, do nothing
              return;
            }

            // Replace line endings with platform specific line endings
            strNewContent = strNewContent.Replace("\n", "\r\n");
          }

          // Overwrite file
          File.Delete(pDestFilename);
          WriteFile(strNewContent, pDestFilename, pEncoding);
        }

      }
      catch (Exception ex) {
        throw new PVException("Error generating File '" + pDestFilename + "' - \n" + ex.Message);
      }
    }

    private string ReadFile(string pFilename, generatefileEncoding pEncoding) {

      FileInfo info=new FileInfo(pFilename);

      FileStream fs=File.OpenRead(pFilename);
      BinaryReader rd= new BinaryReader(fs);

      byte[] bytes=new byte[info.Length];
      rd.Read(bytes,0,(int)info.Length);
      rd.Close();
      fs.Close();

      if (pEncoding == generatefileEncoding.UTF8) {
        return ENCODING_UTF_8.GetString(bytes, 0, bytes.Length).Trim();
      }
      else {
        return ENCODING_ISO_8859_1.GetString(bytes, 0, bytes.Length).Trim();
      }
    }

    private string DoTransform(XslCompiledTransform pTransform, string pEntity, generatefileEncoding pEncoding) {
      XsltArgumentList arguments = new XsltArgumentList();
      if (pEntity!=null) {
        arguments.AddParam("entity", "", pEntity);
      }

      MemoryStream memoryStream = new MemoryStream();
      StreamWriter writer;
      if (pEncoding == generatefileEncoding.UTF8) {
        writer = new StreamWriter(memoryStream, ENCODING_UTF_8);
      }
      else {
        writer = new StreamWriter(memoryStream, ENCODING_ISO_8859_1);
      }
 
      pTransform.Transform(mDataDocument, arguments, writer);

      string result;
      byte[] resultData = memoryStream.GetBuffer();
      if (pEncoding == generatefileEncoding.UTF8) {
        result = ENCODING_UTF_8.GetString(resultData, 0, (int)memoryStream.Length);
      }
      else {
        result = ENCODING_ISO_8859_1.GetString(resultData, 0, (int)memoryStream.Length);
      }

      memoryStream.Close();

      return result.Trim();
    }

    private XslCompiledTransform GetTransform(string pStylesheet) {
      XslCompiledTransform transform = new XslCompiledTransform();
      try {
        transform.Load(pStylesheet);
      }
      catch (XsltCompileException ex) {
        throw new PVException("Error compiling XSLT stylesheet '" + pStylesheet + "'.", ex.InnerException);
      }
      return transform;
    }

    private string ExpandGlobalParameters(string pValue) {
      string strValue = pValue;

      if (mConfig.projectdefinition!=null && mConfig.projectdefinition.parameterdefinitions!=null
        && mProjectSettings.parameters!=null) {
        foreach (parameterdefinitionsParameterdefinition paramdef in mConfig.projectdefinition.parameterdefinitions) {
          strValue = strValue.Replace("{$" + paramdef.name + "}", ParameterHelper.GetParameter(paramdef.name, paramdef, mProjectSettings.parameters));
        }
      }

      return strValue;
    }

    private string ExpandPlatformParameters(string pValue) {
      string strValue = pValue;

      if (mPlatformDef.parameterdefinitions!=null && mCurrentPlatform.parameters!=null) {
        foreach (parameterdefinitionsParameterdefinition paramdef in mPlatformDef.parameterdefinitions) {
          strValue = strValue.Replace("{$" + paramdef.name + "}", ParameterHelper.GetParameter(paramdef.name, paramdef, mCurrentPlatform.parameters));
        }
      }

      return ExpandGlobalParameters(strValue);
    }

    private string ExpandPlatformParameters(string pValue, string pEntity) {
      string strValue = pValue;

      if (pEntity!=null) {
        strValue = strValue.Replace("{entity}", pEntity);

        if (mPlatformDef.entitygeneration!=null
          && mPlatformDef.entitygeneration.generateentity!=null
          && mPlatformDef.entitygeneration.generateentity.parameterdefinitions!=null) {

          type_generationGenerateentity entity = null;
          if (mCurrentPlatform.entitygeneration!=null && mCurrentPlatform.entitygeneration.generateentity!=null) {
            foreach (type_generationGenerateentity e in mCurrentPlatform.entitygeneration.generateentity) {
              if (e.entity.Equals(pEntity)) {
                entity = e;
              }
            }
          }

          if (entity!=null && entity.parameters!=null) {
            foreach (parameterdefinitionsParameterdefinition paramdef in mPlatformDef.entitygeneration.generateentity.parameterdefinitions) {
              strValue = strValue.Replace("{$" + paramdef.name + "}", ParameterHelper.GetParameter(paramdef.name, paramdef, entity.parameters));
            }
          }

        }

        if (mConfig.entitygeneration!=null
          && mConfig.entitygeneration.generateentity!=null
          && mConfig.entitygeneration.generateentity.parameterdefinitions!=null) {

          type_generationGenerateentity entity = null;
          if (mProjectSettings.entitygeneration!=null && mProjectSettings.entitygeneration.generateentity!=null) {
            foreach (type_generationGenerateentity e in mProjectSettings.entitygeneration.generateentity) {
              if (e.entity.Equals(pEntity)) {
                entity = e;
              }
            }
          }

          if (entity!=null && entity.parameters!=null) {
            foreach (parameterdefinitionsParameterdefinition paramdef in mConfig.entitygeneration.generateentity.parameterdefinitions) {
              strValue = strValue.Replace("{$" + paramdef.name + "}", ParameterHelper.GetParameter(paramdef.name, paramdef, entity.parameters));
            }
          }

        }

      }

      return ExpandPlatformParameters(strValue);
    }

    private string ExpandDbPlatformParameters(string pValue,
      dbplatformdefinition pDbPlatformDef, projectsettingsDbplatformsDbplatform pDbPlatform) {
      string strValue = pValue;

      if (pDbPlatformDef.parameterdefinitions!=null && pDbPlatform.parameters!=null) {
        foreach (parameterdefinitionsParameterdefinition paramdef in pDbPlatformDef.parameterdefinitions) {
          strValue = strValue.Replace("{$" + paramdef.name + "}", ParameterHelper.GetParameter(paramdef.name, paramdef, pDbPlatform.parameters));
        }
      }

      return ExpandPlatformParameters(strValue, null);
    }

  }

}
