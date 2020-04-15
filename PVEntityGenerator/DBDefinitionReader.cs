using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Collections;
using System.Globalization;
using PVEntityGenerator.XMLSchema;

namespace PVEntityGenerator {

  public class DBDefinitionReader {

    private StatusHandler mStatusHandler = null;
    private XmlSerializer mXmlSerializer = null;

    private dbdefinition mDBDefinition = null;
    private bool mfSysTableExport=false;
    private bool mfSysTablePVEntityGenerator=false;

    public DBDefinitionReader(StatusHandler pStatusHanlder) {
      mStatusHandler = pStatusHanlder;
      mXmlSerializer = new XmlSerializer(typeof(dbdefinition));
    }

    public bool SysTableExport {
      get { return mfSysTableExport; }
    }

    public bool SysTablePVEntityGenerator {
      get { return mfSysTablePVEntityGenerator;}
    }

    public void LoadFromMDB(string pFilename) {
      GenerateXMLFromMDB(pFilename);
    }

    public void LoadFromXml(string pFilename) {
      XmlTextReader reader = new XmlTextReader(pFilename);
      mDBDefinition = (dbdefinition)mXmlSerializer.Deserialize(reader);
      reader.Close();
    }

    public dbdefinition GetDBDefinition() {
      return mDBDefinition;
    }

    public String GetXmlString() {
      StringWriter writer = new StringWriter();
      XmlTextWriter xmlwriter = new XmlTextWriter(writer);
      xmlwriter.Formatting = System.Xml.Formatting.Indented;
      mXmlSerializer.Serialize(xmlwriter, mDBDefinition);
      string strXML = writer.ToString();
      xmlwriter.Close();
      writer.Close();
      return strXML;
    }

    public XmlDocument GetXmlDocument() {
      string strXML = GetXmlString();
      XmlDocument doc = new XmlDocument();
      doc.LoadXml(strXML);
      return doc;
    }

    private void GenerateXMLFromMDB(string pFilename) {
      mfSysTableExport=false;
      mfSysTablePVEntityGenerator=false;

      mDBDefinition = new dbdefinition();
      mDBDefinition.generatedon = DateTime.Now;

      // Generate entities
      ArrayList alEntities = new ArrayList();

      ADODB.Connection con = new ADODB.ConnectionClass();
      try {
        con.Open(DBServerHelper.getDatabaseOleDbConnectionString(pFilename), "", "", 0);
      }
      catch (Exception ex) {
        throw new Exception("Unable to open database file:\n" + ex.Message);
      }
      ADOX.Catalog cat = new ADOX.Catalog();
      cat.ActiveConnection = con;

      mStatusHandler.InitStatus("Analyzing MDB table structure...", cat.Tables.Count);

      int intTable = 0;
      int intTableRealCount = 0;
      foreach (ADOX.Table tbl in cat.Tables) {
        intTable++;
        if (!IsSystemTableName(tbl.Name)) {
          intTableRealCount++;
          mStatusHandler.SetStatus("Analyzing table '" + tbl.Name + "'...", intTable);

          alEntities.Add(GenerateEntity(tbl, con));
        } else {
          // Check if settings from older versions of SQLExporter or PVEntityGenerator exist
          if (!mfSysTableExport)
            mfSysTableExport=tbl.Name.ToLower().Equals("_export");
          if (!mfSysTablePVEntityGenerator)
            mfSysTablePVEntityGenerator=tbl.Name.ToLower().Equals("_pventitygenerator");
        }
      }
      con.Close();

      if (alEntities.Count!=0) {
        mDBDefinition.entities = (dbdefinitionEntity[])alEntities.ToArray(typeof(dbdefinitionEntity));
      }

      mStatusHandler.ClearStatus("Analyzed " + intTableRealCount + " table(s).");
    }

