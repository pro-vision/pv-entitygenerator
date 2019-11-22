using System;
using System.Windows.Forms;
using System.Threading;
using System.Xml;
using PVFramework;
using PVEntityGenerator.Util;

namespace PVEntityGenerator {

  class App {

    public static bool DebugMode = false;

    // central configuration for writing XML files - with unix-style new-line chars
    public static readonly XmlWriterSettings XML_WRITER_SETTINGS = new XmlWriterSettings() {
      Encoding = System.Text.Encoding.UTF8,
      Indent = true,
      NewLineChars = "\n"
    };

    [STAThread]
    static void Main(string[] pArgs) {
      Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);

      CommandLineParser cli = new CommandLineParser(pArgs);

      if (cli["?"] != null) {
        MessageBox.Show("Syntax: PVEntityGenerator.exe [<Path to MDB/XML File>] [<Command line parameters>]\n"
          + "\n"
          + "Available command line parameters:\n"
          + "  /exportdata - Export to DB Server\n"
          + "  /importdata - Import from DB Server\n"
          + "  /exportscript <filename> - Export to SQL Script\n"
          + "  /createentityscript <filename> - Generate SQL Script for Entity Creation\n"
          + "  /patchentityscript <filename> - Generate SQL Script for Entity Patch\n"
          + "  /generateentityfiles - Generate Entity code files (to predefined paths)\n"
          + "  /dbplatform <SQLServer|Oracle|MySQL|PostgreSQL> - Switch database platform\n"
          + "  /debug - Launch in debug mode\n"
          + "  /? - Show command line help",
          "PVEntityGenerator", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }

      if (cli["debug"] != null) {
        App.DebugMode = true;
      }

      StartupOptions startupOptions = new StartupOptions();
      if (pArgs.Length >= 1 && (pArgs[0].ToLower().EndsWith(".mdb")
          || pArgs[0].ToLower().EndsWith(".accdb") || pArgs[0].ToLower().EndsWith(".xml"))) {
        startupOptions.DBDefinitionFilename = pArgs[0];
      }
      startupOptions.ExportData = (cli["exportdata"] != null);
      startupOptions.ImportData = (cli["importdata"] != null);
      startupOptions.ExportScript = (cli["exportscript"] != null);
      startupOptions.ExportScriptFilename = cli["exportscript"];
      startupOptions.CreateEntityScript = (cli["createentityscript"] != null);
      startupOptions.CreateEntityScriptFilename = cli["createentityscript"];
      startupOptions.PatchEntityScript = (cli["patchentityscript"] != null);
      startupOptions.PatchEntityScriptFilename = cli["patchentityscript"];
      startupOptions.GenerateEntityFiles = (cli["generateentityfiles"] != null);
      startupOptions.DBPlatformName = cli["dbplatform"];

      Application.Run(new frmMain(startupOptions));
    }

    private static void Application_ThreadException(object pSender, ThreadExceptionEventArgs pArgs) {
      if (pArgs.Exception is PVExceptionBusiness) {
        MessageBox.Show(pArgs.Exception.Message, "Warnung", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
      else {
        MessageBox.Show(pArgs.Exception.Message + "\n\n" + pArgs.Exception.StackTrace, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

  }

}
