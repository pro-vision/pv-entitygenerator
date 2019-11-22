using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Xml;
using PVEntityGenerator.XMLSchema;

namespace PVEntityGenerator.Dialogs {

  public class ctlAttributeSelector : System.Windows.Forms.UserControl {
    private System.Windows.Forms.ListBox lstAttrSelected;
    private System.Windows.Forms.ListBox lstAttrAvaiable;
    private System.Windows.Forms.Label lblAttrSelected;
    private System.Windows.Forms.Label lblAttrAvaiable;
    private System.Windows.Forms.Button cmdAttrAdd;
    private System.Windows.Forms.Button cmdAttrRemove;
    private System.Windows.Forms.Button cmdAttrRemoveAll;
    private System.Windows.Forms.Button cmdAttrAddCustom;

    private System.ComponentModel.Container components = null;

    private string mEntity = null;
    private XmlDocument mDbDefinitionDocument = null;
    private type_generationGenerateentityCustomcreatemethod mCustomCreateMethod = null;
    private type_generationGenerateentityCustomfindmethod mCustomFindMethod = null;

    private ArrayList malSelected = null;
    private ArrayList malAll = null;

    public delegate void AttributeEventHandler(object pSender, AttributeEventArgs pArgs);
    public event AttributeEventHandler AttributeAdded;
    public event AttributeEventHandler AttributeRemoved;