    private dbdefinitionEntity GenerateEntity(ADOX.Table ptbl, ADODB.Connection pcon) {

      dbdefinitionEntity entity = new dbdefinitionEntity();
      entity.name = ptbl.Name;
      entity.description = GetTableDescription(ptbl, pcon);

      // Generate attributes
      ArrayList alAttributes = new ArrayList();
      int intColIndex = 0;
      ADODB.Recordset rs = new ADODB.RecordsetClass();
      rs.Open("SELECT * FROM [" + ptbl.Name + "] WHERE (3=4)", pcon, ADODB.CursorTypeEnum.adOpenStatic, ADODB.LockTypeEnum.adLockReadOnly, 0);
      foreach (ADODB.Field fld in rs.Fields) {
        alAttributes.Add(GenerateColumn(ptbl.Columns[fld.Name], intColIndex));
        intColIndex++;
      }
      rs.Close();
      if (alAttributes.Count!=0) {
        entity.attributes = (dbdefinitionEntityAttribute[])alAttributes.ToArray(typeof(dbdefinitionEntityAttribute));
      }

      // Generate primary/unique keys/constraints
      entity.keys = new dbdefinitionEntityKeys();
      ArrayList alUniqueKeys = new ArrayList();
      ArrayList alForeignKeys = new ArrayList();
      int intIndex = 0;
      foreach (ADOX.Key key in ptbl.Keys) {

        switch (key.Type) {

          case ADOX.KeyTypeEnum.adKeyPrimary:
            entity.keys.primarykey = new dbdefinitionEntityKeysPrimarykey();
            entity.keys.primarykey.name = ptbl.Name + "_pk";

            // get all primary key columns
            ArrayList aAttr=new ArrayList();
            foreach (ADOX.Column col in key.Columns) {
              dbdefinitionEntityKeysPrimarykeyAttributeref attr=new dbdefinitionEntityKeysPrimarykeyAttributeref();
              attr.attribute=col.Name;

              aAttr.Add(attr);
            }

            // create the primary key attribute array
            entity.keys.primarykey.attributeref=(dbdefinitionEntityKeysPrimarykeyAttributeref [])
              aAttr.ToArray(typeof(dbdefinitionEntityKeysPrimarykeyAttributeref));
            break;

          case ADOX.KeyTypeEnum.adKeyUnique:
            dbdefinitionEntityKeysUniquekey uniquekey = new dbdefinitionEntityKeysUniquekey();

            ArrayList alAttributeRefs = new ArrayList();
            foreach (ADOX.Column col in key.Columns) {
              dbdefinitionEntityKeysUniquekeyAttributeref attributeref = new dbdefinitionEntityKeysUniquekeyAttributeref();
              attributeref.attribute = col.Name;
              alAttributeRefs.Add(attributeref);
            }
            if (alAttributeRefs.Count!=0) {
              uniquekey.attributeref = (dbdefinitionEntityKeysUniquekeyAttributeref[])alAttributeRefs.ToArray(typeof(dbdefinitionEntityKeysUniquekeyAttributeref));
            }

            // check for duplicate indexes - ignore duplicate index defintions
            bool isDuplicate = false;
            foreach (dbdefinitionEntityKeysUniquekey existingKey in alUniqueKeys) {
              if (uniquekey.attributeref.Length == existingKey.attributeref.Length) {
                isDuplicate = true;
                for (int i = 0; i < uniquekey.attributeref.Length; i++) {
                  if (!uniquekey.attributeref[i].attribute.Equals(existingKey.attributeref[i].attribute)) {
                    isDuplicate = false;
                    break;
                  }
                }
                if (isDuplicate) {
                  break;
                }
              }
            }
            if (!isDuplicate) {
              uniquekey.name = ptbl.Name + "_uk" + intIndex;
              alUniqueKeys.Add(uniquekey);
            }
            break;

          case ADOX.KeyTypeEnum.adKeyForeign:
            dbdefinitionEntityKeysForeignkey foreignkey = new dbdefinitionEntityKeysForeignkey();
            foreignkey.name = ptbl.Name + "_fk" + intIndex;
            foreignkey.foreignentity = key.RelatedTable;
            foreignkey.cascadingdelete = (key.DeleteRule & ADOX.RuleEnum.adRICascade)!=0;

            foreignkey.attributeref = new dbdefinitionEntityKeysForeignkeyAttributeref();
            foreignkey.attributeref.attribute = key.Columns[0].Name;
            foreignkey.attributeref.foreignattribute = key.Columns[0].RelatedColumn;

            alForeignKeys.Add(foreignkey);
            break;

        }

        intIndex++;
      }
      if (alUniqueKeys.Count!=0) {
        entity.keys.uniquekey = (dbdefinitionEntityKeysUniquekey[])alUniqueKeys.ToArray(typeof(dbdefinitionEntityKeysUniquekey));
      }
      if (alForeignKeys.Count!=0) {
        entity.keys.foreignkey = (dbdefinitionEntityKeysForeignkey[])alForeignKeys.ToArray(typeof(dbdefinitionEntityKeysForeignkey));
      }

      // Generate indices
      ArrayList alIndexes = new ArrayList();
      intIndex = 0;
      foreach (ADOX.Index idx in ptbl.Indexes) {
        if (!idx.PrimaryKey) {
          dbdefinitionEntityIndex index = new dbdefinitionEntityIndex();
          index.unique = idx.Unique;
          index.ignorenulls = (idx.IndexNulls == ADOX.AllowNullsEnum.adIndexNullsIgnore);

          ArrayList alAttributeRefs = new ArrayList();
          foreach (ADOX.Column col in idx.Columns) {
            dbdefinitionEntityIndexAttributeref attributeref = new dbdefinitionEntityIndexAttributeref();
            attributeref.attribute = col.Name;
            alAttributeRefs.Add(attributeref);
          }
          index.attributeref = (dbdefinitionEntityIndexAttributeref[])alAttributeRefs.ToArray(typeof(dbdefinitionEntityIndexAttributeref));

          // check for duplicate indexes - ignore duplicate index defintions
          bool isDuplicate = false;
          foreach (dbdefinitionEntityIndex existingIndex in alIndexes) {
            if (index.attributeref.Length == existingIndex.attributeref.Length) {
              isDuplicate = true;
              for (int i = 0; i < index.attributeref.Length; i++) {
                if (!index.attributeref[i].attribute.Equals(existingIndex.attributeref[i].attribute)) {
                  isDuplicate = false;
                  break;
                }
              }
              if (isDuplicate) {
                break;
              }
            }
          }
          if (!isDuplicate) {
            index.name = ptbl.Name + "_idx" + intIndex;
            alIndexes.Add(index);
          }
          intIndex++;
        }
      }
      if (alIndexes.Count!=0) {
        entity.indexes = (dbdefinitionEntityIndex[])alIndexes.ToArray(typeof(dbdefinitionEntityIndex));
      }

      // mark 1:1 relationship foreign keys with "one-to-one" attribute
      if (entity.keys != null && entity.keys.foreignkey != null && entity.indexes != null) {
        foreach (dbdefinitionEntityKeysForeignkey foreignkey in entity.keys.foreignkey) {
          foreach (dbdefinitionEntityIndex index in entity.indexes) {
            if (index.attributeref.Length == 1 && index.attributeref[0].attribute.Equals(foreignkey.attributeref.foreignattribute)) {
              if (index.unique) {
                foreignkey.onetoone = true;
              }
            }
          }
        }
      }

      // Generate hashcode of entity definition
      XmlSerializer entityDefSerializer = new XmlSerializer(typeof(dbdefinitionEntity));
      StringWriter stringWriter = new StringWriter();
      entityDefSerializer.Serialize(stringWriter, entity);
      String entityDef = stringWriter.ToString();
      entity.hashcode = entityDef.GetHashCode().ToString();

      return entity;
    }

