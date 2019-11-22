using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using PVEntityGenerator.XMLSchema;

namespace PVEntityGenerator.Dialogs {

  public class fdlgCustomFindMethod : System.Windows.Forms.Form {

    private System.ComponentModel.Container components = null;

    private platformdefinition mPlatformDefinition = null;
    private string mEntity = null;
    private XmlDocument mDbDefinitionDocument = null;
    private System.Windows.Forms.Button cmdCancel;
    private System.Windows.Forms.Button cmdOK;
    private PVEntityGenerator.Dialogs.ctlAttributeSelector AttributeSelector;
    private System.Windows.Forms.TextBox txtMethodName;
    private System.Windows.Forms.Label lblMethodName;
    private System.Windows.Forms.TextBox txtEntityAlias;
    private System.Windows.Forms.Label lblEntityAlias;
    private System.Windows.Forms.TextBox txtWhereExpression;
    private System.Windows.Forms.Label lblWhereExpression;
    private System.Windows.Forms.TextBox txtOrderBy;
    private System.Windows.Forms.Label lblOrderBy;
    private System.Windows.Forms.CheckBox chkReturnsMultiple;
    private System.Windows.Forms.TextBox txtDescription;
    private System.Windows.Forms.Label lblDescription;
    private System.Windows.Forms.Label lblInnerJoin;
    private System.Windows.Forms.ListBox lstInnerJoin;
    private System.Windows.Forms.Button cmdInnerJoinAdd;
    private System.Windows.Forms.Button cmdInnerJoinRemove;
    private System.Windows.Forms.CheckBox chkGenerateTest;
    private type_generationGenerateentityCustomfindmethod mCustomFindMethod = null;

