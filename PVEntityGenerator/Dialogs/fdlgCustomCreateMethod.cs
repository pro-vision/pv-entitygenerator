using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using PVEntityGenerator.XMLSchema;

namespace PVEntityGenerator.Dialogs {

  public class fdlgCustomCreateMethod : System.Windows.Forms.Form {
    private System.Windows.Forms.Label lblMethodName;
    private System.Windows.Forms.TextBox txtMethodName;
    private System.Windows.Forms.Button cmdCancel;
    private System.Windows.Forms.Button cmdOK;

    private System.ComponentModel.Container components = null;

    private platformdefinition mPlatformDefinition = null;
    private string mEntity = null;
    private XmlDocument mDbDefinitionDocument = null;
    private PVEntityGenerator.Dialogs.ctlAttributeSelector AttributeSelector;
    private type_generationGenerateentityCustomcreatemethod mCustomCreateMethod = null;

    public fdlgCustomCreateMethod() {
      InitializeComponent();
    }

    protected override void Dispose( bool disposing ) {
      if( disposing ) {
        if(components != null) {
          components.Dispose();
        }
      }
      base.Dispose( disposing );
    }

    public platformdefinition PlatformDefinition {
      get {
        return mPlatformDefinition;
      }
      set {
        mPlatformDefinition = value;
      }
    }

    public string Entity {
      get {
        return mEntity;
      }
      set {
        mEntity = value;
        AttributeSelector.Entity = value;
      }
    }

    public XmlDocument DbDefinitionDocument {
      get {
        return mDbDefinitionDocument;
      }
      set {
        mDbDefinitionDocument = value;
        AttributeSelector.DbDefinitionDocument = value;
      }
    }

    public type_generationGenerateentityCustomcreatemethod CustomCreateMethod {
      get {
        return mCustomCreateMethod;
      }
      set {
        mCustomCreateMethod = value;
        AttributeSelector.CustomCreateMethod = value;
      }
    }

    public void Initialize() {
      AttributeSelector.Initialize();

      if (mCustomCreateMethod.name!=null && mCustomCreateMethod.name.Length!=0) {
        txtMethodName.Text = mCustomCreateMethod.name;
      }
      else {
        if (mPlatformDefinition.uppercasemethodnames) {
          txtMethodName.Text = "Create";
        }
        else {
          txtMethodName.Text = "create";
        }
      }
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(fdlgCustomCreateMethod));
      this.lblMethodName = new System.Windows.Forms.Label();
      this.txtMethodName = new System.Windows.Forms.TextBox();
      this.AttributeSelector = new PVEntityGenerator.Dialogs.ctlAttributeSelector();
      this.cmdCancel = new System.Windows.Forms.Button();
      this.cmdOK = new System.Windows.Forms.Button();
      this.SuspendLayout();
      //
      // lblMethodName
      //
      this.lblMethodName.AutoSize = true;
      this.lblMethodName.Location = new System.Drawing.Point(12, 12);
      this.lblMethodName.Name = "lblMethodName";
      this.lblMethodName.Size = new System.Drawing.Size(73, 17);
      this.lblMethodName.TabIndex = 0;
      this.lblMethodName.Text = "Method name";
      //
      // txtMethodName
      //
      this.txtMethodName.Location = new System.Drawing.Point(88, 8);
      this.txtMethodName.Name = "txtMethodName";
      this.txtMethodName.Size = new System.Drawing.Size(196, 21);
      this.txtMethodName.TabIndex = 1;
      this.txtMethodName.Text = "";
      //
      // AttributeSelector
      //
      this.AttributeSelector.AttributeAddCustomAllowed = false;
      this.AttributeSelector.CustomCreateMethod = null;
      this.AttributeSelector.CustomFindMethod = null;
      this.AttributeSelector.DbDefinitionDocument = null;
      this.AttributeSelector.Entity = null;
      this.AttributeSelector.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.AttributeSelector.Location = new System.Drawing.Point(8, 32);
      this.AttributeSelector.Name = "AttributeSelector";
      this.AttributeSelector.Size = new System.Drawing.Size(456, 228);
      this.AttributeSelector.TabIndex = 2;
      //
      // cmdCancel
      //
      this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cmdCancel.Location = new System.Drawing.Point(480, 44);
      this.cmdCancel.Name = "cmdCancel";
      this.cmdCancel.Size = new System.Drawing.Size(72, 24);
      this.cmdCancel.TabIndex = 4;
      this.cmdCancel.Text = "Cancel";
      //
      // cmdOK
      //
      this.cmdOK.Location = new System.Drawing.Point(480, 12);
      this.cmdOK.Name = "cmdOK";
      this.cmdOK.Size = new System.Drawing.Size(72, 24);
      this.cmdOK.TabIndex = 3;
      this.cmdOK.Text = "OK";
      this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
      //
      // fdlgCustomCreateMethod
      //
      this.AcceptButton = this.cmdOK;
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
      this.CancelButton = this.cmdCancel;
      this.ClientSize = new System.Drawing.Size(562, 267);
      this.Controls.Add(this.cmdCancel);
      this.Controls.Add(this.cmdOK);
      this.Controls.Add(this.AttributeSelector);
      this.Controls.Add(this.txtMethodName);
      this.Controls.Add(this.lblMethodName);
      this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "fdlgCustomCreateMethod";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Custom create method";
      this.ResumeLayout(false);

    }
    #endregion

    private void cmdOK_Click(object sender, System.EventArgs e) {
      if (txtMethodName.Text.Length==0) {
        MessageBox.Show(this, "Please enter a method name.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }

      mCustomCreateMethod.name = txtMethodName.Text;
      AttributeSelector.Store();

      this.DialogResult = DialogResult.OK;
      this.Hide();
    }

  }
}
