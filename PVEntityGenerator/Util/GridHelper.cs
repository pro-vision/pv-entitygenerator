using System;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;
using System.Xml;
using System.Text;
using System.Collections;
using PVEntityGenerator.XMLSchema;
using PVFramework;
using PVFramework.Util;

namespace PVEntityGenerator.Util {

  public class GridHelper {

    private const int ENUMERATION_MAXROWS = 1000;

    public static DataView GetAttributes(dbdefinition pDbDefinition, string pEntity) {

      DataTable dt = new DataTable();
      dt.Columns.Add("Attribute", typeof(string));
      dt.Columns.Add("Type", typeof(string));
      dt.Columns.Add("Size", typeof(string));
      dt.Columns.Add("Default", typeof(string));
      dt.Columns.Add("Required", typeof(bool));
      dt.Columns.Add("Description", typeof(string));

      dbdefinitionEntity entity = null;
      foreach (dbdefinitionEntity e in pDbDefinition.entities) {
        if (e.name.Equals(pEntity)) {
          entity = e;
          break;
        }
      }

      if (entity!=null) {
        foreach (dbdefinitionEntityAttribute attr in entity.attributes) {
          DataRow row = dt.NewRow();
          row["Attribute"] = attr.name;
          row["Type"] = attr.type.ToString();
          if (attr.size!=null) {
            row["Size"] = attr.size;
            if (attr.scale != null) {
              row["Size"] = row["Size"] + "/" + attr.scale;
            }
          }
          if (attr.defaultvalue!=null) {
            row["Default"] = attr.defaultvalue;
          }
          row["Required"] = attr.required;
          if (attr.description!=null) {
            row["Description"] = attr.description;
          }
          dt.Rows.Add(row);
        }
      }

      DataView dv = new DataView(dt);
      dv.AllowEdit = false;
      dv.AllowNew = false;
      dv.AllowDelete = false;
      return dv;
    }

    public static DataGridTableStyle GetAttributeTableStyle() {

      DataGridTableStyle ts = new DataGridTableStyle();
      DataGridTextBoxColumn col = null;

      ts.AllowSorting = false;
      ts.RowHeadersVisible = false;
      ts.ReadOnly = true;

      col = new DataGridTextBoxColumn();
      col.MappingName = "Attribute";
      col.HeaderText = "Attribute";
      col.Width = 180;
      col.NullText = "";
      ts.GridColumnStyles.Add(col);

      col = new DataGridTextBoxColumn();
      col.MappingName = "Type";
      col.HeaderText = "Type";
      col.Width = 80;
      col.NullText = "";
      ts.GridColumnStyles.Add(col);

      col = new DataGridTextBoxColumn();
      col.MappingName = "Size";
      col.HeaderText = "Size";
      col.Width = 50;
      col.NullText = "";
      ts.GridColumnStyles.Add(col);

      col = new DataGridTextBoxColumn();
      col.MappingName = "Default";
      col.HeaderText = "Default";
      col.Width = 100;
      col.NullText = "";
      ts.GridColumnStyles.Add(col);

      DataGridBoolColumn bcol = new DataGridBoolColumn();
      bcol.MappingName = "Required";
      bcol.HeaderText = "Required";
      bcol.Width = 50;
      ts.GridColumnStyles.Add(bcol);

      col = new DataGridTextBoxColumn();
      col.MappingName = "Description";
      col.HeaderText = "Description";
      col.Width = 250;
      col.NullText = "";
      ts.GridColumnStyles.Add(col);

      return ts;
    }


