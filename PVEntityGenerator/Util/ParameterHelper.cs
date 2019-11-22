using System;
using System.Collections;
using PVEntityGenerator.XMLSchema;
using PVEntityGenerator.Controls;

namespace PVEntityGenerator.Util {

  public class ParameterHelper {

    public static string GetParameter(string pParam,
      parameterdefinitionsParameterdefinition[] paParamDef, parametersParameter[] paParam) {
      string strValue = null;

      parameterdefinitionsParameterdefinition paramdef = null;
      if (paParamDef!=null) {
        foreach (parameterdefinitionsParameterdefinition pdef in paParamDef) {
          if (paramdef.name.Equals(pParam)) {
            paramdef = pdef;
            break;
          }
        }
      }

      if (paramdef!=null) {
        strValue = GetParameter(pParam, paramdef, paParam);
      }

      return strValue;
    }

    public static string GetParameter(string pParam,
      parameterdefinitionsParameterdefinition pParamDef, parametersParameter[] paParam) {

      string strValue = pParamDef.Value;
      if (paParam!=null) {
        foreach (parametersParameter param in paParam) {
          if (param.name.Equals(pParam)) {
            strValue = param.Value;
            break;
          }
        }
      }
      if (strValue==null) {
        strValue = "";
      }

      return strValue;
    }

    public static void CompleteParameter(parameterdefinitionsParameterdefinition[] paParamDef,
        ref parametersParameter[] paParam) {

      ArrayList alParam = new ArrayList();

      foreach (parameterdefinitionsParameterdefinition paramdef in paParamDef) {

        parametersParameter param = null;
        if (paParam!=null) {
          foreach (parametersParameter p in paParam) {
            if (p.name.Equals(paramdef.name)) {
              param = p;
              break;
            }
          }
        }
        if (param==null) {
          param = new parametersParameter();
          param.name = paramdef.name;
          param.Value = paramdef.Value;
        }
        alParam.Add(param);

      }

      paParam = (parametersParameter[])alParam.ToArray(typeof(parametersParameter));

    }

    public static bool SetPropertySetting(ref parametersParameter[] pParameters, PVPropertyGrid.Setting pSetting) {

      // Value abhängig vom Typ ermitteln
      string strValue = "";
      if (pSetting.Value!=null) {
        if (pSetting.Value is string) {
          strValue = (string)pSetting.Value;
        }
        else if (pSetting.Value is bool) {
          strValue = ((bool)pSetting.Value) ? "true" : "false";
        }
        else if (pSetting.Value is int) {
          strValue = ((int)pSetting.Value).ToString();
        }
        else if (pSetting.Value is DateTime) {
          strValue = ((DateTime)pSetting.Value).ToString("s");
        }
        else {
          strValue = pSetting.Value.ToString();
        }
      }

      // Prüfen, ob bereits vorhanden; dann neu setzen
      foreach (parametersParameter param in pParameters) {
        if (param.name.Equals(pSetting.Key)) {
          param.Value = strValue;
          return true;
        }
      }

      // Ansonsten Parameter neu dem Array hinzufügen
      ArrayList alParam = new ArrayList(pParameters);

      parametersParameter newparam = new parametersParameter();
      newparam.name = pSetting.Key;
      newparam.Value = strValue;
      alParam.Add(newparam);

      pParameters = (parametersParameter[])alParam.ToArray(typeof(parametersParameter));

      return true;
    }

  }

}