    private dbdefinitionEntityAttribute GenerateColumn(ADOX.Column pcol, int pColIndex) {

      dbdefinitionEntityAttribute attr = new dbdefinitionEntityAttribute();
      attr.name = pcol.Name;
      attr.type = GetColumnType(pcol);

      if (attr.type==type_AttributeType.VARCHAR) {
        int intSize = GetColumnCustomVarcharSize(pcol);
        if (intSize==0) {
          intSize = pcol.DefinedSize;
        }
        attr.size = intSize.ToString();
      }

      if (attr.type == type_AttributeType.DECIMAL) {
        attr.size = pcol.Precision.ToString();
        if (pcol.Type == ADOX.DataTypeEnum.adCurrency) {
          attr.scale = "4";
        }
        else {
          attr.scale = pcol.NumericScale.ToString();
        }
      }

      string strDefault = GetColumnDefaultValue(pcol, attr.type);
      if (strDefault.Length!=0) {
        attr.defaultvalue = strDefault;
      }
      string strDescription = GetColumnDescription(pcol, true);
      if (strDescription.Length!=0) {
        attr.description = strDescription;
      }
      attr.required = (pColIndex==0) || ((pcol.Attributes & ADOX.ColumnAttributesEnum.adColNullable) == 0);

      string strDesc = GetColumnDescription(pcol, false);
      if ((strDesc.IndexOf("#identity") >= 0) || (strDesc.IndexOf("#auto-increment") >= 0)) {
        attr.autoincrementSpecified = true;
        attr.autoincrement = true;
      }
      if ((strDesc.IndexOf("#notest") >= 0)) {
        attr.nounittestSpecified = true;
        attr.nounittest = true;
      }
      if ((strDesc.IndexOf("#deprecated") >= 0)) {
        attr.deprecatedSpecified = true;
        attr.deprecated = true;
      }
      if ((strDesc.IndexOf("#spatial") >= 0)) {
        attr.spatial = true;
      }
      if ((strDesc.IndexOf("#longitude") >= 0)) {
        attr.longitude = true;
      }
      if ((strDesc.IndexOf("#latitude") >= 0)) {
        attr.latitude = true;
      }
      if ((strDesc.IndexOf("#xmlmapping") >= 0)) {
        string xmlMapping = GetColumnDescriptionProperty(strDesc, "#xmlmapping");
        if (xmlMapping.Length != 0) {
          attr.xmlmapping = xmlMapping;
        }
      }
      if ((strDesc.IndexOf("#searchindex") >= 0)) {
        string indexMode = GetColumnDescriptionProperty(strDesc, "#searchindex");
        if (indexMode.Length == 0) {
          indexMode = "tokenized";
        }
        attr.searchindex = indexMode;
      }
      if ((strDesc.IndexOf("#sortablefield") >= 0)) {
        attr.sortablereffield = GetColumnDescriptionProperty(strDesc, "#sortablefield");
        attr.sortable = true;
      }
      if ((strDesc.IndexOf("#sortable") >=0 )) {
        attr.sortable = true;
      }
      if ((strDesc.IndexOf("#searchstore") >= 0)) {
        string searchStore = GetColumnDescriptionProperty(strDesc, "#searchstore");
        if (searchStore.Length != 0) {
          attr.searchstore = searchStore;
        }
      }
      if ((strDesc.IndexOf("#searchembeddepth") >= 0)) {
        string searchEmbedDepthString = GetColumnDescriptionProperty(strDesc, "#searchembeddepth");
        if (searchEmbedDepthString.Length != 0) {
          try {
            int searchEmbedDepth = int.Parse(searchEmbedDepthString, CultureInfo.GetCultureInfo("en-US"));
            attr.searchembeddepthSpecified = true;
            attr.searchembeddepth = searchEmbedDepth;
          }
          catch (Exception) {
            // ignore
          }
        }
      }
      if ((strDesc.IndexOf("#searchboost") >= 0)) {
        string searchBoostString = GetColumnDescriptionProperty(strDesc, "#searchboost");
        if (searchBoostString.Length != 0) {
          try {
            float searchBoost = float.Parse(searchBoostString, CultureInfo.GetCultureInfo("en-US"));
            attr.searchboostSpecified = true;
            attr.searchboost = searchBoost;
          }
          catch (Exception) {
            // ignore
          }
        }
      }
      if ((strDesc.IndexOf("#searchdateresolution") >= 0)) {
        string searchDateResolution = GetColumnDescriptionProperty(strDesc, "#searchdateresolution");
        if (searchDateResolution.Length != 0) {
          attr.searchdateresolution = searchDateResolution;
        }
      }
      if ((strDesc.IndexOf("#searchfieldbridge") >= 0)) {
        string searchFieldBridge = GetColumnDescriptionProperty(strDesc, "#searchfieldbridge");
        if (searchFieldBridge.Length != 0) {
          attr.searchfieldbridge = searchFieldBridge;
        }
      }

      return attr;
    }

