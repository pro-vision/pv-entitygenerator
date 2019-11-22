using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using PVFramework.WinApp.Util;
using PVFramework;
using PVEntityGenerator.Util;

namespace  PVEntityGenerator.Controls {

  public class fdlgPasswordEditor  :  System.Windows.Forms.Form {
    private  System.Windows.Forms.TextBox txtValue;
    private  System.Windows.Forms.Button  cmdOK;
    private  System.Windows.Forms.Button  cmdCancel;
    private System.Windows.Forms.Label lblPassword;
    private  System.ComponentModel.Container  components = null;

    public fdlgPasswordEditor() {
      InitializeComponent();
    }

    protected  override void  Dispose( bool  disposing  ) {
      if(  disposing  ) {
        if(components  != null) {
          components.Dispose();
        }
      }
      base.Dispose(  disposing  );
    }

    public string Value {
      get {
        if (txtValue.Text.Length!=0) {
          return PasswordHelper.EncryptPassword(txtValue.Text);
        }
        else {
          return "";
        }
      }
      set {
        if (value!=null && value.Length!=0) {
          try {
            txtValue.Text = PasswordHelper.DecryptPassword(value);
          }
          catch (Exception) {
            txtValue.Text = value;
          }
        }
        else {
          txtValue.Text = "";
        }
      }
    }

    #region  Windows  Form Designer  generated  code
    ///  <summary>
    ///  Required method  for  Designer support - do  not  modify
    ///  the  contents of  this method  with the code  editor.
    ///  </summary>
    private  void InitializeComponent() {
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(fdlgPasswordEditor));
      this.txtValue = new System.Windows.Forms.TextBox();
      this.cmdOK = new System.Windows.Forms.Button();
      this.cmdCancel = new System.Windows.Forms.Button();
      this.lblPassword = new System.Windows.Forms.Label();
      this.SuspendLayout();
      //
      // txtValue
      //
      this.txtValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.txtValue.Location = new System.Drawing.Point(72, 16);
      this.txtValue.Name = "txtValue";
      this.txtValue.PasswordChar = '*';
      this.txtValue.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.txtValue.Size = new System.Drawing.Size(204, 21);
      this.txtValue.TabIndex = 0;
      this.txtValue.Text = "";
      //
      // cmdOK
      //
      this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.cmdOK.Location = new System.Drawing.Point(330, 12);
      this.cmdOK.Name = "cmdOK";
      this.cmdOK.Size = new System.Drawing.Size(72, 24);
      this.cmdOK.TabIndex = 1;
      this.cmdOK.Text = "OK";
      //
      // cmdCancel
      //
      this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cmdCancel.Location = new System.Drawing.Point(330, 44);
      this.cmdCancel.Name = "cmdCancel";
      this.cmdCancel.Size = new System.Drawing.Size(72, 24);
      this.cmdCancel.TabIndex = 2;
      this.cmdCancel.Text = "Cancel";
      //
      // lblPassword
      //
      this.lblPassword.AutoSize = true;
      this.lblPassword.Location = new System.Drawing.Point(12, 16);
      this.lblPassword.Name = "lblPassword";
      this.lblPassword.Size = new System.Drawing.Size(51, 17);
      this.lblPassword.TabIndex = 3;
      this.lblPassword.Text = "Password";
      //
      // fdlgPasswordEditor
      //
      this.AcceptButton = this.cmdOK;
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
      this.CancelButton = this.cmdCancel;
      this.ClientSize = new System.Drawing.Size(412, 81);
      this.Controls.Add(this.lblPassword);
      this.Controls.Add(this.txtValue);
      this.Controls.Add(this.cmdCancel);
      this.Controls.Add(this.cmdOK);
      this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "fdlgPasswordEditor";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Edit property";
      this.Closing += new System.ComponentModel.CancelEventHandler(this.ClobEditorDialog_Closing);
      this.Load += new System.EventHandler(this.ClobEditorDialog_Load);
      this.ResumeLayout(false);

    }
    #endregion

    private void ClobEditorDialog_Load(object sender, System.EventArgs e) {
      FormUtil.RestoreWindowPos(this);
    }

    private void ClobEditorDialog_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
      FormUtil.SaveWindowPos(this);
    }

  }
}
