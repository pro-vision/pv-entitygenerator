using System;
using System.Collections.Generic;
using System.Text;

namespace PVEntityGenerator {

  public class StartupOptions {

    public string DBDefinitionFilename;
    public string DBPlatformName;

    public bool ExportData;
    public bool ImportData;
    public bool ExportScript;
    public string ExportScriptFilename;
    public bool CreateEntityScript;
    public string CreateEntityScriptFilename;
    public bool PatchEntityScript;
    public string PatchEntityScriptFilename;
    public bool GenerateEntityFiles;

    public bool HasCommandLineAction() {
      return this.ExportData
        || this.ImportData
        || this.ExportScript
        || this.CreateEntityScript
        || this.PatchEntityScript
        || this.GenerateEntityFiles;
    }

  }

}