    public fdlgCustomFindMethod() {
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

    public type_generationGenerateentityCustomfindmethod CustomFindMethod {
      get {
        return mCustomFindMethod;
      }
      set {
        mCustomFindMethod = value;
        AttributeSelector.CustomFindMethod = value;
      }
    }

    public void Initialize() {
      AttributeSelector.Initialize();

      if (mCustomFindMethod.name!=null && mCustomFindMethod.name.Length!=0) {
        txtMethodName.Text = mCustomFindMethod.name;
      }
      else {
        if (mPlatformDefinition.uppercasemethodnames) {
          txtMethodName.Text = "FindBy";
        }
        else {
          txtMethodName.Text = "findBy";
        }
      }

      txtDescription.Text = mCustomFindMethod.description==null ? "" : mCustomFindMethod.description;
      txtEntityAlias.Text = mCustomFindMethod.entityalias==null ? "" : mCustomFindMethod.entityalias;
      txtWhereExpression.Text = mCustomFindMethod.whereexpression==null ? "" : mCustomFindMethod.whereexpression;
      txtOrderBy.Text = mCustomFindMethod.orderbyexpression==null ? "" : mCustomFindMethod.orderbyexpression;
      chkReturnsMultiple.Checked = mCustomFindMethod.returnsmultiple;
      chkGenerateTest.Checked = mCustomFindMethod.generatetest;

      LoadInnerJoins();
    }

    private void LoadInnerJoins() {
      lstInnerJoin.Items.Clear();
      if (mCustomFindMethod.innerjoin!=null) {
        foreach (type_generationGenerateentityCustomfindmethodInnerjoin innerjoin in mCustomFindMethod.innerjoin) {
          string joinInfo = innerjoin.entity + " " + innerjoin.entityalias;
          if (innerjoin.joinexpression != null && innerjoin.joinexpression.Length != 0) {
            joinInfo += " ON (" + innerjoin.joinexpression + ")";
          }
          lstInnerJoin.Items.Add(joinInfo);
        }
      }
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(fdlgCustomFindMethod));
      this.cmdCancel = new System.Windows.Forms.Button();
      this.cmdOK = new System.Windows.Forms.Button();
      this.AttributeSelector = new PVEntityGenerator.Dialogs.ctlAttributeSelector();
      this.txtMethodName = new System.Windows.Forms.TextBox();
      this.lblMethodName = new System.Windows.Forms.Label();
      this.txtEntityAlias = new System.Windows.Forms.TextBox();
      this.lblEntityAlias = new System.Windows.Forms.Label();
      this.txtWhereExpression = new System.Windows.Forms.TextBox();
      this.lblWhereExpression = new System.Windows.Forms.Label();
      this.txtOrderBy = new System.Windows.Forms.TextBox();
      this.lblOrderBy = new System.Windows.Forms.Label();
      this.chkReturnsMultiple = new System.Windows.Forms.CheckBox();
      this.chkGenerateTest = new System.Windows.Forms.CheckBox();
      this.txtDescription = new System.Windows.Forms.TextBox();
      this.lblDescription = new System.Windows.Forms.Label();
      this.lblInnerJoin = new System.Windows.Forms.Label();
      this.lstInnerJoin = new System.Windows.Forms.ListBox();
      this.cmdInnerJoinAdd = new System.Windows.Forms.Button();
      this.cmdInnerJoinRemove = new System.Windows.Forms.Button();
      this.SuspendLayout();
      //
      // cmdCancel
      //
      this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cmdCancel.Location = new System.Drawing.Point(480, 44);
      this.cmdCancel.Name = "cmdCancel";
      this.cmdCancel.Size = new System.Drawing.Size(72, 24);
      this.cmdCancel.TabIndex = 18;
      this.cmdCancel.Text = "Cancel";
      //
      // cmdOK
      //
      this.cmdOK.Location = new System.Drawing.Point(480, 12);
      this.cmdOK.Name = "cmdOK";
      this.cmdOK.Size = new System.Drawing.Size(72, 24);
      this.cmdOK.TabIndex = 17;
      this.cmdOK.Text = "OK";
      this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
      //
      // AttributeSelector
      //
      this.AttributeSelector.AttributeAddCustomAllowed = true;
      this.AttributeSelector.CustomCreateMethod = null;
      this.AttributeSelector.CustomFindMethod = null;
      this.AttributeSelector.DbDefinitionDocument = null;
      this.AttributeSelector.Entity = null;
      this.AttributeSelector.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.AttributeSelector.Location = new System.Drawing.Point(8, 32);
      this.AttributeSelector.Name = "AttributeSelector";
      this.AttributeSelector.Size = new System.Drawing.Size(456, 168);
      this.AttributeSelector.TabIndex = 2;
      this.AttributeSelector.AttributeAdded += new PVEntityGenerator.Dialogs.ctlAttributeSelector.AttributeEventHandler(this.AttributeSelector_AttributeAdded);
      this.AttributeSelector.AttributeRemoved += new PVEntityGenerator.Dialogs.ctlAttributeSelector.AttributeEventHandler(this.AttributeSelector_AttributeRemoved);
      //
      // txtMethodName
      //
      this.txtMethodName.Location = new System.Drawing.Point(88, 8);
      this.txtMethodName.Name = "txtMethodName";
      this.txtMethodName.Size = new System.Drawing.Size(196, 21);
      this.txtMethodName.TabIndex = 1;
      this.txtMethodName.Text = "";
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
      // txtEntityAlias
      //
      this.txtEntityAlias.Location = new System.Drawing.Point(88, 204);
      this.txtEntityAlias.Name = "txtEntityAlias";
      this.txtEntityAlias.Size = new System.Drawing.Size(120, 21);
      this.txtEntityAlias.TabIndex = 4;
      this.txtEntityAlias.Text = "";
      //
      // lblEntityAlias
      //
      this.lblEntityAlias.AutoSize = true;
      this.lblEntityAlias.Location = new System.Drawing.Point(12, 208);
      this.lblEntityAlias.Name = "lblEntityAlias";
      this.lblEntityAlias.Size = new System.Drawing.Size(59, 17);
      this.lblEntityAlias.TabIndex = 3;
      this.lblEntityAlias.Text = "Entity Alias";
      //
      // txtWhereExpression
      //
      this.txtWhereExpression.Location = new System.Drawing.Point(88, 280);
      this.txtWhereExpression.Multiline = true;
      this.txtWhereExpression.Name = "txtWhereExpression";
      this.txtWhereExpression.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.txtWhereExpression.Size = new System.Drawing.Size(372, 48);
      this.txtWhereExpression.TabIndex = 12;
      this.txtWhereExpression.Text = "";
      //
      // lblWhereExpression
      //
      this.lblWhereExpression.Location = new System.Drawing.Point(12, 284);
      this.lblWhereExpression.Name = "lblWhereExpression";
      this.lblWhereExpression.Size = new System.Drawing.Size(60, 32);
      this.lblWhereExpression.TabIndex = 11;
      this.lblWhereExpression.Text = "Where Expression";
      //
      // txtOrderBy
      //
      this.txtOrderBy.Location = new System.Drawing.Point(88, 332);
      this.txtOrderBy.Name = "txtOrderBy";
      this.txtOrderBy.Size = new System.Drawing.Size(372, 21);
      this.txtOrderBy.TabIndex = 14;
      this.txtOrderBy.Text = "";
      //
      // lblOrderBy
      //
      this.lblOrderBy.AutoSize = true;
      this.lblOrderBy.Location = new System.Drawing.Point(12, 336);
      this.lblOrderBy.Name = "lblOrderBy";
      this.lblOrderBy.Size = new System.Drawing.Size(48, 17);
      this.lblOrderBy.TabIndex = 13;
      this.lblOrderBy.Text = "Order by";
      //
      // chkReturnsMultiple
      //
      this.chkReturnsMultiple.Location = new System.Drawing.Point(232, 208);
      this.chkReturnsMultiple.Name = "chkReturnsMultiple";
      this.chkReturnsMultiple.Size = new System.Drawing.Size(108, 16);
      this.chkReturnsMultiple.TabIndex = 5;
      this.chkReturnsMultiple.Text = "Returns multiple";
      //
      // chkGenerateTest
      //
      this.chkGenerateTest.Location = new System.Drawing.Point(348, 208);
      this.chkGenerateTest.Name = "chkGenerateTest";
      this.chkGenerateTest.Size = new System.Drawing.Size(124, 16);
      this.chkGenerateTest.TabIndex = 6;
      this.chkGenerateTest.Text = "Generate test case";
      //
      // txtDescription
      //
      this.txtDescription.Location = new System.Drawing.Point(88, 360);
      this.txtDescription.Multiline = true;
      this.txtDescription.Name = "txtDescription";
      this.txtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.txtDescription.Size = new System.Drawing.Size(372, 36);
      this.txtDescription.TabIndex = 16;
      this.txtDescription.Text = "";
      //
      // lblDescription
      //
      this.lblDescription.AutoSize = true;
      this.lblDescription.Location = new System.Drawing.Point(12, 364);
      this.lblDescription.Name = "lblDescription";
      this.lblDescription.Size = new System.Drawing.Size(60, 17);
      this.lblDescription.TabIndex = 15;
      this.lblDescription.Text = "Description";
      //
      // lblInnerJoin
      //
      this.lblInnerJoin.AutoSize = true;
      this.lblInnerJoin.Location = new System.Drawing.Point(12, 236);
      this.lblInnerJoin.Name = "lblInnerJoin";
      this.lblInnerJoin.Size = new System.Drawing.Size(54, 17);
      this.lblInnerJoin.TabIndex = 7;
      this.lblInnerJoin.Text = "Inner Join";
      //
      // lstInnerJoin
      //
      this.lstInnerJoin.Location = new System.Drawing.Point(88, 232);
      this.lstInnerJoin.Name = "lstInnerJoin";
      this.lstInnerJoin.Size = new System.Drawing.Size(372, 43);
      this.lstInnerJoin.TabIndex = 8;
      this.lstInnerJoin.DoubleClick += new System.EventHandler(this.lstInnerJoin_DoubleClick);
      //
      // cmdInnerJoinAdd
      //
      this.cmdInnerJoinAdd.Location = new System.Drawing.Point(464, 232);
      this.cmdInnerJoinAdd.Name = "cmdInnerJoinAdd";
      this.cmdInnerJoinAdd.Size = new System.Drawing.Size(52, 20);
      this.cmdInnerJoinAdd.TabIndex = 9;
      this.cmdInnerJoinAdd.Text = "Add";
      this.cmdInnerJoinAdd.Click += new System.EventHandler(this.cmdInnerJoinAdd_Click);
      //
      // cmdInnerJoinRemove
      //
      this.cmdInnerJoinRemove.Location = new System.Drawing.Point(464, 256);
      this.cmdInnerJoinRemove.Name = "cmdInnerJoinRemove";
      this.cmdInnerJoinRemove.Size = new System.Drawing.Size(52, 20);
      this.cmdInnerJoinRemove.TabIndex = 10;
      this.cmdInnerJoinRemove.Text = "Remove";
      this.cmdInnerJoinRemove.Click += new System.EventHandler(this.cmdInnerJoinRemove_Click);
      //
      // fdlgCustomFindMethod
      //
      this.AcceptButton = this.cmdOK;
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
      this.CancelButton = this.cmdCancel;
      this.ClientSize = new System.Drawing.Size(562, 403);
      this.Controls.Add(this.cmdInnerJoinRemove);
      this.Controls.Add(this.cmdInnerJoinAdd);
      this.Controls.Add(this.lstInnerJoin);
      this.Controls.Add(this.lblInnerJoin);
      this.Controls.Add(this.txtDescription);
      this.Controls.Add(this.lblDescription);
      this.Controls.Add(this.txtOrderBy);
      this.Controls.Add(this.lblOrderBy);
      this.Controls.Add(this.txtWhereExpression);
      this.Controls.Add(this.txtEntityAlias);
      this.Controls.Add(this.lblEntityAlias);
      this.Controls.Add(this.txtMethodName);
      this.Controls.Add(this.lblMethodName);
      this.Controls.Add(this.chkGenerateTest);
      this.Controls.Add(this.chkReturnsMultiple);
      this.Controls.Add(this.lblWhereExpression);
      this.Controls.Add(this.cmdCancel);
      this.Controls.Add(this.cmdOK);
      this.Controls.Add(this.AttributeSelector);
      this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "fdlgCustomFindMethod";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Custom find method";
      this.ResumeLayout(false);

    }
    #endregion