    public ctlAttributeSelector() {
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

    public bool AttributeAddCustomAllowed {
      get {
        return cmdAttrAddCustom.Visible;
      }
      set {
        cmdAttrAddCustom.Visible = value;
      }
    }

    public string Entity {
      get {
        return mEntity;
      }
      set {
        mEntity = value;
      }
    }

    public XmlDocument DbDefinitionDocument {
      get {
        return mDbDefinitionDocument;
      }
      set {
        mDbDefinitionDocument = value;
      }
    }

    public type_generationGenerateentityCustomcreatemethod CustomCreateMethod {
      get {
        return mCustomCreateMethod;
      }
      set {
        mCustomCreateMethod = value;
      }
    }

    public type_generationGenerateentityCustomfindmethod CustomFindMethod {
      get {
        return mCustomFindMethod;
      }
      set {
        mCustomFindMethod = value;
      }
    }

    public void Initialize() {
      malAll = new ArrayList();
      foreach (XmlNode node in mDbDefinitionDocument.SelectNodes("/db-definition/entities/entity[@name='" + mEntity + "']/attributes/attribute")) {
        if (mCustomCreateMethod!=null) {
          type_generationGenerateentityCustomcreatemethodMethodattribute attr = new type_generationGenerateentityCustomcreatemethodMethodattribute();
          attr.name = node.Attributes["name"].Value;
          malAll.Add(attr);
        }
        else if (mCustomFindMethod!=null) {
          type_generationGenerateentityCustomfindmethodMethodattribute attr = new type_generationGenerateentityCustomfindmethodMethodattribute();
          attr.name = node.Attributes["name"].Value;
          malAll.Add(attr);
        }
      }

      malSelected = new ArrayList();
      if (mCustomCreateMethod!=null && mCustomCreateMethod.methodattribute!=null) {
        foreach (type_generationGenerateentityCustomcreatemethodMethodattribute attr in mCustomCreateMethod.methodattribute) {
          foreach (type_generationGenerateentityCustomcreatemethodMethodattribute attr2 in malAll) {
            if (attr.name.Equals(attr2.name)) {
              malSelected.Add(attr2);
              break;
            }
          }
        }
      }
      else if (mCustomFindMethod!=null && mCustomFindMethod.methodattribute!=null) {
        foreach (type_generationGenerateentityCustomfindmethodMethodattribute attr in mCustomFindMethod.methodattribute) {
          bool fFound = false;
          foreach (type_generationGenerateentityCustomfindmethodMethodattribute attr2 in malAll) {
            if (attr.name.Equals(attr2.name)) {
              malSelected.Add(attr2);
              fFound = true;
              break;
            }
          }
          if (!fFound && attr.typeSpecified) {
            type_generationGenerateentityCustomfindmethodMethodattribute attr2 = new type_generationGenerateentityCustomfindmethodMethodattribute();
            attr2.name = attr.name;
            attr2.type = attr.type;
            attr2.typeSpecified = true;
            attr2.entityalias = attr.entityalias;
            malSelected.Add(attr2);
          }
        }
      }

      LoadAttributes();
    }

    private void LoadAttributes() {
      lstAttrSelected.Items.Clear();
      lstAttrAvaiable.Items.Clear();
      if (mCustomCreateMethod!=null) {
        foreach (type_generationGenerateentityCustomcreatemethodMethodattribute attr in malSelected) {
          lstAttrSelected.Items.Add(attr.name);
        }
        foreach (type_generationGenerateentityCustomcreatemethodMethodattribute attr in malAll) {
          if (!malSelected.Contains(attr)) {
            lstAttrAvaiable.Items.Add(attr.name);
          }
        }
      }
      else if (mCustomFindMethod!=null) {
        foreach (type_generationGenerateentityCustomfindmethodMethodattribute attr in malSelected) {
          if (attr.typeSpecified) {
            lstAttrSelected.Items.Add(((attr.entityalias!=null) ? attr.entityalias + "." : "") + attr.name + " (" + attr.type.ToString() + ")");
          }
          else {
            lstAttrSelected.Items.Add(attr.name);
          }
        }
        foreach (type_generationGenerateentityCustomfindmethodMethodattribute attr in malAll) {
          if (!malSelected.Contains(attr)) {
            lstAttrAvaiable.Items.Add(attr.name);
          }
        }
      }
    }

    public void Store() {
      if (mCustomCreateMethod!=null) {
        mCustomCreateMethod.methodattribute = (type_generationGenerateentityCustomcreatemethodMethodattribute[])malSelected.ToArray(typeof(type_generationGenerateentityCustomcreatemethodMethodattribute));
      }
      else if (mCustomFindMethod!=null) {
        mCustomFindMethod.methodattribute = (type_generationGenerateentityCustomfindmethodMethodattribute[])malSelected.ToArray(typeof(type_generationGenerateentityCustomfindmethodMethodattribute));
      }
    }

    #region Component Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.lstAttrSelected = new System.Windows.Forms.ListBox();
      this.lstAttrAvaiable = new System.Windows.Forms.ListBox();
      this.lblAttrSelected = new System.Windows.Forms.Label();
      this.lblAttrAvaiable = new System.Windows.Forms.Label();
      this.cmdAttrAdd = new System.Windows.Forms.Button();
      this.cmdAttrRemove = new System.Windows.Forms.Button();
      this.cmdAttrRemoveAll = new System.Windows.Forms.Button();
      this.cmdAttrAddCustom = new System.Windows.Forms.Button();
      this.SuspendLayout();
      //
      // lstAttrSelected
      //
      this.lstAttrSelected.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)));
      this.lstAttrSelected.IntegralHeight = false;
      this.lstAttrSelected.Location = new System.Drawing.Point(4, 20);
      this.lstAttrSelected.Name = "lstAttrSelected";
      this.lstAttrSelected.Size = new System.Drawing.Size(200, 212);
      this.lstAttrSelected.TabIndex = 1;
      this.lstAttrSelected.DoubleClick += new System.EventHandler(this.lstAttrSelected_DoubleClick);
      //
      // lstAttrAvaiable
      //
      this.lstAttrAvaiable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)));
      this.lstAttrAvaiable.IntegralHeight = false;
      this.lstAttrAvaiable.Location = new System.Drawing.Point(252, 20);
      this.lstAttrAvaiable.Name = "lstAttrAvaiable";
      this.lstAttrAvaiable.Size = new System.Drawing.Size(200, 212);
      this.lstAttrAvaiable.TabIndex = 7;
      this.lstAttrAvaiable.DoubleClick += new System.EventHandler(this.lstAttrAvaiable_DoubleClick);
      //
      // lblAttrSelected
      //
      this.lblAttrSelected.AutoSize = true;
      this.lblAttrSelected.Location = new System.Drawing.Point(4, 4);
      this.lblAttrSelected.Name = "lblAttrSelected";
      this.lblAttrSelected.Size = new System.Drawing.Size(97, 17);
      this.lblAttrSelected.TabIndex = 0;
      this.lblAttrSelected.Text = "Selected attributes";
      //
      // lblAttrAvaiable
      //
      this.lblAttrAvaiable.AutoSize = true;
      this.lblAttrAvaiable.Location = new System.Drawing.Point(252, 4);
      this.lblAttrAvaiable.Name = "lblAttrAvaiable";
      this.lblAttrAvaiable.Size = new System.Drawing.Size(104, 17);
      this.lblAttrAvaiable.TabIndex = 6;
      this.lblAttrAvaiable.Text = "Avaibable Attributes";
      //
      // cmdAttrAdd
      //
      this.cmdAttrAdd.Location = new System.Drawing.Point(212, 24);
      this.cmdAttrAdd.Name = "cmdAttrAdd";
      this.cmdAttrAdd.Size = new System.Drawing.Size(32, 20);
      this.cmdAttrAdd.TabIndex = 2;
      this.cmdAttrAdd.Text = "<";
      this.cmdAttrAdd.Click += new System.EventHandler(this.cmdAttrAdd_Click);
      //
      // cmdAttrRemove
      //
      this.cmdAttrRemove.Location = new System.Drawing.Point(212, 48);
      this.cmdAttrRemove.Name = "cmdAttrRemove";
      this.cmdAttrRemove.Size = new System.Drawing.Size(32, 20);
      this.cmdAttrRemove.TabIndex = 3;
      this.cmdAttrRemove.Text = ">";
      this.cmdAttrRemove.Click += new System.EventHandler(this.cmdAttrRemove_Click);
      //
      // cmdAttrRemoveAll
      //
      this.cmdAttrRemoveAll.Location = new System.Drawing.Point(212, 72);
      this.cmdAttrRemoveAll.Name = "cmdAttrRemoveAll";
      this.cmdAttrRemoveAll.Size = new System.Drawing.Size(32, 20);
      this.cmdAttrRemoveAll.TabIndex = 4;
      this.cmdAttrRemoveAll.Text = ">>";
      this.cmdAttrRemoveAll.Click += new System.EventHandler(this.cmdAttrRemoveAll_Click);
      //
      // cmdAttrAddCustom
      //
      this.cmdAttrAddCustom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.cmdAttrAddCustom.Location = new System.Drawing.Point(212, 208);
      this.cmdAttrAddCustom.Name = "cmdAttrAddCustom";
      this.cmdAttrAddCustom.Size = new System.Drawing.Size(32, 20);
      this.cmdAttrAddCustom.TabIndex = 5;
      this.cmdAttrAddCustom.Text = "+";
      this.cmdAttrAddCustom.Click += new System.EventHandler(this.cmdAttrAddCustom_Click);
      //
      // ctlAttributeSelector
      //
      this.Controls.Add(this.cmdAttrAddCustom);
      this.Controls.Add(this.cmdAttrRemoveAll);
      this.Controls.Add(this.cmdAttrRemove);
      this.Controls.Add(this.cmdAttrAdd);
      this.Controls.Add(this.lstAttrAvaiable);
      this.Controls.Add(this.lstAttrSelected);
      this.Controls.Add(this.lblAttrAvaiable);
      this.Controls.Add(this.lblAttrSelected);
      this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.Name = "ctlAttributeSelector";
      this.Size = new System.Drawing.Size(456, 236);
      this.ResumeLayout(false);

    }
    #endregion

    private void cmdAttrAdd_Click(object sender, System.EventArgs e) {
      if (lstAttrAvaiable.SelectedIndex >= 0) {
        if (mCustomCreateMethod!=null) {
          foreach (type_generationGenerateentityCustomcreatemethodMethodattribute attr in malAll) {
            if (attr.name.Equals(lstAttrAvaiable.Text)) {
              malSelected.Add(attr);
              break;
            }
          }
        }
        else if (mCustomFindMethod!=null) {
          foreach (type_generationGenerateentityCustomfindmethodMethodattribute attr in malAll) {
            if (attr.name.Equals(lstAttrAvaiable.Text)) {
              malSelected.Add(attr);
              break;
            }
          }
        }
      }
      if (this.AttributeAdded!=null) {
        this.AttributeAdded(this, new AttributeEventArgs(lstAttrAvaiable.Text));
      }
      LoadAttributes();
    }

    private void cmdAttrRemove_Click(object sender, System.EventArgs e) {
      if (lstAttrSelected.SelectedIndex >= 0) {
        malSelected.RemoveAt(lstAttrSelected.SelectedIndex);
        if (this.AttributeRemoved!=null) {
          string strAttr = lstAttrSelected.Text;
          if (strAttr.IndexOf(" ") >= 0) {
            strAttr = strAttr.Substring(0, strAttr.IndexOf(" "));
          }
          this.AttributeRemoved(this, new AttributeEventArgs(strAttr));
        }
        LoadAttributes();
      }
    }

    private void cmdAttrRemoveAll_Click(object sender, System.EventArgs e) {
      if (this.AttributeRemoved!=null) {
        foreach (string strAttribute in lstAttrSelected.Items) {
          this.AttributeRemoved(this, new AttributeEventArgs(strAttribute));
        }
      }
      malSelected = new ArrayList();
      LoadAttributes();
    }

    private void cmdAttrAddCustom_Click(object sender, System.EventArgs e) {
      fdlgCustomFindMethod_CustomAttribute dlg = new fdlgCustomFindMethod_CustomAttribute();
      if (dlg.ShowDialog(this)==DialogResult.OK) {
        type_generationGenerateentityCustomfindmethodMethodattribute attr = new type_generationGenerateentityCustomfindmethodMethodattribute();
        attr.name = dlg.AttributeName;
        attr.type = dlg.AttributeType;
        attr.typeSpecified = true;
        attr.entityalias = dlg.EntityAlias;
        malSelected.Add(attr);
        LoadAttributes();

        if (this.AttributeAdded!=null) {
          this.AttributeAdded(this, new AttributeEventArgs(((attr.entityalias!=null) ? attr.entityalias + "." : "") + attr.name));
        }
      }
    }

    private void lstAttrAvaiable_DoubleClick(object sender, System.EventArgs e) {
      cmdAttrAdd_Click(sender, e);
    }

    private void lstAttrSelected_DoubleClick(object sender, System.EventArgs e) {
      cmdAttrRemove_Click(sender, e);
    }

    public class AttributeEventArgs : EventArgs {
      private string mAttribute = null;
      public AttributeEventArgs(string pAttribute) {
        mAttribute = pAttribute;
      }
      public string Attribute {
        get {
          return mAttribute;
        }
      }
    }

  }
}