    private bool IsSystemTableName(string pTableName) {
      string strName = pTableName.ToLower();
      return strName.StartsWith("msys") || strName.StartsWith("usys") || strName.StartsWith("~tmp")
        || strName.Equals("_export") || strName.Equals("_pventitygenerator");
    }

    private type_AttributeType GetColumnType(ADOX.Column pcol) {
      switch (pcol.Type) {
        case ADOX.DataTypeEnum.adBoolean:
          return type_AttributeType.BIT;
        case ADOX.DataTypeEnum.adTinyInt:
          return type_AttributeType.BYTE;
        case ADOX.DataTypeEnum.adDate:
          return type_AttributeType.TIMESTAMP;
        case ADOX.DataTypeEnum.adSingle:
        case ADOX.DataTypeEnum.adDouble:
          return type_AttributeType.FLOAT;
        case ADOX.DataTypeEnum.adSmallInt:
          return type_AttributeType.SMALLINT;
        case ADOX.DataTypeEnum.adInteger:
          if (pcol.Name.EndsWith("ID") || pcol.Name.EndsWith("Id")) {
            return type_AttributeType.ID;
          }
          else if (pcol.Name.ToLower().Equals("vstamp")) {
            return type_AttributeType.VSTAMP;
          }
          else {
            return type_AttributeType.INTEGER;
          }
        case ADOX.DataTypeEnum.adLongVarChar:
        case ADOX.DataTypeEnum.adLongVarWChar:
          if (GetColumnCustomVarcharSize(pcol) > 0) {
            return type_AttributeType.VARCHAR;
          }
          else {
            return type_AttributeType.CLOB;
          }
        case ADOX.DataTypeEnum.adCurrency:
          return type_AttributeType.DECIMAL;
        case ADOX.DataTypeEnum.adNumeric:
          if (pcol.Precision==18 && pcol.NumericScale == 0) {
            return type_AttributeType.BIGINT;
          }
          else {
            return type_AttributeType.DECIMAL;
          }
        case ADOX.DataTypeEnum.adVarChar:
        case ADOX.DataTypeEnum.adWChar:
        case ADOX.DataTypeEnum.adVarWChar:
          return type_AttributeType.VARCHAR;
        case ADOX.DataTypeEnum.adLongVarBinary:
          return type_AttributeType.BLOB;
        default:
          throw new Exception("Column '" + pcol.Name + "': Unsupported Field Type #" + pcol.Type + ".");
      }
    }