    private void cmdOK_Click(object sender, System.EventArgs e) {
      if (txtMethodName.Text.Length==0) {
        MessageBox.Show(this, "Please enter a method name.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }

      mCustomFindMethod.name = txtMethodName.Text;
      mCustomFindMethod.description = txtDescription.Text.Length==0 ? null : txtDescription.Text;
      mCustomFindMethod.entityalias = txtEntityAlias.Text.Length==0 ? null : txtEntityAlias.Text;
      mCustomFindMethod.whereexpression = txtWhereExpression.Text.Length==0 ? null : txtWhereExpression.Text;
      mCustomFindMethod.orderbyexpression = txtOrderBy.Text.Length==0 ? null : txtOrderBy.Text;
      mCustomFindMethod.returnsmultiple = chkReturnsMultiple.Checked;
      mCustomFindMethod.generatetest = chkGenerateTest.Checked;
      AttributeSelector.Store();

      this.DialogResult = DialogResult.OK;
      this.Hide();
    }

    private void lstInnerJoin_DoubleClick(object sender, System.EventArgs e) {
      if (lstInnerJoin.SelectedIndex >= 0) {
        type_generationGenerateentityCustomfindmethodInnerjoin innerjoin = mCustomFindMethod.innerjoin[lstInnerJoin.SelectedIndex];

        fdlgCustomFindMethod_InnerJoin dlg = new fdlgCustomFindMethod_InnerJoin();
        dlg.DbDefinitionDocument = mDbDefinitionDocument;
        dlg.Entity = innerjoin.entity;
        dlg.EntityAlias = innerjoin.entityalias;
        dlg.JoinExpression = innerjoin.joinexpression;

        if (dlg.ShowDialog(this)==DialogResult.OK) {
          innerjoin.entity = dlg.Entity;
          innerjoin.entityalias = dlg.EntityAlias;
          innerjoin.joinexpression = dlg.JoinExpression;
          LoadInnerJoins();
        }
      }
    }

    private void cmdInnerJoinAdd_Click(object sender, System.EventArgs e) {
      fdlgCustomFindMethod_InnerJoin dlg = new fdlgCustomFindMethod_InnerJoin();
      dlg.DbDefinitionDocument = mDbDefinitionDocument;

      if (dlg.ShowDialog(this)==DialogResult.OK) {
        type_generationGenerateentityCustomfindmethodInnerjoin innerjoin = new type_generationGenerateentityCustomfindmethodInnerjoin();
        innerjoin.entity = dlg.Entity;
        innerjoin.entityalias = dlg.EntityAlias;
        innerjoin.joinexpression = dlg.JoinExpression;

        ArrayList alInnerJoins = new ArrayList();
        if (mCustomFindMethod.innerjoin!=null) {
          alInnerJoins.AddRange(mCustomFindMethod.innerjoin);
        }
        alInnerJoins.Add(innerjoin);
        mCustomFindMethod.innerjoin = (type_generationGenerateentityCustomfindmethodInnerjoin[])alInnerJoins.ToArray(typeof(type_generationGenerateentityCustomfindmethodInnerjoin));
        LoadInnerJoins();
      }
    }

    private void cmdInnerJoinRemove_Click(object sender, System.EventArgs e) {
      if (lstInnerJoin.SelectedIndex >= 0) {
        ArrayList alInnerJoins = new ArrayList(mCustomFindMethod.innerjoin);
        alInnerJoins.RemoveAt(lstInnerJoin.SelectedIndex);
        if (alInnerJoins.Count==0) {
          mCustomFindMethod.innerjoin = null;
        }
        else {
          mCustomFindMethod.innerjoin = (type_generationGenerateentityCustomfindmethodInnerjoin[])alInnerJoins.ToArray(typeof(type_generationGenerateentityCustomfindmethodInnerjoin));
        }
        LoadInnerJoins();
      }
    }

    private void AttributeSelector_AttributeAdded(object pSender, ctlAttributeSelector.AttributeEventArgs pArgs) {
      string strWhere = txtWhereExpression.Text.Trim();

      if (strWhere.Length!=0) {
        strWhere += " AND ";
      }
      strWhere += "(" + pArgs.Attribute + "=?)";
      while (strWhere.IndexOf("  ") >= 0) {
        strWhere = strWhere.Replace("  ", " ");
      }

      txtWhereExpression.Text = strWhere;
    }

    private void AttributeSelector_AttributeRemoved(object pSender, ctlAttributeSelector.AttributeEventArgs pArgs) {
      string strWhere = txtWhereExpression.Text.Trim();

      if (strWhere.IndexOf("AND (" + pArgs.Attribute + "=?)") >= 0) {
        strWhere = strWhere.Replace("AND (" + pArgs.Attribute + "=?)", "").Trim();
      }
      else if (strWhere.IndexOf("(" + pArgs.Attribute + "=?) AND") >= 0) {
        strWhere = strWhere.Replace("(" + pArgs.Attribute + "=?) AND", "").Trim();
      }
      else if (strWhere.IndexOf("(" + pArgs.Attribute + "=?)") >= 0) {
        strWhere = strWhere.Replace("(" + pArgs.Attribute + "=?)", "").Trim();
      }
      while (strWhere.IndexOf("  ") >= 0) {
        strWhere = strWhere.Replace("  ", " ");
      }

      txtWhereExpression.Text = strWhere;
    }

  }
}
