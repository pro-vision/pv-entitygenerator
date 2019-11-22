using System;
using System.Data;
using System.Data.OleDb;
using System.Drawing.Design;
using System.Collections;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using PVFramework;
using PVFramework.Util;
using PVFramework.WinApp.Util;
using PVEntityGenerator.Controls;
using PVEntityGenerator.XMLSchema;
using PVEntityGenerator.Dialogs;
using PVEntityGenerator.Util;
using Microsoft.Win32;
using System.Text;

namespace PVEntityGenerator {

  public class frmMain : System.Windows.Forms.Form {
    private System.Windows.Forms.Label lblDBDefinition;
    private System.Windows.Forms.ComboBox cboDBDefinition;
    private System.Windows.Forms.Button cmdDBDefinitionBrowse;
    private System.Windows.Forms.StatusBarPanel sbarPanel;
    private System.Windows.Forms.ProgressBar pbar;

    private System.ComponentModel.Container components = null;
    private System.Windows.Forms.TextBox txtXML;

    private System.Windows.Forms.ComboBox cboExportDataProvider;
    private System.Windows.Forms.Label lblExportDataProvider;
    private System.Windows.Forms.ComboBox cboExportDBPlatform;
    private System.Windows.Forms.Label lblExportDBPlatform;
    private System.Windows.Forms.TabPage tabPageEntityGeneration;
    private System.Windows.Forms.TabPage tabPageProject;
    private System.Windows.Forms.TabPage tabPageSQLExport;
    private System.Windows.Forms.TabPage tabPageXML;
    private System.Windows.Forms.TabPage tabPageAttributes;
    private System.Windows.Forms.TabPage tabPageEnumeration;
    private System.Windows.Forms.TabControl tabMain;
    private System.Windows.Forms.TabControl tabEntityGeneration;
    private System.Windows.Forms.CheckedListBox lstSupportedDatabasePlatform;
    private System.Windows.Forms.Label lblDBPlatform;
    private System.Windows.Forms.ComboBox cboPlatform;
    private System.Windows.Forms.Label lblPlatform;

    private System.Windows.Forms.Label lblEntity;
    private System.Windows.Forms.Label lblEntityCaption;
    private System.Windows.Forms.Label lblEntity2;
    private System.Windows.Forms.Label lblEntityCaption2;
    private System.Windows.Forms.Label lblEntity3;
    private System.Windows.Forms.Label lblEntityCaption3;
    private System.Windows.Forms.TabPage tabPageExport;
    private System.Windows.Forms.TabPage tabPageExportEntityOptions;
    private System.Windows.Forms.TabPage tabPageExportEntityPatch;
    private PVPropertyGrid pgProject;
    private PVPropertyGrid pgEntiyGenerationEntityOptions;
    private PVPropertyGrid pgEntiyGenerationGlobalOptions;
    private PVPropertyGrid pgSQLExport;
    private System.Windows.Forms.Label lblDeleteConstraint;
    private System.Windows.Forms.Label lblCustomCreateMethod;
    private System.Windows.Forms.ListBox lstCustomCreateMethod;
    private System.Windows.Forms.Button cmdCustomFindMethodRemove;
    private System.Windows.Forms.Button cmdCustomFindMethodAdd;
    private System.Windows.Forms.Label lblCustomFindMethod;
    private System.Windows.Forms.ListBox lstCustomFindMethod;
    private System.Windows.Forms.Button cmdCustomCreateMethodRemove;
    private System.Windows.Forms.Button cmdCustomCreateMethodAdd;
    private System.Windows.Forms.Button cmdEntityGeneration;
    private System.Windows.Forms.Button cmdSaveProjectSettings;
    private System.Windows.Forms.StatusBar sbStatus;
    private System.Windows.Forms.GroupBox grpEntityGeneration;
    private System.Windows.Forms.GroupBox grpExportImportData;
    private System.Windows.Forms.RadioButton optTransferImport;
    private System.Windows.Forms.RadioButton optTransferExport;
    private System.Windows.Forms.Button cmdExportTransfer;
    private System.Windows.Forms.GroupBox grpGenerateScript;
    private System.Windows.Forms.Button cmdExportPatchScript;
    private System.Windows.Forms.Button cmdExportCreateScript;
    private System.Windows.Forms.TabControl tabExport;
    private System.Windows.Forms.Button cmdExportScriptExecute;
    private System.Windows.Forms.Button cmdExportScriptOpen;
    private System.Windows.Forms.Button cmdExportScriptSave;
    private System.Windows.Forms.Button cmdExportScriptClear;

    private Hashtable mhashPlatform = null;
    private Hashtable mhashDBPlatform = null;

    private StatusHandler mStatusHandler = null;
    private dbdefinition mDBDefinition = null;
    private XmlDocument mDBDefinitionDocument = null;
    private string mDBDefinitionFilename = null;
    private pventitygeneratorconfig mConfig = null;
    private string mConfigDir = null;

    private projectsettings mProjectSettings = null;
    private string mProjectSettingsFilename = null;
    private XmlSerializer mProjectSettingsSerializer = new XmlSerializer(typeof(projectsettings));
    private XmlSerializer mDBDefinitionSerializer = new XmlSerializer(typeof(dbdefinition));

    private bool mfDirty = false;
    private bool mfDataTransferPossible = false;
    private bool mfDuringLoad = false;
    private bool mfEntityEnumeration = false;
    private bool mfAttributeDetails = false;

    private projectsettingsPlatformsPlatform mProjectSettings_CurrentPlatform = null;
    private projectsettingsDbplatformsDbplatform mProjectSettings_CurrentDbPlatform = null;
    private projectsettingsDbplatformsDbplatformDbprovidersDbprovider mProjectSettings_CurrentDbProvider = null;

    private string mCurrentEntityName = null;
    private type_generationGenerateentity mCurrentEntity = null;
    private type_generationGenerateentity mCurrentEntityPlatform = null;

    private System.Windows.Forms.DataGrid dgEntityAttribute;
    private System.Windows.Forms.DataGrid dgEntityEnumeration;
    private System.Windows.Forms.DataGrid dgExportEntityOptions;
    private System.Windows.Forms.Panel pnlEntitySelector;
    private System.Windows.Forms.Button cmdDeselectAll;
    private System.Windows.Forms.Button cmdSelectAll;
    private System.Windows.Forms.CheckedListBox lstEntity;
    private System.Windows.Forms.DataGrid dgDeleteConstraint;
    private System.Windows.Forms.TabPage tabPageGenerationOptions;
    private System.Windows.Forms.TabPage tabPageGlobalOptions;
    private System.Windows.Forms.Button cmdResetEnumeration;
    private System.Windows.Forms.Button cmdRefreshAllEnumerations;
    private System.Windows.Forms.CheckBox chkExportToScript;
    private System.Windows.Forms.RichTextBox txtExportSQL;
    private System.Windows.Forms.ListBox lstPatchEntity;
    private System.Windows.Forms.Button cmdDeletePatchAttribute;
    private System.Windows.Forms.ComboBox cboPatchAttribute;
    private System.Windows.Forms.Button cmdSavePatchAttribute;
    private System.Windows.Forms.ComboBox cboPatchAttributeType;
    private System.Windows.Forms.ListBox lstPatchAttribute;
    private System.Windows.Forms.Button cmdDeletePatchEntity;
    private System.Windows.Forms.ComboBox cboPatchEntity;
    private System.Windows.Forms.Button cmdSavePatchEntity;
    private System.Windows.Forms.ComboBox cboPatchEntityType;
    private System.Windows.Forms.Label lblEntityPatchAttributes;
    private System.Windows.Forms.Label lblEntityPatchEntities;
    private Button cmdGenerateDocumentation;

    private StartupOptions mStartupOptions;
    private const string PROJECTSETTINGS_EXTENSION = ".PVEntityGenerator.xml";
    private const string DATABASEMODEL_EXTENSION = ".PVDatabaseModel.xml";