    public static DataView GetDeleteConstraints(XmlDocument pDbDefinitionDocument, type_generationGenerateentity pEntity) {

      DataTable dt = new DataTable();
      dt.Columns.Add("ForeignEntity", typeof(string));
      dt.Columns.Add("Message", typeof(string));

      foreach (XmlNode node in pDbDefinitionDocument.SelectNodes("/db-definition/entities/entity"
        + "[keys/foreign-key[@foreign-entity='" + pEntity.entity + "' and @cascading-delete='false']]")) {

        string strForeignEntity = node.Attributes["name"].Value;
        string strMessage = "";

        if (pEntity.removeconstraintmessages!=null) {
          foreach (type_generationGenerateentityRemoveconstraintmessage msg in pEntity.removeconstraintmessages) {
            if (msg.foreignentity.Equals(strForeignEntity)) {
              strMessage = msg.Value;
              break;
            }
          }
        }

        DataRow row = dt.NewRow();
        row["ForeignEntity"] = strForeignEntity;
        row["Message"] = strMessage;
        dt.Rows.Add(row);
      }

      DataView dv = new DataView(dt);
      dv.AllowEdit = true;
      dv.AllowNew = false;
      dv.AllowDelete = false;
      return dv;
    }

    public static DataGridTableStyle GetDeleteConstraintsTableStyle() {

      DataGridTableStyle ts = new DataGridTableStyle();
      DataGridTextBoxColumn col = null;

      ts.AllowSorting = false;
      ts.RowHeadersVisible = false;

      col = new DataGridTextBoxColumn();
      col.MappingName = "ForeignEntity";
      col.HeaderText = "Foreign entity";
      col.Width = 120;
      col.NullText = "";
      col.ReadOnly = true;
      ts.GridColumnStyles.Add(col);

      col = new DataGridTextBoxColumn();
      col.MappingName = "Message";
      col.HeaderText = "Message";
      col.Width = 300;
      col.NullText = "";
      ts.GridColumnStyles.Add(col);

      return ts;
    }

    public static DataView GetEnumeration(type_generationGenerateentity pEntity, string pDbDefinitionFilename, ref bool pfDefChanged) {
      pfDefChanged = false;

      DataTable dt = new DataTable();
      dt.Columns.Add("ID", typeof(int));
      dt.Columns.Add("Identifier", typeof(string));
      dt.Columns.Add("Name", typeof(string));
      dt.Columns.Add("Description", typeof(string));
      dt.Columns.Add("Generate", typeof(bool));

      if (pDbDefinitionFilename.ToLower().EndsWith(".mdb") || pDbDefinitionFilename.ToLower().EndsWith(".accdb")) {
        String strConnect = DBServerHelper.getDatabaseOleDbConnectionString(pDbDefinitionFilename);
        OleDbConnection con = new OleDbConnection(strConnect);
        con.Open();

        OleDbCommand cmd = con.CreateCommand();
        cmd.CommandType = CommandType.TableDirect;
        cmd.CommandText = pEntity.entity;
        OleDbDataReader dr = cmd.ExecuteReader();

        ArrayList alEnum = new ArrayList();
        if (pEntity.enumerationentries!=null) {
          alEnum.AddRange(pEntity.enumerationentries);
        }

        int intRowCount = 0;
        while (dr.Read()) {
          int intID = 0;
          string strName = null;
          string strDescription = null;

          for (int intField=0; intField<dr.FieldCount; intField++) {
            object objValue = dr.GetValue(intField);
            if ((objValue is int) && intID==0) {
              intID = (int)objValue;
            }
            else if ((objValue is string) && strName==null) {
              strName = (string)objValue;
            }
            else if ((objValue is string) && strDescription==null) {
              strDescription = (string)objValue;
            }
            if (intID!=0 && strName!=null && strDescription!=null) {
              break;
            }
          }

          type_generationGenerateentityEnumerationentry enumEntry = null;
          foreach (type_generationGenerateentityEnumerationentry e in alEnum) {
            if (PVFormatUtil.ParseInt(e.id)==intID) {
              enumEntry = e;
              break;
            }
          }
          if (enumEntry==null) {
            pfDefChanged = true;

            if (strName==null) {
              strName = "enum" + intID.ToString();
            }
            if (strDescription==null) {
              strDescription = strName;
            }

            enumEntry = new type_generationGenerateentityEnumerationentry();
            enumEntry.id = intID.ToString();
            enumEntry.identifier = GetIdentifier(strName);
            enumEntry.name = strName;
            enumEntry.description = strDescription;
            enumEntry.generate = true;
            alEnum.Add(enumEntry);
          }

          DataRow row = dt.NewRow();
          row["ID"] = PVFormatUtil.ParseInt(enumEntry.id);
          row["Identifier"] = enumEntry.identifier;
          row["Name"] = enumEntry.name;
          row["Description"] = enumEntry.description;
          row["Generate"] = enumEntry.generate;
          dt.Rows.Add(row);

          intRowCount++;
          if (intRowCount >= ENUMERATION_MAXROWS) {
            break;
          }
        }
        dr.Close();

        con.Close();

        pEntity.enumerationentries = (type_generationGenerateentityEnumerationentry[])alEnum.ToArray(typeof(type_generationGenerateentityEnumerationentry));
      }

      DataView dv = new DataView(dt);
      dv.AllowEdit = true;
      dv.AllowNew = false;
      dv.AllowDelete = false;
      return dv;
    }

