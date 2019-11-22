using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using PVEntityGenerator.XMLSchema;

namespace PVEntityGenerator.Dialogs {

  public class fdlgCustomFindMethod_CustomAttribute : System.Windows.Forms.Form {
    private System.Windows.Forms.Button cmdCancel;
    private System.Windows.Forms.Button cmdOK;
    private System.Windows.Forms.Label lblName;
    private System.Windows.Forms.TextBox txtName;
    private System.Windows.Forms.Label lblType;
    private System.Windows.Forms.ComboBox cboType;
    private System.Windows.Forms.TextBox txtEntityAlias;
    private System.Windows.Forms.Label lblEntityAlias;

    private System.ComponentModel.Container components = null;

    public fdlgCustomFindMethod_CustomAttribute() {
      InitializeComponent();

      cboType.Items.Add(type_AttributeType.BIGINT);
      cboType.Items.Add(type_AttributeType.BIT);
      cboType.Items.Add(type_AttributeType.BLOB);
      cboType.Items.Add(type_AttributeType.BYTE);
      cboType.Items.Add(type_AttributeType.CLOB);
      cboType.Items.Add(type_AttributeType.FLOAT);
      cboType.Items.Add(type_AttributeType.ID);
      cboType.Items.Add(type_AttributeType.INTEGER);
      cboType.Items.Add(type_AttributeType.SMALLINT);
      cboType.Items.Add(type_AttributeType.TIMESTAMP);
      cboType.Items.Add(type_AttributeType.VARCHAR);
      cboType.Items.Add(type_AttributeType.VSTAMP);
    }

    protected override void Dispose( bool disposing ) {
      if( disposing ) {
        if(components != null) {
          components.Dispose();
        }
      }
      base.Dispose( disposing );
    }

    public string AttributeName {
      get {
        return txtName.Text;
      }
      set {
        txtName.Text = value;
      }
    }

    public type_AttributeType AttributeType {
      get {
        return (type_AttributeType)cboType.SelectedItem;
      }
      set {
        cboType.SelectedItem = value;
      }
    }

    public string EntityAlias {
      get {
        if (txtEntityAlias.Text.Length==0) {
          return null;
        }
        else {
          return txtEntityAlias.Text;
        }
      }
      set {
        if (value!=null) {
          txtEntityAlias.Text = value;
        }
        else {
          txtEntityAlias.Text = "";
        }
      }
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(fdlgCustomFindMethod_CustomAttribute));
      this.cmdCancel = new System.Windows.Forms.Button();
      this.cmdOK = new System.Windows.Forms.Button();
      this.lblName = new System.Windows.Forms.Label();
      this.txtName = new System.Windows.Forms.TextBox();
      this.lblType = new System.Windows.Forms.Label();
      this.cboType = new System.Windows.Forms.ComboBox();
      this.txtEntityAlias = new System.Windows.Forms.TextBox();
      this.lblEntityAlias = new System.Windows.Forms.Label();
      this.SuspendLayout();
      //
      // cmdCancel
      //
      this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cmdCancel.Location = new System.Drawing.Point(296, 44);
      this.cmdCancel.Name = "cmdCancel";
      this.cmdCancel.Size = new System.Drawing.Size(72, 24);
      this.cmdCancel.TabIndex = 7;
      this.cmdCancel.Text = "Cancel";
      //
      // cmdOK
      //
      this.cmdOK.Location = new System.Drawing.Point(296, 12);
      this.cmdOK.Name = "cmdOK";
      this.cmdOK.Size = new System.Drawing.Size(72, 24);
      this.cmdOK.TabIndex = 6;
      this.cmdOK.Text = "OK";
      this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
      //
      // lblName
      //
      this.lblName.AutoSize = true;
      this.lblName.Location = new System.Drawing.Point(16, 20);
      this.lblName.Name = "lblName";
      this.lblName.Size = new System.Drawing.Size(79, 17);
      this.lblName.TabIndex = 0;
      this.lblName.Text = "Attribute name";
      //
      // txtName
      //
      this.txtName.Location = new System.Drawing.Point(100, 16);
      this.txtName.Name = "txtName";
      this.txtName.Size = new System.Drawing.Size(172, 21);
      this.txtName.TabIndex = 1;
      this.txtName.Text = "";
      //
      // lblType
      //
      this.lblType.AutoSize = true;
      this.lblType.Location = new System.Drawing.Point(16, 52);
      this.lblType.Name = "lblType";
      this.lblType.Size = new System.Drawing.Size(73, 17);
      this.lblType.TabIndex = 2;
      this.lblType.Text = "Attribute type";
      //
      // cboType
      //
      this.cboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cboType.Location = new System.Drawing.Point(100, 48);
      this.cboType.Name = "cboType";
      this.cboType.Size = new System.Drawing.Size(172, 21);
      this.cboType.TabIndex = 3;
      //
      // txtEntityAlias
      //
      this.txtEntityAlias.Location = new System.Drawing.Point(100, 80);
      this.txtEntityAlias.Name = "txtEntityAlias";
      this.txtEntityAlias.Size = new System.Drawing.Size(88, 21);
      this.txtEntityAlias.TabIndex = 5;
      this.txtEntityAlias.Text = "";
      //
      // lblEntityAlias
      //
      this.lblEntityAlias.AutoSize = true;
      this.lblEntityAlias.Location = new System.Drawing.Point(16, 84);
      this.lblEntityAlias.Name = "lblEntityAlias";
      this.lblEntityAlias.Size = new System.Drawing.Size(59, 17);
      this.lblEntityAlias.TabIndex = 4;
      this.lblEntityAlias.Text = "Entity Alias";
      //
      // fdlgCustomFindMethod_CustomAttribute
      //
      this.AcceptButton = this.cmdOK;
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
      this.CancelButton = this.cmdCancel;
      this.ClientSize = new System.Drawing.Size(376, 115);
      this.Controls.Add(this.txtEntityAlias);
      this.Controls.Add(this.lblEntityAlias);
      this.Controls.Add(this.cboType);
      this.Controls.Add(this.lblType);
      this.Controls.Add(this.txtName);
      this.Controls.Add(this.lblName);
      this.Controls.Add(this.cmdCancel);
      this.Controls.Add(this.cmdOK);
      this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "fdlgCustomFindMethod_CustomAttribute";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Add custom attribute";
      this.ResumeLayout(false);

    }
    #endregion

    private void cmdOK_Click(object sender, System.EventArgs e) {
      if (txtName.Text.Length==0) {
        MessageBox.Show(this, "Please enter a attribute name.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }
      if (cboType.SelectedIndex < 0) {
        MessageBox.Show(this, "Please choose a attribute type.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }

      this.DialogResult = DialogResult.OK;
      this.Hide();
    }
  }
}