    public frmMain(StartupOptions pStartupOptions) {
      InitializeComponent();
      this.Text = Application.ProductName + " " + Application.ProductVersion;
      mStartupOptions = pStartupOptions;
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose( bool disposing ) {
      if( disposing ) {
        if (components != null) {
          components.Dispose();
        }
      }
      base.Dispose( disposing );
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
      this.lblDBDefinition = new System.Windows.Forms.Label();
      this.cboDBDefinition = new System.Windows.Forms.ComboBox();
      this.cmdDBDefinitionBrowse = new System.Windows.Forms.Button();
      this.sbStatus = new System.Windows.Forms.StatusBar();
      this.sbarPanel = new System.Windows.Forms.StatusBarPanel();
      this.pbar = new System.Windows.Forms.ProgressBar();
      this.tabMain = new System.Windows.Forms.TabControl();
      this.tabPageProject = new System.Windows.Forms.TabPage();
      this.cmdGenerateDocumentation = new System.Windows.Forms.Button();
      this.pgProject = new PVEntityGenerator.Controls.PVPropertyGrid();
      this.lstSupportedDatabasePlatform = new System.Windows.Forms.CheckedListBox();
      this.lblDBPlatform = new System.Windows.Forms.Label();
      this.cboPlatform = new System.Windows.Forms.ComboBox();
      this.lblPlatform = new System.Windows.Forms.Label();
      this.tabPageEntityGeneration = new System.Windows.Forms.TabPage();
      this.cmdRefreshAllEnumerations = new System.Windows.Forms.Button();
      this.grpEntityGeneration = new System.Windows.Forms.GroupBox();
      this.cmdEntityGeneration = new System.Windows.Forms.Button();
      this.tabEntityGeneration = new System.Windows.Forms.TabControl();
      this.tabPageGlobalOptions = new System.Windows.Forms.TabPage();
      this.pgEntiyGenerationGlobalOptions = new PVEntityGenerator.Controls.PVPropertyGrid();
      this.tabPageAttributes = new System.Windows.Forms.TabPage();
      this.dgEntityAttribute = new System.Windows.Forms.DataGrid();
      this.lblEntity = new System.Windows.Forms.Label();
      this.lblEntityCaption = new System.Windows.Forms.Label();
      this.tabPageGenerationOptions = new System.Windows.Forms.TabPage();
      this.dgDeleteConstraint = new System.Windows.Forms.DataGrid();
      this.cmdCustomFindMethodRemove = new System.Windows.Forms.Button();
      this.cmdCustomFindMethodAdd = new System.Windows.Forms.Button();
      this.cmdCustomCreateMethodRemove = new System.Windows.Forms.Button();
      this.cmdCustomCreateMethodAdd = new System.Windows.Forms.Button();
      this.lblEntity2 = new System.Windows.Forms.Label();
      this.lblEntityCaption2 = new System.Windows.Forms.Label();
      this.pgEntiyGenerationEntityOptions = new PVEntityGenerator.Controls.PVPropertyGrid();
      this.lstCustomCreateMethod = new System.Windows.Forms.ListBox();
      this.lstCustomFindMethod = new System.Windows.Forms.ListBox();
      this.lblDeleteConstraint = new System.Windows.Forms.Label();
      this.lblCustomCreateMethod = new System.Windows.Forms.Label();
      this.lblCustomFindMethod = new System.Windows.Forms.Label();
      this.tabPageEnumeration = new System.Windows.Forms.TabPage();
      this.cmdResetEnumeration = new System.Windows.Forms.Button();
      this.dgEntityEnumeration = new System.Windows.Forms.DataGrid();
      this.lblEntity3 = new System.Windows.Forms.Label();
      this.lblEntityCaption3 = new System.Windows.Forms.Label();
      this.tabPageSQLExport = new System.Windows.Forms.TabPage();
      this.grpExportImportData = new System.Windows.Forms.GroupBox();
      this.chkExportToScript = new System.Windows.Forms.CheckBox();
      this.optTransferImport = new System.Windows.Forms.RadioButton();
      this.optTransferExport = new System.Windows.Forms.RadioButton();
      this.cmdExportTransfer = new System.Windows.Forms.Button();
      this.grpGenerateScript = new System.Windows.Forms.GroupBox();
      this.cmdExportPatchScript = new System.Windows.Forms.Button();
      this.cmdExportCreateScript = new System.Windows.Forms.Button();
      this.tabExport = new System.Windows.Forms.TabControl();
      this.tabPageExport = new System.Windows.Forms.TabPage();
      this.txtExportSQL = new System.Windows.Forms.RichTextBox();
      this.cmdExportScriptExecute = new System.Windows.Forms.Button();
      this.cmdExportScriptOpen = new System.Windows.Forms.Button();
      this.cmdExportScriptSave = new System.Windows.Forms.Button();
      this.cmdExportScriptClear = new System.Windows.Forms.Button();
      this.pgSQLExport = new PVEntityGenerator.Controls.PVPropertyGrid();
      this.tabPageExportEntityOptions = new System.Windows.Forms.TabPage();
      this.dgExportEntityOptions = new System.Windows.Forms.DataGrid();
      this.tabPageExportEntityPatch = new System.Windows.Forms.TabPage();
      this.cmdDeletePatchAttribute = new System.Windows.Forms.Button();
      this.cboPatchAttribute = new System.Windows.Forms.ComboBox();
      this.cmdSavePatchAttribute = new System.Windows.Forms.Button();
      this.cboPatchAttributeType = new System.Windows.Forms.ComboBox();
      this.lstPatchAttribute = new System.Windows.Forms.ListBox();
      this.cmdDeletePatchEntity = new System.Windows.Forms.Button();
      this.cboPatchEntity = new System.Windows.Forms.ComboBox();
      this.cmdSavePatchEntity = new System.Windows.Forms.Button();
      this.cboPatchEntityType = new System.Windows.Forms.ComboBox();
      this.lstPatchEntity = new System.Windows.Forms.ListBox();
      this.lblEntityPatchAttributes = new System.Windows.Forms.Label();
      this.lblEntityPatchEntities = new System.Windows.Forms.Label();
      this.cboExportDataProvider = new System.Windows.Forms.ComboBox();
      this.lblExportDataProvider = new System.Windows.Forms.Label();
      this.cboExportDBPlatform = new System.Windows.Forms.ComboBox();
      this.lblExportDBPlatform = new System.Windows.Forms.Label();
      this.tabPageXML = new System.Windows.Forms.TabPage();
      this.txtXML = new System.Windows.Forms.TextBox();
      this.cmdSaveProjectSettings = new System.Windows.Forms.Button();
      this.pnlEntitySelector = new System.Windows.Forms.Panel();
      this.cmdDeselectAll = new System.Windows.Forms.Button();
      this.cmdSelectAll = new System.Windows.Forms.Button();
      this.lstEntity = new System.Windows.Forms.CheckedListBox();
      ((System.ComponentModel.ISupportInitialize)(this.sbarPanel)).BeginInit();
      this.tabMain.SuspendLayout();
      this.tabPageProject.SuspendLayout();
      this.tabPageEntityGeneration.SuspendLayout();
      this.grpEntityGeneration.SuspendLayout();
      this.tabEntityGeneration.SuspendLayout();
      this.tabPageGlobalOptions.SuspendLayout();
      this.tabPageAttributes.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dgEntityAttribute)).BeginInit();
      this.tabPageGenerationOptions.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dgDeleteConstraint)).BeginInit();
      this.tabPageEnumeration.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dgEntityEnumeration)).BeginInit();
      this.tabPageSQLExport.SuspendLayout();
      this.grpExportImportData.SuspendLayout();
      this.grpGenerateScript.SuspendLayout();
      this.tabExport.SuspendLayout();
      this.tabPageExport.SuspendLayout();
      this.tabPageExportEntityOptions.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dgExportEntityOptions)).BeginInit();
      this.tabPageExportEntityPatch.SuspendLayout();
      this.tabPageXML.SuspendLayout();
      this.pnlEntitySelector.SuspendLayout();
      this.SuspendLayout();
      // 
      // lblDBDefinition
      // 
      this.lblDBDefinition.AutoSize = true;
      this.lblDBDefinition.Location = new System.Drawing.Point(8, 12);
      this.lblDBDefinition.Name = "lblDBDefinition";
      this.lblDBDefinition.Size = new System.Drawing.Size(68, 13);
      this.lblDBDefinition.TabIndex = 0;
      this.lblDBDefinition.Text = "DB Definition";
      // 
      // cboDBDefinition
      // 
      this.cboDBDefinition.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cboDBDefinition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cboDBDefinition.ItemHeight = 13;
      this.cboDBDefinition.Location = new System.Drawing.Point(80, 8);
      this.cboDBDefinition.MaxDropDownItems = 20;
      this.cboDBDefinition.Name = "cboDBDefinition";
      this.cboDBDefinition.Size = new System.Drawing.Size(716, 21);
      this.cboDBDefinition.TabIndex = 1;
      this.cboDBDefinition.SelectedIndexChanged += new System.EventHandler(this.cboDBDefinition_SelectedIndexChanged);
      this.cboDBDefinition.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cboDBDefinition_KeyDown);
      // 
      // cmdDBDefinitionBrowse
      // 
      this.cmdDBDefinitionBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.cmdDBDefinitionBrowse.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.cmdDBDefinitionBrowse.Location = new System.Drawing.Point(800, 8);
      this.cmdDBDefinitionBrowse.Name = "cmdDBDefinitionBrowse";
      this.cmdDBDefinitionBrowse.Size = new System.Drawing.Size(24, 20);
      this.cmdDBDefinitionBrowse.TabIndex = 2;
      this.cmdDBDefinitionBrowse.Text = "...";
      this.cmdDBDefinitionBrowse.Click += new System.EventHandler(this.cmdDBDefinitionBrowse_Click);
      // 
      // sbStatus
      // 
      this.sbStatus.Location = new System.Drawing.Point(0, 493);
      this.sbStatus.Name = "sbStatus";
      this.sbStatus.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
            this.sbarPanel});
      this.sbStatus.ShowPanels = true;
      this.sbStatus.Size = new System.Drawing.Size(892, 20);
      this.sbStatus.TabIndex = 8;
      // 
      // sbarPanel
      // 
      this.sbarPanel.Name = "sbarPanel";
      this.sbarPanel.Width = 545;
      // 
      // pbar
      // 
      this.pbar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.pbar.Location = new System.Drawing.Point(548, 495);
      this.pbar.Name = "pbar";
      this.pbar.Size = new System.Drawing.Size(324, 18);
      this.pbar.TabIndex = 9;
      // 
      // tabMain
      // 
      this.tabMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabMain.Controls.Add(this.tabPageProject);
      this.tabMain.Controls.Add(this.tabPageEntityGeneration);
      this.tabMain.Controls.Add(this.tabPageSQLExport);
      this.tabMain.Controls.Add(this.tabPageXML);
      this.tabMain.Location = new System.Drawing.Point(204, 36);
      this.tabMain.Name = "tabMain";
      this.tabMain.SelectedIndex = 0;
      this.tabMain.Size = new System.Drawing.Size(684, 452);
      this.tabMain.TabIndex = 0;
      // 
      // tabPageProject
      // 
      this.tabPageProject.Controls.Add(this.cmdGenerateDocumentation);
      this.tabPageProject.Controls.Add(this.pgProject);
      this.tabPageProject.Controls.Add(this.lstSupportedDatabasePlatform);
      this.tabPageProject.Controls.Add(this.lblDBPlatform);
      this.tabPageProject.Controls.Add(this.cboPlatform);
      this.tabPageProject.Controls.Add(this.lblPlatform);
      this.tabPageProject.Location = new System.Drawing.Point(4, 22);
      this.tabPageProject.Name = "tabPageProject";
      this.tabPageProject.Size = new System.Drawing.Size(676, 426);
      this.tabPageProject.TabIndex = 3;
      this.tabPageProject.Text = "Project";
      // 
      // cmdGenerateDocumentation
      // 
      this.cmdGenerateDocumentation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.cmdGenerateDocumentation.Location = new System.Drawing.Point(172, 396);
      this.cmdGenerateDocumentation.Name = "cmdGenerateDocumentation";
      this.cmdGenerateDocumentation.Size = new System.Drawing.Size(156, 20);
      this.cmdGenerateDocumentation.TabIndex = 5;
      this.cmdGenerateDocumentation.Text = "Generate documentation";
      this.cmdGenerateDocumentation.Click += new System.EventHandler(this.cmdGenerateDocumentation_Click);
      // 
      // pgProject
      // 
      this.pgProject.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.pgProject.HelpVisible = false;
      this.pgProject.LineColor = System.Drawing.SystemColors.ScrollBar;
      this.pgProject.Location = new System.Drawing.Point(340, 12);
      this.pgProject.Name = "pgProject";
      this.pgProject.Size = new System.Drawing.Size(292, 408);
      this.pgProject.TabIndex = 4;
      this.pgProject.ToolbarVisible = false;
      // 
      // lstSupportedDatabasePlatform
      // 
      this.lstSupportedDatabasePlatform.Location = new System.Drawing.Point(176, 40);
      this.lstSupportedDatabasePlatform.Name = "lstSupportedDatabasePlatform";
      this.lstSupportedDatabasePlatform.Size = new System.Drawing.Size(152, 68);
      this.lstSupportedDatabasePlatform.TabIndex = 3;
      this.lstSupportedDatabasePlatform.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lstSupportedDatabasePlatform_ItemCheck);
      // 
      // lblDBPlatform
      // 
      this.lblDBPlatform.AutoSize = true;
      this.lblDBPlatform.Location = new System.Drawing.Point(12, 44);
      this.lblDBPlatform.Name = "lblDBPlatform";
      this.lblDBPlatform.Size = new System.Drawing.Size(153, 13);
      this.lblDBPlatform.TabIndex = 2;
      this.lblDBPlatform.Text = "Supported database platforms";
      // 
      // cboPlatform
      // 
      this.cboPlatform.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cboPlatform.Location = new System.Drawing.Point(176, 12);
      this.cboPlatform.Name = "cboPlatform";
      this.cboPlatform.Size = new System.Drawing.Size(152, 21);
      this.cboPlatform.TabIndex = 1;
      this.cboPlatform.SelectedValueChanged += new System.EventHandler(this.cboPlatform_SelectedValueChanged);
      // 
      // lblPlatform
      // 
      this.lblPlatform.AutoSize = true;
      this.lblPlatform.Location = new System.Drawing.Point(12, 16);
      this.lblPlatform.Name = "lblPlatform";
      this.lblPlatform.Size = new System.Drawing.Size(47, 13);
      this.lblPlatform.TabIndex = 0;
      this.lblPlatform.Text = "Platform";
      // 
      // tabPageEntityGeneration
      // 
      this.tabPageEntityGeneration.Controls.Add(this.cmdRefreshAllEnumerations);
      this.tabPageEntityGeneration.Controls.Add(this.grpEntityGeneration);
      this.tabPageEntityGeneration.Controls.Add(this.tabEntityGeneration);
      this.tabPageEntityGeneration.Location = new System.Drawing.Point(4, 22);
      this.tabPageEntityGeneration.Name = "tabPageEntityGeneration";
      this.tabPageEntityGeneration.Size = new System.Drawing.Size(676, 426);
      this.tabPageEntityGeneration.TabIndex = 0;
      this.tabPageEntityGeneration.Text = "Entity Generation";
      // 
      // cmdRefreshAllEnumerations
      // 
      this.cmdRefreshAllEnumerations.Location = new System.Drawing.Point(8, 12);
      this.cmdRefreshAllEnumerations.Name = "cmdRefreshAllEnumerations";
      this.cmdRefreshAllEnumerations.Size = new System.Drawing.Size(140, 20);
      this.cmdRefreshAllEnumerations.TabIndex = 0;
      this.cmdRefreshAllEnumerations.Text = "Refresh all enumerations";
      this.cmdRefreshAllEnumerations.Click += new System.EventHandler(this.cmdRefreshAllEnumerations_Click);
      // 
      // grpEntityGeneration
      // 
      this.grpEntityGeneration.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.grpEntityGeneration.Controls.Add(this.cmdEntityGeneration);
      this.grpEntityGeneration.Location = new System.Drawing.Point(508, 4);
      this.grpEntityGeneration.Name = "grpEntityGeneration";
      this.grpEntityGeneration.Size = new System.Drawing.Size(124, 52);
      this.grpEntityGeneration.TabIndex = 1;
      this.grpEntityGeneration.TabStop = false;
      this.grpEntityGeneration.Text = "Entity Generation";
      // 
      // cmdEntityGeneration
      // 
      this.cmdEntityGeneration.Location = new System.Drawing.Point(12, 20);
      this.cmdEntityGeneration.Name = "cmdEntityGeneration";
      this.cmdEntityGeneration.Size = new System.Drawing.Size(100, 20);
      this.cmdEntityGeneration.TabIndex = 0;
      this.cmdEntityGeneration.Text = "Generate files";
      this.cmdEntityGeneration.Click += new System.EventHandler(this.cmdEntityGeneration_Click);
      // 
      // tabEntityGeneration
      // 
      this.tabEntityGeneration.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabEntityGeneration.Controls.Add(this.tabPageGlobalOptions);
      this.tabEntityGeneration.Controls.Add(this.tabPageAttributes);
      this.tabEntityGeneration.Controls.Add(this.tabPageGenerationOptions);
      this.tabEntityGeneration.Controls.Add(this.tabPageEnumeration);
      this.tabEntityGeneration.Location = new System.Drawing.Point(8, 44);
      this.tabEntityGeneration.Name = "tabEntityGeneration";
      this.tabEntityGeneration.SelectedIndex = 0;
      this.tabEntityGeneration.Size = new System.Drawing.Size(628, 376);
      this.tabEntityGeneration.TabIndex = 2;
      // 
      // tabPageGlobalOptions
      // 
      this.tabPageGlobalOptions.Controls.Add(this.pgEntiyGenerationGlobalOptions);
      this.tabPageGlobalOptions.Location = new System.Drawing.Point(4, 22);
      this.tabPageGlobalOptions.Name = "tabPageGlobalOptions";
      this.tabPageGlobalOptions.Size = new System.Drawing.Size(620, 350);
      this.tabPageGlobalOptions.TabIndex = 3;
      this.tabPageGlobalOptions.Text = "Global Options";
      // 
      // pgEntiyGenerationGlobalOptions
      // 
      this.pgEntiyGenerationGlobalOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.pgEntiyGenerationGlobalOptions.HelpVisible = false;
      this.pgEntiyGenerationGlobalOptions.LineColor = System.Drawing.SystemColors.ScrollBar;
      this.pgEntiyGenerationGlobalOptions.Location = new System.Drawing.Point(8, 8);
      this.pgEntiyGenerationGlobalOptions.Name = "pgEntiyGenerationGlobalOptions";
      this.pgEntiyGenerationGlobalOptions.Size = new System.Drawing.Size(604, 336);
      this.pgEntiyGenerationGlobalOptions.TabIndex = 0;
      this.pgEntiyGenerationGlobalOptions.ToolbarVisible = false;
      // 
      // tabPageAttributes
      // 
      this.tabPageAttributes.Controls.Add(this.dgEntityAttribute);
      this.tabPageAttributes.Controls.Add(this.lblEntity);
      this.tabPageAttributes.Controls.Add(this.lblEntityCaption);
      this.tabPageAttributes.Location = new System.Drawing.Point(4, 22);
      this.tabPageAttributes.Name = "tabPageAttributes";
      this.tabPageAttributes.Size = new System.Drawing.Size(620, 350);
      this.tabPageAttributes.TabIndex = 0;
      this.tabPageAttributes.Text = "Attributes";
      // 
      // dgEntityAttribute
      // 
      this.dgEntityAttribute.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.dgEntityAttribute.CaptionVisible = false;
      this.dgEntityAttribute.DataMember = "";
      this.dgEntityAttribute.HeaderForeColor = System.Drawing.SystemColors.ControlText;
      this.dgEntityAttribute.Location = new System.Drawing.Point(8, 28);
      this.dgEntityAttribute.Name = "dgEntityAttribute";
      this.dgEntityAttribute.Size = new System.Drawing.Size(608, 316);
      this.dgEntityAttribute.TabIndex = 2;
      // 
      // lblEntity
      // 
      this.lblEntity.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblEntity.Location = new System.Drawing.Point(52, 8);
      this.lblEntity.Name = "lblEntity";
      this.lblEntity.Size = new System.Drawing.Size(264, 17);
      this.lblEntity.TabIndex = 1;
      this.lblEntity.Text = "<Entity>";
      // 
      // lblEntityCaption
      // 
      this.lblEntityCaption.AutoSize = true;
      this.lblEntityCaption.Location = new System.Drawing.Point(8, 8);
      this.lblEntityCaption.Name = "lblEntityCaption";
      this.lblEntityCaption.Size = new System.Drawing.Size(39, 13);
      this.lblEntityCaption.TabIndex = 0;
      this.lblEntityCaption.Text = "Entity:";
      // 
      // tabPageGenerationOptions
      // 
      this.tabPageGenerationOptions.Controls.Add(this.dgDeleteConstraint);
      this.tabPageGenerationOptions.Controls.Add(this.cmdCustomFindMethodRemove);
      this.tabPageGenerationOptions.Controls.Add(this.cmdCustomFindMethodAdd);
      this.tabPageGenerationOptions.Controls.Add(this.cmdCustomCreateMethodRemove);
      this.tabPageGenerationOptions.Controls.Add(this.cmdCustomCreateMethodAdd);
      this.tabPageGenerationOptions.Controls.Add(this.lblEntity2);
      this.tabPageGenerationOptions.Controls.Add(this.lblEntityCaption2);
      this.tabPageGenerationOptions.Controls.Add(this.pgEntiyGenerationEntityOptions);
      this.tabPageGenerationOptions.Controls.Add(this.lstCustomCreateMethod);
      this.tabPageGenerationOptions.Controls.Add(this.lstCustomFindMethod);
      this.tabPageGenerationOptions.Controls.Add(this.lblDeleteConstraint);
      this.tabPageGenerationOptions.Controls.Add(this.lblCustomCreateMethod);
      this.tabPageGenerationOptions.Controls.Add(this.lblCustomFindMethod);
      this.tabPageGenerationOptions.Location = new System.Drawing.Point(4, 22);
      this.tabPageGenerationOptions.Name = "tabPageGenerationOptions";
      this.tabPageGenerationOptions.Size = new System.Drawing.Size(620, 350);
      this.tabPageGenerationOptions.TabIndex = 1;
      this.tabPageGenerationOptions.Text = "Generation Options";
      // 
      // dgDeleteConstraint
      // 
      this.dgDeleteConstraint.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.dgDeleteConstraint.CaptionVisible = false;
      this.dgDeleteConstraint.DataMember = "";
      this.dgDeleteConstraint.HeaderForeColor = System.Drawing.SystemColors.ControlText;
      this.dgDeleteConstraint.Location = new System.Drawing.Point(8, 52);
      this.dgDeleteConstraint.Name = "dgDeleteConstraint";
      this.dgDeleteConstraint.Size = new System.Drawing.Size(304, 88);
      this.dgDeleteConstraint.TabIndex = 3;
      // 
      // cmdCustomFindMethodRemove
      // 
      this.cmdCustomFindMethodRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cmdCustomFindMethodRemove.Location = new System.Drawing.Point(260, 324);
      this.cmdCustomFindMethodRemove.Name = "cmdCustomFindMethodRemove";
      this.cmdCustomFindMethodRemove.Size = new System.Drawing.Size(52, 20);
      this.cmdCustomFindMethodRemove.TabIndex = 12;
      this.cmdCustomFindMethodRemove.Text = "Remove";
      this.cmdCustomFindMethodRemove.Click += new System.EventHandler(this.cmdCustomFindMethodRemove_Click);
      // 
      // cmdCustomFindMethodAdd
      // 
      this.cmdCustomFindMethodAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cmdCustomFindMethodAdd.Location = new System.Drawing.Point(204, 324);
      this.cmdCustomFindMethodAdd.Name = "cmdCustomFindMethodAdd";
      this.cmdCustomFindMethodAdd.Size = new System.Drawing.Size(52, 20);
      this.cmdCustomFindMethodAdd.TabIndex = 11;
      this.cmdCustomFindMethodAdd.Text = "Add";
      this.cmdCustomFindMethodAdd.Click += new System.EventHandler(this.cmdCustomFindMethodAdd_Click);
      // 
      // cmdCustomCreateMethodRemove
      // 
      this.cmdCustomCreateMethodRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cmdCustomCreateMethodRemove.Location = new System.Drawing.Point(260, 216);
      this.cmdCustomCreateMethodRemove.Name = "cmdCustomCreateMethodRemove";
      this.cmdCustomCreateMethodRemove.Size = new System.Drawing.Size(52, 20);
      this.cmdCustomCreateMethodRemove.TabIndex = 7;
      this.cmdCustomCreateMethodRemove.Text = "Remove";
      this.cmdCustomCreateMethodRemove.Click += new System.EventHandler(this.cmdCustomCreateMethodRemove_Click);
      // 
      // cmdCustomCreateMethodAdd
      // 
      this.cmdCustomCreateMethodAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cmdCustomCreateMethodAdd.Location = new System.Drawing.Point(204, 216);
      this.cmdCustomCreateMethodAdd.Name = "cmdCustomCreateMethodAdd";
      this.cmdCustomCreateMethodAdd.Size = new System.Drawing.Size(52, 20);
      this.cmdCustomCreateMethodAdd.TabIndex = 6;
      this.cmdCustomCreateMethodAdd.Text = "Add";
      this.cmdCustomCreateMethodAdd.Click += new System.EventHandler(this.cmdCustomCreateMethodAdd_Click);
      // 
      // lblEntity2
      // 
      this.lblEntity2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblEntity2.Location = new System.Drawing.Point(52, 8);
      this.lblEntity2.Name = "lblEntity2";
      this.lblEntity2.Size = new System.Drawing.Size(264, 17);
      this.lblEntity2.TabIndex = 1;
      this.lblEntity2.Text = "<Entity>";
      // 
      // lblEntityCaption2
      // 
      this.lblEntityCaption2.AutoSize = true;
      this.lblEntityCaption2.Location = new System.Drawing.Point(8, 8);
      this.lblEntityCaption2.Name = "lblEntityCaption2";
      this.lblEntityCaption2.Size = new System.Drawing.Size(39, 13);
      this.lblEntityCaption2.TabIndex = 0;
      this.lblEntityCaption2.Text = "Entity:";
      // 
      // pgEntiyGenerationEntityOptions
      // 
      this.pgEntiyGenerationEntityOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.pgEntiyGenerationEntityOptions.HelpVisible = false;
      this.pgEntiyGenerationEntityOptions.LineColor = System.Drawing.SystemColors.ScrollBar;
      this.pgEntiyGenerationEntityOptions.Location = new System.Drawing.Point(320, 8);
      this.pgEntiyGenerationEntityOptions.Name = "pgEntiyGenerationEntityOptions";
      this.pgEntiyGenerationEntityOptions.Size = new System.Drawing.Size(292, 336);
      this.pgEntiyGenerationEntityOptions.TabIndex = 0;
      this.pgEntiyGenerationEntityOptions.ToolbarVisible = false;
      // 
      // lstCustomCreateMethod
      // 
      this.lstCustomCreateMethod.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lstCustomCreateMethod.Location = new System.Drawing.Point(8, 168);
      this.lstCustomCreateMethod.Name = "lstCustomCreateMethod";
      this.lstCustomCreateMethod.Size = new System.Drawing.Size(304, 43);
      this.lstCustomCreateMethod.TabIndex = 5;
      this.lstCustomCreateMethod.DoubleClick += new System.EventHandler(this.lstCustomCreateMethod_DoubleClick);
      // 
      // lstCustomFindMethod
      // 
      this.lstCustomFindMethod.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lstCustomFindMethod.Location = new System.Drawing.Point(8, 252);
      this.lstCustomFindMethod.Name = "lstCustomFindMethod";
      this.lstCustomFindMethod.Size = new System.Drawing.Size(304, 69);
      this.lstCustomFindMethod.TabIndex = 10;
      this.lstCustomFindMethod.DoubleClick += new System.EventHandler(this.lstCustomFindMethod_DoubleClick);
      // 
      // lblDeleteConstraint
      // 
      this.lblDeleteConstraint.AutoSize = true;
      this.lblDeleteConstraint.Location = new System.Drawing.Point(8, 36);
      this.lblDeleteConstraint.Name = "lblDeleteConstraint";
      this.lblDeleteConstraint.Size = new System.Drawing.Size(137, 13);
      this.lblDeleteConstraint.TabIndex = 2;
      this.lblDeleteConstraint.Text = "Delete Constraint warnings";
      // 
      // lblCustomCreateMethod
      // 
      this.lblCustomCreateMethod.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.lblCustomCreateMethod.AutoSize = true;
      this.lblCustomCreateMethod.Location = new System.Drawing.Point(8, 152);
      this.lblCustomCreateMethod.Name = "lblCustomCreateMethod";
      this.lblCustomCreateMethod.Size = new System.Drawing.Size(121, 13);
      this.lblCustomCreateMethod.TabIndex = 4;
      this.lblCustomCreateMethod.Text = "Custom create methods";
      // 
      // lblCustomFindMethod
      // 
      this.lblCustomFindMethod.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.lblCustomFindMethod.AutoSize = true;
      this.lblCustomFindMethod.Location = new System.Drawing.Point(8, 236);
      this.lblCustomFindMethod.Name = "lblCustomFindMethod";
      this.lblCustomFindMethod.Size = new System.Drawing.Size(108, 13);
      this.lblCustomFindMethod.TabIndex = 8;
      this.lblCustomFindMethod.Text = "Custom find methods";
      // 
      // tabPageEnumeration
      // 
      this.tabPageEnumeration.Controls.Add(this.cmdResetEnumeration);
      this.tabPageEnumeration.Controls.Add(this.dgEntityEnumeration);
      this.tabPageEnumeration.Controls.Add(this.lblEntity3);
      this.tabPageEnumeration.Controls.Add(this.lblEntityCaption3);
      this.tabPageEnumeration.Location = new System.Drawing.Point(4, 22);
      this.tabPageEnumeration.Name = "tabPageEnumeration";
      this.tabPageEnumeration.Size = new System.Drawing.Size(620, 350);
      this.tabPageEnumeration.TabIndex = 2;
      this.tabPageEnumeration.Text = "Enumeration";
      // 
      // cmdResetEnumeration
      // 
      this.cmdResetEnumeration.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.cmdResetEnumeration.Location = new System.Drawing.Point(500, 4);
      this.cmdResetEnumeration.Name = "cmdResetEnumeration";
      this.cmdResetEnumeration.Size = new System.Drawing.Size(116, 20);
      this.cmdResetEnumeration.TabIndex = 3;
      this.cmdResetEnumeration.Text = "Reset enumeration";
      this.cmdResetEnumeration.Click += new System.EventHandler(this.cmdResetEnumeration_Click);
      // 
      // dgEntityEnumeration
      // 
      this.dgEntityEnumeration.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.dgEntityEnumeration.CaptionVisible = false;
      this.dgEntityEnumeration.DataMember = "";
      this.dgEntityEnumeration.HeaderForeColor = System.Drawing.SystemColors.ControlText;
      this.dgEntityEnumeration.Location = new System.Drawing.Point(8, 28);
      this.dgEntityEnumeration.Name = "dgEntityEnumeration";
      this.dgEntityEnumeration.Size = new System.Drawing.Size(608, 316);
      this.dgEntityEnumeration.TabIndex = 2;
      // 
      // lblEntity3
      // 
      this.lblEntity3.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblEntity3.Location = new System.Drawing.Point(52, 8);
      this.lblEntity3.Name = "lblEntity3";
      this.lblEntity3.Size = new System.Drawing.Size(264, 17);
      this.lblEntity3.TabIndex = 1;
      this.lblEntity3.Text = "<Entity>";
      // 
      // lblEntityCaption3
      // 
      this.lblEntityCaption3.AutoSize = true;
      this.lblEntityCaption3.Location = new System.Drawing.Point(8, 8);
      this.lblEntityCaption3.Name = "lblEntityCaption3";
      this.lblEntityCaption3.Size = new System.Drawing.Size(39, 13);
      this.lblEntityCaption3.TabIndex = 0;
      this.lblEntityCaption3.Text = "Entity:";
      // 
      // tabPageSQLExport
      // 
      this.tabPageSQLExport.Controls.Add(this.grpExportImportData);
      this.tabPageSQLExport.Controls.Add(this.grpGenerateScript);
      this.tabPageSQLExport.Controls.Add(this.tabExport);
      this.tabPageSQLExport.Controls.Add(this.cboExportDataProvider);
      this.tabPageSQLExport.Controls.Add(this.lblExportDataProvider);
      this.tabPageSQLExport.Controls.Add(this.cboExportDBPlatform);
      this.tabPageSQLExport.Controls.Add(this.lblExportDBPlatform);
      this.tabPageSQLExport.Location = new System.Drawing.Point(4, 22);
      this.tabPageSQLExport.Name = "tabPageSQLExport";
      this.tabPageSQLExport.Size = new System.Drawing.Size(676, 426);
      this.tabPageSQLExport.TabIndex = 1;
      this.tabPageSQLExport.Text = "SQL Export";
      // 
      // grpExportImportData
      // 
      this.grpExportImportData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.grpExportImportData.Controls.Add(this.chkExportToScript);
      this.grpExportImportData.Controls.Add(this.optTransferImport);
      this.grpExportImportData.Controls.Add(this.optTransferExport);
      this.grpExportImportData.Controls.Add(this.cmdExportTransfer);
      this.grpExportImportData.Location = new System.Drawing.Point(468, 4);
      this.grpExportImportData.Name = "grpExportImportData";
      this.grpExportImportData.Size = new System.Drawing.Size(200, 72);
      this.grpExportImportData.TabIndex = 5;
      this.grpExportImportData.TabStop = false;
      this.grpExportImportData.Text = "Export/Import Data";
      // 
      // chkExportToScript
      // 
      this.chkExportToScript.Location = new System.Drawing.Point(12, 44);
      this.chkExportToScript.Name = "chkExportToScript";
      this.chkExportToScript.Size = new System.Drawing.Size(112, 16);
      this.chkExportToScript.TabIndex = 3;
      this.chkExportToScript.Text = "Export to script";
      // 
      // optTransferImport
      // 
      this.optTransferImport.Location = new System.Drawing.Point(136, 44);
      this.optTransferImport.Name = "optTransferImport";
      this.optTransferImport.Size = new System.Drawing.Size(56, 16);
      this.optTransferImport.TabIndex = 2;
      this.optTransferImport.Text = "Import";
      // 
      // optTransferExport
      // 
      this.optTransferExport.Checked = true;
      this.optTransferExport.Location = new System.Drawing.Point(136, 20);
      this.optTransferExport.Name = "optTransferExport";
      this.optTransferExport.Size = new System.Drawing.Size(56, 16);
      this.optTransferExport.TabIndex = 1;
      this.optTransferExport.TabStop = true;
      this.optTransferExport.Text = "Export";
      // 
      // cmdExportTransfer
      // 
      this.cmdExportTransfer.Location = new System.Drawing.Point(12, 20);
      this.cmdExportTransfer.Name = "cmdExportTransfer";
      this.cmdExportTransfer.Size = new System.Drawing.Size(116, 20);
      this.cmdExportTransfer.TabIndex = 0;
      this.cmdExportTransfer.Text = "Transfer Data";
      this.cmdExportTransfer.Click += new System.EventHandler(this.cmdExportTransfer_Click);
      // 
      // grpGenerateScript
      // 
      this.grpGenerateScript.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.grpGenerateScript.Controls.Add(this.cmdExportPatchScript);
      this.grpGenerateScript.Controls.Add(this.cmdExportCreateScript);
      this.grpGenerateScript.Location = new System.Drawing.Point(328, 4);
      this.grpGenerateScript.Name = "grpGenerateScript";
      this.grpGenerateScript.Size = new System.Drawing.Size(132, 72);
      this.grpGenerateScript.TabIndex = 4;
      this.grpGenerateScript.TabStop = false;
      this.grpGenerateScript.Text = "Generate script";
      // 
      // cmdExportPatchScript
      // 
      this.cmdExportPatchScript.Location = new System.Drawing.Point(12, 44);
      this.cmdExportPatchScript.Name = "cmdExportPatchScript";
      this.cmdExportPatchScript.Size = new System.Drawing.Size(108, 20);
      this.cmdExportPatchScript.TabIndex = 1;
      this.cmdExportPatchScript.Text = "Patch entities";
      this.cmdExportPatchScript.Click += new System.EventHandler(this.cmdExportPatchScript_Click);
      // 
      // cmdExportCreateScript
      // 
      this.cmdExportCreateScript.Location = new System.Drawing.Point(12, 20);
      this.cmdExportCreateScript.Name = "cmdExportCreateScript";
      this.cmdExportCreateScript.Size = new System.Drawing.Size(108, 20);
      this.cmdExportCreateScript.TabIndex = 0;
      this.cmdExportCreateScript.Text = "Create entities";
      this.cmdExportCreateScript.Click += new System.EventHandler(this.cmdExportCreateScript_Click);
      // 
      // tabExport
      // 
      this.tabExport.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabExport.Controls.Add(this.tabPageExport);
      this.tabExport.Controls.Add(this.tabPageExportEntityOptions);
      this.tabExport.Controls.Add(this.tabPageExportEntityPatch);
      this.tabExport.Location = new System.Drawing.Point(8, 64);
      this.tabExport.Name = "tabExport";
      this.tabExport.SelectedIndex = 0;
      this.tabExport.Size = new System.Drawing.Size(664, 356);
      this.tabExport.TabIndex = 6;
      // 
      // tabPageExport
      // 
      this.tabPageExport.Controls.Add(this.txtExportSQL);
      this.tabPageExport.Controls.Add(this.cmdExportScriptExecute);
      this.tabPageExport.Controls.Add(this.cmdExportScriptOpen);
      this.tabPageExport.Controls.Add(this.cmdExportScriptSave);
      this.tabPageExport.Controls.Add(this.cmdExportScriptClear);
      this.tabPageExport.Controls.Add(this.pgSQLExport);
      this.tabPageExport.Location = new System.Drawing.Point(4, 22);
      this.tabPageExport.Name = "tabPageExport";
      this.tabPageExport.Size = new System.Drawing.Size(656, 330);
      this.tabPageExport.TabIndex = 0;
      this.tabPageExport.Text = "Export";
      // 
      // txtExportSQL
      // 
      this.txtExportSQL.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtExportSQL.Location = new System.Drawing.Point(8, 8);
      this.txtExportSQL.Name = "txtExportSQL";
      this.txtExportSQL.Size = new System.Drawing.Size(340, 288);
      this.txtExportSQL.TabIndex = 6;
      this.txtExportSQL.Text = "";
      this.txtExportSQL.WordWrap = false;
      // 
      // cmdExportScriptExecute
      // 
      this.cmdExportScriptExecute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.cmdExportScriptExecute.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.cmdExportScriptExecute.Location = new System.Drawing.Point(8, 304);
      this.cmdExportScriptExecute.Name = "cmdExportScriptExecute";
      this.cmdExportScriptExecute.Size = new System.Drawing.Size(68, 20);
      this.cmdExportScriptExecute.TabIndex = 1;
      this.cmdExportScriptExecute.Text = "Execute";
      this.cmdExportScriptExecute.Click += new System.EventHandler(this.cmdExportScriptExecute_Click);
      // 
      // cmdExportScriptOpen
      // 
      this.cmdExportScriptOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cmdExportScriptOpen.Location = new System.Drawing.Point(196, 304);
      this.cmdExportScriptOpen.Name = "cmdExportScriptOpen";
      this.cmdExportScriptOpen.Size = new System.Drawing.Size(48, 20);
      this.cmdExportScriptOpen.TabIndex = 2;
      this.cmdExportScriptOpen.Text = "Open";
      this.cmdExportScriptOpen.Click += new System.EventHandler(this.cmdExportScriptOpen_Click);
      // 
      // cmdExportScriptSave
      // 
      this.cmdExportScriptSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cmdExportScriptSave.Location = new System.Drawing.Point(248, 304);
      this.cmdExportScriptSave.Name = "cmdExportScriptSave";
      this.cmdExportScriptSave.Size = new System.Drawing.Size(48, 20);
      this.cmdExportScriptSave.TabIndex = 3;
      this.cmdExportScriptSave.Text = "Save";
      this.cmdExportScriptSave.Click += new System.EventHandler(this.cmdExportScriptSave_Click);
      // 
      // cmdExportScriptClear
      // 
      this.cmdExportScriptClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cmdExportScriptClear.Location = new System.Drawing.Point(300, 304);
      this.cmdExportScriptClear.Name = "cmdExportScriptClear";
      this.cmdExportScriptClear.Size = new System.Drawing.Size(48, 20);
      this.cmdExportScriptClear.TabIndex = 4;
      this.cmdExportScriptClear.Text = "Clear";
      this.cmdExportScriptClear.Click += new System.EventHandler(this.cmdExportScriptClear_Click);
      // 
      // pgSQLExport
      // 
      this.pgSQLExport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.pgSQLExport.HelpVisible = false;
      this.pgSQLExport.LineColor = System.Drawing.SystemColors.ScrollBar;
      this.pgSQLExport.Location = new System.Drawing.Point(356, 8);
      this.pgSQLExport.Name = "pgSQLExport";
      this.pgSQLExport.Size = new System.Drawing.Size(292, 316);
      this.pgSQLExport.TabIndex = 5;
      this.pgSQLExport.ToolbarVisible = false;
      // 
      // tabPageExportEntityOptions
      // 
      this.tabPageExportEntityOptions.Controls.Add(this.dgExportEntityOptions);
      this.tabPageExportEntityOptions.Location = new System.Drawing.Point(4, 22);
      this.tabPageExportEntityOptions.Name = "tabPageExportEntityOptions";
      this.tabPageExportEntityOptions.Size = new System.Drawing.Size(656, 330);
      this.tabPageExportEntityOptions.TabIndex = 1;
      this.tabPageExportEntityOptions.Text = "Entity Options";
      this.tabPageExportEntityOptions.Visible = false;
      // 
      // dgExportEntityOptions
      // 
      this.dgExportEntityOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.dgExportEntityOptions.CaptionVisible = false;
      this.dgExportEntityOptions.DataMember = "";
      this.dgExportEntityOptions.HeaderForeColor = System.Drawing.SystemColors.ControlText;
      this.dgExportEntityOptions.Location = new System.Drawing.Point(8, 7);
      this.dgExportEntityOptions.Name = "dgExportEntityOptions";
      this.dgExportEntityOptions.Size = new System.Drawing.Size(644, 316);
      this.dgExportEntityOptions.TabIndex = 3;
      // 
      // tabPageExportEntityPatch
      // 
      this.tabPageExportEntityPatch.Controls.Add(this.cmdDeletePatchAttribute);
      this.tabPageExportEntityPatch.Controls.Add(this.cboPatchAttribute);
      this.tabPageExportEntityPatch.Controls.Add(this.cmdSavePatchAttribute);
      this.tabPageExportEntityPatch.Controls.Add(this.cboPatchAttributeType);
      this.tabPageExportEntityPatch.Controls.Add(this.lstPatchAttribute);
      this.tabPageExportEntityPatch.Controls.Add(this.cmdDeletePatchEntity);
      this.tabPageExportEntityPatch.Controls.Add(this.cboPatchEntity);
      this.tabPageExportEntityPatch.Controls.Add(this.cmdSavePatchEntity);
      this.tabPageExportEntityPatch.Controls.Add(this.cboPatchEntityType);
      this.tabPageExportEntityPatch.Controls.Add(this.lstPatchEntity);
      this.tabPageExportEntityPatch.Controls.Add(this.lblEntityPatchAttributes);
      this.tabPageExportEntityPatch.Controls.Add(this.lblEntityPatchEntities);
      this.tabPageExportEntityPatch.Location = new System.Drawing.Point(4, 22);
      this.tabPageExportEntityPatch.Name = "tabPageExportEntityPatch";
      this.tabPageExportEntityPatch.Size = new System.Drawing.Size(656, 330);
      this.tabPageExportEntityPatch.TabIndex = 2;
      this.tabPageExportEntityPatch.Text = "Entity Patch";
      this.tabPageExportEntityPatch.Visible = false;
      // 
      // cmdDeletePatchAttribute
      // 
      this.cmdDeletePatchAttribute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.cmdDeletePatchAttribute.Location = new System.Drawing.Point(348, 308);
      this.cmdDeletePatchAttribute.Name = "cmdDeletePatchAttribute";
      this.cmdDeletePatchAttribute.Size = new System.Drawing.Size(52, 20);
      this.cmdDeletePatchAttribute.TabIndex = 17;
      this.cmdDeletePatchAttribute.Text = "Delete";
      this.cmdDeletePatchAttribute.Click += new System.EventHandler(this.cmdDeletePatchAttribute_Click);
      // 
      // cboPatchAttribute
      // 
      this.cboPatchAttribute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.cboPatchAttribute.Location = new System.Drawing.Point(276, 280);
      this.cboPatchAttribute.Name = "cboPatchAttribute";
      this.cboPatchAttribute.Size = new System.Drawing.Size(168, 21);
      this.cboPatchAttribute.TabIndex = 16;
      // 
      // cmdSavePatchAttribute
      // 
      this.cmdSavePatchAttribute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.cmdSavePatchAttribute.Location = new System.Drawing.Point(276, 308);
      this.cmdSavePatchAttribute.Name = "cmdSavePatchAttribute";
      this.cmdSavePatchAttribute.Size = new System.Drawing.Size(68, 20);
      this.cmdSavePatchAttribute.TabIndex = 15;
      this.cmdSavePatchAttribute.Text = "Add/Save";
      this.cmdSavePatchAttribute.Click += new System.EventHandler(this.cmdSavePatchAttribute_Click);
      // 
      // cboPatchAttributeType
      // 
      this.cboPatchAttributeType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.cboPatchAttributeType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cboPatchAttributeType.Location = new System.Drawing.Point(448, 280);
      this.cboPatchAttributeType.Name = "cboPatchAttributeType";
      this.cboPatchAttributeType.Size = new System.Drawing.Size(88, 21);
      this.cboPatchAttributeType.TabIndex = 14;
      // 
      // lstPatchAttribute
      // 
      this.lstPatchAttribute.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)));
      this.lstPatchAttribute.Location = new System.Drawing.Point(276, 24);
      this.lstPatchAttribute.Name = "lstPatchAttribute";
      this.lstPatchAttribute.Size = new System.Drawing.Size(260, 251);
      this.lstPatchAttribute.TabIndex = 13;
      this.lstPatchAttribute.SelectedValueChanged += new System.EventHandler(this.lstPatchAttribute_SelectedValueChanged);
      // 
      // cmdDeletePatchEntity
      // 
      this.cmdDeletePatchEntity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.cmdDeletePatchEntity.Location = new System.Drawing.Point(80, 308);
      this.cmdDeletePatchEntity.Name = "cmdDeletePatchEntity";
      this.cmdDeletePatchEntity.Size = new System.Drawing.Size(52, 20);
      this.cmdDeletePatchEntity.TabIndex = 6;
      this.cmdDeletePatchEntity.Text = "Delete";
      this.cmdDeletePatchEntity.Click += new System.EventHandler(this.cmdDeletePatchEntity_Click);
      // 
      // cboPatchEntity
      // 
      this.cboPatchEntity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.cboPatchEntity.Location = new System.Drawing.Point(8, 280);
      this.cboPatchEntity.Name = "cboPatchEntity";
      this.cboPatchEntity.Size = new System.Drawing.Size(168, 21);
      this.cboPatchEntity.TabIndex = 5;
      // 
      // cmdSavePatchEntity
      // 
      this.cmdSavePatchEntity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.cmdSavePatchEntity.Location = new System.Drawing.Point(8, 308);
      this.cmdSavePatchEntity.Name = "cmdSavePatchEntity";
      this.cmdSavePatchEntity.Size = new System.Drawing.Size(68, 20);
      this.cmdSavePatchEntity.TabIndex = 4;
      this.cmdSavePatchEntity.Text = "Add/Save";
      this.cmdSavePatchEntity.Click += new System.EventHandler(this.cmdSavePatchEntity_Click);
      // 
      // cboPatchEntityType
      // 
      this.cboPatchEntityType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.cboPatchEntityType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cboPatchEntityType.Location = new System.Drawing.Point(180, 280);
      this.cboPatchEntityType.Name = "cboPatchEntityType";
      this.cboPatchEntityType.Size = new System.Drawing.Size(88, 21);
      this.cboPatchEntityType.TabIndex = 2;
      // 
      // lstPatchEntity
      // 
      this.lstPatchEntity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)));
      this.lstPatchEntity.Location = new System.Drawing.Point(8, 24);
      this.lstPatchEntity.Name = "lstPatchEntity";
      this.lstPatchEntity.Size = new System.Drawing.Size(260, 251);
      this.lstPatchEntity.TabIndex = 0;
      this.lstPatchEntity.SelectedValueChanged += new System.EventHandler(this.lstPatchEntity_SelectedValueChanged);
      // 
      // lblEntityPatchAttributes
      // 
      this.lblEntityPatchAttributes.AutoSize = true;
      this.lblEntityPatchAttributes.Location = new System.Drawing.Point(276, 8);
      this.lblEntityPatchAttributes.Name = "lblEntityPatchAttributes";
      this.lblEntityPatchAttributes.Size = new System.Drawing.Size(55, 13);
      this.lblEntityPatchAttributes.TabIndex = 18;
      this.lblEntityPatchAttributes.Text = "Attributes";
      // 
      // lblEntityPatchEntities
      // 
      this.lblEntityPatchEntities.AutoSize = true;
      this.lblEntityPatchEntities.Location = new System.Drawing.Point(8, 8);
      this.lblEntityPatchEntities.Name = "lblEntityPatchEntities";
      this.lblEntityPatchEntities.Size = new System.Drawing.Size(42, 13);
      this.lblEntityPatchEntities.TabIndex = 12;
      this.lblEntityPatchEntities.Text = "Entities";
      // 
      // cboExportDataProvider
      // 
      this.cboExportDataProvider.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cboExportDataProvider.Location = new System.Drawing.Point(112, 36);
      this.cboExportDataProvider.Name = "cboExportDataProvider";
      this.cboExportDataProvider.Size = new System.Drawing.Size(180, 21);
      this.cboExportDataProvider.TabIndex = 3;
      this.cboExportDataProvider.SelectedValueChanged += new System.EventHandler(this.cboExportDataProvider_SelectedValueChanged);
      // 
      // lblExportDataProvider
      // 
      this.lblExportDataProvider.AutoSize = true;
      this.lblExportDataProvider.Location = new System.Drawing.Point(12, 40);
      this.lblExportDataProvider.Name = "lblExportDataProvider";
      this.lblExportDataProvider.Size = new System.Drawing.Size(73, 13);
      this.lblExportDataProvider.TabIndex = 2;
      this.lblExportDataProvider.Text = "Data Provider";
      // 
      // cboExportDBPlatform
      // 
      this.cboExportDBPlatform.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cboExportDBPlatform.Location = new System.Drawing.Point(112, 8);
      this.cboExportDBPlatform.Name = "cboExportDBPlatform";
      this.cboExportDBPlatform.Size = new System.Drawing.Size(180, 21);
      this.cboExportDBPlatform.TabIndex = 1;
      this.cboExportDBPlatform.SelectedValueChanged += new System.EventHandler(this.cboExportDBPlatform_SelectedValueChanged);
      // 
      // lblExportDBPlatform
      // 
      this.lblExportDBPlatform.AutoSize = true;
      this.lblExportDBPlatform.Location = new System.Drawing.Point(12, 12);
      this.lblExportDBPlatform.Name = "lblExportDBPlatform";
      this.lblExportDBPlatform.Size = new System.Drawing.Size(96, 13);
      this.lblExportDBPlatform.TabIndex = 0;
      this.lblExportDBPlatform.Text = "Database platform";
      // 
      // tabPageXML
      // 
      this.tabPageXML.Controls.Add(this.txtXML);
      this.tabPageXML.Location = new System.Drawing.Point(4, 22);
      this.tabPageXML.Name = "tabPageXML";
      this.tabPageXML.Size = new System.Drawing.Size(676, 426);
      this.tabPageXML.TabIndex = 2;
      this.tabPageXML.Text = "XML";
      // 
      // txtXML
      // 
      this.txtXML.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtXML.BackColor = System.Drawing.SystemColors.Window;
      this.txtXML.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.txtXML.Location = new System.Drawing.Point(4, 4);
      this.txtXML.Multiline = true;
      this.txtXML.Name = "txtXML";
      this.txtXML.ReadOnly = true;
      this.txtXML.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.txtXML.Size = new System.Drawing.Size(632, 420);
      this.txtXML.TabIndex = 0;
      // 
      // cmdSaveProjectSettings
      // 
      this.cmdSaveProjectSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.cmdSaveProjectSettings.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.cmdSaveProjectSettings.Location = new System.Drawing.Point(836, 8);
      this.cmdSaveProjectSettings.Name = "cmdSaveProjectSettings";
      this.cmdSaveProjectSettings.Size = new System.Drawing.Size(52, 20);
      this.cmdSaveProjectSettings.TabIndex = 3;
      this.cmdSaveProjectSettings.Text = "Save";
      this.cmdSaveProjectSettings.Click += new System.EventHandler(this.cmdSaveProjectSettings_Click);
      // 
      // pnlEntitySelector
      // 
      this.pnlEntitySelector.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)));
      this.pnlEntitySelector.Controls.Add(this.cmdDeselectAll);
      this.pnlEntitySelector.Controls.Add(this.cmdSelectAll);
      this.pnlEntitySelector.Controls.Add(this.lstEntity);
      this.pnlEntitySelector.Location = new System.Drawing.Point(0, 32);
      this.pnlEntitySelector.Name = "pnlEntitySelector";
      this.pnlEntitySelector.Size = new System.Drawing.Size(204, 460);
      this.pnlEntitySelector.TabIndex = 4;
      // 
      // cmdDeselectAll
      // 
      this.cmdDeselectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.cmdDeselectAll.Location = new System.Drawing.Point(108, 436);
      this.cmdDeselectAll.Name = "cmdDeselectAll";
      this.cmdDeselectAll.Size = new System.Drawing.Size(92, 20);
      this.cmdDeselectAll.TabIndex = 2;
      this.cmdDeselectAll.Text = "Deselect all";
      this.cmdDeselectAll.Click += new System.EventHandler(this.cmdDeselectAll_Click);
      // 
      // cmdSelectAll
      // 
      this.cmdSelectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.cmdSelectAll.Location = new System.Drawing.Point(4, 436);
      this.cmdSelectAll.Name = "cmdSelectAll";
      this.cmdSelectAll.Size = new System.Drawing.Size(92, 20);
      this.cmdSelectAll.TabIndex = 1;
      this.cmdSelectAll.Text = "Select all";
      this.cmdSelectAll.Click += new System.EventHandler(this.cmdSelectAll_Click);
      // 
      // lstEntity
      // 
      this.lstEntity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)));
      this.lstEntity.IntegralHeight = false;
      this.lstEntity.Location = new System.Drawing.Point(4, 4);
      this.lstEntity.Name = "lstEntity";
      this.lstEntity.Size = new System.Drawing.Size(196, 428);
      this.lstEntity.TabIndex = 0;
      this.lstEntity.SelectedValueChanged += new System.EventHandler(this.lstEntity_SelectedValueChanged);
      // 
      // frmMain
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
      this.ClientSize = new System.Drawing.Size(892, 513);
      this.Controls.Add(this.pnlEntitySelector);
      this.Controls.Add(this.cmdSaveProjectSettings);
      this.Controls.Add(this.tabMain);
      this.Controls.Add(this.pbar);
      this.Controls.Add(this.sbStatus);
      this.Controls.Add(this.lblDBDefinition);
      this.Controls.Add(this.cmdDBDefinitionBrowse);
      this.Controls.Add(this.cboDBDefinition);
      this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.KeyPreview = true;
      this.MinimumSize = new System.Drawing.Size(864, 500);
      this.Name = "frmMain";
      this.Text = "PVEntityGenerator.NET";
      this.Load += new System.EventHandler(this.frmMain_Load);
      this.Closing += new System.ComponentModel.CancelEventHandler(this.frmMain_Closing);
      this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmMain_KeyDown);
      ((System.ComponentModel.ISupportInitialize)(this.sbarPanel)).EndInit();
      this.tabMain.ResumeLayout(false);
      this.tabPageProject.ResumeLayout(false);
      this.tabPageProject.PerformLayout();
      this.tabPageEntityGeneration.ResumeLayout(false);
      this.grpEntityGeneration.ResumeLayout(false);
      this.tabEntityGeneration.ResumeLayout(false);
      this.tabPageGlobalOptions.ResumeLayout(false);
      this.tabPageAttributes.ResumeLayout(false);
      this.tabPageAttributes.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dgEntityAttribute)).EndInit();
      this.tabPageGenerationOptions.ResumeLayout(false);
      this.tabPageGenerationOptions.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dgDeleteConstraint)).EndInit();
      this.tabPageEnumeration.ResumeLayout(false);
      this.tabPageEnumeration.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dgEntityEnumeration)).EndInit();
      this.tabPageSQLExport.ResumeLayout(false);
      this.tabPageSQLExport.PerformLayout();
      this.grpExportImportData.ResumeLayout(false);
      this.grpGenerateScript.ResumeLayout(false);
      this.tabExport.ResumeLayout(false);
      this.tabPageExport.ResumeLayout(false);
      this.tabPageExportEntityOptions.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.dgExportEntityOptions)).EndInit();
      this.tabPageExportEntityPatch.ResumeLayout(false);
      this.tabPageExportEntityPatch.PerformLayout();
      this.tabPageXML.ResumeLayout(false);
      this.tabPageXML.PerformLayout();
      this.pnlEntitySelector.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }
    #endregion

    private void cmdDBDefinitionBrowse_Click(object sender, System.EventArgs e) {
      OpenFileDialog dlg = new OpenFileDialog();
      dlg.AddExtension = true;
      dlg.CheckFileExists = true;
      dlg.CheckPathExists = true;
      dlg.DefaultExt = "mdb";
      dlg.Filter = "All database definition files (*.mdb, *.accdb, *.xml)|*.mdb;*.accdb;*.xml|Access database files (*.mdb, *.accdb)|*.mdb;*.accdb|XML files (*.xml)|*.xml|All files (*.*)|*.*";
      dlg.ShowDialog(this);
      if (dlg.FileName.Length!=0) {

        // Wenn PVEntityGenrerator Settings-File ausgewählt wird passende DBDefinition-Datei dazu suchen
        string strFilename = dlg.FileName;
        if (strFilename.ToLower().EndsWith(PROJECTSETTINGS_EXTENSION.ToLower())) {
          strFilename = dlg.FileName.Substring(0, dlg.FileName.Length - PROJECTSETTINGS_EXTENSION.Length) + ".mdb";
          if (!File.Exists(strFilename)) {
            strFilename = dlg.FileName.Substring(0, dlg.FileName.Length - PROJECTSETTINGS_EXTENSION.Length) + ".accdb";
          }
          if (!File.Exists(strFilename)) {
            strFilename = dlg.FileName.Substring(0, dlg.FileName.Length - PROJECTSETTINGS_EXTENSION.Length) + ".xml";
          }
          if (!File.Exists(strFilename)) {
            strFilename = dlg.FileName;
          }
        }

        cboDBDefinition_AddFile(strFilename);
        cboDBDefinition.Text = strFilename;
      }
    }

    private void cboDBDefinition_AddFile(String pFilename) {
      if (!cboDBDefinition.Items.Contains(pFilename)) {
        cboDBDefinition.Items.Add(pFilename);
      }
    }

    private void LoadDBDefinition(string pFilename) {
      if (this.Dirty) {
        switch (MessageBox.Show(this, "Do you want to save the project changes?", "Save changes",
          MessageBoxButtons.YesNo, MessageBoxIcon.Question)) {
          case DialogResult.Yes:
            SaveProjectSettings();
            break;
        }
      }

      try {
        mDBDefinition = null;
        LoadEntityList();
        txtXML.Text = "";
        mDBDefinitionFilename = null;
        mProjectSettingsFilename = null;
        mProjectSettings = null;
        mProjectSettings_CurrentPlatform = null;
        mProjectSettings_CurrentDbPlatform = null;
        mProjectSettings_CurrentDbProvider = null;

        cboPlatform.SelectedIndex = -1;
        cboExportDBPlatform.SelectedIndex = -1;
        cboExportDataProvider.SelectedIndex = -1;
        this.Dirty = false;

        this.Cursor = Cursors.WaitCursor;
        Application.DoEvents();

        if (!File.Exists(pFilename)) {
          throw new PVException("File not found.");
        }

        // DBDefinition laden
        DBDefinitionReader defreader = new DBDefinitionReader(mStatusHandler);
        if (pFilename.ToLower().EndsWith(".xml")) {
          defreader.LoadFromXml(pFilename);
        }
        else if (pFilename.ToLower().EndsWith(".mdb") || pFilename.ToLower().EndsWith(".accdb")) {
          defreader.LoadFromMDB(pFilename);
        }
        else {
          throw new PVException("Invalid DB Definition file '" + pFilename + "'. Only MDB and XML files allowed.");
        }

        mDBDefinition = defreader.GetDBDefinition();
        mDBDefinitionDocument = defreader.GetXmlDocument();
        txtXML.Text = defreader.GetXmlString();
        LoadEntityList();
        mDBDefinitionFilename = pFilename;

        // Project settings laden (falls vorhanden)
        bool fXmlFile=false;
        mProjectSettingsFilename = pFilename.Substring(0, pFilename.LastIndexOf(".")) + PROJECTSETTINGS_EXTENSION;
        if (File.Exists(mProjectSettingsFilename)) {
          // Aus XML laden
          XmlTextReader reader = new XmlTextReader(mProjectSettingsFilename);
          mProjectSettings = (projectsettings)mProjectSettingsSerializer.Deserialize(reader);
          reader.Close();
          fXmlFile=true;
        } else {
          // Neue Project settings anlegen
          mProjectSettings = new projectsettings();
          mProjectSettings.parameters = new parametersParameter[0];
          mProjectSettings.entitygeneration = new projectsettingsEntitygeneration();
          mProjectSettings.entityexport = new projectsettingsEntityexport();
          mProjectSettings.platforms = new projectsettingsPlatforms();
          mProjectSettings.dbplatforms = new projectsettingsDbplatforms();
        }
        mProjectSettings.pventitygeneratorversion = Application.ProductVersion;

        // Project settings in Controls darstellen
        LoadProjectSettings();

        //Prüfen auf _Export-Tabelle
        if (!fXmlFile) {
          string strMsg="Project-settings from older versions of SQLExporter or PVEntityGenerator found.\nDo you wish to import them?";

          if ((defreader.SysTableExport || defreader.SysTablePVEntityGenerator) &&
            MessageBox.Show(strMsg,this.Text,MessageBoxButtons.YesNo,MessageBoxIcon.Question)==
            DialogResult.Yes) {

            //import settings
            ImportOldProjectSettings(defreader.SysTableExport, defreader.SysTablePVEntityGenerator);

            //display settings
            RefreshPlatform();
            RefreshDBPlatform();
            LoadProjectSettings();

            this.Dirty=true;
          }
        }

        this.EnabledGlobal = true;
        this.DataTransferPossible = pFilename.ToLower().EndsWith(".mdb") || pFilename.ToLower().EndsWith(".accdb");
        if (fXmlFile) {
          this.Dirty = false;
        }


        // automatically store xml version of database model if activated
        if (pFilename.ToLower().EndsWith(".mdb") || pFilename.ToLower().EndsWith(".accdb")) {
          bool generateDatabaseModelFile = false;
          if (mProjectSettings.parameters != null) {
            foreach (parametersParameter param in mProjectSettings.parameters) {
              if (param.name.Equals("autogenerate-datamodel-xml") && PVFormatUtil.ParseBoolean(param.Value)) {
                generateDatabaseModelFile = true;
                break;
              }
            }
          }
          if (generateDatabaseModelFile) {
            try {
              string databaseModelFilename = pFilename.Substring(0, pFilename.LastIndexOf(".")) + DATABASEMODEL_EXTENSION;
              XmlWriter xmlwriter = XmlTextWriter.Create(databaseModelFilename, App.XML_WRITER_SETTINGS);
              xmlwriter.WriteStartDocument();
              xmlwriter.WriteComment(" This is a XML version of the Access Database data model automatically generated by PVEntityGenerator. ");
              xmlwriter.WriteComment(" Please do not modify this file manually - it is only intended to track database model changes in version control systems. ");
              mDBDefinitionSerializer.Serialize(xmlwriter, mDBDefinition);
              xmlwriter.WriteEndDocument();
              xmlwriter.Close();
            }
            catch (Exception) {
              // ignore
            }
          }
        }

      }
      catch (Exception ex) {
        mDBDefinition = null;
        mDBDefinitionDocument = null;
        LoadEntityList();
        txtXML.Text = "";
        mDBDefinitionFilename = null;
        mProjectSettingsFilename = null;
        this.EnabledGlobal = false;
        this.Dirty = false;

        MessageBox.Show(this, "Unable to load database definition from '" + pFilename + "':\n" + ex.Message + "\n" + ex.StackTrace,
          "Load database definition", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
      finally {
        this.Cursor = Cursors.Default;
      }

    }

    private void ImportOldProjectSettings(bool pfSysTableExport, bool pfSysTablePVEntityGenerator) {
      string strConnect = DBServerHelper.getDatabaseOleDbConnectionString(mDBDefinitionFilename);
      OleDbConnection con=null;
      try {
        con=new OleDbConnection(strConnect);
        con.Open();

        DBImportSettings import=new DBImportSettings(pfSysTableExport,pfSysTablePVEntityGenerator,
          mDBDefinitionFilename,mProjectSettings,mDBDefinition,con);

        if (pfSysTableExport) {
          //Table _Export
          import.ImportTable_Export();
        }

        if (pfSysTablePVEntityGenerator) {
          //Get all old parameters from table _PVEntityGenerator
          import.FetchOldParameters();

          //Set the platform & dbplatform if found
          if (import.Platform.Length>0 && !cboPlatform.Text.Equals(import.Platform)) {
            cboPlatform.Text=import.Platform;
          }

          if (import.DBPlatform.Length>0 && !cboExportDBPlatform.Text.Equals(import.DBPlatform)) {
            cboExportDBPlatform.Text=import.DBPlatform;
          }

          //Set all other paremters (i.e. outputdir or custom entity generate functions)
          import.ImportTable_PVEntityGenerator();
        }
      } finally {
        if (con!=null) con.Close();
      }
    }

    private void LoadProjectSettings() {
      try {
        mfDuringLoad = true;

        // Zunächst alle existierenden Entities und Parameter mit Default-Werten "auffüllen"
        CompleteProjectSettings();

        // Global
        if (mProjectSettings.platforms.selected!=null && mProjectSettings.platforms.selected.Length!=0) {
          cboPlatform.Text = mProjectSettings.platforms.selected;
        }

        // Supported db platforms, export db platform
        cboExportDBPlatform.Items.Clear();
        for (int intIndex=0; intIndex<lstSupportedDatabasePlatform.Items.Count; intIndex++) {
          string strDbPlatform = (string)lstSupportedDatabasePlatform.Items[intIndex];
          lstSupportedDatabasePlatform.SetItemChecked(intIndex, false);
          if (mProjectSettings.dbplatforms.dbplatform!=null) {
            foreach (projectsettingsDbplatformsDbplatform dbp in mProjectSettings.dbplatforms.dbplatform) {
              if (dbp.name.Equals(strDbPlatform)) {
                lstSupportedDatabasePlatform.SetItemChecked(intIndex, true);
                cboExportDBPlatform.Items.Add(strDbPlatform);
                break;
              }
            }
          }
        }
        if (mProjectSettings.dbplatforms.selected!=null && mProjectSettings.dbplatforms.selected.Length!=0) {
          cboExportDBPlatform.Text = mProjectSettings.dbplatforms.selected;
        }
        else if (cboExportDBPlatform.Items.Count!=0) {
          cboExportDBPlatform.SelectedIndex = 0;
        }

        // Project parameters
        PVPropertyGrid.SettingContainer settings = new PVPropertyGrid.SettingContainer();
        LoadPropertyGridSettings(settings, mConfig.projectdefinition.parameterdefinitions, mProjectSettings.parameters,
          new PVPropertyGrid.SettingEventHandler(pgProject_PropertyValueChanged));
        pgProject.Settings = settings;

        // Export settings
        if (mProjectSettings.dbplatforms.selected!=null && mProjectSettings.dbplatforms.selected.Length!=0) {
          cboExportDBPlatform.Text = mProjectSettings.dbplatforms.selected;
        }

        // Export Entity Options
        DataView dvExportEntityOptions = GridHelper.GetExportEntityOptions(mProjectSettings.entityexport, mDBDefinitionDocument);
        dvExportEntityOptions.Table.ColumnChanged += new DataColumnChangeEventHandler(dvExportEntityOptions_ColumnChanged);
        dvExportEntityOptions.Table.RowDeleting  += new DataRowChangeEventHandler(dvExportEntityOptions_RowDeleting);
        dgExportEntityOptions.DataSource = dvExportEntityOptions;

        //Export Entity Patch-Type
        string[] aItems=Enum.GetNames(typeof(type_PatchType));
        cboPatchEntityType.Items.Clear();
        cboPatchEntityType.Items.AddRange(aItems);
        cboPatchAttributeType.Items.Clear();
        cboPatchAttributeType.Items.AddRange(aItems);

        //Entity-Patch Combos
        cboPatchEntity.Items.Clear();
        if (mDBDefinition.entities!=null) {
          foreach (dbdefinitionEntity e in mDBDefinition.entities)
            cboPatchEntity.Items.Add(e.name);
        }

        lstPatchEntity.Items.Clear();
        if (mProjectSettings.entityexport.patchentities!=null) {
          foreach (projectsettingsEntityexportPatchentity e in mProjectSettings.entityexport.patchentities)
            lstPatchEntity.Items.Add(FormatPatchItem(e.entity ,e.patchtype));
        }

      }
      finally {
        mfDuringLoad = false;
      }
    }

    private string FormatPatchItem(string p1, type_PatchType pType) {
      return p1 + " {" + pType.ToString() + "}";
    }

    private string ExtractPatchItem(string p) {
      string[] aItems=Enum.GetNames(typeof(type_PatchType));
      foreach (string strType in aItems) {
        int intPos=p.IndexOf(" {" + strType + "}");
        if (intPos!=-1)
          return p.Substring(0,intPos);
      }
      return p;
    }

    private void pgProject_PropertyValueChanged(PVPropertyGrid.Setting pSetting, PropertyValueChangedEventArgs pArgs) {
      parametersParameter[] paramarray = mProjectSettings.parameters;
      if (ParameterHelper.SetPropertySetting(ref paramarray, pSetting)) {
        this.Dirty = true;
        mProjectSettings.parameters = paramarray;
      }
    }
    private void pgEntityGenerationGlobalOptions_PropertyValueChanged(PVPropertyGrid.Setting pSetting, PropertyValueChangedEventArgs pArgs) {
      parametersParameter[] paramarray = mProjectSettings_CurrentPlatform.parameters;
      if (ParameterHelper.SetPropertySetting(ref paramarray, pSetting)) {
        this.Dirty = true;
        mProjectSettings_CurrentPlatform.parameters = paramarray;
      }
    }
    private void pgSQLExport_DBPlatform_PropertyValueChanged(PVPropertyGrid.Setting pSetting, PropertyValueChangedEventArgs pArgs) {
      parametersParameter[] paramarray = mProjectSettings_CurrentDbPlatform.parameters;
      if (ParameterHelper.SetPropertySetting(ref paramarray, pSetting)) {
        this.Dirty = true;
        mProjectSettings_CurrentDbPlatform.parameters = paramarray;
      }
    }
    private void pgSQLExport_DBProvider_PropertyValueChanged(PVPropertyGrid.Setting pSetting, PropertyValueChangedEventArgs pArgs) {
      parametersParameter[] paramarray = mProjectSettings_CurrentDbProvider.parameters;
      if (ParameterHelper.SetPropertySetting(ref paramarray, pSetting)) {
        this.Dirty = true;
        mProjectSettings_CurrentDbProvider.parameters = paramarray;
      }
    }
    private void pgEntiyGenerationEntityOptions_PropertyValueChanged(PVPropertyGrid.Setting pSetting, PropertyValueChangedEventArgs pArgs) {
      parametersParameter[] paramarray = mCurrentEntity.parameters;
      if (ParameterHelper.SetPropertySetting(ref paramarray, pSetting)) {
        this.Dirty = true;
        mCurrentEntity.parameters = paramarray;
      }
      if (pSetting.Key.Equals("generate-enumeration")) {
        CheckEntityEnumerationVisible();
      }
    }
    private void pgEntiyGenerationEntityOptions_Platform_PropertyValueChanged(PVPropertyGrid.Setting pSetting, PropertyValueChangedEventArgs pArgs) {
      parametersParameter[] paramarray = mCurrentEntityPlatform.parameters;
      if (ParameterHelper.SetPropertySetting(ref paramarray, pSetting)) {
        this.Dirty = true;
        mCurrentEntityPlatform.parameters = paramarray;
      }
    }

    private void CheckEntityEnumerationVisible() {
      int intOldTabPage = tabEntityGeneration.SelectedIndex;
      mfEntityEnumeration = false;
      if (mCurrentEntity!=null && mCurrentEntity.parameters!=null) {
        foreach (parametersParameter param in mCurrentEntity.parameters) {
          if (param.name.Equals("generate-enumeration")) {
            mfEntityEnumeration = PVFormatUtil.ParseBoolean(param.Value);
          }
        }
      }
      if (!mfEntityEnumeration) {
        if (tabEntityGeneration.TabPages.Contains(tabPageEnumeration)) {
          tabEntityGeneration.TabPages.Remove(tabPageEnumeration);
        }
      }
      else {
        if (!tabEntityGeneration.TabPages.Contains(tabPageEnumeration)) {
          tabEntityGeneration.TabPages.Add(tabPageEnumeration);
        }
        bool fDefChanged = false;
        DataView dvEnumeration = GridHelper.GetEnumeration(mCurrentEntity, cboDBDefinition.Text, ref fDefChanged);
        dvEnumeration.Table.ColumnChanged += new DataColumnChangeEventHandler(dtEnumeration_ColumnChanged);
        dgEntityEnumeration.DataSource = dvEnumeration;
      }
      if (tabEntityGeneration.TabCount > intOldTabPage) {
        tabEntityGeneration.SelectedIndex = intOldTabPage;
      }
      else {
        tabEntityGeneration.SelectedIndex = 0;
      }
    }

    private void CheckAttributeDetailsVisible() {
      int intOldTabPage = tabEntityGeneration.SelectedIndex;
      mfAttributeDetails = !(mProjectSettings_CurrentPlatform==null || mCurrentEntityName==null || mCurrentEntityName.Length==0
        || mCurrentEntityName.Equals(FileGenerator.GLOBAL_ITEMS));
      if (!mfAttributeDetails) {
        if (tabEntityGeneration.TabPages.Contains(tabPageAttributes)) {
          tabEntityGeneration.TabPages.Remove(tabPageAttributes);
          tabEntityGeneration.TabPages.Remove(tabPageGenerationOptions);
        }
      }
      else {
        if (!tabEntityGeneration.TabPages.Contains(tabPageAttributes)) {
          if (tabEntityGeneration.TabPages.Contains(tabPageEnumeration)) {
            tabEntityGeneration.TabPages.Remove(tabPageEntityGeneration);
          }
          tabEntityGeneration.TabPages.Add(tabPageAttributes);
          tabEntityGeneration.TabPages.Add(tabPageGenerationOptions);
          if (mfEntityEnumeration) {
            tabEntityGeneration.TabPages.Add(tabPageEnumeration);
          }
        }
      }
      if (tabEntityGeneration.TabCount > intOldTabPage) {
        tabEntityGeneration.SelectedIndex = intOldTabPage;
      }
      else {
        tabEntityGeneration.SelectedIndex = 0;
      }
    }


    private void LoadPropertyGridSettings(PVPropertyGrid.SettingContainer pSettings,
        parameterdefinitionsParameterdefinition[] pParameterDefinitions,
        parametersParameter[] pParameters,
        PVPropertyGrid.SettingEventHandler pEventHandler) {

      foreach (parameterdefinitionsParameterdefinition paramdef in pParameterDefinitions) {

        // Wert ermitteln: Default-Wert bzw. Wert aus Project-Settings, falls gesetzt
        string strValue = paramdef.Value;
        if (strValue==null) {
          strValue = "";
        }
        foreach (parametersParameter param in pParameters) {
          if (paramdef.name.Equals(param.name)) {
            if (param.Value!=null) {
              strValue = param.Value;
            }
            break;
          }
        }

        // Wert in korrekten Typ konvertieren
        object objValue = null;
        UITypeEditor editor = null;

        switch (paramdef.type) {
          case parameterdefinitionsParameterdefinitionType.BIT:
            objValue = PVFormatUtil.ParseBoolean(strValue);
            break;
          case parameterdefinitionsParameterdefinitionType.CLOB:
            objValue = strValue;
            editor = new ClobEditor();
            break;
          case parameterdefinitionsParameterdefinitionType.INTEGER:
            objValue = PVFormatUtil.ParseInt(strValue);
            break;
          case parameterdefinitionsParameterdefinitionType.PASSWORD:
            objValue = strValue;
            editor = new PasswordEditor();
            break;
          case parameterdefinitionsParameterdefinitionType.PATH:
            objValue = strValue;
            editor = new PathEditor();
            break;
          case parameterdefinitionsParameterdefinitionType.TIMESTAMP:
            try {
              objValue = DateTime.ParseExact(strValue, "s", null);
            }
            catch (Exception) {
              objValue = DateTime.Today;
            }
            break;
          case parameterdefinitionsParameterdefinitionType.VARCHAR:
            objValue = strValue;
            break;
        }

        // Setting hinzufügen
        pSettings[paramdef.externalname] = new PVPropertyGrid.Setting(
          paramdef.name,
          paramdef.description,
          paramdef.category,
          objValue,
          pEventHandler,
          editor
        );

      }

    }

    private void LoadEntityList() {
      lstEntity.Items.Clear();
      mCurrentEntityName = null;
      mCurrentEntity = null;
      mCurrentEntityPlatform = null;

      if (mDBDefinition==null) {
        return;
      }

      int intIndex = lstEntity.Items.Add(FileGenerator.GLOBAL_ITEMS);
      lstEntity.SetItemChecked(intIndex, true);

      foreach (dbdefinitionEntity entity in mDBDefinition.entities) {
        intIndex = lstEntity.Items.Add(entity.name);
        lstEntity.SetItemChecked(intIndex, true);
      }

      LoadEntitySettings();
    }

    private void frmMain_Load(object sender, System.EventArgs e) {
      mStatusHandler = new StatusHandler();
      mStatusHandler.Status +=new StatusHandler.StatusEventHandler(ShowStatus);
      mStatusHandler.Error +=new PVEntityGenerator.StatusHandler.ErrorEventHandler(mStatusHandler_Error);

      LoadConfig();
      LoadEntitySettings();

      FormUtil.RestoreWindowPos(this);

      // restore DBDefinition combo entries
      RegistryKey key = RegistryUtil.GetUserKey(this);
      int intCount = (int)key.GetValue("dbdefinition_count", 0);
      for (int intIndex=0; intIndex<intCount; intIndex++) {
        String strItem = (string)key.GetValue("dbdefinition_"+intIndex, "");
        if (strItem.Length!=0) {
          cboDBDefinition.Items.Add(strItem);
        }
      }

      // restore selected tabs
      int intSelectedTab = (int)key.GetValue("tabMain_SelectedIndex", 0);
      if (intSelectedTab < tabMain.TabPages.Count) {
        tabMain.SelectedIndex = intSelectedTab;
      }
      intSelectedTab = (int)key.GetValue("tabEntityGeneration_SelectedIndex", 0);
      if (intSelectedTab < tabEntityGeneration.TabPages.Count) {
        tabEntityGeneration.SelectedIndex = intSelectedTab;
      }
      intSelectedTab = (int)key.GetValue("tabExport_SelectedIndex", 0);
      if (intSelectedTab < tabExport.TabPages.Count) {
        tabExport.SelectedIndex = intSelectedTab;
      }

      // select first definition or load from startup options
      if (mStartupOptions.DBDefinitionFilename != null) {
        cboDBDefinition_AddFile(mStartupOptions.DBDefinitionFilename);
        cboDBDefinition.Text = mStartupOptions.DBDefinitionFilename;
      }
      else {
        int intSelectedIndex = (int)key.GetValue("dbdefinition_selected", 0);
        if (cboDBDefinition.Items.Count > 0) {
          if (cboDBDefinition.Items.Count > intSelectedIndex) {
            cboDBDefinition.SelectedIndex = intSelectedIndex;
          }
          else {
            cboDBDefinition.SelectedIndex = 0;
          }
        }
        else {
          this.EnabledGlobal = false;
        }
      }

      // Property Grids initialisieren
      pgProject.HelpVisible = true;
      pgProject.ToolbarVisible = true;
      pgEntiyGenerationEntityOptions.HelpVisible = true;
      pgEntiyGenerationEntityOptions.ToolbarVisible = true;
      pgEntiyGenerationGlobalOptions.HelpVisible = true;
      pgEntiyGenerationGlobalOptions.ToolbarVisible = true;
      pgSQLExport.HelpVisible = true;
      pgSQLExport.ToolbarVisible = true;

      // Data Grids initialisieren
      dgEntityAttribute.TableStyles.Add(GridHelper.GetAttributeTableStyle());
      dgDeleteConstraint.TableStyles.Add(GridHelper.GetDeleteConstraintsTableStyle());
      dgEntityEnumeration.TableStyles.Add(GridHelper.GetEnumerationTableStyle());
      dgExportEntityOptions.TableStyles.Add(GridHelper.GetExportEntityOptionsTableStyle());


      // preselect database platform
      if (mStartupOptions.DBPlatformName != null) {
        cboExportDBPlatform.Text = mStartupOptions.DBPlatformName;
      }

      // Handle startup options - quit after execution
      if (mStartupOptions.HasCommandLineAction()) {
        Application.DoEvents();

        if (mStartupOptions.ExportData) {
          TransferData(true, false, null, true);
        }

        if (mStartupOptions.ImportData) {
          TransferData(false, false, null, true);
        }

        if (mStartupOptions.ExportScript && mStartupOptions.ExportScriptFilename!=null) {
          TransferData(true, true, mStartupOptions.ExportScriptFilename, true);
        }

        if (mStartupOptions.CreateEntityScript && mStartupOptions.CreateEntityScriptFilename!=null) {
          CreateEntityScript(true, mStartupOptions.CreateEntityScriptFilename);
        }

        if (mStartupOptions.PatchEntityScript && mStartupOptions.PatchEntityScriptFilename!=null) {
          PatchEntityScript(true, mStartupOptions.PatchEntityScriptFilename);
        }

        if (mStartupOptions.GenerateEntityFiles) {
          GenerateEntityFiles(true);
        }

        Application.Exit();
      }
      
    }

    private void LoadConfig() {
      cboPlatform.Items.Clear();
      lstSupportedDatabasePlatform.Items.Clear();
      cboExportDBPlatform.Items.Clear();

      DirectoryInfo dir = new DirectoryInfo(Properties.Settings.Default.xml_config);
      mConfigDir = dir.FullName;
      if (!mConfigDir.EndsWith("\\")) {
        mConfigDir += "\\";
      }

      XmlSerializer configserializer = new XmlSerializer(typeof(pventitygeneratorconfig));
      XmlSerializer platformserializer = new XmlSerializer(typeof(platformdefinition));
      XmlSerializer dbplatformserializer = new XmlSerializer(typeof(dbplatformdefinition));

      XmlTextReader xmlreader = new XmlTextReader(mConfigDir + "PVEntityGenerator.xml");
      mConfig = (pventitygeneratorconfig)configserializer.Deserialize(xmlreader);
      xmlreader.Close();

      // Platforms
      mhashPlatform = new Hashtable();
      foreach (pventitygeneratorconfigPlatform platform in mConfig.platforms) {
        string strConfigXML = mConfigDir + platform.definitiondir + "/platform.xml";

        xmlreader = new XmlTextReader(strConfigXML);
        platformdefinition platformdef = (platformdefinition)platformserializer.Deserialize(xmlreader);
        xmlreader.Close();

        mhashPlatform.Add(platform.name, platformdef);

        cboPlatform.Items.Add(platform.name);
      }

      // DB Platforms
      mhashDBPlatform = new Hashtable();
      foreach (pventitygeneratorconfigDbplatform dbplatform in mConfig.dbplatforms) {
        string strConfigXML = mConfigDir + dbplatform.definitiondir + "/db-platform.xml";

        xmlreader = new XmlTextReader(strConfigXML);
        dbplatformdefinition dbplatformdef = (dbplatformdefinition)dbplatformserializer.Deserialize(xmlreader);
        xmlreader.Close();

        mhashDBPlatform.Add(dbplatform.name, dbplatformdef);

        lstSupportedDatabasePlatform.Items.Add(dbplatform.name);
        cboExportDBPlatform.Items.Add(dbplatform.name);
      }

    }

    private void frmMain_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
      if (this.Dirty) {
        switch (MessageBox.Show(this, "Do you want to save the project changes?", "Save changes",
            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)) {
          case DialogResult.Yes:
            SaveProjectSettings();
            break;
          case DialogResult.Cancel:
            e.Cancel = true;
            return;
        }
      }

      FormUtil.SaveWindowPos(this);

      // save DBDefinition combo entries
      RegistryKey key = RegistryUtil.GetUserKey(this);
      key.SetValue("dbdefinition_count", cboDBDefinition.Items.Count);
      for (int intIndex=0; intIndex<cboDBDefinition.Items.Count; intIndex++) {
        key.SetValue("dbdefinition_" + intIndex, cboDBDefinition.Items[intIndex]);
      }
      key.SetValue("dbdefinition_selected", cboDBDefinition.SelectedIndex);

      // save selected tabs
      key.SetValue("tabMain_SelectedIndex", tabMain.SelectedIndex);
      key.SetValue("tabEntityGeneration_SelectedIndex", tabEntityGeneration.SelectedIndex);
      key.SetValue("tabExport_SelectedIndex", tabExport.SelectedIndex);
    }

    private void ShowStatus(object pSender, StatusHandler.StatusHandlerEventArgs pArgs) {
      sbarPanel.Text = pArgs.Status;
      pbar.Maximum = pArgs.MaxValue;
      pbar.Value = pArgs.ActualValue;
      Application.DoEvents();
    }

    private void cmdSelectAll_Click(object sender, System.EventArgs e) {
      for (int intIndex=0, intCount=lstEntity.Items.Count; intIndex<intCount; intIndex++) {
        lstEntity.SetItemChecked(intIndex, true);
      }
    }

    private void cmdDeselectAll_Click(object sender, System.EventArgs e) {
      for (int intIndex=0, intCount=lstEntity.Items.Count; intIndex<intCount; intIndex++) {
        lstEntity.SetItemChecked(intIndex, false);
      }
    }

    private void LoadExportDBPlatformProvider(string pDBPlatform) {
      cboExportDataProvider.Items.Clear();
      if (!mhashDBPlatform.ContainsKey(pDBPlatform)) {
        return;
      }

      dbplatformdefinition dbplatformdef = (dbplatformdefinition)mhashDBPlatform[pDBPlatform];

      foreach (dbplatformdefinitionDbproviderdefinition provider in dbplatformdef.dbproviderdefinitions) {
        cboExportDataProvider.Items.Add(provider.name);
        if (mProjectSettings_CurrentDbPlatform.dbproviders.selected!=null && mProjectSettings_CurrentDbPlatform.dbproviders.selected.Equals(provider.name)) {
          cboExportDataProvider.SelectedIndex = cboExportDataProvider.Items.Count - 1;
        }
      }
      if (mProjectSettings_CurrentDbPlatform.dbproviders.selected!=null && mProjectSettings_CurrentDbPlatform.dbproviders.selected.Length!=0) {
        cboExportDataProvider.Text = mProjectSettings_CurrentDbPlatform.dbproviders.selected;
      }
      else if (cboExportDataProvider.Items.Count > 0) {
        cboExportDataProvider.SelectedIndex = 0;
      }
    }

    private void LoadExportProperties() {

      // Global Entity Generation parameter
      PVPropertyGrid.SettingContainer settings = new PVPropertyGrid.SettingContainer();

      if (mProjectSettings_CurrentDbPlatform!=null) {
        dbplatformdefinition dbplatformdef = (dbplatformdefinition)mhashDBPlatform[mProjectSettings_CurrentDbPlatform.name];

        LoadPropertyGridSettings(settings, dbplatformdef.parameterdefinitions, mProjectSettings_CurrentDbPlatform.parameters,
          new PVPropertyGrid.SettingEventHandler(pgSQLExport_DBPlatform_PropertyValueChanged));

        if (mProjectSettings_CurrentDbProvider!=null) {
          foreach (dbplatformdefinitionDbproviderdefinition dbproviderdef in dbplatformdef.dbproviderdefinitions) {
            if (dbproviderdef.name.Equals(mProjectSettings_CurrentDbProvider.name)) {

              LoadPropertyGridSettings(settings, dbproviderdef.parameterdefinitions, mProjectSettings_CurrentDbProvider.parameters,
                new PVPropertyGrid.SettingEventHandler(pgSQLExport_DBProvider_PropertyValueChanged));

              break;
            }
          }
        }
      }


      pgSQLExport.Settings = settings;

    }

    private void cboDBDefinition_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {
      if (e.KeyCode==Keys.Delete && cboDBDefinition.SelectedIndex >= 0) {
        cboDBDefinition.Items.RemoveAt(cboDBDefinition.SelectedIndex);
      }
    }

    private void cmdSaveProjectSettings_Click(object sender, System.EventArgs e) {
      SaveProjectSettings();
    }

    private void SaveProjectSettings() {
      try {
        XmlWriter xmlwriter = XmlTextWriter.Create(mProjectSettingsFilename, App.XML_WRITER_SETTINGS);
        mProjectSettingsSerializer.Serialize(xmlwriter, mProjectSettings);
        xmlwriter.Close();

        this.Dirty = false;
      }
      catch (Exception ex) {
        MessageBox.Show(this, "Unable to save project settings to '" + mProjectSettingsFilename + "':\n" + ex.Message + "\n" + ex.StackTrace,
          "Save project settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
    }

    private void cboExportDBPlatform_SelectedValueChanged(object sender, System.EventArgs e) {
      if (mProjectSettings==null) return;

      mProjectSettings.dbplatforms.selected = cboExportDBPlatform.Text;
      RefreshDBPlatform();
    }

    private void RefreshDBPlatform() {
      if (mProjectSettings==null) return;
      this.Dirty = true;

      if (cboExportDBPlatform.Text.Length!=0) {
        foreach (projectsettingsDbplatformsDbplatform dbplatform in mProjectSettings.dbplatforms.dbplatform) {
          if (dbplatform.name.Equals(cboExportDBPlatform.Text)) {
            mProjectSettings_CurrentDbPlatform = dbplatform;
            break;
          }
        }

        LoadExportDBPlatformProvider(cboExportDBPlatform.Text);
      }
    }

    private void cboPlatform_SelectedValueChanged(object sender, System.EventArgs e) {
      if (mProjectSettings==null) return;

      mProjectSettings.platforms.selected = cboPlatform.Text;
      RefreshPlatform();
    }

    private void RefreshPlatform() {
      if (mProjectSettings==null) return;

      // Set selected platform
      if (mProjectSettings.platforms.platform==null || mProjectSettings.platforms.platform.Length==0) {
        projectsettingsPlatformsPlatform platform = new projectsettingsPlatformsPlatform();
        platform.entitygeneration = new projectsettingsEntitygeneration();
        platform.parameters = new parametersParameter[0];
        mProjectSettings.platforms.platform = new projectsettingsPlatformsPlatform[] { platform };
      }
      mProjectSettings_CurrentPlatform = mProjectSettings.platforms.platform[0];
      mProjectSettings_CurrentPlatform.name = cboPlatform.Text;

      // Platform definition
      platformdefinition platformdef = (platformdefinition)mhashPlatform[cboPlatform.Text];

      // Global Entity Generation parameter
      PVPropertyGrid.SettingContainer settings = new PVPropertyGrid.SettingContainer();
      LoadPropertyGridSettings(settings, platformdef.parameterdefinitions, mProjectSettings_CurrentPlatform.parameters,
        new PVPropertyGrid.SettingEventHandler(pgEntityGenerationGlobalOptions_PropertyValueChanged));
      pgEntiyGenerationGlobalOptions.Settings = settings;

      // Zunächst alle existierenden Entities und Parameter mit Default-Werten "auffüllen"
      CompleteProjectSettings();

      this.Dirty = true;
    }

    private void cboDBDefinition_SelectedIndexChanged(object sender, System.EventArgs e) {
      if (cboDBDefinition.Text.Length!=0) {
        LoadDBDefinition(cboDBDefinition.Text);
      }
    }

    private void frmMain_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {
      if (e.KeyCode==Keys.S && e.Control) {
        cmdDBDefinitionBrowse.Select();
        if (this.Dirty) {
          SaveProjectSettings();
        }
      }
    }

    private void lstSupportedDatabasePlatform_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e) {
      if (mfDuringLoad) {
        return;
      }

      string strDbPlatform = (string)lstSupportedDatabasePlatform.Items[e.Index];

      // Zunächste alle DBPlatform-Einträge sammeln, die gecheckt und bereits vorhanden sind
      ArrayList alDbPlatforms = new ArrayList();
      if (mProjectSettings.dbplatforms.dbplatform!=null) {
        alDbPlatforms.AddRange(mProjectSettings.dbplatforms.dbplatform);
      }

      // DBPlatform hinzufügen oder entfernen
      if (e.NewValue==CheckState.Checked) {
        projectsettingsDbplatformsDbplatform dbplatform = new projectsettingsDbplatformsDbplatform();
        dbplatform.name = strDbPlatform;
        dbplatform.parameters = new parametersParameter[0];
        dbplatform.dbproviders = new projectsettingsDbplatformsDbplatformDbproviders();
        dbplatform.scriptgeneration = new type_generation();
        alDbPlatforms.Add(dbplatform);
        cboExportDBPlatform.Items.Add(strDbPlatform);
      }
      else if (e.NewValue==CheckState.Unchecked) {
        foreach (projectsettingsDbplatformsDbplatform dbplatform in mProjectSettings.dbplatforms.dbplatform) {
          if (dbplatform.name.Equals(strDbPlatform)) {
            alDbPlatforms.Remove(dbplatform);
            cboExportDBPlatform.Items.Remove(strDbPlatform);
            break;
          }
        }
      }

      mProjectSettings.dbplatforms.dbplatform = (projectsettingsDbplatformsDbplatform[])alDbPlatforms.ToArray(typeof(projectsettingsDbplatformsDbplatform));

      // export db platform neu wählen
      if (cboExportDBPlatform.Items.Count!=0) {
        cboExportDBPlatform.SelectedIndex = 0;
      }

      this.Dirty = true;
    }

    private void cboExportDataProvider_SelectedValueChanged(object sender, System.EventArgs e) {
      if (mProjectSettings_CurrentDbPlatform==null) {
        return;
      }

      if (mProjectSettings_CurrentDbPlatform.dbproviders.selected==null
          || !mProjectSettings_CurrentDbPlatform.dbproviders.selected.Equals(cboExportDataProvider.Text)) {
        mProjectSettings_CurrentDbPlatform.dbproviders.selected = cboExportDataProvider.Text;
        this.Dirty = true;
      }

      mProjectSettings_CurrentDbProvider = null;
      if (mProjectSettings_CurrentDbPlatform.dbproviders.dbprovider!=null) {
        foreach (projectsettingsDbplatformsDbplatformDbprovidersDbprovider dbp in mProjectSettings_CurrentDbPlatform.dbproviders.dbprovider) {
          if (dbp.name.Equals(cboExportDataProvider.Text)) {
            mProjectSettings_CurrentDbProvider = dbp;
          }
        }
      }
      if (mProjectSettings_CurrentDbProvider==null) {
        mProjectSettings_CurrentDbProvider = new projectsettingsDbplatformsDbplatformDbprovidersDbprovider();
        mProjectSettings_CurrentDbProvider.name = cboExportDataProvider.Text;
        mProjectSettings_CurrentDbProvider.parameters = new parametersParameter[0];

        ArrayList alDbProvider = new ArrayList();
        if (mProjectSettings_CurrentDbPlatform.dbproviders.dbprovider!=null) {
          alDbProvider.AddRange(mProjectSettings_CurrentDbPlatform.dbproviders.dbprovider);
        }
        alDbProvider.Add(mProjectSettings_CurrentDbProvider);
        mProjectSettings_CurrentDbPlatform.dbproviders.dbprovider
          = (projectsettingsDbplatformsDbplatformDbprovidersDbprovider[])alDbProvider.ToArray(typeof(projectsettingsDbplatformsDbplatformDbprovidersDbprovider));

      }

      LoadExportProperties();
    }

    private void lstEntity_SelectedValueChanged(object sender, System.EventArgs e) {
      mCurrentEntityName = lstEntity.Text;
      LoadEntitySettings();
    }

    private void LoadEntitySettings() {
      if (mProjectSettings_CurrentPlatform==null || mCurrentEntityName==null || mCurrentEntityName.Length==0
          || mCurrentEntityName.Equals(FileGenerator.GLOBAL_ITEMS)) {

        mCurrentEntityName = null;
        mCurrentEntity = null;
        mCurrentEntityPlatform = null;

        lblEntity.Text = "";
        lblEntity2.Text = "";
        lblEntity3.Text = "";
        pgEntiyGenerationEntityOptions.Settings = new PVPropertyGrid.SettingContainer();
        dgEntityAttribute.DataSource = null;

        dgDeleteConstraint.DataSource = null;
        lstCustomCreateMethod.Items.Clear();
        cmdCustomCreateMethodAdd.Enabled = false;
        cmdCustomCreateMethodRemove.Enabled = false;
        lstCustomFindMethod.Items.Clear();
        cmdCustomFindMethodAdd.Enabled = false;
        cmdCustomFindMethodRemove.Enabled = false;

        CheckAttributeDetailsVisible();
        CheckEntityEnumerationVisible();
        dgEntityEnumeration.DataSource = null;

        return;
      }

      try {
        this.Cursor = Cursors.WaitCursor;

        lblEntity.Text = mCurrentEntityName;
        lblEntity2.Text = mCurrentEntityName;
        lblEntity3.Text = mCurrentEntityName;

        platformdefinition platformdef = (platformdefinition)mhashPlatform[mProjectSettings_CurrentPlatform.name];

        mCurrentEntity = GetGenerationEntity(mCurrentEntityName);
        mCurrentEntityPlatform = GetGenerationEntityPlatform(mCurrentEntityName);

        // Entity Properties laden
        PVPropertyGrid.SettingContainer settings = new PVPropertyGrid.SettingContainer();

        if (mConfig.entitygeneration.generateentity.parameterdefinitions!=null) {
          LoadPropertyGridSettings(settings, mConfig.entitygeneration.generateentity.parameterdefinitions, mCurrentEntity.parameters,
            new PVPropertyGrid.SettingEventHandler(pgEntiyGenerationEntityOptions_PropertyValueChanged));
        }
        if (platformdef.entitygeneration.generateentity.parameterdefinitions!=null) {
          LoadPropertyGridSettings(settings, platformdef.entitygeneration.generateentity.parameterdefinitions, mCurrentEntityPlatform.parameters,
            new PVPropertyGrid.SettingEventHandler(pgEntiyGenerationEntityOptions_Platform_PropertyValueChanged));
        }

        pgEntiyGenerationEntityOptions.Settings = settings;

        // Attributes Grid
        dgEntityAttribute.DataSource = GridHelper.GetAttributes(mDBDefinition, mCurrentEntityName);

        // Custom Constraint Messages
        DataView dvDeleteConstraint = GridHelper.GetDeleteConstraints(mDBDefinitionDocument, mCurrentEntity);
        dvDeleteConstraint.Table.ColumnChanged += new DataColumnChangeEventHandler(dtDeleteConstraint_ColumnChanged);
        dgDeleteConstraint.DataSource = dvDeleteConstraint;

        LoadCustomCreateMethods();
        cmdCustomCreateMethodAdd.Enabled = true;
        cmdCustomCreateMethodRemove.Enabled = true;

        LoadCustomFindMethods();
        cmdCustomFindMethodAdd.Enabled = true;
        cmdCustomFindMethodRemove.Enabled = true;

        CheckAttributeDetailsVisible();
        CheckEntityEnumerationVisible();
      }
      finally {
        this.Cursor = Cursors.Default;
      }
    }

    private void LoadCustomCreateMethods() {
      lstCustomCreateMethod.Items.Clear();

      if (mCurrentEntity.customcreatemethods!=null) {
        foreach (type_generationGenerateentityCustomcreatemethod method in mCurrentEntity.customcreatemethods) {
          StringBuilder sbAttr = new StringBuilder();
          if (method.methodattribute!=null) {
            foreach (type_generationGenerateentityCustomcreatemethodMethodattribute attr in method.methodattribute) {
              if (sbAttr.Length!=0) {
                sbAttr.Append(",");
              }
              sbAttr.Append(attr.name);
            }
          }
          lstCustomCreateMethod.Items.Add(method.name + "(" + sbAttr.ToString() + ")");
        }
      }
    }

    private void LoadCustomFindMethods() {
      lstCustomFindMethod.Items.Clear();

      if (mCurrentEntity.customfindmethods!=null) {
        foreach (type_generationGenerateentityCustomfindmethod method in mCurrentEntity.customfindmethods) {
          StringBuilder sbAttr = new StringBuilder();
          if (method.methodattribute!=null) {
            foreach (type_generationGenerateentityCustomfindmethodMethodattribute attr in method.methodattribute) {
              if (sbAttr.Length!=0) {
                sbAttr.Append(",");
              }
              sbAttr.Append(attr.name);
            }
          }
          lstCustomFindMethod.Items.Add(method.name + "(" + sbAttr.ToString() + ")");
        }
      }
    }

    public bool EnabledGlobal {
      get {
        return tabMain.Enabled;
      }
      set {
        tabMain.Enabled = value;
        lstEntity.Enabled = value;
        cmdSelectAll.Enabled = value;
        cmdDeselectAll.Enabled = value;
        cmdSaveProjectSettings.Enabled = value && this.Dirty;
      }
    }

    public bool Dirty {
      get {
        return mfDirty;
      }
      set {
        mfDirty = value;
        cmdSaveProjectSettings.Enabled = this.EnabledGlobal && value;
      }
    }

    public bool DataTransferPossible {
      get {
        return mfDataTransferPossible;
      }
      set {
        mfDataTransferPossible = value;
        cmdExportTransfer.Enabled = value;
        optTransferExport.Enabled = value;
        optTransferImport.Enabled = value;
        grpExportImportData.Enabled = value;
      }
    }

    private void dtDeleteConstraint_ColumnChanged(object sender, DataColumnChangeEventArgs e) {
      if (mCurrentEntity!=null) {
        string strForeignEntity = (string)e.Row["ForeignEntity"];
        string strMessage = (string)e.Row["Message"];
        if (strMessage==null) {
          strMessage = "";
        }

        type_generationGenerateentityRemoveconstraintmessage message = null;
        if (mCurrentEntity.removeconstraintmessages!=null) {
          foreach (type_generationGenerateentityRemoveconstraintmessage msg in mCurrentEntity.removeconstraintmessages) {
            if (msg.foreignentity.Equals(strForeignEntity)) {
              message = msg;
              break;
            }
          }
        }
        if (message==null) {
          ArrayList alMessages = new ArrayList();
          if (mCurrentEntity.removeconstraintmessages!=null) {
            alMessages.AddRange(mCurrentEntity.removeconstraintmessages);
          }
          message = new type_generationGenerateentityRemoveconstraintmessage();
          message.foreignentity = strForeignEntity;
          alMessages.Add(message);
          mCurrentEntity.removeconstraintmessages = (type_generationGenerateentityRemoveconstraintmessage[])alMessages.ToArray(typeof(type_generationGenerateentityRemoveconstraintmessage));
        }

        message.Value = strMessage;
        this.Dirty = true;
      }
    }

    private void lstCustomCreateMethod_DoubleClick(object sender, System.EventArgs e) {
      if (lstCustomCreateMethod.SelectedIndex >= 0) {
        type_generationGenerateentityCustomcreatemethod method = mCurrentEntity.customcreatemethods[lstCustomCreateMethod.SelectedIndex];

        fdlgCustomCreateMethod dlg = new fdlgCustomCreateMethod();
        dlg.PlatformDefinition = (platformdefinition)mhashPlatform[cboPlatform.Text];
        dlg.Entity = mCurrentEntityName;
        dlg.DbDefinitionDocument = mDBDefinitionDocument;
        dlg.CustomCreateMethod = method;
        dlg.Initialize();

        if (dlg.ShowDialog(this)==DialogResult.OK) {
          this.Dirty = true;
          LoadCustomCreateMethods();
        }
      }
    }

    private void cmdCustomCreateMethodAdd_Click(object sender, System.EventArgs e) {
      type_generationGenerateentityCustomcreatemethod method = new type_generationGenerateentityCustomcreatemethod();

      fdlgCustomCreateMethod dlg = new fdlgCustomCreateMethod();
      dlg.PlatformDefinition = (platformdefinition)mhashPlatform[cboPlatform.Text];
      dlg.Entity = mCurrentEntityName;
      dlg.DbDefinitionDocument = mDBDefinitionDocument;
      dlg.CustomCreateMethod = method;
      dlg.Initialize();

      if (dlg.ShowDialog(this)==DialogResult.OK) {
        ArrayList alMethods = new ArrayList();
        if (mCurrentEntity.customcreatemethods!=null) {
          alMethods.AddRange(mCurrentEntity.customcreatemethods);
        }
        alMethods.Add(method);
        mCurrentEntity.customcreatemethods = (type_generationGenerateentityCustomcreatemethod[])alMethods.ToArray(typeof(type_generationGenerateentityCustomcreatemethod));

        this.Dirty = true;
        LoadCustomCreateMethods();
      }
    }

    private void cmdCustomCreateMethodRemove_Click(object sender, System.EventArgs e) {
      if (lstCustomCreateMethod.SelectedIndex >= 0) {
        ArrayList alMethods = new ArrayList(mCurrentEntity.customcreatemethods);
        alMethods.RemoveAt(lstCustomCreateMethod.SelectedIndex);
        mCurrentEntity.customcreatemethods = (type_generationGenerateentityCustomcreatemethod[])alMethods.ToArray(typeof(type_generationGenerateentityCustomcreatemethod));

        this.Dirty = true;
        LoadCustomCreateMethods();
      }
    }

    private void lstCustomFindMethod_DoubleClick(object sender, System.EventArgs e) {
      if (lstCustomFindMethod.SelectedIndex >= 0) {
        type_generationGenerateentityCustomfindmethod method = mCurrentEntity.customfindmethods[lstCustomFindMethod.SelectedIndex];

        fdlgCustomFindMethod dlg = new fdlgCustomFindMethod();
        dlg.PlatformDefinition = (platformdefinition)mhashPlatform[cboPlatform.Text];
        dlg.Entity = mCurrentEntityName;
        dlg.DbDefinitionDocument = mDBDefinitionDocument;
        dlg.CustomFindMethod = method;
        dlg.Initialize();

        if (dlg.ShowDialog(this)==DialogResult.OK) {
          this.Dirty = true;
          LoadCustomFindMethods();
        }
      }
    }

    private void cmdCustomFindMethodAdd_Click(object sender, System.EventArgs e) {
      type_generationGenerateentityCustomfindmethod method = new type_generationGenerateentityCustomfindmethod();
      method.returnsmultiple = true;
      method.generatetest = true;

      fdlgCustomFindMethod dlg = new fdlgCustomFindMethod();
      dlg.PlatformDefinition = (platformdefinition)mhashPlatform[cboPlatform.Text];
      dlg.Entity = mCurrentEntityName;
      dlg.DbDefinitionDocument = mDBDefinitionDocument;
      dlg.CustomFindMethod = method;
      dlg.Initialize();

      if (dlg.ShowDialog(this)==DialogResult.OK) {
        ArrayList alMethods = new ArrayList();
        if (mCurrentEntity.customfindmethods!=null) {
          alMethods.AddRange(mCurrentEntity.customfindmethods);
        }
        alMethods.Add(method);
        mCurrentEntity.customfindmethods = (type_generationGenerateentityCustomfindmethod[])alMethods.ToArray(typeof(type_generationGenerateentityCustomfindmethod));

        this.Dirty = true;
        LoadCustomFindMethods();
      }
    }

    private void cmdCustomFindMethodRemove_Click(object sender, System.EventArgs e) {
      if (lstCustomFindMethod.SelectedIndex >= 0) {
        ArrayList alMethods = new ArrayList(mCurrentEntity.customfindmethods);
        alMethods.RemoveAt(lstCustomFindMethod.SelectedIndex);
        mCurrentEntity.customfindmethods = (type_generationGenerateentityCustomfindmethod[])alMethods.ToArray(typeof(type_generationGenerateentityCustomfindmethod));

        this.Dirty = true;
        LoadCustomFindMethods();
      }
    }

    private void dtEnumeration_ColumnChanged(object sender, DataColumnChangeEventArgs e) {
      if (mCurrentEntity!=null) {
        int intID = (int)e.Row["ID"];
        foreach (type_generationGenerateentityEnumerationentry enumEntry in mCurrentEntity.enumerationentries) {
          if (PVFormatUtil.ParseInt(enumEntry.id)==intID) {
            enumEntry.identifier = (string)e.Row["Identifier"];
            enumEntry.name = (string)e.Row["Name"];
            enumEntry.description = (string)e.Row["Description"];
            enumEntry.generate = (bool)e.Row["Generate"];
            this.Dirty = true;
            break;
          }
        }
      }
    }

    private void cmdResetEnumeration_Click(object sender, System.EventArgs e) {
      if (mCurrentEntity==null) {
        return;
      }
      if (MessageBox.Show(this, "All enumeration entries will be reset to the database names.",
          "Reset enumeration", MessageBoxButtons.OKCancel, MessageBoxIcon.Information)==DialogResult.Cancel) {
        return;
      }
      mCurrentEntity.enumerationentries = null;
      this.Dirty = true;
      CheckEntityEnumerationVisible();
    }

    private void dvExportEntityOptions_ColumnChanged(object sender, DataColumnChangeEventArgs e) {
      if (mProjectSettings==null) {
        return;
      }
      string strEntity = (string)e.Row["Entity"];
      projectsettingsEntityexportExportentity expentity = null;

      if (mProjectSettings.entityexport.exportentities!=null) {
        foreach (projectsettingsEntityexportExportentity ee in mProjectSettings.entityexport.exportentities) {
          if (ee.entity.Equals(strEntity)) {
            expentity = ee;
          }
        }
      }

      if (expentity==null) {
        expentity = new projectsettingsEntityexportExportentity();
        expentity.entity = strEntity;

        ArrayList alExpEntities = new ArrayList();
        if (mProjectSettings.entityexport.exportentities!=null) {
          alExpEntities.AddRange(mProjectSettings.entityexport.exportentities);
        }
        alExpEntities.Add(expentity);
        mProjectSettings.entityexport.exportentities = (projectsettingsEntityexportExportentity[])alExpEntities.ToArray(typeof(projectsettingsEntityexportExportentity));
      }
      if (e.Row["SortNo"]==DBNull.Value) {
        expentity.sortno = "";
      }
      else {
        expentity.sortno = ((int)e.Row["SortNo"]).ToString();
      }
      expentity.exportstructure = (bool)e.Row["ExportStructure"];
      expentity.exportdata = (bool)e.Row["ExportData"];
      expentity.exportdrop = (bool)e.Row["ExportDrop"];

      this.Dirty = true;
    }

    private void dvExportEntityOptions_RowDeleting(object sender, DataRowChangeEventArgs e) {
      if (mProjectSettings==null) {
        return;
      }
      string strEntity = (string)e.Row["Entity"];

      ArrayList alExpEntities = new ArrayList();
      if (mProjectSettings.entityexport.exportentities!=null) {
        alExpEntities.AddRange(mProjectSettings.entityexport.exportentities);
      }
      foreach (projectsettingsEntityexportExportentity expentity in alExpEntities) {
        if (expentity.entity.Equals(strEntity)) {
          alExpEntities.Remove(expentity);
          break;
        }
      }
      mProjectSettings.entityexport.exportentities = (projectsettingsEntityexportExportentity[])alExpEntities.ToArray(typeof(projectsettingsEntityexportExportentity));
      this.Dirty = true;
    }

    private void cmdEntityGeneration_Click(object sender, System.EventArgs e) {
      GenerateEntityFiles(false);
    }

    private void GenerateEntityFiles(bool pCommandline) {
      if (this.Dirty && !pCommandline) {
        switch (MessageBox.Show(this, "Do you want to save the project changes?", "Save changes",
          MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)) {
          case DialogResult.Yes:
            SaveProjectSettings();
            break;
          case DialogResult.Cancel:
            return;
        }
      }

      try {
        this.Cursor = Cursors.WaitCursor;

        // Platform/Db Platform definition
        platformdefinition platformdef = (platformdefinition)mhashPlatform[cboPlatform.Text];
        dbplatformdefinition dbplatformdef = (dbplatformdefinition)mhashDBPlatform[cboExportDBPlatform.Text];

        FileGenerator generator = new FileGenerator(mConfigDir, mConfig, platformdef, mhashDBPlatform,
          mDBDefinition, mProjectSettings, mProjectSettings_CurrentPlatform, lstEntity.CheckedItems,
          cboDBDefinition.Text, mStatusHandler);
        generator.GenerateFiles_EntityGeneration();
      }
      catch (Exception ex) {
        MessageBox.Show(this, "Error while generating files:\n" + ex.Message + "\n" + ex.StackTrace,
          "Entity Generation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        mStatusHandler.ClearStatus();
      }
      finally {
        this.Cursor = Cursors.Default;
      }
    }

    private void cmdExportCreateScript_Click(object sender, System.EventArgs e) {
      CreateEntityScript(false, null);
    }

    private void CreateEntityScript(bool pCommandline, string pFilename) {
      if (this.Dirty && !pCommandline) {
        switch (MessageBox.Show(this, "Do you want to save the project changes?", "Save changes",
          MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)) {
          case DialogResult.Yes:
            SaveProjectSettings();
            break;
          case DialogResult.Cancel:
            return;
        }
      }

      try {
        this.Cursor = Cursors.WaitCursor;

        // Platform/Db Platform definition
        platformdefinition platformdef = (platformdefinition)mhashPlatform[cboPlatform.Text];
        dbplatformdefinition dbplatformdef = (dbplatformdefinition)mhashDBPlatform[cboExportDBPlatform.Text];

        FileGenerator generator = new FileGenerator(mConfigDir, mConfig, platformdef, mhashDBPlatform,
          mDBDefinition, mProjectSettings, mProjectSettings_CurrentPlatform, lstEntity.CheckedItems,
          cboDBDefinition.Text, mStatusHandler);

        string script = generator.GenerateScript_CreateEntities(mProjectSettings_CurrentDbPlatform);
        if (pCommandline) {
          StreamWriter writer = File.CreateText(pFilename);
          writer.Write(script);
          writer.Close();
        }
        else {
          txtExportSQL.Text = script;
        }
      }
      catch (Exception ex) {
        MessageBox.Show(this, "Error while generating script:\n" + ex.Message + "\n" + ex.StackTrace,
          "Generate create script", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        mStatusHandler.ClearStatus();
      }
      finally {
        this.Cursor = Cursors.Default;
      }
    }

    private void cmdExportPatchScript_Click(object sender, System.EventArgs e) {
      PatchEntityScript(false, null);
    }

    private void PatchEntityScript(bool pCommandline, string pFilename) {
      if (this.Dirty && !pCommandline) {
        switch (MessageBox.Show(this, "Do you want to save the project changes?", "Save changes",
          MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)) {
          case DialogResult.Yes:
            SaveProjectSettings();
            break;
          case DialogResult.Cancel:
            return;
        }
      }

      try {
        this.Cursor = Cursors.WaitCursor;

        // Platform/Db Platform definition
        platformdefinition platformdef = (platformdefinition)mhashPlatform[cboPlatform.Text];
        dbplatformdefinition dbplatformdef = (dbplatformdefinition)mhashDBPlatform[cboExportDBPlatform.Text];

        FileGenerator generator = new FileGenerator(mConfigDir, mConfig, platformdef, mhashDBPlatform,
          mDBDefinition, mProjectSettings, mProjectSettings_CurrentPlatform, lstEntity.CheckedItems,
          cboDBDefinition.Text, mStatusHandler);

        string script = generator.GenerateScript_PatchEntities(mProjectSettings_CurrentDbPlatform);
        if (pCommandline) {
          StreamWriter writer = File.CreateText(pFilename);
          writer.Write(script);
          writer.Close();
        }
        else {
          txtExportSQL.Text = script;
        }
      }
      catch (Exception ex) {
        MessageBox.Show(this, "Error while generating script:\n" + ex.Message + "\n" + ex.StackTrace,
          "Generate patch script", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        mStatusHandler.ClearStatus();
      }
      finally {
        this.Cursor = Cursors.Default;
      }
    }

    private void cmdExportScriptOpen_Click(object sender, System.EventArgs e) {
      OpenFileDialog dlg = new OpenFileDialog();
      dlg.DefaultExt = "sql";
      dlg.Filter = "SQL files (*.sql)|*.sql|All files (*.*)|*.*";

      if (dlg.ShowDialog(this)==DialogResult.OK) {
        StreamReader reader = File.OpenText(dlg.FileName);
        txtExportSQL.Text = reader.ReadToEnd();
        reader.Close();
      }
    }

    private void cmdExportScriptSave_Click(object sender, System.EventArgs e) {
      SaveFileDialog dlg = new SaveFileDialog();
      dlg.DefaultExt = "sql";
      dlg.Filter = "SQL files (*.sql)|*.sql|All files (*.*)|*.*";
      dlg.CheckPathExists = true;
      dlg.OverwritePrompt = true;

      if (dlg.ShowDialog(this)==DialogResult.OK) {
        if (File.Exists(dlg.FileName)) {
          File.Delete(dlg.FileName);
        }
        StreamWriter writer = File.CreateText(dlg.FileName);
        writer.Write(txtExportSQL.Text);
        writer.Close();
      }
    }

    private void cmdExportScriptClear_Click(object sender, System.EventArgs e) {
      txtExportSQL.Text = "";
    }

    private void cmdExportScriptExecute_Click(object sender, System.EventArgs e) {
      IDbConnection con = null;
      try {
        con = DBServerHelper.GetConnection(mhashDBPlatform, mProjectSettings);
      }
      catch (Exception ex) {
        MessageBox.Show(this, ex.Message, "Error connecting to DB server", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return;
      }
      try {
        dbplatformdefinition dbplatformdef = (dbplatformdefinition)mhashDBPlatform[mProjectSettings.dbplatforms.selected];
        DBServerHelper.ExecuteScript(dbplatformdef,mProjectSettings_CurrentDbPlatform,
          txtExportSQL.Text, mStatusHandler);
      }
      catch (Exception ex) {
        MessageBox.Show(this, ex.Message, "Error executing script", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return;
      }
      finally {
        con.Close();
      }
    }

    private void cmdExportTransfer_Click(object sender, System.EventArgs e) {
      TransferData(optTransferExport.Checked, chkExportToScript.Checked, null, false);
    }

    private void TransferData(bool pExport, bool pExportToScript, string pExportScriptFilename, bool pCommandLine) {
      if (pExportToScript) {
        SaveFileDialog dlgSave = new SaveFileDialog();
        dlgSave.DefaultExt = "sql";
        dlgSave.Filter = "SQL File (*.sql)|*.sql|DBUnit XML File (*.xml)|*.xml|All files (*.*)|*.*";
        dlgSave.CheckPathExists = true;
        dlgSave.OverwritePrompt = true;

        string exportFilename = null;
        if (pCommandLine) {
          exportFilename = pExportScriptFilename;
        }
        else {
          if (dlgSave.ShowDialog(this) == DialogResult.OK) {
            exportFilename = dlgSave.FileName;
          }
        }

        if (exportFilename!=null) {
          if (File.Exists(exportFilename)) {
            File.Delete(exportFilename);
          }

          DBDataTransfer dataTransfer = null;
          try {
            dbplatformdefinition platformdef = (dbplatformdefinition)mhashDBPlatform[mProjectSettings.dbplatforms.selected];
            dataTransfer = new DBDataTransfer(optTransferExport.Checked, cboDBDefinition.Text,
              platformdef, mProjectSettings_CurrentDbPlatform, mDBDefinition, mProjectSettings, mhashDBPlatform);

            dataTransfer.ExportToScript(exportFilename, mStatusHandler);
          }
          catch (Exception pex) {
            MessageBox.Show("Error exporting script:\n" + pex.Message, this.Text, MessageBoxButtons.OK,
              MessageBoxIcon.Exclamation);
          }
          finally {
            if (dataTransfer != null) {
              dataTransfer.CloseServerConnection();
            }
          }
        }
        return;
      }
      else {
        string title = null;
        string msg = null;
        if (optTransferExport.Checked) {
          title = "Export data";
          msg = "This will export the data:\n"
            + "From:  Local MDB\n"
            + "To: Database server\n"
            + "\n"
            + "All data on the ** Database server ** will be deleted and replaced with the local data!";
        }
        else if (optTransferImport.Checked) {
          title = "Import data";
          msg = "This will import the data:\n"
            + "From: Database server\n"
            + "To:  Local MDB\n"
            + "\n"
            + "All data in the ** local MDB ** will be deleted and replaced with the Database server data!";
        }
        if (!pCommandLine && MessageBox.Show(this, msg, title, MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.Cancel) {
          return;
        }

        fdlgTransferData dlg = new fdlgTransferData(pExport, cboDBDefinition.Text,
          mProjectSettings, mhashDBPlatform, mProjectSettings_CurrentDbPlatform, mDBDefinition);
        dlg.ShowDialog();

      }
    }

    private void CompleteProjectSettings() {

      // Global parameters
      if (mConfig.projectdefinition!=null && mConfig.projectdefinition.parameterdefinitions!=null) {
        parametersParameter[] paramarray = mProjectSettings.parameters;
        ParameterHelper.CompleteParameter(mConfig.projectdefinition.parameterdefinitions, ref paramarray);
        mProjectSettings.parameters = paramarray;
      }

      // Entity generation parameters
      if (mConfig.entitygeneration!=null) {
        if (mProjectSettings.entitygeneration==null) {
          mProjectSettings.entitygeneration = new projectsettingsEntitygeneration();
        }
        if (mConfig.entitygeneration.generateglobal!=null && mConfig.entitygeneration.generateglobal.parameterdefinitions!=null) {
          if (mProjectSettings.entitygeneration.generateglobal==null) {
            mProjectSettings.entitygeneration.generateglobal = new type_generationGenerateglobal();
          }
          parametersParameter[] paramarray = mProjectSettings.entitygeneration.generateglobal.parameters;
          ParameterHelper.CompleteParameter(mConfig.entitygeneration.generateglobal.parameterdefinitions, ref paramarray);
          mProjectSettings.entitygeneration.generateglobal.parameters = paramarray;
        }
        if (mConfig.entitygeneration.generateentity!=null && mConfig.entitygeneration.generateentity.parameterdefinitions!=null) {
          ArrayList alEntity = new ArrayList();
          foreach (dbdefinitionEntity entity in mDBDefinition.entities) {
            type_generationGenerateentity genentity = null;
            if (mProjectSettings.entitygeneration.generateentity!=null) {
              foreach (type_generationGenerateentity ge in mProjectSettings.entitygeneration.generateentity) {
                if (ge.entity.Equals(entity.name)) {
                  genentity = ge;
                  break;
                }
              }
            }
            if (genentity==null) {
              genentity = new type_generationGenerateentity();
              genentity.entity = entity.name;
            }
            parametersParameter[] paramarray = genentity.parameters;
            ParameterHelper.CompleteParameter(mConfig.entitygeneration.generateentity.parameterdefinitions, ref paramarray);
            genentity.parameters = paramarray;
            alEntity.Add(genentity);
          }
          mProjectSettings.entitygeneration.generateentity = (type_generationGenerateentity[])alEntity.ToArray(typeof(type_generationGenerateentity));
        }
      }

      // Platform parameters
      if (mProjectSettings.platforms.platform==null) {
        mProjectSettings.platforms.platform = new projectsettingsPlatformsPlatform[1];
        mProjectSettings.platforms.platform[0] = new projectsettingsPlatformsPlatform();
        mProjectSettings.platforms.platform[0].name = "NET";
        mProjectSettings.platforms.selected = mProjectSettings.platforms.platform[0].name;
      }
      foreach (projectsettingsPlatformsPlatform platform in mProjectSettings.platforms.platform) {
        platformdefinition platformdef = (platformdefinition)mhashPlatform[platform.name];

        if (platformdef == null) {
          throw new PVException("Unknown platform name: " + platform.name);
        }

        if (platformdef.parameterdefinitions!=null) {
          parametersParameter[] paramarray = platform.parameters;
          ParameterHelper.CompleteParameter(platformdef.parameterdefinitions, ref paramarray);
          platform.parameters = paramarray;
        }

        if (platformdef.entitygeneration!=null) {
          if (platform.entitygeneration==null) {
            platform.entitygeneration = new type_generation();
          }

          if (platformdef.entitygeneration.generateglobal!=null && platformdef.entitygeneration.generateglobal.parameterdefinitions!=null) {
            if (platform.entitygeneration.generateglobal==null) {
              platform.entitygeneration.generateglobal = new type_generationGenerateglobal();
            }
            parametersParameter[] paramarray = platform.entitygeneration.generateglobal.parameters;
            ParameterHelper.CompleteParameter(platformdef.entitygeneration.generateglobal.parameterdefinitions, ref paramarray);
            platform.entitygeneration.generateglobal.parameters = paramarray;
          }

          if (platformdef.entitygeneration.generateentity!=null && platformdef.entitygeneration.generateentity.parameterdefinitions!=null) {
            ArrayList alEntity = new ArrayList();
            foreach (dbdefinitionEntity entity in mDBDefinition.entities) {
              type_generationGenerateentity genentity = null;
              if (platform.entitygeneration.generateentity!=null) {
                foreach (type_generationGenerateentity ge in platform.entitygeneration.generateentity) {
                  if (ge.entity.Equals(entity.name)) {
                    genentity = ge;
                    break;
                  }
                }
              }
              if (genentity==null) {
                genentity = new type_generationGenerateentity();
                genentity.entity = entity.name;
              }
              parametersParameter[] paramarray = genentity.parameters;
              ParameterHelper.CompleteParameter(platformdef.entitygeneration.generateentity.parameterdefinitions, ref paramarray);
              genentity.parameters = paramarray;
              alEntity.Add(genentity);
            }
            platform.entitygeneration.generateentity = (type_generationGenerateentity[])alEntity.ToArray(typeof(type_generationGenerateentity));
          }
        }

      }

      // DB Platforms in project settings
      if (mProjectSettings.dbplatforms.dbplatform==null) {
        mProjectSettings.dbplatforms.dbplatform = new projectsettingsDbplatformsDbplatform[0];
      }
      ArrayList alDbPlatforms = new ArrayList(mProjectSettings.dbplatforms.dbplatform);
      foreach (dbplatformdefinition dbplatformdef in mhashDBPlatform.Values) {
        projectsettingsDbplatformsDbplatform dbplatform = null;
        foreach (projectsettingsDbplatformsDbplatform dbp in alDbPlatforms) {
          if (dbp.name.Equals(dbplatformdef.name)) {
            dbplatform = dbp;
          }
        }
        if (dbplatform==null) {
          dbplatform = new projectsettingsDbplatformsDbplatform();
          dbplatform.name = dbplatformdef.name;
          dbplatform.dbproviders = new projectsettingsDbplatformsDbplatformDbproviders();
          dbplatform.dbproviders.dbprovider = new projectsettingsDbplatformsDbplatformDbprovidersDbprovider[0];
          dbplatform.scriptgeneration = new type_generation();
          alDbPlatforms.Add(dbplatform);
        }
      }
      mProjectSettings.dbplatforms.dbplatform = (projectsettingsDbplatformsDbplatform[])alDbPlatforms.ToArray(typeof(projectsettingsDbplatformsDbplatform));

      // DB Platform parameters
      foreach (projectsettingsDbplatformsDbplatform dbplatform in mProjectSettings.dbplatforms.dbplatform) {
        dbplatformdefinition dbplatformdef = (dbplatformdefinition)mhashDBPlatform[dbplatform.name];
        if (dbplatformdef==null) continue;

        if (dbplatformdef.parameterdefinitions!=null) {
          parametersParameter[] paramarray = dbplatform.parameters;
          ParameterHelper.CompleteParameter(dbplatformdef.parameterdefinitions, ref paramarray);
          dbplatform.parameters = paramarray;
        }

        if (dbplatformdef.dbproviderdefinitions!=null) {
          if (dbplatform.dbproviders==null) {
            dbplatform.dbproviders = new projectsettingsDbplatformsDbplatformDbproviders();
          }
          if (dbplatform.dbproviders.dbprovider==null) {
            dbplatform.dbproviders.dbprovider = new projectsettingsDbplatformsDbplatformDbprovidersDbprovider[0];
          }

          foreach (dbplatformdefinitionDbproviderdefinition providerdef in dbplatformdef.dbproviderdefinitions) {
            projectsettingsDbplatformsDbplatformDbprovidersDbprovider provider = null;
            foreach (projectsettingsDbplatformsDbplatformDbprovidersDbprovider p in dbplatform.dbproviders.dbprovider) {
              if (p.name.Equals(providerdef.name)) {
                provider = p;
                break;
              }
            }
            if (provider==null) {
              provider = new projectsettingsDbplatformsDbplatformDbprovidersDbprovider();
              provider.name = providerdef.name;
              ArrayList al = new ArrayList(dbplatform.dbproviders.dbprovider);
              al.Add(provider);
              dbplatform.dbproviders.dbprovider = (projectsettingsDbplatformsDbplatformDbprovidersDbprovider[])al.ToArray(typeof(projectsettingsDbplatformsDbplatformDbprovidersDbprovider));
            }
            parametersParameter[] paramarray = provider.parameters;
            ParameterHelper.CompleteParameter(providerdef.parameterdefinitions, ref paramarray);
            provider.parameters = paramarray;
          }
        }

      }

    }

    private void cmdRefreshAllEnumerations_Click(object sender, System.EventArgs e) {
      this.Cursor = Cursors.WaitCursor;

      foreach (dbdefinitionEntity ent in mDBDefinition.entities) {
        type_generationGenerateentity entity = GetGenerationEntity(ent.name);

        bool fEntityEnumeration = false;
        if (entity.parameters!=null) {
          foreach (parametersParameter param in entity.parameters) {
            if (param.name.Equals("generate-enumeration")) {
              fEntityEnumeration = PVFormatUtil.ParseBoolean(param.Value);
            }
          }
        }

        if (fEntityEnumeration) {
          bool fDefChanged = false;
          GridHelper.GetEnumeration(entity, cboDBDefinition.Text, ref fDefChanged);
          if (fDefChanged) {
            this.Dirty = true;
          }
        }
      }

      this.Cursor = Cursors.Default;
    }

    private type_generationGenerateentity GetGenerationEntity(String pEntity) {
      if (mProjectSettings.entitygeneration.generateentity==null) {
        mProjectSettings.entitygeneration.generateentity = new type_generationGenerateentity[0];
      }

      // Entity wählen
      type_generationGenerateentity e = null;
      foreach (type_generationGenerateentity entity in mProjectSettings.entitygeneration.generateentity) {
        if (entity.entity.Equals(pEntity)) {
          e = entity;
        }
      }
      if (e==null) {
        e = new type_generationGenerateentity();
        e.entity = pEntity;
        e.parameters = new parametersParameter[0];

        ArrayList alEntity = new ArrayList(mProjectSettings.entitygeneration.generateentity);
        alEntity.Add(e);
        mProjectSettings.entitygeneration.generateentity = (type_generationGenerateentity[])alEntity.ToArray(typeof(type_generationGenerateentity));
      }

      return e;
    }

    private type_generationGenerateentity GetGenerationEntityPlatform(String pEntity) {
      if (mProjectSettings_CurrentPlatform.entitygeneration.generateentity==null) {
        mProjectSettings_CurrentPlatform.entitygeneration = new projectsettingsEntitygeneration();
        mProjectSettings_CurrentPlatform.entitygeneration.generateentity = new type_generationGenerateentity[0];
      }

      // Platform Entity wählen
      type_generationGenerateentity e = null;
      foreach (type_generationGenerateentity entity in mProjectSettings_CurrentPlatform.entitygeneration.generateentity) {
        if (entity.entity.Equals(pEntity)) {
          e = entity;
        }
      }
      if (e==null) {
        e = new type_generationGenerateentity();
        e.entity = pEntity;
        e.parameters = new parametersParameter[0];

        ArrayList alEntity = new ArrayList(mProjectSettings_CurrentPlatform.entitygeneration.generateentity);
        alEntity.Add(e);
        mProjectSettings_CurrentPlatform.entitygeneration.generateentity = (type_generationGenerateentity[])alEntity.ToArray(typeof(type_generationGenerateentity));
      }

      return e;
    }

    private void mStatusHandler_Error(object pSender, PVEntityGenerator.StatusHandler.ErrorHandlerEventArgs pArgs) {
      pArgs.Result=MessageBox.Show(pArgs.ErrorDescription,this.Text,MessageBoxButtons.AbortRetryIgnore,MessageBoxIcon.Warning,MessageBoxDefaultButton.Button1);
    }

    private void lstPatchEntity_SelectedValueChanged(object sender, System.EventArgs e) {
      if (lstPatchEntity.Items.Count<=0) {
        ClearPatchEntityCombos();
        ClearPatchAttributeCombos();
      } else
        RefreshPatchEntity((string)lstPatchEntity.SelectedItem);
    }

    private void RefreshPatchEntity(string pEntity) {
      if (pEntity==null) return;
      string strEntity=ExtractPatchItem(pEntity);

      //Patch-Entity-Combo
      cboPatchEntity.Text=strEntity;

      //Attribute-Combo laden
      cboPatchAttribute.Items.Clear();
      foreach(dbdefinitionEntity ent in mDBDefinition.entities) {
        if (ent.name.Equals(strEntity)) {
          foreach(dbdefinitionEntityAttribute attr in ent.attributes)
            cboPatchAttribute.Items.Add(attr.name);
          break;
        }
      }

      //Attribute-ListBox / EntityPatchType combo laden
      lstPatchAttribute.Items.Clear();
      foreach (projectsettingsEntityexportPatchentity ent in mProjectSettings.entityexport.patchentities) {
        if (ent.entity.Equals(strEntity)) {
          cboPatchEntityType.SelectedItem=ent.patchtype.ToString();

          if (ent.patchattribute!=null) {
            foreach (projectsettingsEntityexportPatchentityPatchattribute attr in ent.patchattribute)
              lstPatchAttribute.Items.Add(FormatPatchItem(attr.name,attr.patchtype));
          }

          break;
        }
      }

      if (lstPatchAttribute.Items.Count>0)
        lstPatchAttribute.SelectedIndex=0;
      else {
        cboPatchAttribute.Text="";
        cboPatchAttributeType.SelectedIndex=-1;
      }
    }

    private void ClearPatchAttributeCombos() {
      cboPatchAttribute.Items.Clear();
      cboPatchAttribute.Text="";
      cboPatchAttributeType.SelectedIndex=-1;
    }

    private void ClearPatchEntityCombos() {
      cboPatchEntity.Text="";
      cboPatchEntityType.SelectedIndex=-1;
    }

    private void lstPatchAttribute_SelectedValueChanged(object sender, System.EventArgs e) {
      if (lstPatchAttribute.SelectedItem==null || lstPatchEntity.SelectedItem==null) return;

      string strAttr= ExtractPatchItem((string) lstPatchAttribute.SelectedItem);
      string strEntity=ExtractPatchItem((string)lstPatchEntity.SelectedItem);

      cboPatchAttribute.Text=strAttr;

      foreach (projectsettingsEntityexportPatchentity ent in mProjectSettings.entityexport.patchentities) {
        if (ent.entity.Equals(strEntity)) {
          foreach (projectsettingsEntityexportPatchentityPatchattribute attr in ent.patchattribute)
            if (attr.name.Equals(strAttr)) {
              cboPatchAttributeType.SelectedItem=attr.patchtype.ToString();
              break;
            }
          break;
        }
      }
    }

    private void cmdSavePatchEntity_Click(object sender, System.EventArgs e) {
      string strEntity=cboPatchEntity.Text;
      string strPatchtype=cboPatchEntityType.Text;
      if (strEntity==null || strEntity.Length<=0) {
        MessageBox.Show("Please select an entity.",this.Text,MessageBoxButtons.OK,MessageBoxIcon.Information);
        return;
      }

      if (strPatchtype==null || strPatchtype.Length<=0) {
        MessageBox.Show("Please select a patch type.",this.Text,MessageBoxButtons.OK,MessageBoxIcon.Information);
        return;
      }

      bool fReplace=false;
      foreach (object str in lstPatchEntity.Items) {
        if ((ExtractPatchItem((string)str)).ToLower().Equals(strEntity.ToLower())) {
          fReplace=true;
          break;
        }
      }

      //patchtype ermitteln
      type_PatchType pType=(type_PatchType)Enum.Parse(typeof(type_PatchType),strPatchtype);

      if (!fReplace) {
        //neues patchentity
        lstPatchEntity.Items.Add(strEntity + " {" + pType.ToString() + "}");
        projectsettingsEntityexportPatchentity ent=new projectsettingsEntityexportPatchentity();
        ent.entity=strEntity;
        ent.patchtype=pType;

        ArrayList alPatchEntities=new ArrayList();
        if (mProjectSettings.entityexport.patchentities!=null)
          alPatchEntities.AddRange(mProjectSettings.entityexport.patchentities);
        alPatchEntities.Add(ent);
        mProjectSettings.entityexport.patchentities=(projectsettingsEntityexportPatchentity[])alPatchEntities.
          ToArray(typeof(projectsettingsEntityexportPatchentity));

      } else {
        //patchentity aktualisieren
        foreach (projectsettingsEntityexportPatchentity ent in mProjectSettings.entityexport.patchentities) {
          if (ent.entity.Equals(strEntity)) {
            ent.patchtype=pType;
            break;
          }
        }

        //listbox aktualisieren
        for (int i=0;i<lstPatchEntity.Items.Count;i++) {
          if ((ExtractPatchItem((string)lstPatchEntity.Items[i])).ToLower().Equals(strEntity.ToLower())) {
            lstPatchEntity.Items[i]=FormatPatchItem(strEntity,pType);
            break;
          }
        }
      }

      //bearbeitetes patchentity auswählen
      lstPatchEntity.SelectedItem=FormatPatchItem(strEntity,pType);
      this.Dirty=true;
    }

    private void cmdDeletePatchEntity_Click(object sender, System.EventArgs e) {
      string strEntity=cboPatchEntity.Text;
      if (strEntity==null || strEntity.Length<=0) return;

      //PatchEntity suchen
      ArrayList alPatchEntities=new ArrayList();
      if (mProjectSettings.entityexport.patchentities!=null)
        alPatchEntities.AddRange(mProjectSettings.entityexport.patchentities);
      int intPosRemove=-1;
      for (int i=0;i<alPatchEntities.Count;i++) {
        if (((projectsettingsEntityexportPatchentity)alPatchEntities[i]).entity.ToLower().Equals(strEntity.ToLower())) {
          intPosRemove=i;
          break;
        }
      }

      //PatchEntity löschen
      if (intPosRemove!=-1) alPatchEntities.RemoveAt(intPosRemove);

      mProjectSettings.entityexport.patchentities=(projectsettingsEntityexportPatchentity[])alPatchEntities.
        ToArray(typeof(projectsettingsEntityexportPatchentity));

      //PatchEntity aus ListBox löschen
      intPosRemove=-1;
      for (int i=0;i<lstPatchEntity.Items.Count;i++) {
        if ((ExtractPatchItem((string)lstPatchEntity.Items[i])).ToLower().Equals(strEntity.ToLower())) {
          intPosRemove=i;
          break;
        }
      }

      if (intPosRemove!=-1) lstPatchEntity.Items.RemoveAt(intPosRemove);

      //PatchAttributes-Liste löschen
      lstPatchAttribute.Items.Clear();

      if (lstPatchEntity.Items.Count<=0) {
        ClearPatchAttributeCombos();
        ClearPatchEntityCombos();
      }


      this.Dirty=true;
    }

    private void cmdSavePatchAttribute_Click(object sender, System.EventArgs e) {
      string strEntity=cboPatchEntity.Text;
      string strAttr=cboPatchAttribute.Text;
      string strPatchtype=cboPatchAttributeType.Text;
      if (strAttr==null || strAttr.Length<=0) {
        MessageBox.Show("Please select an attribute.",this.Text,MessageBoxButtons.OK,MessageBoxIcon.Information);
        return;
      }

      if (strPatchtype==null || strPatchtype.Length<=0) {
        MessageBox.Show("Please select a patch type.",this.Text,MessageBoxButtons.OK,MessageBoxIcon.Information);
        return;
      }

      bool fReplace=false;
      foreach (object str in lstPatchAttribute.Items) {
        if ((ExtractPatchItem((string)str)).ToLower().Equals(strAttr.ToLower())) {
          fReplace=true;
          break;
        }
      }

      //patchtype ermitteln
      type_PatchType pType=(type_PatchType)Enum.Parse(typeof(type_PatchType),strPatchtype);

      //PatchEntity suchen
      projectsettingsEntityexportPatchentity entity=null;
      foreach (projectsettingsEntityexportPatchentity ent in mProjectSettings.entityexport.patchentities) {
        if (ent.entity.Equals(strEntity))
          entity=ent;
      }

      if (!fReplace) {
        //Attribute hinzufügen
        lstPatchAttribute.Items.Add(FormatPatchItem(strAttr,pType));

        projectsettingsEntityexportPatchentityPatchattribute attr=new projectsettingsEntityexportPatchentityPatchattribute();
        attr.name=strAttr;
        attr.patchtype=pType;

        ArrayList alPatchAttributes=new ArrayList();
        if (entity.patchattribute!=null)
          alPatchAttributes.AddRange(entity.patchattribute);

        alPatchAttributes.Add(attr);
        entity.patchattribute=(projectsettingsEntityexportPatchentityPatchattribute[])alPatchAttributes.
          ToArray(typeof(projectsettingsEntityexportPatchentityPatchattribute));

      } else {
        //Attribute aktualiseren
        foreach (projectsettingsEntityexportPatchentityPatchattribute attr in entity.patchattribute) {
          if (attr.name.Equals(strAttr)) {
            //Attribute gefunden, patchtype aktualisieren
            attr.patchtype=pType;
            break;
          }
        }

        //listbox aktualisieren
        for (int i=0;i<lstPatchAttribute.Items.Count;i++) {
          if ((ExtractPatchItem((string)lstPatchAttribute.Items[i])).ToLower().Equals(strAttr.ToLower())) {
            lstPatchAttribute.Items[i]=FormatPatchItem(strAttr,pType);
            break;
          }
        }
      }

      lstPatchAttribute.SelectedItem=FormatPatchItem(strAttr,pType);

      this.Dirty=true;
    }

    private void cmdDeletePatchAttribute_Click(object sender, System.EventArgs e) {
      string strAttr=cboPatchAttribute.Text;
      string strEntity=cboPatchEntity.Text;
      if (strAttr==null || strAttr.Length<=0) return;

      //PatchEntity suchen
      projectsettingsEntityexportPatchentity entity=null;
      foreach (projectsettingsEntityexportPatchentity ent in mProjectSettings.entityexport.patchentities) {
        if (ent.entity.Equals(strEntity))
          entity=ent;
      }


      ArrayList alPatchAttributes=new ArrayList();
      if (entity.patchattribute!=null)
        alPatchAttributes.AddRange(entity.patchattribute);

      //Attribute suchen
      int intPosRemove=-1;
      for (int i=0;i<alPatchAttributes.Count;i++) {
        if (((projectsettingsEntityexportPatchentityPatchattribute)alPatchAttributes[i]).name.Equals(strAttr)) {
          intPosRemove=i;
          break;
        }
      }

      //Attribute gefunden, löschen
      if (intPosRemove!=-1) alPatchAttributes.RemoveAt(intPosRemove);

      entity.patchattribute=(projectsettingsEntityexportPatchentityPatchattribute[])alPatchAttributes.
        ToArray(typeof(projectsettingsEntityexportPatchentityPatchattribute));

      //Attribute aus Listbox löschen
      intPosRemove=-1;
      for (int i=0;i<lstPatchAttribute.Items.Count;i++) {
        if ((ExtractPatchItem((string)lstPatchAttribute.Items[i])).ToLower().Equals(strAttr.ToLower())) {
          intPosRemove=i;
          break;
        }
      }
      if (intPosRemove!=-1) lstPatchAttribute.Items.RemoveAt(intPosRemove);

      if (lstPatchAttribute.Items.Count<=0) {
        cboPatchAttribute.Text="";
        cboPatchAttributeType.SelectedIndex=-1;
      }

      this.Dirty=true;
    }

    private void cmdGenerateDocumentation_Click(object sender, EventArgs e) {
      if (this.Dirty) {
        switch (MessageBox.Show(this, "Do you want to save the project changes?", "Save changes",
          MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)) {
          case DialogResult.Yes:
            SaveProjectSettings();
            break;
          case DialogResult.Cancel:
            return;
        }
      }

      try {
        this.Cursor = Cursors.WaitCursor;

        // Platform/Db Platform definition
        platformdefinition platformdef = (platformdefinition)mhashPlatform[cboPlatform.Text];
        dbplatformdefinition dbplatformdef = (dbplatformdefinition)mhashDBPlatform[cboExportDBPlatform.Text];

        FileGenerator generator = new FileGenerator(mConfigDir, mConfig, platformdef, mhashDBPlatform,
          mDBDefinition, mProjectSettings, mProjectSettings_CurrentPlatform, lstEntity.CheckedItems,
          cboDBDefinition.Text, mStatusHandler);
        generator.GenerateFile_Documentation();
      }
      catch (Exception ex) {
        MessageBox.Show(this, "Error while generating documenation:\n" + ex.Message + "\n" + ex.StackTrace,
          "Generate documentation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        mStatusHandler.ClearStatus();
      }
      finally {
        this.Cursor = Cursors.Default;
      }
    }
  }
}