    private static string GetIdentifier(string pName) {

      string strIdentifier = pName.ToUpper().Trim();

      strIdentifier = strIdentifier.Replace(" ", "_");
      strIdentifier = strIdentifier.Replace("Ä", "AE");
      strIdentifier = strIdentifier.Replace("Ü", "UE");
      strIdentifier = strIdentifier.Replace("Ö", "OE");
      strIdentifier = strIdentifier.Replace("ß", "SS");

      StringBuilder sbIdentifier = new StringBuilder();
      char cLast = '_';
      foreach (char c in strIdentifier) {
        if (c >= 'A' && c <= 'Z') {
          sbIdentifier.Append(c);
        }
        else if (c >= '0' && c <= '9') {
          sbIdentifier.Append(c);
        }
        else if (c=='_' && cLast!='_') {
          sbIdentifier.Append("_");
        }
        cLast = c;
      }

      return sbIdentifier.ToString();
    }

    public static DataGridTableStyle GetEnumerationTableStyle() {

      DataGridTableStyle ts = new DataGridTableStyle();
      DataGridTextBoxColumn col = null;

      ts.AllowSorting = false;
      ts.RowHeadersVisible = false;

      col = new DataGridTextBoxColumn();
      col.MappingName = "ID";
      col.HeaderText = "ID";
      col.Width = 60;
      col.NullText = "";
      col.ReadOnly = true;
      ts.GridColumnStyles.Add(col);

      col = new DataGridTextBoxColumn();
      col.MappingName = "Identifier";
      col.HeaderText = "Identifier";
      col.Width = 180;
      col.NullText = "";
      ts.GridColumnStyles.Add(col);

      col = new DataGridTextBoxColumn();
      col.MappingName = "Name";
      col.HeaderText = "Name";
      col.Width = 180;
      col.NullText = "";
      ts.GridColumnStyles.Add(col);

      col = new DataGridTextBoxColumn();
      col.MappingName = "Description";
      col.HeaderText = "Description";
      col.Width = 180;
      col.NullText = "";
      ts.GridColumnStyles.Add(col);

      DataGridBoolColumn bcol = new DataGridBoolColumn();
      bcol.MappingName = "Generate";
      bcol.HeaderText = "Generate";
      bcol.Width = 60;
      bcol.AllowNull = false;
      ts.GridColumnStyles.Add(bcol);

      return ts;
    }

    public static ArrayList GetEntityPatch(projectsettingsEntityexport pEntityExport) {
      ArrayList aRet=new ArrayList();
      if (pEntityExport.patchentities!=null) {
        foreach (projectsettingsEntityexportPatchentity e in pEntityExport.patchentities)
          aRet.Add(e.entity);
      }

      return aRet;
    }