    private string GetTableDescription(ADOX.Table pTable, ADODB.Connection pcon) {
      string strDesc = null;
      try {
        ADODB.Recordset rsSchema = pcon.OpenSchema(ADODB.SchemaEnum.adSchemaTables, new object[] { null, null, pTable.Name, "TABLE" }, System.Reflection.Missing.Value);
        if (!rsSchema.EOF) {
          if (rsSchema.Fields["DESCRIPTION"].Value != System.DBNull.Value) {
            strDesc = (string)rsSchema.Fields["DESCRIPTION"].Value;
          }
        }
      }
      catch (Exception) { }
      if ("".Equals(strDesc)) {
        strDesc = null;
      }
      return strDesc;
    }

    private string GetColumnDescription(ADOX.Column pcol, bool pfStripFlags) {
      string strDesc = "";
      try {
        strDesc = pcol.Properties["Description"].Value.ToString();
      }
      catch (Exception) { }
      if (strDesc.Length != 0 && pfStripFlags) {
        int intPos = 0;
        while ((intPos = strDesc.IndexOf("#")) >= 0) {
          int intPosEnd = strDesc.IndexOf(" ", intPos + 1);
          if (intPosEnd >= 0) {
            strDesc = strDesc.Substring(0, intPos) + strDesc.Substring(intPosEnd + 1);
          }
          else {
            strDesc = strDesc.Substring(0, intPos);
            break;
          }
        }
      }
      return strDesc;
    }

