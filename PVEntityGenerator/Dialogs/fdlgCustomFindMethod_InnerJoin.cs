using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;

namespace PVEntityGenerator.Dialogs {

  public class fdlgCustomFindMethod_InnerJoin : System.Windows.Forms.Form {
    private System.Windows.Forms.Button cmdCancel;
    private System.Windows.Forms.Button cmdOK;
    private System.Windows.Forms.Label lblEntity;
    private System.Windows.Forms.TextBox txtEntityAlias;
    private System.Windows.Forms.Label lblEntityAlias;
    private System.Windows.Forms.TextBox txtJoinExpression;
    private System.Windows.Forms.Label lblJoinExpression;

    private System.ComponentModel.Container components = null;
    private TextBox txtEntity;
    private XmlDocument mDbDefinitionDocument = null;

    public fdlgCustomFindMethod_InnerJoin() {
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

    public XmlDocument DbDefinitionDocument {
      get {
        return mDbDefinitionDocument;
      }
      set {
        mDbDefinitionDocument = value;
      }
    }

    public string Entity {
      get {
        return txtEntity.Text;
      }
      set {
        try {
          txtEntity.Text = value;
        }
        catch {
          // ignorieren
        }
      }
    }

    public string EntityAlias {
      get {
        return txtEntityAlias.Text.Length==0 ? null : txtEntityAlias.Text;
      }
      set {
        txtEntityAlias.Text = (value==null ? "" : value);
      }
    }

    public string JoinExpression {
      get {
        return txtJoinExpression.Text;
      }
      set {
        txtJoinExpression.Text = value;
      }
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(fdlgCustomFindMethod_InnerJoin));
      this.cmdCancel = new System.Windows.Forms.Button();
      this.cmdOK = new System.Windows.Forms.Button();
      this.lblEntity = new System.Windows.Forms.Label();
      this.txtEntityAlias = new System.Windows.Forms.TextBox();
      this.lblEntityAlias = new System.Windows.Forms.Label();
      this.txtJoinExpression = new System.Windows.Forms.TextBox();
      this.lblJoinExpression = new System.Windows.Forms.Label();
      this.txtEntity = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // cmdCancel
      // 
      this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cmdCancel.Location = new System.Drawing.Point(356, 44);
      this.cmdCancel.Name = "cmdCancel";
      this.cmdCancel.Size = new System.Drawing.Size(72, 24);
      this.cmdCancel.TabIndex = 7;
      this.cmdCancel.Text = "Cancel";
      // 
      // cmdOK
      // 
      this.cmdOK.Location = new System.Drawing.Point(356, 12);
      this.cmdOK.Name = "cmdOK";
      this.cmdOK.Size = new System.Drawing.Size(72, 24);
      this.cmdOK.TabIndex = 6;
      this.cmdOK.Text = "OK";
      this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
      // 
      // lblEntity
      // 
      this.lblEntity.AutoSize = true;
      this.lblEntity.Location = new System.Drawing.Point(12, 16);
      this.lblEntity.Name = "lblEntity";
      this.lblEntity.Size = new System.Drawing.Size(82, 13);
      this.lblEntity.TabIndex = 0;
      this.lblEntity.Text = "Inner Join Entity";
      // 
      // txtEntityAlias
      // 
      this.txtEntityAlias.Location = new System.Drawing.Point(96, 44);
      this.txtEntityAlias.Name = "txtEntityAlias";
      this.txtEntityAlias.Size = new System.Drawing.Size(88, 20);
      this.txtEntityAlias.TabIndex = 3;
      // 
      // lblEntityAlias
      // 
      this.lblEntityAlias.AutoSize = true;
      this.lblEntityAlias.Location = new System.Drawing.Point(12, 48);
      this.lblEntityAlias.Name = "lblEntityAlias";
      this.lblEntityAlias.Size = new System.Drawing.Size(58, 13);
      this.lblEntityAlias.TabIndex = 2;
      this.lblEntityAlias.Text = "Entity Alias";
      // 
      // txtJoinExpression
      // 
      this.txtJoinExpression.Location = new System.Drawing.Point(96, 76);
      this.txtJoinExpression.Name = "txtJoinExpression";
      this.txtJoinExpression.Size = new System.Drawing.Size(232, 20);
      this.txtJoinExpression.TabIndex = 5;
      // 
      // lblJoinExpression
      // 
      this.lblJoinExpression.AutoSize = true;
      this.lblJoinExpression.Location = new System.Drawing.Point(12, 80);
      this.lblJoinExpression.Name = "lblJoinExpression";
      this.lblJoinExpression.Size = new System.Drawing.Size(80, 13);
      this.lblJoinExpression.TabIndex = 4;
      this.lblJoinExpression.Text = "Join Expression";
      // 
      // txtEntity
      // 
      this.txtEntity.Location = new System.Drawing.Point(96, 13);
      this.txtEntity.Name = "txtEntity";
      this.txtEntity.Size = new System.Drawing.Size(232, 20);
      this.txtEntity.TabIndex = 1;
      // 
      // fdlgCustomFindMethod_InnerJoin
      // 
      this.AcceptButton = this.cmdOK;
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(438, 107);
      this.Controls.Add(this.txtEntity);
      this.Controls.Add(this.txtJoinExpression);
      this.Controls.Add(this.txtEntityAlias);
      this.Controls.Add(this.cmdCancel);
      this.Controls.Add(this.cmdOK);
      this.Controls.Add(this.lblJoinExpression);
      this.Controls.Add(this.lblEntity);
      this.Controls.Add(this.lblEntityAlias);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "fdlgCustomFindMethod_InnerJoin";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Inner Join";
      this.ResumeLayout(false);
      this.PerformLayout();

    }
    #endregion

    private void cmdOK_Click(object sender, System.EventArgs e) {
      if (txtEntity.Text.Length==0) {
        MessageBox.Show(this, "Please choose a entity.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }

      this.DialogResult = DialogResult.OK;
      this.Hide();
    }
  }
}
