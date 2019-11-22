using System;
using System.Windows.Forms;
using System.Drawing.Design;
using System.ComponentModel;

namespace PVEntityGenerator.Controls {

  public class PasswordEditor : UITypeEditor {

    public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context) {
      return UITypeEditorEditStyle.Modal;
    }

    public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object value) {
      string strValue = "";
      if (value!=null && value is String) {
        strValue = (string)value;
      }

      fdlgPasswordEditor dlg = new fdlgPasswordEditor();
      dlg.Value = strValue;
      if (dlg.ShowDialog()==DialogResult.OK) {
        strValue = dlg.Value;
      }

      return strValue;
    }

  }

}