    private int GetColumnCustomVarcharSize(ADOX.Column pcol) {
      string description = GetColumnDescription(pcol, false);
      int size = 0;

      string sizeString = GetColumnDescriptionProperty(description, "#size");
      if (sizeString.Length != 0) {
        try {
          size = int.Parse(sizeString);
        }
        catch (Exception) { }
      }

      return size;
    }

    private string GetColumnDescriptionProperty(string pDescription, string pProperty) {
      string value = "";
      string SEARCHFOR = pProperty + "=";
      int pos = pDescription.IndexOf(SEARCHFOR);
      if (pos >= 0) {
        int posEnd = pDescription.IndexOf(" ", pos + SEARCHFOR.Length);
        if (posEnd >= 0) {
          value = pDescription.Substring(pos + SEARCHFOR.Length, posEnd - pos - SEARCHFOR.Length);
        }
        else {
          value = pDescription.Substring(pos + SEARCHFOR.Length);
        }
      }
      return value;
    }

    private string GetColumnDefaultValue(ADOX.Column pcol, type_AttributeType pType) {
      string strDefault = "";
      try {
        if (pcol.Properties["Default"]!=null && pcol.Properties["Default"].Value!=null) {
          strDefault = pcol.Properties["Default"].Value.ToString();
        }
      }
      catch (Exception) {}

      if (strDefault.Length!=0) {
        switch (pType) {
          case type_AttributeType.VARCHAR:
          case type_AttributeType.CLOB:
            if (strDefault.StartsWith("\"") && strDefault.EndsWith("\"")) {
              strDefault = strDefault.Substring(1, strDefault.Length-2);
            }
            break;
          case type_AttributeType.INTEGER:
          case type_AttributeType.SMALLINT:
          case type_AttributeType.BYTE:
          case type_AttributeType.BIGINT:
          case type_AttributeType.ID:
          case type_AttributeType.VSTAMP:
            // keep default value
            break;
          case type_AttributeType.FLOAT:
            strDefault = strDefault.Replace(",",".");
            break;
          case type_AttributeType.BIT:
            if (strDefault.Equals("1") || strDefault.ToLower().Equals("true") || strDefault.ToLower().Equals("yes")) {
              strDefault = "true";
            }
            else {
              strDefault = "false";
            }
            break;
          case type_AttributeType.TIMESTAMP:
            strDefault = strDefault.Substring(1, strDefault.Length-2);
            try {
              DateTime dat = DateTime.Parse(strDefault, System.Globalization.CultureInfo.CreateSpecificCulture("en"));
              strDefault = dat.ToString("s");
            }
            catch (Exception) {}
            break;
          case type_AttributeType.BLOB:
            strDefault = "";
            break;
        }
      }

      return strDefault;
    }

  }

}
