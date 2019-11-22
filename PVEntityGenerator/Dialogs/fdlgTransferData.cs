using System;
using System.Data;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using PVFramework;
using PVFramework.WinApp.Util;
using PVEntityGenerator.Controls;
using PVEntityGenerator.XMLSchema;
using PVEntityGenerator.Dialogs;
using System.Text;

namespace PVEntityGenerator.Dialogs {

  public class fdlgTransferData : System.Windows.Forms.Form {
    private System.Windows.Forms.TextBox txtLog;
    private System.ComponentModel.IContainer components;
    bool mfExport=true;
    dbplatformdefinition mDbPlatformDef;
    projectsettingsDbplatformsDbplatform mDbPlatform;
    string mDBFile;
    private projectsettings mProjectSettings = null;
    private Hashtable mhashDBPlatform = null;
    private System.Windows.Forms.Timer tmrMain;
    private dbdefinition mDBDefinition=null;
    private System.Windows.Forms.Button cmdClose;
    private System.Windows.Forms.Button cmdStopTransfer;
    private System.Windows.Forms.ProgressBar pBar;
    private DBDataTransfer mTransfer=null;

    public fdlgTransferData(bool pfExport, string pDBFile, projectsettings pProjectSettings,
      Hashtable phashDBPlatform, projectsettingsDbplatformsDbplatform pDbPlatform,
      dbdefinition pDBDefinition) {
      InitializeComponent();

      mfExport=pfExport;
      mDbPlatform=pDbPlatform;
      mDBFile=pDBFile;
      mProjectSettings=pProjectSettings;
      mhashDBPlatform=phashDBPlatform;
      mDBDefinition=pDBDefinition;

      mDbPlatformDef = (dbplatformdefinition)mhashDBPlatform[mProjectSettings.dbplatforms.selected];

      //Wait a bit before starting
      tmrMain.Interval=10;
      tmrMain.Tick+=new EventHandler(tmrMain_Tick);
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose( bool disposing )
    {
      if( disposing )
      {
        if(components != null)
        {
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
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(fdlgTransferData));
      this.cmdClose = new System.Windows.Forms.Button();
      this.txtLog = new System.Windows.Forms.TextBox();
      this.tmrMain = new System.Windows.Forms.Timer(this.components);
      this.cmdStopTransfer = new System.Windows.Forms.Button();
      this.pBar = new System.Windows.Forms.ProgressBar();
      this.SuspendLayout();
      //
      // cmdClose
      //
      this.cmdClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cmdClose.Location = new System.Drawing.Point(648, 477);
      this.cmdClose.Name = "cmdClose";
      this.cmdClose.Size = new System.Drawing.Size(93, 24);
      this.cmdClose.TabIndex = 0;
      this.cmdClose.Text = "Close";
      this.cmdClose.Click += new System.EventHandler(this.cmdClose_Click);
      //
      // txtLog
      //
      this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.txtLog.Location = new System.Drawing.Point(6, 6);
      this.txtLog.Multiline = true;
      this.txtLog.Name = "txtLog";
      this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.txtLog.Size = new System.Drawing.Size(735, 447);
      this.txtLog.TabIndex = 1;
      this.txtLog.Text = "";
      //
      // cmdStopTransfer
      //
      this.cmdStopTransfer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.cmdStopTransfer.Enabled = false;
      this.cmdStopTransfer.Location = new System.Drawing.Point(6, 477);
      this.cmdStopTransfer.Name = "cmdStopTransfer";
      this.cmdStopTransfer.Size = new System.Drawing.Size(93, 24);
      this.cmdStopTransfer.TabIndex = 2;
      this.cmdStopTransfer.Text = "Stop transfer";
      this.cmdStopTransfer.Click += new System.EventHandler(this.cmdStopTransfer_Click);
      //
      // pBar
      //
      this.pBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.pBar.Location = new System.Drawing.Point(6, 456);
      this.pBar.Name = "pBar";
      this.pBar.Size = new System.Drawing.Size(735, 18);
      this.pBar.TabIndex = 3;
      //
      // fdlgTransferData
      //
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(748, 506);
      this.Controls.Add(this.pBar);
      this.Controls.Add(this.cmdStopTransfer);
      this.Controls.Add(this.txtLog);
      this.Controls.Add(this.cmdClose);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "fdlgTransferData";
      this.Text = "Transfer data";
      this.Closing += new System.ComponentModel.CancelEventHandler(this.fdlgTransferData_Closing);
      this.Load += new System.EventHandler(this.fdlgTransferData_Load);
      this.ResumeLayout(false);

    }
    #endregion

    private void fdlgTransferData_Load(object sender, System.EventArgs e) {
      txtLog.Focus();
      tmrMain.Start();
    }

    private void AppendLog(string pAppend) {
      txtLog.AppendText(pAppend);
      txtLog.AppendText("\r\n");
      txtLog.SelectionStart = txtLog.Text.Length;
      txtLog.ScrollToCaret();
      Application.DoEvents();
    }

    private void AppendError(string pErrDesc) {
      AppendLog("ERROR: " + pErrDesc);
    }

    public void StartTransfer() {
      Application.DoEvents();

      cmdStopTransfer.Enabled = true;
      cmdClose.Enabled = false;

      try {
        mTransfer = new DBDataTransfer(mfExport, mDBFile, mDbPlatformDef, mDbPlatform, mDBDefinition, mProjectSettings, mhashDBPlatform);
        mTransfer.TransferDataMsg += new PVEntityGenerator.DBDataTransfer.TransferDataMsgEventHandler(DBServerHelper_TransferDataMsg);

        pBar.Minimum = 0;
        mTransfer.TransferData(pBar);
      }
      catch (Exception ex) {
        MessageBox.Show(this, ex.Message, "Error transferring data", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return;
      }
      finally {
        if (mTransfer != null) {
          mTransfer.CloseServerConnection();
        }
        cmdStopTransfer.Enabled = false;
        cmdClose.Enabled = true;
      }
    }

    private void DBServerHelper_TransferDataMsg(object pSender, DBDataTransfer.TransferDataMsgEventArgs pArgs) {
      AppendLog(pArgs.Message);
    }

    private void fdlgTransferData_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
      tmrMain.Stop();
      if (mTransfer!=null) mTransfer.Cancel=true;
    }

    private void tmrMain_Tick(object sender, EventArgs e) {
      tmrMain.Stop();
      StartTransfer();
    }

    private void cmdStopTransfer_Click(object sender, System.EventArgs e) {
      if (mTransfer!=null) mTransfer.Cancel=true;
    }

    private void cmdClose_Click(object sender, System.EventArgs e) {
      this.Hide();
    }
  }
}
