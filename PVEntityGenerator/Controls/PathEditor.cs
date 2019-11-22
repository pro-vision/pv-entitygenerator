using System;
using System.Windows.Forms;
using System.Drawing.Design;

namespace PVEntityGenerator.Controls {

  public class PathEditor : UITypeEditor {

    public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context) {
      return UITypeEditorEditStyle.Modal;
    }

    public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object value) {
      string strValue = "";
      if (value!=null && value is String) {
        strValue = (string)value;
      }

      FolderBrowserDialog dlg = new FolderBrowserDialog();
      dlg.SelectedPath = strValue;
      if (dlg.ShowDialog()==DialogResult.OK) {
        strValue = dlg.SelectedPath;
      }

      return strValue;
    }

  }

}