    public static DataView GetExportEntityOptions(projectsettingsEntityexport pEntityExport, XmlDocument pDbDefinitionDocument) {

      DataTable dt = new DataTable();
      dt.Columns.Add("Entity", typeof(string));
      dt.Columns.Add("SortNo", typeof(int));
      dt.Columns.Add("ExportStructure", typeof(bool));
      dt.Columns.Add("ExportData", typeof(bool));
      dt.Columns.Add("ExportDrop", typeof(bool));

      dt.Columns["ExportStructure"].DefaultValue = false;
      dt.Columns["ExportData"].DefaultValue = false;
      dt.Columns["ExportDrop"].DefaultValue = false;

      ArrayList alEntity = new ArrayList();
      if (pEntityExport.exportentities!=null) {
        alEntity.AddRange(pEntityExport.exportentities);
      }

      ArrayList alEntityName = new ArrayList();

      foreach (XmlNode node in pDbDefinitionDocument.SelectNodes("/db-definition/entities/entity")) {
        string strEntity = node.Attributes["name"].Value;
        alEntityName.Add(strEntity);

        projectsettingsEntityexportExportentity expentity = null;
        foreach (projectsettingsEntityexportExportentity e in alEntity) {
          if (e.entity.Equals(strEntity)) {
            expentity = e;
            break;
          }
        }
        if (expentity==null) {
          expentity = new projectsettingsEntityexportExportentity();
          expentity.entity = strEntity;
          expentity.exportstructure = true;
          expentity.exportdata = true;
          expentity.exportdrop = false;
          alEntity.Add(expentity);
        }

        DataRow row = dt.NewRow();
        row["Entity"] = expentity.entity;
        if (PVFormatUtil.ParseInt(expentity.sortno)!=0) {
          row["SortNo"] = expentity.sortno;
        }
        row["ExportStructure"] = expentity.exportstructure;
        row["ExportData"] = expentity.exportdata;
        row["ExportDrop"] = expentity.exportdrop;
        dt.Rows.Add(row);
      }

      foreach (projectsettingsEntityexportExportentity expentity in alEntity) {
        if (!alEntityName.Contains(expentity.entity)) {
          alEntityName.Add(expentity.entity);

          DataRow row = dt.NewRow();
          row["Entity"] = expentity.entity;
          if (PVFormatUtil.ParseInt(expentity.sortno)!=0) {
            row["SortNo"] = expentity.sortno;
          }
          row["ExportStructure"] = expentity.exportstructure;
          row["ExportData"] = expentity.exportdata;
          row["ExportDrop"] = expentity.exportdrop;
          dt.Rows.Add(row);
        }
      }

      pEntityExport.exportentities = (projectsettingsEntityexportExportentity[])alEntity.ToArray(typeof(projectsettingsEntityexportExportentity));

      DataView dv = new DataView(dt);
      dv.AllowEdit = true;
      dv.AllowNew = true;
      dv.AllowDelete = true;

      dv.Sort = "SortNo, Entity";

      return dv;
    }

    public static DataGridTableStyle GetExportEntityOptionsTableStyle() {

      DataGridTableStyle ts = new DataGridTableStyle();
      DataGridTextBoxColumn col = null;
      DataGridBoolColumn bcol = null;

      ts.AllowSorting = true;
      ts.RowHeadersVisible = true;

      col = new DataGridTextBoxColumn();
      col.MappingName = "Entity";
      col.HeaderText = "Entity entity";
      col.Width = 200;
      col.NullText = "";
      ts.GridColumnStyles.Add(col);

      col = new DataGridTextBoxColumn();
      col.MappingName = "SortNo";
      col.HeaderText = "Sort no.";
      col.Width = 80;
      col.NullText = "";
      ts.GridColumnStyles.Add(col);

      bcol = new DataGridBoolColumn();
      bcol.MappingName = "ExportStructure";
      bcol.HeaderText = "Export struct.";
      bcol.Width = 80;
      bcol.AllowNull = false;
      bcol.NullValue = false;
      ts.GridColumnStyles.Add(bcol);

      bcol = new DataGridBoolColumn();
      bcol.MappingName = "ExportData";
      bcol.HeaderText = "Export data";
      bcol.Width = 80;
      bcol.AllowNull = false;
      bcol.NullValue = false;
      ts.GridColumnStyles.Add(bcol);

      bcol = new DataGridBoolColumn();
      bcol.MappingName = "ExportDrop";
      bcol.HeaderText = "Drop only";
      bcol.Width = 80;
      bcol.AllowNull = false;
      bcol.NullValue = false;
      ts.GridColumnStyles.Add(bcol);

      return ts;
    }

  }

}
