using  System;
using  System.Drawing;
using  System.Collections;
using  System.ComponentModel;
using  System.Windows.Forms;
using PVFramework.WinApp.Util;

namespace  PVEntityGenerator.Controls {

  public class fdlgClobEditor  :  System.Windows.Forms.Form {
    private  System.Windows.Forms.TextBox txtValue;
    private  System.Windows.Forms.Button  cmdOK;
    private  System.Windows.Forms.Button  cmdCancel;
    private  System.ComponentModel.Container  components = null;

    public fdlgClobEditor() {
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
        return txtValue.Text;
      }
      set {
        txtValue.Text = value;
      }
    }

    #region  Windows  Form Designer  generated  code
    ///  <summary>
    ///  Required method  for  Designer support - do  not  modify
    ///  the  contents of  this method  with the code  editor.
    ///  </summary>
    private  void InitializeComponent() {
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(fdlgClobEditor));
      this.txtValue = new System.Windows.Forms.TextBox();
      this.cmdOK = new System.Windows.Forms.Button();
      this.cmdCancel = new System.Windows.Forms.Button();
      this.SuspendLayout();
      //
      // txtValue
      //
      this.txtValue.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.txtValue.Location = new System.Drawing.Point(8, 8);
      this.txtValue.Multiline = true;
      this.txtValue.Name = "txtValue";
      this.txtValue.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.txtValue.Size = new System.Drawing.Size(480, 204);
      this.txtValue.TabIndex = 0;
      this.txtValue.Text = "";
      //
      // cmdOK
      //
      this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.cmdOK.Location = new System.Drawing.Point(504, 12);
      this.cmdOK.Name = "cmdOK";
      this.cmdOK.Size = new System.Drawing.Size(72, 24);
      this.cmdOK.TabIndex = 1;
      this.cmdOK.Text = "OK";
      //
      // cmdCancel
      //
      this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cmdCancel.Location = new System.Drawing.Point(504, 44);
      this.cmdCancel.Name = "cmdCancel";
      this.cmdCancel.Size = new System.Drawing.Size(72, 24);
      this.cmdCancel.TabIndex = 2;
      this.cmdCancel.Text = "Cancel";
      //
      // fdlgClobEditor
      //
      this.AcceptButton = this.cmdOK;
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.CancelButton = this.cmdCancel;
      this.ClientSize = new System.Drawing.Size(586, 219);
      this.Controls.Add(this.cmdCancel);
      this.Controls.Add(this.cmdOK);
      this.Controls.Add(this.txtValue);
      this.Font = new System.Drawing.Font("Tahoma", 8F);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "fdlgClobEditor";
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
